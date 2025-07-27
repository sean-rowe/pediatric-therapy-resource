Feature: Caseload Integration API Endpoints (FR-025)
  As a therapy professional
  I want integrated caseload and resource management
  So that I can efficiently manage my entire caseload with linked resources

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # GET /api/caseload/overview
  @endpoint @caseload @overview @not-implemented
  Scenario: View comprehensive caseload overview
    When I send a GET request to "/api/caseload/overview"
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | totalStudents      | number |
      | sessionsThisWeek   | number |
      | goalsInProgress    | number |
      | upcomingEvaluations| array  |
      | resourceUsage      | object |
      | productivityMetrics| object |

  # POST /api/caseload/goals/align-resources
  @endpoint @caseload @goals @not-implemented
  Scenario: Auto-align resources to IEP goals
    When I send a POST request to "/api/caseload/goals/align-resources" with:
      | field         | value                    |
      | studentId     | student-123              |
      | goalIds       | ["goal-1", "goal-2"]     |
      | autoSuggest   | true                     |
      | skillLevel    | emerging                 |
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | alignedResources   | array  |
      | confidenceScores   | array  |
      | alternativeSuggestions | array |

  # GET /api/caseload/resource-effectiveness
  @endpoint @caseload @effectiveness @not-implemented
  Scenario: Analyze resource effectiveness across caseload
    When I send a GET request to "/api/caseload/resource-effectiveness?period=semester"
    Then the response status should be 200
    And analysis should show:
      | metric              | data                   |
      | mostEffective       | Resources with best outcomes |
      | leastUsed           | Underutilized resources |
      | goalCorrelation     | Resource-to-progress mapping |
      | recommendations     | Data-driven suggestions |

  # POST /api/caseload/groups/optimize
  @endpoint @caseload @groups @not-implemented
  Scenario: Optimize group session composition
    When I send a POST request to "/api/caseload/groups/optimize" with:
      | field           | value                    |
      | availableSlots  | ["Mon-9am", "Wed-2pm"]   |
      | groupSize       | 4                        |
      | criteria        | ["goals", "age", "level"] |
    Then the response status should be 200
    And suggestions should include:
      | groupId    | students          | commonGoals       | compatibility |
      | group-1    | [s1, s2, s3, s4]  | social-skills     | 95%          |
      | group-2    | [s5, s6, s7, s8]  | articulation-/r/  | 88%          |

  # GET /api/caseload/analytics/dashboard
  @endpoint @caseload @analytics @not-implemented
  Scenario: Access caseload analytics dashboard
    When I send a GET request to "/api/caseload/analytics/dashboard"
    Then the response status should be 200
    And dashboard should display:
      | widget            | metrics                    |
      | progressOverview  | Goal achievement by student |
      | sessionFrequency  | Attendance patterns         |
      | minutesDelivered  | Service delivery compliance |
      | outcomesTrending  | Progress trajectories       |

  # POST /api/caseload/productivity/calculate
  @endpoint @caseload @productivity @not-implemented
  Scenario: Calculate productivity metrics
    When I send a POST request to "/api/caseload/productivity/calculate" with:
      | field         | value                    |
      | dateRange     | {"start": "2024-01-01", "end": "2024-01-31"} |
      | includePrep   | true                     |
      | includeDoc    | true                     |
    Then the response status should be 200
    And metrics should include:
      | metric           | value    |
      | directService    | 75%      |
      | documentation    | 15%      |
      | preparation      | 10%      |
      | efficiency       | 92%      |

  # POST /api/caseload/schedule/optimize
  @endpoint @caseload @scheduling @not-implemented
  Scenario: Optimize caseload scheduling
    When I send a POST request to "/api/caseload/schedule/optimize" with:
      | field            | value                    |
      | constraints      | ["lunch-12pm", "no-back-to-back"] |
      | priorities       | ["high-need-first", "group-afternoon"] |
      | timeframe        | "next-month"             |
    Then the response status should be 200
    And optimized schedule should:
      | feature          | benefit                  |
      | travelTime       | Minimized between locations |
      | prepTime         | Built-in between sessions |
      | groupings        | Efficient student clusters |

  # GET /api/caseload/resources/usage-report
  @endpoint @caseload @usage @not-implemented
  Scenario: Generate resource usage report
    When I send a GET request to "/api/caseload/resources/usage-report?groupBy=student"
    Then the response status should be 200
    And report should show:
      | student      | topResources         | frequency | outcomes    |
      | student-123  | ["res-1", "res-2"]   | daily     | improving   |
      | student-124  | ["res-3", "res-4"]   | weekly    | maintaining |

  # POST /api/caseload/recommendations/generate
  @endpoint @caseload @recommendations @not-implemented
  Scenario: Generate caseload-wide recommendations
    When I send a POST request to "/api/caseload/recommendations/generate" with:
      | field         | value                    |
      | focusArea     | "efficiency"             |
      | constraints   | ["budget", "time"]       |
    Then the response status should be 200
    And recommendations should include:
      | type             | suggestion                      |
      | grouping         | Combine compatible students     |
      | resources        | High-impact materials           |
      | scheduling       | Optimal session timing          |
      | technology       | Time-saving digital tools       |

  # GET /api/caseload/compliance/status
  @endpoint @caseload @compliance @not-implemented
  Scenario: Check caseload compliance status
    When I send a GET request to "/api/caseload/compliance/status"
    Then the response status should be 200
    And compliance check should show:
      | area              | status      | details                |
      | serviceMinutes    | compliant   | 98% delivered          |
      | documentation     | warning     | 3 notes pending        |
      | evaluations       | compliant   | All current            |
      | progressReports   | due         | 5 due this month       |