package de.zeiss.mongodbws.geoservice.rest.resource.validation;

import org.geojson.Point;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.Arguments;
import org.junit.jupiter.params.provider.MethodSource;

import java.util.stream.Stream;

import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;

class ValidCoordinatesValidatorTest {

    private static final ValidCoordinatesValidator VALIDATOR = new ValidCoordinatesValidator();

    static Stream<Arguments> validCoordinatesProvider() {
        return Stream.of(
                Arguments.of(12.34, 56.78, "somewhere valid"),
                Arguments.of(-12.34, -56.78, "negative valid scenario"),
                Arguments.of(90.0, 56.78, "positive edge case latitude"),
                Arguments.of(-90.0, -56.78, "negative edge case latitude"),
                Arguments.of(12.34, 180.0, "positive edge case longitude"),
                Arguments.of(-12.34, -180.0, "negative edge case longitude")
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

    @ParameterizedTest(name = "Valid parameters #{index}: lon={0}, lat={1}, description={3}")
    @MethodSource("validCoordinatesProvider")
    public void testIsValid_ValidCoordinates_ReturnsTrue(double latitude, double longitude, String description) {
        Point p = new Point(longitude, latitude);
        assertTrue(VALIDATOR.isValid(p, null), description);
    }

    @ParameterizedTest(name = "Invalid parameters #{index}: lon={0}, lat={1}, description={3}")
    @MethodSource("invalidCoordinatesProvider")
    public void testIsValid_InvalidCoordinates_ReturnsFalse(double latitude, double longitude, String description) {
        Point p = new Point(longitude, latitude);
        assertFalse(VALIDATOR.isValid(p, null), description);
    }
}