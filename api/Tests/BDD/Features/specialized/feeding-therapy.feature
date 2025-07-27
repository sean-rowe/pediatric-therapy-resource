Feature: Feeding Therapy Resources API Endpoints (FR-041)
  As a therapy professional
  I want comprehensive feeding therapy resources
  So that I can address feeding and swallowing difficulties

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"
    And I have feeding therapy training

  # GET /api/feeding-therapy/assessments
  @endpoint @feeding @assessment @not-implemented
  Scenario: Get feeding assessment tools
    When I send a GET request to "/api/feeding-therapy/assessments?age=toddler"
    Then the response status should be 200
    And the therapy response should contain array of:
      | field             | type    |
      | assessmentId      | string  |
      | name              | string  |
      | ageRange          | object  |
      | domains           | array   |
      | includesOralMotor | boolean |
      | includesSensory   | boolean |
      | parentVersion     | boolean |

  # POST /api/feeding-therapy/evaluation
  @endpoint @feeding @evaluation @not-implemented
  Scenario: Document feeding evaluation
    When I send a POST request to "/api/feeding-therapy/evaluation" with:
      | field              | value                        |
      | studentId          | student-123                  |
      | evaluationType     | comprehensive                |
      | medicalHistory     | {"reflux": true, "allergies": ["dairy"]} |
      | currentDiet        | {"textures": ["puree", "soft-mashed"]} |
      | oralMotorStatus    | {"jaw": "weak", "tongue": "limited-lateralization"} |
      | sensoryProfile     | {"oral-defensive": true}     |
      | mealObservation    | {"duration": 45, "intake": "minimal"} |
    Then the response status should be 201
    And evaluation report should be generated
    And treatment recommendations should be provided

  # GET /api/feeding-therapy/protocols/sos-approach
  @endpoint @feeding @sos @not-implemented
  Scenario: Access SOS feeding approach materials
    When I send a GET request to "/api/feeding-therapy/protocols/sos-approach"
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | hierarchySteps     | array  |
      | playActivities     | array  |
      | parentHandouts     | array  |
      | dataSheets         | array  |
      | foodChaining       | object |

  # POST /api/feeding-therapy/food-chaining
  @endpoint @feeding @food-chaining @not-implemented
  Scenario: Create food chaining plan
    When I send a POST request to "/api/feeding-therapy/food-chaining" with:
      | field         | value                         |
      | studentId     | student-123                   |
      | targetFood    | broccoli                      |
      | currentFoods  | ["chicken-nuggets", "crackers"] |
      | chainingPath  | ["green-beans", "asparagus", "broccoli"] |
      | timeframe     | 8-weeks                       |
    Then the response status should be 201
    And chaining steps should be created
    And progress tracking should be enabled

  # GET /api/feeding-therapy/oral-motor-exercises
  @endpoint @feeding @oral-motor @not-implemented
  Scenario: Get oral motor exercise library
    When I send a GET request to "/api/feeding-therapy/oral-motor-exercises?target=tongue-lateralization"
    Then the response status should be 200
    And exercises should include:
      | field            | type   |
      | exerciseName     | string |
      | videoDemo        | string |
      | pictureCards     | array  |
      | instructions     | string |
      | precautions      | array  |
      | progressionLevels| array  |

  # POST /api/feeding-therapy/texture-progression
  @endpoint @feeding @textures @not-implemented
  Scenario: Plan texture progression
    When I send a POST request to "/api/feeding-therapy/texture-progression" with:
      | field            | value                    |
      | studentId        | student-123              |
      | currentTexture   | puree                    |
      | targetTexture    | regular                  |
      | medicalClearance | true                     |
      | timeline         | 12-weeks                 |
    Then the response status should be 200
    And progression plan should include:
      | stage           | texture                  |
      | week-1-2        | smooth-puree             |
      | week-3-4        | thick-puree              |
      | week-5-6        | puree-with-lumps         |
      | week-7-8        | mashed                   |
      | week-9-10       | soft-solids              |
      | week-11-12      | regular-with-modifications|

  # GET /api/feeding-therapy/mealtime-strategies
  @endpoint @feeding @mealtime @not-implemented
  Scenario: Get mealtime behavior strategies
    When I send a GET request to "/api/feeding-therapy/mealtime-strategies?behavior=food-refusal"
    Then the response status should be 200
    And strategies should include:
      | strategy           | description              |
      | environmental-mods | Reduce distractions      |
      | scheduling         | Consistent meal times    |
      | presentation       | Small portions           |
      | reinforcement      | Non-food rewards         |

  # POST /api/feeding-therapy/parent-training
  @endpoint @feeding @parent-education @not-implemented
  Scenario: Create parent feeding program
    When I send a POST request to "/api/feeding-therapy/parent-training" with:
      | field          | value                    |
      | familyId       | family-123               |
      | trainingTopics | ["food-chaining", "mealtime-structure"] |
      | deliveryMethod | virtual                  |
      | language       | es                       |
    Then the response status should be 201
    And training modules should be created
    And materials should be in Spanish

  # GET /api/feeding-therapy/equipment-recommendations
  @endpoint @feeding @equipment @not-implemented
  Scenario: Get adaptive equipment recommendations
    When I send a GET request to "/api/feeding-therapy/equipment-recommendations?need=lip-closure"
    Then the response status should be 200
    And recommendations should include:
      | equipment        | purpose                  |
      | cut-out-cups     | Promotes lip closure     |
      | straw-hierarchy  | Oral motor development   |
      | textured-spoons  | Sensory input            |

  # POST /api/feeding-therapy/data-collection
  @endpoint @feeding @data @not-implemented
  Scenario: Record feeding session data
    When I send a POST request to "/api/feeding-therapy/data-collection" with:
      | field          | value                         |
      | studentId      | student-123                   |
      | sessionDate    | 2024-01-22                    |
      | mealType       | lunch                         |
      | duration       | 30                            |
      | foodsPresented | ["apple", "sandwich", "milk"] |
      | foodsAccepted  | ["milk"]                      |
      | behaviors      | ["pushing-plate", "turning-head"] |
      | techniques     | ["food-chaining", "positive-reinforcement"] |
    Then the response status should be 201
    And data should be tracked
    And progress graphs should update