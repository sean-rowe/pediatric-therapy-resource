# TODO-010: Student and Session Service Implementation

These tasks implement services for student management and therapy sessions.

## IStudentService Interface

- [ ] Create IStudentService.cs in Services folder
- [ ] Add using statements for Models.Domain
- [ ] Add using statements for Models.DTOs
- [ ] Add namespace UPTRMS.Api.Services
- [ ] Create interface IStudentService
- [ ] Add method Task<StudentDto> GetByIdAsync(Guid id)
- [ ] Add method Task<StudentDto> GetByAccessCodeAsync(string accessCode)
- [ ] Add method Task<IEnumerable<StudentDto>> GetByTherapistAsync(Guid therapistId)
- [ ] Add method Task<StudentDto> CreateAsync(StudentCreateDto createDto, Guid therapistId)
- [ ] Add method Task<StudentDto> UpdateAsync(Guid id, StudentUpdateDto updateDto, Guid userId)
- [ ] Add method Task<bool> DeleteAsync(Guid id, Guid userId)
- [ ] Add method Task<bool> AssignGoalsAsync(Guid studentId, IEnumerable<Guid> goalIds, Guid userId)
- [ ] Add method Task<IEnumerable<IEPGoalDto>> GetGoalsAsync(Guid studentId)
- [ ] Add method Task<StudentProgressDto> GetProgressAsync(Guid studentId, DateTime? startDate, DateTime? endDate)
- [ ] Add method Task<string> GenerateAccessCodeAsync(Guid studentId, Guid userId)
- [ ] Add method Task<bool> SendParentInviteAsync(Guid studentId, string parentEmail, Guid userId)
- [ ] Add method Task<IEnumerable<StudentDto>> ImportFromSISAsync(SISImportDto importData, Guid userId)
- [ ] Add method Task<Stream> ExportStudentsAsync(Guid therapistId, ExportFormat format)
- [ ] Add method Task<bool> TransferStudentAsync(Guid studentId, Guid newTherapistId, Guid userId)
- [ ] Add method Task<CaseloadSummaryDto> GetCaseloadSummaryAsync(Guid therapistId)

## StudentService Implementation - Setup

- [ ] Create StudentService.cs in Services folder
- [ ] Add using statements for all required namespaces
- [ ] Add namespace UPTRMS.Api.Services
- [ ] Create class StudentService
- [ ] Implement IStudentService interface
- [ ] Add private readonly IStudentRepository _studentRepository field
- [ ] Add private readonly IGoalRepository _goalRepository field
- [ ] Add private readonly IProgressRepository _progressRepository field
- [ ] Add private readonly IEmailService _emailService field
- [ ] Add private readonly ILogger<StudentService> _logger field
- [ ] Add private readonly IMapper _mapper field
- [ ] Add private readonly IAuditService _auditService field
- [ ] Add constructor accepting all dependencies
- [ ] Store all dependencies in fields

## StudentService Implementation - Basic CRUD

- [ ] Implement GetByIdAsync method
- [ ] Call repository GetByIdWithDetailsAsync
- [ ] If student is null, throw NotFoundException
- [ ] Check user has permission to view student
- [ ] Map entity to StudentDto using AutoMapper
- [ ] Include goals in mapping
- [ ] Include recent sessions in mapping
- [ ] Return mapped DTO
- [ ] Implement GetByAccessCodeAsync method
- [ ] Validate access code format
- [ ] Call repository GetByAccessCodeAsync
- [ ] If student is null, throw NotFoundException
- [ ] Map to limited StudentDto (parent view)
- [ ] Exclude sensitive therapist info
- [ ] Return mapped DTO
- [ ] Implement GetByTherapistAsync method
- [ ] Call repository GetByTherapistAsync
- [ ] Map all students to DTOs
- [ ] Include basic info only for list view
- [ ] Order by last name, first name
- [ ] Return student list
- [ ] Implement CreateAsync method
- [ ] Validate create DTO
- [ ] Check for duplicate students (name + DOB)
- [ ] Create Student entity
- [ ] Set TherapistId to provided value
- [ ] Generate unique access code
- [ ] Call repository GenerateUniqueAccessCodeAsync
- [ ] Set access code on student
- [ ] Set CreatedAt to current UTC time
- [ ] Set IsActive to true
- [ ] Call repository AddAsync
- [ ] Call SaveChangesAsync
- [ ] Create audit log entry
- [ ] Send welcome email if parent email provided
- [ ] Map created entity to DTO
- [ ] Return created student DTO
- [ ] Implement UpdateAsync method
- [ ] Get existing student by id
- [ ] If not found, throw NotFoundException
- [ ] Check user has permission to update
- [ ] Update allowed properties from DTO
- [ ] Don't allow changing TherapistId here
- [ ] Don't allow changing AccessCode
- [ ] Set UpdatedAt to current UTC time
- [ ] Call repository Update
- [ ] Call SaveChangesAsync
- [ ] Create audit log entry
- [ ] Map updated entity to DTO
- [ ] Return updated student DTO
- [ ] Implement DeleteAsync method
- [ ] Get existing student by id
- [ ] If not found, return false
- [ ] Check user has permission to delete
- [ ] Set IsActive to false (soft delete)
- [ ] Set UpdatedAt to current UTC time
- [ ] Call repository Update
- [ ] Call SaveChangesAsync
- [ ] Create audit log entry
- [ ] Return true

## StudentService Implementation - Goal Management

- [ ] Implement AssignGoalsAsync method
- [ ] Get student by id
- [ ] If not found, throw NotFoundException
- [ ] Check user has permission to assign goals
- [ ] Get existing goal assignments
- [ ] Calculate goals to add (new ones)
- [ ] Calculate goals to remove (deselected)
- [ ] For each goal to add
- [ ] Verify goal exists
- [ ] Create goal assignment
- [ ] For each goal to remove
- [ ] Delete goal assignment
- [ ] Save all changes
- [ ] Create audit log entry
- [ ] Return true
- [ ] Implement GetGoalsAsync method
- [ ] Get student with goals included
- [ ] If not found, throw NotFoundException
- [ ] Filter active goals only
- [ ] Map goals to DTOs
- [ ] Include progress data
- [ ] Order by target date
- [ ] Return goal list

## StudentService Implementation - Progress Tracking

- [ ] Implement GetProgressAsync method
- [ ] Get student by id
- [ ] If not found, throw NotFoundException
- [ ] Set default date range if not provided
- [ ] Default to last 90 days
- [ ] Get all progress data in range
- [ ] Group by goal
- [ ] Calculate progress percentage per goal
- [ ] Get baseline vs current for each goal
- [ ] Calculate rate of progress
- [ ] Get session attendance rate
- [ ] Get resource usage statistics
- [ ] Create StudentProgressDto
- [ ] Set StudentId and Name
- [ ] Set DateRange
- [ ] Set GoalProgress collection
- [ ] Set AttendanceRate
- [ ] Set TotalSessions
- [ ] Set ResourcesUsed count
- [ ] Return progress DTO

## StudentService Implementation - Access Management

- [ ] Implement GenerateAccessCodeAsync method
- [ ] Get student by id
- [ ] If not found, throw NotFoundException
- [ ] Check user has permission
- [ ] Generate new unique code
- [ ] Update student access code
- [ ] Save changes
- [ ] Create audit log entry
- [ ] Send notification to parent if email exists
- [ ] Return new access code
- [ ] Implement SendParentInviteAsync method
- [ ] Get student by id
- [ ] If not found, throw NotFoundException
- [ ] Validate parent email format
- [ ] Update student parent email
- [ ] Create email content
- [ ] Include access code
- [ ] Include platform URL
- [ ] Include instructions
- [ ] Send email via email service
- [ ] Log email sent
- [ ] Return true

## StudentService Implementation - Import/Export

- [ ] Implement ImportFromSISAsync method
- [ ] Validate import data format
- [ ] Check user has permission to import
- [ ] Create collection for results
- [ ] For each student in import
- [ ] Validate required fields
- [ ] Check for existing student
- [ ] Map SIS fields to student entity
- [ ] Generate access code
- [ ] Add to database
- [ ] Add to results collection
- [ ] Save all changes in batch
- [ ] Create audit log for import
- [ ] Return created students
- [ ] Implement ExportStudentsAsync method
- [ ] Get all students for therapist
- [ ] Create memory stream
- [ ] Based on format parameter
- [ ] If CSV, create CSV writer
- [ ] Write headers
- [ ] Write student data rows
- [ ] If Excel, create workbook
- [ ] Add student worksheet
- [ ] Format columns
- [ ] If PDF, create document
- [ ] Add student table
- [ ] Reset stream position
- [ ] Return stream

## ISessionService Interface

- [ ] Create ISessionService.cs in Services folder
- [ ] Add using statements for Models.Domain
- [ ] Add using statements for Models.DTOs
- [ ] Add namespace UPTRMS.Api.Services
- [ ] Create interface ISessionService
- [ ] Add method Task<SessionDto> GetByIdAsync(Guid id)
- [ ] Add method Task<IEnumerable<SessionDto>> GetUpcomingAsync(Guid therapistId, int days = 7)
- [ ] Add method Task<SessionDto> CreateAsync(SessionCreateDto createDto, Guid therapistId)
- [ ] Add method Task<SessionDto> UpdateAsync(Guid id, SessionUpdateDto updateDto, Guid userId)
- [ ] Add method Task<bool> CancelAsync(Guid id, string reason, Guid userId)
- [ ] Add method Task<SessionDto> StartSessionAsync(Guid id, Guid userId)
- [ ] Add method Task<SessionDto> EndSessionAsync(Guid id, SessionEndDto endData, Guid userId)
- [ ] Add method Task<bool> RecordProgressAsync(Guid sessionId, ProgressDataDto progressData, Guid userId)
- [ ] Add method Task<SessionReportDto> GetSessionReportAsync(Guid id, Guid userId)
- [ ] Add method Task<bool> CheckScheduleConflictAsync(Guid therapistId, DateTime scheduledAt, int duration, Guid? excludeId)
- [ ] Add method Task<IEnumerable<SessionDto>> GetByDateRangeAsync(DateTime start, DateTime end, Guid? therapistId)
- [ ] Add method Task<SessionStatisticsDto> GetStatisticsAsync(Guid therapistId, DateTime start, DateTime end)
- [ ] Add method Task<bool> AddResourcesAsync(Guid sessionId, IEnumerable<Guid> resourceIds, Guid userId)
- [ ] Add method Task<bool> RecordSignatureAsync(Guid sessionId, string signatureData, Guid userId)

## SessionService Implementation - Setup

- [ ] Create SessionService.cs in Services folder
- [ ] Add using statements for all required namespaces
- [ ] Add namespace UPTRMS.Api.Services
- [ ] Create class SessionService
- [ ] Implement ISessionService interface
- [ ] Add private readonly ISessionRepository _sessionRepository field
- [ ] Add private readonly IStudentRepository _studentRepository field
- [ ] Add private readonly IResourceRepository _resourceRepository field
- [ ] Add private readonly IProgressRepository _progressRepository field
- [ ] Add private readonly INotificationService _notificationService field
- [ ] Add private readonly ILogger<SessionService> _logger field
- [ ] Add private readonly IMapper _mapper field
- [ ] Add constructor accepting all dependencies
- [ ] Store all dependencies in fields

## SessionService Implementation - Session Lifecycle

- [ ] Implement CreateAsync method
- [ ] Validate session data
- [ ] Check student exists and is active
- [ ] Check for schedule conflicts
- [ ] Create Session entity
- [ ] Set TherapistId
- [ ] Set all properties from DTO
- [ ] Set SessionStatus to "Scheduled"
- [ ] Set CreatedAt
- [ ] Add to database
- [ ] Send confirmation email
- [ ] Create calendar invite
- [ ] Return mapped DTO
- [ ] Implement StartSessionAsync method
- [ ] Get session by id
- [ ] Verify session is scheduled
- [ ] Verify user is the therapist
- [ ] Set StartedAt to now
- [ ] Set SessionStatus to "InProgress"
- [ ] Save changes
- [ ] Return updated DTO
- [ ] Implement EndSessionAsync method
- [ ] Get session by id
- [ ] Verify session is in progress
- [ ] Set EndedAt to now
- [ ] Set SessionStatus to "Completed"
- [ ] Update session notes
- [ ] Calculate actual duration
- [ ] Save changes
- [ ] Return updated DTO