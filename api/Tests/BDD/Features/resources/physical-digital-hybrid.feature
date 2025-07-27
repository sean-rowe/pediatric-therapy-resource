Feature: Physical/Digital Hybrid API Endpoints (FR-013)
  As a therapy professional
  I want to integrate physical therapy materials with digital features
  So that I can enhance traditional materials with technology

  Background:
    Given the API is available
    And I am authenticated as "therapist@clinic.com"

  # POST /api/hybrid/products/register
  @endpoint @hybrid @registration @not-implemented
  Scenario: Register physical product for digital access
    When I send a POST request to "/api/hybrid/products/register" with:
      | field          | value                    |
      | productCode    | SENSORY-CARDS-2024       |
      | purchaseDate   | 2024-01-15               |
      | receiptNumber  | REC-123456               |
      | email          | therapist@clinic.com     |
    Then the response status should be 200
    And the response should contain:
      | field              | type   |
      | digitalAccessCode  | string |
      | unlockedContent    | array  |
      | expirationDate     | string |

  # POST /api/hybrid/qr-scan
  @endpoint @hybrid @qr @not-implemented
  Scenario: Scan QR code from physical product
    When I send a POST request to "/api/hybrid/qr-scan" with:
      | field      | value                         |
      | qrData     | QR://UPTRMS/PROD/CARD-SET-123 |
      | deviceId   | ipad-456                      |
      | location   | therapy-room                  |
    Then the response status should be 200
    And the response should contain:
      | field           | type   |
      | productName     | string |
      | digitalContent  | array  |
      | arEnabled       | boolean |
      | sessionLink     | string |

  # GET /api/hybrid/products/{productId}/digital-content
  @endpoint @hybrid @content @not-implemented
  Scenario: Access digital companions for physical product
    Given I own physical product "prod-123"
    When I send a GET request to "/api/hybrid/products/prod-123/digital-content"
    Then the response status should be 200
    And the response should contain:
      | field            | type   |
      | videos           | array  |
      | printables       | array  |
      | apps             | array  |
      | arMarkers        | array  |
      | instructions     | object |

  # POST /api/hybrid/ar/session
  @endpoint @hybrid @ar @not-implemented
  Scenario: Start AR session with physical materials
    When I send a POST request to "/api/hybrid/ar/session" with:
      | field         | value                    |
      | productId     | anatomy-cards-123        |
      | deviceType    | tablet                   |
      | cameraAccess  | granted                  |
    Then the response status should be 201
    And the response should contain:
      | field          | type   |
      | sessionId      | string |
      | arModels       | array  |
      | trackingConfig | object |
      | interactionMap | object |

  # POST /api/hybrid/print-on-demand
  @endpoint @hybrid @pod @not-implemented
  Scenario: Order print-on-demand materials
    When I send a POST request to "/api/hybrid/print-on-demand" with:
      | field           | value                    |
      | resourceIds     | ["res-123", "res-124"]   |
      | customization   | {"name": "Emma's Book"}  |
      | quantity        | 1                        |
      | binding         | spiral                   |
      | shipping        | standard                 |
    Then the response status should be 201
    And the response should contain:
      | field          | type   |
      | orderId        | string |
      | estimatedCost  | number |
      | deliveryDate   | string |
      | trackingInfo   | object |

  # GET /api/hybrid/bundles
  @endpoint @hybrid @bundles @not-implemented
  Scenario: Browse physical/digital bundles
    When I send a GET request to "/api/hybrid/bundles?category=sensory"
    Then the response status should be 200
    And the response should contain array of:
      | field             | type    |
      | bundleId          | string  |
      | name              | string  |
      | physicalItems     | array   |
      | digitalItems      | array   |
      | totalValue        | number  |
      | bundlePrice       | number  |
      | shippingIncluded  | boolean |

  # POST /api/hybrid/verify-authenticity
  @endpoint @hybrid @verification @not-implemented
  Scenario: Verify product authenticity
    When I send a POST request to "/api/hybrid/verify-authenticity" with:
      | field            | value                    |
      | hologramCode     | HOLO-123-456-789         |
      | productSerial    | SN-2024-001234           |
    Then the response status should be 200
    And the response should contain:
      | field       | type    |
      | authentic   | boolean |
      | productInfo | object  |
      | warranty    | object  |

  # POST /api/hybrid/shipping-calculator
  @endpoint @hybrid @shipping @not-implemented
  Scenario: Calculate shipping for physical items
    When I send a POST request to "/api/hybrid/shipping-calculator" with:
      | field         | value                    |
      | items         | [{"id": "kit-123", "qty": 2}] |
      | destination   | {"zip": "90210", "country": "US"} |
      | expedited     | false                    |
    Then the response status should be 200
    And the response should contain:
      | field          | type   |
      | shippingCost   | number |
      | deliveryTime   | string |
      | carriers       | array  |

  # GET /api/hybrid/inventory/{productId}
  @endpoint @hybrid @inventory @not-implemented
  Scenario: Check physical product availability
    When I send a GET request to "/api/hybrid/inventory/sensory-kit-pro"
    Then the response status should be 200
    And the response should contain:
      | field         | type    |
      | inStock       | boolean |
      | quantity      | number  |
      | backorderDate | string  |
      | locations     | array   |

  # FR-013 Missing Critical User Workflow Scenarios
  @qr-code @physical-cards @workflow @not-implemented
  Scenario: Complete QR code workflow from physical card to digital activity
    Given I have purchased "Articulation Card Deck - R Sounds"
    And each card has a unique QR code
    When I scan the QR code on "Rabbit" card using mobile app
    Then I should receive digital content within 3 seconds:
      | Digital Feature    | Content                        |
      | Audio model        | Native speaker pronunciation   |
      | Video model        | Mouth position close-up        |
      | Practice games     | Digital activities with card   |
      | Progress tracking  | Log correct/incorrect          |
    And I should be able to:
      | Action             | Result                         |
      | Record student     | Compare to model               |
      | Play minimal pairs | Rabbit vs Wabbit contrast     |
      | Access home version| Parent scans same code        |
      | Track usage        | Automatic session logging     |

  @print-on-demand @customization @workflow @not-implemented
  Scenario: Complete print-on-demand workflow for custom communication book
    Given I am designing custom communication book for student "Jake"
    When I access the print-on-demand service
    And I specify customization options:
      | Customization      | Details                        |
      | Student name       | "Jake's Communication Book"    |
      | Core vocabulary    | 48 most-used words            |
      | Personal photos    | Family members, favorite items |
      | Size/binding       | 8.5x11", spiral bound         |
      | Lamination         | Heavy duty, wipeable           |
    Then print preview should display:
      | Feature            | Appearance                     |
      | Cover page         | Student name and photo         |
      | Organization       | Tabbed sections by category    |
      | Symbols            | Consistent with digital use    |
      | Durability         | Reinforced corners             |
    When I complete the order
    Then I should see order confirmation with:
      | Order Detail       | Information                    |
      | Production time    | 3-5 business days             |
      | Shipping options   | Standard or expedited         |
      | Cost breakdown     | Materials, printing, shipping |
      | Digital copy       | Included for backup           |

  @ar-features @interactive-print @workflow @not-implemented
  Scenario: Complete augmented reality workflow with printed worksheets
    Given I have AR-enabled worksheets
    And student has tablet with AR app installed
    When student points tablet camera at worksheet
    Then AR features should activate within 5 seconds:
      | Worksheet Type     | AR Enhancement                 |
      | Anatomy diagram    | 3D rotating body systems       |
      | Math problems      | Animated problem solving       |
      | Handwriting        | Tracing guides appear          |
      | Categories         | Items float to correct boxes   |
    And interaction should include:
      | Feature            | Function                       |
      | Touch targets      | Tap to hear names/sounds       |
      | Animation          | Show correct technique         |
      | Rewards            | Virtual stickers when complete |
      | Data capture       | Track accuracy and time        |
    When worksheet is completed
    Then AR app should:
      | Action             | Result                         |
      | Save work          | Digital copy of completed work |
      | Generate report    | Performance summary            |
      | Unlock reward      | New AR character or game       |

  @hybrid-bundles @value-packs @workflow @not-implemented
  Scenario: Purchase and receive physical/digital bundle packages
    Given I want comprehensive sensory program
    When I view "Sensory Diet Starter Kit" bundle
    Then bundle should include:
      | Physical Items     | Digital Components             |
      | Therapy putty      | Exercise videos               |
      | Balance disc       | Activity cards (printable)    |
      | Sensory balls      | Progress tracking sheets      |
      | Visual timers      | Digital timer app access      |
      | Instruction manual | Online video course           |
    And digital components should provide:
      | Feature            | Access                         |
      | Immediate access   | Download upon purchase         |
      | Updates            | New activities added monthly   |
      | Community          | Private user group            |
      | Certification      | Complete course for CEUs       |
    When I purchase bundle
    Then fulfillment should include:
      | Component          | Delivery                       |
      | Physical items     | Shipped within 2 days          |
      | Digital access     | Immediate email with login     |
      | QR cards           | Link physical to digital       |
      | Support            | Setup video call included      |

  @digital-companion @offline-sync @workflow @not-implemented
  Scenario: Use digital companion apps with offline capability
    Given I have physical card decks with digital companions
    And I am working in area with poor internet connection
    When I scan QR codes to access digital content
    Then the app should:
      | Offline Feature    | Capability                     |
      | Cache content      | Download for offline use       |
      | Track progress     | Store locally until sync       |
      | Audio playback     | Work without internet          |
      | Save recordings    | Store student responses        |
    When internet connection is restored
    Then the app should:
      | Sync Action        | Result                         |
      | Upload progress    | Send to therapist dashboard    |
      | Download updates   | Get new content automatically  |
      | Backup recordings  | Store in secure cloud          |
      | Merge conflicts    | Handle duplicate entries       |

  @marketplace @physical-products @workflow @not-implemented
  Scenario: Sell physical products through marketplace platform
    Given I am a verified seller with physical products
    When I list "Custom Visual Schedule Cards" for sale
    And I configure product options:
      | Option             | Choices                        |
      | Size               | 2x2", 3x3", 4x4"             |
      | Quantity           | 10, 20, 50 cards             |
      | Lamination         | Standard, heavy duty          |
      | Customization      | Add student name              |
    Then the system should:
      | Feature            | Implementation                 |
      | Calculate shipping | Based on size and weight      |
      | Manage inventory   | Track stock levels            |
      | Handle orders      | Send to print fulfillment     |
      | Track delivery     | Provide tracking numbers      |
    When customer places order
    Then fulfillment process should:
      | Step               | Timeline                       |
      | Order confirmation | Immediate                     |
      | Production start   | Within 24 hours               |
      | Quality check      | Before shipping               |
      | Shipping           | 2-3 business days production  |
      | Delivery tracking  | Updates sent to customer      |

  @ar-markers @tracking @performance @not-implemented
  Scenario: Track AR marker recognition performance and optimization
    Given I am using AR-enabled therapy materials
    When students interact with AR markers over multiple sessions
    Then the system should track performance metrics:
      | Performance Metric | Measurement                    |
      | Recognition speed  | Time to detect marker          |
      | Tracking stability | How well marker stays tracked  |
      | User engagement    | Time spent with AR content     |
      | Completion rates   | Activities finished            |
    And provide analytics on:
      | Analytics          | Purpose                        |
      | Marker effectiveness| Which markers work best        |
      | Device performance | AR capability by device        |
      | Usage patterns     | When/how AR is used           |
      | Student outcomes   | Learning improvement with AR   |
    When performance issues are detected
    Then the system should:
      | Issue Type         | Auto-Response                  |
      | Poor tracking      | Suggest lighting adjustments   |
      | Slow recognition   | Recommend marker cleaning      |
      | Low engagement     | Suggest alternative activities |
      | Device limitations | Provide compatibility warnings |