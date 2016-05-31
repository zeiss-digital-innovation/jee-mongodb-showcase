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

import org.mongodb.morphia.geo.Point;
import org.mongodb.morphia.geo.PointBuilder;

import de.saxsys.mongodbws.geoservice.persistence.entity.PointOfInterestEntity;

/**
 * 
 * @author Andreas Post
 */
@Stateless
public class MongoDBPersistenceService {

	private static final Logger LOG = Logger.getLogger(MongoDBPersistenceService.class.getName());

	private static final String DATABASE_NAME = "saxonia_campus";

	@Inject
	MongoDBClientProvider mongoDBClientProvider;

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
