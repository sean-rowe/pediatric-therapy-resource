Feature: Clinical Procedures and Standardized Assessment Protocols
  As a therapy professional
  I want to follow standardized clinical procedures
  So that assessments and interventions are reliable and valid

  Background:
    Given clinical procedures are evidence-based
    And standardized protocols ensure consistency
    And training requirements are met
    And documentation standards are followed
    And quality assurance is maintained

  # Standardized Assessment Procedures
  @protocols @clinical @assessment @administration @critical @not-implemented
  Scenario: Administer standardized assessments with fidelity
    Given standardized assessments require specific procedures
    And validity depends on administration fidelity
    When conducting standardized assessments:
      | Assessment Type | Setup Requirements | Administration Rules | Scoring Procedures | Interpretation | Documentation |
      | Norm-referenced | Standardized materials | Verbatim instructions | Manual scoring rules | Percentile ranks | Full protocol |
      | Criterion-referenced | Specific equipment | Exact procedures | Mastery criteria | Skill achievement | Item analysis |
      | Curriculum-based | Grade-level materials | Timed procedures | Fluency calculations | Benchmark comparison | Progress monitoring |
      | Dynamic assessment | Test-teach-retest | Mediation protocols | Learning potential | Response to intervention | Process notes |
      | Observational | Natural environment | Structured observation | Behavior coding | Pattern analysis | Time sampling |
      | Performance-based | Functional tasks | Task analysis | Component scoring | Functional level | Video documentation |
    Then assessments should yield valid results
    And reliability should be maintained
    And results should guide intervention
    And compliance documentation should be complete

  @protocols @clinical @screening @early-identification @critical @not-implemented
  Scenario: Implement comprehensive screening procedures
    Given screening identifies children needing assessment
    And procedures must be efficient and accurate
    When conducting screening procedures:
      | Screening Level | Tools Used | Administration Time | Decision Rules | Follow-up Actions | Documentation |
      | Universal screening | Brief screeners | 5-10 minutes | Cut scores | Referral or monitor | Screening database |
      | Targeted screening | Risk indicators | 15-20 minutes | Multiple criteria | Comprehensive assessment | Risk factors |
      | Progress monitoring | CBM probes | 1-3 minutes | Trend analysis | Intervention adjustment | Progress graphs |
      | Diagnostic screening | Domain-specific | 20-30 minutes | Clinical judgment | Targeted evaluation | Detailed report |
      | Developmental | Milestone checklists | Parent report | Age expectations | Early intervention | Developmental history |
      | Sensory screening | Sensory profiles | Observation + report | Clinical indicators | Sensory assessment | Sensory patterns |
    Then at-risk children should be identified
    And over-identification should be minimized
    And referrals should be appropriate
    And early intervention should begin

  @protocols @clinical @diagnosis @differential @high @not-implemented
  Scenario: Conduct differential diagnosis procedures
    Given accurate diagnosis guides appropriate intervention
    And systematic procedures ensure thoroughness
    When performing differential diagnosis:
      | Diagnostic Step | Information Gathered | Analysis Method | Decision Points | Rule-outs | Confirmation |
      | History taking | Developmental, medical, family | Timeline construction | Red flags | Alternative explanations | Pattern matching |
      | Symptom analysis | Onset, frequency, severity | Diagnostic criteria | DSM-5/ICD-11 | Similar conditions | Criterion meeting |
      | Functional impact | Home, school, community | Impairment assessment | Clinical significance | Environmental factors | Functional limitations |
      | Comorbidity screen | Associated conditions | Systematic review | Co-occurring disorders | Primary vs secondary | Multiple diagnoses |
      | Team consultation | Multi-disciplinary input | Case conference | Consensus building | Discipline perspectives | Integrated formulation |
      | Trial intervention | Response to treatment | Progress monitoring | Diagnostic therapy | Non-responders | Treatment validation |
    Then diagnosis should be accurate
    And comorbidities should be identified
    And treatment planning should be informed
    And families should understand

  # Intervention Procedures
  @protocols @clinical @intervention @treatment-planning @critical @not-implemented
  Scenario: Develop comprehensive treatment plans
    Given treatment plans guide systematic intervention
    And evidence-based practice requires careful planning
    When creating treatment plans:
      | Planning Component | Data Sources | Goal Development | Intervention Selection | Progress Monitoring | Plan Updates |
      | Needs assessment | Evaluation results | Priority ranking | Evidence review | Baseline data | Initial plan |
      | Goal setting | Family priorities | SMART criteria | Intervention match | Goal attainment | Quarterly review |
      | Service delivery | Setting analysis | Frequency/duration | Service model | Attendance tracking | As needed |
      | Intervention strategies | Evidence base | Protocol selection | Fidelity planning | Strategy effectiveness | Data-based changes |
      | Discharge planning | Progress criteria | Transition goals | Maintenance plan | Outcome measures | Discharge summary |
      | Coordination | Team members | Role delineation | Communication plan | Team meetings | Integrated services |
    Then plans should be comprehensive
    And goals should be measurable
    And interventions should be evidence-based
    And progress should be tracked

  @protocols @clinical @documentation @medical-records @critical @not-implemented
  Scenario: Maintain clinical documentation standards
    Given documentation supports quality care
    And legal requirements must be met
    When documenting clinical services:
      | Documentation Type | Required Elements | Timing Requirements | Quality Standards | Legal Compliance | Storage |
      | Initial evaluation | All findings, recommendations | Within 48 hours | Professional language | Signature, credentials | Secure EMR |
      | Progress notes | SOAP format, objective data | Same day | Clear, concise | Date, time, sign | Encrypted |
      | Treatment plans | Goals, interventions, frequency | Before services | Measurable objectives | Parent consent | Accessible |
      | Progress reports | Data, graphs, interpretation | Per schedule | Family-friendly | Review with family | Shared securely |
      | Discharge summary | Outcomes, recommendations | Within 1 week | Comprehensive | Final outcomes | Permanent record |
      | Communication log | Contacts, content, actions | Immediately | Factual record | HIPAA compliant | Audit trail |
    Then documentation should be timely
    And quality should meet standards
    And legal requirements should be met
    And records should be secure

  # Safety Procedures
  @protocols @clinical @safety @risk-management @critical @not-implemented
  Scenario: Implement clinical safety procedures
    Given patient safety is paramount
    And procedures prevent adverse events
    When ensuring clinical safety:
      | Safety Domain | Risk Assessment | Prevention Protocols | Emergency Response | Reporting | Quality Improvement |
      | Physical safety | Environment scan | Equipment checks | First aid ready | Incident reports | Safety rounds |
      | Infection control | Health screening | Universal precautions | Isolation procedures | Exposure tracking | Protocol updates |
      | Medical emergencies | Risk factors | Emergency plans | Rapid response | Code team activation | Drill practice |
      | Behavioral safety | Behavior assessment | De-escalation | Crisis intervention | Behavior incidents | Team debriefing |
      | Equipment safety | Inspection schedule | Maintenance logs | Equipment failure | Repair tracking | Replacement planning |
      | Medication safety | Allergy verification | Administration protocols | Adverse reactions | Error reporting | System improvements |
    Then safety risks should be minimized
    And responses should be prepared
    And incidents should be rare
    And learning should drive improvement

  @protocols @clinical @consent @informed-consent @high @not-implemented
  Scenario: Obtain appropriate informed consent
    Given informed consent is ethical and legal requirement
    And procedures ensure understanding
    When obtaining consent:
      | Consent Type | Information Provided | Comprehension Check | Documentation | Special Considerations | Renewal |
      | Treatment consent | Risks, benefits, alternatives | Teach-back method | Written consent | Interpreter if needed | Annual |
      | Assessment consent | Purpose, procedures, use | Questions answered | Signed forms | Capacity assessment | Per assessment |
      | Video/photo | Use, storage, sharing | Specific permissions | Release forms | Opt-out options | Per use |
      | Research participation | Voluntary, withdrawal rights | Understanding verification | IRB-approved forms | Assent for minors | Study duration |
      | Information sharing | What, with whom, why | Privacy explanation | HIPAA forms | Specific authorizations | As needed |
      | Telehealth consent | Technology, limitations | Platform demonstration | E-signature | State regulations | Annual update |
    Then consent should be truly informed
    And understanding should be verified
    And compliance documentation should be complete
    And rights should be protected

  # Clinical Reasoning Procedures
  @protocols @clinical @reasoning @decision-making @high @not-implemented
  Scenario: Apply systematic clinical reasoning
    Given clinical decisions impact outcomes
    And systematic reasoning improves accuracy
    When applying clinical reasoning:
      | Reasoning Step | Information Processing | Analysis Tools | Decision Support | Documentation | Outcome |
      | Problem identification | Pattern recognition | Assessment data | Clinical guidelines | Problem list | Clear focus |
      | Hypothesis generation | Differential thinking | Decision trees | Evidence review | Clinical reasoning | Multiple options |
      | Data gathering | Targeted collection | Specific tests | Literature search | Supporting data | Complete picture |
      | Hypothesis testing | Critical analysis | Statistical tools | Peer consultation | Analysis notes | Best hypothesis |
      | Intervention planning | Option weighing | Decision matrix | Clinical pathways | Rationale | Optimal plan |
      | Outcome evaluation | Progress analysis | Outcome measures | Benchmark comparison | Results summary | Effective treatment |
    Then decisions should be evidence-based
    And reasoning should be transparent
    And outcomes should improve
    And learning should occur

  # Quality Assurance Procedures
  @protocols @clinical @quality @peer-review @critical @not-implemented
  Scenario: Conduct clinical quality assurance
    Given quality assurance ensures best practice
    And systematic review identifies improvements
    When implementing QA procedures:
      | QA Component | Review Method | Standards Applied | Data Collection | Improvement Actions | Follow-up |
      | Chart review | Random sampling | Documentation standards | Audit checklist | Feedback to clinicians | Re-audit |
      | Peer review | Case presentation | Clinical guidelines | Peer feedback forms | Learning plans | Skill verification |
      | Outcome monitoring | Data analysis | Benchmark comparison | Outcome databases | Process improvement | Continuous monitoring |
      | Family satisfaction | Surveys, interviews | Service standards | Satisfaction scores | Service adjustments | Trend tracking |
      | Clinical competency | Observation, testing | Competency standards | Skills checklist | Training plans | Annual review |
      | Evidence updates | Literature review | Current evidence | Research databases | Protocol updates | Implementation tracking |
    Then quality should be maintained
    And improvements should be identified
    And standards should be met
    And outcomes should optimize

  # Discharge Procedures
  @protocols @clinical @discharge @transition-planning @high @not-implemented
  Scenario: Execute systematic discharge procedures
    Given discharge planning ensures continuity
    And procedures support successful transitions
    When planning discharge:
      | Discharge Phase | Criteria Assessment | Preparation Steps | Handoff Process | Follow-up Plan | Documentation |
      | Readiness assessment | Goal achievement | Progress review | Team discussion | Criteria checklist | Readiness summary |
      | Transition planning | Support needs | Resource identification | Service coordination | Community resources | Transition plan |
      | Family preparation | Confidence building | Home program training | Skill demonstration | Written instructions | Training records |
      | Service tapering | Independence testing | Gradual reduction | Progress monitoring | Check-in schedule | Tapering notes |
      | Final assessment | Outcome measurement | Comprehensive testing | Results interpretation | Comparison to baseline | Discharge report |
      | Resource provision | Ongoing needs | Material preparation | Resource handoff | Contact information | Resource list |
    Then discharge should be well-planned
    And families should be prepared
    And continuity should be ensured
    And outcomes should be sustained

  # Emergency Response Procedures
  @protocols @clinical @emergency @crisis-response @critical @not-implemented
  Scenario: Implement emergency response procedures
    Given emergencies require immediate response
    And procedures ensure appropriate action
    When responding to emergencies:
      | Emergency Type | Initial Response | Assessment Protocol | Intervention Steps | Communication | Follow-up |
      | Medical emergency | Scene safety, ABC | Vital signs, symptoms | First aid, 911 | Family, physician | Incident report |
      | Behavioral crisis | Safety first | Risk assessment | De-escalation | Crisis team | Debrief, plan |
      | Anaphylaxis | Epi-pen ready | Symptom recognition | Medication, EMS | Emergency contacts | Allergy plan update |
      | Seizure | Protect, time | Seizure protocol | Recovery position | Parent, nurse | Seizure log |
      | Choking | Heimlich ready | Airway check | Age-appropriate technique | Call for help | Feeding plan review |
      | Mental health | Calm approach | Suicide screening | Safety plan | Crisis hotline | Follow-up services |
    Then responses should be immediate
    And procedures should be followed
    And safety should be restored
    And prevention should be enhanced

  # Infection Control Procedures
  @protocols @clinical @infection-control @hygiene @critical @not-implemented
  Scenario: Maintain infection control standards
    Given infection control protects all participants
    And procedures prevent disease transmission
    When implementing infection control:
      | Control Measure | Implementation | Monitoring Method | Compliance Tracking | Education | Updates |
      | Hand hygiene | Wash/sanitize protocol | Observation audits | Compliance rates | Regular training | CDC guidelines |
      | PPE use | Appropriate selection | Supply tracking | Usage monitoring | Donning/doffing | Situation-specific |
      | Environmental cleaning | Schedule, products | Cleaning logs | Surface testing | Staff training | Product updates |
      | Isolation precautions | Transmission-based | Signage, supplies | Adherence checks | Family education | Outbreak response |
      | Equipment disinfection | Between patients | Cleaning checklist | Random checks | Proper techniques | New equipment |
      | Outbreak management | Early detection | Case tracking | Response activation | Communication | Lessons learned |
    Then infections should be prevented
    And compliance should be high
    And outbreaks should be contained
    And safety should be maintained

  # Data Management Procedures
  @protocols @clinical @data @privacy-security @high @not-implemented
  Scenario: Ensure clinical data management standards
    Given clinical data requires careful management
    And procedures ensure privacy and accuracy
    When managing clinical data:
      | Data Type | Collection Method | Storage Protocol | Access Controls | Quality Checks | Retention |
      | Assessment scores | Direct entry | Encrypted database | Role-based access | Range validation | 7 years minimum |
      | Progress data | Session collection | Secure cloud | Need-to-know | Trend analysis | Treatment duration |
      | Video recordings | Consent-based | Encrypted storage | Limited access | Quality review | Per consent |
      | Research data | IRB protocols | De-identified | Research team only | Data verification | Study period |
      | Outcome measures | Standardized tools | Central repository | Clinician access | Completeness check | Indefinite |
      | Communication | Secure messaging | Audit trail | Participant only | Privacy review | Legal requirement |
    Then data should be secure
    And privacy should be protected
    And quality should be ensured
    And compliance should be maintained

  # Training and Competency Procedures
  @protocols @clinical @training @competency-verification @high @not-implemented
  Scenario: Verify clinical competencies systematically
    Given competency ensures quality care
    And verification procedures ensure standards
    When verifying competencies:
      | Competency Area | Verification Method | Performance Criteria | Documentation | Remediation | Renewal |
      | Assessment skills | Observation, testing | Accuracy, efficiency | Competency checklist | Focused training | Annual |
      | Intervention delivery | Video review | Fidelity measures | Performance rubric | Coaching sessions | Biannual |
      | Documentation | Chart review | Completeness, quality | Audit results | Writing workshop | Ongoing |
      | Safety procedures | Simulation, drills | Response accuracy | Drill participation | Practice sessions | Quarterly |
      | Technology use | Hands-on demo | Platform proficiency | Skills checklist | Tutorial completion | As updated |
      | Professional behavior | 360 feedback | Professional standards | Feedback summary | Development plan | Annual review |
    Then competencies should be verified
    And standards should be maintained
    And gaps should be addressed
    And quality should be assured

  # Research and Evidence Procedures
  @protocols @clinical @research @evidence-integration @medium @not-implemented
  Scenario: Integrate research evidence into practice
    Given evidence-based practice improves outcomes
    And procedures ensure systematic integration
    When integrating research evidence:
      | Evidence Level | Search Strategy | Appraisal Method | Integration Process | Implementation | Evaluation |
      | Systematic reviews | Database search | GRADE criteria | Guideline development | Protocol update | Outcome monitoring |
      | RCTs | Focused questions | Critical appraisal | Team discussion | Pilot testing | Effect measurement |
      | Practice guidelines | Professional sources | AGREE tool | Adaptation planning | Staff training | Fidelity checking |
      | Expert consensus | Literature + experts | Delphi process | Local modification | Phased rollout | Feedback collection |
      | Case studies | Similar populations | Applicability review | Careful testing | Single case design | Individual tracking |
      | Innovation | Emerging evidence | Risk-benefit analysis | Controlled trial | IRB approval | Safety monitoring |
    Then practice should be evidence-based
    And integration should be systematic
    And outcomes should improve
    And knowledge should advance

  # Communication Procedures
  @protocols @clinical @communication @interprofessional @high @not-implemented
  Scenario: Facilitate interprofessional communication
    Given team communication impacts outcomes
    And procedures ensure effective exchange
    When communicating clinically:
      | Communication Type | Format/Method | Key Information | Timing | Documentation | Follow-up |
      | Team meetings | Structured agenda | Updates, planning | Weekly/monthly | Meeting minutes | Action items |
      | Consultation | Written/verbal | Question, findings | Within 48 hours | Consult note | Recommendations |
      | Handoffs | SBAR format | Critical information | Shift change | Handoff form | Verification |
      | Family conferences | In-person/virtual | Progress, planning | Scheduled | Conference summary | Next steps |
      | Crisis communication | Immediate contact | Safety, actions | Real-time | Incident report | Debrief |
      | Progress updates | Reports, calls | Data, interpretation | Per schedule | Written summary | Questions addressed |
    Then communication should be clear
    And information should be complete
    And understanding should be verified
    And outcomes should benefit

  # Ethical Procedures
  @protocols @clinical @ethics @decision-making @critical @not-implemented
  Scenario: Navigate ethical decision-making
    Given ethical dilemmas arise in practice
    And procedures guide principled decisions
    When facing ethical dilemmas:
      | Ethical Principle | Assessment Method | Stakeholder Input | Decision Process | Documentation | Review |
      | Autonomy | Capacity evaluation | Patient preferences | Supported decision | Consent forms | Ethics review |
      | Beneficence | Risk-benefit analysis | Team perspectives | Best interest | Clinical reasoning | Outcome tracking |
      | Non-maleficence | Harm assessment | Safety evaluation | Precautionary principle | Risk documentation | Incident prevention |
      | Justice | Resource allocation | Equity analysis | Fair distribution | Allocation criteria | Equity monitoring |
      | Veracity | Truth-telling | Information needs | Honest communication | Disclosure notes | Trust building |
      | Fidelity | Promise keeping | Commitment review | Reliability | Agreement tracking | Relationship quality |
    Then decisions should be ethical
    And principles should guide practice
    And stakeholders should be heard
    And integrity should be maintained

  # Technology Integration Procedures
  @protocols @clinical @technology @digital-health @high @not-implemented
  Scenario: Integrate technology into clinical practice
    Given technology enhances clinical services
    And procedures ensure appropriate use
    When integrating clinical technology:
      | Technology Type | Selection Criteria | Implementation Steps | Training Required | Quality Monitoring | Updates |
      | Telehealth platforms | HIPAA compliance | Pilot testing | Platform proficiency | Session quality | Regular updates |
      | Assessment apps | Validity evidence | Gradual rollout | Administration training | Score accuracy | Version control |
      | Data systems | Security features | Phased implementation | System navigation | Data integrity | Backup procedures |
      | Therapy software | Evidence base | License management | Feature mastery | Outcome tracking | License renewal |
      | Communication tools | Encryption standards | Policy development | Security training | Usage monitoring | Security patches |
      | Assistive technology | Individual matching | Trial periods | Customization skills | Functional use | Maintenance schedule |
    Then technology should enhance practice
    And security should be maintained
    And competency should be ensured
    And outcomes should improve

  # Supervision Procedures
  @protocols @clinical @supervision @professional-development @high @not-implemented
  Scenario: Provide systematic clinical supervision
    Given supervision develops clinical skills
    And procedures ensure effective mentoring
    When providing clinical supervision:
      | Supervision Component | Structure | Methods Used | Documentation | Evaluation | Outcomes |
      | Goal setting | Collaborative | SMART goals | Supervision contract | Progress review | Skill development |
      | Observation | Scheduled sessions | Direct, video | Observation forms | Competency rubrics | Performance improvement |
      | Feedback | Timely, specific | Strengths-based | Written feedback | Receptivity | Behavior change |
      | Reflection | Guided questions | Critical incidents | Reflection logs | Insight development | Professional growth |
      | Skill development | Targeted practice | Modeling, coaching | Skill checklists | Competency testing | Mastery achievement |
      | Professional planning | Career development | Goal progression | Development plans | Milestone tracking | Career advancement |
    Then supervision should be systematic
    And skills should develop
    And reflection should deepen
    And practice should improve

  # Outcome Measurement Procedures
  @protocols @clinical @outcomes @measurement @critical @not-implemented
  Scenario: Implement comprehensive outcome measurement
    Given outcome measurement demonstrates effectiveness
    And procedures ensure valid measurement
    When measuring clinical outcomes:
      | Outcome Domain | Measurement Tools | Collection Schedule | Analysis Method | Interpretation | Use |
      | Functional status | Standardized measures | Admission, discharge | Change scores | Clinical significance | Treatment planning |
      | Quality of life | QOL scales | Quarterly | Domain analysis | Norm comparison | Holistic planning |
      | Goal attainment | GAS | Per goal timeline | T-score calculation | Achievement level | Progress monitoring |
      | Satisfaction | Surveys, interviews | Post-service | Statistical analysis | Benchmark comparison | Service improvement |
      | Cost-effectiveness | Service utilization | Ongoing tracking | Cost-benefit ratio | Value demonstration | Resource allocation |
      | Long-term outcomes | Follow-up contact | 6-month, 1-year | Maintenance analysis | Sustainability | Program evaluation |
    Then outcomes should be measured validly
    And progress should be demonstrated
    And value should be shown
    And improvements should be guided