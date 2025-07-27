Feature: Free Resources API Endpoints (FR-021)
  As a platform visitor or user
  I want access to free resources and samples
  So that I can try the platform and access basic materials

  Background:
    Given the API is available

  # GET /api/resources/free/weekly
  @endpoint @free @weekly @not-implemented
  Scenario: Access weekly free resources
    When I send a GET request to "/api/resources/free/weekly"
    Then the response status should be 200
    And the response should contain:
      | field            | type   |
      | weekOf           | string |
      | resources        | array  |
      | downloadLimit    | number |
      | nextRotation     | string |
      | requiresAccount  | boolean |

  # GET /api/resources/free/samples/{resourceId}
  @endpoint @free @samples @not-implemented
  Scenario: Download sample pages from paid resource
    When I send a GET request to "/api/resources/free/samples/res-premium-123"
    Then the response status should be 200
    And the response should contain:
      | field          | type    |
      | samplePages    | array   |
      | watermarked    | boolean |
      | fullVersion    | object  |
      | purchaseLink   | string  |

  # POST /api/resources/free/newsletter-signup
  @endpoint @free @newsletter @not-implemented
  Scenario: Sign up for newsletter with free resources
    When I send a POST request to "/api/resources/free/newsletter-signup" with:
      | field           | value                    |
      | email           | parent@email.com         |
      | preferences     | ["OT", "preschool"]      |
      | frequency       | weekly                   |
      | includeFreebies | true                     |
    Then the response status should be 201
    And welcome email should be sent
    And bonus resources should be unlocked

  # GET /api/resources/free/educational-handouts
  @endpoint @free @handouts @not-implemented
  Scenario: Browse free educational handouts
    When I send a GET request to "/api/resources/free/educational-handouts?topic=development"
    Then the response status should be 200
    And handouts should include:
      | topic               | audience        |
      | milestones          | parents         |
      | warning signs       | educators       |
      | activity ideas      | caregivers      |
      | referral guidelines | professionals   |

  # GET /api/resources/free/birthday-special
  @endpoint @free @birthday @not-implemented
  Scenario: Access birthday month special resources
    Given I am authenticated as "user@email.com"
    And my birthday month is January
    And current month is January
    When I send a GET request to "/api/resources/free/birthday-special"
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | specialResources   | array  |
      | bonusCredits       | number |
      | exclusiveContent   | array  |
      | validUntil         | string |

  # POST /api/resources/free/first-time-bonus
  @endpoint @free @firsttime @not-implemented
  Scenario: Claim first-time user bonus
    Given I am a new authenticated user
    When I send a POST request to "/api/resources/free/first-time-bonus" with:
      | field         | value              |
      | claimCode     | WELCOME2024        |
      | userType      | therapist          |
    Then the response status should be 200
    And bonus should include:
      | item            | quantity |
      | freeResources   | 20       |
      | aiCredits       | 5        |
      | trialDays       | 14       |

  # GET /api/resources/free/trial-resources
  @endpoint @free @trial @not-implemented
  Scenario: Access resources during free trial
    Given I am in free trial period
    When I send a GET request to "/api/resources/free/trial-resources"
    Then the response status should be 200
    And the response should contain:
      | field           | type    |
      | trialResources  | array   |
      | daysRemaining   | number  |
      | fullAccessDemo  | boolean |
      | conversionOffer | object  |

  # GET /api/resources/free/community-contributions
  @endpoint @free @community @not-implemented
  Scenario: Access community-contributed free resources
    When I send a GET request to "/api/resources/free/community-contributions?sort=popular"
    Then the response status should be 200
    And resources should include:
      | field         | type    |
      | contributor   | string  |
      | downloads     | number  |
      | rating        | number  |
      | license       | string  |

  # POST /api/resources/free/share-to-unlock
  @endpoint @free @social @not-implemented
  Scenario: Unlock resources by sharing
    Given I am authenticated
    When I send a POST request to "/api/resources/free/share-to-unlock" with:
      | field         | value                    |
      | platform      | facebook                 |
      | resourceId    | free-pack-123            |
      | verified      | true                     |
    Then the response status should be 200
    And resource should be unlocked
    And share should be tracked

  # GET /api/resources/free/preview-library
  @endpoint @free @preview @not-implemented
  Scenario: Browse preview library
    When I send a GET request to "/api/resources/free/preview-library?limit=10"
    Then the response status should be 200
    And each resource should contain:
      | field            | type    |
      | previewType      | string  |
      | previewDuration  | number  |
      | watermarked      | boolean |
      | upgradePrompt    | object  |