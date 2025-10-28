/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * <p>
 * Copyright (C) 2025 Carl Zeiss Digital Innovation GmbH
 */
package de.zeiss.mongodbws.geoservice.service.mapper;

import org.bson.types.ObjectId;

public class ObjectIdMapper {

    public static ObjectId mapToObjectId(String id, String href) {
        String objectId = null;

        if (id != null) {
            objectId = id;
        } else if (href != null && !href.isEmpty()) {
            int lastIndexOfSlash = href.lastIndexOf("/");

            if (lastIndexOfSlash > -1) {
                objectId = href.substring(lastIndexOfSlash + 1);
            }
        } else {
            return null;
        }

        if (!ObjectId.isValid(objectId)) {
            throw new IllegalArgumentException("Invalid ObjectId: " + objectId);
        }

        return new ObjectId(objectId);
    }

    public static String mapToString(ObjectId objectId) {
        if (objectId == null) {
            return null;
        }
        return objectId.toString();
    }
}
