Feature: Continuing Education API Endpoints (FR-018)
  As a therapy professional
  I want to access continuing education opportunities
  So that I can maintain my license and improve my skills

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # GET /api/continuing-education/courses
  @endpoint @ce @catalog @not-implemented
  Scenario: Browse available CE courses
    When I send a GET request to "/api/continuing-education/courses?discipline=OT&credits=2"
    Then the response status should be 200
    And the response should contain:
      | field    | type   |
      | courses  | array  |
      | total    | number |
      | filters  | object |
    And each course should contain:
      | field         | type    |
      | courseId      | string  |
      | title         | string  |
      | credits       | number  |
      | discipline    | array   |
      | format        | string  |
      | price         | number  |
      | instructor    | object  |
      | accreditation | array   |

  # GET /api/continuing-education/courses/{id}
  @endpoint @ce @details @not-implemented
  Scenario: Get course details
    Given CE course "course-123" exists
    When I send a GET request to "/api/continuing-education/courses/course-123"
    Then the response status should be 200
    And the response should contain:
      | field           | type    |
      | courseId        | string  |
      | title           | string  |
      | description     | string  |
      | objectives      | array   |
      | outline         | array   |
      | prerequisites   | array   |
      | credits         | object  |
      | duration        | string  |
      | expirationDate  | string  |
      | passRate        | number  |

  # POST /api/continuing-education/enrollment
  @endpoint @ce @enrollment @not-implemented
  Scenario: Enroll in CE course
    When I send a POST request to "/api/continuing-education/enrollment" with:
      | field         | value              |
      | courseId      | course-123         |
      | paymentMethod | saved-card         |
      | licenseNumber | OT-12345          |
      | state         | CA                 |
    Then the response status should be 201
    And the response should contain:
      | field         | type   |
      | enrollmentId  | string |
      | accessUrl     | string |
      | expiresAt     | string |
    And course materials should be accessible

  # GET /api/continuing-education/my-courses
  @endpoint @ce @progress @not-implemented
  Scenario: Get enrolled courses
    Given I am enrolled in CE courses
    When I send a GET request to "/api/continuing-education/my-courses"
    Then the response status should be 200
    And the response should contain array of:
      | field           | type   |
      | courseId        | string |
      | enrollmentDate  | string |
      | progress        | number |
      | status          | string |
      | certificateUrl  | string |
      | expiresAt       | string |

  # POST /api/continuing-education/courses/{courseId}/progress
  @endpoint @ce @tracking @not-implemented
  Scenario: Update course progress
    Given I am enrolled in "course-123"
    When I send a POST request to "/api/continuing-education/courses/course-123/progress" with:
      | field          | value                    |
      | moduleId       | module-3                 |
      | status         | completed                |
      | timeSpent      | 1800                     |
      | quizScore      | 85                       |
    Then the response status should be 200
    And progress should be saved
    And next module should be unlocked

  # POST /api/continuing-education/courses/{courseId}/exam
  @endpoint @ce @assessment @not-implemented
  Scenario: Take course exam
    Given I completed all modules for "course-123"
    When I send a POST request to "/api/continuing-education/courses/course-123/exam" with:
      | field    | value                                |
      | answers  | [{"questionId": "q1", "answer": "a"}] |
      | timeSpent| 2400                                 |
    Then the response status should be 200
    And the response should contain:
      | field      | type    |
      | score      | number  |
      | passed     | boolean |
      | attempts   | number  |
      | retakeDate | string  |

  # GET /api/continuing-education/certificates
  @endpoint @ce @certificates @not-implemented
  Scenario: Get CE certificates
    Given I have completed CE courses
    When I send a GET request to "/api/continuing-education/certificates"
    Then the response status should be 200
    And the response should contain array of:
      | field           | type   |
      | certificateId   | string |
      | courseTitle     | string |
      | completionDate  | string |
      | credits         | number |
      | downloadUrl     | string |
      | verificationCode| string |

  # POST /api/continuing-education/webinars/register
  @endpoint @ce @webinars @not-implemented
  Scenario: Register for live webinar
    When I send a POST request to "/api/continuing-education/webinars/register" with:
      | field      | value                         |
      | webinarId  | webinar-456                   |
      | timeZone   | America/Los_Angeles           |
      | reminders  | ["1-day", "1-hour"]           |
    Then the response status should be 200
    And calendar invite should be sent
    And access link should be provided

  # GET /api/continuing-education/recommendations
  @endpoint @ce @personalized @not-implemented
  Scenario: Get CE recommendations
    Given I have CE history
    When I send a GET request to "/api/continuing-education/recommendations"
    Then the response status should be 200
    And recommendations should be based on:
      | factor                |
      | License requirements  |
      | Skill gaps           |
      | Interest areas       |
      | Upcoming deadlines   |

  # POST /api/continuing-education/tracking/external
  @endpoint @ce @external @not-implemented
  Scenario: Log external CE credits
    When I send a POST request to "/api/continuing-education/tracking/external" with:
      | field          | value                    |
      | courseName     | Sensory Integration 101  |
      | provider       | AOTA                     |
      | completionDate | 2024-01-15              |
      | credits        | 3                        |
      | certificate    | certificate.pdf          |
    Then the response status should be 201
    And credits should be tracked
    And verification should be pending

  # GET /api/continuing-education/requirements
  @endpoint @ce @compliance @not-implemented
  Scenario: Get license renewal requirements
    When I send a GET request to "/api/continuing-education/requirements?state=CA&license=OT"
    Then the response status should be 200
    And the response should contain:
      | field            | type   |
      | totalRequired    | number |
      | periodLength     | string |
      | currentCredits   | number |
      | expirationDate   | string |
      | specificRequirements | array |
      | remainingCredits | number |

  # POST /api/continuing-education/groups
  @endpoint @ce @group-learning @not-implemented
  Scenario: Create learning group
    When I send a POST request to "/api/continuing-education/groups" with:
      | field         | value                         |
      | name          | Pediatric OT Study Group      |
      | courseId      | course-123                    |
      | maxMembers    | 10                            |
      | meetingSchedule | weekly                      |
      | startDate     | 2024-02-01                    |
    Then the response status should be 201
    And group should be created
    And discussion forum should be available