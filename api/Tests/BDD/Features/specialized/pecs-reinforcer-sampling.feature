Feature: Comprehensive PECS Reinforcer Assessment and Sampling
  As a therapy professional implementing PECS
  I want comprehensive reinforcer assessment tools
  So that I can identify highly motivating items to ensure PECS success

  Background:
    Given PECS reinforcer assessment system is configured
    And assessment protocols are evidence-based
    And data collection tools are integrated
    And preference hierarchies can be established
    And reinforcer effectiveness can be tracked

  # Core Reinforcer Assessment Workflows
  @specialized @pecs @reinforcer-assessment @critical @not-implemented
  Scenario: Conduct systematic reinforcer sampling assessment
    Given I am preparing to start PECS with a new student
    And comprehensive reinforcer identification is required
    When conducting reinforcer sampling assessment:
      | Assessment Phase | Items Tested           | Approach Used    | Duration | Student Response | Engagement Level | Preference Rank | Notes                          |
      | Initial screen   | Food items (10)        | Free access      | 10 min   | Chose 7/10      | High            | 1-7            | Strong food motivation         |
      | Initial screen   | Toys/activities (15)   | Free access      | 15 min   | Chose 9/15      | Moderate        | 8-16           | Variable interest              |
      | Initial screen   | Sensory items (8)      | Free access      | 8 min    | Chose 3/8       | Low             | 17-19          | Limited sensory seeking        |
      | Initial screen   | Social activities (5)  | Offered          | 5 min    | Chose 2/5       | High            | 20-21          | Enjoys interaction             |
      | Paired choice    | Top 10 vs each other   | Systematic pairs | 20 min   | Clear choices   | Sustained       | Final ranking   | Goldfish crackers #1           |
      | Multiple stimulus| Top 5 in array         | MSW assessment   | 15 min   | Consistent      | High            | Validated       | iPad games strong second       |
    Then preference hierarchy should be established
    And motivating items should be clearly identified
    And assessment reliability should be confirmed
    And PECS readiness should be evaluated

  @specialized @pecs @reinforcer-assessment @preference-hierarchy @high @not-implemented
  Scenario: Establish detailed preference hierarchy through structured assessment
    Given multiple potential reinforcers have been identified
    And systematic ranking is needed
    When establishing preference hierarchy:
      | Ranking Method    | Items Compared | Trials Per Pair | Student Choice Pattern | Hierarchy Position | Reliability Score | Assessment Confidence |
      | Paired choice     | Food vs Food   | 3 trials       | 85% consistency       | Goldfish #1       | 0.92             | Very high            |
      | Paired choice     | Food vs Toy    | 3 trials       | 90% consistency       | Food wins         | 0.94             | Very high            |
      | Paired choice     | Toy vs Toy     | 3 trials       | 70% consistency       | iPad #2           | 0.75             | Moderate             |
      | Multiple stimulus | Top 5 array    | 5 trials       | 80% consistency       | Stable ranking    | 0.88             | High                 |
      | Multiple stimulus | Top 3 focus    | 3 trials       | 95% consistency       | Clear preferences | 0.97             | Very high            |
      | Restriction test  | Remove #1      | 2 trials       | Strong protest        | Confirmed #1      | 1.0              | Absolute             |
    Then preference ranking should be reliable
    And assessment data should support PECS implementation
    And backup reinforcers should be identified
    And reassessment schedule should be planned

  @specialized @pecs @reinforcer-assessment @developmental-considerations @high @not-implemented
  Scenario: Adapt reinforcer assessment for different developmental levels
    Given students have varying developmental levels
    And assessment must be appropriate
    When adapting assessment for development:
      | Developmental Level | Age Range | Assessment Adaptations     | Items Focus          | Trial Duration | Response Mode    | Success Indicators        |
      | Early intervention  | 18m-3yr   | Shorter trials, parent input| Primary needs       | 2-3 minutes   | Reach/grab       | Clear approach behavior   |
      | Preschool          | 3-5 yr    | Play-based, choice boards  | Toys, snacks, videos| 5-8 minutes   | Point/grab       | Consistent selection      |
      | School age         | 6-12 yr   | Structured choices         | Activities, privileges| 10-15 minutes| Point/verbal     | Verbal confirmation       |
      | Adolescent         | 13-18 yr  | Interview + observation    | Social, independence | 15-20 minutes| Verbal ranking   | Self-advocacy emerging    |
      | Adult              | 18+ yr    | Self-report emphasis       | Functional activities| 20-30 minutes| Written/verbal   | Clear communication       |
      | Significant delays | All ages  | Simplified, repeated       | Basic needs         | 1-2 minutes   | Any observable   | Minimal but clear response|
    Then assessments should match developmental capacity
    And items should be age-appropriate
    And response expectations should be realistic
    And individual needs should be accommodated

  @specialized @pecs @reinforcer-assessment @naturalistic-observation @medium @not-implemented
  Scenario: Conduct naturalistic reinforcer observation
    Given natural environment observation provides valuable data
    And structured assessment may miss important items
    When conducting naturalistic observation:
      | Observation Setting | Duration | Behaviors Observed        | Items/Activities Approached | Engagement Duration | Repeated Choices | Environmental Notes     |
      | Classroom          | 30 min   | Free play, work time     | Art supplies, blocks        | 8-12 minutes       | Art: 4 times    | Quiet, structured       |
      | Playground         | 20 min   | Recess, peer interaction | Swings, sandbox            | 5-10 minutes       | Swings: 6 times | Social opportunities    |
      | Home               | 45 min   | After school, family     | iPad, snacks, dog          | 15+ minutes        | iPad: continuous| Comfort zone            |
      | Cafeteria          | 15 min   | Lunch time              | Specific foods             | Until finished     | Chocolate: priority| Social eating context  |
      | Therapy room       | 25 min   | Structured activities   | Sensory bin, bubbles       | 3-7 minutes        | Bubbles: 3 times| Adult interaction      |
      | Community          | 30 min   | Errands with family     | Specific stores, activities| Variable           | McDonald's: excitement| Real-world motivation  |
    Then natural preferences should be documented
    And setting-specific motivators should be noted
    And social vs solitary preferences should be identified
    And assessment should inform PECS material selection

  @specialized @pecs @reinforcer-assessment @functional-analysis @high @not-implemented
  Scenario: Analyze functional properties of preferred items
    Given reinforcers serve different functions
    And PECS success depends on understanding functions
    When analyzing reinforcer functions:
      | Preferred Item    | Primary Function | Secondary Function | Sensory Properties | Social Component | Duration of Effect | Satiation Risk | PECS Suitability |
      | Goldfish crackers | Edible/taste    | Oral motor        | Crunchy, salty    | None            | 30 seconds        | High           | Excellent        |
      | iPad games       | Entertainment   | Stimulation       | Visual, auditory   | Minimal         | 15+ minutes       | Low            | Good with limits |
      | Bubble play      | Sensory         | Social interaction| Visual, tactile    | High with adult | 3-5 minutes       | Medium         | Excellent        |
      | Weighted blanket | Calming         | Self-regulation   | Deep pressure      | None            | 10+ minutes       | Very low       | Good for breaks  |
      | Music/dancing    | Auditory        | Movement          | Rhythmic, melodic  | Can be social   | 5-10 minutes      | Medium         | Good            |
      | Specific book    | Visual          | Routine/comfort   | Familiar, tactile  | Can be social   | 5-15 minutes      | Low            | Good            |
    Then functional understanding should guide PECS planning
    And reinforcer variety should be ensured
    And satiation management should be planned
    And backup reinforcers should be functionally different

  @specialized @pecs @reinforcer-assessment @family-input @critical @not-implemented
  Scenario: Integrate family input into reinforcer assessment
    Given families know their children best
    And home reinforcers are crucial for generalization
    When gathering family input on preferences:
      | Family Member | Relationship | Reinforcers Suggested    | Context Provided           | Reliability Rating | Usage Frequency | Availability at Home |
      | Mother        | Primary      | Yogurt pouches, puzzles | Works for bedtime routine  | High              | Daily          | Always available    |
      | Father        | Secondary    | Truck toys, outside time| Weekend activities        | Medium            | Weekends       | Weather dependent   |
      | Sibling       | Peer         | Shared iPad time        | Motivates sharing behavior | Medium            | After school   | Limited time slots  |
      | Grandmother   | Caregiver    | Specific songs, hugs    | Calming strategies        | High              | Visits         | During visits only  |
      | Babysitter    | Occasional   | Sticker charts          | Behavior management       | Medium            | When working   | Provider dependent  |
      | Teacher       | Educational  | Computer time, helper jobs| Classroom motivation     | High              | School days    | School only        |
    Then family insights should be valued
    And home-school consistency should be planned
    And cultural preferences should be respected
    And practical availability should be confirmed

  # Advanced Reinforcer Assessment Features
  @specialized @pecs @reinforcer-assessment @dynamic-tracking @high @not-implemented
  Scenario: Track reinforcer effectiveness over time
    Given reinforcer effectiveness changes over time
    And dynamic tracking prevents PECS breakdown
    When monitoring reinforcer effectiveness:
      | Time Period | Reinforcer Used | Exchanges Motivated | Enthusiasm Level | Satiation Signs | Effectiveness Rating | Action Needed        |
      | Week 1      | Goldfish       | 45/50              | Very high       | None           | 9/10                | Continue use         |
      | Week 2      | Goldfish       | 42/50              | High            | Mild           | 8/10                | Monitor closely      |
      | Week 3      | Goldfish       | 35/50              | Moderate        | Noticeable     | 6/10                | Introduce variety    |
      | Week 4      | Mixed items    | 47/50              | High            | None           | 9/10                | Rotation working     |
      | Week 5      | iPad priority  | 48/50              | Very high       | None           | 9/10                | New #1 established   |
      | Week 6      | iPad + backup  | 46/50              | High            | Slight         | 8/10                | Continue rotation    |
    Then effectiveness should be continuously monitored
    And adjustments should be made proactively
    And variety should prevent satiation
    And PECS momentum should be maintained

  @specialized @pecs @reinforcer-assessment @environmental-factors @medium @not-implemented
  Scenario: Account for environmental influences on preferences
    Given environment affects reinforcer value
    And PECS must work across settings
    When assessing environmental impact on preferences:
      | Setting Type    | Environmental Factors    | Reinforcer Modifications  | Effectiveness Change | Practical Considerations | Implementation Notes    |
      | Quiet classroom | Low stimulation, peers   | Standard items work       | Baseline            | Easy storage            | Consistent with home    |
      | Busy classroom  | High stimulation, noise  | Higher value items needed | 20% increase needed | Compete with environment| May need more powerful  |
      | Cafeteria       | Food smells, social      | Non-food items better     | Variable            | Hygiene considerations  | Different strategy      |
      | Playground      | High energy, movement    | Active items preferred    | Context dependent   | Outdoor durability      | Weather factors         |
      | Home           | Comfort, family          | Lower intensity works     | Easier motivation   | Parent comfort level    | Training needed         |
      | Community       | Unpredictable, stimulating| Highly portable needed   | Must be strongest   | Public appropriateness  | Social acceptance       |
    Then environmental adaptation should be planned
    And setting-specific reinforcers should be identified
    And portability should be considered
    And social appropriateness should be ensured

  @specialized @pecs @reinforcer-assessment @special-considerations @medium @not-implemented
  Scenario: Address special dietary and medical considerations
    Given students may have restrictions
    And safety is paramount
    When assessing with special considerations:
      | Restriction Type    | Specific Limitations     | Assessment Adaptations   | Alternative Strategies  | Safety Protocols       | Family Coordination    |
      | Food allergies     | Nuts, dairy, gluten      | Eliminate unsafe items   | Non-food focus         | EpiPen available       | Detailed allergy list  |
      | Texture aversions  | Wet, sticky, rough       | Respect preferences      | Gradual exposure       | No forced contact      | OT collaboration      |
      | Religious dietary  | Halal, kosher, vegetarian| Culturally appropriate   | Family-approved items  | Respect beliefs        | Religious leaders input|
      | Medical diet       | Ketogenic, low sodium    | Doctor-approved only     | Non-food emphasis      | Medical monitoring     | Healthcare team coord  |
      | Swallowing issues  | Dysphagia, aspiration    | Avoid risky textures     | Alternative modalities | SLP assessment         | Consistent thickening  |
      | Medication effects | Appetite changes         | Monitor fluctuations     | Flexible timing        | Med schedule awareness | Prescriber communication|
    Then safety should never be compromised
    And restrictions should be absolutely respected
    And alternatives should be readily available
    And team coordination should be comprehensive

  @specialized @pecs @reinforcer-assessment @cultural-sensitivity @medium @not-implemented
  Scenario: Ensure cultural sensitivity in reinforcer selection
    Given families have diverse cultural backgrounds
    And reinforcers must be culturally appropriate
    When considering cultural factors:
      | Cultural Factor    | Family Background      | Traditional Preferences | Adaptation Needed      | Respect Strategies     | Implementation Approach|
      | Food preferences   | Hispanic family        | Familia foods important | Include cultural items | Honor food traditions  | Family recipe items    |
      | Social interactions| Collectivist culture   | Group activities valued | Emphasize social       | Include peer component | Shared reinforcement   |
      | Religious practices| Muslim family          | Prayer times, holidays  | Schedule around        | Respect obligations    | Flexible timing        |
      | Gender roles       | Traditional values     | Activity appropriateness| Consider expectations  | Honor family values    | Culturally suitable    |
      | Language use       | Bilingual household    | Native language comfort | Bilingual materials    | Honor home language    | Code-switching OK      |
      | Extended family    | Multi-generational     | Elder input valued     | Include grandparents   | Respect hierarchy      | Family decision making |
    Then cultural competence should be demonstrated
    And family values should be honored
    And reinforcers should be culturally appropriate
    And collaboration should be respectful

  # Error Handling and Troubleshooting
  @specialized @pecs @reinforcer-assessment @error @assessment-challenges @medium @not-implemented
  Scenario: Handle challenging reinforcer assessment situations
    Given some students present assessment challenges
    When encountering assessment difficulties:
      | Challenge Type        | Specific Issue           | Modified Approach        | Alternative Strategies   | Success Indicators     | Follow-up Plan         |
      | No clear preferences  | Everything seems equal   | Extend observation time  | Naturalistic methods     | Any consistent choice  | Weekly reassessment    |
      | Extreme food focus    | Only wants one item      | Functional alternatives  | Same function, different | Accepts alternatives   | Gradual expansion      |
      | Aggressive behavior   | Grabs, hits during test  | Shorter sessions         | Environmental control    | Calm participation     | Behavior plan first    |
      | Avoidance/escape      | Runs away, shuts down    | Follow student's lead    | Child-directed approach  | Voluntary engagement   | Relationship building  |
      | Inconsistent responses| Changes daily            | Multiple assessments     | Pattern identification   | Emerging consistency   | Environmental analysis |
      | Parent disagreement  | Objects to preferences   | Family conference        | Compromise solutions     | Agreement reached      | Ongoing communication  |
    Then challenges should be addressed systematically
    And modifications should maintain assessment validity
    And student well-being should be prioritized
    And team collaboration should solve problems

  @specialized @pecs @reinforcer-assessment @error @reliability-issues @medium @not-implemented
  Scenario: Address assessment reliability and validity concerns
    Given assessment quality affects PECS success
    When reliability issues arise:
      | Reliability Issue     | Manifestation           | Assessment Impact       | Correction Strategy     | Validation Method      | Quality Assurance      |
      | Inconsistent choices  | Different results daily | Unreliable hierarchy    | Multiple sessions       | Test-retest reliability| 80% consistency target |
      | Examiner bias        | Leading student         | Invalid results         | Blind assessment        | Inter-rater reliability| Multiple assessors     |
      | Setting influence    | Only works in one place | Limited generalization  | Multiple environments   | Cross-setting validity | Setting comparison     |
      | Time of day effects  | Morning vs afternoon    | Variable motivation     | Time-matched sessions   | Temporal stability     | Schedule consistency   |
      | Medication timing    | Before vs after meds    | Confounded results      | Consistent timing       | Medical coordination   | Healthcare team input  |
      | Fatigue factors      | Tired student           | Reduced responding      | Energy-matched sessions | Alertness monitoring   | Optimal timing         |
    Then reliability should be quantified
    And validity should be confirmed
    And bias should be minimized
    And assessment quality should be ensured

  @specialized @pecs @reinforcer-assessment @error @implementation-barriers @low @not-implemented
  Scenario: Overcome practical implementation barriers
    Given real-world constraints affect assessment
    When facing implementation barriers:
      | Barrier Type         | Specific Constraint     | Impact on Assessment    | Workaround Strategy     | Resource Requirements  | Success Probability    |
      | Time limitations     | Only 15 minutes available| Incomplete assessment  | Structured priority    | Efficient protocols    | Moderate success       |
      | Material access      | Items not available     | Cannot test preferences | Substitute similar items| Creative alternatives  | Good if validated      |
      | Space constraints    | Small therapy room      | Limited choice array    | Sequential presentation | Flexible setup        | Adequate results       |
      | Staff shortage       | No second person        | Cannot do 2-person     | Modified single-person  | Adapted procedures     | Reduced reliability    |
      | Budget limitations   | Cannot buy test items   | Limited scope          | Borrow, make, substitute| Creative sourcing      | Variable quality       |
      | Family resistance    | Objects to certain items| Restricted options     | Negotiate alternatives  | Communication skills   | Compromise possible    |
    Then barriers should be addressed creatively
    And quality should be maintained when possible
    And limitations should be documented
    And best available assessment should be conducted

  @specialized @pecs @reinforcer-assessment @reassessment-protocols @high @not-implemented
  Scenario: Implement systematic reassessment protocols
    Given preferences change over time
    And PECS success requires current motivators
    When implementing ongoing reassessment:
      | Reassessment Trigger  | Timing/Frequency       | Assessment Scope       | Comparison to Previous | Decision Criteria      | Implementation Changes |
      | Scheduled routine     | Monthly                | Full hierarchy review  | Track changes over time| Significant shifts     | Update materials       |
      | Performance decline   | Immediate when noticed | Quick preference check | Focus on current #1    | Motivation loss        | Emergency alternatives |
      | Developmental changes | Quarterly              | Age-appropriate items  | Developmental progress | New capacities         | Expand options         |
      | Environmental change  | Before setting switch  | Setting-specific items | Context comparison     | Environmental fit      | Setting adaptations    |
      | Family request        | As requested           | Targeted concern areas | Address specific issues| Family satisfaction    | Collaborative updates  |
      | Seasonal factors      | Seasonally             | Activity alignment     | Seasonal preferences   | Motivation patterns    | Seasonal materials     |
    Then reassessment should be systematic
    And changes should be data-driven
    And PECS materials should stay current
    And motivation should be maintained