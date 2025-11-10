[![Java](https://img.shields.io/badge/java-21-blue.svg)](https://www.oracle.com/java/technologies/javase/jdk21-archive-downloads.html)
[![Spring Boot](https://img.shields.io/badge/spring--boot-3.5.7-green.svg)](https://spring.io/projects/spring-boot)
[![Maven](https://img.shields.io/badge/maven-build-brightgreen)](https://maven.apache.org/)
[![Docker](https://img.shields.io/badge/docker-container-blue.svg)](https://www.docker.com/)
[![MongoDB](https://img.shields.io/badge/mongodb-database-green.svg)](https://www.mongodb.com/)
[![Coverage](https://img.shields.io/badge/coverage-jacoco-yellowgreen)](target/site/jacoco/index.html)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](../LICENSE.md)

# REST backend using Spring Boot technology stack

This project is a REST backend application built using the Spring Boot framework. It provides a REST API to manage
POIs (points of
interest). The data is stored in a MongoDB database.

## Table of Contents

- [Quickstart](#quickstart)
    - [Using Maven](#using-maven)
    - [Using Docker](#using-docker)
    - [Access the running application](#access-the-running-application)
- [Prerequisites](#prerequisites)
    - [Java](#java)
    - [MongoDB](#mongodb)
- [Configuration (important properties)](#configuration-important-properties)
- [Build](#build)
    - [Build reports](#build-reports)
- [Run](#run)
    - [Run with embedded server](#run-with-embedded-server)
    - [Run without embedded server](#run-without-embedded-server)
    - [Development / Production configuration](#development--production-configuration)
- [Testing](#testing)
- [Docker](#docker)
    - [Build the Docker image](#build-the-docker-image)
    - [Docker network](#docker-network)
    - [Run the Docker container](#run-the-docker-container)
- [Troubleshooting](#troubleshooting)
- [REST API Endpoints](#rest-api-endpoints)
    - [Find Points of Interest](#find-points-of-interest)
    - [CRUD Operations for Points of Interest](#crud-operations-for-points-of-interest)
    - [Overview on available operations and expected response codes](#overview-on-available-operations-and-expected-response-codes)
        - [PUT Semantics](#put-semantics)
- [Swagger API Endpoint](#swagger-api-endpoint)
- [License](#license)
    - [Third‑party software and licenses](#third%E2%80%91party-software-and-licenses)
- [Further Information](#further-information)
    - [Reference Documentation](#reference-documentation)
    - [Guides](#guides)

## Quickstart

These steps will get a local demo running with a MongoDB available on http://localhost:27017 (for configuring other
endpoints see the following sections).

### Using Maven

1. Build the project with `mvn clean package`
2. Run the backend with `mvn spring-boot:run`

### Using Docker

1. Build the project with `mvn clean package`
2. Build the Docker image with `docker build -t demo-campus-spring-backend .`
3. Run the backend with `docker run --name demo-campus-spring-backend -p 8080:8080 demo-campus-spring-backend`

### Access the running application

Once the application is running, you can access the REST API at:

http://localhost:8080/zdi-geo-service/api/poi

Sample request to find POIs near a location:

```http
GET http://localhost:8080/zdi-geo-service/api/poi?lat=51.0490455&lon=13.7383389&radius=100&expand=details
```

See [REST API Endpoints](#rest-api-endpoints) for more details and examples.

## Prerequisites

### Java

- [Java 21](https://www.oracle.com/java/technologies/javase/jdk21-archive-downloads.html)
- [Apache Maven](https://maven.apache.org/) 3.9

### MongoDB

- REST backend uses [MongoDB](https://www.mongodb.com/) as data provider.
  Uses [Spring Data MongoDB](https://spring.io/projects/spring-data-mongodb) to work
  with the database.
- The default connection settings are host=localhost, port=27017, database=demo_campus.
  You can set different settings by supplying different values in application properties or
  via environment variables. See the `application-dev.yaml` and `application-prod.yaml` files.
- As this is only a small demo application, there is currently no support for authentication with MongoDB. So it only
  works with a MongoDB instance without any authentication necessary (default after install).
- See also the [MongoDB README](../MongoDB/README.md) for a docker-compose setup.

## Configuration (important properties)

| Property                     |          Default | Description               |
|------------------------------|-----------------:|---------------------------|
| spring.data.mongodb.host     |        localhost | MongoDB host              |
| spring.data.mongodb.port     |            27017 | MongoDB port              |
| spring.data.mongodb.database |      demo_campus | DB name                   |
| server.servlet.context-path  | /zdi-geo-service | Application context path  |
| spring.profiles.active       |              dev | Active profile (dev/prod) |

## Build

Use the Maven build `mvn clean package` to create a `jar` file for deployment.

### Build reports

To generate project reports, you can use:

```bash
mvn clean site
mvn clean test jacoco:report
start target\site\index.html
start target\site\jacoco\index.html
```

This will create documentation in the `target/site` folder.

## Run

This Spring Boot project is configured to run with an embedded Tomcat server. Within the embedded Tomcat server, the
context path is set via application properties.

### Run with embedded server

You can run the application with Docker (see the [Docker](#docker) section) or using the Spring Boot Maven plugin:

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

## Testing

You can run the tests with Maven using:

```bash
mvn clean test
```

You can also generate code coverage reports with:

```bash
mvn clean test jacoco:report
```

After running the tests, the code coverage report can be found in the folder `target/site/jacoco/index.html`.

## Docker

After building the Java project you can use the [Dockerfile](Dockerfile) provided in the repository to create an image.
The image will
then contain:

- Java 21 Runtime: eclipse-temurin:21-jre
- Latest build from the folder `/target/`
- Using the provided `application-prod.yaml` as active profile. This assumes that a MongoDB service is
  available with the host name `mongodb`. You can change this by providing your own application properties or
  environment
  variables.

### Build the Docker image

In the root folder (contains the Dockerfile) execute the following (don't forget the dot at the end):

```bash
docker build -t demo-campus-spring-backend .
```

This command builds the Docker image and tags it as demo-campus-spring-backend.

### Docker network

If you run MongoDB with the provided Docker configuration (see [MongoDB-Readme](../MongoDB/README.md)), the container
will use a
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
  ```http
  HTTP/1.1 200 OK
  Content-Type: application/json
  ```
  ```json
  [
    {
      "href": "http://localhost:8080/zdi-geo-service/api/poi/68daa16c2dae92ecfb8823a6",
      "name": "Carl Zeiss Digital Innovation GmbH",
      "location": {
        "type": "Point",
        "coordinates": [
          13.7383389,
          51.0490455
        ]
      },
      "category": "company",
      "details": "Fritz-Foerster-Platz 2, 01069 Dresden, Tel.: +49 (0)351 497 01-500, https://www.zeiss.de/digital-innovation"
    }
  ]
  ```

### CRUD Operations for Points of Interest

- All CRUD operations (Create, Read, Update, Delete) are available under the endpoint
  `/zdi-geo-service/api/poi/{id}` using standard HTTP methods (POST, GET, PUT, DELETE).

- **Example POST request to create a new POI using curl:**
  ```bash
  curl -X POST "http://localhost:8080/zdi-geo-service/api/poi" \
    -H "Content-Type: application/json" \
    -d '{
      "name":"New POI",
      "category":"company",
      "location":{"type":"Point","coordinates":[13.7383389,51.0490455]},
      "details":"Example details"
    }' -i
  ```

- **Expected response (example):**
  ```http
  HTTP/1.1 201 Created
  Location: http://localhost:8080/zdi-geo-service/api/poi/{new-id}
  Content-Type: application/json
  ```

### Overview on available operations and expected response codes

| Endpoint                      | Method |        Success Status         |     Error Status     |
|-------------------------------|-------:|:-----------------------------:|:--------------------:|
| /zdi-geo-service/api/poi      |    GET |              200              | 400 (invalid params) |
| /zdi-geo-service/api/poi      |   POST |              201              |   400 (validation)   |
| /zdi-geo-service/api/poi/{id} |    GET |              200              |         404          |
| /zdi-geo-service/api/poi/{id} |    PUT | 201 (created) / 204 (updated) |   400 (validation)   |
| /zdi-geo-service/api/poi/{id} | DELETE |              204              |         404          |

#### PUT Semantics

For PUT this project follows the upsert semantics. This means a `PUT /zdi-geo-service/api/poi/{id}` will create the
resource if it does not exist (returning
`201 Created` with a `Location` header set) and will update an existing resource if it already exists (returning
`204 No Content`). Below are example responses for both cases:

- **Example: resource created (201)**
  ```http
  HTTP/1.1 201 Created
  Location: http://localhost:8080/zdi-geo-service/api/poi/{new-id}
  Content-Type: application/json
  ```
- **Example: resource updated (204)**
  ```http
  HTTP/1.1 204 No Content
  ```

## Swagger API Endpoint

Swagger UI is available at:
http://localhost:8080/zdi-geo-service/swagger

You can find the OpenAPI specification in JSON format at:
http://localhost:8080/zdi-geo-service/v3/api-docs

## License

This project is licensed under the MIT License — see the repository root LICENSE file: [../LICENSE.md](../LICENSE.md).

### Third‑party software and licenses

This project also uses third‑party libraries with their own licenses (Apache‑2.0, MIT, EPL, EDL, LGPL, ...).
A detailed dependency and license report is generated by Maven: run `mvn -DskipTests site` and open
`target/site/licenses.html`.
You can find a summary of third‑party attributions in [THIRD-PARTY-LICENSES](THIRD-PARTY-LICENSES.md).

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
