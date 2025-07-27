Feature: Documentation Helpers API Endpoints (FR-028)
  As a therapy professional
  I want integrated documentation support tools
  So that I can efficiently complete required documentation

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # POST /api/documentation/session-notes/generate
  @endpoint @documentation @notes @not-implemented
  Scenario: Generate session notes from activities
    When I send a POST request to "/api/documentation/session-notes/generate" with:
      | field            | value                       |
      | sessionId        | session-123                 |
      | resourcesUsed    | ["res-1", "res-2", "res-3"] |
      | dataPoints       | [{"metric": "accuracy", "value": 85}] |
      | duration         | 30                          |
      | studentResponse  | "engaged, required cues"    |
    Then the response status should be 200
    And generated note should include:
      | section          | content                     |
      | objective        | Resources and goals used    |
      | performance      | Data-driven summary         |
      | clinicalJudgment | Template for observations   |
      | plan             | Next session suggestions    |

  # GET /api/documentation/goal-bank
  @endpoint @documentation @goals @not-implemented
  Scenario: Access insurance-compliant goal bank
    When I send a GET request to "/api/documentation/goal-bank?discipline=OT&setting=outpatient"
    Then the response status should be 200
    And goals should be formatted with:
      | component        | example                     |
      | condition        | "Given visual cues..."      |
      | behavior         | "patient will complete..."  |
      | criteria         | "with 80% accuracy..."      |
      | timeframe        | "within 4 weeks"            |
      | medicalNecessity | "to improve independence"   |

  # POST /api/documentation/progress-report/auto-generate
  @endpoint @documentation @progress @not-implemented
  Scenario: Auto-generate progress report
    When I send a POST request to "/api/documentation/progress-report/auto-generate" with:
      | field         | value                    |
      | studentId     | student-123              |
      | dateRange     | {"start": "2024-01-01", "end": "2024-03-31"} |
      | includeGraphs | true                     |
      | reportType    | quarterly                |
    Then the response status should be 200
    And report should contain:
      | section         | content                  |
      | attendance      | Session summary          |
      | goalProgress    | Objective data charts    |
      | recommendations | Data-driven suggestions  |
      | parentSummary   | Simplified version       |

  # GET /api/documentation/templates/soap
  @endpoint @documentation @soap @not-implemented
  Scenario: Get SOAP note templates
    When I send a GET request to "/api/documentation/templates/soap?specialty=pediatric-PT"
    Then the response status should be 200
    And templates should include:
      | section     | prompts                          |
      | subjective  | ["Parent report", "Pain level"]  |
      | objective   | ["ROM measurements", "Gait"]     |
      | assessment  | ["Progress toward goals"]        |
      | plan        | ["Continue current POC"]         |

  # POST /api/documentation/cpt-helper
  @endpoint @documentation @billing @not-implemented
  Scenario: Get CPT code recommendations
    When I send a POST request to "/api/documentation/cpt-helper" with:
      | field           | value                    |
      | sessionType     | individual               |
      | duration        | 45                       |
      | procedures      | ["ther-ex", "gait-training"] |
      | setting         | outpatient               |
    Then the response status should be 200
    And recommendations should include:
      | cptCode | description              | units |
      | 97110   | Therapeutic exercise     | 2     |
      | 97116   | Gait training            | 1     |

  # POST /api/documentation/quick-phrases/save
  @endpoint @documentation @phrases @not-implemented
  Scenario: Save frequently used documentation phrases
    When I send a POST request to "/api/documentation/quick-phrases/save" with:
      | field         | value                         |
      | category      | progress-notes                |
      | phrase        | "Demonstrated improved motor planning" |
      | shortcut      | "imp-mp"                      |
    Then the response status should be 201
    And phrase should be saved
    And available in documentation tools

  # GET /api/documentation/compliance-check/{noteId}
  @endpoint @documentation @compliance @not-implemented
  Scenario: Check documentation compliance
    When I send a GET request to "/api/documentation/compliance-check/note-123"
    Then the response status should be 200
    And compliance check should show:
      | requirement          | status    | issue              |
      | medicalNecessity     | pass      | null               |
      | skillsAddressed      | pass      | null               |
      | objectiveData        | warning   | "Add measurements" |
      | signatureDate        | fail      | "Missing"          |

  # POST /api/documentation/batch-sign
  @endpoint @documentation @signature @not-implemented
  Scenario: Batch sign documentation
    When I send a POST request to "/api/documentation/batch-sign" with:
      | field         | value                         |
      | noteIds       | ["note-1", "note-2", "note-3"] |
      | signatureType | electronic                    |
      | credentials   | {"pin": "1234"}               |
    Then the response status should be 200
    And all notes should be signed
    And audit trail should be created

  # GET /api/documentation/time-tracking/{date}
  @endpoint @documentation @time @not-implemented
  Scenario: Track documentation time
    When I send a GET request to "/api/documentation/time-tracking/2024-01-22"
    Then the response status should be 200
    And tracking should show:
      | activity          | duration  | percentage |
      | direct service    | 360       | 75%        |
      | documentation     | 90        | 19%        |
      | prep time         | 30        | 6%         |

  # POST /api/documentation/report-writer
  @endpoint @documentation @reports @not-implemented
  Scenario: Use AI-assisted report writer
    When I send a POST request to "/api/documentation/report-writer" with:
      | field         | value                    |
      | reportType    | initial-evaluation       |
      | testScores    | {"BOT-2": 45, "Beery": 38} |
      | observations  | "Difficulty with bilateral coordination" |
      | diagnosis     | "Developmental Coordination Disorder" |
    Then the response status should be 200
    And draft report should include:
      | section         | content                  |
      | background      | Referral information     |
      | results         | Test score interpretation |
      | recommendations | Evidence-based interventions |
      | goals           | Measurable objectives    |

  # FR-028 Missing Critical Documentation Helpers Business Workflow Scenarios
  @auto-session-notes @resource-correlation @workflow @not-implemented
  Scenario: Generate comprehensive session notes from resource usage and data collection
    Given I completed a 30-minute session with student "Emma Rodriguez"
    And I used multiple resources during the session
    When I access the auto-generated session note feature
    And I review the session timeline:
      | Time    | Resource/Activity       | Student Performance    | Data Collected     |
      | 0-5min  | Sensory warm-up cards  | Required verbal cues   | Engagement: High   |
      | 5-15min | Fine motor worksheets  | 75% accuracy           | Correct: 15/20     |
      | 15-25min| Handwriting practice   | Improved from baseline | Letter formation: 8/10 |
      | 25-30min| Calming activities     | Independent use        | Self-regulation: Yes |
    Then the system should auto-populate comprehensive notes:
      | Session Section      | Auto-Generated Content            |
      | Objective activities | List of resources used with timestamps |
      | Performance data     | Quantitative measures from activities |
      | Clinical observations| Template prompts for qualitative notes |
      | Student engagement   | Engagement levels throughout session |
      | Progress indicators  | Comparison to previous sessions    |
    And I should be able to customize each section:
      | Customization Option | Implementation                  |
      | Edit auto-text       | Full text editing capability    |
      | Add observations     | Free text with clinical prompts |
      | Link to goals        | Connect activities to IEP goals |
      | Insert quick phrases | Pre-saved common observations   |
    When I finalize the session note
    Then the documentation should include:
      | Documentation Element| Content                         |
      | Time efficiency      | Note completed in < 3 minutes  |
      | Compliance check     | All required fields verified   |
      | Goal alignment       | Activities linked to objectives |
      | Data integration     | Quantitative measures included  |
      | Professional format  | Insurance-acceptable language   |

  @goal-bank @insurance-compliance @medical-necessity @workflow @not-implemented
  Scenario: Create insurance-compliant goals using comprehensive goal bank
    Given I need to write therapy goals for evaluation report
    And the patient has Medicare coverage requiring specific language
    When I access the comprehensive goal bank system
    And I search for "balance and mobility goals"
    Then I should find goals organized by:
      | Organization Method  | Categories Available            |
      | Insurance type       | Medicare, Medicaid, Commercial  |
      | Skill domain         | Mobility, ADL, Cognitive, Motor |
      | Setting              | Outpatient, Home health, School |
      | Age group            | Pediatric, Adult, Geriatric    |
    And each goal should include required components:
      | Goal Component       | Medicare Example                |
      | Condition statement  | "With minimal assistance and visual cues..." |
      | Behavior description | "Patient will maintain static balance..." |
      | Measurable criteria  | "for 30 seconds without loss of balance" |
      | Timeframe           | "within 4 weeks of treatment initiation" |
      | Medical necessity   | "to safely perform activities of daily living" |
    When I select and customize a goal
    Then the system should provide:
      | Customization Feature| Functionality                   |
      | Component editing    | Modify each goal element        |
      | Insurance validation | Check language compliance       |
      | CPT code linking     | Suggest appropriate billing codes |
      | Baseline integration | Connect to assessment data      |
    And goal validation should verify:
      | Validation Check     | Requirement                     |
      | SMART criteria       | Specific, Measurable, Achievable |
      | Medical necessity    | Clear functional relevance      |
      | Insurance language   | Payer-specific terminology      |
      | Timeframe feasibility| Realistic for condition         |
    When goals are approved
    Then the system should:
      | Output Feature       | Delivery                        |
      | Copy to evaluation   | Insert into report template     |
      | Progress tracking    | Set up measurement schedule     |
      | Goal library update  | Save successful customizations  |

  @progress-reports @data-visualization @comprehensive-reporting @workflow @not-implemented
  Scenario: Generate comprehensive progress report with automated data visualization
    Given student "Liam Johnson" has 12 weeks of therapy data
    And I need to create quarterly progress report
    When I initiate automated progress report generation
    And I specify report parameters:
      | Parameter           | Setting                         |
      | Time period         | January 1 - March 31, 2024     |
      | Audience            | Parents and IEP team           |
      | Detail level        | Comprehensive with graphs       |
      | Format              | Professional PDF               |
    Then the system should compile comprehensive data:
      | Data Category       | Information Included            |
      | Attendance          | 22 sessions attended, 2 missed  |
      | Goal progress       | Percentage completion per goal  |
      | Skill development   | Trend analysis with graphs      |
      | Resource effectiveness| Most beneficial materials used |
      | Engagement patterns | Motivation and participation data |
    And data visualization should include:
      | Graph Type          | Data Displayed                  |
      | Line graphs         | Progress trends over time       |
      | Bar charts          | Goal achievement comparison     |
      | Heat maps           | Skill development patterns      |
      | Pie charts          | Time allocation by activity type|
    When report generation is complete
    Then the report should contain:
      | Report Section      | Content                         |
      | Executive summary   | Overall progress statement      |
      | Goal-by-goal analysis| Detailed progress per objective |
      | Visual data         | Charts and graphs embedded      |
      | Recommendations     | Data-driven next steps          |
      | Parent-friendly version| Simplified language summary   |
    And report customization should allow:
      | Customization Type  | Options Available               |
      | Format selection    | PDF, Word, PowerPoint          |
      | Audience adaptation | Technical vs. family language   |
      | Detail level        | Summary vs. comprehensive       |
      | Branding           | Organization logo and colors   |

  @soap-notes @specialty-templates @clinical-documentation @workflow @not-implemented
  Scenario: Create SOAP notes using therapy-specific templates with clinical prompts
    Given I need to document a complex pediatric OT session
    And I require detailed SOAP format documentation
    When I select "Pediatric OT SOAP Template" from documentation tools
    Then the template should provide specialty-specific prompts:
      | SOAP Section    | Pediatric OT Prompts            |
      | Subjective      | Parent report of home function, child's mood |
      | Objective       | Standardized test scores, clinical observations |
      | Assessment      | Progress toward goals, clinical reasoning |
      | Plan            | Next session focus, home program updates |
    And each section should offer:
      | Template Feature    | Functionality                   |
      | Quick phrase library| Common OT observations dropdown |
      | Goal auto-linking   | Connect activities to IEP goals |
      | CPT code helper     | Suggest billing codes based on activities |
      | Time validation     | Ensure documented time matches billing |
    When I complete each SOAP section
    Then the system should provide:
      | Quality Assurance   | Check Performed                 |
      | Completeness check  | Flag missing required elements  |
      | Consistency validation| Align objective data with assessment |
      | Billing compliance  | Verify time units and procedures |
      | Medical necessity   | Ensure therapeutic justification |
    And documentation workflow should include:
      | Workflow Step       | System Support                  |
      | Draft creation      | Auto-save every 30 seconds     |
      | Peer review         | Share for clinical feedback     |
      | Supervisor approval | Electronic signature workflow   |
      | Final documentation | Lock note after signature       |
    When SOAP note is finalized
    Then the system should:
      | Final Step          | Action                          |
      | Compliance archive  | Store per retention policy      |
      | Billing integration | Send to revenue cycle if applicable |
      | Progress tracking   | Update student goal data        |
      | Template refinement | Learn from successful notes     |

  @insurance-documentation @medical-necessity @workflow @not-implemented
  Scenario: Ensure medical necessity documentation meets insurance requirements
    Given I am documenting therapy for insurance reimbursement
    And patient has complex medical history
    When I create documentation for continued therapy authorization
    Then medical necessity justification should include:
      | Justification Element| Required Content               |
      | Functional limitations| Specific ADL/work impairments |
      | Objective measures   | Standardized test scores       |
      | Progress indicators  | Measurable improvement data    |
      | Skilled intervention | Why therapy expertise needed   |
      | Goal relevance       | How goals address limitations  |
    And documentation should demonstrate:
      | Demonstration Requirement| Evidence Type                |
      | Skilled care necessity   | Complex intervention rationale |
      | Measurable progress      | Objective data trends         |
      | Functional relevance     | Real-world application        |
      | Safety considerations    | Risk factors if untreated     |
    When I submit for insurance review
    Then the system should:
      | Review Support      | Feature                         |
      | Compliance checker  | Flag missing required elements  |
      | Language optimizer  | Suggest insurance-preferred terms |
      | Supporting data     | Include relevant test scores    |
      | Denial prevention   | Highlight strong justifications |
    And documentation package should include:
      | Package Component   | Content                         |
      | Primary note        | Detailed skilled intervention   |
      | Supporting data     | Assessment scores and graphs    |
      | Progress summary    | Trend analysis over time        |
      | Functional goals    | ADL-focused objectives          |

  @time-efficiency @documentation-productivity @workflow @not-implemented
  Scenario: Track and optimize documentation efficiency across caseload
    Given I have a full caseload requiring daily documentation
    And I need to maximize patient contact time
    When I use comprehensive documentation tracking
    Then the system should monitor:
      | Efficiency Metric   | Measurement                     |
      | Time per note       | Average minutes for completion  |
      | Template usage      | Most efficient formats         |
      | Compliance rate     | First-time approval percentage  |
      | Productivity trends | Documentation time over months  |
    And efficiency optimization should provide:
      | Optimization Feature| Benefit                         |
      | Template learning   | Faster note completion         |
      | Quick phrase expansion| Reduce typing time            |
      | Voice-to-text       | Hands-free documentation       |
      | Bulk operations     | Sign multiple notes at once    |
    When I review productivity data
    Then insights should include:
      | Productivity Insight| Analysis                        |
      | Peak efficiency times| When documentation is fastest   |
      | Template effectiveness| Which formats save most time   |
      | Error patterns      | Common compliance issues        |
      | Improvement opportunities| Suggested workflow changes   |
    And system recommendations should suggest:
      | Recommendation Type | Specific Suggestion             |
      | Workflow optimization| Schedule documentation blocks   |
      | Template refinement | Customize for frequent cases    |
      | Training needs      | Areas requiring skill development |
      | Technology adoption | New tools to improve efficiency |