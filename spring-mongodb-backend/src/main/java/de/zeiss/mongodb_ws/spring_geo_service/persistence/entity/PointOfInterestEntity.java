package de.zeiss.mongodb_ws.spring_geo_service.persistence.entity;

import org.springframework.data.annotation.Id;
import org.springframework.data.mongodb.core.geo.GeoJsonPoint;
import org.springframework.data.mongodb.core.mapping.Document;

@Document(collection = "point-of-interest")
public class PointOfInterestEntity {

    @Id
    private String id;

    private String category;

    private String name;

    private String details;

    private GeoJsonPoint location;

    public PointOfInterestEntity() {

    }

    /**
     * @return the id
     */
    public String getId() {
        return id;
    }

    /**
     * @param id the id to set
     */
    public void setId(String id) {
        this.id = id;
    }

    /**
     * @return the category
     */
    public String getCategory() {
        return category;
    }

    /**
     * @param category the category to set
     */
    public void setCategory(String category) {
        this.category = category;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    /**
     * @return the details
     */
    public String getDetails() {
        return details;
    }

    /**
     * @param details the details to set
     */
    public void setDetails(String details) {
        this.details = details;
    }

    /**
     * @return the location
     */
    public GeoJsonPoint getLocation() {
        return location;
    }

    /**
     * @param location the location to set
     */
    public void setLocation(GeoJsonPoint location) {
        this.location = location;
    }
}
