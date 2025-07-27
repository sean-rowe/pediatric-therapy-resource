Feature: EHR Integration API Endpoints (FR-010)
  As a therapy professional
  I want to integrate with Electronic Health Record systems
  So that I can streamline documentation and billing

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"
    And HIPAA compliance is enforced

  # GET /api/integrations/ehr/providers
  @endpoint @ehr @providers @not-implemented
  Scenario: List supported EHR providers
    When I send a GET request to "/api/integrations/ehr/providers"
    Then the response status should be 200
    And the response should contain array of:
      | field         | type    |
      | providerId    | string  |
      | name          | string  |
      | logo          | string  |
      | features      | array   |
      | authMethod    | string  |
      | apiVersion    | string  |
      | popular       | boolean |

  # POST /api/integrations/ehr/{provider}/connect
  @endpoint @ehr @connection @not-implemented
  Scenario: Connect to SimplePractice EHR
    When I send a POST request to "/api/integrations/ehr/simplepractice/connect" with:
      | field          | value                    |
      | apiKey         | encrypted-api-key        |
      | practiceId     | practice-123             |
      | syncDirection  | bidirectional            |
      | syncItems      | ["clients", "sessions", "notes"] |
      | autoSync       | true                     |
    Then the response status should be 200
    And the response should contain:
      | field           | type   |
      | connectionId    | string |
      | status          | string |
      | capabilities    | array  |
      | lastSync        | string |
    And test connection should succeed

  # GET /api/integrations/ehr/{connectionId}/patients
  @endpoint @ehr @patient-sync @not-implemented
  Scenario: Sync patients from EHR
    Given EHR connection "conn-123" exists
    When I send a GET request to "/api/integrations/ehr/conn-123/patients?sync=true"
    Then the response status should be 200
    And the response should contain:
      | field          | type   |
      | syncedPatients | array  |
      | newCount       | number |
      | updatedCount   | number |
      | errors         | array  |
    And patient data should be mapped correctly

  # POST /api/integrations/ehr/{connectionId}/sessions
  @endpoint @ehr @session-sync @not-implemented
  Scenario: Push session data to EHR
    Given EHR connection "conn-123" exists
    When I send a POST request to "/api/integrations/ehr/conn-123/sessions" with:
      | field          | value                    |
      | sessionId      | session-456              |
      | patientId      | patient-789              |
      | date           | 2024-01-22               |
      | duration       | 30                       |
      | cptCode        | 92507                    |
      | activities     | ["articulation therapy"] |
    Then the response status should be 201
    And session should sync to EHR
    And billing codes should transfer

  # POST /api/integrations/ehr/{connectionId}/notes
  @endpoint @ehr @documentation @not-implemented
  Scenario: Sync therapy notes to EHR
    Given EHR connection "conn-123" exists
    When I send a POST request to "/api/integrations/ehr/conn-123/notes" with:
      | field          | value                         |
      | sessionId      | session-456                   |
      | noteType       | progress                      |
      | content        | {"S": "Patient reports...", "O": "Observed..."} |
      | attachments    | ["res-123", "res-124"]        |
      | signedBy       | therapist-123                 |
    Then the response status should be 201
    And note should appear in EHR
    And maintain SOAP format

  # GET /api/integrations/ehr/{connectionId}/sync-status
  @endpoint @ehr @sync-monitoring @not-implemented
  Scenario: Check sync status
    Given EHR connection "conn-123" is syncing
    When I send a GET request to "/api/integrations/ehr/conn-123/sync-status"
    Then the response status should be 200
    And the response should contain:
      | field          | type    |
      | syncInProgress | boolean |
      | lastSync       | string  |
      | nextSync       | string  |
      | pendingItems   | number  |
      | errorCount     | number  |
      | syncHistory    | array   |

  # PUT /api/integrations/ehr/{connectionId}/mapping
  @endpoint @ehr @field-mapping @not-implemented
  Scenario: Configure field mapping
    Given EHR connection "conn-123" exists
    When I send a PUT request to "/api/integrations/ehr/conn-123/mapping" with:
      | field              | value                        |
      | patientMapping     | {"firstName": "first_name", "lastName": "last_name"} |
      | diagnosisMapping   | {"icd10": "diagnosis_code"}  |
      | customFields       | {"therapyGoals": "custom_field_1"} |
    Then the response status should be 200
    And mapping should be saved
    And future syncs should use mapping

  # POST /api/integrations/ehr/{connectionId}/test
  @endpoint @ehr @testing @not-implemented
  Scenario: Test EHR connection
    Given EHR connection "conn-123" exists
    When I send a POST request to "/api/integrations/ehr/conn-123/test"
    Then the response status should be 200
    And the response should contain:
      | field              | type    |
      | connectionValid    | boolean |
      | authenticationOk   | boolean |
      | readAccess         | boolean |
      | writeAccess        | boolean |
      | latency            | number  |
      | availableEndpoints | array   |

  # DELETE /api/integrations/ehr/{connectionId}
  @endpoint @ehr @disconnection @not-implemented
  Scenario: Disconnect EHR integration
    Given EHR connection "conn-123" exists
    When I send a DELETE request to "/api/integrations/ehr/conn-123" with:
      | field          | value              |
      | retainData     | true               |
      | reason         | switching-systems  |
    Then the response status should be 200
    And integration should be disconnected
    And local data should be retained
    And audit log should be created

  # Core FR-010 Acceptance Criteria - Missing Critical Scenarios
  @ehr @session-documentation @real-time-sync @not-implemented
  Scenario: Real-time session documentation sync to EHR
    Given I have an active EHR connection to SimplePractice
    And I am conducting a therapy session with patient "John Doe"
    When I document the therapy session with:
      | Field              | Value                           |
      | SessionDate        | 2025-01-15                     |
      | Duration           | 45 minutes                     |
      | Resources Used     | Fine Motor Worksheets, Sensory Cards |
      | Progress Notes     | Patient showed improvement in bilateral coordination |
      | Treatment Goals    | Improve fine motor skills      |
      | Next Session Plan  | Continue with cutting activities |
    And I save the session documentation
    Then the session should be automatically synced to SimplePractice
    And the EHR should receive the session within 30 seconds
    And the resources used should be logged in the EHR treatment notes
    And the session should appear in the patient's EHR timeline

  @ehr @oauth-authentication @security @not-implemented
  Scenario: OAuth 2.0 authentication flow for EHR connection
    Given I want to connect to WebPT EHR system
    When I initiate the EHR connection process
    Then I should be redirected to WebPT's OAuth authorization page
    When I authorize the connection with my WebPT credentials
    Then I should be redirected back to UPTRMS with an authorization code
    And the system should exchange the code for access tokens
    And the OAuth tokens should be securely stored
    And I should see confirmation of successful EHR connection
    And the connection should be tested automatically

  @ehr @resource-usage-tracking @bidirectional @not-implemented
  Scenario: Track resource usage in EHR system
    Given I have an EHR connection to TheraNest
    And I am using multiple resources during a session
    When I use the following resources:
      | Resource Type      | Resource Name                  | Usage Time |
      | Worksheet          | Handwriting Practice Sheet 1  | 15 minutes |
      | Digital Activity   | Letter Recognition Game        | 20 minutes |
      | Assessment Tool    | Fine Motor Skills Checklist   | 10 minutes |
    And I complete the therapy session
    Then each resource should be logged in TheraNest with:
      | Field              | Requirement                    |
      | Resource Name      | Exact resource title           |
      | Usage Duration     | Time spent on each resource    |
      | Activity Type      | Classification (worksheet/game/assessment) |
      | Therapeutic Value  | How it addressed treatment goals |
    And the total session time should match individual resource times
    And the EHR should show a detailed activity breakdown

  @ehr @session-sync @data-integrity @not-implemented
  Scenario: Bi-directional session data synchronization
    Given I have bi-directional sync enabled with SimplePractice
    And there are existing sessions in both systems
    When I create a new session in UPTRMS
    Then the session should sync to SimplePractice within 60 seconds
    When a session is updated in SimplePractice
    Then the changes should sync back to UPTRMS within 60 seconds
    And conflicts should be detected and flagged for manual resolution
    And data integrity should be maintained across both systems
    And sync history should be logged for audit purposes

  @ehr @connection-management @monitoring @not-implemented
  Scenario: Monitor EHR connection health and auto-recovery
    Given I have an active EHR connection to WebPT
    When the EHR connection experiences temporary network issues
    Then the system should detect the connection failure
    And retry the connection automatically with exponential backoff
    And queue pending sync operations for retry
    And notify me of connection issues if they persist > 5 minutes
    When the connection is restored
    Then queued operations should be processed automatically
    And I should receive confirmation of restored connectivity
    And sync should resume normal operation

  @ehr @multi-provider @provider-switching @not-implemented
  Scenario: Support multiple EHR providers simultaneously
    Given I work with multiple clinics using different EHR systems
    When I connect to both SimplePractice and WebPT
    Then I should be able to manage both connections independently
    And switch between EHR contexts when documenting sessions
    And each session should sync to the correct EHR system
    And resource usage should be tracked separately per EHR
    And I should see provider-specific dashboards for each connection

  @ehr @data-retention @compliance @not-implemented
  Scenario: Handle EHR disconnection with data retention policies
    Given I have an established EHR connection with 6 months of synced data
    When I disconnect from the EHR system
    Then I should be prompted about data retention preferences:
      | Option             | Description                    |
      | Retain all data    | Keep local copies of all EHR data |
      | Retain UPTRMS data | Keep only UPTRMS-originated data |
      | Purge all data     | Remove all synced data locally |
    And the selected retention policy should be applied
    And an audit log should record the disconnection and retention choice
    And compliance requirements should be met for data handling

  @ehr @error-handling @sync-failures @not-implemented
  Scenario: Handle EHR sync failures gracefully
    Given I have an active EHR connection
    When a session sync fails due to EHR system maintenance
    Then the failed sync should be queued for retry
    And I should be notified of the sync failure
    And the local session data should remain intact
    And automatic retry should occur every 15 minutes
    When the EHR system becomes available
    Then the queued sync should complete successfully
    And I should receive confirmation of successful sync
    And the sync failure should be logged for analysis

  @ehr @performance @sync-optimization @not-implemented
  Scenario: Optimize sync performance for large datasets
    Given I have 500+ patients and 10,000+ sessions to sync
    When I initiate a full EHR synchronization
    Then the sync should process in batches of 50 records
    And progress should be reported every 10% completion
    And the sync should complete within 30 minutes
    And system performance should remain responsive during sync
    And users should be able to continue working during background sync
    And critical operations should take priority over bulk sync

  @ehr @compliance @audit-trail @not-implemented
  Scenario: Maintain comprehensive audit trail for EHR operations
    Given HIPAA compliance requires detailed audit logs
    When any EHR operation occurs
    Then the audit log should capture:
      | Field              | Requirement                    |
      | Timestamp          | Exact time of operation        |
      | User ID            | Who performed the operation    |
      | Operation Type     | Create/Read/Update/Delete/Sync |
      | Patient ID         | Which patient data was accessed |
      | Data Changes       | What specific data changed     |
      | EHR System         | Which EHR system was involved  |
      | Success/Failure    | Operation outcome              |
    And audit logs should be tamper-proof
    And logs should be retained for 7 years minimum
    And compliance reports should be generated monthly