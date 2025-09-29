FROM mongodb/mongodb-community-server:8.0.13-ubuntu2204

# Copy initialization script
COPY init-mongo.js /docker-entrypoint-initdb.d/

# Set environment variables
ENV MONGO_INITDB_DATABASE=demo-campus

# Expose MongoDB port
EXPOSE 27017

# MongoDB will automatically execute scripts in /docker-entrypoint-initdb.d/ on first startup
