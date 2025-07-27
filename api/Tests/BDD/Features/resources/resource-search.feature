Feature: Resource Library Search API Endpoints (FR-002)
  As a therapy professional
  I want to search and discover therapy resources
  So that I can find appropriate materials for my sessions

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"
    And the resource library contains 100,000+ resources

  # GET /api/resources/search
  @endpoint @resources @search @not-implemented
  Scenario: Search resources with keyword
    When I send a GET request to "/api/resources/search?q=fine+motor+kindergarten"
    Then the response status should be 200
    And the response should contain:
      | field      | type   |
      | results    | array  |
      | total      | number |
      | page       | number |
      | limit      | number |
      | facets     | object |
    And each result should contain:
      | field          | type    |
      | id             | string  |
      | title          | string  |
      | description    | string  |
      | thumbnailUrl   | string  |
      | skillAreas     | array   |
      | gradeLevels    | array   |
      | resourceType   | string  |
      | evidenceLevel  | number  |
      | rating         | number  |
      | downloadCount  | number  |
    And results should be sorted by relevance

  @endpoint @resources @search @filters @not-implemented
  Scenario: Search with multiple filters
    When I send a GET request to "/api/resources/search" with parameters:
      | parameter      | value                  |
      | therapyType    | OT,PT                  |
      | skillArea      | bilateral-coordination |
      | ageRange       | 4-6                    |
      | resourceType   | digital-interactive    |
      | evidenceLevel  | 4                      |
      | language       | en,es                  |
    Then the response status should be 200
    And all results should match the filter criteria
    And facets should show available filter options with counts

  @endpoint @resources @search @pagination @not-implemented
  Scenario: Paginate search results
    Given a search returns 150 results
    When I send a GET request to "/api/resources/search?q=sensory&page=2&limit=50"
    Then the response status should be 200
    And the response should contain:
      | field      | value |
      | page       | 2     |
      | limit      | 50    |
      | total      | 150   |
      | totalPages | 3     |
    And results array should contain 50 items

  @endpoint @resources @search @sorting @not-implemented
  Scenario Outline: Sort search results
    When I send a GET request to "/api/resources/search?q=math&sort=<sort>"
    Then the response status should be 200
    And results should be sorted by "<field>" in "<order>" order

    Examples:
      | sort               | field         | order      |
      | relevance         | _score        | descending |
      | newest            | createdAt     | descending |
      | popular           | downloadCount | descending |
      | rating            | rating        | descending |
      | title             | title         | ascending  |

  # GET /api/resources/featured
  @endpoint @resources @discovery @not-implemented
  Scenario: Get featured resources
    When I send a GET request to "/api/resources/featured"
    Then the response status should be 200
    And the response should contain array of featured resources
    And each resource should have "featured" flag true
    And results should be limited to 20 items

  # GET /api/resources/new
  @endpoint @resources @discovery @not-implemented
  Scenario: Get newest resources
    When I send a GET request to "/api/resources/new?days=7"
    Then the response status should be 200
    And all resources should be created within last 7 days
    And results should be sorted by creation date descending

  # GET /api/resources/popular
  @endpoint @resources @discovery @not-implemented
  Scenario: Get popular resources
    When I send a GET request to "/api/resources/popular?period=month"
    Then the response status should be 200
    And results should be sorted by download count descending
    And download counts should be from the last month

  # GET /api/resources/recommended
  @endpoint @resources @ai @not-implemented
  Scenario: Get AI-powered recommendations
    Given I have download history of sensory and handwriting resources
    When I send a GET request to "/api/resources/recommended"
    Then the response status should be 200
    And the response should contain:
      | field          | type  |
      | recommendations| array |
      | basedOn        | array |
      | algorithm      | string|
    And recommendations should be relevant to my usage patterns

  # GET /api/resources/categories
  @endpoint @resources @taxonomy @not-implemented
  Scenario: Get all resource categories
    When I send a GET request to "/api/resources/categories"
    Then the response status should be 200
    And the response should contain hierarchical category tree:
      | field       | type    |
      | id          | string  |
      | name        | string  |
      | slug        | string  |
      | parent      | string  |
      | children    | array   |
      | count       | number  |

  # GET /api/resources/categories/{id}/resources
  @endpoint @resources @taxonomy @not-implemented
  Scenario: Get resources by category
    Given category "fine-motor" exists with 500 resources
    When I send a GET request to "/api/resources/categories/fine-motor/resources"
    Then the response status should be 200
    And all resources should belong to "fine-motor" category
    And subcategory resources should be included

  # GET /api/resources/skills
  @endpoint @resources @taxonomy @not-implemented
  Scenario: Get all skill areas
    When I send a GET request to "/api/resources/skills"
    Then the response status should be 200
    And the response should contain array of:
      | field       | type    |
      | id          | string  |
      | name        | string  |
      | category    | string  |
      | description | string  |
      | ageRanges   | array   |

  # GET /api/resources/grades
  @endpoint @resources @taxonomy @not-implemented
  Scenario: Get all grade levels
    When I send a GET request to "/api/resources/grades"
    Then the response status should be 200
    And the response should contain array of:
      | field      | type   |
      | id         | string |
      | name       | string |
      | ageRange   | string |
      | order      | number |

  # GET /api/resources/therapy-types
  @endpoint @resources @taxonomy @not-implemented
  Scenario: Get therapy types
    When I send a GET request to "/api/resources/therapy-types"
    Then the response status should be 200
    And the response should include:
      | id   | name                        | abbreviation |
      | ot   | Occupational Therapy        | OT           |
      | pt   | Physical Therapy            | PT           |
      | slp  | Speech-Language Pathology   | SLP          |
      | aba  | Applied Behavior Analysis   | ABA          |

  @endpoint @resources @search @performance @not-implemented
  Scenario: Search completes within performance threshold
    When I send a GET request to "/api/resources/search?q=complex+query+with+filters"
    Then the response status should be 200
    And the response time should be less than 2000ms
    And search results should use cached data when available

  @endpoint @resources @search @security @not-implemented
  Scenario: Search respects user permissions
    Given I have a "basic" subscription with resource limits
    When I send a GET request to "/api/resources/search?q=premium"
    Then the response status should be 200
    And premium resources should be marked as "locked"
    And preview-only access should be indicated

  @endpoint @resources @search @empty @not-implemented
  Scenario: Handle no search results
    When I send a GET request to "/api/resources/search?q=xyznonexistent123"
    Then the response status should be 200
    And the response should contain:
      | field   | value |
      | total   | 0     |
      | results | []    |
    And suggested alternatives should be provided

  @endpoint @resources @search @special-characters @not-implemented
  Scenario: Handle special characters in search
    When I send a GET request to "/api/resources/search?q=O%26P+therapy"
    Then the response status should be 200
    And the search should handle "O&P therapy" correctly
    And results should include occupational and physical therapy resources

  # FR-002 Comprehensive Resource Library Business Scenarios from CLAUDE.md
  @resource-search @therapy-specific @performance @not-implemented
  Scenario: Quick search for fine motor kindergarten resources
    Given I am logged in as a Pro subscriber
    And the resource library contains 100,000+ resources
    When I search for "fine motor kindergarten"
    Then results should display within 2 seconds
    And I should see resources matching all criteria:
      | Skill Area  | Fine Motor   |
      | Grade Level | Kindergarten |
    And results should be sorted by relevance
    And each result should show:
      | Element            | Information                |
      | Title              | Resource name              |
      | Preview thumbnail  | Visual preview             |
      | Skill areas        | Tagged skills              |
      | Age range          | Appropriate ages           |
      | Resource type      | Format (PDF, Digital, etc) |
      | Evidence level     | 1-5 star rating           |
    And I should be able to preview without downloading
    And each result should indicate if it's available offline

  @resource-search @complex-filtering @multi-criteria @not-implemented
  Scenario: Complex multi-filter search with evidence level
    Given I am browsing the resource library
    When I apply the following filters:
      | Filter Type    | Selection                    |
      | Therapy Type   | OT, PT                      |
      | Skill Area     | Bilateral Coordination      |
      | Age Range      | 4-6 years                   |
      | Resource Type  | Digital Interactive         |
      | Evidence Level | 4+ stars                    |
      | Language       | English, Spanish            |
    Then only resources matching ALL criteria should display
    And the result count should update dynamically
    And I should be able to save this filter combination as "My Bilateral Search"
    And results should load progressively as I scroll
    And I should see faceted navigation showing:
      | Facet Category | Available Options           |
      | Skills         | Related skill areas         |
      | Age Groups     | Adjacent age ranges         |
      | Authors        | Top contributors in area    |
      | Formats        | Available resource types    |
    And filter combinations should be shareable via URL

  @resource-search @ai-recommendations @personalization @not-implemented
  Scenario: AI-powered resource recommendations based on usage
    Given I have downloaded resources in the past month:
      | Resource Type        | Count |
      | Sensory activities   | 15    |
      | Handwriting sheets   | 8     |
      | Visual schedules     | 12    |
    When I visit the "Recommended for You" section
    Then I should see AI-generated recommendations for:
      | Category           | Reason                           |
      | Sensory resources  | Based on your frequent downloads |
      | Handwriting tools  | Similar to your recent selections|
      | Visual supports    | Popular with similar users       |
    And recommendations should include:
      | Information        | Details                         |
      | Match percentage   | Why this resource suits me      |
      | Usage stats        | Downloaded by X similar users   |
      | Effectiveness      | Success rate data              |
      | Related resources  | Often used together with       |
    And recommendations should update based on my activity
    And I should be able to dismiss recommendations I don't want
    And the system should learn from my dismissals

  @resource-search @organization @favorites @not-implemented
  Scenario: Organizing resources with folders and favorites
    Given I have found useful resources
    When I click the star icon on a resource
    Then it should be added to my favorites
    And I should see a confirmation message
    When I create a new folder called "Sensory Diet Activities"
    And I add 10 favorited resources to this folder
    Then the folder should appear in my sidebar
    And I should be able to:
      | Action              | Result                        |
      | Share folder        | Send link to colleagues       |
      | Export folder       | Download all resources as ZIP |
      | Set folder privacy  | Public, private, or team-only |
      | Add folder notes    | Description of folder purpose |
    And resources should remain accessible offline
    And folder changes should sync across devices

  @resource-search @discovery @trending @not-implemented
  Scenario: Discover trending and new resources
    Given I am exploring the resource library
    When I visit the "Trending This Week" section
    Then I should see resources with high engagement:
      | Metric Type        | Display                       |
      | Most downloaded    | Resources with download count |
      | Highest rated      | 5-star resources this week   |
      | Most shared        | Shared via email/social      |
      | Newest additions   | Published within 7 days      |
    And trending should be filtered by:
      | Filter             | Options                      |
      | My therapy type    | OT, PT, SLP specific        |
      | Age groups I serve | Filter by my typical ages   |
      | Skill areas        | My specialization areas     |
    When I click "What's New"
    Then I should see:
      | Content Type       | Description                  |
      | New resources      | Recently published materials |
      | Updated resources  | Resources with new versions  |
      | Seasonal content   | Holiday/themed materials     |
      | Platform features  | New tools and capabilities   |

  @resource-search @seasonal @themed-content @not-implemented
  Scenario: Search for seasonal and themed content
    Given it is approaching Halloween (October)
    When I search for therapy resources
    Then I should see seasonal suggestions:
      | Content Type       | Examples                     |
      | Halloween themed   | Pumpkin fine motor activities|
      | Fall activities    | Leaf collection gross motor  |
      | October goals      | Autumn-themed IEP materials  |
    And seasonal content should be marked with:
      | Indicator          | Purpose                      |
      | Seasonal badge     | Shows it's time-relevant    |
      | Expiration date    | When content becomes outdated|
      | Cultural variations| Different cultural holidays  |
    When I filter by "Year-round appropriate"
    Then seasonal content should be excluded
    And I should see only evergreen materials

  @resource-search @multilingual @cultural-adaptation @not-implemented
  Scenario: Search resources in multiple languages
    Given I work with Spanish-speaking families
    When I search for "parent handouts"
    And I select language filter "Spanish"
    Then I should see resources with:
      | Language Option    | Features                     |
      | Spanish only       | Fully translated materials   |
      | Bilingual          | English/Spanish side-by-side |
      | Cultural adaptation| Culturally relevant examples |
    And each resource should indicate:
      | Information        | Details                      |
      | Translation quality| Professional vs automated    |
      | Cultural relevance | Appropriate for target culture|
      | Dialect variations | Mexican, Puerto Rican, etc.  |
    When I select a bilingual resource
    Then I should be able to:
      | Action             | Result                       |
      | Preview both languages| See content in both languages|
      | Download separately| Individual language files    |
      | Print options      | Single or dual language     |

  @resource-search @evidence-based @clinical-research @not-implemented
  Scenario: Filter resources by evidence base
    Given I need evidence-based interventions
    When I search for "autism social skills"
    And I filter by evidence level "Research-based"
    Then I should see resources with:
      | Evidence Indicator | Requirements                 |
      | Research citations | Peer-reviewed studies        |
      | Evidence level     | 4-5 star rating             |
      | Outcome data       | Success rates documented     |
      | Clinical trials    | RCT or controlled studies    |
    And each resource should display:
      | Information        | Details                      |
      | Research basis     | Link to supporting studies   |
      | Population tested  | Age groups, diagnoses        |
      | Effectiveness data | Success rates, effect sizes  |
      | Replication studies| How many times validated     |
    When I click "View Research"
    Then I should see:
      | Content            | Format                       |
      | Study abstracts    | Summary of key findings      |
      | Citation format    | APA style references         |
      | Meta-analysis data | Aggregated effectiveness    |

  @resource-search @professional-development @continuing-education @not-implemented
  Scenario: Discover professional development resources
    Given I need continuing education credits
    When I search for "CE courses"
    And I filter by "My State Requirements"
    Then I should see:
      | Course Type        | Features                     |
      | ASHA approved      | For SLP professionals        |
      | AOTA approved      | For OT professionals         |
      | APTA approved      | For PT professionals         |
      | State-specific     | Meets local requirements     |
    And each course should display:
      | Information        | Details                      |
      | CEU credits        | Number of hours available    |
      | Approval numbers   | Official accreditation info  |
      | Prerequisites      | Required background          |
      | Delivery format    | Online, in-person, hybrid    |
      | Cost              | Free, paid, subscription     |
    When I enroll in a course
    Then I should be able to:
      | Action             | Result                       |
      | Track progress     | See completion percentage    |
      | Download certificate| PDF upon completion         |
      | Sync with CE tracker| Automatic record keeping    |

  @resource-search @collaborative @team-sharing @not-implemented
  Scenario: Share resources with therapy team
    Given I found a useful resource
    When I click "Share with Team"
    Then I should be able to:
      | Sharing Option     | Details                      |
      | Email to colleagues| Send resource link via email |
      | Add to team library| Share with practice group    |
      | Create presentation| Export for team training     |
      | Discussion thread  | Start conversation about use |
    And sharing should include:
      | Information        | Content                      |
      | My recommendation  | Why I think it's useful      |
      | Usage notes        | How I plan to use it         |
      | Adaptation ideas   | Modifications for our clients|
      | Success stories    | Results from other users     |
    When team members view the resource
    Then they should see:
      | Context            | Display                      |
      | Sharer's notes     | My comments and suggestions  |
      | Team discussion    | Collaborative feedback       |
      | Usage analytics    | How team is using resource   |

  @resource-search @quality-assurance @content-validation @not-implemented
  Scenario: Ensure resource quality and accuracy
    Given I am viewing a therapy resource
    When I check the quality indicators
    Then I should see:
      | Quality Measure    | Information                  |
      | Clinical review    | Reviewed by certified therapist|
      | Accuracy check     | Content verified for errors  |
      | Safety assessment  | Age-appropriate and safe     |
      | Copyright status   | Properly licensed content    |
    And I should be able to:
      | Action             | Purpose                      |
      | Rate resource      | Provide feedback on quality  |
      | Report issues      | Flag problems for review     |
      | Suggest improvements| Recommend modifications     |
      | Verify information | Check clinical accuracy      |
    When I report a quality issue
    Then the system should:
      | Response           | Action                       |
      | Acknowledge report | Confirm receipt of feedback  |
      | Review timeline    | Provide expected resolution  |
      | Follow up          | Update me on investigation   |
      | Correction notice  | Notify if resource updated   |

  @resource-search @accessibility @universal-design @not-implemented
  Scenario: Find accessible resources for diverse needs
    Given I work with students with disabilities
    When I search for "accessible worksheets"
    And I filter by accessibility features:
      | Feature            | Options                      |
      | Visual supports    | High contrast, large print   |
      | Audio compatible   | Screen reader friendly       |
      | Motor adaptations  | Switch accessible            |
      | Cognitive supports | Simplified language          |
      | Sensory friendly   | Reduced stimuli              |
    Then I should see resources that:
      | Accessibility      | Implementation               |
      | Meet WCAG standards| Web accessibility compliant |
      | Offer multiple formats| PDF, HTML, audio versions |
      | Include alt text   | Image descriptions provided  |
      | Support assistive tech| Compatible with AT devices|
    And each resource should indicate:
      | Information        | Details                      |
      | Accessibility level| AA, AAA compliance          |
      | Supported devices  | Screen readers, switches     |
      | Adaptation options | Available modifications      |

  @resource-search @outcome-tracking @effectiveness @not-implemented
  Scenario: Track resource effectiveness and outcomes
    Given I have used resources with students
    When I access my "Resource Analytics"
    Then I should see:
      | Metric             | Information                  |
      | Usage frequency    | How often I use each resource|
      | Student engagement | Participation rates          |
      | Goal achievement   | Progress toward IEP goals    |
      | Time efficiency    | How long activities take     |
    And I should be able to:
      | Action             | Result                       |
      | Rate effectiveness | Score impact on student goals|
      | Add usage notes    | Record modifications made    |
      | Share outcomes     | Help other therapists learn  |
      | Request similar    | Find resources with same impact|
    When I mark a resource as "Highly Effective"
    Then the system should:
      | Response           | Action                       |
      | Boost in search    | Prioritize in my results     |
      | Recommend to others| Suggest to similar users     |
      | Track patterns     | Learn from successful uses   |

  # FR-002 Additional Comprehensive Business Workflow Scenarios
  @resource-library @advanced-search @clinical-context @workflow @not-implemented
  Scenario: Advanced search with clinical context and treatment planning integration
    Given I am planning therapy for student "Maya Thompson" with specific IEP goals
    And Maya's current goals include:
      | Goal Area          | Specific Target                 | Timeline   |
      | Fine Motor         | Pencil grip improvement         | 3 months   |
      | Bilateral Coord    | Cutting along curved lines      | 4 months   |
      | Visual Perception  | Shape discrimination            | 6 months   |
    When I use the "Goal-Aligned Resource Search"
    And the system analyzes Maya's goals and current performance level
    Then I should receive contextualized search results:
      | Resource Category  | Alignment Score | Rationale                  |
      | Pencil grip cards  | 95%            | Direct goal alignment      |
      | Cutting practice   | 90%            | Progressive skill building |
      | Shape puzzles      | 85%            | Addresses visual perception|
    And each resource should show:
      | Context Element    | Information                     |
      | Goal alignment     | Which specific IEP goals addressed|
      | Difficulty level   | Matched to Maya's current ability|
      | Progress tracking  | Built-in data collection tools  |
      | Time requirement   | Fits within 30-min sessions     |
    When I select multiple aligned resources
    Then I should be able to:
      | Action             | Result                          |
      | Create session plan| Auto-generate weekly schedule   |
      | Track usage        | Link resources to goals         |
      | Monitor progress   | See goal achievement metrics    |
      | Adjust difficulty  | Resources adapt to performance  |

  @resource-library @bulk-operations @school-district @workflow @not-implemented
  Scenario: District-wide resource management and bulk operations
    Given I am a district therapy coordinator managing 50 therapists
    And our district has specific curriculum requirements
    When I access the "District Resource Management" portal
    Then I should be able to perform bulk operations:
      | Operation Type     | Scope                           |
      | Bulk licensing     | Purchase 500 copies of resource |
      | Access control     | Set permissions by school       |
      | Usage monitoring   | Track district-wide adoption    |
      | Compliance check   | Ensure curriculum alignment     |
    And I should see district analytics:
      | Metric             | Visualization                   |
      | Resource usage     | Heat map by school/therapist    |
      | Student outcomes   | Correlation with resource use   |
      | Cost per student   | ROI analysis by resource type   |
      | Adoption rates     | Which resources actually used   |
    When I identify high-impact resources
    Then I should be able to:
      | Action             | Implementation                  |
      | Mandate usage      | Add to required resource list   |
      | Provide training   | Schedule PD on resource use     |
      | Share best practices| Distribute success stories     |
      | Budget allocation  | Prioritize effective resources  |

  @resource-library @offline-sync @mobile-first @workflow @not-implemented
  Scenario: Comprehensive offline resource management for field work
    Given I work in multiple locations with limited internet
    And I need resources available offline on my tablet
    When I use the "Offline Resource Manager"
    Then I should be able to configure:
      | Setting            | Options                         |
      | Auto-download      | Resources for tomorrow's students|
      | Storage limit      | 5GB with smart management       |
      | Priority system    | Frequently used stay cached     |
      | Update schedule    | Sync when on WiFi only          |
    And offline functionality should include:
      | Feature            | Capability                      |
      | Full search        | Search downloaded resources     |
      | Data collection    | Store locally, sync later       |
      | Annotations        | Add notes to resources offline  |
      | Print queue        | Queue jobs for later printing   |
    When I work offline for a full day
    Then sync process should:
      | Sync Element       | Behavior                        |
      | Prioritize data    | Student data syncs first        |
      | Conflict resolution| Handle simultaneous edits       |
      | Progress indication| Show sync status clearly        |
      | Error recovery     | Resume interrupted syncs        |

  @resource-library @content-curation @quality-control @workflow @not-implemented
  Scenario: Community-driven content curation and quality improvement
    Given I am an experienced therapist with 15 years practice
    When I join the "Resource Review Board"
    Then I should be able to participate in:
      | Activity           | Contribution                    |
      | Clinical review    | Validate therapeutic accuracy   |
      | Effectiveness rating| Share outcome data             |
      | Adaptation sharing | Upload my modifications         |
      | Translation help   | Provide cultural context        |
    And the review process should include:
      | Review Stage       | Requirements                    |
      | Initial screening  | Check for safety and accuracy   |
      | Clinical validation| Verify evidence-based approach  |
      | Field testing      | Pilot with select therapists    |
      | Final approval     | Board consensus required        |
    When I submit a resource adaptation
    Then the system should:
      | Process Step       | Action                          |
      | Attribution        | Credit me as contributor        |
      | Version control    | Maintain original and adapted   |
      | Impact tracking    | Monitor usage of my version     |
      | Recognition        | Award contributor badges        |

  @resource-library @predictive-search @ai-enhancement @workflow @not-implemented
  Scenario: AI-enhanced predictive search and resource discovery
    Given I have consistent patterns in my resource usage
    And the AI has learned my preferences over 6 months
    When I start typing in the search box
    Then predictive features should activate:
      | AI Feature         | Functionality                   |
      | Auto-complete      | Suggest based on my history     |
      | Query expansion    | Add related terms automatically |
      | Context awareness  | Consider time of year/day       |
      | Student matching   | Suggest based on current caseload|
    And search predictions should consider:
      | Context Factor     | Influence on Results            |
      | Day of week        | Monday = week planning resources|
      | Time of year       | September = assessment tools    |
      | Recent IEPs        | New goal areas highlighted      |
      | Colleague activity | What my team is using           |
    When I select an AI suggestion
    Then the system should:
      | Learning Action    | Improvement                     |
      | Refine model       | Better future predictions       |
      | Expand suggestions | Related resources appear        |
      | Save time          | Reduce clicks to find resources |