# UPTRMS API Endpoint Requirements

## Overview
This document comprehensively lists ALL API endpoints required to implement the Unified Pediatric Therapy Resource Management System (UPTRMS) as specified in CLAUDE.md. 

Total Functional Requirements: 42
Total Endpoints Required: ~300+

## Status Legend
- ✅ Implemented with tests
- 🚧 Partially implemented
- ❌ Not implemented

---

## 1. Authentication & User Management (FR-001)
**Current Status**: 🚧 Partially implemented (only basic auth exists)

### Authentication Endpoints
- ✅ POST /api/auth/register - Register new user
- ✅ POST /api/auth/login - User login
- ✅ GET /api/auth/verify-email/{token} - Verify email
- ✅ POST /api/auth/resend-verification - Resend verification email
- ❌ POST /api/auth/logout - Logout user
- ❌ POST /api/auth/refresh-token - Refresh JWT token
- ❌ POST /api/auth/forgot-password - Request password reset
- ❌ POST /api/auth/reset-password - Reset password with token
- ❌ POST /api/auth/change-password - Change password (authenticated)
- ❌ POST /api/auth/mfa/setup - Setup MFA
- ❌ POST /api/auth/mfa/verify - Verify MFA code
- ❌ POST /api/auth/mfa/disable - Disable MFA
- ❌ GET /api/auth/sso/providers - List SSO providers
- ❌ GET /api/auth/sso/{provider}/redirect - SSO redirect
- ❌ POST /api/auth/sso/{provider}/callback - SSO callback

### User Management Endpoints
- ❌ GET /api/users/profile - Get current user profile
- ❌ PUT /api/users/profile - Update user profile
- ❌ DELETE /api/users/profile - Delete user account
- ❌ GET /api/users/{id} - Get user by ID (admin)
- ❌ GET /api/users - List users (admin)
- ❌ PUT /api/users/{id}/status - Update user status (admin)
- ❌ GET /api/users/licenses - Get user licenses
- ❌ POST /api/users/licenses/verify - Verify new license
- ❌ PUT /api/users/preferences - Update user preferences
- ❌ GET /api/users/notifications - Get notification settings
- ❌ PUT /api/users/notifications - Update notification settings

### Subscription Management
- ❌ GET /api/subscriptions/plans - List subscription plans
- ❌ GET /api/subscriptions/current - Get current subscription
- ❌ POST /api/subscriptions/subscribe - Subscribe to plan
- ❌ PUT /api/subscriptions/upgrade - Upgrade subscription
- ❌ PUT /api/subscriptions/downgrade - Downgrade subscription
- ❌ POST /api/subscriptions/cancel - Cancel subscription
- ❌ GET /api/subscriptions/invoices - Get invoices
- ❌ GET /api/subscriptions/payment-methods - List payment methods
- ❌ POST /api/subscriptions/payment-methods - Add payment method
- ❌ DELETE /api/subscriptions/payment-methods/{id} - Remove payment method

### Organization Management
- ❌ POST /api/organizations - Create organization
- ❌ GET /api/organizations/{id} - Get organization details
- ❌ PUT /api/organizations/{id} - Update organization
- ❌ GET /api/organizations/{id}/members - List org members
- ❌ POST /api/organizations/{id}/members/invite - Invite member
- ❌ DELETE /api/organizations/{id}/members/{userId} - Remove member
- ❌ PUT /api/organizations/{id}/members/{userId}/role - Update member role
- ❌ GET /api/organizations/{id}/billing - Get billing info
- ❌ PUT /api/organizations/{id}/billing - Update billing info

---

## 2. Resource Library (FR-002)
**Current Status**: ❌ Not implemented

### Resource Search & Discovery
- ❌ GET /api/resources/search - Search resources with filters
- ❌ GET /api/resources/featured - Get featured resources
- ❌ GET /api/resources/new - Get newest resources
- ❌ GET /api/resources/popular - Get popular resources
- ❌ GET /api/resources/recommended - Get AI recommendations
- ❌ GET /api/resources/categories - List all categories
- ❌ GET /api/resources/categories/{id}/resources - Get resources by category
- ❌ GET /api/resources/skills - List all skill areas
- ❌ GET /api/resources/grades - List all grade levels
- ❌ GET /api/resources/therapy-types - List therapy types

### Resource Management
- ❌ GET /api/resources/{id} - Get resource details
- ❌ GET /api/resources/{id}/preview - Get resource preview
- ❌ GET /api/resources/{id}/download - Download resource
- ❌ POST /api/resources/{id}/favorite - Add to favorites
- ❌ DELETE /api/resources/{id}/favorite - Remove from favorites
- ❌ GET /api/resources/favorites - Get user favorites
- ❌ POST /api/resources/folders - Create folder
- ❌ GET /api/resources/folders - List user folders
- ❌ PUT /api/resources/folders/{id} - Update folder
- ❌ DELETE /api/resources/folders/{id} - Delete folder
- ❌ POST /api/resources/folders/{id}/resources - Add resource to folder
- ❌ DELETE /api/resources/folders/{id}/resources/{resourceId} - Remove from folder
- ❌ POST /api/resources/{id}/rate - Rate resource
- ❌ GET /api/resources/{id}/ratings - Get resource ratings
- ❌ POST /api/resources/{id}/report - Report inappropriate content

### Bulk Operations
- ❌ POST /api/resources/bulk/download - Bulk download resources
- ❌ POST /api/resources/bulk/favorite - Bulk add to favorites
- ❌ POST /api/resources/bulk/folder - Bulk add to folder
- ❌ DELETE /api/resources/bulk/favorite - Bulk remove from favorites

---

## 3. Therapy Planning (FR-003)
**Current Status**: ❌ Not implemented

### Session Planning
- ❌ POST /api/therapy-plans - Create therapy plan
- ❌ GET /api/therapy-plans - List therapy plans
- ❌ GET /api/therapy-plans/{id} - Get therapy plan details
- ❌ PUT /api/therapy-plans/{id} - Update therapy plan
- ❌ DELETE /api/therapy-plans/{id} - Delete therapy plan
- ❌ POST /api/therapy-plans/{id}/duplicate - Duplicate plan
- ❌ POST /api/therapy-plans/{id}/share - Share plan
- ❌ GET /api/therapy-plans/shared - Get shared plans
- ❌ POST /api/therapy-plans/generate - AI generate plan
- ❌ POST /api/therapy-plans/{id}/sessions - Add session to plan
- ❌ PUT /api/therapy-plans/{id}/sessions/{sessionId} - Update session
- ❌ DELETE /api/therapy-plans/{id}/sessions/{sessionId} - Remove session
- ❌ POST /api/therapy-plans/{id}/export - Export plan (PDF/calendar)

### Group Planning
- ❌ POST /api/group-plans - Create group plan
- ❌ GET /api/group-plans - List group plans
- ❌ PUT /api/group-plans/{id} - Update group plan
- ❌ POST /api/group-plans/{id}/students - Add students to group
- ❌ DELETE /api/group-plans/{id}/students/{studentId} - Remove from group

---

## 4. Data Collection (FR-004)
**Current Status**: ❌ Not implemented

### Session Data
- ❌ POST /api/sessions - Create session record
- ❌ GET /api/sessions - List sessions
- ❌ GET /api/sessions/{id} - Get session details
- ❌ PUT /api/sessions/{id} - Update session
- ❌ DELETE /api/sessions/{id} - Delete session
- ❌ POST /api/sessions/{id}/data - Add data point
- ❌ PUT /api/sessions/{id}/data/{dataId} - Update data point
- ❌ DELETE /api/sessions/{id}/data/{dataId} - Delete data point
- ❌ GET /api/sessions/{id}/data - Get session data
- ❌ POST /api/sessions/{id}/notes - Add session note
- ❌ PUT /api/sessions/{id}/notes/{noteId} - Update note
- ❌ POST /api/sessions/{id}/complete - Mark session complete

### Progress Tracking
- ❌ GET /api/students/{id}/progress - Get student progress
- ❌ GET /api/students/{id}/progress/goals/{goalId} - Get goal progress
- ❌ POST /api/students/{id}/progress/entries - Add progress entry
- ❌ GET /api/students/{id}/progress/reports - Generate progress report
- ❌ GET /api/students/{id}/progress/graphs - Get progress graphs

---

## 5. Content Management (FR-005)
**Current Status**: ❌ Not implemented

### Content Upload & Review
- ❌ POST /api/content/upload - Upload new content
- ❌ GET /api/content/pending - Get pending reviews (admin)
- ❌ GET /api/content/{id}/review - Get content for review
- ❌ POST /api/content/{id}/approve - Approve content
- ❌ POST /api/content/{id}/reject - Reject content
- ❌ PUT /api/content/{id}/metadata - Update content metadata
- ❌ POST /api/content/{id}/versions - Create new version
- ❌ GET /api/content/{id}/versions - Get version history
- ❌ POST /api/content/{id}/retire - Retire content

---

## 6. AI Content Generation (FR-006)
**Current Status**: ❌ Not implemented

### AI Generation
- ❌ POST /api/ai/generate/worksheet - Generate worksheet
- ❌ POST /api/ai/generate/activity - Generate activity
- ❌ POST /api/ai/generate/visual-schedule - Generate visual schedule
- ❌ POST /api/ai/generate/social-story - Generate social story
- ❌ POST /api/ai/generate/communication-board - Generate comm board
- ❌ GET /api/ai/generation/{id}/status - Check generation status
- ❌ GET /api/ai/generation/{id}/result - Get generation result
- ❌ POST /api/ai/generation/{id}/approve - Approve AI content
- ❌ POST /api/ai/generation/{id}/reject - Reject AI content
- ❌ GET /api/ai/credits - Get generation credits
- ❌ POST /api/ai/credits/purchase - Purchase credits

---

## 7. AI Quality Assurance (FR-007)
**Current Status**: ❌ Not implemented

### AI Review
- ❌ GET /api/ai/review/queue - Get AI content review queue
- ❌ POST /api/ai/review/{id}/validate - Validate AI content
- ❌ POST /api/ai/review/{id}/flag - Flag content issue
- ❌ GET /api/ai/review/statistics - Get AI quality statistics

---

## 8. Marketplace (FR-008)
**Current Status**: ❌ Not implemented

### Seller Management
- ❌ POST /api/marketplace/seller/apply - Apply to be seller
- ❌ GET /api/marketplace/seller/application - Get application status
- ❌ GET /api/marketplace/seller/dashboard - Get seller dashboard
- ❌ GET /api/marketplace/seller/analytics - Get seller analytics
- ❌ GET /api/marketplace/seller/earnings - Get earnings
- ❌ POST /api/marketplace/seller/payout - Request payout
- ❌ GET /api/marketplace/seller/payouts - Get payout history
- ❌ PUT /api/marketplace/seller/profile - Update seller profile
- ❌ GET /api/marketplace/seller/followers - Get followers
- ❌ GET /api/marketplace/seller/reviews - Get seller reviews

### Product Management
- ❌ POST /api/marketplace/products - Create product listing
- ❌ GET /api/marketplace/products - List products
- ❌ GET /api/marketplace/products/{id} - Get product details
- ❌ PUT /api/marketplace/products/{id} - Update product
- ❌ DELETE /api/marketplace/products/{id} - Delete product
- ❌ POST /api/marketplace/products/{id}/publish - Publish product
- ❌ POST /api/marketplace/products/{id}/unpublish - Unpublish product
- ❌ POST /api/marketplace/products/{id}/images - Upload product images
- ❌ DELETE /api/marketplace/products/{id}/images/{imageId} - Delete image
- ❌ POST /api/marketplace/products/{id}/preview - Create preview
- ❌ PUT /api/marketplace/products/{id}/pricing - Update pricing
- ❌ POST /api/marketplace/products/{id}/discount - Create discount
- ❌ GET /api/marketplace/products/{id}/sales - Get sales data

### Shopping
- ❌ GET /api/marketplace/search - Search marketplace
- ❌ GET /api/marketplace/categories - List marketplace categories
- ❌ GET /api/marketplace/featured - Get featured products
- ❌ GET /api/marketplace/bestsellers - Get bestsellers
- ❌ POST /api/marketplace/cart - Add to cart
- ❌ GET /api/marketplace/cart - Get cart
- ❌ PUT /api/marketplace/cart/{itemId} - Update cart item
- ❌ DELETE /api/marketplace/cart/{itemId} - Remove from cart
- ❌ POST /api/marketplace/checkout - Checkout
- ❌ GET /api/marketplace/orders - Get order history
- ❌ GET /api/marketplace/orders/{id} - Get order details
- ❌ GET /api/marketplace/downloads - Get purchased downloads
- ❌ POST /api/marketplace/products/{id}/review - Review product
- ❌ POST /api/marketplace/products/{id}/question - Ask question
- ❌ GET /api/marketplace/products/{id}/questions - Get Q&A

### Bundles & Promotions
- ❌ POST /api/marketplace/bundles - Create bundle
- ❌ GET /api/marketplace/bundles/{id} - Get bundle details
- ❌ PUT /api/marketplace/bundles/{id} - Update bundle
- ❌ POST /api/marketplace/promotions - Create promotion
- ❌ GET /api/marketplace/promotions/active - Get active promotions

---

## 9. Interactive Digital Activities (FR-009)
**Current Status**: ❌ Not implemented

### Activity Management
- ❌ GET /api/activities/digital - List digital activities
- ❌ GET /api/activities/digital/{id} - Get activity details
- ❌ POST /api/activities/digital/{id}/start - Start activity
- ❌ PUT /api/activities/digital/{id}/response - Submit response
- ❌ POST /api/activities/digital/{id}/complete - Complete activity
- ❌ GET /api/activities/digital/{id}/results - Get results
- ❌ POST /api/activities/decks - Create custom deck
- ❌ GET /api/activities/decks - List custom decks
- ❌ PUT /api/activities/decks/{id} - Update deck
- ❌ DELETE /api/activities/decks/{id} - Delete deck
- ❌ POST /api/activities/decks/{id}/cards - Add card to deck
- ❌ PUT /api/activities/decks/{id}/cards/{cardId} - Update card
- ❌ DELETE /api/activities/decks/{id}/cards/{cardId} - Remove card

### Activity Data
- ❌ GET /api/activities/data/student/{studentId} - Get student activity data
- ❌ GET /api/activities/data/activity/{activityId} - Get activity statistics
- ❌ POST /api/activities/audio/record - Save audio recording
- ❌ GET /api/activities/audio/{id} - Get audio recording

---

## 10. EHR Integration (FR-010)
**Current Status**: ❌ Not implemented

### EHR Connections
- ❌ GET /api/integrations/ehr/providers - List EHR providers
- ❌ POST /api/integrations/ehr/connect - Connect EHR
- ❌ GET /api/integrations/ehr/status - Get connection status
- ❌ DELETE /api/integrations/ehr/disconnect - Disconnect EHR
- ❌ POST /api/integrations/ehr/sync - Manual sync
- ❌ GET /api/integrations/ehr/sync/status - Get sync status
- ❌ POST /api/integrations/ehr/sessions/{id}/export - Export session to EHR
- ❌ GET /api/integrations/ehr/patients - Import patients from EHR

---

## 11. Seller Tools (FR-011)
**Current Status**: ❌ Not implemented

### Storefront Management
- ❌ GET /api/storefront/{sellerId} - Get public storefront
- ❌ PUT /api/storefront/customize - Customize storefront
- ❌ POST /api/storefront/banner - Upload banner
- ❌ GET /api/storefront/analytics - Get storefront analytics
- ❌ POST /api/storefront/follow - Follow seller
- ❌ DELETE /api/storefront/follow - Unfollow seller
- ❌ GET /api/storefront/followers - Get follower list
- ❌ POST /api/storefront/announcements - Post announcement
- ❌ GET /api/storefront/sales-report - Generate sales report

---

## 12. Student Management (FR-012)
**Current Status**: ❌ Not implemented

### Student Records
- ❌ POST /api/students - Create student
- ❌ GET /api/students - List students
- ❌ GET /api/students/{id} - Get student details
- ❌ PUT /api/students/{id} - Update student
- ❌ DELETE /api/students/{id} - Delete student
- ❌ POST /api/students/import - Import from SIS
- ❌ GET /api/students/{id}/goals - Get IEP goals
- ❌ POST /api/students/{id}/goals - Add IEP goal
- ❌ PUT /api/students/{id}/goals/{goalId} - Update goal
- ❌ DELETE /api/students/{id}/goals/{goalId} - Delete goal
- ❌ GET /api/students/{id}/documents - Get student documents
- ❌ POST /api/students/{id}/documents - Upload document
- ❌ GET /api/students/{id}/schedule - Get therapy schedule
- ❌ POST /api/students/{id}/assign-resources - Assign resources
- ❌ GET /api/students/{id}/assigned-resources - Get assigned resources

### Parent Access
- ❌ POST /api/students/{id}/parent-access - Generate parent access
- ❌ GET /api/parent-portal/validate/{code} - Validate fast pin
- ❌ GET /api/parent-portal/student - Get student info (parent view)
- ❌ GET /api/parent-portal/resources - Get assigned resources
- ❌ GET /api/parent-portal/progress - Get progress summary
- ❌ POST /api/parent-portal/message - Send message to therapist

### Groups
- ❌ POST /api/student-groups - Create group
- ❌ GET /api/student-groups - List groups
- ❌ PUT /api/student-groups/{id} - Update group
- ❌ DELETE /api/student-groups/{id} - Delete group
- ❌ POST /api/student-groups/{id}/students - Add students
- ❌ DELETE /api/student-groups/{id}/students/{studentId} - Remove student

---

## 13. Physical/Digital Hybrid (FR-013)
**Current Status**: ❌ Not implemented

### QR Code Integration
- ❌ POST /api/qr/generate - Generate QR code
- ❌ GET /api/qr/scan/{code} - Process QR scan
- ❌ POST /api/qr/link - Link physical to digital
- ❌ GET /api/physical-products - List physical products
- ❌ GET /api/physical-products/{id}/digital - Get digital companion
- ❌ POST /api/print-on-demand/order - Create print order
- ❌ GET /api/print-on-demand/status/{orderId} - Get order status
- ❌ POST /api/ar/markers - Upload AR marker
- ❌ GET /api/ar/content/{markerId} - Get AR content

---

## 14. Communication Tools (FR-014)
**Current Status**: ❌ Not implemented

### Sharing & Messaging
- ❌ POST /api/share/quicklink - Generate quicklink
- ❌ GET /api/share/quicklink/{code} - Access via quicklink
- ❌ POST /api/messages/send - Send message
- ❌ GET /api/messages/templates - Get message templates
- ❌ POST /api/homework/assign - Assign homework
- ❌ GET /api/homework/assignments - List assignments
- ❌ PUT /api/homework/{id}/complete - Mark complete
- ❌ POST /api/reports/progress - Generate progress report
- ❌ GET /api/reports/templates - Get report templates
- ❌ POST /api/notifications/send - Send notification

---

## 15. Assessment & Screening (FR-015)
**Current Status**: ❌ Not implemented

### Assessment Tools
- ❌ GET /api/assessments/tools - List assessment tools
- ❌ GET /api/assessments/tools/{id} - Get tool details
- ❌ POST /api/assessments/start - Start assessment
- ❌ PUT /api/assessments/{id}/responses - Submit responses
- ❌ POST /api/assessments/{id}/complete - Complete assessment
- ❌ GET /api/assessments/{id}/results - Get results
- ❌ GET /api/assessments/{id}/report - Generate report
- ❌ POST /api/assessments/{id}/norm-compare - Compare to norms
- ❌ GET /api/assessments/history/{studentId} - Get assessment history

---

## 16. Adult Therapy Resources (FR-016)
**Current Status**: ❌ Not implemented

### Adult-Specific Resources
- ❌ GET /api/resources/adult - List adult resources
- ❌ GET /api/resources/adult/cognitive - Cognitive rehab resources
- ❌ GET /api/resources/adult/aphasia - Aphasia resources
- ❌ GET /api/resources/adult/dysphagia - Dysphagia protocols
- ❌ GET /api/resources/adult/return-to-work - Work assessments
- ❌ GET /api/resources/adult/caregiver - Caregiver materials

---

## 17. Movement & Sensory (FR-017)
**Current Status**: ❌ Not implemented

### Movement Resources
- ❌ GET /api/resources/movement/videos - Exercise videos
- ❌ GET /api/resources/movement/yoga - Yoga sequences
- ❌ GET /api/resources/movement/brain-breaks - Brain breaks
- ❌ POST /api/sensory-diet/create - Create sensory diet
- ❌ GET /api/sensory-diet/{id} - Get sensory diet
- ❌ PUT /api/sensory-diet/{id} - Update sensory diet
- ❌ GET /api/equipment/recommendations - Equipment recommendations
- ❌ POST /api/resources/movement/filter-by-space - Filter by space

---

## 18. Professional Development (FR-018)
**Current Status**: ❌ Not implemented

### Training & CEUs
- ❌ GET /api/professional/courses - List courses
- ❌ GET /api/professional/courses/{id} - Get course details
- ❌ POST /api/professional/courses/{id}/enroll - Enroll in course
- ❌ PUT /api/professional/courses/{id}/progress - Update progress
- ❌ POST /api/professional/courses/{id}/complete - Complete course
- ❌ GET /api/professional/certificates - Get certificates
- ❌ GET /api/professional/ceu-tracking - Track CEUs
- ❌ GET /api/professional/webinars - List webinars
- ❌ POST /api/professional/webinars/{id}/register - Register for webinar
- ❌ GET /api/professional/podcasts - List podcasts
- ❌ POST /api/professional/mentorship/request - Request mentorship
- ❌ GET /api/professional/mentorship/matches - Get mentor matches

---

## 19. Multilingual Support (FR-019)
**Current Status**: ❌ Not implemented

### Language Management
- ❌ GET /api/languages/supported - List supported languages
- ❌ PUT /api/users/language - Set user language
- ❌ GET /api/resources/translations/{resourceId} - Get translations
- ❌ POST /api/translations/report - Report translation issue
- ❌ GET /api/resources/bilingual - Get bilingual resources
- ❌ GET /api/resources/asl - Get ASL video resources
- ❌ GET /api/interface/translations/{lang} - Get UI translations

---

## 20. Seasonal & Holiday (FR-020)
**Current Status**: ❌ Not implemented

### Seasonal Content
- ❌ GET /api/resources/seasonal/current - Current seasonal content
- ❌ GET /api/resources/holidays/{holiday} - Holiday-specific
- ❌ GET /api/calendars/cultural - Cultural calendars
- ❌ PUT /api/users/holiday-preferences - Set holiday preferences

---

## 21. Free Resources (FR-021)
**Current Status**: ❌ Not implemented

### Free Tier Access
- ❌ GET /api/resources/free - List free resources
- ❌ GET /api/resources/free/weekly - Weekly free resources
- ❌ GET /api/resources/samples/{id} - Get resource samples
- ❌ POST /api/newsletter/subscribe - Subscribe to newsletter
- ❌ GET /api/resources/birthday-special - Birthday month specials

---

## 22. External Integrations (FR-022)
**Current Status**: ❌ Not implemented

### Third-Party Platforms
- ❌ POST /api/integrations/etsy/connect - Connect Etsy shop
- ❌ POST /api/integrations/etsy/sync - Sync inventory
- ❌ POST /api/integrations/youtube/embed - Embed YouTube
- ❌ POST /api/integrations/pinterest/board - Create Pinterest board
- ❌ POST /api/integrations/instagram/share - Share to Instagram
- ❌ POST /api/integrations/tiktok/post - Post to TikTok

---

## 23. Specialized Content (FR-023)
**Current Status**: ❌ Not implemented

### Specialized Therapy Content
- ❌ GET /api/resources/apraxia - Apraxia card sets
- ❌ GET /api/resources/minimal-pairs - Minimal pairs library
- ❌ GET /api/resources/vocalic-r - Vocalic R resources
- ❌ GET /api/resources/feeding - Feeding protocols
- ❌ GET /api/resources/literacy - Literacy units
- ❌ GET /api/resources/social-stories - Social stories
- ❌ POST /api/resources/visual-schedules/create - Create schedule
- ❌ GET /api/resources/articulation/{sound} - Sound-specific

---

## 24. Virtual Tools (FR-024)
**Current Status**: ❌ Not implemented

### Teletherapy Tools
- ❌ GET /api/virtual/backgrounds - Virtual backgrounds
- ❌ GET /api/virtual/tools/dice - Dice roller
- ❌ GET /api/virtual/tools/spinner - Spinner tool
- ❌ POST /api/virtual/tokens/award - Award token
- ❌ GET /api/virtual/manipulatives - Virtual manipulatives
- ❌ POST /api/virtual/annotation/save - Save annotations

---

## 25. Caseload Integration (FR-025)
**Current Status**: ❌ Not implemented

### Caseload Management
- ❌ GET /api/caseload/overview - Caseload overview
- ❌ GET /api/caseload/analytics - Caseload analytics
- ❌ POST /api/caseload/goals/suggest - AI goal suggestions
- ❌ GET /api/caseload/productivity - Productivity metrics
- ❌ POST /api/caseload/schedule/optimize - Optimize schedule

---

## 26. Creation Tools (FR-026)
**Current Status**: ❌ Not implemented

### Resource Creation
- ❌ GET /api/templates - List templates
- ❌ GET /api/templates/{id} - Get template
- ❌ POST /api/templates/customize - Customize template
- ❌ GET /api/images/library - Image library
- ❌ POST /api/resources/create - Create custom resource
- ❌ PUT /api/resources/custom/{id} - Update custom resource
- ❌ POST /api/resources/collaborate - Share for collaboration
- ❌ GET /api/resources/versions/{id} - Version history

---

## 27. Gamification (FR-027)
**Current Status**: ❌ Not implemented

### Student Motivation
- ❌ GET /api/gamification/student/{id}/points - Get points
- ❌ POST /api/gamification/points/award - Award points
- ❌ GET /api/gamification/badges - List available badges
- ❌ GET /api/gamification/student/{id}/badges - Get earned badges
- ❌ GET /api/gamification/leaderboard - Get leaderboard
- ❌ POST /api/gamification/rewards/redeem - Redeem rewards
- ❌ GET /api/gamification/store - Get reward store

---

## 28. Documentation Helpers (FR-028)
**Current Status**: ❌ Not implemented

### Documentation Automation
- ❌ POST /api/documentation/session-notes/generate - Generate notes
- ❌ GET /api/documentation/templates - Note templates
- ❌ GET /api/documentation/goal-bank - Goal bank
- ❌ POST /api/documentation/goals/suggest - Suggest goals
- ❌ POST /api/documentation/progress-notes/generate - Generate progress notes
- ❌ POST /api/documentation/reports/generate - Generate reports
- ❌ GET /api/documentation/icd-codes - ICD code lookup
- ❌ GET /api/documentation/cpt-codes - CPT code lookup
- ❌ POST /api/documentation/soap-notes - Create SOAP note

---

## 29. Research & Evidence (FR-029)
**Current Status**: ❌ Not implemented

### Evidence Base
- ❌ GET /api/research/papers - Research library
- ❌ GET /api/research/evidence-levels - Evidence ratings
- ❌ GET /api/research/citations/{resourceId} - Get citations
- ❌ GET /api/research/outcomes/{interventionId} - Outcome data
- ❌ POST /api/research/alerts/subscribe - Subscribe to updates

---

## 30. Community Features (FR-030)
**Current Status**: ❌ Not implemented

### Limited Community
- ❌ POST /api/community/reviews - Post review
- ❌ GET /api/community/reviews/{resourceId} - Get reviews
- ❌ POST /api/community/questions - Ask question
- ❌ POST /api/community/questions/{id}/answer - Answer question
- ❌ POST /api/community/feature-requests - Request feature
- ❌ POST /api/community/feature-requests/{id}/vote - Vote on feature
- ❌ POST /api/community/bugs - Report bug

---

## 31. Curriculum Planning (FR-031)
**Current Status**: ❌ Not implemented

### Long-term Planning
- ❌ POST /api/curriculum/plans - Create curriculum plan
- ❌ GET /api/curriculum/plans - List plans
- ❌ PUT /api/curriculum/plans/{id} - Update plan
- ❌ GET /api/curriculum/standards - Get standards
- ❌ POST /api/curriculum/align - Align to standards
- ❌ GET /api/curriculum/scope-sequence - Get scope & sequence
- ❌ POST /api/curriculum/calendar/generate - Generate calendar

---

## 32. Outcome Measurement (FR-032)
**Current Status**: ❌ Not implemented

### Standardized Outcomes
- ❌ GET /api/outcomes/measures - List outcome measures
- ❌ POST /api/outcomes/administer - Administer measure
- ❌ GET /api/outcomes/results/{id} - Get results
- ❌ POST /api/outcomes/compare - Compare to norms
- ❌ GET /api/outcomes/reports - Generate outcome reports
- ❌ POST /api/outcomes/insurance-report - Insurance report

---

## 33. PECS Implementation (FR-033)
**Current Status**: ❌ Not implemented

### PECS System
- ❌ POST /api/pecs/student/{id}/initialize - Initialize PECS
- ❌ GET /api/pecs/student/{id}/phase - Get current phase
- ❌ POST /api/pecs/student/{id}/phase/advance - Advance phase
- ❌ POST /api/pecs/reinforcer-sampling - Document preferences
- ❌ GET /api/pecs/materials/{phase} - Get phase materials
- ❌ POST /api/pecs/exchange/record - Record exchange
- ❌ GET /api/pecs/data/{studentId} - Get PECS data
- ❌ POST /api/pecs/discrimination/setup - Setup discrimination
- ❌ POST /api/pecs/sentence-strip/build - Build sentence
- ❌ GET /api/pecs/vocabulary/{studentId} - Get vocabulary

---

## 34. ABA Integration (FR-034)
**Current Status**: ❌ Not implemented

### ABA Tools
- ❌ POST /api/aba/abc-data - Record ABC data
- ❌ GET /api/aba/abc-data/{studentId} - Get ABC data
- ❌ POST /api/aba/abc-data/analyze - Analyze patterns
- ❌ POST /api/aba/token-economy/setup - Setup token economy
- ❌ POST /api/aba/tokens/award - Award token
- ❌ POST /api/aba/tokens/exchange - Exchange tokens
- ❌ GET /api/aba/reinforcement-schedule - Get schedules
- ❌ POST /api/aba/dtt/session - Start DTT session
- ❌ POST /api/aba/dtt/trial - Record trial
- ❌ GET /api/aba/dtt/results - Get DTT results
- ❌ POST /api/aba/task-analysis/create - Create task analysis
- ❌ POST /api/aba/behavior-plan/create - Create BIP
- ❌ POST /api/aba/visual-schedule/create - Create schedule

---

## 35. AAC Comprehensive (FR-035)
**Current Status**: ❌ Not implemented

### AAC Suite
- ❌ POST /api/aac/boards/create - Create comm board
- ❌ GET /api/aac/boards/templates - Board templates
- ❌ PUT /api/aac/boards/{id} - Update board
- ❌ GET /api/aac/vocabulary/core - Core vocabulary
- ❌ GET /api/aac/vocabulary/fringe - Fringe vocabulary
- ❌ POST /api/aac/switch-access/configure - Configure switch
- ❌ POST /api/aac/switch-access/calibrate - Calibrate timing
- ❌ POST /api/aac/eye-gaze/setup - Setup eye gaze
- ❌ GET /api/aac/partner-training - Training materials
- ❌ POST /api/aac/device-support/request - Request support

---

## 36. Clinical Education (FR-036)
**Current Status**: ❌ Not implemented

### Student Supervision
- ❌ POST /api/clinical-ed/students - Add student clinician
- ❌ GET /api/clinical-ed/students - List students
- ❌ POST /api/clinical-ed/competencies/assess - Assess competency
- ❌ GET /api/clinical-ed/competencies/{studentId} - Get competencies
- ❌ POST /api/clinical-ed/observations/schedule - Schedule observation
- ❌ POST /api/clinical-ed/observations/{id}/complete - Complete observation
- ❌ POST /api/clinical-ed/video-review - Submit for review
- ❌ POST /api/clinical-ed/video-review/{id}/annotate - Add annotations
- ❌ POST /api/clinical-ed/hours/log - Log supervision hours
- ❌ GET /api/clinical-ed/reports - Generate reports

---

## 37. Transition Planning (FR-037)
**Current Status**: ❌ Not implemented

### Life Skills & Transition
- ❌ GET /api/transition/assessments - List assessments
- ❌ POST /api/transition/assessments/administer - Administer
- ❌ GET /api/transition/skills-checklists - Get checklists
- ❌ POST /api/transition/goals/create - Create transition goals
- ❌ GET /api/transition/resources/vocational - Vocational resources
- ❌ GET /api/transition/resources/independent-living - Living skills
- ❌ GET /api/transition/resources/self-advocacy - Self-advocacy
- ❌ POST /api/transition/plan/create - Create transition plan

---

## 38. Specialized Protocols (FR-038)
**Current Status**: ❌ Not implemented

### Evidence-Based Protocols
- ❌ GET /api/protocols/prompt - PROMPT resources
- ❌ GET /api/protocols/dir-floortime - DIR/Floortime
- ❌ GET /api/protocols/hanen - Hanen program
- ❌ GET /api/protocols/social-thinking - Social Thinking
- ❌ GET /api/protocols/zones-regulation - Zones materials
- ❌ GET /api/protocols/alert-program - Alert Program
- ❌ GET /api/protocols/handwriting - Handwriting W/O Tears
- ❌ POST /api/protocols/fidelity-check - Check fidelity
- ❌ GET /api/protocols/certification/verify - Verify cert

---

## 39. Advocacy & Legal (FR-039)
**Current Status**: ❌ Not implemented

### Advocacy Resources
- ❌ GET /api/advocacy/iep-prep - IEP prep materials
- ❌ GET /api/advocacy/rights - Rights information
- ❌ GET /api/advocacy/templates - Letter templates
- ❌ GET /api/advocacy/process-guides - Due process guides
- ❌ GET /api/advocacy/grant-templates - Grant writing
- ❌ GET /api/advocacy/insurance-appeals - Appeal letters
- ❌ POST /api/advocacy/customize-template - Customize template

---

## 40. Sensory Rooms (FR-040)
**Current Status**: ❌ Not implemented

### Sensory Room Design
- ❌ GET /api/sensory-rooms/templates - Room templates
- ❌ POST /api/sensory-rooms/design - Create design
- ❌ GET /api/sensory-rooms/equipment - Equipment catalog
- ❌ POST /api/sensory-rooms/safety-check - Safety checklist
- ❌ GET /api/sensory-rooms/budget-calculator - Calculate budget
- ❌ GET /api/sensory-rooms/portable-kits - Portable options

---

## 41. Feeding Therapy (FR-041)
**Current Status**: ❌ Not implemented

### Feeding Resources
- ❌ GET /api/feeding/oral-motor - Oral motor exercises
- ❌ GET /api/feeding/protocols - Feeding protocols
- ❌ POST /api/feeding/food-chaining - Create food chain
- ❌ GET /api/feeding/texture-progression - Texture guides
- ❌ POST /api/feeding/mealtime-behavior - Track behaviors
- ❌ GET /api/feeding/parent-education - Parent resources
- ❌ GET /api/feeding/sos-approach - SOS feeding
- ❌ POST /api/feeding/progress/track - Track progress

---

## 42. Multi-Sensory Learning (FR-042)
**Current Status**: ❌ Not implemented

### Learning Styles
- ❌ POST /api/learning-styles/assess - Assess style
- ❌ GET /api/resources/tactile - Tactile materials
- ❌ GET /api/resources/auditory - Auditory activities
- ❌ GET /api/resources/visual - Visual supports
- ❌ GET /api/resources/kinesthetic - Movement cards
- ❌ GET /api/resources/proprioceptive - Proprioceptive
- ❌ GET /api/resources/vestibular - Vestibular input
- ❌ POST /api/learning-styles/match - Match resources

---

## Additional System Endpoints

### Health & Monitoring
- ❌ GET /api/health - Health check
- ❌ GET /api/health/ready - Readiness check
- ❌ GET /api/health/live - Liveness check
- ❌ GET /api/metrics - System metrics

### Admin
- ❌ GET /api/admin/dashboard - Admin dashboard
- ❌ GET /api/admin/users - User management
- ❌ GET /api/admin/content - Content management
- ❌ GET /api/admin/reports - System reports
- ❌ GET /api/admin/audit-logs - Audit logs
- ❌ POST /api/admin/broadcast - Send broadcast

### Analytics
- ❌ POST /api/analytics/event - Track event
- ❌ GET /api/analytics/usage - Usage statistics
- ❌ GET /api/analytics/revenue - Revenue analytics

---

## Real-time & WebSocket Endpoints

### WebSocket Connections
- ❌ WS /ws/connect - Establish WebSocket connection
- ❌ WS /ws/subscribe/session/{sessionId} - Subscribe to session updates
- ❌ WS /ws/subscribe/student/{studentId} - Subscribe to student updates
- ❌ WS /ws/subscribe/activity/{activityId} - Subscribe to activity updates
- ❌ WS /ws/subscribe/collaboration/{resourceId} - Collaborative editing

### Real-time Events
- ❌ WS /ws/events/session-update - Real-time session data
- ❌ WS /ws/events/progress-update - Real-time progress updates
- ❌ WS /ws/events/activity-response - Real-time activity responses
- ❌ WS /ws/events/collaboration-change - Collaborative edit events

### Streaming
- ❌ GET /api/streaming/video/{id} - Stream therapy videos
- ❌ GET /api/streaming/audio/{id} - Stream audio content
- ❌ POST /api/streaming/screen-share - Screen sharing for teletherapy

---

## Webhook Endpoints

### Outgoing Webhooks
- ❌ POST /api/webhooks/configure - Configure webhook
- ❌ GET /api/webhooks - List configured webhooks
- ❌ PUT /api/webhooks/{id} - Update webhook
- ❌ DELETE /api/webhooks/{id} - Delete webhook
- ❌ POST /api/webhooks/{id}/test - Test webhook

### Incoming Webhooks
- ❌ POST /api/webhooks/stripe - Stripe payment webhooks
- ❌ POST /api/webhooks/ehr/{provider} - EHR sync webhooks
- ❌ POST /api/webhooks/lms/{provider} - LMS update webhooks
- ❌ POST /api/webhooks/sso/{provider} - SSO event webhooks

---

## Batch & Async Operations

### Batch Processing
- ❌ POST /api/batch/students/import - Batch import students
- ❌ POST /api/batch/resources/generate - Batch generate resources
- ❌ POST /api/batch/reports/generate - Batch generate reports
- ❌ GET /api/batch/jobs/{jobId}/status - Check batch job status

### Async Jobs
- ❌ POST /api/jobs/export-data - Start data export job
- ❌ POST /api/jobs/backup - Start backup job
- ❌ GET /api/jobs/{jobId} - Get job status
- ❌ DELETE /api/jobs/{jobId} - Cancel job

---

## File Management

### File Operations
- ❌ POST /api/files/upload - Upload file
- ❌ GET /api/files/{id} - Download file
- ❌ DELETE /api/files/{id} - Delete file
- ❌ POST /api/files/scan - Virus scan file
- ❌ GET /api/files/{id}/metadata - Get file metadata
- ❌ POST /api/files/convert - Convert file format

---

## Search & Discovery

### Global Search
- ❌ GET /api/search/global - Search across all content
- ❌ GET /api/search/suggestions - Search suggestions
- ❌ POST /api/search/advanced - Advanced search with complex filters
- ❌ GET /api/search/history - User search history
- ❌ DELETE /api/search/history - Clear search history

---

## Notifications & Messaging

### Push Notifications
- ❌ POST /api/notifications/register-device - Register device for push
- ❌ DELETE /api/notifications/unregister-device - Unregister device
- ❌ POST /api/notifications/send - Send notification
- ❌ GET /api/notifications - Get user notifications
- ❌ PUT /api/notifications/{id}/read - Mark as read
- ❌ PUT /api/notifications/read-all - Mark all as read
- ❌ DELETE /api/notifications/{id} - Delete notification

### In-App Messaging
- ❌ POST /api/messages - Send message
- ❌ GET /api/messages/conversations - List conversations
- ❌ GET /api/messages/conversation/{id} - Get conversation
- ❌ PUT /api/messages/{id}/read - Mark message read
- ❌ DELETE /api/messages/{id} - Delete message

### Email Management
- ❌ GET /api/emails/templates - List email templates
- ❌ POST /api/emails/send - Send email
- ❌ GET /api/emails/queue - View email queue
- ❌ POST /api/emails/unsubscribe - Unsubscribe from emails

---

## Data Privacy & Compliance

### GDPR/CCPA
- ❌ GET /api/privacy/data-export - Export user data
- ❌ POST /api/privacy/data-deletion - Request data deletion
- ❌ GET /api/privacy/consent - Get consent status
- ❌ PUT /api/privacy/consent - Update consent
- ❌ GET /api/privacy/audit-log - Get data access log

### Compliance Reports
- ❌ GET /api/compliance/hipaa-audit - HIPAA audit report
- ❌ GET /api/compliance/ferpa-audit - FERPA audit report
- ❌ POST /api/compliance/breach-report - Report data breach
- ❌ GET /api/compliance/certifications - Get compliance certs

---

## API Management & Developer Tools

### API Keys & Rate Limiting
- ❌ POST /api/developer/keys - Generate API key
- ❌ GET /api/developer/keys - List API keys
- ❌ DELETE /api/developer/keys/{id} - Revoke API key
- ❌ GET /api/developer/usage - Get API usage stats
- ❌ GET /api/developer/rate-limits - Get rate limit status

### API Documentation
- ❌ GET /api/docs - API documentation
- ❌ GET /api/docs/openapi.json - OpenAPI specification
- ❌ GET /api/docs/postman.json - Postman collection

---

## Summary Statistics

### Total Endpoints Required: ~590
- ✅ Implemented: 4 (0.68%)
- 🚧 Partially Implemented: 0 (0%)
- ❌ Not Implemented: ~586 (99.32%)

### By Feature Area:
1. Authentication & User Management: 4/40 implemented (10%)
2. Resource Library: 0/30 implemented (0%)
3. Therapy Planning: 0/18 implemented (0%)
4. Data Collection: 0/17 implemented (0%)
5. Content Management: 0/9 implemented (0%)
6. AI Content Generation: 0/11 implemented (0%)
7. Marketplace: 0/45 implemented (0%)
8. All other features: 0/330+ implemented (0%)

### Critical Missing Components:
- No resource library (core feature)
- No AI integration
- No marketplace
- No student management
- No therapy planning
- No data collection
- No specialized therapy tools (PECS, ABA, AAC)
- No integrations (EHR, LMS, SSO beyond basic auth)
- No multi-language support
- No mobile/offline capabilities

---

## Next Steps:
1. Decide on technology stack (Keep ASP.NET or switch to Node.js/GraphQL as specified)
2. Prioritize Phase 1 endpoints based on CLAUDE.md timeline
3. Design comprehensive API architecture
4. Implement core features systematically
5. Add extensive test coverage for each endpoint
6. Document all APIs with OpenAPI/Swagger
7. Implement security, rate limiting, and monitoring
8. Deploy with proper CI/CD pipeline

This is a massive undertaking that requires the full 18-month timeline specified in CLAUDE.md.