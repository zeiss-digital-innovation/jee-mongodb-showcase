/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * <p>
 * Copyright (C) 2025 Carl Zeiss Digital Innovation GmbH
 */
package de.zeiss.mongodbws.geoservice.service;

import de.zeiss.mongodbws.geoservice.persistence.PersistenceService;
import de.zeiss.mongodbws.geoservice.persistence.entity.PointOfInterestEntity;
import de.zeiss.mongodbws.geoservice.rest.resource.PointOfInterest;
import de.zeiss.mongodbws.geoservice.service.mapper.PointOfInterestMapper;
import jakarta.ejb.LocalBean;
import jakarta.ejb.Stateless;
import jakarta.inject.Inject;
import org.bson.types.ObjectId;

import java.util.List;

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

    /**
     * Get a poi by id.
     *
     * @param id            String representation of object id.
     * @param expandDetails If true returnes all data of the poi.
     * @return
     */
    public PointOfInterest getPOI(String id, boolean expandDetails) {
        PointOfInterestEntity entity = persistenceService.getPointOfInterest(new ObjectId(id), expandDetails);

        if (entity == null) {
            return null;
        }

        return PointOfInterestMapper.mapToModel(entity);
    }

    /**
     * Create a new poi.
     *
     * @param poi
     * @return The new poi including its id.
     */
    public PointOfInterest createPOI(PointOfInterest poi) {

        PointOfInterestEntity entity = PointOfInterestMapper.mapToEntity(poi);

        entity = persistenceService.createPointOfInterest(entity);

        return PointOfInterestMapper.mapToModel(entity);
    }

    public PointOfInterest updatePOI(PointOfInterest poi) {
        if (poi.getId() == null) {
            throw new IllegalArgumentException("POI id must not be null for update.");
        }
        PointOfInterestEntity entity = persistenceService.getPointOfInterest(new ObjectId(poi.getId()), true);

        if (entity == null) {
            return null;
        }

        entity.setCategory(poi.getCategory());
        entity.setDetails(poi.getDetails());

        entity = persistenceService.updatePointOfInterest(entity);

        return PointOfInterestMapper.mapToModel(entity);
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
     * Returns a list of nearest points of interest.
     *
     * @param lat
     * @param lon
     * @param radius
     * @param expandDetails If true returnes all data of the poi.
     * @return
     */
    public List<PointOfInterest> listPOIs(double lat, double lon, int radius, boolean expandDetails) {
        List<PointOfInterestEntity> entityList = persistenceService.listPOIs(lat, lon, radius, expandDetails);

        return entityList.stream().map(PointOfInterestMapper::mapToModel).toList();
    }
}
