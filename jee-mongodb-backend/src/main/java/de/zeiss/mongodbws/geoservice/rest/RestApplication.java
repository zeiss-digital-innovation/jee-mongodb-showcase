package de.zeiss.mongodbws.geoservice.rest;

import jakarta.ws.rs.ApplicationPath;
import jakarta.ws.rs.core.Application;
import org.eclipse.microprofile.openapi.annotations.OpenAPIDefinition;
import org.eclipse.microprofile.openapi.annotations.info.Info;
import org.eclipse.microprofile.openapi.annotations.info.License;

@OpenAPIDefinition(
        info = @Info(
                title = "GeoService API",
                version = "2.0.0",
                description = "REST API for managing Points of Interest with MongoDB backend",
                license = @License(
                        name = "MIT License",
                        url = "https://opensource.org/license/mit"
                )
        )
)
@ApplicationPath("/api")
public class RestApplication extends Application {
    // No additional configuration needed
}
