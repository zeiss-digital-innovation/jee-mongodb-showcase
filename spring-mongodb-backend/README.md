# REST backend using Spring Boot technology stack

This project is a REST backend application built using the Spring Boot framework. It provides a REST API to manage
POIs (points of
interest). The data is stored in a MongoDB database.

## Table of Contents

- [Quickstart](#quickstart)
- [Prerequisites](#prerequisites)
- [Build](#build)
- [Run](#run)
- [Docker](#docker)
- [Troubleshooting](#troubleshooting)
- [REST API Endpoints](#rest-api-endpoints)
- [Swagger API Endpoint](#swagger-api-endpoint)
- [Further Information](#further-information)

## Quickstart

These steps will get a local demo running with a MongoDB available on http://localhost:27017 (for configuring other
endpoints see the following sections).

### Using Maven

1. Build the project with `mvn clean package`
2. Run the backend with `mvn spring-boot:run`
3. Access the API with this sample
   request http://localhost:8080/zdi-geo-service/api/poi?lat=51.0490455&lon=13.7383389&radius=100&expand=details

### Using Docker

1. Build the project with `mvn clean package`
2. Build the Docker image with `docker build -t demo-campus-spring-backend .`
3. Run the backend with `docker run --name demo-campus-spring-backend -p 8080:8080 demo-campus-spring-backend`
4. Access the API with this sample
   request http://localhost:8080/zdi-geo-service/api/poi?lat=51.0490455&lon=13.7383389&radius=100&expand=details

## Prerequisites

### Java

- Java 21
- [Apache Maven](https://maven.apache.org/) 3.9

### MongoDB

- REST backend uses [MongoDB](https://www.mongodb.com/) as data provider.
  Uses [Spring Data MongoDB](https://spring.io/projects/spring-data-mongodb) to work
  with the database.
- The default connection settings are host=localhost, port=27017, database=demo_campus.
  You can set different settings by supplying different values in application properties or
  via environment variables. See the `application-dev.yaml` and `application-prod.yaml` files
- As this is only a small demo application, there is currently no support for authentication with MongoDB. So it only
  works with a MongoDB instance without any authentication necessary (default after install).
- see also the [MongoDB README](../MongoDB/README.md) for a docker-compose setup.

## Build

Use the Maven build `mvn clean package` to create a `jar` file for deployment.

## Run

This Spring Boot project is configured to run with an embedded Tomcat server. Within the embedded Tomcat server, the
context path is set via application properties.

### Run with embedded server

You can run the application with Docker (see the next section) or using the Spring Boot Maven plugin:

```bash
mvn spring-boot:run
```

### Run without embedded server

To run without the embedded server, change the pom.xml to build a WAR file and deploy it to an existing servlet
container. For this you need to adjust the following configuration:

- Remove the `spring-boot-starter-tomcat` dependency (or set to `provided`) and set the packaging to `war` in the
  `pom.xml`.
- Keep in mind, that the servlet container typically uses the name of the WAR file to set the context path and ignores
  any context path set in the application properties (server > servlet > context-path). Currently, the pom.xml ensures
  that the final name of the jar / war file is the same as the context path.

### Development / Production configuration

You can use different application properties for development and production.

- The `application-prod.yaml` defines `mongodb` instead of `localhost` for the MongoDB host. It is intended to be used
  in a
  Docker / Kubernetes environment where a MongoDB service is available with the name `mongodb`.
- The active profile can be set via the `SPRING_PROFILES_ACTIVE` environment variable or via command line argument
  `--spring.profiles.active=dev|prod`.

## Docker

After building the Java project you can use the Dockerfile provided in the repository to create an image. The image will
then contain:

- Java 21 Runtime: eclipse-temurin:21-jre
- Latest build from the folder `/target/`
- Using the provided `application-prod.yaml` as active profile. This assumes that a MongoDB service is
  available with the name `mongodb`. You can change this by providing your own application properties or environment
  variables.

### Build the Docker image

In the root folder (contains the Dockerfile) execute the following:

```bash
docker build -t demo-campus-spring-backend .
```

This command builds the Docker image and tags it as demo-campus-spring-backend.

### Docker network

If you run MongoDB with the provided docker configuration (see `../MongoDB/README.md`), the container will use a
docker network `demo-campus` and MongoDB will be available under the name `mongodb` in this network. Then run the
backend container in the same network.

You can check if the network already exists with:

```bash
docker network inspect demo-campus
```

If it does not exist, create it with:

```bash
docker network create demo-campus
```

### Run the Docker container

To run the backend container with the network, use:

```bash
docker run -d --network demo-campus --name demo-campus-spring-backend -p 8080:8080 demo-campus-spring-backend
```

If you don't need the network, simply run the container without the `--network` option:

```bash
docker run -d --name demo-campus-spring-backend -p 8080:8080 demo-campus-spring-backend
``` 

## Troubleshooting

- Port conflicts: Make sure ports 8080 and 27017 are not used by other services.
- Network issues: When using Docker for MongoDB and the app ensure both containers are running in the same Docker
  network.
- Build errors: Make sure the JAR file exists in the target directory before building the Docker image.

## REST API Endpoints

This backend exposes the following main REST endpoints:

### Find Points of Interest

- **Endpoint:** `GET /zdi-geo-service/api/poi`
- **Description:** Returns a list of points of interest (POIs) near a given location.
- **Parameters:**
    - `lat` (required): Latitude of the center point
    - `lon` (required): Longitude of the center point
    - `radius` (optional): Search radius in meters (default: 100)
    - `expand` (optional): If set to `details`, includes detailed information
- **Example request:**
  ```http
  GET http://localhost:8080/zdi-geo-service/api/poi?lat=51.0490455&lon=13.7383389&radius=100&expand=details
  ```
- **Example response:**
  ```json
  [
    {
      "href": "http://localhost:8080/zdi-geo-service/api/poi/68daa16c2dae92ecfb8823a6",
      "name": "Carl Zeiss Digital Innovation GmbH",
      "location": {
        "type": "Point",
        "coordinates": [13.7383389, 51.0490455]
      },
      "category": "company",
      "details": "Fritz-Foerster-Platz 2, 01069 Dresden, Tel.: +49 (0)351 497 01-500, https://www.zeiss.de/digital-innovation"
    }
  ]
  ```

### CRUD Operations for Points of Interest

- All CRUD operations (Create, Read, Update, Delete) are available under the endpoint
  `/zdi-geo-service/api/poi/{id}` using standard HTTP methods (POST, GET, PUT, DELETE).

## Swagger API Endpoint

The backend provides a Swagger UI for exploring and testing the REST API. It is available at:

```
http://localhost:8080/zdi-geo-service/swagger
```

## Further Information

### Reference Documentation

For further reference, please consider the following sections:

* [Official Apache Maven documentation](https://maven.apache.org/guides/index.html)
* [Spring Boot Maven Plugin Reference Guide](https://docs.spring.io/spring-boot/3.5.7/maven-plugin)
* [Spring Data MongoDB](https://docs.spring.io/spring-boot/3.5.7/reference/data/nosql.html#data.nosql.mongodb)
* [Spring Web](https://docs.spring.io/spring-boot/3.5.7/reference/web/servlet.html)

### Guides

The following guides illustrate how to use some features concretely:

* [Accessing Data with MongoDB](https://spring.io/guides/gs/accessing-data-mongodb/)
* [Building a RESTful Web Service](https://spring.io/guides/gs/rest-service/)
* [Serving Web Content with Spring MVC](https://spring.io/guides/gs/serving-web-content/)
* [Building REST services with Spring](https://spring.io/guides/tutorials/rest/)
