package de.zeiss.mongodb_ws.spring_geo_service.rest.controller;

import com.fasterxml.jackson.databind.ObjectMapper;
import de.zeiss.mongodb_ws.spring_geo_service.rest.model.PointOfInterest;
import de.zeiss.mongodb_ws.spring_geo_service.service.PointOfInterestService;
import org.geojson.Point;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.WebMvcTest;
import org.springframework.http.MediaType;
import org.springframework.test.context.bean.override.mockito.MockitoBean;
import org.springframework.test.web.servlet.MockMvc;

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
}
