using FourniProApi.Data;
using FourniProApi.Mapping;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWindowsService();

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(UserProfile));

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
        npgsql.CommandTimeout(60);
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// Auto-create DB and apply all pending migrations on startup.
// Safe to run every time — skips if already up to date.
using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

    var dbName = db.Database.GetDbConnection().Database;

    if (await db.Database.CanConnectAsync())
    {
        logger.LogInformation("Connecting to database: {DbName}", dbName);
    }
    else
    {
        logger.LogInformation("Creating database: {DbName}", dbName);
    }

    await db.Database.MigrateAsync();

    logger.LogInformation("Database ready: {DbName}", dbName);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
