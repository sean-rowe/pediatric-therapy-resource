Feature: Therapy Planning API Endpoints (FR-003)
  As a therapy professional
  I want to create and manage therapy plans
  So that I can provide structured, goal-oriented interventions

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # POST /api/therapy-plans
  @endpoint @therapy-plans @creation @not-implemented
  Scenario: Create new therapy plan
    When I send a POST request to "/api/therapy-plans" with:
      | field           | value                               |
      | studentId       | student-123                         |
      | planType        | individual                          |
      | duration        | 12-weeks                            |
      | frequency       | 2x-weekly                           |
      | sessionLength   | 30-minutes                          |
      | startDate       | 2024-02-01                          |
      | goals           | ["goal-456", "goal-789"]            |
      | focusAreas      | ["articulation", "language"]        |
      | settingType     | pull-out                            |
    Then the response status should be 201
    And the response should contain:
      | field      | type   |
      | planId     | string |
      | schedule   | array  |
      | resources  | array  |
    And plan should be generated with appropriate resources

  # POST /api/therapy-plans/generate
  @endpoint @therapy-plans @ai-generation @not-implemented
  Scenario: Generate AI-powered therapy plan
    Given student "student-123" has IEP goals
    When I send a POST request to "/api/therapy-plans/generate" with:
      | field              | value                    |
      | studentIds         | ["student-123"]          |
      | planDuration       | 4-weeks                  |
      | sessionFrequency   | 2x-weekly                |
      | includeHomeProgram | true                     |
      | evidenceLevel      | high                     |
      | adaptForNeeds      | ["autism", "sensory"]    |
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | suggestedPlan      | object |
      | weeklyBreakdown    | array  |
      | resourceList       | array  |
      | homeActivities     | array  |
      | progressMilestones | array  |
    And plan should align with IEP goals

  # GET /api/therapy-plans
  @endpoint @therapy-plans @list @not-implemented
  Scenario: List all therapy plans
    Given I have created multiple therapy plans
    When I send a GET request to "/api/therapy-plans?status=active"
    Then the response status should be 200
    And the therapy response should contain array of:
      | field         | type   |
      | planId        | string |
      | studentName   | string |
      | planType      | string |
      | startDate     | string |
      | endDate       | string |
      | progressStatus| string |
      | completion    | number |

  # GET /api/therapy-plans/{id}
  @endpoint @therapy-plans @details @not-implemented
  Scenario: Get therapy plan details
    Given therapy plan "plan-123" exists
    When I send a GET request to "/api/therapy-plans/plan-123"
    Then the response status should be 200
    And the response should contain:
      | field             | type   |
      | planId            | string |
      | student           | object |
      | goals             | array  |
      | weeklySchedule    | array  |
      | sessionPlans      | array  |
      | resourcesAssigned | array  |
      | progressTracking  | object |
      | modifications     | array  |

  # PUT /api/therapy-plans/{id}
  @endpoint @therapy-plans @update @not-implemented
  Scenario: Update therapy plan
    Given I own therapy plan "plan-123"
    When I send a PUT request to "/api/therapy-plans/plan-123" with:
      | field        | value                        |
      | frequency    | 3x-weekly                    |
      | addGoals     | ["goal-012"]                 |
      | notes        | Increased frequency due to progress |
    Then the response status should be 200
    And plan should be updated
    And modification history should be recorded

  # POST /api/therapy-plans/{id}/sessions
  @endpoint @therapy-plans @sessions @not-implemented
  Scenario: Create session plan from therapy plan
    Given therapy plan "plan-123" exists
    When I send a POST request to "/api/therapy-plans/plan-123/sessions" with:
      | field        | value                    |
      | weekNumber   | 3                        |
      | sessionNumber| 1                        |
      | customizeFor | student-123              |
    Then the response status should be 201
    And the response should contain:
      | field           | type   |
      | sessionId       | string |
      | activities      | array  |
      | materials       | array  |
      | timeBreakdown   | object |
      | dataCollection  | array  |

  # GET /api/therapy-plans/{id}/progress
  @endpoint @therapy-plans @progress @not-implemented
  Scenario: Get therapy plan progress
    Given therapy plan "plan-123" has been active for 4 weeks
    When I send a GET request to "/api/therapy-plans/plan-123/progress"
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | overallProgress    | number |
      | goalProgress       | array  |
      | sessionsCompleted  | number |
      | sessionsRemaining  | number |
      | projectedCompletion| string |
      | recommendations    | array  |

  # POST /api/therapy-plans/{id}/modify
  @endpoint @therapy-plans @modifications @not-implemented
  Scenario: Modify therapy plan based on progress
    Given therapy plan "plan-123" shows slow progress
    When I send a POST request to "/api/therapy-plans/plan-123/modify" with:
      | field              | value                           |
      | modificationType   | increase-support                |
      | changes            | ["add-visual-supports", "simplify-goals"] |
      | effectiveDate      | 2024-03-01                      |
      | rationale          | Student requires additional support |
    Then the response status should be 200
    And modifications should be applied
    And parent should be notified

  # POST /api/therapy-plans/{id}/copy
  @endpoint @therapy-plans @duplication @not-implemented
  Scenario: Copy therapy plan for another student
    Given successful therapy plan "plan-123" exists
    When I send a POST request to "/api/therapy-plans/plan-123/copy" with:
      | field          | value                    |
      | targetStudent  | student-456              |
      | adjustForAge   | true                     |
      | keepResources  | true                     |
    Then the response status should be 201
    And new plan should be created
    And age-appropriate adjustments should be made

  # DELETE /api/therapy-plans/{id}
  @endpoint @therapy-plans @deletion @not-implemented
  Scenario: Archive therapy plan
    Given therapy plan "plan-123" is complete
    When I send a DELETE request to "/api/therapy-plans/plan-123"
    Then the response status should be 200
    And plan should be archived
    And historical data should be preserved
    And plan should not appear in active list

  # POST /api/therapy-plans/group
  @endpoint @therapy-plans @group @not-implemented
  Scenario: Create group therapy plan
    Given I have students with similar goals
    When I send a POST request to "/api/therapy-plans/group" with:
      | field           | value                              |
      | studentIds      | ["student-123", "student-456", "student-789"] |
      | groupName       | Social Skills Group                |
      | commonGoals     | ["turn-taking", "conversation"]    |
      | meetingDay      | Tuesday                            |
      | meetingTime     | 14:00                              |
      | duration        | 45-minutes                         |
      | weeks           | 8                                  |
    Then the response status should be 201
    And group plan should accommodate all students
    And individual tracking should be maintained

  # GET /api/therapy-plans/{id}/materials
  @endpoint @therapy-plans @materials @not-implemented
  Scenario: Get required materials for plan
    Given therapy plan "plan-123" exists
    When I send a GET request to "/api/therapy-plans/plan-123/materials"
    Then the response status should be 200
    And the response should contain:
      | field            | type  |
      | digitalResources | array |
      | printables       | array |
      | manipulatives    | array |
      | equipment        | array |
      | preparationTime  | number|
    And materials should be organized by week

  # POST /api/therapy-plans/{id}/share
  @endpoint @therapy-plans @collaboration @not-implemented
  Scenario: Share plan with colleague
    Given therapy plan "plan-123" exists
    When I send a POST request to "/api/therapy-plans/plan-123/share" with:
      | field          | value                    |
      | recipientEmail | colleague@clinic.com     |
      | permissions    | view-only                |
      | message        | Great plan for articulation |
    Then the response status should be 200
    And colleague should receive notification
    And plan should be viewable by colleague

  # POST /api/therapy-plans/templates
  @endpoint @therapy-plans @templates @not-implemented
  Scenario: Save plan as template
    Given successful therapy plan "plan-123" exists
    When I send a POST request to "/api/therapy-plans/templates" with:
      | field          | value                         |
      | sourcePlanId   | plan-123                      |
      | templateName   | Articulation 8-Week Program   |
      | description    | Evidence-based /r/ program    |
      | tags           | ["articulation", "speech"]    |
      | shareWithTeam  | true                          |
    Then the response status should be 201
    And template should be created
    And be available for future use

  # GET /api/therapy-plans/templates
  @endpoint @therapy-plans @templates @not-implemented
  Scenario: Get available plan templates
    When I send a GET request to "/api/therapy-plans/templates?type=articulation"
    Then the response status should be 200
    And the therapy response should contain array of:
      | field         | type   |
      | templateId    | string |
      | name          | string |
      | description   | string |
      | duration      | string |
      | successRate   | number |
      | usageCount    | number |

  # POST /api/therapy-plans/{id}/report
  @endpoint @therapy-plans @reporting @not-implemented
  Scenario: Generate plan summary report
    Given therapy plan "plan-123" is 75% complete
    When I send a POST request to "/api/therapy-plans/plan-123/report" with:
      | field      | value                  |
      | reportType | progress-summary       |
      | format     | pdf                    |
      | audience   | parent                 |
      | language   | en                     |
    Then the response status should be 200
    And therapy report should include:
      | content           |
      | Goals addressed   |
      | Progress made     |
      | Activities used   |
      | Recommendations   |
      | Next steps        |

  # FR-003 Comprehensive Therapy Planning Business Scenarios from CLAUDE.md
  @therapy-planning @iep-alignment @automated-planning @not-implemented
  Scenario: Generate 4-week therapy plan for student with multiple goals
    Given I have a student "Emma Johnson" with IEP goals:
      | Goal Area              | Specific Goal                                      | Target Date |
      | Fine Motor             | Will cut along curved lines with 80% accuracy     | 05/30/2025  |
      | Bilateral Coordination | Will catch a ball 8/10 times from 5 feet         | 05/30/2025  |
      | Handwriting           | Will write lowercase letters with proper formation | 05/30/2025  |
    When I click "Generate Therapy Plan"
    And I specify:
      | Setting              | Individual therapy    |
      | Session frequency    | 2x per week          |
      | Session duration     | 30 minutes           |
      | Planning period      | 4 weeks              |
    Then the system should generate a plan with:
      | Week | Session | Activities                                      | Goals Addressed    |
      | 1    | 1       | Cutting practice with adaptive scissors        | Fine Motor         |
      | 1    | 2       | Ball activities, letter formation practice     | Bilateral, Writing |
      | 2    | 1       | Curved line cutting, bilateral games          | Fine Motor, Bilateral |
      | 2    | 2       | Handwriting with verbal cues                  | Handwriting        |
    And each activity should link to specific resources
    And progress monitoring tools should be included
    And the plan should be editable and customizable
    And baseline data collection should be suggested
    And parent home activities should be generated

  @therapy-planning @group-planning @efficiency @not-implemented
  Scenario: Create group therapy plan for students with similar goals
    Given I have 3 students with similar gross motor goals:
      | Student        | Primary Goal                    | Secondary Goal        |
      | Alex Chen      | Improve balance and coordination| Increase core strength|
      | Maria Garcia   | Increase core strength         | Develop ball skills   |
      | James Wilson   | Develop ball skills            | Improve balance       |
    When I select multiple students
    And I choose "Create Group Plan"
    And I specify group parameters:
      | Group size    | 3 students      |
      | Session type  | Gross motor group|
      | Duration      | 45 minutes      |
      | Setting       | Gymnasium       |
    Then the system should generate activities suitable for all students
    And indicate differentiation strategies for each student:
      | Student        | Differentiation                   |
      | Alex Chen      | Focus on balance challenges      |
      | Maria Garcia   | Core strengthening emphasis       |
      | James Wilson   | Ball handling progression        |
    And suggest station rotation schedules
    And provide group data collection sheets
    And include team-building activities
    And recommend group size adjustments if needed

  @therapy-planning @adaptive-planning @special-needs @not-implemented
  Scenario: Adapt therapy plan for student with additional needs
    Given I have a student "Marcus" with autism and sensory needs
    And Marcus has existing therapy goals for communication
    When I enable "Adaptive Planning Mode"
    And I specify additional considerations:
      | Consideration        | Details                      |
      | Sensory preferences  | Avoids loud noises          |
      | Communication       | Uses AAC device             |
      | Behavioral supports | Needs visual schedule       |
      | Attention span      | 10-minute maximum           |
    Then the generated plan should include:
      | Adaptation Type     | Implementation                |
      | Sensory modifications| Quiet activity alternatives  |
      | AAC integration     | Communication boards ready   |
      | Visual supports     | Schedule cards for each activity|
      | Attention breaks    | Built-in movement breaks     |
    And transition strategies between activities
    And sensory breaks built into the schedule
    And communication partner training suggestions
    And crisis intervention protocols

  @therapy-planning @evidence-based @research-integration @not-implemented
  Scenario: Generate plan using evidence-based practices
    Given I need to create a plan for "Sophie" with apraxia
    When I select "Evidence-Based Planning"
    And I specify:
      | Condition        | Childhood Apraxia of Speech |
      | Severity         | Moderate                    |
      | Age              | 6 years                     |
      | Previous therapy | 1 year of traditional therapy|
    Then the system should recommend:
      | Intervention      | Evidence Level | Research Basis        |
      | PROMPT technique  | High          | Multiple RCTs         |
      | Dynamic assessment| High          | Systematic reviews    |
      | Intensive practice| High          | Motor learning theory |
      | Multimodal cues   | Moderate      | Clinical studies      |
    And each intervention should include:
      | Information       | Details                       |
      | Research citations| Links to supporting studies   |
      | Dosage guidelines | Frequency and intensity      |
      | Progress markers  | Expected timeline             |
      | Outcome measures  | Valid assessment tools       |
    And the plan should align with clinical practice guidelines
    And include fidelity monitoring tools

  @therapy-planning @progress-monitoring @data-integration @not-implemented
  Scenario: Plan with integrated progress monitoring
    Given I am creating a plan for "David" with fluency goals
    When I set up the therapy plan
    And I enable "Continuous Progress Monitoring"
    Then the system should:
      | Feature              | Implementation                 |
      | Baseline collection  | Schedule initial assessments   |
      | Regular probes       | Weekly fluency samples        |
      | Progress graphs      | Real-time visual feedback      |
      | Decision rules       | When to modify intervention    |
    And monitoring schedule should include:
      | Frequency        | Measure                        |
      | Daily           | Severity rating scale          |
      | Weekly          | Fluency rate calculation       |
      | Bi-weekly       | Attitude assessment            |
      | Monthly         | Functional communication       |
    And progress alerts should notify when:
      | Condition           | Alert Type                     |
      | No progress 3 weeks | Intervention modification needed|
      | Regression noted    | Immediate review required      |
      | Goal achieved       | Advance to next level         |
    And data should automatically populate reports

  @therapy-planning @family-involvement @home-programs @not-implemented
  Scenario: Create therapy plan with family involvement
    Given I am planning therapy for "Isabella" with language delays
    And her parents want to be actively involved
    When I create the therapy plan
    And I enable "Family-Centered Planning"
    Then the plan should include:
      | Component           | Details                        |
      | Parent training     | Weekly coaching sessions       |
      | Home activities     | Daily practice routines        |
      | Family goals        | Priorities identified by family|
      | Cultural considerations| Respect for family values    |
    And home program should provide:
      | Resource            | Format                         |
      | Activity instructions| Step-by-step guides           |
      | Video demonstrations| Short clips showing techniques |
      | Progress tracking   | Simple data collection forms  |
      | Troubleshooting     | Common problems and solutions  |
    And family support should include:
      | Support Type        | Description                    |
      | Regular check-ins   | Weekly progress discussions    |
      | Skill workshops     | Monthly parent training        |
      | Sibling activities  | Include other family members   |
      | Community resources | Local support groups          |

  @therapy-planning @teletherapy @remote-delivery @not-implemented
  Scenario: Adapt therapy plan for teletherapy delivery
    Given student "Ryan" will receive services via teletherapy
    And he has articulation goals
    When I create a teletherapy plan
    Then the plan should include:
      | Teletherapy Element | Adaptation                     |
      | Technology setup    | Platform requirements          |
      | Caregiver coaching  | Parent as therapy partner     |
      | Digital resources   | Screen-shareable materials     |
      | Engagement strategies| Interactive online activities |
    And each session should specify:
      | Component          | Teletherapy Modification       |
      | Setup time         | 5 minutes for tech check      |
      | Caregiver briefing | 5 minutes parent instruction   |
      | Direct therapy     | 20 minutes structured activity |
      | Wrap-up           | 5 minutes review and homework  |
    And troubleshooting guides should address:
      | Technical Issue    | Solution                       |
      | Poor connection    | Offline backup activities      |
      | Audio problems     | Visual cue alternatives        |
      | Engagement issues  | Movement breaks and games      |
      | Caregiver questions| Quick reference guides         |

  @therapy-planning @transition-planning @life-skills @not-implemented
  Scenario: Create transition-focused therapy plan
    Given I have a 17-year-old student "Alex" preparing for graduation
    And he has communication goals for employment
    When I create a transition therapy plan
    Then the plan should focus on:
      | Transition Area     | Therapy Goals                  |
      | Workplace communication| Professional interaction skills|
      | Self-advocacy       | Requesting accommodations      |
      | Social skills       | Peer relationships at work     |
      | Independence        | Problem-solving strategies     |
    And activities should include:
      | Activity Type       | Real-world Application         |
      | Job interviews      | Practice sessions with feedback|
      | Workplace scenarios | Role-playing common situations |
      | Community access    | Ordering food, asking directions|
      | Technology use      | Work-related communication apps|
    And the plan should coordinate with:
      | Team Member         | Collaboration Focus            |
      | Vocational counselor| Job placement preparation      |
      | Special education   | IEP transition planning        |
      | Family             | Home independence skills       |
      | Employers          | Workplace accommodation needs   |

  @therapy-planning @caseload-management @efficiency @not-implemented
  Scenario: Plan therapy across full caseload efficiently
    Given I have 25 students on my caseload
    And I need to create individual therapy plans
    When I use "Caseload Planning Mode"
    Then the system should:
      | Feature             | Benefit                        |
      | Schedule optimization| Minimize travel time          |
      | Resource sharing    | Reuse materials across students|
      | Goal clustering     | Group similar therapy targets  |
      | Efficiency alerts   | Suggest time-saving strategies |
    And caseload overview should show:
      | Information         | Display                        |
      | Weekly schedule     | All students with time slots   |
      | Material needs      | Consolidated supply list       |
      | Due dates           | IEP reviews and evaluations    |
      | Progress alerts     | Students needing attention     |
    And the system should suggest:
      | Optimization        | Rationale                      |
      | Group sessions      | Students with similar goals    |
      | Consultation model  | Appropriate service delivery   |
      | Resource bundles    | Efficient material organization|
      | Documentation blocks| Dedicated time for paperwork   |

  @therapy-planning @collaboration @team-planning @not-implemented
  Scenario: Create collaborative therapy plan with team
    Given student "Maya" receives services from multiple therapists
    And she has complex needs requiring coordination
    When I create a collaborative therapy plan
    Then the plan should include:
      | Team Member         | Role                           |
      | Speech therapist    | Communication and swallowing   |
      | Occupational therapist| Fine motor and sensory       |
      | Physical therapist  | Gross motor and mobility       |
      | Special educator    | Academic and behavioral        |
    And coordination should include:
      | Collaboration Type  | Implementation                 |
      | Shared goals        | Common targets across disciplines|
      | Schedule coordination| Minimize student disruption   |
      | Progress sharing    | Real-time updates between team |
      | Joint sessions      | Integrated therapy approaches  |
    And the plan should prevent:
      | Issue              | Prevention Strategy             |
      | Conflicting goals  | Team consensus on priorities    |
      | Scheduling conflicts| Shared calendar system        |
      | Duplicate services | Clear role definitions         |
      | Communication gaps | Regular team meetings          |

  @therapy-planning @outcome-prediction @data-analytics @not-implemented
  Scenario: Use predictive analytics for therapy planning
    Given I am planning therapy for "Noah" with similar profile to past students
    When I enable "Predictive Planning"
    Then the system should analyze:
      | Data Source         | Analysis                       |
      | Historical outcomes | Similar student success rates  |
      | Intervention patterns| Most effective approaches     |
      | Progress timelines  | Expected improvement rates     |
      | Resource effectiveness| Best materials for this profile|
    And predictions should include:
      | Prediction Type     | Information                    |
      | Goal achievement    | Likelihood of success          |
      | Timeline estimates  | Expected duration to mastery   |
      | Intervention needs  | Intensity and frequency        |
      | Support requirements| Additional services needed     |
    And the system should:
      | Feature            | Purpose                         |
      | Confidence intervals| Show prediction reliability    |
      | Success factors    | Identify key variables         |
      | Risk alerts        | Warn of potential challenges   |
      | Adjustment suggestions| Modify plan based on data    |
    And I should be able to:
      | Action             | Benefit                        |
      | Compare alternatives| See different intervention options|
      | Adjust variables   | Test impact on predictions     |
      | Set monitoring     | Track actual vs predicted      |

  @therapy-planning @quality-assurance @plan-validation @not-implemented
  Scenario: Validate therapy plan quality and compliance
    Given I have completed a therapy plan for "Zoe"
    When I submit the plan for validation
    Then the system should check:
      | Validation Area     | Requirements                   |
      | IEP alignment      | Goals match legal requirements |
      | Evidence base      | Interventions are research-based|
      | Dosage appropriateness| Frequency matches needs      |
      | Developmental appropriateness| Age-suitable activities|
    And compliance checks should verify:
      | Compliance Requirement| Validation                   |
      | IDEA regulations     | IEP requirements met         |
      | State standards      | Local policy compliance      |
      | Professional standards| Ethical practice guidelines |
      | Safety protocols     | Age-appropriate activities   |
    And quality indicators should assess:
      | Quality Metric      | Measurement                    |
      | Goal specificity    | SMART goal criteria met       |
      | Intervention variety| Balanced activity types        |
      | Progress monitoring | Data collection planned       |
      | Family involvement  | Meaningful participation       |
    When validation issues are found:
      | Issue Type         | System Response                |
      | Compliance violation| Block plan until corrected   |
      | Quality concern    | Provide improvement suggestions|
      | Missing elements   | Highlight required components  |
      | Best practice gap  | Offer evidence-based alternatives|