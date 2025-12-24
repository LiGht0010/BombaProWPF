using BombaProMaxApi.Data;
using BombaProMaxApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Only use hardcoded URL in Development - Production uses ASPNETCORE_URLS env variable
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("https://localhost:7100");
}

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register application services
builder.Services.AddScoped<IStockLotService, StockLotService>();

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
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
var app = builder.Build();

// Initialize database - create if not exists and apply migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        
        logger.LogInformation("Checking database connection and applying migrations...");
        
        // This will create the database if it doesn't exist and apply any pending migrations
        await context.Database.MigrateAsync();
        
        logger.LogInformation("Database initialization completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database.");
        
        // Optionally fall back to EnsureCreated if migrations fail
        // This is useful if you don't have any migrations yet
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            logger.LogWarning("Attempting to create database without migrations...");
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Database created using EnsureCreated.");
        }
        catch (Exception innerEx)
        {
            logger.LogError(innerEx, "Failed to create database. Please check your connection string and database server.");
            throw;
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
