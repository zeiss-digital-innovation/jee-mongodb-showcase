/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * 
 * Copyright (C) 2016 Saxonia Systems AG
 */
package de.saxsys.mongodbws.geoservice.service;

import java.util.ArrayList;
import java.util.List;

import javax.ejb.LocalBean;
import javax.ejb.Stateless;
import javax.inject.Inject;

import org.geojson.Point;
import org.mongodb.morphia.geo.PointBuilder;

import de.saxsys.mongodbws.geoservice.persistence.MongoDBPersistenceService;
import de.saxsys.mongodbws.geoservice.persistence.entity.PointOfInterestEntity;
import de.saxsys.mongodbws.geoservice.rest.resource.PointOfInterest;

/**
 * 
 * @author Andreas Post
 */
@Stateless
@LocalBean
public class GeoDataService {

	@Inject
	MongoDBPersistenceService mongoService;

	public PointOfInterest createPOI(PointOfInterest poi) {

		PointOfInterestEntity entity = new PointOfInterestEntity();
		entity.setCategory(poi.getCategory());
		entity.setName(poi.getName());

		PointBuilder pointBuilder = PointBuilder.pointBuilder();
		pointBuilder.latitude(poi.getLocation().getCoordinates().getLatitude());
		pointBuilder.longitude(poi.getLocation().getCoordinates().getLongitude());

		entity.setLocation(pointBuilder.build());

		entity = mongoService.createPointOfInterest(entity);

		PointOfInterest json = new PointOfInterest();
		json.setId(entity.getId().toString());
		json.setCategory(entity.getCategory());
		json.setName(entity.getName());
		json.setLocation(new Point(entity.getLocation().getLongitude(), entity.getLocation().getLatitude()));

		return json;
	}

	/**
	 * Returns a list of nearest points of interest.
	 * 
	 * @param lat
	 * @param lon
	 * @param radius
	 * @return
	 */
	public List<PointOfInterest> listPOIs(double lat, double lon, int radius) {
		List<PointOfInterest> poiList = new ArrayList<PointOfInterest>();

		List<PointOfInterestEntity> entityList = mongoService.listPOIs(lat, lon, radius);

		for (PointOfInterestEntity entity : entityList) {
			PointOfInterest json = new PointOfInterest();
			json.setId(entity.getId().toString());
			json.setCategory(entity.getCategory());
			json.setName(entity.getName());
			json.setLocation(new Point(entity.getLocation().getLongitude(), entity.getLocation().getLatitude()));
			poiList.add(json);
		}

		return poiList;
	}
}
