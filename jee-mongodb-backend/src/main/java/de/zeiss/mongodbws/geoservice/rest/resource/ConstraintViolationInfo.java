package de.zeiss.mongodbws.geoservice.rest.resource;

/**
 * Little model class to represent a constraint violation information for Bad Request JSON Responses.
 */
public class ConstraintViolationInfo {
    public String message;
    public Object value;

    public ConstraintViolationInfo(String message, Object value) {
        this.message = message;
        this.value = value;
    }
}
