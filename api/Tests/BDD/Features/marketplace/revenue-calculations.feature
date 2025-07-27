Feature: Advanced Marketplace Revenue Calculations
  As a marketplace administrator and seller
  I want comprehensive revenue calculation and split management
  So that I can ensure accurate financial transactions and transparent earnings

  Background:
    Given marketplace revenue calculation system is configured
    And revenue split rates are established
    And tax calculation services are integrated
    And payment processing is active
    And financial reporting is available

  # Core Revenue Calculations
  @marketplace @revenue @calculations @split-management @critical @not-implemented
  Scenario: Calculate accurate 70/30 revenue splits with complex scenarios
    Given marketplace operates on 70% seller / 30% platform split
    And various transaction types require different handling
    When processing revenue calculations:
      | Transaction Type | Sale Amount | Seller Share | Platform Share | Processing Fee | Tax Amount | Net to Seller | Platform Net |
      | Standard sale | $24.99 | $17.49 | $7.50 | $0.75 | $2.00 | $14.74 | $4.75 |
      | Bundle sale | $89.95 | $62.97 | $26.98 | $2.70 | $7.20 | $53.07 | $17.08 |
      | Subscription sale | $19.99/month | $13.99 | $6.00 | $0.60 | $1.60 | $11.79 | $3.80 |
      | Discounted sale | $15.00 (40% off) | $10.50 | $4.50 | $0.45 | $1.20 | $8.85 | $2.85 |
      | International sale | €22.50 | €15.75 | €6.75 | €0.68 | €3.60 | €11.47 | €3.47 |
      | Refunded sale | -$24.99 | -$17.49 | -$7.50 | -$0.75 | -$2.00 | -$14.74 | -$4.75 |
    Then revenue splits should be calculated accurately
    And processing fees should be factored correctly
    And tax amounts should be computed properly
    And net amounts should balance to gross sales

  @marketplace @revenue @calculations @tax-compliance @critical @not-implemented
  Scenario: Handle multi-jurisdiction tax calculations
    Given sellers operate in different tax jurisdictions
    And tax rates vary by location and product type
    When calculating taxes for marketplace transactions:
      | Seller Location | Buyer Location | Product Type | Base Amount | Tax Rate | Tax Type | Tax Amount | Net Amount |
      | California, US | California, US | Digital goods | $25.00 | 10.25% | Sales tax | $2.56 | $22.44 |
      | New York, US | Florida, US | Physical goods | $35.00 | 8.25% | Sales tax | $2.89 | $32.11 |
      | Ontario, CA | British Columbia, CA | Educational materials | $45.00 | 12.00% | HST | $5.40 | $39.60 |
      | London, UK | Berlin, DE | Digital resources | €30.00 | 19.00% | VAT | €5.70 | €24.30 |
      | Texas, US | International | Therapy tools | $50.00 | 0.00% | Export exempt | $0.00 | $50.00 |
      | France | France | Subscription | €15.00 | 20.00% | VAT | €3.00 | €12.00 |
    Then tax calculations should be jurisdiction-specific
    And tax types should be correctly identified
    And exemptions should be properly applied
    And compliance should be maintained across regions

  @marketplace @revenue @calculations @commission-tiers @high @not-implemented
  Scenario: Implement tiered commission structures for high-volume sellers
    Given commission rates can vary based on seller performance
    And volume-based incentives encourage growth
    When applying tiered commission structures:
      | Seller Tier | Monthly Sales Volume | Commission Rate | Additional Benefits | Qualification Period | Review Frequency |
      | Bronze | $0 - $999 | 30% platform fee | Standard support | Immediate | Quarterly |
      | Silver | $1,000 - $4,999 | 25% platform fee | Priority support | 3 months | Quarterly |
      | Gold | $5,000 - $14,999 | 20% platform fee | Marketing support | 6 months | Bi-annually |
      | Platinum | $15,000 - $49,999 | 15% platform fee | Featured placement | 12 months | Annually |
      | Diamond | $50,000+ | 10% platform fee | Dedicated account manager | 18 months | Annually |
      | Partner | Invitation only | 5% platform fee | Co-marketing opportunities | By invitation | Ongoing |
    Then commission rates should adjust based on tier
    And benefits should be automatically applied
    And tier changes should be processed monthly
    And sellers should be notified of tier changes

  @marketplace @revenue @calculations @promotional-pricing @high @not-implemented
  Scenario: Calculate revenue for promotional campaigns and discounts
    Given promotional campaigns affect revenue calculations
    And various discount types require different handling
    When processing promotional revenue:
      | Promotion Type | Original Price | Discount Amount | Final Price | Platform Absorbs | Seller Absorbs | Revenue Split Basis |
      | Platform coupon | $30.00 | 20% ($6.00) | $24.00 | $6.00 | $0.00 | Original price |
      | Seller discount | $30.00 | 15% ($4.50) | $25.50 | $0.00 | $4.50 | Discounted price |
      | First-time buyer | $30.00 | $5.00 flat | $25.00 | $2.50 | $2.50 | Discounted price |
      | Bundle discount | $100.00 | 25% ($25.00) | $75.00 | $0.00 | $25.00 | Discounted price |
      | Seasonal sale | $30.00 | 30% ($9.00) | $21.00 | $4.50 | $4.50 | Discounted price |
      | Volume discount | $200.00 | 10% ($20.00) | $180.00 | $0.00 | $20.00 | Discounted price |
    Then discount absorption should be calculated correctly
    And revenue split basis should be determined properly
    And promotional impact should be tracked
    And seller earnings should reflect discount responsibility

  # Advanced Revenue Features
  @marketplace @revenue @calculations @recurring-revenue @high @not-implemented
  Scenario: Manage recurring revenue and subscription calculations
    Given subscription products generate recurring revenue
    And different subscription models require specific handling
    When managing subscription revenue:
      | Subscription Type | Billing Frequency | Base Price | Platform Fee | Processing Fee | Seller Net | Annual Value | Churn Impact |
      | Monthly therapy plans | Monthly | $19.99 | $6.00 | $0.60 | $13.39 | $160.68 | -$13.39/month |
      | Quarterly resource access | Quarterly | $49.99 | $15.00 | $1.50 | $33.49 | $133.96 | -$33.49/quarter |
      | Annual premium membership | Annually | $199.99 | $60.00 | $6.00 | $133.99 | $133.99 | -$133.99/year |
      | Weekly activity updates | Weekly | $4.99 | $1.50 | $0.15 | $3.34 | $173.68 | -$3.34/week |
      | Bi-annual certification | Bi-annually | $99.99 | $30.00 | $3.00 | $66.99 | $133.98 | -$66.99/6months |
      | Usage-based pricing | Variable | $0.50/use | $0.15 | $0.02 | $0.33 | Variable | Variable |
    Then recurring calculations should be accurate
    And billing cycles should be respected
    And churn impact should be calculated
    And lifetime value should be projected

  @marketplace @revenue @calculations @international-currency @medium @not-implemented
  Scenario: Handle multi-currency transactions and exchange rates
    Given marketplace operates globally with multiple currencies
    And exchange rates fluctuate daily
    When processing international transactions:
      | Base Currency | Transaction Currency | Amount | Exchange Rate | USD Equivalent | Rate Date | Fee Structure | Settlement Currency |
      | USD | EUR | €25.00 | 1.08 | $27.00 | 2025-01-15 | USD-based | USD |
      | USD | GBP | £20.00 | 1.25 | $25.00 | 2025-01-15 | GBP-based | GBP |
      | USD | CAD | CAD $30.00 | 0.74 | $22.20 | 2025-01-15 | USD-based | USD |
      | USD | AUD | AUD $35.00 | 0.68 | $23.80 | 2025-01-15 | AUD-based | AUD |
      | USD | JPY | ¥3,000 | 0.0067 | $20.10 | 2025-01-15 | USD-based | USD |
      | USD | INR | ₹2,000 | 0.012 | $24.00 | 2025-01-15 | USD-based | USD |
    Then exchange rates should be applied at transaction time
    And currency conversion should be transparent
    And fees should be calculated in appropriate currency
    And settlement should match seller preference

  @marketplace @revenue @calculations @refund-handling @medium @not-implemented
  Scenario: Process refunds and revenue adjustments
    Given refunds may be full or partial
    And refund policies vary by product type
    When processing refund transactions:
      | Original Sale | Refund Type | Refund Amount | Platform Refund | Seller Refund | Processing Reversal | Platform Cost | Timeline |
      | $30.00 | Full refund | $30.00 | $9.00 | $21.00 | $0.90 | $0.00 | Within 30 days |
      | $50.00 | Partial refund | $20.00 | $6.00 | $14.00 | $0.60 | $0.50 | Within 60 days |
      | $100.00 | Store credit | $100.00 | $0.00 | $0.00 | $0.00 | $3.00 | Credit issued |
      | $25.00 | Defective product | $25.00 | $25.00 | $0.00 | $0.75 | $0.00 | Seller fault |
      | $75.00 | Policy violation | $75.00 | $0.00 | $75.00 | $2.25 | $0.00 | Platform fault |
      | $45.00 | Disputed charge | $45.00 | $13.50 | $31.50 | $1.35 | $15.00 | Chargeback |
    Then refund amounts should be calculated proportionally
    And processing fees should be handled appropriately
    And platform costs should be tracked
    And refund timelines should be respected

  @marketplace @revenue @calculations @financial-reporting @critical @not-implemented
  Scenario: Generate comprehensive financial reports for stakeholders
    Given financial transparency is required for all stakeholders
    And reports must be accurate and timely
    When generating financial reports:
      | Report Type | Audience | Frequency | Data Included | Accuracy Requirement | Delivery Method |
      | Seller earnings | Individual sellers | Monthly | Personal sales, fees, taxes, net | 99.9% accuracy | Email + dashboard |
      | Platform revenue | Management | Monthly | Total sales, commission, costs | 99.9% accuracy | Management dashboard |
      | Tax reporting | Tax authorities | Quarterly | Tax collected, jurisdictions | 100% accuracy | Regulatory filing |
      | Investor reports | Investors | Quarterly | GMV, revenue, growth metrics | 99.9% accuracy | Investor portal |
      | Compliance reports | Auditors | Annually | Full transaction history | 100% accuracy | Secure download |
      | Marketplace metrics | Public | Quarterly | Aggregate statistics | 99% accuracy | Public website |
    Then reports should be generated automatically
    And accuracy requirements should be met
    And delivery should be timely and secure
    And compliance should be maintained

  # Financial Analytics and Insights
  @marketplace @revenue @calculations @analytics @high @not-implemented
  Scenario: Provide revenue analytics and business intelligence
    Given analytics drive business decisions
    And insights help optimize marketplace performance
    When analyzing revenue data:
      | Analytics Type | Metrics Tracked | Analysis Period | Insights Generated | Action Recommendations | Business Impact |
      | Revenue trends | GMV, commission, growth | Monthly/quarterly | Seasonal patterns | Marketing timing | Revenue optimization |
      | Seller performance | Sales volume, conversion rates | Monthly | Top performers | Seller incentives | Seller retention |
      | Product analysis | Category performance, pricing | Weekly | Popular categories | Inventory focus | Category growth |
      | Geographic analysis | Regional sales, currency | Monthly | Market opportunities | Expansion strategy | Market penetration |
      | Customer analysis | Purchase patterns, lifetime value | Quarterly | Customer segments | Targeting strategy | Customer retention |
      | Competitive analysis | Market share, pricing trends | Quarterly | Competitive position | Pricing strategy | Market advantage |
    Then analytics should provide actionable insights
    And recommendations should be data-driven
    And business impact should be measurable
    And decision-making should be informed

  @marketplace @revenue @calculations @forecasting @medium @not-implemented
  Scenario: Implement revenue forecasting and projection models
    Given revenue forecasting enables business planning
    And projections help with resource allocation
    When creating revenue forecasts:
      | Forecast Type | Time Horizon | Input Variables | Model Type | Accuracy Target | Confidence Interval |
      | Monthly GMV | 3 months | Historical sales, seasonality | Time series | 85% accuracy | 90% confidence |
      | Quarterly revenue | 12 months | Growth trends, market factors | Regression | 80% accuracy | 85% confidence |
      | Annual projections | 3 years | Strategic initiatives, market | Scenario-based | 70% accuracy | 75% confidence |
      | Seller growth | 6 months | Individual performance | Machine learning | 90% accuracy | 95% confidence |
      | Category trends | 12 months | Category adoption rates | Trend analysis | 75% accuracy | 80% confidence |
      | Market expansion | 24 months | Geographic opportunities | Market modeling | 65% accuracy | 70% confidence |
    Then forecasts should be statistically sound
    And accuracy should be monitored and improved
    And confidence intervals should be provided
    And projections should guide business planning

  @marketplace @revenue @calculations @audit-trail @critical @not-implemented
  Scenario: Maintain comprehensive audit trail for financial transactions
    Given financial transactions require complete audit trails
    And regulatory compliance demands detailed records
    When maintaining audit trails:
      | Transaction Element | Audit Information | Retention Period | Access Controls | Compliance Requirements | Verification Methods |
      | Revenue calculations | Formulas, rates, amounts | 7 years | Finance team only | SOX compliance | Independent verification |
      | Tax calculations | Rates, jurisdictions, amounts | 7 years | Tax team + auditors | Tax authority requirements | Tax software validation |
      | Commission changes | Old rate, new rate, effective date | Indefinite | Admin approval | Internal controls | Approval workflow |
      | Refund processing | Reason, authorization, amounts | 7 years | Customer service + finance | Consumer protection | Manager approval |
      | Currency conversions | Rates, sources, timestamps | 5 years | Finance team | Foreign exchange regulations | Rate source verification |
      | Promotional impacts | Campaign details, financial impact | 3 years | Marketing + finance | Marketing compliance | Campaign tracking |
    Then audit trails should be complete and immutable
    And access should be controlled and logged
    And retention periods should be respected
    And compliance should be maintained

  # Integration and Automation
  @marketplace @revenue @calculations @payment-integration @critical @not-implemented
  Scenario: Integrate with payment processors for automated revenue handling
    Given payment processing affects revenue calculations
    And automation reduces errors and delays
    When integrating payment processing:
      | Payment Processor | Integration Type | Fee Structure | Settlement Timeline | Currency Support | Automation Level |
      | Stripe Connect | API integration | 2.9% + 30¢ | 2-7 business days | 135+ currencies | Fully automated |
      | PayPal Marketplace | Webhook integration | 2.9% + fixed fee | 1-3 business days | 25 currencies | Semi-automated |
      | Square | Direct API | 2.6% + 10¢ | Next business day | USD, CAD, GBP, AUD | Fully automated |
      | Adyen | Platform integration | 2.8% + interchange | 1-2 business days | 150+ currencies | Fully automated |
      | Braintree | Marketplace API | 2.9% + 30¢ | 1-3 business days | 45+ currencies | Fully automated |
      | Bank transfers | ACH/Wire integration | Fixed fees | 1-5 business days | Local currencies | Manual verification |
    Then payment processing should be seamless
    And fees should be calculated automatically
    And settlements should be tracked
    And automation should minimize manual work

  @marketplace @revenue @calculations @accounting-integration @high @not-implemented
  Scenario: Integrate with accounting systems for financial management
    Given accounting integration ensures accurate financial records
    And automated bookkeeping reduces errors
    When integrating with accounting systems:
      | Accounting System | Integration Method | Data Synchronization | Account Mapping | Reconciliation | Reporting Integration |
      | QuickBooks Online | API integration | Real-time | Automated | Daily | Monthly P&L |
      | Xero | OAuth connection | Hourly batches | Manual setup | Weekly | Financial statements |
      | NetSuite | Cloud integration | Real-time | Automated | Daily | Management reports |
      | FreshBooks | API sync | Daily batches | Semi-automated | Weekly | Invoice tracking |
      | Sage Intacct | Enterprise API | Real-time | Automated | Daily | Advanced analytics |
      | Custom ERP | API development | Configurable | Custom mapping | Configurable | Custom reports |
    Then integration should maintain data integrity
    And synchronization should be reliable
    And reconciliation should be automated
    And reporting should be consolidated

  # Error Handling and Quality Assurance
  @marketplace @revenue @calculations @error @calculation-errors @medium @not-implemented
  Scenario: Handle revenue calculation errors and discrepancies
    Given calculation errors can cause financial losses
    When calculation errors occur:
      | Error Type | Detection Method | Resolution Process | Timeline | Impact Assessment | Prevention Measures |
      | Split rate error | Automated validation | Recalculate and adjust | <24 hours | Financial impact analysis | Rate validation rules |
      | Tax calculation error | Tax service verification | Tax recalculation | <48 hours | Compliance risk assessment | Tax service redundancy |
      | Currency conversion error | Rate source validation | Exchange rate correction | <12 hours | Exchange impact analysis | Multiple rate sources |
      | Processing fee error | Fee structure audit | Fee recalculation | <24 hours | Fee impact assessment | Fee validation logic |
      | Rounding error | Precision validation | Precision adjustment | <6 hours | Accuracy impact analysis | Standardized rounding |
      | System calculation error | Algorithm verification | Code correction | <72 hours | System impact analysis | Algorithm testing |
    Then errors should be detected quickly
    And resolution should be prompt and accurate
    And financial adjustments should be made
    And prevention measures should be implemented

  @marketplace @revenue @calculations @sustainability @high @not-implemented
  Scenario: Ensure sustainable revenue calculation and management systems
    Given revenue systems require long-term sustainability
    When planning sustainability:
      | Sustainability Factor | Current Challenge | Sustainability Strategy | Resource Requirements | Success Indicators | Long-term Viability |
      | Calculation accuracy | Complex calculations | Automated validation systems | Validation infrastructure | 99.9% accuracy | Accuracy sustainability |
      | System scalability | Growing transaction volume | Scalable architecture | Computing resources | Linear scaling | Performance sustainability |
      | Regulatory compliance | Changing regulations | Adaptive compliance systems | Legal resources | Full compliance | Compliance sustainability |
      | Cost management | Processing costs | Cost optimization | Efficiency resources | Controlled costs | Financial sustainability |
      | Integration maintenance | System dependencies | Robust integration architecture | Technical resources | Reliable integrations | Technical sustainability |
      | Revenue optimization | Market competition | Competitive pricing strategies | Analytical resources | Market competitiveness | Business sustainability |
    Then sustainability should be systematically planned
    And challenges should be proactively addressed
    And resources should be adequate
    And long-term success should be ensured