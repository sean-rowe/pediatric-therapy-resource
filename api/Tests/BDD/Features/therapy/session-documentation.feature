Feature: Session Documentation
  As a therapist
  I want to document therapy sessions efficiently
  So that I maintain compliance and track student progress

  Background:
    Given I am logged in as therapist "Sarah Johnson"
    And I have the following scheduled appointments:
      | Student      | Date       | Time        | Location      |
      | Emma Wilson  | 2024-01-15 | 09:00-09:30 | OT Room      |
      | Liam Brown   | 2024-01-15 | 10:00-10:30 | Classroom 12 |

  Rule: Sessions must be documented using SOAP format

    @not-implemented
    Scenario: Document a completed session
      Given I completed session with "Emma Wilson" on "2024-01-15"
      When I document the session with:
        | Section    | Content                                                           |
        | Subjective | Emma reported feeling "frustrated with writing." Teacher noted she avoided writing tasks this week. |
        | Objective  | 15 min handwriting practice. Formed 18/26 letters correctly (69%). Required verbal cues for proper pencil grip. Completed 3 sentences with 2 rest breaks. |
        | Assessment | Improvement from last session (65%). Fatigue affects letter formation. Benefits from frequent breaks. Making steady progress toward goal. |
        | Plan       | Continue letter formation practice. Implement pencil grip reminder card. Teacher to provide 2-min breaks during writing tasks. |
      Then the documentation is saved successfully
      And the session status changes to "completed"
      And the actual duration is recorded as "30 minutes"
      And an audit log entry is created for "session_documented"

    @not-implemented
    Scenario: Auto-save documentation while typing
      Given I am documenting session for "Emma Wilson"
      When I type in the Subjective field
      Then the content auto-saves every 30 seconds
      And I see "Draft saved" indicator
      And I can recover the draft if connection is lost

    @not-implemented
    Scenario: Documentation time limit enforcement
      Given a session occurred on "2024-01-08" (8 days ago)
      When I attempt to document this session
      Then I receive warning "Session older than 7 days"
      And I must provide late documentation reason
      And supervisor notification is triggered

  Rule: Sessions must track goals addressed

    @not-implemented
    Scenario: Link session to IEP goals
      Given "Emma Wilson" has goals:
        | Goal Number | Goal Area    |
        | OT-1       | Fine Motor   |
        | OT-2       | Visual Motor |
      When I document the session
      And I select goals addressed: "OT-1", "OT-2"
      And I add goal-specific notes:
        | Goal | Progress Note                           |
        | OT-1 | Formed 18/26 letters correctly (69%)   |
        | OT-2 | Completed shape copying with 80% accuracy |
      Then the goals are linked to the session
      And progress is automatically updated for each goal

    @not-implemented
    Scenario: Require goal selection
      Given I am documenting a session
      When I attempt to save without selecting goals
      Then validation fails with "At least one goal must be addressed"

  Rule: Sessions support multiple service delivery models

    @not-implemented
    Scenario: Document individual session
      Given I have an individual session with "Emma Wilson"
      When I document the session
      Then session type is "individual"
      And I document one-on-one interventions

    @not-implemented
    Scenario: Document group session
      Given I have a group session with:
        | Student       | Attendance |
        | Emma Wilson   | Present    |
        | Liam Brown    | Present    |
        | Noah Davis    | Absent     |
      When I document the group session
      Then I can add group-level documentation
      And I can add individual notes for each present student
      And absent students are marked appropriately

    @not-implemented
    Scenario: Document consultation session
      Given I have a consultation with "Emma Wilson's" teacher
      When I document the consultation
      Then session type is "consultation"
      And I document:
        | Field               | Content                                |
        | Participants        | Ms. Smith (Teacher), Sarah Johnson (OT) |
        | Topics Discussed    | Classroom accommodations for handwriting |
        | Recommendations     | Slant board, pencil grip, frequent breaks |
        | Follow-up Plan      | Check in 2 weeks                      |

    @not-implemented
    Scenario: Document teletherapy session
      Given I have a virtual session with "Emma Wilson"
      When I document the teletherapy session
      Then I also record:
        | Field               | Value    |
        | Platform           | Zoom     |
        | Connection Quality | Good     |
        | Parent Present     | Yes      |
        | Technical Issues   | None     |

  Rule: Sessions track materials and activities

    @not-implemented
    Scenario: Document activities and materials
      Given I am documenting a session
      When I add session details:
        | Field               | Content                                    |
        | Activities         | Handwriting practice, Cutting skills, Sensory break |
        | Materials Used     | Therapy putty, Adapted scissors, Pencil grips |
        | Student Engagement | 4/5 - Required one redirection           |
        | Session Productivity | 4/5 - Completed most planned activities |
      Then the details are saved with the session
      And materials can be tracked for inventory

    @not-implemented
    Scenario: Use AI-generated content in session
      Given I used AI-generated worksheet "Fine Motor Maze #123"
      When I document the session
      And I rate the content effectiveness as "4/5"
      And I add feedback "Student engaged well, slightly too easy"
      Then the rating is linked to the content
      And the feedback improves future recommendations

  Rule: Session scheduling and attendance

    @not-implemented
    Scenario: Check in for scheduled session
      Given I have a session scheduled at "09:00"
      When I check in at "09:02"
      Then the actual start time is recorded as "09:02"
      And the session status changes to "in progress"

    @not-implemented
    Scenario: Mark student as no-show
      Given I have a session scheduled with "Emma Wilson"
      And the student does not arrive by "09:10"
      When I mark the session as "no show"
      Then the session status changes to "no show"
      And parent notification is triggered
      And I must document attempted contact

    @not-implemented
    Scenario: Cancel session with notice
      Given I have a session scheduled tomorrow
      When I cancel the session with reason "Therapist ill"
      Then the session status changes to "cancelled"
      And parent notification is sent
      And makeup session scheduling is triggered

    @not-implemented
    Scenario: Handle early dismissal
      Given I am in session with "Emma Wilson"
      And the session started at "09:00"
      When the student leaves at "09:20" due to "Feeling unwell"
      Then actual end time is recorded as "09:20"
      And actual duration is "20 minutes"
      And early dismissal reason is documented

  Rule: Documentation supports billing

    @not-implemented
    Scenario: Billable session documentation
      Given I completed a billable session
      When I document the session
      Then billing information is captured:
        | Field          | Value      |
        | CPT Code       | 97530     |
        | Units         | 2         |
        | Billable Time | 30 minutes |
      And the session is marked as "ready to bill"

    @not-implemented
    Scenario: Non-billable session documentation
      Given I completed a non-billable activity
      When I document as "IEP meeting attendance"
      Then the time is tracked as non-billable
      And it appears in productivity reports
      But not in billing queue

  Rule: Documentation supports compliance

    @not-implemented
    Scenario: Medicaid-compliant documentation
      Given the payer requires specific documentation
      When I complete the session note
      Then the system validates:
        | Requirement          | Status |
        | Start/End time       | ✓     |
        | Specific goals       | ✓     |
        | Measurable progress  | ✓     |
        | Plan for next visit  | ✓     |
        | Therapist signature  | ✓     |

    @not-implemented
    Scenario: Lock documentation after signing
      Given I have completed documentation
      When I electronically sign the note
      Then the documentation is locked
      And no edits are allowed
      And timestamp and signature are recorded
      And addendums require supervisor approval

    @not-implemented
    Scenario: Documentation audit trail
      Given I am viewing a completed session note
      When I check the audit history
      Then I see:
        | Action              | User          | Timestamp           |
        | Created            | Sarah Johnson | 2024-01-15 11:30:00 |
        | Auto-saved         | System        | 2024-01-15 11:32:30 |
        | Signed             | Sarah Johnson | 2024-01-15 11:45:00 |