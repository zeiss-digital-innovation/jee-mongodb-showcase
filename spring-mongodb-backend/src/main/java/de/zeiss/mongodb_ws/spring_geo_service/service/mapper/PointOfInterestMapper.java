package de.zeiss.mongodb_ws.spring_geo_service.service.mapper;

import de.zeiss.mongodb_ws.spring_geo_service.persistence.entity.PointOfInterestEntity;
import de.zeiss.mongodb_ws.spring_geo_service.rest.model.PointOfInterest;
import org.geojson.Point;
import org.springframework.data.mongodb.core.geo.GeoJsonPoint;

public class PointOfInterestMapper {

    public static PointOfInterest mapToResource(PointOfInterestEntity entity) {
        if (entity == null) {
            return null;
        }
        PointOfInterest resource = new PointOfInterest();
        resource.setId(entity.getId());
        resource.setName(entity.getName());
        resource.setCategory(entity.getCategory());
        resource.setDetails(entity.getDetails());
        resource.setLocation(new Point(entity.getLocation().getX(), entity.getLocation().getY()));

        return resource;
    }

    public static PointOfInterestEntity mapToEntity(PointOfInterest resource) {
        if (resource == null) {
            return null;
        }
        PointOfInterestEntity entity = new PointOfInterestEntity();

        if (resource.getId() != null) {
            entity.setId(resource.getId());
        } else if (resource.getHref() != null && !resource.getHref().isEmpty()) {
            // Extract id from href if possible
            String[] parts = resource.getHref().split("/");
            entity.setId(parts[parts.length - 1]);

            int lastIndexOfSlash = resource.getHref().lastIndexOf("/");

            if (lastIndexOfSlash > -1) {
                entity.setId(resource.getHref().substring(lastIndexOfSlash + 1));
            }
        }

        entity.setName(resource.getName());
        entity.setCategory(resource.getCategory());
        entity.setDetails(resource.getDetails());
        if (resource.getLocation() != null) {
            entity.setLocation(new GeoJsonPoint(
                    resource.getLocation().getCoordinates().getLongitude(),
                    resource.getLocation().getCoordinates().getLatitude()
            ));
        }

        return entity;
    }

    /**
     * Update an existing entity from a model object. Won't set the id.
     *
     * @param resource
     * @param entity
     */
    public static void updateEntityFromModel(PointOfInterest resource, PointOfInterestEntity entity) {
        entity.setCategory(resource.getCategory());
        entity.setName(resource.getName());
        entity.setDetails(resource.getDetails());

        entity.setLocation(new GeoJsonPoint(
                resource.getLocation().getCoordinates().getLongitude(),
                resource.getLocation().getCoordinates().getLatitude()
        ));
    }
}
