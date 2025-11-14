package de.zeiss.mongodbws.geoservice.persistence;

import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertDoesNotThrow;
import static org.junit.jupiter.api.Assertions.assertNotNull;

class MongoDBClientProviderTest {

    @Test
    public void testMongoDBClientProvider_PreDestroyWontFail_WithoutClient() {
        MongoDBClientProvider provider = new MongoDBClientProvider();
        assertNotNull(provider);

        // Call preDestroy without initializing the client
        assertDoesNotThrow(provider::preDestroy);
    }
}