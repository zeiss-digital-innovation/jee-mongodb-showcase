package de.zeiss.mongodb_ws.spring_geo_service.persistence;

import de.zeiss.mongodb_ws.spring_geo_service.persistence.entity.PointOfInterestEntity;
import org.springframework.data.mongodb.repository.MongoRepository;

public interface IPointOfInterestRepository extends MongoRepository<PointOfInterestEntity, String> {
}
