package de.zeiss.mongodbws.geoservice.rest.resource.validation;

import jakarta.validation.Constraint;
import jakarta.validation.Payload;

import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

@Constraint(validatedBy = ValidCoordinatesValidator.class)
@Target({ElementType.FIELD})
@Retention(RetentionPolicy.RUNTIME)
public @interface ValidCoordinates {

    String message() default "Invalid coordinates: latitude must be between -90 and 90, and longitude must be between -180 and 180.";

    Class<?>[] groups() default {};

    Class<? extends Payload>[] payload() default {};
}