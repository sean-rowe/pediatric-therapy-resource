Feature: External Integration API Endpoints (FR-022)
  As a platform user
  I want to integrate with external systems
  So that I can streamline my workflow

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # GET /api/integrations/available
  @endpoint @integrations @discovery @not-implemented
  Scenario: List available integrations
    When I send a GET request to "/api/integrations/available"
    Then the response status should be 200
    And the response should contain array of:
      | field         | type    |
      | integrationId | string  |
      | name          | string  |
      | category      | string  |
      | description   | string  |
      | status        | string  |
      | configured    | boolean |

  # POST /api/integrations/ehr/connect
  @endpoint @integrations @ehr @not-implemented
  Scenario: Connect EHR system
    When I send a POST request to "/api/integrations/ehr/connect" with:
      | field       | value                    |
      | provider    | SimplePractice           |
      | apiKey      | encrypted-api-key        |
      | practiceId  | practice-123             |
      | syncOptions | ["sessions", "notes"]    |
    Then the response status should be 200
    And the response should contain:
      | field          | type   |
      | connectionId   | string |
      | status         | string |
      | lastSync       | string |
      | capabilities   | array  |

  # GET /api/integrations/ehr/{connectionId}/sync
  @endpoint @integrations @ehr @not-implemented
  Scenario: Sync with EHR
    Given EHR connection "conn-123" exists
    When I send a POST request to "/api/integrations/ehr/conn-123/sync"
    Then the response status should be 202
    And the response should contain:
      | field      | type   |
      | syncJobId  | string |
      | status     | string |
      | itemsToSync| number |

  # POST /api/integrations/lms/connect
  @endpoint @integrations @lms @not-implemented
  Scenario: Connect to LMS
    When I send a POST request to "/api/integrations/lms/connect" with:
      | field        | value              |
      | lmsType      | google-classroom   |
      | authCode     | oauth-code-123     |
      | permissions  | ["create", "grade"]|
    Then the response status should be 200
    And OAuth flow should complete
    And classes should be imported

  # POST /api/integrations/lms/{connectionId}/assign
  @endpoint @integrations @lms-assignment @not-implemented
  Scenario: Create LMS assignment
    Given LMS connection "lms-123" exists
    When I send a POST request to "/api/integrations/lms/lms-123/assign" with:
      | field         | value                    |
      | classId       | class-456                |
      | resourceIds   | ["res-123", "res-124"]   |
      | title         | Speech Practice Week 3   |
      | dueDate       | 2024-01-29               |
      | points        | 100                      |
    Then the response status should be 201
    And assignment should appear in LMS
    And students should be notified

  # POST /api/integrations/sso/configure
  @endpoint @integrations @sso @not-implemented
  Scenario: Configure SSO provider
    Given I have admin permissions
    When I send a POST request to "/api/integrations/sso/configure" with:
      | field         | value                         |
      | provider      | clever                        |
      | clientId      | client-123                    |
      | clientSecret  | encrypted-secret              |
      | redirectUri   | https://app.uptrms.com/auth   |
      | scopes        | ["profile", "roster"]         |
    Then the response status should be 200
    And SSO should be configured
    And test connection should succeed

  # GET /api/integrations/calendar/available-slots
  @endpoint @integrations @calendar @not-implemented
  Scenario: Get calendar availability
    Given calendar integration is connected
    When I send a GET request to "/api/integrations/calendar/available-slots?date=2024-01-22"
    Then the response status should be 200
    And the response should contain:
      | field      | type  |
      | slots      | array |
      | timezone   | string|
      | conflicts  | array |

  # POST /api/integrations/calendar/schedule
  @endpoint @integrations @scheduling @not-implemented
  Scenario: Schedule via calendar integration
    When I send a POST request to "/api/integrations/calendar/schedule" with:
      | field        | value                    |
      | studentId    | student-123              |
      | dateTime     | 2024-01-22T10:00:00Z     |
      | duration     | 30                       |
      | recurring    | weekly                   |
      | endDate      | 2024-05-30               |
    Then the response status should be 201
    And calendar events should be created
    And invitations should be sent

  # POST /api/integrations/drive/import
  @endpoint @integrations @cloud-storage @not-implemented
  Scenario: Import from cloud storage
    When I send a POST request to "/api/integrations/drive/import" with:
      | field       | value                    |
      | provider    | google-drive             |
      | folderId    | folder-123               |
      | fileTypes   | ["pdf", "docx"]          |
      | importAs    | draft                    |
    Then the response status should be 202
    And import job should start
    And progress should be trackable

  # POST /api/integrations/marketplace/sync
  @endpoint @integrations @marketplace @not-implemented
  Scenario: Sync with external marketplace
    Given I am a marketplace seller
    When I send a POST request to "/api/integrations/marketplace/sync" with:
      | field         | value              |
      | marketplace   | etsy               |
      | storeId       | store-123          |
      | syncDirection | bidirectional      |
    Then the response status should be 200
    And products should sync
    And inventory should update

  # POST /api/integrations/email/configure
  @endpoint @integrations @email @not-implemented
  Scenario: Configure email integration
    When I send a POST request to "/api/integrations/email/configure" with:
      | field        | value                    |
      | provider     | sendgrid                 |
      | apiKey       | encrypted-key            |
      | fromEmail    | clinic@example.com       |
      | fromName     | Sunshine Therapy         |
    Then the response status should be 200
    And email sending should be configured
    And test email should be sent

  # GET /api/integrations/status
  @endpoint @integrations @monitoring @not-implemented
  Scenario: Get all integration statuses
    When I send a GET request to "/api/integrations/status"
    Then the response status should be 200
    And the response should contain array of:
      | field           | type   |
      | integrationName | string |
      | status          | string |
      | lastActivity    | string |
      | errorCount      | number |
      | nextSync        | string |

  # DELETE /api/integrations/{integrationId}
  @endpoint @integrations @disconnect @not-implemented
  Scenario: Disconnect integration
    Given integration "int-123" is connected
    When I send a DELETE request to "/api/integrations/int-123"
    Then the response status should be 200
    And integration should be disconnected
    And data should be retained
    And audit log should be created

  # POST /api/integrations/webhooks
  @endpoint @integrations @webhooks @not-implemented
  Scenario: Configure webhook
    When I send a POST request to "/api/integrations/webhooks" with:
      | field      | value                         |
      | url        | https://example.com/webhook   |
      | events     | ["resource.created", "student.progress"] |
      | secret     | webhook-secret                |
      | active     | true                          |
    Then the response status should be 201
    And webhook should be configured
    And test payload should be sent

  # GET /api/integrations/logs/{integrationId}
  @endpoint @integrations @logs @not-implemented
  Scenario: Get integration logs
    Given integration "int-123" has activity
    When I send a GET request to "/api/integrations/logs/int-123?days=7"
    Then the response status should be 200
    And the response should contain:
      | field     | type  |
      | logs      | array |
      | summary   | object|
      | errors    | array |

  # FR-022 Missing Critical Multi-Platform Sync Workflows
  @marketplace @multi-platform @etsy-sync @not-implemented
  Scenario: Sync marketplace products with Etsy integration
    Given I am a verified seller with Etsy integration
    And I have 20 products in my UPTRMS store
    When I enable Etsy synchronization
    And I configure sync settings:
      | Setting            | Value                          |
      | Sync Direction     | Bidirectional                  |
      | Price Sync         | Enabled                        |
      | Inventory Sync     | Enabled                        |
      | Description Sync   | UPTRMS to Etsy only           |
      | Image Sync         | Enabled                        |
    Then all qualifying products should sync to Etsy within 1 hour
    And Etsy product listings should match UPTRMS data:
      | Field              | Sync Status                    |
      | Product Title      | Synced with SEO optimization   |
      | Description        | Formatted for Etsy audience    |
      | Price             | Synced with currency conversion |
      | Inventory Count    | Real-time sync                 |
      | Product Images     | Optimized for Etsy display     |
    When I update a product price in UPTRMS
    Then Etsy price should update within 15 minutes
    And sync log should record the change

  @marketplace @multi-platform @amazon-sync @not-implemented
  Scenario: Sync marketplace products with Amazon integration
    Given I am a verified seller with Amazon Handmade account
    And I have Amazon integration configured
    When I select products to sync with Amazon
    And I configure Amazon-specific settings:
      | Setting            | Value                          |
      | Category Mapping   | Crafts & Sewing > Educational  |
      | Fulfillment Method | Merchant Fulfilled            |
      | Shipping Templates | Standard Education Materials   |
      | Tax Settings       | Use Amazon tax calculation     |
    Then products should appear in Amazon Handmade
    And Amazon listings should include:
      | Required Field     | Source                         |
      | Product Title      | Auto-generated from UPTRMS     |
      | Bullet Points      | Generated from features        |
      | Product Description| Formatted for Amazon audience  |
      | Keywords           | Auto-generated SEO tags        |
      | Images             | Optimized for Amazon display   |
    When order is placed on Amazon
    Then UPTRMS should receive order notification
    And inventory should decrease automatically
    And fulfillment workflow should begin

  @video-content @youtube-integration @not-implemented
  Scenario: Integrate YouTube content with therapy resources
    Given I am a content creator with YouTube channel
    And I have YouTube integration enabled
    When I upload therapy demonstration videos to YouTube
    And I tag videos with UPTRMS integration codes
    Then videos should be automatically linked to related resources
    And UPTRMS should display:
      | Integration Feature| Implementation                 |
      | Video Thumbnails   | Auto-generated from YouTube    |
      | Video Descriptions | Synced from YouTube metadata   |
      | View Counts        | Real-time YouTube analytics    |
      | Comments           | Moderated YouTube comments     |
    When users access resources in UPTRMS
    Then related YouTube videos should appear in sidebar
    And video engagement should be tracked for analytics

  @social-media @tiktok-integration @not-implemented
  Scenario: Integrate TikTok therapy tips with platform content
    Given I am a therapist with TikTok account
    And I create short therapy tip videos
    When I connect my TikTok account to UPTRMS
    And I tag TikTok videos with therapy resource hashtags
    Then UPTRMS should:
      | Integration Feature| Implementation                 |
      | Import TikTok Videos| Embed approved videos         |
      | Hashtag Matching   | Link to relevant resources     |
      | Engagement Metrics | Track views and shares         |
      | Content Moderation | Review before platform display |
    When I create new therapy resource
    Then system should suggest related TikTok content
    And cross-platform engagement should be tracked

  @pinterest @board-creation @not-implemented
  Scenario: Create Pinterest boards from therapy resources
    Given I am a therapist with Pinterest business account
    And I have Pinterest integration configured
    When I select therapy resources to share on Pinterest
    And I configure Pinterest board settings:
      | Setting            | Value                          |
      | Board Name         | Pediatric Therapy Resources    |
      | Board Description  | Evidence-based therapy materials|
      | Board Category     | Education                      |
      | Pin Descriptions   | Auto-generated from resources  |
    Then Pinterest boards should be created automatically
    And resource images should be optimized for Pinterest
    And pins should include:
      | Pin Element        | Content                        |
      | Pin Title          | Resource title + age range     |
      | Pin Description    | Therapy benefits summary       |
      | Pin Image          | Optimized resource preview     |
      | Pin Link           | Direct link to UPTRMS resource |
    When Pinterest users save or click pins
    Then UPTRMS should track referral traffic
    And creator should receive attribution credit

  @inventory-management @unified-dashboard @not-implemented
  Scenario: Manage inventory across multiple platforms
    Given I sell on UPTRMS, Etsy, and Amazon
    And I have multi-platform sync enabled
    When I view my unified inventory dashboard
    Then I should see real-time inventory across all platforms:
      | Platform           | Inventory Display              |
      | UPTRMS             | Primary inventory count        |
      | Etsy               | Synced count with status       |
      | Amazon             | Synced count with status       |
      | TeachersPayTeachers| Synced count with status       |
    When inventory reaches low stock threshold
    Then I should receive alerts across all platforms
    And automatic reorder suggestions should appear
    When product sells out on one platform
    Then all platforms should update to "Out of Stock"
    And customers should see restock notifications

  @analytics @cross-platform @unified-reporting @not-implemented
  Scenario: Generate unified analytics across platforms
    Given I have products on multiple platforms
    When I access cross-platform analytics dashboard
    Then I should see unified metrics:
      | Metric Type        | Data Aggregation               |
      | Total Sales        | Sum across all platforms       |
      | Revenue by Platform| Breakdown by marketplace       |
      | Traffic Sources    | Referral tracking              |
      | Customer Geography | Combined location data         |
      | Product Performance| Top sellers across platforms   |
      | Seasonal Trends    | Multi-platform trend analysis  |
    And I should be able to:
      | Action             | Capability                     |
      | Export Reports     | CSV, PDF formats available    |
      | Set Date Ranges    | Custom period analysis        |
      | Compare Platforms  | Side-by-side performance      |
      | Track ROI          | Platform-specific profitability|
    When I generate monthly report
    Then it should include recommendations for optimization

  @pricing-strategy @dynamic-pricing @not-implemented
  Scenario: Implement dynamic pricing across platforms
    Given I have products on multiple marketplaces
    When I enable dynamic pricing strategy
    And I configure pricing rules:
      | Rule Type          | Configuration                  |
      | Competitive Pricing| Match lowest competitor price  |
      | Platform Fees      | Adjust for marketplace fees    |
      | Seasonal Adjustments| Increase during back-to-school |
      | Volume Discounts   | Bulk pricing for schools       |
    Then pricing should adjust automatically across platforms
    And price changes should be logged with reasoning
    When competitor changes price
    Then my pricing should respond within 24 hours
    And I should receive notification of price changes

  @order-management @fulfillment-routing @not-implemented
  Scenario: Route orders efficiently across platforms
    Given I receive orders from multiple platforms
    When orders are placed simultaneously
    Then system should route orders based on:
      | Routing Factor     | Priority Logic                 |
      | Platform Fees      | Lower fee platforms first      |
      | Fulfillment Speed  | Fastest delivery method        |
      | Inventory Location | Nearest fulfillment center     |
      | Customer Preferences| Expedited vs standard shipping |
    And order management should include:
      | Feature            | Implementation                 |
      | Order Consolidation| Combine multiple item orders   |
      | Automatic Tracking | Update all platforms with status|
      | Customer Communication| Unified messaging system    |
      | Returns Processing | Handle returns across platforms|
    When order is fulfilled
    Then all platforms should receive tracking updates
    And customer should receive unified confirmation