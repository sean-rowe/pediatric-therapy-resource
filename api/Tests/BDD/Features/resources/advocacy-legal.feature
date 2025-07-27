Feature: Advocacy and Legal Resources API Endpoints (FR-039)
  As a therapy professional or parent
  I want access to advocacy resources and legal templates
  So that I can effectively advocate for student needs

  Background:
    Given the API is available
    And I am authenticated

  # GET /api/advocacy/iep-prep/checklist
  @endpoint @advocacy @iep @not-implemented
  Scenario: Access IEP preparation checklist
    When I send a GET request to "/api/advocacy/iep-prep/checklist?meeting=annual"
    Then the response status should be 200
    And checklist should include:
      | category        | items                         |
      | documentation   | Progress reports, evaluations |
      | goals           | Draft goals with data         |
      | questions       | Key questions to ask          |
      | rights          | Parent rights summary         |
      | timeline        | Important deadlines           |

  # GET /api/advocacy/rights/{state}
  @endpoint @advocacy @rights @not-implemented
  Scenario: Get state-specific rights information
    When I send a GET request to "/api/advocacy/rights/CA"
    Then the response status should be 200
    And information should include:
      | topic              | details                    |
      | evaluationRights   | Timeline and procedures    |
      | iepRights          | Meeting requirements       |
      | disciplineRights   | Manifestation determination|
      | privateSchool      | Equitable services         |

  # POST /api/advocacy/letters/generate
  @endpoint @advocacy @letters @not-implemented
  Scenario: Generate advocacy letter template
    When I send a POST request to "/api/advocacy/letters/generate" with:
      | field         | value                         |
      | letterType    | "request-evaluation"          |
      | recipient     | "school-district"             |
      | childInfo     | {"name": "Emma", "grade": 3} |
      | concerns      | ["reading", "attention"]      |
    Then the response status should be 200
    And letter should include:
      | section       | content                       |
      | formalRequest | Legal language                |
      | timeline      | Response requirements         |
      | citations     | Relevant laws                 |
      | nextSteps     | What happens next             |

  # GET /api/advocacy/due-process/guides
  @endpoint @advocacy @dueprocess @not-implemented
  Scenario: Access due process guides
    When I send a GET request to "/api/advocacy/due-process/guides"
    Then the response status should be 200
    And guides should cover:
      | stage         | resources                     |
      | preFiling     | Resolution attempts           |
      | filing        | Complaint templates           |
      | mediation     | Preparation tips              |
      | hearing       | Evidence organization         |

  # POST /api/advocacy/training/modules
  @endpoint @advocacy @training @not-implemented
  Scenario: Access advocacy training modules
    When I send a POST request to "/api/advocacy/training/modules" with:
      | field         | value                    |
      | role          | "parent"                 |
      | experience    | "beginner"               |
      | topics        | ["iep-basics", "rights"] |
    Then the response status should be 200
    And modules should include:
      | module        | format         | duration |
      | IEP 101       | video         | 30 min   |
      | Your Rights   | interactive   | 45 min   |
      | Effective Communication | workbook | self-paced |

  # GET /api/advocacy/grants/templates
  @endpoint @advocacy @grants @not-implemented
  Scenario: Access grant writing templates
    When I send a GET request to "/api/advocacy/grants/templates?purpose=therapy-equipment"
    Then the response status should be 200
    And templates should include:
      | grantType     | components                    |
      | foundation    | Narrative, budget, outcomes   |
      | corporate     | Brief proposal, impact        |
      | government    | Detailed application          |

  # POST /api/advocacy/insurance/appeals
  @endpoint @advocacy @insurance @not-implemented
  Scenario: Generate insurance appeal letter
    When I send a POST request to "/api/advocacy/insurance/appeals" with:
      | field         | value                         |
      | denialReason  | "not medically necessary"     |
      | service       | "occupational therapy"        |
      | diagnosis     | "autism spectrum disorder"    |
      | evidence      | ["eval-report", "physician-letter"] |
    Then the response status should be 200
    And appeal should include:
      | section       | content                       |
      | argument      | Medical necessity justification|
      | research      | Supporting studies            |
      | credentials   | Provider qualifications       |
      | request       | Specific approval sought      |

  # GET /api/advocacy/resources/organizations
  @endpoint @advocacy @organizations @not-implemented
  Scenario: Find advocacy organizations
    When I send a GET request to "/api/advocacy/resources/organizations?disability=autism&state=CA"
    Then the response status should be 200
    And organizations should include:
      | name          | services              | contact        |
      | Autism Speaks | Resources, toolkits   | 1-888-288-4762 |
      | DREDF         | Legal advocacy        | info@dredf.org |
      | CID           | Parent training       | Local chapters |

  # POST /api/advocacy/case-management/create
  @endpoint @advocacy @case @not-implemented
  Scenario: Create advocacy case file
    When I send a POST request to "/api/advocacy/case-management/create" with:
      | field         | value                         |
      | studentId     | student-123                   |
      | concerns      | ["services", "placement"]     |
      | documents     | ["iep-2023", "eval-2023"]     |
      | timeline      | {"started": "2024-01-01"}     |
    Then the response status should be 201
    And case file should be created
    And document organization should be provided

  # GET /api/advocacy/updates/legislation
  @endpoint @advocacy @legislation @not-implemented
  Scenario: Get legislative updates
    When I send a GET request to "/api/advocacy/updates/legislation?impacting=special-education"
    Then the response status should be 200
    And updates should include:
      | bill          | status        | impact                |
      | HR-1234       | committee     | Funding increase      |
      | SB-5678       | passed        | New requirements      |
      | AB-9012       | pending       | Service definitions   |