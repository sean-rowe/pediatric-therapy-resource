Feature: Adult Therapy Resources API Endpoints (FR-016)
  As a therapy professional working with adults
  I want access to age-appropriate adult and geriatric resources
  So that I can provide effective therapy for adult populations

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # GET /api/resources/adult/search
  @endpoint @adult @search @not-implemented
  Scenario: Search adult-specific therapy resources
    When I send a GET request to "/api/resources/adult/search?condition=aphasia&level=moderate"
    Then the response status should be 200
    And the therapy response should contain array of:
      | field            | type    |
      | resourceId       | string  |
      | title            | string  |
      | ageRange         | string  |
      | conditionFocus   | array   |
      | complexityLevel  | string  |
      | format           | string  |

  # GET /api/resources/adult/cognitive-rehab/{category}
  @endpoint @adult @cognitive @not-implemented
  Scenario: Access cognitive rehabilitation materials
    When I send a GET request to "/api/resources/adult/cognitive-rehab/memory"
    Then the response status should be 200
    And resources should include:
      | type              | content                     |
      | worksheets        | Memory strategy exercises   |
      | functionalTasks   | Medication management       |
      | compensatory      | Memory aid templates        |
      | carryover         | Home practice activities    |

  # POST /api/resources/adult/dysphagia/protocol
  @endpoint @adult @dysphagia @not-implemented
  Scenario: Generate dysphagia treatment protocol
    When I send a POST request to "/api/resources/adult/dysphagia/protocol" with:
      | field           | value                    |
      | diagnosisCode   | R13.12                   |
      | dietLevel       | IDDSI-4                  |
      | strategies      | ["chin-tuck", "effortful-swallow"] |
      | contraindications | ["thin-liquids"]       |
    Then the response status should be 201
    And protocol should include:
      | section         | content                  |
      | exercises       | Swallowing exercises     |
      | dietGuidelines  | IDDSI level 4 foods      |
      | precautions     | Aspiration precautions   |
      | education       | Patient/caregiver handouts |

  # GET /api/resources/adult/return-to-work
  @endpoint @adult @vocational @not-implemented
  Scenario: Access return-to-work assessments
    When I send a GET request to "/api/resources/adult/return-to-work?injury=TBI"
    Then the response status should be 200
    And assessments should include:
      | tool                | purpose                    |
      | cognitiveScreening  | Work readiness evaluation  |
      | functionalCapacity  | Physical demands analysis  |
      | workSimulation      | Job-specific tasks         |
      | accommodations      | Workplace modifications    |

  # POST /api/resources/adult/caregiver-training
  @endpoint @adult @caregiver @not-implemented
  Scenario: Create caregiver education materials
    When I send a POST request to "/api/resources/adult/caregiver-training" with:
      | field         | value                         |
      | diagnosis     | dementia                      |
      | stage         | moderate                      |
      | topics        | ["transfers", "communication", "safety"] |
      | language      | es                            |
    Then the response status should be 201
    And materials should include:
      | type          | content                       |
      | videos        | Safe transfer techniques      |
      | handouts      | Communication strategies      |
      | checklists    | Home safety evaluation        |
      | schedules     | Daily routine templates       |

  # GET /api/resources/adult/geriatric/{domain}
  @endpoint @adult @geriatric @not-implemented
  Scenario: Access geriatric-specific resources
    When I send a GET request to "/api/resources/adult/geriatric/fall-prevention"
    Then the response status should be 200
    And resources should be tailored for:
      | consideration    | adaptation                  |
      | visionChanges    | Large print, high contrast  |
      | cognitiveDecline | Simplified instructions      |
      | motivation       | Meaningful activities       |
      | socialContext    | Group exercise options      |

  # POST /api/resources/adult/functional-goals
  @endpoint @adult @goals @not-implemented
  Scenario: Generate adult functional goals
    When I send a POST request to "/api/resources/adult/functional-goals" with:
      | field          | value                      |
      | diagnosis      | CVA                        |
      | affectedSide   | right                      |
      | priorLevel     | independent                |
      | currentNeeds   | ["dressing", "cooking", "driving"] |
    Then the response status should be 200
    And goals should be:
      | type         | example                         |
      | measurable   | Don shirt independently in 5 min |
      | functional   | Prepare simple meal safely      |
      | timebound    | Within 4 weeks                  |
      | relevant     | Return to prior activities      |

  # GET /api/resources/adult/compensatory-strategies/{area}
  @endpoint @adult @compensatory @not-implemented
  Scenario: Access compensatory strategy resources
    When I send a GET request to "/api/resources/adult/compensatory-strategies/memory"
    Then the response status should be 200
    And strategies should include:
      | strategy        | implementation              |
      | external aids   | Smartphone apps, calendars  |
      | environmental   | Label drawers, routines     |
      | internal        | Mnemonics, visualization    |
      | training        | Strategy practice sheets    |

  # POST /api/resources/adult/independent-living
  @endpoint @adult @adl @not-implemented
  Scenario: Create independent living curriculum
    When I send a POST request to "/api/resources/adult/independent-living" with:
      | field         | value                          |
      | focusAreas    | ["money", "medication", "meal-prep"] |
      | cognitiveLevel| mild-impairment                |
      | setting       | assisted-living                |
    Then the response status should be 201
    And curriculum should contain:
      | module        | activities                     |
      | moneyMgmt     | Budgeting worksheets          |
      | medication    | Pill box organization         |
      | mealPrep      | Simple recipe cards           |
      | safety        | Emergency procedures          |

  # GET /api/resources/adult/outcome-measures
  @endpoint @adult @outcomes @not-implemented
  Scenario: Access adult-specific outcome measures
    When I send a GET request to "/api/resources/adult/outcome-measures?setting=snf"
    Then the response status should be 200
    And measures should include:
      | measure      | domain                    | requirements |
      | FIM          | Functional independence   | Certified    |
      | COPM         | Occupational performance  | Training     |
      | MoCA         | Cognitive screening       | Free         |
      | Berg Balance | Fall risk                 | Equipment    |