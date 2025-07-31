# TODO-005: Entity Framework Configuration (Continued)

These tasks continue the Entity Framework configuration for remaining entities.

## Organization Entity Configuration

- [ ] Create OrganizationConfiguration.cs file in EntityConfigurations folder
- [ ] Add using statements for Microsoft.EntityFrameworkCore
- [ ] Add using statements for Microsoft.EntityFrameworkCore.Metadata.Builders
- [ ] Add namespace UPTRMS.Api.Data.EntityConfigurations
- [ ] Create class OrganizationConfiguration implementing IEntityTypeConfiguration<Organization>
- [ ] Implement Configure method
- [ ] Call ToTable("Organizations") on builder
- [ ] Configure Id as primary key using HasKey
- [ ] Configure Name as required with HasMaxLength(200)
- [ ] Add index on Name
- [ ] Configure Type as required with HasMaxLength(50)
- [ ] Add index on Type
- [ ] Configure TaxId with HasMaxLength(50)
- [ ] Configure Address with HasMaxLength(500)
- [ ] Configure City with HasMaxLength(100)
- [ ] Configure State with HasMaxLength(50)
- [ ] Configure ZipCode with HasMaxLength(20)
- [ ] Configure Country with HasMaxLength(100)
- [ ] Configure Phone with HasMaxLength(20)
- [ ] Configure Email with HasMaxLength(255)
- [ ] Configure Website with HasMaxLength(500)
- [ ] Configure LogoUrl with HasMaxLength(1000)
- [ ] Configure PrimaryColor with HasMaxLength(7)
- [ ] Configure SecondaryColor with HasMaxLength(7)
- [ ] Configure SubscriptionTier with HasMaxLength(50)
- [ ] Configure CurrentUsers with default value 0
- [ ] Configure StorageUsedGB with default value 0.0m
- [ ] Configure StorageUsedGB with precision (10,3)
- [ ] Configure BillingContactName with HasMaxLength(200)
- [ ] Configure BillingContactEmail with HasMaxLength(255)
- [ ] Configure BillingContactPhone with HasMaxLength(20)
- [ ] Configure StripeCustomerId with HasMaxLength(255)
- [ ] Configure IsActive with default value true
- [ ] Configure CreatedAt with default value sql function GETUTCDATE()
- [ ] Configure UpdatedAt with default value sql function GETUTCDATE()
- [ ] Configure relationship with Subscription
- [ ] Set delete behavior to SetNull for Subscription relationship
- [ ] Add index on SubscriptionId
- [ ] Add composite index on (IsActive, Type)
- [ ] Add index on StripeCustomerId

## Subscription Entity Configuration

- [ ] Create SubscriptionConfiguration.cs file in EntityConfigurations folder
- [ ] Add using statements for Microsoft.EntityFrameworkCore
- [ ] Add using statements for Microsoft.EntityFrameworkCore.Metadata.Builders
- [ ] Add namespace UPTRMS.Api.Data.EntityConfigurations
- [ ] Create class SubscriptionConfiguration implementing IEntityTypeConfiguration<Subscription>
- [ ] Implement Configure method
- [ ] Call ToTable("Subscriptions") on builder
- [ ] Configure Id as primary key using HasKey
- [ ] Configure PlanId as required with HasMaxLength(50)
- [ ] Add index on PlanId
- [ ] Configure PlanName as required with HasMaxLength(100)
- [ ] Configure PlanType as required with HasMaxLength(50)
- [ ] Configure Price as required with precision (10,2)
- [ ] Configure BillingInterval as required with HasMaxLength(20)
- [ ] Configure Status as required with HasMaxLength(50)
- [ ] Add index on Status
- [ ] Configure CancellationReason with HasMaxLength(500)
- [ ] Configure StripeSubscriptionId with HasMaxLength(255)
- [ ] Add unique index on StripeSubscriptionId
- [ ] Configure StripeCustomerId with HasMaxLength(255)
- [ ] Add index on StripeCustomerId
- [ ] Configure PaymentMethodId with HasMaxLength(255)
- [ ] Configure LastPaymentAmount with precision (10,2)
- [ ] Configure LastPaymentStatus with HasMaxLength(50)
- [ ] Configure NextPaymentAmount with precision (10,2)
- [ ] Configure FailedPaymentCount with default value 0
- [ ] Configure Quantity with default value 1
- [ ] Configure Discount with precision (5,2)
- [ ] Configure DiscountType with HasMaxLength(20)
- [ ] Configure CouponCode with HasMaxLength(50)
- [ ] Configure CreatedAt with default value sql function GETUTCDATE()
- [ ] Configure UpdatedAt with default value sql function GETUTCDATE()
- [ ] Add composite index on (Status, CurrentPeriodEnd)
- [ ] Add index on CurrentPeriodEnd
- [ ] Add index on NextPaymentDate

## IEPGoal Entity Configuration

- [ ] Create IEPGoalConfiguration.cs file in EntityConfigurations folder
- [ ] Add using statements for Microsoft.EntityFrameworkCore
- [ ] Add using statements for Microsoft.EntityFrameworkCore.Metadata.Builders
- [ ] Add namespace UPTRMS.Api.Data.EntityConfigurations
- [ ] Create class IEPGoalConfiguration implementing IEntityTypeConfiguration<IEPGoal>
- [ ] Implement Configure method
- [ ] Call ToTable("IEPGoals") on builder
- [ ] Configure Id as primary key using HasKey
- [ ] Configure StudentId as required
- [ ] Add index on StudentId
- [ ] Configure GoalArea as required with HasMaxLength(100)
- [ ] Add index on GoalArea
- [ ] Configure GoalText as required with HasMaxLength(2000)
- [ ] Configure BaselineText with HasMaxLength(1000)
- [ ] Configure TargetDate as required
- [ ] Configure MasteryLevel as required
- [ ] Configure CurrentLevel with default value 0
- [ ] Configure MeasurementMethod with HasMaxLength(500)
- [ ] Configure Frequency with HasMaxLength(100)
- [ ] Configure Status as required with HasMaxLength(50)
- [ ] Add index on Status
- [ ] Configure ProgressMonitoring with HasMaxLength(500)
- [ ] Configure ServiceType with HasMaxLength(50)
- [ ] Configure ResponsibleParty with HasMaxLength(200)
- [ ] Configure IsActive with default value true
- [ ] Configure CreatedAt with default value sql function GETUTCDATE()
- [ ] Configure UpdatedAt with default value sql function GETUTCDATE()
- [ ] Configure relationship with Student
- [ ] Set delete behavior to Cascade for Student relationship
- [ ] Configure relationship with User for Creator
- [ ] Set delete behavior to Restrict for Creator relationship
- [ ] Configure relationship with User for Updater
- [ ] Set delete behavior to SetNull for Updater relationship
- [ ] Add composite index on (StudentId, IsActive)
- [ ] Add composite index on (Status, TargetDate)
- [ ] Add index on NextReviewDate

## Junction Table Configurations

### SessionResource Configuration

- [ ] Create SessionResourceConfiguration.cs file in EntityConfigurations folder
- [ ] Add using statements for Microsoft.EntityFrameworkCore
- [ ] Add using statements for Microsoft.EntityFrameworkCore.Metadata.Builders
- [ ] Add namespace UPTRMS.Api.Data.EntityConfigurations
- [ ] Create class SessionResourceConfiguration implementing IEntityTypeConfiguration<SessionResource>
- [ ] Implement Configure method
- [ ] Call ToTable("SessionResources") on builder
- [ ] Configure composite primary key using HasKey with SessionId and ResourceId
- [ ] Configure UsedAt as required
- [ ] Configure CompletionRate with precision (5,2)
- [ ] Configure Notes with HasMaxLength(500)
- [ ] Configure relationship with Session
- [ ] Set delete behavior to Cascade for Session relationship
- [ ] Configure relationship with Resource
- [ ] Set delete behavior to Cascade for Resource relationship
- [ ] Add index on ResourceId
- [ ] Add index on UsedAt

### GoalResource Configuration

- [ ] Create GoalResourceConfiguration.cs file in EntityConfigurations folder
- [ ] Add using statements for Microsoft.EntityFrameworkCore
- [ ] Add using statements for Microsoft.EntityFrameworkCore.Metadata.Builders
- [ ] Add namespace UPTRMS.Api.Data.EntityConfigurations
- [ ] Create class GoalResourceConfiguration implementing IEntityTypeConfiguration<GoalResource>
- [ ] Implement Configure method
- [ ] Call ToTable("GoalResources") on builder
- [ ] Configure composite primary key using HasKey with GoalId and ResourceId
- [ ] Configure AssignedAt as required
- [ ] Configure AssignedBy as required
- [ ] Configure Priority with default value 0
- [ ] Configure relationship with IEPGoal
- [ ] Set delete behavior to Cascade for Goal relationship
- [ ] Configure relationship with Resource
- [ ] Set delete behavior to Cascade for Resource relationship
- [ ] Configure relationship with User for AssignedByUser
- [ ] Set delete behavior to Restrict for AssignedByUser relationship
- [ ] Add index on ResourceId
- [ ] Add index on AssignedBy

### SessionGoal Configuration

- [ ] Create SessionGoalConfiguration.cs file in EntityConfigurations folder
- [ ] Add using statements for Microsoft.EntityFrameworkCore
- [ ] Add using statements for Microsoft.EntityFrameworkCore.Metadata.Builders
- [ ] Add namespace UPTRMS.Api.Data.EntityConfigurations
- [ ] Create class SessionGoalConfiguration implementing IEntityTypeConfiguration<SessionGoal>
- [ ] Implement Configure method
- [ ] Call ToTable("SessionGoals") on builder
- [ ] Configure composite primary key using HasKey with SessionId and GoalId
- [ ] Configure ProgressMade with HasMaxLength(1000)
- [ ] Configure PercentComplete with precision (5,2)
- [ ] Configure relationship with Session
- [ ] Set delete behavior to Cascade for Session relationship
- [ ] Configure relationship with IEPGoal
- [ ] Set delete behavior to Cascade for Goal relationship
- [ ] Add index on GoalId

### OrganizationUser Configuration

- [ ] Create OrganizationUserConfiguration.cs file in EntityConfigurations folder
- [ ] Add using statements for Microsoft.EntityFrameworkCore
- [ ] Add using statements for Microsoft.EntityFrameworkCore.Metadata.Builders
- [ ] Add namespace UPTRMS.Api.Data.EntityConfigurations
- [ ] Create class OrganizationUserConfiguration implementing IEntityTypeConfiguration<OrganizationUser>
- [ ] Implement Configure method
- [ ] Call ToTable("OrganizationUsers") on builder
- [ ] Configure composite primary key using HasKey with OrganizationId and UserId
- [ ] Configure Role as required with HasMaxLength(50)
- [ ] Configure IsAdmin with default value false
- [ ] Configure JoinedAt as required
- [ ] Configure Department with HasMaxLength(100)
- [ ] Configure JobTitle with HasMaxLength(100)
- [ ] Configure relationship with Organization
- [ ] Set delete behavior to Cascade for Organization relationship
- [ ] Configure relationship with User
- [ ] Set delete behavior to Cascade for User relationship
- [ ] Configure relationship with User for InvitedByUser
- [ ] Set delete behavior to SetNull for InvitedByUser relationship
- [ ] Add index on UserId
- [ ] Add composite index on (Role, IsAdmin)

## Register All Configurations in ApplicationDbContext

- [ ] Open Data/ApplicationDbContext.cs file
- [ ] Add method override for OnModelCreating if not exists
- [ ] Add call to base.OnModelCreating(modelBuilder)
- [ ] Add modelBuilder.ApplyConfiguration(new ResourceConfiguration())
- [ ] Add modelBuilder.ApplyConfiguration(new StudentConfiguration())
- [ ] Add modelBuilder.ApplyConfiguration(new SellerProfileConfiguration())
- [ ] Add modelBuilder.ApplyConfiguration(new SessionConfiguration())
- [ ] Add modelBuilder.ApplyConfiguration(new OrganizationConfiguration())
- [ ] Add modelBuilder.ApplyConfiguration(new SubscriptionConfiguration())
- [ ] Add modelBuilder.ApplyConfiguration(new IEPGoalConfiguration())
- [ ] Add modelBuilder.ApplyConfiguration(new ResourceCategoryConfiguration())
- [ ] Add modelBuilder.ApplyConfiguration(new ResourceRatingConfiguration())
- [ ] Add modelBuilder.ApplyConfiguration(new SessionResourceConfiguration())
- [ ] Add modelBuilder.ApplyConfiguration(new GoalResourceConfiguration())
- [ ] Add modelBuilder.ApplyConfiguration(new SessionGoalConfiguration())
- [ ] Add modelBuilder.ApplyConfiguration(new OrganizationUserConfiguration())
- [ ] Add modelBuilder.ApplyConfiguration(new MarketplaceProductConfiguration())
- [ ] Add modelBuilder.ApplyConfiguration(new MarketplaceTransactionConfiguration())
- [ ] Add modelBuilder.ApplyConfiguration(new ProgressDataConfiguration())
- [ ] Add modelBuilder.ApplyConfiguration(new CommunicationLogConfiguration())

## Global Query Filters

- [ ] In ResourceConfiguration Configure method, add query filter HasQueryFilter(r => !r.IsDeleted)
- [ ] In StudentConfiguration Configure method, add query filter HasQueryFilter(s => s.IsActive)
- [ ] In SellerProfileConfiguration Configure method, add query filter HasQueryFilter(s => s.IsActive)
- [ ] In OrganizationConfiguration Configure method, add query filter HasQueryFilter(o => o.IsActive)
- [ ] In IEPGoalConfiguration Configure method, add query filter HasQueryFilter(g => g.IsActive)
- [ ] In ResourceCategoryConfiguration Configure method, add query filter HasQueryFilter(c => c.IsActive)
- [ ] In MarketplaceProductConfiguration Configure method, add query filter HasQueryFilter(p => p.IsActive)