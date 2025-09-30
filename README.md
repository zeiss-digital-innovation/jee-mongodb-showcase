# jee-mongodb-showcase
Demo Application showing a JEE application using MongoDB with Morphia object mapping plus an Angular2 frontend.

## JEE Backend

Simple REST client with a MongoDB connection.

Uses [Morphia Object Mapper](http://mongodb.github.io/morphia/) to work with MongoDB.

Expects a MongoDB running on localhost:27017. (TODO: add configuration for url).

### Build

Run `mvn package`

### Wildfly deployment

Running `mvn install` will also deploy the war file to a running wildfly instance on localhost.

## Angular2 frontend

Angular 2 client with Typescript displaying geo data via Google Maps.

Uses [angular2-google-maps](https://angular-maps.com/) for displaying Google Maps.

To build everything you need the Node.js package manager npm, available here: [Node.js](https://nodejs.org/en/) 

### Download dependencies

Run `npm install` to resolve dependencies.

### Build and run

Run `npm start` for serving the app and using it with a browser of your choice.

### Build distribution for environment

Run `npm run gulp` to build to dist directory

## License
This project is licensed under the terms of the MIT license.