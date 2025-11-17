package de.zeiss.mongodbws.geoservice.config;

import org.testcontainers.utility.DockerImageName;

import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.util.Properties;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * Central test configuration for tests.
 * Properties can be configured in three ways: system property, environment variable and test-config.properties.
 * <p>
 * The order of preference is: system property > environment variable > test-config.properties.
 * <p>
 *
 */
public final class TestConfig {


    private static Properties properties;

    private static final Logger LOGGER = Logger.getLogger(TestConfig.class.getName());

    private static final String PROP_FILE = "test-config.properties";

    static {
        initProperties();
    }

    private TestConfig() {
        // utility
    }

    private static String getFromEnvOrProperty(String systemPropName, String envName, String confPropName, String defaultValue) {
        String value = System.getProperty(systemPropName);
        if (isSet(value)) {
            return value;
        }
        value = System.getenv(envName);
        if (isSet(value)) {
            return value;
        }
        value = properties.getProperty(confPropName);
        if (isSet(value)) {
            return value;
        }
        return defaultValue;
    }

    private static void initProperties() {
        properties = new Properties();

        try (InputStream input = TestConfig.class.getClassLoader().getResourceAsStream(PROP_FILE)) {
            properties.load(input);
        } catch (FileNotFoundException e) {
            LOGGER.warning("Cannot find property file: " + PROP_FILE);
        } catch (IOException e) {
            LOGGER.log(Level.WARNING, "Cannot load property file: " + PROP_FILE, e);
        }
    }

    private static boolean isSet(String string) {
        return string != null && !string.isBlank();
    }

    /**
     * Docker Image Name for MongoDB.
     * Usage: override the image by setting the system property `MONGODB_IMAGE` or
     * environment variable `MONGODB_IMAGE` or the config property mongodb.image.
     * <p>
     * mvn -DMONGODB_IMAGE=mongo:8.0 test
     */
    public static final DockerImageName MONGODB_IMAGE = DockerImageName.parse(
            getFromEnvOrProperty("MONGODB_IMAGE", "MONGODB_IMAGE", "mongodb.image", "mongo:8.0")
    );
}

