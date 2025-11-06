package de.zeiss.mongodb_ws.spring_geo_service.service.mapper;

import de.zeiss.mongodb_ws.spring_geo_service.persistence.entity.PointOfInterestEntity;
import de.zeiss.mongodb_ws.spring_geo_service.rest.model.PointOfInterest;
import org.geojson.Point;
import org.junit.jupiter.api.Test;
import org.springframework.data.mongodb.core.geo.GeoJsonPoint;

import static org.junit.jupiter.api.Assertions.*;

public class PointOfInterestMapperTest {

    private static double LATITUDE = 51.12345;
    private static double LONGITUDE = 13.12345;

    private static double LATITUDE_UPDATE = -51.12345;
    private static double LONGITUDE_UPDATE = -13.12345;

    @Test
    public void testMapToResource() {
        PointOfInterestEntity entity = new PointOfInterestEntity();
        entity.setId("123");
        entity.setName("Test POI");
        entity.setCategory("Test Category");
        entity.setDetails("Test Details");
        entity.setLocation(new GeoJsonPoint(13.12345, LATITUDE));

        PointOfInterest resource = PointOfInterestMapper.mapToResource(entity);

        assertNotNull(resource);
        assertEquals("123", resource.getId());
        assertEquals("Test POI", resource.getName());
        assertEquals("Test Category", resource.getCategory());
        assertEquals("Test Details", resource.getDetails());
        assertEquals(LATITUDE, resource.getLocation().getCoordinates().getLatitude());
        assertEquals(LONGITUDE, resource.getLocation().getCoordinates().getLongitude());
    }

    @Test
    public void testMapToEntity() {
        PointOfInterest resource = new PointOfInterest();
        resource.setId("123");
        resource.setName("Test POI");
        resource.setCategory("Test Category");
        resource.setDetails("Test Details");
        resource.setLocation(new Point(LONGITUDE, LATITUDE));

        PointOfInterestEntity entity = PointOfInterestMapper.mapToEntity(resource);

        assertNotNull(entity);
        assertEquals("123", entity.getId());
        assertEquals("Test POI", entity.getName());
        assertEquals("Test Category", entity.getCategory());
        assertEquals("Test Details", entity.getDetails());
        assertEquals(LONGITUDE, entity.getLocation().getX());
        assertEquals(LATITUDE, entity.getLocation().getY());
    }

    @Test
    public void testMapToEntity_FromHref() {
        PointOfInterest resource = new PointOfInterest();
        resource.setHref("/api/pois/456");

        PointOfInterestEntity entity = PointOfInterestMapper.mapToEntity(resource);

        assertNotNull(entity);
        assertEquals("456", entity.getId());
    }

    @Test
    public void testMapToEntity_FromHrefWithoutFullUri() {
        PointOfInterest resource = new PointOfInterest();
        resource.setHref("456");

        PointOfInterestEntity entity = PointOfInterestMapper.mapToEntity(resource);

        assertNotNull(entity);
        assertEquals("456", entity.getId());
    }

    @Test
    public void testMapToEntity_NullHref() {
        PointOfInterest resource = new PointOfInterest();
        resource.setHref(null);

        PointOfInterestEntity entity = PointOfInterestMapper.mapToEntity(resource);

        assertNotNull(entity);
        assertNull(entity.getId());
    }

    @Test
    public void testMapToEntity_EmptyHref() {
        PointOfInterest resource = new PointOfInterest();
        resource.setHref("");

        PointOfInterestEntity entity = PointOfInterestMapper.mapToEntity(resource);

        assertNotNull(entity);
        assertNull(entity.getId());
    }

    @Test
    public void testMapToEntity_NullResource() {
        assertNull(PointOfInterestMapper.mapToEntity(null));
    }

    @Test
    public void testMapToResource_NullEntity() {
        assertNull(PointOfInterestMapper.mapToResource(null));
    }

    @Test
    public void testUpdateEntityFromModel() {
        PointOfInterest resource = new PointOfInterest();
        resource.setName("Updated POI");
        resource.setCategory("Updated Category");
        resource.setDetails("Updated Details");
        resource.setLocation(new Point(LONGITUDE_UPDATE, LATITUDE_UPDATE));

        PointOfInterestEntity entity = new PointOfInterestEntity();
        entity.setName("Old POI");
        entity.setCategory("Old Category");
        entity.setDetails("Old Details");
        entity.setLocation(new GeoJsonPoint(LONGITUDE, LATITUDE));

        PointOfInterestMapper.updateEntityFromModel(resource, entity);

        assertEquals("Updated POI", entity.getName());
        assertEquals("Updated Category", entity.getCategory());
        assertEquals("Updated Details", entity.getDetails());
        assertEquals(LONGITUDE_UPDATE, entity.getLocation().getX());
        assertEquals(LATITUDE_UPDATE, entity.getLocation().getY());
    }
}
