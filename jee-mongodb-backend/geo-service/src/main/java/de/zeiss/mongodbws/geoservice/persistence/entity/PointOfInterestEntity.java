/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * <p>
 * Copyright (C) 2025 Carl Zeiss Digital Innovation GmbH
 */
package de.zeiss.mongodbws.geoservice.persistence.entity;

import dev.morphia.annotations.*;
import dev.morphia.utils.IndexType;
import org.bson.types.ObjectId;

/**
 * This is our morphia entity for a point of interest.
 *
 * @author Andreas Post
 */
@Entity(value = "point_of_interest", useDiscriminator = false)
@Indexes({@Index(fields = {@Field(value = "location", type = IndexType.GEO2DSPHERE)}),
        @Index(fields = {@Field(value = "category")})})
public class PointOfInterestEntity {

    @Id
    private ObjectId id;

    private String category;

    private String details;

    private GeoPoint location;

    public PointOfInterestEntity() {

    }

    /**
     * @return the id
     */
    public ObjectId getId() {
        return id;
    }

    /**
     * @param id
     *            the id to set
     */
    public void setId(ObjectId id) {
        this.id = id;
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

    /**
     * @return the location
     */
    public GeoPoint getLocation() {
        return location;
    }

    /**
     * @param location
     *            the location to set
     */
    public void setLocation(GeoPoint location) {
        this.location = location;
    }
}
