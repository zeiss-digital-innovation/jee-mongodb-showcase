namespace DotNetMongoDbBackend.Configurations;

/// <summary>
/// Zentrale Konstanten für MongoDB-Konfiguration
/// Diese Klasse stellt sicher, dass DB- und Collection-Namen überall konsistent verwendet werden
/// </summary>
public static class MongoConstants
{
    /// <summary>
    /// Name der MongoDB Datenbank
    /// Sollte immer 'demo-campus' sein (kompatibel mit JEE Backend)
    /// </summary>
    public const string DatabaseName = "demo-campus";
    
    /// <summary>
    /// Name der Point-of-Interest Collection
    /// Sollte immer 'point-of-interest' sein (singular, mit Bindestrich)
    /// </summary>
    public const string PoiCollectionName = "point-of-interest";
    
    /// <summary>
    /// Standard MongoDB Connection String für lokale Entwicklung
    /// </summary>
    public const string DefaultConnectionString = "mongodb://localhost:27017";
}