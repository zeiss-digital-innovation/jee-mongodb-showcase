/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * 
 * Copyright (C) 2016 Saxonia Systems AG
 */
package de.saxsys.mongodbws.geoservice.service;

import java.util.ArrayList;
import java.util.List;

import de.saxsys.mongodbws.geoservice.persistence.entity.GeoPoint;
import org.bson.types.ObjectId;
import org.geojson.Point;

import de.saxsys.mongodbws.geoservice.persistence.entity.PointOfInterestEntity;
import de.saxsys.mongodbws.geoservice.rest.resource.PointOfInterest;

/**
 * Converts {@link PointOfInterestEntity} to {@link PointOfInterest} and back.
 * 
 * @author Andreas Post
 */
public class PointOfInterestEntityConverter {

	/**
	 *
	 * @param entity
	 * @return
	 */
	public PointOfInterest decode(PointOfInterestEntity entity) {
		PointOfInterest poi = new PointOfInterest();
		poi.setId(entity.getId().toString());
		poi.setCategory(entity.getCategory());
		poi.setDetails(entity.getDetails());
		poi.setLocation(new Point(entity.getLocation().getLongitude(), entity.getLocation().getLatitude()));

		return poi;
	}

	/**
	 *
	 * @param entityList
	 * @return
	 */
	public List<PointOfInterest> decodeList(List<PointOfInterestEntity> entityList) {
		List<PointOfInterest> resultList = new ArrayList<>();

		if (entityList == null) {
			return resultList;
		}

		entityList.stream().forEach(poi -> resultList.add(decode(poi)));

		return resultList;
	}

	/**
	 *
	 * @param poi
	 * @return
	 */
	public PointOfInterestEntity encode(PointOfInterest poi) {
		PointOfInterestEntity entity = new PointOfInterestEntity();

		entity.setId(getObjectId(poi));

		entity.setCategory(poi.getCategory());
		entity.setDetails(poi.getDetails());

		GeoPoint point = new GeoPoint(poi.getLocation().getCoordinates().getLatitude(), poi.getLocation().getCoordinates().getLongitude());

		entity.setLocation(point);

		return entity;
	}

	/**
	 *
	 * @param poiList
	 * @return
	 */
	public List<PointOfInterestEntity> encodeList(List<PointOfInterest> poiList) {
		List<PointOfInterestEntity> entityList = new ArrayList<>();

		if (poiList == null) {
			return entityList;
		}

		poiList.stream().forEach(poi -> entityList.add(encode(poi)));

		return entityList;
	}

	/**
	 *
	 * @param poi
	 * @return
	 */
	private ObjectId getObjectId(PointOfInterest poi) {
		String id = null;

		if (poi.getId() != null) {
			id = poi.getId();
		} else if (poi.getHref() != null && !poi.getHref().isEmpty()) {
			int lastIndexOfSlash = poi.getHref().lastIndexOf("/");
			id = poi.getHref().substring(lastIndexOfSlash);
		} else {
			return null;
		}

		return new ObjectId(id);
	}
}
