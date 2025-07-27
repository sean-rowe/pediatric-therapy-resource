Feature: Student Management API Endpoints (FR-012)
  As a therapy professional
  I want to manage my student caseload
  So that I can track progress and assign appropriate resources

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"
    And FERPA compliance is enabled

  # GET /api/students
  @endpoint @students @not-implemented
  Scenario: List all students in caseload
    Given I have 30 students in my caseload
    When I send a GET request to "/api/students?page=1&limit=20"
    Then the response status should be 200
    And the response should contain:
      | field    | type   |
      | students | array  |
      | total    | number |
      | page     | number |
    And each student should contain:
      | field          | type    |
      | id             | string  |
      | firstName      | string  |
      | lastInitial    | string  |
      | grade          | string  |
      | primaryGoals   | array   |
      | lastSession    | string  |
      | progressStatus | string  |
      | accessCode     | string  |

  # POST /api/students
  @endpoint @students @creation @not-implemented
  Scenario: Add new student to caseload
    When I send a POST request to "/api/students" with:
      | field           | value                    |
      | firstName       | Emma                     |
      | lastName        | Johnson                  |
      | dateOfBirth     | 2018-05-15              |
      | grade           | 1st                      |
      | schoolId        | school-123               |
      | parentEmail     | parent@email.com         |
      | diagnosis       | ["autism", "apraxia"]    |
      | therapyType     | SLP                      |
      | frequency       | 2x weekly                |
      | sessionDuration | 30                       |
    Then the response status should be 201
    And the response should contain:
      | field       | type   |
      | studentId   | string |
      | accessCode  | string |
    And parent access should be configured
    And data should be encrypted

  # GET /api/students/{id}
  @endpoint @students @details @not-implemented
  Scenario: Get detailed student profile
    Given student "student-123" is in my caseload
    When I send a GET request to "/api/students/student-123"
    Then the response status should be 200
    And the response should contain:
      | field              | type    |
      | id                 | string  |
      | demographics       | object  |
      | iepGoals           | array   |
      | currentLevel       | object  |
      | accommodations     | array   |
      | therapySchedule    | object  |
      | progressSummary    | object  |
      | assignedResources  | array   |
      | parentAccess       | object  |
      | documents          | array   |

  @endpoint @students @authorization @not-implemented
  Scenario: Cannot access student not in caseload
    Given student "other-student" is not in my caseload
    When I send a GET request to "/api/students/other-student"
    Then the response status should be 403
    And the response should contain error "Access denied"

  # PUT /api/students/{id}
  @endpoint @students @update @not-implemented
  Scenario: Update student information
    Given student "student-123" is in my caseload
    When I send a PUT request to "/api/students/student-123" with:
      | field      | value        |
      | grade      | 2nd          |
      | frequency  | 3x weekly    |
    Then the response status should be 200
    And changes should be logged
    And parent should be notified if requested

  # DELETE /api/students/{id}
  @endpoint @students @discharge @not-implemented
  Scenario: Discharge student from caseload
    Given student "student-123" is in my caseload
    When I send a DELETE request to "/api/students/student-123" with:
      | field          | value                    |
      | dischargeDate  | 2024-06-15              |
      | reason         | Goals met                |
      | finalReport    | report-url              |
    Then the response status should be 200
    And student should be marked as discharged
    And data should be archived per retention policy
    And parent access should be maintained for 30 days

  # POST /api/students/{id}/goals
  @endpoint @students @goals @not-implemented
  Scenario: Add IEP goal to student
    Given student "student-123" is in my caseload
    When I send a POST request to "/api/students/student-123/goals" with:
      | field          | value                                           |
      | goalArea       | Articulation                                    |
      | goalText       | Will produce /r/ in all positions with 80% accuracy |
      | baseline       | 45% accuracy                                    |
      | targetDate     | 2025-05-30                                      |
      | measurementType| percentage                                      |
      | frequency      | Weekly data collection                          |
    Then the response status should be 201
    And goal should be added to student profile
    And progress tracking should be initialized

  # GET /api/students/{id}/goals
  @endpoint @students @goals @not-implemented
  Scenario: Get student's IEP goals
    Given student "student-123" has 3 active goals
    When I send a GET request to "/api/students/student-123/goals"
    Then the response status should be 200
    And the response should contain array of:
      | field         | type   |
      | goalId        | string |
      | goalArea      | string |
      | goalText      | string |
      | currentLevel  | number |
      | targetLevel   | number |
      | progressTrend | string |
      | lastUpdated   | string |

  # PUT /api/students/{id}/goals/{goalId}
  @endpoint @students @goals @not-implemented
  Scenario: Update goal progress
    Given student "student-123" has goal "goal-456"
    When I send a PUT request to "/api/students/student-123/goals/goal-456" with:
      | field         | value                |
      | currentLevel  | 65                   |
      | notes         | Improving steadily   |
      | dataPoints    | [{"date": "2024-01-15", "value": 65}] |
    Then the response status should be 200
    And progress should be recorded
    And trend analysis should be updated

  # POST /api/students/{id}/sessions
  @endpoint @students @sessions @not-implemented
  Scenario: Record therapy session
    Given student "student-123" is in my caseload
    When I send a POST request to "/api/students/student-123/sessions" with:
      | field            | value                           |
      | sessionDate      | 2024-01-22T10:00:00Z           |
      | duration         | 30                              |
      | attendance       | present                         |
      | goalsAddressed   | ["goal-456", "goal-789"]        |
      | activitiesUsed   | ["res-123", "res-124"]          |
      | studentResponse  | engaged                         |
      | progressNotes    | Great session, met all targets  |
      | dataCollected    | true                            |
    Then the response status should be 201
    And session should be recorded
    And resources should be linked
    And billing data should be updated if applicable

  # GET /api/students/{id}/sessions
  @endpoint @students @sessions @not-implemented
  Scenario: Get student session history
    Given student "student-123" has session history
    When I send a GET request to "/api/students/student-123/sessions?limit=10"
    Then the response status should be 200
    And the response should contain recent sessions
    And each session should include:
      | field          | type   |
      | sessionId      | string |
      | date           | string |
      | duration       | number |
      | attendance     | string |
      | summary        | string |
      | resourcesUsed  | array  |

  # POST /api/students/{id}/assign-resources
  @endpoint @students @resources @not-implemented
  Scenario: Assign resources to student
    Given student "student-123" is in my caseload
    When I send a POST request to "/api/students/student-123/assign-resources" with:
      | field        | value                    |
      | resourceIds  | ["res-123", "res-124"]   |
      | purpose      | Home practice            |
      | dueDate      | 2024-01-29               |
      | instructions | Complete 1 page daily    |
      | parentNotify | true                     |
    Then the response status should be 200
    And resources should be assigned
    And parent should receive notification with access code

  # GET /api/students/{id}/assigned-resources
  @endpoint @students @resources @not-implemented
  Scenario: Get student's assigned resources
    Given student "student-123" has assigned resources
    When I send a GET request to "/api/students/student-123/assigned-resources"
    Then the response status should be 200
    And the response should contain:
      | field       | type  |
      | active      | array |
      | completed   | array |
      | overdue     | array |
    And each assignment should show completion status

  # POST /api/students/{id}/progress-report
  @endpoint @students @reporting @not-implemented
  Scenario: Generate progress report
    Given student "student-123" has 3 months of data
    When I send a POST request to "/api/students/student-123/progress-report" with:
      | field       | value                    |
      | reportType  | quarterly                |
      | startDate   | 2024-01-01               |
      | endDate     | 2024-03-31               |
      | includeGraphs | true                   |
      | audience    | parent                   |
    Then the response status should be 200
    And the response should contain:
      | field       | type   |
      | reportId    | string |
      | downloadUrl | string |
      | preview     | string |
    And report should include progress graphs

  # POST /api/students/{id}/parent-access
  @endpoint @students @parents @not-implemented
  Scenario: Configure parent access
    Given student "student-123" is in my caseload
    When I send a POST request to "/api/students/student-123/parent-access" with:
      | field           | value               |
      | parentEmail     | newparent@email.com |
      | accessLevel     | view-and-download   |
      | notifyFrequency | weekly              |
      | language        | es                  |
    Then the response status should be 200
    And parent access should be configured
    And welcome email should be sent in Spanish

  # POST /api/students/import
  @endpoint @students @import @not-implemented
  Scenario: Import students from school system
    When I send a POST request to "/api/students/import" with:
      | field         | value                    |
      | source        | clever                   |
      | schoolId      | school-123               |
      | importType    | iep-students             |
      | gradeFilter   | ["K", "1", "2"]          |
    Then the response status should be 202
    And the response should contain:
      | field      | type   |
      | importId   | string |
      | status     | string |
      | totalCount | number |
    And import should process in background

  # GET /api/students/{id}/documents
  @endpoint @students @documents @not-implemented
  Scenario: Get student documents
    Given student "student-123" has documents
    When I send a GET request to "/api/students/student-123/documents"
    Then the response status should be 200
    And the response should contain array of:
      | field        | type   |
      | documentId   | string |
      | documentType | string |
      | uploadedBy   | string |
      | uploadedAt   | string |
      | fileName     | string |
      | accessUrl    | string |

  # POST /api/students/{id}/documents
  @endpoint @students @documents @not-implemented
  Scenario: Upload student document
    Given student "student-123" is in my caseload
    When I send a POST request to "/api/students/student-123/documents"
    And I attach "IEP_2024.pdf" with:
      | field        | value     |
      | documentType | iep       |
      | schoolYear   | 2024-2025 |
    Then the response status should be 201
    And document should be encrypted
    And access should be logged

  # FR-012 Missing Critical Student Management Business Workflow Scenarios
  @student-roster @school-integration @sis-import @workflow @not-implemented
  Scenario: Import comprehensive student roster from school SIS
    Given my school district uses PowerSchool SIS
    And I have import permissions for "Jefferson Elementary"
    When I initiate roster import for the new school year
    And I configure import parameters:
      | Parameter        | Setting                        |
      | School year      | 2024-2025                     |
      | Grade filter     | K, 1, 2, 3, 4, 5              |
      | Service filter   | OT, PT, SLP services only     |
      | Data fields      | Student ID, Name, Grade, IEP   |
    And I map SIS fields to platform fields:
      | SIS Field        | Platform Field                 |
      | Student_ID       | External Student ID            |
      | First_Name       | Student First Name (encrypted) |
      | Last_Name        | Student Last Name (encrypted)  |
      | Grade_Level      | Current Grade                  |
      | IEP_Status       | Has Active IEP (boolean)       |
      | Primary_Disability| Disability Category           |
      | Service_Minutes  | Weekly Service Allocation      |
    Then the system should validate all data before import:
      | Validation Type  | Requirement                    |
      | Required fields  | No missing critical data       |
      | Data format      | Proper types and formats       |
      | Duplicate check  | No duplicate student IDs       |
      | Privacy compliance| FERPA requirements met        |
    When validation passes and I confirm import
    Then the import should process:
      | Processing Step  | Action                         |
      | Encrypt PII      | Names, DOB, IDs encrypted      |
      | Generate codes   | Unique access codes per student|
      | Create profiles  | Individual student records     |
      | Assign therapists| Based on service type          |
      | Audit logging    | Complete import trail          |
    And import summary should display:
      | Import Result    | Count                          |
      | Students imported| 127                           |
      | New students     | 89                            |
      | Updated records  | 38                            |
      | Errors           | 3 (missing grade level)       |
      | Warnings         | 12 (IEP status unclear)       |
    And error handling should provide:
      | Error Support    | Feature                        |
      | Error details    | Specific field issues          |
      | Retry mechanism  | Fix and re-import failed records|
      | Manual override  | Admin can manually resolve     |
      | Rollback option  | Undo import if needed          |

  @parent-access @fast-pins @communication @workflow @not-implemented
  Scenario: Complete parent access workflow with Fast Pins
    Given I have student "Olivia Martinez" on my caseload
    And her parents "Maria and Carlos Martinez" need home practice access
    When I generate parent access for home engagement
    And I configure access parameters:
      | Access Parameter | Setting                        |
      | Access type      | Fast Pin (temporary)           |
      | Duration         | 5 days                         |
      | Resource scope   | This week's assignments only   |
      | Data visibility  | Progress summary graphs only   |
      | Download rights  | Watermarked PDFs only          |
      | Language         | Spanish                        |
    Then the system should generate secure access:
      | Security Feature | Implementation                 |
      | Fast PIN code    | 6-digit: 847291               |
      | Expiration timer | Automatic 5-day cutoff        |
      | IP tracking      | Log access locations           |
      | Session limits   | Max 3 concurrent sessions      |
    And parent communication should include:
      | Communication    | Content                        |
      | Email template   | Spanish instructions           |
      | SMS option       | PIN delivery method            |
      | Access guide     | Step-by-step screenshots       |
      | Support contact  | Therapist direct line          |
    When parents use the Fast Pin
    Then they should have access to:
      | Parent View      | Availability                   |
      | Weekly assignments| Current week only             |
      | Progress graphs  | Visual summary data           |
      | Resource downloads| Watermarked, tracked         |
      | Secure messaging | Contact therapist form        |
      | Session notes    | Simplified summaries          |
    And access should be monitored:
      | Monitoring       | Tracking                       |
      | Usage analytics  | Time spent, resources viewed  |
      | Download tracking| Which materials accessed       |
      | Engagement metrics| Frequency of access          |
      | Auto-notifications| Alert therapist of activity  |
    When Fast Pin expires
    Then system should:
      | Expiration Action| Result                         |
      | Block access     | Immediate PIN deactivation     |
      | Generate report  | Usage summary for therapist    |
      | Offer renewal    | Extend access option           |
      | Archive data     | Maintain access logs           |

  @goal-tracking @iep-alignment @resource-correlation @workflow @not-implemented
  Scenario: Comprehensive IEP goal tracking with resource alignment
    Given student "James Chen" has multiple IEP goals
    And I need to track progress systematically
    When I access his comprehensive goal tracking dashboard
    Then I should see detailed goal organization:
      | Goal Area        | Specific Goal                  | Current Progress | Target Date |
      | Articulation     | /r/ in all positions 80% accuracy| 67%          | 05/30/2025  |
      | Language         | 4-word utterances consistently | 73%             | 05/30/2025  |
      | Social Skills    | Turn-taking in games           | 48%             | 05/30/2025  |
      | Fine Motor       | Handwriting legibility         | 55%             | 05/30/2025  |
    When I assign resources to specific goals
    Then the system should provide:
      | Resource Feature | Implementation                 |
      | Smart suggestions| Auto-match resources to goals  |
      | Usage tracking   | Which resources used when      |
      | Effectiveness data| Resource impact on progress   |
      | Goal alignment   | Visual connection indicators   |
    And progress correlation analysis should show:
      | Analysis Type    | Insight Provided               |
      | Resource impact  | Which materials most effective |
      | Session patterns | Optimal frequency/duration     |
      | Skill transfer   | Generalization across contexts |
      | Trend prediction | Projected goal achievement     |
    When I document session activities
    Then goal progress should auto-update:
      | Auto-Update      | Source                         |
      | Data collection  | Session performance scores     |
      | Resource usage   | Materials effectiveness rating |
      | Attendance impact| Consistency correlation        |
      | Engagement level | Student motivation factors     |
    And IEP reporting should generate:
      | Report Element   | Content                        |
      | Progress graphs  | Visual trends over time        |
      | Goal status      | On track, behind, ahead        |
      | Resource summary | Most effective interventions   |
      | Recommendation   | Next steps for each goal       |

  @caseload-organization @group-therapy @scheduling @workflow @not-implemented
  Scenario: Advanced caseload organization and group therapy management
    Given I manage a diverse caseload of 45 students
    And I need to optimize service delivery
    When I access my caseload management dashboard
    Then I should see comprehensive organization:
      | View Type        | Information Displayed          |
      | Grade level      | Students grouped K-5           |
      | Service type     | OT, PT, SLP breakdown         |
      | Goal areas       | Common skill focuses          |
      | Frequency needs  | 1x, 2x, 3x weekly students   |
      | Group potential  | Students with similar goals    |
    When I plan group therapy sessions
    Then the system should suggest optimal groupings:
      | Grouping Factor  | Consideration                  |
      | Skill compatibility| Similar goal areas           |
      | Social dynamics  | Age and personality match     |
      | Schedule alignment| Available time slots         |
      | Progress levels  | Appropriate challenge level   |
    And group management should include:
      | Group Feature    | Functionality                  |
      | Session planning | Multi-student goal addressing  |
      | Individual tracking| Progress per student in group|
      | Resource sharing | Materials work for all levels |
      | Parent communication| Group and individual updates|
    When I schedule group sessions
    Then scheduling optimization should:
      | Optimization     | Benefit                        |
      | Maximize attendance| Consider student schedules    |
      | Balance caseload | Distribute individual/group   |
      | Resource efficiency| Share materials across groups|
      | Documentation ease| Streamlined group notes      |
    And progress monitoring should maintain:
      | Individual Data  | Group Data                     |
      | Personal goals   | Group dynamics                 |
      | Skill development| Social interaction skills      |
      | Attendance       | Group cohesion                 |
      | Family satisfaction| Communication effectiveness  |

  @student-transitions @data-continuity @workflow @not-implemented
  Scenario: Student transition and data continuity management
    Given student "Sarah Kim" is transitioning to middle school
    And I need to ensure seamless service continuity
    When I initiate the transition process
    Then I should complete comprehensive transition planning:
      | Transition Element| Preparation Required           |
      | Record compilation| Complete service history       |
      | Progress summary  | Current levels and trends      |
      | Goal recommendations| Future focus areas           |
      | Service specifications| Recommended frequency/type   |
    And transition documentation should include:
      | Document Type    | Content                        |
      | Service summary  | 3-year progress overview       |
      | Effective strategies| What works best for student  |
      | Resource recommendations| Successful materials used |
      | Family communication| Preferred contact methods    |
    When I transfer data to receiving school
    Then data transfer should ensure:
      | Transfer Requirement| Implementation               |
      | FERPA compliance   | Proper authorization         |
      | Complete records   | No data loss                 |
      | Secure transmission| Encrypted transfer           |
      | Receipt confirmation| Acknowledgment required     |
    And continuity support should provide:
      | Support Type     | Duration                       |
      | Consultation     | 30-day transition support     |
      | Training         | New therapist orientation     |
      | Progress monitoring| First quarter check-ins     |
      | Troubleshooting  | Implementation guidance       |
    When transition is complete
    Then system should:
      | Completion Task  | Action                         |
      | Archive records  | Secure long-term storage       |
      | Update status    | Mark as transitioned          |
      | Generate report  | Transition success summary     |
      | Maintain access  | Emergency record retrieval     |

  @parent-engagement @multilingual-families @workflow @not-implemented
  Scenario: Enhanced parent engagement for multilingual families
    Given I work with diverse families speaking different languages
    And I need to maximize parent engagement in therapy
    When I develop parent engagement strategies
    Then I should have culturally responsive approaches:
      | Cultural Element | Adaptation Strategy            |
      | Language barriers| Professional interpretation    |
      | Communication style| Formal vs informal approach   |
      | Family structure | Extended family involvement    |
      | Educational background| Adjust complexity level     |
      | Work schedules   | Flexible communication times   |
    And communication tools should support:
      | Tool Type        | Multilingual Feature           |
      | Progress reports | Auto-translate to family language|
      | Resource instructions| Visual + translated text    |
      | Video messages   | Captions in native language    |
      | Home activities  | Culturally adapted examples    |
    When I send communications to families
    Then the system should:
      | Communication Feature| Implementation               |
      | Auto-detect language| Use family preference       |
      | Cultural sensitivity| Appropriate formality level |
      | Visual supports     | Universal symbols and images|
      | Timing optimization | Respect cultural schedules  |
    And engagement tracking should monitor:
      | Engagement Metric| Measurement                    |
      | Response rates   | Family communication frequency |
      | Resource usage   | Home practice completion       |
      | Meeting attendance| IEP and conference participation|
      | Satisfaction     | Regular feedback collection    |
    When families need additional support
    Then system should provide:
      | Support Option   | Availability                   |
      | Interpreter services| Professional translation     |
      | Cultural liaisons| Community partnership        |
      | Extended timelines| Accommodation for work schedules|
      | Technology support| Help with digital access     |

  @data-privacy @ferpa-compliance @audit-trail @workflow @not-implemented
  Scenario: Comprehensive data privacy and FERPA compliance
    Given student data must meet strict privacy requirements
    And I need to maintain complete compliance
    When I access any student information
    Then privacy protections should include:
      | Privacy Control  | Implementation                 |
      | Role-based access| Only assigned students visible |
      | Encryption       | All PII encrypted at rest      |
      | Audit logging    | Complete access trail          |
      | Session timeout  | Auto-logout after inactivity   |
    And data handling should ensure:
      | Handling Requirement| Compliance Feature           |
      | Need-to-know access| Minimal necessary data       |
      | Purpose limitation | Use only for therapy planning |
      | Retention limits   | Auto-delete per policy       |
      | Consent tracking   | Parent permissions documented|
    When sharing data with authorized parties
    Then sharing controls should verify:
      | Verification Step| Requirement                    |
      | Authorization    | Proper consent on file        |
      | Purpose validation| Educational use only          |
      | Recipient approval| Authorized school personnel   |
      | Transfer security| Encrypted transmission        |
    And compliance monitoring should track:
      | Monitoring Area  | Tracking Method                |
      | Access patterns  | Unusual activity detection     |
      | Data modifications| Complete change history       |
      | Sharing incidents| Log all external transfers     |
      | Policy violations| Automatic flag and investigate|
    When compliance issues arise
    Then incident response should:
      | Response Action  | Timeline                       |
      | Immediate containment| Stop unauthorized access    |
      | Investigation    | Determine scope within 24hrs  |
      | Notification     | Inform administration/parents  |
      | Remediation      | Fix vulnerabilities           |
      | Documentation    | Complete incident report       |
    And ongoing compliance should include:
      | Compliance Activity| Frequency                     |
      | Staff training   | Annual FERPA certification     |
      | System audits    | Quarterly security reviews     |
      | Policy updates   | Stay current with regulations  |
      | Parent education | Rights and privacy information |