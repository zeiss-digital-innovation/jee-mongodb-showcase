# MongoDB

- Currently the backend(s) doesn't comes with a pre configured MongoDB.
- If already availabe, you can use an own installation or a Docker image.

## Prerequisites
The backend(s) use the following database configuration:
- database: `demo-campus`
- collection: `point-of-interest`
- The collection needs at least the following index (besides the default `_id_` index): `db.point-of-interest.createIndex( { location : "2dsphere" } )`

## Docker Setup

### Method 1: Using Docker Compose (Recommended)

The easiest way to set up MongoDB with initial data is using docker-compose. This will automatically create the database, collection, and required indexes.

```bash
docker-compose up -d
```

This will:
- Start MongoDB 8.0.13 on port 27017
- Create the `demo-campus` database  
- Create the `point-of-interest` collection
- Add the required 2dsphere index for location-based queries
- Insert sample data (optional)
- Create a network named exactly `demo-campus` (not prefixed with project name)

### Method 2: Manual Docker Commands

If you prefer to use Docker commands directly:

```bash
# Pull the MongoDB image
docker pull mongodb/mongodb-community-server:8.0.13-ubuntu2204

# Run MongoDB with initialization
docker run --name mongodb-8.0.13 \
  -p 27017:27017 \
  -v ./init-mongo.js:/docker-entrypoint-initdb.d/init-mongo.js:ro \
  -e MONGO_INITDB_DATABASE=demo-campus \
  -d mongodb/mongodb-community-server:8.0.13-ubuntu2204
```

### Method 3: Using the Provided Dockerfile

You can also use the custom Dockerfile provided in this directory:

```bash
docker build -t mongodb-demo-campus .
docker run --name mongodb-demo-campus -p 27017:27017 -d mongodb-demo-campus
```

## Verification

After starting MongoDB, you can verify the setup:

### Connect to MongoDB
```bash
# Using MongoDB Compass (GUI)
mongodb://localhost:27017

# Using mongosh (CLI)
mongosh mongodb://localhost:27017
```

### Verify Database and Collection
```javascript
// List databases
show dbs

// Switch to demo-campus database  
use demo-campus

// List collections
show collections

// Verify the 2dsphere index exists
db['point-of-interest'].getIndexes()

// Check sample data (if inserted)
db['point-of-interest'].find().pretty()
```

## Useful Commands

```bash
# Stop the container
docker stop mongodb-demo-campus

# Start the container  
docker start mongodb-demo-campus

# Remove the container
docker rm mongodb-demo-campus

# View container logs
docker logs mongodb-demo-campus

# For docker-compose
docker-compose down     # Stop and remove containers only
docker-compose down -v  # Stop and remove containers AND volumes (complete reset)
docker-compose up -d    # Start in background
docker-compose logs     # View logs
```

## Important: Resetting MongoDB

**The initialization script (`init-mongo.js`) only runs when MongoDB starts with an empty data directory.**

If you want to reset MongoDB and trigger the initialization script again:

```bash
# Complete reset - removes containers AND data volumes
docker-compose down -v

# Start fresh (initialization script will run)
docker-compose up -d
```

**Note**: `docker-compose down` (without `-v`) only removes containers but keeps the data volume, so the initialization script won't run again.

## Container Network Information

### Getting MongoDB IP Address and Connection Details

Use the provided script to get detailed network information:

```bash
# PowerShell (Windows)
.\get-mongodb-info.ps1

# Bash (Linux/Mac)  
./get-mongodb-info.sh
```

Or manually check the IP address:

```bash
# Get IP address
docker inspect mongodb-demo-campus --format '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}'

# Get detailed network info
docker inspect mongodb-demo-campus --format '{{json .NetworkSettings.Networks}}'
```

### JEE Backend Connection

**Recommended**: Use container/service names instead of IP addresses:

```properties
# In your JEE application configuration
mongodb.host=mongodb-demo-campus
mongodb.port=27017  
mongodb.database=demo-campus

# Or as connection string
mongodb://mongodb-demo-campus:27017/demo-campus
```

**Why container names are better than IP addresses:**
- IP addresses can change when containers restart
- Docker provides automatic DNS resolution for container names
- More maintainable and reliable

## Network Configuration

**Important Note**: This setup uses the existing `demo-campus` network that's shared with your WildFly/JEE application. The docker-compose.yml file specifies:

```yaml
networks:
  demo-campus:
    external: true  # Uses the existing demo-campus network
```

This allows MongoDB to communicate with your JEE application on the same network. If the `demo-campus` network doesn't exist, create it first:

```bash
docker network create demo-campus
```

## Configuration Files

This directory contains:
- `init-mongo.js`: MongoDB initialization script that creates the database, collection, and indexes
- `docker-compose.yml`: Docker Compose configuration for easy setup with proper network naming and health checks
- `Dockerfile`: Custom Docker image configuration
- `get-mongodb-info.ps1`: PowerShell script to display container network information
- `get-mongodb-info.sh`: Bash script to display container network information