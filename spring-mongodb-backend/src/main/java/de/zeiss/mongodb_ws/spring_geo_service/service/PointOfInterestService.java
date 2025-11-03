package de.zeiss.mongodb_ws.spring_geo_service.service;

import de.zeiss.mongodb_ws.spring_geo_service.persistence.IPointOfInterestRepository;
import de.zeiss.mongodb_ws.spring_geo_service.persistence.entity.PointOfInterestEntity;
import de.zeiss.mongodb_ws.spring_geo_service.rest.model.PointOfInterest;
import de.zeiss.mongodb_ws.spring_geo_service.service.mapper.PointOfInterestMapper;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.util.Optional;

@Service
public class PointOfInterestService {

    @Autowired
    private IPointOfInterestRepository poiRepository;

    public PointOfInterest getPointOfInterestById(String id) {
        Optional<PointOfInterestEntity> pointOfInterestEntity = poiRepository.findById(id);

        return pointOfInterestEntity.map(PointOfInterestMapper::mapToModel).orElse(null);
    }
}
