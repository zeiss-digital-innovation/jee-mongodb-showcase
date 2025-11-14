package de.zeiss.mongodbws.geoservice.rest;

import de.zeiss.mongodbws.geoservice.rest.resource.ConstraintViolationInfo;
import jakarta.validation.ConstraintViolation;
import jakarta.validation.ConstraintViolationException;
import jakarta.ws.rs.core.Response;
import org.junit.jupiter.api.Test;
import org.mockito.Mockito;

import java.util.List;
import java.util.Set;

import static org.junit.jupiter.api.Assertions.assertEquals;

class ConstraintViolationExceptionMapperTest {

    @Test
    public void testToResponse() {
        ConstraintViolation<?> mockViolation = Mockito.mock(ConstraintViolation.class);
        Mockito.when(mockViolation.getMessage()).thenReturn("Invalid value");
        Mockito.when(mockViolation.getInvalidValue()).thenReturn("some invalid value");
        Set<ConstraintViolation<?>> violations = Set.of(mockViolation);

        ConstraintViolationException exception = new ConstraintViolationException("Test exception", violations);
        ConstraintViolationExceptionMapper mapper = new ConstraintViolationExceptionMapper();
        try (Response response = mapper.toResponse(exception)) {
            assert response.getStatus() == Response.Status.BAD_REQUEST.getStatusCode();
            @SuppressWarnings("unchecked")
            var entity = (List<ConstraintViolationInfo>) response.getEntity();
            assertEquals(1, entity.size());
            ConstraintViolationInfo info = entity.getFirst();

            assertEquals("Invalid value", info.message);
            assertEquals("some invalid value", info.value);
        }
    }
}