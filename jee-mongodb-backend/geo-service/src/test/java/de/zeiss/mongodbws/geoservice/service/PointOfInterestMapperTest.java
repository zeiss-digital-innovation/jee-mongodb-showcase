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
import org.junit.Test;

import java.util.Arrays;
import java.util.List;

import static org.junit.Assert.*;

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
        entity1.setDetails("Pizza place");
        GeoPoint geoPoint = new GeoPoint();
        geoPoint.setCoordinates(51.0504, 13.7373);
        entity1.setLocation(geoPoint);

        PointOfInterestEntity entity2 = new PointOfInterestEntity();
        entity2.setId(objectId2);
        entity2.setCategory("pharmacy");
        entity2.setDetails("City pharmacy");
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
        assertEquals("Pizza place", poi1.getDetails());

        PointOfInterest poi2 = result.get(1);
        assertEquals(objectId2.toString(), poi2.getId());
        assertEquals("pharmacy", poi2.getCategory());
        assertEquals("City pharmacy", poi2.getDetails());
    }

    @Test
    public void testEntityToModelBack_ValidPointOfInterest_ShouldReturnMap() {
        // Given
        String objectIdString = new ObjectId().toString();
        PointOfInterest poi = new PointOfInterest();
        poi.setId(objectIdString);
        poi.setCategory("supermarket");
        poi.setDetails("24/7 supermarket");
        poi.setLocation(new Point(13.7373, 51.0504));

        // When
        PointOfInterestEntity result = PointOfInterestMapper.mapToEntity(poi);

        // Then
        assertNotNull(result);
        assertEquals(objectIdString, result.getId().toString());
        assertEquals("supermarket", result.getCategory());
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
        poi.setDetails("Free parking");
        poi.setLocation(new Point(13.7373, 51.0504));

        // When
        PointOfInterestEntity result = PointOfInterestMapper.mapToEntity(poi);

        // Then
        assertNotNull(result);
        assertNull(result.getId());
        assertEquals("parking", result.getCategory());
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
        poi1.setDetails("Italian restaurant");
        poi1.setLocation(new Point(13.7373, 51.0504));

        PointOfInterest poi2 = new PointOfInterest();
        poi2.setId(objectId2String);
        poi2.setCategory("hotel");
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
        assertEquals("Italian restaurant", entity1.getDetails());

        PointOfInterestEntity entity2 = result.get(1);
        assertEquals(objectId2String, entity2.getId().toString());
        assertEquals("hotel", entity2.getCategory());
        assertEquals("5-star hotel", entity2.getDetails());
    }

}
