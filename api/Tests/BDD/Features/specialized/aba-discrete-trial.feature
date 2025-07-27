Feature: ABA Discrete Trial Training Implementation
  As an ABA practitioner or BCBA
  I want comprehensive discrete trial training support
  So that I can implement systematic instruction with precise data collection

  Background:
    Given ABA discrete trial system is configured
    And DTT protocols are evidence-based
    And trial structure is standardized
    And data collection is automated
    And prompt hierarchies are established

  # Core DTT Implementation
  @specialized @aba @discrete-trial @dtt-structure @critical @not-implemented
  Scenario: Implement systematic discrete trial training structure
    Given I am conducting DTT sessions
    And targets are clearly defined
    When implementing discrete trial structure:
      | Program Area | Target Behavior | Discriminative Stimulus | Student Response | Consequence | Inter-trial Interval | Trial Count | Accuracy Rate |
      | Receptive ID | Touch nose | "Touch your nose" | Touches nose correctly | "Great job!" + reinforcer | 3 seconds | 10 trials | 8/10 (80%) |
      | Expressive ID | Label apple | Shows apple picture | Says "apple" | "Yes, apple!" + reinforcer | 3 seconds | 10 trials | 7/10 (70%) |
      | Matching | Match colors | "Match red" | Places red with red | "Perfect match!" + reinforcer | 2 seconds | 10 trials | 9/10 (90%) |
      | Imitation | Wave hand | Therapist waves | Student waves back | "Nice waving!" + reinforcer | 3 seconds | 10 trials | 6/10 (60%) |
      | Academic | Count to 5 | "Count to 5" | Says "1,2,3,4,5" | "Excellent counting!" + reinforcer | 4 seconds | 8 trials | 5/8 (63%) |
      | Self-care | Brush teeth | "Brush your teeth" | Demonstrates brushing | "Good brushing!" + reinforcer | 5 seconds | 6 trials | 4/6 (67%) |
    Then trial structure should be consistent
    And data should be collected accurately
    And reinforcement should be immediate
    And inter-trial intervals should be maintained

  @specialized @aba @discrete-trial @prompt-hierarchy @critical @not-implemented
  Scenario: Implement systematic prompt hierarchy and fading
    Given prompting supports learning
    And prompts must be faded systematically
    When implementing prompt hierarchy:
      | Target Skill | Prompt Level | Prompt Type | Student Response | Independence Level | Prompt Fading Plan | Success Criteria |
      | Point to red | Most-to-least | Full physical guidance | 100% accuracy | 0% independent | Reduce physical contact | 3 consecutive sessions 80%+ |
      | Say "hello" | Most-to-least | Full verbal model | 95% accuracy | 5% independent | Fade to partial verbal | 80% with partial prompt |
      | Sort shapes | Least-to-most | No prompt first | 20% accuracy | 20% independent | Add gestural if needed | Independent 70% accuracy |
      | Write name | Graduated guidance | Hand-over-hand writing | 85% accuracy | 15% independent | Gradual hand withdrawal | Write with light touch |
      | Follow 2-step | Time delay | 5-second delay | 40% accuracy | 40% independent | Increase delay to 10 sec | 60% within 10-second delay |
      | Label emotions | Simultaneous prompting | Immediate full prompt | 100% accuracy | 0% independent | Introduce prompt delay | 50% before prompt |
    Then prompts should be applied consistently
    And fading should be systematic
    And independence should increase
    And data should track prompt levels

  @specialized @aba @discrete-trial @mastery-criteria @high @not-implemented
  Scenario: Define and track mastery criteria systematically
    Given mastery criteria must be objective
    And progression depends on mastery
    When tracking mastery across programs:
      | Program | Target | Mastery Criteria | Current Performance | Sessions at Criteria | Consecutive Days | Mastery Status | Next Steps |
      | Receptive Language | Body parts (10 items) | 90% accuracy over 3 sessions | 92%, 88%, 94% | 3 sessions | 3 consecutive | Mastered | Expand to clothing items |
      | Expressive Language | Common objects (15 items) | 80% accuracy over 2 sessions | 85%, 82% | 2 sessions | 2 consecutive | Mastered | Add action words |
      | Visual Matching | Colors (8 items) | 85% accuracy over 2 sessions | 90%, 75% | 1 session | Not consecutive | In progress | Continue training |
      | Motor Imitation | Gross motor (12 actions) | 75% accuracy over 3 sessions | 80%, 70%, 72% | 1 session | Not consecutive | In progress | Focus on difficult actions |
      | Academic Skills | Numbers 1-10 | 90% accuracy over 3 sessions | 85%, 88%, 91% | 1 session | 1 day | Emerging | Continue current level |
      | Social Skills | Greetings (5 contexts) | 70% accuracy over 5 sessions | 65%, 68%, 72%, 75%, 73% | 3 sessions | 3 consecutive | Mastered | Generalize across people |
    Then mastery criteria should be evidence-based
    And tracking should be systematic
    And progression should be data-driven
    And generalization should follow mastery

  @specialized @aba @discrete-trial @error-correction @high @not-implemented
  Scenario: Implement systematic error correction procedures
    Given errors are learning opportunities
    And correction must be immediate and consistent
    When implementing error correction:
      | Error Type | Incorrect Response | Correction Procedure | Re-teaching Strategy | Success After Correction | Prevention Strategy |
      | Wrong selection | Points to blue instead of red | "No, try again" + physical guidance to red | Repeat trial with prompt | 85% success | Increase prompting |
      | No response | Student doesn't respond in 5 seconds | Prompt immediately, then repeat | Model correct response | 90% success | Reduce wait time |
      | Partial response | Says "ap" instead of "apple" | "Apple" + have student repeat | Break down syllables | 75% success | Syllable practice |
      | Prompt dependency | Always waits for prompt | Delay prompt, encourage independence | Gradually increase wait time | 60% success | Confidence building |
      | Attention issues | Looks away during instruction | Gain attention, re-present | Visual/gestural cues | 80% success | Environmental changes |
      | Motor difficulties | Can't perform action | Physical assistance + practice | Break into smaller steps | 70% success | Task analysis |
    Then error correction should be immediate
    And procedures should be consistent
    And learning should be facilitated
    And independence should be encouraged

  # Advanced DTT Features
  @specialized @aba @discrete-trial @randomization @medium @not-implemented
  Scenario: Implement trial randomization and rotation
    Given randomization prevents pattern learning
    And rotation maintains engagement
    When implementing trial randomization:
      | Randomization Type | Implementation Method | Program Areas Affected | Randomization Schedule | Student Response | Learning Benefits |
      | Target rotation | Random order within session | All receptive programs | Every 3-5 trials | Maintained attention | Prevents position bias |
      | Distractor variation | Different wrong choices | Discrimination programs | Each trial | Improved discrimination | True understanding |
      | Reinforcer rotation | Different rewards | All programs | Every 10 minutes | Sustained motivation | Prevents satiation |
      | Prompt timing | Variable delay intervals | Independence targets | Random within range | Increased spontaneity | Natural responding |
      | Trial pacing | Varied inter-trial intervals | All programs | 2-5 second range | Reduced predictability | Active engagement |
      | Material presentation | Different exemplars | Generalization targets | Every session | Broader learning | True generalization |
    Then randomization should be systematic
    And learning should be enhanced
    And patterns should be prevented
    And engagement should be maintained

  @specialized @aba @discrete-trial @generalization @critical @not-implemented
  Scenario: Promote generalization across conditions
    Given skills must transfer beyond training
    And generalization is the ultimate goal
    When implementing generalization strategies:
      | Generalization Type | Training Modification | Assessment Method | Success Criteria | Transfer Evidence | Maintenance Plan |
      | Across people | Multiple trainers | Novel therapist probes | 70% with new person | Skills demonstrated | Regular novel person probes |
      | Across settings | Multiple locations | Different room trials | 65% in new setting | Location independence | Setting rotation |
      | Across materials | Various exemplars | Novel item presentations | 60% with new materials | Concept understanding | Material variety |
      | Across time | Distributed practice | Retention probes | 75% after 1 week | Skill maintenance | Spaced practice |
      | Across responses | Response variations | Functional equivalents | 50% novel responses | Flexible responding | Response class training |
      | Natural contexts | Embedded opportunities | Real-world observations | 40% in natural settings | Functional use | Environmental programming |
    Then generalization should be programmed
    And assessment should be comprehensive
    And transfer should be documented
    And maintenance should be planned

  @specialized @aba @discrete-trial @motivation @high @not-implemented
  Scenario: Optimize motivation and reinforcement systems
    Given motivation drives learning
    And reinforcement must be individualized
    When optimizing motivation systems:
      | Motivation Strategy | Implementation | Effectiveness Measure | Student Response | Adjustment Needed | Outcome Impact |
      | Preference assessment | Weekly choice sampling | Selection consistency | High engagement | None | Sustained effort |
      | Reinforcer variety | 5 different rewards | Maintains responding | Variable interest | Rotate more frequently | Improved performance |
      | Schedule thinning | Reduce reinforcement rate | Response maintenance | Slight decrease | Thin more gradually | Maintained motivation |
      | Natural reinforcement | Functional consequences | Intrinsic motivation | Increased independence | Continue strategy | Enhanced learning |
      | Token systems | Delayed gratification | Goal achievement | Good understanding | Increase token value | Better self-regulation |
      | Choice opportunities | Student selects activity | Autonomy demonstration | High compliance | Expand choices | Increased cooperation |
    Then motivation should be optimized
    And reinforcement should be effective
    And systems should be sustainable
    And learning should be enhanced

  # Data Collection and Analysis
  @specialized @aba @discrete-trial @data-precision @critical @not-implemented
  Scenario: Collect precise trial-by-trial data
    Given DTT requires precise data
    And decisions depend on accurate information
    When collecting trial-by-trial data:
      | Data Element | Recording Method | Accuracy Standard | Collection Frequency | Analysis Schedule | Decision Criteria |
      | Response accuracy | +/- per trial | 100% inter-observer agreement | Every trial | Daily session summary | <70% = program modification |
      | Prompt level | P1-P5 code per trial | 95% agreement | Every trial | Weekly prompt analysis | Prompt dependency = fading plan |
      | Response latency | Stopwatch timing | ±0.5 second accuracy | Selected trials | Bi-weekly fluency review | >5 seconds = fluency training |
      | Problem behaviors | Frequency count | 90% agreement | Continuous | Daily behavior summary | Increase = intervention needed |
      | Reinforcement delivery | Immediate/delayed coding | 100% accuracy | Every trial | Weekly reinforcement review | Delays = procedure training |
      | Environmental notes | Narrative recording | Relevant details captured | As needed | Weekly environmental review | Patterns = environmental changes |
    Then data should be collected accurately
    And reliability should be maintained
    And analysis should be timely
    And decisions should be data-driven

  @specialized @aba @discrete-trial @progress-monitoring @high @not-implemented
  Scenario: Monitor progress and make data-driven decisions
    Given progress monitoring guides instruction
    And data drives program modifications
    When monitoring DTT progress:
      | Monitoring Period | Data Analysis | Performance Trend | Decision Made | Program Modification | Expected Outcome |
      | Week 1 | 85% accuracy maintained | Stable performance | Continue program | No changes | Maintain performance |
      | Week 2 | Dropping to 70% accuracy | Declining trend | Investigate causes | Check reinforcers | Improved performance |
      | Week 3 | Accuracy at 55% | Significant decline | Emergency review | Reduce difficulty | Immediate improvement |
      | Week 4 | Recovery to 75% | Improving trend | Gradual advancement | Increase criteria slightly | Continued growth |
      | Week 5 | Plateau at 80% | Stable but not progressing | Strategy change | Add prompt fading | Breakthrough progress |
      | Week 6 | Achieving 90%+ | Excellent progress | Advance to next level | Introduce new targets | Skill expansion |
    Then monitoring should be continuous
    And decisions should be timely
    And modifications should be systematic
    And outcomes should improve

  # Error Handling and Troubleshooting
  @specialized @aba @discrete-trial @error @learning-plateaus @medium @not-implemented
  Scenario: Address learning plateaus and lack of progress
    Given learning plateaus are common
    When progress stalls:
      | Plateau Indicator | Duration | Possible Causes | Assessment Strategy | Intervention Plan | Success Criteria |
      | No accuracy improvement | 2 weeks | Task too difficult | Break into smaller steps | Task analysis | 10% improvement |
      | High prompt dependency | 3 weeks | Prompts too intrusive | Assess prompt levels | Systematic fading | 20% reduction |
      | Motivation decline | 1 week | Reinforcer satiation | Preference reassessment | New reinforcers | Renewed engagement |
      | Inconsistent responding | 2 weeks | Environmental distractions | Environmental analysis | Setting modifications | Stable responding |
      | Regression in skills | 1 week | Extended break/illness | Skill reassessment | Review and practice | Return to baseline |
      | Behavioral interference | Ongoing | Competing behaviors | Functional assessment | Behavior intervention | Reduced interference |
    Then plateaus should be addressed systematically
    And causes should be identified
    And interventions should be evidence-based
    And progress should resume

  @specialized @aba @discrete-trial @error @data-reliability @critical @not-implemented
  Scenario: Ensure data collection reliability
    Given reliable data is essential
    When data reliability is questioned:
      | Reliability Issue | Detection Method | Impact Assessment | Correction Strategy | Quality Assurance | Prevention Plan |
      | Observer bias | Inter-observer checks | Inflated accuracy scores | Blind observers | Monthly reliability checks | Observer training |
      | Inconsistent timing | Video review | Variable latency data | Standardized procedures | Timer training | Timing protocols |
      | Recording errors | Data audit | Missing/incorrect data | Double-check systems | Daily data review | Recording training |
      | Environmental inconsistency | Session notes review | Variable performance | Environmental standardization | Setting checklists | Environment protocols |
      | Student variables | Performance correlation | Unexplained fluctuations | Health/motivation tracking | Holistic monitoring | Comprehensive assessment |
      | Technology failures | System monitoring | Data loss | Backup systems | Redundant recording | Technology maintenance |
    Then reliability should be monitored
    And issues should be addressed immediately
    And quality should be maintained
    And trust should be preserved

  @specialized @aba @discrete-trial @sustainability @high @not-implemented
  Scenario: Ensure sustainable DTT implementation
    Given DTT programs must be maintainable
    When planning for sustainability:
      | Sustainability Factor | Current Challenge | Sustainability Strategy | Resource Requirements | Success Indicators | Long-term Viability |
      | Staff training | High turnover | Comprehensive training program | Training materials/time | Consistent implementation | Maintained quality |
      | Data management | Time-intensive | Automated data systems | Technology investment | Efficient data collection | Reduced burden |
      | Program fidelity | Implementation drift | Regular fidelity checks | Monitoring systems | 85%+ fidelity maintained | Quality assurance |
      | Student progress | Variable outcomes | Evidence-based practices | Ongoing professional development | Improved outcomes | Better results |
      | Family involvement | Limited engagement | Family training programs | Family education resources | Active participation | Home generalization |
      | Cost effectiveness | High resource needs | Efficient program design | Streamlined procedures | Reduced cost per hour | Economic viability |
    Then sustainability should be planned
    And systems should be efficient
    And quality should be maintained
    And outcomes should be sustained