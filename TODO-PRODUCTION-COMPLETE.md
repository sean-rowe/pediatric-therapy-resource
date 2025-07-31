# Complete Atomic Task List for UPTRMS Production Launch

Every single task needed to go from current state to production. No grouping - just pure atomic tasks.

## Current State Assessment

- [x] Run `dotnet build` in api directory
- [x] Document all build warnings in a text file
- [x] Run `dotnet test` in api directory  
- [x] Document all test failures in a text file
- [x] Check database connection string in appsettings.Development.json
- [x] Verify SQL Server is running locally
- [x] Run `dotnet ef database update` to verify migrations work
- [x] List all tables currently in database
- [x] Count total Gherkin scenarios in BDD tests (2,903 scenarios)
- [x] Count implemented vs not implemented BDD steps (3,627 step definitions)
- [x] Check which controllers return NotImplementedException (AdminController, MarketplaceController, ResourcesController, StudentsController, UsersController)
- [x] List all external service dependencies needed (SQL Server, SendGrid, AWS S3, Stripe, OpenAI, Stable Diffusion/Replicate, State License APIs)
- [x] Document current Azure/AWS resources if any (None currently - using local development)
- [x] Check if Docker is installed locally (Docker version 28.3.2)
- [x] Check if Kubernetes is configured (kubectl v1.32.2 installed)
- [x] Verify Git branch structure is correct (master, develop, feature branches present)

## Fix Immediate Issues

- [x] Open api/UPTRMS.Api.csproj file
- [x] Change `<RootNamespace>` to UPTRMS.Api if needed (Already correct)
- [x] Change `<AssemblyName>` to UPTRMS.Api if needed (Already set to UPTRMS.Api)
- [x] Delete api/TherapyDocs.Api.csproj if it exists (Doesn't exist)
- [x] Update all namespace declarations from TherapyDocs to UPTRMS (Already done)
- [x] Fix all using statements from TherapyDocs to UPTRMS (Already done)
- [x] Run find/replace TherapyDocs -> UPTRMS across solution (Already complete)
- [x] Rebuild solution to verify namespace fixes
- [x] Fix any remaining compilation errors (None found)
- [x] Commit namespace fixes to Git (Already done - no namespace changes needed)

## User Management Implementation

- [x] Open Controllers/UsersController.cs
- [x] Remove NotImplementedException from GetProfile method
- [x] Add `var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;`
- [x] Add null check `if (userId == null) return Unauthorized();`
- [x] Add `var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));`
- [x] Add null check `if (user == null) return NotFound();`
- [x] Add `return Ok(MapToDto(user));` (Using existing UserDto instead of UserProfileDto)
- [x] Implement MapToDto method to convert User to UserDto
- [x] Remove NotImplementedException from UpdateProfile method
- [x] Implement UpdateProfile logic with conditional field updates
- [x] Remove duplicate UpdateProfileRequest from controller (exists in UserDtos.cs)
- [x] Check User entity already has FirstName and LastName (Already exists)
- [x] Add `public string? ProfileImageUrl { get; set; }` to User entity
- [x] Create new migration `dotnet ef migrations add AddUserProfileImageUrl`
- [x] Apply migration `dotnet ef database update`
- [x] Skip AutoMapper setup - using manual MapToDto method instead
- [x] Verify IUserRepository exists and has GetByIdAsync (inherited from IRepository)
- [x] Verify UpdateAsync exists in IRepository
- [x] Check if UserRepository is registered in Program.cs
- [x] Build project successfully with no errors
- [ ] Test GetProfile endpoint with curl or API testing
- [ ] Verify 401 returned when not authenticated
- [ ] Get valid JWT token from login
- [ ] Test GetProfile with valid token
- [ ] Verify user data returned correctly

## UpdateProfile Endpoint

- [x] Open Controllers/UsersController.cs
- [x] Remove NotImplementedException from UpdateProfile method (Already implemented)
- [x] Implement UpdateLanguage endpoint
- [x] Implement GetSubscription endpoint
- [x] Implement GetUser endpoint (Admin only)
- [x] Implement SearchUsers endpoint (Admin only)
- [x] All UserController endpoints implemented successfully
- [ ] Create Models/DTOs/UserUpdateDto.cs
- [ ] Add `public string? FirstName { get; set; }`
- [ ] Add `public string? LastName { get; set; }`
- [ ] Add `public string? PhoneNumber { get; set; }`
- [ ] Add validation attributes to UserUpdateDto
- [ ] Add `[StringLength(100)]` to FirstName
- [ ] Add `[StringLength(100)]` to LastName  
- [ ] Add `[Phone]` to PhoneNumber
- [ ] Back in UsersController.UpdateProfile
- [ ] Add `var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;`
- [ ] Add `if (userId == null) return Unauthorized();`
- [ ] Add `var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));`
- [ ] Add `if (user == null) return NotFound();`
- [ ] Add `if (!string.IsNullOrEmpty(updateDto.FirstName)) user.FirstName = updateDto.FirstName;`
- [ ] Add `if (!string.IsNullOrEmpty(updateDto.LastName)) user.LastName = updateDto.LastName;`
- [ ] Add `if (!string.IsNullOrEmpty(updateDto.PhoneNumber)) user.PhoneNumber = updateDto.PhoneNumber;`
- [ ] Add `user.UpdatedAt = DateTime.UtcNow;`
- [ ] Add `await _userRepository.UpdateAsync(user);`
- [ ] Add `var dto = _mapper.Map<UserProfileDto>(user);`
- [ ] Add `return Ok(dto);`
- [ ] Add UpdateAsync method to IUserRepository
- [ ] Implement UpdateAsync in UserRepository
- [ ] Add `_context.Users.Update(user);`
- [ ] Add `await _context.SaveChangesAsync();`
- [ ] Test UpdateProfile with Postman
- [ ] Verify profile updates correctly

## ChangePassword Endpoint

- [ ] Open Controllers/UsersController.cs
- [ ] Remove NotImplementedException from ChangePassword
- [ ] Create Models/DTOs/ChangePasswordDto.cs
- [ ] Add `public string CurrentPassword { get; set; }`
- [ ] Add `public string NewPassword { get; set; }`
- [ ] Add `[Required]` to CurrentPassword
- [ ] Add `[Required]` to NewPassword
- [ ] Add `[StringLength(100, MinimumLength = 8)]` to NewPassword
- [ ] Back in ChangePassword method
- [ ] Add `var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;`
- [ ] Add `if (userId == null) return Unauthorized();`
- [ ] Add `var result = await _authService.ChangePasswordAsync(Guid.Parse(userId), changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);`
- [ ] Add `if (!result.Success) return BadRequest(new { error = result.Error });`
- [ ] Add `return Ok(new { message = "Password changed successfully" });`
- [ ] Open Interfaces/IAuthenticationService.cs
- [ ] Add `Task<(bool Success, string? Error)> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);`
- [ ] Open Services/AuthenticationService.cs
- [ ] Implement ChangePasswordAsync method
- [ ] Add `var user = await _context.Users.FindAsync(userId);`
- [ ] Add `if (user == null) return (false, "User not found");`
- [ ] Add `if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash)) return (false, "Current password is incorrect");`
- [ ] Add `user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);`
- [ ] Add `user.UpdatedAt = DateTime.UtcNow;`
- [ ] Add `_context.Users.Update(user);`
- [ ] Add `await _context.SaveChangesAsync();`
- [ ] Add `return (true, null);`
- [ ] Test ChangePassword endpoint
- [ ] Verify password changes correctly

## Organization Entity Creation

- [ ] Create Models/Domain/Organization.cs
- [ ] Add `using System;`
- [ ] Add `using System.ComponentModel.DataAnnotations;`
- [ ] Add `namespace UPTRMS.Api.Models.Domain`
- [ ] Add `public class Organization`
- [ ] Add `[Key] public Guid Id { get; set; }`
- [ ] Add `[Required] [MaxLength(200)] public string Name { get; set; }`
- [ ] Add `[Required] [MaxLength(50)] public string Type { get; set; }`
- [ ] Add `[MaxLength(50)] public string? TaxId { get; set; }`
- [ ] Add `[MaxLength(500)] public string? Address { get; set; }`
- [ ] Add `[MaxLength(100)] public string? City { get; set; }`
- [ ] Add `[MaxLength(50)] public string? State { get; set; }`
- [ ] Add `[MaxLength(20)] public string? ZipCode { get; set; }`
- [ ] Add `[MaxLength(100)] public string? Country { get; set; }`
- [ ] Add `[Phone] [MaxLength(20)] public string? Phone { get; set; }`
- [ ] Add `[EmailAddress] [MaxLength(255)] public string? Email { get; set; }`
- [ ] Add `[Url] [MaxLength(500)] public string? Website { get; set; }`
- [ ] Add `[MaxLength(1000)] public string? LogoUrl { get; set; }`
- [ ] Add `public bool IsActive { get; set; } = true;`
- [ ] Add `public DateTime CreatedAt { get; set; }`
- [ ] Add `public DateTime UpdatedAt { get; set; }`
- [ ] Add `public virtual ICollection<OrganizationUser> Users { get; set; }`
- [ ] Add closing brace
- [ ] Create Models/Domain/OrganizationUser.cs
- [ ] Add required using statements
- [ ] Add `public class OrganizationUser`
- [ ] Add `public Guid OrganizationId { get; set; }`
- [ ] Add `public Guid UserId { get; set; }`
- [ ] Add `[Required] [MaxLength(50)] public string Role { get; set; }`
- [ ] Add `public bool IsAdmin { get; set; } = false;`
- [ ] Add `public DateTime JoinedAt { get; set; }`
- [ ] Add `public virtual Organization Organization { get; set; }`
- [ ] Add `public virtual User User { get; set; }`
- [ ] Open Data/ApplicationDbContext.cs
- [ ] Add `public DbSet<Organization> Organizations { get; set; }`
- [ ] Add `public DbSet<OrganizationUser> OrganizationUsers { get; set; }`
- [ ] In OnModelCreating method add composite key configuration
- [ ] Add `modelBuilder.Entity<OrganizationUser>().HasKey(ou => new { ou.OrganizationId, ou.UserId });`
- [ ] Create migration `dotnet ef migrations add AddOrganizationEntities`
- [ ] Apply migration `dotnet ef database update`

## Student Entity Creation

- [ ] Create Models/Domain/Student.cs
- [ ] Add using statements
- [ ] Add `public class Student`
- [ ] Add `[Key] public Guid Id { get; set; }`
- [ ] Add `[Required] [MaxLength(100)] public string FirstName { get; set; }`
- [ ] Add `[Required] [MaxLength(100)] public string LastName { get; set; }`
- [ ] Add `[Required] public DateTime DateOfBirth { get; set; }`
- [ ] Add `[MaxLength(20)] public string? Grade { get; set; }`
- [ ] Add `[Required] public Guid TherapistId { get; set; }`
- [ ] Add `public Guid? OrganizationId { get; set; }`
- [ ] Add `[Required] [MaxLength(10)] public string AccessCode { get; set; }`
- [ ] Add `[EmailAddress] [MaxLength(255)] public string? ParentEmail { get; set; }`
- [ ] Add `[Phone] [MaxLength(20)] public string? ParentPhone { get; set; }`
- [ ] Add `public bool IsActive { get; set; } = true;`
- [ ] Add `public DateTime CreatedAt { get; set; }`
- [ ] Add `public DateTime UpdatedAt { get; set; }`
- [ ] Add `public virtual User Therapist { get; set; }`
- [ ] Add `public virtual Organization? Organization { get; set; }`
- [ ] Open Data/ApplicationDbContext.cs
- [ ] Add `public DbSet<Student> Students { get; set; }`
- [ ] Create migration `dotnet ef migrations add AddStudentEntity`
- [ ] Apply migration

## Resource Entity Creation

- [ ] Create Models/Domain/Resource.cs
- [ ] Add using statements
- [ ] Add `public class Resource`
- [ ] Add `[Key] public Guid Id { get; set; }`
- [ ] Add `[Required] [MaxLength(500)] public string Title { get; set; }`
- [ ] Add `[MaxLength(2000)] public string? Description { get; set; }`
- [ ] Add `[Required] [MaxLength(50)] public string ResourceType { get; set; }`
- [ ] Add `[Required] public string SkillAreas { get; set; }` // JSON
- [ ] Add `[Required] public string GradeLevels { get; set; }` // JSON
- [ ] Add `[MaxLength(1000)] public string? FileUrl { get; set; }`
- [ ] Add `[MaxLength(1000)] public string? ThumbnailUrl { get; set; }`
- [ ] Add `public long FileSize { get; set; }`
- [ ] Add `[MaxLength(50)] public string? FileType { get; set; }`
- [ ] Add `public bool IsActive { get; set; } = true;`
- [ ] Add `public bool IsDeleted { get; set; } = false;`
- [ ] Add `public int ViewCount { get; set; } = 0;`
- [ ] Add `public int DownloadCount { get; set; } = 0;`
- [ ] Add `public decimal? AverageRating { get; set; }`
- [ ] Add `public int TotalRatings { get; set; } = 0;`
- [ ] Add `public Guid CreatedBy { get; set; }`
- [ ] Add `public DateTime CreatedAt { get; set; }`
- [ ] Add `public DateTime UpdatedAt { get; set; }`
- [ ] Add `public virtual User Creator { get; set; }`
- [ ] Open Data/ApplicationDbContext.cs
- [ ] Add `public DbSet<Resource> Resources { get; set; }`
- [ ] Create migration
- [ ] Apply migration

## Session Entity Creation

- [ ] Create Models/Domain/Session.cs
- [ ] Add using statements
- [ ] Add `public class Session`
- [ ] Add `[Key] public Guid Id { get; set; }`
- [ ] Add `[Required] public Guid TherapistId { get; set; }`
- [ ] Add `[Required] public Guid StudentId { get; set; }`
- [ ] Add `[Required] public DateTime ScheduledAt { get; set; }`
- [ ] Add `public DateTime? StartedAt { get; set; }`
- [ ] Add `public DateTime? EndedAt { get; set; }`
- [ ] Add `[Required] public int DurationMinutes { get; set; }`
- [ ] Add `[Required] [MaxLength(50)] public string SessionType { get; set; }`
- [ ] Add `[Required] [MaxLength(50)] public string SessionStatus { get; set; }`
- [ ] Add `[MaxLength(200)] public string? Location { get; set; }`
- [ ] Add `public bool IsVirtual { get; set; } = false;`
- [ ] Add `public string? Notes { get; set; }`
- [ ] Add `public bool IsBillable { get; set; } = true;`
- [ ] Add `public DateTime CreatedAt { get; set; }`
- [ ] Add `public DateTime UpdatedAt { get; set; }`
- [ ] Add `public virtual User Therapist { get; set; }`
- [ ] Add `public virtual Student Student { get; set; }`
- [ ] Open Data/ApplicationDbContext.cs
- [ ] Add `public DbSet<Session> Sessions { get; set; }`
- [ ] Create migration
- [ ] Apply migration

## Create All Repositories

- [ ] Create Interfaces/IOrganizationRepository.cs
- [ ] Add `public interface IOrganizationRepository : IRepository<Organization>`
- [ ] Add `Task<Organization?> GetByIdWithUsersAsync(Guid id);`
- [ ] Add `Task<IEnumerable<Organization>> GetActiveOrganizationsAsync();`
- [ ] Create Repositories/OrganizationRepository.cs
- [ ] Add `public class OrganizationRepository : BaseRepository<Organization>, IOrganizationRepository`
- [ ] Add constructor `public OrganizationRepository(ApplicationDbContext context) : base(context) { }`
- [ ] Implement GetByIdWithUsersAsync
- [ ] Add `return await _context.Organizations.Include(o => o.Users).ThenInclude(ou => ou.User).FirstOrDefaultAsync(o => o.Id == id);`
- [ ] Implement GetActiveOrganizationsAsync
- [ ] Add `return await _context.Organizations.Where(o => o.IsActive).ToListAsync();`
- [ ] Register in Program.cs
- [ ] Add `builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();`

- [ ] Create Interfaces/IStudentRepository.cs
- [ ] Add interface definition
- [ ] Add method signatures
- [ ] Create Repositories/StudentRepository.cs
- [ ] Implement all methods
- [ ] Register in Program.cs

- [ ] Create Interfaces/IResourceRepository.cs
- [ ] Add interface definition
- [ ] Add method signatures
- [ ] Create Repositories/ResourceRepository.cs
- [ ] Implement all methods
- [ ] Register in Program.cs

- [ ] Create Interfaces/ISessionRepository.cs
- [ ] Add interface definition
- [ ] Add method signatures
- [ ] Create Repositories/SessionRepository.cs
- [ ] Implement all methods
- [ ] Register in Program.cs

## Create All Services

- [ ] Create Services/IOrganizationService.cs
- [ ] Add interface methods
- [ ] Create Services/OrganizationService.cs
- [ ] Add private fields for dependencies
- [ ] Add constructor
- [ ] Implement each method
- [ ] Register in Program.cs

- [ ] Create Services/IStudentService.cs
- [ ] Add interface methods
- [ ] Create Services/StudentService.cs
- [ ] Implement all methods
- [ ] Register in Program.cs

- [ ] Create Services/IResourceService.cs
- [ ] Add interface methods
- [ ] Create Services/ResourceService.cs
- [ ] Implement all methods
- [ ] Register in Program.cs

- [ ] Create Services/ISessionService.cs
- [ ] Add interface methods
- [ ] Create Services/SessionService.cs
- [ ] Implement all methods
- [ ] Register in Program.cs

## Implement All Controller Endpoints

- [ ] Open Controllers/OrganizationController.cs (create if needed)
- [ ] Add constructor with IOrganizationService
- [ ] Implement GET /api/organizations
- [ ] Implement GET /api/organizations/{id}
- [ ] Implement POST /api/organizations
- [ ] Implement PUT /api/organizations/{id}
- [ ] Implement DELETE /api/organizations/{id}
- [ ] Implement POST /api/organizations/{id}/invite
- [ ] Add authorization attributes
- [ ] Test each endpoint

- [ ] Open Controllers/StudentsController.cs
- [ ] Remove all NotImplementedException
- [ ] Implement GetStudents
- [ ] Implement GetStudentById
- [ ] Implement CreateStudent
- [ ] Implement UpdateStudent
- [ ] Implement DeleteStudent
- [ ] Implement GenerateAccessCode
- [ ] Test each endpoint

- [ ] Open Controllers/ResourcesController.cs
- [ ] Remove all NotImplementedException (there are many!)
- [ ] Implement SearchResources
- [ ] Implement GetResourceById
- [ ] Implement CreateResource
- [ ] Implement UpdateResource
- [ ] Implement DeleteResource
- [ ] Implement DownloadResource
- [ ] Implement FavoriteResource
- [ ] Implement UnfavoriteResource
- [ ] Implement RateResource
- [ ] Implement GetRecommendations
- [ ] Continue with all other endpoints...
- [ ] Test each endpoint

- [ ] Open Controllers/SessionsController.cs
- [ ] Remove all NotImplementedException
- [ ] Implement all session endpoints
- [ ] Test each endpoint

## External Service Integrations

- [ ] Create AWS account if not exists
- [ ] Create S3 bucket for file storage
- [ ] Get AWS access key and secret key
- [ ] Add AWS settings to appsettings.json
- [ ] Install AWSSDK.S3 NuGet package
- [ ] Create Services/IFileStorageService.cs
- [ ] Create Services/S3StorageService.cs
- [ ] Implement UploadFileAsync
- [ ] Implement DeleteFileAsync
- [ ] Implement GetFileUrlAsync
- [ ] Implement GetFileStreamAsync
- [ ] Register service in Program.cs
- [ ] Test file upload

- [ ] Create Stripe account
- [ ] Get Stripe API keys
- [ ] Add Stripe settings to appsettings.json
- [ ] Install Stripe.net NuGet package
- [ ] Create Services/IPaymentService.cs
- [ ] Create Services/StripePaymentService.cs
- [ ] Implement CreateCustomer
- [ ] Implement CreateSubscription
- [ ] Implement UpdateSubscription
- [ ] Implement CancelSubscription
- [ ] Implement ProcessPayment
- [ ] Register service in Program.cs
- [ ] Test payment processing

- [ ] Create SendGrid account
- [ ] Get SendGrid API key
- [ ] Add SendGrid settings to appsettings.json
- [ ] Install SendGrid NuGet package
- [ ] Update EmailService to use SendGrid
- [ ] Test email sending

- [ ] Create Twilio account
- [ ] Get Twilio credentials
- [ ] Add Twilio settings to appsettings.json
- [ ] Install Twilio NuGet package
- [ ] Create Services/ISmsService.cs
- [ ] Create Services/TwilioSmsService.cs
- [ ] Implement SendSmsAsync
- [ ] Register service in Program.cs
- [ ] Test SMS sending

## Database Performance

- [ ] Add index on Students.TherapistId
- [ ] Add index on Students.AccessCode (unique)
- [ ] Add index on Resources.CreatedBy
- [ ] Add index on Resources.ResourceType
- [ ] Add index on Sessions.TherapistId
- [ ] Add index on Sessions.StudentId
- [ ] Add index on Sessions.ScheduledAt
- [ ] Add composite index on Sessions (TherapistId, ScheduledAt)
- [ ] Add index on Users.Email (unique)
- [ ] Create migration for indexes
- [ ] Apply migration

## Security Implementation

- [ ] Install Microsoft.AspNetCore.RateLimiting
- [ ] Configure rate limiting in Program.cs
- [ ] Add rate limit policy for API endpoints
- [ ] Add rate limit policy for auth endpoints
- [ ] Apply rate limiting attributes to controllers
- [ ] Test rate limiting works

- [ ] Add security headers middleware
- [ ] Add X-Content-Type-Options: nosniff
- [ ] Add X-Frame-Options: DENY
- [ ] Add X-XSS-Protection: 1; mode=block
- [ ] Add Strict-Transport-Security
- [ ] Add Content-Security-Policy
- [ ] Test headers are present

- [ ] Configure CORS properly
- [ ] Add allowed origins
- [ ] Add allowed methods
- [ ] Add allowed headers
- [ ] Test CORS from browser

## Error Handling

- [ ] Create Middleware/GlobalErrorHandlingMiddleware.cs
- [ ] Add exception catching
- [ ] Add logging of errors
- [ ] Add user-friendly error responses
- [ ] Add correlation ID to errors
- [ ] Register middleware in Program.cs
- [ ] Test error handling

- [ ] Create custom exception classes
- [ ] Create NotFoundException
- [ ] Create ValidationException
- [ ] Create ForbiddenException
- [ ] Create ConflictException
- [ ] Update services to use custom exceptions
- [ ] Test exception responses

## Logging and Monitoring

- [ ] Install Serilog packages
- [ ] Configure Serilog in Program.cs
- [ ] Add file sink for logs
- [ ] Add console sink for development
- [ ] Add structured logging throughout
- [ ] Add request/response logging middleware
- [ ] Test logs are written correctly

- [ ] Create Application Insights resource in Azure
- [ ] Get instrumentation key
- [ ] Install Application Insights packages
- [ ] Configure in Program.cs
- [ ] Add telemetry throughout
- [ ] Test metrics appear in Azure

## API Documentation

- [ ] Install Swashbuckle.AspNetCore
- [ ] Configure Swagger in Program.cs
- [ ] Add XML documentation to project
- [ ] Add XML comments to all endpoints
- [ ] Add XML comments to all DTOs
- [ ] Configure Swagger to use XML docs
- [ ] Add JWT authentication to Swagger
- [ ] Test Swagger UI works
- [ ] Add API versioning
- [ ] Document all response types

## Testing

- [ ] Create unit test for UserRepository.GetByIdAsync
- [ ] Create unit test for UserRepository.UpdateAsync
- [ ] Create unit test for OrganizationRepository.GetByIdWithUsersAsync
- [ ] Create unit test for StudentRepository.GetByAccessCodeAsync
- [ ] Create unit test for ResourceRepository.SearchAsync
- [ ] Continue creating unit tests for all repository methods...

- [ ] Create unit test for OrganizationService.CreateAsync
- [ ] Create unit test for StudentService.GenerateAccessCodeAsync
- [ ] Create unit test for ResourceService.SearchAsync
- [ ] Continue creating unit tests for all service methods...

- [ ] Create integration test for POST /api/auth/register
- [ ] Create integration test for POST /api/auth/login
- [ ] Create integration test for GET /api/users/profile
- [ ] Create integration test for PUT /api/users/profile
- [ ] Continue creating integration tests for all endpoints...

- [ ] Run all BDD tests
- [ ] Fix any failing BDD tests
- [ ] Implement missing step definitions
- [ ] Achieve 100% BDD test pass rate

## Docker Configuration

- [ ] Create Dockerfile in api directory
- [ ] Add FROM mcr.microsoft.com/dotnet/aspnet:8.0
- [ ] Add WORKDIR /app
- [ ] Add COPY commands
- [ ] Add ENTRYPOINT
- [ ] Build Docker image
- [ ] Test Docker image runs

- [ ] Create docker-compose.yml in root
- [ ] Add api service
- [ ] Add SQL Server service
- [ ] Add Redis service
- [ ] Add volume mappings
- [ ] Add network configuration
- [ ] Test docker-compose up

## Kubernetes Configuration

- [ ] Create kubernetes directory
- [ ] Create deployment.yaml for API
- [ ] Create service.yaml for API
- [ ] Create configmap.yaml for settings
- [ ] Create secret.yaml for sensitive data
- [ ] Create ingress.yaml for routing
- [ ] Create horizontal pod autoscaler
- [ ] Test deployment locally with minikube

## CI/CD Pipeline

- [ ] Create .github/workflows/build.yml
- [ ] Add checkout step
- [ ] Add .NET setup step
- [ ] Add restore step
- [ ] Add build step
- [ ] Add test step
- [ ] Add publish step
- [ ] Test pipeline runs on push

- [ ] Create .github/workflows/deploy.yml
- [ ] Add environment configuration
- [ ] Add Azure login step
- [ ] Add Docker build and push
- [ ] Add Kubernetes deployment
- [ ] Add smoke test step
- [ ] Test deployment pipeline

## Production Environment Setup

- [ ] Create Azure resource group
- [ ] Create Azure Kubernetes Service cluster
- [ ] Create Azure Container Registry
- [ ] Create Azure SQL Database
- [ ] Create Azure Redis Cache
- [ ] Create Azure Application Gateway
- [ ] Create Azure Key Vault
- [ ] Create Azure Monitor workspace
- [ ] Configure all connection strings
- [ ] Configure all secrets

- [ ] Create production appsettings.Production.json
- [ ] Add production database connection string
- [ ] Add production Redis connection string
- [ ] Add production storage settings
- [ ] Add production email settings
- [ ] Add production payment settings
- [ ] Configure for Azure Key Vault
- [ ] Test configuration loading

## Security Hardening

- [ ] Run security scan with OWASP ZAP
- [ ] Fix any high vulnerabilities
- [ ] Fix any medium vulnerabilities
- [ ] Run dependency vulnerability scan
- [ ] Update any vulnerable packages
- [ ] Configure WAF rules
- [ ] Enable DDoS protection
- [ ] Configure backup encryption
- [ ] Enable audit logging
- [ ] Test security measures

## Performance Testing

- [ ] Install Apache JMeter
- [ ] Create test plan for login endpoint
- [ ] Create test plan for resource search
- [ ] Create test plan for file upload
- [ ] Run load test with 100 users
- [ ] Run load test with 1000 users
- [ ] Document response times
- [ ] Identify bottlenecks
- [ ] Optimize slow queries
- [ ] Re-test after optimization

## Data Migration

- [ ] Create data migration scripts
- [ ] Migrate user accounts
- [ ] Migrate organization data
- [ ] Migrate student records
- [ ] Migrate resource metadata
- [ ] Migrate session history
- [ ] Verify data integrity
- [ ] Create rollback scripts
- [ ] Test migration process
- [ ] Document migration steps

## Monitoring Setup

- [ ] Configure Azure Monitor dashboards
- [ ] Create API performance dashboard
- [ ] Create error rate dashboard
- [ ] Create business metrics dashboard
- [ ] Set up alerts for high error rate
- [ ] Set up alerts for slow response time
- [ ] Set up alerts for low disk space
- [ ] Set up alerts for high CPU usage
- [ ] Test all alerts trigger correctly
- [ ] Create runbook for alerts

## Documentation

- [ ] Write API documentation
- [ ] Write deployment guide
- [ ] Write operations manual
- [ ] Write troubleshooting guide
- [ ] Write database schema documentation
- [ ] Write architecture documentation
- [ ] Write security documentation
- [ ] Write disaster recovery plan
- [ ] Create video walkthrough
- [ ] Update README.md

## Final Testing

- [ ] Run full regression test suite
- [ ] Test all happy paths
- [ ] Test all error scenarios
- [ ] Test with different user roles
- [ ] Test on different browsers
- [ ] Test on mobile devices
- [ ] Test with slow network
- [ ] Test with high latency
- [ ] Run penetration test
- [ ] Get security sign-off

## Launch Preparation

- [ ] Create launch checklist
- [ ] Schedule maintenance window
- [ ] Notify stakeholders
- [ ] Prepare rollback plan
- [ ] Brief support team
- [ ] Update status page
- [ ] Prepare launch announcement
- [ ] Create user guides
- [ ] Set up user training
- [ ] Final go/no-go decision

## Production Deployment

- [ ] Execute deployment runbook
- [ ] Deploy database changes
- [ ] Deploy application code
- [ ] Run smoke tests
- [ ] Verify all services healthy
- [ ] Enable production traffic
- [ ] Monitor error rates
- [ ] Monitor performance metrics
- [ ] Watch for any issues
- [ ] Celebrate successful launch! 🎉