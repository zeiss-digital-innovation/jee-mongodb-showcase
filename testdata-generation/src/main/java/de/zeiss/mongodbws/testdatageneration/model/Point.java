package de.zeiss.mongodbws.testdatageneration.model;

import java.math.BigDecimal;

public class Point {
    private String type = "Point";

    private BigDecimal[] coordinates;

    public Point(BigDecimal latitude, BigDecimal longitude) {
        this.coordinates = new BigDecimal[]{longitude, latitude};
    }

    public String getType() {
        return type;
    }

    public void setType(String type) {
        this.type = type;
    }

    public BigDecimal[] getCoordinates() {
        return coordinates;
    }

    public void setCoordinates(BigDecimal[] coordinates) {
        this.coordinates = coordinates;
    }
}
