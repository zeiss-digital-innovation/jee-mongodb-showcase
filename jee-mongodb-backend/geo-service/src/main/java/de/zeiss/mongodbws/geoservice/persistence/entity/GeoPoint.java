package de.zeiss.mongodbws.geoservice.persistence.entity;

public class GeoPoint {
    public double getLatitude() {
        return latitude;
    }

    public GeoPoint(double latitude, double longitude) {
        this.latitude = latitude;
        this.longitude = longitude;
    }

    public void setLatitude(double latitude) {
        this.latitude = latitude;
    }

    public double getLongitude() {
        return longitude;
    }

    public void setLongitude(double longitude) {
        this.longitude = longitude;
    }

    private double latitude;
    private double longitude;

    // Getter und Setter
}
