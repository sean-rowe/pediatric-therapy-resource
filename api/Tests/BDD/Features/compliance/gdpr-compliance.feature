Feature: GDPR Compliance and EU Data Protection
  As a platform potentially serving EU residents
  I want to ensure full GDPR compliance
  So that personal data is protected according to EU requirements

  Background:
    Given GDPR compliance systems are operational
    And lawful basis for processing is established
    And data subject rights mechanisms are implemented
    And privacy by design principles are applied
    And data protection measures are active

  # Core GDPR Requirements
  @compliance @gdpr @lawful-basis @processing-grounds @critical @not-implemented
  Scenario: Establish and document lawful basis for data processing
    Given GDPR requires lawful basis for all processing
    And basis must be documented and communicated
    When establishing lawful basis:
      | Processing Activity | Lawful Basis | Justification | Data Subjects Affected | Documentation Required | Review Schedule |
      | Therapy service delivery | Contract performance | Service agreement | Patients/students | Service contracts | Annual review |
      | Healthcare operations | Vital interests | Health/safety necessity | Emergency situations | Medical necessity docs | Per incident |
      | Educational services | Public task | Educational institution | Students | Educational agreements | Academic year |
      | Marketing communications | Consent | Explicit opt-in | All users | Consent records | Continuous |
      | Legal compliance | Legal obligation | Regulatory requirements | All data subjects | Legal requirements | Quarterly |
      | Platform improvement | Legitimate interests | Service enhancement | All users | LIA documentation | Semi-annual |
    Then lawful basis should be established for all processing
    And justifications should be documented
    And communications should be clear
    And reviews should ensure continued validity

  @compliance @gdpr @data-subject-rights @individual-rights @critical @not-implemented
  Scenario: Implement comprehensive data subject rights
    Given GDPR grants specific rights to individuals
    And rights must be easily exercisable
    When implementing data subject rights:
      | Right Type | Implementation Method | Response Timeline | Verification Process | System Support | Exceptions |
      | Access (Article 15) | Self-service portal + requests | 30 days | Identity verification | Automated data export | None |
      | Rectification (Article 16) | Online editing + requests | 30 days | Account verification | Edit tracking | None |
      | Erasure (Article 17) | Delete button + requests | 30 days | Identity confirmation | Cascade deletion | Legal retention |
      | Restriction (Article 18) | Processing freeze option | 72 hours | Request validation | Processing blocks | Investigation period |
      | Portability (Article 20) | Data export tools | 30 days | Account ownership | Standard formats | Technical feasibility |
      | Objection (Article 21) | Opt-out mechanisms | Immediate | Simple declaration | Processing cessation | Compelling grounds |
    Then all rights should be implementable
    And timelines should be met
    And processes should be user-friendly
    And exceptions should be documented

  @compliance @gdpr @privacy-notice @transparency @high @not-implemented
  Scenario: Provide comprehensive privacy notices and transparency
    Given GDPR requires detailed privacy information
    And transparency must be meaningful
    When providing privacy notices:
      | Notice Component | Content Requirements | Presentation Format | Accessibility | Updates Trigger | Version Control |
      | Controller identity | Full legal entity details | Clear identification | Multi-language | Entity changes | Version history |
      | Processing purposes | Specific, explicit purposes | Categorized list | Plain language | Purpose changes | Change tracking |
      | Legal basis | Basis for each purpose | Linked to purposes | Visual aids | Basis changes | Update notifications |
      | Data categories | Types of data processed | Comprehensive inventory | Examples provided | Category changes | Addition alerts |
      | Recipients | Third party categories | Detailed listing | Purpose explanation | Recipient changes | Sharing updates |
      | Retention periods | Specific timeframes | Retention schedule | Deletion timeline | Period changes | Schedule updates |
    Then privacy notices should be comprehensive
    And information should be accessible
    And updates should be communicated
    And understanding should be facilitated

  @compliance @gdpr @consent-management @explicit-consent @critical @not-implemented
  Scenario: Manage explicit consent and granular controls
    Given GDPR consent must be freely given and specific
    And withdrawal must be as easy as giving consent
    When managing consent:
      | Consent Type | Collection Method | Granularity Level | Withdrawal Method | Documentation | Child Provisions |
      | Service consent | Checkbox at signup | Per service feature | One-click withdrawal | Timestamped log | Parental consent |
      | Marketing consent | Separate opt-in | By channel and type | Unsubscribe links | Consent database | Age verification |
      | Cookie consent | Cookie banner | By cookie category | Cookie preferences | Consent string | Simplified options |
      | Data sharing consent | Explicit approval | Per third party | Sharing dashboard | Sharing records | Enhanced protection |
      | Profiling consent | Clear explanation | Per profiling type | Profile deletion | Algorithm consent | Prohibited for children |
      | Special category data | Explicit consent | Per data type | Immediate cessation | Enhanced records | Extra safeguards |
    Then consent should be properly obtained
    And controls should be granular
    And withdrawal should be simple
    And records should prove compliance

  # Data Protection Measures
  @compliance @gdpr @data-minimization @purpose-limitation @high @not-implemented
  Scenario: Enforce data minimization and purpose limitation
    Given GDPR requires minimal necessary data
    And purposes must be specified and limited
    When implementing minimization:
      | Data Collection | Purpose Specification | Minimization Strategy | Retention Limit | Access Restriction | Deletion Trigger |
      | Registration data | Account creation | Essential fields only | Active + 2 years | Account team only | Inactivity period |
      | Therapy records | Healthcare delivery | Clinical necessity | Legal requirements | Healthcare providers | Retention expiry |
      | Usage analytics | Service improvement | Aggregated data preferred | 13 months | Analytics team | Rolling deletion |
      | Support tickets | Customer service | Issue resolution data | Resolution + 1 year | Support team | Ticket closure |
      | Payment data | Transaction processing | Payment essentials | Legal requirements | Finance team | Account closure |
      | Communication logs | Service delivery | Metadata only | 6 months | Audit purposes | Automatic purge |
    Then data collection should be minimal
    And purposes should be explicit
    And retention should be limited
    And access should be restricted

  @compliance @gdpr @security-measures @technical-safeguards @critical @not-implemented
  Scenario: Implement appropriate technical and organizational measures
    Given GDPR requires security appropriate to risk
    And measures must be demonstrable
    When implementing security measures:
      | Security Domain | Technical Measures | Organizational Measures | Risk Assessment | Effectiveness Testing | Documentation |
      | Encryption | AES-256, TLS 1.3 | Encryption policies | Data classification | Penetration testing | Security architecture |
      | Access control | MFA, RBAC | Access procedures | User risk assessment | Access reviews | Permission matrix |
      | Pseudonymization | ID tokenization | Data handling rules | Re-identification risk | Anonymization testing | Pseudonymization map |
      | Resilience | HA infrastructure | BC/DR procedures | Availability risk | Failover testing | Recovery plans |
      | Testing | Security scanning | Security training | Vulnerability assessment | Red team exercises | Test results |
      | Incident response | SIEM, IDS/IPS | Response procedures | Incident risk matrix | Tabletop exercises | Response playbooks |
    Then security should be risk-appropriate
    And measures should be comprehensive
    And effectiveness should be tested
    And compliance should be demonstrable

  @compliance @gdpr @cross-border-transfers @data-localization @high @not-implemented
  Scenario: Manage cross-border data transfers and localizations
    Given GDPR restricts transfers outside EEA
    And adequate protections must exist
    When managing transfers:
      | Transfer Scenario | Legal Mechanism | Safeguards Required | Documentation | Monitoring | Data Subject Rights |
      | EU to US | Standard Contractual Clauses | SCC implementation | Executed SCCs | Transfer logs | Information rights |
      | EU to adequate country | Adequacy decision | Continued monitoring | Adequacy reliance | Adequacy status | Standard rights |
      | Cloud processing | Processor SCCs | Sub-processor controls | Cloud agreements | Processing locations | Transparency |
      | Group transfers | Binding Corporate Rules | BCR approval | BCR documentation | Internal audit | Group-wide rights |
      | Emergency transfers | Vital interests | Necessity documentation | Emergency records | Post review | Notification |
      | Consent-based | Explicit consent | Risk disclosure | Informed consent | Consent tracking | Withdrawal option |
    Then transfers should have legal basis
    And safeguards should be implemented
    And transparency should be maintained
    And rights should be preserved

  @compliance @gdpr @dpia-process @risk-assessment @high @not-implemented
  Scenario: Conduct Data Protection Impact Assessments
    Given DPIAs are required for high-risk processing
    And assessments must be thorough
    When conducting DPIAs:
      | Processing Type | Risk Factors | Assessment Scope | Mitigation Measures | DPO Consultation | Review Trigger |
      | AI/ML processing | Automated decisions | Algorithm fairness | Human oversight | Mandatory consultation | Model changes |
      | Child data processing | Vulnerable subjects | Enhanced protections | Age verification | Early involvement | Policy updates |
      | Health data processing | Special category | Security assessment | Enhanced encryption | Risk evaluation | Breach events |
      | Large scale monitoring | Systematic observation | Scope limitation | Purpose restriction | Privacy review | Scope expansion |
      | Behavioral analysis | Profiling risks | Transparency measures | Opt-out options | Ethics review | Algorithm updates |
      | New technology | Unknown risks | Comprehensive review | Pilot testing | Innovation guidance | Tech updates |
    Then DPIAs should identify all risks
    And mitigations should be effective
    And consultation should be documented
    And reviews should be triggered appropriately

  # Compliance Operations
  @compliance @gdpr @breach-notification @72-hour-requirement @critical @not-implemented
  Scenario: Manage data breach notification within 72 hours
    Given GDPR requires 72-hour breach notification
    And high-risk breaches require individual notification
    When managing breach notifications:
      | Breach Detection | Risk Assessment | Authority Notification | Individual Notification | Documentation | Mitigation |
      | Immediate detection | Risk scoring matrix | Within 72 hours to DPA | Without undue delay if high risk | Breach register | Immediate containment |
      | Security monitoring | Impact assessment | Phased notification allowed | Clear language required | Assessment records | Technical response |
      | User report | Rights impact analysis | Preliminary + follow-up | Multiple channels | Timeline documentation | User protection |
      | Partner notification | Cross-border assessment | Lead DPA notification | Coordinated approach | Communication logs | Partner coordination |
      | Internal discovery | Category assessment | Delayed if justified | Risk-based decision | Justification records | Process improvement |
      | Audit finding | Historical assessment | Voluntary disclosure | Retrospective notice | Audit documentation | Remediation plan |
    Then breaches should be detected quickly
    And notifications should meet deadlines
    And communications should be clear
    And improvements should prevent recurrence

  @compliance @gdpr @dpo-requirements @data-protection-officer @high @not-implemented
  Scenario: Establish Data Protection Officer role and responsibilities
    Given certain organizations must appoint a DPO
    And DPO must have independence
    When implementing DPO function:
      | DPO Requirement | Implementation | Independence Safeguards | Responsibilities | Resources Provided | Reporting Structure |
      | Appointment | Qualified professional | No conflict of interest | Privacy program oversight | Dedicated budget | Direct to highest level |
      | Expertise | Privacy law expertise | Continuous education | Legal compliance guidance | Training resources | Board reporting |
      | Accessibility | Published contact | Direct access by subjects | Query response | Support team | Public contact info |
      | Involvement | Early consultation | Mandatory involvement | DPIA oversight | Project integration | Steering committees |
      | Protection | No dismissal for duties | Performance protection | Independent opinions | Legal protection | Whistleblower protection |
      | Tasks | Defined responsibilities | Autonomous operation | Compliance monitoring | Audit authority | Regular reporting |
    Then DPO should be properly appointed
    And independence should be guaranteed
    And responsibilities should be clear
    And effectiveness should be enabled

  @compliance @gdpr @vendor-management @processor-agreements @high @not-implemented
  Scenario: Manage data processors and sub-processors
    Given processors must provide sufficient guarantees
    And agreements must contain required clauses
    When managing processors:
      | Processor Type | Due Diligence | Contract Requirements | Audit Rights | Sub-processor Controls | Termination Rights |
      | Cloud providers | Security assessment | Article 28 clauses | Annual audit rights | Prior approval required | Data return/deletion |
      | SaaS vendors | Privacy review | Processing limitations | Audit reports access | Notification of changes | Export capabilities |
      | Analytics services | Data practices review | Purpose restrictions | Inspection rights | Objection rights | Immediate termination |
      | Support vendors | Access controls review | Confidentiality terms | Security audits | Location restrictions | Data segregation |
      | Development partners | Security practices | Data protection terms | Code reviews | No sub-processing | IP separation |
      | Marketing tools | Consent handling | Lawful basis alignment | Compliance audits | EU-only processing | List management |
    Then processors should be vetted
    And agreements should be compliant
    And oversight should be maintained
    And rights should be enforceable

  # Advanced GDPR Features
  @compliance @gdpr @privacy-by-design @data-protection-engineering @high @not-implemented
  Scenario: Implement privacy by design and default principles
    Given privacy must be built into systems
    And defaults must protect privacy
    When implementing privacy engineering:
      | Design Principle | Implementation Approach | Default Settings | Architecture Decisions | Testing Methods | Success Metrics |
      | Proactive prevention | Threat modeling | Privacy-protective defaults | Security-first design | Privacy testing | Incident prevention |
      | Privacy as default | Opt-in design | Minimal data sharing | Data segregation | Default testing | User privacy scores |
      | Full functionality | Privacy-preserving features | No privacy tradeoffs | Feature parity | Functionality testing | User satisfaction |
      | End-to-end security | Lifecycle protection | Encrypted by default | Zero-trust architecture | Security testing | Breach prevention |
      | Visibility/transparency | Clear data flows | Transparent processing | Observable systems | Transparency testing | User understanding |
      | User respect | User-centric design | User control defaults | Empowerment features | Usability testing | Control usage |
    Then privacy should be embedded
    And defaults should protect users
    And functionality should be maintained
    And transparency should be achieved

  @compliance @gdpr @legitimate-interests @balancing-test @medium @not-implemented
  Scenario: Conduct legitimate interests assessments
    Given legitimate interests require balancing tests
    And interests must not override rights
    When assessing legitimate interests:
      | Processing Purpose | Legitimate Interest | Necessity Test | Balancing Factors | Safeguards Applied | Final Determination |
      | Fraud prevention | Security interest | Essential for platform | Low privacy impact | Minimal data use | Interest prevails |
      | Service improvement | Business interest | Improves user experience | Moderate impact | Anonymization used | Interest balanced |
      | Direct marketing | Commercial interest | Revenue generation | High privacy impact | Easy opt-out | Rights prevail |
      | Analytics | Operational interest | Service optimization | Data minimized | Aggregation only | Interest prevails |
      | Security monitoring | Security interest | Threat prevention | Proportionate monitoring | Limited retention | Interest prevails |
      | Research | Scientific interest | Public benefit | Pseudonymization | Ethics approval | Interest balanced |
    Then assessments should be thorough
    And balancing should be fair
    And safeguards should be implemented
    And determinations should be documented

  @compliance @gdpr @special-categories @sensitive-data @critical @not-implemented
  Scenario: Handle special categories of personal data
    Given special categories require additional protection
    And explicit consent is generally required
    When processing special categories:
      | Data Category | Lawful Basis Options | Additional Safeguards | Access Restrictions | Retention Limits | Risk Mitigation |
      | Health data | Explicit consent, healthcare | Enhanced encryption | Medical professionals | Medical requirements | Access logging |
      | Biometric data | Explicit consent only | Biometric vault | Authorized systems | Minimal retention | Template storage |
      | Genetic data | Research consent | Anonymization preferred | Research ethics | Study duration | De-identification |
      | Religious beliefs | Explicit consent | Cultural sensitivity | Need-to-know | Minimal retention | Training required |
      | Sexual orientation | Made public by subject | No inferencing | Strict limitations | User-controlled | Anti-discrimination |
      | Children's data | Parental consent | Age verification | Enhanced protection | Minimal retention | Safety measures |
    Then special categories should be identified
    And protections should be enhanced
    And consent should be explicit
    And risks should be mitigated

  @compliance @gdpr @automated-decisions @profiling-rights @high @not-implemented
  Scenario: Manage automated decision-making and profiling
    Given automated decisions require special provisions
    And subjects have right to human intervention
    When implementing automated processing:
      | Decision Type | Human Oversight | Transparency Measures | Subject Rights | Safeguards | Documentation |
      | Risk scoring | Review threshold | Score explanation | Contest decision | Bias testing | Algorithm documentation |
      | Content recommendation | Preference controls | Algorithm transparency | Opt-out option | Fairness audit | Recommendation logic |
      | Access decisions | Appeal process | Decision factors | Human review right | Regular audits | Decision logs |
      | Behavioral analysis | Purpose limitation | Profile visibility | Correction rights | Accuracy checks | Analysis documentation |
      | Performance evaluation | Manager review | Criteria transparency | Challenge process | Calibration | Evaluation records |
      | Eligibility assessment | Override capability | Assessment logic | Explanation right | Validation testing | Assessment audit |
    Then automation should include safeguards
    And transparency should be provided
    And human review should be available
    And fairness should be ensured

  @compliance @gdpr @accountability @compliance-demonstration @high @not-implemented
  Scenario: Demonstrate accountability and compliance
    Given GDPR requires demonstrable compliance
    And accountability must be comprehensive
    When demonstrating accountability:
      | Accountability Area | Evidence Required | Documentation Method | Review Frequency | Stakeholder Access | Improvement Process |
      | Governance structure | Policies and procedures | Document management | Annual review | Auditor access | Gap remediation |
      | Compliance measures | Technical controls | Control documentation | Quarterly assessment | Regulator ready | Control enhancement |
      | Risk management | Risk assessments | Risk register | Continuous update | Risk committee | Risk mitigation |
      | Training programs | Training records | LMS tracking | Training effectiveness | HR verification | Program improvement |
      | Incident handling | Response procedures | Incident database | Post-incident | Investigation access | Process refinement |
      | Third-party management | Vendor assessments | Contract repository | Annual review | Procurement access | Vendor improvement |
    Then accountability should be systematic
    And evidence should be comprehensive
    And reviews should drive improvement
    And compliance should be demonstrable

  @compliance @gdpr @sustainability @evolving-compliance @high @not-implemented
  Scenario: Maintain sustainable GDPR compliance program
    Given GDPR compliance requires ongoing effort
    When ensuring sustainable compliance:
      | Sustainability Factor | Current Challenge | Sustainability Strategy | Resource Requirements | Success Indicators | Long-term Viability |
      | Regulatory evolution | GDPR interpretations | Regulatory monitoring | Legal expertise | Timely adaptations | Adaptive compliance |
      | Technology change | New processing methods | Privacy engineering | Technical resources | Privacy preservation | Tech-ready compliance |
      | Scale growth | Data volume increase | Scalable processes | Automation investment | Maintained protection | Growth-compatible |
      | Rights complexity | Sophisticated requests | Efficient workflows | Process optimization | Timely responses | Sustainable operations |
      | Global coordination | Multi-jurisdiction | Harmonized approach | Global team | Consistent protection | International viability |
      | Cost efficiency | Compliance costs | Process automation | Technology investment | Cost reduction | Financial sustainability |
    Then sustainability should be planned
    And strategies should address challenges
    And resources should enable success
    And compliance should remain effective