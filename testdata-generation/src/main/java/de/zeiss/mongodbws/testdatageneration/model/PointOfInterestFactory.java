package de.zeiss.mongodbws.testdatageneration.model;

import java.math.BigDecimal;

public class PointOfInterestFactory {

    public static PointOfInterest createPointOfInterest(BigDecimal latitude, BigDecimal longitude, String category, String details) {
        Point location = new Point(latitude, longitude);
        return new PointOfInterest(location, category, details);
    }
}
