// MongoDB initialization script
// This script will be executed when the MongoDB container starts for the first time

// Switch to the demo-campus database
db = db.getSiblingDB('demo-campus');

// Create the point_of_interest collection
db.createCollection('point_of_interest');

// Create the required 2dsphere index for location-based queries
db.point_of_interest.createIndex({ location: "2dsphere" });

// Insert sample data (optional - remove if not needed)
db.point_of_interest.insertMany([
    {
        category: "company",
        details: "Carl Zeiss Digital Innovation GmbH, Fritz-Foerster-Platz 2, 01069 Dresden, Tel.: +49 (0)351 497 01-500,https://www.zeiss.de/digital-innovation",
        location: {
            type: "Point",
            coordinates: [13.730123, 51.050407] // Dresden, Germany [longitude, latitude]
        }
    },
    {
        category: "company",
        details: "Carl Zeiss Digital Innovation Hungary Kft., Miskolc, Arany János tér 1, 3526 Ungarn,https://www.zeiss.com/digital-innovation",
        location: {
            type: "Point",
            coordinates: [20.787347, 48.107337] // Miskolc, Hungary [longitude, latitude]
        }
    }
]);

print("Database 'demo-campus' initialized successfully!");
print("Collection 'point_of_interest' created with 2dsphere index!");
print("Sample data inserted!");