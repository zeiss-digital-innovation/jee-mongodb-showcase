using Microsoft.Extensions.Options;
using MongoDB.Driver;
using DotNetMongoDbBackend.Configurations;
using DotNetMongoDbBackend.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;
using DotNetMongoDbBackend.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

// Configure logging with custom timestamp format (updated for .NET 9)
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "dd.MM.yy HH:mm ";
    options.IncludeScopes = false;
});

// URL configuration - unified port 8080 with correct container binding
if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    // Container: Bind to all interfaces
    builder.WebHost.UseUrls("http://0.0.0.0:8080");
}
else
{
    // Local: Bind to localhost
    builder.WebHost.UseUrls("http://localhost:8080");
}

// Bind Mongo settings
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoSettings"));

// Register MongoClient with optimized timeouts for .NET 9
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var cfg = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    var mongoSettings = MongoClientSettings.FromConnectionString(cfg.ConnectionString);

    // Optimized connection settings for .NET 9
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
    return db.GetCollection<PointOfInterestEntity>(cfg.Collections.Pois);
});

// Register PointOfInterestService as Singleton (not Scoped) 
// This ensures indexes are created only ONCE at startup, not on every request
builder.Services.AddSingleton<IPointOfInterestService, PointOfInterestService>();

// Use System.Text.Json for controller JSON (preferred for performance and default in .NET)
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        // Use camelCase in JSON output to match typical JS clients
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        // Ignore null values to reduce payload size (equivalent to NullValueHandling.Ignore)
        opts.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        // Note: System.Text.Json uses ISO-8601 for DateTime. If you need custom DateTime handling, add a converter here.
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

var serviceBase = builder.Configuration.GetValue<string>("ServiceBase");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Poi Service API",
        Description = "API for managing Points of Interest (POI)",
        Version = "v1"
    });
    // Add server information so the Swagger UI uses the correct API base (including '/api')
    if (!string.IsNullOrWhiteSpace(serviceBase))
    {
        c.AddServer(new Microsoft.OpenApi.Models.OpenApiServer { Url = $"/{serviceBase}/api" });
    }
});

// Forwarded Headers: configure support for X-Forwarded-For / X-Forwarded-Proto when hosting behind a reverse proxy
// This is important so LinkGenerator and URL generation use the original scheme/host forwarded by the proxy.
// Note: for security, restrict KnownProxies/KnownNetworks in production instead of allowing all.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Example: options.KnownProxies.Add(IPAddress.Parse("10.0.0.100"));
    // Example: options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("10.0.0.0"), 24));
});

var app = builder.Build();

// Apply forwarded headers before modifying PathBase so PathBase and generated URLs reflect original host/proto
app.UseForwardedHeaders();

// Serve the application under a base path so the base URL starts with /{serviceBase}
// Map the API controllers under /api so final API base becomes /{serviceBase}/api
app.UsePathBase($"/{serviceBase}");


// Swagger JSON will be available under the application PathBase + /swagger/v1/swagger.json
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // Serve the UI at {PathBase}/swagger (RoutePrefix defaults to "swagger")
    c.SwaggerEndpoint($"/{serviceBase}/swagger/v1/swagger.json", "Poi Service API V1");
    c.RoutePrefix = "swagger"; // explicit for clarity
});

app.UseCors("AllowAngular");

// Map all controllers under the /api path so they are reachable at {PathBase}/api/...
app.Map("/api", apiApp =>
{
    // Important: configure the branch to use routing and map controllers
    apiApp.UseRouting();
    apiApp.UseCors("AllowAngular");
    apiApp.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
});

await app.RunAsync();

public partial class Program { protected Program() { } }
