package de.zeiss.mongodbws.geoservice.service.mapper;

import de.zeiss.mongodbws.geoservice.persistence.entity.GeoPoint;
import org.geojson.Point;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

class PointMapperTest {
    @Test
    void testMapToModel_validGeoPoint() {
        GeoPoint geoPoint = new GeoPoint();
        geoPoint.setCoordinates(51.0, 13.0);
        Point point = PointMapper.mapToModel(geoPoint);
        assertNotNull(point);
        assertEquals(13.0, point.getCoordinates().getLongitude());
        assertEquals(51.0, point.getCoordinates().getLatitude());
    }

    @Test
    void testMapToModel_nullGeoPoint() {
        assertNull(PointMapper.mapToModel(null));
    }

    @Test
    void testMapToEntity_validPoint() {
        Point point = new Point(13.0, 51.0);
        GeoPoint geoPoint = PointMapper.mapToEntity(point);
        assertNotNull(geoPoint);
        assertEquals(51.0, geoPoint.getLatitude());
        assertEquals(13.0, geoPoint.getLongitude());
    }

    @Test
    void testMapToEntity_nullPoint() {
        assertNull(PointMapper.mapToEntity(null));
    }

    @Test
    void testMapToEntity_nullCoordinates() {
        Point point = new Point();
        point.setCoordinates(null);
        assertNull(PointMapper.mapToEntity(point));
    }
}