/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * <p>
 * Copyright (C) 2025 Carl Zeiss Digital Innovation GmbH
 */
package de.zeiss.mongodbws.geoservice.service.mapper;

import de.zeiss.mongodbws.geoservice.persistence.entity.PointOfInterestEntity;
import de.zeiss.mongodbws.geoservice.rest.resource.PointOfInterest;

/**
 * Converts {@link PointOfInterestEntity} to {@link PointOfInterest} and back.
 *
 * @author Andreas Post
 */
public class PointOfInterestMapper {

    /**
     * Convert entity to model objects.
     *
     * @param entity the entity to convert
     * @return the converted model object
     */
    public static PointOfInterest mapToModel(PointOfInterestEntity entity) {
        PointOfInterest poi = new PointOfInterest();
        poi.setId(ObjectIdMapper.mapToString(entity.getId()));
        poi.setCategory(entity.getCategory());
        poi.setName(entity.getName());
        poi.setDetails(entity.getDetails());
        poi.setLocation(PointMapper.mapToModel(entity.getLocation()));

        return poi;
    }

    /**
     * Convert model object back to entity.
     *
     * @param poi
     * @return
     */
    public static PointOfInterestEntity mapToEntity(PointOfInterest poi) {
        PointOfInterestEntity entity = new PointOfInterestEntity();

        entity.setId(ObjectIdMapper.mapToObjectId(poi.getId(), poi.getHref()));

        entity.setCategory(poi.getCategory());
        entity.setName(poi.getName());
        entity.setDetails(poi.getDetails());

        entity.setLocation(PointMapper.mapToEntity(poi.getLocation()));

        return entity;
    }
}
