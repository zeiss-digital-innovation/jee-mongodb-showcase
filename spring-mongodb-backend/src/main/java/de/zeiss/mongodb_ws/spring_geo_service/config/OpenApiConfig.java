package de.zeiss.mongodb_ws.spring_geo_service.config;

import io.swagger.v3.oas.models.OpenAPI;
import io.swagger.v3.oas.models.info.Info;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.web.servlet.config.annotation.ViewControllerRegistry;
import org.springframework.web.servlet.config.annotation.WebMvcConfigurer;

/**
 * OpenAPI configuration for the geo service.
 */
@Configuration
public class OpenApiConfig implements WebMvcConfigurer {

    @Bean
    public OpenAPI customOpenAPI() {
        return new OpenAPI()
                .info(new Info()
                        .title("ZDI Geo Service API")
                        .version("1.0")
                        .description("Spring Boot based REST API for managing Points of Interest"));
    }

    /**
     * Adding redirects for Swagger UI to be available under /swagger
     * @param registry
     */
    @Override
    public void addViewControllers(ViewControllerRegistry registry) {
        // Redirect /swagger to /swagger-ui/index.html
        registry.addRedirectViewController("/swagger", "/swagger-ui/index.html");
        registry.addRedirectViewController("/swagger/", "/swagger-ui/index.html");
    }
}

