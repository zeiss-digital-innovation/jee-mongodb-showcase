#!/bin/bash
# Script to display MongoDB container network information

echo "=========================================="
echo "MongoDB Container Network Information"
echo "=========================================="

CONTAINER_NAME="mongodb-demo-campus"

if docker ps -q -f name=${CONTAINER_NAME} | grep -q .; then
    echo "Container Status: Running ✅"
    
    # Get IP address in demo-campus network
    IP_ADDRESS=$(docker inspect ${CONTAINER_NAME} --format '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}')
    echo "IP Address: ${IP_ADDRESS}"
    
    # Get network details
    echo ""
    echo "Network Details:"
    docker inspect ${CONTAINER_NAME} --format '{{range $network, $config := .NetworkSettings.Networks}}Network: {{$network}}{{printf "\n"}}  IP: {{$config.IPAddress}}{{printf "\n"}}  Gateway: {{$config.Gateway}}{{printf "\n"}}  Aliases: {{range $config.Aliases}}{{.}} {{end}}{{printf "\n"}}{{end}}'
    
    echo ""
    echo "JEE Backend Connection Options:"
    echo "  - mongodb://mongodb-demo-campus:27017/demo-campus (recommended)"
    echo "  - mongodb://mongodb:27017/demo-campus (short alias)"
    echo "  - mongodb://${IP_ADDRESS}:27017/demo-campus (IP - not recommended)"
    
else
    echo "Container Status: Not running ❌"
    echo "Start the container with: docker-compose up -d"
fi

echo "=========================================="