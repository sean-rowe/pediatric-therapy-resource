using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Models.DTOs;
using UPTRMS.Api.Repositories;

namespace UPTRMS.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserRepository userRepository, ILogger<UsersController> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        throw new NotImplementedException("GetProfile endpoint not yet implemented");
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        throw new NotImplementedException("UpdateProfile endpoint not yet implemented");
    }

    [HttpPut("language")]
    public async Task<IActionResult> UpdateLanguage([FromBody] UpdateLanguageRequest request)
    {
        throw new NotImplementedException("UpdateLanguage endpoint not yet implemented");
    }

    [HttpGet("subscription")]
    public async Task<ActionResult<SubscriptionDto>> GetSubscription()
    {
        throw new NotImplementedException("GetSubscription endpoint not yet implemented");
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        throw new NotImplementedException("GetUser endpoint not yet implemented");
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<List<UserDto>>> SearchUsers([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        throw new NotImplementedException("SearchUsers endpoint not yet implemented");
    }

    private UserDto MapToDto(User user)
    {
        throw new NotImplementedException("MapToDto not yet implemented");
    }
}

public class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<string> Languages { get; set; } = new();
    public List<string> Specialties { get; set; } = new();
}

public class UpdateLanguageRequest
{
    public string Language { get; set; } = string.Empty;
}

public class SubscriptionDto
{
    public SubscriptionTier Tier { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public OrganizationDto? Organization { get; set; }
}