package de.zeiss.mongodbws.testdatageneration.model;

public class PointOfInterest {

    private Point location;

    private String category;

    private String name;

    private String details;

    public PointOfInterest(Point location, String category, String name, String details) {
        this.location = location;
        this.category = category;
        this.name = name;
        this.details = details;
    }

    public Point getLocation() {
        return location;
    }

    public String getCategory() {
        return category;
    }

    public String getName() {
        return name;
    }

    public String getDetails() {
        return details;
    }
}
