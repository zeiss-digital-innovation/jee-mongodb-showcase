/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * <p>
 * Copyright (C) 2025 Carl Zeiss Digital Innovation GmbH
 */
package de.zeiss.mongodbws.geoservice.persistence.entity;

import dev.morphia.annotations.Embedded;

@Embedded
public class GeoPoint {

    private static final int LONGITUDE_INDEX = 0;
    private static final int LATITUDE_INDEX = 1;

    private String type = "Point";

    private double[] coordinates;

    public GeoPoint() {

    }

    public GeoPoint(double latitude, double longitude) {
        setCoordinates(latitude, longitude);
    }

    public double getLatitude() {
        return coordinates[LATITUDE_INDEX];
    }

    public double getLongitude() {
        return coordinates[LONGITUDE_INDEX];
    }

    public void setCoordinates(double[] coordinates) {
        this.coordinates = coordinates;
    }

    public void setCoordinates(double latitude, double longitude) {
        // MongoDB Point specifies coordinates as [longitude, latitude]
        this.coordinates = new double[]{longitude, latitude};
    }

    public String getType() {
        return type;
    }

    public void setType(String type) {
        this.type = type;
    }
}
