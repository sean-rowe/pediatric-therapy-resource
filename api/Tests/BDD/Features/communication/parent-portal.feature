Feature: Parent Portal
  As a parent
  I want to access my child's therapy information
  So that I can support their progress and communicate with therapists

  Background:
    Given the parent portal is enabled
    And I am logged in as parent "Lisa Wilson"
    And I have children:
      | Child Name  | Services    | Therapist      |
      | Emma Wilson | OT, Speech  | Sarah Johnson  |
      | Jack Wilson | PT          | Mike Chen      |

  Rule: Parents must have secure authenticated access

    @not-implemented
    Scenario: Parent registration with access code
      Given therapist sends portal invitation to "robert.wilson@email.com"
      When "Robert Wilson" receives invitation email
      Then email contains:
        | Content              | Details                           |
        | Access Code         | Unique 8-character code          |
        | Registration Link   | Expires in 48 hours              |
        | Student Connection  | For Emma Wilson                  |
      When he registers with the access code
      Then account is created
      And he has access to "Emma Wilson" information only

    @not-implemented
    Scenario: Parent login with 2FA
      Given parent has two-factor authentication enabled
      When parent logs in with email and password
      Then SMS code is sent to registered phone
      And login requires code entry
      And session expires after 30 minutes of inactivity

    @not-implemented
    Scenario: Multiple parents access same student
      Given "Emma Wilson" has two registered parents
      When both parents access the portal
      Then both see identical information
      And both can communicate with therapist
      But changes by one parent are visible to other

  Rule: Parents can view therapy progress

    @not-implemented
    Scenario: View session summaries
      Given "Emma Wilson" had sessions this week
      When I view session history
      Then I see parent-friendly summaries:
        | Date       | Therapist     | Focus Area        | Progress Note                    |
        | 2024-01-15 | Sarah Johnson | Fine Motor        | Worked on letter formation       |
        | 2024-01-17 | Amy Lee       | Articulation      | Practiced 'r' sounds in words    |
      But I cannot see:
        | Restricted Info        |
        | Clinical diagnoses     |
        | Other student names    |
        | Detailed SOAP notes    |

    @not-implemented
    Scenario: View goal progress
      Given "Emma Wilson" has active IEP goals
      When I view goals section
      Then I see simplified goal information:
        | Goal Area      | Current Level | Target    | Progress |
        | Handwriting    | Forms 65%     | Forms 80% | On Track |
        | Speech Sounds  | 70% accurate  | 90%       | Improving |
      And visual progress bars
      And last updated dates
      But not clinical measurement details

    @not-implemented
    Scenario: Access progress reports
      Given quarterly reports are available
      When I view reports section
      Then I can download:
        | Report Type        | Format | Content                      |
        | Progress Summary   | PDF    | Quarter achievements         |
        | Home Activities    | PDF    | Recommended practice         |
        | Goal Update       | PDF    | IEP goal progress            |
      And reports are watermarked
      And download is logged

  Rule: Parents can access educational resources

    @not-implemented
    Scenario: View assigned home activities
      Given therapist assigned home practice
      When I view home activities
      Then I see:
        | Activity          | Frequency    | Instructions              | Demo |
        | Pencil Exercises  | Daily 5 min  | Step-by-step guide       | Video |
        | Speech Practice   | 3x daily     | Word list with audio     | Audio |
      And I can mark activities as completed
      And completion is visible to therapist

    @not-implemented
    Scenario: Access resource library
      Given general resources are available
      When I browse resource library
      Then I see categories:
        | Category              | Resources                        |
        | Fine Motor           | 15 activities, 5 videos         |
        | Speech Development   | 20 games, 10 printables         |
        | Sensory Activities   | 12 guides, 8 videos             |
      And resources are filtered by child's age
      And downloads are tracked

    @not-implemented
    Scenario: Watch instructional videos
      Given therapist shared technique videos
      When I access video library
      Then videos include:
        | Video Title                  | Duration | Therapist    |
        | Proper Pencil Grip          | 3:45     | Sarah Johnson |
        | Making Speech Practice Fun   | 5:20     | Amy Lee      |
      And videos track completion
      And captions are available

  Rule: Parents can communicate with therapy team

    @not-implemented
    Scenario: Send message to therapist
      Given I have a question about home practice
      When I send message:
        | To      | Sarah Johnson                           |
        | Subject | Question about pencil grip             |
        | Message | Emma is holding the pencil differently |
      Then message is delivered to therapist
      And I receive confirmation
      And therapist has 48 hours to respond
      And conversation is threaded

    @not-implemented
    Scenario: Request appointment
      Given I need to discuss concerns
      When I request meeting with:
        | Therapist       | Sarah Johnson           |
        | Preferred Times | Weekdays after 3 PM     |
        | Meeting Type    | Phone call              |
        | Topics          | Progress concerns       |
      Then request is sent to therapist
      And therapist proposes available times
      And confirmation includes video link if virtual

    @not-implemented
    Scenario: Emergency communication
      Given urgent situation arose
      When I mark message as urgent
      Then therapist receives immediate notification
      And backup contact is notified if no response
      And school is alerted per protocol

  Rule: Parents can manage practical matters

    @not-implemented
    Scenario: Update contact information
      Given my phone number changed
      When I update profile:
        | Field         | New Value      |
        | Phone        | 555-0123       |
        | Alt Phone    | 555-0456       |
      Then changes are saved
      And therapist is notified
      And verification is required for email changes

    @not-implemented
    Scenario: Manage consent forms
      Given new consent forms are required
      When I view pending forms
      Then I see:
        | Form                    | Due Date    | Status   |
        | Photo Release          | 2024-02-01  | Pending  |
        | Field Trip Permission  | 2024-02-15  | Pending  |
      And I can electronically sign
      And signed forms are timestamped
      And copies are available for download

    @not-implemented
    Scenario: View scheduling calendar
      Given "Emma Wilson" has recurring sessions
      When I view calendar
      Then I see:
        | Day       | Time        | Service | Location     |
        | Monday    | 9:00 AM     | OT      | OT Room      |
        | Thursday  | 1:30 PM     | Speech  | Speech Room  |
      And upcoming changes are highlighted
      And I can request schedule changes

  Rule: Parent portal respects privacy regulations

    @not-implemented
    Scenario: Access restricted by custody agreement
      Given custody restricts information sharing
      When non-custodial parent attempts access
      Then access is denied
      And attempt is logged
      And appropriate parties are notified

    @not-implemented
    Scenario: Information sharing preferences
      Given I have two children in therapy
      When I set privacy preferences:
        | Setting                    | Choice           |
        | Share progress with spouse | Yes              |
        | Include in group emails   | No               |
        | Allow therapist photos    | Yes, internal only |
      Then preferences are applied
      And override any defaults

    @not-implemented
    Scenario: Data export request
      Given FERPA rights
      When I request full data export
      Then request is processed within 45 days
      And export includes all:
        | Data Type        |
        | Session records  |
        | Communications   |
        | Progress reports |
        | Consent forms    |

  Rule: Portal provides notification preferences

    @not-implemented
    Scenario: Configure notifications
      Given I want specific updates
      When I set notification preferences:
        | Event                  | Email | SMS | Push |
        | Session completed      | Yes   | No  | Yes  |
        | New message           | Yes   | Yes | Yes  |
        | Progress report ready | Yes   | No  | Yes  |
        | Schedule change       | Yes   | Yes | Yes  |
      Then preferences are saved
      And notifications follow settings

    @not-implemented
    Scenario: Weekly summary email
      Given I opted into weekly summaries
      When Friday evening arrives
      Then I receive email with:
        | Section              | Content                     |
        | Sessions This Week   | 3 completed, 0 missed      |
        | Progress Highlights  | New milestone reached      |
        | Upcoming Week       | 3 sessions scheduled       |
        | Home Practice       | 80% completion rate        |
        | Messages            | 1 unread from therapist    |