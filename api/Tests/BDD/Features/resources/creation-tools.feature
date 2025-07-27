Feature: Creation Tools API Endpoints (FR-026)
  As a therapy professional
  I want template-based resource creation tools
  So that I can create customized materials efficiently

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # GET /api/creation/templates
  @endpoint @creation @templates @not-implemented
  Scenario: Browse creation templates
    When I send a GET request to "/api/creation/templates?category=worksheets"
    Then the response status should be 200
    And templates should include:
      | templateId   | name                  | customizable        |
      | tmpl-001     | Tracing Lines         | text, images, difficulty |
      | tmpl-002     | Word Search           | words, size, theme   |
      | tmpl-003     | Bingo Cards           | items, layout, size  |
      | tmpl-004     | Visual Schedule       | activities, times    |

  # POST /api/creation/customize
  @endpoint @creation @customize @not-implemented
  Scenario: Customize template with drag-drop editor
    When I send a POST request to "/api/creation/customize" with:
      | field         | value                    |
      | templateId    | tmpl-word-search         |
      | customization | {"words": ["cat", "dog", "bird"], "size": "10x10"} |
      | theme         | animals                  |
      | difficulty    | easy                     |
    Then the response status should be 201
    And the response should contain:
      | field         | type   |
      | projectId     | string |
      | previewUrl    | string |
      | editUrl       | string |

  # GET /api/creation/image-library
  @endpoint @creation @images @not-implemented
  Scenario: Access copyright-cleared image library
    When I send a GET request to "/api/creation/image-library?search=emotions"
    Then the response status should be 200
    And images should be:
      | field         | value                    |
      | copyrightFree | true                     |
      | resolution    | high                     |
      | formats       | ["png", "svg"]           |
      | categories    | ["faces", "expressions"] |

  # POST /api/creation/brand/apply
  @endpoint @creation @branding @not-implemented
  Scenario: Apply brand customization to materials
    When I send a POST request to "/api/creation/brand/apply" with:
      | field         | value                    |
      | projectId     | proj-123                 |
      | brandElements | {"logo": "url", "colors": ["#FF5733", "#33FF57"]} |
      | position      | top-right                |
      | opacity       | 0.8                      |
    Then the response status should be 200
    And materials should include brand elements
    And consistency should be maintained

  # POST /api/creation/collaborative/invite
  @endpoint @creation @collaborative @not-implemented
  Scenario: Create collaborative template
    When I send a POST request to "/api/creation/collaborative/invite" with:
      | field         | value                         |
      | projectId     | proj-123                      |
      | collaborators | ["therapist2@clinic.com"]     |
      | permissions   | ["edit", "comment"]           |
      | message       | "Let's work on this together" |
    Then the response status should be 200
    And invitations should be sent
    And real-time collaboration should be enabled

  # PUT /api/creation/projects/{projectId}/version
  @endpoint @creation @versioning @not-implemented
  Scenario: Save template version
    When I send a PUT request to "/api/creation/projects/proj-123/version" with:
      | field         | value                    |
      | versionName   | "Spring 2024 Update"     |
      | changes       | "Added new vocabulary"   |
      | autoSave      | true                     |
    Then the response status should be 200
    And version should be saved
    And previous versions should be accessible

  # POST /api/creation/export
  @endpoint @creation @export @not-implemented
  Scenario: Export created materials
    When I send a POST request to "/api/creation/export" with:
      | field         | value                    |
      | projectId     | proj-123                 |
      | format        | pdf                      |
      | quality       | print                    |
      | pages         | all                      |
    Then the response status should be 200
    And export should include:
      | field         | value                    |
      | downloadUrl   | string                   |
      | fileSize      | number                   |
      | printReady    | true                     |

  # GET /api/creation/fonts
  @endpoint @creation @fonts @not-implemented
  Scenario: Access therapy-appropriate fonts
    When I send a GET request to "/api/creation/fonts?category=handwriting"
    Then the response status should be 200
    And fonts should include:
      | fontFamily      | features                |
      | DNealian        | Handwriting practice    |
      | ComicSans       | Friendly, readable      |
      | OpenDyslexic    | Dyslexia-friendly       |
      | SignLanguage    | ASL characters          |

  # POST /api/creation/merge-documents
  @endpoint @creation @merge @not-implemented
  Scenario: Merge multiple created resources
    When I send a POST request to "/api/creation/merge-documents" with:
      | field         | value                    |
      | projectIds    | ["proj-1", "proj-2", "proj-3"] |
      | order         | sequential               |
      | pageNumbers   | true                     |
      | tableOfContents | true                   |
    Then the response status should be 201
    And merged document should be created
    And formatting should be preserved

  # POST /api/creation/ai-assist
  @endpoint @creation @ai-assist @not-implemented
  Scenario: Get AI assistance for content creation
    When I send a POST request to "/api/creation/ai-assist" with:
      | field         | value                    |
      | projectId     | proj-123                 |
      | requestType   | "suggest-content"        |
      | context       | "working on /r/ sounds"  |
      | studentAge    | 6                        |
    Then the response status should be 200
    And AI should suggest:
      | suggestion    | type                     |
      | words         | Age-appropriate /r/ words |
      | sentences     | Practice phrases         |
      | activities    | Complementary exercises  |