/**
 * This file is part of a demo application showing MongoDB usage with Morphia library.
 * <p>
 * Copyright (C) 2025 Carl Zeiss Digital Innovation GmbH
 */
package de.zeiss.mongodbws.geoservice.rest;

import jakarta.ws.rs.Priorities;
import jakarta.ws.rs.container.*;
import jakarta.ws.rs.core.Response;
import jakarta.ws.rs.ext.Provider;

import java.io.IOException;
import java.util.regex.Pattern;

/**
 * CORS Filter: allows localhost calls regardless of the port and answers Preflight requests.
 *
 * @author AI Generated
 */
@Provider
@PreMatching
@jakarta.annotation.Priority(Priorities.AUTHENTICATION)
public class CorsFilter implements ContainerRequestFilter, ContainerResponseFilter {

    private static final Pattern LOCALHOST_ORIGIN = Pattern.compile("^https?://(?:localhost|127\\.0\\.0\\.1)(?::\\d+)?$");
    private static final String HEADER_ORIGIN = "Origin";
    private static final String HDR_ALLOW_METHODS = "Access-Control-Allow-Methods";
    private static final String HDR_ALLOW_HEADERS = "Access-Control-Allow-Headers";
    static final String HDR_ALLOW_ORIGIN = "Access-Control-Allow-Origin";
    static final String HDR_ALLOW_CREDENTIALS = "Access-Control-Allow-Credentials";
    private static final String HDR_MAX_AGE = "Access-Control-Max-Age";

    @Override
    public void filter(ContainerRequestContext requestContext) throws IOException {
        String origin = requestContext.getHeaderString(HEADER_ORIGIN);
        String allowedOrigin = origin != null && LOCALHOST_ORIGIN.matcher(origin).matches() ? origin : null;

        // Preflight request handling
        if ("OPTIONS".equalsIgnoreCase(requestContext.getMethod())) {
            Response.ResponseBuilder rb = Response.ok();
            if (allowedOrigin != null) {
                rb.header(HDR_ALLOW_ORIGIN, allowedOrigin)
                        .header(HDR_ALLOW_CREDENTIALS, "true");
            }
            rb.header(HDR_ALLOW_METHODS, "GET, POST, PUT, DELETE, OPTIONS")
                    .header(HDR_ALLOW_HEADERS, "Origin, Content-Type, Accept, Authorization")
                    .header(HDR_MAX_AGE, "3600");
            requestContext.abortWith(rb.build());
        }
    }

    @Override
    public void filter(ContainerRequestContext requestContext, ContainerResponseContext responseContext) throws IOException {
        String origin = requestContext.getHeaderString(HEADER_ORIGIN);
        if (origin != null && LOCALHOST_ORIGIN.matcher(origin).matches()) {
            responseContext.getHeaders().putSingle(HDR_ALLOW_ORIGIN, origin);
            responseContext.getHeaders().putSingle(HDR_ALLOW_CREDENTIALS, "true");
            responseContext.getHeaders().putSingle(HDR_ALLOW_METHODS, "GET, POST, PUT, DELETE, OPTIONS");
            responseContext.getHeaders().putSingle(HDR_ALLOW_HEADERS, "Origin, Content-Type, Accept, Authorization");
            responseContext.getHeaders().putSingle(HDR_MAX_AGE, "3600");
        }
    }
}