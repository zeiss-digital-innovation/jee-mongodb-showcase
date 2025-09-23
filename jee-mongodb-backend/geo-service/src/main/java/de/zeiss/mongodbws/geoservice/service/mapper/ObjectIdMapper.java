package de.zeiss.mongodbws.geoservice.service.mapper;

import org.bson.types.ObjectId;

public class ObjectIdMapper {

    public static ObjectId mapToObjectId(String id, String href) {
        String objectId = null;

        if (id != null) {
            objectId = id;
        } else if (href != null && !href.isEmpty()) {
            int lastIndexOfSlash = href.lastIndexOf("/");
            objectId = href.substring(lastIndexOfSlash);
        } else {
            return null;
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
