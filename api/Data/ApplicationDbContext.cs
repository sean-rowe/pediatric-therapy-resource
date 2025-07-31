using Microsoft.EntityFrameworkCore;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Services;

namespace UPTRMS.Api.Data;

public class ApplicationDbContext : DbContext
{
    private readonly IEncryptionService _encryptionService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IEncryptionService encryptionService)
        : base(options)
    {
        _encryptionService = encryptionService;
    }

    // Core entities
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<Resource> Resources { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Session> Sessions { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;

    // Marketplace entities
    public DbSet<SellerProfile> SellerProfiles { get; set; } = null!;
    public DbSet<MarketplaceTransaction> MarketplaceTransactions { get; set; } = null!;
    public DbSet<SellerFollower> SellerFollowers { get; set; } = null!;
    public DbSet<SellerPayout> SellerPayouts { get; set; } = null!;

    // Tracking entities
    public DbSet<ResourceDownload> ResourceDownloads { get; set; } = null!;
    public DbSet<ResourceRating> ResourceRatings { get; set; } = null!;
    public DbSet<StudentProgress> StudentProgressEntries { get; set; } = null!;
    public DbSet<SessionDataPoint> SessionDataPoints { get; set; } = null!;

    // Access entities
    public DbSet<ParentAccess> ParentAccesses { get; set; } = null!;
    public DbSet<StudentAssignment> StudentAssignments { get; set; } = null!;
    public DbSet<AIGeneration> AIGenerations { get; set; } = null!;

    // Content management entities
    public DbSet<ReviewAssignment> ReviewAssignments { get; set; } = null!;
    public DbSet<ReviewEvaluation> ReviewEvaluations { get; set; } = null!;

    // Junction tables
    public DbSet<ResourceCategory> ResourceCategories { get; set; } = null!;
    public DbSet<SessionResource> SessionResources { get; set; } = null!;

    // Authentication and audit entities
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; } = null!;
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Languages)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
            entity.Property(e => e.Specialties)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
        });

        // Organization configuration
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasMany(o => o.Users)
                .WithOne(u => u.Organization)
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Resource configuration
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.ResourceType);
            entity.HasIndex(e => e.IsPublished);
            entity.HasIndex(e => e.IsMarketplaceItem);
            entity.HasIndex(e => e.ClinicalReviewStatus);
            entity.HasIndex(e => e.RetiredAt);
            entity.Property(e => e.LanguagesAvailable)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
            entity.Property(e => e.SuggestedAlternatives)
                .HasConversion(
                    v => string.Join(',', v.Select(x => x.ToString())),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Where(x => !string.IsNullOrWhiteSpace(x))
                          .Select(x => new Guid(x))
                          .ToList());
        });

        // Student configuration with encryption
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasIndex(e => e.AccessCode).IsUnique();
            entity.HasIndex(e => e.ExternalId);
            entity.Property(e => e.FirstNameEncrypted)
                .HasConversion(
                    v => _encryptionService.Encrypt(v),
                    v => _encryptionService.Decrypt(v));
            entity.Property(e => e.LastNameEncrypted)
                .HasConversion(
                    v => _encryptionService.Encrypt(v),
                    v => _encryptionService.Decrypt(v));
            entity.Property(e => e.DateOfBirthEncrypted)
                .HasConversion(
                    v => _encryptionService.Encrypt(v),
                    v => _encryptionService.Decrypt(v));
            entity.Property(e => e.ParentEmailEncrypted)
                .HasConversion(
                    v => v == null ? null : _encryptionService.Encrypt(v),
                    v => v == null ? null : _encryptionService.Decrypt(v));
            entity.Property(e => e.IepGoalsEncrypted)
                .HasConversion(
                    v => v == null ? null : _encryptionService.Encrypt(v),
                    v => v == null ? null : _encryptionService.Decrypt(v));
        });

        // Session configuration
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasIndex(e => e.SessionDate);
            entity.HasIndex(e => new { e.TherapistId, e.SessionDate });
            entity.Property(e => e.NotesEncrypted)
                .HasConversion(
                    v => v == null ? null : _encryptionService.Encrypt(v),
                    v => v == null ? null : _encryptionService.Decrypt(v));
        });

        // SellerProfile configuration
        modelBuilder.Entity<SellerProfile>(entity =>
        {
            entity.HasIndex(e => e.StoreUrl).IsUnique();
            entity.HasOne(s => s.User)
                .WithOne(u => u.SellerProfile)
                .HasForeignKey<SellerProfile>(s => s.UserId);
            entity.Property(e => e.Specialties)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
        });

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasMany(c => c.SubCategories)
                .WithOne(c => c.ParentCategory)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ResourceCategory many-to-many configuration
        modelBuilder.Entity<ResourceCategory>(entity =>
        {
            entity.HasKey(rc => rc.ResourceCategoryId);
            entity.HasIndex(rc => new { rc.ResourceId, rc.CategoryId }).IsUnique();
            entity.HasOne(rc => rc.Resource)
                .WithMany(r => r.Categories)
                .HasForeignKey(rc => rc.ResourceId);
            entity.HasOne(rc => rc.Category)
                .WithMany(c => c.Resources)
                .HasForeignKey(rc => rc.CategoryId);
        });

        // SessionResource configuration
        modelBuilder.Entity<SessionResource>(entity =>
        {
            entity.HasOne(sr => sr.Session)
                .WithMany(s => s.SessionResources)
                .HasForeignKey(sr => sr.SessionId);
            entity.HasOne(sr => sr.Resource)
                .WithMany(r => r.SessionUses)
                .HasForeignKey(sr => sr.ResourceId);
        });

        // ResourceDownload configuration
        modelBuilder.Entity<ResourceDownload>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.ResourceId, e.DownloadedAt });
        });

        // ResourceRating configuration
        modelBuilder.Entity<ResourceRating>(entity =>
        {
            entity.HasIndex(e => new { e.ResourceId, e.UserId }).IsUnique();
            entity.HasOne(rr => rr.Resource)
                .WithMany(r => r.Ratings)
                .HasForeignKey(rr => rr.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(rr => rr.User)
                .WithMany()
                .HasForeignKey(rr => rr.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // MarketplaceTransaction configuration
        modelBuilder.Entity<MarketplaceTransaction>(entity =>
        {
            entity.HasIndex(e => e.StripePaymentIntentId);
            entity.HasIndex(e => e.CreatedAt);

            // Configure foreign keys to avoid multiple cascade paths
            entity.HasOne(mt => mt.Buyer)
                .WithMany()
                .HasForeignKey(mt => mt.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(mt => mt.Seller)
                .WithMany()
                .HasForeignKey(mt => mt.SellerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SellerFollower configuration
        modelBuilder.Entity<SellerFollower>(entity =>
        {
            entity.HasIndex(e => new { e.SellerId, e.UserId }).IsUnique();

            // Configure foreign keys to avoid multiple cascade paths
            entity.HasOne(sf => sf.Seller)
                .WithMany()
                .HasForeignKey(sf => sf.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sf => sf.User)
                .WithMany()
                .HasForeignKey(sf => sf.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ParentAccess configuration
        modelBuilder.Entity<ParentAccess>(entity =>
        {
            entity.HasIndex(e => e.AccessCode).IsUnique();
            entity.HasIndex(e => new { e.StudentId, e.ParentUserId }).IsUnique();

            // Configure foreign keys to avoid multiple cascade paths
            entity.HasOne(pa => pa.Student)
                .WithMany()
                .HasForeignKey(pa => pa.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(pa => pa.ParentUser)
                .WithMany()
                .HasForeignKey(pa => pa.ParentUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // StudentAssignment configuration
        modelBuilder.Entity<StudentAssignment>(entity =>
        {
            entity.HasIndex(e => new { e.StudentId, e.Status });
            entity.HasIndex(e => e.DueAt);
        });

        // AIGeneration configuration
        modelBuilder.Entity<AIGeneration>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => e.Status);
        });

        // ReviewAssignment configuration
        modelBuilder.Entity<ReviewAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId);
            entity.HasIndex(e => new { e.ResourceId, e.ReviewerId });
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.AssignedAt);
        });

        // ReviewEvaluation configuration
        modelBuilder.Entity<ReviewEvaluation>(entity =>
        {
            entity.HasKey(e => e.EvaluationId);
            entity.HasIndex(e => e.ResourceId);
            entity.HasIndex(e => e.ReviewerId);
            entity.HasIndex(e => e.ReviewedAt);
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.Action);
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.TokenId);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => new { e.UserId, e.IsRevoked });

            // Configure foreign key to avoid cascade issues
            entity.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // EmailVerificationToken configuration
        modelBuilder.Entity<EmailVerificationToken>(entity =>
        {
            entity.HasKey(e => e.TokenId);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => new { e.UserId, e.IsUsed });

            // Configure foreign key to avoid cascade issues
            entity.HasOne(evt => evt.User)
                .WithMany()
                .HasForeignKey(evt => evt.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PasswordResetToken configuration
        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.TokenId);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => new { e.UserId, e.IsUsed });

            // Configure foreign key to avoid cascade issues
            entity.HasOne(prt => prt.User)
                .WithMany()
                .HasForeignKey(prt => prt.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure all User foreign keys to avoid cascade cycles in SQL Server
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var relationship in entityType.GetForeignKeys())
            {
                // If the principal entity is User, set delete behavior to Restrict
                if (relationship.PrincipalEntityType.ClrType == typeof(User))
                {
                    relationship.DeleteBehavior = DeleteBehavior.Restrict;
                }
                // Also check for SellerProfile relationships to avoid cascades
                if (relationship.PrincipalEntityType.ClrType == typeof(SellerProfile))
                {
                    relationship.DeleteBehavior = DeleteBehavior.Restrict;
                }
            }
        }

        // Apply decimal precision
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var properties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?));

            foreach (var property in properties)
            {
                modelBuilder.Entity(entityType.Name).Property(property.Name)
                    .HasColumnType("decimal(10,2)");
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Resource || e.Entity is Session || e.Entity is ResourceRating)
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Resource resource)
                resource.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is Session session)
                session.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is ResourceRating rating)
                rating.UpdatedAt = DateTime.UtcNow;
        }
    }
}