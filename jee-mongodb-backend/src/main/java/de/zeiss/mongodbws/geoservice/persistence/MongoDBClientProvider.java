/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * <p>
 * Copyright (C) 2025 Carl Zeiss Digital Innovation GmbH
 */
package de.zeiss.mongodbws.geoservice.persistence;

import com.mongodb.client.MongoClient;
import com.mongodb.client.MongoClients;
import dev.morphia.Datastore;
import dev.morphia.Morphia;
import jakarta.annotation.PostConstruct;
import jakarta.annotation.PreDestroy;
import jakarta.ejb.*;
import jakarta.inject.Inject;
import org.eclipse.microprofile.config.inject.ConfigProperty;

import java.util.logging.Logger;

/**
 * @author Andreas Post
 */
@Singleton
@ConcurrencyManagement(ConcurrencyManagementType.CONTAINER)
public class MongoDBClientProvider {

    private static final Logger LOG = Logger.getLogger(MongoDBClientProvider.class.getName());

    @Inject
    @ConfigProperty(name = "mongodb.database", defaultValue = "demo-campus")
    String databaseName = "demo-campus";

    @Inject
    @ConfigProperty(name = "mongodb.host", defaultValue = "localhost")
    String hostname;

    @Inject
    @ConfigProperty(name = "mongodb.port", defaultValue = "27017")
    int port;

    private MongoClient mongoClient = null;

    private Morphia morphia;

    private Datastore datastore;

    @PostConstruct
    public void init() {
        // TODO add user and password support
        LOG.info("Creating a MongoDB client for " + hostname + ":" + port + "/" + databaseName);
        LOG.info("To set a different host, port or database name, please adjust the configuration properties " +
                "'mongodb.host', 'mongodb.port' and 'mongodb.database' by supplying custom microprofile.properties in 'src/main/webapp/META-INF'" +
                " by creating a copy of the template file 'microprofile-config.properties.template'." +
                " Default values are 'localhost', '27017' and 'demo-campus'.");
        mongoClient = MongoClients.create("mongodb://" + hostname + ":" + port);

        datastore = Morphia.createDatastore(mongoClient, databaseName);
        // looks like we don't need this anymore with Morphia 2.x
        //datastore.getMapper().mapPackage("de.zeiss.mongodbws.geoservice.persistence.entity");
        //datastore.ensureIndexes();
    }

    @Lock(LockType.READ)
    public Datastore getDatastore() {
        return datastore;
    }

    /**
     *
     */
    @PreDestroy
    public void preDestroy() {
        LOG.info("preDestroy()");
        if (mongoClient != null) {
            mongoClient.close();
        }
    }
}
