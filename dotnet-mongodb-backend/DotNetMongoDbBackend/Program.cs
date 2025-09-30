using Microsoft.Extensions.Options;
using MongoDB.Driver;
using DotNetMongoDbBackend.Configurations;
using DotNetMongoDbBackend.Models;
using DotNetMongoDbBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Fallback falls ASPNETCORE_URLS nicht gesetzt ist
if (string.IsNullOrEmpty(builder.Configuration["ASPNETCORE_URLS"]))
{
    builder.WebHost.UseUrls("https://+:443;http://+:80");
}

// HTTPS-Redirection für alle Umgebungen aktivieren
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    options.HttpsPort = 5001;
});

// Bind Mongo settings
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoSettings"));

// Register MongoClient as singleton
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var cfg = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return new MongoClient(cfg.ConnectionString);
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

// Add controllers (existing app uses controllers elsewhere)
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        // Verwende Newtonsoft.Json als Workaround für .NET 10 RC JSON Serialization Bug
        options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

// Alternative JSON-Serializer für Integration Tests
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.DefaultBufferSize = 16384;
    options.SerializerOptions.WriteIndented = false;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200") // HTTP und HTTPS für Angular
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// HTTPS-Redirection für alle Umgebungen aktivieren (nicht nur Development)
app.UseHttpsRedirection();

app.UseCors("AllowAngular");
app.MapControllers();

await app.RunAsync();

// Expose Program for integration tests that use WebApplicationFactory<Program>
public partial class Program { protected Program() { } }
