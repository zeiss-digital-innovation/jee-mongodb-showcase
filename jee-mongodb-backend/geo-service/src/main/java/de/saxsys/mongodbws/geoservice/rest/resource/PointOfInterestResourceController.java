/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * 
 * Copyright (C) 2016 Saxonia Systems AG
 */
package de.saxsys.mongodbws.geoservice.rest.resource;

import java.util.List;

import javax.inject.Inject;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.Produces;
import javax.ws.rs.QueryParam;
import javax.ws.rs.core.Context;
import javax.ws.rs.core.Response;
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

	@GET
	@Produces(Constants.MEDIA_TYPE_JSON)
	public Response listPOIs(@QueryParam("lat") double lat, @QueryParam("lon") double lon,
			@QueryParam("radius") int radius) {

		List<PointOfInterest> poiList = geoDataService.listPOIs(lat, lon, radius);

		for (PointOfInterest poi : poiList) {
			poi.setHref(uriInfo.getBaseUri().toString() + Constants.POI_RESOURCE_PATH + poi.getId());
		}

		return Response.ok(poiList).header(Constants.CONTENT_ENC_KEY, Constants.CHARSET_UTF8).build();
	}
}
