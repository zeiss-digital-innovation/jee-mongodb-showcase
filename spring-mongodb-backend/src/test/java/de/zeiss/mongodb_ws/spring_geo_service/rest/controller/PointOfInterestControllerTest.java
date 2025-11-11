package de.zeiss.mongodb_ws.spring_geo_service.rest.controller;

import com.fasterxml.jackson.databind.ObjectMapper;
import de.zeiss.mongodb_ws.spring_geo_service.rest.model.PointOfInterest;
import de.zeiss.mongodb_ws.spring_geo_service.service.PointOfInterestService;
import org.geojson.Point;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.Arguments;
import org.junit.jupiter.params.provider.MethodSource;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.WebMvcTest;
import org.springframework.http.MediaType;
import org.springframework.test.context.bean.override.mockito.MockitoBean;
import org.springframework.test.web.servlet.MockMvc;

import java.util.stream.Stream;

import static org.mockito.Mockito.*;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.put;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.*;

@WebMvcTest(PointOfInterestController.class)
public class PointOfInterestControllerTest {

    @Autowired
    private MockMvc mockMvc;

    @MockitoBean
    private PointOfInterestService poiService;

    @Autowired
    private ObjectMapper objectMapper;

    static Stream<Arguments> validFindParametersProvider() {
        return Stream.of(
                Arguments.of(12.34, 56.78, 1000, "somewhere valid"),
                Arguments.of(-12.34, -56.78, 5000, "negative valid scenario"),
                Arguments.of(90.0, 56.78, 1000, "positive edge case latitude"),
                Arguments.of(-90.0, -56.78, 1000, "negative edge case latitude"),
                Arguments.of(12.34, 180.0, 1000, "positive edge case  longitude"),
                Arguments.of(-12.34, -180.0, 1000, "negative edge case longitude"),
                Arguments.of(12.34, 56.78, 1, "lower edge case radius"),
                Arguments.of(12.34, 56.78, 100000, "upper edge case radius")
        );
    }

    static Stream<Arguments> invalidFindParametersProvider() {
        return Stream.of(
                Arguments.of(-90.1, -180.0, 1000, "invalid lower bound latitude"),
                Arguments.of(90.1, 180.0, 1000, "invalid upper bound latitude"),
                Arguments.of(-90.0, -180.1, 1000, "invalid lower bound longitude"),
                Arguments.of(90.0, -180.1, 1000, "invalid lower bound longitude"),
                Arguments.of(90.0, 180.0, -1, "invalid negative radius"),
                Arguments.of(90.0, 180.0, 0, "invalid lower bound radius"),
                Arguments.of(90.0, 180.0, 100001, "invalid upper bound radius")
        );
    }

    @Test
    public void testGetPointOfInterest_KnownId_ShouldReturnOk() throws Exception {
        String knownId = "known-id";

        when(poiService.getPointOfInterestById(knownId)).thenReturn(new PointOfInterest());

        mockMvc.perform(org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get("/api/poi/{id}", knownId)
                        .accept(MediaType.APPLICATION_JSON))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.href").exists())
                .andExpect(jsonPath("$.href").value("http://localhost/api/poi/" + knownId));
    }

    @Test
    public void testGetPointOfInterest_UnknownId_ShouldReturnNotFound() throws Exception {
        String unknownId = "unknown-id";

        when(poiService.getPointOfInterestById(unknownId)).thenReturn(null);

        mockMvc.perform(org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get("/api/poi/{id}", unknownId)
                        .accept(MediaType.APPLICATION_JSON))
                .andExpect(status().isNotFound());
    }

    @ParameterizedTest(name = "Valid parameters #{index}: lon={0}, lat={1}, radius={2}, description={3}")
    @MethodSource("validFindParametersProvider")
    public void testFindPointsOfInterest_NearLocation_ShouldReturnOk(double lat, double lon, int radius, String description) throws Exception {
        mockMvc.perform(org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get("/api/poi")
                        .param("lat", "" + lat)
                        .param("lon", "" + lon)
                        .param("radius", "" + radius)
                        .param("expand", "details")
                        .accept(MediaType.APPLICATION_JSON))
                .andExpect(status().isOk());
    }

    @ParameterizedTest(name = "Invalid parameters #{index}: lon={0}, lat={1}, radius={2}, description={3}")
    @MethodSource("invalidFindParametersProvider")
    public void testFindPointsOfInterest_InvalidParameters_ShouldReturnBadRequest(double lat, double lon, int radius, String description) throws Exception {
        mockMvc.perform(org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get("/api/poi")
                        .param("lat", "" + lat)
                        .param("lon", "" + lon)
                        .param("radius", "" + radius)
                        .accept(MediaType.APPLICATION_JSON))
                .andExpect(status().isBadRequest());
    }

    @Test
    public void testCreatePointOfInterest_ValidInput_ShouldReturnCreated() throws Exception {
        PointOfInterest poi = new PointOfInterest();
        poi.setName("Test POI");
        poi.setCategory("Test Category");
        Point location = new Point(12.34, 56.78);
        poi.setLocation(location);

        when(poiService.createPOI(any(PointOfInterest.class))).thenReturn(poi);

        mockMvc.perform(post("/api/poi")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(poi)))
                .andExpect(status().isCreated())
                .andExpect(header().exists("Location")); // Check if Location header is set

        verify(poiService, times(1)).createPOI(any(PointOfInterest.class));
    }

    @Test
    public void testCreatePointOfInterest_MissingNameAndLocation_ShouldReturnBadRequest() throws Exception {
        PointOfInterest poi = new PointOfInterest();
        poi.setCategory("Test Category"); // Missing name and location

        mockMvc.perform(post("/api/poi")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(poi)))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.name").exists())
                .andExpect(jsonPath("$.location").exists()); // Check if validation result includes "name" attribute

        verify(poiService, never()).createPOI(any(PointOfInterest.class));
    }

    @Test
    public void testCreatePointOfInterest_MissingCategory_ShouldReturnBadRequest() throws Exception {
        PointOfInterest poi = new PointOfInterest();
        poi.setName("Test Name"); // Missing category
        poi.setLocation(new Point(12.34, 56.78));

        mockMvc.perform(post("/api/poi")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(poi)))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.category").exists()); // Check if validation result includes "name" attribute

        verify(poiService, never()).createPOI(any(PointOfInterest.class));
    }

    @Test
    public void testCreatePointOfInterest_BadLocation_ShouldReturnBadRequest() throws Exception {
        PointOfInterest poi = new PointOfInterest();
        poi.setName("Test Name");
        poi.setCategory("Test Category");
        poi.setLocation(new Point(181, 56.78)); // Invalid longitude

        mockMvc.perform(post("/api/poi")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(poi)))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.location").exists()); // Check if validation result includes "location" attribute

        verify(poiService, never()).createPOI(any(PointOfInterest.class));

        poi.setLocation(new Point(-181, 56.78)); // Invalid longitude

        mockMvc.perform(post("/api/poi")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(poi)))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.location").exists()); // Check if validation result includes "location" attribute

        verify(poiService, never()).createPOI(any(PointOfInterest.class));

        poi.setLocation(new Point(12.34, 91)); // Invalid latitude

        mockMvc.perform(post("/api/poi")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(poi)))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.location").exists()); // Check if validation result includes "location" attribute

        verify(poiService, never()).createPOI(any(PointOfInterest.class));

        poi.setLocation(new Point(12.34, -91)); // Invalid latitude

        mockMvc.perform(post("/api/poi")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(poi)))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.location").exists()); // Check if validation result includes "location" attribute

        verify(poiService, never()).createPOI(any(PointOfInterest.class));
    }

    @Test
    public void testUpdateExistingPointOfInterest_ValidInput_ShouldReturnNoContent() throws Exception {
        PointOfInterest poi = new PointOfInterest();
        poi.setName("Test POI");
        poi.setCategory("Test Category");
        Point location = new Point(12.34, 56.78);
        poi.setLocation(location);

        // Simulate that the POI does not exist yet
        when(poiService.getPointOfInterestById("123")).thenReturn(poi);
        when(poiService.updatePOI(poi)).thenReturn(poi);

        mockMvc.perform(put("/api/poi/123")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(poi)))
                .andExpect(status().isNoContent()); // Check if Location header is set

        verify(poiService, times(1)).updatePOI(any(PointOfInterest.class));
    }

    @Test
    public void testUpdateNewPointOfInterest_ValidInput_ShouldReturnCreated() throws Exception {
        PointOfInterest poi = new PointOfInterest();
        poi.setName("Test POI");
        poi.setCategory("Test Category");
        Point location = new Point(12.34, 56.78);
        poi.setLocation(location);

        // Simulate that the POI does not exist yet
        when(poiService.getPointOfInterestById("123")).thenReturn(null);
        when(poiService.createPOI(any(PointOfInterest.class))).thenReturn(poi);

        mockMvc.perform(put("/api/poi/123")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(poi)))
                .andExpect(status().isCreated())
                .andExpect(header().exists("Location")); // Check if Location header is set

        verify(poiService, times(1)).createPOI(any(PointOfInterest.class));
    }

    @Test
    public void testUpdatePointOfInterest_MissingNameAndLocation_ShouldReturnBadRequest() throws Exception {
        PointOfInterest poi = new PointOfInterest();
        poi.setCategory("Test Category"); // Missing name and location

        mockMvc.perform(put("/api/poi/123")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(poi)))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.name").exists())
                .andExpect(jsonPath("$.location").exists()); // Check if validation result includes "name" attribute

        verify(poiService, never()).createPOI(any(PointOfInterest.class));
    }

    @Test
    public void testUpdatePointOfInterest_MissingCategory_ShouldReturnBadRequest() throws Exception {
        PointOfInterest poi = new PointOfInterest();
        poi.setName("Test Name"); // Missing category
        poi.setLocation(new Point(12.34, 56.78));

        mockMvc.perform(put("/api/poi/123")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(poi)))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.category").exists()); // Check if validation result includes "name" attribute

        verify(poiService, never()).createPOI(any(PointOfInterest.class));
    }

    @Test
    public void testUpdatePointOfInterest_BadLocation_ShouldReturnBadRequest() throws Exception {
        PointOfInterest poi = new PointOfInterest();
        poi.setName("Test Name");
        poi.setCategory("Test Category");
        poi.setLocation(new Point(181, 56.78)); // Invalid longitude

        mockMvc.perform(put("/api/poi/123")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(poi)))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.location").exists()); // Check if validation result includes "location" attribute

        verify(poiService, never()).createPOI(any(PointOfInterest.class));

        poi.setLocation(new Point(-181, 56.78)); // Invalid longitude

        mockMvc.perform(post("/api/poi")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(poi)))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.location").exists()); // Check if validation result includes "location" attribute

        verify(poiService, never()).createPOI(any(PointOfInterest.class));

        poi.setLocation(new Point(12.34, 91)); // Invalid latitude

        mockMvc.perform(post("/api/poi")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(poi)))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.location").exists()); // Check if validation result includes "location" attribute

        verify(poiService, never()).createPOI(any(PointOfInterest.class));

        poi.setLocation(new Point(12.34, -91)); // Invalid latitude

        mockMvc.perform(post("/api/poi")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(poi)))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.location").exists()); // Check if validation result includes "location" attribute

        verify(poiService, never()).createPOI(any(PointOfInterest.class));
    }

    @Test
    public void testDeletePointOfInterest_KnownId_ShouldReturnOk() throws Exception {
        String knownId = "known-id";

        when(poiService.getPointOfInterestById(knownId)).thenReturn(new PointOfInterest());

        mockMvc.perform(org.springframework.test.web.servlet.request.MockMvcRequestBuilders.delete("/api/poi/{id}", knownId)
                        .accept(MediaType.APPLICATION_JSON))
                .andExpect(status().isNoContent());
    }

    @Test
    public void testDeletePointOfInterest_UnknownId_ShouldReturnNotFound() throws Exception {
        String unknownId = "unknown-id";

        when(poiService.getPointOfInterestById(unknownId)).thenReturn(null);

        mockMvc.perform(org.springframework.test.web.servlet.request.MockMvcRequestBuilders.delete("/api/poi/{id}", unknownId)
                        .accept(MediaType.APPLICATION_JSON))
                .andExpect(status().isNotFound());
    }
}
