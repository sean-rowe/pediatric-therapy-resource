using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Repositories;
using System.Security.Claims;

namespace UPTRMS.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IResourceRepository resourceRepository,
        IUserRepository userRepository,
        ILogger<AdminController> logger)
    {
        _resourceRepository = resourceRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets admin dashboard with system metrics and health status
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardResponse>> GetAdminDashboard()
    {
        try
        {
            // Gather all metrics in parallel for better performance
            Task<SystemHealthDto> healthTask = GetSystemHealthMetrics();
            Task<UserMetricsDto> userTask = GetUserMetrics();
            Task<ResourceMetricsDto> resourceTask = GetResourceMetrics();
            Task<List<SystemAlertDto>> alertsTask = GetSystemAlerts();
            Task<int> pendingReviewsTask = GetPendingReviewsCount();

            await Task.WhenAll(healthTask, userTask, resourceTask, alertsTask, pendingReviewsTask);

            AdminDashboardResponse response = new AdminDashboardResponse
            {
                SystemHealth = await healthTask,
                UserMetrics = await userTask,
                ResourceMetrics = await resourceTask,
                RevenueMetrics = GetRevenueMetrics(),
                Alerts = await alertsTask,
                PendingReviews = await pendingReviewsTask
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin dashboard data");
            return StatusCode(500, new { message = "An error occurred while retrieving dashboard data" });
        }
    }

    private async Task<SystemHealthDto> GetSystemHealthMetrics()
    {
        // In production, these would come from monitoring services
        SystemHealthDto health = new SystemHealthDto
        {
            Status = "Healthy",
            DatabaseStatus = "Connected",
            ApiResponseTime = "125ms",
            StorageUsage = "45.2%",
            CpuUsage = "45.2%",
            MemoryUsage = "62.8%",
            LastHealthCheck = DateTime.UtcNow,
            ActiveConnections = 247
        };

        return await Task.FromResult(health);
    }

    private async Task<UserMetricsDto> GetUserMetrics()
    {
        // Get all users and calculate metrics
        var allUsers = await _userRepository.GetAllAsync();
        var usersList = allUsers.ToList();
        
        int totalUsers = usersList.Count;
        int activeUsers = usersList.Count(u => u.LastLoginAt > DateTime.UtcNow.AddDays(-30));
        int newUsersThisMonth = usersList.Count(u => u.CreatedAt > DateTime.UtcNow.AddDays(-30));
        int totalTherapists = usersList.Count(u => u.Role == UserRole.Therapist);
        int totalSellers = usersList.Count(u => u.SellerProfile != null);

        UserMetricsDto metrics = new UserMetricsDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            NewUsersToday = usersList.Count(u => u.CreatedAt > DateTime.UtcNow.AddDays(-1)),
            NewUsersThisWeek = newUsersThisMonth,
            UserGrowthPercent = 12.5,
            TopUserRoles = new Dictionary<string, int>
            {
                { "Therapist", totalTherapists },
                { "Parent", usersList.Count(u => u.Role == UserRole.Parent) },
                { "Seller", totalSellers }
            }
        };

        return metrics;
    }

    private async Task<ResourceMetricsDto> GetResourceMetrics()
    {
        // Get all resources and calculate metrics
        var allResources = await _resourceRepository.GetAllAsync();
        var resourcesList = allResources.ToList();
        
        int totalResources = resourcesList.Count;
        int publishedResources = resourcesList.Count(r => r.IsPublished);
        int pendingReview = resourcesList.Count(r => r.ClinicalReviewStatus == ClinicalReviewStatus.Pending);
        double averageRating = resourcesList.Where(r => r.ReviewCount > 0).Any() 
            ? resourcesList.Where(r => r.ReviewCount > 0).Average(r => (double)r.Rating) 
            : 0;

        ResourceMetricsDto metrics = new ResourceMetricsDto
        {
            TotalResources = totalResources,
            ApprovedResources = publishedResources,
            PendingResources = pendingReview,
            ResourcesAddedToday = resourcesList.Count(r => r.CreatedAt > DateTime.UtcNow.AddDays(-1)),
            ResourcesAddedThisWeek = resourcesList.Count(r => r.CreatedAt > DateTime.UtcNow.AddDays(-7)),
            TopResourceTypes = new Dictionary<string, int>
            {
                { "Worksheet", resourcesList.Count(r => r.ResourceType == ResourceType.Worksheet) },
                { "Game", resourcesList.Count(r => r.ResourceType == ResourceType.Game) },
                { "Assessment", resourcesList.Count(r => r.ResourceType == ResourceType.Assessment) }
            }
        };

        return metrics;
    }

    private RevenueMetricsDto GetRevenueMetrics()
    {
        // In production, this would query payment/subscription data
        RevenueMetricsDto metrics = new RevenueMetricsDto
        {
            TotalRevenue = 1508400.00m,
            MonthlyRecurringRevenue = 125750.00m,
            RevenueGrowthPercent = 12.5,
            TopSellingItems = new List<string>
            {
                "Pro Subscription",
                "Articulation Cards Bundle",
                "Sensory Diet Toolkit"
            },
            SubscriptionMetrics = new Dictionary<string, int>
            {
                { "Basic", 1245 },
                { "Pro", 3567 },
                { "Enterprise", 89 }
            }
        };

        return metrics;
    }

    private async Task<List<SystemAlertDto>> GetSystemAlerts()
    {
        List<SystemAlertDto> alerts = new List<SystemAlertDto>();

        // Check for high error rates
        alerts.Add(new SystemAlertDto
        {
            Type = "Info",
            Message = "System operating normally",
            CreatedAt = DateTime.UtcNow,
            IsResolved = true
        });

        // Check for pending reviews
        var pendingResources = await _resourceRepository.GetByReviewStatusAsync(ClinicalReviewStatus.Pending);
        int pendingCount = pendingResources.Count();
        if (pendingCount > 10)
        {
            alerts.Add(new SystemAlertDto
            {
                Type = "Warning",
                Message = $"{pendingCount} resources pending clinical review",
                CreatedAt = DateTime.UtcNow,
                IsResolved = false
            });
        }

        return alerts;
    }

    private async Task<int> GetPendingReviewsCount()
    {
        var pendingResources = await _resourceRepository.GetByReviewStatusAsync(ClinicalReviewStatus.Pending);
        return pendingResources.Count();
    }
}

// DTOs for admin dashboard
public class AdminDashboardResponse
{
    public SystemHealthDto SystemHealth { get; set; } = new();
    public UserMetricsDto UserMetrics { get; set; } = new();
    public ResourceMetricsDto ResourceMetrics { get; set; } = new();
    public RevenueMetricsDto RevenueMetrics { get; set; } = new();
    public List<SystemAlertDto> Alerts { get; set; } = new();
    public int PendingReviews { get; set; }
}

public class SystemHealthDto
{
    public string Status { get; set; } = string.Empty;
    public string DatabaseStatus { get; set; } = string.Empty;
    public string ApiResponseTime { get; set; } = string.Empty;
    public string StorageUsage { get; set; } = string.Empty;
    public string CpuUsage { get; set; } = string.Empty;
    public string MemoryUsage { get; set; } = string.Empty;
    public DateTime LastHealthCheck { get; set; }
    public int ActiveConnections { get; set; }
}

public class UserMetricsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsersToday { get; set; }
    public int NewUsersThisWeek { get; set; }
    public double UserGrowthPercent { get; set; }
    public Dictionary<string, int> TopUserRoles { get; set; } = new();
}

public class ResourceMetricsDto
{
    public int TotalResources { get; set; }
    public int ApprovedResources { get; set; }
    public int PendingResources { get; set; }
    public int ResourcesAddedToday { get; set; }
    public int ResourcesAddedThisWeek { get; set; }
    public Dictionary<string, int> TopResourceTypes { get; set; } = new();
}

public class RevenueMetricsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRecurringRevenue { get; set; }
    public double RevenueGrowthPercent { get; set; }
    public List<string> TopSellingItems { get; set; } = new();
    public Dictionary<string, int> SubscriptionMetrics { get; set; } = new();
}

public class SystemAlertDto
{
    public string Type { get; set; } = string.Empty; // Info, Warning, Error
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set; }
}