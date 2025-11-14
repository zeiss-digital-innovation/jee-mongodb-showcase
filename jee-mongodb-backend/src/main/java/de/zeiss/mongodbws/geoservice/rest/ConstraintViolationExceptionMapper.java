package de.zeiss.mongodbws.geoservice.rest;

import de.zeiss.mongodbws.geoservice.rest.resource.ConstraintViolationInfo;
import jakarta.validation.ConstraintViolationException;
import jakarta.ws.rs.core.Response;
import jakarta.ws.rs.ext.ExceptionMapper;
import jakarta.ws.rs.ext.Provider;

import java.util.stream.Collectors;

/**
 * Exception mapper to handle ConstraintViolationExceptions and return a proper JSON response.
 */
@Provider
public class ConstraintViolationExceptionMapper implements ExceptionMapper<ConstraintViolationException> {

    @Override
    public Response toResponse(ConstraintViolationException exception) {
        var violations = exception.getConstraintViolations().stream()
                .map(v -> new ConstraintViolationInfo(v.getMessage(), v.getInvalidValue()))
                .collect(Collectors.toList());
        return Response.status(Response.Status.BAD_REQUEST)
                .entity(violations)
                .type(Constants.MEDIA_TYPE_JSON)
                .build();
    }
}
