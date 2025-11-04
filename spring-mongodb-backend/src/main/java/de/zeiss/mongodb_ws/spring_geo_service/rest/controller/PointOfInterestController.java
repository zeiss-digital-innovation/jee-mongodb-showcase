package de.zeiss.mongodb_ws.spring_geo_service.rest.controller;

import de.zeiss.mongodb_ws.spring_geo_service.rest.model.PointOfInterest;
import de.zeiss.mongodb_ws.spring_geo_service.service.PointOfInterestService;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.media.ArraySchema;
import io.swagger.v3.oas.annotations.media.Content;
import io.swagger.v3.oas.annotations.media.Schema;
import io.swagger.v3.oas.annotations.responses.ApiResponse;
import io.swagger.v3.oas.annotations.responses.ApiResponses;
import io.swagger.v3.oas.annotations.tags.Tag;
import jakarta.validation.Valid;
import jakarta.validation.constraints.Max;
import jakarta.validation.constraints.Min;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.validation.annotation.Validated;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.server.ResponseStatusException;
import org.springframework.web.servlet.support.ServletUriComponentsBuilder;

import java.net.URI;
import java.util.Collection;
import java.util.List;
import java.util.logging.Logger;

@Tag(name = "Points of Interest", description = "API for managing Points of Interest (POIs)")
@RestController
@Validated
@RequestMapping(value = "/api/poi")
public class PointOfInterestController {

    @Autowired
    private PointOfInterestService poiService;

    private static final Logger logger = Logger.getLogger(PointOfInterestController.class.getName());

    @Operation(summary = "Find a Point of Interest by its id")
    @ApiResponses(value = {
            @ApiResponse(responseCode = "200", description = "Found the POI",
                    content = {@Content(mediaType = "application/json",
                            schema = @Schema(implementation = PointOfInterest.class))}),
            @ApiResponse(responseCode = "400", description = "Invalid id supplied",
                    content = @Content),
            @ApiResponse(responseCode = "404", description = "POI not found",
                    content = @Content)})
    @GetMapping(value = "/{id}", produces = MediaType.APPLICATION_JSON_VALUE)
    public PointOfInterest getPointOfInterest(@PathVariable("id") String id) {
        logger.info("Received request for POI with id: " + id);
        PointOfInterest poi = poiService.getPointOfInterestById(id);

        if (poi == null) {
            throw new ResponseStatusException(HttpStatus.NOT_FOUND, "Point of Interest with id " + id + " not found.");
        }

        poi.setHref(ServletUriComponentsBuilder.fromCurrentRequestUri().toUriString());
        return poi;
    }

    @Operation(summary = "Searches for Points of Interest within a given radius around specified coordinates")
    @ApiResponses(value = {
            @ApiResponse(responseCode = "200", description = "POIs found",
                    content = {@Content(mediaType = "application/json",
                            array = @ArraySchema(schema = @Schema(implementation = PointOfInterest.class)))}),
            @ApiResponse(responseCode = "400", description = "Invalid search parameters",
                    content = @Content)})
    @GetMapping
    public Collection<PointOfInterest> findPointsOfInterest(@Min(-90) @Max(90) @RequestParam double lat, @Min(-180) @Max(180) @RequestParam double lon,
                                                            @Min(1) @Max(100000) @RequestParam int radius, @RequestParam(value = "expand", required = false) String expand) {

        List<PointOfInterest> poiList = poiService.listPOIs(lat, lon, radius, "details".equalsIgnoreCase(expand));

        for (PointOfInterest poi : poiList) {
            String href = ServletUriComponentsBuilder.fromCurrentRequest()
                    .path("/{id}")
                    .buildAndExpand(poi.getId()).toUriString();

            poi.setHref(href);
        }

        return poiList;
    }

    @Operation(summary = "Creates a new Point of Interest")
    @ApiResponses(value = {
            @ApiResponse(responseCode = "201", description = "POI created successfully",
                    content = @Content),
            @ApiResponse(responseCode = "400", description = "Invalid POI data supplied - see the response body for details",
                    content = @Content)})
    @PostMapping
    @ResponseStatus(HttpStatus.CREATED)
    public ResponseEntity<Void> create(@Valid @RequestBody PointOfInterest resource) {

        PointOfInterest resultPoi = poiService.createPOI(resource);

        // set the Location header
        URI location = ServletUriComponentsBuilder.fromCurrentRequest()
                .path("/{id}")
                .buildAndExpand(resultPoi.getId())
                .toUri();

        logger.info("Location header for created POI: " + location);
        return ResponseEntity.created(location).build();
    }

    @Operation(summary = "Updates a Point of Interest by its id")
    @ApiResponses(value = {
            @ApiResponse(responseCode = "201", description = "New Point of interest created (if not existing for given ID)",
                    content = @Content),
            @ApiResponse(responseCode = "204", description = "Point of interest updated",
                    content = @Content),
            @ApiResponse(responseCode = "400", description = "Invalid POI data supplied - see the response body for details",
                    content = @Content)})
    @PutMapping(value = "/{id}")
    @ResponseStatus(HttpStatus.NO_CONTENT)
    public ResponseEntity<Void> update(@PathVariable("id") String id, @Valid @RequestBody PointOfInterest resource) {
        PointOfInterest resultPoi = null;
        boolean isNew = false;

        resultPoi = poiService.getPointOfInterestById(id);

        if (resultPoi == null) {
            isNew = true;
            resource.setId(id);
            resultPoi = poiService.createPOI(resource);
        } else {
            resource.setId(id);
            resultPoi = poiService.updatePOI(resource);
        }

        if (isNew) {
            // set the Location header
            URI location = ServletUriComponentsBuilder.fromCurrentRequest().build().toUri();
            return ResponseEntity.created(location).build();
        } else {
            return ResponseEntity.noContent().build();
        }
    }

    @Operation(summary = "Deletes a Point of Interest by its id")
    @ApiResponses(value = {
            @ApiResponse(responseCode = "204", description = "POI deleted successfully",
                    content = @Content),
            @ApiResponse(responseCode = "404", description = "POI not found",
                    content = @Content)})
    @DeleteMapping(value = "/{id}")
    @ResponseStatus(HttpStatus.NO_CONTENT)
    public ResponseEntity<Void> delete(@PathVariable("id") String id) {
        if (poiService.getPointOfInterestById(id) == null) {
            throw new ResponseStatusException(HttpStatus.NOT_FOUND, "Point of Interest with id " + id + " not found.");
        }
        poiService.deletePOI(id);

        return ResponseEntity.noContent().build();
    }
}
