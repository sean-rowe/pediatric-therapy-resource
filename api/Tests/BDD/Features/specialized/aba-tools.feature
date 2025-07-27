Feature: ABA Tools and Tracking API Endpoints (FR-034)
  As an ABA therapist or BCBA
  I want comprehensive ABA tools and data collection
  So that I can implement evidence-based interventions effectively

  Background:
    Given the API is available
    And I am authenticated as "bcba@clinic.com"
    And I have ABA certification

  # POST /api/aba/behavior-plans
  @endpoint @aba @bip @not-implemented
  Scenario: Create behavior intervention plan
    When I send a POST request to "/api/aba/behavior-plans" with:
      | field              | value                                    |
      | studentId          | student-123                              |
      | targetBehaviors    | [{"behavior": "aggression", "definition": "hitting others"}] |
      | functionAssessment | escape                                   |
      | replacementBehaviors| [{"behavior": "request-break", "teaching": "FCT"}] |
      | antecedentStrategies| ["visual-schedule", "choice-making"]    |
      | consequenceStrategies| ["extinction", "reinforcement"]        |
      | crisisPlan         | {"procedure": "safety-protocol"}         |
    Then the response status should be 201
    And the response should contain:
      | field       | type   |
      | planId      | string |
      | status      | string |
      | approvalRequired | boolean |

  # POST /api/aba/abc-data
  @endpoint @aba @data-collection @not-implemented
  Scenario: Record ABC data
    Given I am observing student "student-123"
    When I send a POST request to "/api/aba/abc-data" with:
      | field         | value                           |
      | studentId     | student-123                     |
      | dateTime      | 2024-01-22T10:15:00Z           |
      | antecedent    | Asked to complete math worksheet|
      | behavior      | Threw materials off desk        |
      | consequence   | Worksheet removed, break given  |
      | duration      | 45 seconds                      |
      | intensity     | moderate                        |
      | location      | classroom                       |
      | peoplePresent | ["teacher", "aide", "peers"]   |
    Then the response status should be 201
    And ABC pattern analysis should update
    And function hypothesis should be refined

  # POST /api/aba/token-economy
  @endpoint @aba @reinforcement @not-implemented
  Scenario: Create token economy system
    When I send a POST request to "/api/aba/token-economy" with:
      | field            | value                              |
      | studentId        | student-123                        |
      | tokenType        | star-stickers                      |
      | targetBehaviors  | ["on-task", "hand-raising", "sharing"] |
      | tokenSchedule    | {"type": "FR", "ratio": 3}         |
      | backupReinforcers| [{"item": "iPad-time", "cost": 10}] |
      | visualSupport    | token-board-5x4                    |
    Then the response status should be 201
    And token board template should be generated
    And tracking system should be initialized

  # POST /api/aba/token-economy/{systemId}/award
  @endpoint @aba @token-tracking @not-implemented
  Scenario: Award tokens in system
    Given token system "token-123" exists for student
    When I send a POST request to "/api/aba/token-economy/token-123/award" with:
      | field         | value                    |
      | behavior      | on-task                  |
      | tokensEarned  | 1                        |
      | timestamp     | 2024-01-22T10:20:00Z     |
      | notes         | 5 minutes continuous work|
    Then the response status should be 200
    And token count should update
    And student progress should be visible

  # POST /api/aba/dtt-sessions
  @endpoint @aba @discrete-trial @not-implemented
  Scenario: Create DTT session
    When I send a POST request to "/api/aba/dtt-sessions" with:
      | field         | value                         |
      | studentId     | student-123                   |
      | programs      | ["receptive-id", "matching"]  |
      | sessionLength | 30                            |
      | trialsPerProgram | 10                         |
      | reinforcementSchedule | FR2                   |
      | promptingHierarchy | ["full", "partial", "gestural"] |
    Then the response status should be 201
    And the response should contain:
      | field         | type   |
      | sessionId     | string |
      | programSheets | array  |
      | randomization | array  |

  # POST /api/aba/dtt-sessions/{sessionId}/trials
  @endpoint @aba @trial-data @not-implemented
  Scenario: Record DTT trial data
    Given DTT session "session-456" is active
    When I send a POST request to "/api/aba/dtt-sessions/session-456/trials" with:
      | field       | value                    |
      | programId   | receptive-id             |
      | target      | "Touch nose"             |
      | response    | correct                  |
      | promptLevel | independent              |
      | latency     | 2.5                      |
      | trialNumber | 1                        |
    Then the response status should be 201
    And mastery criteria should be checked
    And next trial should be prepared

  # GET /api/aba/students/{studentId}/progress
  @endpoint @aba @progress @not-implemented
  Scenario: Get comprehensive ABA progress
    Given student "student-123" has ABA programs
    When I send a GET request to "/api/aba/students/student-123/progress"
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | behaviorReduction  | object |
      | skillAcquisition   | object |
      | generalization     | object |
      | maintenanceData    | object |
      | graphLinks         | array  |

  # POST /api/aba/functional-analysis
  @endpoint @aba @fa @not-implemented
  Scenario: Record functional analysis data
    When I send a POST request to "/api/aba/functional-analysis" with:
      | field         | value                    |
      | studentId     | student-123              |
      | condition     | attention                |
      | sessionNumber | 3                        |
      | duration      | 300                      |
      | behaviorCount | 8                        |
      | notes         | High rates when attention removed |
    Then the response status should be 201
    And FA graph should update
    And function hypothesis should strengthen

  # POST /api/aba/preference-assessment
  @endpoint @aba @reinforcer @not-implemented
  Scenario: Conduct preference assessment
    When I send a POST request to "/api/aba/preference-assessment" with:
      | field          | value                           |
      | studentId      | student-123                     |
      | assessmentType | paired-choice                   |
      | items          | ["iPad", "bubbles", "snack", "toy-car"] |
      | results        | [{"pair": ["iPad", "bubbles"], "choice": "iPad"}] |
      | setting        | therapy-room                    |
    Then the response status should be 201
    And preference hierarchy should be calculated
    And reinforcer effectiveness should be tracked

  # GET /api/aba/task-analysis/{skillId}
  @endpoint @aba @task-analysis @not-implemented
  Scenario: Get task analysis steps
    When I send a GET request to "/api/aba/task-analysis/hand-washing"
    Then the response status should be 200
    And the response should contain:
      | field         | type  |
      | skillName     | string|
      | steps         | array |
      | chainingType  | string|
      | visualSupports| array |
      | dataSheet     | string|

  # POST /api/aba/task-analysis/{skillId}/data
  @endpoint @aba @chaining @not-implemented
  Scenario: Record task analysis data
    Given task analysis "hand-washing" exists
    When I send a POST request to "/api/aba/task-analysis/hand-washing/data" with:
      | field      | value                                    |
      | studentId  | student-123                              |
      | date       | 2024-01-22                               |
      | stepData   | [{"step": 1, "performance": "independent"}] |
      | chainingMethod | forward                              |
      | totalTime  | 120                                      |
    Then the response status should be 201
    And independence levels should be calculated
    And next teaching targets should be identified

  # POST /api/aba/behavior-contracts
  @endpoint @aba @contracts @not-implemented
  Scenario: Create behavior contract
    When I send a POST request to "/api/aba/behavior-contracts" with:
      | field           | value                              |
      | studentId       | student-123                        |
      | targetBehaviors | ["complete-homework", "arrive-on-time"] |
      | rewards         | [{"criteria": "5-days", "reward": "extra-recess"}] |
      | duration        | 2-weeks                            |
      | reviewSchedule  | daily                              |
      | signatures      | ["student", "teacher", "parent"]   |
    Then the response status should be 201
    And contract document should be generated
    And tracking sheet should be created

  # GET /api/aba/visual-supports
  @endpoint @aba @visuals @not-implemented
  Scenario: Get visual support library
    When I send a GET request to "/api/aba/visual-supports?type=first-then"
    Then the response status should be 200
    And the response should contain:
      | field          | type  |
      | templates      | array |
      | customizable   | boolean |
      | printFormats   | array |
      | digitalVersions| array |

  # POST /api/aba/scatterplot
  @endpoint @aba @patterns @not-implemented
  Scenario: Record scatterplot data
    When I send a POST request to "/api/aba/scatterplot" with:
      | field      | value                         |
      | studentId  | student-123                   |
      | behavior   | vocal-stereotypy              |
      | date       | 2024-01-22                    |
      | timeSlots  | [{"time": "9:00", "count": 5}] |
      | activities | [{"time": "9:00", "activity": "circle-time"}] |
    Then the response status should be 201
    And pattern analysis should identify high-probability times
    And environmental correlations should be noted

  # POST /api/aba/extinction-burst
  @endpoint @aba @intervention @not-implemented
  Scenario: Track extinction burst data
    When I send a POST request to "/api/aba/extinction-burst" with:
      | field         | value                    |
      | studentId     | student-123              |
      | behaviorPlanId| bip-456                  |
      | date          | 2024-01-22               |
      | baselineRate  | 5                        |
      | currentRate   | 12                       |
      | interventionDay| 3                       |
      | notes         | Expected burst pattern   |
    Then the response status should be 201
    And extinction curve should be plotted
    And team alerts should be sent

  # GET /api/aba/resources/parent-training
  @endpoint @aba @training @not-implemented
  Scenario: Get parent training materials
    When I send a GET request to "/api/aba/resources/parent-training?topic=reinforcement"
    Then the response status should be 200
    And the response should contain:
      | field         | type  |
      | handouts      | array |
      | videos        | array |
      | practiceGuides| array |
      | dataSheets    | array |
      | translations  | array |

  # FR-034 Missing Critical ABA Integration Business Workflow Scenarios
  @abc-data @antecedent-behavior-consequence @systematic-observation @workflow @not-implemented
  Scenario: Comprehensive ABC data collection during systematic observation
    Given I am conducting a functional behavior assessment for student "Tyler"
    And Tyler's target behaviors include hand flapping and vocal scripting
    When I observe Tyler during a structured classroom observation
    And I document systematic ABC data for each occurrence:
      | Time     | Antecedent                    | Behavior                | Consequence             | Duration | Setting      |
      | 9:15 AM  | Math worksheet presented      | Hand flapping (8 sec)   | Teacher waited          | 8 sec    | Classroom    |
      | 9:23 AM  | Peer noise from hallway       | Vocal scripting         | No response given       | 12 sec   | Classroom    |
      | 9:31 AM  | Demand to write name          | Hand flapping           | Task removed            | 5 sec    | Classroom    |
      | 9:47 AM  | Transition announcement       | Vocal scripting         | Redirected to line up   | 15 sec   | Classroom    |
    Then I should capture comprehensive contextual information:
      | Contextual Factor    | Details Recorded                    |
      | People present       | Teacher, aide, 18 peers            |
      | Environmental factors| Fluorescent lighting, morning heat  |
      | Student state        | Arrived late, missed breakfast     |
      | Task difficulty      | Above instructional level           |
      | Time of day         | 9:15-10:00 AM block                |
    And data analysis should identify patterns:
      | Pattern Analysis     | Findings                            |
      | Antecedent patterns  | 75% occur during academic demands   |
      | Temporal patterns    | Peak frequency 9:15-9:30 AM        |
      | Consequence patterns | Escape/avoidance maintains 60% of behaviors |
      | Environmental correlations| High noise increases vocal scripting |
    When I complete the observation session
    Then comprehensive analysis should include:
      | Analysis Component   | Generated Insights                  |
      | Function hypothesis  | Primary: escape/avoidance, Secondary: sensory |
      | Intervention targets | Reduce academic demand level        |
      | Replacement behaviors| Functional communication training   |
      | Environmental modifications| Noise reduction, visual supports |
    And recommendations should guide intervention planning:
      | Recommendation Type  | Specific Strategy                   |
      | Antecedent strategies| Choice-making, visual schedules     |
      | Teaching strategies  | Communication alternatives          |
      | Consequence strategies| Reinforcement for appropriate behavior|
      | Data collection plan | Ongoing monitoring protocol         |

  @token-economy @reinforcement-systems @behavior-modification @workflow @not-implemented
  Scenario: Complete token economy implementation with systematic reinforcement
    Given I am implementing a token economy for student "Sophia"
    And Sophia needs support for on-task behavior and social interactions
    When I design a comprehensive token economy system
    And I configure all system components:
      | Component            | Specification                       |
      | Token type           | Star stickers (visual, tangible)   |
      | Target behaviors     | Following directions, sharing, helping others |
      | Reinforcement schedule| Fixed Ratio 3 (FR3) initially     |
      | Backup reinforcers   | 5 tokens = 5 min iPad time, 10 tokens = snack choice |
      | Visual support       | 20-space token board with interests|
      | Fading plan          | FR3 → FR5 → VR7 → natural reinforcement |
    Then I should create comprehensive visual materials:
      | Visual Material      | Features                            |
      | Token board          | 20 spaces, Sophia's favorite colors |
      | Behavior rule cards  | Picture cards showing target behaviors |
      | Choice menu          | Photos of available backup reinforcers |
      | Progress tracker     | Weekly graph showing tokens earned  |
    And implementation should follow systematic protocol:
      | Implementation Step  | Procedure                           |
      | Behavior definition  | Operationally defined, observable   |
      | Token delivery       | Immediate, with specific praise     |
      | Exchange opportunities| Scheduled and demand-based         |
      | Data collection      | Continuous behavior tracking        |
    When Sophia engages in target behaviors
    Then reinforcement delivery should include:
      | Reinforcement Element| Implementation                     |
      | Token presentation   | Physical token with verbal praise  |
      | Specific feedback    | "Great job following directions!"  |
      | Visual celebration   | Sound effect, board lighting       |
      | Progress tracking    | Real-time counter updates          |
    And token exchange process should provide:
      | Exchange Feature     | Functionality                       |
      | Choice presentation  | Visual menu of available items      |
      | Preference assessment| Track which items chosen most       |
      | Social reinforcement | Celebration of achievement          |
      | System refinement    | Adjust based on preferences        |
    When Sophia demonstrates consistent success
    Then systematic fading should include:
      | Fading Stage         | Schedule Modification               |
      | Stage 1              | FR3 to FR5 (every 5th behavior)    |
      | Stage 2              | FR5 to VR7 (average every 7th)     |
      | Stage 3              | Variable schedule with natural cues |
      | Stage 4              | Natural reinforcement only          |
    And long-term maintenance should ensure:
      | Maintenance Element  | Strategy                            |
      | Behavior generalization| Multiple settings and people      |
      | Intrinsic motivation | Connect to natural consequences     |
      | Parent training      | Home implementation support         |
      | Teacher consultation | Classroom strategy integration      |

  @discrete-trial-training @dtt-protocols @skill-acquisition @workflow @not-implemented
  Scenario: Systematic discrete trial training session implementation
    Given I am running DTT programs for student "Aiden"
    And Aiden has programs for receptive identification, matching, and imitation
    When I prepare for a structured DTT session
    And I set up comprehensive programming:
      | Program Area         | Current Targets                     | Mastery Criteria        |
      | Receptive ID         | Body parts (nose, eyes, mouth)     | 90% over 3 sessions     |
      | Matching             | Identical objects (cup, book, ball)| 80% over 2 sessions     |
      | Motor imitation      | Gross motor actions (clap, wave)   | 90% first trial accuracy|
    Then session structure should follow protocol:
      | Session Element      | Implementation                      |
      | Clear workspace      | Minimal distractions, organized materials |
      | Sitting arrangement  | Face-to-face, appropriate distance  |
      | Material preparation | All items ready, data sheet accessible |
      | Reinforcer availability| High-preference items identified   |
    When I begin receptive identification trials
    Then trial structure should include:
      | Trial Component      | Procedure                           |
      | Instruction delivery | Clear, consistent discriminative stimulus |
      | Response opportunity | 3-5 second wait time               |
      | Response recording   | Correct (+), Incorrect (-), Prompted (P), No response (NR) |
      | Consequence delivery | Immediate reinforcement or correction |
      | Inter-trial interval | 3-second pause between trials      |
    And prompt hierarchy should be systematically applied:
      | Prompt Level         | Implementation                      |
      | Independent          | No prompts, natural response       |
      | Gestural prompt      | Point to correct choice             |
      | Partial physical     | Light guidance to correct response  |
      | Full physical        | Hand-over-hand completion           |
    When recording trial-by-trial data
    Then data collection should capture:
      | Data Element         | Recording Method                    |
      | Response accuracy    | Correct/incorrect for each trial    |
      | Prompt level needed  | Least to most hierarchy tracking    |
      | Response latency     | Time from instruction to response   |
      | Problem behaviors    | Any interfering behaviors           |
      | Session duration     | Total time and trial count          |
    And mastery determination should consider:
      | Mastery Analysis     | Criteria                            |
      | Accuracy percentage  | 80-90% depending on program         |
      | Consistency          | Performance across multiple sessions|
      | Generalization probes| Novel materials and settings        |
      | Maintenance checks   | Retention over time                 |
    When programs reach mastery
    Then advancement should include:
      | Advancement Type     | Next Steps                          |
      | Target expansion     | Add new exemplars to program        |
      | Complexity increase  | More difficult discrimination tasks |
      | Generalization training| Multiple people and environments   |
      | Maintenance schedule | Intermittent review sessions        |

  @functional-analysis @behavior-function @experimental-design @workflow @not-implemented
  Scenario: Comprehensive functional analysis implementation and interpretation
    Given student "Jackson" displays aggressive behaviors requiring function identification
    When I implement a systematic functional analysis
    Then I should establish experimental conditions:
      | FA Condition         | Setup and Procedures                |
      | Attention condition  | Adult attention contingent on behavior |
      | Demand condition     | Academic tasks presented, escape available |
      | Tangible condition   | Preferred items restricted, available contingent |
      | Control/Play condition| Free access to attention and activities |
    And each condition should be systematically implemented:
      | Implementation Element| Procedure                          |
      | Session duration     | 10-15 minutes per condition        |
      | Counterbalancing     | Randomized condition order         |
      | Multiple sessions    | 3-5 sessions per condition minimum |
      | Data collection      | Frequency/rate of target behavior  |
      | Safety protocols     | Immediate intervention if needed    |
    When conducting attention condition sessions
    Then procedures should include:
      | Attention Procedure  | Implementation                      |
      | Pre-session setup    | Adult busy with other activities   |
      | Behavior contingency | Brief attention delivered for behavior |
      | Attention type       | "Don't do that" or similar concern  |
      | Non-contingent attention| Minimal during session            |
    And demand condition should involve:
      | Demand Procedure     | Implementation                      |
      | Task presentation    | Age-appropriate academic demands    |
      | Escape contingency   | Brief break from task for behavior  |
      | Task difficulty      | Slightly above current ability level|
      | Re-presentation      | Task returns after brief break      |
    When analyzing functional analysis results
    Then data interpretation should examine:
      | Analysis Component   | Evaluation Criteria                 |
      | Condition differentiation| Clear differences in behavior rates|
      | Highest rates        | Which condition produces most behavior|
      | Consistent patterns  | Replicated across multiple sessions |
      | Individual variability| Session-to-session consistency     |
    And function determination should identify:
      | Function Category    | Evidence Required                   |
      | Attention-maintained | Higher rates in attention condition |
      | Escape-maintained    | Higher rates in demand condition    |
      | Tangible-maintained  | Higher rates in tangible condition  |
      | Automatic reinforcement| Consistent rates across conditions |
    When function is identified
    Then intervention planning should include:
      | Intervention Component| Function-Based Strategy            |
      | Antecedent modifications| Reduce establishing operations     |
      | Replacement behavior teaching| Functional communication alternative|
      | Consequence modifications| Reinforce appropriate behavior    |
      | Extinction procedures | Discontinue reinforcing problem behavior|
      | Generalization planning| Multiple settings and people       |

  @behavior-intervention-plan @function-based-intervention @comprehensive-bip @workflow @not-implemented
  Scenario: Complete behavior intervention plan development and implementation
    Given functional analysis identified escape function for Tyler's aggressive behavior
    When I develop a comprehensive behavior intervention plan (BIP)
    Then the plan should include all required components:
      | BIP Component        | Specific Content                    |
      | Target behavior definition| Operational definition of aggression|
      | Functional assessment summary| Escape function clearly identified|
      | Antecedent strategies| Reduce task difficulty, provide choices|
      | Replacement behavior | Functional communication training   |
      | Consequence strategies| Extinction + reinforcement for FCT |
      | Crisis/safety plan  | Procedures for dangerous behavior   |
      | Data collection plan | Ongoing monitoring protocols        |
      | Generalization plan  | Multiple settings implementation    |
    And antecedent strategies should prevent problem behavior:
      | Antecedent Strategy  | Implementation                      |
      | Task modification    | Reduce difficulty to instructional level|
      | Choice making        | Provide options within tasks        |
      | Visual supports      | Schedule and task completion cues   |
      | Environmental changes| Reduce noise and distractions       |
      | Proactive teaching   | Teach coping strategies             |
    When teaching replacement behaviors
    Then functional communication training should include:
      | FCT Component        | Teaching Procedure                  |
      | Communication modality| "Break please" card or sign         |
      | Teaching method      | Discrete trial training initially   |
      | Prompt hierarchy     | Most to least prompting             |
      | Reinforcement schedule| Dense initially, then fade         |
      | Generalization training| Multiple people and settings      |
    And extinction procedures should be:
      | Extinction Element   | Implementation                      |
      | Consequence removal  | No escape from tasks for aggression |
      | Consistent application| All team members follow protocol   |
      | Extinction burst preparation| Expect temporary increase      |
      | Safety considerations| Protective procedures in place      |
    When implementing the BIP
    Then data collection should monitor:
      | Data Type            | Measurement                         |
      | Problem behavior     | Frequency, duration, intensity      |
      | Replacement behavior | Frequency and independence level    |
      | Academic engagement  | Time on task and task completion    |
      | Generalization       | Performance across settings         |
    And team coordination should ensure:
      | Coordination Element | Implementation                      |
      | Staff training       | All implementers competent          |
      | Consistent procedures| Fidelity monitoring                 |
      | Regular meetings     | Weekly progress reviews             |
      | Plan modifications   | Data-based decision making          |
    When progress is evaluated
    Then success criteria should include:
      | Success Indicator    | Target Performance                  |
      | Behavior reduction   | 80% decrease in problem behavior    |
      | Replacement increase | Independent FCT use in 80% of opportunities|
      | Academic improvement | Increased task completion           |
      | Generalization       | Success across multiple settings    |

  @preference-assessment @reinforcer-identification @motivation-analysis @workflow @not-implemented
  Scenario: Systematic preference assessment and reinforcer identification
    Given I need to identify effective reinforcers for student "Maya"
    When I conduct comprehensive preference assessment
    Then I should use multiple assessment methods:
      | Assessment Type      | Procedure                           | Purpose                  |
      | Free operant         | Access to all items, observe engagement| Natural preferences     |
      | Paired choice        | Systematic presentation of item pairs | Relative preferences    |
      | Multiple stimulus    | Array of items, selection tracking  | Hierarchy development   |
      | Single stimulus      | Individual item presentation         | Approach/avoidance      |
    And preference assessment should sample across categories:
      | Category             | Items Tested                        |
      | Edible items         | Crackers, fruit snacks, juice       |
      | Tangible items       | Fidget toys, iPad, books            |
      | Activity items       | Bubbles, music, movement            |
      | Social items         | Praise, high-fives, tickles         |
      | Sensory items        | Weighted lap pad, textured materials|
    When conducting paired choice assessment
    Then systematic procedures should include:
      | Procedure Element    | Implementation                      |
      | Item selection       | Based on informant reports and observation|
      | Presentation method  | Simultaneous presentation, equal distance|
      | Choice recording     | Selection with approach/touch       |
      | Rotation schedule    | All possible pairs presented        |
      | Session structure    | Multiple sessions for reliability   |
    And data analysis should determine:
      | Analysis Component   | Calculation Method                  |
      | Selection percentage | (Selections/opportunities) × 100   |
      | Preference hierarchy | Rank order from highest to lowest  |
      | Consistency check    | Stability across sessions          |
      | Reliability          | Inter-observer agreement           |
    When preference hierarchy is established
    Then reinforcer effectiveness should be tested:
      | Effectiveness Test   | Procedure                           |
      | Reinforcement probe  | Work task with contingent access    |
      | Motivation assessment| Test under different states        |
      | Satiation checks     | Monitor effectiveness over time     |
      | Contextual variables | Test in different settings          |
    And ongoing monitoring should track:
      | Monitoring Element   | Schedule                            |
      | Preference shifts    | Monthly re-assessment               |
      | Reinforcer satiation | Daily effectiveness checks          |
      | New item introduction| Weekly novel item testing           |
      | Seasonal changes     | Quarterly comprehensive assessment  |
    When preferences change
    Then reinforcer management should include:
      | Management Strategy  | Implementation                      |
      | Rotation schedule    | Prevent satiation with variety     |
      | Novel item integration| Regular introduction of new options|
      | Contextual matching  | Different reinforcers for different tasks|
      | Individual adaptation| Customize based on current preferences|

  @task-analysis @skill-breakdown @chaining-procedures @workflow @not-implemented
  Scenario: Comprehensive task analysis and skill chaining implementation
    Given I need to teach student "David" complex self-care skills
    And David needs to learn hand washing with systematic instruction
    When I conduct detailed task analysis for hand washing
    Then I should break down the skill into discrete steps:
      | Step # | Task Component                      | Teaching Focus          |
      | 1      | Approach sink                       | Initiation and orientation|
      | 2      | Turn on water faucet                | Fine motor manipulation |
      | 3      | Adjust water temperature            | Safety and sensory awareness|
      | 4      | Wet hands under water               | Bilateral coordination  |
      | 5      | Apply soap to hands                 | Amount and distribution |
      | 6      | Rub hands together vigorously       | Duration and technique  |
      | 7      | Rinse hands thoroughly              | Complete soap removal   |
      | 8      | Turn off water faucet               | Conservation awareness  |
      | 9      | Dry hands with towel                | Thorough drying technique|
      | 10     | Put towel back in place             | Organization and cleanup|
    And teaching method should be selected based on learner characteristics:
      | Chaining Method      | When to Use                         | Implementation          |
      | Forward chaining     | Strong imitation skills             | Teach steps 1-10 in order|
      | Backward chaining    | Enjoys task completion              | Start with step 10, work backward|
      | Total task chaining  | Can handle full sequence            | Teach all steps each trial|
      | Graduated guidance   | Needs physical prompting            | Provide varying levels of help|
    When implementing forward chaining instruction
    Then systematic teaching should include:
      | Teaching Element     | Procedure                           |
      | Step mastery criteria| 80% independence over 3 days        |
      | Prompt hierarchy     | Verbal → gestural → physical       |
      | Error correction     | Immediate re-teaching of step       |
      | Reinforcement        | After each mastered step            |
      | Data collection      | Step-by-step independence tracking  |
    And visual supports should enhance learning:
      | Visual Support Type  | Purpose                             |
      | Picture sequence     | Step-by-step visual guide           |
      | Video modeling       | Demonstration of complete sequence  |
      | Checklist format     | Self-monitoring tool                |
      | Social story         | Context and importance explanation  |
    When collecting task analysis data
    Then recording should capture:
      | Data Element         | Recording Method                    |
      | Step independence    | Independent (+), Prompted (P), Error (-)|
      | Prompt level used    | Least intrusive effective prompt    |
      | Error patterns       | Which steps most difficult          |
      | Total task time      | Duration from start to completion   |
      | Quality indicators   | Thoroughness and technique          |
    And mastery decisions should consider:
      | Mastery Criterion    | Standard                            |
      | Step independence    | 80% unprompted across 3 sessions   |
      | Quality execution    | Meets hygiene and safety standards  |
      | Generalization       | Performance across settings         |
      | Maintenance          | Retention over time                 |
    When skills are mastered
    Then generalization programming should include:
      | Generalization Type  | Implementation                      |
      | Setting generalization| Practice in multiple bathroom locations|
      | Material generalization| Different soaps, towels, faucets   |
      | People generalization | Different supervisors present       |
      | Time generalization   | Various times throughout day        |

  @parent-training @home-implementation @aba-techniques @workflow @not-implemented
  Scenario: Comprehensive parent training in ABA techniques for home implementation
    Given parents need training to implement ABA strategies at home
    When I design parent training curriculum
    Then training should cover fundamental ABA principles:
      | Training Module      | Content Focus                       | Duration    |
      | ABA principles       | Reinforcement, punishment, extinction| 2 hours     |
      | Data collection      | Simple tracking methods             | 1 hour      |
      | Antecedent strategies| Environmental modifications         | 1.5 hours   |
      | Teaching procedures  | Prompting and fading               | 2 hours     |
      | Problem solving      | Troubleshooting common issues       | 1 hour      |
      | Generalization       | Transferring skills across settings| 1.5 hours   |
    And parent training should be individualized:
      | Individualization Factor| Adaptation Strategy              |
      | Family schedule      | Flexible training times             |
      | Language preferences | Materials in family's primary language|
      | Learning style       | Hands-on practice vs. theoretical   |
      | Child's specific needs| Focus on relevant interventions    |
      | Previous experience  | Build on existing knowledge         |
    When conducting hands-on training sessions
    Then practice should include:
      | Practice Component   | Implementation                      |
      | Demonstration        | Trainer models techniques with child|
      | Guided practice      | Parent implements with coaching     |
      | Independent practice | Parent implements while observed    |
      | Feedback and refinement| Specific suggestions for improvement|
      | Problem-solving      | Address challenges as they arise    |
    And home program development should include:
      | Program Element      | Customization                       |
      | Target behaviors     | Family-identified priorities       |
      | Teaching schedule    | Integrated into daily routines      |
      | Reinforcement system | Using naturally available reinforcers|
      | Data collection      | Simple, family-friendly methods     |
      | Crisis management    | Safety procedures for home setting  |
    When parents begin home implementation
    Then ongoing support should provide:
      | Support Type         | Delivery Method                     |
      | Weekly check-ins     | Phone calls or video conferences   |
      | Troubleshooting help | Problem-solving consultation        |
      | Data review          | Analysis and program adjustments    |
      | Refresher training   | Additional skill development        |
      | Peer support         | Parent support group connections    |
    And progress monitoring should track:
      | Monitoring Area      | Assessment Method                   |
      | Parent skill use     | Implementation fidelity checks      |
      | Child progress       | Home data collection review         |
      | Family satisfaction  | Regular feedback surveys            |
      | Generalization       | Skill use across family activities  |
    When challenges arise
    Then troubleshooting should address:
      | Common Challenge     | Solution Strategy                   |
      | Inconsistent implementation| Simplify procedures, increase support|
      | Sibling interference | Include siblings in program         |
      | Time constraints     | Integrate into existing routines    |
      | Limited resources    | Use naturally available materials   |
      | Behavior escalation  | Review safety and de-escalation     |
    And long-term support should ensure:
      | Support Element      | Implementation                      |
      | Maintenance training | Quarterly skill refreshers          |
      | Program evolution    | Adapt to child's changing needs     |
      | Community connections| Link to local ABA resources         |
      | Advocacy training    | Help parents navigate systems       |