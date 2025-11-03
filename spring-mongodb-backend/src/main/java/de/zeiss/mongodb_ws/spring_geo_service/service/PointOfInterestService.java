package de.zeiss.mongodb_ws.spring_geo_service.service;

import de.zeiss.mongodb_ws.spring_geo_service.persistence.IPointOfInterestRepository;
import de.zeiss.mongodb_ws.spring_geo_service.persistence.entity.PointOfInterestEntity;
import de.zeiss.mongodb_ws.spring_geo_service.rest.model.PointOfInterest;
import de.zeiss.mongodb_ws.spring_geo_service.service.mapper.PointOfInterestMapper;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.geo.Distance;
import org.springframework.data.geo.Metrics;
import org.springframework.data.geo.Point;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Optional;
import java.util.logging.Logger;

@Service
public class PointOfInterestService {

    @Autowired
    private IPointOfInterestRepository poiRepository;

    Logger logger = Logger.getLogger(PointOfInterestService.class.getName());

    public PointOfInterest getPointOfInterestById(String id) {
        Optional<PointOfInterestEntity> pointOfInterestEntity = poiRepository.findById(id);

        return pointOfInterestEntity.map(PointOfInterestMapper::mapToResource).orElse(null);
    }

    public List<PointOfInterest> listPOIs(double lat, double lon, int radius, boolean expandDetails) {
        double radiusInKm = radius / 1000.0; // Convert radius from meters to kilometers
        Point p = new Point(lon, lat);
        Distance d = new Distance(radiusInKm, Metrics.KILOMETERS);
        logger.info("Searching POIs near point: " + p + " with radius: " + d);

        List<PointOfInterestEntity> entityList = poiRepository.findByLocationNear(p, d);

        logger.info("Found " + entityList.size() + " POIs");

        if (!expandDetails) {
            entityList.forEach(poi -> poi.setDetails(null));
        }

        return entityList.stream().map(PointOfInterestMapper::mapToResource).toList();
    }

    public PointOfInterest createPOI(PointOfInterest poi) {
        PointOfInterestEntity entity = PointOfInterestMapper.mapToEntity(poi);

        entity = poiRepository.save(entity);

        return PointOfInterestMapper.mapToResource(entity);
    }

    public void deletePOI(String id) {
        logger.info("Deleting POI with id: " + id);
        poiRepository.deleteById(id);
    }
}
