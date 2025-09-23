# Demo REST backend using Jakarta Enterprise technology.

## Dependencies and environment
- Java 21
- Jakarta Enterprise
- Wildfly Application Server Version 37

## Build with Maven
- Use `mvn package` to create a `war` file for deployment.
- Deploy the service to a running Wildfly using `mvn install`. Setup Wildfly credentials and hostname in the ~/.m2/settings.xml. For this set `wildfly.deploy.username`, `wildfly.deploy.password` and `wildfly.deploy.hostname`.

## Docker

After building the Java project you can use the Dockerfile provided in the repository to create an image. It contains:
- Wildfly 37.0.1.Final-jdk21
- Latest build from the folder geo-service/target/