Feature: Outcome Measurement API Endpoints (FR-032)
  As a therapy professional
  I want to use standardized outcome measures
  So that I can demonstrate treatment effectiveness for insurance and quality improvement

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # GET /api/outcome-measures/available
  @endpoint @outcomes @catalog @not-implemented
  Scenario: List available outcome measures
    When I send a GET request to "/api/outcome-measures/available?discipline=PT&setting=outpatient"
    Then the response status should be 200
    And the therapy response should contain array of:
      | field             | type    |
      | measureId         | string  |
      | name              | string  |
      | abbreviation      | string  |
      | domains           | array   |
      | populationAge     | object  |
      | adminTime         | number  |
      | insuranceAccepted | array   |
      | licenseRequired   | boolean |

  # POST /api/outcome-measures/{measureId}/administer
  @endpoint @outcomes @administration @not-implemented
  Scenario: Administer FOTO assessment
    When I send a POST request to "/api/outcome-measures/foto/administer" with:
      | field         | value                    |
      | patientId     | patient-123              |
      | bodyPart      | shoulder                 |
      | visitType     | initial-evaluation       |
      | diagnosis     | M75.30                   |
      | surgeryDate   | null                     |
    Then the response status should be 201
    And the response should contain:
      | field          | type   |
      | assessmentId   | string |
      | questions      | array  |
      | adaptive       | boolean |

  # POST /api/outcome-measures/{assessmentId}/responses
  @endpoint @outcomes @data-collection @not-implemented
  Scenario: Submit outcome measure responses
    Given assessment "assess-456" is in progress
    When I send a POST request to "/api/outcome-measures/assess-456/responses" with:
      | field      | value                                    |
      | responses  | [{"questionId": "q1", "value": 3}]      |
      | completed  | true                                     |
      | timeSpent  | 480                                      |
    Then the response status should be 200
    And the response should contain:
      | field             | type   |
      | functionalScore   | number |
      | predictedVisits   | number |
      | riskAdjustment    | object |
      | comparisonData    | object |

  # GET /api/outcome-measures/benchmarks
  @endpoint @outcomes @benchmarking @not-implemented
  Scenario: Get outcome benchmarks
    When I send a GET request to "/api/outcome-measures/benchmarks?diagnosis=M75.30&measure=foto"
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | nationalAverage    | number |
      | regionalAverage    | number |
      | facilityAverage    | number |
      | improvementTarget  | number |
      | visitBenchmark     | number |

  # GET /api/outcome-measures/patients/{patientId}/progress
  @endpoint @outcomes @tracking @not-implemented
  Scenario: Track patient outcome progress
    Given patient "patient-123" has multiple assessments
    When I send a GET request to "/api/outcome-measures/patients/patient-123/progress"
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | assessments        | array  |
      | changeScores       | object |
      | trajectoryAnalysis | object |
      | predictedDischarge | string |
      | goalsMetPercentage | number |

  # POST /api/outcome-measures/reports/generate
  @endpoint @outcomes @reporting @not-implemented
  Scenario: Generate outcome report for insurance
    When I send a POST request to "/api/outcome-measures/reports/generate" with:
      | field         | value                    |
      | patientId     | patient-123              |
      | measureIds    | ["foto", "nprs"]         |
      | dateRange     | {"start": "2024-01-01", "end": "2024-03-31"} |
      | reportType    | insurance-summary        |
      | payerId       | bcbs-123                 |
    Then the response status should be 200
    And report should include required metrics
    And be formatted for payer requirements

  # FR-032 Missing Critical Outcome Measurement Business Workflow Scenarios
  @foto-integration @functional-outcomes @insurance-reporting @workflow @not-implemented
  Scenario: Complete FOTO assessment administration and outcome tracking for insurance requirements
    Given I am treating patient "Robert Davis" for shoulder impingement
    And insurance requires FOTO outcome reporting for continued authorization
    When I initiate comprehensive FOTO assessment at intake
    And I gather required assessment information:
      | Assessment Element   | Details                         |
      | Primary diagnosis    | M75.30 - Calcific tendinitis  |
      | Body region          | Shoulder                       |
      | Surgery history      | None                           |
      | Visit type           | Initial evaluation             |
      | Risk factors         | Age 52, diabetic               |
    Then the FOTO system should present adaptive questions:
      | Question Category    | Example Questions               |
      | Pain level           | 0-10 scale with body diagram    |
      | Functional activities| Reaching overhead, lifting      |
      | Work impact          | Job-specific task limitations   |
      | Quality of life      | Sleep, recreation restrictions  |
    When Robert completes the initial assessment
    Then the system should calculate comprehensive scores:
      | Score Type           | Robert's Results | National Average |
      | Functional Status    | 45               | 55 for similar dx|
      | Pain level           | 7/10             | 6/10 for similar |
      | Predicted visits     | 12 sessions      | 8-14 range       |
      | Risk adjustment      | Applied for diabetes| Standard modifier|
    And outcome tracking should provide:
      | Tracking Element     | Implementation                  |
      | Baseline establishment| Initial scores documented      |
      | Progress benchmarks  | Expected improvement milestones|
      | Discharge prediction | Data-driven visit estimate     |
      | Insurance reporting  | Real-time outcome data         |
    When I conduct reassessment after 6 sessions
    Then progress analysis should show:
      | Progress Metric      | Results                         |
      | Functional improvement| 55 (10-point gain)             |
      | Pain reduction       | 4/10 (3-point improvement)     |
      | Goal achievement     | 60% of functional targets met   |
      | Discharge readiness  | 6 more sessions predicted       |
    And insurance reporting should demonstrate:
      | Insurance Requirement| Evidence Provided               |
      | Medical necessity    | Functional limitations documented|
      | Measurable progress  | 22% functional score improvement|
      | Skilled intervention | Complex shoulder rehabilitation |
      | Outcome trajectory   | Meeting expected benchmarks     |

  @copm-administration @client-centered-outcomes @occupational-performance @workflow @not-implemented
  Scenario: Administer Canadian Occupational Performance Measure for client-centered evaluation
    Given I am evaluating "Linda Thompson" who has multiple sclerosis
    And I need client-centered outcome measurement
    When I begin comprehensive COPM interview
    And I guide Linda through systematic identification process:
      | COPM Step            | Process                         |
      | Problem identification| Daily activities causing difficulty |
      | Importance rating    | 1-10 scale for each identified issue |
      | Priority selection   | Choose top 5 most important problems |
      | Performance rating   | Current ability level 1-10      |
      | Satisfaction rating  | How satisfied with performance 1-10|
    Then Linda identifies and prioritizes her concerns:
      | Occupational Issue   | Importance | Performance | Satisfaction |
      | Meal preparation     | 9          | 3           | 2            |
      | Household cleaning   | 8          | 4           | 3            |
      | Driving safety       | 10         | 2           | 1            |
      | Work computer tasks  | 9          | 5           | 4            |
      | Leisure gardening    | 7          | 6           | 5            |
    And COPM scoring should calculate:
      | COMP Score Type      | Calculation                     | Linda's Scores |
      | Total Performance    | Average of 5 performance ratings| 4.0           |
      | Total Satisfaction   | Average of 5 satisfaction ratings| 3.0          |
      | Weighted scores      | Importance × performance ratings| Used for goals |
    When developing intervention priorities
    Then COMP results should guide:
      | Intervention Planning| Priority Focus                  |
      | Goal development     | Driving safety (highest importance)|
      | Service planning     | Meal prep and work tasks        |
      | Outcome tracking     | Performance and satisfaction    |
      | Client engagement    | Client-chosen meaningful activities|
    And reassessment protocol should include:
      | Reassessment Element | Implementation                  |
      | Same problem areas   | Re-rate performance and satisfaction|
      | Change calculation   | Difference scores from baseline |
      | Clinical significance| Minimum 2-point change considered meaningful|
      | Client priorities    | Opportunity to adjust focus areas|

  @insurance-outcome-reporting @medicare-compliance @quality-reporting @workflow @not-implemented
  Scenario: Generate comprehensive outcome reports for Medicare and insurance compliance
    Given I treat multiple Medicare patients requiring outcome reporting
    And Medicare requires functional outcome documentation
    When I access the comprehensive outcome dashboard
    Then I should see facility-level metrics:
      | Outcome Metric       | Current Period | Previous Period | Benchmark  |
      | Average improvement  | 18.5 points    | 16.2 points    | 15 points  |
      | Goal achievement rate| 78%            | 72%            | 70%        |
      | Successful discharge | 82%            | 79%            | 75%        |
      | Readmission rate     | 8%             | 11%            | 12%        |
      | Patient satisfaction | 4.6/5          | 4.4/5          | 4.0/5      |
    When I analyze outcomes by diagnosis category
    Then detailed breakdowns should show:
      | Diagnosis Category   | Avg Visits | Improvement | Meeting Threshold |
      | CVA rehabilitation   | 24.5       | 22 points   | Yes (>20 required)|
      | Total knee replacement| 15.2      | 28 points   | Yes (>25 required)|
      | Low back pain        | 10.8       | 15 points   | No (<18 required) |
      | Shoulder impingement | 12.3       | 19 points   | Yes (>15 required)|
    And comparative analysis should include:
      | Comparison Type      | Data Source                     |
      | National benchmarks  | CMS quality reporting database  |
      | Regional performance | State therapy association data |
      | Facility trends      | Historical performance tracking |
      | Payer-specific       | Individual insurance requirements|
    When generating Medicare reports
    Then compliance reporting should include:
      | Report Section       | Required Content                |
      | Quality measures     | PQRS compliance indicators     |
      | Functional outcomes  | Standardized assessment results |
      | Value-based metrics  | Cost per quality point achieved |
      | Patient safety       | Adverse events and prevention  |
    And automated reporting should provide:
      | Automation Feature   | Benefit                         |
      | Scheduled generation | Monthly/quarterly automatic reports|
      | Compliance alerts    | Flag potential reporting issues |
      | Trend analysis       | Identify performance patterns   |
      | Benchmarking         | Compare to relevant standards   |

  @pediatric-outcome-measurement @school-based-outcomes @educational-relevance @workflow @not-implemented
  Scenario: Implement school-based therapy outcome measurement for educational relevance
    Given I work in school setting providing therapy services
    And I need educationally relevant outcome measures
    When I assess student "Carlos Martinez" using School Function Assessment
    Then I should evaluate participation across educational environments:
      | School Environment   | Participation Elements          | Carlos's Performance |
      | General classroom    | Desk work, group activities     | Partial participation|
      | Cafeteria           | Eating, social interaction      | Full participation   |
      | Playground          | Play skills, peer interaction   | Limited participation|
      | Library             | Book selection, computer use    | Full participation   |
      | PE/Movement         | Physical activities             | Minimal participation|
    And assessment should include multiple dimensions:
      | Assessment Dimension | Evaluation Criteria             |
      | Participation level  | Full, partial, minimal, none    |
      | Task supports needed | Adaptations required            |
      | Activity performance | Consistency and quality         |
      | Environmental factors| Settings that support success   |
    When completing comprehensive SFA
    Then scoring should generate:
      | SFA Score Category   | Carlos's Results                | Educational Impact   |
      | Participation score  | 72%                            | Moderate limitations |
      | Task supports score  | Level 3 adaptations needed     | Substantial support  |
      | Activity performance | 65% consistency                | Inconsistent         |
    And educational recommendations should address:
      | Recommendation Type  | Specific Interventions          |
      | IEP goal development | Measurable educational objectives|
      | Accommodations       | Specific classroom supports     |
      | Service minutes      | Educationally necessary services|
      | Environment modifications| Successful participation strategies|
    When tracking progress over school year
    Then educational outcome measurement should show:
      | Outcome Indicator    | Progress Tracking               |
      | Curriculum access    | Increased participation in academics|
      | Social inclusion     | Improved peer interactions      |
      | Independence         | Reduced need for adult support  |
      | Academic achievement | Grade-level performance gains   |