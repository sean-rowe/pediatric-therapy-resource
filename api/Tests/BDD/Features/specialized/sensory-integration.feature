Feature: Sensory Integration Resources API Endpoints (FR-017, FR-040)
  As a therapy professional
  I want comprehensive sensory integration resources
  So that I can address sensory processing needs effectively

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # GET /api/sensory/assessments
  @endpoint @sensory @assessment @not-implemented
  Scenario: Get sensory assessment tools
    When I send a GET request to "/api/sensory/assessments"
    Then the response status should be 200
    And the therapy response should contain array of:
      | field               | type    |
      | assessmentId        | string  |
      | name                | string  |
      | ageRange            | object  |
      | sensoryDomains      | array   |
      | administrationTime  | number  |
      | parentQuestionnaire | boolean |

  # POST /api/sensory/profile
  @endpoint @sensory @profile @not-implemented
  Scenario: Create sensory profile
    When I send a POST request to "/api/sensory/profile" with:
      | field              | value                         |
      | studentId          | student-123                   |
      | assessmentData     | {"tactile": "over-responsive", "vestibular": "under-responsive"} |
      | behavioralIndicators| ["avoids-textures", "seeks-movement"] |
      | functionalImpact   | ["difficulty-with-dressing", "poor-attention"] |
      | environmentalFactors| ["fluorescent-lights", "classroom-noise"] |
    Then the response status should be 201
    And sensory profile should be created
    And recommendations should be generated

  # POST /api/sensory/diet-builder
  @endpoint @sensory @diet @not-implemented
  Scenario: Build individualized sensory diet
    When I send a POST request to "/api/sensory/diet-builder" with:
      | field              | value                        |
      | studentId          | student-123                  |
      | sensoryNeeds       | {"proprioceptive": "high", "vestibular": "moderate"} |
      | settingType        | classroom                    |
      | duration           | full-school-day              |
      | equipmentAvailable | ["therapy-ball", "weighted-lap-pad"] |
      | breakFrequency     | every-hour                   |
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | morningActivities  | array  |
      | middayActivities   | array  |
      | afternoonActivities| array  |
      | transitionSupports | array  |
      | visualSchedule     | string |
      | parentVersion      | string |

  # GET /api/sensory/activities/proprioceptive
  @endpoint @sensory @activities @not-implemented
  Scenario: Get proprioceptive activities
    When I send a GET request to "/api/sensory/activities/proprioceptive?intensity=heavy-work"
    Then the response status should be 200
    And activities should include:
      | activity           | equipment              | duration |
      | wall-pushups       | none                   | 2-3-min  |
      | chair-pushups      | classroom-chair        | 1-2-min  |
      | carrying-books     | heavy-books            | 5-min    |
      | theraband-pulls    | theraband              | 3-min    |

  # GET /api/sensory/movement-breaks
  @endpoint @sensory @movement @not-implemented
  Scenario: Get movement break activities
    When I send a GET request to "/api/sensory/movement-breaks?setting=classroom&duration=5"
    Then the response status should be 200
    And activities should be classroom-appropriate
    And include video demonstrations

  # POST /api/sensory/room-design
  @endpoint @sensory @environment @not-implemented
  Scenario: Design sensory room/space
    When I send a POST request to "/api/sensory/room-design" with:
      | field         | value                         |
      | spaceType     | corner-of-classroom           |
      | dimensions    | {"width": 6, "length": 6}     |
      | budget        | 500                           |
      | primaryNeeds  | ["calming", "organizing"]     |
      | studentAges   | [5, 6, 7]                     |
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | layoutDiagram      | string |
      | equipmentList      | array  |
      | safetyChecklist    | array  |
      | visualGuide        | string |
      | shoppingList       | array  |
      | totalCost          | number |

  # GET /api/sensory/alert-program
  @endpoint @sensory @self-regulation @not-implemented
  Scenario: Access Alert Program resources
    When I send a GET request to "/api/sensory/alert-program?level=elementary"
    Then the response status should be 200
    And resources should include:
      | resource           | format                |
      | engine-speeds      | visual-cards          |
      | body-check-in      | worksheet             |
      | strategy-cards     | printable             |
      | parent-guide       | pdf                   |

  # POST /api/sensory/data-collection
  @endpoint @sensory @tracking @not-implemented
  Scenario: Track sensory responses
    When I send a POST request to "/api/sensory/data-collection" with:
      | field            | value                       |
      | studentId        | student-123                 |
      | dateTime         | 2024-01-22T10:30:00Z        |
      | triggerEvent     | fire-drill                  |
      | sensoryResponse  | covered-ears-and-cried      |
      | duration         | 15-minutes                  |
      | recoveryStrategy | deep-pressure-vest          |
      | recoveryTime     | 10-minutes                  |
    Then the response status should be 201
    And pattern analysis should be updated
    And trigger identification should improve

  # GET /api/sensory/equipment-catalog
  @endpoint @sensory @equipment @not-implemented
  Scenario: Browse sensory equipment catalog
    When I send a GET request to "/api/sensory/equipment-catalog?category=oral-motor"
    Then the response status should be 200
    And catalog should include:
      | item              | price  | purpose              |
      | chewy-tubes       | 15.99  | oral-input           |
      | vibrating-tools   | 24.99  | oral-stimulation     |
      | textured-strips   | 12.99  | tactile-input        |

  # POST /api/sensory/home-program
  @endpoint @sensory @home @not-implemented
  Scenario: Create sensory home program
    When I send a POST request to "/api/sensory/home-program" with:
      | field           | value                      |
      | studentId       | student-123                |
      | homeEnvironment | apartment                  |
      | familySchedule  | both-parents-work          |
      | equipment       | ["yoga-ball", "weighted-blanket"] |
      | duration        | 4-weeks                    |
    Then the response status should be 201
    And home program should include:
      | component         | details                   |
      | morning-routine   | 10-minute activities      |
      | after-school      | 15-minute activities      |
      | bedtime-routine   | calming strategies        |
      | weekend-activities| longer sensory experiences|