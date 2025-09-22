/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * 
 * Copyright (C) 2016 Saxonia Systems AG
 */
package de.zeiss.mongodbws.geoservice.rest.resource;

import static io.restassured.RestAssured.given;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertTrue;

import java.io.IOException;
import java.util.List;
import java.util.logging.Logger;

import jakarta.ws.rs.core.Response.Status;

import org.geojson.Point;
import org.junit.Ignore;
import org.junit.Test;

import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;
import io.restassured.response.Response;

/**
 * Some test for the REST interface.
 * 
 * @author andreas.post
 */
public class PointOfInterestResourceControllerIntTest extends TestsBase {

	private static final Logger LOG = Logger.getLogger(PointOfInterestResourceControllerIntTest.class.getName());

	private static final double LATITUDE_DRESDEN_FFP = 51.030812;

	private static final double LONGITUDE_DRESDEN_FFP = 13.730119;

	@Ignore
	@Test
	public void createAndDeletePoi() {
		PointOfInterest poi = new PointOfInterest();
		poi.setDetails("Unit Test POI");
		poi.setCategory("gasstation");
		poi.setLocation(new Point(LONGITUDE_DRESDEN_FFP, LATITUDE_DRESDEN_FFP));

		// create
		Response response = given().headers(headers).contentType(CONTENT_TYPE).body(poi).log().all()
				.post("poi");

		response.then().assertThat().statusCode(Status.CREATED.getStatusCode());

		String location = response.getHeader("location");

		assertNotNull("Location header must not be null", location);

		// get
		response = given().headers(headers).contentType(CONTENT_TYPE).body(poi).log().all().get(location);

		response.then().assertThat().statusCode(Status.OK.getStatusCode());

		// now delete it
		response = given().headers(headers).contentType(CONTENT_TYPE).body(poi).log().all().delete(location);

		response.then().assertThat().statusCode(Status.NO_CONTENT.getStatusCode());

		// at least check the deletion
		response = given().headers(headers).contentType(CONTENT_TYPE).body(poi).log().all().get(location);

		response.then().assertThat().statusCode(Status.NOT_FOUND.getStatusCode());
	}

	@Ignore
	@Test
	public void listPois() {
		Response response = given().headers(headers).contentType(CONTENT_TYPE).queryParam("lat", LATITUDE_DRESDEN_FFP)
				.queryParam("lon", LONGITUDE_DRESDEN_FFP).queryParam("radius", 5000).log().all().get("poi");

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

		String poiType = "gasstation";

		for (PointOfInterest poi : poiList) {

			assertTrue("Wrong Poi-type: " + poi.getCategory() + " - " + poiType + " erwartet.",
					poiType.equals(poi.getCategory()));
		}
	}
}
