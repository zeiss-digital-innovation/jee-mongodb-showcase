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
    private PointOfInterest testPoi;

    @BeforeEach
    public void setUp() {
        testObjectId = new ObjectId();

        // Setup test entity
        testEntity = new PointOfInterestEntity();
        testEntity.setId(testObjectId);
        testEntity.setCategory("restaurant");
        testEntity.setDetails("Best restaurant in town");
//        testEntity.setLocation(new GeoPoint(51.0504,13.7373));

        // Setup test POI
        testPoi = new PointOfInterest();
        testPoi.setId(testObjectId.toString());
        testPoi.setCategory("restaurant");
        testPoi.setDetails("Best restaurant in town");
        testPoi.setLocation(new Point(13.7373, 51.0504));
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
        assertEquals("restaurant", result.getCategory());
        assertEquals("Best restaurant in town", result.getDetails());
        verify(persistenceService).getPointOfInterest(testObjectId, true);
    }

    @Test
    public void testGetPOI_ValidIdNoExpandDetails_ShouldReturnPointOfInterest() {
        // Given
        String id = testObjectId.toString();
        when(persistenceService.getPointOfInterest(testObjectId, false)).thenReturn(testEntity);

        // When
        PointOfInterest result = geoDataService.getPOI(id, false);

        // Then
        assertNotNull(result);
        assertEquals(testObjectId.toString(), result.getId());
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
}
