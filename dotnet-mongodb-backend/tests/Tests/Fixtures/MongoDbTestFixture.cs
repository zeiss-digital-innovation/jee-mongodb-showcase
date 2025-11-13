using System;
using System.Threading.Tasks;
using DotNetMongoDbBackend.Configurations;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit;

namespace DotNetMongoDbBackend.Tests.Tests.Fixtures;

/// <summary>
/// Base test fixture for MongoDB integration tests using Testcontainers.
/// Provides a real MongoDB instance running in a Docker container for integration testing.
/// </summary>
#nullable enable
public class MongoDbTestFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer;
    private IMongoClient? _mongoClient;
    private IMongoDatabase? _database;
#nullable restore
    
    /// <summary>
    /// Unique database name for this test run to enable parallel test execution
    /// </summary>
    public string DatabaseName { get; }
    
    /// <summary>
    /// MongoDB connection string for the test container
    /// </summary>
    public string ConnectionString => _mongoContainer.GetConnectionString();
    
    /// <summary>
    /// MongoClient instance connected to the test container
    /// </summary>
    public IMongoClient MongoClient => _mongoClient 
        ?? throw new InvalidOperationException("MongoClient not initialized. Call InitializeAsync first.");
    
    /// <summary>
    /// MongoDB database instance for testing
    /// </summary>
    public IMongoDatabase Database => _database 
        ?? throw new InvalidOperationException("Database not initialized. Call InitializeAsync first.");

    public MongoDbTestFixture()
    {
        // Generate unique database name to allow parallel test execution
        DatabaseName = $"test_db_{Guid.NewGuid():N}";
        
        // Configure MongoDB container
        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:8.0")
            .WithPortBinding(27017, true) // Random host port
            .Build();
    }

    /// <summary>
    /// Initialize the MongoDB container and create database connection.
    /// Called automatically by xUnit before tests run.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Start MongoDB container
        await _mongoContainer.StartAsync();
        
        // Create MongoDB client and database
        _mongoClient = new MongoClient(ConnectionString);
        _database = _mongoClient.GetDatabase(DatabaseName);
        
        // Create 2dsphere index for geo-spatial queries
        var collection = _database.GetCollection<BsonDocument>("pois");
        var indexKeys = Builders<BsonDocument>.IndexKeys.Geo2DSphere("location");
        var indexModel = new CreateIndexModel<BsonDocument>(indexKeys);
        await collection.Indexes.CreateOneAsync(indexModel);
    }

    /// <summary>
    /// Cleanup: Stop MongoDB container and dispose resources.
    /// Called automatically by xUnit after tests complete.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_mongoContainer != null)
        {
            await _mongoContainer.StopAsync();
            await _mongoContainer.DisposeAsync();
        }
    }

    /// <summary>
    /// Get configured MongoSettings for dependency injection in tests
    /// </summary>
    public IOptions<MongoSettings> GetMongoSettings()
    {
        var settings = new MongoSettings
        {
            ConnectionString = ConnectionString,
            Database = DatabaseName,
            Collections = new MongoSettings.CollectionNames
            {
                Pois = "pois"
            }
        };
        return Options.Create(settings);
    }

    /// <summary>
    /// Clear all data from the POIs collection between tests
    /// </summary>
    public async Task ClearCollectionAsync()
    {
        var collection = Database.GetCollection<BsonDocument>("pois");
        await collection.DeleteManyAsync(Builders<BsonDocument>.Filter.Empty);
    }
}
