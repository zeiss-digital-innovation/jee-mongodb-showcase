package de.zeiss.mongodb_ws.spring_geo_service.persistence;

import de.zeiss.mongodb_ws.spring_geo_service.persistence.entity.PointOfInterestEntity;
import org.springframework.data.geo.Distance;
import org.springframework.data.geo.Point;
import org.springframework.data.mongodb.repository.MongoRepository;

import java.util.List;

public interface IPointOfInterestRepository extends MongoRepository<PointOfInterestEntity, String> {

    List<PointOfInterestEntity> findByLocationNear(Point location, Distance distance);
}
