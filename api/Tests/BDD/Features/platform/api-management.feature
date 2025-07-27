Feature: API Management and Developer Tools
  As a developer or third-party integrator
  I want API management tools
  So that I can integrate with the UPTRMS platform

  Background:
    Given the API is available
    And I am authenticated as "developer@company.com"

  # POST /api/developer/apps
  @endpoint @api-management @apps @not-implemented
  Scenario: Register new application
    When I send a POST request to "/api/developer/apps" with:
      | field          | value                         |
      | appName        | Therapy Assistant Pro         |
      | description    | Mobile app for therapists     |
      | redirectUris   | ["https://app.example.com/callback"] |
      | scopes         | ["read:resources", "write:sessions"] |
      | webhookUrl     | https://app.example.com/webhook |
    Then the response status should be 201
    And the response should contain:
      | field         | type   |
      | appId         | string |
      | clientId      | string |
      | clientSecret  | string |
      | apiKey        | string |

  # GET /api/developer/apps/{appId}/keys
  @endpoint @api-management @keys @not-implemented
  Scenario: Manage API keys
    Given I have app "app-123"
    When I send a GET request to "/api/developer/apps/app-123/keys"
    Then the response status should be 200
    And the response should contain:
      | field      | type  |
      | keys       | array |
      | rateLimit  | object|
      | usage      | object|

  # PUT /api/developer/apps/{appId}/rate-limits
  @endpoint @api-management @rate-limiting @not-implemented
  Scenario: Configure rate limits
    When I send a PUT request to "/api/developer/apps/app-123/rate-limits" with:
      | field           | value    |
      | requestsPerHour | 10000    |
      | burstLimit      | 100      |
      | concurrentLimit | 50       |
    Then the response status should be 200
    And rate limits should be updated

  # GET /api/developer/usage
  @endpoint @api-management @usage @not-implemented
  Scenario: Get API usage statistics
    When I send a GET request to "/api/developer/usage?period=month"
    Then the response status should be 200
    And the response should contain:
      | field            | type   |
      | totalRequests    | number |
      | successfulCalls  | number |
      | errorRate        | number |
      | endpointBreakdown| object |
      | quotaRemaining   | number |

  # GET /api/developer/documentation
  @endpoint @api-management @docs @not-implemented
  Scenario: Access API documentation
    When I send a GET request to "/api/developer/documentation"
    Then the response status should be 200
    And the response should contain:
      | field          | type   |
      | openApiSpec    | object |
      | interactiveDocs| string |
      | sdkLinks       | object |
      | changelog      | array  |

  # POST /api/developer/webhooks
  @endpoint @api-management @webhooks @not-implemented
  Scenario: Configure webhooks
    When I send a POST request to "/api/developer/webhooks" with:
      | field      | value                              |
      | url        | https://app.example.com/webhook    |
      | events     | ["resource.created", "session.completed"] |
      | secret     | webhook-secret-key                 |
      | active     | true                               |
    Then the response status should be 201
    And webhook should be registered
    And test event should be sent

  # GET /api/developer/sandbox
  @endpoint @api-management @sandbox @not-implemented
  Scenario: Access sandbox environment
    When I send a GET request to "/api/developer/sandbox"
    Then the response status should be 200
    And the response should contain:
      | field           | type   |
      | sandboxUrl      | string |
      | testCredentials | object |
      | sampleData      | object |
      | resetEndpoint   | string |