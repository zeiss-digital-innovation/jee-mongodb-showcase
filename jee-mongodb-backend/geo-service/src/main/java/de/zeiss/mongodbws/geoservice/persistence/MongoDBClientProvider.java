/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * 
 * Copyright (C) 2016 Saxonia Systems AG
 */
package de.saxsys.mongodbws.geoservice.persistence;

import java.util.logging.Logger;

import com.mongodb.client.MongoClients;
import jakarta.annotation.PostConstruct;
import jakarta.annotation.PreDestroy;
import jakarta.ejb.ConcurrencyManagement;
import jakarta.ejb.ConcurrencyManagementType;
import jakarta.ejb.Lock;
import jakarta.ejb.LockType;
import jakarta.ejb.Singleton;

import dev.morphia.Datastore;
import dev.morphia.Morphia;

import com.mongodb.client.MongoClient;
import com.mongodb.MongoClientOptions;
import com.mongodb.ServerAddress;

/**
 * 
 * @author Andreas Post
 */
@Singleton
@ConcurrencyManagement(ConcurrencyManagementType.CONTAINER)
public class MongoDBClientProvider {

	private static final Logger LOG = Logger.getLogger(MongoDBClientProvider.class.getName());

	// TODO extract to property file
	private static final String DATABASE_NAME = "saxonia_campus";

	// TODO extract to property file
	private static final String HOST = "localhost";
	// TODO extract to property file
	private static final int PORT = 27017;

	private MongoClient mongoClient = null;

	private Morphia morphia;

	private Datastore datastore;

	@PostConstruct
	public void init() {
		MongoClientOptions settings = MongoClientOptions.builder()
				.codecRegistry(com.mongodb.MongoClient.getDefaultCodecRegistry()).build();
		mongoClient = MongoClients.create("mongodb://localhost:27017");

		// tell morphia where to find your classes
		// can be called multiple times with different packages or classes

		datastore = Morphia.createDatastore(mongoClient, DATABASE_NAME);
		datastore.getMapper().mapPackage("de.saxsys.mongodbws.geoservice.persistence.entity");
//		datastore.ensureIndexes();
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
