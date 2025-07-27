Feature: Evidence-Based Protocol Libraries API Endpoints (FR-023, FR-038)
  As a therapy professional
  I want access to evidence-based therapy protocols
  So that I can implement proven interventions with fidelity

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # GET /api/protocols
  @endpoint @protocols @catalog @not-implemented
  Scenario: List available therapy protocols
    When I send a GET request to "/api/protocols?approach=PROMPT&certification=required"
    Then the response status should be 200
    And the therapy response should contain array of:
      | field              | type    |
      | protocolId         | string  |
      | name               | string  |
      | approach           | string  |
      | evidenceLevel      | string  |
      | certificationReq   | boolean |
      | targetPopulation   | array   |
      | outcomes           | array   |

  # GET /api/protocols/{protocolId}
  @endpoint @protocols @details @not-implemented
  Scenario: Get PROMPT protocol details
    Given protocol "prompt-level-1" exists
    When I send a GET request to "/api/protocols/prompt-level-1"
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | protocolSteps      | array  |
      | cuingHierarchy     | object |
      | targetSelection    | object |
      | dataCollection     | object |
      | fidelityChecklist  | array  |
      | videoExamples      | array  |

  # POST /api/protocols/{protocolId}/implement
  @endpoint @protocols @implementation @not-implemented
  Scenario: Start protocol implementation
    When I send a POST request to "/api/protocols/prompt-level-1/implement" with:
      | field         | value                    |
      | studentId     | student-123              |
      | targetSounds  | ["p", "b", "m"]          |
      | frequency     | 3x-weekly                |
      | settingType   | individual               |
    Then the response status should be 201
    And implementation plan should be created
    And fidelity tracking should be enabled

  # GET /api/protocols/dir-floortime/activities
  @endpoint @protocols @dir @not-implemented
  Scenario: Get DIR/Floortime activities
    When I send a GET request to "/api/protocols/dir-floortime/activities?functionalLevel=3"
    Then the response status should be 200
    And activities should be appropriate for functional level
    And include parent coaching materials

  # POST /api/protocols/hanen/parent-training
  @endpoint @protocols @hanen @not-implemented
  Scenario: Access Hanen parent program
    When I send a POST request to "/api/protocols/hanen/parent-training" with:
      | field          | value                    |
      | programType    | it-takes-two             |
      | familyId       | family-123               |
      | language       | es                       |
    Then the response status should be 200
    And parent materials should be provided
    And training modules should be accessible

  # GET /api/protocols/social-thinking/curriculum
  @endpoint @protocols @social-thinking @not-implemented
  Scenario: Get Social Thinking curriculum
    When I send a GET request to "/api/protocols/social-thinking/curriculum?ageGroup=elementary"
    Then the response status should be 200
    And curriculum should include:
      | component          |
      | vocabulary-lessons |
      | thinking-sheets    |
      | video-examples     |
      | generalization     |

  # POST /api/protocols/zones-of-regulation/setup
  @endpoint @protocols @zones @not-implemented
  Scenario: Set up Zones of Regulation
    When I send a POST request to "/api/protocols/zones-of-regulation/setup" with:
      | field         | value                         |
      | classroomId   | classroom-456                 |
      | studentIds    | ["student-123", "student-456"]|
      | customization | {"colors": "standard"}        |
    Then the response status should be 201
    And visual materials should be generated
    And tracking tools should be created

  # GET /api/protocols/handwriting-without-tears
  @endpoint @protocols @hwt @not-implemented
  Scenario: Access HWT curriculum
    When I send a GET request to "/api/protocols/handwriting-without-tears?grade=kindergarten"
    Then the response status should be 200
    And materials should include:
      | resource           |
      | letter-formations  |
      | practice-pages     |
      | multisensory-tools |
      | assessment-tools   |

  # POST /api/protocols/{protocolId}/fidelity-check
  @endpoint @protocols @fidelity @not-implemented
  Scenario: Submit fidelity checklist
    Given I'm implementing "prompt-level-1"
    When I send a POST request to "/api/protocols/prompt-level-1/fidelity-check" with:
      | field         | value                         |
      | sessionId     | session-789                   |
      | checklistItems| [{"item": "cues-hierarchy", "implemented": true}] |
      | videoReview   | video-url                     |
      | selfRating    | 4                             |
    Then the response status should be 200
    And fidelity score should be calculated
    And feedback should be provided

  # GET /api/protocols/certification-tracking
  @endpoint @protocols @certification @not-implemented
  Scenario: Track protocol certifications
    When I send a GET request to "/api/protocols/certification-tracking"
    Then the response status should be 200
    And the response should contain:
      | field              | type  |
      | activeCertifications | array |
      | expiringCertifications | array |
      | availableTrainings | array |
      | ceuCredits         | object |