package de.zeiss.mongodb_ws.spring_geo_service.service;

import de.zeiss.mongodb_ws.spring_geo_service.persistence.IPointOfInterestRepository;
import de.zeiss.mongodb_ws.spring_geo_service.persistence.entity.PointOfInterestEntity;
import de.zeiss.mongodb_ws.spring_geo_service.rest.model.PointOfInterest;
import org.geojson.Point;
import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.*;
import org.springframework.data.geo.Distance;
import org.springframework.data.mongodb.core.geo.GeoJsonPoint;

import java.util.ArrayList;
import java.util.List;
import java.util.Optional;
import java.util.logging.Logger;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.Mockito.*;

/**
 * Unit tests for {@link PointOfInterestService} using a mocked repository.
 */
public class PointOfInterestServiceTest {

    private static final Logger LOGGER = Logger.getLogger(PointOfInterestServiceTest.class.getName());

    @Mock
    private IPointOfInterestRepository poiRepository;

    @InjectMocks
    private PointOfInterestService poiService;

    @Captor
    private ArgumentCaptor<PointOfInterestEntity> entityCaptor;

    private AutoCloseable openedMocks;

    @BeforeEach
    void setUp() {
        openedMocks = MockitoAnnotations.openMocks(this);
    }

    @AfterEach
    void tearDown() {
        try {
            openedMocks.close();
        } catch (Exception e) {
            LOGGER.severe("Failed to close mocks: " + e.getMessage());
        }
    }

    private PointOfInterestEntity sampleEntity(String id, String name, String category, double lon, double lat, String details) {
        PointOfInterestEntity e = new PointOfInterestEntity();
        e.setId(id);
        e.setName(name);
        e.setCategory(category);
        e.setLocation(new GeoJsonPoint(lon, lat));
        e.setDetails(details);
        return e;
    }

    private PointOfInterest sampleModel(String name, String category, double lon, double lat, String details) {
        PointOfInterest p = new PointOfInterest();
        p.setName(name);
        p.setCategory(category);
        p.setLocation(new Point(lon, lat));
        p.setDetails(details);
        return p;
    }

    @Test
    void getPointOfInterestById_Found_ShouldReturnResource() {
        PointOfInterestEntity e = sampleEntity("id1", "Name1", "cat", 13.4, 52.5, "d");
        when(poiRepository.findById("id1")).thenReturn(Optional.of(e));

        PointOfInterest res = poiService.getPointOfInterestById("id1");

        assertNotNull(res);
        assertPointOfInterestEquals(res, "Name1", "cat", 13.4, 52.5, "d");

        verify(poiRepository).findById("id1");
    }

    @Test
    void getPointOfInterestById_NotFound_ShouldReturnNull() {
        when(poiRepository.findById("nope")).thenReturn(Optional.empty());

        PointOfInterest res = poiService.getPointOfInterestById("nope");

        assertNull(res);
        verify(poiRepository).findById("nope");
    }

    @Test
    void listPOIs_ShouldCallRepositoryAndMapResults_AndStripDetailsWhenNotExpanded() {
        List<PointOfInterestEntity> entities = new ArrayList<>();
        entities.add(sampleEntity("id1", "A", "cat", 13.0, 52.0, "details-A"));
        entities.add(sampleEntity("id2", "B", "cat", 13.1, 52.1, "details-B"));

        when(poiRepository.findByLocationNear(any(org.springframework.data.geo.Point.class), any(Distance.class)))
                .thenReturn(entities);

        List<PointOfInterest> results = poiService.listPOIs(52.0, 13.0, 1000, false);

        assertNotNull(results);
        assertEquals(2, results.size());
        // details should be stripped
        assertNull(results.get(0).getDetails());
        assertNull(results.get(1).getDetails());

        verify(poiRepository).findByLocationNear(any(org.springframework.data.geo.Point.class), any(Distance.class));
    }

    @Test
    void listPOIs_ShouldCallRepositoryAndMapResults_AndReturnDetailsWhenExpanded() {
        List<PointOfInterestEntity> entities = new ArrayList<>();
        entities.add(sampleEntity("id1", "A", "cat", 13.0, 52.0, "details-A"));
        entities.add(sampleEntity("id2", "B", "cat", 13.1, 52.1, "details-B"));

        when(poiRepository.findByLocationNear(any(org.springframework.data.geo.Point.class), any(Distance.class)))
                .thenReturn(entities);

        List<PointOfInterest> results = poiService.listPOIs(52.0, 13.0, 1000, true);

        assertNotNull(results);
        assertEquals(2, results.size());
        // details should be stripped
        assertNotNull(results.get(0).getDetails());
        assertNotNull(results.get(1).getDetails());

        verify(poiRepository).findByLocationNear(any(org.springframework.data.geo.Point.class), any(Distance.class));
    }

    @Test
    void createPOI_ShouldSaveAndReturnResource() {
        PointOfInterest input = sampleModel("New POI", "cat", 13.2, 52.2, "dd");
        PointOfInterestEntity saved = sampleEntity("generated-id", "New POI", "cat", 13.2, 52.2, "dd");

        when(poiRepository.save(any(PointOfInterestEntity.class))).thenReturn(saved);

        PointOfInterest res = poiService.createPOI(input);

        assertNotNull(res);
        assertEquals("New POI", res.getName());
        assertEquals("generated-id", res.getId());

        verify(poiRepository).save(entityCaptor.capture());
        PointOfInterestEntity captured = entityCaptor.getValue();
        assertNotNull(captured);

        assertPointOfInterestEquals(input, captured.getName(), captured.getCategory(), captured.getLocation().getX(), captured.getLocation().getY(), captured.getDetails());
    }

    @Test
    void deletePOI_ShouldCallRepositoryDelete() {
        doNothing().when(poiRepository).deleteById("id-to-delete");

        poiService.deletePOI("id-to-delete");

        verify(poiRepository).deleteById("id-to-delete");
    }

    @Test
    void updatePOI_Existing_ShouldUpdateAndReturnResource() {
        PointOfInterestEntity existing = sampleEntity("id-ex", "Old", "cat-ex", 13.0, 52.0, "old-details");
        when(poiRepository.findById("id-ex")).thenReturn(Optional.of(existing));
        when(poiRepository.save(any(PointOfInterestEntity.class))).thenAnswer(invocation -> invocation.getArgument(0));

        PointOfInterest toUpdate = new PointOfInterest();
        toUpdate.setId("id-ex");
        toUpdate.setName("NewName");
        toUpdate.setCategory("cat");
        toUpdate.setLocation(new Point(13.0, 52.0));

        PointOfInterest res = poiService.updatePOI(toUpdate);

        assertNotNull(res);
        assertEquals("NewName", res.getName());
        verify(poiRepository).findById("id-ex");
        verify(poiRepository).save(any(PointOfInterestEntity.class));

        assertPointOfInterestEquals(res, "NewName", "cat", 13.0, 52.0, null);
    }

    @Test
    void updatePOI_NonExisting_ShouldReturnNull() {
        when(poiRepository.findById("not-ex")).thenReturn(Optional.empty());

        PointOfInterest toUpdate = new PointOfInterest();
        toUpdate.setId("not-ex");
        toUpdate.setName("NewName");

        PointOfInterest res = poiService.updatePOI(toUpdate);

        assertNull(res);
        verify(poiRepository).findById("not-ex");
    }

    @Test
    void updatePOI_NullId_ShouldThrow() {
        PointOfInterest toUpdate = new PointOfInterest();
        toUpdate.setName("Name");

        assertThrows(IllegalArgumentException.class, () -> poiService.updatePOI(toUpdate));
    }

    private static void assertPointOfInterestEquals(PointOfInterest poi,
                                                    String expectedName,
                                                    String expectedCategory,
                                                    double expectedLon,
                                                    double expectedLat,
                                                    String expectedDetails) {
        assertNotNull(poi, "PointOfInterest must not be null");
        assertEquals(expectedName, poi.getName(), "name");
        assertEquals(expectedCategory, poi.getCategory(), "category");
        assertNotNull(poi.getLocation(), "location must not be null");
        assertEquals(expectedLon, poi.getLocation().getCoordinates().getLongitude(), 1e-9, "longitude");
        assertEquals(expectedLat, poi.getLocation().getCoordinates().getLatitude(), 1e-9, "latitude");
        assertEquals(expectedDetails, poi.getDetails(), "details");
    }

}

