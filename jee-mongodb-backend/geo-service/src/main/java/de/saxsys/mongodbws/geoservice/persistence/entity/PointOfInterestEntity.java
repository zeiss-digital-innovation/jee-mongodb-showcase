/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * 
 * Copyright (C) 2016 Saxonia Systems AG
 */
package de.saxsys.mongodbws.geoservice.persistence.entity;

import org.bson.types.ObjectId;
import org.mongodb.morphia.annotations.Embedded;
import org.mongodb.morphia.annotations.Entity;
import org.mongodb.morphia.annotations.Field;
import org.mongodb.morphia.annotations.Id;
import org.mongodb.morphia.annotations.Index;
import org.mongodb.morphia.annotations.Indexes;
import org.mongodb.morphia.geo.Point;
import org.mongodb.morphia.utils.IndexType;

/**
 * This is our morphia entity for a point of interest.
 * 
 * @author Andreas Post
 */
@Entity("point_of_interest")
@Indexes({ @Index(fields = { @Field(value = "location", type = IndexType.GEO2DSPHERE) }),
		@Index(fields = { @Field(value = "category") }), @Index(fields = { @Field(value = "name") }) })
public class PointOfInterestEntity {

	@Id
	private ObjectId id;

	private String category;

	private String name;

	@Embedded
	private Point location;

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
	 * @return the name
	 */
	public String getName() {
		return name;
	}

	/**
	 * @param name
	 *            the name to set
	 */
	public void setName(String name) {
		this.name = name;
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
}
