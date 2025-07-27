Feature: AAC Comprehensive Suite API Endpoints (FR-035)
  As a therapy professional supporting AAC users
  I want comprehensive AAC tools beyond PECS
  So that I can support diverse communication needs

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"
    And I have AAC training

  # POST /api/aac/students/{studentId}/profile
  @endpoint @aac @assessment @not-implemented
  Scenario: Create AAC user profile
    When I send a POST request to "/api/aac/students/student-123/profile" with:
      | field                | value                              |
      | communicationLevel   | emerging                           |
      | motorAbilities      | limited-fine-motor                 |
      | visionStatus        | functional                         |
      | hearingStatus       | normal                             |
      | cognitiveLevel      | moderate-delays                    |
      | currentMethods      | ["gestures", "vocalizations"]      |
      | accessMethod        | direct-selection                   |
      | previousAAC         | none                               |
    Then the response status should be 201
    And AAC recommendations should be generated
    And assessment tools should be suggested

  # POST /api/aac/core-boards
  @endpoint @aac @core-vocabulary @not-implemented
  Scenario: Generate core vocabulary board
    When I send a POST request to "/api/aac/core-boards" with:
      | field          | value                          |
      | studentId      | student-123                    |
      | boardSize      | 36                             |
      | layout         | 6x6                            |
      | wordSet        | first-40-core                  |
      | colorCoding    | parts-of-speech                |
      | symbolSet      | symbolstix                     |
      | includePhotos  | ["family", "favorite-items"]   |
    Then the response status should be 201
    And the response should contain:
      | field         | type   |
      | boardId       | string |
      | downloadUrl   | string |
      | printVersion  | string |
      | digitalVersion| string |
    And motor planning should be consistent

  # GET /api/aac/core-boards/{boardId}/variations
  @endpoint @aac @board-formats @not-implemented
  Scenario: Get board format variations
    Given core board "board-123" exists
    When I send a GET request to "/api/aac/core-boards/board-123/variations"
    Then the response status should be 200
    And the response should contain:
      | field           | type  |
      | flipBook        | object|
      | keyguard        | object|
      | lowTech         | object|
      | eyeGazeLayout   | object|
      | switchScanning  | object|

  # POST /api/aac/activity-boards
  @endpoint @aac @activity-specific @not-implemented
  Scenario: Create activity-specific board
    When I send a POST request to "/api/aac/activity-boards" with:
      | field         | value                           |
      | studentId     | student-123                     |
      | activity      | snack-time                      |
      | vocabulary    | ["more", "all-done", "drink", "eat", "yummy"] |
      | fringe        | ["juice", "crackers", "apple"]  |
      | layout        | activity-specific               |
      | includeCore   | true                            |
    Then the response status should be 201
    And board should combine core and fringe
    And visual supports should be included

  # POST /api/aac/switch-setup
  @endpoint @aac @switch-access @not-implemented
  Scenario: Configure switch access settings
    When I send a POST request to "/api/aac/switch-setup" with:
      | field              | value                    |
      | studentId          | student-123              |
      | switchType         | single                   |
      | switchLocation     | right-hand               |
      | scanPattern        | row-column               |
      | scanSpeed          | 2000                     |
      | acceptanceTime     | 1000                     |
      | auditoryCues       | true                     |
      | visualHighlight    | yellow-border            |
      | practiceMode       | true                     |
    Then the response status should be 201
    And switch training activities should be provided
    And settings should be saved

  # POST /api/aac/switch-data
  @endpoint @aac @switch-tracking @not-implemented
  Scenario: Record switch usage data
    Given student "student-123" uses switch access
    When I send a POST request to "/api/aac/switch-data" with:
      | field            | value                    |
      | studentId        | student-123              |
      | sessionDuration  | 600                      |
      | switchHits       | 45                       |
      | accurateSelections| 38                      |
      | missedScans      | 7                        |
      | fatigueObserved  | mild                     |
    Then the response status should be 201
    And efficiency metrics should be calculated
    And scan speed recommendations should update

  # POST /api/aac/partner-training
  @endpoint @aac @communication-partners @not-implemented
  Scenario: Create partner training plan
    When I send a POST request to "/api/aac/partner-training" with:
      | field           | value                           |
      | studentId       | student-123                     |
      | partners        | ["parent", "teacher", "aide"]   |
      | trainingFocus   | ["modeling", "wait-time", "responding"] |
      | aacSystem       | core-board-36                   |
      | schedule        | weekly                          |
    Then the response status should be 201
    And training materials should be generated
    And video examples should be provided
    And progress tracking should be set up

  # POST /api/aac/modeling-data
  @endpoint @aac @aided-language @not-implemented
  Scenario: Track aided language modeling
    When I send a POST request to "/api/aac/modeling-data" with:
      | field           | value                           |
      | studentId       | student-123                     |
      | activityType    | book-reading                    |
      | modeledWords    | ["want", "more", "turn", "look"] |
      | studentResponse | ["more", "look"]                |
      | engagementLevel | high                            |
      | duration        | 10                              |
      | partner         | therapist                       |
    Then the response status should be 201
    And modeling frequency should be tracked
    And student progress should be analyzed

  # GET /api/aac/students/{studentId}/vocabulary-development
  @endpoint @aac @vocabulary @not-implemented
  Scenario: Get AAC vocabulary development
    Given student "student-123" has been using AAC
    When I send a GET request to "/api/aac/students/student-123/vocabulary-development"
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | coreWordsUsed      | array  |
      | fringeExpansion    | object |
      | combinationLength  | number |
      | functionalUse      | object |
      | spontaneousUse     | number |
      | contextsUsed       | array  |

  # POST /api/aac/choice-making
  @endpoint @aac @choice-boards @not-implemented
  Scenario: Create choice making boards
    When I send a POST request to "/api/aac/choice-making" with:
      | field         | value                           |
      | studentId     | student-123                     |
      | choiceType    | preferred-activities            |
      | numChoices    | 4                               |
      | presentation  | visual-cards                    |
      | items         | ["iPad", "bubbles", "book", "puzzle"] |
      | responseMode  | eye-gaze                        |
    Then the response status should be 201
    And choice board should be generated
    And data collection sheet should be included

  # POST /api/aac/yes-no-assessment
  @endpoint @aac @yes-no @not-implemented
  Scenario: Assess yes/no response reliability
    When I send a POST request to "/api/aac/yes-no-assessment" with:
      | field           | value                         |
      | studentId       | student-123                   |
      | responseMethod  | head-movement                 |
      | knownQuestions  | [{"q": "Is your name [name]?", "expected": "yes", "response": "yes"}] |
      | unknownQuestions| [{"q": "Do you want to go home?", "response": "no"}] |
      | consistency     | 85                            |
    Then the response status should be 201
    And reliability score should be calculated
    And recommendations should be provided

  # GET /api/aac/resources/symbol-libraries
  @endpoint @aac @symbols @not-implemented
  Scenario: Access symbol libraries
    When I send a GET request to "/api/aac/resources/symbol-libraries"
    Then the response status should be 200
    And the response should contain:
      | field          | type  |
      | symbolstix     | object|
      | pcs            | object|
      | widgit         | object|
      | mulberry       | object|
      | arasaac        | object|
    And licensing information should be included

  # POST /api/aac/device-trial
  @endpoint @aac @high-tech @not-implemented
  Scenario: Document device trial
    When I send a POST request to "/api/aac/device-trial" with:
      | field          | value                        |
      | studentId      | student-123                  |
      | deviceName     | iPad with Proloquo2Go        |
      | trialStart     | 2024-01-22                   |
      | trialDuration  | 30-days                      |
      | goals          | ["request", "comment"]       |
      | trainingPlan   | {"week1": "navigation"}      |
    Then the response status should be 201
    And trial documentation should be created
    And data collection tools should be provided

  # GET /api/aac/students/{studentId}/communication-matrix
  @endpoint @aac @assessment-tools @not-implemented
  Scenario: Get communication matrix assessment
    Given student "student-123" has AAC assessment data
    When I send a GET request to "/api/aac/students/student-123/communication-matrix"
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | communicationLevel | string |
      | skillsByDomain     | object |
      | emergingSkills     | array  |
      | recommendations    | array  |
      | progressFromBaseline| object|

  # POST /api/aac/visual-scene-displays
  @endpoint @aac @vsd @not-implemented
  Scenario: Create visual scene display
    When I send a POST request to "/api/aac/visual-scene-displays" with:
      | field         | value                          |
      | studentId     | student-123                    |
      | scene         | playground                     |
      | hotspots      | [{"x": 100, "y": 200, "word": "swing"}] |
      | embedded      | ["go", "stop", "more", "help"] |
      | photograph    | playground.jpg                 |
    Then the response status should be 201
    And interactive VSD should be created
    And navigation supports should be added

  # POST /api/aac/language-samples
  @endpoint @aac @data @not-implemented
  Scenario: Record AAC language sample
    When I send a POST request to "/api/aac/language-samples" with:
      | field            | value                         |
      | studentId        | student-123                   |
      | duration         | 300                           |
      | context          | play                          |
      | utterances       | [{"message": ["I", "want", "more"], "function": "request"}] |
      | communicationMode| aided-aac                     |
      | partnerTurns     | 15                            |
      | studentTurns     | 12                            |
    Then the response status should be 201
    And MLU should be calculated
    And communication functions should be analyzed

  # FR-035 Missing Critical AAC Comprehensive Suite Business Workflow Scenarios
  @core-vocabulary @communication-boards @motor-planning @workflow @not-implemented
  Scenario: Create comprehensive core vocabulary board with customized motor planning
    Given I need to create a core vocabulary board for student "Maya"
    And Maya is at emerging communication level with limited fine motor skills
    When I design her personalized core vocabulary system
    And I select appropriate core vocabulary foundation:
      | Core Word Category   | Essential Words                     | Motor Position Priority |
      | People               | I, you, mom, dad                   | Top left (easiest access) |
      | Actions              | go, stop, want, help               | Center (frequent use)    |
      | Descriptors          | more, all done, big, little        | Right side (expansion)   |
      | Social               | hi, bye, please, thank you         | Bottom (social courtesy) |
      | Quick words          | yes, no, like, don't like          | Corner positions         |
    And I customize for Maya's specific needs:
      | Customization Factor | Adaptation Strategy                 |
      | Fine motor limitations| Larger symbols, increased spacing  |
      | Visual processing    | High contrast, clear borders       |
      | Cognitive level      | Consistent placement, familiar icons|
      | Family preferences   | Include family photos for people   |
      | Cultural considerations| Respect for cultural communication styles|
    Then the system should generate multiple board formats:
      | Board Format         | Features                            | Use Case                |
      | Print version        | High contrast, laminate-ready       | Backup and travel       |
      | Digital tablet version| Touch-accessible, audio feedback   | Primary communication   |
      | Ring binder flip book| Portable, organized by category     | Classroom/therapy use   |
      | Large wall display   | Group activities, partner modeling  | Classroom environment   |
    And motor planning should be optimized:
      | Motor Planning Element| Implementation                     |
      | Consistent placement | Same words in same location always |
      | Progressive complexity| Start with 4 words, expand gradually|
      | Access patterns      | Most frequent words in easiest reach|
      | Bilateral coordination| Consider dominant hand positioning |
    When implementing the core board system
    Then training protocol should include:
      | Training Component   | Implementation Strategy            |
      | Partner modeling     | Adults use board to communicate    |
      | Aided language input | Model target words during activities|
      | Motor practice       | Structured pointing and selection  |
      | Functional use       | Real communication opportunities   |
    And success should be measured by:
      | Success Metric       | Target Performance                 |
      | Symbol recognition   | 90% accuracy identifying core words|
      | Motor access         | Independent symbol selection       |
      | Functional use       | Spontaneous communication attempts |
      | Vocabulary growth    | Regular addition of new core words |

  @switch-access @scanning-patterns @physical-access @workflow @not-implemented
  Scenario: Configure comprehensive switch access system for scanning patterns
    Given student "Leo" has limited motor abilities requiring switch access
    And Leo has reliable head movement for single switch activation
    When I set up his comprehensive switch scanning system
    And I configure optimal scanning parameters:
      | Scanning Parameter   | Initial Setting                     | Rationale              |
      | Scan pattern         | Row-column with audio cues         | Reduces cognitive load |
      | Scan speed           | 3 seconds per item                 | Allows processing time |
      | Switch activation    | Single switch, head-activated      | Uses reliable movement |
      | Visual feedback      | Yellow border, 4px thick           | High contrast visibility|
      | Audio feedback       | Text-to-speech for each item       | Auditory confirmation  |
      | Auto-scan vs manual  | Auto-scan with pause option        | Reduces fatigue        |
    Then comprehensive switch training should address:
      | Training Area        | Specific Skills                     |
      | Cause and effect     | Switch press = action happens       |
      | Timing accuracy      | Press during target highlighting    |
      | Scanning patience    | Wait for desired item to be highlighted|
      | Error recovery       | Continue scanning after mistakes    |
    And activity adaptations should include:
      | Activity Type        | Switch Access Adaptation           |
      | Communication board  | Row-column scanning array          |
      | Interactive games    | Switch-activated choices           |
      | Educational software | Single-switch navigation           |
      | Environmental control| Device activation via switch       |
    When conducting switch access sessions
    Then data collection should track:
      | Performance Metric   | Measurement Method                 |
      | Switch accuracy      | Correct activations/total attempts |
      | Scan cycle efficiency| Items scanned before activation   |
      | Fatigue indicators   | Performance degradation over time  |
      | Communication success| Messages completed vs. attempted  |
    And systematic adjustments should optimize:
      | Adjustment Category  | Optimization Strategy              |
      | Scan speed           | Faster as accuracy improves        |
      | Array organization   | Most frequent items first          |
      | Rest break timing    | Prevent fatigue-related errors     |
      | Switch positioning   | Maximize comfort and reliability   |
    When Leo demonstrates competency
    Then advanced features should include:
      | Advanced Feature     | Implementation                     |
      | Prediction software  | Anticipate word completion         |
      | Context-aware arrays | Different boards for different activities|
      | Multi-level navigation| Nested categories for vocabulary  |
      | Communication history| Recent messages easily accessible |

  @partner-assisted-scanning @eye-gaze @low-tech-aac @workflow @not-implemented
  Scenario: Implement partner-assisted scanning with eye gaze communication
    Given student "Isabella" uses eye gaze for communication
    And she cannot access switches or touchscreens reliably
    When I establish partner-assisted scanning protocols
    And I create systematic communication materials:
      | Material Type        | Specifications                      | Partner Training Focus |
      | Choice arrays        | 2-4 items, consistent layout       | Steady presentation     |
      | Eye gaze boards      | High contrast, clear spacing        | Reading eye movements   |
      | Communication book   | Ring-bound, organized categories    | Page navigation         |
      | Partner cue cards    | "Look at what you want"            | Consistent prompting    |
    Then partner training should be comprehensive:
      | Training Component   | Partner Skill Development          |
      | Reading eye gaze     | Recognize sustained gaze patterns   |
      | Holding materials    | Steady, appropriate distance        |
      | Timing patience      | Allow processing time               |
      | Confirmation process | "You're looking at [item]?"        |
      | Response honoring    | Always provide chosen item/activity|
    And systematic scanning protocol should include:
      | Scanning Step        | Partner Action                      |
      | 1. Present array     | Hold at eye level, keep steady     |
      | 2. Give instruction  | "What do you want?" or similar      |
      | 3. Scan methodically | Point to each item systematically   |
      | 4. Watch for gaze    | Look for sustained eye contact      |
      | 5. Confirm selection | "You want [item]?"                  |
      | 6. Honor choice      | Provide item immediately            |
    When implementing partner-assisted scanning
    Then quality indicators should monitor:
      | Quality Indicator    | Standard                            |
      | Partner consistency  | Same scanning procedure every time  |
      | Gaze recognition     | Accurate reading of Isabella's intent|
      | Response timing      | 2-second sustained gaze minimum     |
      | Choice honoring      | 100% follow-through on selections  |
    And communication development should progress:
      | Development Stage    | Communication Expectations         |
      | Basic choice making  | Consistent selection from 2 items  |
      | Category navigation  | Choose activity type, then specifics|
      | Message building     | Combine multiple selections         |
      | Conversational turns | Back-and-forth exchanges           |
    When Isabella's skills advance
    Then system expansion should include:
      | Expansion Area       | Advanced Implementation            |
      | Larger arrays        | 6-8 items with systematic scanning |
      | Complex messages     | Multi-part communication sequences |
      | Social interactions  | Greetings, comments, questions     |
      | Academic participation| Answer questions, make choices     |

  @high-tech-device-support @speech-generating-devices @app-integration @workflow @not-implemented
  Scenario: Support high-tech AAC device users with comprehensive integration
    Given student "Noah" uses a speech-generating device (iPad with Proloquo2Go)
    And I need to support his device use with platform resources
    When I access comprehensive AAC device support materials
    Then I should find integrated resources that support his device:
      | Resource Type        | Device Integration Features        |
      | Core word activities | Match Proloquo2Go vocabulary layout|
      | Modeling videos      | Adults using same AAC app naturally|
      | Device overlays      | Visual guides for app navigation   |
      | Therapy activities   | Use device vocabulary in tasks     |
      | Partner training     | How to interact with AAC device users|
    And device-specific support should include:
      | Support Category     | Implementation                     |
      | Vocabulary consistency| Platform activities use same symbols|
      | Navigation practice  | Exercises for finding words quickly|
      | Message combination  | Activities promoting multi-word messages|
      | Device care          | Maintenance and troubleshooting tips|
    When creating therapy activities for Noah
    Then activities should seamlessly integrate his device:
      | Activity Integration | Implementation Strategy            |
      | Vocabulary targeting | Use words available in his device  |
      | Navigation practice  | Hide/reveal vocabulary for practice|
      | Speed building       | Timed activities for fluency       |
      | Social interaction   | Partner activities using device    |
    And device optimization should consider:
      | Optimization Area    | Strategy                           |
      | Vocabulary organization| Customize based on Noah's needs  |
      | Message banking      | Store frequently used phrases      |
      | Voice selection      | Age-appropriate voice options      |
      | Backup systems       | Low-tech alternatives available    |
    When supporting device generalization
    Then comprehensive planning should include:
      | Generalization Type  | Support Strategy                   |
      | Setting transfer     | Use device across environments     |
      | Partner training     | Teach others to interact appropriately|
      | Maintenance planning | Regular device updates and backups |
      | Skill advancement    | Progressive vocabulary expansion   |
    And troubleshooting support should address:
      | Common Issue         | Solution Strategy                  |
      | Device malfunction   | Backup communication systems       |
      | Vocabulary gaps      | Rapid addition of needed words     |
      | Speed concerns       | Efficiency training and shortcuts  |
      | Partner confusion    | Clear interaction guidelines       |

  @communication-functions @pragmatic-skills @social-interaction @workflow @not-implemented
  Scenario: Develop comprehensive communication functions beyond basic requesting
    Given AAC user "Emma" has mastered basic requesting
    And she needs to develop broader communication functions
    When I expand her communication function repertoire
    Then systematic function development should include:
      | Communication Function| Teaching Strategy                  | Context Examples       |
      | Requesting           | Established foundation             | "I want cookie"        |
      | Commenting           | Describe environment/activities    | "I see big dog"        |
      | Greeting/social      | Initiate and respond to others     | "Hi friend"            |
      | Questioning          | Ask for information                | "Where is mom?"        |
      | Rejecting/protesting | Express disagreement               | "No thank you"         |
      | Information sharing  | Tell about experiences             | "I went to park"       |
    And pragmatic skill development should address:
      | Pragmatic Area       | Teaching Focus                     |
      | Turn-taking          | Wait for partner response          |
      | Topic maintenance    | Stay on conversation subject       |
      | Repair strategies    | Fix communication breakdowns       |
      | Appropriate timing   | When to interrupt vs. wait         |
      | Partner awareness    | Adjust communication to listener   |
    When teaching commenting skills
    Then environmental setups should promote commenting:
      | Setup Type           | Communication Opportunities       |
      | Interesting visuals  | "I see..." statements              |
      | Novel experiences    | "This is..." descriptions          |
      | Changes in routine   | "Different today" observations     |
      | Peer interactions    | Social commenting about others     |
    And questioning skills should be developed through:
      | Question Type        | Teaching Method                    |
      | Yes/no questions     | Partner modeling and practice      |
      | Wh-questions         | Systematic introduction by type    |
      | Clarification        | "What?" when don't understand      |
      | Information seeking  | "Where?" "When?" for daily events  |
    When measuring communication function success
    Then assessment should evaluate:
      | Assessment Area      | Measurement Method                 |
      | Function frequency   | Count of each function type used   |
      | Function appropriateness| Match between context and function|
      | Spontaneous use      | Unprompted communication attempts  |
      | Conversational turns | Back-and-forth exchanges sustained |
    And intervention should systematically expand:
      | Expansion Phase      | Communication Goals                |
      | Phase 1              | Request + comment                  |
      | Phase 2              | Add greeting and rejection         |
      | Phase 3              | Include questioning                |
      | Phase 4              | Develop conversation skills        |

  @symbol-libraries @cultural-adaptation @multilingual-aac @workflow @not-implemented
  Scenario: Integrate comprehensive symbol libraries with cultural adaptation
    Given I work with diverse students requiring culturally appropriate AAC symbols
    When I access multiple symbol library systems
    Then I should have comprehensive symbol options:
      | Symbol Library       | Characteristics                    | Best Use Cases         |
      | SymbolStix           | Realistic, diverse representation  | Multicultural students |
      | PCS (Boardmaker)     | Traditional, widely recognized     | School-based programs  |
      | Widgit              | Simple, clear line drawings        | Cognitive support needs|
      | Mulberry Symbols     | Open source, customizable          | Budget-conscious settings|
      | ARASAAC             | Multilingual, free access          | Spanish-speaking students|
    And cultural adaptation should consider:
      | Cultural Factor      | Adaptation Strategy                |
      | Skin tone representation| Diverse figures in symbols       |
      | Family structures    | Extended family configurations     |
      | Food preferences     | Culturally relevant food symbols   |
      | Clothing styles      | Appropriate dress representations  |
      | Religious considerations| Respectful religious symbols     |
    When creating culturally responsive AAC materials
    Then customization should include:
      | Customization Type   | Implementation                     |
      | Photo integration    | Family photos for people/places    |
      | Community symbols    | Local landmarks and businesses     |
      | Cultural activities  | Tradition-specific events          |
      | Language integration | Bilingual text when appropriate    |
    And multilingual support should provide:
      | Language Support     | Feature                            |
      | Symbol consistency   | Same symbol across languages       |
      | Text alternatives    | Multiple language labels           |
      | Cultural context     | Appropriate social conventions     |
      | Family collaboration | Include family in symbol selection |
    When implementing culturally adapted systems
    Then quality assurance should verify:
      | Quality Check        | Verification Method                |
      | Cultural accuracy    | Community member review            |
      | Symbol appropriateness| Age and context suitability      |
      | Family acceptance    | Parent/guardian approval          |
      | Effectiveness        | Student engagement and use        |
    And ongoing maintenance should include:
      | Maintenance Activity | Implementation Schedule            |
      | Cultural review      | Annual symbol library updates     |
      | Family feedback      | Quarterly satisfaction surveys    |
      | Symbol updates       | Monthly new addition reviews      |
      | Usage analysis       | Data-driven symbol selection      |

  @aac-assessment @communication-matrix @comprehensive-evaluation @workflow @not-implemented
  Scenario: Conduct comprehensive AAC assessment using Communication Matrix
    Given I need to assess student "Marcus" for appropriate AAC interventions
    When I implement comprehensive AAC assessment protocol
    Then Communication Matrix assessment should evaluate:
      | Communication Level  | Skills Assessed                    | Evidence Required      |
      | Pre-intentional      | Behavioral states, reflexive responses| Observation data     |
      | Intentional          | Purposeful behavior for needs      | Partner recognition    |
      | Unconventional       | Informal gestures, vocalizations   | Function identification|
      | Conventional         | Recognized symbols, signs, words   | Symbol understanding   |
      | Abstract             | Text, complex symbol systems       | Academic integration   |
      | Formal language      | Grammar, syntax, conversation      | Linguistic competence  |
    And assessment should span multiple communication functions:
      | Function Category    | Specific Skills                    |
      | Unconditional communication| Refusal, social interaction     |
      | Acts on environment  | Request objects, actions           |
      | Requests attention   | Social attention, comfort          |
      | Provides information | Labeling, describing, questioning  |
    When conducting assessment across contexts
    Then evaluation should include:
      | Assessment Context   | Data Collection Method             |
      | Structured tasks     | Systematic symbol presentation     |
      | Natural interactions | Observation during daily activities|
      | Partner interviews   | Family and teacher reports         |
      | Environmental analysis| Communication opportunity mapping |
    And comprehensive results should generate:
      | Assessment Output    | Content                            |
      | Current abilities    | Matrix level and function summary  |
      | Communication needs  | Gap analysis for daily activities  |
      | AAC recommendations  | Specific system and vocabulary     |
      | Implementation plan  | Step-by-step intervention sequence |
    When developing AAC intervention plan
    Then systematic progression should address:
      | Intervention Phase   | Focus Areas                        |
      | Foundation building  | Cause-effect, intentional communication|
      | Symbol introduction  | Basic vocabulary and navigation    |
      | Function expansion   | Multiple communication purposes    |
      | Generalization       | Multiple partners and settings     |
    And progress monitoring should track:
      | Progress Indicator   | Measurement Method                 |
      | Matrix advancement   | Regular re-assessment              |
      | Function frequency   | Communication act counting         |
      | Partner effectiveness| Interaction quality ratings       |
      | Generalization success| Performance across contexts       |
    When reassessment indicates progress
    Then intervention updates should include:
      | Update Category      | Modification Strategy              |
      | Vocabulary expansion | Add new symbols based on needs     |
      | Technology advancement| Introduce higher-tech options     |
      | Function development | Target next communication level   |
      | Partner training     | Expand communication partner circle|

  @aac-implementation @team-coordination @systematic-intervention @workflow @not-implemented
  Scenario: Implement systematic AAC intervention with comprehensive team coordination
    Given student "Zara" requires comprehensive AAC system implementation
    And multiple team members need coordinated training
    When I establish systematic AAC implementation protocol
    Then team coordination should include all stakeholders:
      | Team Member Role     | Training Responsibilities          | Implementation Focus   |
      | Speech therapist     | Assessment, system selection       | Direct intervention    |
      | Classroom teacher    | Daily integration, curriculum alignment| Academic participation|
      | Paraprofessional     | Consistent modeling, data collection| Ongoing support       |
      | Family members       | Home implementation, generalization | Natural contexts      |
      | Peers (when appropriate)| Natural interaction modeling     | Social inclusion       |
    And systematic implementation should follow phases:
      | Implementation Phase | Duration | Focus Activities                  |
      | Assessment/Planning  | 2-4 weeks| Evaluation, system selection      |
      | Introduction/Training| 4-6 weeks| Initial teaching, partner training|
      | Skill Development    | 8-12 weeks| Vocabulary expansion, functions  |
      | Generalization      | Ongoing  | Multiple settings, maintenance    |
    When conducting team training
    Then comprehensive education should cover:
      | Training Topic       | Content Focus                      |
      | AAC principles       | Communication development theory   |
      | System operation     | Technical skills for chosen system|
      | Modeling techniques  | Aided language input strategies    |
      | Prompting hierarchy  | Least to most intrusive support    |
      | Data collection      | Progress monitoring methods        |
    And implementation fidelity should be monitored through:
      | Fidelity Check       | Method                             |
      | Direct observation   | Systematic observation protocols   |
      | Self-assessment      | Team member checklists            |
      | Student progress     | Communication data analysis       |
      | Partner feedback     | Regular team meetings             |
    When challenges arise during implementation
    Then problem-solving should address:
      | Challenge Type       | Solution Strategy                  |
      | Technical difficulties| Additional training, tech support |
      | Inconsistent use     | Simplify procedures, increase support|
      | Slow progress        | Adjust expectations, modify goals  |
      | Partner resistance   | Education, benefits demonstration  |
    And long-term success should ensure:
      | Success Factor       | Maintenance Strategy               |
      | Ongoing training     | Regular skill refreshers           |
      | System updates       | Technology and vocabulary evolution|
      | Team communication   | Monthly progress meetings          |
      | Family support       | Continuous home-school collaboration|