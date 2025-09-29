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
        name: "Sample Location 1",
        description: "A sample point of interest",
        location: {
            type: "Point",
            coordinates: [13.4050, 52.5200] // Berlin, Germany [longitude, latitude]
        },
        category: "landmark",
        created_at: new Date()
    },
    {
        name: "Sample Location 2",
        description: "Another sample point of interest",
        location: {
            type: "Point",
            coordinates: [2.3522, 48.8566] // Paris, France [longitude, latitude]
        },
        category: "tourist_attraction",
        created_at: new Date()
    },
    {
        name: "Sample Location 3",
        description: "Third sample point of interest",
        location: {
            type: "Point",
            coordinates: [-74.0060, 40.7128] // New York, USA [longitude, latitude]
        },
        category: "city",
        created_at: new Date()
    }
]);

print("Database 'demo-campus' initialized successfully!");
print("Collection 'point_of_interest' created with 2dsphere index!");
print("Sample data inserted!");