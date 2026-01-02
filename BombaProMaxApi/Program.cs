using BombaProMaxApi.Data;
using BombaProMaxApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Only use hardcoded URL in Development - Production uses ASPNETCORE_URLS env variable
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("https://localhost:7100");
}

// Add services to the container
builder.Services.AddControllers();

// Register DbContext with connection string from configuration
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // Enable retry on transient failures (network issues, connection drops)
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
        
        // Set command timeout for slow connections
        npgsqlOptions.CommandTimeout(60);
    });
});

// Register application services
builder.Services.AddScoped<IStockLotService, StockLotService>();
builder.Services.AddScoped<IPeriodeCascadeService, PeriodeCascadeService>();

// Add CORS for MAUI app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMAUI",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "BombaProMax API",
        Version = "v1",
        Description = "API for BombaProMax gas station management"
    });
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
var app = builder.Build();

// Initialize database - create if not exists and apply migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Initializing database...");

        var context = services.GetRequiredService<AppDbContext>();

        // This will create the database if it doesn't exist and apply any pending migrations
        await context.Database.MigrateAsync();

        logger.LogInformation("Database initialization completed.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing database.");

        // Optionally fall back to EnsureCreated if migrations fail
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            logger.LogWarning("Attempting to create database without migrations...");
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Database created using EnsureCreated.");
        }
        catch (Exception innerEx)
        {
            logger.LogError(innerEx, "Failed to create database. Please check connection string.");
        }
    }
}

// Configure the HTTP request pipeline
// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI();

// Only redirect to HTTPS in Development (Nginx handles SSL in Production)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowMAUI");

app.UseAuthorization();

app.MapControllers();

// Auto-launch browser only in Development (no browser on Linux server)
if (app.Environment.IsDevelopment())
{
    var launchUrl = "https://localhost:7100/swagger";
    Console.WriteLine($"Opening browser at: {launchUrl}");
    try
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = launchUrl,
            UseShellExecute = true
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Could not open browser: {ex.Message}");
    }
}

app.Run();
