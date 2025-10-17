# 🐳 Docker Deployment - .NET MongoDB Backend

## 🚀 Overview

This .NET Backend can be deployed with Docker in various ways. The intelligent deploy scripts automatically detect the MongoDB environment and select the optimal configuration.

## 📋 Deployment Options

### 1. **Automatic Deployment (Recommended)**

#### Windows
```cmd
# Automatic MongoDB detection and deployment
.\deploy.bat
```

#### Linux/macOS
```bash
# Automatic MongoDB detection and deployment
chmod +x deploy.sh
./deploy.sh
```

### 2. **Manual Docker-Compose Variants**

#### Complete System (Backend + MongoDB)
```bash
# Starts own MongoDB + Backend
docker-compose up --build -d
```

#### Backend Only (external MongoDB)
```bash
# Uses existing MongoDB
docker-compose -f docker-compose.external-mongo.yml up --build -d
```

#### Development Mode
```bash
# Development configuration with Hot Reload
docker-compose -f docker-compose.local.yml up --build
```

## 🔧 Deploy Script Features

### Intelligent MongoDB Detection

The deploy scripts automatically perform the following checks:

1. **Network Creation**: Creates `demo-campus` network if not present
2. **MongoDB Port Check**: Checks if port 27017 is in use
3. **Container Detection**: Searches for running MongoDB containers
4. **Automatic Configuration**: Selects appropriate docker-compose file

### Deployment Scenarios

| Scenario | MongoDB Status | Used Configuration | Behavior |
|----------|----------------|-------------------|----------|
| **No MongoDB** | Not found | `docker-compose.yml` | Starts Backend + own MongoDB |
| **MongoDB Container** | Container running | `docker-compose.external-mongo.yml` | Uses existing container |
| **External MongoDB** | Port 27017 in use | `docker-compose.external-mongo.yml` | Connects to external DB |

## 🌐 Container Configurations

### Standard Configuration (`docker-compose.yml`)
```yaml
services:
  backend:
    ports: ["8080:8080"]
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - MongoSettings__ConnectionString=mongodb://mongodb:27017
  
  mongodb:
    image: mongodb/mongodb-community-server:latest
    ports: ["27017:27017"]
```

### External MongoDB (`docker-compose.external-mongo.yml`)
```yaml
services:
  backend:
    ports: ["8080:8080"]
    environment:
      - DOTNET_RUNNING_IN_CONTAINER=true
      - MongoSettings__ConnectionString=mongodb://mongodb:27017
    networks: [demo-campus]
```

### Development Mode (`docker-compose.local.yml`)
```yaml
services:
  backend:
    ports: ["5000:80", "5001:443"]
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    volumes:
      - ./DotNetMongoDbBackend:/app/src
```

## 🛠 Manual Docker Commands

### Build Image
```bash
# Release Build
docker build -t dotnet-mongodb-backend .

# Development Build
docker build -t dotnet-mongodb-backend:dev --target development .
```

### Start Container
```bash
# Standard configuration
docker run -d --name backend \
  -p 8080:8080 \
  -e MongoSettings__ConnectionString=mongodb://host.docker.internal:27017 \
  dotnet-mongodb-backend

# With external MongoDB
docker run -d --name backend \
  -p 8080:8080 \
  --network demo-campus \
  -e DOTNET_RUNNING_IN_CONTAINER=true \
  dotnet-mongodb-backend
```

## 🏥 Health Checks

### Container Health Check
```bash
# Check health check status
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# Detailed health information
docker inspect --format='{{.State.Health.Status}}' dotnet-mongodb-backend
```

### API Health Check
```bash
# Backend health check
curl http://localhost:8080/geoservice/health

# Debug information
curl http://localhost:8080/geoservice/debug
```

## 📊 Monitoring & Debugging

### View Container Logs
```bash
# Follow live logs
docker logs dotnet-mongodb-backend -f

# Last 50 lines
docker logs dotnet-mongodb-backend --tail 50

# Logs with timestamps
docker logs dotnet-mongodb-backend -t
```

### Container Information
```bash
# Container details
docker inspect dotnet-mongodb-backend

# Network information
docker network inspect demo-campus

# Resource usage
docker stats dotnet-mongodb-backend
```

## 🔄 Container Management

### Stop and Restart
```bash
# Stop container
docker-compose down

# Restart container
docker-compose restart backend

# Rebuild complete system
docker-compose down
docker-compose up --build -d
```

### Manage Data Volumes
```bash
# Show volumes
docker volume ls

# Delete MongoDB data
docker volume rm mongo-data

# Remove all unused volumes
docker volume prune
```

## 🌍 Environment Variables

### Configuration Options
```bash
# MongoDB Connection
MongoSettings__ConnectionString=mongodb://localhost:27017

# Database Name
MongoSettings__Database=demo-campus

# Collection Name
MongoSettings__Collections__Pois=point-of-interest

# ASP.NET Environment
ASPNETCORE_ENVIRONMENT=Production|Development

# Container Detection
DOTNET_RUNNING_IN_CONTAINER=true
```

## 🔍 Troubleshooting

### Port Already in Use
```bash
# Find process using port 8080
netstat -ano | findstr :8080  # Windows
lsof -i :8080                  # Linux/macOS

# Stop conflicting service
docker stop $(docker ps -q --filter "publish=8080")
```

### MongoDB Connection Failed
```bash
# Check MongoDB container
docker ps | grep mongo

# Test MongoDB connection
docker exec mongodb mongosh --eval "db.adminCommand('ping')"

# Check network connectivity
docker network inspect demo-campus
```

### Container Restart Loop
```bash
# View container logs
docker logs dotnet-mongodb-backend --tail 100

# Check container events
docker events --filter container=dotnet-mongodb-backend

# Inspect container exit code
docker inspect dotnet-mongodb-backend --format='{{.State.ExitCode}}'
```

## 📦 Multi-Stage Build

The Dockerfile uses multi-stage builds for optimal image size:

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "DotNetMongoDbBackend.dll"]
```

### Build Stages
- **build**: Compiles application (SDK image ~500MB)
- **runtime**: Production image (ASP.NET ~200MB)

## 🚀 Production Deployment

### Best Practices
```bash
# Use specific version tags
docker pull mcr.microsoft.com/dotnet/aspnet:9.0

# Set resource limits
docker run -d \
  --memory="512m" \
  --cpus="1.0" \
  --restart=unless-stopped \
  dotnet-mongodb-backend

# Use secrets for sensitive data
docker secret create mongo_connection_string connection.txt
```

### Docker Compose Production
```yaml
services:
  backend:
    image: dotnet-mongodb-backend:latest
    container_name: dotnet-mongodb-backend-prod
    restart: unless-stopped
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - MongoSettings__ConnectionString=mongodb://mongodb:27017
      - MongoSettings__Database=demo-campus
      - DOTNET_RUNNING_IN_CONTAINER=true
    networks:
      - demo-campus
    depends_on:
      - mongodb
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/zdi-geo-service/api/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
    deploy:
      resources:
        limits:
          cpus: '1.0'
          memory: 512M
        reservations:
          cpus: '0.5'
          memory: 256M

  mongodb:
    image: mongodb/mongodb-community-server:latest
    container_name: mongodb-prod
    restart: unless-stopped
    ports:
      - "27017:27017"
    volumes:
      - mongo-data:/data/db
    networks:
      - demo-campus

networks:
  demo-campus:
    external: true

volumes:
  mongo-data:
    driver: local
```

## 🔐 Security Considerations

### Production Hardening
```bash
# Run as non-root user
docker run -d --user 1000:1000 dotnet-mongodb-backend

# Read-only filesystem
docker run -d --read-only dotnet-mongodb-backend

# Drop capabilities
docker run -d --cap-drop ALL dotnet-mongodb-backend

# Use security scanning
docker scan dotnet-mongodb-backend
```

### MongoDB Security
- Use authentication in production
- Configure SSL/TLS for MongoDB connections
- Restrict network access with firewall rules
- Regular security updates

## 📊 Monitoring & Observability

### Health Check Endpoints
```bash
# Application health
curl http://localhost:8080/zdi-geo-service/api/health

# Debug information (development only)
curl http://localhost:8080/zdi-geo-service/api/debug
```

### Prometheus Metrics (optional)
```bash
# Add to docker-compose.yml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - EnablePrometheusMetrics=true
```

## 🔄 CI/CD Integration

### GitHub Actions Example
```yaml
name: Docker Build and Deploy

on:
  push:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Build Docker Image
        run: docker build -t dotnet-mongodb-backend:${{ github.sha }} .
      - name: Run Tests
        run: docker run dotnet-mongodb-backend:${{ github.sha }} dotnet test
      - name: Push to Registry
        run: docker push dotnet-mongodb-backend:${{ github.sha }}
```

## 📚 Additional Resources

### Documentation
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [MongoDB C# Driver Documentation](https://mongodb.github.io/mongo-csharp-driver/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)

### Related Projects
- **Angular Frontend**: `../angular-maps-frontend/`
- **JEE Backend** (Reference Implementation): `../jee-mongodb-backend/`
- **MongoDB Setup**: `../MongoDB/`

## 🐛 Known Issues

### Issue: Connection Timeout
**Solution**: Increase MongoDB timeout settings in `appsettings.json`

### Issue: Container Cannot Connect to MongoDB
**Solution**: Ensure both containers are on the same Docker network

### Issue: Port 8080 Already in Use
**Solution**: Stop conflicting services or change port mapping

## 🤝 Contributing

### Development Workflow
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests: `dotnet test`
5. Build Docker image: `docker build -t dotnet-mongodb-backend .`
6. Submit pull request

## 📄 License

See [LICENSE.md](../LICENSE.md) for details.

## 📞 Support

For issues and questions:
- Check the main [README.md](README.md)
- Review [MONGODB_CONFIG_README.md](MONGODB_CONFIG_README.md) for configuration help
- Compare with JEE Backend reference implementation

## 🎯 Quick Reference

### Most Common Commands
```bash
# Quick start
./deploy.bat                                  # Windows
./deploy.sh                                   # Linux/macOS

# View logs
docker logs dotnet-mongodb-backend -f

# Restart service
docker-compose restart backend

# Clean rebuild
docker-compose down && docker-compose up --build -d

# Health check
curl http://localhost:8080/zdi-geo-service/api/health
```

---

**Last Updated**: October 2025  
**Version**: 1.0  
**Docker Image Base**: mcr.microsoft.com/dotnet/aspnet:9.0
