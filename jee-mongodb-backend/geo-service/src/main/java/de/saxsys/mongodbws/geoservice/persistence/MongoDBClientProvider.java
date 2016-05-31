/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * 
 * Copyright (C) 2016 Saxonia Systems AG
 */
package de.saxsys.mongodbws.geoservice.persistence;

import java.util.logging.Logger;

import javax.annotation.PostConstruct;
import javax.annotation.PreDestroy;
import javax.ejb.ConcurrencyManagement;
import javax.ejb.ConcurrencyManagementType;
import javax.ejb.Lock;
import javax.ejb.LockType;
import javax.ejb.Singleton;

import org.mongodb.morphia.Datastore;
import org.mongodb.morphia.Morphia;

import com.mongodb.MongoClient;
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
		mongoClient = new MongoClient(new ServerAddress(HOST, PORT), settings);
		morphia = new Morphia();

		// tell morphia where to find your classes
		// can be called multiple times with different packages or classes
		morphia.mapPackage("de.saxsys.mongodbws.geoservice.persistence.entity");

		datastore = morphia.createDatastore(mongoClient, "saxonia_campus");
		datastore.ensureIndexes();
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
