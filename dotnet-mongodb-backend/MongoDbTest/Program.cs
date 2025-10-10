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
            var databaseName = "demo_campus";

            try
            {
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(databaseName);

                Console.WriteLine($"Connection: {connectionString}");
                Console.WriteLine($"Database: {databaseName}");

                // Liste alle Collections auf
                var collections = await database.ListCollectionNamesAsync();
                var collectionsList = await collections.ToListAsync();
                Console.WriteLine("\nAvailable collections:");
                foreach (var coll in collectionsList)
                {
                    Console.WriteLine($"- {coll}");
                }

                // Test verschiedene Collection-Namen
                Console.WriteLine("\n=== Testing different collection names ===");

                // Test 1: point_of_interest (singular)
                try
                {
                    var collection1 = database.GetCollection<BsonDocument>("point_of_interest");
                    var count1 = await collection1.CountDocumentsAsync(new BsonDocument());
                    Console.WriteLine($"Collection 'point_of_interest' (singular): {count1} documents");

                    if (count1 > 0)
                    {
                        // Test TOILET (korrekte Schreibweise) statt toilette
                        var toiletFilter = Builders<BsonDocument>.Filter.Eq("category", "toilet");
                        var toiletCount = await collection1.CountDocumentsAsync(toiletFilter);
                        Console.WriteLine($"  - Documents with category 'toilet': {toiletCount}");

                        var toiletteFilter1 = Builders<BsonDocument>.Filter.Eq("category", "toilette");
                        var toiletteCount1 = await collection1.CountDocumentsAsync(toiletteFilter1);
                        Console.WriteLine($"  - Documents with category 'toilette': {toiletteCount1}");

                        // Test andere Kategorien
                        var healthFilter = Builders<BsonDocument>.Filter.Eq("category", "health");
                        var healthCount = await collection1.CountDocumentsAsync(healthFilter);
                        Console.WriteLine($"  - Documents with category 'health': {healthCount}");

                        var cashFilter = Builders<BsonDocument>.Filter.Eq("category", "cash");
                        var cashCount = await collection1.CountDocumentsAsync(cashFilter);
                        Console.WriteLine($"  - Documents with category 'cash': {cashCount}");

                        // Zeige alle verfügbaren Kategorien
                        var distinctCategories = await collection1.DistinctAsync<string>("category", new BsonDocument());
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
                        var firstDoc1 = await collection1.Find(new BsonDocument()).FirstOrDefaultAsync();
                        if (firstDoc1 != null)
                        {
                            Console.WriteLine($"  - First document structure:");
                            Console.WriteLine($"    {firstDoc1.ToJson()}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accessing 'point_of_interest': {ex.Message}");
                }

                // Test 2: points_of_interest (plural)
                try
                {
                    var collection2 = database.GetCollection<BsonDocument>("points_of_interest");
                    var count2 = await collection2.CountDocumentsAsync(new BsonDocument());
                    Console.WriteLine($"Collection 'points_of_interest' (plural): {count2} documents");

                    if (count2 > 0)
                    {
                        var toiletteFilter2 = Builders<BsonDocument>.Filter.Eq("category", "toilette");
                        var toiletteCount2 = await collection2.CountDocumentsAsync(toiletteFilter2);
                        Console.WriteLine($"  - Documents with category 'toilette': {toiletteCount2}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accessing 'points_of_interest': {ex.Message}");
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
