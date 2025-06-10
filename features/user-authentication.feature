Feature: User Authentication
  As a therapist
  I want to securely authenticate with the system
  So that I can access my patients and documentation

  Background:
    Given the database is initialized
    And the authentication system is running

  Rule: User registration requires valid data

    Scenario: Successful therapist registration
      Given no user exists with email "sarah.johnson@therapy.com"
      When I register with the following details:
        | Field          | Value                    |
        | Email          | sarah.johnson@therapy.com |
        | Password       | SecurePass123!           |
        | First Name     | Sarah                    |
        | Last Name      | Johnson                  |
        | Service Type   | occupational_therapy     |
        | License Number | OT12345                  |
        | License State  | TX                       |
      Then the registration is successful
      And a user account is created with email "sarah.johnson@therapy.com"
      And the user has role "therapist"
      And the user has subscription tier "basic"
      And the password is securely hashed
      And an audit log entry is created for "user_registered"

    Scenario: Registration with duplicate email fails
      Given a user exists with email "existing@therapy.com"
      When I attempt to register with email "existing@therapy.com"
      Then the registration fails with error "Email already registered"
      And no new user account is created

    Scenario Outline: Registration with invalid data
      Given no user exists with email "<email>"
      When I attempt to register with:
        | Field        | Value        |
        | Email        | <email>      |
        | Password     | <password>   |
        | First Name   | <firstName>  |
        | Last Name    | <lastName>   |
        | Service Type | <service>    |
      Then the registration fails with error "<error>"

      Examples:
        | email                | password    | firstName | lastName | service              | error                          |
        |                     | Pass123!    | John      | Doe      | occupational_therapy | Email is required              |
        | invalid-email       | Pass123!    | John      | Doe      | occupational_therapy | Invalid email format           |
        | john@therapy.com    | weak        | John      | Doe      | occupational_therapy | Password too weak              |
        | john@therapy.com    | Pass123!    |           | Doe      | occupational_therapy | First name is required         |
        | john@therapy.com    | Pass123!    | John      |          | occupational_therapy | Last name is required          |
        | john@therapy.com    | Pass123!    | John      | Doe      | invalid_service      | Invalid service type           |

  Rule: Users must authenticate with valid credentials

    Scenario: Successful login
      Given a user exists with:
        | Email    | john@therapy.com |
        | Password | SecurePass123!   |
        | Active   | true            |
      When I login with email "john@therapy.com" and password "SecurePass123!"
      Then the login is successful
      And I receive a JWT access token
      And the token expires in 30 minutes
      And the user's last login timestamp is updated
      And an audit log entry is created for "user_login"

    Scenario: Login with incorrect password
      Given a user exists with email "john@therapy.com"
      When I login with email "john@therapy.com" and password "WrongPassword"
      Then the login fails with error "Invalid credentials"
      And no access token is provided
      And an audit log entry is created for "failed_login_attempt"

    Scenario: Login with non-existent email
      Given no user exists with email "ghost@therapy.com"
      When I login with email "ghost@therapy.com" and password "AnyPassword"
      Then the login fails with error "Invalid credentials"
      And no access token is provided

    Scenario: Login with inactive account
      Given a user exists with:
        | Email    | inactive@therapy.com |
        | Password | SecurePass123!       |
        | Active   | false               |
      When I login with email "inactive@therapy.com" and password "SecurePass123!"
      Then the login fails with error "Account is inactive"
      And no access token is provided

  Rule: Password reset must be secure

    Scenario: Request password reset
      Given a user exists with email "forgot@therapy.com"
      When I request a password reset for "forgot@therapy.com"
      Then a password reset token is generated
      And the token expires in 1 hour
      And an email is sent to "forgot@therapy.com" with reset link
      And an audit log entry is created for "password_reset_requested"

    Scenario: Reset password with valid token
      Given a user has a valid reset token "valid-token-123"
      When I reset the password with token "valid-token-123" and new password "NewSecure123!"
      Then the password is successfully changed
      And the reset token is invalidated
      And the user can login with the new password
      And an audit log entry is created for "password_reset_completed"

    Scenario: Reset password with expired token
      Given a user has an expired reset token "expired-token-123"
      When I attempt to reset the password with token "expired-token-123"
      Then the reset fails with error "Token expired"
      And the password remains unchanged

  Rule: Authentication tokens must be managed securely

    Scenario: Access protected resource with valid token
      Given I have a valid JWT token for user "john@therapy.com"
      When I request my user profile
      Then the request is successful
      And I receive my user information

    Scenario: Access protected resource with expired token
      Given I have an expired JWT token
      When I request my user profile
      Then the request fails with status 401
      And I receive error "Token expired"

    Scenario: Refresh expired token
      Given I have a valid refresh token
      When I request a new access token
      Then I receive a new JWT access token
      And the new token expires in 30 minutes

    Scenario: Logout invalidates tokens
      Given I am logged in as "john@therapy.com"
      When I logout
      Then my access token is invalidated
      And my refresh token is invalidated
      And an audit log entry is created for "user_logout"