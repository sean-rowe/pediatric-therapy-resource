Feature: Clinical Supervision and Education API Endpoints (FR-036)
  As a clinical educator or supervisor
  I want comprehensive supervision tools
  So that I can effectively train future therapists

  Background:
    Given the API is available
    And I am authenticated as "supervisor@university.edu"
    And I have clinical instructor credentials

  # POST /api/supervision/students
  @endpoint @supervision @student-management @not-implemented
  Scenario: Add student clinician to supervision
    When I send a POST request to "/api/supervision/students" with:
      | field              | value                         |
      | studentEmail       | student@university.edu        |
      | studentName        | Ashley Chen                   |
      | program            | MS-SLP                        |
      | semester           | Spring 2024                   |
      | placementSite      | Pediatric Therapy Clinic      |
      | startDate          | 2024-01-15                    |
      | endDate            | 2024-05-15                    |
      | requiredHours      | 150                           |
      | supervisorRatio    | 25%                           |
    Then the response status should be 201
    And the response should contain:
      | field            | type   |
      | supervisionId    | string |
      | competencyFramework | object |
      | documentTemplates | array |

  # GET /api/supervision/students/{studentId}/competencies
  @endpoint @supervision @competency-tracking @not-implemented
  Scenario: Get student competency assessment
    Given student "student-123" is under supervision
    When I send a GET request to "/api/supervision/students/student-123/competencies"
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | clinicalSkills     | object |
      | professionalSkills | object |
      | criticalThinking   | object |
      | culturalCompetence | object |
      | overallProgress    | string |
      | strengthAreas      | array  |
      | growthAreas        | array  |

  # POST /api/supervision/students/{studentId}/competencies
  @endpoint @supervision @evaluation @not-implemented
  Scenario: Update competency ratings
    Given student "student-123" is under supervision
    When I send a POST request to "/api/supervision/students/student-123/competencies" with:
      | field                  | value                        |
      | evaluationType         | midterm                      |
      | clinicalSkills.assessment | emerging                   |
      | clinicalSkills.intervention | developing               |
      | clinicalSkills.documentation | competent               |
      | professionalSkills.ethics | competent                  |
      | evidence               | ["video-review", "observation"] |
      | comments              | Shows good rapport building   |
    Then the response status should be 200
    And competency progression should be tracked
    And student should receive notification

  # POST /api/supervision/observations
  @endpoint @supervision @direct-observation @not-implemented
  Scenario: Record direct observation session
    When I send a POST request to "/api/supervision/observations" with:
      | field            | value                           |
      | studentId        | student-123                     |
      | observationDate  | 2024-01-22T10:00:00Z           |
      | duration         | 60                              |
      | sessionType      | individual-therapy              |
      | clientAge        | 5                               |
      | disorderType     | articulation                    |
      | observationType  | in-person                       |
      | competenciesObserved | ["rapport", "clinical-skills"] |
    Then the response status should be 201
    And the response should contain:
      | field           | type   |
      | observationId   | string |
      | feedbackForm    | string |
      | signatureRequired | boolean |

  # POST /api/supervision/observations/{observationId}/feedback
  @endpoint @supervision @feedback @not-implemented
  Scenario: Provide observation feedback
    Given observation "obs-123" exists
    When I send a POST request to "/api/supervision/observations/obs-123/feedback" with:
      | field              | value                          |
      | strengths          | ["Clear instructions", "Good pacing"] |
      | areasForGrowth     | ["Data collection timing"]     |
      | specificExamples   | [{"time": "10:15", "observation": "Nice use of wait time"}] |
      | actionItems        | ["Practice data collection during play"] |
      | supervisorReflection | Student showing good progress |
      | nextObservationFocus | Data collection accuracy     |
    Then the response status should be 200
    And feedback should be saved
    And student should have access to review

  # POST /api/supervision/video-reviews
  @endpoint @supervision @video-analysis @not-implemented
  Scenario: Create video review session
    When I send a POST request to "/api/supervision/video-reviews" with:
      | field          | value                    |
      | studentId      | student-123              |
      | videoUrl       | secure-video-link        |
      | sessionDate    | 2024-01-20               |
      | duration       | 30                       |
      | reviewScheduled| 2024-01-23T14:00:00Z     |
      | focusAreas     | ["prompting", "reinforcement"] |
    Then the response status should be 201
    And video should be accessible for annotation
    And review session should be scheduled

  # POST /api/supervision/video-reviews/{reviewId}/annotations
  @endpoint @supervision @video-annotation @not-implemented
  Scenario: Add video annotations
    Given video review "review-123" exists
    When I send a POST request to "/api/supervision/video-reviews/review-123/annotations" with:
      | field         | value                           |
      | timestamp     | 125                             |
      | annotation    | Excellent use of visual support |
      | competencyTag | therapeutic-use-of-self         |
      | type          | strength                        |
    Then the response status should be 201
    And annotation should be saved at timestamp
    And be visible during playback

  # GET /api/supervision/students/{studentId}/hours
  @endpoint @supervision @hour-tracking @not-implemented
  Scenario: Get supervision hours summary
    Given student "student-123" has logged hours
    When I send a GET request to "/api/supervision/students/student-123/hours"
    Then the response status should be 200
    And the response should contain:
      | field                | type   |
      | directObservation    | number |
      | indirectSupervision  | number |
      | totalHours           | number |
      | requiredHours        | number |
      | percentComplete      | number |
      | hoursByCategory      | object |
      | signedHours          | array  |

  # POST /api/supervision/hours
  @endpoint @supervision @hour-logging @not-implemented
  Scenario: Log supervision hours
    When I send a POST request to "/api/supervision/hours" with:
      | field              | value                    |
      | studentId          | student-123              |
      | date               | 2024-01-22               |
      | startTime          | 10:00                    |
      | endTime            | 11:30                    |
      | supervisionType    | direct                   |
      | activities         | ["observation", "feedback"] |
      | clientPresent      | true                     |
      | supervisorSignature| digital-signature-id     |
    Then the response status should be 201
    And hours should be calculated
    And require student acknowledgment

  # POST /api/supervision/case-presentations
  @endpoint @supervision @case-study @not-implemented
  Scenario: Submit case presentation
    When I send a POST request to "/api/supervision/case-presentations" with:
      | field              | value                         |
      | studentId          | student-123                   |
      | presentationDate   | 2024-02-15                    |
      | clientInitials     | J.D.                          |
      | diagnosis          | childhood-apraxia             |
      | presentationFormat | oral-with-slides              |
      | materialsSubmitted | ["slides.pdf", "handout.pdf"] |
      | peerReviewers      | 3                             |
    Then the response status should be 201
    And presentation should be scheduled
    And evaluation rubric should be generated

  # GET /api/supervision/students/{studentId}/reports
  @endpoint @supervision @reporting @not-implemented
  Scenario: Generate supervision report
    Given student "student-123" at midterm
    When I send a GET request to "/api/supervision/students/student-123/reports?type=midterm"
    Then the response status should be 200
    And the response should contain:
      | field               | type   |
      | reportUrl           | string |
      | competencySummary   | object |
      | hoursSummary        | object |
      | strengthsNoted      | array  |
      | improvementPlan     | object |
      | supervisorComments  | string |
      | signatureFields     | array  |

  # POST /api/supervision/learning-contracts
  @endpoint @supervision @planning @not-implemented
  Scenario: Create learning contract
    When I send a POST request to "/api/supervision/learning-contracts" with:
      | field            | value                           |
      | studentId        | student-123                     |
      | semester         | Spring 2024                     |
      | learningGoals    | ["Improve assessment skills", "Develop treatment planning"] |
      | activities       | ["Weekly observations", "Case presentations"] |
      | evaluationMethod | ["Direct observation", "Portfolio review"] |
      | timeline         | {"week4": "First observation", "week8": "Midterm eval"} |
    Then the response status should be 201
    And contract should be created
    And require signatures from both parties

  # POST /api/supervision/remediation-plans
  @endpoint @supervision @remediation @not-implemented
  Scenario: Create remediation plan
    Given student "student-123" struggling with competencies
    When I send a POST request to "/api/supervision/remediation-plans" with:
      | field              | value                         |
      | studentId          | student-123                   |
      | concernAreas       | ["clinical-reasoning", "documentation"] |
      | specificObjectives | ["Complete decision tree exercises"] |
      | additionalSupport  | ["Extra supervision", "Peer mentoring"] |
      | timeline           | 4-weeks                       |
      | successCriteria    | ["80% accuracy on assessments"] |
    Then the response status should be 201
    And remediation tracking should begin
    And university should be notified

  # GET /api/supervision/resources
  @endpoint @supervision @resources @not-implemented
  Scenario: Get supervision resources
    When I send a GET request to "/api/supervision/resources?category=evaluation-tools"
    Then the response status should be 200
    And the response should contain:
      | field             | type  |
      | evaluationForms   | array |
      | rubrics           | array |
      | learningModules   | array |
      | videoExamples     | array |
      | bestPractices     | array |

  # POST /api/supervision/group-supervision
  @endpoint @supervision @group @not-implemented
  Scenario: Schedule group supervision
    When I send a POST request to "/api/supervision/group-supervision" with:
      | field          | value                              |
      | studentIds     | ["student-123", "student-456"]     |
      | date           | 2024-01-25T13:00:00Z               |
      | duration       | 90                                 |
      | topic          | Ethics in pediatric practice       |
      | format         | case-discussion                    |
      | materials      | ["ethics-scenarios.pdf"]           |
    Then the response status should be 201
    And meeting should be scheduled
    And students should receive invitation
    And hours should be pre-allocated

  # POST /api/supervision/portfolio-reviews
  @endpoint @supervision @portfolio @not-implemented
  Scenario: Submit portfolio for review
    When I send a POST request to "/api/supervision/portfolio-reviews" with:
      | field           | value                         |
      | studentId       | student-123                   |
      | portfolioItems  | ["assessment-video", "treatment-plan", "progress-note"] |
      | reflections     | ["growth-reflection.pdf"]     |
      | competencyEvidence | {"assessment": ["video1", "report1"]} |
      | submissionType  | final                         |
    Then the response status should be 201
    And portfolio should be queued for review
    And evaluation timeline should be set

  # FR-036 Missing Critical Clinical Supervision Business Workflow Scenarios
  @competency-tracking @skill-development @clinical-education @workflow @not-implemented
  Scenario: Comprehensive competency tracking and progression monitoring for student clinicians
    Given I am supervising graduate student "Ashley Chen" in pediatric speech therapy placement
    And Ashley is in her second semester of clinical practice
    When I conduct systematic competency assessment
    And I evaluate her across all required domains:
      | Competency Domain    | Specific Skills                 | Ashley's Level    | Evidence Required    |
      | Clinical Skills      | Assessment administration       | Emerging          | Direct observation   |
      |                     | Intervention planning           | Developing        | Treatment plans      |
      |                     | Data collection                 | Competent         | Session documentation|
      |                     | Progress monitoring             | Emerging          | Chart reviews        |
      | Professional Skills  | Ethical decision-making         | Competent         | Case discussions     |
      |                     | Communication with families     | Developing        | Parent meetings      |
      |                     | Collaboration with team         | Competent         | IEP participation    |
      |                     | Documentation quality           | Emerging          | Note reviews         |
      | Critical Thinking    | Clinical reasoning              | Developing        | Case presentations   |
      |                     | Problem-solving                 | Emerging          | Difficult cases      |
      |                     | Evidence-based practice         | Developing        | Research integration |
      |                     | Self-reflection                 | Competent         | Supervision discussions|
    Then comprehensive tracking should monitor:
      | Tracking Element     | Implementation                  |
      | Progression over time| Weekly competency ratings       |
      | Evidence documentation| Link specific examples to ratings|
      | Growth trajectory    | Predict readiness for independence|
      | Targeted learning    | Focus on emerging/developing areas|
    And competency development should include:
      | Development Support  | Strategy                        |
      | Targeted learning    | Specific skill-building activities|
      | Progressive challenges| Gradually increase complexity   |
      | Mentorship          | Pair with experienced clinicians|
      | Resource access      | Specialized training materials  |
    When Ashley shows improvement
    Then advancement decisions should consider:
      | Decision Factor      | Requirement                     |
      | Consistency         | Demonstrates skill across multiple cases|
      | Independence        | Performs with minimal guidance  |
      | Quality             | Meets professional standards    |
      | Safety              | Makes sound clinical decisions  |
    And competency portfolio should include:
      | Portfolio Element    | Documentation                   |
      | Skill demonstrations | Video examples of competencies  |
      | Written work         | Case studies and reports        |
      | Reflection papers    | Self-assessment and growth plans|
      | Supervisor feedback  | Detailed progression notes      |

  @video-review @clinical-feedback @reflective-practice @workflow @not-implemented
  Scenario: Comprehensive video review and annotation system for clinical skill development
    Given student clinician "Marcus Rodriguez" recorded therapy session
    And video contains 30-minute pediatric language session
    When I access the secure video review platform
    And I prepare for systematic analysis
    Then I should be able to use comprehensive annotation tools:
      | Annotation Feature   | Functionality                   |
      | Timestamp markers    | Flag specific moments for discussion|
      | Text comments        | Add detailed observations       |
      | Drawing tools        | Highlight positioning or materials|
      | Competency tags      | Link observations to skills     |
      | Rating scales        | Score specific competencies     |
    And video review should cover:
      | Review Category      | Focus Areas                     |
      | Clinical skills      | Assessment technique, intervention quality|
      | Interpersonal skills | Rapport building, communication |
      | Professional behavior| Ethics, documentation, safety  |
      | Technical competence | Use of materials, data collection|
    When I create timestamp annotations
    Then each annotation should include:
      | Annotation Element   | Content                         |
      | Specific timestamp   | 5:32 - "Nice use of wait time" |
      | Competency connection| Links to "Therapeutic use of self"|
      | Feedback type        | Strength, growth area, question |
      | Actionable suggestion| Specific improvement strategy   |
    And collaborative review should enable:
      | Collaboration Feature| Implementation                  |
      | Student self-reflection| Marcus adds own observations   |
      | Peer review          | Other students provide feedback |
      | Supervisor guidance  | Leading questions and prompts   |
      | Action planning      | Specific goals for next session |
    When video review is complete
    Then learning outcomes should include:
      | Learning Outcome     | Achievement                     |
      | Self-awareness       | Student recognizes own performance|
      | Skill refinement     | Specific techniques to improve  |
      | Professional growth  | Understanding of best practices |
      | Confidence building  | Recognition of strengths        |
    And video library should maintain:
      | Library Feature      | Organization                    |
      | Skill exemplars      | Best practice examples          |
      | Common challenges    | Learning from difficulties      |
      | Progress timeline    | Student growth over semester    |
      | Privacy protection   | Secure, limited access         |

  @supervision-hours @accreditation-compliance @clinical-education @workflow @not-implemented
  Scenario: Comprehensive supervision hour tracking and accreditation compliance
    Given I supervise multiple graduate students requiring detailed hour documentation
    And university accreditation requires specific supervision ratios
    When I manage supervision hour compliance
    Then I should track multiple supervision types:
      | Supervision Type     | Ratio Requirement               | Documentation Needed |
      | Direct observation   | 25% of student hours minimum    | Real-time supervision|
      | Indirect supervision | Discussion, planning, feedback  | Structured meetings  |
      | Group supervision    | Max 3 students per session     | Group learning focus |
      | Video review         | Counts as direct when live     | Annotation records   |
    And hour logging should capture:
      | Hour Documentation   | Required Information            |
      | Date and time        | Precise start/end times         |
      | Supervision type     | Direct, indirect, group         |
      | Student names        | All students involved           |
      | Activities conducted | Specific supervision focus      |
      | Client presence      | Whether clients were present    |
      | Topics covered       | Competencies addressed          |
      | Supervisor signature | Digital verification required   |
    When tracking semester progress
    Then compliance monitoring should show:
      | Compliance Metric    | Ashley Chen | Marcus Rodriguez | Status   |
      | Total hours required | 150 hours   | 150 hours       | On track |
      | Direct hours completed| 38 hours   | 42 hours        | Meeting  |
      | Direct percentage    | 25.3%       | 28%             | Compliant|
      | Indirect hours       | 112 hours   | 108 hours       | On track |
      | Signed documentation | 100%        | 98%             | Mostly compliant|
    And accreditation reporting should generate:
      | Report Type          | Content                         |
      | University summary   | All students' hour completion   |
      | Individual transcripts| Detailed hour breakdown per student|
      | Compliance verification| Meeting all accreditation standards|
      | Quality indicators   | Supervision effectiveness metrics|
    When deficiencies are identified
    Then corrective actions should include:
      | Deficiency Type      | Corrective Action               |
      | Insufficient hours   | Additional supervision scheduled|
      | Low direct percentage| Increase observation frequency  |
      | Missing documentation| Supervisor meeting to complete  |
      | Quality concerns     | Enhanced supervision intensity  |

  @learning-plans @individualized-education @remediation @workflow @not-implemented
  Scenario: Create individualized learning plans and remediation strategies for struggling students
    Given student "Jordan Taylor" is having difficulty with clinical reasoning skills
    And mid-semester evaluation shows concerning patterns
    When I develop comprehensive individualized learning plan
    Then learning needs assessment should identify:
      | Learning Need Area   | Specific Challenges             | Evidence                |
      | Assessment skills    | Difficulty selecting tests      | Inappropriate test choices|
      | Clinical reasoning   | Struggles with hypothesis formation| Unclear treatment rationale|
      | Treatment planning   | Goals not measurable or functional| Non-specific objectives |
      | Data interpretation  | Cannot analyze assessment results| Incorrect conclusions   |
    And individualized strategies should include:
      | Learning Strategy    | Implementation                  | Timeline    |
      | Structured mentoring | Pair with expert clinician     | 4 weeks     |
      | Case study method    | Work through decision trees    | 2 weeks     |
      | Observation increase | Shadow experienced therapists  | 3 weeks     |
      | Reflection practice  | Guided self-assessment activities| Ongoing   |
      | Competency coaching  | Targeted skill-building sessions| 6 weeks   |
    When implementing learning plan
    Then progress monitoring should track:
      | Progress Indicator   | Measurement Method              |
      | Skill demonstration  | Direct observation frequency    |
      | Knowledge application| Case presentation quality       |
      | Clinical decisions   | Accuracy of assessment choices  |
      | Self-awareness       | Quality of reflection responses |
    And support resources should include:
      | Resource Type        | Specific Support                |
      | Additional readings  | Evidence-based practice articles|
      | Video examples       | Expert clinician demonstrations |
      | Practice cases       | Graduated complexity scenarios  |
      | Peer collaboration   | Study groups with stronger students|
    When evaluating plan effectiveness
    Then success criteria should measure:
      | Success Criterion    | Target Performance              |
      | Assessment selection | 80% appropriate test choices    |
      | Clinical reasoning   | Clear rationale in 90% of cases|
      | Goal writing         | SMART goals in all treatment plans|
      | Data interpretation  | Accurate conclusions drawn      |
    And if remediation is unsuccessful
    Then escalation procedures should include:
      | Escalation Step      | Action Required                 |
      | University notification| Formal academic concern report|
      | Extended timeline    | Additional semester if needed   |
      | Alternative placement| Different supervision setting   |
      | Program review       | Fitness for profession evaluation|