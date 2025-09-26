package de.zeiss.mongodbws.testdatageneration.model;

public class PointOfInterest {

    private Point location;

    private String category;

    private String details;

    public PointOfInterest(Point location, String category, String details) {
        this.location = location;
        this.category = category;
        this.details = details;
    }

    public Point getLocation() {
        return location;
    }

    public void setLocation(Point location) {
        this.location = location;
    }

    public String getCategory() {
        return category;
    }

    public void setCategory(String category) {
        this.category = category;
    }

    public String getDetails() {
        return details;
    }

    public void setDetails(String details) {
        this.details = details;
    }
}
