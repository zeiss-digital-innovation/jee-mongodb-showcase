package de.zeiss.mongodbws.testdatageneration.model;

import com.topografix.gpx.WptType;

import java.math.BigDecimal;
import java.util.Objects;

public class PointOfInterestFactory {

    public static PointOfInterest createPointOfInterest(BigDecimal latitude, BigDecimal longitude, String category, String name, String details) {
        Objects.requireNonNull(latitude, "latitude must not be null");
        Objects.requireNonNull(longitude, "longitude must not be null");
        Objects.requireNonNull(category, "category must not be null");
        Objects.requireNonNull(name, "name must not be null");
        Objects.requireNonNull(details, "details must not be null");

        return new PointOfInterest(new Point(latitude, longitude), category, name, details);
    }

    public static PointOfInterest createFromWptType(WptType wpt, String category) {
        Objects.requireNonNull(wpt, "wpt must not be null");

        String[] parts = wpt.getName().split(",");

        if (parts.length == 1) {
            return createPointOfInterest(wpt.getLat(), wpt.getLon(), category, parts[0], parts[0]);
        } else {
            String name = parts[0];
            String details = String.join(",", java.util.Arrays.copyOfRange(parts, 1, parts.length)).trim();
            return createPointOfInterest(wpt.getLat(), wpt.getLon(), category, name, details);
        }
    }
}
