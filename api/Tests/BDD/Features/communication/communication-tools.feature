Feature: Communication Tools API Endpoints (FR-014)
  As a therapy professional
  I want multi-channel communication and sharing tools
  So that I can effectively communicate with parents and share resources

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # POST /api/communication/quicklinks
  @endpoint @communication @sharing @not-implemented
  Scenario: Create expiring quicklink for resources
    When I send a POST request to "/api/communication/quicklinks" with:
      | field           | value                    |
      | resourceIds     | ["res-123", "res-124"]   |
      | expirationHours | 72                       |
      | requireAuth     | false                    |
      | accessLimit     | 10                       |
      | message         | "This week's homework"   |
    Then the response status should be 201
    And the response should contain:
      | field       | type   |
      | linkId      | string |
      | shortUrl    | string |
      | expiresAt   | string |
      | qrCode      | string |

  # POST /api/communication/email/send
  @endpoint @communication @email @not-implemented
  Scenario: Send templated email to parents
    When I send a POST request to "/api/communication/email/send" with:
      | field         | value                           |
      | recipientIds  | ["parent-123", "parent-124"]    |
      | template      | weekly-update                   |
      | variables     | {"studentName": "Emma", "week": "12"} |
      | attachments   | ["res-123", "res-124"]          |
      | language      | es                              |
    Then the response status should be 202
    And emails should be queued
    And delivery tracking should be enabled

  # POST /api/communication/sms/send
  @endpoint @communication @sms @not-implemented
  Scenario: Send SMS reminder to parent
    When I send a POST request to "/api/communication/sms/send" with:
      | field        | value                         |
      | phoneNumber  | "+1-555-123-4567"             |
      | message      | "Reminder: Therapy tomorrow at 3pm" |
      | studentId    | student-123                   |
      | scheduledAt  | "2024-01-23T08:00:00Z"        |
    Then the response status should be 202
    And SMS should be scheduled
    And opt-out link should be included

  # GET /api/communication/portal/{studentId}
  @endpoint @communication @portal @not-implemented
  Scenario: Access parent portal for student
    Given parent has access to student "student-123"
    When I send a GET request to "/api/communication/portal/student-123"
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | currentAssignments | array  |
      | progressReports    | array  |
      | upcomingSessions   | array  |
      | resources          | array  |
      | messages           | array  |

  # POST /api/communication/homework/assign
  @endpoint @communication @homework @not-implemented
  Scenario: Assign homework with tracking
    When I send a POST request to "/api/communication/homework/assign" with:
      | field         | value                    |
      | studentId     | student-123              |
      | resources     | ["res-123", "res-124"]   |
      | dueDate       | "2024-01-30"             |
      | instructions  | "Complete 10 minutes daily" |
      | trackProgress | true                     |
      | parentNotify  | true                     |
    Then the response status should be 201
    And assignment should be created
    And parent notification should be sent
    And tracking link should be generated

  # GET /api/communication/history/{studentId}
  @endpoint @communication @history @not-implemented
  Scenario: View communication history
    When I send a GET request to "/api/communication/history/student-123?days=30"
    Then the response status should be 200
    And the response should contain:
      | field    | type  |
      | emails   | array |
      | sms      | array |
      | portal   | array |
      | homework | array |
      | notes    | array |

  # POST /api/communication/progress-report/generate
  @endpoint @communication @reports @not-implemented
  Scenario: Generate parent-friendly progress report
    When I send a POST request to "/api/communication/progress-report/generate" with:
      | field          | value                    |
      | studentId      | student-123              |
      | reportPeriod   | monthly                  |
      | includeGraphs  | true                     |
      | language       | en                       |
      | simplifyTerms  | true                     |
    Then the response status should be 200
    And report should be generated
    And language should be parent-friendly
    And visuals should be included

  # POST /api/communication/secure-message
  @endpoint @communication @messaging @not-implemented
  Scenario: Send secure message to therapist
    Given I am authenticated as a parent
    When I send a POST request to "/api/communication/secure-message" with:
      | field       | value                         |
      | therapistId | therapist-123                 |
      | subject     | "Question about exercises"    |
      | message     | "Should we continue if..."    |
      | studentId   | student-123                   |
      | urgent      | false                         |
    Then the response status should be 201
    And message should be encrypted
    And therapist should be notified

  # POST /api/communication/bulk-message
  @endpoint @communication @bulk @not-implemented
  Scenario: Send bulk communication to multiple families
    When I send a POST request to "/api/communication/bulk-message" with:
      | field         | value                         |
      | recipientType | active-caseload               |
      | channel       | ["email", "sms"]              |
      | message       | "Clinic closed for holiday"   |
      | sendAt        | "2024-01-22T09:00:00Z"        |
      | trackOpens    | true                          |
    Then the response status should be 202
    And messages should be queued
    And delivery stats should be tracked

  # GET /api/communication/templates
  @endpoint @communication @templates @not-implemented
  Scenario: List communication templates
    When I send a GET request to "/api/communication/templates?category=parent"
    Then the response status should be 200
    And the response should contain array of:
      | field         | type    |
      | templateId    | string  |
      | name          | string  |
      | category      | string  |
      | languages     | array   |
      | variables     | array   |
      | lastUsed      | string  |