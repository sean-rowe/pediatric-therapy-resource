Feature: Transition Planning API Endpoints (FR-037)
  As a therapy professional
  I want transition planning and vocational assessment tools
  So that I can prepare students for adulthood and employment

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # POST /api/transition-planning/assessments
  @endpoint @transition @assessment @not-implemented
  Scenario: Create transition assessment
    When I send a POST request to "/api/transition-planning/assessments" with:
      | field          | value                         |
      | studentId      | student-123                   |
      | assessmentType | comprehensive                 |
      | age            | 16                            |
      | areas          | ["vocational", "independent-living", "education"] |
      | includeFamily  | true                          |
    Then the response status should be 201
    And the response should contain:
      | field           | type   |
      | assessmentId    | string |
      | assessmentTools | array  |
      | timeline        | object |

  # POST /api/transition-planning/vocational-assessment
  @endpoint @transition @vocational @not-implemented
  Scenario: Conduct vocational interest assessment
    When I send a POST request to "/api/transition-planning/vocational-assessment" with:
      | field         | value                         |
      | studentId     | student-123                   |
      | interests     | ["technology", "animals", "art"] |
      | skills        | ["computer-use", "following-directions"] |
      | workSamples   | ["data-entry", "sorting"]     |
      | environment   | ["quiet", "structured"]       |
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | careerMatches      | array  |
      | strengthAreas      | array  |
      | accommodationNeeds | array  |
      | trainingPaths      | array  |

  # GET /api/transition-planning/life-skills-inventory
  @endpoint @transition @life-skills @not-implemented
  Scenario: Get life skills assessment tools
    When I send a GET request to "/api/transition-planning/life-skills-inventory"
    Then the response status should be 200
    And the response should contain:
      | field         | type  |
      | categories    | array |
      | assessments   | array |
      | checklistsq   | array |
    And categories should include daily living and community skills

  # POST /api/transition-planning/goals
  @endpoint @transition @goal-setting @not-implemented
  Scenario: Create transition goals
    When I send a POST request to "/api/transition-planning/goals" with:
      | field         | value                              |
      | studentId     | student-123                        |
      | goalArea      | employment                         |
      | specificGoal  | Obtain part-time job in retail     |
      | timeline      | By age 18                          |
      | steps         | ["job-skills-training", "resume-prep"] |
      | supports      | ["job-coach", "transportation"]    |
    Then the response status should be 201
    And goal should be added to transition plan
    And progress tracking should be initialized

  # GET /api/transition-planning/resources/self-advocacy
  @endpoint @transition @self-advocacy @not-implemented
  Scenario: Get self-advocacy resources
    When I send a GET request to "/api/transition-planning/resources/self-advocacy"
    Then the response status should be 200
    And resources should include:
      | type              |
      | rights-education  |
      | communication-scripts |
      | practice-scenarios |
      | video-models      |

  # POST /api/transition-planning/college-readiness
  @endpoint @transition @college @not-implemented
  Scenario: Assess college readiness
    When I send a POST request to "/api/transition-planning/college-readiness" with:
      | field              | value                    |
      | studentId          | student-123              |
      | academicSkills     | {"reading": 3, "writing": 2} |
      | executiveFunction  | {"organization": 2, "time-management": 2} |
      | socialSkills       | {"self-advocacy": 3}     |
      | accommodationNeeds | ["extended-time", "note-taker"] |
    Then the response status should be 200
    And readiness report should be generated
    And recommendations should be provided