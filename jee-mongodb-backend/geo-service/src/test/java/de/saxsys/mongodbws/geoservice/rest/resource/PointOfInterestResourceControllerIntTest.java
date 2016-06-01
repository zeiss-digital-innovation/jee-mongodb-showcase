/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * 
 * Copyright (C) 2016 Saxonia Systems AG
 */
package de.saxsys.mongodbws.geoservice.rest.resource;

import static com.jayway.restassured.RestAssured.given;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertTrue;

import java.io.IOException;
import java.io.InputStream;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;
import java.util.logging.Level;
import java.util.logging.Logger;

import javax.ws.rs.core.Response.Status;

import org.geojson.Point;
import org.junit.Before;
import org.junit.Test;

import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.jayway.restassured.RestAssured;
import com.jayway.restassured.response.Header;
import com.jayway.restassured.response.Headers;
import com.jayway.restassured.response.Response;

/**
 * Some test for the REST interface.
 * 
 * @author andreas.post
 */
public class PointOfInterestResourceControllerIntTest {

	private static final Logger LOG = Logger.getLogger(PointOfInterestResourceControllerIntTest.class.getName());

	private static final double LATITUDE_DRESDEN_FFP = 51.030812;

	private static final double LONGITUDE_DRESDEN_FFP = 13.730119;

	private static final String CONTENT_TYPE = "application/json; charset=UTF-8";

	private static final String BASE_PATH = "geoservice/rest";

	private Headers headers;

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
	}

	@Test
	public void createPoi() {
		PointOfInterest poi = new PointOfInterest();
		poi.setName("Unit Test POI");
		poi.setCategory("Tankstelle");
		poi.setLocation(new Point(LONGITUDE_DRESDEN_FFP, LATITUDE_DRESDEN_FFP));

		Response response = given().headers(headers).contentType(CONTENT_TYPE).body(poi).expect().log().all()
				.post("poi");

		response.then().assertThat().statusCode(Status.CREATED.getStatusCode());

		assertNotNull("Location header must not be null", response.getHeader("location"));
	}

	@Test
	public void listPois() {
		Response response = given().headers(headers).contentType(CONTENT_TYPE).queryParam("lat", LATITUDE_DRESDEN_FFP)
				.queryParam("lon", LONGITUDE_DRESDEN_FFP).queryParam("radius", 5000).expect().log().all().get("poi");

		response.then().assertThat().statusCode(Status.OK.getStatusCode());

		List<PointOfInterest> poiList = null;

		try {
			poiList = new ObjectMapper().readValue(response.body().asString(),
					new TypeReference<List<PointOfInterest>>() {
					});
		} catch (IOException e) {
			assertTrue(e.getMessage(), false);
		}

		assertNotNull("Expected at least an empty list.", poiList);

		String poiType = "Tankstelle";

		for (PointOfInterest poi : poiList) {

			assertTrue("Wrong Poi-type: " + poi.getCategory() + " - " + poiType + " erwartet.",
					poiType.equals(poi.getCategory()));
		}
	}

	/**
	 * Load props for test.
	 * 
	 * @param propertyRessource
	 * @return
	 */
	private static Properties loadProperties(final String propertyRessource) {

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
