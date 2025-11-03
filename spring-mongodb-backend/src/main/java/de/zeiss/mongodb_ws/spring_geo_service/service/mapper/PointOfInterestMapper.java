package de.zeiss.mongodb_ws.spring_geo_service.service.mapper;

import de.zeiss.mongodb_ws.spring_geo_service.persistence.entity.PointOfInterestEntity;
import de.zeiss.mongodb_ws.spring_geo_service.rest.model.PointOfInterest;
import org.geojson.Point;

public class PointOfInterestMapper {

    public static PointOfInterest mapToModel(PointOfInterestEntity entity) {
        if (entity == null) {
            return null;
        }
        PointOfInterest model = new PointOfInterest();
        model.setId(entity.getId());
        model.setName(entity.getName());
        model.setCategory(entity.getCategory());
        model.setDetails(entity.getDetails());
        model.setLocation(new Point(entity.getLocation().getX(), entity.getLocation().getY()));

        return model;
    }
}
