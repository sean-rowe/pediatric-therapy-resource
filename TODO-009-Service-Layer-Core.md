# TODO-009: Core Service Layer Implementation

These tasks implement the business logic layer services.

## IResourceService Interface

- [ ] Create IResourceService.cs in Services folder
- [ ] Add using statements for Models.Domain
- [ ] Add using statements for Models.DTOs
- [ ] Add namespace UPTRMS.Api.Services
- [ ] Create interface IResourceService
- [ ] Add method Task<ResourceDto> GetByIdAsync(Guid id)
- [ ] Add method Task<PagedResult<ResourceDto>> SearchAsync(ResourceSearchDto searchDto)
- [ ] Add method Task<ResourceDto> CreateAsync(ResourceCreateDto createDto, Guid userId)
- [ ] Add method Task<ResourceDto> UpdateAsync(Guid id, ResourceUpdateDto updateDto, Guid userId)
- [ ] Add method Task<bool> DeleteAsync(Guid id, Guid userId)
- [ ] Add method Task<Stream> DownloadAsync(Guid id, Guid userId)
- [ ] Add method Task<bool> AddToFavoritesAsync(Guid resourceId, Guid userId)
- [ ] Add method Task<bool> RemoveFromFavoritesAsync(Guid resourceId, Guid userId)
- [ ] Add method Task<bool> RateResourceAsync(Guid resourceId, Guid userId, int rating, string? review)
- [ ] Add method Task<IEnumerable<ResourceDto>> GetRecommendationsAsync(Guid userId, int count)
- [ ] Add method Task<ResourceDto> GenerateAIResourceAsync(AIGenerationRequestDto request, Guid userId)
- [ ] Add method Task<IEnumerable<ResourceCategoryDto>> GetCategoriesAsync()
- [ ] Add method Task<ResourceAnalyticsDto> GetAnalyticsAsync(Guid resourceId)
- [ ] Add method Task<BulkOperationResult> BulkUploadAsync(IEnumerable<ResourceCreateDto> resources, Guid userId)
- [ ] Add method Task<Stream> BulkDownloadAsync(IEnumerable<Guid> resourceIds, Guid userId)
- [ ] Add method Task<ResourceDto> CloneResourceAsync(Guid resourceId, Guid userId)
- [ ] Add method Task<ShareResult> ShareResourceAsync(Guid resourceId, ShareRequestDto shareRequest, Guid userId)
- [ ] Add method Task<IEnumerable<ResourceVersionDto>> GetVersionHistoryAsync(Guid resourceId)

## ResourceService Implementation - Setup

- [ ] Create ResourceService.cs in Services folder
- [ ] Add using statements for all required namespaces
- [ ] Add namespace UPTRMS.Api.Services
- [ ] Create class ResourceService
- [ ] Implement IResourceService interface
- [ ] Add private readonly IResourceRepository _resourceRepository field
- [ ] Add private readonly IFileStorageService _fileStorageService field
- [ ] Add private readonly ICacheService _cacheService field
- [ ] Add private readonly ILogger<ResourceService> _logger field
- [ ] Add private readonly IMapper _mapper field
- [ ] Add private readonly IAuditService _auditService field
- [ ] Add constructor accepting all dependencies
- [ ] Store all dependencies in fields

## ResourceService Implementation - GetByIdAsync

- [ ] Implement GetByIdAsync method
- [ ] Create cache key "resource_{id}"
- [ ] Try get from cache first
- [ ] If cached, deserialize and return
- [ ] If not cached, call repository GetByIdWithDetailsAsync
- [ ] If resource is null, throw NotFoundException
- [ ] Map entity to ResourceDto using AutoMapper
- [ ] Serialize and cache for 5 minutes
- [ ] Log resource access for analytics
- [ ] Return mapped DTO

## ResourceService Implementation - SearchAsync

- [ ] Implement SearchAsync method
- [ ] Validate search parameters
- [ ] Set default page size to 20 if not specified
- [ ] Set maximum page size to 100
- [ ] Build specification from search criteria
- [ ] Add skill area filter if provided
- [ ] Add grade level filter if provided
- [ ] Add resource type filter if provided
- [ ] Add rating filter if provided
- [ ] Add language filter if provided
- [ ] Add price range filter for marketplace items
- [ ] Add text search if search term provided
- [ ] Call repository with specification
- [ ] Get total count for pagination
- [ ] Calculate total pages
- [ ] Map results to DTOs
- [ ] Create PagedResult object
- [ ] Set Items property
- [ ] Set CurrentPage property
- [ ] Set PageSize property
- [ ] Set TotalPages property
- [ ] Set TotalCount property
- [ ] Return paged result

## ResourceService Implementation - CreateAsync

- [ ] Implement CreateAsync method
- [ ] Validate create DTO
- [ ] Check user has permission to create
- [ ] Generate unique file name if file provided
- [ ] Upload file to storage service
- [ ] Get file URL from storage service
- [ ] Generate thumbnail if image/PDF
- [ ] Get thumbnail URL
- [ ] Create Resource entity
- [ ] Set all properties from DTO
- [ ] Set CreatedBy to userId
- [ ] Set CreatedAt to current UTC time
- [ ] Set initial view/download counts to 0
- [ ] Parse and validate skill areas JSON
- [ ] Parse and validate grade levels JSON
- [ ] Call repository AddAsync
- [ ] Call SaveChangesAsync
- [ ] Create audit log entry
- [ ] Map created entity to DTO
- [ ] Return created resource DTO

## ResourceService Implementation - UpdateAsync

- [ ] Implement UpdateAsync method
- [ ] Get existing resource by id
- [ ] If not found, throw NotFoundException
- [ ] Check user has permission to update
- [ ] If not creator or admin, throw ForbiddenException
- [ ] Update allowed properties from DTO
- [ ] Don't allow updating CreatedBy
- [ ] Don't allow updating CreatedAt
- [ ] Set UpdatedBy to userId
- [ ] Set UpdatedAt to current UTC time
- [ ] Increment Version number
- [ ] If new file provided, upload to storage
- [ ] Delete old file if being replaced
- [ ] Update file URL
- [ ] If thumbnail needed, regenerate
- [ ] Call repository Update
- [ ] Call SaveChangesAsync
- [ ] Invalidate cache for resource
- [ ] Create audit log entry
- [ ] Map updated entity to DTO
- [ ] Return updated resource DTO

## ResourceService Implementation - DeleteAsync

- [ ] Implement DeleteAsync method
- [ ] Get existing resource by id
- [ ] If not found, return false
- [ ] Check user has permission to delete
- [ ] If not creator or admin, throw ForbiddenException
- [ ] Set IsDeleted to true (soft delete)
- [ ] Set DeletedAt to current UTC time
- [ ] Set DeletedBy to userId
- [ ] Call repository Update
- [ ] Call SaveChangesAsync
- [ ] Invalidate cache for resource
- [ ] Create audit log entry
- [ ] Return true

## ResourceService Implementation - DownloadAsync

- [ ] Implement DownloadAsync method
- [ ] Get resource by id
- [ ] If not found, throw NotFoundException
- [ ] Check user has permission to download
- [ ] Check subscription allows downloads
- [ ] Get file stream from storage service
- [ ] If file not found, throw FileNotFoundException
- [ ] Increment download count
- [ ] Record download in analytics
- [ ] Create audit log entry
- [ ] Return file stream

## ResourceService Implementation - Favorites

- [ ] Implement AddToFavoritesAsync method
- [ ] Check resource exists
- [ ] Check not already favorited
- [ ] Create favorite record
- [ ] Save to database
- [ ] Return true
- [ ] Implement RemoveFromFavoritesAsync method
- [ ] Find favorite record
- [ ] If not found, return false
- [ ] Delete favorite record
- [ ] Save to database
- [ ] Return true

## ResourceService Implementation - Rating

- [ ] Implement RateResourceAsync method
- [ ] Validate rating between 1 and 5
- [ ] Get resource by id
- [ ] If not found, throw NotFoundException
- [ ] Check user hasn't already rated
- [ ] Create ResourceRating entity
- [ ] Set ResourceId
- [ ] Set UserId
- [ ] Set Rating value
- [ ] Set Review text if provided
- [ ] Set CreatedAt to current UTC time
- [ ] Add rating to database
- [ ] Recalculate average rating
- [ ] Get all ratings for resource
- [ ] Calculate new average
- [ ] Update resource average rating
- [ ] Update total ratings count
- [ ] Save changes
- [ ] Invalidate cache
- [ ] Return true

## ResourceService Implementation - AI Generation

- [ ] Implement GenerateAIResourceAsync method
- [ ] Validate generation request
- [ ] Check user has AI generation credits
- [ ] Check rate limit not exceeded
- [ ] Create generation job
- [ ] Set job status to pending
- [ ] Queue job for processing
- [ ] Deduct credit from user
- [ ] Create placeholder resource
- [ ] Set GenerationMethod to "ai_generated"
- [ ] Set status to "generating"
- [ ] Save placeholder
- [ ] Return resource DTO with generating status
- [ ] (Note: Actual generation happens async)

## ResourceService Implementation - Helper Methods

- [ ] Implement GetCategoriesAsync method
- [ ] Get all active categories
- [ ] Order by DisplayOrder then Name
- [ ] Map to DTOs
- [ ] Cache for 1 hour
- [ ] Return category list
- [ ] Implement GetAnalyticsAsync method
- [ ] Get resource with all related data
- [ ] Calculate view rate
- [ ] Calculate download rate
- [ ] Get usage by date
- [ ] Get usage by user type
- [ ] Create analytics DTO
- [ ] Return analytics
- [ ] Implement GetVersionHistoryAsync method
- [ ] Query audit log for resource
- [ ] Filter by resource id
- [ ] Order by timestamp descending
- [ ] Map to version DTOs
- [ ] Return version list