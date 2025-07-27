Feature: Community Features API Endpoints (FR-030)
  As a platform user
  I want limited community interaction features
  So that I can share experiences and get help

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # POST /api/community/resources/{resourceId}/review
  @endpoint @community @reviews @not-implemented
  Scenario: Submit resource review
    When I send a POST request to "/api/community/resources/res-123/review" with:
      | field         | value                         |
      | rating        | 5                             |
      | title         | "Perfect for my students!"    |
      | comment       | "Clear instructions, engaging" |
      | ageGroup      | "5-7 years"                   |
      | effectiveness | "highly-effective"            |
    Then the response status should be 201
    And review should be submitted for moderation
    And contributor should receive notification

  # GET /api/community/resources/{resourceId}/qa
  @endpoint @community @qa @not-implemented
  Scenario: View Q&A for resource
    When I send a GET request to "/api/community/resources/res-123/qa"
    Then the response status should be 200
    And Q&A should include:
      | questionId | question                    | answers | helpful |
      | q-1        | "Can this be used for..."   | 3       | 12      |
      | q-2        | "Modifications for..."      | 2       | 8       |

  # POST /api/community/success-stories/submit
  @endpoint @community @stories @not-implemented
  Scenario: Submit success story
    When I send a POST request to "/api/community/success-stories/submit" with:
      | field         | value                         |
      | title         | "Breakthrough with AAC"       |
      | story         | "After 6 months using..."     |
      | resourcesUsed | ["res-123", "res-124"]        |
      | outcomes      | "First 2-word combinations"   |
      | permission    | "anonymous"                   |
    Then the response status should be 201
    And story should be queued for review
    And editorial team should be notified

  # POST /api/community/feature-requests/vote
  @endpoint @community @features @not-implemented
  Scenario: Vote on feature request
    When I send a POST request to "/api/community/feature-requests/vote" with:
      | field         | value                    |
      | requestId     | feature-req-456          |
      | voteType      | upvote                   |
      | comment       | "This would save hours!" |
    Then the response status should be 200
    And vote should be recorded
    And request priority should update

  # POST /api/community/bug-reports/submit
  @endpoint @community @bugs @not-implemented
  Scenario: Submit bug report
    When I send a POST request to "/api/community/bug-reports/submit" with:
      | field         | value                         |
      | title         | "PDF download fails"          |
      | description   | "When clicking download..."   |
      | steps         | ["Go to resource", "Click download"] |
      | browser       | "Chrome 120"                  |
      | severity      | "medium"                      |
    Then the response status should be 201
    And ticket should be created
    And user should receive tracking number

  # GET /api/community/moderation/queue
  @endpoint @community @moderation @not-implemented
  Scenario: Access moderation queue
    Given I have moderator privileges
    When I send a GET request to "/api/community/moderation/queue"
    Then the response status should be 200
    And queue should show:
      | type     | count | oldest    |
      | reviews  | 12    | 2 hours   |
      | qa       | 5     | 1 day     |
      | stories  | 3     | 3 days    |

  # PUT /api/community/content/{contentId}/moderate
  @endpoint @community @moderate @not-implemented
  Scenario: Moderate community content
    Given I have moderator privileges
    When I send a PUT request to "/api/community/content/review-123/moderate" with:
      | field         | value                    |
      | action        | approve                  |
      | reason        | "Helpful and appropriate"|
      | edited        | false                    |
    Then the response status should be 200
    And content should be published
    And author should be notified

  # GET /api/community/guidelines
  @endpoint @community @guidelines @not-implemented
  Scenario: View community guidelines
    When I send a GET request to "/api/community/guidelines"
    Then the response status should be 200
    And guidelines should include:
      | section         | content                  |
      | acceptable      | Professional discourse   |
      | prohibited      | Marketing, spam, PHI     |
      | moderation      | Review process           |
      | consequences    | Warning system           |

  # POST /api/community/report-content
  @endpoint @community @report @not-implemented
  Scenario: Report inappropriate content
    When I send a POST request to "/api/community/report-content" with:
      | field         | value                    |
      | contentType   | review                   |
      | contentId     | review-789               |
      | reason        | inappropriate-content    |
      | details       | "Contains student name"  |
    Then the response status should be 201
    And report should be logged
    And content should be flagged for review

  # GET /api/community/feature-requests
  @endpoint @community @requests @not-implemented
  Scenario: View feature requests
    When I send a GET request to "/api/community/feature-requests?status=open&sort=votes"
    Then the response status should be 200
    And requests should be sorted by:
      | requestId | title                    | votes | status      |
      | req-1     | "Offline mode for iPad"  | 245   | under-review |
      | req-2     | "More languages"         | 189   | planned      |
      | req-3     | "Video tutorials"        | 156   | open         |