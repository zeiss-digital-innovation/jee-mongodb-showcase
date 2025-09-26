package de.zeiss.mongodbws.testdatageneration;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.topografix.gpx.GpxType;
import com.topografix.gpx.WptType;
import de.zeiss.mongodbws.testdatageneration.model.PointOfInterest;
import de.zeiss.mongodbws.testdatageneration.model.PointOfInterestFactory;
import jakarta.xml.bind.JAXBContext;
import jakarta.xml.bind.JAXBElement;
import jakarta.xml.bind.Unmarshaller;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.util.Arrays;
import java.util.List;
import java.util.logging.Level;
import java.util.logging.Logger;

public class Main {

    private final ObjectMapper JSON_MAPPER = new ObjectMapper();

    private static final HttpClient client = HttpClient.newHttpClient();
    private static final String POI_SERVICE_URL = Config.getPoiServiceUrl();

    private static final Logger LOG = Logger.getLogger(Main.class.getName());

    public static void main(String[] args) {

        LOG.info("Starting POI data generation and upload to: " + POI_SERVICE_URL);

        Main main = new Main();

        main.processFolder("company");
    }

    /**
     * Processes all GPX files in the specified folder.
     *
     * @param folderName the name of the folder containing GPX files
     */
    private void processFolder(String folderName) {
        LOG.info("Processing files in: " + folderName);
        ClassLoader classLoader = ClassLoader.getSystemClassLoader();

        try {
            File folder = new File(classLoader.getResource(folderName).toURI());
            List<File> files = Arrays.asList(folder.listFiles((f, name) -> name.toLowerCase().endsWith(".gpx")));

            files.forEach(f -> processFile(f, folderName));
        } catch (Exception e) {
            throw new IllegalStateException("Error accessing directory: " + folderName, e);
        }
    }

    /**
     * Processes a single GPX file, extracting waypoints and posting them to the POI service.
     *
     * @param file       the GPX file to process
     * @param folderName the category name derived from the folder
     */
    private void processFile(File file, String folderName) {
        LOG.info("Processing file: " + file.getAbsolutePath() + " in category: " + folderName);

        try {
            JAXBContext jaxbContext = JAXBContext.newInstance(GpxType.class);
            Unmarshaller unmarshaller = jaxbContext.createUnmarshaller();
            JAXBElement<GpxType> root = unmarshaller.unmarshal(new StreamSource(file), GpxType.class);
            GpxType gpx = root.getValue();

            LOG.info("Found " + gpx.getWpt().size() + " waypoints in file: " + file.getName());

            gpx.getWpt().forEach(wpt -> postWaypoint(wpt, folderName));
        } catch (Exception e) {
            LOG.log(Level.WARNING, "Error processing file: " + file.getAbsolutePath(), e);
        }
    }

    /**
     * Posts a waypoint as a PointOfInterest to the POI service.
     *
     * @param waypoint the waypoint to post
     * @param category the category of the waypoint
     */
    private void postWaypoint(WptType waypoint, String category) {
        PointOfInterest poi = PointOfInterestFactory.createPointOfInterest(waypoint.getLat(), waypoint.getLon(), category, waypoint.getName());

        String json = "";

        try {
            json = JSON_MAPPER.writeValueAsString(poi);
        } catch (JsonProcessingException e) {
            LOG.warning("Error converting Waypoint to JSON: " + e.getMessage());
            return;
        }
        try {
            HttpRequest request = HttpRequest.newBuilder()
                    .uri(URI.create(POI_SERVICE_URL))
                    .header("Content-Type", "application/json")
                    .POST(HttpRequest.BodyPublishers.ofString(json))
                    .build();
            HttpResponse<String> response = client.send(request, HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() != 201) {
                LOG.warning("Error posting POI: " + response.statusCode() + " - " + response.body());
                throw new IllegalStateException("Error posting POI: " + response.statusCode() + " - " + response.body());
            }

            LOG.fine("Response: " + response.statusCode() + " - " + response.body());
        } catch (Exception e) {
            LOG.warning("Error posting POI: " + e.getMessage());
            throw new IllegalStateException("Error posting POI: " + e.getMessage(), e);
        }
    }
}