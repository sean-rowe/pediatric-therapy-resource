Feature: Complete 6-Phase PECS Protocol Implementation
  As a therapy professional implementing PECS
  I want comprehensive support for all 6 phases of PECS
  So that I can effectively teach functional communication using the Picture Exchange Communication System

  Background:
    Given PECS protocol system is configured
    And all six phases are available
    And reinforcer assessment tools are ready
    And communication book templates exist
    And data collection sheets are integrated

  # Phase 1: Physical Exchange
  @specialized @pecs @phase1 @critical @not-implemented
  Scenario: Implement Phase 1 physical exchange training
    Given I am starting PECS with a new student
    And reinforcer assessment has been completed
    When implementing Phase 1 training:
      | Student ID | Preferred Items      | Exchange Success | Prompting Level | Sessions | Independence % | Notes                           |
      | S001      | Goldfish, iPad      | 15/20           | Full physical   | 5        | 25%           | Requires hand-over-hand         |
      | S002      | Bubbles, Music      | 18/20           | Partial physical| 8        | 45%           | Fading prompts successfully     |
      | S003      | Toy car, Snacks     | 20/20           | No prompt      | 12       | 85%           | Ready for Phase 2              |
      | S004      | Playdough, Books    | 12/20           | Full physical   | 3        | 15%           | Still learning exchange         |
      | S005      | Blocks, Juice       | 16/20           | Gestural       | 6        | 55%           | Inconsistent initiation        |
      | S006      | Puzzle, Crackers    | 19/20           | Shadow         | 10       | 75%           | Nearly independent             |
    Then Phase 1 criteria should be tracked
    And two-person prompting should be documented
    And exchange data should show progress
    And readiness for Phase 2 should be assessed

  @specialized @pecs @phase2 @high @not-implemented
  Scenario: Progress through Phase 2 distance and persistence
    Given student has mastered Phase 1 exchanges
    And Phase 2 materials are prepared
    When training Phase 2 skills:
      | Skill Component        | Starting Distance | Current Distance | Partner Distance | Persistence Trials | Success Rate | Next Goal               |
      | Travel to book        | 1 foot           | 5 feet          | 3 feet          | 8/10             | 80%         | Increase to 10 feet     |
      | Travel to partner     | 2 feet           | 8 feet          | Stationary      | 9/10             | 90%         | Add obstacles           |
      | Remove picture        | With help        | Independent     | N/A             | 10/10            | 100%        | Maintain skill          |
      | Navigate barriers     | None             | 1 barrier       | Behind barrier  | 6/10             | 60%         | Multiple barriers       |
      | Multiple partners     | 1 partner        | 3 partners      | Various         | 7/10             | 70%         | Generalize further      |
      | Different settings    | Therapy room     | 3 locations     | Mobile          | 8/10             | 80%         | Natural environments    |
    Then distance achievements should be recorded
    And persistence should improve
    And independence should increase
    And Phase 3 readiness should be evaluated

  @specialized @pecs @phase3 @high @not-implemented
  Scenario: Master Phase 3A and 3B picture discrimination
    Given student reliably travels and exchanges
    And discrimination training begins
    When implementing Phase 3A (simple discrimination):
      | Trial Type             | Preferred Item   | Distractor Item | Correct Choice | Error Correction | Correspondence | Mastery Status |
      | Highly preferred vs neutral | Cookie      | Sock           | 10/10         | N/A             | 100%          | Mastered      |
      | Preferred vs non      | Juice           | Vegetable       | 9/10          | 1 trial         | 100%          | Mastered      |
      | Two preferred         | iPad            | Bubbles         | 8/10          | 2 trials        | 90%           | Emerging      |
      | Similar items         | Crackers        | Chips           | 7/10          | 3 trials        | 85%           | Practice needed|
      | Neutral items         | Book            | Paper           | 6/10          | 4 trials        | 80%           | More practice |
      | Abstract concepts     | Play            | Work            | 5/10          | 5 trials        | 70%           | Challenging   |
    When progressing to Phase 3B (multiple pictures):
      | Array Size | Picture Categories | Scanning Pattern | Accurate Selection | Organization    | Independence  |
      | 3 pictures | Food items        | Left to right    | 85%               | By category     | 90%          |
      | 5 pictures | Mixed categories  | Systematic       | 80%               | By preference   | 85%          |
      | 8 pictures | All categories    | Efficient        | 75%               | Categorical     | 80%          |
      | 12 pictures| Comprehensive     | Quick scan       | 70%               | Alphabetical    | 75%          |
      | 20 pictures| Full vocabulary   | Organized        | 65%               | Frequency-based | 70%          |
      | Book format| Multiple pages    | Page turning     | 60%               | Topic-based     | 65%          |
    Then discrimination accuracy should improve
    And correspondence checks should confirm understanding
    And error correction should be systematic
    And vocabulary should expand appropriately

  @specialized @pecs @phase4 @critical @not-implemented
  Scenario: Build Phase 4 sentence structure with "I want"
    Given student discriminates between multiple pictures
    And sentence strip is introduced
    When teaching Phase 4 sentence building:
      | Sentence Component | Teaching Step     | Student Performance | Prompt Level    | Generalization | Mastery   |
      | "I want" card     | Introduction      | Places correctly   | Model           | 3 settings     | 85%       |
      | Picture placement | After "I want"    | Sequences properly | Gestural        | 5 partners     | 90%       |
      | Strip exchange    | Complete sentence | Delivers strip     | Verbal reminder | Multiple items | 80%       |
      | Point and read    | Therapist models  | Points to each     | Physical guide  | Self-corrects  | 75%       |
      | Rebuild sentence  | After exchange    | Replaces cards     | Independent     | Maintains book | 95%       |
      | Multiple requests | Various items     | Builds new sentences| Independent    | Throughout day | 85%       |
    Then sentence structure should become automatic
    And word order should be consistent
    And communication should be more complex
    And spontaneous use should increase

  @specialized @pecs @phase5 @medium @not-implemented
  Scenario: Develop Phase 5 responsive requesting
    Given student uses "I want" sentences independently
    And responsive communication is targeted
    When teaching response to "What do you want?":
      | Question Format        | Response Time | Accuracy | Spontaneous Request | Adult Wait Time | Progress     |
      | Direct question       | 3 seconds     | 90%      | Before question 40% | 5 seconds      | Excellent    |
      | Indirect prompt       | 5 seconds     | 85%      | Before prompt 35%   | 8 seconds      | Very good    |
      | Expectant look only   | 8 seconds     | 75%      | Initiated 30%       | 10 seconds     | Good         |
      | Natural opportunity   | 10 seconds    | 70%      | Initiated 45%       | 15 seconds     | Improving    |
      | No prompt given       | Variable      | N/A      | Initiated 60%       | Natural pause  | Target met   |
      | Across environments   | 5 seconds avg | 80%      | Initiated 50%       | Appropriate    | Generalized  |
    Then responsive requesting should develop
    And spontaneous communication should increase
    And prompt dependency should decrease
    And functional communication should strengthen

  @specialized @pecs @phase6 @high @not-implemented
  Scenario: Expand Phase 6 commenting and attributes
    Given student masters responsive requesting
    And commenting function is introduced
    When teaching Phase 6 expansions:
      | Communication Function | Carrier Phrase | Attributes Added    | Context Used        | Accuracy | Spontaneous Use |
      | Commenting            | "I see"        | Colors, sizes      | Natural observation | 75%      | 20%            |
      | Describing            | "I have"       | Adjectives         | Show and tell       | 80%      | 25%            |
      | Answering questions   | "It is"        | Properties         | Direct questions    | 85%      | 15%            |
      | Social comments       | "I like"       | Preferences        | Social situations   | 70%      | 30%            |
      | Negation             | "I don't want" | Rejection          | Non-preferred items | 90%      | 40%            |
      | Multiple attributes   | Various        | Size+color+item    | Complex descriptions| 65%      | 10%            |
    Then commenting skills should emerge
    And vocabulary should be rich and varied
    And social communication should develop
    And conversation skills should begin

  # PECS Data Tracking and Progress Monitoring
  @specialized @pecs @data-tracking @critical @not-implemented
  Scenario: Track comprehensive PECS progress data
    Given PECS implementation requires detailed tracking
    And data drives phase decisions
    When monitoring PECS progress:
      | Student ID | Current Phase | Days in Phase | Exchanges/Day | Vocabulary Size | Spontaneous % | Next Decision Point |
      | S001      | Phase 1       | 10           | 25            | 1              | 5%           | Increase trials     |
      | S002      | Phase 2       | 15           | 40            | 1              | 10%          | Begin Phase 3       |
      | S003      | Phase 3B      | 25           | 60            | 15             | 25%          | Ready for Phase 4   |
      | S004      | Phase 4       | 30           | 80            | 25             | 40%          | Solidify sentences  |
      | S005      | Phase 5       | 20           | 100           | 40             | 60%          | Introduce Phase 6   |
      | S006      | Phase 6       | 45           | 120           | 75             | 70%          | Expand attributes   |
    Then progress should be data-driven
    And phase transitions should be justified
    And individualization should be evident
    And outcomes should be measurable

  @specialized @pecs @generalization @high @not-implemented
  Scenario: Ensure PECS generalization across settings
    Given PECS skills are established in therapy
    And generalization is critical for success
    When planning generalization:
      | Setting         | Communication Partners | Materials Available | Opportunities/Day | Success Rate | Support Needed    |
      | Classroom       | Teacher, Aide, Peers  | Full book          | 50+             | 85%         | Minimal prompts   |
      | Cafeteria       | Staff, Peers          | Mini book          | 10              | 75%         | Environmental setup|
      | Home            | Parents, Siblings     | Home book          | 100+            | 90%         | Parent training   |
      | Community       | Various adults        | Travel book        | 20              | 65%         | Pre-planning      |
      | Playground      | Peers, Supervisors    | Core board backup  | 30              | 70%         | Peer training     |
      | Specials        | Art, Music, PE staff  | Portable strips    | 15              | 60%         | Staff orientation |
    Then generalization data should be collected
    And communication partners should be trained
    And materials should be available everywhere
    And functional use should be prioritized

  @specialized @pecs @troubleshooting @medium @not-implemented
  Scenario: Address common PECS implementation challenges
    Given PECS implementation may face obstacles
    When encountering challenges:
      | Challenge Type          | Specific Issue         | Intervention Strategy      | Outcome Measure     | Success Indicator |
      | Low motivation         | Limited interests      | Expand reinforcer menu     | Exchanges increase  | 50% improvement   |
      | Physical limitations   | Motor difficulties     | Adapt picture size/velcro  | Independent use     | Accommodation works|
      | Behavior interfering   | Grabbing items        | Teach waiting, use timer   | Appropriate requests| 80% reduction     |
      | Picture confusion      | Similar images        | Enhance distinctiveness    | Discrimination accuracy| 90% correct    |
      | Book management        | Loses pictures        | Organization system        | Book intact         | Materials maintained|
      | Prompt dependency      | Waits for cues        | Fade prompts systematically| Spontaneous use    | 40% initiations  |
    Then solutions should be individualized
    And data should guide decisions
    And Team collaboration should occur
    And Progress should resume

  # PECS Fidelity and Quality Assurance
  @specialized @pecs @fidelity @critical @not-implemented
  Scenario: Maintain PECS implementation fidelity
    Given PECS requires specific procedures
    And fidelity ensures effectiveness
    When monitoring implementation quality:
      | Fidelity Component      | Target Standard        | Current Performance | Action Needed      | Review Schedule   |
      | Two-person prompting    | 100% in Phase 1       | 95%                | Refresher training | Weekly           |
      | Error correction        | 4-step procedure      | Consistent 90%     | Video review       | Bi-weekly        |
      | Reinforcer delivery     | Immediate (<3 sec)    | 98% compliance     | Maintain standard  | Monthly          |
      | Communication temptations| Natural throughout day| 75% opportunities  | Increase setups    | Daily planning   |
      | Data collection         | Every exchange        | 85% recorded       | Simplify system    | Daily            |
      | Phase criteria          | 80% over 3 days      | Properly applied   | Continue monitoring| Per phase        |
    Then fidelity checks should be regular
    And training should be ongoing
    And Quality should be maintained
    And Outcomes should improve

  # Error Handling and Special Considerations
  @specialized @pecs @error @special-needs @not-implemented
  Scenario: Adapt PECS for special circumstances
    Given some students have additional challenges
    When implementing PECS with modifications:
      | Special Need           | PECS Adaptation         | Additional Support    | Success Measure    | Outcome          |
      | Visual impairment     | Tactile symbols         | Object schedule      | Touch discrimination| Functional system|
      | Hearing + ASD         | Visual emphasis only    | Clear sight lines    | Exchange success   | Effective        |
      | Physical disability   | Switch for exchange     | Scanning option      | Independent request| Accessible       |
      | Cognitive delay       | Simplified phases       | Smaller steps        | Gradual progress   | Achievable       |
      | Dual language         | Bilingual cards        | Cultural symbols     | Both languages used| Inclusive        |
      | Medical complexity    | Bedside setup          | Portable system      | Maintains skills   | Continuous       |
    Then adaptations should maintain PECS principles
    And functional communication should be achieved
    And Individual needs should be met
    And Progress should be documented

  @specialized @pecs @error @regression @not-implemented
  Scenario: Handle PECS skill regression
    Given students may lose previously mastered skills
    When regression occurs:
      | Regression Type        | Possible Cause         | Intervention          | Recovery Time      | Prevention Plan   |
      | Phase drop-back       | Extended break         | Intensive practice    | 1-2 weeks         | Maintenance plan  |
      | Discrimination errors | New pictures added     | Systematic review     | 3-5 days          | Gradual expansion |
      | Reduced spontaneity   | Over-prompting         | Fade prompts again    | 1 week            | Monitor independence|
      | Exchange breakdown    | Environmental change   | Re-establish routine  | 2-3 days          | Transition planning|
      | Motivation loss       | Reinforcer satiation   | New assessment        | Immediate         | Regular updates   |
      | Partner dependence    | Limited generalization | Expand partners       | 2 weeks           | Systematic plan   |
    Then regression should be addressed quickly
    And root causes should be identified
    And Skills should be rebuilt
    And Future regression should be prevented

  @specialized @pecs @error @integration @not-implemented
  Scenario: Integrate PECS with other communication systems
    Given students may use multiple communication modes
    When coordinating PECS with other systems:
      | Other System          | Integration Strategy    | Coordination Plan     | Expected Benefit   | Monitoring Plan   |
      | Speech attempts       | Honor all attempts     | PECS + verbal model   | Total communication| Track both modes  |
      | Sign language        | Teach alongside PECS   | Sign + picture       | Multimodal options | Use assessment    |
      | AAC device           | PECS as bridge         | Transition plan      | High-tech readiness| Gradual shift     |
      | Written words        | Add text to pictures   | Literacy development | Reading preparation| Academic tracking |
      | Gesture/pointing     | Shape to PECS          | Systematic transfer  | Clear communication| Fade gestures     |
      | PODD books          | Combine strategies     | Best of both         | Comprehensive system| Team decision     |
    Then multiple modes should be coordinated
    And confusion should be minimized
    And Strongest system should emerge
    And Communication should be maximized