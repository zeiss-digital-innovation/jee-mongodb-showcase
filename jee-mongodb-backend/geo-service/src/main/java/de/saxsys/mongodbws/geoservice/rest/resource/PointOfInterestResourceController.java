/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * 
 * Copyright (C) 2016 Saxonia Systems AG
 */
package de.saxsys.mongodbws.geoservice.rest.resource;

import java.net.URI;
import java.net.URISyntaxException;
import java.util.List;

import jakarta.inject.Inject;
import jakarta.ws.rs.Consumes;
import jakarta.ws.rs.DELETE;
import jakarta.ws.rs.GET;
import jakarta.ws.rs.NotFoundException;
import jakarta.ws.rs.POST;
import jakarta.ws.rs.Path;
import jakarta.ws.rs.PathParam;
import jakarta.ws.rs.Produces;
import jakarta.ws.rs.QueryParam;
import jakarta.ws.rs.core.Context;
import jakarta.ws.rs.core.Response;
import jakarta.ws.rs.core.Response.Status;
import jakarta.ws.rs.core.UriInfo;

import de.saxsys.mongodbws.geoservice.rest.Constants;
import de.saxsys.mongodbws.geoservice.service.GeoDataService;

/**
 * 
 * @author Andreas Post
 */
@Path(Constants.POI_RESOURCE_PATH)
public class PointOfInterestResourceController {

	private static final String EXPAND_DETAILS = "details";

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
	public Response getPOI(@PathParam("id") String id, @QueryParam("expand") String expand) {

		PointOfInterest poi = geoDataService.getPOI(id, EXPAND_DETAILS.equalsIgnoreCase(expand));

		if (poi == null) {
			throw new NotFoundException();
		}

		poi.setHref(createUriString(poi));

		return Response.ok(poi).header(Constants.CONTENT_ENC_KEY, Constants.CHARSET_UTF8).build();
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

		geoDataService.deletePOI(id);

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
			@QueryParam("radius") int radius, @QueryParam("expand") String expand) {

		List<PointOfInterest> poiList = geoDataService.listPOIs(latitude, longitude, radius,
				EXPAND_DETAILS.equalsIgnoreCase(expand));

		for (PointOfInterest poi : poiList) {
			poi.setHref(createUriString(poi));
		}

		return Response.ok(poiList).header(Constants.CONTENT_ENC_KEY, Constants.CHARSET_UTF8).build();
	}

	/**
	 * Create the URI of the poi as string.
	 * 
	 * @param poi
	 * @return
	 */
	private String createUriString(PointOfInterest poi) {
		return uriInfo.getBaseUri().toString() + Constants.POI_RESOURCE_PATH + poi.getId();
	}
}
