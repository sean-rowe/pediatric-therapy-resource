Feature: PECS Implementation API Endpoints (FR-033)
  As a therapy professional implementing PECS
  I want comprehensive PECS protocol support
  So that I can effectively implement all 6 phases with fidelity

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"
    And I have PECS training certification

  # POST /api/pecs/students
  @endpoint @pecs @setup @not-implemented
  Scenario: Initialize PECS for new student
    When I send a POST request to "/api/pecs/students" with:
      | field              | value                    |
      | studentId          | student-123              |
      | startDate          | 2024-01-22               |
      | communicationLevel | non-verbal               |
      | motorSkills        | adequate                 |
      | visualSkills       | functional               |
      | behaviorConcerns   | ["limited-initiation"]   |
    Then the response status should be 201
    And the response should contain:
      | field           | type   |
      | pecsProfileId   | string |
      | currentPhase    | number |
      | setupChecklist  | array  |
      | materialsList   | array  |

  # POST /api/pecs/students/{studentId}/reinforcer-assessment
  @endpoint @pecs @assessment @not-implemented
  Scenario: Record reinforcer assessment
    Given student "student-123" is starting PECS
    When I send a POST request to "/api/pecs/students/student-123/reinforcer-assessment" with:
      | field       | value                                        |
      | assessmentType | preference-assessment                     |
      | items       | [{"item": "goldfish", "rank": 1, "engagement": "high"}] |
      | duration    | 15                                           |
      | setting     | therapy-room                                 |
      | notes       | Strong preference for edibles                |
    Then the response status should be 201
    And top reinforcers should be identified
    And Phase 1 materials should be prepared

  # GET /api/pecs/students/{studentId}/phase
  @endpoint @pecs @progress @not-implemented
  Scenario: Get current PECS phase and progress
    Given student "student-123" is in PECS program
    When I send a GET request to "/api/pecs/students/student-123/phase"
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | currentPhase       | number |
      | phaseStartDate     | string |
      | daysInPhase        | number |
      | masteryPercentage  | number |
      | exchangeCount      | number |
      | spontaneousRate    | number |
      | nextPhaseCriteria  | object |

  # POST /api/pecs/students/{studentId}/exchanges
  @endpoint @pecs @data @not-implemented
  Scenario: Record PECS exchange
    Given student "student-123" is in Phase 1
    When I send a POST request to "/api/pecs/students/student-123/exchanges" with:
      | field            | value                    |
      | phase            | 1                        |
      | pictureUsed      | goldfish-crackers        |
      | promptLevel      | physical                 |
      | exchangeType     | prompted                 |
      | communicationPartner | therapist            |
      | reinforcerDelivered | true                  |
      | timestamp        | 2024-01-22T10:15:00Z     |
    Then the response status should be 201
    And exchange should be recorded
    And prompt fading data should update

  # PUT /api/pecs/students/{studentId}/phase
  @endpoint @pecs @phase-change @not-implemented
  Scenario: Advance to next PECS phase
    Given student "student-123" has met Phase 1 criteria
    When I send a PUT request to "/api/pecs/students/student-123/phase" with:
      | field         | value                           |
      | newPhase      | 2                               |
      | criteriaData  | {"exchanges": 50, "spontaneous": 80} |
      | notes         | Consistent reaching and releasing |
    Then the response status should be 200
    And phase should advance
    And new materials should be prepared
    And parent notification should be sent

  # GET /api/pecs/materials/phase/{phase}
  @endpoint @pecs @materials @not-implemented
  Scenario: Get phase-specific materials
    When I send a GET request to "/api/pecs/materials/phase/3"
    Then the response status should be 200
    And the response should contain:
      | field              | type  |
      | pictureCards       | array |
      | discriminationSets | array |
      | communicationBook  | object|
      | dataSheets         | array |
      | parentHandouts     | array |
      | trainingVideos     | array |

  # POST /api/pecs/discrimination-training
  @endpoint @pecs @discrimination @not-implemented
  Scenario: Record Phase 3 discrimination training
    Given student "student-123" is in Phase 3
    When I send a POST request to "/api/pecs/discrimination-training" with:
      | field             | value                         |
      | studentId         | student-123                   |
      | discriminationType| highly-preferred-vs-neutral   |
      | preferredItem     | goldfish                      |
      | distracterItem    | pencil                        |
      | trials            | [{"trial": 1, "choice": "goldfish", "correct": true}] |
      | errorCorrection   | false                         |
    Then the response status should be 201
    And discrimination data should be recorded
    And progress toward 3B should be tracked

  # POST /api/pecs/sentence-strip
  @endpoint @pecs @phase4 @not-implemented
  Scenario: Record Phase 4 sentence strip usage
    Given student "student-123" is in Phase 4
    When I send a POST request to "/api/pecs/sentence-strip" with:
      | field          | value                      |
      | studentId      | student-123                |
      | sentenceUsed   | ["I-want", "cookie"]       |
      | spontaneous    | false                      |
      | promptType     | pointing                   |
      | responseTime   | 5                          |
      | correct        | true                       |
    Then the response status should be 201
    And sentence complexity should be tracked
    And vocabulary growth should be monitored

  # GET /api/pecs/students/{studentId}/vocabulary
  @endpoint @pecs @vocabulary @not-implemented
  Scenario: Get student's PECS vocabulary
    Given student "student-123" has been using PECS
    When I send a GET request to "/api/pecs/students/student-123/vocabulary"
    Then the response status should be 200
    And the response should contain:
      | field              | type  |
      | masteredPictures   | array |
      | emergingPictures   | array |
      | requestFrequency   | object|
      | categoriesUsed     | array |
      | sentenceComplexity | object|

  # POST /api/pecs/attributes
  @endpoint @pecs @phase6 @not-implemented
  Scenario: Record Phase 6 attribute usage
    Given student "student-123" is in Phase 6
    When I send a POST request to "/api/pecs/attributes" with:
      | field         | value                               |
      | studentId     | student-123                         |
      | sentenceUsed  | ["I-want", "big", "blue", "ball"]  |
      | context       | play-time                           |
      | spontaneous   | true                                |
      | commenting    | false                               |
    Then the response status should be 201
    And attribute usage should be tracked
    And language complexity should be analyzed

  # POST /api/pecs/generalization
  @endpoint @pecs @generalization @not-implemented
  Scenario: Track generalization across settings
    When I send a POST request to "/api/pecs/generalization" with:
      | field              | value                    |
      | studentId          | student-123              |
      | setting            | cafeteria                |
      | communicationPartner| peer                    |
      | exchangeSuccessful | true                     |
      | pictureUsed        | juice                    |
      | promptNeeded       | false                    |
    Then the response status should be 201
    And generalization matrix should update
    And new settings/partners should be tracked

  # GET /api/pecs/students/{studentId}/report
  @endpoint @pecs @reporting @not-implemented
  Scenario: Generate PECS progress report
    Given student "student-123" has 3 months of PECS data
    When I send a GET request to "/api/pecs/students/student-123/report?type=comprehensive"
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | reportUrl          | string |
      | phasesCompleted    | array  |
      | currentSkills      | object |
      | vocabularySize     | number |
      | spontaneousRate    | number |
      | generalizationData | object |
      | recommendations    | array  |

  # POST /api/pecs/troubleshooting
  @endpoint @pecs @support @not-implemented
  Scenario: Get troubleshooting suggestions
    When I send a POST request to "/api/pecs/troubleshooting" with:
      | field       | value                           |
      | studentId   | student-123                     |
      | issue       | not-releasing-picture           |
      | phase       | 1                               |
      | triedStrategies | ["hand-over-hand", "backward-chaining"] |
    Then the response status should be 200
    And the response should contain:
      | field         | type  |
      | suggestions   | array |
      | videoExamples | array |
      | commonCauses  | array |
      | nextSteps     | array |

  # GET /api/pecs/fidelity-checklist/{phase}
  @endpoint @pecs @fidelity @not-implemented
  Scenario: Get fidelity checklist for phase
    When I send a GET request to "/api/pecs/fidelity-checklist/2"
    Then the response status should be 200
    And the response should contain:
      | field           | type  |
      | criticalSteps   | array |
      | environmentSetup| array |
      | promptingHierarchy| array |
      | commonErrors    | array |
      | videoExamples   | array |

  # POST /api/pecs/team-training
  @endpoint @pecs @training @not-implemented
  Scenario: Track team member training
    When I send a POST request to "/api/pecs/team-training" with:
      | field         | value                    |
      | studentId     | student-123              |
      | traineeEmail  | aide@school.com          |
      | traineeRole   | classroom-aide           |
      | phaseTrained  | 1                        |
      | competencyMet | true                     |
      | practiceTrials| 10                       |
    Then the response status should be 201
    And team member should be added
    And training certificate should be generated

  # GET /api/pecs/resources
  @endpoint @pecs @resources @not-implemented
  Scenario: Get PECS implementation resources
    When I send a GET request to "/api/pecs/resources?category=parent-training"
    Then the response status should be 200
    And the response should contain:
      | field          | type  |
      | handouts       | array |
      | videos         | array |
      | pictureLibrary | array |
      | dataSheets     | array |
      | quickGuides    | array |

  # FR-033 Missing Critical PECS Complete 6-Phase Implementation Business Workflow Scenarios
  @pecs-setup @reinforcer-sampling @phase1-prep @workflow @not-implemented
  Scenario: Complete PECS Phase 1 setup with comprehensive reinforcer sampling
    Given I have a new student "Marcus" starting PECS
    And Marcus is currently non-verbal with limited communication
    When I begin comprehensive reinforcer sampling
    And I conduct preference assessment across categories:
      | Item Category | Specific Items              | Marcus's Response   |
      | Food items    | Goldfish crackers, cookies  | High interest       |
      | Beverages     | Juice box, water bottle     | Medium interest     |
      | Toys          | Bubble gun, fidget spinner  | High interest       |
      | Activities    | iPad games, music           | Very high interest  |
      | Sensory items | Squeeze ball, weighted lap pad| Low interest      |
      | Social items  | High-fives, tickles         | Medium interest     |
    And I document preference hierarchy:
      | Rank | Item             | Interest Level | Engagement Duration |
      | 1    | iPad games       | Very high      | 10+ minutes        |
      | 2    | Goldfish crackers| High           | 5 minutes          |
      | 3    | Bubble gun       | High           | 4 minutes          |
      | 4    | Juice box        | Medium         | 2 minutes          |
    Then I should prepare Phase 1 materials:
      | Material Type        | Contents                          |
      | Picture cards        | Only high-interest items (top 3)  |
      | Communication book   | Single picture strip setup        |
      | Data collection sheets| Phase 1 exchange tracking       |
      | Two-person protocol  | Prompting hierarchy guide         |
      | Environmental setup  | Motivating environment checklist  |
    And training materials should include:
      | Training Component   | Details                           |
      | Video examples       | Phase 1 exchange demonstrations   |
      | Fidelity checklist   | Critical implementation steps     |
      | Troubleshooting guide| Common Phase 1 challenges        |
      | Parent handout       | Home implementation tips          |
    When I begin Phase 1 training
    Then implementation should follow protocol:
      | Protocol Element     | Requirement                       |
      | Two-person training  | Communicative partner + prompter  |
      | Physical prompting   | Hand-over-hand guidance           |
      | Immediate reinforcement| Item delivered within 2 seconds |
      | No verbal prompts    | Silent exchange training          |
      | Consistent environment| Same setup each session          |

  @pecs-phase2 @distance-persistence @independent-seeking @workflow @not-implemented  
  Scenario: PECS Phase 2 distance training and persistence development
    Given Marcus has mastered Phase 1 basic exchanges
    And he exchanges pictures 80% independently over 3 consecutive days
    When I advance to Phase 2 distance and persistence training
    Then I should systematically increase demands:
      | Distance Type        | Starting Distance | Target Distance     |
      | Book to student      | 1 foot           | Across classroom    |
      | Student to partner   | 1 foot           | 10 feet away       |
      | Partner attention    | Facing student   | Back turned         |
    And Phase 2 data collection should track:
      | Skill Measured       | Criteria for Mastery              |
      | Distance to book     | Retrieves from 6+ feet away       |
      | Distance to partner  | Approaches partner 10+ feet away  |
      | Persistence seeking  | Continues trying when ignored     |
      | Independence level   | 80% exchanges without prompts     |
    When Marcus reaches appropriate distances
    Then I should add persistence challenges:
      | Persistence Challenge| Implementation                    |
      | Partner not looking  | Train tapping or moving to face   |
      | Partner busy         | Train waiting for attention       |
      | Book moved           | Train searching and retrieving    |
      | Multiple attempts    | Train continuing after first try  |
    And mastery criteria should require:
      | Mastery Requirement  | Performance Standard              |
      | Consistent distance  | 80% success over 3 days          |
      | Multiple partners    | Generalizes to 3+ people          |
      | Various locations    | Works in 3+ environments          |
      | Spontaneous seeking  | Initiates without setup prompts   |
    When Phase 2 criteria are met
    Then I should prepare for Phase 3:
      | Phase 3 Preparation  | Action Required                   |
      | Add neutral items    | Introduce non-preferred pictures  |
      | Create discrimination sets| Highly preferred vs neutral  |
      | Train correspondence | Picture-to-item matching         |
      | Prepare error correction| 4-step correction procedure    |

  @pecs-phase3 @picture-discrimination @correspondence @workflow @not-implemented
  Scenario: PECS Phase 3 picture discrimination with systematic progression
    Given Marcus consistently travels distances and persists in Phase 2
    When I introduce Phase 3 discrimination training
    Then I should follow systematic discrimination protocol:
      | Phase 3 Stage        | Picture Array                     |
      | 3A Introduction      | 1 highly preferred, 1 neutral     |
      | 3A Mastery          | 80% correct with 2 pictures       |
      | 3B Expansion        | Multiple preferred items           |
      | 3B Mastery          | 80% correct with 6+ pictures      |
    And error correction procedure should include:
      | Error Correction Step| Implementation                    |
      | 1. Block exchange    | Prevent incorrect picture pickup  |
      | 2. Show correspondence| Point to item, point to picture  |
      | 3. Repeat trial      | Same array, same target           |
      | 4. Success trial     | Ensure next exchange is correct    |
    When conducting discrimination training
    Then I should systematically vary:
      | Variation Type       | Implementation Strategy           |
      | Picture positions    | Randomize left/right placement    |
      | Preferred items      | Rotate through reinforcer hierarchy|
      | Neutral items        | Use consistent non-preferred pics  |
      | Environmental factors| Different times, settings, people |
    And correspondence checks should verify:
      | Correspondence Type  | Assessment Method                 |
      | Picture-to-item      | Student matches physical items    |
      | Item-to-picture      | Student finds correct picture     |
      | Functional relationship| Uses picture to request actual item|
    And Phase 3 progression should include:
      | Progression Element  | Requirement                       |
      | Mastery data         | 80% accuracy over 3 consecutive days|
      | Multiple distractors | Success with 4-6 picture choices  |
      | Novel situations     | Works in new environments         |
      | Various partners     | Discriminates for different people |
    When Phase 3 mastery is achieved
    Then prepare for sentence structure:
      | Phase 4 Preparation  | Materials Needed                  |
      | "I want" card        | Sentence starter symbol           |
      | Sentence strips      | Visual structure for building     |
      | Book reorganization  | Categorical organization          |
      | Advanced data sheets | Sentence construction tracking    |

  @pecs-phase4 @sentence-structure @i-want-training @workflow @not-implemented
  Scenario: PECS Phase 4 sentence structure with systematic complexity building
    Given Marcus discriminates between multiple pictures accurately
    When I introduce Phase 4 sentence structure training
    Then I should systematically teach sentence building:
      | Sentence Component   | Teaching Method                   |
      | "I want" card        | Always first on sentence strip    |
      | Object picture       | Second position on strip          |
      | Strip construction   | Left-to-right sequence training   |
      | Complete exchange    | Full strip given to partner       |
    And sentence training should progress through:
      | Training Stage       | Complexity Level                  |
      | Physical prompting   | Hand-over-hand strip building     |
      | Gestural prompting   | Pointing to sequence              |
      | Independence         | Student builds without prompts    |
      | Fluency building     | Increase speed and consistency    |
    When building sentence construction skills
    Then I should track:
      | Skill Development    | Measurement                       |
      | Sequence accuracy    | Correct "I want" + object order   |
      | Independence level   | Percentage without prompts        |
      | Response time        | Speed of sentence construction    |
      | Generalization       | Multiple vocabulary items         |
    And sentence complexity should gradually expand:
      | Expansion Type       | Example Progression               |
      | Basic requests       | "I want" + single item            |
      | Multiple items       | "I want" + preferred choice       |
      | Different sentence starters| Prepare for Phase 5 variation|
    And error correction should address:
      | Error Type           | Correction Strategy               |
      | Wrong sequence       | Point to correct positions        |
      | Missing "I want"     | Prompt sentence starter first     |
      | Incomplete strips    | Require complete sentence         |
      | Speed issues         | Practice fluency building         |
    When Phase 4 mastery is demonstrated
    Then prepare for Phase 5:
      | Phase 5 Preparation  | Requirements                      |
      | "What do you want?" training| Responding to questions      |
      | Spontaneous requesting| Independent sentence building    |
      | Multiple functions   | Beyond just requesting           |

  @pecs-phase5 @responsive-communication @question-answering @workflow @not-implemented
  Scenario: PECS Phase 5 responsive communication and spontaneous requesting
    Given Marcus consistently builds "I want" sentences independently
    When I introduce Phase 5 responsive communication
    Then I should teach dual communication functions:
      | Communication Function| Training Implementation          |
      | Spontaneous requesting| Independent sentence initiation |
      | Responsive communication| Answer "What do you want?"     |
      | Question discrimination| Different responses to questions|
    And Phase 5 training should alternate between:
      | Training Type        | Procedure                        |
      | Spontaneous trials   | No verbal prompt, student initiates|
      | Responsive trials    | "What do you want?" prompt given |
      | Mixed practice       | Random alternation of both types |
    When conducting responsive training
    Then questioning should follow protocol:
      | Question Protocol    | Implementation                   |
      | Visual prompt first  | Present motivating item          |
      | Verbal question      | "What do you want?"              |
      | Wait time           | 3-5 seconds for student response |
      | Prompt if needed    | Point to communication book      |
    And spontaneous behavior should be:
      | Spontaneous Criteria | Requirement                      |
      | No verbal prompt     | Student initiates without question|
      | Environmental setup  | Item visible but not offered     |
      | Independence         | Complete sentence construction    |
      | Functional purpose   | Gets desired item/activity       |
    And data collection should track:
      | Data Category        | Specific Measurements            |
      | Spontaneous rate     | Percentage of unprompted exchanges|
      | Response accuracy    | Correct answers to questions     |
      | Discrimination       | Responds differently to questions|
      | Generalization       | Multiple people and settings     |
    When Phase 5 proficiency is achieved
    Then expand communication functions:
      | Function Expansion   | Implementation                   |
      | Commenting preparation| "I see" sentence starter intro  |
      | Multiple questions   | "What do you want?" vs others   |
      | Social interactions  | Greetings and social exchanges  |

  @pecs-phase6 @commenting @attributes @advanced-language @workflow @not-implemented
  Scenario: PECS Phase 6 commenting and attribute use for complex communication
    Given Marcus responds to questions and initiates requests spontaneously
    When I introduce Phase 6 commenting and attributes
    Then I should expand sentence starter repertoire:
      | Sentence Starter     | Communication Function           |
      | "I want"            | Requesting (established)         |
      | "I see"             | Commenting on environment        |
      | "I hear"            | Responding to sounds             |
      | "I feel"            | Expressing emotions/states       |
      | "It is"             | Describing attributes            |
    And attribute training should include:
      | Attribute Category   | Examples                         |
      | Colors              | Red, blue, green, yellow         |
      | Sizes               | Big, little, medium              |
      | Descriptors         | Hot, cold, fast, slow           |
      | Quantities          | More, all done, some             |
      | Emotions            | Happy, sad, excited, tired       |
    When teaching commenting skills
    Then environmental setups should promote:
      | Commenting Opportunity| Environmental Trigger           |
      | "I see" statements   | Interesting items in view       |
      | "I hear" responses   | Novel or attention-getting sounds|
      | "I feel" expressions | Physical sensations or emotions |
      | Attribute descriptions| Items with obvious qualities   |
    And sentence complexity should expand:
      | Complexity Level     | Example Sentences                |
      | Basic commenting     | "I see ball"                     |
      | Single attributes    | "I want big cookie"              |
      | Multiple attributes  | "I see little red car"           |
      | Complex sentences    | "I want the big blue ball please"|
    And Phase 6 mastery should demonstrate:
      | Mastery Criteria     | Performance Standard             |
      | Commenting frequency | Spontaneous comments 5+ per session|
      | Attribute accuracy   | 80% correct attribute use        |
      | Function variety     | Uses 3+ sentence starters        |
      | Social appropriateness| Comments relevant to context    |
      | Generalization       | Multiple settings and partners   |
    When Phase 6 skills are established
    Then focus on communication refinement:
      | Refinement Area      | Advanced Skills                  |
      | Conversational turns | Back-and-forth exchanges         |
      | Topic maintenance    | Staying on conversation themes   |
      | Social pragmatics    | Appropriate timing and content   |
      | Complex language     | Multi-word descriptive sentences |

  @pecs-generalization @multiple-settings @communication-partners @workflow @not-implemented
  Scenario: Comprehensive PECS generalization across settings and partners
    Given Marcus demonstrates PECS skills across all 6 phases
    When I implement systematic generalization training
    Then I should program generalization across:
      | Generalization Dimension| Target Variations               |
      | Communication partners  | Family, teachers, peers, strangers|
      | Physical settings       | Home, school, community, therapy |
      | Times of day           | Morning, afternoon, evening      |
      | Activities/contexts    | Meals, play, work, transitions   |
      | Materials              | Different communication books    |
    And generalization assessment should include:
      | Assessment Area      | Evaluation Method                |
      | Novel partners       | Test with unfamiliar people      |
      | New environments     | Community outings and field trips|
      | Spontaneous use      | No prompting in natural contexts |
      | Functional outcomes  | Gets needs met across settings   |
    When conducting generalization probes
    Then data collection should track:
      | Data Point           | Measurement Standard             |
      | Success rate         | Percentage successful exchanges  |
      | Prompt independence  | Level of support needed          |
      | Communication functions| Requesting vs commenting ratio  |
      | Partner responsiveness| How well others understand/respond|
    And generalization strategies should include:
      | Strategy Type        | Implementation                   |
      | Multiple exemplar training| Practice with variety       |
      | Common stimulus elements| Consistent book/picture format|
      | Natural contingencies| Real-world reinforcement        |
      | Partner training     | Teach others to respond appropriately|
    And troubleshooting should address:
      | Generalization Challenge| Solution Strategy               |
      | Partner doesn't respond| Train communication partner     |
      | Student doesn't initiate| Increase motivation/opportunities|
      | Setting interferes     | Modify environment temporarily  |
      | Materials unavailable  | Create portable communication systems|
    When generalization is established
    Then long-term maintenance should include:
      | Maintenance Activity | Implementation Schedule          |
      | Periodic probes      | Monthly generalization checks    |
      | Partner refresher training| Quarterly reviews with team   |
      | Material updates     | Add new vocabulary as needed     |
      | Progress monitoring  | Ongoing data collection          |

  @pecs-team-training @two-person-protocol @fidelity @workflow @not-implemented
  Scenario: Comprehensive team training and implementation fidelity
    Given multiple team members need PECS training for consistent implementation
    When I conduct comprehensive team training
    Then training should cover all team roles:
      | Team Member Role     | Training Focus                   |
      | Classroom teacher    | All phases, daily integration    |
      | Paraprofessional aide| Prompting, data collection       |
      | Related service providers| Phase-specific skills         |
      | Family members       | Home implementation, generalization|
      | Peers (when appropriate)| Natural interaction support   |
    And two-person training protocol should be emphasized:
      | Two-Person Role      | Responsibilities                 |
      | Communication partner| Receives exchange, provides reinforcement|
      | Physical prompter    | Provides hand-over-hand guidance |
      | Role switching       | Team members alternate roles     |
      | Fading procedures    | Systematically reduce prompting  |
    When conducting team competency checks
    Then fidelity assessment should evaluate:
      | Fidelity Component   | Assessment Method                |
      | Critical steps       | Checklist completion             |
      | Timing accuracy      | Prompt delivery and fading       |
      | Error correction     | Proper procedure implementation  |
      | Data collection      | Accurate and consistent recording|
      | Environmental setup  | Motivation and materials ready   |
    And ongoing support should provide:
      | Support Type         | Delivery Method                  |
      | Initial training     | Hands-on workshop with practice  |
      | Competency verification| Direct observation and feedback|
      | Refresher sessions   | Monthly implementation reviews   |
      | Troubleshooting      | Problem-solving consultation     |
      | Video modeling       | Examples of correct implementation|
    And team coordination should ensure:
      | Coordination Element | Implementation Standard          |
      | Consistent data      | Same collection procedures       |
      | Communication        | Regular team meetings            |
      | Material management  | Shared resources and updates     |
      | Progress monitoring  | Team-wide goal tracking          |
    When training is complete
    Then ongoing fidelity should include:
      | Fidelity Maintenance | Monitoring System                |
      | Self-assessment tools| Team member checklists           |
      | Peer observation     | Buddy system for feedback        |
      | Supervisor checks    | Regular fidelity observations    |
      | Data review meetings | Weekly progress analysis         |

  @pecs-troubleshooting @problem-solving @implementation-challenges @workflow @not-implemented
  Scenario: Systematic PECS troubleshooting and problem-solving protocols
    Given PECS implementation challenges arise during training
    When I encounter common implementation problems
    Then I should have systematic troubleshooting protocols:
      | Problem Category     | Common Issues                    |
      | Picture discrimination| Incorrect choices, confusion    |
      | Physical prompting   | Student resistance, prompt dependence|
      | Motivation          | Low interest, reinforcer satiation|
      | Generalization      | Limited settings, partner issues |
      | Team implementation | Inconsistent procedures, training needs|
    And Phase 1 troubleshooting should address:
      | Phase 1 Challenge    | Solution Strategy                |
      | Won't release picture| Backward chaining, preferred items|
      | Doesn't reach for book| Increase motivation, check positioning|
      | Prompt dependence    | Systematic fading procedures     |
      | Limited engagement   | Re-assess reinforcer preferences |
    And Phase 3 discrimination issues should include:
      | Discrimination Problem| Troubleshooting Approach        |
      | Always picks same picture| Increase motivation for targets|
      | Random selection     | Ensure correspondence understanding|
      | Error pattern        | Analyze data for systematic errors|
      | Slow acquisition     | Simplify array, increase practice|
    When troubleshooting advanced phases
    Then solutions should consider:
      | Advanced Challenge   | Problem-Solving Strategy         |
      | Sentence building errors| Break down into component steps|
      | Limited spontaneous use| Create more natural opportunities|
      | Commenting difficulties| Model appropriate comments     |
      | Generalization failure| Systematic programming needed   |
    And data-based decision making should guide:
      | Decision Point       | Data Analysis Required           |
      | Phase advancement    | Mastery criteria met consistently|
      | Strategy modification| Progress trend analysis          |
      | Reinforcer changes   | Motivation assessment results    |
      | Training intensity   | Rate of skill acquisition        |
    And systematic problem-solving should follow:
      | Problem-Solving Step | Implementation                   |
      | Define specific problem| Objective, measurable description|
      | Analyze contributing factors| Environmental, procedural, learner|
      | Develop intervention plan| Evidence-based strategy selection|
      | Implement with fidelity| Consistent execution of plan    |
      | Monitor and adjust   | Data-driven modification process |
    When problems are resolved
    Then prevention strategies should include:
      | Prevention Strategy  | Implementation                   |
      | Regular fidelity checks| Prevent implementation drift   |
      | Ongoing training     | Maintain team competency         |
      | Data review meetings | Early problem identification     |
      | Environmental design | Setup for success               |