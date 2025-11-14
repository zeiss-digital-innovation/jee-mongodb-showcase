package de.zeiss.mongodbws.geoservice.rest.resource;

import de.zeiss.mongodbws.geoservice.service.GeoDataService;
import jakarta.validation.ConstraintViolation;
import jakarta.validation.Validation;
import jakarta.validation.Validator;
import jakarta.validation.ValidatorFactory;
import org.geojson.Point;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.Arguments;
import org.junit.jupiter.params.provider.MethodSource;
import org.mockito.Mockito;

import java.util.Set;
import java.util.stream.Stream;

import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;

/**
 * Validation tests for PointOfInterestResourceController for list, create and update operations.
 */
public class PointOfInterestResourceControllerValidationTest {

    private PointOfInterestResourceController controller;
    private Validator validator;

    static Stream<Arguments> validFindParametersProvider() {
        return Stream.of(
                Arguments.of(12.34, 56.78, 1000, "somewhere valid"),
                Arguments.of(-12.34, -56.78, 5000, "negative valid scenario"),
                Arguments.of(90.0, 56.78, 1000, "positive edge case latitude"),
                Arguments.of(-90.0, -56.78, 1000, "negative edge case latitude"),
                Arguments.of(12.34, 180.0, 1000, "positive edge case  longitude"),
                Arguments.of(-12.34, -180.0, 1000, "negative edge case longitude"),
                Arguments.of(12.34, 56.78, 1, "lower edge case radius"),
                Arguments.of(12.34, 56.78, 100000, "upper edge case radius")
        );
    }

    static Stream<Arguments> invalidFindParametersProvider() {
        return Stream.of(
                Arguments.of(-90.1, -180.0, 1000, "invalid lower bound latitude"),
                Arguments.of(90.1, 180.0, 1000, "invalid upper bound latitude"),
                Arguments.of(-90.0, -180.1, 1000, "invalid lower bound longitude"),
                Arguments.of(90.0, -180.1, 1000, "invalid lower bound longitude"),
                Arguments.of(90.0, 180.0, -1, "invalid negative radius"),
                Arguments.of(90.0, 180.0, 0, "invalid lower bound radius"),
                Arguments.of(90.0, 180.0, 100001, "invalid upper bound radius")
        );
    }

    static Stream<Arguments> invalidCoordinatesProvider() {
        return Stream.of(
                Arguments.of(-90.1, -180.0, "invalid lower bound latitude"),
                Arguments.of(90.1, 180.0, "invalid upper bound latitude"),
                Arguments.of(-90.0, -180.1, "invalid lower bound longitude"),
                Arguments.of(90.0, -180.1, "invalid lower bound longitude")
        );
    }

    @BeforeEach
    public void setUp() {
        controller = new PointOfInterestResourceController();
        controller.geoDataService = Mockito.mock(GeoDataService.class);
        try (ValidatorFactory factory = Validation.buildDefaultValidatorFactory()) {
            validator = factory.getValidator();
        }
    }

    @ParameterizedTest(name = "Valid parameters #{index}: latitude={0}, longitude={1}, radius={2}, description={3}")
    @MethodSource("validFindParametersProvider")
    public void testListPOIs_ValidParameters_ShouldPassValidation(double latitude, double longitude, int radius, String description) throws NoSuchMethodException {
        // Validate parameters
        Set<ConstraintViolation<PointOfInterestResourceController>> violations = validator.forExecutables()
                .validateParameters(controller,
                        PointOfInterestResourceController.class.getDeclaredMethod("listPOIs", double.class, double.class, int.class, String.class),
                        new Object[]{latitude, longitude, radius, null});
        assertTrue(violations.isEmpty(), "Expected no validation violations for valid parameters");
    }

    @ParameterizedTest(name = "Invalid parameters #{index}: latitude={0}, longitude={1}, radius={2}, description={3}")
    @MethodSource("invalidFindParametersProvider")
    public void testListPOIs_InvalidParameters_ShouldFailValidation(double latitude, double longitude, int radius, String description) throws NoSuchMethodException {
        Set<ConstraintViolation<PointOfInterestResourceController>> violations = validator.forExecutables()
                .validateParameters(controller,
                        PointOfInterestResourceController.class.getDeclaredMethod("listPOIs", double.class, double.class, int.class, String.class),
                        new Object[]{latitude, longitude, radius, null});
        assertFalse(violations.isEmpty(), "Expected validation violation");
    }

    @ParameterizedTest(name = "Invalid parameters #{index}: latitude={0}, longitude={1}, description={3}")
    @MethodSource("invalidCoordinatesProvider")
    public void testCreatePoi_InvalidCoordinates_ShouldFailValidation(double latitude, double longitude, String description) throws NoSuchMethodException {
        PointOfInterest poi = new PointOfInterest();
        poi.setName("Unit Test POI");
        poi.setCategory("gasstation");
        poi.setLocation(new Point(longitude, latitude));

        Set<ConstraintViolation<PointOfInterestResourceController>> violations = validator.forExecutables()
                .validateParameters(controller,
                        PointOfInterestResourceController.class.getDeclaredMethod("createPOI", PointOfInterest.class),
                        new Object[]{poi});
        assertFalse(violations.isEmpty(), "Expected validation violation");
    }

    @Test
    public void testCreatePoi_MissingCoordinates_ShouldFailValidation() throws NoSuchMethodException {
        PointOfInterest poi = new PointOfInterest();
        poi.setName("Unit Test POI");
        poi.setCategory("gasstation");

        Set<ConstraintViolation<PointOfInterestResourceController>> violations = validator.forExecutables()
                .validateParameters(controller,
                        PointOfInterestResourceController.class.getDeclaredMethod("createPOI", PointOfInterest.class),
                        new Object[]{poi});
        assertFalse(violations.isEmpty(), "Expected validation violation");
    }

    @Test
    public void testCreatePoi_MissingName_ShouldFailValidation() throws NoSuchMethodException {
        PointOfInterest poi = new PointOfInterest();
        poi.setCategory("gasstation");
        poi.setLocation(new Point(13.730119, 51.030812));

        Set<ConstraintViolation<PointOfInterestResourceController>> violations = validator.forExecutables()
                .validateParameters(controller,
                        PointOfInterestResourceController.class.getDeclaredMethod("createPOI", PointOfInterest.class),
                        new Object[]{poi});
        assertFalse(violations.isEmpty(), "Expected validation violation");
    }

    @Test
    public void testCreatePoi_MissingCategory_ShouldFailValidation() throws NoSuchMethodException {
        PointOfInterest poi = new PointOfInterest();
        poi.setName("Unit Test POI");
        poi.setLocation(new Point(13.730119, 51.030812));

        Set<ConstraintViolation<PointOfInterestResourceController>> violations = validator.forExecutables()
                .validateParameters(controller,
                        PointOfInterestResourceController.class.getDeclaredMethod("createPOI", PointOfInterest.class),
                        new Object[]{poi});
        assertFalse(violations.isEmpty(), "Expected validation violation");
    }

    @ParameterizedTest(name = "Invalid parameters #{index}: latitude={0}, longitude={1}, description={3}")
    @MethodSource("invalidCoordinatesProvider")
    public void testUpdatePoi_InvalidCoordinates_ShouldFailValidation(double latitude, double longitude, String description) throws NoSuchMethodException {
        PointOfInterest poi = new PointOfInterest();
        poi.setName("Unit Test POI");
        poi.setCategory("gasstation");
        poi.setLocation(new Point(longitude, latitude));

        Set<ConstraintViolation<PointOfInterestResourceController>> violations = validator.forExecutables()
                .validateParameters(controller,
                        PointOfInterestResourceController.class.getDeclaredMethod("updatePOI", String.class, PointOfInterest.class),
                        new Object[]{"testID", poi});
        assertFalse(violations.isEmpty(), "Expected validation violation");
    }

    @Test
    public void testUpdatePoi_MissingCoordinates_ShouldFailValidation() throws NoSuchMethodException {
        PointOfInterest poi = new PointOfInterest();
        poi.setName("Unit Test POI");
        poi.setCategory("gasstation");

        Set<ConstraintViolation<PointOfInterestResourceController>> violations = validator.forExecutables()
                .validateParameters(controller,
                        PointOfInterestResourceController.class.getDeclaredMethod("updatePOI", String.class, PointOfInterest.class),
                        new Object[]{"testID", poi});
        assertFalse(violations.isEmpty(), "Expected validation violation");
    }

    @Test
    public void testUpdatePoi_MissingName_ShouldFailValidation() throws NoSuchMethodException {
        PointOfInterest poi = new PointOfInterest();
        poi.setCategory("gasstation");
        poi.setLocation(new Point(13.730119, 51.030812));

        Set<ConstraintViolation<PointOfInterestResourceController>> violations = validator.forExecutables()
                .validateParameters(controller,
                        PointOfInterestResourceController.class.getDeclaredMethod("updatePOI", String.class, PointOfInterest.class),
                        new Object[]{"testID", poi});
        assertFalse(violations.isEmpty(), "Expected validation violation");
    }

    @Test
    public void testUpdatePoi_MissingCategory_ShouldFailValidation() throws NoSuchMethodException {
        PointOfInterest poi = new PointOfInterest();
        poi.setName("Unit Test POI");
        poi.setLocation(new Point(13.730119, 51.030812));

        Set<ConstraintViolation<PointOfInterestResourceController>> violations = validator.forExecutables()
                .validateParameters(controller,
                        PointOfInterestResourceController.class.getDeclaredMethod("updatePOI", String.class, PointOfInterest.class),
                        new Object[]{"testID", poi});
        assertFalse(violations.isEmpty(), "Expected validation violation");
    }
}

