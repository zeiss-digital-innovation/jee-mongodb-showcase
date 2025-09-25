# Demo REST backend using Jakarta Enterprise technology.

## Dependencies and environment

### Java
- Java 21
- Jakarta Enterprise
- Wildfly Application Server Version 37

### MongoDB
- REST backend uses MongoDB as data provider.
- The default connection settings are host=localhost, port=27017, database=demo_campus. 
You can set different settings by supplying a custom `/src/main/webapp/META-INF/microprofile-config.properties` file. 
See the `microprofile-config.properties.template` file.
- As this is only a small demo application, there is currently no support for authentication with MongoDB. So it only works with a MongoDB instance without any authentication necessary (default after install).

## Build with Maven
- Use `mvn package` to create a `war` file for deployment.
- Deploy the service to a running Wildfly using `mvn install`. Setup Wildfly credentials and hostname in the ~/.m2/settings.xml. For this set `wildfly.deploy.username`, `wildfly.deploy.password` and `wildfly.deploy.hostname`.

## Docker
After building the Java project you can use the Dockerfile provided in the repository to create an image. It contains:
- Wildfly 37.0.1.Final-jdk21
- Latest build from the folder geo-service/target/

### MongoDB
- Currently the backend doesn't comes with a pre configured MongoDB.
- If you want to use MongoDB as a docker container, you can use the image below. The code was tested with this version.
- `docker pull mongodb/mongodb-community-server:latest`
- `docker run --name mongodb -p 27017:27017 --network demo-campus -d mongodb/mongodb-community-server:latest`
- See the `--network` option to specify a common network for MongoDB and Wildfly.