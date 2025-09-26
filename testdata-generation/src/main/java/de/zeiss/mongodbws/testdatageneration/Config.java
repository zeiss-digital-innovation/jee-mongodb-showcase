package de.zeiss.mongodbws.testdatageneration;

import java.io.IOException;
import java.io.InputStream;
import java.io.UncheckedIOException;
import java.util.Properties;

/**
 * Utility class to load configuration properties from application.properties file.
 *
 * @author AI Generated
 * @author Andreas Post
 */
public final class Config {

    private static final Properties PROPS = new Properties();

    private static final String POI_SERVICE_HOST = "poiservice.host";
    private static final String POI_SERVICE_PORT = "poiservice.port";
    private static final String POI_SERVICE_PATH = "poiservice.restpath";

    static {
        try (InputStream is = Thread.currentThread()
                .getContextClassLoader()
                .getResourceAsStream("application.properties")) {
            if (is != null) {
                PROPS.load(is);
            }
        } catch (IOException e) {
            throw new UncheckedIOException("Failed to load application.properties", e);
        }
    }

    private Config() {
        // we don't want instances of this class
    }

    /**
     * Constructs the full URL for the POI service based on configuration properties.
     *
     * @return
     */
    public static String getPoiServiceUrl() {
        String host = PROPS.getProperty(POI_SERVICE_HOST, "localhost");
        String port = PROPS.getProperty(POI_SERVICE_PORT); // no default port to support standard ports 80 and 443
        String path = PROPS.getProperty(POI_SERVICE_PATH, "/geoservice/rest/poi");

        return getPoiServiceUrl(host, port, path);
    }

    /**
     * Constructs the full URL for the POI service based on given parameters.
     * <p>
     * We could integrate this method into getPoiServiceUrl, but this way we can test it more easily.
     *
     * @param host the host of the POI service
     * @param port the port of the POI service, can be null or empty if standard ports are used
     * @param path the path of the POI service
     * @return the full URL as a String
     */
    public static String getPoiServiceUrl(String host, String port, String path) {
        String url = "";

        if (!host.startsWith("http://") && !host.startsWith("https://")) {
            url += "http://";
        }
        url += host;

        if (port != null && !port.isEmpty()) {
            url += ":" + port;
        }

        if (path != null && !path.isEmpty()) {
            if (!path.startsWith("/")) {
                url += "/";
            }
            url += path;
        }
        return url;
    }
}
