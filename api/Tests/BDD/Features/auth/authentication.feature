@collection:BDD_Sequential_Tests
Feature: Authentication API Endpoints (FR-001)
  As a therapy professional
  I want to authenticate and manage my account
  So that I can securely access the platform

  Background:
    Given the API is available
    And the database is connected

  # POST /api/auth/register
  @endpoint @auth @not-implemented
  Scenario: Successfully register a new user
    Given I have valid registration details:
      | field           | value                |
      | email           | john.doe@clinic.com  |
      | password        | SecurePass123!       |
      | confirmPassword | SecurePass123!       |
      | firstName       | John                 |
      | lastName        | Doe                  |
      | licenseNumber   | OT-12345            |
      | licenseState    | CA                   |
      | licenseType     | OT                   |
      | phone           | 555-123-4567        |
      | acceptedTerms   | true                 |
    When I send a POST request to "/api/auth/register"
    Then the response status should be 200
    And the response should contain:
      | field    | type    |
      | success  | boolean |
      | message  | string  |
      | userId   | string  |
    And a verification email should be sent
    And the user should be created in the database

  @endpoint @auth @error @not-implemented
  Scenario: Fail to register with existing email
    Given a user exists with email "existing@clinic.com"
    And I have registration details with email "existing@clinic.com"
    When I send a POST request to "/api/auth/register"
    Then the response status should be 400
    And the response should contain error "Email already registered"

  @endpoint @auth @validation @not-implemented
  Scenario Outline: Validate registration fields
    Given I have registration details with <field> set to "<value>"
    When I send a POST request to "/api/auth/register"
    Then the response status should be 400
    And the response should contain error "<error>"

    Examples:
      | field          | value        | error                          |
      | email          | invalid      | Invalid email format           |
      | email          |              | Email is required              |
      | password       | weak         | Password too weak              |
      | password       |              | Password is required           |
      | firstName      |              | First name is required         |
      | lastName       |              | Last name is required          |
      | licenseNumber  |              | License number is required     |
      | acceptedTerms  | false        | Terms must be accepted         |

  # POST /api/auth/login
  @endpoint @auth @not-implemented
  Scenario: Successfully login with valid credentials
    Given a verified user exists with:
      | email    | user@clinic.com |
      | password | SecurePass123!  |
    When I send a POST request to "/api/auth/login" with:
      | field    | value           |
      | email    | user@clinic.com |
      | password | SecurePass123!  |
    Then the response status should be 200
    And the response should contain:
      | field        | type    |
      | success      | boolean |
      | token        | string  |
      | refreshToken | string  |
      | user         | object  |
    And the JWT token should be valid
    And the refresh token should be stored

  @endpoint @auth @error @not-implemented
  Scenario: Fail to login with invalid password
    Given a verified user exists with email "user@clinic.com"
    When I send a POST request to "/api/auth/login" with:
      | field    | value           |
      | email    | user@clinic.com |
      | password | WrongPassword   |
    Then the response status should be 400
    And the response should contain error "Invalid credentials"
    And the failed attempt should be logged

  @endpoint @auth @security @not-implemented
  Scenario: Account lockout after failed attempts
    Given a verified user exists with email "user@clinic.com"
    And the account has 4 failed login attempts
    When I send a POST request to "/api/auth/login" with invalid credentials
    Then the response status should be 429
    And the response should contain error "Account temporarily locked"
    And the account should be locked for 15 minutes

  # GET /api/auth/verify-email/{token}
  @endpoint @auth @not-implemented
  Scenario: Successfully verify email with valid token
    Given an unverified user exists with verification token "valid-token-123"
    When I send a GET request to "/api/auth/verify-email/valid-token-123"
    Then the response status should be 200
    And the response should contain:
      | field   | value                    |
      | success | true                     |
      | message | Email verified successfully |
    And the user should be marked as verified
    And the token should be marked as used

  @endpoint @auth @error @not-implemented
  Scenario: Fail to verify with invalid token
    When I send a GET request to "/api/auth/verify-email/invalid-token"
    Then the response status should be 400
    And the response should contain error "Invalid or expired token"

  @endpoint @auth @error @not-implemented
  Scenario: Fail to verify with expired token
    Given an unverified user exists with expired token "expired-token-123"
    When I send a GET request to "/api/auth/verify-email/expired-token-123"
    Then the response status should be 400
    And the response should contain error "Invalid or expired token"

  # POST /api/auth/resend-verification
  @endpoint @auth @not-implemented
  Scenario: Successfully resend verification email
    Given an unverified user exists with email "unverified@clinic.com"
    When I send a POST request to "/api/auth/resend-verification" with:
      | field | value                  |
      | email | unverified@clinic.com  |
    Then the response status should be 200
    And the response should contain message "Verification email sent"
    And a new verification email should be sent
    And the old token should be invalidated

  @endpoint @auth @rate-limit @not-implemented
  Scenario: Rate limit resend verification requests
    Given an unverified user exists with email "unverified@clinic.com"
    And a verification email was sent 30 seconds ago
    When I send a POST request to "/api/auth/resend-verification" with the same email
    Then the response status should be 429
    And the response should contain error "Please wait before requesting another email"

  # POST /api/auth/logout
  @endpoint @auth @not-implemented
  Scenario: Successfully logout authenticated user
    Given I am authenticated as "user@clinic.com"
    When I send a POST request to "/api/auth/logout"
    Then the response status should be 200
    And the response should contain message "Logged out successfully"
    And the refresh token should be revoked
    And the session should be terminated

  # POST /api/auth/refresh-token
  @endpoint @auth @not-implemented
  Scenario: Successfully refresh authentication token
    Given I have a valid refresh token "refresh-token-123"
    When I send a POST request to "/api/auth/refresh-token" with:
      | field        | value              |
      | refreshToken | refresh-token-123  |
    Then the response status should be 200
    And the response should contain:
      | field        | type   |
      | token        | string |
      | refreshToken | string |
    And the old refresh token should be revoked
    And the new tokens should be valid

  # POST /api/auth/forgot-password
  @endpoint @auth @not-implemented
  Scenario: Successfully request password reset
    Given a user exists with email "user@clinic.com"
    When I send a POST request to "/api/auth/forgot-password" with:
      | field | value           |
      | email | user@clinic.com |
    Then the response status should be 200
    And the response should contain message "Password reset email sent"
    And a password reset email should be sent
    And a reset token should be generated

  @endpoint @auth @security @not-implemented
  Scenario: Prevent user enumeration on password reset
    When I send a POST request to "/api/auth/forgot-password" with:
      | field | value                |
      | email | nonexistent@test.com |
    Then the response status should be 200
    And the response should contain message "Password reset email sent"
    And the response time should be similar to successful requests

  # POST /api/auth/reset-password
  @endpoint @auth @not-implemented
  Scenario: Successfully reset password with valid token
    Given a password reset token "reset-token-123" exists for "user@clinic.com"
    When I send a POST request to "/api/auth/reset-password" with:
      | field           | value           |
      | token           | reset-token-123 |
      | newPassword     | NewSecure123!   |
      | confirmPassword | NewSecure123!   |
    Then the response status should be 200
    And the response should contain message "Password reset successfully"
    And the user should be able to login with the new password
    And the reset token should be invalidated
    And a confirmation email should be sent

  # POST /api/auth/change-password
  @endpoint @auth @not-implemented
  Scenario: Successfully change password when authenticated
    Given I am authenticated as "user@clinic.com"
    When I send a POST request to "/api/auth/change-password" with:
      | field           | value          |
      | currentPassword | OldPass123!    |
      | newPassword     | NewPass123!    |
      | confirmPassword | NewPass123!    |
    Then the response status should be 200
    And the response should contain message "Password changed successfully"
    And the password history should be updated
    And all sessions should be terminated except current

  @endpoint @auth @validation @not-implemented
  Scenario: Prevent password reuse
    Given I am authenticated as "user@clinic.com"
    And my last 5 passwords included "OldPass123!"
    When I send a POST request to "/api/auth/change-password" with:
      | field           | value       |
      | currentPassword | Current123! |
      | newPassword     | OldPass123! |
      | confirmPassword | OldPass123! |
    Then the response status should be 400
    And the response should contain error "Password has been used recently"

  # POST /api/auth/mfa/setup
  @endpoint @auth @mfa @not-implemented
  Scenario: Successfully setup MFA
    Given I am authenticated as "user@clinic.com"
    When I send a POST request to "/api/auth/mfa/setup"
    Then the response status should be 200
    And the response should contain:
      | field      | type   |
      | secret     | string |
      | qrCode     | string |
      | backupCodes| array  |
    And MFA should be pending verification

  # POST /api/auth/mfa/verify
  @endpoint @auth @mfa @not-implemented
  Scenario: Successfully verify and enable MFA
    Given I am authenticated as "user@clinic.com"
    And I have MFA setup pending with secret "secret123"
    When I send a POST request to "/api/auth/mfa/verify" with:
      | field | value  |
      | code  | 123456 |
    Then the response status should be 200
    And the response should contain message "MFA enabled successfully"
    And MFA should be active for the account

  # POST /api/auth/mfa/disable
  @endpoint @auth @mfa @not-implemented
  Scenario: Successfully disable MFA
    Given I am authenticated as "user@clinic.com"
    And I have MFA enabled
    When I send a POST request to "/api/auth/mfa/disable" with:
      | field    | value       |
      | password | Current123! |
      | code     | 123456      |
    Then the response status should be 200
    And the response should contain message "MFA disabled successfully"
    And MFA should be inactive for the account

  # GET /api/auth/sso/providers
  @endpoint @auth @sso @not-implemented
  Scenario: Get list of available SSO providers
    When I send a GET request to "/api/auth/sso/providers"
    Then the response status should be 200
    And the auth response should contain array of:
      | field    | type    |
      | id       | string  |
      | name     | string  |
      | type     | string  |
      | enabled  | boolean |
    And should include providers like "google", "clever", "classlink"

  # GET /api/auth/sso/{provider}/redirect
  @endpoint @auth @sso @not-implemented
  Scenario: Get SSO redirect URL for Google
    When I send a GET request to "/api/auth/sso/google/redirect"
    Then the response status should be 302
    And the location header should contain "accounts.google.com"
    And the redirect should include proper OAuth parameters

  # POST /api/auth/sso/{provider}/callback
  @endpoint @auth @sso @not-implemented
  Scenario: Handle SSO callback from Google
    Given Google returns a valid OAuth code "auth-code-123"
    When I send a POST request to "/api/auth/sso/google/callback" with:
      | field | value          |
      | code  | auth-code-123  |
      | state | state-token    |
    Then the response status should be 200
    And the response should contain:
      | field        | type    |
      | success      | boolean |
      | token        | string  |
      | refreshToken | string  |
      | user         | object  |
    And the user should be created or updated
    And the SSO link should be established

  # License Verification Scenarios
  @endpoint @auth @license-verification @not-implemented
  Scenario: Verify therapist license with state board during registration
    Given I have valid registration details:
      | field           | value                |
      | email           | jane.smith@clinic.com|
      | password        | SecurePass123!       |
      | confirmPassword | SecurePass123!       |
      | firstName       | Jane                 |
      | lastName        | Smith                |
      | licenseNumber   | PT-67890            |
      | licenseState    | CA                   |
      | licenseType     | PT                   |
      | phone           | 555-987-6543        |
      | acceptedTerms   | true                 |
    When I send a POST request to "/api/auth/register"
    Then the system should verify license "PT-67890" with "CA" state board
    And the license verification should return:
      | field             | value                |
      | isValid           | true                 |
      | licenseStatus     | Active               |
      | expirationDate    | 2025-12-31          |
      | disciplinaryAction| false                |
    And the response status should be 200
    And the license verification should be recorded in the database

  @endpoint @auth @license-verification @error @not-implemented
  Scenario: Reject registration with invalid license
    Given I have registration details with invalid license "INVALID-123"
    When I send a POST request to "/api/auth/register"
    Then the system should verify the license with state board
    And the license verification should fail
    And the response status should be 400
    And the response should contain error "License verification failed"
    And the registration should not proceed

  @endpoint @auth @license-verification @not-implemented
  Scenario: Register with multiple state licenses
    Given I have valid registration details
    And I have multiple licenses:
      | state | licenseNumber | licenseType |
      | CA    | OT-12345     | OT          |
      | AZ    | OT-54321     | OT          |
      | NV    | OT-98765     | OT          |
    When I send a POST request to "/api/auth/register"
    Then the system should verify all licenses with respective state boards
    And all licenses should be valid
    And the response status should be 200
    And all licenses should be stored in professional_licenses table

  # HIPAA Compliance During Registration
  @endpoint @auth @hipaa @security @not-implemented
  Scenario: Ensure HIPAA-compliant registration data handling
    Given I have valid registration details with PHI-related fields
    When I send a POST request to "/api/auth/register"
    Then all data transmission should use TLS 1.3
    And sensitive fields should be encrypted with AES-256:
      | field         | encryptionStatus |
      | firstName     | encrypted        |
      | lastName      | encrypted        |
      | phone         | encrypted        |
      | licenseNumber | encrypted        |
    And the registration audit log should contain:
      | field               | value                    |
      | action              | user_registration        |
      | encryptionVerified  | true                    |
      | dataClassification  | PHI                     |
      | complianceStandard  | HIPAA                   |
    And no sensitive data should be logged in plain text

  @endpoint @auth @hipaa @audit @not-implemented
  Scenario: Create HIPAA-compliant audit trail for registration
    Given HIPAA audit logging is enabled
    When I send a POST request to "/api/auth/register" with valid data
    Then the audit log should record:
      | field             | requirement                      |
      | timestamp         | ISO 8601 format with timezone   |
      | userId            | Anonymized until verified       |
      | action            | registration_attempt            |
      | ipAddress         | Hashed for privacy             |
      | userAgent         | Recorded                       |
      | dataAccessed      | List of fields accessed        |
      | outcome           | success/failure                |
    And the audit log should be immutable
    And the audit log should be retained for 7 years

  # Subscription Selection During Registration
  @endpoint @auth @subscription @not-implemented
  Scenario: Select subscription tier during registration flow
    Given I have successfully registered and verified my email
    When I am redirected to subscription selection
    Then I should see available tiers:
      | tier         | price        | features                        |
      | Basic        | $9.95/month  | 10 resources/month, basic tools |
      | Professional | $19.95/month | Unlimited resources, all tools  |
      | Team         | $15/user/mo  | 5+ users, admin dashboard      |
    When I select "Professional" tier
    And I provide payment information
    Then my subscription should be activated
    And I should have access to Professional features
    And the billing should start immediately

  @endpoint @auth @subscription @trial @not-implemented
  Scenario: Start with free trial before subscription
    Given I have successfully registered as a new therapist
    When I am prompted for subscription selection
    And I choose "Start 14-day free trial"
    Then my account should be provisioned with:
      | field            | value                |
      | subscriptionType | trial               |
      | trialEndDate     | 14 days from today  |
      | accessLevel      | professional        |
      | paymentRequired  | false               |
    And I should receive trial reminder emails at:
      | daysBeforeEnd | emailType              |
      | 7             | trial_halfway          |
      | 3             | trial_ending_soon      |
      | 1             | trial_last_day         |
    And trial limitations should apply:
      | limitation        | value |
      | aiGenerations     | 10    |
      | marketplaceSales  | 0     |

  # Professional Registration Details
  @endpoint @auth @professional-details @not-implemented
  Scenario: Register with complete professional details
    Given I am registering as a therapist
    When I provide professional details:
      | field                   | value                        |
      | npiNumber              | 1234567890                   |
      | insuranceProviderIds   | BCBS:12345,Aetna:67890      |
      | specializations        | Pediatric,Sensory Integration|
      | yearsOfExperience      | 8                           |
      | workSettings           | School,Private Practice      |
      | certifications         | SIPT,NDT                    |
      | continuingEdInterests  | Autism,Feeding              |
    Then all professional details should be validated:
      | field      | validation                    |
      | npiNumber  | Valid format and checksum    |
      | experience | Positive integer             |
      | specialties| From approved list           |
    And the registration should proceed
    And professional profile should be created

  @endpoint @auth @professional-details @verification @not-implemented
  Scenario: Verify professional certifications during registration
    Given I claim to have "SIPT" certification
    When I provide certification details:
      | field              | value               |
      | certificateNumber  | SIPT-2023-1234     |
      | issuingBody       | USC/WPS            |
      | issueDate         | 2023-01-15         |
      | expirationDate    | 2026-01-15         |
    Then the system should verify with issuing body
    And the verification should return:
      | field     | value    |
      | valid     | true     |
      | current   | true     |
      | holder    | Matches registrant name |
    And certification should be added to profile

  # State-Specific Requirements
  @endpoint @auth @state-requirements @not-implemented
  Scenario: Handle state-specific registration requirements
    Given I am registering with a "TX" license
    When the system checks state-specific requirements
    Then additional fields should be required:
      | field                    | requirement         |
      | continuingEdHours       | 30 hours/2 years   |
      | jurisprudenceExam      | Passed within 1 year|
      | liabilityInsurance     | $1M/$3M minimum    |
    And I must acknowledge state-specific terms:
      | acknowledgment                                      |
      | I understand TX supervision requirements           |
      | I will maintain required CE hours                  |
      | I will report any disciplinary actions            |
    And state-specific validations should apply