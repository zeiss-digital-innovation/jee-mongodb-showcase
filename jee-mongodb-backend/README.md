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
- As this is only a small demo application, there is currently no support for authentication with MongoDB. So it only
  works with a MongoDB instance without any authentication necessary (default after install).

## Build with Maven

- Use `mvn package` to create a `war` file for deployment.
- Deploy the service to a running Wildfly using `mvn install`. Setup Wildfly credentials and hostname in the ~
  /.m2/settings.xml. For this set `wildfly.deploy.username`, `wildfly.deploy.password` and `wildfly.deploy.hostname`.

## Docker

After building the Java project you can use the Dockerfile provided in the repository to create an image. The image will
then contain:

- Wildfly 37.0.1.Final-jdk21
- Latest build from the folder /target/

### Build the Docker image

In the root folder (contains the Dockerfile) execute the following:

```bash
docker build -t demo-campus-jee-backend .
```

This command builds the Docker image and tags it as demo-campus-jee-backend.

After building the image, setup a network and run a container using the following command:

```bash
docker network create demo-campus 
docker run --network demo-campus --name demo-campus-jee-backend -p 8080:8080 demo-campus-jee-backend
```

The network is necessary if you want to connect the backend to a MongoDB running in another container. See below for
more.

