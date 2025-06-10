Feature: Teletherapy Sessions
  As a therapist
  I want to conduct virtual therapy sessions
  So that I can provide services remotely when needed

  Background:
    Given I am logged in as therapist "Sarah Johnson"
    And teletherapy is enabled for my account
    And I have the following virtual sessions scheduled:
      | Student      | Date       | Time        | Service |
      | Emma Wilson  | 2024-01-15 | 09:00-09:30 | OT      |
      | Liam Brown   | 2024-01-15 | 10:00-10:30 | OT      |

  Rule: Virtual sessions require proper setup and consent

    Scenario: Verify teletherapy consent
      Given "Emma Wilson" is scheduled for virtual session
      When I check session prerequisites
      Then I verify:
        | Requirement           | Status    | Details                        |
        | Parent Consent       | Completed | Signed on 2024-01-01          |
        | Technology Agreement | Completed | Agreed to platform terms      |
        | Emergency Contact    | On file   | Mother: 555-0123              |
        | Session Location     | Confirmed | Home with parent supervision  |
      And session can proceed if all requirements met

    Scenario: Pre-session technology check
      Given session starts in 15 minutes
      When I initiate tech check with parent
      Then system verifies:
        | Component      | Required    | Status Check           |
        | Internet Speed | 10+ Mbps    | Testing bandwidth      |
        | Camera        | 720p min    | Video preview working  |
        | Microphone    | Clear audio | Echo test completed    |
        | Screen Share  | Enabled     | Permissions granted    |
      And troubleshooting guide available if issues

    Scenario: Parent not present for required session
      Given "Emma Wilson" requires parent supervision
      When session starts without parent present
      Then I must:
        | Action                  | Reason                          |
        | Document situation      | Compliance requirement          |
        | Attempt parent contact  | Call/text provided numbers      |
        | Reschedule if needed   | Cannot proceed without parent   |
        | File incident report   | Policy violation                |

  Rule: Virtual platform must support therapy activities

    Scenario: Start teletherapy session
      Given all prerequisites are met
      When I start session with "Emma Wilson"
      Then the platform provides:
        | Feature              | Purpose                         |
        | HD Video            | Clear visual for demonstrations |
        | Screen Annotation   | Draw/highlight during activities |
        | Virtual Whiteboard  | Interactive writing practice    |
        | File Sharing        | Share worksheets instantly     |
        | Recording Option    | With consent only              |
        | Timer Display       | Session duration tracking      |
      And session is marked "in progress"

    Scenario: Use interactive therapy tools
      Given I am in session with "Emma Wilson"
      When I launch therapy activities
      Then I can use:
        | Tool Type           | Examples                        | Features                    |
        | Digital Manipulatives | Virtual blocks, puzzles       | Drag-drop, rotate, resize   |
        | Drawing Tools       | Pencil, shapes, stamps         | Pressure sensitivity        |
        | Games              | Memory match, sequencing       | Turn-taking enabled         |
        | Assessment Tools   | Standardized test materials    | Secure, timed sections      |
      And student's screen mirrors my demonstrations
      And activities are logged for documentation

    Scenario: Manage session disruptions
      Given active session with "Emma Wilson"
      When connection is disrupted
      Then system:
        | Response            | Action                          |
        | Auto-reconnect     | Attempts for 60 seconds         |
        | Save session state | Preserves activity progress     |
        | Notify parent      | SMS about connection issue      |
        | Document disruption| Logs time and duration          |
      And if reconnection fails:
        | Follow-up          | Implementation                  |
        | Contact parent     | Call within 5 minutes           |
        | Reschedule        | Offer make-up session           |
        | Document          | Note partial session completion |

  Rule: Virtual sessions must maintain security

    Scenario: Secure session access
      Given session link is generated
      When participants join
      Then security measures include:
        | Measure              | Implementation                 |
        | Unique session ID   | One-time use code             |
        | Waiting room       | Therapist admits participants  |
        | Participant limit  | Max 4 (therapist, student, parents) |
        | Screen recording   | Blocked unless consented       |
        | Chat monitoring    | Saved to session record        |
      And unauthorized participants cannot join

    Scenario: Handle unauthorized participant
      Given session is in progress
      When unknown user attempts to join
      Then:
        | Action              | Result                         |
        | Block entry        | User remains in waiting room   |
        | Alert therapist    | Pop-up notification            |
        | Log attempt        | IP and timestamp recorded      |
        | Continue session   | No disruption to activities    |

    Scenario: Protect student privacy
      Given I'm documenting virtual session
      When session includes sensitive information
      Then privacy protections include:
        | Protection          | Implementation                 |
        | Background blur    | Automatic for home sessions    |
        | Audio filtering    | Remove background voices       |
        | Screenshot blocking| Disabled on student side       |
        | Session encryption | End-to-end encryption         |

  Rule: Documentation adapts for virtual delivery

    Scenario: Document teletherapy session
      Given I completed virtual session with "Emma Wilson"
      When I document the session
      Then documentation includes standard fields plus:
        | Teletherapy Field    | Required Info                  |
        | Platform Used       | Integrated video platform      |
        | Connection Quality  | Good/Fair/Poor                 |
        | Parent Participation| Present and engaged            |
        | Technical Issues    | None/Minor/Major               |
        | Home Distractions   | Minimal                        |
        | Student Engagement  | High/Medium/Low                |
      And billing code reflects teletherapy modifier

    Scenario: Compare virtual to in-person effectiveness
      Given "Emma Wilson" has both session types
      When I review progress data
      Then I can analyze:
        | Comparison Metric    | In-Person | Virtual | Notes              |
        | Goal Progress       | 15%/month | 12%/month | Slightly slower    |
        | Engagement Level    | 4.5/5     | 4.0/5    | Good engagement    |
        | Parent Involvement  | Low       | High     | Benefit of virtual |
        | Session Completion  | 95%       | 88%      | Tech issues impact |

  Rule: Virtual sessions support group therapy

    Scenario: Conduct virtual group session
      Given I have group session with 3 students
      When all participants join
      Then I can:
        | Group Feature        | Implementation                 |
        | Gallery view        | See all participants           |
        | Breakout rooms      | Pair activities                |
        | Shared whiteboard   | Collaborative drawing          |
        | Turn indicators     | Show who's speaking            |
        | Mute controls       | Manage background noise        |
      And each student's participation is tracked

    Scenario: Manage different student needs in group
      Given group has varied technical setups
      When I run group activities
      Then I accommodate:
        | Student      | Need                  | Accommodation            |
        | Emma        | Slow internet         | Lower video quality      |
        | Liam        | No touchscreen        | Mouse-friendly activities |
        | Noah        | Hearing aids          | Visual cues emphasized   |
      And all students can participate fully

  Rule: Platform supports asynchronous components

    Scenario: Assign between-session activities
      Given "Emma Wilson" completed virtual session
      When I assign home practice
      Then parent portal shows:
        | Component           | Details                        |
        | Video instructions | 5-minute demonstration         |
        | Digital worksheet  | Interactive PDF                |
        | Practice schedule  | 3x daily, 10 minutes          |
        | Progress tracking  | Parent can mark complete      |
      And completion syncs to my dashboard

    Scenario: Review recorded session segments
      Given session recording was consented
      When I review for documentation
      Then I can:
        | Action              | Purpose                        |
        | Mark timestamps    | Key moments for progress       |
        | Extract clips      | Parent education segments      |
        | Delete full video  | After documentation complete   |
        | Share segments     | With IEP team if authorized   |
      And recordings auto-delete after 30 days

  Rule: Emergency protocols for virtual sessions

    Scenario: Medical emergency during session
      Given I'm in session with "Emma Wilson"
      When I observe signs of medical distress
      Then I immediately:
        | Priority | Action                          |
        | 1        | Instruct parent to call 911    |
        | 2        | Stay on video for support      |
        | 3        | Document observations          |
        | 4        | Contact school nurse           |
        | 5        | Complete incident report       |
      And session recording is preserved

    Scenario: Behavioral crisis in virtual setting
      Given student showing escalated behavior
      When de-escalation needed
      Then virtual strategies include:
        | Strategy            | Implementation                 |
        | Calm voice         | Lower tone, slower pace        |
        | Visual supports    | Screen share calming images    |
        | Parent coaching    | Guide parent intervention      |
        | Sensory breaks     | Suggest specific activities    |
        | End if needed      | Safety over session completion |
      And follow-up required within 24 hours