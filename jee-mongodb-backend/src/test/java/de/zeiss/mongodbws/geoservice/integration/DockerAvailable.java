package de.zeiss.mongodbws.geoservice.integration;

import org.junit.jupiter.api.extension.ConditionEvaluationResult;
import org.junit.jupiter.api.extension.ExecutionCondition;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.extension.ExtensionContext;
import org.testcontainers.DockerClientFactory;

import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;
import java.util.logging.Logger;

/**
 * Custom JUnit 5 annotation to conditionally enable tests based on Docker availability.
 * Tests annotated with @DockerAvailable will only run if Docker is available on the host system.
 */
@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.TYPE)
@ExtendWith(DockerAvailable.DockerAvailableCondition.class)
public @interface DockerAvailable {

    class DockerAvailableCondition implements ExecutionCondition {

        private static final Logger LOGGER = Logger.getLogger(DockerAvailableCondition.class.getName());

        @Override
        public ConditionEvaluationResult evaluateExecutionCondition(ExtensionContext context) {
            if (DockerClientFactory.instance().isDockerAvailable()) {
                return ConditionEvaluationResult.enabled("Docker is available");
            } else {
                LOGGER.warning("Docker is not available - skipping tests that require Testcontainers");
                return ConditionEvaluationResult.disabled("Docker is not available - skipping tests that require Testcontainers");
            }
        }
    }
}
