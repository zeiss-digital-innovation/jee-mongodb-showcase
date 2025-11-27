# Spring Boot MongoDB POI REST Backend - AI Coding Instructions

## Project Architecture

**Service**: ZDI Geo Service - A Spring Boot 3.5.7 REST API for managing Points of Interest (POIs) with geospatial queries.

### Core Layers

The codebase follows a **layered architecture** with clear separation:

1. **REST Controller Layer** (`src/main/java/de/zeiss/mongodb_ws/spring_geo_service/rest/`)
   - `PointOfInterestController`: Single controller handling all CRUD + geospatial search operations
   - **Key pattern**: Returns HATEOAS `href` links in all responses for navigability
   - Validates parameters using Jakarta validation annotations (@Min, @Max on request params; @Valid on request body)
   - All CRUD endpoints use REST semantics: POST/201, GET/200, PUT/204 (update) or 201 (create), DELETE/204

2. **Service Layer** (`src/main/java/de/zeiss/.../service/`)
   - `PointOfInterestService`: Business logic isolated from HTTP concerns
   - Uses `PointOfInterestMapper` to convert between REST models and database entities
   - **Key logic**: `listPOIs()` converts radius from meters to kilometers for MongoDB query
   - Strips `details` field from results unless `expand=details` is passed

3. **Data Layer** (`src/main/java/de/zeiss/.../persistence/`)
   - `IPointOfInterestRepository`: Spring Data MongoDB interface extending MongoRepository
   - **Custom query**: `findByLocationNear(Point, Distance)` uses geospatial 2dsphere index
   - `PointOfInterestEntity`: MongoDB document with `@Document(collection = "point-of-interest")`
   - MongoDB suppresses `_class` field via `MongoDBConfig` to keep documents clean

### Configuration

- **Profiles**: `application.yaml` (default), `application-dev.yaml` (local), `application-prod.yaml` (Docker)
- **MongoDB**: Default localhost:27017, database `demo_campus`; prod uses service name `mongodb`
- **Context Path**: `/zdi-geo-service` (set in application.yaml)
- **OpenAPI**: Swagger UI at `/swagger` (redirected from SpringDoc), JSON spec at `/v3/api-docs`
- **CORS**: Localhost-only for development (port-agnostic)

### Validation Strategy

Custom annotation `@ValidCoordinates` (in `rest.model.validation/`) ensures latitude ∈ [-90, 90] and longitude ∈ [-180, 180]. Global exception handler (`GlobalExceptionHandler`) returns field-level validation errors as JSON map.

## Key Workflows

### Build & Run
```bash
# Local development with embedded Tomcat
mvn clean package
mvn spring-boot:run

# Docker deployment (prod profile)
docker build -t demo-campus-spring-backend .
docker run -d --network demo-campus -p 8080:8080 demo-campus-spring-backend
```

### Testing
- **Unit tests**: `PointOfInterestServiceTest` (mocked repository), `PointOfInterestMapperTest`, `PointOfInterestControllerTest` (MockMvc)
- **Integration tests**: `SpringGeoServiceIntegrationTest` uses Testcontainers with real MongoDB
  - Conditionally skipped if Docker unavailable via `@DockerAvailable` annotation
  - Set MongoDB image version: `mvn -DMONGODB_IMAGE=mongo:7.0 test`
- **Coverage**: JaCoCo generates reports at `target/site/jacoco/`

### Key Coordinate System
- GeoJSON uses **[longitude, latitude]** order (NOT latitude, latitude)
- `PointOfInterest` REST model uses `org.geojson.Point` (external library)
- `PointOfInterestEntity` uses Spring's `GeoJsonPoint` for MongoDB storage
- **Mapper responsibility**: Convert between these coordinate orders

## Project-Specific Patterns

### PUT Upsert Semantics
PUT to `/{id}` returns:
- **201 Created** + Location header if resource doesn't exist → creates it
- **204 No Content** if resource exists → updates it
- Implementation: `PointOfInterestController.update()` checks if entity exists, routes to create/update accordingly

### Search with Expand Parameter
- Default: GET `/api/poi?lat=...&lon=...&radius=...` returns compact POIs (no `details` field)
- With expansion: `?expand=details` includes full `details` field
- Implementation: Service conditionally nulls `details` before mapping unless `expandDetails=true`

### Geospatial Indexing
MongoDB collection requires 2dsphere index on location field. Integration tests auto-create it via `mongoTemplate.indexOps()`. Production deployments may need manual index creation.

## File Organization

```
src/main/java/de/zeiss/mongodb_ws/spring_geo_service/
├── SpringGeoServiceApplication.java          # Spring Boot entry point
├── ServletInitializer.java                   # WAR deployment support
├── config/
│   ├── MongoDBConfig.java                    # Suppress _class field
│   ├── CorsConfig.java                       # Localhost-only CORS
│   └── OpenApiConfig.java                    # Swagger/OpenAPI setup
├── rest/
│   ├── controller/PointOfInterestController.java
│   ├── model/PointOfInterest.java            # @JsonInclude(NON_NULL) - omit nulls in JSON
│   ├── model/validation/{ValidCoordinates, ValidCoordinatesValidator}.java
│   └── GlobalExceptionHandler.java           # @ControllerAdvice for validation errors
├── service/
│   ├── PointOfInterestService.java
│   └── mapper/PointOfInterestMapper.java     # Bidirectional model↔entity conversion
└── persistence/
    ├── IPointOfInterestRepository.java
    └── entity/PointOfInterestEntity.java
```

## Dependencies to Know
- **Spring Data MongoDB**: Auto-configures MongoTemplate, enables @Document mapping
- **SpringDoc OpenAPI 2.8.13**: Generates Swagger UI (commons-lang3 3.18.0 override for CVE-2025-48924)
- **GeoJSON Jackson 1.14**: Provides `org.geojson.Point` for REST models
- **Testcontainers**: Integration test MongoDB instances (requires Docker)
- **JaCoCo 0.8.14**: Test coverage reports
- **Mockito**: Unit test mocking with javaagent for inline self-attachment

## Common Development Tasks

### Adding a New POI Field
1. Add to `PointOfInterestEntity` (e.g., `String rating`)
2. Add to `PointOfInterest` REST model with validation annotation if needed
3. Update `PointOfInterestMapper`: `mapToResource()`, `mapToEntity()`, `updateEntityFromModel()`
4. Add unit tests in `PointOfInterestMapperTest` covering null cases
5. Add integration test in `SpringGeoServiceIntegrationTest` covering the new field in CRUD cycle

### Adding a Search Parameter
1. Add `@RequestParam` to `PointOfInterestController.findPointsOfInterest()`
2. Add validation annotations (@Min, @Max, @NotNull as needed)
3. Pass to `poiService.listPOIs()`, extend signature if needed
4. Add unit test to `PointOfInterestControllerTest` (parameterized tests already cover boundary cases)
5. Add integration test to `SpringGeoServiceIntegrationTest`

### Extending Validation
- Custom validators: Create annotation + ConstraintValidator in `rest.model.validation/` (follow `ValidCoordinates` pattern)
- Constraint messages: Keep concise and field-specific for API responses
- Test: Add test cases to controller and integration tests for invalid inputs

## Notes for AI Agents
- **No-auth design**: Production requires adding authentication; current code has placeholder security posture
- **Single POI entity**: Design focuses on read-heavy geospatial queries; writes are straightforward
- **Mapper is critical**: Coordinate order conversion bugs will cause silent failures; always validate in tests
- **Docker networking**: Prod profile assumes `mongodb` hostname in Docker network; adjust for cloud deployments
- **Lombok not used**: All getters/setters explicit; maintain this pattern for clarity
