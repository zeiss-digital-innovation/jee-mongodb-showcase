/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * <p>
 * Copyright (C) 2025 Carl Zeiss Digital Innovation GmbH
 */
package de.zeiss.mongodbws.geoservice.service.mapper;

import org.bson.types.ObjectId;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

/**
 * Unit tests for {@link ObjectIdMapper}
 *
 * @author Generated Tests
 */
public class ObjectIdMapperTest {

    @Test
    public void testMapToObjectId_WithValidId_ShouldReturnObjectId() {
        // Given
        String validId = "507f1f77bcf86cd799439011";

        // When
        ObjectId result = ObjectIdMapper.mapToObjectId(validId, null);

        // Then
        assertNotNull(result);
        assertEquals(validId, result.toString());
    }

    @Test
    public void testMapToObjectId_WithIdAndHref_ShouldPreferIdOverHref() {
        // Given
        String id = "507f1f77bcf86cd799439011";
        String href = "/api/poi/507f191e810c19729de860ea";

        // When
        ObjectId result = ObjectIdMapper.mapToObjectId(id, href);

        // Then
        assertNotNull(result);
        assertEquals(id, result.toString());
    }

    @Test
    public void testMapToObjectId_WithValidHref_ShouldExtractObjectIdFromHref() {
        // Given
        String href = "/api/poi/507f1f77bcf86cd799439011";
        String expectedId = "507f1f77bcf86cd799439011";

        // When
        ObjectId result = ObjectIdMapper.mapToObjectId(null, href);

        // Then
        assertNotNull(result);
        assertEquals(expectedId, result.toString());
    }

    @Test
    public void testMapToObjectId_WithHrefContainingMultipleSlashes_ShouldExtractLastPart() {
        // Given
        String href = "/api/v1/poi/507f1f77bcf86cd799439011";
        String expectedId = "507f1f77bcf86cd799439011";

        // When
        ObjectId result = ObjectIdMapper.mapToObjectId(null, href);

        // Then
        assertNotNull(result);
        assertEquals(expectedId, result.toString());
    }

    @Test
    public void testMapToObjectId_WithNullIdAndNullHref_ShouldReturnNull() {
        // When
        ObjectId result = ObjectIdMapper.mapToObjectId(null, null);

        // Then
        assertNull(result);
    }

    @Test
    public void testMapToObjectId_WithNullIdAndEmptyHref_ShouldReturnNull() {
        // When
        ObjectId result = ObjectIdMapper.mapToObjectId(null, "");

        // Then
        assertNull(result);
    }

    @Test
    public void testMapToObjectId_WithInvalidId_ShouldThrowException() {
        // Given
        String invalidId = "invalid-object-id";

        // When & Then
        assertThrows(IllegalArgumentException.class, () -> {
            ObjectIdMapper.mapToObjectId(invalidId, null);
        });
    }

    @Test
    public void testMapToObjectId_WithTooShortId_ShouldThrowException() {
        // Given - Valid hex but only 12 characters instead of 24
        String tooShortId = "507f1f77bcf8";

        // When & Then
        assertThrows(IllegalArgumentException.class, () -> {
            ObjectIdMapper.mapToObjectId(tooShortId, null);
        });
    }

    @Test
    public void testMapToObjectId_WithTooLongId_ShouldThrowException() {
        // Given - Valid hex but 25 characters instead of 24
        String tooLongId = "507f1f77bcf86cd799439011a";

        // When & Then
        assertThrows(IllegalArgumentException.class, () -> {
            ObjectIdMapper.mapToObjectId(tooLongId, null);
        });
    }

    @Test
    public void testMapToObjectId_WithNonHexCharacters_ShouldThrowException() {
        // Given - 24 characters but contains non-hex characters (g, h, z)
        String nonHexId = "507f1f77bcf86cd79943g01z";

        // When & Then
        assertThrows(IllegalArgumentException.class, () -> {
            ObjectIdMapper.mapToObjectId(nonHexId, null);
        });
    }

    @Test
    public void testMapToObjectId_WithUppercaseHex_ShouldWork() {
        // Given - Valid ObjectId with uppercase hex characters
        String uppercaseId = "507F1F77BCF86CD799439011";

        // When
        ObjectId result = ObjectIdMapper.mapToObjectId(uppercaseId, null);

        // Then
        assertNotNull(result);
        assertEquals(uppercaseId.toLowerCase(), result.toString());
    }

    @Test
    public void testMapToObjectId_WithMixedCaseHex_ShouldWork() {
        // Given - Valid ObjectId with mixed case hex characters
        String mixedCaseId = "507f1F77BcF86cD799439011";

        // When
        ObjectId result = ObjectIdMapper.mapToObjectId(mixedCaseId, null);

        // Then
        assertNotNull(result);
        assertEquals(mixedCaseId.toLowerCase(), result.toString());
    }

    @Test
    public void testMapToString_WithValidObjectId_ShouldReturnString() {
        // Given
        ObjectId objectId = new ObjectId("507f1f77bcf86cd799439011");

        // When
        String result = ObjectIdMapper.mapToString(objectId);

        // Then
        assertNotNull(result);
        assertEquals("507f1f77bcf86cd799439011", result);
    }

    @Test
    public void testMapToString_WithNullObjectId_ShouldReturnNull() {
        // When
        String result = ObjectIdMapper.mapToString(null);

        // Then
        assertNull(result);
    }

    @Test
    public void testMapToString_WithNewObjectId_ShouldReturnValidString() {
        // Given
        ObjectId objectId = new ObjectId();

        // When
        String result = ObjectIdMapper.mapToString(objectId);

        // Then
        assertNotNull(result);
        assertEquals(24, result.length()); // ObjectId string is always 24 characters
        assertEquals(objectId.toString(), result);
    }

    @Test
    public void testRoundTrip_StringToObjectIdAndBack_ShouldPreserveValue() {
        // Given
        String originalId = "507f1f77bcf86cd799439011";

        // When
        ObjectId objectId = ObjectIdMapper.mapToObjectId(originalId, null);
        String resultId = ObjectIdMapper.mapToString(objectId);

        // Then
        assertEquals(originalId, resultId);
    }

    @Test
    public void testRoundTrip_ObjectIdToStringAndBack_ShouldPreserveValue() {
        // Given
        ObjectId original = new ObjectId();

        // When
        String idString = ObjectIdMapper.mapToString(original);
        ObjectId result = ObjectIdMapper.mapToObjectId(idString, null);

        // Then
        assertEquals(original, result);
    }


}
