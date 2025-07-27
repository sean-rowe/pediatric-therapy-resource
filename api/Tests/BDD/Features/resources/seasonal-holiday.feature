Feature: Seasonal and Holiday Content API Endpoints (FR-020)
  As a therapy professional
  I want access to themed seasonal and holiday resources
  So that I can provide engaging, timely therapy materials

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # GET /api/resources/seasonal/current
  @endpoint @seasonal @current @not-implemented
  Scenario: Get current seasonal resources
    Given the current date is "2024-10-15"
    When I send a GET request to "/api/resources/seasonal/current"
    Then the response status should be 200
    And the response should contain:
      | field           | value                |
      | season          | fall                 |
      | featuredThemes  | ["halloween", "harvest", "thanksgiving"] |
      | resources       | array                |
      | autoRotated     | true                 |

  # GET /api/resources/holidays/calendar
  @endpoint @holidays @calendar @not-implemented
  Scenario: View multi-cultural holiday calendar
    When I send a GET request to "/api/resources/holidays/calendar?year=2024&cultures=all"
    Then the response status should be 200
    And calendar should include:
      | culture      | holidays                          |
      | christian    | Christmas, Easter                 |
      | jewish       | Hanukkah, Passover                |
      | islamic      | Eid al-Fitr, Eid al-Adha          |
      | hindu        | Diwali, Holi                      |
      | secular      | New Year, Valentine's Day         |
      | cultural     | Lunar New Year, Cinco de Mayo     |

  # POST /api/resources/seasonal/preferences
  @endpoint @seasonal @preferences @not-implemented
  Scenario: Set seasonal content preferences
    When I send a POST request to "/api/resources/seasonal/preferences" with:
      | field              | value                         |
      | autoRotate         | true                          |
      | includedHolidays   | ["secular", "cultural"]       |
      | excludedThemes     | ["scary", "religious"]        |
      | ageAppropriate     | 3-8                           |
      | notificationDays   | 7                             |
    Then the response status should be 200
    And preferences should be saved
    And content should filter accordingly

  # GET /api/resources/seasonal/theme/{theme}
  @endpoint @seasonal @theme @not-implemented
  Scenario: Get resources for specific theme
    When I send a GET request to "/api/resources/seasonal/theme/winter?skills=fine-motor"
    Then the response status should be 200
    And resources should include:
      | type         | examples                      |
      | crafts       | Snowflake cutting practice    |
      | worksheets   | Winter clothing sequences     |
      | games        | Snowman building dice game    |
      | sensory      | Fake snow sensory bins        |

  # POST /api/resources/seasonal/custom-theme
  @endpoint @seasonal @custom @not-implemented
  Scenario: Create custom seasonal theme pack
    When I send a POST request to "/api/resources/seasonal/custom-theme" with:
      | field         | value                         |
      | themeName     | "Spring Garden"               |
      | elements      | ["flowers", "bugs", "rain"]   |
      | skills        | ["counting", "colors", "vocabulary"] |
      | ageRange      | 4-6                           |
    Then the response status should be 201
    And custom theme should be created
    And AI should suggest matching resources

  # GET /api/resources/holidays/{holiday}/activities
  @endpoint @holidays @activities @not-implemented
  Scenario: Get holiday-specific therapy activities
    When I send a GET request to "/api/resources/holidays/thanksgiving/activities"
    Then the response status should be 200
    And activities should include:
      | category      | examples                      |
      | gratitude     | Thankful tree craft           |
      | sequencing    | Turkey sandwich making         |
      | social        | Family traditions discussion   |
      | motor         | Leaf pile jumping cards        |

  # POST /api/resources/seasonal/backgrounds
  @endpoint @seasonal @backgrounds @not-implemented
  Scenario: Get seasonal virtual backgrounds
    When I send a POST request to "/api/resources/seasonal/backgrounds" with:
      | field        | value                    |
      | season       | winter                   |
      | style        | animated                 |
      | interactive  | true                     |
    Then the response status should be 200
    And backgrounds should include:
      | type         | features                 |
      | snow scene   | Falling snow animation   |
      | fireplace    | Crackling fire sounds    |
      | ice palace   | Interactive elements     |

  # GET /api/resources/seasonal/countdown/{event}
  @endpoint @seasonal @countdown @not-implemented
  Scenario: Access countdown calendars
    When I send a GET request to "/api/resources/seasonal/countdown/christmas"
    Then the response status should be 200
    And calendar should include:
      | day  | activity                    |
      | 1    | Fine motor ornament craft   |
      | 2    | Holiday vocabulary bingo    |
      | 3    | Gift wrapping practice      |
      | 24   | Special celebration activity |

  # POST /api/resources/seasonal/schedule
  @endpoint @seasonal @scheduling @not-implemented
  Scenario: Schedule seasonal content rotation
    When I send a POST request to "/api/resources/seasonal/schedule" with:
      | field          | value                         |
      | rotationType   | automatic                     |
      | leadTime       | 2-weeks                       |
      | transitionDays | 3                             |
      | notify         | true                          |
    Then the response status should be 200
    And rotation schedule should be set
    And notifications should be configured

  # GET /api/resources/seasonal/rewards/{season}
  @endpoint @seasonal @rewards @not-implemented
  Scenario: Get seasonal reward systems
    When I send a GET request to "/api/resources/seasonal/rewards/fall"
    Then the response status should be 200
    And rewards should include:
      | type          | theme                   |
      | stickers      | Autumn leaves, pumpkins |
      | certificates  | Harvest themed          |
      | tokens        | Acorn collection        |
      | charts        | Apple tree progress     |