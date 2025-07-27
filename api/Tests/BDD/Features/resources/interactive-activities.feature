Feature: Interactive Digital Activities API Endpoints (FR-009, FR-042)
  As a therapy professional
  I want to create and manage interactive digital activities
  So that students can practice independently with immediate feedback

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # GET /api/activities
  @endpoint @activities @not-implemented
  Scenario: List available digital activities
    When I send a GET request to "/api/activities?type=interactive&skillArea=articulation"
    Then the response status should be 200
    And the response should contain:
      | field      | type   |
      | activities | array  |
      | total      | number |
      | filters    | object |
    And each activity should contain:
      | field           | type    |
      | activityId      | string  |
      | title           | string  |
      | activityType    | string  |
      | skillAreas      | array   |
      | gradeLevel      | array   |
      | duration        | number  |
      | selfGrading     | boolean |
      | dataCollection  | boolean |

  # GET /api/activities/{id}
  @endpoint @activities @details @not-implemented
  Scenario: Get digital activity details
    Given digital activity "activity-123" exists
    When I send a GET request to "/api/activities/activity-123"
    Then the response status should be 200
    And the response should contain:
      | field              | type    |
      | activityId         | string  |
      | title              | string  |
      | instructions       | string  |
      | settings           | object  |
      | interactionTypes   | array   |
      | feedbackMode       | string  |
      | scoringCriteria    | object  |
      | customizationOptions| object |

  # POST /api/activities
  @endpoint @activities @creation @not-implemented
  Scenario: Create new digital activity
    When I send a POST request to "/api/activities" with:
      | field            | value                           |
      | title            | R Sound Matching Game           |
      | activityType     | drag-drop                       |
      | skillAreas       | ["articulation", "phonology"]   |
      | targetAge        | 5-8                             |
      | items            | [{"word": "rabbit", "audio": "rabbit.mp3"}] |
      | feedbackType     | immediate                       |
      | scoringMethod    | percentage                      |
    Then the response status should be 201
    And the response should contain:
      | field      | type   |
      | activityId | string |
      | editUrl    | string |
      | previewUrl | string |

  # PUT /api/activities/{id}
  @endpoint @activities @update @not-implemented
  Scenario: Update digital activity
    Given I own activity "activity-123"
    When I send a PUT request to "/api/activities/activity-123" with:
      | field         | value                    |
      | title         | Updated R Sound Game     |
      | difficulty    | adaptive                 |
      | maxAttempts   | 3                        |
    Then the response status should be 200
    And the activity should be updated
    And assigned students should see updates

  # POST /api/activities/{id}/assign
  @endpoint @activities @assignment @not-implemented
  Scenario: Assign activity to students
    Given activity "activity-123" exists
    When I send a POST request to "/api/activities/activity-123/assign" with:
      | field         | value                         |
      | studentIds    | ["student-123", "student-456"]|
      | dueDate       | 2024-01-29                    |
      | attempts      | unlimited                     |
      | instructions  | Complete 3 times this week    |
    Then the response status should be 200
    And activities should be assigned
    And students should receive notifications

  # GET /api/activities/{id}/results
  @endpoint @activities @results @not-implemented
  Scenario: Get activity results
    Given students completed activity "activity-123"
    When I send a GET request to "/api/activities/activity-123/results"
    Then the response status should be 200
    And the response should contain:
      | field           | type   |
      | completions     | array  |
      | averageScore    | number |
      | averageTime     | number |
      | commonErrors    | array  |
      | masteryRate     | number |

  # POST /api/activities/{id}/attempt
  @endpoint @activities @student-attempt @not-implemented
  Scenario: Student attempts activity
    Given I am a student with activity "activity-123" assigned
    When I send a POST request to "/api/activities/activity-123/attempt" with:
      | field        | value                           |
      | responses    | [{"itemId": "1", "answer": "rabbit", "time": 5.2}] |
      | startTime    | 2024-01-22T10:00:00Z           |
      | endTime      | 2024-01-22T10:15:00Z           |
      | deviceInfo   | {"platform": "iPad", "os": "iOS 16"} |
    Then the response status should be 200
    And the response should contain:
      | field        | type    |
      | score        | number  |
      | feedback     | array   |
      | accuracy     | number  |
      | nextActivity | string  |

  # GET /api/activities/{id}/customize
  @endpoint @activities @customization @not-implemented
  Scenario: Get customization options
    Given activity "activity-123" is customizable
    When I send a GET request to "/api/activities/activity-123/customize"
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | hidableElements    | array  |
      | difficultyOptions  | array  |
      | contentOptions     | array  |
      | visualOptions      | object |
      | audioOptions       | object |

  # PUT /api/activities/{id}/customize
  @endpoint @activities @apply-customization @not-implemented
  Scenario: Apply customizations for student
    When I send a PUT request to "/api/activities/activity-123/customize" with:
      | field          | value                      |
      | studentId      | student-123                |
      | hideElements   | ["timer", "score"]         |
      | fontSize       | large                      |
      | audioFeedback  | true                       |
      | reducedItems   | 10                         |
    Then the response status should be 200
    And customizations should be saved
    And apply only to specified student

  # POST /api/activities/generate
  @endpoint @activities @ai-generation @not-implemented
  Scenario: Generate activity with AI
    When I send a POST request to "/api/activities/generate" with:
      | field         | value                         |
      | activityType  | memory-match                  |
      | content       | CVC words with 'at' family    |
      | pairs         | 6                             |
      | visuals       | simple-illustrations          |
    Then the response status should be 202
    And AI should generate activity
    And preview should be available

  # POST /api/activities/{id}/duplicate
  @endpoint @activities @duplication @not-implemented
  Scenario: Duplicate and modify activity
    Given activity "activity-123" exists
    When I send a POST request to "/api/activities/activity-123/duplicate" with:
      | field       | value                      |
      | newTitle    | R Sound Game - Level 2     |
      | modifications| {"difficulty": "harder", "items": "+10"} |
    Then the response status should be 201
    And new activity should be created
    And maintain original structure

  # GET /api/activities/templates
  @endpoint @activities @templates @not-implemented
  Scenario: Get activity templates
    When I send a GET request to "/api/activities/templates?category=articulation"
    Then the response status should be 200
    And the response should contain:
      | field      | type  |
      | templates  | array |
      | categories | array |
    And each template should be customizable

  # POST /api/activities/{id}/offline
  @endpoint @activities @offline @not-implemented
  Scenario: Enable offline mode for activity
    When I send a POST request to "/api/activities/activity-123/offline" with:
      | field      | value                         |
      | studentIds | ["student-123", "student-456"]|
      | duration   | 7-days                        |
    Then the response status should be 200
    And activity should be downloadable
    And work without internet connection

  # GET /api/activities/{id}/analytics
  @endpoint @activities @analytics @not-implemented
  Scenario: Get detailed activity analytics
    Given activity "activity-123" has usage data
    When I send a GET request to "/api/activities/activity-123/analytics"
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | usageStats         | object |
      | performanceMetrics | object |
      | engagementData     | object |
      | errorPatterns      | array  |
      | recommendations    | array  |