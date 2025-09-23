package de.zeiss.mongodbws.geoservice.service.mapper;

import de.zeiss.mongodbws.geoservice.persistence.entity.GeoPoint;
import org.geojson.Point;

public class PointMapper {

    public static Point mapToModel(GeoPoint entity) {
        if (entity == null) {
            return null;
        }
        return new Point(entity.getLongitude(), entity.getLatitude());
    }

    public static GeoPoint mapToEntity(Point model) {
        if (model == null || model.getCoordinates() == null) {
            return null;
        }
        GeoPoint point = new GeoPoint();
        point.setCoordinates(model.getCoordinates().getLatitude(), model.getCoordinates().getLongitude());
        return point;
    }
}
