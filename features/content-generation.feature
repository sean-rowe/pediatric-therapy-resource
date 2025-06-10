Feature: AI Content Generation
  As a therapist
  I want to generate educational materials and activities
  So that I can provide engaging therapy resources quickly

  Background:
    Given I am logged in as therapist "Sarah Johnson"
    And I have access to AI content generation
    And the following content templates exist:
      | ID | Type              | Category    | Grade Range |
      | 1  | Worksheet         | Fine Motor  | K-2         |
      | 2  | Visual Schedule   | Behavior    | All         |
      | 3  | Social Story      | Social      | 3-5         |

  Rule: Content generation requires clear parameters

    Scenario: Generate fine motor worksheet
      Given I am working with "Emma Wilson" in grade "3"
      When I request content generation with:
        | Field             | Value                                    |
        | Content Type      | Worksheet                               |
        | Category          | Fine Motor                              |
        | Skill Focus       | Letter formation                        |
        | Difficulty Level  | Intermediate                            |
        | Theme            | Animals                                 |
        | Additional Notes  | Include tracing and independent writing |
      Then content is generated within 10 seconds
      And the content includes:
        | Element            | Description                        |
        | Title             | Animal Letter Tracing              |
        | Instructions      | Clear student directions           |
        | Tracing Section   | Dotted letters with animal themes  |
        | Practice Section  | Blank lines for independent work   |
        | Visual Supports   | Animal illustrations as guides     |
      And the content is saved to my library
      And usage metrics are tracked

    Scenario: Generate social story
      Given "Liam Brown" needs support with "transitioning between activities"
      When I generate a social story with:
        | Field           | Value                              |
        | Topic           | Transitioning between activities   |
        | Student Name    | Liam                              |
        | Setting         | Classroom                         |
        | Key Points      | Warning before transition, cleanup steps, what comes next |
        | Visual Style    | Simple line drawings              |
      Then a personalized social story is created
      And it includes:
        | Component        | Content                                    |
        | Title Page       | "Liam's Transition Story"                 |
        | Scenario Pages   | 6-8 pages with simple text               |
        | Visual Supports  | Consistent character representing Liam    |
        | Positive Ending  | Success scenario reinforcement           |

    Scenario: Generate with specific IEP goal alignment
      Given "Emma Wilson" has goal "OT-1" for handwriting
      When I generate content aligned to goal "OT-1"
      Then the system suggests appropriate content types
      And generated content references the goal
      And progress tracking is integrated

  Rule: Generated content must be customizable

    Scenario: Customize generated worksheet
      Given I generated a worksheet "Animal Letter Tracing"
      When I edit the worksheet to:
        | Action                | Details                          |
        | Add section          | Number formation 1-5             |
        | Remove element       | Advanced cursive section         |
        | Modify instructions  | Add parent guidance note         |
        | Change difficulty    | Make tracing lines bolder        |
      Then the edits are applied
      And a new version is saved
      And the original remains available

    Scenario: Adjust content for student needs
      Given I have a generated visual schedule
      When I customize for "Emma Wilson" who uses hearing aids
      Then I can:
        | Customization          | Implementation                  |
        | Add visual alerts      | Flashing border for transitions |
        | Increase image size    | 150% larger icons              |
        | Simplify text         | Single words only              |
        | Add tactile markers   | Textured areas for key items   |

  Rule: Content must be evidence-based

    Scenario: View content effectiveness
      Given I have used "Fine Motor Worksheet #123" with 5 students
      When I view content analytics
      Then I see:
        | Metric                  | Value                         |
        | Usage Count            | 5 students, 12 sessions       |
        | Average Engagement     | 4.2/5                        |
        | Goal Progress Impact   | +15% average improvement     |
        | Therapist Ratings      | 4.5/5 from 3 therapists      |
        | Modification Frequency | 20% customize before use     |

    Scenario: Rate content after use
      Given I used "Social Story #456" with "Liam Brown"
      When I complete the session
      Then I am prompted to rate:
        | Rating Category      | Scale |
        | Student Engagement   | 1-5   |
        | Effectiveness       | 1-5   |
        | Appropriateness     | 1-5   |
      And I can provide qualitative feedback
      And ratings influence future recommendations

  Rule: Content library must be searchable

    Scenario: Search content library
      Given I have 50+ items in my content library
      When I search for "sensory" materials for "grade 2"
      Then results are filtered by:
        | Filter           | Applied Value    |
        | Keyword         | sensory          |
        | Grade Level     | 2                |
        | My Content      | Created/Modified |
        | Shared Content  | Team accessible  |
      And results show preview thumbnails
      And I can further filter by rating

    Scenario: Browse by student need
      Given I am planning for "Emma Wilson"
      When I browse content recommendations
      Then I see content organized by:
        | Organization        | Based On                    |
        | Current Goals      | Her active IEP goals        |
        | Past Success       | Previously effective items  |
        | Similar Students   | What worked for others      |
        | Recent Additions   | New content this month      |

  Rule: Content sharing must maintain compliance

    Scenario: Share content with team
      Given I created "Handwriting Improvement Pack"
      When I share with my OT team
      Then the content is:
        | Action              | Result                           |
        | Copied to team library | Available to all team OTs     |
        | Attribution maintained | Shows "Created by Sarah Johnson" |
        | Edit permissions    | View only for others            |
        | Usage tracked       | Shows who used and when         |

    Scenario: Export content for parents
      Given I have content approved for home use
      When I export "Home Practice Pack" for "Emma Wilson's" parents
      Then the export:
        | Feature             | Implementation                  |
        | Format             | PDF with print settings         |
        | Watermark          | "For Wilson family use only"    |
        | Instructions       | Parent-friendly language        |
        | Tracking          | Logged in student record        |
        | Expiration        | Links expire after 30 days      |

  Rule: AI prompts must be appropriate

    Scenario: Content generation with inappropriate request
      When I request content with inappropriate elements:
        | Field    | Value                           |
        | Type     | Worksheet                       |
        | Content  | Generate medical advice content |
      Then generation fails with "Content must be educational therapy materials only"
      And I am reminded of appropriate use guidelines

    Scenario: Generate culturally sensitive content
      Given I need materials for diverse student population
      When I generate content with cultural considerations:
        | Field                | Value                        |
        | Include Diverse Names | Yes                         |
        | Multiple Languages   | English and Spanish         |
        | Cultural Holidays    | Inclusive representations   |
        | Family Structures    | Various configurations      |
      Then content reflects requested diversity
      And imagery is culturally appropriate
      And language is inclusive

  Rule: Content generation has usage limits

    Scenario: Track generation quota
      Given I have "Professional" subscription with 100 monthly generations
      And I have used 95 generations this month
      When I view my usage
      Then I see:
        | Metric              | Value                    |
        | Used               | 95/100                   |
        | Remaining          | 5                        |
        | Reset Date         | February 1, 2024         |
        | Average per Day    | 4.2                      |
      And I receive warning about approaching limit

    Scenario: Exceed generation limit
      Given I have used 100/100 monthly generations
      When I attempt to generate new content
      Then I receive options:
        | Option              | Details                       |
        | Upgrade Plan       | Professional Plus (200/month) |
        | Purchase Add-on    | 20 generations for $9.99      |
        | Wait              | Resets in 3 days              |
        | Use Existing      | Browse 500+ library items     |