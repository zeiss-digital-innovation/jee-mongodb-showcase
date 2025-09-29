/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * <p>
 * Copyright (C) 2025 Carl Zeiss Digital Innovation GmbH
 */
package de.zeiss.mongodbws.geoservice.rest.resource;

import de.zeiss.mongodbws.geoservice.service.GeoDataService;
import jakarta.ws.rs.NotFoundException;
import jakarta.ws.rs.core.Response;
import jakarta.ws.rs.core.UriInfo;
import org.geojson.Point;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import java.net.URI;
import java.util.Arrays;
import java.util.List;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.Mockito.*;

/**
 * Unit tests for {@link PointOfInterestResourceController}
 *
 * @author Generated Tests
 */
@ExtendWith(MockitoExtension.class)
public class PointOfInterestResourceControllerTest {

    @Mock
    private GeoDataService geoDataService;

    @Mock
    private UriInfo uriInfo;

    @InjectMocks
    private PointOfInterestResourceController controller;

    private PointOfInterest testPoi;
    private String testId;

    @BeforeEach
    public void setUp() throws Exception {
        testId = "507f1f77bcf86cd799439011";

        testPoi = new PointOfInterest();
        testPoi.setId(testId);
        testPoi.setCategory("restaurant");
        testPoi.setDetails("Test restaurant");
        testPoi.setLocation(new Point(13.7373, 51.0504));

        // Mock UriInfo
        URI baseUri = new URI("http://localhost:8080/api/");
        lenient().when(uriInfo.getBaseUri()).thenReturn(baseUri);
    }

    @Test
    public void testGetPOI_ValidIdWithoutExpand_ShouldReturnPOI() {
        // Given
        when(geoDataService.getPOI(testId, false)).thenReturn(testPoi);

        // When
        Response response = controller.getPOI(testId, null);

        // Then
        assertEquals(Response.Status.OK.getStatusCode(), response.getStatus());
        assertNotNull(response.getEntity());
        assertInstanceOf(PointOfInterest.class, response.getEntity());

        PointOfInterest returnedPoi = (PointOfInterest) response.getEntity();
        assertEquals(testId, returnedPoi.getId());
        assertEquals("restaurant", returnedPoi.getCategory());
        assertNotNull(returnedPoi.getHref());
        assertTrue(returnedPoi.getHref().contains(testId));

        verify(geoDataService).getPOI(testId, false);
    }

    @Test
    public void testGetPOI_ValidIdWithExpandDetails_ShouldReturnPOIWithDetails() {
        // Given
        when(geoDataService.getPOI(testId, true)).thenReturn(testPoi);

        // When
        Response response = controller.getPOI(testId, "details");

        // Then
        assertEquals(Response.Status.OK.getStatusCode(), response.getStatus());
        assertNotNull(response.getEntity());
        verify(geoDataService).getPOI(testId, true);
    }

    @Test
    public void testGetPOI_ValidIdWithExpandDetailsIgnoreCase_ShouldReturnPOIWithDetails() {
        // Given
        when(geoDataService.getPOI(testId, true)).thenReturn(testPoi);

        // When
        Response response = controller.getPOI(testId, "DETAILS");

        // Then
        assertEquals(Response.Status.OK.getStatusCode(), response.getStatus());
        verify(geoDataService).getPOI(testId, true);
    }

    @Test
    public void testGetPOI_NonExistentId_ShouldThrowNotFoundException() {
        // Given
        when(geoDataService.getPOI(testId, false)).thenReturn(null);

        assertThrows(NotFoundException.class, () -> controller.getPOI(testId, null));
    }

    @Test
    public void testCreatePOI_ValidPOI_ShouldReturnCreatedResponse() {
        // Given
        PointOfInterest inputPoi = new PointOfInterest();
        inputPoi.setCategory("pharmacy");
        inputPoi.setDetails("New pharmacy");
        inputPoi.setLocation(new Point(13.7373, 51.0504));

        PointOfInterest createdPoi = new PointOfInterest();
        createdPoi.setId(testId);
        createdPoi.setCategory("pharmacy");
        createdPoi.setDetails("New pharmacy");
        createdPoi.setLocation(new Point(13.7373, 51.0504));

        when(geoDataService.createPOI(inputPoi)).thenReturn(createdPoi);

        // When
        Response response = controller.createPOI(inputPoi);

        // Then
        assertEquals(Response.Status.CREATED.getStatusCode(), response.getStatus());
        assertNotNull(response.getLocation());
        assertTrue(response.getLocation().toString().contains(testId));
        verify(geoDataService).createPOI(inputPoi);
    }

    @Test
    public void testDeletePOI_ValidId_ShouldReturnNoContentResponse() {
        // When
        Response response = controller.deletePOI(testId);

        // Then
        assertEquals(Response.Status.NO_CONTENT.getStatusCode(), response.getStatus());
        verify(geoDataService).deletePOI(testId);
    }

    @Test
    public void testListPOIs_ValidParametersWithoutExpand_ShouldReturnPOIList() {
        // Given
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 1000;

        PointOfInterest poi2 = new PointOfInterest();
        poi2.setId("507f1f77bcf86cd799439012");
        poi2.setCategory("pharmacy");
        poi2.setDetails("Local pharmacy");
        poi2.setLocation(new Point(13.7400, 51.0600));

        List<PointOfInterest> poiList = Arrays.asList(testPoi, poi2);
        when(geoDataService.listPOIs(lat, lon, radius, false)).thenReturn(poiList);

        // When
        Response response = controller.listPOIs(lat, lon, radius, null);

        // Then
        assertEquals(Response.Status.OK.getStatusCode(), response.getStatus());
        assertNotNull(response.getEntity());
        assertTrue(response.getEntity() instanceof List);

        @SuppressWarnings("unchecked")
        List<PointOfInterest> returnedList = (List<PointOfInterest>) response.getEntity();
        assertEquals(2, returnedList.size());

        // Check that href is set for all POIs
        for (PointOfInterest poi : returnedList) {
            assertNotNull(poi.getHref());
            assertTrue(poi.getHref().contains(poi.getId()));
        }

        verify(geoDataService).listPOIs(lat, lon, radius, false);
    }

    @Test
    public void testListPOIs_ValidParametersWithExpandDetails_ShouldReturnPOIListWithDetails() {
        // Given
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 500;

        List<PointOfInterest> poiList = Arrays.asList(testPoi);
        when(geoDataService.listPOIs(lat, lon, radius, true)).thenReturn(poiList);

        // When
        Response response = controller.listPOIs(lat, lon, radius, "details");

        // Then
        assertEquals(Response.Status.OK.getStatusCode(), response.getStatus());
        verify(geoDataService).listPOIs(lat, lon, radius, true);
    }

    @Test
    public void testListPOIs_EmptyResult_ShouldReturnEmptyList() {
        // Given
        double lat = 51.0504;
        double lon = 13.7373;
        int radius = 100;

        when(geoDataService.listPOIs(lat, lon, radius, false)).thenReturn(Arrays.asList());

        // When
        Response response = controller.listPOIs(lat, lon, radius, null);

        // Then
        assertEquals(Response.Status.OK.getStatusCode(), response.getStatus());
        assertNotNull(response.getEntity());
        assertTrue(response.getEntity() instanceof List);

        @SuppressWarnings("unchecked")
        List<PointOfInterest> returnedList = (List<PointOfInterest>) response.getEntity();
        assertTrue(returnedList.isEmpty());

        verify(geoDataService).listPOIs(lat, lon, radius, false);
    }
}
