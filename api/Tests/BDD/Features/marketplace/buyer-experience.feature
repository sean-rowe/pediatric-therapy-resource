Feature: Marketplace Buyer Experience API Endpoints
  As a therapy professional
  I want to purchase resources from the marketplace
  So that I can access specialized content from other professionals

  Background:
    Given the API is available
    And I am authenticated as "buyer@clinic.com"

  # GET /api/marketplace/products
  @endpoint @marketplace @browse @not-implemented
  Scenario: Browse marketplace products
    When I send a GET request to "/api/marketplace/products?category=speech-therapy&sort=popular"
    Then the response status should be 200
    And the response should contain:
      | field    | type   |
      | products | array  |
      | total    | number |
      | facets   | object |
    And each product should contain:
      | field          | type   |
      | id             | string |
      | title          | string |
      | price          | number |
      | rating         | number |
      | sellerName     | string |
      | thumbnailUrl   | string |
      | instantDownload| boolean |

  # GET /api/marketplace/products/{id}
  @endpoint @marketplace @products @not-implemented
  Scenario: View marketplace product details
    Given marketplace product "prod-123" exists
    When I send a GET request to "/api/marketplace/products/prod-123"
    Then the response status should be 200
    And the response should contain:
      | field            | type    |
      | id               | string  |
      | title            | string  |
      | description      | string  |
      | price            | number  |
      | compareAtPrice   | number  |
      | seller           | object  |
      | previewImages    | array   |
      | includedResources| array   |
      | rating           | number  |
      | reviewCount      | number  |
      | salesCount       | number  |
      | licenseType      | string  |
      | instantDownload  | boolean |
      | lastUpdated      | string  |

  # POST /api/marketplace/cart
  @endpoint @marketplace @cart @not-implemented
  Scenario: Add product to cart
    Given marketplace product "prod-123" exists
    When I send a POST request to "/api/marketplace/cart" with:
      | field     | value    |
      | productId | prod-123 |
      | quantity  | 1        |
    Then the response status should be 200
    And the response should contain:
      | field      | type   |
      | cartId     | string |
      | itemCount  | number |
      | subtotal   | number |
    And the product should be in my cart

  # GET /api/marketplace/cart
  @endpoint @marketplace @cart @not-implemented
  Scenario: View shopping cart
    Given I have 3 items in my cart
    When I send a GET request to "/api/marketplace/cart"
    Then the response status should be 200
    And the response should contain:
      | field      | type   |
      | items      | array  |
      | subtotal   | number |
      | tax        | number |
      | total      | number |
      | savings    | number |
    And each item should show product details

  # DELETE /api/marketplace/cart/{itemId}
  @endpoint @marketplace @cart @not-implemented
  Scenario: Remove item from cart
    Given I have item "item-123" in my cart
    When I send a DELETE request to "/api/marketplace/cart/item-123"
    Then the response status should be 200
    And the item should be removed from cart
    And cart totals should be updated

  # POST /api/marketplace/checkout
  @endpoint @marketplace @checkout @not-implemented
  Scenario: Initiate checkout process
    Given I have items in my cart totaling $49.99
    When I send a POST request to "/api/marketplace/checkout" with:
      | field            | value              |
      | paymentMethodId  | pm_saved_card      |
      | billingAddress   | {...}              |
      | applyCoupon      | SAVE10             |
    Then the response status should be 200
    And the response should contain:
      | field          | type   |
      | checkoutId     | string |
      | subtotal       | number |
      | discount       | number |
      | tax            | number |
      | total          | number |
      | paymentIntent  | string |

  # POST /api/marketplace/checkout/{id}/complete
  @endpoint @marketplace @checkout @not-implemented
  Scenario: Complete purchase
    Given I have checkout session "checkout-123"
    When I send a POST request to "/api/marketplace/checkout/checkout-123/complete"
    Then the response status should be 200
    And the response should contain:
      | field         | type   |
      | orderId       | string |
      | status        | string |
      | downloadLinks | array  |
      | receipt       | string |
    And products should be available for download
    And sellers should be notified
    And commission should be calculated

  # GET /api/marketplace/orders
  @endpoint @marketplace @orders @not-implemented
  Scenario: View purchase history
    Given I have made marketplace purchases
    When I send a GET request to "/api/marketplace/orders"
    Then the response status should be 200
    And the response should contain array of:
      | field        | type   |
      | orderId      | string |
      | orderDate    | string |
      | items        | array  |
      | total        | number |
      | status       | string |
      | downloadLinks| array  |

  # GET /api/marketplace/orders/{id}
  @endpoint @marketplace @orders @not-implemented
  Scenario: View order details
    Given I have order "order-123"
    When I send a GET request to "/api/marketplace/orders/order-123"
    Then the response status should be 200
    And the response should contain:
      | field         | type   |
      | orderId       | string |
      | items         | array  |
      | payment       | object |
      | downloadHistory| array |
      | licenseKey    | string |

  # GET /api/marketplace/downloads
  @endpoint @marketplace @downloads @not-implemented
  Scenario: Access purchased downloads
    Given I have purchased marketplace products
    When I send a GET request to "/api/marketplace/downloads"
    Then the response status should be 200
    And the response should contain array of:
      | field          | type   |
      | productId      | string |
      | productName    | string |
      | purchaseDate   | string |
      | downloadUrl    | string |
      | expiresAt      | string |
      | timesDownloaded| number |

  # POST /api/marketplace/downloads/{id}/redownload
  @endpoint @marketplace @downloads @not-implemented
  Scenario: Request new download link
    Given I purchased product "prod-123" 
    And download link has expired
    When I send a POST request to "/api/marketplace/downloads/prod-123/redownload"
    Then the response status should be 200
    And the response should contain:
      | field       | type   |
      | downloadUrl | string |
      | expiresAt   | string |
    And download should be tracked

  # POST /api/marketplace/reviews
  @endpoint @marketplace @reviews @not-implemented
  Scenario: Submit product review
    Given I purchased product "prod-123" at least 24 hours ago
    When I send a POST request to "/api/marketplace/reviews" with:
      | field     | value                                    |
      | productId | prod-123                                 |
      | rating    | 5                                        |
      | title     | Excellent resource!                      |
      | comment   | These cards work great with my students  |
      | verified  | true                                     |
    Then the response status should be 201
    And the review should be published
    And marked as verified purchase

  # POST /api/marketplace/reviews/{id}/helpful
  @endpoint @marketplace @reviews @not-implemented
  Scenario: Mark review as helpful
    Given review "rev-123" exists
    When I send a POST request to "/api/marketplace/reviews/rev-123/helpful"
    Then the response status should be 200
    And helpful count should increment
    And I should not be able to vote again

  # GET /api/marketplace/sellers/{id}
  @endpoint @marketplace @sellers @not-implemented
  Scenario: View seller profile
    Given seller "seller-123" exists
    When I send a GET request to "/api/marketplace/sellers/seller-123"
    Then the response status should be 200
    And the response should contain:
      | field         | type   |
      | sellerId      | string |
      | storeName     | string |
      | bio           | string |
      | rating        | number |
      | totalProducts | number |
      | totalSales    | number |
      | joinedDate    | string |
      | specialties   | array  |
      | featuredProducts | array |

  # POST /api/marketplace/sellers/{id}/follow
  @endpoint @marketplace @sellers @not-implemented
  Scenario: Follow a seller
    Given seller "seller-123" exists
    When I send a POST request to "/api/marketplace/sellers/seller-123/follow"
    Then the response status should be 200
    And I should receive seller updates
    And seller follower count should increase

  # GET /api/marketplace/wishlist
  @endpoint @marketplace @wishlist @not-implemented
  Scenario: View wishlist
    Given I have items in my wishlist
    When I send a GET request to "/api/marketplace/wishlist"
    Then the response status should be 200
    And the response should contain array of wished products
    And show price changes since added

  # POST /api/marketplace/wishlist
  @endpoint @marketplace @wishlist @not-implemented
  Scenario: Add to wishlist
    Given product "prod-123" exists
    When I send a POST request to "/api/marketplace/wishlist" with:
      | field     | value    |
      | productId | prod-123 |
    Then the response status should be 200
    And product should be in wishlist
    And I should be notified of price drops

  # POST /api/marketplace/gift
  @endpoint @marketplace @gifting @not-implemented
  Scenario: Purchase as gift
    Given product "prod-123" exists
    When I send a POST request to "/api/marketplace/gift" with:
      | field          | value                    |
      | productId      | prod-123                 |
      | recipientEmail | colleague@clinic.com     |
      | recipientName  | Jane Smith               |
      | message        | Hope this helps!         |
      | sendDate       | 2024-12-25               |
    Then the response status should be 200
    And gift purchase should be processed
    And recipient should receive gift email on send date

  # GET /api/marketplace/recommendations
  @endpoint @marketplace @discovery @not-implemented
  Scenario: Get personalized recommendations
    Given I have purchase history
    When I send a GET request to "/api/marketplace/recommendations"
    Then the response status should be 200
    And recommendations should be based on:
      | factor            |
      | purchase history  |
      | browsing history  |
      | similar buyers    |
      | trending products |

  # FR-008 Comprehensive Marketplace Business Scenarios from CLAUDE.md
  @marketplace @seller-onboarding @verification @not-implemented
  Scenario: Therapist becomes a verified marketplace seller
    Given I want to sell my therapy resources
    When I apply to become a seller
    And I provide required information:
      | Field                  | Value                        |
      | Professional License   | SLP-54321                   |
      | Years of Experience    | 8                           |
      | Specialty Areas        | Autism, Apraxia             |
      | Sample Work            | 3 resource files uploaded   |
      | Tax Information        | W-9 completed               |
    Then my application should be reviewed within 48 hours
    When my application is approved
    Then I should receive seller onboarding materials
    And I should have access to:
      | Feature              | Description                    |
      | Seller Dashboard     | Upload and manage products    |
      | Analytics           | View sales and traffic data   |
      | Storefront          | Customizable seller page      |
      | Direct Deposits     | Monthly payment schedule      |
    And I should be able to set up my seller profile
    And receive training materials for successful selling

  @marketplace @product-listing @quality-control @not-implemented
  Scenario: List a new therapy resource with clinical review
    Given I am an approved seller
    When I create a new product listing:
      | Field              | Value                                    |
      | Title              | Sensory Diet Visual Cards - School Edition|
      | Category           | Sensory Integration                      |
      | Age Range          | 5-12 years                              |
      | Price              | $12.99                                  |
      | License Type       | Single classroom use                    |
      | Preview Images     | 5 sample cards uploaded                 |
      | Description        | 48 visual cards for sensory breaks     |
    And I submit for review
    Then the resource should undergo clinical review:
      | Review Aspect      | Requirement                    |
      | Clinical accuracy  | Evidence-based techniques      |
      | Age appropriateness| Suitable for stated age range  |
      | Quality standards  | Clear images, correct spelling |
      | Copyright         | Original work verification     |
    When review is approved
    Then my product should go live within 24 hours
    And appear in search results
    And I should be notified via email
    And product should be tagged for discoverability

  @marketplace @purchase-flow @instant-delivery @not-implemented
  Scenario: Purchase and download marketplace resource
    Given I found a resource I want to purchase
    And the resource costs $24.99
    When I click "Add to Cart"
    And proceed to checkout
    And apply coupon code "SAVE20"
    Then the price should update to $19.99
    When I complete payment with saved card
    Then I should immediately receive:
      | Item                | Delivery Method               |
      | Download link       | Email and in-app             |
      | Receipt            | Email with tax breakdown      |
      | Resource files     | Secure download (3 attempts)  |
      | License key        | For future reference         |
    And the seller should be notified of the sale
    And commission should be calculated:
      | Amount Type    | Value    |
      | Gross Sale     | $19.99   |
      | Platform Fee   | $6.00    |
      | Seller Earnings| $13.99   |
    And download should be tracked for analytics

  @marketplace @seller-analytics @insights @not-implemented
  Scenario: View comprehensive seller analytics dashboard
    Given I have been selling for 3 months
    And I have 15 products listed
    When I access my seller dashboard
    Then I should see analytics including:
      | Metric                  | Time Period | Data                |
      | Total Sales             | This month  | $1,247.50          |
      | Units Sold              | This month  | 96                 |
      | Conversion Rate         | This month  | 3.2%               |
      | Most Popular Product    | All time    | "AAC First Words"  |
      | Customer Geography      | This month  | US(72%), CA(15%), UK(13%) |
      | Average Rating          | All time    | 4.7/5 (127 reviews)|
    And I should be able to:
      | Action                    | Result                        |
      | Export sales data         | CSV download                  |
      | View individual orders    | Buyer info (anonymized)       |
      | Respond to reviews        | Public seller responses       |
      | Schedule promotions       | Discount periods             |
    And I should see traffic sources and conversion funnels
    And receive recommendations for improving sales

  @marketplace @storefront-customization @branding @not-implemented
  Scenario: Create and customize seller storefront
    Given I am an approved seller
    When I access my storefront settings
    Then I should be able to customize:
      | Element            | Options                        |
      | Store name         | Business or personal name      |
      | Store URL          | Custom subdomain               |
      | Banner image       | Header graphic                 |
      | Bio section        | About me and credentials       |
      | Featured products  | Highlight best sellers         |
      | Color scheme       | Brand colors                   |
    And I should be able to organize products by:
      | Organization Method| Purpose                        |
      | Categories         | Group similar products         |
      | Collections        | Themed product bundles         |
      | Sale sections      | Promotional items              |
      | New arrivals       | Recently added products        |
    When I save my storefront changes
    Then customers should see updated branding
    And SEO should be optimized for discovery

  @marketplace @bundle-creation @pricing-strategy @not-implemented
  Scenario: Create product bundles with volume pricing
    Given I have multiple related products
    When I create a product bundle:
      | Bundle Component   | Individual Price | Bundle Inclusion |
      | Articulation Cards | $15.99          | Yes             |
      | Data Sheets        | $8.99           | Yes             |
      | Parent Handouts    | $12.99          | Yes             |
    And I set bundle price to $29.99
    Then customers should see:
      | Information        | Display                       |
      | Bundle savings     | Save $7.98 (21% off)         |
      | Individual prices  | Compare to separate purchases |
      | Bundle contents    | All included items listed     |
    And bundle should appear in search results
    And individual products should show bundle suggestion
    And analytics should track bundle performance

  @marketplace @review-system @trust-building @not-implemented
  Scenario: Comprehensive review and rating system
    Given I purchased a resource 3 days ago
    When I leave a review:
      | Field          | Value                           |
      | Overall Rating | 5 stars                        |
      | Quality Rating | 5 stars                        |
      | Value Rating   | 4 stars                        |
      | Review Title   | Perfect for my autism students  |
      | Written Review | These cards work beautifully... |
    Then the review should be published
    And marked as "Verified Purchase"
    And seller should be notified
    And review should update seller's average rating
    When other buyers view the product
    Then they should see:
      | Review Feature     | Display                        |
      | Overall ratings    | Breakdown by star count        |
      | Verified purchases | Badges for confirmed buyers    |
      | Helpful votes      | Community feedback on reviews  |
      | Seller responses   | Professional replies           |
    And reviews should be sortable by relevance, date, rating

  @marketplace @seller-support-system @success-tools @not-implemented
  Scenario: Seller support and success tools
    Given I am a new seller
    When I access seller resources
    Then I should have access to:
      | Resource Type      | Content                        |
      | Getting Started    | Video tutorials and guides     |
      | Best Practices     | Proven strategies for success  |
      | Marketing Tools    | Social media templates         |
      | Legal Resources    | Copyright and licensing info   |
      | Community Forum    | Seller peer support            |
    And I should receive:
      | Communication      | Frequency                      |
      | Success newsletter | Monthly tips and updates       |
      | Performance alerts | When metrics change            |
      | Policy updates     | Important changes              |
      | Seasonal guidance  | Holiday selling strategies     |
    When I have questions or issues
    Then I should be able to:
      | Support Channel    | Response Time                  |
      | Email support      | 24 hours                      |
      | Live chat          | Business hours                |
      | Phone support      | Premium sellers only          |
      | Community forum    | Peer assistance               |

  @marketplace @seasonal-promotions @marketing-events @not-implemented
  Scenario: Participate in seasonal promotions and sales events
    Given it's approaching back-to-school season
    When I opt into seasonal promotions
    Then I should be able to:
      | Promotion Type     | Details                        |
      | Flash sales        | Limited-time discounts         |
      | Seasonal collections| Themed product groupings       |
      | Featured placement | Higher search visibility       |
      | Bundle deals       | Cross-seller collaborations    |
    And promotional materials should include:
      | Material Type      | Content                        |
      | Email templates    | Customer communication         |
      | Social media posts | Ready-to-share content         |
      | Banner graphics    | Professional promotional images|
      | Product badges     | "Sale" and "Featured" labels   |
    When customers browse during promotions
    Then they should see:
      | Promotion Feature  | Display                        |
      | Sale pricing       | Original and discounted prices |
      | Limited time       | Countdown timers               |
      | Special collections| Curated seasonal content       |
      | Bulk discounts     | Volume pricing incentives      |

  @marketplace @customer-communication @relationship-building @not-implemented
  Scenario: Manage customer relationships and communication
    Given I have customers who purchased my products
    When I access customer communication tools
    Then I should be able to:
      | Communication Type | Purpose                        |
      | Thank you messages | Post-purchase appreciation     |
      | Product updates    | Notify about new versions      |
      | Educational content| Share tips and best practices  |
      | Exclusive offers   | Reward loyal customers         |
    And I should have access to:
      | Feature            | Capability                     |
      | Customer lists     | Segment by purchase history    |
      | Email templates    | Professional messaging         |
      | Automated workflows| Trigger-based communications   |
      | Feedback collection| Gather improvement suggestions |
    When customers contact me
    Then I should be able to:
      | Response Type      | Method                         |
      | Q&A responses      | Public product page answers    |
      | Support tickets    | Private customer assistance    |
      | Review responses   | Professional review replies    |
      | Bulk announcements | Update all customers at once   |

  @marketplace @quality-assurance @content-standards @not-implemented
  Scenario: Maintain marketplace quality and content standards
    Given I am a marketplace seller
    When I upload content to the marketplace
    Then all content should meet standards:
      | Quality Standard   | Requirement                    |
      | Educational value  | Supports learning objectives   |
      | Clinical accuracy  | Evidence-based practices       |
      | Age appropriateness| Suitable for stated age range  |
      | Technical quality  | Clear images, good formatting  |
      | Legal compliance   | Copyright, fair use            |
    And content should be reviewed by:
      | Review Stage       | Process                        |
      | Automated checks   | Technical and format validation|
      | Peer review        | Fellow therapist evaluation    |
      | Clinical review    | Expert professional assessment |
      | Final approval     | Platform quality assurance    |
    When content doesn't meet standards
    Then I should receive:
      | Feedback Type      | Information                    |
      | Specific issues    | Detailed improvement guidance  |
      | Resubmission option| Opportunity to revise and retry|
      | Educational resources| Help improve content quality |
      | Appeal process     | Dispute resolution mechanism  |

  @marketplace @international-expansion @global-reach @not-implemented
  Scenario: Expand marketplace to international customers
    Given I want to sell to international customers
    When I enable international sales
    Then I should be able to:
      | Feature            | Capability                     |
      | Multi-currency     | Price in local currencies      |
      | Tax compliance     | Handle international tax       |
      | Shipping options   | Physical product delivery      |
      | Language support   | Translate product descriptions |
    And customers should see:
      | Display Element    | Localization                   |
      | Prices             | Local currency conversion      |
      | Shipping costs     | Accurate delivery estimates    |
      | Tax information    | Local tax requirements         |
      | Payment methods    | Regional payment options       |
    When international sales occur
    Then I should receive:
      | Information        | Details                        |
      | Exchange rates     | Currency conversion data       |
      | Tax obligations    | Compliance requirements        |
      | Shipping tracking  | International delivery status  |
      | Customer support   | Multi-language assistance      |

  @marketplace @intellectual-property @copyright-protection @not-implemented
  Scenario: Protect intellectual property and handle copyright issues
    Given I create original therapy resources
    When I list products in the marketplace
    Then I should have protection through:
      | Protection Type    | Implementation                 |
      | Copyright notices  | Automatic application          |
      | Digital watermarks | Embedded ownership info        |
      | License agreements | Clear usage terms              |
      | DMCA compliance    | Takedown request handling      |
    And if copyright infringement occurs
    Then I should be able to:
      | Action             | Process                        |
      | Report infringement| Submit DMCA takedown notice    |
      | Provide evidence   | Upload proof of ownership      |
      | Request removal    | Fast-track content removal     |
      | Seek damages       | Legal support resources        |
    When I use licensed content
    Then I should:
      | Requirement        | Implementation                 |
      | Obtain permissions | Proper licensing agreements    |
      | Provide attribution| Clear credit to original authors|
      | Respect limitations| Follow license terms exactly   |
      | Maintain records   | Keep licensing documentation   |

  @marketplace @performance-optimization @success-metrics @not-implemented
  Scenario: Optimize marketplace performance and track success
    Given I have products in the marketplace
    When I access performance analytics
    Then I should see metrics including:
      | Metric Category    | Specific Measurements          |
      | Sales performance  | Revenue, units sold, growth    |
      | Customer behavior  | Views, clicks, conversion rates|
      | Product performance| Best sellers, slow movers      |
      | Market position    | Rankings, competitive analysis |
    And I should receive optimization suggestions:
      | Optimization Area  | Recommendations                |
      | Pricing strategy   | Competitive pricing analysis   |
      | Product descriptions| SEO and conversion improvements|
      | Image quality      | Visual enhancement suggestions |
      | Keywords          | Search optimization tips       |
    When I implement optimizations
    Then I should see:
      | Improvement Area   | Expected Results               |
      | Search ranking     | Higher visibility              |
      | Click-through rate | More product views             |
      | Conversion rate    | Higher purchase percentages    |
      | Customer satisfaction| Better reviews and ratings   |
    And success should be tracked over time with trend analysis