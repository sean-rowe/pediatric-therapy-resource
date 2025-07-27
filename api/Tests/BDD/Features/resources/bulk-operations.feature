Feature: Bulk Resource Operations
  As a therapy professional
  I want to perform bulk operations on resources
  So that I can efficiently manage large sets of materials

  Background:
    Given I am logged in as a therapist
    And I have access to the resource library

  @resources @bulk @download
  Scenario: Bulk download with progress tracking
    Given I have selected 50 resources for download
    And the total size is approximately 500MB
    When I initiate bulk download
    Then I should see a progress bar showing "0% complete"
    And the progress should update in real-time
    And downloads should be packaged in a zip file named "resources_[timestamp].zip"
    And failed downloads should be retried up to 3 times
    And I should receive a notification when complete
    And the zip should contain a manifest.json with resource metadata

  @resources @bulk @download @large
  Scenario: Handle large bulk downloads with chunking
    Given I have selected 200 resources totaling 2GB
    When I initiate bulk download
    Then the system should split into multiple zip files of max 500MB each
    And each zip should be numbered sequentially
    And a master manifest should list all zip files
    And downloads should support resume on interruption

  @resources @bulk @favorites
  Scenario: Bulk add resources to favorites
    Given I have search results showing 25 resources
    When I select all resources
    And I click "Add to Favorites"
    Then all 25 resources should be added to my favorites
    And I should see a success message "25 resources added to favorites"
    And the operation should complete within 5 seconds
    And duplicate favorites should be ignored

  @resources @bulk @folders
  Scenario: Bulk organize resources into folders
    Given I have selected 30 resources
    And I have existing folders:
      | folderName          | resourceCount |
      | Fine Motor Skills   | 15            |
      | Gross Motor Skills  | 20            |
      | Speech Activities   | 10            |
    When I choose "Move to Folder" bulk action
    And I select "Fine Motor Skills" folder
    Then all 30 resources should be moved to the folder
    And the folder count should update to 45
    And resources should be removed from other folders
    And I should see "30 resources moved successfully"

  @resources @bulk @metadata
  Scenario: Bulk update resource metadata
    Given I am an admin user
    And I have selected 15 resources needing updates
    When I choose "Bulk Edit Metadata"
    And I update:
      | field        | value                    |
      | gradeLevel   | K-2                     |
      | therapyType  | OT                      |
      | evidenceLevel| 4                       |
      | lastReviewed | current_date            |
    Then all selected resources should be updated
    And an audit log should record the bulk change
    And affected users should see updated metadata

  @resources @bulk @delete
  Scenario: Bulk delete from favorites with confirmation
    Given I have 40 resources in my favorites
    And I select 20 resources to remove
    When I click "Remove from Favorites"
    Then I should see a confirmation dialog
    And the dialog should show "Remove 20 resources from favorites?"
    When I confirm the action
    Then the 20 resources should be removed
    And my favorites should show 20 remaining
    And the action should be undoable for 30 seconds

  @resources @bulk @export
  Scenario: Bulk export resource list
    Given I have filtered resources by:
      | filter      | value              |
      | skillArea   | Fine Motor         |
      | gradeLevel  | Pre-K              |
      | format      | Printable PDF      |
    And the search returns 75 resources
    When I click "Export List"
    Then I should see export format options:
      | format | description                    |
      | CSV    | Spreadsheet compatible        |
      | PDF    | Formatted resource catalog    |
      | JSON   | For API integration           |
      | HTML   | Web page with links           |
    When I select "CSV" format
    Then a file should download with all resource metadata
    And the CSV should include resource IDs for re-import

  @resources @bulk @share
  Scenario: Bulk share resources with colleague
    Given I have selected 12 resources
    And I have colleague contacts in the system
    When I click "Share Resources"
    And I select colleague "sarah.johnson@clinic.com"
    And I add message "Here are the sensory diet resources we discussed"
    Then a share link should be generated
    And an email should be sent to the colleague
    And the colleague should see all 12 resources
    And access should expire after 30 days
    And I should see share analytics

  @resources @bulk @print
  Scenario: Bulk print resources with options
    Given I have selected 8 printable resources
    When I click "Bulk Print"
    Then I should see print options:
      | option           | choices                        |
      | copies           | 1-30                          |
      | paperSize        | Letter, Legal, A4             |
      | color            | Color, Grayscale, B&W         |
      | duplex           | Single-sided, Double-sided    |
      | collate          | Yes, No                       |
      | studentsPerCopy  | 1-10                          |
    When I configure print settings
    And I click "Send to Print Queue"
    Then resources should be combined into one print job
    And page breaks should be inserted between resources
    And a cover sheet should list all resources
    And copyright notices should be included

  @resources @bulk @ai
  Scenario: AI recommendation feedback for multiple resources
    Given I received 30 AI-recommended resources last week
    And I have used 20 of them in sessions
    When I access "Recommendation Feedback"
    Then I should see all 30 recommendations
    And I can bulk rate them:
      | rating        | count |
      | Very Helpful  | 12    |
      | Helpful       | 5     |
      | Not Helpful   | 3     |
      | Not Used      | 10    |
    When I submit bulk feedback
    Then the AI model should process all feedback
    And future recommendations should improve
    And I should see "Feedback submitted for 20 resources"

  @resources @bulk @permissions
  Scenario: Bulk permission management for shared resources
    Given I have shared folder with 50 resources
    And 5 team members have various access levels
    When I select 25 resources
    And I click "Manage Permissions"
    Then I should see current permissions matrix
    When I update permissions:
      | user                | oldPermission | newPermission |
      | john.doe@clinic.com | View          | Edit          |
      | jane.smith@clinic.com | None        | View          |
      | team.lead@clinic.com | Edit         | Admin         |
    Then permissions should update for selected resources only
    And affected users should be notified of changes
    And an audit log should record permission changes

  @resources @bulk @offline
  Scenario: Bulk download for offline access on mobile
    Given I am using the mobile app
    And I have limited storage (500MB available)
    When I select 100 resources for offline access
    Then the system should calculate required space
    And warn if insufficient storage
    And offer to compress images for space saving
    When I proceed with compression
    Then resources should download in background
    And I should see progress in notification tray
    And downloaded resources should sync when online
    And old offline resources should auto-cleanup after 30 days

  @resources @bulk @errors
  Scenario: Handle errors in bulk operations gracefully
    Given I have selected 100 resources for download
    And some resources have issues:
      | resourceId | issue                    |
      | RES-045    | File not found          |
      | RES-067    | Corrupted file          |
      | RES-089    | Access denied           |
      | RES-102    | File too large (>100MB) |
    When I initiate bulk download
    Then successful resources should download
    And failed resources should be listed separately
    And I should see "96 of 100 resources downloaded successfully"
    And error report should detail each failure
    And I should have option to retry failed items
    And support ticket option should be available

  @resources @bulk @performance
  Scenario: Bulk operations performance requirements
    Given I have 500 resources in my library
    When I select all resources
    Then selection should complete within 2 seconds
    When I perform bulk operations:
      | operation      | maxTime  |
      | Add Favorites  | 5 sec    |
      | Create Folder  | 3 sec    |
      | Update Metadata| 10 sec   |
      | Generate ZIP   | 30 sec   |
      | Delete Items   | 5 sec    |
    Then each operation should complete within specified time
    And UI should remain responsive
    And progress indicators should be accurate
    And operations should be cancelable