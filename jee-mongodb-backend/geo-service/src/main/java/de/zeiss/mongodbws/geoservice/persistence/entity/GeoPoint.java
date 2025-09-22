package de.zeiss.mongodbws.geoservice.persistence.entity;

import dev.morphia.annotations.Embedded;

@Embedded
public class GeoPoint {
    private String type = "Point";

    private double[] coordinates;

    public GeoPoint() {

    }

//    public GeoPoint(double latitude, double longitude) {
//        this.latitude = latitude;
//        this.longitude = longitude;
//    }
//
//    public GeoPoint(String type, double latitude, double longitude) {
//        this.latitude = latitude;
//        this.longitude = longitude;
//    }

    public double getLatitude() {
        return coordinates[0];
    }

    public double getLongitude() {
        return coordinates[1];
    }

    public void setCoordinates(double[] coordinates) {
        this.coordinates = coordinates;
    }

    // Getter und Setter
}
