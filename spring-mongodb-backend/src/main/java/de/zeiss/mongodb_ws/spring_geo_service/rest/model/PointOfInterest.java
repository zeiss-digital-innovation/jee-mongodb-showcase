package de.zeiss.mongodb_ws.spring_geo_service.rest.model;

import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonInclude;
import org.geojson.Point;

@JsonInclude(JsonInclude.Include.NON_NULL)
public class PointOfInterest {

    private String href;

    @JsonIgnore
    private String id;

    private Point location;

    private String category;

    private String details;

    /**
     * @return the href
     */
    public String getHref() {
        return href;
    }

    /**
     * @param href
     *            the href to set
     */
    public void setHref(String href) {
        this.href = href;
    }

    /**
     * @return the id
     */
    public String getId() {
        return id;
    }

    /**
     * @param id
     *            the id to set
     */
    public void setId(String id) {
        this.id = id;
    }

    /**
     * @return the location
     */
    public Point getLocation() {
        return location;
    }

    /**
     * @param location
     *            the location to set
     */
    public void setLocation(Point location) {
        this.location = location;
    }

    /**
     * @return the category
     */
    public String getCategory() {
        return category;
    }

    /**
     * @param category
     *            the category to set
     */
    public void setCategory(String category) {
        this.category = category;
    }

    /**
     * @return the details
     */
    public String getDetails() {
        return details;
    }

    /**
     * @param details
     *            the details to set
     */
    public void setDetails(String details) {
        this.details = details;
    }
}
