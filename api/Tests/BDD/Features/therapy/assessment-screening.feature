Feature: Assessment and Screening Tools API Endpoints (FR-015)
  As a therapy professional
  I want to use standardized assessment tools
  So that I can evaluate students accurately and track progress

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # GET /api/assessments/tools
  @endpoint @assessments @catalog @not-implemented
  Scenario: List available assessment tools
    When I send a GET request to "/api/assessments/tools?discipline=OT&ageRange=3-5"
    Then the response status should be 200
    And the therapy response should contain array of:
      | field           | type    |
      | assessmentId    | string  |
      | name            | string  |
      | abbreviation    | string  |
      | publisher       | string  |
      | ageRange        | object  |
      | domains         | array   |
      | normReferenced  | boolean |
      | adminTime       | number  |
      | qualifications  | array   |

  # GET /api/assessments/tools/{id}
  @endpoint @assessments @tool-details @not-implemented
  Scenario: Get assessment tool details
    Given assessment tool "peabody-2" exists
    When I send a GET request to "/api/assessments/tools/peabody-2"
    Then the response status should be 200
    And the response should contain:
      | field              | type    |
      | assessmentId       | string  |
      | fullName           | string  |
      | description        | string  |
      | subtests           | array   |
      | scoringMethod      | string  |
      | interpretiveRanges | object  |
      | trainingRequired   | boolean |
      | digitalVersion     | boolean |
      | priceInfo          | object  |

  # POST /api/assessments/sessions
  @endpoint @assessments @start-session @not-implemented
  Scenario: Start assessment session
    When I send a POST request to "/api/assessments/sessions" with:
      | field         | value                    |
      | assessmentId  | peabody-2                |
      | studentId     | student-123              |
      | testDate      | 2024-01-22               |
      | birthDate     | 2019-03-15               |
      | testForm      | A                        |
      | reason        | initial-evaluation       |
    Then the response status should be 201
    And the response should contain:
      | field          | type   |
      | sessionId      | string |
      | ageCalculation | object |
      | subtestsReady  | array  |
      | basalCeiling   | object |

  # POST /api/assessments/sessions/{sessionId}/items
  @endpoint @assessments @score-items @not-implemented
  Scenario: Record assessment item scores
    Given assessment session "session-456" is active
    When I send a POST request to "/api/assessments/sessions/session-456/items" with:
      | field      | value                              |
      | subtest    | gross-motor                        |
      | items      | [{"item": 1, "score": 2, "notes": "Independent"}] |
      | basal      | true                               |
      | ceiling    | false                              |
    Then the response status should be 200
    And the response should contain:
      | field         | type    |
      | rawScore      | number  |
      | basalMet      | boolean |
      | ceilingMet    | boolean |
      | nextItem      | number  |
      | stopRule      | boolean |

  # POST /api/assessments/sessions/{sessionId}/complete
  @endpoint @assessments @calculate-scores @not-implemented
  Scenario: Complete assessment and calculate scores
    Given assessment session "session-456" has all scores
    When I send a POST request to "/api/assessments/sessions/session-456/complete"
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | standardScores     | object |
      | percentileRanks    | object |
      | ageEquivalents     | object |
      | confidenceIntervals| object |
      | interpretiveSummary| string |
      | reportUrl          | string |

  # GET /api/assessments/quick-screens
  @endpoint @assessments @screening @not-implemented
  Scenario: Get quick screening tools
    When I send a GET request to "/api/assessments/quick-screens?area=articulation"
    Then the response status should be 200
    And the therapy response should contain array of:
      | field         | type   |
      | screenId      | string |
      | name          | string |
      | duration      | number |
      | ageRange      | object |
      | cutoffScores  | object |
      | followUpRecs  | array  |

  # POST /api/assessments/quick-screens/{id}/administer
  @endpoint @assessments @quick-screen @not-implemented
  Scenario: Administer quick screener
    When I send a POST request to "/api/assessments/quick-screens/artic-screen/administer" with:
      | field      | value                         |
      | studentId  | student-123                   |
      | responses  | {"p": "correct", "b": "substitution"} |
      | duration   | 5                             |
      | notes      | Cooperative, good attention   |
    Then the response status should be 200
    And the response should contain:
      | field           | type    |
      | passFailStatus  | string  |
      | flaggedAreas    | array   |
      | recommendations | array   |
      | referralNeeded  | boolean |

  # GET /api/assessments/students/{studentId}/history
  @endpoint @assessments @history @not-implemented
  Scenario: Get student assessment history
    Given student "student-123" has assessment history
    When I send a GET request to "/api/assessments/students/student-123/history"
    Then the response status should be 200
    And the therapy response should contain array of:
      | field          | type   |
      | assessmentName | string |
      | testDate       | string |
      | scores         | object |
      | evaluator      | string |
      | reportUrl      | string |
      | nextDueDate    | string |

  # POST /api/assessments/compare
  @endpoint @assessments @comparison @not-implemented
  Scenario: Compare assessment results over time
    When I send a POST request to "/api/assessments/compare" with:
      | field         | value                         |
      | studentId     | student-123                   |
      | assessmentIds | ["session-456", "session-789"]|
      | domains       | ["gross-motor", "fine-motor"] |
    Then the response status should be 200
    And the response should contain:
      | field          | type   |
      | progressCharts | array  |
      | changeScores   | object |
      | significance   | object |
      | interpretation | string |

  # GET /api/assessments/norms/{assessmentId}
  @endpoint @assessments @normative-data @not-implemented
  Scenario: Get normative data for assessment
    When I send a GET request to "/api/assessments/norms/peabody-2?age=48&gender=M"
    Then the response status should be 200
    And the response should contain:
      | field           | type   |
      | normTables      | object |
      | meanScores      | object |
      | standardDev     | object |
      | percentileBands | array  |

  # FR-015 Missing Critical Assessment and Screening Business Workflow Scenarios
  @quick-screening @articulation-screener @triage-assessment @workflow @not-implemented
  Scenario: Conduct comprehensive 5-minute articulation screener for initial triage
    Given I need to screen student "Kevin Martinez" for articulation concerns
    And I have limited time during walk-in screening day
    When I select the "Quick Articulation Screener" from assessment tools
    And I begin the systematic screening protocol:
      | Sound Category   | Test Words           | Kevin's Productions   | Scoring        |
      | Initial /p/      | pig, pencil         | [pig], [pensil]       | 1 correct, 1 error |
      | Final /p/        | cup, jump           | [kʌp], [dʌmp]         | 2 correct      |
      | Initial /b/      | ball, book          | [bɔl], [bʊk]          | 2 correct      |
      | Initial /m/      | mouse, milk         | [maʊs], [mɪlk]        | 2 correct      |
      | Initial /t/      | table, top          | [teɪbl], [tɑp]        | 2 correct      |
      | Final /t/        | cat, bat            | [kæ], [bæ]            | 2 errors (omissions)|
      | Initial /d/      | dog, door           | [dɔg], [dor]          | 2 correct      |
      | Final /d/        | bed, road           | [bɛ], [ro]            | 2 errors (omissions)|
    Then I should be able to rapidly score each production:
      | Scoring Method   | Input Options                   | Speed Requirement  |
      | Correct          | Tap checkmark or press '1'      | < 2 seconds       |
      | Substitution     | Tap 'S' + type replacement     | < 4 seconds       |
      | Omission         | Tap 'O' or press '3'           | < 2 seconds       |
      | Distortion       | Tap 'D' or press '2'           | < 2 seconds       |
    And the screener should automatically calculate results:
      | Results Category | Calculation                     | Kevin's Results    |
      | Overall accuracy | Correct sounds / Total sounds   | 68% (17/25)       |
      | Error patterns   | Most frequent error types       | Final consonant deletion |
      | Age comparison   | Performance vs. age expectations| Below age level    |
      | Referral decision| Based on cutoff scores          | Recommend full eval|
    When screening is complete in under 5 minutes
    Then comprehensive results should include:
      | Result Component | Details                         |
      | Summary score    | 68% accuracy (below criterion)  |
      | Error analysis   | Consistent final consonant deletion |
      | Recommendations  | Full articulation assessment needed |
      | Parent handout   | "What is an articulation evaluation?" |
      | Next steps       | Schedule comprehensive evaluation |

  @norm-referenced-assessment @peabody-motor @comprehensive-evaluation @workflow @not-implemented
  Scenario: Administer comprehensive norm-referenced motor assessment with accurate scoring
    Given I am conducting a formal evaluation for student "Isabella Thompson"
    And I am using the "Peabody Developmental Motor Scales (PDMS-2)"
    When I begin the systematic assessment protocol
    And I prepare for administration:
      | Setup Element    | Requirement                     |
      | Age calculation  | 4 years, 7 months exactly      |
      | Subtest selection| Reflexes, Stationary, Locomotion, Object Manipulation |
      | Materials check  | All required items present      |
      | Environment      | Quiet space, adequate lighting  |
      | Timing tools     | Stopwatch for timed items       |
    Then I should systematically administer each subtest:
      | Subtest          | Starting Point   | Basal/Ceiling Rules   |
      | Reflexes         | Age-appropriate  | 3 consecutive pass/fail|
      | Stationary       | Item 1           | 3 consecutive pass/fail|
      | Locomotion       | Age-appropriate  | 3 consecutive pass/fail|
      | Object Manip     | Age-appropriate  | 3 consecutive pass/fail|
    When administering individual items
    Then I should score each item systematically:
      | Item Type        | Scoring Criteria                | Examples           |
      | Balance beam     | Steps without falling off       | 3/5 steps = partial|
      | Ball catch       | Secure grasp with two hands     | 2/3 trials = pass |
      | Stair climbing   | Alternating feet pattern        | Observed = pass    |
      | Block stacking   | 8 blocks without falling        | 6 blocks = fail   |
    And data collection should include:
      | Data Element     | Recording Method                |
      | Raw scores       | Sum correct per subtest         |
      | Observations     | Qualitative notes on performance|
      | Behavior notes   | Attention, cooperation, fatigue |
      | Environmental factors| Distractions, accommodations |
    When all subtests are completed
    Then scoring should generate:
      | Score Type       | Calculation                     | Isabella's Results |
      | Raw scores       | Sum of item scores              | R: 12, S: 18, L: 15, OM: 22 |
      | Standard scores  | Based on age norms              | 85, 92, 88, 95    |
      | Percentile ranks | Compared to age peers           | 16th, 30th, 21st, 37th |
      | Motor quotients  | Composite scores                | GMQ: 87, FMQ: 93, TMQ: 90 |
      | Confidence intervals| 90% confidence bands         | TMQ: 85-95         |
    And interpretation should provide:
      | Interpretation Element| Content                       |
      | Performance level    | Average to low average range   |
      | Relative strengths   | Fine motor > gross motor      |
      | Areas of concern     | Balance and bilateral coordination |
      | Recommendations      | OT services, sensory assessment |

  @curriculum-based-measurement @progress-monitoring @reading-fluency @workflow @not-implemented
  Scenario: Implement systematic curriculum-based measurement for reading fluency monitoring
    Given I monitor weekly reading progress for student "Marcus Johnson"
    And Marcus is in 3rd grade receiving reading intervention
    When I conduct systematic CBM reading probe
    And I prepare grade-level passage:
      | Passage Element  | Specification                   |
      | Grade level      | 3rd grade benchmark passage     |
      | Word count       | 200+ words                      |
      | Complexity       | Appropriate for progress monitoring |
      | Font size        | 12-point, clear formatting      |
      | Timing           | 1-minute probe                  |
    Then I should implement standardized administration:
      | Administration Element| Procedure                     |
      | Directions           | "Read as many words as you can"|
      | Timing               | Start timer when student begins|
      | Error marking        | Mark errors without disrupting |
      | Encouragement        | Neutral, supportive prompts    |
    When Marcus reads the passage
    Then I should track performance systematically:
      | Performance Metric   | Measurement                     | Marcus's Performance |
      | Words read           | Total words attempted           | 105 words           |
      | Errors made          | Substitutions, omissions, additions| 8 errors         |
      | Words correct/minute | (Total words - errors) per minute | 97 WCPM          |
      | Accuracy percentage  | (Correct words / total words) × 100| 92.4%           |
      | Error analysis       | Types of errors made            | Mostly multisyllabic|
    And progress tracking should show:
      | Progress Element     | Data Visualization              |
      | Weekly performance   | Line graph with data points     |
      | Trend analysis       | Slope of improvement            |
      | Goal comparison      | Target vs. actual performance   |
      | Benchmark status     | Above/below grade level         |
    When analyzing 8 weeks of data
    Then decision-making should consider:
      | Decision Factor      | Analysis                        |
      | Rate of improvement  | 2.1 words per week gain        |
      | Goal attainment      | Will meet annual goal if maintained|
      | Instructional changes| Continue current intervention   |
      | Frequency adjustments| Maintain 3x weekly sessions     |

  @developmental-checklist @early-childhood @milestone-assessment @workflow @not-implemented
  Scenario: Complete comprehensive developmental checklist assessment for preschooler
    Given I am evaluating 3-year-old "Zara Patel" for early intervention services
    And I am using the "Ages and Stages Developmental Checklist"
    When I conduct systematic developmental assessment
    Then I should evaluate all developmental domains:
      | Domain           | Sample Skills                   | Zara's Performance |
      | Gross Motor      | Runs smoothly, jumps with feet together | Emerging      |
      | Fine Motor       | Copies circle, strings large beads | Achieved        |
      | Communication    | Uses 4-5 word sentences        | Not yet achieved|
      | Problem Solving  | Completes 3-4 piece puzzles    | Achieved        |
      | Personal-Social  | Plays cooperatively with others| Emerging        |
    And scoring should reflect developmental progression:
      | Scoring Level    | Criteria                        | Implementation  |
      | Not yet          | Skill not demonstrated          | 0 points       |
      | Emerging         | Sometimes demonstrates skill    | 5 points       |
      | Achieved         | Consistently demonstrates skill | 10 points      |
      | Advanced         | Exceeds age expectations        | 15 points      |
    When completing domain-by-domain assessment
    Then I should document:
      | Documentation Element| Content                        |
      | Skill observations   | Specific behaviors witnessed   |
      | Context notes        | Where/when skills demonstrated |
      | Prompting needed     | Level of support required      |
      | Quality indicators   | How well skill performed       |
    And checklist analysis should generate:
      | Analysis Component   | Output                         | Zara's Results |
      | Domain percentages   | Percentage skills mastered     | GM: 60%, FM: 85%, C: 40% |
      | Overall development  | Composite developmental level   | Mild delays    |
      | Priority areas       | Domains needing intervention   | Communication, Gross Motor |
      | Strengths            | Areas of typical development   | Fine Motor, Problem Solving |
    When assessment is complete
    Then recommendations should include:
      | Recommendation Type  | Specific Actions               |
      | Service eligibility  | Qualifies for speech therapy   |
      | Goal development     | Focus on language expansion    |
      | Family education     | Home activities for communication |
      | Re-evaluation        | 6-month progress review        |

  @functional-assessment @school-based @participation-assessment @workflow @not-implemented
  Scenario: Conduct comprehensive school-based functional assessment
    Given I am evaluating student "David Chen" for school-based therapy services
    And I need to assess his participation in educational activities
    When I implement functional assessment across school environments
    Then I should evaluate participation in:
      | School Environment   | Activities Assessed             | Participation Level |
      | General classroom    | Desk work, group activities     | Moderate support needed |
      | Cafeteria           | Eating, social interaction      | Minimal support    |
      | Playground          | Play skills, peer interaction   | Substantial support|
      | Library             | Book selection, computer use    | Independent        |
      | Physical education  | Sports, fitness activities      | Intensive support  |
    And assessment should consider:
      | Assessment Factor    | Evaluation Method               |
      | Task demands         | Cognitive, physical, social requirements |
      | Environmental supports| Available accommodations      |
      | Peer interactions    | Social dynamics and relationships |
      | Teacher expectations | Academic and behavioral standards |
    When documenting functional performance
    Then I should rate:
      | Performance Area     | Rating Scale                    | David's Ratings |
      | Task initiation      | 1-5 (independent to dependent)  | 3 (moderate support) |
      | Task completion      | 1-5 (consistent to inconsistent)| 2 (substantial support) |
      | Social participation | 1-5 (engaged to withdrawn)      | 2 (limited engagement) |
      | Problem-solving      | 1-5 (flexible to rigid)         | 3 (some flexibility) |
    And functional analysis should identify:
      | Analysis Component   | Findings                        |
      | Performance patterns | Stronger in structured tasks    |
      | Support needs        | Benefits from visual schedules  |
      | Environmental factors| Large groups challenging        |
      | Skill priorities     | Social communication skills     |
    When assessment is complete
    Then functional recommendations should include:
      | Recommendation Category| Specific Strategies            |
      | Environmental modifications| Reduce noise, provide visual supports |
      | Task accommodations     | Break tasks into smaller steps |
      | Social supports         | Facilitate peer interactions  |
      | Service provisions      | OT 2x/week, SLP 1x/week      |

  @behavioral-assessment @functional-behavior-assessment @comprehensive-fba @workflow @not-implemented
  Scenario: Conduct systematic functional behavior assessment for intervention planning
    Given student "Jordan Smith" exhibits challenging behaviors in school
    And I need to determine function of behaviors for intervention planning
    When I implement comprehensive FBA protocol
    Then I should conduct systematic data collection across:
      | Data Collection Method| Purpose                        | Duration/Frequency |
      | Direct observation    | Document ABC patterns          | 5 days, 2 hours each |
      | Interview data        | Gather stakeholder perspectives| Teachers, parents, student |
      | Record review         | Historical behavior patterns   | Previous 2 years    |
      | Environmental analysis| Setting factors                | All school environments |
    And ABC data collection should capture:
      | Data Element         | Recording Details               |
      | Antecedent events    | What happened immediately before|
      | Behavior description | Objective, observable behaviors |
      | Consequence events   | What happened immediately after |
      | Duration/intensity   | How long, how severe           |
      | Setting factors      | Where, when, who present       |
    When analyzing collected data
    Then pattern analysis should identify:
      | Pattern Analysis     | Findings                        | Jordan's Patterns |
      | Temporal patterns    | Time of day behaviors occur     | Peak: 10-11 AM   |
      | Antecedent patterns  | Common triggers                 | Academic demands  |
      | Consequence patterns | How behaviors are handled       | Escape provided 70% |
      | Setting patterns     | Where behaviors occur most      | Math class primarily |
    And hypothesis development should determine:
      | Function Category    | Evidence                        | Hypothesis Strength |
      | Escape/avoidance     | High rates during demands       | Strong evidence    |
      | Attention-seeking    | Moderate rates when ignored     | Weak evidence      |
      | Tangible access      | Low rates when items restricted | No evidence        |
      | Sensory/automatic    | No clear environmental patterns | No evidence        |
    When FBA is complete
    Then intervention planning should include:
      | Intervention Component| Function-Based Strategy         |
      | Antecedent strategies | Modify task difficulty level    |
      | Replacement behaviors | Teach appropriate help-seeking  |
      | Consequence strategies| Minimize escape, reinforce alternatives |
      | Environmental changes | Provide scheduled breaks        |
      | Data collection plan  | Monitor intervention effectiveness |