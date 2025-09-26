package de.zeiss.mongodbws.testdatageneration;

import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;

/**
 * Unit tests for the Config class.
 *
 * @author AI Generated
 */
public class ConfigTest {

    @Test
    void hostWithoutSchemeAddsHttp() {
        String result = Config.getPoiServiceUrl("example.com", "8080", "/path");
        assertEquals("http://example.com:8080/path", result);
    }

    @Test
    void hostWithHttpsKeepsSchemeAndOmitsEmptyPort() {
        String result = Config.getPoiServiceUrl("https://example.com", "", "path");
        assertEquals("https://example.com/path", result);
    }

    @Test
    void nullPortIsOmitted() {
        String result = Config.getPoiServiceUrl("example.com", null, "path");
        assertEquals("http://example.com/path", result);
    }

    @Test
    void pathWithoutLeadingSlashIsFixed() {
        String result = Config.getPoiServiceUrl("example.com", "8080", "path");
        assertEquals("http://example.com:8080/path", result);
    }

    @Test
    void emptyPathIsOmitted() {
        String result = Config.getPoiServiceUrl("example.com", "8080", "");
        assertEquals("http://example.com:8080", result);
    }
}
