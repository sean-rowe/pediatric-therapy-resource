Feature: Organization Management API Endpoints
  As an organization owner or admin
  I want to manage my organization
  So that my team can collaborate effectively

  Background:
    Given the API is available
    And I am authenticated as "owner@therapyclinic.com"

  # POST /api/organizations
  @endpoint @organizations @not-implemented
  Scenario: Create a new organization
    Given I have a "team" subscription
    When I send a POST request to "/api/organizations" with:
      | field          | value                    |
      | name           | Sunshine Therapy Center  |
      | type           | clinic                   |
      | taxId          | 12-3456789              |
      | address        | 123 Main St, City, ST   |
      | phone          | 555-100-2000            |
      | website        | www.sunshinetherapy.com |
    Then the response status should be 201
    And the response should contain:
      | field    | type   |
      | id       | string |
      | name     | string |
      | slug     | string |
      | ownerId  | string |
    And I should be set as the organization owner
    And default roles should be created

  @endpoint @organizations @validation @not-implemented
  Scenario: Validate organization creation
    When I send a POST request to "/api/organizations" with:
      | field | value |
      | name  |       |
    Then the response status should be 400
    And the response should contain error "Organization name is required"

  # GET /api/organizations/{id}
  @endpoint @organizations @not-implemented
  Scenario: Get organization details
    Given I own organization "org-123"
    When I send a GET request to "/api/organizations/org-123"
    Then the response status should be 200
    And the response should contain:
      | field         | type    |
      | id            | string  |
      | name          | string  |
      | type          | string  |
      | memberCount   | number  |
      | subscription  | object  |
      | settings      | object  |
      | createdAt     | string  |

  @endpoint @organizations @authorization @not-implemented
  Scenario: Non-member cannot access organization
    Given organization "org-other" exists
    And I am not a member of "org-other"
    When I send a GET request to "/api/organizations/org-other"
    Then the response status should be 403
    And the response should contain error "Access denied"

  # PUT /api/organizations/{id}
  @endpoint @organizations @not-implemented
  Scenario: Update organization details
    Given I own organization "org-123"
    When I send a PUT request to "/api/organizations/org-123" with:
      | field   | value                  |
      | name    | Sunshine Therapy Group |
      | website | www.sunshinetherapy.org|
    Then the response status should be 200
    And the organization should be updated
    And an audit log should be created

  # GET /api/organizations/{id}/members
  @endpoint @organizations @members @not-implemented
  Scenario: List organization members
    Given I am admin of organization "org-123"
    And the organization has 15 members
    When I send a GET request to "/api/organizations/org-123/members?page=1&limit=10"
    Then the response status should be 200
    And the response should contain:
      | field   | type   |
      | members | array  |
      | total   | number |
      | page    | number |
    And each member should contain:
      | field      | type    |
      | id         | string  |
      | email      | string  |
      | name       | string  |
      | role       | string  |
      | status     | string  |
      | joinedAt   | string  |

  # POST /api/organizations/{id}/members/invite
  @endpoint @organizations @members @not-implemented
  Scenario: Invite member to organization
    Given I am admin of organization "org-123"
    When I send a POST request to "/api/organizations/org-123/members/invite" with:
      | field | value                |
      | email | newuser@clinic.com   |
      | role  | therapist            |
    Then the response status should be 200
    And the response should contain:
      | field      | type   |
      | inviteId   | string |
      | status     | string |
      | expiresAt  | string |
    And an invitation email should be sent
    And the invite should expire in 7 days

  @endpoint @organizations @members @validation @not-implemented
  Scenario: Cannot invite existing member
    Given I am admin of organization "org-123"
    And "existing@clinic.com" is already a member
    When I send a POST request to "/api/organizations/org-123/members/invite" with:
      | field | value               |
      | email | existing@clinic.com |
      | role  | therapist           |
    Then the response status should be 400
    And the response should contain error "User is already a member"

  # DELETE /api/organizations/{id}/members/{userId}
  @endpoint @organizations @members @not-implemented
  Scenario: Remove member from organization
    Given I am admin of organization "org-123"
    And "user-456" is a member with role "therapist"
    When I send a DELETE request to "/api/organizations/org-123/members/user-456"
    Then the response status should be 200
    And the member should be removed
    And the member should receive a notification
    And their access should be revoked immediately

  @endpoint @organizations @members @validation @not-implemented
  Scenario: Cannot remove the last admin
    Given I am the only admin of organization "org-123"
    When I send a DELETE request to "/api/organizations/org-123/members/{my-user-id}"
    Then the response status should be 400
    And the response should contain error "Cannot remove the last admin"

  # PUT /api/organizations/{id}/members/{userId}/role
  @endpoint @organizations @members @not-implemented
  Scenario: Update member role
    Given I am admin of organization "org-123"
    And "user-456" is a member with role "therapist"
    When I send a PUT request to "/api/organizations/org-123/members/user-456/role" with:
      | field | value |
      | role  | admin |
    Then the response status should be 200
    And the member role should be updated
    And the member should be notified of the change

  # GET /api/organizations/{id}/billing
  @endpoint @organizations @billing @not-implemented
  Scenario: Get organization billing information
    Given I am owner of organization "org-123"
    When I send a GET request to "/api/organizations/org-123/billing"
    Then the response status should be 200
    And the response should contain:
      | field             | type    |
      | planId            | string  |
      | seats             | number  |
      | usedSeats         | number  |
      | billingCycle      | string  |
      | nextBillingDate   | string  |
      | paymentMethod     | object  |
      | billingAddress    | object  |

  # PUT /api/organizations/{id}/billing
  @endpoint @organizations @billing @not-implemented
  Scenario: Update billing information
    Given I am owner of organization "org-123"
    When I send a PUT request to "/api/organizations/org-123/billing" with:
      | field                  | value           |
      | billingAddress.street  | 456 New Ave     |
      | billingAddress.city    | New City        |
      | billingAddress.state   | NY              |
      | billingAddress.zip     | 10001           |
    Then the response status should be 200
    And the billing address should be updated
    And the changes should sync with Stripe