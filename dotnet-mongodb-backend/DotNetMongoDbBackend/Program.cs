using Microsoft.Extensions.Options;
using MongoDB.Driver;
using DotNetMongoDbBackend.Configurations;
using DotNetMongoDbBackend.Models;
using DotNetMongoDbBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Fallback falls ASPNETCORE_URLS nicht gesetzt ist
if (string.IsNullOrEmpty(builder.Configuration["ASPNETCORE_URLS"]))
{
    builder.WebHost.UseUrls("http://+:80");
}

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
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
    policy.WithOrigins("http://localhost:4200") // Angular server (dev)
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

var app = builder.Build();
if(app.Environment.IsDevelopment()){
    app.UseHttpsRedirection();
}

app.UseCors("AllowAngular");
app.MapControllers();

await app.RunAsync();

// Expose Program for integration tests that use WebApplicationFactory<Program>
public partial class Program { protected Program() { } }
