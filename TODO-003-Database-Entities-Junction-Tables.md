# TODO-003: Database Junction Tables and Remaining Entities

These junction tables and remaining entities must be created for many-to-many relationships.

## Junction Tables

### SessionResource Junction Table

- [ ] Create SessionResource.cs file in Models/Domain folder
- [ ] Add using statements for System and System.ComponentModel.DataAnnotations
- [ ] Add namespace UPTRMS.Api.Models.Domain to SessionResource.cs
- [ ] Add public class SessionResource declaration
- [ ] Add SessionId property with type Guid
- [ ] Add [Required] attribute to SessionId property
- [ ] Add ResourceId property with type Guid
- [ ] Add [Required] attribute to ResourceId property
- [ ] Add composite key annotation for SessionId and ResourceId
- [ ] Add UsedAt property with type DateTime
- [ ] Add DurationMinutes property with type int?
- [ ] Add CompletionRate property with type decimal?
- [ ] Add [Range(0, 100)] attribute to CompletionRate property
- [ ] Add Notes property with type string
- [ ] Add [MaxLength(500)] attribute to Notes property
- [ ] Add navigation property Session with type Session
- [ ] Add navigation property Resource with type Resource

### GoalResource Junction Table

- [ ] Create GoalResource.cs file in Models/Domain folder
- [ ] Add using statements for System and System.ComponentModel.DataAnnotations
- [ ] Add namespace UPTRMS.Api.Models.Domain to GoalResource.cs
- [ ] Add public class GoalResource declaration
- [ ] Add GoalId property with type Guid
- [ ] Add [Required] attribute to GoalId property
- [ ] Add ResourceId property with type Guid
- [ ] Add [Required] attribute to ResourceId property
- [ ] Add composite key annotation for GoalId and ResourceId
- [ ] Add AssignedAt property with type DateTime
- [ ] Add AssignedBy property with type Guid
- [ ] Add Priority property with type int
- [ ] Add default value 0 to Priority property
- [ ] Add navigation property Goal with type IEPGoal
- [ ] Add navigation property Resource with type Resource
- [ ] Add navigation property AssignedByUser with type User

### SessionGoal Junction Table

- [ ] Create SessionGoal.cs file in Models/Domain folder
- [ ] Add using statements for System and System.ComponentModel.DataAnnotations
- [ ] Add namespace UPTRMS.Api.Models.Domain to SessionGoal.cs
- [ ] Add public class SessionGoal declaration
- [ ] Add SessionId property with type Guid
- [ ] Add [Required] attribute to SessionId property
- [ ] Add GoalId property with type Guid
- [ ] Add [Required] attribute to GoalId property
- [ ] Add composite key annotation for SessionId and GoalId
- [ ] Add ProgressMade property with type string
- [ ] Add [MaxLength(1000)] attribute to ProgressMade property
- [ ] Add PercentComplete property with type decimal?
- [ ] Add [Range(0, 100)] attribute to PercentComplete property
- [ ] Add navigation property Session with type Session
- [ ] Add navigation property Goal with type IEPGoal

### OrganizationUser Junction Table

- [ ] Create OrganizationUser.cs file in Models/Domain folder
- [ ] Add using statements for System and System.ComponentModel.DataAnnotations
- [ ] Add namespace UPTRMS.Api.Models.Domain to OrganizationUser.cs
- [ ] Add public class OrganizationUser declaration
- [ ] Add OrganizationId property with type Guid
- [ ] Add [Required] attribute to OrganizationId property
- [ ] Add UserId property with type Guid
- [ ] Add [Required] attribute to UserId property
- [ ] Add composite key annotation for OrganizationId and UserId
- [ ] Add Role property with type string
- [ ] Add [Required] attribute to Role property
- [ ] Add [MaxLength(50)] attribute to Role property
- [ ] Add IsAdmin property with type bool
- [ ] Add default value false to IsAdmin property
- [ ] Add JoinedAt property with type DateTime
- [ ] Add InvitedBy property with type Guid?
- [ ] Add Department property with type string
- [ ] Add [MaxLength(100)] attribute to Department property
- [ ] Add JobTitle property with type string
- [ ] Add [MaxLength(100)] attribute to JobTitle property
- [ ] Add navigation property Organization with type Organization
- [ ] Add navigation property User with type User
- [ ] Add navigation property InvitedByUser with type User

## Additional Core Entities

### MarketplaceProduct Entity

- [ ] Create MarketplaceProduct.cs file in Models/Domain folder
- [ ] Add using statements for System and System.ComponentModel.DataAnnotations
- [ ] Add namespace UPTRMS.Api.Models.Domain to MarketplaceProduct.cs
- [ ] Add public class MarketplaceProduct declaration
- [ ] Add Id property with type Guid
- [ ] Add [Key] attribute to Id property
- [ ] Add SellerId property with type Guid (foreign key to SellerProfile)
- [ ] Add [Required] attribute to SellerId property
- [ ] Add ResourceId property with type Guid (foreign key to Resource)
- [ ] Add [Required] attribute to ResourceId property
- [ ] Add Title property with type string
- [ ] Add [Required] attribute to Title property
- [ ] Add [MaxLength(200)] attribute to Title property
- [ ] Add Description property with type string
- [ ] Add [Required] attribute to Description property
- [ ] Add [MaxLength(4000)] attribute to Description property
- [ ] Add Price property with type decimal
- [ ] Add [Required] attribute to Price property
- [ ] Add [Range(0.99, 9999.99)] attribute to Price property
- [ ] Add OriginalPrice property with type decimal?
- [ ] Add IsOnSale property with type bool
- [ ] Add default value false to IsOnSale property
- [ ] Add SaleEndsAt property with type DateTime? (nullable)
- [ ] Add LicenseType property with type string
- [ ] Add [Required] attribute to LicenseType property
- [ ] Add [MaxLength(50)] attribute to LicenseType property
- [ ] Add PreviewImages property with type string (will store JSON array)
- [ ] Add PreviewPdfUrl property with type string
- [ ] Add [MaxLength(1000)] attribute to PreviewPdfUrl property
- [ ] Add Tags property with type string (will store JSON array)
- [ ] Add Categories property with type string (will store JSON array)
- [ ] Add TotalSales property with type int
- [ ] Add default value 0 to TotalSales property
- [ ] Add TotalRevenue property with type decimal
- [ ] Add default value 0.0m to TotalRevenue property
- [ ] Add AverageRating property with type decimal?
- [ ] Add TotalRatings property with type int
- [ ] Add default value 0 to TotalRatings property
- [ ] Add IsActive property with type bool
- [ ] Add default value true to IsActive property
- [ ] Add IsFeatured property with type bool
- [ ] Add default value false to IsFeatured property
- [ ] Add PublishedAt property with type DateTime
- [ ] Add UpdatedAt property with type DateTime
- [ ] Add navigation property Seller with type SellerProfile
- [ ] Add navigation property Resource with type Resource
- [ ] Add navigation property Transactions with type ICollection<MarketplaceTransaction>
- [ ] Add navigation property Reviews with type ICollection<ProductReview>

### MarketplaceTransaction Entity

- [ ] Create MarketplaceTransaction.cs file in Models/Domain folder
- [ ] Add using statements for System and System.ComponentModel.DataAnnotations
- [ ] Add namespace UPTRMS.Api.Models.Domain to MarketplaceTransaction.cs
- [ ] Add public class MarketplaceTransaction declaration
- [ ] Add Id property with type Guid
- [ ] Add [Key] attribute to Id property
- [ ] Add BuyerId property with type Guid (foreign key to User)
- [ ] Add [Required] attribute to BuyerId property
- [ ] Add SellerId property with type Guid (foreign key to SellerProfile)
- [ ] Add [Required] attribute to SellerId property
- [ ] Add ProductId property with type Guid (foreign key to MarketplaceProduct)
- [ ] Add [Required] attribute to ProductId property
- [ ] Add Amount property with type decimal
- [ ] Add [Required] attribute to Amount property
- [ ] Add Commission property with type decimal
- [ ] Add [Required] attribute to Commission property
- [ ] Add SellerEarnings property with type decimal
- [ ] Add [Required] attribute to SellerEarnings property
- [ ] Add PaymentStatus property with type string
- [ ] Add [Required] attribute to PaymentStatus property
- [ ] Add [MaxLength(50)] attribute to PaymentStatus property
- [ ] Add PaymentMethod property with type string
- [ ] Add [MaxLength(50)] attribute to PaymentMethod property
- [ ] Add StripePaymentIntentId property with type string
- [ ] Add [MaxLength(255)] attribute to StripePaymentIntentId property
- [ ] Add StripeChargeId property with type string
- [ ] Add [MaxLength(255)] attribute to StripeChargeId property
- [ ] Add RefundAmount property with type decimal?
- [ ] Add RefundedAt property with type DateTime? (nullable)
- [ ] Add RefundReason property with type string
- [ ] Add [MaxLength(500)] attribute to RefundReason property
- [ ] Add DownloadUrl property with type string
- [ ] Add [MaxLength(1000)] attribute to DownloadUrl property
- [ ] Add DownloadExpiry property with type DateTime
- [ ] Add DownloadCount property with type int
- [ ] Add default value 0 to DownloadCount property
- [ ] Add MaxDownloads property with type int
- [ ] Add default value 5 to MaxDownloads property
- [ ] Add LicenseKey property with type string
- [ ] Add [MaxLength(100)] attribute to LicenseKey property
- [ ] Add CreatedAt property with type DateTime
- [ ] Add navigation property Buyer with type User
- [ ] Add navigation property Seller with type SellerProfile
- [ ] Add navigation property Product with type MarketplaceProduct

### ProgressData Entity

- [ ] Create ProgressData.cs file in Models/Domain folder
- [ ] Add using statements for System and System.ComponentModel.DataAnnotations
- [ ] Add namespace UPTRMS.Api.Models.Domain to ProgressData.cs
- [ ] Add public class ProgressData declaration
- [ ] Add Id property with type Guid
- [ ] Add [Key] attribute to Id property
- [ ] Add StudentId property with type Guid (foreign key to Student)
- [ ] Add [Required] attribute to StudentId property
- [ ] Add SessionId property with type Guid? (foreign key to Session, nullable)
- [ ] Add GoalId property with type Guid? (foreign key to IEPGoal, nullable)
- [ ] Add DataType property with type string
- [ ] Add [Required] attribute to DataType property
- [ ] Add [MaxLength(50)] attribute to DataType property
- [ ] Add DataValue property with type string
- [ ] Add [Required] attribute to DataValue property
- [ ] Add NumericValue property with type decimal?
- [ ] Add Unit property with type string
- [ ] Add [MaxLength(20)] attribute to Unit property
- [ ] Add CollectedAt property with type DateTime
- [ ] Add CollectedBy property with type Guid (foreign key to User)
- [ ] Add Method property with type string
- [ ] Add [MaxLength(100)] attribute to Method property
- [ ] Add Context property with type string
- [ ] Add [MaxLength(500)] attribute to Context property
- [ ] Add Notes property with type string
- [ ] Add [MaxLength(1000)] attribute to Notes property
- [ ] Add IsBaseline property with type bool
- [ ] Add default value false to IsBaseline property
- [ ] Add navigation property Student with type Student
- [ ] Add navigation property Session with type Session
- [ ] Add navigation property Goal with type IEPGoal
- [ ] Add navigation property Collector with type User

### CommunicationLog Entity

- [ ] Create CommunicationLog.cs file in Models/Domain folder
- [ ] Add using statements for System and System.ComponentModel.DataAnnotations
- [ ] Add namespace UPTRMS.Api.Models.Domain to CommunicationLog.cs
- [ ] Add public class CommunicationLog declaration
- [ ] Add Id property with type Guid
- [ ] Add [Key] attribute to Id property
- [ ] Add StudentId property with type Guid (foreign key to Student)
- [ ] Add [Required] attribute to StudentId property
- [ ] Add SenderId property with type Guid (foreign key to User)
- [ ] Add [Required] attribute to SenderId property
- [ ] Add RecipientEmail property with type string
- [ ] Add [EmailAddress] attribute to RecipientEmail property
- [ ] Add [MaxLength(255)] attribute to RecipientEmail property
- [ ] Add RecipientPhone property with type string
- [ ] Add [Phone] attribute to RecipientPhone property
- [ ] Add [MaxLength(20)] attribute to RecipientPhone property
- [ ] Add CommunicationType property with type string
- [ ] Add [Required] attribute to CommunicationType property
- [ ] Add [MaxLength(50)] attribute to CommunicationType property
- [ ] Add Subject property with type string
- [ ] Add [MaxLength(500)] attribute to Subject property
- [ ] Add Message property with type string
- [ ] Add [Required] attribute to Message property
- [ ] Add AttachmentUrls property with type string (will store JSON array)
- [ ] Add Status property with type string
- [ ] Add [Required] attribute to Status property
- [ ] Add [MaxLength(50)] attribute to Status property
- [ ] Add SentAt property with type DateTime
- [ ] Add OpenedAt property with type DateTime? (nullable)
- [ ] Add ResponseReceivedAt property with type DateTime? (nullable)
- [ ] Add ResponseText property with type string
- [ ] Add navigation property Student with type Student
- [ ] Add navigation property Sender with type User