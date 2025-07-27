Feature: AI Content Generation API Endpoints (FR-006, FR-007)
  As a therapy professional
  I want to generate custom therapy materials using AI
  So that I can create personalized resources for specific needs

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"
    And I have AI generation credits available

  # POST /api/ai/generate
  @endpoint @ai @generation @not-implemented
  Scenario: Generate custom worksheet with AI
    When I send a POST request to "/api/ai/generate" with:
      | field         | value                                    |
      | resourceType  | worksheet                                |
      | title         | Dinosaur-Themed Fine Motor Practice      |
      | description   | Tracing and cutting practice with dinosaurs |
      | targetAge     | 5-6                                      |
      | skillFocus    | ["fine-motor", "pencil-grip", "cutting"] |
      | theme         | dinosaurs                                |
      | difficulty    | beginner                                 |
      | language      | en                                       |
      | pages         | 3                                        |
    Then the response status should be 202
    And the response should contain:
      | field        | type   |
      | generationId | string |
      | status       | string |
      | estimatedTime| number |
      | creditsUsed  | number |
    And generation job should be queued

  @endpoint @ai @generation @limits @not-implemented
  Scenario: Enforce generation limits
    Given I have 0 AI generation credits remaining
    When I send a POST request to "/api/ai/generate" with any valid data
    Then the response status should be 403
    And the response should contain error "Insufficient AI generation credits"
    And upgrade options should be provided

  # GET /api/ai/generate/{id}/status
  @endpoint @ai @generation @status @not-implemented
  Scenario: Check generation status
    Given I have initiated generation "gen-123"
    When I send a GET request to "/api/ai/generate/gen-123/status"
    Then the response status should be 200
    And the response should contain:
      | field       | type   |
      | status      | string |
      | progress    | number |
      | currentStep | string |
      | preview     | string |

  # GET /api/ai/generate/{id}/result
  @endpoint @ai @generation @result @not-implemented
  Scenario: Get completed generation
    Given generation "gen-123" is complete
    When I send a GET request to "/api/ai/generate/gen-123/result"
    Then the response status should be 200
    And the response should contain:
      | field         | type   |
      | resourceId    | string |
      | downloadUrl   | string |
      | previewUrls   | array  |
      | metadata      | object |
      | qualityScore  | number |

  # POST /api/ai/generate/{id}/approve
  @endpoint @ai @generation @approval @not-implemented
  Scenario: Approve AI generated content
    Given generation "gen-123" is complete and pending approval
    When I send a POST request to "/api/ai/generate/gen-123/approve" with:
      | field    | value                          |
      | approved | true                           |
      | feedback | Excellent quality, using it!   |
    Then the response status should be 200
    And the resource should be added to my library
    And AI model feedback should be recorded

  @endpoint @ai @generation @rejection @not-implemented
  Scenario: Reject AI generated content
    Given generation "gen-123" is complete
    When I send a POST request to "/api/ai/generate/gen-123/approve" with:
      | field    | value                              |
      | approved | false                              |
      | feedback | Spelling errors in word list       |
      | issues   | ["spelling", "age-inappropriate"] |
    Then the response status should be 200
    And credits should be refunded
    And quality feedback should be recorded

  # POST /api/ai/generate/{id}/regenerate
  @endpoint @ai @generation @retry @not-implemented
  Scenario: Regenerate with modifications
    Given I rejected generation "gen-123"
    When I send a POST request to "/api/ai/generate/gen-123/regenerate" with:
      | field         | value                    |
      | modifications | Fix spelling errors      |
      | keepElements  | ["layout", "images"]     |
    Then the response status should be 202
    And a new generation should begin
    And only partial credits should be charged

  # POST /api/ai/templates
  @endpoint @ai @templates @not-implemented
  Scenario: Create AI generation template
    When I send a POST request to "/api/ai/templates" with:
      | field        | value                                |
      | name         | My Sensory Diet Template             |
      | resourceType | visual-schedule                      |
      | defaults     | {"theme": "ocean", "pages": 2}      |
      | prompts      | ["Include proprioceptive activities"] |
    Then the response status should be 201
    And the template should be saved
    And be available for future generations

  # GET /api/ai/templates
  @endpoint @ai @templates @not-implemented
  Scenario: Get user's AI templates
    Given I have created 5 AI templates
    When I send a GET request to "/api/ai/templates"
    Then the response status should be 200
    And the response should contain array of templates
    And each template should include usage count

  # POST /api/ai/enhance
  @endpoint @ai @enhancement @not-implemented
  Scenario: Enhance existing resource with AI
    Given I have a basic worksheet "res-123"
    When I send a POST request to "/api/ai/enhance" with:
      | field         | value                        |
      | resourceId    | res-123                      |
      | enhancements  | ["add-visuals", "color-code"] |
      | style         | kid-friendly                 |
    Then the response status should be 202
    And AI should enhance the existing resource
    And original should be preserved

  # POST /api/ai/analyze
  @endpoint @ai @analysis @not-implemented
  Scenario: Analyze resource with AI
    When I send a POST request to "/api/ai/analyze" with:
      | field      | value    |
      | resourceId | res-123  |
      | analyze    | ["readability", "clinical-accuracy", "age-appropriateness"] |
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | readabilityScore   | number |
      | gradeLevel         | string |
      | clinicalIssues     | array  |
      | ageAppropriate     | boolean|
      | suggestions        | array  |

  # POST /api/ai/adapt
  @endpoint @ai @adaptation @not-implemented
  Scenario: Create adaptive versions with AI
    Given I have resource "res-123" for typical development
    When I send a POST request to "/api/ai/adapt" with:
      | field         | value                      |
      | resourceId    | res-123                    |
      | adaptations   | ["low-vision", "dyslexia"] |
      | maintainGoals | true                       |
    Then the response status should be 202
    And AI should create adapted versions
    And clinical goals should be preserved

  # GET /api/ai/suggestions
  @endpoint @ai @suggestions @not-implemented
  Scenario: Get AI content suggestions
    Given I frequently create fine motor resources
    When I send a GET request to "/api/ai/suggestions"
    Then the response status should be 200
    And the response should contain:
      | field         | type  |
      | suggestions   | array |
      | basedOn       | object|
      | trending      | array |
    And suggestions should be personalized

  # POST /api/ai/session-plan
  @endpoint @ai @planning @not-implemented
  Scenario: Generate AI session plan
    When I send a POST request to "/api/ai/session-plan" with:
      | field        | value                           |
      | studentGoals | ["improve-pencil-grip", "letter-formation"] |
      | sessionLength| 30                              |
      | setting      | individual                      |
      | materials    | ["standard-classroom"]          |
      | studentAge   | 6                               |
    Then the response status should be 200
    And the response should contain:
      | field          | type  |
      | activities     | array |
      | timeBreakdown  | object|
      | resourceLinks  | array |
      | adaptations    | array |

  # POST /api/ai/translate
  @endpoint @ai @translation @not-implemented
  Scenario: AI-powered resource translation
    Given I have English resource "res-123"
    When I send a POST request to "/api/ai/translate" with:
      | field       | value                     |
      | resourceId  | res-123                   |
      | targetLang  | es                        |
      | cultural    | true                      |
      | maintainLayout | true                   |
    Then the response status should be 202
    And AI should translate content
    And cultural adaptations should be made
    And layout should be preserved

  # GET /api/ai/credits
  @endpoint @ai @billing @not-implemented
  Scenario: Get AI generation credits balance
    When I send a GET request to "/api/ai/credits"
    Then the response status should be 200
    And the response should contain:
      | field           | type   |
      | creditsRemaining| number |
      | creditsUsed     | number |
      | resetDate       | string |
      | subscriptionType| string |

  # POST /api/ai/credits/purchase
  @endpoint @ai @billing @not-implemented
  Scenario: Purchase additional AI credits
    When I send a POST request to "/api/ai/credits/purchase" with:
      | field         | value              |
      | creditAmount  | 100                |
      | paymentMethod | saved-card-123     |
    Then the response status should be 200
    And credits should be added immediately
    And receipt should be generated

  # POST /api/ai/feedback
  @endpoint @ai @quality @not-implemented
  Scenario: Submit AI quality feedback
    Given I used AI generation "gen-123"
    When I send a POST request to "/api/ai/feedback" with:
      | field         | value                        |
      | generationId  | gen-123                      |
      | quality       | 4                            |
      | accuracy      | 5                            |
      | usefulness    | 4                            |
      | comments      | Great visuals, minor typo    |
    Then the response status should be 200
    And feedback should improve AI model
    And bonus credits might be awarded

  # Error Condition Scenarios
  @endpoint @ai @error @security @not-implemented
  Scenario: Block inappropriate content requests
    When I send a POST request to "/api/ai/generate" with:
      | field         | value                        |
      | resourceType  | worksheet                    |
      | title         | Inappropriate Content        |
      | description   | Contains harmful material    |
      | targetAge     | 5-6                         |
    Then the response status should be 400
    And the response should contain error "Content policy violation"
    And the request should be flagged for review
    And no credits should be charged

  @endpoint @ai @error @validation @not-implemented
  Scenario Outline: Validate AI generation parameters
    When I send a POST request to "/api/ai/generate" with <field> set to "<value>"
    Then the response status should be 400
    And the response should contain error "<error>"

    Examples:
      | field        | value     | error                          |
      | targetAge    | -5        | Age must be positive           |
      | pages        | 0         | Page count must be at least 1 |
      | pages        | 100       | Page count exceeds maximum     |
      | difficulty   | invalid   | Invalid difficulty level       |
      | language     |           | Language is required           |
      | resourceType |           | Resource type is required      |
      | title        |           | Title is required              |

  @endpoint @ai @error @rate-limiting @not-implemented
  Scenario: Enforce rate limiting for AI generation
    Given I have made 10 AI generation requests in the last minute
    When I send a POST request to "/api/ai/generate" with valid data
    Then the response status should be 429
    And the response should contain error "Rate limit exceeded"
    And the response should include "Retry-After" header
    And no credits should be charged

  @endpoint @ai @error @service-outage @not-implemented
  Scenario: Handle AI service outage gracefully
    Given the AI generation service is unavailable
    When I send a POST request to "/api/ai/generate" with valid data
    Then the response status should be 503
    And the response should contain error "AI service temporarily unavailable"
    And the response should include estimated recovery time
    And request should be queued for retry

  @endpoint @ai @error @timeout @not-implemented
  Scenario: Handle AI generation timeout
    Given AI generation takes longer than 5 minutes
    When I check generation status
    Then the response status should be 200
    And the status should be "timeout"
    And partial credits should be refunded
    And retry option should be offered

  @endpoint @ai @error @corruption @not-implemented
  Scenario: Handle corrupted generation output
    Given AI generation "gen-123" produces corrupted output
    When I send a GET request to "/api/ai/generate/gen-123/result"
    Then the response status should be 500
    And the response should contain error "Generation corrupted"
    And full credits should be refunded
    And regeneration should be offered at no cost

  @endpoint @ai @error @unauthorized @not-implemented
  Scenario: Block unauthorized access to AI features
    Given I am not authenticated
    When I send a POST request to "/api/ai/generate" with valid data
    Then the response status should be 401
    And the response should contain error "Authentication required"

  @endpoint @ai @error @insufficient-permissions @not-implemented
  Scenario: Block access without AI permissions
    Given I am authenticated but lack AI generation permissions
    When I send a POST request to "/api/ai/generate" with valid data
    Then the response status should be 403
    And the response should contain error "AI generation not available for your subscription"
    And upgrade options should be provided

  @endpoint @ai @error @quota-exceeded @not-implemented
  Scenario: Handle daily quota exceeded
    Given I have exceeded my daily AI generation quota
    When I send a POST request to "/api/ai/generate" with valid data
    Then the response status should be 429
    And the response should contain error "Daily quota exceeded"
    And the response should include quota reset time
    And purchase options should be provided

  @endpoint @ai @error @generation-not-found @not-implemented
  Scenario: Handle non-existent generation ID
    When I send a GET request to "/api/ai/generate/non-existent-id/status"
    Then the response status should be 404
    And the response should contain error "Generation not found"

  @endpoint @ai @error @already-approved @not-implemented
  Scenario: Prevent duplicate approval of generation
    Given generation "gen-123" is already approved
    When I send a POST request to "/api/ai/generate/gen-123/approve" with approval
    Then the response status should be 409
    And the response should contain error "Generation already approved"

  @endpoint @ai @error @insufficient-storage @not-implemented
  Scenario: Handle insufficient storage space
    Given my account storage is full
    When I complete an AI generation
    Then the response status should be 507
    And the response should contain error "Insufficient storage space"
    And storage upgrade options should be provided

  @endpoint @ai @error @malformed-request @not-implemented
  Scenario: Handle malformed AI generation requests
    When I send a POST request to "/api/ai/generate" with malformed JSON
    Then the response status should be 400
    And the response should contain error "Invalid JSON format"
    And helpful formatting tips should be provided

  @endpoint @ai @error @content-moderation @not-implemented
  Scenario: Block content that fails moderation
    When I send a POST request to "/api/ai/generate" with content that fails moderation
    Then the response status should be 400
    And the response should contain error "Content failed safety review"
    And the user should be notified of content guidelines
    And the request should be logged for review

  @endpoint @ai @error @concurrent-limit @not-implemented
  Scenario: Limit concurrent AI generations per user
    Given I have 3 AI generations running concurrently
    When I send a POST request to "/api/ai/generate" with valid data
    Then the response status should be 429
    And the response should contain error "Maximum concurrent generations reached"
    And current generation status should be provided

  @endpoint @ai @error @recovery @not-implemented
  Scenario: Test automatic error recovery
    Given AI generation "gen-123" fails due to temporary error
    When the system automatically retries the generation
    Then the generation should complete successfully
    And no additional credits should be charged
    And the user should be notified of the retry

  # FR-006 Comprehensive AI Content Generation Business Scenarios from CLAUDE.md
  @ai-generation @worksheets @personalized-content @not-implemented
  Scenario: Generate custom fine motor worksheet with student interests
    Given I need a worksheet for a student who loves dinosaurs
    When I access the AI generator
    And I specify parameters:
      | Parameter          | Value                                    |
      | Resource Type      | Fine motor worksheet                     |
      | Age Level          | 5-6 years                               |
      | Interest Theme     | Dinosaurs                               |
      | Skill Focus        | Pencil grip, line tracing              |
      | Difficulty         | Beginner                                |
    And I click "Generate Resource"
    Then the AI should create a worksheet within 30 seconds
    And the worksheet should include:
      | Element                  | Requirement                        |
      | Dinosaur illustrations   | Age-appropriate, engaging         |
      | Tracing activities      | Progressive difficulty            |
      | Instructions            | Clear, simple language            |
      | Skill indicators        | Visual cues for pencil grip       |
    And text should be programmatically verified for accuracy
    And I should be able to preview before finalizing
    And one generation credit should be deducted
    And the worksheet should be saved to my library

  @ai-generation @safety-validation @clinical-review @not-implemented
  Scenario: AI generation with clinical safety validation
    Given I request generation of a sensory diet plan
    When I submit parameters:
      | Parameter       | Value                         |
      | Resource Type   | Sensory diet visual schedule  |
      | Sensory Needs   | Proprioceptive, vestibular   |
      | Setting         | Classroom                     |
      | Duration        | Full school day              |
    Then the AI should generate appropriate activities
    And each activity should pass safety validation:
      | Validation Check        | Requirement                   |
      | Age appropriateness    | Safe for specified age       |
      | Equipment needed       | Standard classroom items     |
      | Supervision level      | Clearly indicated           |
      | Contraindications      | Listed if applicable        |
    And a clinician review flag should appear
    And I must approve before student use
    And safety protocols should be embedded in the resource

  @ai-generation @generation-limits @quality-control @not-implemented
  Scenario: Handle generation limits and quality issues
    Given I have 2 generation credits remaining
    When I attempt to generate 3 resources
    Then I should see a warning after the second generation
    And be offered options to:
      | Option                    | Result                      |
      | Purchase more credits     | Add 10 credits for $5       |
      | Upgrade subscription      | Unlimited generations       |
      | Wait for monthly refresh  | Credits reset on billing date|
    When I generate a resource that fails quality check
    Then the generation should not count against my limit
    And I should receive specific feedback:
      | Issue Type         | Feedback Example                    |
      | Spelling error     | "Word 'therapee' should be 'therapy'"|
      | Safety concern     | "Activity may be too advanced"      |
      | Clinical accuracy  | "Technique needs expert review"     |
    And I should be offered a free regeneration

  @ai-generation @hybrid-approach @accuracy-control @not-implemented
  Scenario: Use hybrid AI approach for educational accuracy
    Given I request a math worksheet with word problems
    When I specify:
      | Parameter        | Value                           |
      | Grade Level      | 3rd grade                       |
      | Math Skill       | Addition with regrouping        |
      | Theme           | School supplies                  |
      | Problem Count    | 12 problems                     |
    Then the AI should use hybrid generation:
      | Component        | Generation Method               |
      | Visual design    | AI-generated illustrations     |
      | Problem setup    | AI-generated scenarios         |
      | Mathematical text| Programmatically verified      |
      | Answer key       | Computationally generated       |
    And all text should be:
      | Requirement      | Validation                      |
      | Spell-checked    | No misspellings allowed         |
      | Grade-appropriate| Reading level verified          |
      | Mathematically accurate| Computationally verified  |
    And the resource should include teacher notes

  @ai-generation @special-needs @adaptation @not-implemented
  Scenario: Generate resources adapted for special needs
    Given I work with a student with autism
    When I request AI generation with adaptations:
      | Parameter           | Value                        |
      | Base Resource       | Social story about lunch     |
      | Adaptations        | Visual schedule, simplified text|
      | Sensory Considerations| Calm colors, minimal text   |
      | Communication Level | Picture-supported           |
    Then the AI should create adapted version:
      | Adaptation         | Implementation               |
      | Visual schedule    | Step-by-step picture sequence|
      | Simplified text    | Short, clear sentences      |
      | Calm colors        | Soft blues and greens       |
      | Picture support    | Icon for each major concept |
    And the resource should maintain educational objectives
    And include implementation tips for teachers
    And provide data collection suggestions

  @ai-generation @iep-alignment @goal-focused @not-implemented
  Scenario: Generate resources aligned with IEP goals
    Given I have a student with specific IEP goals:
      | Goal Area          | Specific Goal                     |
      | Written Expression | Write 3-sentence paragraph        |
      | Organization       | Use graphic organizer             |
      | Fine Motor         | Improve handwriting legibility    |
    When I request AI generation aligned with these goals
    Then the AI should create resources that:
      | Alignment Feature  | Implementation                    |
      | Address all goals  | Activities target each goal area |
      | Provide scaffolding| Progressive skill building       |
      | Include assessment | Data collection opportunities    |
      | Suggest modifications| Adaptations for different levels|
    And the resource should include:
      | Component          | Purpose                          |
      | Goal references    | Clear connection to IEP goals    |
      | Progress tracking  | Data collection sheets          |
      | Mastery criteria   | When to advance to next level    |
      | Parent activities  | Home practice suggestions        |

  @ai-generation @multilingual @cultural-adaptation @not-implemented
  Scenario: Generate culturally adapted multilingual resources
    Given I work with Spanish-speaking families
    When I request AI generation for:
      | Parameter          | Value                           |
      | Language           | Spanish                         |
      | Cultural Context   | Mexican-American families       |
      | Resource Type      | Parent communication letter     |
      | Topic             | Home speech practice            |
    Then the AI should create culturally appropriate content:
      | Cultural Element   | Adaptation                      |
      | Language variety   | Mexican Spanish dialect         |
      | Cultural references| Familiar family structures      |
      | Activities         | Home-based, culturally relevant |
      | Communication style| Respectful, family-centered     |
    And the resource should include:
      | Feature            | Implementation                  |
      | Translation quality| Professional-level accuracy     |
      | Cultural sensitivity| Appropriate customs and values |
      | Accessibility      | Various literacy levels         |
      | Visual elements    | Culturally relevant images      |

  @ai-generation @professional-development @continuing-education @not-implemented
  Scenario: Generate professional development content
    Given I need CE materials for my therapy team
    When I request AI generation for:
      | Parameter          | Value                           |
      | Content Type       | Training module                 |
      | Topic             | Feeding therapy techniques      |
      | Audience          | SLPs and OTs                    |
      | Duration          | 2-hour workshop                 |
      | Learning Objectives| Evidence-based practices       |
    Then the AI should create comprehensive materials:
      | Component          | Content                         |
      | Presentation slides| Visual, engaging format         |
      | Handout materials  | Reference guides and checklists |
      | Case studies       | Real-world application examples |
      | Assessment tools   | Knowledge check activities      |
    And the materials should include:
      | Feature            | Requirement                     |
      | CE accreditation   | Meets professional standards    |
      | Evidence base      | Research citations included     |
      | Practical application| Hands-on learning activities  |
      | Evaluation tools   | Feedback and assessment forms   |

  @ai-generation @teletherapy @remote-delivery @not-implemented
  Scenario: Generate teletherapy-optimized resources
    Given I provide teletherapy services
    When I request AI generation for:
      | Parameter          | Value                           |
      | Delivery Method    | Teletherapy                     |
      | Resource Type      | Interactive articulation game  |
      | Age Group         | 6-8 years                       |
      | Technology        | Screen sharing compatible       |
    Then the AI should create teletherapy-specific content:
      | Optimization       | Implementation                  |
      | Screen resolution  | Clear at various sizes          |
      | Interactive elements| Click/tap friendly              |
      | Caregiver support  | Instructions for parent helpers |
      | Engagement features| Attention-holding activities    |
    And the resource should include:
      | Feature            | Purpose                         |
      | Technical requirements| System compatibility info     |
      | Troubleshooting guide| Common issue solutions        |
      | Offline alternatives| Backup activities              |
      | Parent coaching     | Support for caregivers         |

  @ai-generation @assessment-tools @data-collection @not-implemented
  Scenario: Generate assessment and data collection tools
    Given I need to track student progress
    When I request AI generation for:
      | Parameter          | Value                           |
      | Tool Type          | Progress monitoring probe       |
      | Skill Area         | Reading fluency                 |
      | Grade Level        | 2nd grade                       |
      | Assessment Frequency| Weekly                         |
    Then the AI should create assessment tools:
      | Component          | Features                        |
      | Assessment passages| Grade-appropriate text          |
      | Scoring rubrics    | Clear, objective criteria       |
      | Data collection sheets| Easy-to-use forms            |
      | Progress graphs    | Visual progress tracking        |
    And the tools should include:
      | Feature            | Implementation                  |
      | Reliability measures| Validated assessment criteria  |
      | Norm references    | Comparison to grade-level peers |
      | Intervention suggestions| Data-driven recommendations |
      | Parent reporting   | Family-friendly summaries       |

  @ai-generation @collaborative-planning @team-resources @not-implemented
  Scenario: Generate resources for collaborative team planning
    Given I work with multidisciplinary teams
    When I request AI generation for:
      | Parameter          | Value                           |
      | Resource Type      | Team meeting template          |
      | Purpose           | IEP goal coordination           |
      | Team Members      | SLP, OT, PT, Special Ed teacher |
      | Student Profile   | Multiple complex needs          |
    Then the AI should create collaboration tools:
      | Tool Type          | Features                        |
      | Meeting agenda     | Structured discussion format    |
      | Goal alignment sheet| Cross-discipline planning      |
      | Resource sharing   | Common material suggestions     |
      | Progress tracking  | Team accountability measures    |
    And the tools should facilitate:
      | Collaboration Aspect| Implementation                 |
      | Role clarity       | Clear responsibilities         |
      | Information sharing| Efficient data exchange        |
      | Decision making    | Consensus-building tools       |
      | Follow-up planning | Action item tracking           |

  @ai-generation @crisis-intervention @behavior-support @not-implemented
  Scenario: Generate crisis intervention and behavior support materials
    Given I need resources for challenging behaviors
    When I request AI generation for:
      | Parameter          | Value                           |
      | Resource Type      | Behavior intervention plan      |
      | Target Behavior    | Classroom disruption           |
      | Age Group         | Elementary school               |
      | Setting           | Inclusive classroom             |
    Then the AI should create evidence-based materials:
      | Component          | Features                        |
      | Behavior analysis  | ABC data collection sheets      |
      | Intervention strategies| Positive behavior supports    |
      | Crisis protocols   | Step-by-step response plans     |
      | Prevention strategies| Proactive environmental changes|
    And the materials should include:
      | Feature            | Implementation                  |
      | Safety protocols   | Staff and student safety first  |
      | De-escalation techniques| Calming strategies           |
      | Data collection    | Objective behavior tracking     |
      | Team coordination  | Consistent responses across staff|

  @ai-generation @outcome-measurement @effectiveness-tracking @not-implemented
  Scenario: Generate outcome measurement and effectiveness tracking tools
    Given I need to measure intervention effectiveness
    When I request AI generation for:
      | Parameter          | Value                           |
      | Measurement Type   | Pre/post intervention assessment|
      | Skill Domain      | Social communication            |
      | Age Range         | Middle school                   |
      | Intervention Period| 12 weeks                       |
    Then the AI should create measurement tools:
      | Tool Type          | Features                        |
      | Baseline assessment| Initial skill measurement       |
      | Progress probes    | Regular monitoring tools        |
      | Outcome measures   | Post-intervention assessment    |
      | Data analysis      | Progress visualization          |
    And the tools should provide:
      | Feature            | Implementation                  |
      | Statistical analysis| Meaningful change indicators   |
      | Visual reporting   | Charts and graphs               |
      | Clinical significance| Functional improvement measures|
      | Recommendations    | Next steps based on data        |