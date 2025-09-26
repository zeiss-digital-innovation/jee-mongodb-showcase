# Helper project to import test data

This is a helper project to import GPX files (GPS Exchange Format) to generate test data for the MongoDB.

## Usage

### Prerequisites

- Java Development Kit (JDK) 21 or higher
- Maven 3.6 or higher
- MongoDB instance running locally or remotely
- One of the backend services need to run to access the MongoDB
- GPX files to import

### Steps to Import GPX Files

- Download original GPX 1.1 file here: [topografix](http://www.topografix.com/GPX/1/1/gpx.xsd)
- Place it in `/src/main/xsd` folder
- Run `mvn generate-sources` to generate Java classes from the XSD file
- The generated classes will be placed in `target/generated-sources/jaxb` with the package `com.topografix.gpx`.
- Don't add the xsd file to version control (currently excluded in gitignore).
- Check the `application.properties` file in `/src/main/resources` to setup the correct backend service URL.
- Run Main.java. It looks for GPX files in the `/src/main/resources` sub folders. Sample files are included.
- Every subfolder of `/src/main/resources` is used as separate Point of Interest (POI) category. The name of the POI
  category is the name of the subfolder.