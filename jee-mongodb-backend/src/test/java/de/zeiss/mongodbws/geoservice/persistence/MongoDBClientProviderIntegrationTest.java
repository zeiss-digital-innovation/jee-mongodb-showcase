package de.zeiss.mongodbws.geoservice.persistence;

import dev.morphia.Datastore;
import org.junit.jupiter.api.*;
import org.testcontainers.mongodb.MongoDBContainer;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;

class MongoDBClientProviderIntegrationTest {

    static MongoDBContainer mongoDBContainer;
    MongoDBClientProvider provider;

    @BeforeAll
    static void startMongo() {
        mongoDBContainer = new MongoDBContainer("mongo:7.0");
        mongoDBContainer.start();
    }

    @AfterAll
    static void stopMongo() {
        mongoDBContainer.stop();
    }

    @BeforeEach
    void setUp() {
        provider = new MongoDBClientProvider();
        provider.hostname = mongoDBContainer.getHost();
        provider.port = mongoDBContainer.getFirstMappedPort();
        provider.databaseName = "test-db";
        provider.init();
    }

    @AfterEach
    void tearDown() {
        provider.preDestroy();
    }

    @Test
    void testInitCreatesDatastore() {
        Datastore ds = provider.getDatastore();
        assertNotNull(ds);
        assertEquals("test-db", ds.getDatabase().getName());
    }

    @Test
    void testPreDestroyClosesClient() {
        provider.preDestroy();
        // No direct way to check if closed, but no exception should be thrown
    }
}