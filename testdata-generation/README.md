# Helper project to import test data

This is a helper project to import GPX files (GPS Exchange Format) to generate test data for the MongoDB.

## Prerequisites

- **Java Development Kit (JDK)**: 21 or higher
- **Maven**: 3.6 or higher
- **MongoDB**: instance running locally or remotely
- **Backend Service**: At least one backend service must be running to access MongoDB. I.e. use `jee-mongodb-backend`.
- **GPX Files**: Required for import

## Setup

### Download GPX Schema

1. Download the GPX 1.1 schema file from [Topografix](http://www.topografix.com/GPX/1/1/gpx.xsd).
2. Place the file in the `/src/main/xsd` folder.

### Generate Java Classes

Run the following command to generate Java classes from the XSD file:

```bash
mvn generate-sources
```

The generated classes will be placed in `target/generated-sources/jaxb` with the package `com.topografix.gpx`.

> Note: The XSD file is excluded from version control (see `.gitignore`).

### Configuration

Check the `application.properties` file in `/src/main/resources` to setup the correct backend service URL.

### Usage

#### Import GPX Files

1. Place GPX files in the `/src/main/resources` subfolders. Sample files are included.
2. Each subfolder represents a separate Point of Interest (POI) category. The folder name will be used as the POI
   category name.
3. Run the `Main.java` class to start the import process.

#### Example Folder Structure

```
/src/main/resources/
├── parks/
│ ├── park1.gpx
│ └── park2.gpx
├── restaurants/
│ ├── restaurant1.gpx
│ └── restaurant2.gpx
```

#### Build and Run the Application

To build the project, use:

```bash
mvn clean package
```

To run the JAR file:

```bash
java -jar target/geo-service-testdata-generation-1.0-SNAPSHOT.jar
```

### Sample Output

When the application runs successfully, you should see logs like:

```
Starting POI data generation and upload to: http://localhost:8080/geoservice/rest/poi
Processing files in: cash
Processing file: \jee-mongodb-showcase\testdata-generation\src\main\resources\cash\cash.gpx in category: cash
Found 1 waypoints in file: cash.gpx
```

### Additional Resources

* [GPX Format Specification](https://www.topografix.com/gpx.asp)