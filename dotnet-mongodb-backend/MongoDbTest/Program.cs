using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;

namespace MongoDbTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== MongoDB Direct Test ===");

            var connectionString = "mongodb://localhost:27017";
            var databaseName = "demo-campus";
            var collectionName = "point-of-interest"; // Korrekte Collection mit Bindestrich

            try
            {
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(databaseName);

                Console.WriteLine($"Connection: {connectionString}");
                Console.WriteLine($"Database: {databaseName}");
                Console.WriteLine($"Collection: {collectionName}");

                // Liste alle Collections auf
                var collections = await database.ListCollectionNamesAsync();
                var collectionsList = await collections.ToListAsync();
                Console.WriteLine("\nAvailable collections:");
                foreach (var coll in collectionsList)
                {
                    Console.WriteLine($"- {coll}");
                }

                // Test korrekte Collection
                Console.WriteLine("\n=== Testing correct collection ===");

                try
                {
                    var collection = database.GetCollection<BsonDocument>(collectionName);
                    var count = await collection.CountDocumentsAsync(new BsonDocument());
                    Console.WriteLine($"Collection '{collectionName}': {count} documents");

                    if (count > 0)
                    {
                        // Test TOILET (korrekte Schreibweise) statt toilette
                        var toiletFilter = Builders<BsonDocument>.Filter.Eq("category", "toilet");
                        var toiletCount = await collection.CountDocumentsAsync(toiletFilter);
                        Console.WriteLine($"  - Documents with category 'toilet': {toiletCount}");

                        var toiletteFilter = Builders<BsonDocument>.Filter.Eq("category", "toilette");
                        var toiletteCount = await collection.CountDocumentsAsync(toiletteFilter);
                        Console.WriteLine($"  - Documents with category 'toilette': {toiletteCount}");

                        // Test andere Kategorien
                        var healthFilter = Builders<BsonDocument>.Filter.Eq("category", "health");
                        var healthCount = await collection.CountDocumentsAsync(healthFilter);
                        Console.WriteLine($"  - Documents with category 'health': {healthCount}");

                        var cashFilter = Builders<BsonDocument>.Filter.Eq("category", "cash");
                        var cashCount = await collection.CountDocumentsAsync(cashFilter);
                        Console.WriteLine($"  - Documents with category 'cash': {cashCount}");

                        // Zeige alle verfügbaren Kategorien
                        var distinctCategories = await collection.DistinctAsync<string>("category", new BsonDocument());
                        var categoriesList = await distinctCategories.ToListAsync();
                        Console.WriteLine($"  - Available categories ({categoriesList.Count}):");
                        foreach (var cat in categoriesList.Take(10))
                        {
                            Console.WriteLine($"    * {cat}");
                        }
                        if (categoriesList.Count > 10)
                        {
                            Console.WriteLine($"    ... and {categoriesList.Count - 10} more");
                        }

                        // Zeige erste Dokument
                        var firstDoc = await collection.Find(new BsonDocument()).FirstOrDefaultAsync();
                        if (firstDoc != null)
                        {
                            Console.WriteLine($"  - First document structure:");
                            Console.WriteLine($"    {firstDoc.ToJson()}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accessing '{collectionName}': {ex.Message}");
                }

                Console.WriteLine("\n=== Test completed ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
