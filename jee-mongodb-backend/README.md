# REST backend using Jakarta Enterprise technology.

This is a simple REST backend using Jakarta Enterprise technology. It provides a REST API to manage POIs (points of
interest). The data is stored in a MongoDB database.

## Table of Contents

- [Quickstart](#quickstart)
- [Prerequisites](#prerequisites)
- [Build](#build)
- [Run](#run)
- [Troubleshooting](#troubleshooting)
- [REST API Endpoints](#rest-api-endpoints)
- [Third‑party Licenses](#third-party-licenses)

## Quickstart

These steps will get a local demo running with a MongoDB available on http://localhost:27017 (for configuring other
endpoints see the following sections).

1. Build the project with `mvn clean package`
2. Build the Docker image with `docker build -t demo-campus-jee-backend .`
4. Run the backend with `docker run --name demo-campus-jee-backend -p 8080:8080 demo-campus-jee-backend`
5. Access the API with this sample
   request http://localhost:8080/geoservice/poi?lat=51.0490455&lon=13.7383389&radius=100&expand=details

## Prerequisites

### Java

- Java 21
- Maven 3.9
- Wildfly Application Server Version 37 or Docker (for containerization)

### MongoDB

- REST backend uses MongoDB as data provider. Uses [Morphia Object Mapper](http://mongodb.github.io/morphia/) to work
  with the database.
- The default connection settings are host=localhost, port=27017, database=demo_campus.
  You can set different settings by supplying a custom `/src/main/webapp/META-INF/microprofile-config.properties` file.
  See the `microprofile-config.properties.template` file.
- As this is only a small demo application, there is currently no support for authentication with MongoDB. So it only
  works with a MongoDB instance without any authentication necessary (default after install).
- see also the [MongoDB README](../MongoDB/README.md) for a docker-compose setup.

## Build

- Use the Maven build `mvn clean package` to create a `war` file for deployment.

## Run

You can run the application in two ways:

### Deploy to existing Wildfly

- Setup Wildfly credentials and hostname in the ~
  /.m2/settings.xml. For this set `wildfly.deploy.username`, `wildfly.deploy.password` and `wildfly.deploy.hostname`.
- Use the Maven build `mvn clean install` to deploy the war file to a running Wildfly instance.
- Access the API with this sample
  request http://localhost:8080/zdi-geo-service/api/poi?lat=51.0490455&lon=13.7383389&radius=100&expand=details
- If you want to access the Swagger UI, start your Wildfly with the standalone-microprofile.xml configuration then after
  deployment you can assess it with http://localhost:8080/zdi-geo-service/swagger/

### Docker

After building the Java project you can use the Dockerfile provided in the repository to create an image. The image will
then contain:

- Wildfly 37.0.1.Final-jdk21
- Latest build from the folder /target/

#### Build the Docker image

In the root folder (contains the Dockerfile) execute the following:

```bash
docker build -t demo-campus-jee-backend .
```

This command builds the Docker image and tags it as demo-campus-jee-backend.

#### Docker network

If you run MongoDB with the provided docker configuration (see `../MongoDB/README.md`), the container will use a
docker network `demo-campus`. Then you need to set the MongoDB host in the
`/src/main/webapp/META-INF/microprofile-config.properties` file to `mongodb-demo-campus` (the name of the MongoDB
container) and run the
backend container in the same network.

You can check if the network already exists with:

```bash
docker network inspect demo-campus
```

If it does not exist, create it with:

```bash
docker network create demo-campus
```

To run the backend container with the network, use:

```bash
docker run -d --network demo-campus --name demo-campus-jee-backend -p 8080:8080 demo-campus-jee-backend
```

If you don't need the network, simply run the container without the `--network` option:

```bash
docker run -d --name demo-campus-jee-backend -p 8080:8080 demo-campus-jee-backend
``` 

## Troubleshooting

- Port conflicts: Make sure ports 8080 and 27017 are not used by other services.
- Network issues: When using Docker for MongoDB and Wildfly ensure both containers are running in the same Docker
  network.
- Build errors: Make sure the WAR file exists in the target directory before building the Docker image.

## REST API Endpoints

This backend exposes the following main REST endpoints:

### Get Points of Interest

- **Endpoint:** `GET /geoservice/poi`
- **Description:** Returns a list of points of interest (POIs) near a given location.
- **Parameters:**
    - `lat` (required): Latitude of the center point
    - `lon` (required): Longitude of the center point
    - `radius` (optional): Search radius in meters (default: 100)
    - `expand` (optional): If set to `details`, includes detailed information
- **Example request:**
  ```http
  GET http://localhost:8080/geoservice/poi?lat=51.0490455&lon=13.7383389&radius=100&expand=details
  ```
- **Example response:**
  ```json
  [
    {
      "href": "http://localhost:8080/geoservice/poi/68daa16c2dae92ecfb8823a6",
      "details": "Carl Zeiss Digital Innovation GmbH, Fritz-Foerster-Platz 2, 01069 Dresden",
      "location": {
        "type": "Point",
        "coordinates": [13.7383389, 51.0490455]
      }
    }
  ]
  ```

## Third‑party Licenses

### Swagger UI Integration

This project uses [https://github.com/swagger-api/swagger-ui](Swagger UI) to provide interactive API documentation.

- Swagger UI (distribution from the `dist` folder) is included in the `src/main/webapp/swagger` directory and is
  distributed under
  the [https://www.apache.org/licenses/LICENSE-2.0](Apache License Version 2.0).
- The original `LICENSE` and `NOTICE` files from the Swagger UI project are included in the same directory and are
  packaged into the built WAR; they remain under the Apache License 2.0.