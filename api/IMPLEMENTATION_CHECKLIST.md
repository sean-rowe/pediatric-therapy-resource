# UPTRMS Implementation Checklist

This checklist tracks the implementation progress of the UPTRMS API based on BDD feature files and test requirements. Each item should be implemented following the BDD cycle: Red (test fails) → Green (make test pass) → Refactor.

## Core Infrastructure

### Database Setup
- [ ] Create `ApplicationDbContext` extending `DbContext`
- [ ] Configure connection string in `appsettings.json`
- [ ] Create initial migration
- [ ] Set up database initialization in `Program.cs`

### Entity Models
- [ ] `User` entity with all required properties
- [ ] `Organization` entity for group subscriptions
- [ ] `Resource` entity for therapy materials
- [ ] `Student` entity for student management
- [ ] `Session` entity for therapy sessions
- [ ] `StudentGoal` entity for IEP goals
- [ ] `SellerProfile` entity for marketplace
- [ ] `MarketplaceTransaction` entity
- [ ] `EmailVerificationToken` entity
- [ ] `PasswordResetToken` entity
- [ ] `RefreshToken` entity
- [ ] `AuditLog` entity for compliance

### Repository Interfaces
- [ ] `IUserRepository` interface
- [ ] `IOrganizationRepository` interface
- [ ] `IResourceRepository` interface
- [ ] `IStudentRepository` interface
- [ ] `ISessionRepository` interface
- [ ] `IMarketplaceRepository` interface
- [ ] `ITokenRepository` interface
- [ ] `IAuditRepository` interface

### Repository Implementations
- [ ] `UserRepository` implementation
- [ ] `OrganizationRepository` implementation
- [ ] `ResourceRepository` implementation
- [ ] `StudentRepository` implementation
- [ ] `SessionRepository` implementation
- [ ] `MarketplaceRepository` implementation
- [ ] `TokenRepository` implementation
- [ ] `AuditRepository` implementation

### Service Interfaces
- [ ] `IAuthenticationService` interface
- [ ] `IUserService` interface
- [ ] `IEmailService` interface
- [ ] `ITokenService` interface
- [ ] `IResourceService` interface
- [ ] `IStudentService` interface
- [ ] `ISessionService` interface
- [ ] `IMarketplaceService` interface
- [ ] `ISubscriptionService` interface
- [ ] `IEncryptionService` interface
- [ ] `IAuditService` interface

### Service Implementations
- [ ] `AuthenticationService` implementation
- [ ] `UserService` implementation
- [ ] `EmailService` implementation
- [ ] `TokenService` implementation
- [ ] `ResourceService` implementation
- [ ] `StudentService` implementation
- [ ] `SessionService` implementation
- [ ] `MarketplaceService` implementation
- [ ] `SubscriptionService` implementation
- [ ] `EncryptionService` implementation
- [ ] `AuditService` implementation

### Dependency Injection Configuration
- [ ] Register all repositories in `Program.cs`
- [ ] Register all services in `Program.cs`
- [ ] Configure JWT authentication
- [ ] Configure authorization policies
- [ ] Configure CORS
- [ ] Configure rate limiting
- [ ] Configure logging
- [ ] Configure health checks

## Authentication Feature Implementation

### Registration (POST /api/auth/register)
- [ ] Validate registration request
- [ ] Check if email already exists
- [ ] Validate license number format
- [ ] Hash password using BCrypt/Argon2
- [ ] Create user record
- [ ] Generate email verification token
- [ ] Send verification email
- [ ] Return success response with userId
- [ ] Create audit log entry

### Login (POST /api/auth/login)
- [ ] Validate login request
- [ ] Find user by email
- [ ] Verify password hash
- [ ] Check if email is verified
- [ ] Generate JWT access token
- [ ] Generate refresh token
- [ ] Store refresh token
- [ ] Return tokens and user info
- [ ] Handle account lockout after failed attempts
- [ ] Create audit log entry

### Email Verification (GET /api/auth/verify-email/{token})
- [ ] Validate token format
- [ ] Find token in database
- [ ] Check token expiration
- [ ] Mark user as verified
- [ ] Mark token as used
- [ ] Return success response
- [ ] Create audit log entry

### Resend Verification (POST /api/auth/resend-verification)
- [ ] Find user by email
- [ ] Check if already verified
- [ ] Check rate limiting (prevent spam)
- [ ] Invalidate old tokens
- [ ] Generate new verification token
- [ ] Send verification email
- [ ] Return success response

### Logout (POST /api/auth/logout)
- [ ] Validate JWT token
- [ ] Revoke refresh token
- [ ] Clear any server-side sessions
- [ ] Return success response
- [ ] Create audit log entry

### Forgot Password (POST /api/auth/forgot-password)
- [ ] Validate email
- [ ] Find user by email (timing-safe)
- [ ] Generate password reset token
- [ ] Send reset email
- [ ] Return generic success (prevent enumeration)
- [ ] Create audit log entry

### Reset Password (POST /api/auth/reset-password)
- [ ] Validate reset token
- [ ] Check token expiration
- [ ] Validate new password requirements
- [ ] Hash new password
- [ ] Update user password
- [ ] Invalidate all refresh tokens
- [ ] Send confirmation email
- [ ] Create audit log entry

### Change Password (POST /api/auth/change-password)
- [ ] Validate current password
- [ ] Validate new password requirements
- [ ] Check password history (no reuse)
- [ ] Hash new password
- [ ] Update user password
- [ ] Invalidate other sessions
- [ ] Send notification email
- [ ] Create audit log entry

### SSO Providers (GET /api/auth/sso/providers)
- [ ] Return list of configured SSO providers
- [ ] Include provider metadata
- [ ] Filter by organization if applicable

## User Management Implementation

### Get Profile (GET /api/users/profile)
- [ ] Extract user ID from JWT
- [ ] Fetch user with organization data
- [ ] Map to UserDto
- [ ] Return user profile

### Update Profile (PUT /api/users/profile)
- [ ] Validate update request
- [ ] Update allowed fields only
- [ ] Save changes
- [ ] Create audit log entry
- [ ] Return updated profile

### Update Language (PUT /api/users/language)
- [ ] Validate language code
- [ ] Update user preference
- [ ] Return success response

### Get Subscription (GET /api/users/subscription)
- [ ] Fetch user subscription details
- [ ] Include organization info if applicable
- [ ] Calculate usage limits
- [ ] Return subscription DTO

### Search Users (GET /api/users) [Admin Only]
- [ ] Validate admin permissions
- [ ] Implement search logic
- [ ] Apply pagination
- [ ] Return user list

### Get User by ID (GET /api/users/{id}) [Admin Only]
- [ ] Validate admin permissions
- [ ] Fetch specific user
- [ ] Include related data
- [ ] Return user DTO

## Resource Management Implementation

### Search Resources (GET /api/resources)
- [ ] Implement search filters
- [ ] Apply skill area filtering
- [ ] Apply grade level filtering
- [ ] Apply resource type filtering
- [ ] Apply language filtering
- [ ] Apply evidence level filtering
- [ ] Implement pagination
- [ ] Track view analytics
- [ ] Return search results

### Get Resource (GET /api/resources/{id})
- [ ] Fetch resource by ID
- [ ] Include related data (ratings, reviews)
- [ ] Increment view count
- [ ] Check user access permissions
- [ ] Return resource DTO

### Download Resource (GET /api/resources/{id}/download)
- [ ] Check user subscription limits
- [ ] Verify resource access
- [ ] Track download count
- [ ] Update usage analytics
- [ ] Return file with proper headers
- [ ] Create audit log entry

### Favorite Resource (POST /api/resources/{id}/favorite)
- [ ] Add to user favorites
- [ ] Update favorite count
- [ ] Return success response

### Remove Favorite (DELETE /api/resources/{id}/favorite)
- [ ] Remove from user favorites
- [ ] Update favorite count
- [ ] Return success response

### Get Favorites (GET /api/resources/favorites)
- [ ] Fetch user's favorite resources
- [ ] Apply pagination
- [ ] Include resource details
- [ ] Return favorite list

### Rate Resource (POST /api/resources/{id}/rate)
- [ ] Validate rating (1-5)
- [ ] Store user rating
- [ ] Update average rating
- [ ] Return updated metrics

### Get Ratings (GET /api/resources/{id}/ratings)
- [ ] Fetch all ratings for resource
- [ ] Calculate distribution
- [ ] Include review comments
- [ ] Return ratings data

### Report Resource (POST /api/resources/{id}/report)
- [ ] Validate report reason
- [ ] Create moderation ticket
- [ ] Notify admin team
- [ ] Return confirmation

### Get Versions (GET /api/resources/{id}/versions)
- [ ] Fetch version history
- [ ] Include change notes
- [ ] Return version list

### Copy Resource (POST /api/resources/{id}/copy)
- [ ] Verify user permissions
- [ ] Create resource copy
- [ ] Link to original
- [ ] Return new resource ID

### Get Related (GET /api/resources/{id}/related)
- [ ] Find similar resources
- [ ] Apply relevance algorithm
- [ ] Return related list

### Share Resource (POST /api/resources/{id}/share)
- [ ] Generate share link
- [ ] Set expiration
- [ ] Send share email
- [ ] Return share details

### Get Shared (GET /api/resources/shared-with-me)
- [ ] Fetch resources shared with user
- [ ] Check expiration
- [ ] Return shared list

### Add to Collection (POST /api/resources/{id}/collections)
- [ ] Add resource to collection
- [ ] Update collection metadata
- [ ] Return success response

### Get Usage Stats (GET /api/resources/{id}/usage)
- [ ] Fetch download statistics
- [ ] Calculate usage metrics
- [ ] Return analytics data

### Submit Clinical Review (POST /api/resources/{id}/clinical-review)
- [ ] Create review request
- [ ] Notify reviewers
- [ ] Update review status
- [ ] Return review details

### Create Resource (POST /api/resources) [Seller Only]
- [ ] Validate resource data
- [ ] Upload file to storage
- [ ] Create resource record
- [ ] Submit for review
- [ ] Return created resource

### Update Resource (PUT /api/resources/{id}) [Seller Only]
- [ ] Verify ownership
- [ ] Update resource data
- [ ] Reset review status
- [ ] Return updated resource

### Delete Resource (DELETE /api/resources/{id}) [Seller Only]
- [ ] Verify ownership
- [ ] Soft delete resource
- [ ] Update related data
- [ ] Return success response

### Get Popular Resources (GET /api/resources/popular)
- [ ] Query by download count
- [ ] Apply time window
- [ ] Return popular list

### Get Recent Resources (GET /api/resources/recent)
- [ ] Query by creation date
- [ ] Filter approved only
- [ ] Return recent list

### Get Free Resources (GET /api/resources/free)
- [ ] Filter by price = 0
- [ ] Apply other filters
- [ ] Return free list

### Get Seller Resources (GET /api/resources/seller/{sellerId})
- [ ] Fetch seller's resources
- [ ] Include seller info
- [ ] Return resource list

## Student Management Implementation

### Get Students (GET /api/students)
- [ ] Fetch therapist's students
- [ ] Include basic info
- [ ] Apply sorting
- [ ] Return student list

### Get Student (GET /api/students/{id})
- [ ] Verify therapist access
- [ ] Include goals and sessions
- [ ] Return detailed DTO

### Create Student (POST /api/students)
- [ ] Validate student data
- [ ] Generate access code
- [ ] Encrypt PII data
- [ ] Return created student

### Update Student (PUT /api/students/{id})
- [ ] Verify therapist access
- [ ] Update allowed fields
- [ ] Re-encrypt if needed
- [ ] Return updated student

### Add Goal (POST /api/students/{id}/goals)
- [ ] Verify therapist access
- [ ] Create goal record
- [ ] Link to student
- [ ] Return created goal

### Discharge Student (POST /api/students/{id}/discharge)
- [ ] Verify therapist access
- [ ] Update student status
- [ ] Archive data
- [ ] Return confirmation

## Session Management Implementation

### Get Sessions (GET /api/sessions)
- [ ] Fetch therapist's sessions
- [ ] Apply date filters
- [ ] Include student info
- [ ] Return session list

### Get Session (GET /api/sessions/{id})
- [ ] Verify therapist access
- [ ] Include session details
- [ ] Include resources used
- [ ] Return session DTO

### Create Session (POST /api/sessions)
- [ ] Validate session data
- [ ] Link resources used
- [ ] Track progress data
- [ ] Return created session

### Update Session (PUT /api/sessions/{id})
- [ ] Verify therapist access
- [ ] Update session data
- [ ] Update progress tracking
- [ ] Return updated session

### Delete Session (DELETE /api/sessions/{id})
- [ ] Verify therapist access
- [ ] Soft delete session
- [ ] Update related data
- [ ] Return confirmation

## Marketplace Implementation

### Get Products (GET /api/marketplace/products)
- [ ] Fetch marketplace products
- [ ] Apply filters
- [ ] Include seller info
- [ ] Return product list

### Get Product (GET /api/marketplace/products/{id})
- [ ] Fetch product details
- [ ] Include seller info
- [ ] Include ratings
- [ ] Return product DTO

### Purchase Product (POST /api/marketplace/purchase)
- [ ] Validate purchase request
- [ ] Process payment
- [ ] Create transaction record
- [ ] Grant access to buyer
- [ ] Notify seller
- [ ] Return purchase confirmation

### Get Seller Dashboard (GET /api/marketplace/seller/dashboard)
- [ ] Verify seller status
- [ ] Fetch sales metrics
- [ ] Calculate earnings
- [ ] Return dashboard data

### Apply to Be Seller (POST /api/marketplace/seller/apply)
- [ ] Validate application
- [ ] Create seller profile
- [ ] Submit for review
- [ ] Return application status

## Admin Features Implementation

### Get Admin Dashboard (GET /api/admin/dashboard)
- [ ] Fetch system metrics
- [ ] Calculate user statistics
- [ ] Get resource metrics
- [ ] Get revenue data
- [ ] Check system health
- [ ] Return dashboard DTO

## Testing Checklist

### Authentication Tests
- [ ] All registration scenarios pass
- [ ] All login scenarios pass
- [ ] Email verification tests pass
- [ ] Password reset tests pass
- [ ] Account lockout tests pass
- [ ] SSO tests pass

### Resource Tests
- [ ] Search functionality tests pass
- [ ] Download limit tests pass
- [ ] Favorite management tests pass
- [ ] Rating system tests pass
- [ ] Sharing mechanism tests pass

### Student Management Tests
- [ ] CRUD operations tests pass
- [ ] Access control tests pass
- [ ] Goal tracking tests pass
- [ ] Data encryption tests pass

### Integration Tests
- [ ] End-to-end auth flow works
- [ ] Resource purchase flow works
- [ ] Student session flow works
- [ ] Admin functions work

### Performance Tests
- [ ] API response times < 500ms
- [ ] Database queries optimized
- [ ] Caching implemented where needed
- [ ] Load testing passes

### Security Tests
- [ ] Authentication is secure
- [ ] Authorization works correctly
- [ ] Data encryption verified
- [ ] Input validation complete
- [ ] SQL injection prevented
- [ ] XSS prevention in place

## Notes

- Always run the relevant BDD test before implementing
- Implement just enough to make the test pass
- Refactor after the test is green
- Update this checklist after each test passes
- Commit frequently with descriptive messages
- Follow SOLID principles
- Keep security in mind at all times
- Document any deviations or decisions