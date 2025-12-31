using BombaProMaxApi.Data;
using BombaProMaxApi.Services;
using BombaProMaxApi.MultiTenancy;
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

// Configure Multi-Tenancy
var multiTenantSettings = builder.Configuration
    .GetSection(MultiTenantSettings.SectionName)
    .Get<MultiTenantSettings>() ?? new MultiTenantSettings();

// If no tenants configured, fall back to single-tenant mode using DefaultConnection
if (multiTenantSettings.Tenants.Count == 0)
{
    var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(defaultConnectionString))
    {
        multiTenantSettings.DefaultTenant = "default";
        multiTenantSettings.Tenants.Add(new TenantSettings
        {
            TenantId = "default",
            Name = "Default",
            ConnectionString = defaultConnectionString
        });
    }
}

builder.Services.AddSingleton(multiTenantSettings);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();

// Register DbContext with tenant-aware factory
builder.Services.AddScoped<AppDbContext>(sp =>
{
    var tenantService = sp.GetRequiredService<ITenantService>();
    var connectionString = tenantService.GetConnectionString();
    
    var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
    optionsBuilder.UseNpgsql(connectionString);
    
    return new AppDbContext(optionsBuilder.Options);
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
                  .AllowAnyHeader()
                  .WithExposedHeaders(TenantService.TenantHeaderName);
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
        Description = "Multi-tenant API for BombaProMax gas station management"
    });

    // Add tenant header as a global parameter (not security - just a regular header)
    c.OperationFilter<TenantHeaderOperationFilter>();
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
var app = builder.Build();

// Initialize databases for all tenants - create if not exists and apply migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var settings = services.GetRequiredService<MultiTenantSettings>();

    foreach (var tenant in settings.Tenants)
    {
        try
        {
            logger.LogInformation("Initializing database for tenant '{TenantId}'...", tenant.TenantId);

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(tenant.ConnectionString);

            using var context = new AppDbContext(optionsBuilder.Options);

            // This will create the database if it doesn't exist and apply any pending migrations
            await context.Database.MigrateAsync();

            logger.LogInformation("Database initialization completed for tenant '{TenantId}'.", tenant.TenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing database for tenant '{TenantId}'.", tenant.TenantId);

            // Optionally fall back to EnsureCreated if migrations fail
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseNpgsql(tenant.ConnectionString);

                using var context = new AppDbContext(optionsBuilder.Options);
                logger.LogWarning("Attempting to create database without migrations for tenant '{TenantId}'...", tenant.TenantId);
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database created using EnsureCreated for tenant '{TenantId}'.", tenant.TenantId);
            }
            catch (Exception innerEx)
            {
                logger.LogError(innerEx, "Failed to create database for tenant '{TenantId}'. Please check connection string.", tenant.TenantId);
                // Don't throw - allow other tenants to initialize
            }
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

// Add endpoint to list available tenants (useful for debugging/admin)
app.MapGet("/api/tenants", (ITenantService tenantService) => 
{
    return Results.Ok(tenantService.GetAllTenantIds());
}).WithTags("System");

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
