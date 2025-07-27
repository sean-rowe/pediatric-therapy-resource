Feature: Resource Creation API Endpoints (FR-005)
  As a content creator
  I want to create and upload therapy resources
  So that I can share materials with other professionals

  Background:
    Given the API is available
    And I am authenticated as "creator@clinic.com"
    And I have content creation permissions

  # POST /api/resources
  @endpoint @resources @creation @not-implemented
  Scenario: Create a new resource
    When I send a POST request to "/api/resources" with:
      | field         | value                                |
      | title         | Fine Motor Tracing Worksheets        |
      | description   | Age-appropriate tracing activities   |
      | skillAreas    | ["fine-motor", "pre-writing"]       |
      | gradeLevels   | ["prek", "kindergarten"]            |
      | resourceType  | worksheet                            |
      | format        | pdf                                  |
      | language      | en                                   |
      | tags          | ["tracing", "pencil-grip"]          |
    And I attach file "tracing-worksheets.pdf"
    Then the response status should be 201
    And the response should contain:
      | field      | type   |
      | resourceId | string |
      | status     | string |
      | uploadUrl  | string |
    And the resource should be created in draft status

  @endpoint @resources @creation @validation @not-implemented
  Scenario: Validate required fields
    When I send a POST request to "/api/resources" with:
      | field | value |
      | title |       |
    Then the response status should be 400
    And the response should contain error "Title is required"

  # POST /api/resources/{id}/upload
  @endpoint @resources @upload @not-implemented
  Scenario: Upload resource file
    Given I have created resource "res-123" in draft status
    When I send a POST request to "/api/resources/res-123/upload"
    And I attach file "worksheet.pdf" with:
      | field       | value           |
      | contentType | application/pdf |
      | fileSize    | 2048000         |
    Then the response status should be 200
    And the response should contain:
      | field        | type   |
      | uploadId     | string |
      | status       | string |
      | processingId | string |
    And file processing should begin

  @endpoint @resources @upload @validation @not-implemented
  Scenario: Validate file size limits
    Given I have created resource "res-123"
    When I send a POST request to "/api/resources/res-123/upload"
    And I attach file larger than 50MB
    Then the response status should be 400
    And the response should contain error "File size exceeds 50MB limit"

  # PUT /api/resources/{id}
  @endpoint @resources @update @not-implemented
  Scenario: Update resource metadata
    Given I own resource "res-123" in draft status
    When I send a PUT request to "/api/resources/res-123" with:
      | field       | value                          |
      | title       | Updated Fine Motor Worksheets  |
      | description | Now includes cutting practice  |
      | tags        | ["tracing", "cutting", "scissors"] |
    Then the response status should be 200
    And the resource metadata should be updated
    And version history should be maintained

  @endpoint @resources @update @authorization @not-implemented
  Scenario: Cannot update published resource owned by others
    Given resource "res-456" is owned by another user
    And the resource is published
    When I send a PUT request to "/api/resources/res-456" with any data
    Then the response status should be 403
    And the response should contain error "Access denied"

  # POST /api/resources/{id}/preview
  @endpoint @resources @preview @not-implemented
  Scenario: Generate resource preview
    Given I have uploaded resource "res-123"
    When I send a POST request to "/api/resources/res-123/preview" with:
      | field    | value |
      | pages    | [1,2,3] |
      | quality  | high  |
    Then the response status should be 200
    And the response should contain:
      | field       | type  |
      | previewUrls | array |
      | thumbnailUrl| string |
    And preview images should be generated

  # POST /api/resources/{id}/publish
  @endpoint @resources @publish @not-implemented
  Scenario: Publish resource
    Given I own resource "res-123" in draft status
    And the resource has passed quality checks
    When I send a POST request to "/api/resources/res-123/publish" with:
      | field           | value                    |
      | visibility      | public                   |
      | licensing       | single-classroom         |
      | allowDownload   | true                     |
      | requireApproval | false                    |
    Then the response status should be 200
    And the resource status should be "published"
    And the resource should be searchable

  @endpoint @resources @publish @validation @not-implemented
  Scenario: Cannot publish incomplete resource
    Given I own resource "res-123" without uploaded file
    When I send a POST request to "/api/resources/res-123/publish"
    Then the response status should be 400
    And the response should contain error "Resource file is required"

  # DELETE /api/resources/{id}
  @endpoint @resources @deletion @not-implemented
  Scenario: Delete draft resource
    Given I own resource "res-123" in draft status
    When I send a DELETE request to "/api/resources/res-123"
    Then the response status should be 200
    And the resource should be deleted
    And associated files should be removed

  @endpoint @resources @deletion @soft-delete @not-implemented
  Scenario: Soft delete published resource
    Given I own published resource "res-123"
    When I send a DELETE request to "/api/resources/res-123"
    Then the response status should be 200
    And the resource should be marked as deleted
    And existing downloads should still work
    And the resource should not appear in searches

  # POST /api/resources/batch
  @endpoint @resources @batch @not-implemented
  Scenario: Create multiple resources in batch
    When I send a POST request to "/api/resources/batch" with:
      | resources | array of 5 resource objects |
    Then the response status should be 200
    And the response should contain:
      | field     | type  |
      | created   | array |
      | failed    | array |
      | batchId   | string |
    And batch processing should begin

  # POST /api/resources/{id}/duplicate
  @endpoint @resources @duplication @not-implemented
  Scenario: Duplicate existing resource
    Given I own resource "res-123"
    When I send a POST request to "/api/resources/res-123/duplicate" with:
      | field      | value                    |
      | title      | Copy of Original Resource|
      | copyFiles  | true                     |
    Then the response status should be 201
    And the response should contain:
      | field         | type   |
      | newResourceId | string |
      | status        | string |
    And all files should be copied

  # POST /api/resources/{id}/convert
  @endpoint @resources @conversion @not-implemented
  Scenario: Convert resource format
    Given I own PDF resource "res-123"
    When I send a POST request to "/api/resources/res-123/convert" with:
      | field    | value      |
      | toFormat | interactive |
      | options  | {"editable": true} |
    Then the response status should be 202
    And the response should contain:
      | field       | type   |
      | jobId       | string |
      | status      | string |
      | estimatedTime | number |
    And conversion job should be queued

  # GET /api/resources/{id}/validation
  @endpoint @resources @quality @not-implemented
  Scenario: Validate resource quality
    Given I have uploaded resource "res-123"
    When I send a GET request to "/api/resources/res-123/validation"
    Then the response status should be 200
    And the response should contain:
      | field            | type    |
      | spellCheck       | object  |
      | readabilityScore | number  |
      | accessibility    | object  |
      | clinicalAccuracy | object  |
      | suggestions      | array   |

  # POST /api/resources/import
  @endpoint @resources @import @not-implemented
  Scenario: Import resources from external source
    When I send a POST request to "/api/resources/import" with:
      | field    | value                           |
      | source   | google-drive                    |
      | folderId | 1234567890                      |
      | mapping  | {"title": "name", "tags": "labels"} |
    Then the response status should be 202
    And the response should contain:
      | field     | type   |
      | importId  | string |
      | status    | string |
      | itemCount | number |
    And import job should be initiated

  # POST /api/resources/{id}/translations
  @endpoint @resources @multilingual @not-implemented
  Scenario: Add translation to resource
    Given I own resource "res-123" in English
    When I send a POST request to "/api/resources/res-123/translations" with:
      | field    | value    |
      | language | es       |
      | title    | Hojas de Trazado de Motricidad Fina |
      | description | Actividades de trazado apropiadas para la edad |
    And I attach file "trazado-es.pdf"
    Then the response status should be 201
    And the Spanish translation should be added
    And both versions should be linked

  # POST /api/resources/{id}/clinical-adaptation
  @endpoint @resources @clinical @not-implemented
  Scenario: Create clinical adaptation of resource
    Given resource "res-123" exists for typical development
    When I send a POST request to "/api/resources/res-123/clinical-adaptation" with:
      | field          | value                    |
      | adaptationType | autism-friendly          |
      | modifications  | ["visual-supports", "simplified-language"] |
      | notes          | Added visual schedule and reduced text |
    Then the response status should be 201
    And an adapted version should be created
    And both versions should be linked

  # POST /api/resources/{id}/archive
  @endpoint @resources @lifecycle @not-implemented
  Scenario: Archive outdated resource
    Given I own resource "res-123"
    When I send a POST request to "/api/resources/res-123/archive" with:
      | field  | value                          |
      | reason | Outdated clinical guidelines   |
      | replacementId | res-789              |
    Then the response status should be 200
    And the resource should be archived
    And users should be notified of replacement
    And existing links should redirect to replacement