namespace DotNetMongoDbBackend.Configurations;

public class MongoSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "poi-db";

    public CollectionNames Collections { get; set; } = new();

    public class CollectionNames
    {
        public string Pois { get; set; } = "pois";
    }
}
