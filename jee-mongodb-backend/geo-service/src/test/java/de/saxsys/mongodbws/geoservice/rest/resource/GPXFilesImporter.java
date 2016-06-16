package de.saxsys.mongodbws.geoservice.rest.resource;

import static com.jayway.restassured.RestAssured.given;

import java.io.File;
import java.util.List;
import java.util.logging.Logger;

import javax.xml.bind.JAXBContext;
import javax.xml.bind.JAXBElement;
import javax.xml.bind.Unmarshaller;

import org.geojson.Point;
import org.junit.Test;

import com.jayway.restassured.response.Headers;

import ca.carleton.gcrc.gpx.GpxWayPoint;
import ca.carleton.gcrc.gpx._11.Gpx11;

/**
 * Class for some easy importing of point of interest data given as GPX files.
 * 
 * @author Andreas Post
 */
public class GPXFilesImporter extends TestsBase {

	private static final Logger LOG = Logger.getLogger(GPXFilesImporter.class.getName());

	private Headers headers;

	@Test
	public void importTestData() throws InterruptedException {
		doReadFolder("supermarket");
		doReadFolder("restaurant");
		doReadFolder("gasstation");
		doReadFolder("cash");
		doReadFolder("parking");
		doReadFolder("coffee");
	}

	/**
	 * Read all gpx files in folder.
	 * 
	 * @param directory
	 */
	private void doReadFolder(String directory) {
		System.out.println("Reading files in: " + directory);
		ClassLoader classLoader = ClassLoader.getSystemClassLoader();

		try {
			File folder = new File(classLoader.getResource(directory).toURI());
			File[] listFiles = folder.listFiles((f, name) -> name.toLowerCase().endsWith(".gpx"));

			for (File file : listFiles) {
				doImport(file, directory);
			}
		} catch (Exception e) {
			throw new IllegalStateException("Error accessing directory: " + directory, e);
		}
	}

	/**
	 * Import all waypoints on the given gpx file.
	 * 
	 * @param file
	 * @param category
	 */
	@SuppressWarnings("rawtypes")
	private void doImport(File file, String category) {
		System.out.println("Start importing " + file.getName());

		try {
			JAXBContext jc11 = JAXBContext.newInstance("com.topografix.gpx._1._1");
			Unmarshaller unmarshaller = jc11.createUnmarshaller();
			JAXBElement result = (JAXBElement) unmarshaller.unmarshal(file);

			Gpx11 gpx11 = new Gpx11((com.topografix.gpx._1._1.GpxType) result.getValue());

			List<GpxWayPoint> wayPoints = gpx11.getWayPoints();

			for (GpxWayPoint gpxWayPoint : wayPoints) {

				PointOfInterest poi = new PointOfInterest();
				String name = gpxWayPoint.getName().replace(", ", "\n");
				poi.setName(name);
				poi.setCategory(category);
				poi.setLocation(new Point(gpxWayPoint.getLong().doubleValue(), gpxWayPoint.getLat().doubleValue()));

				given().headers(headers).contentType(CONTENT_TYPE).body(poi).expect().post("poi");
				Thread.sleep(10);
			}

			System.out.println("Imported " + wayPoints.size() + " POIs from " + file.getName());
		} catch (Exception e) {
			throw new IllegalStateException("Error accessing file: " + file, e);
		}
	}

}
