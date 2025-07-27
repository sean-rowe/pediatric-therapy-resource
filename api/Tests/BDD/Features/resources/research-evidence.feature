Feature: Research and Evidence API Endpoints (FR-029)
  As a therapy professional
  I want access to research and evidence-based information
  So that I can make informed clinical decisions

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # GET /api/research/search
  @endpoint @research @search @not-implemented
  Scenario: Search research database
    When I send a GET request to "/api/research/search?topic=sensory-integration&years=2020-2024"
    Then the response status should be 200
    And results should include:
      | field          | type    |
      | papers         | array   |
      | totalResults   | number  |
      | evidenceLevels | object  |
      | filters        | object  |

  # GET /api/research/paper/{paperId}/summary
  @endpoint @research @summary @not-implemented
  Scenario: Get research paper summary
    When I send a GET request to "/api/research/paper/paper-123/summary"
    Then the response status should be 200
    And summary should contain:
      | section        | content                    |
      | objectives     | Study goals                |
      | methods        | Research design            |
      | keyFindings    | Main results               |
      | clinicalImpact | Practice implications      |
      | limitations    | Study constraints          |

  # GET /api/research/evidence-levels/{intervention}
  @endpoint @research @evidence @not-implemented
  Scenario: Check evidence level for intervention
    When I send a GET request to "/api/research/evidence-levels/weighted-vests"
    Then the response status should be 200
    And the response should show:
      | field           | value                     |
      | evidenceLevel   | "Level II"                |
      | studyCount      | 15                        |
      | recommendation  | "Moderate evidence"       |
      | lastUpdated     | "2024-01-15"              |
      | references      | array                     |

  # POST /api/research/citation/generate
  @endpoint @research @citation @not-implemented
  Scenario: Generate proper citations
    When I send a POST request to "/api/research/citation/generate" with:
      | field         | value                    |
      | paperIds      | ["paper-1", "paper-2"]   |
      | style         | APA                      |
      | format        | text                     |
    Then the response status should be 200
    And citations should be properly formatted
    And ready for documentation

  # GET /api/research/practice-guidelines/{condition}
  @endpoint @research @guidelines @not-implemented
  Scenario: Access best practice guidelines
    When I send a GET request to "/api/research/practice-guidelines/autism"
    Then the response status should be 200
    And guidelines should include:
      | source         | recommendation            | strength |
      | AOTA           | Sensory interventions     | Moderate |
      | ASHA           | Social communication      | Strong   |
      | AAP            | Early intervention        | Strong   |

  # POST /api/research/outcomes/track
  @endpoint @research @outcomes @not-implemented
  Scenario: Track intervention outcomes
    When I send a POST request to "/api/research/outcomes/track" with:
      | field           | value                    |
      | intervention    | "CIMT"                   |
      | measureUsed     | "QUEST"                  |
      | preScore        | 45                       |
      | postScore       | 62                       |
      | duration        | "6 weeks"                |
    Then the response status should be 201
    And outcome should be recorded
    And contribute to evidence base

  # GET /api/research/alerts/subscriptions
  @endpoint @research @alerts @not-implemented
  Scenario: Manage research alerts
    When I send a GET request to "/api/research/alerts/subscriptions"
    Then the response status should be 200
    And subscriptions should show:
      | topic              | frequency | lastAlert    |
      | sensory-processing | weekly    | 2024-01-15   |
      | telehealth-OT      | monthly   | 2024-01-01   |

  # POST /api/research/request-review
  @endpoint @research @review @not-implemented
  Scenario: Request research review
    When I send a POST request to "/api/research/request-review" with:
      | field         | value                         |
      | question      | "Effectiveness of hippotherapy for CP" |
      | urgency       | "routine"                     |
      | purpose       | "Treatment planning"          |
    Then the response status should be 201
    And request should be queued
    And estimated completion should be provided

  # GET /api/research/protocols/{protocolId}
  @endpoint @research @protocols @not-implemented
  Scenario: Access research-based protocols
    When I send a GET request to "/api/research/protocols/constraint-induced-therapy"
    Then the response status should be 200
    And protocol should include:
      | component      | details                   |
      | inclusion      | Criteria for use          |
      | procedures     | Step-by-step protocol     |
      | dosage         | Frequency and duration    |
      | modifications  | Adaptations allowed       |
      | measures       | Outcome tracking tools    |

  # GET /api/research/journal-club
  @endpoint @research @journal @not-implemented
  Scenario: Access journal club resources
    When I send a GET request to "/api/research/journal-club?month=2024-01"
    Then the response status should be 200
    And resources should include:
      | resource       | content                   |
      | featuredPaper  | Current month's selection |
      | discussion     | Guided questions          |
      | criticalAppraisal | Evaluation tools       |
      | ceuCredits     | Available credits         |