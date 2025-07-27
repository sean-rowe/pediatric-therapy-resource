Feature: Gamification API Endpoints (FR-027)
  As a platform user (therapist or student)
  I want gamification and reward systems
  So that students stay motivated and engaged

  Background:
    Given the API is available
    And I am authenticated

  # POST /api/gamification/points/award
  @endpoint @gamification @points @not-implemented
  Scenario: Award points for activity completion
    Given I am a therapist
    When I send a POST request to "/api/gamification/points/award" with:
      | field         | value                    |
      | studentId     | student-123              |
      | activityId    | activity-456             |
      | pointsEarned  | 50                       |
      | reason        | "Perfect score"          |
      | bonusPoints   | 10                       |
    Then the response status should be 200
    And student's point total should increase
    And achievement notification should trigger

  # GET /api/gamification/student/{studentId}/progress
  @endpoint @gamification @progress @not-implemented
  Scenario: View student gamification progress
    When I send a GET request to "/api/gamification/student/student-123/progress"
    Then the response status should be 200
    And the response should contain:
      | field           | type   |
      | totalPoints     | number |
      | currentLevel    | number |
      | badges          | array  |
      | streaks         | object |
      | leaderboardRank | number |

  # POST /api/gamification/badges/unlock
  @endpoint @gamification @badges @not-implemented
  Scenario: Unlock achievement badge
    When I send a POST request to "/api/gamification/badges/unlock" with:
      | field         | value                    |
      | studentId     | student-123              |
      | badgeId       | first-perfect-score      |
      | criteria      | {"score": 100, "attempts": 1} |
    Then the response status should be 200
    And badge should be added to collection
    And celebration animation should be triggered

  # GET /api/gamification/leaderboards/{scope}
  @endpoint @gamification @leaderboard @not-implemented
  Scenario: View class leaderboard
    When I send a GET request to "/api/gamification/leaderboards/class-456?privacy=anonymous"
    Then the response status should be 200
    And leaderboard should show:
      | rank | displayName | points | badges |
      | 1    | Star Player | 1250   | 15     |
      | 2    | Champion    | 1100   | 12     |
      | 3    | Rising Star | 950    | 10     |

  # POST /api/gamification/rewards/store
  @endpoint @gamification @rewards @not-implemented
  Scenario: Access custom reward store
    When I send a POST request to "/api/gamification/rewards/store" with:
      | field         | value                    |
      | studentId     | student-123              |
      | storeId       | classroom-store          |
    Then the response status should be 200
    And store should display:
      | item            | cost  | type        |
      | Extra iPad Time | 100   | privilege   |
      | Sticker Pack    | 50    | physical    |
      | Homework Pass   | 200   | privilege   |
      | Virtual Pet     | 150   | digital     |

  # POST /api/gamification/rewards/redeem
  @endpoint @gamification @redeem @not-implemented
  Scenario: Redeem points for rewards
    When I send a POST request to "/api/gamification/rewards/redeem" with:
      | field         | value                    |
      | studentId     | student-123              |
      | rewardId      | extra-ipad-time          |
      | pointsCost    | 100                      |
    Then the response status should be 200
    And points should be deducted
    And reward should be granted
    And redemption history should be updated

  # GET /api/gamification/celebrations
  @endpoint @gamification @celebrations @not-implemented
  Scenario: Get celebration animations
    When I send a GET request to "/api/gamification/celebrations?trigger=level-up"
    Then the response status should be 200
    And celebrations should include:
      | type        | animation           | sound           |
      | confetti    | falling-confetti    | cheer.mp3       |
      | fireworks   | firework-burst      | boom.mp3        |
      | stars       | shooting-stars      | twinkle.mp3     |

  # POST /api/gamification/effort-tracking
  @endpoint @gamification @effort @not-implemented
  Scenario: Track effort-based rewards
    When I send a POST request to "/api/gamification/effort-tracking" with:
      | field         | value                    |
      | studentId     | student-123              |
      | activityId    | activity-789             |
      | attempts      | 5                        |
      | improvement   | 30                       |
      | timeSpent     | 900                      |
    Then the response status should be 200
    And effort points should be calculated
    And persistence badge should be considered

  # PUT /api/gamification/settings/{studentId}
  @endpoint @gamification @settings @not-implemented
  Scenario: Customize gamification settings
    When I send a PUT request to "/api/gamification/settings/student-123" with:
      | field              | value                 |
      | pointsVisible      | true                  |
      | leaderboardOptIn   | false                 |
      | celebrationType    | subtle                |
      | rewardPreferences  | ["digital", "privileges"] |
    Then the response status should be 200
    And settings should be saved
    And experience should be personalized

  # GET /api/gamification/analytics
  @endpoint @gamification @analytics @not-implemented
  Scenario: View gamification analytics
    Given I am a therapist
    When I send a GET request to "/api/gamification/analytics?period=month"
    Then the response status should be 200
    And analytics should show:
      | metric              | value                 |
      | engagementIncrease  | 45%                   |
      | avgPointsPerStudent | 750                   |
      | mostPopularRewards  | ["iPad time", "stickers"] |
      | completionRates     | 85%                   |