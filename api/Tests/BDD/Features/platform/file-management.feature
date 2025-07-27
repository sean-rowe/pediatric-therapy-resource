Feature: File Management API Endpoints
  As a platform user
  I want to manage files and documents
  So that I can organize and access therapy materials efficiently

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # POST /api/files/upload
  @endpoint @files @upload @not-implemented
  Scenario: Upload file with metadata
    When I send a POST request to "/api/files/upload" with:
      | field         | value                    |
      | fileName      | assessment-report.pdf    |
      | fileType      | application/pdf          |
      | category      | student-documents        |
      | tags          | ["assessment", "2024"]   |
      | encrypted     | true                     |
    And I attach the file
    Then the response status should be 201
    And the response should contain:
      | field      | type   |
      | fileId     | string |
      | uploadUrl  | string |
      | checksum   | string |

  # GET /api/files/{fileId}
  @endpoint @files @download @not-implemented
  Scenario: Download file
    Given file "file-123" exists
    When I send a GET request to "/api/files/file-123"
    Then the response status should be 200
    And the response headers should contain:
      | header              | value                |
      | Content-Type        | application/pdf      |
      | Content-Disposition | attachment           |
    And file should be decrypted if encrypted

  # POST /api/files/scan
  @endpoint @files @security @not-implemented
  Scenario: Scan file for security
    When I send a POST request to "/api/files/scan" with:
      | field     | value              |
      | fileId    | file-123           |
      | scanType  | ["virus", "malware", "content"] |
    Then the response status should be 200
    And the response should contain:
      | field         | type    |
      | scanResult    | string  |
      | threats       | array   |
      | quarantined   | boolean |

  # PUT /api/files/{fileId}/metadata
  @endpoint @files @metadata @not-implemented
  Scenario: Update file metadata
    Given file "file-123" exists
    When I send a PUT request to "/api/files/file-123/metadata" with:
      | field       | value                    |
      | tags        | ["reviewed", "approved"] |
      | category    | clinical-resources       |
      | visibility  | organization             |
    Then the response status should be 200
    And metadata should be updated

  # POST /api/files/compress
  @endpoint @files @compression @not-implemented
  Scenario: Compress multiple files
    When I send a POST request to "/api/files/compress" with:
      | field      | value                         |
      | fileIds    | ["file-1", "file-2", "file-3"]|
      | format     | zip                           |
      | fileName   | student-resources.zip         |
    Then the response status should be 202
    And compression job should start
    And download link should be provided

  # GET /api/files/search
  @endpoint @files @search @not-implemented
  Scenario: Search files
    When I send a GET request to "/api/files/search?q=assessment&type=pdf&date=2024"
    Then the response status should be 200
    And the response should contain:
      | field   | type  |
      | files   | array |
      | total   | number|
      | facets  | object|

  # DELETE /api/files/{fileId}
  @endpoint @files @deletion @not-implemented
  Scenario: Delete file with audit
    Given file "file-123" exists
    When I send a DELETE request to "/api/files/file-123" with:
      | field      | value                |
      | reason     | outdated-content     |
      | permanent  | false                |
    Then the response status should be 200
    And file should be moved to trash
    And deletion should be logged

  # POST /api/files/convert
  @endpoint @files @conversion @not-implemented
  Scenario: Convert file format
    When I send a POST request to "/api/files/convert" with:
      | field       | value         |
      | fileId      | file-123      |
      | fromFormat  | docx          |
      | toFormat    | pdf           |
      | preserve    | formatting    |
    Then the response status should be 202
    And conversion should begin
    And new file should be created