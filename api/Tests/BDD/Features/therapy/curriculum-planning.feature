Feature: Curriculum Planning API Endpoints (FR-031)
  As a therapy professional
  I want long-term therapy planning and curriculum mapping tools
  So that I can create comprehensive therapy programs

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # POST /api/curriculum/year-plan/create
  @endpoint @curriculum @yearplan @not-implemented
  Scenario: Create full year therapy plan
    When I send a POST request to "/api/curriculum/year-plan/create" with:
      | field           | value                         |
      | studentId       | student-123                   |
      | schoolYear      | "2024-2025"                   |
      | frequency       | "2x weekly"                   |
      | domains         | ["fine-motor", "visual-motor"]|
      | holidays        | "school-calendar"             |
    Then the response status should be 201
    And plan should include:
      | component       | details                       |
      | totalSessions   | 72                            |
      | monthlyThemes   | Aligned with curriculum       |
      | progressChecks  | Quarterly                     |
      | resourceMapping | Suggested materials           |

  # GET /api/curriculum/standards/alignment
  @endpoint @curriculum @standards @not-implemented
  Scenario: Align therapy goals with educational standards
    When I send a GET request to "/api/curriculum/standards/alignment?state=CA&grade=K"
    Then the response status should be 200
    And alignment should show:
      | standard        | therapyGoals                  | resources    |
      | K.CC.A.3        | Number recognition 1-10       | ["res-456"]  |
      | K.MD.A.1        | Descriptive attributes         | ["res-789"]  |
      | K.G.A.2         | Shape identification          | ["res-012"]  |

  # POST /api/curriculum/spiral/generate
  @endpoint @curriculum @spiral @not-implemented
  Scenario: Generate spiral curriculum
    When I send a POST request to "/api/curriculum/spiral/generate" with:
      | field           | value                         |
      | skills          | ["cutting", "tracing", "copying"] |
      | duration        | "semester"                    |
      | complexity      | "progressive"                 |
      | reviewCycles    | 3                             |
    Then the response status should be 200
    And curriculum should show:
      | week  | newSkill          | reviewSkills        | complexity |
      | 1     | straight lines    | none                | basic      |
      | 4     | curved lines      | straight lines      | moderate   |
      | 8     | simple shapes     | lines, curves       | complex    |

  # GET /api/curriculum/benchmarks/{studentId}
  @endpoint @curriculum @benchmarks @not-implemented
  Scenario: View progress benchmarks
    When I send a GET request to "/api/curriculum/benchmarks/student-123?period=year"
    Then the response status should be 200
    And benchmarks should include:
      | milestone       | targetDate    | currentStatus | projection |
      | Write name      | November      | on-track      | 95%        |
      | Cut shapes      | January       | ahead         | 100%       |
      | Tie shoes       | March         | delayed       | 70%        |

  # POST /api/curriculum/resources/map
  @endpoint @curriculum @mapping @not-implemented
  Scenario: Map resources to curriculum weeks
    When I send a POST request to "/api/curriculum/resources/map" with:
      | field           | value                         |
      | curriculumId    | curr-2024-123                 |
      | mappingType     | "automatic"                   |
      | constraints     | {"max-per-week": 5}           |
    Then the response status should be 200
    And mapping should provide:
      | week  | theme              | resources           | skills      |
      | 1     | School Tools       | ["res-1", "res-2"]  | ["grip"]    |
      | 2     | Fall Leaves        | ["res-3", "res-4"]  | ["cutting"] |

  # PUT /api/curriculum/adjust-for-holidays
  @endpoint @curriculum @holidays @not-implemented
  Scenario: Adjust curriculum for holidays and breaks
    When I send a PUT request to "/api/curriculum/adjust-for-holidays" with:
      | field           | value                         |
      | curriculumId    | curr-2024-123                 |
      | breaks          | ["2024-12-20", "2025-01-03"] |
      | adjustment      | "compress"                    |
    Then the response status should be 200
    And adjusted plan should:
      | change          | implementation                |
      | sessions        | Redistributed around breaks   |
      | goals           | Maintained with new timeline  |
      | resources       | Homework packets for break    |

  # GET /api/curriculum/templates/{setting}
  @endpoint @curriculum @templates @not-implemented
  Scenario: Access curriculum templates
    When I send a GET request to "/api/curriculum/templates/school-based"
    Then the response status should be 200
    And templates should include:
      | template        | description                   | duration    |
      | push-in         | Classroom-based support       | full-year   |
      | pull-out        | Individual therapy sessions   | semester    |
      | consultative    | Teacher support model         | monthly     |

  # POST /api/curriculum/progress/predict
  @endpoint @curriculum @prediction @not-implemented
  Scenario: Predict curriculum progress
    When I send a POST request to "/api/curriculum/progress/predict" with:
      | field           | value                         |
      | studentId       | student-123                   |
      | currentProgress | {"goal1": 45, "goal2": 60}   |
      | weeksRemaining  | 20                            |
    Then the response status should be 200
    And predictions should show:
      | goal   | currentRate | projection | confidence |
      | goal1  | 2.25%/week  | 90%        | high       |
      | goal2  | 3%/week     | 100%       | very high  |

  # POST /api/curriculum/share/team
  @endpoint @curriculum @collaboration @not-implemented
  Scenario: Share curriculum with team
    When I send a POST request to "/api/curriculum/share/team" with:
      | field           | value                         |
      | curriculumId    | curr-2024-123                 |
      | teamMembers     | ["ot@school.edu", "teacher@school.edu"] |
      | permissions     | "view-only"                   |
      | message         | "Please review Emma's plan"   |
    Then the response status should be 200
    And team should have access
    And notifications should be sent

  # GET /api/curriculum/reports/summary
  @endpoint @curriculum @reports @not-implemented
  Scenario: Generate curriculum summary report
    When I send a GET request to "/api/curriculum/reports/summary?curriculumId=curr-2024-123"
    Then the response status should be 200
    And report should contain:
      | section         | content                       |
      | overview        | Goals and timeline            |
      | progress        | Current vs expected           |
      | resources       | Used and planned              |
      | recommendations | Adjustments needed            |