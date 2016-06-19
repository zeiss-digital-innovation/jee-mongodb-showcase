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
import org.mongodb.morphia.mapping.Mapper;
import org.mongodb.morphia.query.Query;

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
	 * Retrieve an poi entity by id.
	 * 
	 * @param id
	 *            The object id of the entity.
	 * @param expandDetails
	 *            If true returnes all data of the poi.
	 * @return
	 */
	public PointOfInterestEntity getPointOfInterest(ObjectId id, boolean expandDetails) {
		/*
		 * This is for showing how fields can be left out. The query would be like:
		 * 
		 * db.getCollection('point_of_interest').find({_id: ObjectId('[id]')},{'details': 0})
		 */
		if (!expandDetails) {
			return mongoDBClientProvider.getDatastore().find(PointOfInterestEntity.class, Mapper.ID_KEY, id)
					.retrievedFields(false, "details").get();
		} else {
			return mongoDBClientProvider.getDatastore().get(PointOfInterestEntity.class, id);
		}
	}

	/**
	 * Delete a poi by id.
	 * 
	 * @param id
	 */
	public void deletePointOfInterest(ObjectId id) {
		LOG.info("deletePointOfInterest: " + id);

		WriteResult writeResult = mongoDBClientProvider.getDatastore().delete(PointOfInterestEntity.class, id);

		LOG.info(writeResult.toString());
	}

	/**
	 * List poi's by coords and radius.
	 * 
	 * @param lat
	 * @param lon
	 * @param radius
	 * @param expandDetails
	 *            If true returnes all data of the poi.
	 * @return
	 */
	public List<PointOfInterestEntity> listPOIs(double lat, double lon, int radius, boolean expandDetails) {
		PointBuilder builder = PointBuilder.pointBuilder();
		Point point = builder.latitude(lat).longitude(lon).build();

		Query<PointOfInterestEntity> query = mongoDBClientProvider.getDatastore()
				.createQuery(PointOfInterestEntity.class);

		if (!expandDetails) {
			query = query.retrievedFields(false, "details");
		}

		return query.field("location").near(point, radius).asList();

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
