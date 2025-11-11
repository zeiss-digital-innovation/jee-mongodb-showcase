package de.zeiss.mongodbws.geoservice.rest;

import jakarta.ws.rs.container.ContainerRequestContext;
import jakarta.ws.rs.container.ContainerResponseContext;
import jakarta.ws.rs.core.MultivaluedHashMap;
import jakarta.ws.rs.core.MultivaluedMap;
import jakarta.ws.rs.core.Response;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.Arguments;
import org.junit.jupiter.params.provider.MethodSource;
import org.mockito.ArgumentCaptor;

import java.util.stream.Stream;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.Mockito.*;

/**
 * Test class for CorsFilter.
 */
class CorsFilterTest {

    static Stream<Arguments> allowedOriginProvider() {
        return Stream.of(
                Arguments.of("http://localhost:3000"),
                Arguments.of("https://localhost:3000"),
                Arguments.of("http://127.0.0.1:1234"),
                Arguments.of("https://127.0.0.1:5678")
        );
    }

    static Stream<Arguments> notAllowedOriginProvider() {
        return Stream.of(
                Arguments.of("http://evil.com"),
                Arguments.of("https://evil.com"),
                Arguments.of("http://127.0.0.2:1234"),
                Arguments.of("https://127.0.0.2:5678"),
                Arguments.of("http://localhostevil.com")
        );
    }

    /**
     * Test Preflight OPTIONS request from allowed origin. Should set CORS headers and abort with OK response.
     *
     * @param allowedOrigin The allowed origin to test.
     */
    @ParameterizedTest(name = "Allowed Origin #{index}: allowedOrigin={0}")
    @MethodSource("allowedOriginProvider")
    void testPreflightAllowedOrigin(String allowedOrigin) {
        CorsFilter filter = new CorsFilter();
        ContainerRequestContext reqCtx = mock(ContainerRequestContext.class);
        when(reqCtx.getHeaderString("Origin")).thenReturn(allowedOrigin);
        when(reqCtx.getMethod()).thenReturn("OPTIONS");

        // Creates an ArgumentCaptor for the Response class.
        // This allows to capture and inspect the Response object that is passed to abortWith().
        ArgumentCaptor<Response> responseCaptor = ArgumentCaptor.forClass(Response.class);
        // Tells Mockito: when abortWith is called on the mock reqCtx, do nothing (don’t actually abort the request),
        // but capture the argument (the Response object) that is passed to abortWith using responseCaptor.
        doNothing().when(reqCtx).abortWith(responseCaptor.capture());
        // Now when filter.filter(reqCtx) is called, if it calls reqCtx.abortWith(response),
        // the Response object will be captured.
        assertDoesNotThrow(() -> filter.filter(reqCtx));

        // This lets us retrieve the captured Response object with responseCaptor.getValue()
        // to assert its properties (status, headers, etc.).
        Response response = responseCaptor.getValue();
        assertEquals(200, response.getStatus());
        assertEquals(allowedOrigin, response.getHeaderString(CorsFilter.HDR_ALLOW_ORIGIN));
        assertEquals("true", response.getHeaderString(CorsFilter.HDR_ALLOW_CREDENTIALS));
    }

    /**
     * Test Preflight OPTIONS request from disallowed origin. Should not set CORS headers.
     *
     * @param notAllowedOrigin The disallowed origin to test.
     */
    @ParameterizedTest(name = "Now allowed Origin #{index}: notAllowedOrigin={0}")
    @MethodSource("notAllowedOriginProvider")
    void testPreflightDisallowedOrigin(String notAllowedOrigin) {
        CorsFilter filter = new CorsFilter();
        ContainerRequestContext reqCtx = mock(ContainerRequestContext.class);
        when(reqCtx.getHeaderString("Origin")).thenReturn(notAllowedOrigin);
        when(reqCtx.getMethod()).thenReturn("OPTIONS");

        ArgumentCaptor<Response> responseCaptor = ArgumentCaptor.forClass(Response.class);
        doNothing().when(reqCtx).abortWith(responseCaptor.capture());

        assertDoesNotThrow(() -> filter.filter(reqCtx));

        // Should not set Access-Control-Allow-Origin header
        Response response = responseCaptor.getValue();
        assertNull(response.getHeaderString(CorsFilter.HDR_ALLOW_ORIGIN));
        assertNull(response.getHeaderString(CorsFilter.HDR_ALLOW_CREDENTIALS));
    }

    /**
     * Test Preflight OPTIONS request with null Origin. Should not set CORS headers.
     */
    @Test
    void testPreflightDisallowedNullOrigin() {
        CorsFilter filter = new CorsFilter();
        ContainerRequestContext reqCtx = mock(ContainerRequestContext.class);
        when(reqCtx.getHeaderString("Origin")).thenReturn(null);
        when(reqCtx.getMethod()).thenReturn("OPTIONS");

        ArgumentCaptor<Response> responseCaptor = ArgumentCaptor.forClass(Response.class);
        doNothing().when(reqCtx).abortWith(responseCaptor.capture());

        assertDoesNotThrow(() -> filter.filter(reqCtx));

        // Should not set Access-Control-Allow-Origin header
        Response response = responseCaptor.getValue();
        assertNull(response.getHeaderString(CorsFilter.HDR_ALLOW_ORIGIN));
        assertNull(response.getHeaderString(CorsFilter.HDR_ALLOW_CREDENTIALS));
    }

    /**
     * Test Preflight request with non-OPTIONS method. Should not abort the request.
     */
    @Test
    void testPreflightNonOptionsMethod() {
        CorsFilter filter = new CorsFilter();
        ContainerRequestContext reqCtx = mock(ContainerRequestContext.class);
        when(reqCtx.getHeaderString("Origin")).thenReturn("http://localhost:3000");
        when(reqCtx.getMethod()).thenReturn("GET");

        // Since it's not an OPTIONS request, abortWith should not be called.
        doNothing().when(reqCtx).abortWith(any(Response.class));

        assertDoesNotThrow(() -> filter.filter(reqCtx));

        // Verify that abortWith was never called
        verify(reqCtx, never()).abortWith(any(Response.class));
    }

    /**
     * Test Response filter from allowed origin. Should set CORS headers.
     *
     * @param allowedOrigin The allowed origin to test.
     */
    @ParameterizedTest(name = "Allowed Origin #{index}: allowedOrigin={0}")
    @MethodSource("allowedOriginProvider")
    void testResponseAllowedOrigin(String allowedOrigin) {
        CorsFilter filter = new CorsFilter();
        ContainerRequestContext reqCtx = mock(ContainerRequestContext.class);
        ContainerResponseContext respCtx = mock(ContainerResponseContext.class);

        when(reqCtx.getHeaderString("Origin")).thenReturn(allowedOrigin);

        // Use a real map for headers
        MultivaluedMap<String, Object> headers = new MultivaluedHashMap<>();
        when(respCtx.getHeaders()).thenReturn(headers);

        assertDoesNotThrow(() -> filter.filter(reqCtx, respCtx));

        String originHeader = (String) headers.getFirst(CorsFilter.HDR_ALLOW_ORIGIN);
        String credentialsHeader = (String) headers.getFirst(CorsFilter.HDR_ALLOW_CREDENTIALS);

        assertEquals(allowedOrigin, originHeader);
        assertEquals("true", credentialsHeader);
    }

    /**
     * Test Response filter from disallowed origin. Should not set CORS headers.
     *
     * @param notAllowedOrigin The disallowed origin to test.
     */
    @ParameterizedTest(name = "Now allowed Origin #{index}: notAllowedOrigin={0}")
    @MethodSource("notAllowedOriginProvider")
    void testResponseDisallowedOrigin(String notAllowedOrigin) {
        CorsFilter filter = new CorsFilter();
        ContainerRequestContext reqCtx = mock(ContainerRequestContext.class);
        ContainerResponseContext respCtx = mock(ContainerResponseContext.class);

        when(reqCtx.getHeaderString("Origin")).thenReturn(notAllowedOrigin);

        MultivaluedMap<String, Object> headers = new MultivaluedHashMap<>();
        when(respCtx.getHeaders()).thenReturn(headers);

        assertDoesNotThrow(() -> filter.filter(reqCtx, respCtx));

        assertNull(headers.get(CorsFilter.HDR_ALLOW_ORIGIN));
        assertNull(headers.get(CorsFilter.HDR_ALLOW_CREDENTIALS));
    }

    /**
     * Test Response filter with null Origin. Should not set CORS headers.
     */
    @Test
    void testResponseDisallowedNullOrigin() {
        CorsFilter filter = new CorsFilter();
        ContainerRequestContext reqCtx = mock(ContainerRequestContext.class);
        ContainerResponseContext respCtx = mock(ContainerResponseContext.class);

        when(reqCtx.getHeaderString("Origin")).thenReturn(null);

        MultivaluedMap<String, Object> headers = new MultivaluedHashMap<>();
        when(respCtx.getHeaders()).thenReturn(headers);

        assertDoesNotThrow(() -> filter.filter(reqCtx, respCtx));

        assertNull(headers.get(CorsFilter.HDR_ALLOW_ORIGIN));
        assertNull(headers.get(CorsFilter.HDR_ALLOW_CREDENTIALS));
    }
}