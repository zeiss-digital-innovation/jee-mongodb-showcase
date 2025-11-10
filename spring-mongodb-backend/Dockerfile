# Using a basic OpenJDK JRE image as the base image
FROM eclipse-temurin:21-jre

# Copying the application JAR file to the container
RUN mkdir /opt/app
COPY target/zdi-geo-service.jar /opt/app

EXPOSE 8080

# Starting the application with the production profile (uses docker internal MongoDB host)
CMD ["java", "-jar", "/opt/app/zdi-geo-service.jar", "--spring.profiles.active=prod"]
