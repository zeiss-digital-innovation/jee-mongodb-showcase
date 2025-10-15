/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * <p>
 * Copyright (C) 2025 Carl Zeiss Digital Innovation GmbH
 */
package de.zeiss.mongodbws.geoservice.rest.resource;

import de.zeiss.mongodbws.geoservice.rest.Constants;
import de.zeiss.mongodbws.geoservice.service.GeoDataService;
import jakarta.enterprise.context.RequestScoped;
import jakarta.inject.Inject;
import jakarta.ws.rs.*;
import jakarta.ws.rs.core.Context;
import jakarta.ws.rs.core.Response;
import jakarta.ws.rs.core.Response.Status;
import jakarta.ws.rs.core.UriInfo;
import org.eclipse.microprofile.openapi.annotations.Operation;
import org.eclipse.microprofile.openapi.annotations.media.Content;
import org.eclipse.microprofile.openapi.annotations.media.Schema;
import org.eclipse.microprofile.openapi.annotations.responses.APIResponse;
import org.eclipse.microprofile.openapi.annotations.responses.APIResponses;
import org.eclipse.microprofile.openapi.annotations.tags.Tag;

import java.net.URI;
import java.net.URISyntaxException;
import java.util.List;

/**
 * REST endpoint for POI operations.
 */
@Path("/poi")
@RequestScoped
@Tag(name = "Points of Interest", description = "Operations for managing points of interest")
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
    @Operation(summary = "Get point of interest by ID", description = "Returns a single point of interest")
    @APIResponses({
            @APIResponse(responseCode = "200", description = "Point of interest details", content = @Content(mediaType = "application/json", schema = @Schema(implementation = PointOfInterest.class))),
            @APIResponse(responseCode = "404", description = "Point of interest not found")})
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
    @Operation(summary = "Create a new point of interest", description = "Adds a new point of interest")
    @APIResponses({
            @APIResponse(responseCode = "201", description = "Point of interest created"),
            @APIResponse(responseCode = "500", description = "Internal server error")})
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
    @Operation(summary = "Delete point of interest", description = "Removes a point of interest by ID")
    @APIResponses({
            @APIResponse(responseCode = "204", description = "Point of interest deleted"),
            @APIResponse(responseCode = "404", description = "Point of interest not found")})
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
    @Operation(summary = "Get points of interest", description = "Returns a list of points of interest near a given location")
    @APIResponses({
            @APIResponse(responseCode = "200", description = "List of points of interest", content = @Content(mediaType = "application/json", schema = @Schema(implementation = PointOfInterest.class))),
            @APIResponse(responseCode = "400", description = "Invalid parameters")})
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
