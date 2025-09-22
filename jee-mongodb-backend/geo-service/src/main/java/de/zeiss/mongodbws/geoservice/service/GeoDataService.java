/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * 
 * Copyright (C) 2016 Saxonia Systems AG
 */
package de.zeiss.mongodbws.geoservice.service;

import java.util.List;

import jakarta.ejb.LocalBean;
import jakarta.ejb.Stateless;
import jakarta.inject.Inject;

import org.bson.types.ObjectId;

import de.zeiss.mongodbws.geoservice.persistence.PersistenceService;
import de.zeiss.mongodbws.geoservice.persistence.entity.PointOfInterestEntity;
import de.zeiss.mongodbws.geoservice.rest.resource.PointOfInterest;

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
	 * Get a poi by id.
	 * 
	 * @param id
	 *            String representation of object id.
	 * @param expandDetails
	 *            If true returnes all data of the poi.
	 * @return
	 */
	public PointOfInterest getPOI(String id, boolean expandDetails) {
		PointOfInterestEntity entity = persistenceService.getPointOfInterest(new ObjectId(id), expandDetails);

		if (entity == null) {
			return null;
		}

		return entityConverter.decode(entity);
	}

	/**
	 * Delete a poi by id.
	 * 
	 * @param id
	 */
	public void deletePOI(String id) {
		persistenceService.deletePointOfInterest(new ObjectId(id));
	}

	/**
	 * Create a new poi.
	 * 
	 * @param poi
	 * @return The new poi including its id.
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
	 * @param expandDetails
	 *            If true returnes all data of the poi.
	 * @return
	 */
	public List<PointOfInterest> listPOIs(double lat, double lon, int radius, boolean expandDetails) {
		List<PointOfInterestEntity> entityList = persistenceService.listPOIs(lat, lon, radius, expandDetails);

		return entityConverter.decodeList(entityList);
	}
}
