# Read Me First
The following was discovered as part of building this project:

* The original package name 'de.zeiss.mongodb-ws.spring-geo-service' is invalid and this project uses 'de.zeiss.mongodb_ws.spring_geo_service' instead.

# Getting Started

## Development / Production configuration

This Spring Boot project is configured to run with an embedded Tomcat server during development and can be packaged as a WAR file for deployment in a production environment.
Both approaches need a different configuration for the service base path (context path).

Within the embedded Tomcat server, the context path can be set via application properties, while in a production environment, it is typically determined by the name of the WAR file or the container configuration.

Typically servlet containers use the name of the WAR file to set the context path and ignore any context path set in the application properties.
But to ensure, that no context path is set in the application properties during production (plus ending up with doubled entries in the path), the project uses separate Spring Boot profile files for development and production.


- The `application-dev.yaml` defines the servlet context-path while the `application-prod.yaml` doesn't.
- The active profile can be set via the `SPRING_PROFILES_ACTIVE` environment variable or via command line argument `--spring.profiles.active=dev|prod`.

### Reference Documentation
For further reference, please consider the following sections:

* [Official Apache Maven documentation](https://maven.apache.org/guides/index.html)
* [Spring Boot Maven Plugin Reference Guide](https://docs.spring.io/spring-boot/3.5.7/maven-plugin)
* [Create an OCI image](https://docs.spring.io/spring-boot/3.5.7/maven-plugin/build-image.html)
* [Spring Data MongoDB](https://docs.spring.io/spring-boot/3.5.7/reference/data/nosql.html#data.nosql.mongodb)
* [Spring Web](https://docs.spring.io/spring-boot/3.5.7/reference/web/servlet.html)

### Guides
The following guides illustrate how to use some features concretely:

* [Accessing Data with MongoDB](https://spring.io/guides/gs/accessing-data-mongodb/)
* [Building a RESTful Web Service](https://spring.io/guides/gs/rest-service/)
* [Serving Web Content with Spring MVC](https://spring.io/guides/gs/serving-web-content/)
* [Building REST services with Spring](https://spring.io/guides/tutorials/rest/)

### Maven Parent overrides

Due to Maven's design, elements are inherited from the parent POM to the project POM.
While most of the inheritance is fine, it also inherits unwanted elements like `<license>` and `<developers>` from the parent.
To prevent this, the project POM contains empty overrides for these elements.
If you manually switch to a different parent and actually want the inheritance, you need to remove those overrides.

