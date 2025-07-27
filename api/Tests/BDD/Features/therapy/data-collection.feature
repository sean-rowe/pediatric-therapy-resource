Feature: Data Collection API Endpoints (FR-004)
  As a therapy professional
  I want to collect and analyze therapy data
  So that I can track progress and make data-driven decisions

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # POST /api/data-collection/sessions/{sessionId}/data
  @endpoint @data-collection @recording @not-implemented
  Scenario: Record session data points
    Given I am in session with student "student-123"
    When I send a POST request to "/api/data-collection/sessions/session-456/data" with:
      | field         | value                                |
      | goalId        | goal-789                             |
      | trialData     | [{"trial": 1, "response": "correct", "prompt": "none"}] |
      | percentCorrect| 80                                   |
      | notes         | Good attention, minimal prompting    |
      | timestamp     | 2024-01-22T10:15:00Z                 |
    Then the response status should be 201
    And data should be saved
    And progress calculations should update

  # POST /api/data-collection/quick-tally
  @endpoint @data-collection @quick @not-implemented
  Scenario: Quick tally data collection
    Given I am collecting frequency data
    When I send a POST request to "/api/data-collection/quick-tally" with:
      | field      | value                    |
      | studentId  | student-123              |
      | behavior   | hand-raising             |
      | count      | 15                       |
      | duration   | 30                       |
      | context    | group-activity           |
    Then the response status should be 201
    And frequency should be calculated
    And data should be graphed automatically

  # POST /api/data-collection/goals/{goalId}/probe
  @endpoint @data-collection @probes @not-implemented
  Scenario: Record goal probe data
    Given goal "goal-789" requires weekly probes
    When I send a POST request to "/api/data-collection/goals/goal-789/probe" with:
      | field          | value                           |
      | probeDate      | 2024-01-22                      |
      | trials         | 10                              |
      | correct        | 7                               |
      | promptLevel    | gestural                        |
      | setting        | therapy-room                    |
      | materials      | ["flashcards", "manipulatives"] |
    Then the response status should be 201
    And probe data should be recorded
    And trend line should update

  # GET /api/data-collection/students/{studentId}/data
  @endpoint @data-collection @retrieval @not-implemented
  Scenario: Get student data history
    Given student "student-123" has collected data
    When I send a GET request to "/api/data-collection/students/student-123/data?startDate=2024-01-01&endDate=2024-01-31"
    Then the response status should be 200
    And the response should contain:
      | field         | type   |
      | dataPoints    | array  |
      | summary       | object |
      | trends        | object |
      | graphs        | array  |
    And data should be organized by goal

  # POST /api/data-collection/abc
  @endpoint @data-collection @behavioral @not-implemented
  Scenario: Record ABC behavioral data
    Given I am observing behavior
    When I send a POST request to "/api/data-collection/abc" with:
      | field         | value                           |
      | studentId     | student-123                     |
      | antecedent    | Transition to math              |
      | behavior      | Threw materials                 |
      | consequence   | Removed from activity           |
      | duration      | 3 minutes                       |
      | intensity     | moderate                        |
      | time          | 2024-01-22T10:30:00Z            |
      | setting       | classroom                       |
    Then the response status should be 201
    And ABC data should be recorded
    And patterns should be analyzed

  # GET /api/data-collection/analysis/{studentId}
  @endpoint @data-collection @analysis @not-implemented
  Scenario: Get data analysis and insights
    Given student "student-123" has 30 days of data
    When I send a GET request to "/api/data-collection/analysis/student-123"
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | progressSummary    | object |
      | trendAnalysis      | object |
      | recommendations    | array  |
      | predictedOutcomes  | object |
      | comparisonToPeers  | object |

  # POST /api/data-collection/rubric
  @endpoint @data-collection @rubric @not-implemented
  Scenario: Use rubric-based data collection
    When I send a POST request to "/api/data-collection/rubric" with:
      | field         | value                               |
      | studentId     | student-123                         |
      | rubricId      | social-skills-rubric                |
      | scores        | {"initiation": 3, "maintenance": 2, "reciprocity": 2} |
      | observations  | Improved initiation this week       |
      | date          | 2024-01-22                          |
    Then the response status should be 201
    And rubric scores should be saved
    And progress visualization should update

  # POST /api/data-collection/duration
  @endpoint @data-collection @duration @not-implemented
  Scenario: Record duration data
    When I send a POST request to "/api/data-collection/duration" with:
      | field         | value                    |
      | studentId     | student-123              |
      | behavior      | on-task                  |
      | startTime     | 2024-01-22T10:00:00Z     |
      | endTime       | 2024-01-22T10:08:30Z     |
      | totalInterval | 600                      |
      | notes         | With visual timer        |
    Then the response status should be 201
    And duration percentage should be calculated
    And added to behavior chart

  # POST /api/data-collection/interval
  @endpoint @data-collection @interval @not-implemented
  Scenario: Record interval-based data
    When I send a POST request to "/api/data-collection/interval" with:
      | field          | value                                    |
      | studentId      | student-123                              |
      | behavior       | vocal-stereotypy                         |
      | intervalType   | partial                                  |
      | intervalLength | 15                                       |
      | intervals      | [true, false, true, true, false, false]  |
      | totalIntervals | 6                                        |
    Then the response status should be 201
    And percentage of intervals should be calculated
    And data should be graphed

  # GET /api/data-collection/graphs/{studentId}/{goalId}
  @endpoint @data-collection @visualization @not-implemented
  Scenario: Get progress graphs
    Given student "student-123" has data for goal "goal-789"
    When I send a GET request to "/api/data-collection/graphs/student-123/goal-789"
    Then the response status should be 200
    And the response should contain:
      | field           | type   |
      | lineGraph       | object |
      | barChart        | object |
      | trendLine       | object |
      | aimLine         | object |
      | phaseChangeLines| array  |

  # POST /api/data-collection/import
  @endpoint @data-collection @import @not-implemented
  Scenario: Import data from external source
    When I send a POST request to "/api/data-collection/import" with:
      | field      | value                    |
      | format     | csv                      |
      | studentId  | student-123              |
      | goalId     | goal-789                 |
      | mapping    | {"date": "A", "score": "B", "notes": "C"} |
    And I attach "progress_data.csv"
    Then the response status should be 200
    And data should be imported
    And existing graphs should update

  # POST /api/data-collection/templates
  @endpoint @data-collection @templates @not-implemented
  Scenario: Create data collection template
    When I send a POST request to "/api/data-collection/templates" with:
      | field          | value                              |
      | name           | Articulation Progress Tracker      |
      | dataType       | percentage                         |
      | fields         | ["position", "wordLevel", "cues"]  |
      | calculations   | ["average", "trend"]               |
      | graphType      | line                               |
    Then the response status should be 201
    And template should be saved
    And be available for future use

  # GET /api/data-collection/export/{studentId}
  @endpoint @data-collection @export @not-implemented
  Scenario: Export student data
    Given student "student-123" has extensive data
    When I send a GET request to "/api/data-collection/export/student-123?format=excel&dateRange=semester"
    Then the response status should be 200
    And the response should contain:
      | field       | type   |
      | downloadUrl | string |
      | fileName    | string |
      | format      | string |
    And file should include all data with graphs

  # POST /api/data-collection/phase-change
  @endpoint @data-collection @phases @not-implemented
  Scenario: Mark phase change in data
    When I send a POST request to "/api/data-collection/phase-change" with:
      | field        | value                         |
      | studentId    | student-123                   |
      | goalId       | goal-789                      |
      | phaseDate    | 2024-01-22                    |
      | phaseName    | Intervention B                |
      | description  | Added visual supports         |
    Then the response status should be 201
    And phase line should appear on graphs
    And data should be analyzed by phase

  # POST /api/data-collection/mastery
  @endpoint @data-collection @mastery @not-implemented
  Scenario: Check mastery criteria
    Given goal "goal-789" has mastery criteria
    When I send a POST request to "/api/data-collection/mastery" with:
      | field     | value      |
      | studentId | student-123|
      | goalId    | goal-789   |
    Then the response status should be 200
    And the response should contain:
      | field           | type    |
      | masteryMet      | boolean |
      | currentLevel    | number  |
      | criteriaRequired| string  |
      | consecutiveDays | number  |
      | recommendation  | string  |

  # POST /api/data-collection/bulk
  @endpoint @data-collection @bulk @not-implemented
  Scenario: Bulk data entry
    When I send a POST request to "/api/data-collection/bulk" with:
      | field    | value                                          |
      | entries  | [{"studentId": "s1", "goalId": "g1", "score": 85}] |
      | date     | 2024-01-22                                     |
      | session  | group-therapy                                  |
    Then the response status should be 200
    And all entries should be processed
    And individual progress should update