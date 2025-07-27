Feature: User Management API Endpoints (FR-001)
  As a platform user
  I want to manage my profile and preferences
  So that I can customize my experience and access enterprise features

  Background:
    Given the API is available
    And I am authenticated as "user@clinic.com"

  # GET /api/users/profile
  @endpoint @users @not-implemented
  Scenario: Successfully get current user profile
    When I send a GET request to "/api/users/profile"
    Then the response status should be 200
    And the response should contain:
      | field         | type    |
      | id            | string  |
      | email         | string  |
      | firstName     | string  |
      | lastName      | string  |
      | licenseNumber | string  |
      | licenseState  | string  |
      | licenseType   | string  |
      | phone         | string  |
      | createdAt     | string  |
      | verified      | boolean |
      | subscription  | object  |

  # PUT /api/users/profile
  @endpoint @users @not-implemented
  Scenario: Successfully update user profile
    When I send a PUT request to "/api/users/profile" with:
      | field     | value        |
      | firstName | Jane         |
      | lastName  | Smith        |
      | phone     | 555-987-6543 |
    Then the response status should be 200
    And the response should contain updated profile
    And the audit log should record the changes

  @endpoint @users @validation @not-implemented
  Scenario: Prevent updating protected fields
    When I send a PUT request to "/api/users/profile" with:
      | field | value              |
      | email | newemail@test.com  |
      | id    | different-id       |
    Then the response status should be 400
    And the response should contain error "Cannot update protected fields"

  # DELETE /api/users/profile
  @endpoint @users @not-implemented
  Scenario: Successfully delete user account
    When I send a DELETE request to "/api/users/profile" with:
      | field    | value       |
      | password | Current123! |
      | confirm  | DELETE      |
    Then the response status should be 200
    And the response should contain message "Account deleted successfully"
    And the user should be marked as deleted
    And personal data should be anonymized
    And an account deletion email should be sent

  # GET /api/users/{id} (Admin only)
  @endpoint @users @admin @not-implemented
  Scenario: Admin successfully gets user by ID
    Given I am authenticated as an admin
    And a user exists with id "user-123"
    When I send a GET request to "/api/users/user-123"
    Then the response status should be 200
    And the response should contain full user details
    And sensitive data should be masked appropriately

  @endpoint @users @authorization @not-implemented
  Scenario: Non-admin cannot access other user profiles
    Given I am authenticated as a regular user
    When I send a GET request to "/api/users/other-user-id"
    Then the response status should be 403
    And the response should contain error "Insufficient permissions"

  # GET /api/users (Admin only)
  @endpoint @users @admin @not-implemented
  Scenario: Admin lists users with pagination
    Given I am authenticated as an admin
    And there are 50 users in the system
    When I send a GET request to "/api/users?page=1&limit=20"
    Then the response status should be 200
    And the response should contain:
      | field      | type    |
      | users      | array   |
      | total      | number  |
      | page       | number  |
      | limit      | number  |
      | totalPages | number  |
    And the users array should contain 20 items

  @endpoint @users @admin @filtering @not-implemented
  Scenario: Admin filters users by criteria
    Given I am authenticated as an admin
    When I send a GET request to "/api/users?licenseType=OT&verified=true"
    Then the response status should be 200
    And all returned users should have licenseType "OT"
    And all returned users should be verified

  # PUT /api/users/{id}/status (Admin only)
  @endpoint @users @admin @not-implemented
  Scenario: Admin suspends a user account
    Given I am authenticated as an admin
    And a user exists with id "user-123"
    When I send a PUT request to "/api/users/user-123/status" with:
      | field  | value     |
      | status | suspended |
      | reason | Terms violation |
    Then the response status should be 200
    And the user should be suspended
    And the user should receive a suspension notification
    And the action should be logged

  # GET /api/users/licenses
  @endpoint @users @licenses @not-implemented
  Scenario: Get user's professional licenses
    When I send a GET request to "/api/users/licenses"
    Then the response status should be 200
    And the auth response should contain array of:
      | field          | type    |
      | licenseNumber  | string  |
      | licenseState   | string  |
      | licenseType    | string  |
      | expirationDate | string  |
      | verified       | boolean |
      | verifiedAt     | string  |

  # POST /api/users/licenses/verify
  @endpoint @users @licenses @not-implemented
  Scenario: Add and verify a new license
    When I send a POST request to "/api/users/licenses/verify" with:
      | field         | value     |
      | licenseNumber | PT-98765  |
      | licenseState  | NY        |
      | licenseType   | PT        |
    Then the response status should be 200
    And the response should contain:
      | field    | value |
      | verified | true  |
    And the license should be verified with external API
    And the license should be added to user profile

  @endpoint @users @licenses @validation @not-implemented
  Scenario: Reject invalid license
    When I send a POST request to "/api/users/licenses/verify" with:
      | field         | value      |
      | licenseNumber | INVALID-123|
      | licenseState  | XX         |
      | licenseType   | OT         |
    Then the response status should be 400
    And the response should contain error "License verification failed"

  # PUT /api/users/preferences
  @endpoint @users @preferences @not-implemented
  Scenario: Update user preferences
    When I send a PUT request to "/api/users/preferences" with:
      | field              | value    |
      | language           | es       |
      | timezone           | EST      |
      | emailNotifications | true     |
      | theme              | dark     |
      | defaultView        | calendar |
    Then the response status should be 200
    And the preferences should be saved
    And the UI should reflect the new preferences

  # GET /api/users/notifications
  @endpoint @users @notifications @not-implemented
  Scenario: Get notification settings
    When I send a GET request to "/api/users/notifications"
    Then the response status should be 200
    And the response should contain:
      | field               | type    |
      | emailNotifications  | object  |
      | pushNotifications   | object  |
      | smsNotifications    | object  |
      | notificationSchedule| object  |

  # PUT /api/users/notifications
  @endpoint @users @notifications @not-implemented
  Scenario: Update notification settings
    When I send a PUT request to "/api/users/notifications" with:
      | path                           | value |
      | emailNotifications.newResources| true  |
      | emailNotifications.weeklyDigest| false |
      | pushNotifications.sessionReminders| true |
      | notificationSchedule.quietHours| 22:00-07:00 |
    Then the response status should be 200
    And the notification settings should be updated
    And future notifications should respect these settings

  # FR-001 Missing Critical Enterprise SSO Scenarios
  @enterprise @sso @clever-integration @not-implemented
  Scenario: Enterprise SSO login through Clever
    Given "Riverside School District" has Enterprise subscription
    And Clever SSO integration is configured for the district
    When a therapist visits the UPTRMS login page
    And they select "Login with School District"
    And they enter district identifier "riverside-unified"
    Then they should be redirected to Clever authentication
    When they complete Clever login with valid credentials
    Then they should be redirected back to UPTRMS
    And their account should be automatically provisioned with:
      | Field          | Value                        |
      | Email          | From Clever directory        |
      | Name           | From Clever directory        |
      | Role           | Therapist                    |
      | Organization   | Riverside School District    |
      | Subscription   | Enterprise                   |
      | Permissions    | Full platform access         |
    And they should be logged in automatically
    And their session should sync with district policies

  @enterprise @sso @classlink-integration @not-implemented
  Scenario: Enterprise SSO login through ClassLink
    Given "Metro Health District" has Enterprise subscription
    And ClassLink SSO integration is configured
    When a therapist accesses UPTRMS through ClassLink portal
    Then they should be automatically authenticated via SAML 2.0
    And their district role should be mapped to UPTRMS permissions:
      | ClassLink Role     | UPTRMS Permission Level      |
      | Therapist          | Full platform access         |
      | Therapy Supervisor | Admin dashboard access        |
      | District Admin     | Organization admin rights     |
    And their caseload should be automatically populated from district SIS
    And billing should be handled at organization level

  @enterprise @pricing @custom-pricing @not-implemented
  Scenario: Enterprise custom pricing and billing
    Given "Large Health System" has 150 therapy professionals
    And they are negotiating Enterprise subscription
    When they request custom pricing for 150+ users
    Then the system should support:
      | Feature              | Requirement                   |
      | Custom pricing tiers | Volume discounts available    |
      | Flexible billing     | Annual, multi-year options    |
      | Usage-based pricing  | Charge per active user        |
      | Bulk user management | CSV import/export             |
      | Dedicated support    | Priority support channel      |
      | SLA guarantees       | 99.9% uptime commitment       |
    And contract terms should be customizable
    And billing should be handled through enterprise procurement

  @enterprise @user-provisioning @directory-sync @not-implemented
  Scenario: Automatic user provisioning from enterprise directory
    Given an Enterprise organization has Active Directory integration
    When a new therapist is added to the directory
    Then their UPTRMS account should be automatically created
    And their profile should be populated from directory attributes:
      | Directory Field    | UPTRMS Field                |
      | employeeId         | External ID                 |
      | mail               | Email                       |
      | givenName          | First Name                  |
      | sn                 | Last Name                   |
      | department         | Department                  |
      | title              | Job Title                   |
      | manager            | Reports To                  |
    When the therapist is deactivated in the directory
    Then their UPTRMS account should be automatically suspended
    And their data should be preserved according to retention policies

  @enterprise @multi-tenant @organization-isolation @not-implemented
  Scenario: Multi-tenant organization data isolation
    Given two enterprise organizations share the same UPTRMS instance
    When "Hospital A" therapist searches for resources
    Then they should only see resources available to their organization
    And they should not see "Hospital B" private resources
    And billing should be tracked separately per organization
    And usage analytics should be isolated per organization
    And each organization should have independent admin controls

  @enterprise @subscription-management @organization-admin @not-implemented
  Scenario: Organization admin manages enterprise subscription
    Given I am an organization admin for "Metro Therapy Network"
    When I access the organization admin dashboard
    Then I should be able to:
      | Action                    | Capability                    |
      | View usage analytics      | See all therapist activity    |
      | Manage user licenses      | Add/remove/modify users       |
      | Configure SSO settings    | Manage authentication methods |
      | Set organization policies | Content access, data retention|
      | Monitor billing usage     | Track costs and usage limits  |
      | Generate compliance reports| HIPAA, audit trails         |
    And I should receive monthly usage reports
    And I should be able to adjust subscription levels

  @enterprise @marketplace @commission-rates @not-implemented
  Scenario: Enterprise marketplace commission structure
    Given "Education Cooperative" has Enterprise subscription
    And they negotiate custom marketplace terms
    When their therapists sell resources in the marketplace
    Then the commission structure should be:
      | Transaction Type   | Enterprise Rate | Standard Rate |
      | Internal sales     | 20% commission  | 30% commission|
      | External sales     | 25% commission  | 30% commission|
      | Bulk purchases     | 15% commission  | 30% commission|
    And enterprise organizations should get volume discounts
    And revenue sharing should be tracked separately
    And tax handling should comply with enterprise accounting

  @enterprise @compliance @audit-requirements @not-implemented
  Scenario: Enterprise compliance and audit requirements
    Given "Healthcare Network" has strict compliance requirements
    When they implement UPTRMS Enterprise
    Then the system should provide:
      | Compliance Feature | Requirement                   |
      | Audit logging      | Comprehensive activity logs   |
      | Data encryption    | End-to-end encryption         |
      | Access controls    | Role-based permissions        |
      | Backup procedures  | Automated daily backups       |
      | Incident response  | 24/7 monitoring and alerts    |
      | Compliance reports | SOC 2, HIPAA, state regulations|
    And compliance officers should have read-only access to all data
    And audit trails should be immutable and exportable
    And compliance reports should be generated automatically

  @enterprise @performance @scalability @not-implemented
  Scenario: Enterprise performance and scalability requirements
    Given "State Department of Education" has 5000+ therapy professionals
    When they access the system during peak hours (8-10 AM)
    Then the system should handle:
      | Performance Metric | Requirement                   |
      | Concurrent users   | 5000+ simultaneous users     |
      | Response times     | <2 seconds for all operations |
      | Database queries   | <500ms for complex searches   |
      | File uploads       | 100MB files without timeout   |
      | Report generation  | <30 seconds for org-wide reports|
    And auto-scaling should handle traffic spikes
    And performance should be monitored and reported
    And SLA violations should trigger automatic alerts

  # FR-001 Comprehensive Subscription Workflow Scenarios from CLAUDE.md
  @subscription @individual @workflow @not-implemented
  Scenario: New user registration with email verification
    Given I am on the registration page
    When I enter valid registration details:
      | Field          | Value                |
      | Email          | therapist@clinic.com |
      | Password       | SecurePass123!       |
      | First Name     | Sarah               |
      | Last Name      | Johnson             |
      | License Number | OT-12345            |
      | Specialty      | Pediatric OT        |
    And I accept the terms and conditions
    And I submit the registration form
    Then I should receive a verification email
    And the email should contain a verification link
    When I click the verification link
    Then my account should be activated
    And I should be redirected to the subscription selection page

  @subscription @individual @payment @not-implemented
  Scenario: Individual therapist subscribes to Pro tier
    Given I am a verified user
    And I am on the subscription selection page
    When I select the "Pro" subscription tier
    And I enter valid payment information
    And I confirm the subscription
    Then my subscription should be activated immediately
    And I should have access to all Pro features
    And I should receive a subscription confirmation email
    And my first billing date should be set for today
    And my next billing date should be one month from today

  @subscription @group @administration @not-implemented
  Scenario: Practice owner sets up Small Group subscription
    Given I am a verified practice owner
    When I select the "Small Group" subscription tier
    And I specify 10 user licenses
    And I enter my practice details:
      | Field           | Value                    |
      | Practice Name   | Sunshine Therapy Center  |
      | Tax ID          | 12-3456789              |
      | Billing Address | 123 Main St, City, ST   |
    And I provide payment information
    Then the monthly cost should be calculated as $150
    And I should be able to invite team members
    And I should have access to the admin dashboard
    And each invited user should receive an invitation email

  @subscription @upgrade @workflow @not-implemented
  Scenario: Individual upgrades to group subscription
    Given I have an active Individual Pro subscription
    And I want to add team members
    When I select "Upgrade to Group"
    And I choose Small Group (5-20 users)
    And I add 3 team members:
      | Name          | Email                | Role      |
      | Dr. Smith     | smith@clinic.com     | Therapist |
      | Jane Doe      | jane@clinic.com      | Assistant |
      | Mike Johnson  | mike@clinic.com      | Therapist |
    Then my subscription should upgrade immediately
    And prorated billing should be calculated
    And team members should receive invitation emails
    And I should have admin privileges
    And billing should change to group rate

  @subscription @enterprise @sso @not-implemented
  Scenario: School district implements Enterprise SSO
    Given "Metro School District" contacts sales
    And they have 150 therapy professionals
    When they sign Enterprise agreement
    And SSO is configured with:
      | Provider | Google Workspace        |
      | Domain   | metroschools.edu       |
      | Method   | SAML 2.0              |
    Then all district therapists should login via SSO
    And user provisioning should sync automatically
    And usage analytics should be available
    And custom pricing should be applied

  @subscription @free-tier @limitations @not-implemented
  Scenario: Free tier user experiences limitations
    Given I am using the free tier
    When I try to access resources
    Then I should be limited to 10 resources per month
    And I should not have access to:
      | Restricted Feature | Reason                    |
      | Data collection   | Pro feature only          |
      | AI generation     | Pro feature only          |
      | Marketplace selling| Pro feature only          |
      | Premium resources | Pro feature only          |
    When I reach 10 resource downloads
    Then I should see upgrade prompts
    And further downloads should be blocked
    And I should be offered subscription options

  @subscription @marketplace @commission @not-implemented
  Scenario: Marketplace seller fee calculations
    Given I am a Pro subscriber selling resources
    When I sell a resource for $19.99
    Then the platform should deduct 30% commission
    And I should receive $13.99
    And the transaction should be recorded as:
      | Field          | Value    |
      | Gross Sale     | $19.99   |
      | Platform Fee   | $6.00    |
      | Seller Earnings| $13.99   |
    And I should receive payment within 7 days
    And tax reporting should be handled automatically

  @subscription @expiration @renewal @not-implemented
  Scenario: Subscription expiration and renewal handling
    Given I have a monthly subscription
    And my subscription expires tomorrow
    When the renewal date arrives
    Then the system should attempt automatic renewal
    And handle payment success with:
      | Action              | Result                        |
      | Extend subscription | Add 30 days to expiration     |
      | Send confirmation   | Email receipt                 |
      | Continue access     | No service interruption       |
    And handle payment failure with:
      | Action              | Result                        |
      | Send payment alert  | Email with update payment link|
      | Grace period        | 7 days continued access       |
      | Downgrade warning   | Email at 5 days remaining     |
      | Account suspension  | After 7 days if not resolved  |

  @subscription @admin @dashboard @not-implemented
  Scenario: Group admin manages subscription dashboard
    Given I am a group administrator
    When I access the admin dashboard
    Then I should see subscription overview:
      | Metric              | Display                       |
      | Active users        | 12 of 15 licenses used       |
      | Monthly cost        | $180 (12 × $15)              |
      | Next billing date   | March 15, 2024               |
      | Usage this month    | 1,247 resource downloads     |
    And I should be able to:
      | Admin Action        | Capability                    |
      | Add/remove users    | Manage team members           |
      | View usage reports  | Individual usage statistics   |
      | Manage billing      | Update payment methods        |
      | Set permissions     | Control feature access        |
    And I should receive monthly usage reports

  @subscription @cancellation @workflow @not-implemented
  Scenario: Subscription cancellation with data retention
    Given I have an active subscription
    When I request subscription cancellation
    Then I should be presented with:
      | Cancellation Option | Description                   |
      | Immediate           | Cancel now, lose access today |
      | End of period       | Cancel at next billing cycle |
      | Pause subscription  | Temporarily suspend account   |
    When I confirm cancellation
    Then my subscription should end at period end
    And I should retain access until expiration
    And my data should be preserved for 90 days
    And I should receive confirmation email
    And I should be offered win-back incentives