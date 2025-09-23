FROM quay.io/wildfly/wildfly:37.0.1.Final-jdk21

# Setting the WildFly version
ENV WILDFLY_VERSION 37.0.1.Final-jdk21
# Setting the directory where deployments are stored - we will use a standalone server
ENV DEPLOYMENT_DIR /opt/jboss/wildfly/standalone/deployments/

# Copy the WAR file to the deployments directory
COPY geo-service/target/zdi-geo-service.war $DEPLOYMENT_DIR

# Expose the default WildFly port
EXPOSE 8080

# Expose the management port (optional, for admin console access)
# EXPOSE 9990

# Start WildFly in standalone mode
CMD ["/opt/jboss/wildfly/bin/standalone.sh", "-b", "0.0.0.0"]

# Use this to start Wildfly with admin console access
#CMD ["/opt/jboss/wildfly/bin/standalone.sh", "-b", "0.0.0.0", "-bmanagement", "0.0.0.0", "--network", "demo-campus"]