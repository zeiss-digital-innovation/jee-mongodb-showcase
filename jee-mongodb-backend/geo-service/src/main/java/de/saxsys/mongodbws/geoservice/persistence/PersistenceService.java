/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * 
 * Copyright (C) 2016 Saxonia Systems AG
 */
package de.saxsys.mongodbws.geoservice.persistence;

import java.util.List;
import java.util.logging.Logger;

import javax.ejb.Stateless;
import javax.inject.Inject;

import org.bson.types.ObjectId;
import org.mongodb.morphia.geo.Point;
import org.mongodb.morphia.geo.PointBuilder;

import com.mongodb.WriteResult;

import de.saxsys.mongodbws.geoservice.persistence.entity.PointOfInterestEntity;

/**
 * Service for our persistence stuff.
 * 
 * @author Andreas Post
 */
@Stateless
public class PersistenceService {

	private static final Logger LOG = Logger.getLogger(PersistenceService.class.getName());

	@Inject
	MongoDBClientProvider mongoDBClientProvider;

	/**
	 * Saves the given {@link PointOfInterestEntity} as new entity. The
	 * returning entity contains the generated id.
	 * 
	 * @param poi
	 *            the entity to store.
	 * @return the entity with id
	 */
	public PointOfInterestEntity createPointOfInterest(PointOfInterestEntity poi) {

		mongoDBClientProvider.getDatastore().save(poi);

		return poi;
	}

	/**
	 * 
	 * @param id
	 * @return
	 */
	public PointOfInterestEntity getPointOfInterest(ObjectId id) {
		return mongoDBClientProvider.getDatastore().get(PointOfInterestEntity.class, id);
	}

	/**
	 * 
	 * @param id
	 */
	public void deletePointOfInterest(ObjectId id) {
		LOG.info("deletePointOfInterest: " + id);

		WriteResult writeResult = mongoDBClientProvider.getDatastore().delete(PointOfInterestEntity.class, id);

		LOG.info(writeResult.toString());
	}

	public List<PointOfInterestEntity> listPOIs(double lat, double lon, int radius) {
		PointBuilder builder = PointBuilder.pointBuilder();
		Point point = builder.latitude(lat).longitude(lon).build();

		return mongoDBClientProvider.getDatastore().createQuery(PointOfInterestEntity.class).field("location")
				.near(point, radius).asList();

		/*
		 * This is what we could do with plain mongodb driver:
		 */
		// FindIterable<Document> iterable =
		// database.getCollection("point_of_interest").find(
		// Filters.nearSphere("location", new
		// com.mongodb.client.model.geojson.Point(new
		// com.mongodb.client.model.geojson.Position(lon, lat)),
		// (double) radius, null));
		//
		// iterable.forEach(new Block<Document>() {
		// @Override
		// public void apply(final Document document) {
		// try {
		// PointOfInterest poi = mapper.readValue(document.toJson(),
		// new TypeReference<PointOfInterest>() {
		// });
		// poi.setId(document.get("_id").toString());
		// poiList.add(poi);
		// } catch (IOException e) {
		// LOG.log(Level.SEVERE, "Fehler beim Object Mapping", e);
		// }
		// }
		// });
	}
}
