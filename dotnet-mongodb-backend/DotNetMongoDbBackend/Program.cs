using Microsoft.Extensions.Options;
using MongoDB.Driver;
using DotNetMongoDbBackend.Configurations;
using DotNetMongoDbBackend.Models;

var builder = WebApplication.CreateBuilder(args);

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

// Add controllers (existing app uses controllers elsewhere)
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();

// Expose Program for integration tests that use WebApplicationFactory<Program>
public partial class Program { protected Program() { } }
