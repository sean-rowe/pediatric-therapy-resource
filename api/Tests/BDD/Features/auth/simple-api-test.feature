@api @smoke
Feature: Simple API Test
    As a developer
    I want to test basic API connectivity
    So that I can verify the test setup is working

    Scenario: API responds to health check
        Given the API is available
        When I send a GET request to "/health"
        Then the response status should be 200