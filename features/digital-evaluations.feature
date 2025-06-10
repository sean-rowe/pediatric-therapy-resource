Feature: Digital Evaluations
  As a therapist
  I want to conduct and document evaluations digitally
  So that I can efficiently assess students and track outcomes

  Background:
    Given I am logged in as evaluating therapist
    And the following evaluation types are configured:
      | Type                        | Service | Duration | Sections                          |
      | Initial OT Evaluation       | OT      | 60 min   | Sensory, Motor, ADL, Academic     |
      | Speech Language Evaluation  | SLP     | 90 min   | Articulation, Language, Pragmatics |
      | Physical Therapy Evaluation | PT      | 60 min   | Strength, Balance, Mobility        |

  Rule: Evaluations must follow standardized protocols

    Scenario: Start new evaluation
      Given "Emma Wilson" is referred for OT evaluation
      When I start a new evaluation on "2024-01-15"
      Then the evaluation includes:
        | Component            | Status                          |
        | Student Demographics | Auto-populated from profile     |
        | Referral Reason     | Required field                  |
        | Parent Concerns     | Text entry section              |
        | Teacher Input       | Structured questionnaire        |
        | Assessment Battery  | Standard protocol checklist     |
        | Observations        | Structured and free-text        |
      And evaluation ID is generated
      And status is "in progress"

    Scenario: Complete standardized assessment
      Given I am conducting "Peabody Developmental Motor Scales"
      When I enter assessment data:
        | Subtest           | Raw Score | Age Equivalent |
        | Reflexes          | 8         | 6.2           |
        | Stationary        | 42        | 6.5           |
        | Locomotion        | 65        | 6.8           |
        | Object Manipulation | 38      | 5.9           |
        | Grasping          | 25        | 6.0           |
        | Visual-Motor      | 52        | 6.3           |
      Then standard scores are calculated automatically
      And percentile ranks are displayed
      And age equivalents are graphed
      And interpretive ranges are highlighted

    Scenario: Document clinical observations
      Given I am observing "Emma Wilson" during evaluation
      When I document observations for:
        | Area                | Observation                                    |
        | Attention          | Sustained for 10-12 minutes with breaks       |
        | Behavior           | Cooperative, sought approval frequently        |
        | Sensory Response   | Avoided loud noises, sought deep pressure     |
        | Motor Planning     | Required verbal cues for multi-step tasks     |
        | Social Interaction | Engaged appropriately with examiner           |
      Then observations are linked to evaluation areas
      And support interpretation of scores

  Rule: Evaluations must integrate multiple data sources

    Scenario: Import teacher questionnaire responses
      Given teacher "Ms. Smith" completed input form
      When I import the responses
      Then I see structured data:
        | Question Category    | Response Summary                    |
        | Classroom Performance | Below grade level in writing       |
        | Peer Interaction     | Age-appropriate                     |
        | Following Directions | Requires repetition and visual cues |
        | Organization Skills  | Significant difficulties noted      |
      And responses inform evaluation summary

    Scenario: Include work samples
      Given I have collected work samples from "Emma Wilson"
      When I attach to evaluation:
        | Sample Type      | File           | Notes                           |
        | Handwriting      | writing_1.jpg  | Baseline writing sample         |
        | Drawing          | person_1.jpg   | Human figure drawing            |
        | Cutting          | cutting_1.jpg  | Curved line cutting attempt     |
      Then samples are stored securely
      And samples support clinical findings
      And samples can be referenced in report

    Scenario: Record parent interview
      Given I am interviewing parents during evaluation
      When I document parent input:
        | Topic               | Parent Report                            |
        | Developmental History | Delayed milestones, walked at 18 months |
        | Medical History     | Premature birth, 32 weeks               |
        | Current Concerns    | Struggles with buttons, zippers          |
        | Home Behavior       | Avoids coloring, prefers physical play  |
        | Previous Services   | ECI services until age 3                |
      Then information integrates into evaluation
      And relevant history highlights appear in summary

  Rule: Evaluation reports must be comprehensive

    Scenario: Generate evaluation report
      Given I have completed all evaluation components
      When I generate the report
      Then the report includes:
        | Section                  | Content                                |
        | Identifying Information  | Student demographics, dates            |
        | Reason for Referral     | Referral source and concerns          |
        | Background Information  | History, previous services            |
        | Evaluation Methods      | Tests administered, observations      |
        | Test Results           | Scores, tables, graphs                |
        | Clinical Observations  | Behavioral notes, performance         |
        | Interpretation         | Score analysis, patterns              |
        | Summary               | Strengths and areas of concern        |
        | Recommendations       | Service eligibility, frequency        |
        | Goals                 | Proposed IEP goals if eligible        |
      And report follows district template
      And language is parent-friendly

    Scenario: Customize report sections
      Given I am finalizing evaluation report
      When I customize the generated content:
        | Action              | Details                              |
        | Edit Summary       | Add specific classroom examples      |
        | Reorder Sections   | Move recommendations before goals    |
        | Add Custom Section | Include sensory profile results      |
        | Adjust Language    | Simplify technical terms             |
      Then customizations are applied
      And report maintains professional format
      And changes are tracked

  Rule: Evaluations determine service eligibility

    Scenario: Determine eligibility - qualifies
      Given "Emma Wilson" completed OT evaluation
      And evaluation shows:
        | Criteria                    | Result                    |
        | Standard Scores            | >1.5 SD below mean        |
        | Educational Impact         | Documented in classroom   |
        | Need for Specialized Services | Confirmed by team      |
      When I complete eligibility determination
      Then student qualifies for services
      And recommended service level is "30 minutes weekly"
      And eligibility summary is generated

    Scenario: Determine eligibility - does not qualify
      Given "Liam Brown" completed evaluation
      And scores are within normal limits
      When I complete eligibility determination
      Then student does not qualify for direct services
      And report includes:
        | Component              | Content                           |
        | Explanation           | Scores within typical range       |
        | Recommendations       | Classroom accommodations only     |
        | Monitoring Plan       | Re-evaluate if concerns arise     |
        | Parent Resources      | Home activity suggestions         |

    Scenario: Borderline eligibility case
      Given evaluation results are borderline
      When I document additional considerations:
        | Factor                  | Impact                          |
        | Teacher Concern Level   | High - daily struggles         |
        | Parent Priority        | Requesting services            |
        | Response to Intervention | Limited progress documented   |
        | Peer Comparison        | Significantly behind peers     |
      Then multi-disciplinary team review is triggered
      And additional documentation supports decision

  Rule: Evaluation data must be secure and accessible

    Scenario: Share evaluation with IEP team
      Given evaluation is complete and signed
      When I share with IEP team members
      Then team members can:
        | Role                | Access Level                   |
        | Special Ed Director | Full report and raw data      |
        | Case Manager       | Full report                    |
        | General Ed Teacher | Summary and recommendations    |
        | Parents           | Full report in parent portal   |
      And access is logged
      And documents are watermarked

    Scenario: Lock evaluation after ARD meeting
      Given evaluation was reviewed in ARD
      When ARD meeting is completed
      Then evaluation is locked from edits
      And addendums require new evaluation
      And original remains unchanged

  Rule: Evaluations support re-evaluation tracking

    Scenario: Schedule re-evaluation
      Given "Emma Wilson" had initial evaluation on "2021-01-15"
      And re-evaluations are required every 3 years
      When system date approaches "2024-01-15"
      Then re-evaluation reminder is triggered
      And notification sent 60 days prior
      And re-evaluation is added to calendar

    Scenario: Compare evaluation results over time
      Given "Emma Wilson" has multiple evaluations:
        | Date       | Type    | Key Finding              |
        | 2021-01-15 | Initial | Significant delays       |
        | 2022-06-01 | Annual  | Moderate progress        |
        | 2024-01-15 | Triennial | Approaching grade level |
      When I view evaluation history
      Then I see progress graphs
      And score comparisons
      And trend analysis
      And progress narrative

    Scenario: Document progress since last evaluation
      Given I am conducting re-evaluation
      When I review previous recommendations
      Then I document:
        | Previous Recommendation | Current Status                |
        | OT 2x weekly           | Implemented, good progress    |
        | Sensory breaks         | Effective strategy            |
        | Pencil grip support    | Still needed                  |
      And progress informs current recommendations