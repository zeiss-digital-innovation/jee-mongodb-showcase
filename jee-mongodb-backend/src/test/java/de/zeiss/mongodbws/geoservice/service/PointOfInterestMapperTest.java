/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * <p>
 * Copyright (C) 2025 Carl Zeiss Digital Innovation GmbH
 */
package de.zeiss.mongodbws.geoservice.service;

import de.zeiss.mongodbws.geoservice.persistence.entity.GeoPoint;
import de.zeiss.mongodbws.geoservice.persistence.entity.PointOfInterestEntity;
import de.zeiss.mongodbws.geoservice.rest.resource.PointOfInterest;
import de.zeiss.mongodbws.geoservice.service.mapper.PointOfInterestMapper;
import org.bson.types.ObjectId;
import org.geojson.Point;
import org.junit.jupiter.api.Test;

import java.util.Arrays;
import java.util.List;

import static org.junit.jupiter.api.Assertions.*;


/**
 * Unit tests for {@link PointOfInterestMapper}
 *
 * @author Generated Tests
 */
public class PointOfInterestMapperTest {

    @Test
    public void testEntityToModel_ValidMap_ShouldReturnPointOfInterest() {
        // Given
        ObjectId objectId = new ObjectId();
        PointOfInterestEntity entity = new PointOfInterestEntity();
        entity.setId(objectId);
        entity.setCategory("restaurant");
        entity.setName("Pizza Place");
        entity.setDetails("Best pizza in town");
        GeoPoint geoPoint = new GeoPoint();
        geoPoint.setCoordinates(51.0504, 13.7373);
        entity.setLocation(geoPoint);

        // When
        PointOfInterest result = PointOfInterestMapper.mapToModel(entity);

        // Then
        assertNotNull(result);
        assertEquals(objectId.toString(), result.getId());
        assertEquals("restaurant", result.getCategory());
        assertEquals("Pizza Place", result.getName());
        assertEquals("Best pizza in town", result.getDetails());
        assertNotNull(result.getLocation());
        assertEquals(13.7373, result.getLocation().getCoordinates().getLongitude(), 0.0001);
        assertEquals(51.0504, result.getLocation().getCoordinates().getLatitude(), 0.0001);
    }

    @Test
    public void testEntityToModelList_ValidMapList_ShouldReturnPointOfInterestList() {
        // Given
        ObjectId objectId1 = new ObjectId();
        ObjectId objectId2 = new ObjectId();

        PointOfInterestEntity entity1 = new PointOfInterestEntity();
        entity1.setId(objectId1);
        entity1.setCategory("restaurant");
        entity1.setName("Pizza Place");
        entity1.setDetails("Pizza Street 123");
        GeoPoint geoPoint = new GeoPoint();
        geoPoint.setCoordinates(51.0504, 13.7373);
        entity1.setLocation(geoPoint);

        PointOfInterestEntity entity2 = new PointOfInterestEntity();
        entity2.setId(objectId2);
        entity2.setCategory("pharmacy");
        entity2.setName("City Pharmacy");
        entity2.setDetails("Central plaza 456");
        geoPoint = new GeoPoint();
        geoPoint.setCoordinates(52.5200, 13.4050);
        entity2.setLocation(geoPoint);

        List<PointOfInterestEntity> entityList = Arrays.asList(entity1, entity2);

        // When
        List<PointOfInterest> result = entityList.stream().map(PointOfInterestMapper::mapToModel).toList();

        // Then
        assertNotNull(result);
        assertEquals(2, result.size());

        PointOfInterest poi1 = result.get(0);
        assertEquals(objectId1.toString(), poi1.getId());
        assertEquals("restaurant", poi1.getCategory());
        assertEquals("Pizza Place", poi1.getName());
        assertEquals("Pizza Street 123", poi1.getDetails());

        PointOfInterest poi2 = result.get(1);
        assertEquals(objectId2.toString(), poi2.getId());
        assertEquals("pharmacy", poi2.getCategory());
        assertEquals("City Pharmacy", poi2.getName());
        assertEquals("Central plaza 456", poi2.getDetails());
    }

    @Test
    public void testEntityToModelBack_ValidPointOfInterest_ShouldReturnMap() {
        // Given
        String objectIdString = new ObjectId().toString();
        PointOfInterest poi = new PointOfInterest();
        poi.setId(objectIdString);
        poi.setCategory("supermarket");
        poi.setName("Supermarket");
        poi.setDetails("24/7 supermarket");
        poi.setLocation(new Point(13.7373, 51.0504));

        // When
        PointOfInterestEntity result = PointOfInterestMapper.mapToEntity(poi);

        // Then
        assertNotNull(result);
        assertEquals(objectIdString, result.getId().toString());
        assertEquals("supermarket", result.getCategory());
        assertEquals("Supermarket", result.getName());
        assertEquals("24/7 supermarket", result.getDetails());
        assertNotNull(result.getLocation());
        assertEquals(13.7373, result.getLocation().getLongitude(), 0.0001);
        assertEquals(51.0504, result.getLocation().getLatitude(), 0.0001);
    }

    @Test
    public void testMapToModelBack_PointOfInterestWithNullId_ShouldHandleGracefully() {
        // Given
        PointOfInterest poi = new PointOfInterest();
        poi.setId(null);
        poi.setCategory("parking");
        poi.setName("Central Parking");
        poi.setDetails("Free parking");
        poi.setLocation(new Point(13.7373, 51.0504));

        // When
        PointOfInterestEntity result = PointOfInterestMapper.mapToEntity(poi);

        // Then
        assertNotNull(result);
        assertNull(result.getId());
        assertEquals("parking", result.getCategory());
        assertEquals("Central Parking", result.getName());
        assertEquals("Free parking", result.getDetails());
    }

    @Test
    public void testEntityToModelBackList_ValidPointOfInterestList_ShouldReturnMapList() {
        // Given
        String objectId1String = new ObjectId().toString();
        String objectId2String = new ObjectId().toString();

        PointOfInterest poi1 = new PointOfInterest();
        poi1.setId(objectId1String);
        poi1.setCategory("restaurant");
        poi1.setName("Pizza Place");
        poi1.setDetails("Italian restaurant");
        poi1.setLocation(new Point(13.7373, 51.0504));

        PointOfInterest poi2 = new PointOfInterest();
        poi2.setId(objectId2String);
        poi2.setCategory("hotel");
        poi2.setName("Grand Hotel");
        poi2.setDetails("5-star hotel");
        poi2.setLocation(new Point(13.4050, 52.5200));

        List<PointOfInterest> poiList = Arrays.asList(poi1, poi2);

        // When
        List<PointOfInterestEntity> result = poiList.stream().map(PointOfInterestMapper::mapToEntity).toList();

        // Then
        assertNotNull(result);
        assertEquals(2, result.size());

        PointOfInterestEntity entity1 = result.get(0);
        assertEquals(objectId1String, entity1.getId().toString());
        assertEquals("restaurant", entity1.getCategory());
        assertEquals("Pizza Place", entity1.getName());
        assertEquals("Italian restaurant", entity1.getDetails());

        PointOfInterestEntity entity2 = result.get(1);
        assertEquals(objectId2String, entity2.getId().toString());
        assertEquals("hotel", entity2.getCategory());
        assertEquals("Grand Hotel", entity2.getName());
        assertEquals("5-star hotel", entity2.getDetails());
    }

    @Test
    public void testUpdateEntityFromModel_ValidUpdate_ShouldModifyEntity() {
        // Given
        PointOfInterest poi = new PointOfInterest();
        poi.setCategory("cafe");
        poi.setName("Coffee Corner");
        poi.setDetails("Cozy place for coffee");
        poi.setLocation(new Point(13.7373, 51.0504));

        PointOfInterestEntity entity = new PointOfInterestEntity();
        entity.setCategory("restaurant");
        entity.setName("Old Name");
        entity.setDetails("Old details");
        GeoPoint geoPoint = new GeoPoint();
        geoPoint.setCoordinates(0.0, 0.0);
        entity.setLocation(geoPoint);

        // When
        PointOfInterestMapper.updateEntityFromModel(poi, entity);

        // Then
        assertEquals("cafe", entity.getCategory());
        assertEquals("Coffee Corner", entity.getName());
        assertEquals("Cozy place for coffee", entity.getDetails());
        assertNotNull(entity.getLocation());
        assertEquals(13.7373, entity.getLocation().getLongitude(), 0.0001);
        assertEquals(51.0504, entity.getLocation().getLatitude(), 0.0001);
    }

    @Test
    public void testUpdateEntityFromModel_DifferentID_ShouldNotModifyEntityID() {
        // Given
        PointOfInterest poi = new PointOfInterest();
        poi.setId(new ObjectId().toString());
        poi.setCategory("cafe");
        poi.setName("Coffee Corner");
        poi.setDetails("Cozy place for coffee");
        poi.setLocation(new Point(13.7373, 51.0504));

        ObjectId originalId = new ObjectId();
        PointOfInterestEntity entity = new PointOfInterestEntity();
        entity.setId(originalId);
        entity.setCategory("restaurant");
        entity.setName("Old Name");
        entity.setDetails("Old details");
        GeoPoint geoPoint = new GeoPoint();
        geoPoint.setCoordinates(0.0, 0.0);
        entity.setLocation(geoPoint);

        // When
        PointOfInterestMapper.updateEntityFromModel(poi, entity);

        // Then
        assertEquals(originalId, entity.getId());
    }

}
