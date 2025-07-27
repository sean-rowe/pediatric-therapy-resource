Feature: Virtual Tools API Endpoints (FR-024)
  As a therapy professional conducting teletherapy
  I want access to virtual therapy tools
  So that I can engage students effectively during remote sessions

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # GET /api/virtual/backgrounds
  @endpoint @virtual @backgrounds @not-implemented
  Scenario: Access virtual backgrounds library
    When I send a GET request to "/api/virtual/backgrounds?category=therapy-room"
    Then the response status should be 200
    And backgrounds should include:
      | type          | features                    |
      | static        | High-res therapy room images |
      | animated      | Subtle movement effects      |
      | interactive   | Clickable elements           |
      | seasonal      | Holiday/season themes        |

  # POST /api/virtual/tools/dice-roller
  @endpoint @virtual @dice @not-implemented
  Scenario: Use virtual dice roller
    When I send a POST request to "/api/virtual/tools/dice-roller" with:
      | field         | value                    |
      | numberOfDice  | 2                        |
      | sides         | 6                        |
      | customFaces   | ["red", "blue", "green"] |
      | animation     | true                     |
    Then the response status should be 200
    And the response should contain:
      | field         | type   |
      | results       | array  |
      | total         | number |
      | animationUrl  | string |

  # POST /api/virtual/tools/spinner
  @endpoint @virtual @spinner @not-implemented
  Scenario: Create custom spinner
    When I send a POST request to "/api/virtual/tools/spinner" with:
      | field      | value                           |
      | sections   | ["hop", "jump", "clap", "spin"] |
      | colors     | ["red", "blue", "green", "yellow"] |
      | weighted   | false                           |
      | soundEffect| true                            |
    Then the response status should be 201
    And the response should contain:
      | field      | type   |
      | spinnerId  | string |
      | shareUrl   | string |
      | embedCode  | string |

  # GET /api/virtual/rewards/tokens
  @endpoint @virtual @tokens @not-implemented
  Scenario: Access virtual token system
    When I send a GET request to "/api/virtual/rewards/tokens?theme=space"
    Then the response status should be 200
    And token system should include:
      | component     | type                    |
      | tokens        | Animated space objects  |
      | board         | Space-themed collection |
      | sounds        | Achievement sounds      |
      | milestones    | Special animations      |

  # POST /api/virtual/manipulatives/create
  @endpoint @virtual @manipulatives @not-implemented
  Scenario: Create virtual manipulatives set
    When I send a POST request to "/api/virtual/manipulatives/create" with:
      | field         | value                    |
      | type          | counting-bears           |
      | quantity      | 20                       |
      | colors        | ["red", "blue", "yellow"] |
      | interactive   | drag-and-drop            |
    Then the response status should be 201
    And manipulatives should be:
      | feature       | enabled                  |
      | grouping      | true                     |
      | counting      | automatic                |
      | patterns      | creation tools           |

  # POST /api/virtual/annotation/start
  @endpoint @virtual @annotation @not-implemented
  Scenario: Start screen annotation session
    When I send a POST request to "/api/virtual/annotation/start" with:
      | field         | value                    |
      | sessionId     | therapy-session-123      |
      | tools         | ["pen", "highlighter", "shapes"] |
      | saveEnabled   | true                     |
    Then the response status should be 200
    And annotation tools should be available
    And drawings should sync in real-time

  # GET /api/virtual/games/movement
  @endpoint @virtual @movement @not-implemented
  Scenario: Access movement-based virtual games
    When I send a GET request to "/api/virtual/games/movement?age=5-7"
    Then the response status should be 200
    And games should include:
      | game            | type                  |
      | Simon Says      | Follow directions     |
      | Freeze Dance    | Music and movement    |
      | Animal Walks    | Gross motor imitation |
      | Yoga Adventure  | Guided positions      |

  # POST /api/virtual/timers/visual
  @endpoint @virtual @timers @not-implemented
  Scenario: Create visual timer
    When I send a POST request to "/api/virtual/timers/visual" with:
      | field         | value                    |
      | duration      | 300                      |
      | style         | countdown-circle         |
      | warningAt     | 60                       |
      | soundAlerts   | true                     |
      | customMessage | "Great job!"             |
    Then the response status should be 200
    And timer should display visually
    And alerts should trigger at specified times

  # POST /api/virtual/whiteboard/shared
  @endpoint @virtual @whiteboard @not-implemented
  Scenario: Create shared whiteboard
    When I send a POST request to "/api/virtual/whiteboard/shared" with:
      | field         | value                    |
      | sessionId     | session-123              |
      | participants  | ["therapist", "student"] |
      | tools         | ["draw", "text", "shapes"] |
      | saveWork      | true                     |
    Then the response status should be 201
    And whiteboard should allow:
      | feature       | capability               |
      | collaboration | Real-time drawing        |
      | templates     | Pre-made activities      |
      | export        | PDF or image             |

  # GET /api/virtual/mouse-control/activities
  @endpoint @virtual @mouse @not-implemented
  Scenario: Get mouse control practice activities
    When I send a GET request to "/api/virtual/mouse-control/activities?skill=click-drag"
    Then the response status should be 200
    And activities should include:
      | activity      | skills                   |
      | puzzles       | Drag pieces to place     |
      | coloring      | Click and fill           |
      | matching      | Drag to match            |
      | tracing       | Follow the path          |