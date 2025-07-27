using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPTRMS.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParentCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IconName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TaxId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SubscriptionTier = table.Column<int>(type: "int", nullable: false),
                    LicenseCount = table.Column<int>(type: "int", nullable: false),
                    UsedLicenses = table.Column<int>(type: "int", nullable: false),
                    SsoEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SsoProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SsoConfiguration = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubscriptionExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.OrganizationId);
                });

            migrationBuilder.CreateTable(
                name: "ReviewAssignments",
                columns: table => new
                {
                    AssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    EstimatedCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignmentReason = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewAssignments", x => x.AssignmentId);
                });

            migrationBuilder.CreateTable(
                name: "ReviewEvaluations",
                columns: table => new
                {
                    EvaluationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicalAccuracy = table.Column<int>(type: "int", nullable: false),
                    AgeAppropriateness = table.Column<int>(type: "int", nullable: false),
                    SafetyCompliance = table.Column<int>(type: "int", nullable: false),
                    TherapeuticValue = table.Column<int>(type: "int", nullable: false),
                    OverallApproval = table.Column<bool>(type: "bit", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewEvaluations", x => x.EvaluationId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LicenseNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubscriptionTier = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsSellerApproved = table.Column<bool>(type: "bit", nullable: false),
                    Languages = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Specialties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubscriptionStatus = table.Column<int>(type: "int", nullable: false),
                    SubscriptionStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubscriptionEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    DeviceInfo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.TokenId);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SellerProfiles",
                columns: table => new
                {
                    SellerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StoreUrl = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Specialties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalSales = table.Column<int>(type: "int", nullable: false),
                    TotalReviews = table.Column<int>(type: "int", nullable: false),
                    CommissionRate = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalEarnings = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AvailableBalance = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSaleAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AcceptsCustomOrders = table.Column<bool>(type: "bit", nullable: false),
                    ProcessingTimeDays = table.Column<int>(type: "int", nullable: false),
                    VacationMode = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerProfiles", x => x.SellerId);
                    table.ForeignKey(
                        name: "FK_SellerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TherapistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstNameEncrypted = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastNameEncrypted = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirthEncrypted = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentEmailEncrypted = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IepGoalsEncrypted = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GradeLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PrimaryDisability = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    HasParentAccess = table.Column<bool>(type: "bit", nullable: false),
                    DischargedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentId);
                    table.ForeignKey(
                        name: "FK_Students_Users_TherapistId",
                        column: x => x.TherapistId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarketplaceTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuyerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SellerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SellerEarnings = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    StripePaymentIntentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CouponCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SellerProfileSellerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_MarketplaceTransactions_SellerProfiles_SellerId",
                        column: x => x.SellerId,
                        principalTable: "SellerProfiles",
                        principalColumn: "SellerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketplaceTransactions_SellerProfiles_SellerProfileSellerId",
                        column: x => x.SellerProfileSellerId,
                        principalTable: "SellerProfiles",
                        principalColumn: "SellerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketplaceTransactions_Users_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SellerFollowers",
                columns: table => new
                {
                    FollowerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SellerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FollowedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SellerProfileSellerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerFollowers", x => x.FollowerId);
                    table.ForeignKey(
                        name: "FK_SellerFollowers_SellerProfiles_SellerId",
                        column: x => x.SellerId,
                        principalTable: "SellerProfiles",
                        principalColumn: "SellerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SellerFollowers_SellerProfiles_SellerProfileSellerId",
                        column: x => x.SellerProfileSellerId,
                        principalTable: "SellerProfiles",
                        principalColumn: "SellerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SellerFollowers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SellerPayouts",
                columns: table => new
                {
                    PayoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SellerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Method = table.Column<int>(type: "int", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerPayouts", x => x.PayoutId);
                    table.ForeignKey(
                        name: "FK_SellerPayouts_SellerProfiles_SellerId",
                        column: x => x.SellerId,
                        principalTable: "SellerProfiles",
                        principalColumn: "SellerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParentAccesses",
                columns: table => new
                {
                    AccessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AccessLevel = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StudentId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentAccesses", x => x.AccessId);
                    table.ForeignKey(
                        name: "FK_ParentAccesses_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParentAccesses_Students_StudentId1",
                        column: x => x.StudentId1,
                        principalTable: "Students",
                        principalColumn: "StudentId");
                    table.ForeignKey(
                        name: "FK_ParentAccesses_Users_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TherapistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    SessionType = table.Column<int>(type: "int", nullable: false),
                    NotesEncrypted = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataPointsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsBillable = table.Column<bool>(type: "bit", nullable: false),
                    CptCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_Sessions_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_TherapistId",
                        column: x => x.TherapistId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentProgressEntries",
                columns: table => new
                {
                    ProgressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoalArea = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MeasurementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PercentageComplete = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DataPoints = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProgressEntries", x => x.ProgressId);
                    table.ForeignKey(
                        name: "FK_StudentProgressEntries_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentProgressEntries_Users_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResourceType = table.Column<int>(type: "int", nullable: false),
                    SkillAreas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GradeLevels = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GenerationMethod = table.Column<int>(type: "int", nullable: false),
                    AiGenerationMetadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClinicalReviewStatus = table.Column<int>(type: "int", nullable: false),
                    EvidenceLevel = table.Column<int>(type: "int", nullable: true),
                    LanguagesAvailable = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsInteractive = table.Column<bool>(type: "bit", nullable: false),
                    HasAudio = table.Column<bool>(type: "bit", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsMarketplaceItem = table.Column<bool>(type: "bit", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    SellerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PreviewUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    DownloadCount = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ReviewCount = table.Column<int>(type: "int", nullable: false),
                    RetiredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetiredReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RetiredBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SuggestedAlternatives = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviousVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LatestVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsSuperseded = table.Column<bool>(type: "bit", nullable: false),
                    SupersededAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.ResourceId);
                    table.ForeignKey(
                        name: "FK_Resources_SellerProfiles_SellerId",
                        column: x => x.SellerId,
                        principalTable: "SellerProfiles",
                        principalColumn: "SellerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Resources_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "SessionId");
                    table.ForeignKey(
                        name: "FK_Resources_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionDataPoints",
                columns: table => new
                {
                    DataPointId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Metric = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionDataPoints", x => x.DataPointId);
                    table.ForeignKey(
                        name: "FK_SessionDataPoints_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentGoal",
                columns: table => new
                {
                    GoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoalText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GoalArea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Progress = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentGoal", x => x.GoalId);
                    table.ForeignKey(
                        name: "FK_StudentGoal_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "SessionId");
                    table.ForeignKey(
                        name: "FK_StudentGoal_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIGenerations",
                columns: table => new
                {
                    GenerationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Prompt = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ModelUsed = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TokensConsumed = table.Column<int>(type: "int", nullable: false),
                    GenerationTimeMs = table.Column<int>(type: "int", nullable: false),
                    OutputResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ClinicalReviewNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    GenerationParameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIGenerations", x => x.GenerationId);
                    table.ForeignKey(
                        name: "FK_AIGenerations_Resources_OutputResourceId",
                        column: x => x.OutputResourceId,
                        principalTable: "Resources",
                        principalColumn: "ResourceId");
                    table.ForeignKey(
                        name: "FK_AIGenerations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResourceCategories",
                columns: table => new
                {
                    ResourceCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceCategories", x => x.ResourceCategoryId);
                    table.ForeignKey(
                        name: "FK_ResourceCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourceCategories_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "ResourceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceDownloads",
                columns: table => new
                {
                    DownloadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DownloadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceDownloads", x => x.DownloadId);
                    table.ForeignKey(
                        name: "FK_ResourceDownloads_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "ResourceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourceDownloads_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResourceRatings",
                columns: table => new
                {
                    RatingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Review = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsVerifiedPurchase = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceRatings", x => x.RatingId);
                    table.ForeignKey(
                        name: "FK_ResourceRatings_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "ResourceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourceRatings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResourceRatings_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionResources",
                columns: table => new
                {
                    SessionResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MinutesUsed = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionResources", x => x.SessionResourceId);
                    table.ForeignKey(
                        name: "FK_SessionResources_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "ResourceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionResources_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentAssignments",
                columns: table => new
                {
                    AssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CompletionData = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAssignments", x => x.AssignmentId);
                    table.ForeignKey(
                        name: "FK_StudentAssignments_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "ResourceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentAssignments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentAssignments_Users_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerations_OutputResourceId",
                table: "AIGenerations",
                column: "OutputResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerations_Status",
                table: "AIGenerations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerations_UserId_CreatedAt",
                table: "AIGenerations",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceTransactions_BuyerId",
                table: "MarketplaceTransactions",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceTransactions_CreatedAt",
                table: "MarketplaceTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceTransactions_SellerId",
                table: "MarketplaceTransactions",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceTransactions_SellerProfileSellerId",
                table: "MarketplaceTransactions",
                column: "SellerProfileSellerId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceTransactions_StripePaymentIntentId",
                table: "MarketplaceTransactions",
                column: "StripePaymentIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentAccesses_AccessCode",
                table: "ParentAccesses",
                column: "AccessCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParentAccesses_ParentUserId",
                table: "ParentAccesses",
                column: "ParentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentAccesses_StudentId_ParentUserId",
                table: "ParentAccesses",
                columns: new[] { "StudentId", "ParentUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParentAccesses_StudentId1",
                table: "ParentAccesses",
                column: "StudentId1");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_IsRevoked",
                table: "RefreshTokens",
                columns: new[] { "UserId", "IsRevoked" });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceCategories_CategoryId",
                table: "ResourceCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceCategories_ResourceId_CategoryId",
                table: "ResourceCategories",
                columns: new[] { "ResourceId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceDownloads_ResourceId",
                table: "ResourceDownloads",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceDownloads_UserId_ResourceId_DownloadedAt",
                table: "ResourceDownloads",
                columns: new[] { "UserId", "ResourceId", "DownloadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceRatings_ResourceId_UserId",
                table: "ResourceRatings",
                columns: new[] { "ResourceId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceRatings_UserId",
                table: "ResourceRatings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceRatings_UserId1",
                table: "ResourceRatings",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ClinicalReviewStatus",
                table: "Resources",
                column: "ClinicalReviewStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_CreatedByUserId",
                table: "Resources",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_IsMarketplaceItem",
                table: "Resources",
                column: "IsMarketplaceItem");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_IsPublished",
                table: "Resources",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ResourceType",
                table: "Resources",
                column: "ResourceType");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_RetiredAt",
                table: "Resources",
                column: "RetiredAt");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_SellerId",
                table: "Resources",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_SessionId",
                table: "Resources",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Title",
                table: "Resources",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewAssignments_AssignedAt",
                table: "ReviewAssignments",
                column: "AssignedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewAssignments_ResourceId_ReviewerId",
                table: "ReviewAssignments",
                columns: new[] { "ResourceId", "ReviewerId" });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewAssignments_Status",
                table: "ReviewAssignments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewEvaluations_ResourceId",
                table: "ReviewEvaluations",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewEvaluations_ReviewedAt",
                table: "ReviewEvaluations",
                column: "ReviewedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewEvaluations_ReviewerId",
                table: "ReviewEvaluations",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerFollowers_SellerId_UserId",
                table: "SellerFollowers",
                columns: new[] { "SellerId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SellerFollowers_SellerProfileSellerId",
                table: "SellerFollowers",
                column: "SellerProfileSellerId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerFollowers_UserId",
                table: "SellerFollowers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerPayouts_SellerId",
                table: "SellerPayouts",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerProfiles_StoreUrl",
                table: "SellerProfiles",
                column: "StoreUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SellerProfiles_UserId",
                table: "SellerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionDataPoints_SessionId",
                table: "SessionDataPoints",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionResources_ResourceId",
                table: "SessionResources",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionResources_SessionId",
                table: "SessionResources",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SessionDate",
                table: "Sessions",
                column: "SessionDate");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_StudentId",
                table: "Sessions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_TherapistId_SessionDate",
                table: "Sessions",
                columns: new[] { "TherapistId", "SessionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentAssignments_AssignedByUserId",
                table: "StudentAssignments",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAssignments_DueAt",
                table: "StudentAssignments",
                column: "DueAt");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAssignments_ResourceId",
                table: "StudentAssignments",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAssignments_StudentId_Status",
                table: "StudentAssignments",
                columns: new[] { "StudentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentGoal_SessionId",
                table: "StudentGoal",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentGoal_StudentId",
                table: "StudentGoal",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgressEntries_RecordedByUserId",
                table: "StudentProgressEntries",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgressEntries_StudentId",
                table: "StudentProgressEntries",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_AccessCode",
                table: "Students",
                column: "AccessCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_ExternalId",
                table: "Students",
                column: "ExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_TherapistId",
                table: "Students",
                column: "TherapistId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIGenerations");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "MarketplaceTransactions");

            migrationBuilder.DropTable(
                name: "ParentAccesses");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "ResourceCategories");

            migrationBuilder.DropTable(
                name: "ResourceDownloads");

            migrationBuilder.DropTable(
                name: "ResourceRatings");

            migrationBuilder.DropTable(
                name: "ReviewAssignments");

            migrationBuilder.DropTable(
                name: "ReviewEvaluations");

            migrationBuilder.DropTable(
                name: "SellerFollowers");

            migrationBuilder.DropTable(
                name: "SellerPayouts");

            migrationBuilder.DropTable(
                name: "SessionDataPoints");

            migrationBuilder.DropTable(
                name: "SessionResources");

            migrationBuilder.DropTable(
                name: "StudentAssignments");

            migrationBuilder.DropTable(
                name: "StudentGoal");

            migrationBuilder.DropTable(
                name: "StudentProgressEntries");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "SellerProfiles");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
