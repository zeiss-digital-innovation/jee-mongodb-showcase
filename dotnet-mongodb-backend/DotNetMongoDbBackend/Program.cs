using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using DotNetMongoDbBackend.Configurations;
using DotNetMongoDbBackend.Models;
using DotNetMongoDbBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// URLs-Konfiguration - einheitlich Port 8080 mit korrekter Container-Bindung
if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    // Container: Bindung an alle Interfaces
    builder.WebHost.UseUrls("http://0.0.0.0:8080");
}
else
{
    // Lokal: localhost-Bindung
    builder.WebHost.UseUrls("http://localhost:8080");
}

// Bind Mongo settings
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoSettings"));

// Register MongoClient mit optimierten Timeouts für .NET 9
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var cfg = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    var mongoSettings = MongoClientSettings.FromConnectionString(cfg.ConnectionString);

    // Optimierte Connection-Settings für .NET 9
    mongoSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
    mongoSettings.ConnectTimeout = TimeSpan.FromSeconds(5);
    mongoSettings.SocketTimeout = TimeSpan.FromSeconds(5);
    mongoSettings.MaxConnectionPoolSize = 10;
    mongoSettings.MinConnectionPoolSize = 1;

    return new MongoClient(mongoSettings);
});

// Register IMongoDatabase
builder.Services.AddSingleton(sp =>
{
    var cfg = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(cfg.Database);
});

// Register IMongoCollection<PointOfInterest>
builder.Services.AddSingleton(sp =>
{
    var cfg = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    var db = sp.GetRequiredService<IMongoDatabase>();
    return db.GetCollection<PointOfInterest>(cfg.Collections.Pois);
});

// Register PointOfInterestService
builder.Services.AddScoped<IPointOfInterestService, PointOfInterestService>();

// Add controllers mit Newtonsoft.Json für .NET 9
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAngular");
app.MapControllers();

await app.RunAsync();

public partial class Program { protected Program() { } }
