/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * <p>
 * Copyright (C) 2025 Carl Zeiss Digital Innovation GmbH
 */
package de.zeiss.mongodbws.geoservice.service;

import de.zeiss.mongodbws.geoservice.persistence.PersistenceService;
import de.zeiss.mongodbws.geoservice.persistence.entity.GeoPoint;
import de.zeiss.mongodbws.geoservice.persistence.entity.PointOfInterestEntity;
import de.zeiss.mongodbws.geoservice.rest.resource.PointOfInterest;
import org.bson.types.ObjectId;
import org.geojson.Point;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import java.util.Arrays;
import java.util.List;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.Mockito.*;

/**
 * Unit tests for {@link GeoDataService}
 *
 * @author Generated Tests
 */
@ExtendWith(MockitoExtension.class)
public class GeoDataServiceTest {

    @Mock
    private PersistenceService persistenceService;

    @InjectMocks
    private GeoDataService geoDataService;

    private ObjectId testObjectId;
    private PointOfInterestEntity testEntity;

    private static final String NAME_RESTAURANT = "Pizza Place";
    private static final String CATEGORY_RESTAURANT = "restaurant";
    private static final String DETAILS_RESTAURANT = "Best restaurant in town";
    private static final double LATITUDE_RESTAURANT = 51.0504;
    private static final double LONGITUDE_RESTAURANT = 13.7373;

    @BeforeEach
    public void setUp() {
        testObjectId = new ObjectId();

        // Setup test entity
        testEntity = new PointOfInterestEntity();
        testEntity.setId(testObjectId);
        testEntity.setName(NAME_RESTAURANT);
        testEntity.setCategory(CATEGORY_RESTAURANT);
        testEntity.setDetails(DETAILS_RESTAURANT);
        testEntity.setLocation(new GeoPoint(LATITUDE_RESTAURANT, LONGITUDE_RESTAURANT));
    }

    @Test
    public void testGetPOI_ValidIdAndExpandDetails_ShouldReturnPointOfInterest() {
        // Given
        String id = testObjectId.toString();
        when(persistenceService.getPointOfInterest(testObjectId, true)).thenReturn(testEntity);

        // When
        PointOfInterest result = geoDataService.getPOI(id, true);

        // Then
        assertNotNull(result);
        assertEquals(testObjectId.toString(), result.getId());
        assertEquals(NAME_RESTAURANT, result.getName());
        assertEquals(CATEGORY_RESTAURANT, result.getCategory());
        assertEquals(DETAILS_RESTAURANT, result.getDetails());
        assertNotNull(result.getLocation());
        assertEquals(LONGITUDE_RESTAURANT, result.getLocation().getCoordinates().getLongitude(), 0.0001);
        assertEquals(LATITUDE_RESTAURANT, result.getLocation().getCoordinates().getLatitude(), 0.0001);
        verify(persistenceService).getPointOfInterest(testObjectId, true);
    }

    @Test
    public void testGetPOI_ValidIdNoExpandDetails_ShouldReturnPointOfInterest() {
        // Given
        String id = testObjectId.toString();
        testEntity.setDetails(null); // Simulate no details when not expanded
        when(persistenceService.getPointOfInterest(testObjectId, false)).thenReturn(testEntity);

        // When
        PointOfInterest result = geoDataService.getPOI(id, false);

        // Then
        assertNotNull(result);
        assertEquals(testObjectId.toString(), result.getId());
        assertNull(result.getDetails());
        verify(persistenceService).getPointOfInterest(testObjectId, false);
    }

    @Test
    public void testGetPOI_NonExistentId_ShouldReturnNull() {
        // Given
        String id = testObjectId.toString();
        when(persistenceService.getPointOfInterest(testObjectId, true)).thenReturn(null);

        // When
        PointOfInterest result = geoDataService.getPOI(id, true);

        // Then
        assertNull(result);
        verify(persistenceService).getPointOfInterest(testObjectId, true);
    }

    @Test
    public void testDeletePOI_ValidId_ShouldCallPersistenceService() {
        // Given
        String id = testObjectId.toString();

        // When
        geoDataService.deletePOI(id);

        // Then
        verify(persistenceService).deletePointOfInterest(testObjectId);
    }

    @Test
    public void testCreatePOI_ValidPOI_ShouldReturnCreatedPOI() {
        // Given
        PointOfInterest inputPoi = new PointOfInterest();
        inputPoi.setCategory("pharmacy");
        inputPoi.setDetails("City pharmacy");
        inputPoi.setLocation(new Point(13.7373, 51.0504));

        PointOfInterestEntity createdEntity = new PointOfInterestEntity();
        createdEntity.setId(testObjectId);
        createdEntity.setCategory("pharmacy");
        createdEntity.setDetails("City pharmacy");
        GeoPoint point = new GeoPoint();
        point.setCoordinates(51.0504, 13.7373);
        createdEntity.setLocation(point);

        when(persistenceService.createPointOfInterest(any(PointOfInterestEntity.class))).thenReturn(createdEntity);

        // When
        PointOfInterest result = geoDataService.createPOI(inputPoi);

        // Then
        assertNotNull(result);
        assertEquals(testObjectId.toString(), result.getId());
        assertEquals("pharmacy", result.getCategory());
        assertEquals("City pharmacy", result.getDetails());
        verify(persistenceService).createPointOfInterest(any(PointOfInterestEntity.class));
    }

    @Test
    public void testListPOIs_ValidParameters_ShouldReturnPOIList() {
        // Given
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 1000;
        boolean expandDetails = true;

        ObjectId objectId2 = new ObjectId();
        PointOfInterestEntity entity2 = new PointOfInterestEntity();
        entity2.setId(objectId2);
        entity2.setCategory("pharmacy");
        entity2.setDetails("Local pharmacy");
        GeoPoint point = new GeoPoint();
        point.setCoordinates(51.0600, 13.7400);
        entity2.setLocation(point);

        List<PointOfInterestEntity> entityList = Arrays.asList(testEntity, entity2);
        when(persistenceService.listPOIs(lat, lon, radius, expandDetails)).thenReturn(entityList);

        // When
        List<PointOfInterest> result = geoDataService.listPOIs(lat, lon, radius, expandDetails);

        // Then
        assertNotNull(result);
        assertEquals(2, result.size());

        PointOfInterest poi1 = result.get(0);
        assertEquals(testObjectId.toString(), poi1.getId());
        assertEquals("restaurant", poi1.getCategory());

        PointOfInterest poi2 = result.get(1);
        assertEquals(objectId2.toString(), poi2.getId());
        assertEquals("pharmacy", poi2.getCategory());

        verify(persistenceService).listPOIs(lat, lon, radius, expandDetails);
    }

    @Test
    public void testListPOIs_NoExpandDetails_ShouldReturnPOIList() {
        // Given
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 500;
        boolean expandDetails = false;

        List<PointOfInterestEntity> entityList = Arrays.asList(testEntity);
        when(persistenceService.listPOIs(lat, lon, radius, expandDetails)).thenReturn(entityList);

        // When
        List<PointOfInterest> result = geoDataService.listPOIs(lat, lon, radius, expandDetails);

        // Then
        assertNotNull(result);
        assertEquals(1, result.size());
        verify(persistenceService).listPOIs(lat, lon, radius, expandDetails);
    }

    @Test
    public void testCreatePOI_MissingDetails_ShouldReturnCreatedPOI() {
        PointOfInterest inputPoi = new PointOfInterest();
        inputPoi.setCategory(CATEGORY_RESTAURANT);
        inputPoi.setLocation(new Point(LONGITUDE_RESTAURANT, LATITUDE_RESTAURANT));
        // Details not set

        PointOfInterestEntity createdEntity = new PointOfInterestEntity();
        createdEntity.setId(testObjectId);
        createdEntity.setCategory(CATEGORY_RESTAURANT);
        createdEntity.setDetails(null);
        GeoPoint point = new GeoPoint();
        point.setCoordinates(LATITUDE_RESTAURANT, LONGITUDE_RESTAURANT);
        createdEntity.setLocation(point);

        when(persistenceService.createPointOfInterest(any(PointOfInterestEntity.class))).thenReturn(createdEntity);

        PointOfInterest result = geoDataService.createPOI(inputPoi);
        assertNotNull(result);
        assertEquals(testObjectId.toString(), result.getId());
        assertEquals(CATEGORY_RESTAURANT, result.getCategory());
        assertNull(result.getDetails());
        verify(persistenceService).createPointOfInterest(any(PointOfInterestEntity.class));
    }

    @Test
    public void testCreatePOI_NullLocation_ShouldReturnCreatedPOIWithNullLocation() {
        PointOfInterest inputPoi = new PointOfInterest();
        inputPoi.setCategory(CATEGORY_RESTAURANT);
        inputPoi.setDetails("No location");
        inputPoi.setLocation(null);

        PointOfInterestEntity createdEntity = new PointOfInterestEntity();
        createdEntity.setId(testObjectId);
        createdEntity.setCategory(CATEGORY_RESTAURANT);
        createdEntity.setDetails("No location");
        createdEntity.setLocation(null);

        when(persistenceService.createPointOfInterest(any(PointOfInterestEntity.class))).thenReturn(createdEntity);

        PointOfInterest result = geoDataService.createPOI(inputPoi);
        assertNotNull(result);
        assertEquals(testObjectId.toString(), result.getId());
        assertEquals(CATEGORY_RESTAURANT, result.getCategory());
        assertEquals("No location", result.getDetails());
        assertNull(result.getLocation());
        verify(persistenceService).createPointOfInterest(any(PointOfInterestEntity.class));
    }

    @Test
    public void testCreatePOI_NullPOI_ShouldThrowException() {
        assertThrows(NullPointerException.class, () -> geoDataService.createPOI(null));
    }

    @Test
    public void testUpdatePOI_ValidPOI_ShouldReturnUpdatedPOI() {
        // Given
        String id = testObjectId.toString();
        PointOfInterest inputPoi = new PointOfInterest();
        inputPoi.setId(id);
        inputPoi.setCategory("museum");
        inputPoi.setDetails("Updated details");
        inputPoi.setLocation(new Point(13.7373, 51.0504));

        PointOfInterestEntity existingEntity = new PointOfInterestEntity();
        existingEntity.setId(testObjectId);
        existingEntity.setCategory("restaurant");
        existingEntity.setDetails("Old details");
        existingEntity.setLocation(new GeoPoint(51.0504, 13.7373));

        PointOfInterestEntity updatedEntity = new PointOfInterestEntity();
        updatedEntity.setId(testObjectId);
        updatedEntity.setCategory("museum");
        updatedEntity.setDetails("Updated details");
        updatedEntity.setLocation(new GeoPoint(51.5555, 13.9999));

        when(persistenceService.getPointOfInterest(testObjectId, true)).thenReturn(existingEntity);
        when(persistenceService.updatePointOfInterest(existingEntity)).thenReturn(updatedEntity);

        // When
        PointOfInterest result = geoDataService.updatePOI(inputPoi);

        // Then
        assertNotNull(result);
        assertEquals(id, result.getId());
        assertEquals("museum", result.getCategory());
        assertEquals("Updated details", result.getDetails());
        assertNotNull(result.getLocation());
        assertEquals(13.9999, result.getLocation().getCoordinates().getLongitude(), 0.0001);
        assertEquals(51.5555, result.getLocation().getCoordinates().getLatitude(), 0.0001);
        verify(persistenceService).getPointOfInterest(testObjectId, true);
        verify(persistenceService).updatePointOfInterest(existingEntity);
    }

    @Test
    public void testUpdatePOI_NullId_ShouldThrowException() {
        PointOfInterest inputPoi = new PointOfInterest();
        inputPoi.setId(null);
        inputPoi.setCategory("museum");
        inputPoi.setDetails("Updated details");
        inputPoi.setLocation(new Point(13.7373, 51.0504));

        assertThrows(IllegalArgumentException.class, () -> geoDataService.updatePOI(inputPoi));
    }

    @Test
    public void testUpdatePOI_InvalidId_ShouldThrowException() {
        PointOfInterest inputPoi = new PointOfInterest();
        inputPoi.setId("invalid-id");
        inputPoi.setCategory("museum");
        inputPoi.setDetails("Updated details");
        inputPoi.setLocation(new Point(13.7373, 51.0504));

        assertThrows(IllegalArgumentException.class, () -> geoDataService.updatePOI(inputPoi));
    }

    @Test
    public void testUpdatePOI_NonExistentPOI_ShouldReturnNull() {
        String id = testObjectId.toString();
        PointOfInterest inputPoi = new PointOfInterest();
        inputPoi.setId(id);
        inputPoi.setCategory("museum");
        inputPoi.setDetails("Updated details");
        inputPoi.setLocation(new Point(13.7373, 51.0504));

        when(persistenceService.getPointOfInterest(testObjectId, true)).thenReturn(null);

        PointOfInterest result = geoDataService.updatePOI(inputPoi);

        assertNull(result);
        verify(persistenceService).getPointOfInterest(testObjectId, true);
        verify(persistenceService, never()).updatePointOfInterest(any());
    }


}
