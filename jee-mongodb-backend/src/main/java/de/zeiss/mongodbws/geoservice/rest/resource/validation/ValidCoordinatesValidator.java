package de.zeiss.mongodbws.geoservice.rest.resource.validation;

import jakarta.validation.ConstraintValidator;
import jakarta.validation.ConstraintValidatorContext;
import org.geojson.Point;

public class ValidCoordinatesValidator implements ConstraintValidator<ValidCoordinates, Point> {

    @Override
    public boolean isValid(Point point, ConstraintValidatorContext context) {
        if (point == null) {
            return false;
        }

        double latitude = point.getCoordinates().getLatitude();
        double longitude = point.getCoordinates().getLongitude();

        return latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180;
    }
}