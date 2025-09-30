# MongoDB Showcase

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](#license)
[![Project status](https://img.shields.io/badge/status-demo-orange.svg)](#)

This repository is a demo application used for an internal campus showing the usage of MongoDBs geospatial index capabilities. The database stores points of interest (POI) that are shown on a map on an Angular website. The website itself calls a backend service that connects to the MongoDB database. To show different ways of working with MongoDB, backends with different technologies are included in the repo.

## Table of Contents
- [Quickstart](#quickstart)
- [Prerequisites](#prerequisites)
- [Run the demo (fast path)](#run-the-demo-fast-path)
- [Subprojects / Layout](#subprojects--layout)
- [Test data](#test-data)
- [Development notes](#development-notes)
- [License](#license)
- [Contact](#contact)

## Quickstart

These steps will get a local demo running (frontend + example backend + local MongoDB).

1. Start MongoDB (from the `MongoDB` folder)
   - Windows PowerShell:
     ```
     docker-compose -f [docker-compose.yml](http://_vscodecontentref_/6) up -d
     ```
   - Confirm MongoDB is up:
     ```
     docker ps
     ```
   - Of course you can use any other MongoDB installation.
   - For more information (i.e. database prerequisites) see the folders README.

2. Start a backend and the frontend:
   - See each subproject README for exact commands and ports (examples below).

3. Open the frontend in your browser (typical Angular dev server):
  - http://localhost:4200

<figure style="text-align:center;">
  <a href="angular-maps-frontend/public/media/screenshots/angular_frontend_screenshot.png" target="_blank" rel="noopener">
    <img src="angular-maps-frontend/public/media/screenshots/angular_frontend_screenshot.png"
         alt="Angular frontend: interactive map with Points of Interest marked"
         style="max-width:480px;height:auto;border:1px solid #ccc;border-radius:4px;" />
  </a>
  <figcaption style="font-size:0.95em;color:#444;margin-top:6px;">
    Frontend demo: POIs loaded relative to the map center — click to view full-size.
  </figcaption>
</figure>

## Prerequisites
- Docker & Docker Compose (when using the provided Docker files).
- Node.js (for the Angular frontend).
- npm / yarn
- Java 21+ and Maven (for the JEE / Spring backend and the testdata generator)
- .NET SDK (for the DOTNET backend)

Its not neccessary to have the Java & .NET - select the backend of your choice.

## Run the demo (fast path — suggested)
- Start MongoDB:
  - PowerShell:
    ```
    docker-compose -f [docker-compose.yml](http://_vscodecontentref_/7) up -d
    ```
- Run the JEE backend (example):
  - From `jee-mongodb-backend`:
    ```
    cd jee-mongodb-backend
    mvn clean package
    # deployment will depend on how the JEE war is executed; see the sub-README
    ```
- Run the Angular frontend:
  - From `angular-maps-frontend`:
    ```
    cd angular-maps-frontend
    npm install
    npm start
    ```
- Open http://localhost:4200 and interact with the map.

Note: exact ports and steps vary by backend. See the subproject READMEs for full, correct commands.

## Subprojects / Layout

- `angular-maps-frontend/` — Angular app that displays POIs on a map and calls the backend for POIs near the current map center. See `angular-maps-frontend/README.md`.
- `jee-mongodb-backend/` — Jakarta (JEE) backend service. Contains REST endpoints and mapping to MongoDB.
- `MongoDB/` — Docker + compose configuration and initialization scripts. Use this to run a local MongoDB instance.
- `testdata-generation/` — Java tool to parse GPX files and POST POIs to a running backend to populate test data.
- `/* other planned backends */` — placeholders for .NET and Spring Boot backends.

## Test data

Use the `testdata-generation` project to generate and POST sample POIs:

- Build the generator:

```
cd testdata-generation
mvn clean package
```

Run with the generated jar (example):

```
java -jar target/geo-service-testdata-generation-1.0-SNAPSHOT.jar
```

See `testdata-generation/README.md` for full options.

## Development notes & tips
- The frontend loads POIs relative to the map center. Adjust backend query parameters to test different geospatial queries.
- The `MongoDB/init-mongo.js` script contains example index creation and sample dataset initialization.
- If you modify backend data models, ensure the frontend and test-data generator still post/consume the same JSON shape.

## License
This project is licensed under the terms of the MIT license. See `LICENSE.md` for details.

## Contact
Maintainers / Authors: see repository metadata or contact the repository owner.