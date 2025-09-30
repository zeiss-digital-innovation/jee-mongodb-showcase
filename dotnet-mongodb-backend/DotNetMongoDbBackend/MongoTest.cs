using MongoDB.Driver;
using MongoDB.Bson;
using System;

namespace MongoTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Einfacher MongoDB-Verbindungstest
            var connectionString = "mongodb://localhost:27017";
            var databaseName = "demo_campus";
            var collectionName = "point_of_interest";

            try
            {
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(databaseName);
                var collection = database.GetCollection<BsonDocument>(collectionName);

                Console.WriteLine($"=== MongoDB Test ===");
                Console.WriteLine($"Connection: {connectionString}");
                Console.WriteLine($"Database: {databaseName}");
                Console.WriteLine($"Collection: {collectionName}");

                // Test Verbindung
                var totalCount = await collection.CountDocumentsAsync(new BsonDocument());
                Console.WriteLine($"Total documents: {totalCount}");

                // Test Kategorie "toilette"
                var toiletteFilter = Builders<BsonDocument>.Filter.Eq("category", "toilette");
                var toiletteCount = await collection.CountDocumentsAsync(toiletteFilter);
                Console.WriteLine($"Documents with category 'toilette': {toiletteCount}");

                // Test verschiedene Collection-Namen
                try
                {
                    var altCollection = database.GetCollection<BsonDocument>("points_of_interest");
                    var altCount = await altCollection.CountDocumentsAsync(new BsonDocument());
                    Console.WriteLine($"Documents in 'points_of_interest' (plural): {altCount}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Collection 'points_of_interest' error: {ex.Message}");
                }

                // Zeige alle Collections
                var collections = await database.ListCollectionNamesAsync();
                var collectionsList = await collections.ToListAsync();
                Console.WriteLine("Available collections:");
                foreach (var coll in collectionsList)
                {
                    Console.WriteLine($"- {coll}");
                }

                // Zeige erste 2 Dokumente
                var firstDocs = await collection.Find(new BsonDocument()).Limit(2).ToListAsync();
                Console.WriteLine($"First 2 documents:");
                foreach (var doc in firstDocs)
                {
                    Console.WriteLine(doc.ToJson());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
            }

            Console.WriteLine("Test completed. Press any key to exit.");
            Console.ReadKey();
        }
    }
}
