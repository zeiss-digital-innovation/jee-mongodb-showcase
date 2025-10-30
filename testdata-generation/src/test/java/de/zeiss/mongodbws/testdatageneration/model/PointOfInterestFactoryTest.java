package de.zeiss.mongodbws.testdatageneration.model;

import com.topografix.gpx.WptType;
import org.junit.jupiter.api.Test;

import java.math.BigDecimal;

import static org.junit.jupiter.api.Assertions.*;

/**
 * Unit tests for PointOfInterestFactory.
 *
 * @author AI Generated
 * @author Andreas Post
 */
public class PointOfInterestFactoryTest {
    @Test
    void createPointOfInterest_nullLatitude_throwsNPE() {
        assertThrows(NullPointerException.class, () ->
                PointOfInterestFactory.createPointOfInterest(null, BigDecimal.ONE, "cat", "name", "det"));
    }

    @Test
    void createPointOfInterest_nullLongitude_throwsNPE() {
        assertThrows(NullPointerException.class, () ->
                PointOfInterestFactory.createPointOfInterest(BigDecimal.ONE, null, "cat", "name", "det"));
    }

    @Test
    void createPointOfInterest_nullCategory_throwsNPE() {
        assertThrows(NullPointerException.class, () ->
                PointOfInterestFactory.createPointOfInterest(BigDecimal.ONE, BigDecimal.ONE, null, "name", "det"));
    }

    @Test
    void createPointOfInterest_nullName_throwsNPE() {
        assertThrows(NullPointerException.class, () ->
                PointOfInterestFactory.createPointOfInterest(BigDecimal.ONE, BigDecimal.ONE, "cat", null, "det"));
    }

    @Test
    void createPointOfInterest_nullDetails_throwsNPE() {
        assertThrows(NullPointerException.class, () ->
                PointOfInterestFactory.createPointOfInterest(BigDecimal.ONE, BigDecimal.ONE, "cat", "name", null));
    }

    @Test
    void createFromWptType_nullWpt_throwsNPE() {
        assertThrows(NullPointerException.class, () ->
                PointOfInterestFactory.createFromWptType(null, "cat"));
    }

    @Test
    void createFromWptType_createsPoiWithValues() {
        BigDecimal lat = new BigDecimal("12.3456");
        BigDecimal lon = new BigDecimal("65.4321");
        String category = "park";
        String name = "MyPark";
        String details = "A nice park, Parkstreet 123, Parkcity";

        String wptName = name + ", " + details;

        WptType w = new WptType();
        w.setLat(lat);
        w.setLon(lon);
        w.setName(wptName);

        PointOfInterest poi = PointOfInterestFactory.createFromWptType(w, category);

        assertNotNull(poi, "PointOfInterest must not be null");
        assertNotNull(poi.getLocation(), "PointOfInterest location must not be null");
        assertNotNull(poi.getLocation().getCoordinates(), "PointOfInterest location coordinates must not be null");

        assertEquals(poi.getLocation().getCoordinates()[0], lon, "Longitude should match");
        assertEquals(poi.getLocation().getCoordinates()[1], lat, "Latitude should match");
        assertEquals(category, poi.getCategory(), "Category should match");
        assertEquals(name, poi.getName(), "Name should match");
        assertEquals(details, poi.getDetails(), "Details should match");
    }

}
