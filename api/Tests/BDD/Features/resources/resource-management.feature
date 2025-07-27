Feature: Resource Management API Endpoints
  As a therapy professional
  I want to manage individual resources
  So that I can access, download, and organize therapy materials

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # GET /api/resources/{id}
  @endpoint @resources @not-implemented
  Scenario: Get resource details by ID
    Given a resource exists with id "res-123"
    When I send a GET request to "/api/resources/res-123"
    Then the response status should be 200
    And the response should contain:
      | field           | type    |
      | id              | string  |
      | title           | string  |
      | description     | string  |
      | content         | string  |
      | thumbnailUrl    | string  |
      | previewUrls     | array   |
      | skillAreas      | array   |
      | gradeLevels     | array   |
      | resourceType    | string  |
      | format          | string  |
      | fileSize        | number  |
      | evidenceLevel   | number  |
      | rating          | number  |
      | downloadCount   | number  |
      | createdAt       | string  |
      | updatedAt       | string  |
      | createdBy       | object  |
      | clinicalReview  | object  |
      | relatedResources| array   |

  @endpoint @resources @not-found @not-implemented
  Scenario: Resource not found
    When I send a GET request to "/api/resources/nonexistent-id"
    Then the response status should be 404
    And the response should contain error "Resource not found"

  # GET /api/resources/{id}/download
  @endpoint @resources @download @not-implemented
  Scenario: Download resource file
    Given a resource exists with id "res-123"
    When I send a GET request to "/api/resources/res-123/download"
    Then the response status should be 200
    And the response headers should contain:
      | header              | value                     |
      | Content-Type        | application/pdf           |
      | Content-Disposition | attachment; filename=...  |
    And the download should be tracked
    And usage analytics should be updated

  @endpoint @resources @download @limits @not-implemented
  Scenario: Enforce download limits for basic users
    Given I have a "basic" subscription with 10 downloads per month
    And I have already downloaded 10 resources this month
    When I send a GET request to "/api/resources/res-123/download"
    Then the response status should be 403
    And the response should contain error "Monthly download limit reached"

  # POST /api/resources/{id}/favorite
  @endpoint @resources @favorites @not-implemented
  Scenario: Add resource to favorites
    Given a resource exists with id "res-123"
    When I send a POST request to "/api/resources/res-123/favorite"
    Then the response status should be 200
    And the response should contain message "Added to favorites"
    And the resource should appear in my favorites list

  # DELETE /api/resources/{id}/favorite
  @endpoint @resources @favorites @not-implemented
  Scenario: Remove resource from favorites
    Given resource "res-123" is in my favorites
    When I send a DELETE request to "/api/resources/res-123/favorite"
    Then the response status should be 200
    And the response should contain message "Removed from favorites"

  # GET /api/resources/favorites
  @endpoint @resources @favorites @not-implemented
  Scenario: Get user's favorite resources
    Given I have 25 favorite resources
    When I send a GET request to "/api/resources/favorites?page=1&limit=20"
    Then the response status should be 200
    And the response should contain:
      | field     | type   |
      | favorites | array  |
      | total     | number |
      | page      | number |
    And each favorite should include resource details

  # POST /api/resources/{id}/rate
  @endpoint @resources @ratings @not-implemented
  Scenario: Rate a resource
    Given a resource exists with id "res-123"
    When I send a POST request to "/api/resources/res-123/rate" with:
      | field   | value |
      | rating  | 5     |
      | comment | Excellent resource for fine motor skills |
    Then the response status should be 200
    And the response should contain:
      | field         | type   |
      | averageRating | number |
      | totalRatings  | number |
    And the rating should be recorded

  @endpoint @resources @ratings @validation @not-implemented
  Scenario: Validate rating value
    When I send a POST request to "/api/resources/res-123/rate" with:
      | field  | value |
      | rating | 7     |
    Then the response status should be 400
    And the response should contain error "Rating must be between 1 and 5"

  # GET /api/resources/{id}/ratings
  @endpoint @resources @ratings @not-implemented
  Scenario: Get resource ratings and reviews
    Given resource "res-123" has multiple ratings
    When I send a GET request to "/api/resources/res-123/ratings"
    Then the response status should be 200
    And the response should contain:
      | field         | type   |
      | averageRating | number |
      | distribution  | object |
      | reviews       | array  |
    And each review should contain:
      | field     | type   |
      | userId    | string |
      | userName  | string |
      | rating    | number |
      | comment   | string |
      | helpfulCount | number |
      | createdAt | string |

  # POST /api/resources/{id}/report
  @endpoint @resources @moderation @not-implemented
  Scenario: Report inappropriate resource
    Given a resource exists with id "res-123"
    When I send a POST request to "/api/resources/res-123/report" with:
      | field  | value                    |
      | reason | Inappropriate content    |
      | details| Contains medical advice  |
    Then the response status should be 200
    And the response should contain message "Report submitted"
    And a moderation review should be triggered

  # GET /api/resources/{id}/versions
  @endpoint @resources @versions @not-implemented
  Scenario: Get resource version history
    Given resource "res-123" has multiple versions
    When I send a GET request to "/api/resources/res-123/versions"
    Then the response status should be 200
    And the response should contain array of:
      | field       | type   |
      | versionId   | string |
      | versionNumber | number |
      | changedBy   | object |
      | changeNotes | string |
      | createdAt   | string |

  # POST /api/resources/{id}/copy
  @endpoint @resources @organization @not-implemented
  Scenario: Create copy of resource for modification
    Given a resource exists with id "res-123"
    When I send a POST request to "/api/resources/res-123/copy" with:
      | field | value                      |
      | title | My Modified Version        |
      | notes | Adapted for younger kids   |
    Then the response status should be 201
    And the response should contain:
      | field        | type   |
      | newResourceId | string |
      | originalId   | string |
    And the new resource should be editable by me

  # GET /api/resources/{id}/related
  @endpoint @resources @discovery @not-implemented
  Scenario: Get related resources
    Given a resource exists with id "res-123" about fine motor skills
    When I send a GET request to "/api/resources/res-123/related"
    Then the response status should be 200
    And the response should contain array of related resources
    And resources should be sorted by relevance
    And should include similar skill areas and grade levels

  # POST /api/resources/{id}/share
  @endpoint @resources @sharing @not-implemented
  Scenario: Share resource with colleague
    Given a resource exists with id "res-123"
    When I send a POST request to "/api/resources/res-123/share" with:
      | field      | value                    |
      | email      | colleague@clinic.com     |
      | message    | Check out this resource  |
      | expiresIn  | 7d                       |
    Then the response status should be 200
    And the response should contain:
      | field    | type   |
      | shareId  | string |
      | shareUrl | string |
    And an email should be sent to the colleague

  # GET /api/resources/shared-with-me
  @endpoint @resources @sharing @not-implemented
  Scenario: Get resources shared with me
    Given colleagues have shared resources with me
    When I send a GET request to "/api/resources/shared-with-me"
    Then the response status should be 200
    And the response should contain array of:
      | field       | type   |
      | resourceId  | string |
      | sharedBy    | object |
      | sharedAt    | string |
      | message     | string |
      | expiresAt   | string |

  # POST /api/resources/{id}/collections
  @endpoint @resources @collections @not-implemented
  Scenario: Add resource to collection
    Given a resource exists with id "res-123"
    And I have a collection "Fine Motor Activities"
    When I send a POST request to "/api/resources/res-123/collections" with:
      | field        | value                |
      | collectionId | coll-456            |
      | notes        | Good for beginners  |
    Then the response status should be 200
    And the resource should be added to the collection

  # GET /api/resources/{id}/usage
  @endpoint @resources @analytics @not-implemented
  Scenario: Get resource usage statistics
    Given I own resource "res-123"
    When I send a GET request to "/api/resources/res-123/usage"
    Then the response status should be 200
    And the response should contain:
      | field           | type   |
      | totalDownloads  | number |
      | uniqueUsers     | number |
      | averageRating   | number |
      | usageByMonth    | array  |
      | usageByGrade    | object |
      | topReferrers    | array  |

  # POST /api/resources/{id}/clinical-review
  @endpoint @resources @quality @not-implemented
  Scenario: Submit resource for clinical review
    Given I created resource "res-123"
    When I send a POST request to "/api/resources/res-123/clinical-review" with:
      | field              | value                               |
      | requestedReviewers | ["OT", "SLP"]                      |
      | notes              | Please verify terminology accuracy  |
    Then the response status should be 200
    And the response should contain:
      | field      | type   |
      | reviewId   | string |
      | status     | string |
      | estimatedCompletion | string |
    And review requests should be created