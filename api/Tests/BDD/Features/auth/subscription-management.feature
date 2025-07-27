Feature: Subscription Management API Endpoints
  As a platform user
  I want to manage my subscription
  So that I can access the features I need

  Background:
    Given the API is available
    And I am authenticated as "user@clinic.com"
    And the following subscription plans exist:
      | id    | name        | price | features                    |
      | basic | Basic       | 9.95  | Limited features            |
      | pro   | Pro         | 19.95 | Full platform access        |
      | team  | Small Team  | 15    | Per user, 5-20 users        |
      | enterprise | Enterprise | custom | 50+ users, SSO         |

  # GET /api/subscriptions/plans
  @endpoint @subscriptions @not-implemented
  Scenario: Get available subscription plans
    When I send a GET request to "/api/subscriptions/plans"
    Then the response status should be 200
    And the auth response should contain array of:
      | field       | type    |
      | id          | string  |
      | name        | string  |
      | price       | number  |
      | currency    | string  |
      | interval    | string  |
      | features    | array   |
      | limits      | object  |
      | popular     | boolean |

  # GET /api/subscriptions/current
  @endpoint @subscriptions @not-implemented
  Scenario: Get current subscription details
    Given I have an active "pro" subscription
    When I send a GET request to "/api/subscriptions/current"
    Then the response status should be 200
    And the response should contain:
      | field            | type    |
      | planId           | string  |
      | planName         | string  |
      | status           | string  |
      | currentPeriodEnd | string  |
      | cancelAtPeriodEnd| boolean |
      | usage            | object  |
      | limits           | object  |

  # POST /api/subscriptions/subscribe
  @endpoint @subscriptions @not-implemented
  Scenario: Subscribe to a new plan
    Given I have no active subscription
    When I send a POST request to "/api/subscriptions/subscribe" with:
      | field         | value              |
      | planId        | pro                |
      | paymentMethod | pm_test_visa       |
    Then the response status should be 200
    And the response should contain:
      | field          | type    |
      | subscriptionId | string  |
      | status         | string  |
      | invoice        | object  |
    And a subscription should be created in Stripe
    And a welcome email should be sent

  @endpoint @subscriptions @validation @not-implemented
  Scenario: Require payment method for paid plans
    When I send a POST request to "/api/subscriptions/subscribe" with:
      | field  | value |
      | planId | pro   |
    Then the response status should be 400
    And the response should contain error "Payment method required"

  # PUT /api/subscriptions/upgrade
  @endpoint @subscriptions @not-implemented
  Scenario: Upgrade subscription plan
    Given I have an active "basic" subscription
    When I send a PUT request to "/api/subscriptions/upgrade" with:
      | field  | value |
      | planId | pro   |
    Then the response status should be 200
    And the response should contain:
      | field         | type   |
      | status        | string |
      | proratedAmount| number |
      | effectiveDate | string |
    And the subscription should be upgraded immediately
    And a prorated charge should be created

  # PUT /api/subscriptions/downgrade
  @endpoint @subscriptions @not-implemented
  Scenario: Downgrade subscription plan
    Given I have an active "pro" subscription
    When I send a PUT request to "/api/subscriptions/downgrade" with:
      | field  | value |
      | planId | basic |
    Then the response status should be 200
    And the response should contain:
      | field         | type   |
      | status        | string |
      | effectiveDate | string |
      | credit        | number |
    And the downgrade should take effect at period end
    And a credit should be applied

  @endpoint @subscriptions @validation @not-implemented
  Scenario: Prevent downgrade if usage exceeds new plan limits
    Given I have an active "pro" subscription
    And I have 100 students (basic limit is 10)
    When I send a PUT request to "/api/subscriptions/downgrade" with:
      | field  | value |
      | planId | basic |
    Then the response status should be 400
    And the response should contain error "Current usage exceeds basic plan limits"

  # POST /api/subscriptions/cancel
  @endpoint @subscriptions @not-implemented
  Scenario: Cancel subscription
    Given I have an active "pro" subscription
    When I send a POST request to "/api/subscriptions/cancel" with:
      | field            | value                    |
      | reason           | Too expensive            |
      | feedback         | Would use if cheaper     |
      | cancelImmediately| false                   |
    Then the response status should be 200
    And the response should contain:
      | field           | type   |
      | cancelDate      | string |
      | accessUntil     | string |
      | refundAmount    | number |
    And the subscription should be marked for cancellation
    And a cancellation email should be sent
    And the feedback should be recorded

  # GET /api/subscriptions/invoices
  @endpoint @subscriptions @not-implemented
  Scenario: Get subscription invoices
    Given I have subscription invoices
    When I send a GET request to "/api/subscriptions/invoices"
    Then the response status should be 200
    And the auth response should contain array of:
      | field       | type    |
      | id          | string  |
      | number      | string  |
      | date        | string  |
      | amount      | number  |
      | status      | string  |
      | downloadUrl | string  |

  # GET /api/subscriptions/payment-methods
  @endpoint @subscriptions @not-implemented
  Scenario: Get saved payment methods
    Given I have saved payment methods
    When I send a GET request to "/api/subscriptions/payment-methods"
    Then the response status should be 200
    And the auth response should contain array of:
      | field      | type    |
      | id         | string  |
      | type       | string  |
      | last4      | string  |
      | brand      | string  |
      | expMonth   | number  |
      | expYear    | number  |
      | isDefault  | boolean |

  # POST /api/subscriptions/payment-methods
  @endpoint @subscriptions @not-implemented
  Scenario: Add new payment method
    When I send a POST request to "/api/subscriptions/payment-methods" with:
      | field            | value              |
      | paymentMethodId  | pm_test_mastercard |
      | setAsDefault     | true               |
    Then the response status should be 200
    And the response should contain:
      | field      | value |
      | id         | string|
      | last4      | 4444  |
      | brand      | Mastercard |
    And the payment method should be saved in Stripe
    And it should be set as default

  # DELETE /api/subscriptions/payment-methods/{id}
  @endpoint @subscriptions @not-implemented
  Scenario: Remove payment method
    Given I have multiple payment methods
    And payment method "pm_123" is not the default
    When I send a DELETE request to "/api/subscriptions/payment-methods/pm_123"
    Then the response status should be 200
    And the payment method should be removed
    And the response should contain message "Payment method removed"

  @endpoint @subscriptions @validation @not-implemented
  Scenario: Cannot remove default payment method with active subscription
    Given I have an active subscription
    And payment method "pm_default" is the default
    When I send a DELETE request to "/api/subscriptions/payment-methods/pm_default"
    Then the response status should be 400
    And the response should contain error "Cannot remove default payment method"