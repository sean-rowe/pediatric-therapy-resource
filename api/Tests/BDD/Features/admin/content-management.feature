Feature: Content Management Admin Portal (FR-005)
  As a content administrator
  I want to manage the upload, categorization, and quality review of therapy resources
  So that only high-quality, properly categorized resources are available in the library

  Background:
    Given I am logged in as a content administrator
    And the content management system is active
    And quality review workflows are configured

  @content-upload @admin @not-implemented
  Scenario: Content creator uploads new resource with metadata
    Given I am a verified content creator
    And I have a new therapy resource to upload
    When I access the content upload portal
    And I upload the resource file "Fine_Motor_Worksheet.pdf"
    And I provide the required metadata:
      | Field              | Value                           |
      | Title              | Fine Motor Skills Practice     |
      | Description        | Worksheet for ages 4-6         |
      | Resource Type      | Worksheet                       |
      | Skill Areas        | Fine motor, bilateral coordination |
      | Grade Levels       | PreK, Kindergarten             |
      | Evidence Level     | 4 (research-based)             |
      | Languages          | English, Spanish               |
      | License Type       | Platform exclusive             |
    And I submit the resource for review
    Then the resource should be assigned a unique ID
    And the resource should enter the QA review queue
    And I should receive a submission confirmation
    And the resource status should be "Pending Review"

  @peer-review @clinical-accuracy @not-implemented
  Scenario: Peer review process for clinical accuracy
    Given there is a resource "Sensory Diet Activities" in the review queue
    And the resource targets children with autism
    When I assign the resource to a clinical reviewer
    And the reviewer evaluates the resource for:
      | Review Criteria    | Assessment                     |
      | Clinical accuracy  | Techniques are evidence-based  |
      | Age appropriateness| Activities suit target age     |
      | Safety compliance  | No contraindicated activities  |
      | Therapeutic value  | Addresses stated goals         |
    And the reviewer approves the resource
    Then the resource should be marked as "Clinically Reviewed"
    And the resource should progress to the next review stage
    And the reviewer's assessment should be logged
    And the resource creator should be notified of approval

  @copyright-verification @automated-checking @not-implemented
  Scenario: Automated copyright checking system
    Given I upload a resource containing images and text
    When the automated copyright verification runs
    Then the system should check all images against copyright databases
    And the system should scan text for potential copyright violations
    And the system should verify I have rights to use all content
    When a potential copyright issue is detected
    Then the resource should be flagged for manual review
    And I should receive notification of the copyright concern
    And the resource should not be published until resolved
    And suggested alternative content should be provided

  @bulk-upload @efficiency @not-implemented
  Scenario: Bulk content upload for large collections
    Given I am a content partner with 50 resources to upload
    When I access the bulk upload interface
    And I upload a CSV file with metadata for all resources
    And I upload a ZIP file containing all resource files
    Then the system should validate the CSV format
    And the system should match each resource file to its metadata
    And the system should process all uploads in background
    And I should receive progress updates during processing
    When processing is complete
    Then all valid resources should be in the review queue
    And any errors should be reported with specific details
    And I should receive a summary report of the upload results

  @content-retirement @lifecycle-management @not-implemented
  Scenario: Content retirement workflow
    Given there is a published resource "Outdated Therapy Technique"
    And the resource has been flagged as outdated by users
    When I initiate the content retirement process
    Then the resource should be removed from active search results
    And existing downloads should remain accessible to users
    And the resource should be marked as "Retired"
    And users should be notified of the retirement
    And alternative resources should be suggested
    And the retirement should be logged in the audit trail

  @version-control @updates @not-implemented
  Scenario: Version control for resource updates
    Given there is a published resource "Handwriting Guidelines v1.0"
    And the creator submits an updated version "Handwriting Guidelines v2.0"
    When I review the updated version
    And I approve the changes
    Then the new version should be published
    And the old version should be marked as "Superseded"
    And users who downloaded the old version should be notified
    And the version history should be maintained
    And links to the old version should redirect to the new version

  @quality-metrics @review-analytics @not-implemented
  Scenario: Quality review analytics and metrics
    Given I am viewing the content management dashboard
    When I access the quality metrics section
    Then I should see review statistics:
      | Metric                 | Current Value | Target |
      | Average review time    | 2.5 days      | 3 days |
      | Approval rate          | 87%           | 80%    |
      | Resources in queue     | 45            | <50    |
      | Reviewer workload      | 8 per reviewer| <10    |
    And I should see quality trends over time
    And I should be able to identify bottlenecks in the review process
    And I should receive alerts for queue backlogs

  @reviewer-assignment @workload-management @not-implemented
  Scenario: Intelligent reviewer assignment system
    Given there are 20 resources in the review queue
    And I have 5 clinical reviewers with different specialties:
      | Reviewer      | Specialty | Current Workload |
      | Dr. Smith     | SLP       | 3 reviews        |
      | Dr. Johnson   | OT        | 5 reviews        |
      | Dr. Williams  | PT        | 2 reviews        |
      | Dr. Brown     | ABA       | 4 reviews        |
      | Dr. Davis     | General   | 6 reviews        |
    When the auto-assignment system runs
    Then resources should be assigned based on:
      | Priority Factor    | Weight | Rule                         |
      | Specialty match    | High   | Assign to relevant specialist|
      | Workload balance   | Medium | Distribute evenly            |
      | Review urgency     | Medium | Priority items first         |
      | Review complexity  | Low    | Complex items to experienced |
    And no reviewer should exceed 8 active reviews
    And urgent reviews should be assigned within 1 hour

  @content-categories @taxonomy-management @not-implemented
  Scenario: Manage content categorization taxonomy
    Given I am managing the content taxonomy
    When I add a new category "Telehealth Resources"
    And I define subcategories:
      | Subcategory           | Description                    |
      | Virtual Backgrounds   | Therapy-appropriate backgrounds|
      | Digital Manipulatives | Interactive therapy tools      |
      | Screen Sharing Tools  | Collaborative activities       |
    Then the new categories should be available for content tagging
    And existing content should be reviewable for new category assignment
    And the taxonomy should be validated for consistency
    And category usage statistics should be tracked

  @content-search @metadata-quality @not-implemented
  Scenario: Validate content metadata quality
    Given I am reviewing submitted content metadata
    When I check the quality of metadata fields
    Then the system should validate:
      | Field           | Validation Rule                |
      | Title           | Descriptive, under 100 chars  |
      | Description     | Complete, 50-500 chars        |
      | Skill Areas     | From approved taxonomy         |
      | Grade Levels    | Valid educational levels       |
      | Evidence Level  | 1-5 scale with justification   |
      | Languages       | ISO language codes             |
    And incomplete metadata should be flagged for revision
    And metadata quality scores should be calculated
    And creators should receive feedback on metadata improvements