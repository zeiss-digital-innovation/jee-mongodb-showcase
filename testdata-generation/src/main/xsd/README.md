# Folder for GPX XSD file

## Usage

- Download original GPX 1.1 file here: [topografix](http://www.topografix.com/GPX/1/1/gpx.xsd)
- Place it in this folder
- Run `mvn generate-sources` to generate Java classes from the XSD file
- The generated classes will be placed in `target/generated-sources/jaxb` with the package `com.topografix.gpx`.
- Don't add the xsd file to version control (currently excluded in gitignore).