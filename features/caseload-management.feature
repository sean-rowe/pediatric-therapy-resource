Feature: Caseload Management
  As a therapy administrator or lead therapist
  I want to manage therapist caseloads effectively
  So that workload is balanced and students receive consistent services

  Background:
    Given I am logged in as therapy coordinator
    And the following therapists are active:
      | Therapist      | Service | FTE | Current Caseload |
      | Sarah Johnson  | OT      | 1.0 | 45 students      |
      | Michael Chen   | PT      | 0.8 | 32 students      |
      | Amy Lee        | SLP     | 1.0 | 52 students      |
      | Jessica Brown  | OT      | 0.5 | 20 students      |

  Rule: Caseloads must be balanced and manageable

    Scenario: View caseload distribution
      Given multiple therapists in the practice
      When I view caseload analytics
      Then I see distribution metrics:
        | Metric                    | Value                          |
        | Average OT Caseload      | 42.5 students                  |
        | Average PT Caseload      | 40 students (FTE adjusted)     |
        | Average SLP Caseload     | 52 students                    |
        | Highest Individual Load  | Amy Lee: 52 students           |
        | Service Hour Distribution| Balanced within 10%            |
      And visual charts show:
        | Chart Type         | Information Displayed          |
        | Heat Map          | Caseload by school location    |
        | Bar Graph         | Students per therapist         |
        | Workload Index    | Complexity-adjusted numbers    |

    Scenario: Calculate workload using complexity factors
      Given students have varying needs
      When I view "Sarah Johnson's" workload analysis
      Then I see complexity-adjusted metrics:
        | Student Type              | Count | Weight | Weighted Units |
        | Regular weekly           | 30    | 1.0    | 30.0          |
        | Intensive (2x/week)      | 10    | 2.0    | 20.0          |
        | Consultation only        | 5     | 0.5    | 2.5           |
        | Total Raw              | 45    | -      | -             |
        | Total Weighted         | -     | -      | 52.5          |
      And recommended caseload is 50 weighted units

    Scenario: Identify overloaded therapists
      Given caseload limits are configured
      When system analyzes current assignments
      Then alerts show:
        | Therapist    | Issue                    | Recommendation           |
        | Amy Lee      | 10% over recommended    | Reassign 5 students     |
        | Sarah Johnson| At capacity             | No new assignments      |
      And redistribution suggestions include:
        | From         | To             | Students | Reason               |
        | Amy Lee      | New SLP hire   | 8        | Geographic proximity |

  Rule: Assignment changes must maintain continuity

    Scenario: Transfer student between therapists
      Given "Emma Wilson" is assigned to "Sarah Johnson"
      And "Jessica Brown" has capacity
      When I transfer "Emma Wilson" to "Jessica Brown"
      Then I must provide:
        | Required Info          | Details                        |
        | Transfer Reason       | Therapist schedule change      |
        | Effective Date        | 2024-02-01                    |
        | Transition Plan       | Joint session on 2024-01-30   |
        | Parent Notification   | Automated email sent          |
      And transfer creates:
        | Action                 | Result                         |
        | History Entry         | Maintains therapist timeline   |
        | Document Access       | New therapist gains access     |
        | Calendar Update       | Sessions reassigned           |
        | Handoff Note         | Required from prior therapist |

    Scenario: Bulk reassignment for therapist leave
      Given "Sarah Johnson" is going on leave
      When I initiate coverage plan for "2024-02-01" to "2024-04-30"
      Then system suggests distribution:
        | Student Group      | Suggested Coverage    | Rationale              |
        | Lincoln Elementary | Jessica Brown         | Same school coverage   |
        | Washington Middle  | Agency substitute     | No internal capacity   |
        | High-need students | Maintain with sub     | Consistency critical   |
      And I can:
        | Action             | Details                        |
        | Accept all        | Implements suggestions         |
        | Modify individual | Change specific assignments    |
        | Create groups     | Assign sets to therapists     |

    Scenario: Emergency coverage needed
      Given "Michael Chen" called in sick
      When I access same-day coverage options
      Then system shows:
        | Coverage Option      | Availability           | Impact                |
        | Jessica Brown       | 2 free periods         | Can cover 2 students  |
        | Sarah Johnson       | Lunch period only      | Can cover 1 student   |
        | Reschedule         | Next available slot    | Parent notification   |
        | Teletherapy        | Backup therapist remote| If consent on file    |
      And critical sessions are prioritized

  Rule: Geographic and schedule optimization

    Scenario: Optimize travel between schools
      Given therapists serve multiple locations
      When I run route optimization
      Then system suggests:
        | Therapist      | Current Route                | Optimized Route           | Time Saved |
        | Sarah Johnson  | School A→C→B→D              | School A→B→C→D           | 45 min/week |
        | Michael Chen   | Daily travel between 3 sites | Block scheduling by site | 2 hrs/week  |
      And changes consider:
        | Factor              | Weight                        |
        | Student needs      | High priority                 |
        | IEP requirements   | Must maintain                 |
        | Therapist preference| Considered                   |

    Scenario: Balance schedule density
      Given varying session requirements
      When I analyze "Amy Lee's" schedule
      Then I see:
        | Day       | Sessions | Breaks | Documentation Time | Utilization |
        | Monday    | 12       | 1      | 30 min            | 95%        |
        | Tuesday   | 10       | 2      | 45 min            | 85%        |
        | Wednesday | 11       | 1      | 45 min            | 90%        |
      And recommendations include:
        | Issue                | Solution                      |
        | Monday overloaded   | Move 2 sessions to Thursday   |
        | Insufficient breaks | Block 15-min breaks required  |

  Rule: Caseload planning supports growth

    Scenario: Project staffing needs
      Given current caseload trends
      When I run staffing projection for "Fall 2024"
      Then analysis shows:
        | Metric                     | Current | Projected | Gap    |
        | Total Students            | 149     | 175       | +26    |
        | Required Service Hours    | 4,200   | 4,900     | +700   |
        | Current FTE Capacity      | 3.3     | 3.3       | 0      |
        | Needed FTE               | 3.3     | 3.85      | +0.55  |
      And recommendations include:
        | Option               | Details                        |
        | Hire 0.5 FTE        | Post by April for August start |
        | Increase current FTE | Offer 0.6 to current 0.5 FTE  |

    Scenario: Model impact of new school contract
      Given potential new school with 40 students
      When I model adding "Riverside Elementary"
      Then impact analysis shows:
        | Impact Area          | Assessment                    |
        | Geographic feasibility| 15 miles from nearest site   |
        | FTE Required         | 0.8 additional               |
        | Current Staff Capacity| Cannot absorb               |
        | Revenue Projection   | $125,000 annually           |
        | Recommendation      | Hire dedicated therapist     |

  Rule: Performance metrics guide caseload decisions

    Scenario: Track therapist productivity
      Given productivity expectations exist
      When I view team productivity dashboard
      Then metrics include:
        | Therapist      | Direct Service | Documentation | Productivity |
        | Sarah Johnson  | 85%           | 15%           | Optimal     |
        | Michael Chen   | 75%           | 25%           | Below target |
        | Amy Lee        | 90%           | 10%           | Review needed|
      And drill-down shows:
        | Factor              | Impact on Productivity        |
        | Travel time        | Reduces direct service        |
        | Complex cases      | Increases documentation       |
        | No-shows           | Impacts utilization          |

    Scenario: Monitor outcome quality by caseload
      Given quality metrics are tracked
      When I analyze outcomes by caseload size
      Then data shows:
        | Caseload Range | Goal Achievement | Parent Satisfaction |
        | 30-40 students | 82%             | 4.7/5              |
        | 41-50 students | 78%             | 4.5/5              |
        | 51-60 students | 71%             | 4.1/5              |
      And correlation suggests optimal at 40-45

  Rule: Substitute and coverage management

    Scenario: Maintain substitute pool
      Given substitute therapists available
      When I view substitute management
      Then I see:
        | Substitute      | Credentials | Last Assignment | Availability  |
        | Maria Garcia    | OT, SLP     | 2024-01-10     | M, W, F      |
        | James Wilson    | PT          | 2024-01-05     | All days     |
        | Contract Agency | All types   | On-demand      | 24hr notice  |
      And performance ratings available

    Scenario: Plan maternity leave coverage
      Given "Amy Lee" announces 12-week leave
      When I create coverage plan
      Then timeline includes:
        | Phase               | Timeline        | Action                   |
        | Knowledge transfer  | 6 weeks before  | Document all students   |
        | Introduce substitute| 4 weeks before  | Joint sessions begin    |
        | Full transition    | 1 week before   | Substitute leads        |
        | Leave period       | 12 weeks        | Check-ins weekly       |
        | Return transition  | 1 week after    | Joint sessions resume  |
      And all stakeholders notified appropriately