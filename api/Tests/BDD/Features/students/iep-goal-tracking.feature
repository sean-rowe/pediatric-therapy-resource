Feature: IEP Goal Tracking
  As a therapist
  I want to create and track IEP goals
  So that I can monitor student progress and meet compliance requirements

  Background:
    Given I am logged in as a therapist
    And I have a student "Emma Wilson" with an active IEP
    And the current date is "2024-01-15"

  Rule: Goals must be SMART (Specific, Measurable, Achievable, Relevant, Time-bound)

    @not-implemented
    Scenario: Create a valid IEP goal
      When I create a goal with:
        | Field               | Value                                                           |
        | Goal Number         | OT-1                                                           |
        | Goal Area           | Fine Motor                                                     |
        | Goal Text           | Emma will improve handwriting legibility to form all lowercase letters correctly |
        | Measurement Method  | 4 out of 5 trials with 80% accuracy                          |
        | Baseline            | Currently forms 12 of 26 letters correctly (46%)             |
        | Target Date         | 2024-06-15                                                    |
      Then the goal is created successfully
      And the goal status is "active"
      And the goal is linked to student "Emma Wilson"
      And I am assigned as the responsible therapist

    @not-implemented
    Scenario Outline: Goal validation rules
      When I attempt to create a goal without "<field>"
      Then the creation fails with error "<error>"

      Examples:
        | field              | error                           |
        | Goal Text          | Goal text is required           |
        | Measurement Method | Measurement method is required  |
        | Target Date        | Target date is required         |

    @not-implemented
    Scenario: Target date must be in the future
      When I attempt to create a goal with target date "2023-12-01"
      Then the creation fails with error "Target date must be in the future"

    @not-implemented
    Scenario: Goal number must be unique per student
      Given "Emma Wilson" has a goal numbered "OT-1"
      When I attempt to create another goal numbered "OT-1" for "Emma Wilson"
      Then the creation fails with error "Goal number already exists for this student"

  Rule: Goal progress must be tracked regularly

    @not-implemented
    Scenario: Record progress on a goal
      Given "Emma Wilson" has goal "OT-1" for handwriting
      When I record progress:
        | Field               | Value                                    |
        | Date                | 2024-02-15                              |
        | Progress Rating     | 3                                       |
        | Trials Attempted    | 5                                       |
        | Trials Successful   | 4                                       |
        | Independence Level  | Minimal assistance                      |
        | Notes              | Showed improvement with letter formation |
      Then the progress entry is saved
      And the goal's current performance is updated
      And the progress appears in the goal timeline

    @not-implemented
    Scenario: Calculate progress percentage
      Given "Emma Wilson" has goal "OT-1" with baseline "46%" and target "80%"
      And the following progress entries exist:
        | Date       | Performance |
        | 2024-01-30 | 52%        |
        | 2024-02-15 | 58%        |
        | 2024-03-01 | 65%        |
      When I view the goal progress
      Then I see the current performance is "65%"
      And the progress toward target is "56%" ((65-46)/(80-46))
      And the trend shows "improving"

    @not-implemented
    Scenario: Progress entries must be dated
      Given "Emma Wilson" has goal "OT-1"
      When I attempt to record progress without a date
      Then the save fails with error "Progress date is required"

    @not-implemented
    Scenario: Cannot record future progress
      Given "Emma Wilson" has goal "OT-1"
      When I attempt to record progress for date "2024-12-31"
      Then the save fails with error "Cannot record progress for future dates"

  Rule: Goals have lifecycle status changes

    @not-implemented
    Scenario: Mark goal as met
      Given "Emma Wilson" has goal "OT-1" with target "80% accuracy"
      And recent progress shows "82% accuracy" consistently
      When I update the goal status to "met"
      Then the goal status changes to "met"
      And I must provide outcome data:
        | Field           | Value                                |
        | Date Met        | 2024-03-15                          |
        | Final Performance | 82% accuracy in letter formation   |
        | Next Steps      | Maintain skill in classroom setting |
      And an audit log entry is created for "goal_met"

    @not-implemented
    Scenario: Discontinue a goal
      Given "Emma Wilson" has goal "OT-1"
      When I update the goal status to "discontinued"
      Then I must provide a reason:
        | Field             | Value                                    |
        | Reason            | Student moved to different service model |
        | Date Discontinued | 2024-02-28                              |
        | Final Notes       | Recommend reassessment in new setting   |
      And the goal status changes to "discontinued"
      And an audit log entry is created for "goal_discontinued"

    @not-implemented
    Scenario: Extend goal target date
      Given "Emma Wilson" has goal "OT-1" with target date "2024-06-15"
      When I extend the target date to "2024-12-15"
      Then I must provide justification:
        | Field         | Value                                      |
        | New Date      | 2024-12-15                                |
        | Justification | Progress slower than expected due to medical absence |
      And the target date is updated
      And the change is recorded in goal history

  Rule: Goals support collaborative tracking

    @not-implemented
    Scenario: Multiple therapists track same goal
      Given "Emma Wilson" has goal "OT-1"
      And I am the primary therapist
      And "John Smith" is a supporting therapist
      When "John Smith" records progress on the goal
      Then the progress entry shows "Recorded by: John Smith"
      And I receive a notification of the update

    @not-implemented
    Scenario: Parent visibility of goals
      Given "Emma Wilson" has active goals
      And parent "Lisa Wilson" has portal access
      When "Lisa Wilson" views goals in parent portal
      Then she sees goal text and current progress
      But she cannot see clinical notes
      And she cannot modify goals

  Rule: Goals must align with state standards

    @not-implemented
    Scenario: Create goal with state standard alignment
      When I create a goal with:
        | Field          | Value                                           |
        | Goal Text      | Student will demonstrate grade-level writing skills |
        | State Standard | TX.ELA.3.11A - Plan drafts by generating ideas |
      Then the goal is linked to state standard "TX.ELA.3.11A"
      And the goal appears in state compliance reports

    @not-implemented
    Scenario: Validate goal measurability
      When I attempt to create a goal with measurement method "Student will try hard"
      Then validation warns "Measurement method must be quantifiable"
      And suggests examples: "4/5 trials", "80% accuracy", "independently"

  Rule: Goal reporting and analytics

    @not-implemented
    Scenario: Generate goal progress report
      Given "Emma Wilson" has multiple goals with progress data
      When I generate a progress report for date range "2024-01-01" to "2024-03-31"
      Then the report includes:
        | Section                | Content                        |
        | Goals Summary          | Total: 3, Met: 1, Active: 2   |
        | Progress Charts        | Line graphs for each goal      |
        | Session Correlation    | Goals addressed per session    |
        | Therapist Notes        | Combined progress notes        |

    @not-implemented
    Scenario: View goal success metrics
      Given multiple students have goals in "Fine Motor" area
      When I view analytics for "Fine Motor" goals
      Then I see:
        | Metric                  | Value |
        | Average Time to Master  | 4.2 months |
        | Success Rate           | 78%    |
        | Most Effective Methods | Visual cues, Hand-over-hand |

    @not-implemented
    Scenario: Predict goal achievement
      Given "Emma Wilson" has goal "OT-1" with 6 months of progress data
      When the system analyzes progress trends
      Then it predicts:
        | Prediction              | Value      |
        | Likely Achievement Date | 2024-05-20 |
        | Confidence Level        | 75%        |
        | Risk Factors           | Attendance |