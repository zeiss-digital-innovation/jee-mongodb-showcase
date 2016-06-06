/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * 
 * Copyright (C) 2016 Saxonia Systems AG
 */
package de.saxsys.mongodbws.geoservice.rest.resource;

import static com.jayway.restassured.RestAssured.given;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertTrue;

import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.net.MalformedURLException;
import java.net.URISyntaxException;
import java.net.URL;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;
import java.util.logging.Level;
import java.util.logging.Logger;

import javax.ws.rs.core.Response.Status;
import javax.xml.bind.JAXBContext;
import javax.xml.bind.JAXBElement;
import javax.xml.bind.Unmarshaller;

import org.geojson.Point;
import org.junit.Before;
import org.junit.Ignore;
import org.junit.Test;

import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.jayway.restassured.RestAssured;
import com.jayway.restassured.config.ConnectionConfig;
import com.jayway.restassured.response.Header;
import com.jayway.restassured.response.Headers;
import com.jayway.restassured.response.Response;

import ca.carleton.gcrc.gpx.GpxWayPoint;
import ca.carleton.gcrc.gpx._11.Gpx11;

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
		RestAssured.config = RestAssured.config()
				.connectionConfig(new ConnectionConfig().closeIdleConnectionsAfterEachResponse());
	}

	@Ignore
	@Test
	public void createAndDeletePoi() {
		PointOfInterest poi = new PointOfInterest();
		poi.setName("Unit Test POI");
		poi.setCategory("Tankstelle");
		poi.setLocation(new Point(LONGITUDE_DRESDEN_FFP, LATITUDE_DRESDEN_FFP));

		// create
		Response response = given().headers(headers).contentType(CONTENT_TYPE).body(poi).expect().log().all()
				.post("poi");

		response.then().assertThat().statusCode(Status.CREATED.getStatusCode());

		String location = response.getHeader("location");

		assertNotNull("Location header must not be null", location);

		// get
		response = given().headers(headers).contentType(CONTENT_TYPE).body(poi).expect().log().all().get(location);

		response.then().assertThat().statusCode(Status.OK.getStatusCode());

		// now delete it
		response = given().headers(headers).contentType(CONTENT_TYPE).body(poi).expect().log().all().delete(location);

		response.then().assertThat().statusCode(Status.NO_CONTENT.getStatusCode());

		// at least check the deletion
		response = given().headers(headers).contentType(CONTENT_TYPE).body(poi).expect().log().all().get(location);

		response.then().assertThat().statusCode(Status.NOT_FOUND.getStatusCode());
	}

	@Ignore
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

	@Ignore
	@Test
	public void importDKVTestData() {
		String testDataFile = "DKV.csv";

		ClassLoader classLoader = ClassLoader.getSystemClassLoader();

		try {
			String content = new String(Files.readAllBytes(Paths.get(classLoader.getResource(testDataFile).toURI())));

			String[] split = content.split("\r\n");
			for (String line : split) {
				String[] parts = line.split(", ");

				PointOfInterest poi = new PointOfInterest();
				String name = parts[2].replace("\"", "").replace("   ", "\n");
				poi.setName(name);
				poi.setCategory("Tankstelle");
				poi.setLocation(new Point(Double.valueOf(parts[0]), Double.valueOf(parts[1])));

				// create
				Response response = given().headers(headers).contentType(CONTENT_TYPE).body(poi).expect().log().all()
						.post("poi");
			}
		} catch (IOException | URISyntaxException e) {
			throw new IllegalStateException("Error accessing file: " + testDataFile, e);
		}
	}

	@Test
	public void importTestData() throws InterruptedException {
		doImport("D-Lidl.gpx", "Supermarket");
		doImport("D-Aldi.gpx", "Supermarket");
		doImport("D-McDonalds.gpx", "Restaurant");
		doImport("DKV.gpx", "Gas Station");
	}

	private void doImport(String filename, String category) {

		ClassLoader classLoader = ClassLoader.getSystemClassLoader();

		try {
			File gpxFile = new File(classLoader.getResource(filename).toURI());

			JAXBContext jc11 = JAXBContext.newInstance("com.topografix.gpx._1._1");
			Unmarshaller unmarshaller = jc11.createUnmarshaller();
			JAXBElement result = (JAXBElement) unmarshaller.unmarshal(gpxFile);

			Gpx11 gpx11 = new Gpx11((com.topografix.gpx._1._1.GpxType) result.getValue());

			List<GpxWayPoint> wayPoints = gpx11.getWayPoints();

			for (GpxWayPoint gpxWayPoint : wayPoints) {
				System.out.println("lat: " + gpxWayPoint.getLat() + " lng: " + gpxWayPoint.getLong() + " name: "
						+ gpxWayPoint.getName());

				PointOfInterest poi = new PointOfInterest();
				String name = gpxWayPoint.getName().replace(", ", "\n");
				poi.setName(name);
				poi.setCategory(category);
				poi.setLocation(new Point(gpxWayPoint.getLong().doubleValue(), gpxWayPoint.getLat().doubleValue()));

				Response response = given().headers(headers).contentType(CONTENT_TYPE).body(poi).expect().log().all()
						.post("poi");
				Thread.sleep(10);
			}
		} catch (Exception e) {
			throw new IllegalStateException("Error accessing file: " + filename, e);
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
