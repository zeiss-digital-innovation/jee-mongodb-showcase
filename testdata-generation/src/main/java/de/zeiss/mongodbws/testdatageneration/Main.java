package de.zeiss.mongodbws.testdatageneration;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.topografix.gpx.GpxType;
import com.topografix.gpx.WptType;
import de.zeiss.mongodbws.testdatageneration.model.PointOfInterest;
import de.zeiss.mongodbws.testdatageneration.model.PointOfInterestFactory;
import jakarta.xml.bind.JAXBContext;
import jakarta.xml.bind.JAXBElement;
import jakarta.xml.bind.JAXBException;
import jakarta.xml.bind.Unmarshaller;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.util.Arrays;
import java.util.List;

public class Main {

    private final ObjectMapper JSON_MAPPER = new ObjectMapper();

    private HttpClient client = HttpClient.newHttpClient();

    public static void main(String[] args) {

        Main main = new Main();
        main.processFolder("company");
    }

    private void processFolder(String folderName) {
        System.out.println("Reading files in: " + folderName);
        ClassLoader classLoader = ClassLoader.getSystemClassLoader();

        try {
            File folder = new File(classLoader.getResource(folderName).toURI());
            List<File> files = Arrays.asList(folder.listFiles((f, name) -> name.toLowerCase().endsWith(".gpx")));

            files.forEach(f -> processFile(f, folderName));
        } catch (Exception e) {
            throw new IllegalStateException("Error accessing directory: " + folderName, e);
        } finally {
            if (client != null) {
                client = null;
            }
        }
    }

    private void processFile(File file, String folderName) {
        System.out.println("Processing file: " + file.getAbsolutePath() + " in category: " + folderName);

        try {
            JAXBContext jaxbContext = JAXBContext.newInstance(GpxType.class);
            Unmarshaller unmarshaller = jaxbContext.createUnmarshaller();
            JAXBElement<GpxType> root = unmarshaller.unmarshal(new StreamSource(file), GpxType.class);
            GpxType gpx = root.getValue();

            gpx.getWpt().forEach(wpt -> postWaypoint(wpt, folderName));

        } catch (JAXBException e) {
            throw new RuntimeException(e);
        }
    }


    private void postWaypoint(WptType waypoint, String category) {
        PointOfInterest poi = PointOfInterestFactory.createPointOfInterest(waypoint.getLat(), waypoint.getLon(), category, waypoint.getName());

        try {
            String json = JSON_MAPPER.writeValueAsString(poi);

            HttpRequest request = HttpRequest.newBuilder()
                    .uri(URI.create(Config.getPoiServiceUrl()))
                    .header("Content-Type", "application/json")
                    .POST(HttpRequest.BodyPublishers.ofString(json))
                    .build();
            HttpResponse<String> response = client.send(request, HttpResponse.BodyHandlers.ofString());
            System.out.println("Response: " + response.statusCode() + " - " + response.body());
        } catch (Exception e) {
            throw new RuntimeException("Error converting POI to JSON", e);
        }
    }
}