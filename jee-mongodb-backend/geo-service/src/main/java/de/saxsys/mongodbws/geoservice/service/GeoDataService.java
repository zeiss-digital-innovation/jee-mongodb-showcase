/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * 
 * Copyright (C) 2016 Saxonia Systems AG
 */
package de.saxsys.mongodbws.geoservice.service;

import java.util.List;

import javax.ejb.LocalBean;
import javax.ejb.Stateless;
import javax.inject.Inject;

import org.bson.types.ObjectId;

import de.saxsys.mongodbws.geoservice.persistence.PersistenceService;
import de.saxsys.mongodbws.geoservice.persistence.entity.PointOfInterestEntity;
import de.saxsys.mongodbws.geoservice.rest.resource.PointOfInterest;

/**
 * Our data service. Does currently nothing more than converting beeing the
 * interface between REST and persistence layer including converting entities.
 * 
 * @author Andreas Post
 */
@Stateless
@LocalBean
public class GeoDataService {

	@Inject
	PersistenceService persistenceService;

	private PointOfInterestEntityConverter entityConverter = new PointOfInterestEntityConverter();

	/**
	 * 
	 * @param id
	 * @return
	 */
	public PointOfInterest getPOI(String id) {
		PointOfInterestEntity entity = persistenceService.getPointOfInterest(new ObjectId(id));

		if (entity == null) {
			return null;
		}

		return entityConverter.decode(entity);
	}

	/**
	 * 
	 * @param id
	 */
	public void deletePOI(String id) {
		persistenceService.deletePointOfInterest(new ObjectId(id));
	}

	/**
	 * 
	 * @param poi
	 * @return
	 */
	public PointOfInterest createPOI(PointOfInterest poi) {

		PointOfInterestEntity entity = entityConverter.encode(poi);

		entity = persistenceService.createPointOfInterest(entity);

		return entityConverter.decode(entity);
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
		List<PointOfInterestEntity> entityList = persistenceService.listPOIs(lat, lon, radius);

		return entityConverter.decodeList(entityList);
	}
}
