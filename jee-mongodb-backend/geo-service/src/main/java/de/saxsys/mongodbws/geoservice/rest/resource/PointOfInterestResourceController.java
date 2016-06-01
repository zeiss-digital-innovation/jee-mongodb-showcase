/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * 
 * Copyright (C) 2016 Saxonia Systems AG
 */
package de.saxsys.mongodbws.geoservice.rest.resource;

import java.net.URI;
import java.net.URISyntaxException;
import java.util.List;

import javax.inject.Inject;
import javax.ws.rs.Consumes;
import javax.ws.rs.DELETE;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.QueryParam;
import javax.ws.rs.core.Context;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.Response.Status;
import javax.ws.rs.core.UriInfo;

import de.saxsys.mongodbws.geoservice.rest.Constants;
import de.saxsys.mongodbws.geoservice.service.GeoDataService;

/**
 * 
 * @author Andreas Post
 */
@Path(Constants.POI_RESOURCE_PATH)
public class PointOfInterestResourceController {

	@Inject
	GeoDataService geoDataService;

	@Context
	protected UriInfo uriInfo;

	/**
	 * GET request on poi resource by id.
	 * 
	 * @param id
	 * @return
	 */
	@GET
	@Path("{id}")
	@Produces(Constants.MEDIA_TYPE_JSON)
	public Response getPOI(@PathParam("id") String id) {

		return Response.ok().header(Constants.CONTENT_ENC_KEY, Constants.CHARSET_UTF8).build();
	}

	/**
	 * POST request for new poi resource. Returns empty response with
	 * {@link Status#CREATED} (HTTP 201).
	 * 
	 * @param poi
	 * @return
	 */
	@POST
	@Consumes(Constants.MEDIA_TYPE_JSON)
	public Response createPOI(PointOfInterest poi) {

		URI location = null;

		PointOfInterest resultPoi = geoDataService.createPOI(poi);

		try {
			location = new URI(createUriString(resultPoi));
		} catch (URISyntaxException e) {
			return Response.serverError().header(Constants.CONTENT_ENC_KEY, Constants.CHARSET_UTF8).build();
		}

		return Response.created(location).header(Constants.CONTENT_ENC_KEY, Constants.CHARSET_UTF8).build();
	}

	/**
	 * DELETE request on poi resource by id. Returns empty response with
	 * {@link Status#NO_CONTENT} (HTTP 204).
	 * 
	 * @param id
	 * @return
	 */
	@DELETE
	@Path("{id}")
	@Produces(Constants.MEDIA_TYPE_JSON)
	public Response deletePOI(@PathParam("id") String id) {

		return Response.noContent().header(Constants.CONTENT_ENC_KEY, Constants.CHARSET_UTF8).build();
	}

	/**
	 * GET request on poi resource with latitude / longitude / radius.
	 * 
	 * @param latitude
	 * @param longitude
	 * @param radius
	 * @return list of poi's within radius from latitude / longitude
	 */
	@GET
	@Produces(Constants.MEDIA_TYPE_JSON)
	public Response listPOIs(@QueryParam("lat") double latitude, @QueryParam("lon") double longitude,
			@QueryParam("radius") int radius) {

		List<PointOfInterest> poiList = geoDataService.listPOIs(latitude, longitude, radius);

		for (PointOfInterest poi : poiList) {
			poi.setHref(createUriString(poi));
		}

		return Response.ok(poiList).header(Constants.CONTENT_ENC_KEY, Constants.CHARSET_UTF8).build();
	}

	private String createUriString(PointOfInterest poi) {
		return uriInfo.getBaseUri().toString() + Constants.POI_RESOURCE_PATH + poi.getId();
	}
}
