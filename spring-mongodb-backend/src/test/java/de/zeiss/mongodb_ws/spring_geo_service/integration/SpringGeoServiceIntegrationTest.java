package de.zeiss.mongodb_ws.spring_geo_service.integration;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.mongodb.client.MongoClient;
import de.zeiss.mongodb_ws.spring_geo_service.persistence.IPointOfInterestRepository;
import de.zeiss.mongodb_ws.spring_geo_service.rest.model.PointOfInterest;
import org.geojson.Point;
import org.junit.jupiter.api.AfterAll;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.TestInstance;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.web.client.TestRestTemplate;
import org.springframework.boot.test.web.server.LocalServerPort;
import org.springframework.data.mongodb.core.MongoTemplate;
import org.springframework.data.mongodb.core.index.GeoSpatialIndexType;
import org.springframework.data.mongodb.core.index.GeospatialIndex;
import org.springframework.http.*;
import org.springframework.test.context.DynamicPropertyRegistry;
import org.springframework.test.context.DynamicPropertySource;
import org.testcontainers.junit.jupiter.Container;
import org.testcontainers.junit.jupiter.Testcontainers;
import org.testcontainers.mongodb.MongoDBContainer;

import java.net.URI;
import java.util.logging.Logger;

import static org.junit.jupiter.api.Assertions.*;

/**
 * Full CRUD integration tests using Testcontainers with a real MongoDB instance.
 * These tests start the entire Spring Boot application on a random port and execute
 * HTTP requests against the running server.
 * <p>
 * Tests will be skipped if Docker is not available on the host system.
 */
@SpringBootTest(webEnvironment = SpringBootTest.WebEnvironment.RANDOM_PORT)
@Testcontainers
@TestInstance(TestInstance.Lifecycle.PER_CLASS)
@DockerAvailable
public class SpringGeoServiceIntegrationTest {

    private static final Logger TEST_LOG = Logger.getLogger(SpringGeoServiceIntegrationTest.class.getName());

    @Container
    // make the container static so it starts before Spring context and works with static @DynamicPropertySource
    static MongoDBContainer mongoDBContainer = new MongoDBContainer("mongo:7.0");

    @DynamicPropertySource
    static void setProperties(DynamicPropertyRegistry registry) {
        registry.add("spring.data.mongodb.uri", () -> { // Defensive: ensure the container is running and return a usable connection string.
            try {
                if (!mongoDBContainer.isRunning()) {
                    mongoDBContainer.start();
                }
            } catch (Throwable ignored) {
            }
            String replicaUrl = null;
            try {
                replicaUrl = mongoDBContainer.getReplicaSetUrl();
            } catch (Throwable ignored) {
            }

            if (replicaUrl != null && !replicaUrl.trim().isEmpty()) {
                return replicaUrl;
            }

            // Fallback to mongodb://host:port
            String host = mongoDBContainer.getHost();
            int mappedPort = mongoDBContainer.getMappedPort(27017);
            return "mongodb://" + host + ":" + mappedPort;
        });
    }

    @LocalServerPort
    private int port;

    @Autowired
    private TestRestTemplate restTemplate;

    @Autowired
    private IPointOfInterestRepository poiRepository;

    @Autowired
    private MongoTemplate mongoTemplate;

    @Autowired
    private ObjectMapper objectMapper;

    @Autowired
    private MongoClient mongoClient;

    private String baseUrl() {
        return "http://localhost:" + port + "/zdi-geo-service/api/poi";
    }

    @BeforeEach
    void cleanup() {
        TEST_LOG.info("[TEST-CONTAINER] MongoDB mapped host port: " + mongoDBContainer.getMappedPort(27017) + ", firstMappedPort: " + mongoDBContainer.getFirstMappedPort() + ", replicaSetUrl: " + mongoDBContainer.getReplicaSetUrl());

        poiRepository.deleteAll();

        // Ensure geospatial 2dsphere index exists for GeoJSON location queries
        mongoTemplate.indexOps("point-of-interest").createIndex(
                new GeospatialIndex("location").typed(GeoSpatialIndexType.GEO_2DSPHERE)
        );
    }

    /**
     * Close MongoClient after all tests to prevent resource leaks.
     * <p>
     * Prevents "Prematurely reached end of stream” log when the container stops.
     */
    @AfterAll
    void shutdownMongoClient() {
        try {
            if (mongoClient != null) {
                TEST_LOG.info("[TEST-CONTAINER] Closing MongoClient before container stop.");
                mongoClient.close();
            }
        } catch (Exception e) {
            TEST_LOG.warning("[TEST-CONTAINER] Error closing MongoClient: " + e.getMessage());
        }
    }

    /**
     * Test CREATE operation: POST a new POI and verify it is created with status 201
     * and Location header is set.
     */
    @Test
    void testCreatePointOfInterest_ValidInput_ShouldReturnCreatedWithLocationHeader() {
        // Arrange
        PointOfInterest poi = new PointOfInterest();
        poi.setName("Integration Test POI");
        poi.setCategory("TestCategory");
        poi.setLocation(new Point(13.404954, 52.520008)); // Berlin coordinates
        poi.setDetails("This is a test POI for integration testing");

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);
        HttpEntity<PointOfInterest> request = new HttpEntity<>(poi, headers);

        // Act
        ResponseEntity<Void> createResponse = restTemplate.postForEntity(baseUrl(), request, Void.class);

        // Assert
        assertEquals(HttpStatus.CREATED, createResponse.getStatusCode());
        assertTrue(createResponse.getHeaders().containsKey(HttpHeaders.LOCATION));

        assertNotNull(createResponse.getHeaders().getLocation());
        String locationHeader = createResponse.getHeaders().getLocation().toString();
        assertNotNull(locationHeader);
        assertTrue(locationHeader.contains("/zdi-geo-service/api/poi/"));
    }

    /**
     * Test CREATE + READ: Create a POI, then GET it by following the Location header.
     */
    @Test
    void testCreateAndReadPointOfInterest_ShouldReturnSameData() {
        // Arrange
        PointOfInterest poi = new PointOfInterest();
        poi.setName("Test POI for Read");
        poi.setCategory("Museum");
        poi.setLocation(new Point(13.377704, 52.516275)); // Brandenburg Gate
        poi.setDetails("Historic landmark");

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);
        HttpEntity<PointOfInterest> request = new HttpEntity<>(poi, headers);

        // Act - Create
        ResponseEntity<Void> createResponse = restTemplate.postForEntity(baseUrl(), request, Void.class);
        URI location = createResponse.getHeaders().getLocation();
        assertNotNull(location);

        // Act - Read
        ResponseEntity<PointOfInterest> getResponse = restTemplate.getForEntity(location, PointOfInterest.class);

        // Assert
        assertEquals(HttpStatus.OK, getResponse.getStatusCode());
        PointOfInterest retrievedPoi = getResponse.getBody();
        assertNotNull(retrievedPoi);
        assertEquals("Test POI for Read", retrievedPoi.getName());
        assertEquals("Museum", retrievedPoi.getCategory());
        assertEquals("Historic landmark", retrievedPoi.getDetails());
        assertNotNull(retrievedPoi.getLocation());
        assertEquals(13.377704, retrievedPoi.getLocation().getCoordinates().getLongitude(), 0.000001);
        assertEquals(52.516275, retrievedPoi.getLocation().getCoordinates().getLatitude(), 0.000001);
        assertNotNull(retrievedPoi.getHref());
        assertTrue(retrievedPoi.getHref().contains(location.getPath()));
    }

    /**
     * Test UPDATE: Create a POI, then update it via PUT, verify 204 No Content.
     */
    @Test
    void testUpdatePointOfInterest_ExistingPOI_ShouldReturnNoContent() {
        // Arrange - Create initial POI
        PointOfInterest poi = new PointOfInterest();
        poi.setName("Original Name");
        poi.setCategory("Park");
        poi.setLocation(new Point(13.4, 52.5));

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);
        HttpEntity<PointOfInterest> createRequest = new HttpEntity<>(poi, headers);

        ResponseEntity<Void> createResponse = restTemplate.postForEntity(baseUrl(), createRequest, Void.class);
        URI location = createResponse.getHeaders().getLocation();
        assertNotNull(location);

        // Arrange - Modify POI for update
        PointOfInterest updatedPoi = new PointOfInterest();
        updatedPoi.setName("Updated Name");
        updatedPoi.setCategory("Museum");
        updatedPoi.setLocation(new Point(13.5, 52.6));
        updatedPoi.setDetails("Updated details");

        HttpEntity<PointOfInterest> updateRequest = new HttpEntity<>(updatedPoi, headers);

        // Act
        ResponseEntity<Void> updateResponse = restTemplate.exchange(
                location,
                HttpMethod.PUT,
                updateRequest,
                Void.class
        );

        // Assert
        assertEquals(HttpStatus.NO_CONTENT, updateResponse.getStatusCode());

        // Verify the update by reading back
        ResponseEntity<PointOfInterest> getResponse = restTemplate.getForEntity(location, PointOfInterest.class);
        PointOfInterest retrieved = getResponse.getBody();
        assertNotNull(retrieved);
        assertEquals("Updated Name", retrieved.getName());
        assertEquals("Museum", retrieved.getCategory());
        assertEquals("Updated details", retrieved.getDetails());
    }

    /**
     * Test UPDATE with non-existing ID: Should create new POI and return 201 Created.
     */
    @Test
    void testUpdatePointOfInterest_NewPOI_ShouldReturnCreated() {
        // Arrange
        String newId = "new-poi-id-12345";
        PointOfInterest poi = new PointOfInterest();
        poi.setName("New POI via PUT");
        poi.setCategory("Restaurant");
        poi.setLocation(new Point(13.3, 52.4));

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);
        HttpEntity<PointOfInterest> request = new HttpEntity<>(poi, headers);

        // Act
        ResponseEntity<Void> response = restTemplate.exchange(
                baseUrl() + "/" + newId,
                HttpMethod.PUT,
                request,
                Void.class
        );

        // Assert
        assertEquals(HttpStatus.CREATED, response.getStatusCode());
        assertTrue(response.getHeaders().containsKey(HttpHeaders.LOCATION));
    }

    /**
     * Test DELETE: Create a POI, then delete it, verify 204 No Content.
     * Then verify GET returns 404.
     */
    @Test
    void testDeletePointOfInterest_ExistingPOI_ShouldReturnNoContent() {
        // Arrange - Create POI
        PointOfInterest poi = new PointOfInterest();
        poi.setName("POI to Delete");
        poi.setCategory("Cafe");
        poi.setLocation(new Point(13.2, 52.3));

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);
        HttpEntity<PointOfInterest> createRequest = new HttpEntity<>(poi, headers);

        ResponseEntity<Void> createResponse = restTemplate.postForEntity(baseUrl(), createRequest, Void.class);
        URI location = createResponse.getHeaders().getLocation();
        assertNotNull(location);

        // Act - Delete
        ResponseEntity<Void> deleteResponse = restTemplate.exchange(
                location,
                HttpMethod.DELETE,
                null,
                Void.class
        );

        // Assert
        assertEquals(HttpStatus.NO_CONTENT, deleteResponse.getStatusCode());

        // Verify deletion - GET should return 404
        ResponseEntity<PointOfInterest> getResponse = restTemplate.getForEntity(location, PointOfInterest.class);
        assertEquals(HttpStatus.NOT_FOUND, getResponse.getStatusCode());
    }

    /**
     * Test DELETE with non-existing ID: Should return 404.
     */
    @Test
    void testDeletePointOfInterest_NonExistingPOI_ShouldReturnNotFound() {
        // Act
        ResponseEntity<Void> deleteResponse = restTemplate.exchange(
                baseUrl() + "/non-existing-id",
                HttpMethod.DELETE,
                null,
                Void.class
        );

        // Assert
        assertEquals(HttpStatus.NOT_FOUND, deleteResponse.getStatusCode());
    }

    /**
     * Test SEARCH/FIND: Create multiple POIs, then search within a radius.
     */
    @Test
    void testFindPointsOfInterest_WithinRadius_ShouldReturnMatchingPOIs() {
        // Arrange - Create multiple POIs near Berlin
        PointOfInterest poi1 = new PointOfInterest();
        poi1.setName("POI 1 - Brandenburg Gate");
        poi1.setCategory("Monument");
        poi1.setLocation(new Point(13.377704, 52.516275));

        PointOfInterest poi2 = new PointOfInterest();
        poi2.setName("POI 2 - Reichstag");
        poi2.setCategory("Government");
        poi2.setLocation(new Point(13.376198, 52.518623));

        PointOfInterest poi3 = new PointOfInterest();
        poi3.setName("POI 3 - Far Away");
        poi3.setCategory("Other");
        poi3.setLocation(new Point(10.0, 50.0)); // Far from Berlin

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);

        restTemplate.postForEntity(baseUrl(), new HttpEntity<>(poi1, headers), Void.class);
        restTemplate.postForEntity(baseUrl(), new HttpEntity<>(poi2, headers), Void.class);
        restTemplate.postForEntity(baseUrl(), new HttpEntity<>(poi3, headers), Void.class);

        // Act - Search near Brandenburg Gate with 1km radius
        String searchUrl = baseUrl() + "?lat=52.516275&lon=13.377704&radius=1000";
        ResponseEntity<PointOfInterest[]> searchResponse = restTemplate.getForEntity(searchUrl, PointOfInterest[].class);

        // Assert
        assertEquals(HttpStatus.OK, searchResponse.getStatusCode());
        PointOfInterest[] results = searchResponse.getBody();
        assertNotNull(results);
        assertEquals(2, results.length); // Should find POI 1 and POI 2, not POI 3
    }

    /**
     * Test validation: Create POI with invalid data (missing name) should return 400.
     */
    @Test
    void testCreatePointOfInterest_InvalidData_ShouldReturnBadRequest() throws Exception {
        // Arrange - POI missing name
        PointOfInterest poi = new PointOfInterest();
        poi.setCategory("TestCategory");
        poi.setLocation(new Point(13.4, 52.5));
        // name is missing

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);
        HttpEntity<PointOfInterest> request = new HttpEntity<>(poi, headers);

        // Act
        ResponseEntity<String> response = restTemplate.postForEntity(baseUrl(), request, String.class);

        // Assert
        assertEquals(HttpStatus.BAD_REQUEST, response.getStatusCode());

        // Verify error response contains validation details for "name"
        String body = response.getBody();
        assertNotNull(body);
        assertTrue(body.contains("name") || body.contains("Name"));
    }

    /**
     * Test validation: Create POI with invalid coordinates should return 400.
     */
    @Test
    void testCreatePointOfInterest_InvalidCoordinates_ShouldReturnBadRequest() {
        // Arrange - Invalid longitude (outside -180 to 180 range)
        PointOfInterest poi = new PointOfInterest();
        poi.setName("Invalid POI");
        poi.setCategory("Test");
        poi.setLocation(new Point(200.0, 52.5)); // Invalid longitude

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);
        HttpEntity<PointOfInterest> request = new HttpEntity<>(poi, headers);

        // Act
        ResponseEntity<String> response = restTemplate.postForEntity(baseUrl(), request, String.class);

        // Assert
        assertEquals(HttpStatus.BAD_REQUEST, response.getStatusCode());

        // Verify error response contains validation details for "location"
        String body = response.getBody();
        assertNotNull(body);
        assertTrue(body.contains("location") || body.contains("coordinate"));
    }

    /**
     * Test GET by ID with non-existing ID should return 404.
     */
    @Test
    void testGetPointOfInterest_NonExistingId_ShouldReturnNotFound() {
        // Act
        ResponseEntity<PointOfInterest> response = restTemplate.getForEntity(
                baseUrl() + "/non-existing-id-xyz",
                PointOfInterest.class
        );

        // Assert
        assertEquals(HttpStatus.NOT_FOUND, response.getStatusCode());
    }

    /**
     * Full CRUD cycle test: Create -> Read -> Update -> Read -> Delete -> Verify deletion.
     */
    @Test
    void testFullCRUDCycle_ShouldWorkEndToEnd() {
        // 1. CREATE
        PointOfInterest poi = new PointOfInterest();
        poi.setName("CRUD Test POI");
        poi.setCategory("Test");
        poi.setLocation(new Point(13.4, 52.5));
        poi.setDetails("Initial details");

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);
        HttpEntity<PointOfInterest> createRequest = new HttpEntity<>(poi, headers);

        ResponseEntity<Void> createResponse = restTemplate.postForEntity(baseUrl(), createRequest, Void.class);
        assertEquals(HttpStatus.CREATED, createResponse.getStatusCode());
        URI location = createResponse.getHeaders().getLocation();
        assertNotNull(location);

        // 2. READ
        ResponseEntity<PointOfInterest> readResponse1 = restTemplate.getForEntity(location, PointOfInterest.class);
        assertEquals(HttpStatus.OK, readResponse1.getStatusCode());
        assertNotNull(readResponse1.getBody());
        assertEquals("CRUD Test POI", readResponse1.getBody().getName());
        assertEquals("Initial details", readResponse1.getBody().getDetails());

        // 3. UPDATE
        PointOfInterest updatedPoi = new PointOfInterest();
        updatedPoi.setName("Updated CRUD POI");
        updatedPoi.setCategory("UpdatedCategory");
        updatedPoi.setLocation(new Point(13.5, 52.6));
        updatedPoi.setDetails("Updated details");

        HttpEntity<PointOfInterest> updateRequest = new HttpEntity<>(updatedPoi, headers);
        ResponseEntity<Void> updateResponse = restTemplate.exchange(location, HttpMethod.PUT, updateRequest, Void.class);
        assertEquals(HttpStatus.NO_CONTENT, updateResponse.getStatusCode());

        // 4. READ again to verify update
        ResponseEntity<PointOfInterest> readResponse2 = restTemplate.getForEntity(location, PointOfInterest.class);
        assertEquals(HttpStatus.OK, readResponse2.getStatusCode());
        assertNotNull(readResponse2.getBody());
        assertEquals("Updated CRUD POI", readResponse2.getBody().getName());
        assertEquals("Updated details", readResponse2.getBody().getDetails());
        assertEquals("UpdatedCategory", readResponse2.getBody().getCategory());

        // 5. DELETE
        ResponseEntity<Void> deleteResponse = restTemplate.exchange(location, HttpMethod.DELETE, null, Void.class);
        assertEquals(HttpStatus.NO_CONTENT, deleteResponse.getStatusCode());

        // 6. Verify deletion
        ResponseEntity<PointOfInterest> readResponse3 = restTemplate.getForEntity(location, PointOfInterest.class);
        assertEquals(HttpStatus.NOT_FOUND, readResponse3.getStatusCode());
    }
}
