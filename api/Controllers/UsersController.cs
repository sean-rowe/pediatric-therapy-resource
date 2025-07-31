using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Models.DTOs;
using UPTRMS.Api.Repositories;
using UPTRMS.Api.Services;

namespace UPTRMS.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UsersController> _logger;
    private readonly IEmailService _emailService;

    public UsersController(
        IUserRepository userRepository,
        ILogger<UsersController> logger,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _logger = logger;
        _emailService = emailService;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
            return NotFound();

        return Ok(MapToDto(user));
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
            return NotFound();

        // Update only non-null values
        if (!string.IsNullOrEmpty(request.FirstName))
            user.FirstName = request.FirstName;

        if (!string.IsNullOrEmpty(request.LastName))
            user.LastName = request.LastName;

        if (request.Languages != null && request.Languages.Any())
            user.Languages = request.Languages;

        if (request.Specialties != null && request.Specialties.Any())
            user.Specialties = request.Specialties;

        await _userRepository.UpdateAsync(user);

        return Ok(MapToDto(user));
    }

    [HttpPut("language")]
    public async Task<IActionResult> UpdateLanguage([FromBody] UpdateLanguageRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
            return NotFound();

        user.PreferredLanguage = request.Language;
        await _userRepository.UpdateAsync(user);

        return Ok(new { message = "Language preference updated successfully" });
    }

    [HttpGet("subscription")]
    public async Task<ActionResult<SubscriptionDto>> GetSubscription()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var user = await _userRepository.GetByIdWithOrganizationAsync(Guid.Parse(userId));
        if (user == null)
            return NotFound();

        var subscriptionDto = new SubscriptionDto
        {
            Tier = user.SubscriptionTier,
            Status = user.SubscriptionStatus,
            StartDate = user.SubscriptionStartDate,
            EndDate = user.SubscriptionEndDate
        };

        if (user.Organization != null)
        {
            subscriptionDto.Organization = new OrganizationDto
            {
                OrganizationId = user.Organization.OrganizationId,
                Name = user.Organization.Name,
                Type = user.Organization.Type,
                SubscriptionTier = user.Organization.SubscriptionTier,
                LicenseCount = user.Organization.LicenseCount,
                UsedLicenses = user.Organization.UsedLicenses,
                SsoEnabled = user.Organization.SsoEnabled
            };
        }

        return Ok(subscriptionDto);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(MapToDto(user));
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<List<UserDto>>> SearchUsers([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var skip = (page - 1) * pageSize;

        IEnumerable<User> users;
        if (!string.IsNullOrEmpty(search))
        {
            users = await _userRepository.SearchUsersAsync(search, skip, pageSize);
        }
        else
        {
            var allUsers = await _userRepository.GetAllAsync();
            users = allUsers.Skip(skip).Take(pageSize);
        }

        var userDtos = users.Select(MapToDto).ToList();
        return Ok(userDtos);
    }

    [HttpPut("{id}/status")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        // Update user status based on the request
        if (request.Status?.ToLower() == "suspended")
        {
            user.IsActive = false;
            // Log the reason (in a real app, you'd store this in an audit table)
            _logger.LogInformation("User {UserId} suspended. Reason: {Reason}", id, request.Reason);
        }
        else if (request.Status?.ToLower() == "active")
        {
            user.IsActive = true;
            _logger.LogInformation("User {UserId} reactivated", id);
        }
        else
        {
            return BadRequest("Invalid status. Use 'suspended' or 'active'.");
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Send notification
        await _emailService.SendAccountStatusChangeAsync(
            user.Email,
            user.FirstName,
            request.Status?.ToLower() == "suspended",
            request.Reason
        );

        return Ok(new { message = $"User status updated to {request.Status}" });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        // Store email before deletion for notification
        var email = user.Email;
        var firstName = user.FirstName;

        // Soft delete - mark as deleted instead of physical deletion
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        // Anonymize personal data for GDPR compliance
        user.Email = $"deleted_{user.UserId}@deleted.com";
        user.FirstName = "DELETED";
        user.LastName = "USER";
        user.LicenseNumber = "DELETED";
        user.LicenseState = "XX";
        user.PasswordHash = "DELETED";
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;

        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("User {UserId} marked as deleted and anonymized", id);

        // Send deletion confirmation email
        await _emailService.SendAccountDeletionConfirmationAsync(email, firstName);

        return Ok(new { message = "User account deleted successfully" });
    }

    /// <summary>
    /// Delete user profile (self-service)
    /// </summary>
    [HttpDelete("profile")]
    public async Task<IActionResult> DeleteProfile([FromBody] DeleteProfileRequest request)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        // Verify password
        User? user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
            return NotFound();

        // In production, you would verify the password hash
        if (request.Confirm != "DELETE")
            return BadRequest(new { message = "Please confirm deletion by typing DELETE" });

        // Store email before deletion for notification
        string email = user.Email;
        string firstName = user.FirstName;

        // Soft delete - mark as deleted instead of physical deletion
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        // Anonymize personal data for GDPR compliance
        user.Email = $"deleted_{user.UserId}@deleted.com";
        user.FirstName = "DELETED";
        user.LastName = "USER";
        user.LicenseNumber = "DELETED";
        user.LicenseState = "XX";
        user.PasswordHash = "DELETED";
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;

        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("User {UserId} self-deleted their account", user.UserId);

        // Send deletion confirmation email
        await _emailService.SendAccountDeletionConfirmationAsync(email, firstName);

        return Ok(new { message = "Account deleted successfully" });
    }

    /// <summary>
    /// Get user's professional licenses
    /// </summary>
    [HttpGet("licenses")]
    public async Task<ActionResult<List<LicenseDto>>> GetLicenses()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        User? user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
            return NotFound();

        // In a real implementation, licenses would be stored in a separate table
        List<LicenseDto> licenses = new List<LicenseDto>();
        
        if (!string.IsNullOrEmpty(user.LicenseNumber))
        {
            licenses.Add(new LicenseDto
            {
                LicenseNumber = user.LicenseNumber,
                LicenseState = user.LicenseState ?? "Unknown",
                LicenseType = user.LicenseType ?? "Unknown",
                Verified = user.LicenseVerified,
                VerifiedAt = user.LicenseVerifiedAt,
                ExpirationDate = user.LicenseExpirationDate
            });
        }

        return Ok(licenses);
    }

    /// <summary>
    /// Add and verify a new license
    /// </summary>
    [HttpPost("licenses/verify")]
    public async Task<ActionResult<LicenseDto>> VerifyLicense([FromBody] VerifyLicenseRequest request)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        User? user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
            return NotFound();

        // In production, this would call an external license verification API
        // For now, simulate verification
        bool isValid = !string.IsNullOrEmpty(request.LicenseNumber) && 
                      request.LicenseNumber.Length >= 5 &&
                      !string.IsNullOrEmpty(request.LicenseState) &&
                      request.LicenseState.Length == 2;

        if (!isValid)
        {
            return BadRequest(new { message = "License verification failed" });
        }

        // Update user's license information
        user.LicenseNumber = request.LicenseNumber;
        user.LicenseState = request.LicenseState;
        user.LicenseType = request.LicenseType;
        user.LicenseVerified = true;
        user.LicenseVerifiedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        return Ok(new LicenseDto
        {
            LicenseNumber = request.LicenseNumber,
            LicenseState = request.LicenseState,
            LicenseType = request.LicenseType,
            Verified = true,
            VerifiedAt = DateTime.UtcNow,
            ExpirationDate = null // Would be set from verification API
        });
    }

    /// <summary>
    /// Get user preferences
    /// </summary>
    [HttpGet("preferences")]
    public async Task<ActionResult<UserPreferencesDto>> GetPreferences()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        User? user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
            return NotFound();

        return Ok(new UserPreferencesDto
        {
            Language = user.PreferredLanguage ?? "en",
            Timezone = user.Timezone ?? "UTC",
            EmailNotifications = user.EmailNotificationsEnabled,
            Theme = user.Theme ?? "light",
            DefaultView = user.DefaultView ?? "dashboard"
        });
    }

    /// <summary>
    /// Update user preferences
    /// </summary>
    [HttpPut("preferences")]
    public async Task<ActionResult<UserPreferencesDto>> UpdatePreferences([FromBody] UpdatePreferencesRequest request)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        User? user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
            return NotFound();

        // Update preferences
        if (!string.IsNullOrEmpty(request.Language))
            user.PreferredLanguage = request.Language;
        if (!string.IsNullOrEmpty(request.Timezone))
            user.Timezone = request.Timezone;
        if (request.EmailNotifications.HasValue)
            user.EmailNotificationsEnabled = request.EmailNotifications.Value;
        if (!string.IsNullOrEmpty(request.Theme))
            user.Theme = request.Theme;
        if (!string.IsNullOrEmpty(request.DefaultView))
            user.DefaultView = request.DefaultView;

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return Ok(new UserPreferencesDto
        {
            Language = user.PreferredLanguage ?? "en",
            Timezone = user.Timezone ?? "UTC",
            EmailNotifications = user.EmailNotificationsEnabled,
            Theme = user.Theme ?? "light",
            DefaultView = user.DefaultView ?? "dashboard"
        });
    }

    /// <summary>
    /// Get notification settings
    /// </summary>
    [HttpGet("notifications")]
    public async Task<ActionResult<NotificationSettingsDto>> GetNotificationSettings()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        User? user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
            return NotFound();

        // In production, this would come from a NotificationSettings table
        return Ok(new NotificationSettingsDto
        {
            EmailNotifications = new EmailNotificationSettings
            {
                NewResources = user.EmailNotificationsEnabled,
                WeeklyDigest = user.EmailNotificationsEnabled,
                MarketingEmails = false
            },
            PushNotifications = new PushNotificationSettings
            {
                SessionReminders = true,
                NewMessages = true,
                SystemAlerts = true
            },
            SmsNotifications = new SmsNotificationSettings
            {
                Enabled = false,
                PhoneNumber = null
            },
            NotificationSchedule = new NotificationSchedule
            {
                QuietHours = "22:00-07:00",
                Timezone = user.Timezone ?? "UTC"
            }
        });
    }

    /// <summary>
    /// Update notification settings
    /// </summary>
    [HttpPut("notifications")]
    public async Task<ActionResult<NotificationSettingsDto>> UpdateNotificationSettings([FromBody] UpdateNotificationSettingsRequest request)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        User? user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null)
            return NotFound();

        // Update basic email notification setting
        if (request.EmailNotifications != null)
        {
            user.EmailNotificationsEnabled = request.EmailNotifications.NewResources;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Return updated settings
        return await GetNotificationSettings();
    }

    /// <summary>
    /// List users with pagination (Admin only)
    /// </summary>
    [HttpGet("list")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<PagedResult<UserDto>>> ListUsers(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? licenseType = null,
        [FromQuery] bool? verified = null)
    {
        if (page < 1) page = 1;
        if (limit < 1) limit = 20;
        if (limit > 100) limit = 100;

        List<User> allUsers = (await _userRepository.GetAllAsync()).ToList();
        
        // Apply filters
        IEnumerable<User> filteredUsers = allUsers;
        
        if (!string.IsNullOrEmpty(licenseType))
        {
            filteredUsers = filteredUsers.Where(u => u.LicenseType == licenseType);
        }
        
        if (verified.HasValue)
        {
            filteredUsers = filteredUsers.Where(u => u.EmailVerified == verified.Value);
        }

        int totalCount = filteredUsers.Count();
        List<User> pagedUsers = filteredUsers
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToList();

        return Ok(new PagedResult<UserDto>
        {
            Users = pagedUsers.Select(MapToDto).ToList(),
            Total = totalCount,
            Page = page,
            Limit = limit,
            TotalPages = (int)Math.Ceiling(totalCount / (double)limit)
        });
    }

    private UserDto MapToDto(User user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            LicenseNumber = user.LicenseNumber,
            SubscriptionTier = user.SubscriptionTier,
            Role = user.Role,
            Languages = user.Languages ?? new List<string>(),
            Specialties = user.Specialties ?? new List<string>(),
            IsSellerApproved = user.IsSellerApproved,
            EmailVerified = user.EmailVerified,
            TwoFactorEnabled = user.TwoFactorEnabled,
            PreferredLanguage = user.PreferredLanguage ?? "en"
        };
    }
}

public class UpdateLanguageRequest
{
    public string Language { get; set; } = string.Empty;
}

public class UpdateUserStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class SubscriptionDto
{
    public SubscriptionTier Tier { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public OrganizationDto? Organization { get; set; }
}

public class DeleteProfileRequest
{
    public string Password { get; set; } = string.Empty;
    public string Confirm { get; set; } = string.Empty; // Must be "DELETE"
}

public class LicenseDto
{
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseState { get; set; } = string.Empty;
    public string LicenseType { get; set; } = string.Empty;
    public DateTime? ExpirationDate { get; set; }
    public bool Verified { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

public class VerifyLicenseRequest
{
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseState { get; set; } = string.Empty;
    public string LicenseType { get; set; } = string.Empty;
}

public class UserPreferencesDto
{
    public string Language { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public bool EmailNotifications { get; set; }
    public string Theme { get; set; } = string.Empty;
    public string DefaultView { get; set; } = string.Empty;
}

public class UpdatePreferencesRequest
{
    public string? Language { get; set; }
    public string? Timezone { get; set; }
    public bool? EmailNotifications { get; set; }
    public string? Theme { get; set; }
    public string? DefaultView { get; set; }
}

public class NotificationSettingsDto
{
    public EmailNotificationSettings EmailNotifications { get; set; } = new();
    public PushNotificationSettings PushNotifications { get; set; } = new();
    public SmsNotificationSettings SmsNotifications { get; set; } = new();
    public NotificationSchedule NotificationSchedule { get; set; } = new();
}

public class EmailNotificationSettings
{
    public bool NewResources { get; set; }
    public bool WeeklyDigest { get; set; }
    public bool MarketingEmails { get; set; }
}

public class PushNotificationSettings
{
    public bool SessionReminders { get; set; }
    public bool NewMessages { get; set; }
    public bool SystemAlerts { get; set; }
}

public class SmsNotificationSettings
{
    public bool Enabled { get; set; }
    public string? PhoneNumber { get; set; }
}

public class NotificationSchedule
{
    public string QuietHours { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
}

public class UpdateNotificationSettingsRequest
{
    public EmailNotificationSettings? EmailNotifications { get; set; }
    public PushNotificationSettings? PushNotifications { get; set; }
    public SmsNotificationSettings? SmsNotifications { get; set; }
    public NotificationSchedule? NotificationSchedule { get; set; }
}

public class PagedResult<T>
{
    public List<T> Users { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalPages { get; set; }
}