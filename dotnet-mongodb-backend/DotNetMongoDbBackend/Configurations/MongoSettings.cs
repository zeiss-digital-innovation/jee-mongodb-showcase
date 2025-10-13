namespace DotNetMongoDbBackend.Configurations;

/// <summary>
/// Zentrale MongoDB-Konfiguration
/// Definiert alle Datenbank- und Collection-Namen an einer Stelle
/// </summary>
public class MongoSettings
{
    public string ConnectionString { get; set; } = MongoConstants.DefaultConnectionString;
    
    /// <summary>
    /// Name der MongoDB Datenbank (sollte immer 'demo-campus' sein)
    /// </summary>
    public string Database { get; set; } = MongoConstants.DatabaseName;

    public CollectionNames Collections { get; set; } = new();

    public class CollectionNames
    {
        /// <summary>
        /// Name der Point-of-Interest Collection (sollte immer 'point-of-interest' sein)
        /// </summary>
        public string Pois { get; set; } = MongoConstants.PoiCollectionName;
    }
}
