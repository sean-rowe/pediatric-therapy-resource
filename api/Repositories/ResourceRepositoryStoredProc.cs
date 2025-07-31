using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;
using UPTRMS.Api.Data;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Models.DTOs;
using Newtonsoft.Json;

namespace UPTRMS.Api.Repositories;

/// <summary>
/// Resource repository implementation using stored procedures
/// </summary>
public class ResourceRepositoryStoredProc : IResourceRepository
{
    private readonly ApplicationDbContext _context;
    private readonly string _connectionString;

    public ResourceRepositoryStoredProc(ApplicationDbContext context)
    {
        _context = context;
        _connectionString = context.Database.GetConnectionString() ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<Resource?> GetByIdAsync(Guid id)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetResourceById", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@ResourceId", id);
        command.Parameters.AddWithValue("@IncludeDeleted", false);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return MapResourceFromReader(reader);
        }
        
        return null;
    }

    public async Task<IEnumerable<Resource>> GetAllAsync()
    {
        return await GetAllAsync(0, int.MaxValue);
    }

    public async Task<IEnumerable<Resource>> GetAllAsync(int skip, int take)
    {
        List<Resource> resources = new List<Resource>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetResourcesByFilter", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@PageNumber", (skip / take) + 1);
        command.Parameters.AddWithValue("@PageSize", take);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            resources.Add(MapResourceFromReader(reader));
        }
        
        return resources;
    }

    public async Task<Resource> AddAsync(Resource entity)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_CreateResource", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@Title", entity.Title);
        command.Parameters.AddWithValue("@Description", (object?)entity.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@ResourceType", (int)entity.ResourceType);
        command.Parameters.AddWithValue("@FileUrl", (object?)entity.FileUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("@ThumbnailUrl", (object?)entity.ThumbnailUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("@SkillAreas", JsonConvert.SerializeObject(entity.SkillAreas));
        command.Parameters.AddWithValue("@GradeLevels", JsonConvert.SerializeObject(entity.GradeLevels));
        command.Parameters.AddWithValue("@Languages", JsonConvert.SerializeObject(entity.Languages));
        command.Parameters.AddWithValue("@IsInteractive", entity.IsInteractive);
        command.Parameters.AddWithValue("@HasAudio", entity.HasAudio);
        command.Parameters.AddWithValue("@GenerationMethod", (int)entity.GenerationMethod);
        command.Parameters.AddWithValue("@AiGenerationMetadata", (object?)entity.AiGenerationMetadata ?? DBNull.Value);
        command.Parameters.AddWithValue("@ClinicalReviewStatus", (int)entity.ClinicalReviewStatus);
        command.Parameters.AddWithValue("@EvidenceLevel", entity.EvidenceLevel);
        command.Parameters.AddWithValue("@CreatedByUserId", entity.CreatedByUserId);
        command.Parameters.AddWithValue("@Price", entity.Price);
        command.Parameters.AddWithValue("@IsFree", entity.IsFree);
        command.Parameters.AddWithValue("@SellerUserId", (object?)entity.SellerUserId ?? DBNull.Value);
        command.Parameters.AddWithValue("@CommissionRate", entity.CommissionRate);
        
        SqlParameter resourceIdParam = new SqlParameter("@ResourceId", SqlDbType.UniqueIdentifier);
        resourceIdParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(resourceIdParam);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
        
        entity.ResourceId = (Guid)resourceIdParam.Value;
        return entity;
    }

    public async Task UpdateAsync(Resource entity)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_UpdateResource", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@ResourceId", entity.ResourceId);
        command.Parameters.AddWithValue("@Title", entity.Title);
        command.Parameters.AddWithValue("@Description", (object?)entity.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@FileUrl", (object?)entity.FileUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("@ThumbnailUrl", (object?)entity.ThumbnailUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("@SkillAreas", JsonConvert.SerializeObject(entity.SkillAreas));
        command.Parameters.AddWithValue("@GradeLevels", JsonConvert.SerializeObject(entity.GradeLevels));
        command.Parameters.AddWithValue("@Languages", JsonConvert.SerializeObject(entity.Languages));
        command.Parameters.AddWithValue("@IsInteractive", entity.IsInteractive);
        command.Parameters.AddWithValue("@HasAudio", entity.HasAudio);
        command.Parameters.AddWithValue("@ClinicalReviewStatus", (int)entity.ClinicalReviewStatus);
        command.Parameters.AddWithValue("@EvidenceLevel", entity.EvidenceLevel);
        command.Parameters.AddWithValue("@Price", entity.Price);
        command.Parameters.AddWithValue("@IsFree", entity.IsFree);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_SoftDeleteResource", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@ResourceId", id);
        command.Parameters.AddWithValue("@DeletedByUserId", DBNull.Value);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<Resource>> GetByReviewStatusAsync(ClinicalReviewStatus status)
    {
        List<Resource> resources = new List<Resource>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetResourcesByFilter", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@ClinicalReviewStatus", (int)status);
        command.Parameters.AddWithValue("@PageNumber", 1);
        command.Parameters.AddWithValue("@PageSize", int.MaxValue);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            resources.Add(MapResourceFromReader(reader));
        }
        
        return resources;
    }

    public async Task<IEnumerable<Resource>> GetFreeResourcesAsync(int take = 10)
    {
        List<Resource> resources = new List<Resource>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetResourcesByFilter", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@IsFree", true);
        command.Parameters.AddWithValue("@PageNumber", 1);
        command.Parameters.AddWithValue("@PageSize", take);
        command.Parameters.AddWithValue("@SortBy", "CreatedAt");
        command.Parameters.AddWithValue("@SortDirection", "DESC");
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            resources.Add(MapResourceFromReader(reader));
        }
        
        return resources;
    }

    public async Task<IEnumerable<Resource>> GetMarketplaceResourcesAsync(int skip = 0, int take = 20)
    {
        List<Resource> resources = new List<Resource>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetResourcesByFilter", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@IsFree", false);
        command.Parameters.AddWithValue("@PageNumber", (skip / take) + 1);
        command.Parameters.AddWithValue("@PageSize", take);
        command.Parameters.AddWithValue("@SortBy", "CreatedAt");
        command.Parameters.AddWithValue("@SortDirection", "DESC");
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            resources.Add(MapResourceFromReader(reader));
        }
        
        return resources;
    }

    public async Task<IEnumerable<Resource>> SearchResourcesAsync(
        string? searchTerm = null,
        List<string>? skillAreas = null,
        List<string>? gradelevels = null,
        ResourceType? resourceType = null,
        List<string>? languages = null,
        int? minEvidenceLevel = null,
        bool? isInteractive = null,
        int skip = 0,
        int take = 20)
    {
        List<Resource> resources = new List<Resource>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetResourcesByFilter", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@SearchTerm", (object?)searchTerm ?? DBNull.Value);
        
        if (resourceType.HasValue)
        {
            command.Parameters.AddWithValue("@ResourceType", (int)resourceType.Value);
        }
        
        if (minEvidenceLevel.HasValue)
        {
            command.Parameters.AddWithValue("@MinEvidenceLevel", minEvidenceLevel.Value);
        }
        
        if (isInteractive.HasValue)
        {
            command.Parameters.AddWithValue("@IsInteractive", isInteractive.Value);
        }
        
        // For now, we'll handle the first skill area and grade level
        // TODO: Update stored procedure to handle multiple values using JSON or table-valued parameters
        if (skillAreas != null && skillAreas.Count > 0)
        {
            command.Parameters.AddWithValue("@SkillArea", skillAreas[0]);
        }
        
        if (gradelevels != null && gradelevels.Count > 0 && int.TryParse(gradelevels[0], out int gradeLevel))
        {
            command.Parameters.AddWithValue("@GradeLevel", gradeLevel);
        }
        
        if (languages != null && languages.Count > 0)
        {
            command.Parameters.AddWithValue("@Language", languages[0]);
        }
        
        command.Parameters.AddWithValue("@PageNumber", (skip / take) + 1);
        command.Parameters.AddWithValue("@PageSize", take);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            resources.Add(MapResourceFromReader(reader));
        }
        
        return resources;
    }

    public async Task<IEnumerable<Resource>> GetResourcesBySeller(Guid sellerUserId, int skip = 0, int take = 20)
    {
        List<Resource> resources = new List<Resource>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetResourcesBySeller", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@SellerUserId", sellerUserId);
        command.Parameters.AddWithValue("@IncludeDeleted", false);
        command.Parameters.AddWithValue("@PageNumber", (skip / take) + 1);
        command.Parameters.AddWithValue("@PageSize", take);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            resources.Add(MapResourceFromReader(reader));
        }
        
        return resources;
    }

    public async Task<IEnumerable<Resource>> GetUserFavorites(Guid userId, int skip = 0, int take = 20)
    {
        List<Resource> resources = new List<Resource>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetUserFavoriteResources", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@PageNumber", (skip / take) + 1);
        command.Parameters.AddWithValue("@PageSize", take);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            resources.Add(MapResourceFromReader(reader));
        }
        
        return resources;
    }

    public async Task AddToFavorites(Guid userId, Guid resourceId)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_AddResourceToFavorites", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@ResourceId", resourceId);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task RemoveFromFavorites(Guid userId, Guid resourceId)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_RemoveResourceFromFavorites", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@ResourceId", resourceId);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task IncrementDownloadCount(Guid resourceId)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_IncrementResourceDownloadCount", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@ResourceId", resourceId);
        command.Parameters.AddWithValue("@UserId", DBNull.Value); // Could be passed in if tracking user downloads

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    private Resource MapResourceFromReader(SqlDataReader reader)
    {
        Resource resource = new Resource
        {
            ResourceId = reader.GetGuid(reader.GetOrdinal("ResourceId")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            ResourceType = (ResourceType)reader.GetInt32(reader.GetOrdinal("ResourceType")),
            IsInteractive = reader.GetBoolean(reader.GetOrdinal("IsInteractive")),
            HasAudio = reader.GetBoolean(reader.GetOrdinal("HasAudio")),
            GenerationMethod = (GenerationMethod)reader.GetInt32(reader.GetOrdinal("GenerationMethod")),
            ClinicalReviewStatus = (ClinicalReviewStatus)reader.GetInt32(reader.GetOrdinal("ClinicalReviewStatus")),
            EvidenceLevel = reader.GetInt32(reader.GetOrdinal("EvidenceLevel")),
            CreatedByUserId = reader.GetGuid(reader.GetOrdinal("CreatedByUserId")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
            IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted")),
            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
            IsFree = reader.GetBoolean(reader.GetOrdinal("IsFree")),
            CommissionRate = reader.GetDecimal(reader.GetOrdinal("CommissionRate")),
            DownloadCount = reader.GetInt32(reader.GetOrdinal("DownloadCount")),
            AverageRating = reader.GetDecimal(reader.GetOrdinal("AverageRating")),
            ReviewCount = reader.GetInt32(reader.GetOrdinal("ReviewCount"))
        };

        // Nullable fields
        int ordinal;
        
        ordinal = reader.GetOrdinal("Description");
        if (!reader.IsDBNull(ordinal))
            resource.Description = reader.GetString(ordinal);
            
        ordinal = reader.GetOrdinal("FileUrl");
        if (!reader.IsDBNull(ordinal))
            resource.FileUrl = reader.GetString(ordinal);
            
        ordinal = reader.GetOrdinal("ThumbnailUrl");
        if (!reader.IsDBNull(ordinal))
            resource.ThumbnailUrl = reader.GetString(ordinal);
            
        ordinal = reader.GetOrdinal("AiGenerationMetadata");
        if (!reader.IsDBNull(ordinal))
            resource.AiGenerationMetadata = reader.GetString(ordinal);
            
        ordinal = reader.GetOrdinal("DeletedAt");
        if (!reader.IsDBNull(ordinal))
            resource.DeletedAt = reader.GetDateTime(ordinal);
            
        ordinal = reader.GetOrdinal("SellerUserId");
        if (!reader.IsDBNull(ordinal))
            resource.SellerUserId = reader.GetGuid(ordinal);

        // JSON fields
        ordinal = reader.GetOrdinal("SkillAreas");
        resource.SkillAreas = reader.GetString(ordinal);
        
        ordinal = reader.GetOrdinal("GradeLevels");
        resource.GradeLevels = reader.GetString(ordinal);
        
        ordinal = reader.GetOrdinal("Languages");
        string languagesJson = reader.GetString(ordinal);
        resource.Languages = JsonConvert.DeserializeObject<List<string>>(languagesJson) ?? new List<string>();

        return resource;
    }

    // Additional IRepository<Resource> interface methods
    public async Task<IEnumerable<Resource>> FindAsync(Expression<Func<Resource, bool>> predicate)
    {
        // Stored procedures don't support expression trees
        // For complex queries, fall back to loading all and filtering in memory
        // In production, create specific stored procedures for common queries
        List<Resource> allResources = (await GetAllAsync()).ToList();
        return allResources.AsQueryable().Where(predicate).ToList();
    }

    public async Task DeleteAsync(Resource entity)
    {
        await DeleteAsync(entity.ResourceId);
    }

    public async Task<bool> ExistsAsync(Expression<Func<Resource, bool>> predicate)
    {
        // Similar limitation as FindAsync
        List<Resource> allResources = (await GetAllAsync()).ToList();
        return allResources.AsQueryable().Any(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<Resource, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            // Could create sp_GetResourceCount for better performance
            return (await GetAllAsync()).Count();
        }
        else
        {
            List<Resource> allResources = (await GetAllAsync()).ToList();
            return allResources.AsQueryable().Count(predicate);
        }
    }

    public IQueryable<Resource> Query()
    {
        // Stored procedures don't support IQueryable
        // This is a limitation of the stored procedure approach
        throw new NotSupportedException("Query() is not supported with stored procedures. Use specific methods like GetByIdAsync, SearchResourcesAsync, etc.");
    }

    // Additional IResourceRepository methods
    public async Task<IEnumerable<Resource>> GetBySellerAsync(Guid sellerId)
    {
        return await GetResourcesBySeller(sellerId);
    }

    public async Task<IEnumerable<Resource>> GetPopularResourcesAsync(int take = 10)
    {
        List<Resource> resources = new List<Resource>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetResourcesByFilter", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@PageNumber", 1);
        command.Parameters.AddWithValue("@PageSize", take);
        command.Parameters.AddWithValue("@SortBy", "DownloadCount");
        command.Parameters.AddWithValue("@SortDirection", "DESC");
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            resources.Add(MapResourceFromReader(reader));
        }
        
        return resources;
    }

    public async Task<IEnumerable<Resource>> GetRecentResourcesAsync(int take = 10)
    {
        List<Resource> resources = new List<Resource>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetResourcesByFilter", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@PageNumber", 1);
        command.Parameters.AddWithValue("@PageSize", take);
        command.Parameters.AddWithValue("@SortBy", "CreatedAt");
        command.Parameters.AddWithValue("@SortDirection", "DESC");
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            resources.Add(MapResourceFromReader(reader));
        }
        
        return resources;
    }

    public async Task<IEnumerable<Resource>> GetResourcesByCategoryAsync(Guid categoryId)
    {
        List<Resource> resources = new List<Resource>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetResourcesByCategory", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@CategoryId", categoryId);
        command.Parameters.AddWithValue("@PageNumber", 1);
        command.Parameters.AddWithValue("@PageSize", int.MaxValue);
        
        SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
        totalCountParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(totalCountParam);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            resources.Add(MapResourceFromReader(reader));
        }
        
        return resources;
    }

    public async Task<Resource?> GetResourceWithDetailsAsync(Guid resourceId)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetResourceWithDetails", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@ResourceId", resourceId);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        Resource? resource = null;
        
        // First result set: Resource
        if (await reader.ReadAsync())
        {
            resource = MapResourceFromReader(reader);
        }
        
        if (resource != null)
        {
            // Second result set: Categories
            await reader.NextResultAsync();
            resource.Categories = new List<ResourceCategory>();
            while (await reader.ReadAsync())
            {
                // Map categories if needed
            }
            
            // Third result set: Ratings
            await reader.NextResultAsync();
            resource.Ratings = new List<ResourceRating>();
            while (await reader.ReadAsync())
            {
                // Map ratings if needed
            }
            
            // Fourth result set: Download stats
            await reader.NextResultAsync();
            // Process download stats if needed
        }
        
        return resource;
    }

    public async Task IncrementViewCountAsync(Guid resourceId)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_IncrementResourceViewCount", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@ResourceId", resourceId);
        command.Parameters.AddWithValue("@ViewedByUserId", DBNull.Value);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<Dictionary<Guid, int>> GetDownloadCountsAsync(List<Guid> resourceIds)
    {
        Dictionary<Guid, int> downloadCounts = new Dictionary<Guid, int>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetResourceDownloadCounts", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        // Convert list to JSON for SQL Server
        string resourceIdsJson = JsonConvert.SerializeObject(resourceIds);
        command.Parameters.AddWithValue("@ResourceIds", resourceIdsJson);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            Guid resourceId = reader.GetGuid(reader.GetOrdinal("ResourceId"));
            int downloadCount = reader.GetInt32(reader.GetOrdinal("DownloadCount"));
            downloadCounts[resourceId] = downloadCount;
        }
        
        return downloadCounts;
    }

    public async Task AssignReviewerAsync(ReviewAssignment assignment)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_AssignResourceReviewer", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@ResourceId", assignment.ResourceId);
        command.Parameters.AddWithValue("@ReviewerUserId", assignment.ReviewerUserId);
        command.Parameters.AddWithValue("@AssignedByUserId", assignment.AssignedByUserId);
        command.Parameters.AddWithValue("@DueDate", (object?)assignment.DueDate ?? DBNull.Value);
        
        SqlParameter assignmentIdParam = new SqlParameter("@ReviewAssignmentId", SqlDbType.UniqueIdentifier);
        assignmentIdParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(assignmentIdParam);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
        
        assignment.ReviewAssignmentId = (Guid)assignmentIdParam.Value;
    }

    public async Task SubmitReviewEvaluationAsync(ReviewEvaluation evaluation)
    {
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_SubmitResourceReviewEvaluation", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@ReviewAssignmentId", evaluation.ReviewAssignmentId);
        command.Parameters.AddWithValue("@ReviewerUserId", evaluation.ReviewerUserId);
        command.Parameters.AddWithValue("@ApprovalStatus", (int)evaluation.ApprovalStatus);
        command.Parameters.AddWithValue("@ClinicalAccuracy", evaluation.ClinicalAccuracy);
        command.Parameters.AddWithValue("@AgeAppropriateness", evaluation.AgeAppropriateness);
        command.Parameters.AddWithValue("@EvidenceLevel", evaluation.EvidenceLevel);
        command.Parameters.AddWithValue("@Comments", evaluation.Comments);
        command.Parameters.AddWithValue("@RequiredChanges", (object?)evaluation.RequiredChanges ?? DBNull.Value);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<ReviewEvaluation>> GetResourceReviewsAsync(Guid resourceId)
    {
        List<ReviewEvaluation> reviews = new List<ReviewEvaluation>();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetResourceReviews", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@ResourceId", resourceId);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            ReviewEvaluation review = new ReviewEvaluation
            {
                ReviewEvaluationId = reader.GetGuid(reader.GetOrdinal("ReviewEvaluationId")),
                ReviewAssignmentId = reader.GetGuid(reader.GetOrdinal("ReviewAssignmentId")),
                ResourceId = reader.GetGuid(reader.GetOrdinal("ResourceId")),
                ReviewerUserId = reader.GetGuid(reader.GetOrdinal("ReviewerUserId")),
                ApprovalStatus = (ReviewApprovalStatus)reader.GetInt32(reader.GetOrdinal("ApprovalStatus")),
                ClinicalAccuracy = reader.GetInt32(reader.GetOrdinal("ClinicalAccuracy")),
                AgeAppropriateness = reader.GetInt32(reader.GetOrdinal("AgeAppropriateness")),
                EvidenceLevel = reader.GetInt32(reader.GetOrdinal("EvidenceLevel")),
                Comments = reader.GetString(reader.GetOrdinal("Comments")),
                ReviewedAt = reader.GetDateTime(reader.GetOrdinal("ReviewedAt")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
            
            int ordinal = reader.GetOrdinal("RequiredChanges");
            if (!reader.IsDBNull(ordinal))
                review.RequiredChanges = reader.GetString(ordinal);
                
            reviews.Add(review);
        }
        
        return reviews;
    }

    public async Task<ReviewStatisticsDto> GetReviewStatisticsAsync()
    {
        ReviewStatisticsDto stats = new ReviewStatisticsDto();
        
        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand("sp_GetReviewStatistics", connection);
        command.CommandType = CommandType.StoredProcedure;

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        
        // First result set: Overall statistics
        if (await reader.ReadAsync())
        {
            stats.Overall = new OverallStatistics
            {
                TotalAssignments = reader.GetInt32(reader.GetOrdinal("TotalAssignments")),
                CompletedReviews = reader.GetInt32(reader.GetOrdinal("CompletedReviews")),
                PendingReviews = reader.GetInt32(reader.GetOrdinal("PendingReviews")),
                InProgressReviews = reader.GetInt32(reader.GetOrdinal("InProgressReviews"))
            };
            
            int avgTimeOrdinal = reader.GetOrdinal("AvgReviewTimeHours");
            if (!reader.IsDBNull(avgTimeOrdinal))
                stats.Overall.AvgReviewTimeHours = reader.GetDouble(avgTimeOrdinal);
        }
        
        // Second result set: Approval statistics
        await reader.NextResultAsync();
        if (await reader.ReadAsync())
        {
            stats.Approval = new ApprovalStatistics
            {
                ApprovedCount = reader.GetInt32(reader.GetOrdinal("ApprovedCount")),
                ApprovedWithChangesCount = reader.GetInt32(reader.GetOrdinal("ApprovedWithChangesCount")),
                RejectedCount = reader.GetInt32(reader.GetOrdinal("RejectedCount")),
                AvgClinicalAccuracy = reader.GetDouble(reader.GetOrdinal("AvgClinicalAccuracy")),
                AvgAgeAppropriateness = reader.GetDouble(reader.GetOrdinal("AvgAgeAppropriateness")),
                AvgEvidenceLevel = reader.GetDouble(reader.GetOrdinal("AvgEvidenceLevel"))
            };
        }
        
        // Third result set: Top reviewers
        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            TopReviewer reviewer = new TopReviewer
            {
                UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                ReviewerName = reader.GetString(reader.GetOrdinal("ReviewerName")),
                ReviewsCompleted = reader.GetInt32(reader.GetOrdinal("ReviewsCompleted"))
            };
            
            int avgTimeOrdinal = reader.GetOrdinal("AvgReviewTimeHours");
            if (!reader.IsDBNull(avgTimeOrdinal))
                reviewer.AvgReviewTimeHours = reader.GetDouble(avgTimeOrdinal);
                
            stats.TopReviewers.Add(reviewer);
        }
        
        // Fourth result set: Monthly stats
        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            stats.MonthlyStats.Add(new MonthlyReviewStats
            {
                Year = reader.GetInt32(reader.GetOrdinal("Year")),
                Month = reader.GetInt32(reader.GetOrdinal("Month")),
                ReviewCount = reader.GetInt32(reader.GetOrdinal("ReviewCount")),
                AvgEvidenceLevel = reader.GetDouble(reader.GetOrdinal("AvgEvidenceLevel"))
            });
        }
        
        return stats;
    }

    public async Task<IEnumerable<Resource>> GetResourcesAsync(Func<Resource, bool> predicate)
    {
        // Similar to FindAsync, stored procedures don't support Func predicates
        // Load all and filter in memory (not ideal for large datasets)
        List<Resource> allResources = (await GetAllAsync()).ToList();
        return allResources.Where(predicate).ToList();
    }
}