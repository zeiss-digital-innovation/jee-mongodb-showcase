package de.saxsys.mongodbws.geoservice.rest.resource;

import java.io.IOException;
import java.io.InputStream;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;
import java.util.logging.Level;
import java.util.logging.Logger;

import org.junit.Before;

import io.restassured.RestAssured;
import io.restassured.config.ConnectionConfig;
import io.restassured.http.ContentType;
import io.restassured.http.Header;
import io.restassured.http.Headers;

/**
 * Abstract base class for several tests.
 * 
 * @author Andreas Post
 */
public abstract class TestsBase {

	private static final Logger LOG = Logger.getLogger(TestsBase.class.getName());

	protected static final String CONTENT_TYPE = "application/json; charset=UTF-8";

	protected static final String BASE_PATH = "geoservice/rest";

	protected Headers headers;

	@Before
	public void init() throws MalformedURLException {
		List<Header> headerList = new ArrayList<Header>();
		headerList = new ArrayList<Header>();
		headerList.add(new Header("Accept-Language", "de"));

		headers = new Headers(headerList);

		Properties props = loadProperties("test-config.properties");
		String hostProperty = (String) props.get("test.hostname");
		String portProperty = (String) props.get("test.port");

		URL baseURL = new URL("http://" + hostProperty + ":" + portProperty + "/" + BASE_PATH);

		RestAssured.baseURI = String.format("%s://%s:%s", baseURL.getProtocol(), baseURL.getHost(), baseURL.getPort());
		RestAssured.port = baseURL.getPort();
		RestAssured.basePath = BASE_PATH;
		RestAssured.config = RestAssured.config()
				.connectionConfig(new ConnectionConfig().closeIdleConnectionsAfterEachResponse());
	}

	/**
	 * Load props for test.
	 * 
	 * @param propertyRessource
	 * @return
	 */
	protected Properties loadProperties(final String propertyRessource) {

		if (propertyRessource == null) {
			throw new IllegalArgumentException("Missing propertyRessource name.");
		}

		ClassLoader classLoader = ClassLoader.getSystemClassLoader();

		Properties result = null;
		InputStream is = null;

		try {
			is = classLoader.getResourceAsStream(propertyRessource);

			if (is != null) {
				result = new Properties();
				result.load(is);
			} else {
				throw new IllegalArgumentException("File not found: " + propertyRessource);
			}
		} catch (IOException e) {
			result = null;
			throw new IllegalStateException("Error accessing file: " + propertyRessource);
		} finally {
			if (is != null) {
				try {
					is.close();
				} catch (IOException e) {
					LOG.log(Level.SEVERE, "Could not close stream", e);
				}
			}
		}
		return result;
	}
}
