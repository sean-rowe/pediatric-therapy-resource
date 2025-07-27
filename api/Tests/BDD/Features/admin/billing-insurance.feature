Feature: Billing and Insurance Management
  As a therapy practice
  I want to manage billing and insurance claims
  So that I can receive proper reimbursement for services

  Background:
    Given the following insurance payers exist:
      | ID | Name                 | Type     | Default Rate |
      | 1  | Texas Medicaid       | medicaid | $45.00      |
      | 2  | BlueCross BlueShield | private  | $125.00     |
    And the following CPT codes are configured:
      | Code  | Description                | Service Type | Units |
      | 97530 | Therapeutic activities     | OT          | 15 min |
      | 97110 | Therapeutic exercises      | PT          | 15 min |
      | 92507 | Speech therapy treatment   | SLP         | 15 min |

  Rule: Insurance information must be complete and valid

    @not-implemented
    Scenario: Add insurance to student
      Given student "Emma Wilson" exists
      When I add insurance information:
        | Field                | Value                  |
        | Payer               | Texas Medicaid         |
        | Policy Number       | TXM123456789          |
        | Group Number        | GRP001                |
        | Subscriber Name     | Lisa Wilson           |
        | Subscriber Relation | Mother                |
        | Effective Date      | 2024-01-01            |
      Then the insurance is saved successfully
      And the insurance is marked as primary
      And eligibility verification is triggered

    @not-implemented
    Scenario: Add secondary insurance
      Given "Emma Wilson" has primary insurance "Texas Medicaid"
      When I add secondary insurance "BlueCross BlueShield"
      Then both insurances are saved
      And coverage priority is set correctly
      And coordination of benefits is enabled

    @not-implemented
    Scenario: Insurance authorization tracking
      Given "Emma Wilson" has "Texas Medicaid" insurance
      When I add authorization:
        | Field               | Value       |
        | Authorization Number | AUTH789456  |
        | Start Date          | 2024-01-01  |
        | End Date            | 2024-06-30  |
        | Authorized Units    | 120         |
        | Services Authorized | OT, PT      |
      Then the authorization is saved
      And remaining units show "120"
      And authorization alerts are configured

  Rule: Claims must be created from documented sessions

    @not-implemented
    Scenario: Create claim for single session
      Given "Emma Wilson" has a completed session on "2024-01-15"
      And the session was 30 minutes of "Therapeutic activities"
      When I create a claim for the session
      Then the claim includes:
        | Field         | Value         |
        | Student       | Emma Wilson   |
        | Service Date  | 2024-01-15    |
        | CPT Code      | 97530         |
        | Units         | 2             |
        | Rate          | $45.00        |
        | Total         | $90.00        |
      And the claim status is "draft"

    @not-implemented
    Scenario: Batch create claims
      Given multiple students have completed sessions in "January 2024"
      When I batch create claims for "Texas Medicaid" from "2024-01-01" to "2024-01-31"
      Then claims are created for all eligible sessions
      And each claim groups sessions by student
      And a summary shows:
        | Metric              | Value |
        | Claims Created      | 45    |
        | Total Amount        | $8,550 |
        | Sessions Included   | 190   |

    @not-implemented
    Scenario: Validate claim before submission
      Given I have a draft claim for "Emma Wilson"
      When I validate the claim
      Then the system checks:
        | Validation                  | Result |
        | Authorization valid         | Pass   |
        | Documentation complete      | Pass   |
        | Timely filing              | Pass   |
        | Diagnosis codes present    | Pass   |
        | Referring physician        | Pass   |
      And any failures prevent submission

  Rule: Claims must be submitted within timely filing limits

    @not-implemented
    Scenario: Submit claim within limits
      Given a claim for service date "2024-01-15"
      And today is "2024-02-10"
      And "Texas Medicaid" has 95-day filing limit
      When I submit the claim
      Then the submission is successful
      And claim status changes to "submitted"
      And submission timestamp is recorded

    @not-implemented
    Scenario: Warn about approaching filing deadline
      Given a claim for service date "2023-11-01"
      And today is "2024-01-25"
      And filing deadline is "2024-02-04" (10 days away)
      When I view the claim
      Then I see warning "Filing deadline in 10 days"
      And the claim is highlighted in the queue

    @not-implemented
    Scenario: Prevent late submission
      Given a claim for service date "2023-10-01"
      And today is "2024-02-01"
      And filing deadline was "2024-01-04"
      When I attempt to submit the claim
      Then submission fails with "Past timely filing deadline"
      And I must request exception with justification

  Rule: Claims tracking and payment posting

    @not-implemented
    Scenario: Track claim status updates
      Given claim "CLM-2024-0123" was submitted on "2024-01-15"
      When the payer updates status to "in review"
      Then the claim status is updated
      And status history shows:
        | Date       | Status     | Notes                |
        | 2024-01-15 | submitted  | Sent via EDI         |
        | 2024-01-18 | received   | Accepted by payer    |
        | 2024-01-22 | in review  | Medical review required |

    @not-implemented
    Scenario: Post payment for claim
      Given claim "CLM-2024-0123" for $180.00
      When I post payment:
        | Field            | Value       |
        | Payment Date     | 2024-02-15  |
        | Check Number     | 789456      |
        | Paid Amount      | $162.00     |
        | Adjustment       | $18.00      |
        | Adjustment Reason| Contractual |
      Then the payment is posted
      And claim status changes to "paid"
      And patient responsibility is $0.00
      And AR balance is updated

    @not-implemented
    Scenario: Post partial payment
      Given claim "CLM-2024-0123" for $180.00
      When I post partial payment of $90.00
      Then claim balance shows $90.00
      And claim status remains "partial payment"
      And follow-up task is created

  Rule: Denials must be managed and appealed

    @not-implemented
    Scenario: Handle claim denial
      Given claim "CLM-2024-0123" was submitted
      When the claim is denied with:
        | Field          | Value                           |
        | Denial Date    | 2024-02-01                     |
        | Denial Code    | 197                            |
        | Denial Reason  | Authorization not on file      |
      Then claim status changes to "denied"
      And denial is tracked in system
      And action items are suggested:
        | Action                    | Due Date   |
        | Submit authorization      | 2024-02-15 |
        | Resubmit claim           | 2024-02-20 |

    @not-implemented
    Scenario: Appeal denied claim
      Given claim "CLM-2024-0123" was denied
      When I create an appeal with:
        | Field              | Value                          |
        | Appeal Date        | 2024-02-05                    |
        | Appeal Level       | 1                             |
        | Justification      | Authorization was valid, see attached |
        | Supporting Docs    | AUTH789456.pdf                |
      Then the appeal is submitted
      And claim status changes to "appealed"
      And appeal deadline is tracked

    @not-implemented
    Scenario: Track appeal outcome
      Given claim "CLM-2024-0123" is under appeal
      When the appeal is approved with payment $162.00
      Then claim status changes to "paid after appeal"
      And payment is posted
      And appeal success rate is updated

  Rule: Financial reporting and analytics

    @not-implemented
    Scenario: Generate aging report
      Given multiple claims in various statuses
      When I generate AR aging report
      Then I see claims grouped by age:
        | Age Range | Count | Amount    |
        | 0-30 days | 45    | $8,550    |
        | 31-60 days| 12    | $2,340    |
        | 61-90 days| 5     | $975      |
        | >90 days  | 2     | $380      |
      And total AR is $12,245

    @not-implemented
    Scenario: Analyze denial trends
      Given claims history for past 6 months
      When I view denial analytics
      Then I see:
        | Denial Reason           | Count | Percentage |
        | Missing authorization   | 15    | 35%       |
        | Timely filing          | 8     | 19%       |
        | Documentation issues   | 12    | 28%       |
        | Invalid diagnosis      | 8     | 19%       |
      And recommendations for improvement

    @not-implemented
    Scenario: Track collection rates
      Given payment history for 2024
      When I view collection metrics
      Then I see:
        | Metric                  | Value   |
        | Gross charges          | $125,000 |
        | Contractual adjustments| $25,000  |
        | Net expected           | $100,000 |
        | Collected              | $92,000  |
        | Collection rate        | 92%      |
        | Days in AR             | 42       |

  Rule: Electronic remittance and EDI

    @not-implemented
    Scenario: Process electronic remittance (835)
      Given an 835 file is received from "Texas Medicaid"
      When I process the remittance
      Then payments are automatically posted
      And denials are recorded
      And adjustments are applied
      And exceptions require manual review:
        | Type               | Count |
        | Patient not found  | 2     |
        | Claim not found   | 1     |
        | Amount mismatch   | 3     |

    @not-implemented
    Scenario: Submit claims via EDI
      Given I have 25 claims ready for "BlueCross BlueShield"
      When I submit via EDI batch
      Then claims are formatted as 837 file
      And file is transmitted securely
      And acknowledgment (999) is received
      And claims update to "submitted" status