package de.zeiss.mongodbws.geoservice.persistence;

import com.mongodb.client.MongoClient;
import com.mongodb.client.MongoClients;
import com.mongodb.client.model.Indexes;
import de.zeiss.mongodbws.geoservice.config.TestConfig;
import de.zeiss.mongodbws.geoservice.integration.DockerAvailable;
import de.zeiss.mongodbws.geoservice.persistence.entity.GeoPoint;
import de.zeiss.mongodbws.geoservice.persistence.entity.PointOfInterestEntity;
import dev.morphia.Datastore;
import dev.morphia.Morphia;
import org.bson.types.ObjectId;
import org.junit.jupiter.api.*;
import org.testcontainers.mongodb.MongoDBContainer;

import java.util.List;

import static org.junit.jupiter.api.Assertions.*;

@TestInstance(TestInstance.Lifecycle.PER_CLASS)
@DockerAvailable
class PersistenceServiceIntegrationTest {
    static MongoDBContainer mongoDBContainer;
    MongoClient mongoClient;
    Datastore datastore;
    PersistenceService persistenceService;
    MongoDBClientProvider mongoDBClientProvider;

    @BeforeAll
    void startMongo() {
        mongoDBContainer = new MongoDBContainer(TestConfig.MONGODB_IMAGE);
        mongoDBContainer.start();
    }

    @AfterAll
    void stopMongo() {
        mongoDBContainer.stop();
    }

    @BeforeEach
    void setUp() {
        String connectionString = mongoDBContainer.getConnectionString();
        mongoClient = MongoClients.create(connectionString);

        // No explicit mapping here: Morphia will pick up annotated entity classes at runtime.
        datastore = Morphia.createDatastore(mongoClient, "test-db");

        // datastore.ensureIndexes() is deprecated, as a workaround creating the geospatial index manually
        datastore.getDatabase()
                .getCollection("point-of-interest")
                .createIndex(Indexes.geo2dsphere("location"));

        mongoDBClientProvider = new MongoDBClientProvider();
        mongoDBClientProvider.mongoClient = mongoClient;
        mongoDBClientProvider.datastore = datastore;
        persistenceService = new PersistenceService();
        persistenceService.mongoDBClientProvider = mongoDBClientProvider;
    }

    @AfterEach
    void tearDown() {
        mongoClient.close();
    }

    @Test
    void testCreateAndGetPointOfInterest() {
        PointOfInterestEntity entity = new PointOfInterestEntity();
        entity.setCategory("test-category");
        entity.setDetails("test-details");
        entity.setLocation(new GeoPoint(51.0, 13.0));
        persistenceService.createPointOfInterest(entity);
        assertNotNull(entity.getId());
        PointOfInterestEntity found = persistenceService.getPointOfInterest(entity.getId(), true);
        assertNotNull(found);
        assertEquals("test-category", found.getCategory());
        assertEquals("test-details", found.getDetails());
        assertEquals(51.0, found.getLocation().getLatitude());
        assertEquals(13.0, found.getLocation().getLongitude());
    }

    @Test
    void testGetPointOfInterestWithoutDetails() {
        PointOfInterestEntity entity = new PointOfInterestEntity();
        entity.setCategory("test-category");
        entity.setDetails("test-details");
        entity.setLocation(new GeoPoint(51.0, 13.0));
        persistenceService.createPointOfInterest(entity);
        PointOfInterestEntity found = persistenceService.getPointOfInterest(entity.getId(), false);
        assertNotNull(found);
        assertEquals("test-category", found.getCategory());
        assertNull(found.getDetails());
    }

    @Test
    void testUpdatePointOfInterest() {
        PointOfInterestEntity entity = new PointOfInterestEntity();
        entity.setCategory("cat1");
        entity.setDetails("details1");
        entity.setLocation(new GeoPoint(51.0, 13.0));
        persistenceService.createPointOfInterest(entity);
        entity.setCategory("cat2");
        entity.setDetails("details2");
        persistenceService.updatePointOfInterest(entity);
        PointOfInterestEntity updated = persistenceService.getPointOfInterest(entity.getId(), true);
        assertEquals("cat2", updated.getCategory());
        assertEquals("details2", updated.getDetails());
    }

    @Test
    void testDeletePointOfInterest() {
        PointOfInterestEntity entity = new PointOfInterestEntity();
        entity.setCategory("cat");
        entity.setDetails("details");
        entity.setLocation(new GeoPoint(51.0, 13.0));
        persistenceService.createPointOfInterest(entity);
        ObjectId id = entity.getId();
        persistenceService.deletePointOfInterest(id);
        PointOfInterestEntity deleted = persistenceService.getPointOfInterest(id, true);
        assertNull(deleted);
    }

    @Test
    void testListPOIsWithDetails() {
        PointOfInterestEntity entity1 = new PointOfInterestEntity();
        entity1.setCategory("cat1");
        entity1.setDetails("details1");
        entity1.setLocation(new GeoPoint(51.0, 13.0));
        entity1 = persistenceService.createPointOfInterest(entity1);
        PointOfInterestEntity entity2 = new PointOfInterestEntity();
        entity2.setCategory("cat2");
        entity2.setDetails("details2");
        entity2.setLocation(new GeoPoint(51.0001, 13.0001));
        entity2 = persistenceService.createPointOfInterest(entity2);
        List<PointOfInterestEntity> results = persistenceService.listPOIs(51.0, 13.0, 1000, true);
        assertTrue(results.size() >= 2);

        for (PointOfInterestEntity poi : results) {
            assertNotNull(poi.getDetails());
        }

        // cleanup
        persistenceService.deletePointOfInterest(entity1.getId());
        persistenceService.deletePointOfInterest(entity2.getId());

        assertNull(persistenceService.getPointOfInterest(entity1.getId(), false));
        assertNull(persistenceService.getPointOfInterest(entity2.getId(), false));
    }

    @Test
    void testListPOIsWithoutDetails() {
        PointOfInterestEntity entity1 = new PointOfInterestEntity();
        entity1.setCategory("cat1");
        entity1.setDetails("details1");
        entity1.setLocation(new GeoPoint(51.0, 13.0));
        persistenceService.createPointOfInterest(entity1);
        PointOfInterestEntity entity2 = new PointOfInterestEntity();
        entity2.setCategory("cat2");
        entity2.setDetails("details2");
        entity2.setLocation(new GeoPoint(51.0001, 13.0001));
        persistenceService.createPointOfInterest(entity2);
        List<PointOfInterestEntity> results = persistenceService.listPOIs(51.0, 13.0, 1000, false);
        assertTrue(results.size() >= 2);

        for (PointOfInterestEntity poi : results) {
            assertNull(poi.getDetails());
        }

        // cleanup
        persistenceService.deletePointOfInterest(entity1.getId());
        persistenceService.deletePointOfInterest(entity2.getId());

        assertNull(persistenceService.getPointOfInterest(entity1.getId(), false));
        assertNull(persistenceService.getPointOfInterest(entity2.getId(), false));
    }
}
