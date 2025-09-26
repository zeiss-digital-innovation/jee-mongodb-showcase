package de.zeiss.mongodbws.testdatageneration.model;

import com.topografix.gpx.WptType;

import java.math.BigDecimal;
import java.util.Objects;

public class PointOfInterestFactory {

    public static PointOfInterest createPointOfInterest(BigDecimal latitude, BigDecimal longitude, String category, String details) {
        Objects.requireNonNull(latitude, "latitude must not be null");
        Objects.requireNonNull(longitude, "longitude must not be null");
        Objects.requireNonNull(category, "category must not be null");
        Objects.requireNonNull(details, "details must not be null");

        return new PointOfInterest(new Point(latitude, longitude), category, details);
    }

    public static PointOfInterest createFromWptType(WptType wpt, String category) {
        Objects.requireNonNull(wpt, "wpt must not be null");
        
        return createPointOfInterest(wpt.getLat(), wpt.getLon(), category, wpt.getName());
    }
}
