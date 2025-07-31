# TODO-006: Database Migrations

These tasks create and apply database migrations after all entities are configured.

## Create Initial Comprehensive Migration

- [ ] Open terminal in api directory
- [ ] Run dotnet build to ensure project compiles
- [ ] Fix any compilation errors from entity models
- [ ] Run dotnet ef migrations add AddCoreEntities
- [ ] Check migration file for any warnings
- [ ] Review Up method in migration file
- [ ] Review Down method in migration file
- [ ] Check all tables are included in migration
- [ ] Check all indexes are included in migration
- [ ] Check all foreign keys are properly configured
- [ ] Check all default values are set correctly
- [ ] Check all constraints are included

## Manual Migration Adjustments

- [ ] Open the AddCoreEntities migration file
- [ ] Add check constraint for Resource.EvidenceLevel BETWEEN 1 AND 5
- [ ] Add check constraint for ResourceRating.Rating BETWEEN 1 AND 5
- [ ] Add check constraint for IEPGoal.MasteryLevel BETWEEN 0 AND 100
- [ ] Add check constraint for IEPGoal.CurrentLevel BETWEEN 0 AND 100
- [ ] Add check constraint for SellerProfile.Rating BETWEEN 0 AND 5
- [ ] Add check constraint for SellerProfile.CommissionRate BETWEEN 0 AND 1
- [ ] Add check constraint for SessionResource.CompletionRate BETWEEN 0 AND 100
- [ ] Add check constraint for SessionGoal.PercentComplete BETWEEN 0 AND 100
- [ ] Add trigger for UpdatedAt auto-update on Resources table
- [ ] Add trigger for UpdatedAt auto-update on Students table
- [ ] Add trigger for UpdatedAt auto-update on Sessions table
- [ ] Add trigger for UpdatedAt auto-update on Organizations table
- [ ] Add trigger for UpdatedAt auto-update on Subscriptions table
- [ ] Add trigger for UpdatedAt auto-update on IEPGoals table
- [ ] Add trigger for UpdatedAt auto-update on SellerProfiles table

## Create Stored Procedures Migration

- [ ] Run dotnet ef migrations add AddStoredProcedures
- [ ] Open the AddStoredProcedures migration file
- [ ] Add CREATE PROCEDURE sp_GetStudentsByTherapist in Up method
- [ ] Add parameters @TherapistId uniqueidentifier
- [ ] Add parameters @IncludeInactive bit = 0
- [ ] Write SELECT query joining Students with latest session data
- [ ] Add ORDER BY LastName, FirstName
- [ ] Add DROP PROCEDURE sp_GetStudentsByTherapist in Down method
- [ ] Add CREATE PROCEDURE sp_GetResourcesByGoal in Up method
- [ ] Add parameters @GoalId uniqueidentifier
- [ ] Write SELECT query joining Resources through GoalResources
- [ ] Add ORDER BY Priority, Title
- [ ] Add DROP PROCEDURE sp_GetResourcesByGoal in Down method
- [ ] Add CREATE PROCEDURE sp_GetSessionReport in Up method
- [ ] Add parameters @SessionId uniqueidentifier
- [ ] Write complex SELECT joining Sessions, Students, Goals, Resources
- [ ] Add FOR JSON PATH to return as JSON
- [ ] Add DROP PROCEDURE sp_GetSessionReport in Down method
- [ ] Add CREATE PROCEDURE sp_GetTherapistProductivity in Up method
- [ ] Add parameters @TherapistId uniqueidentifier
- [ ] Add parameters @StartDate datetime
- [ ] Add parameters @EndDate datetime
- [ ] Write query to calculate sessions, students seen, goals addressed
- [ ] Add DROP PROCEDURE sp_GetTherapistProductivity in Down method
- [ ] Add CREATE PROCEDURE sp_GetMarketplaceBestSellers in Up method
- [ ] Add parameters @Days int = 30
- [ ] Add parameters @Limit int = 10
- [ ] Write query to get top selling products
- [ ] Add ORDER BY TotalSales DESC
- [ ] Add DROP PROCEDURE sp_GetMarketplaceBestSellers in Down method

## Create Views Migration

- [ ] Run dotnet ef migrations add AddDatabaseViews
- [ ] Open the AddDatabaseViews migration file
- [ ] Add CREATE VIEW vw_ActiveStudents in Up method
- [ ] Define view selecting from Students WHERE IsActive = 1
- [ ] Include therapist name join
- [ ] Include organization name join
- [ ] Include last session date subquery
- [ ] Add DROP VIEW vw_ActiveStudents in Down method
- [ ] Add CREATE VIEW vw_ResourceUsageStats in Up method
- [ ] Define view aggregating resource usage data
- [ ] Include download count
- [ ] Include average rating
- [ ] Include session usage count
- [ ] Add DROP VIEW vw_ResourceUsageStats in Down method
- [ ] Add CREATE VIEW vw_GoalProgress in Up method
- [ ] Define view showing goal progress over time
- [ ] Include baseline data
- [ ] Include current progress
- [ ] Include days until target date
- [ ] Add DROP VIEW vw_GoalProgress in Down method
- [ ] Add CREATE VIEW vw_SellerDashboard in Up method
- [ ] Define view aggregating seller metrics
- [ ] Include total sales
- [ ] Include average rating
- [ ] Include pending payouts
- [ ] Include product count
- [ ] Add DROP VIEW vw_SellerDashboard in Down method
- [ ] Add CREATE VIEW vw_OrganizationUsage in Up method
- [ ] Define view showing organization usage stats
- [ ] Include user count
- [ ] Include storage usage
- [ ] Include active students
- [ ] Include session count this month
- [ ] Add DROP VIEW vw_OrganizationUsage in Down method

## Create Indexes Migration

- [ ] Run dotnet ef migrations add AddPerformanceIndexes
- [ ] Open the AddPerformanceIndexes migration file
- [ ] Add index on Resources (CreatedAt DESC) INCLUDE (Title, ThumbnailUrl)
- [ ] Add index on Resources (DownloadCount DESC) WHERE IsDeleted = 0
- [ ] Add index on Resources (AverageRating DESC) WHERE TotalRatings > 5
- [ ] Add index on Sessions (ScheduledAt) INCLUDE (TherapistId, StudentId) WHERE SessionStatus = 'Scheduled'
- [ ] Add index on Students (TherapistId, IsActive) INCLUDE (FirstName, LastName)
- [ ] Add index on MarketplaceProducts (PublishedAt DESC) WHERE IsActive = 1
- [ ] Add index on MarketplaceTransactions (CreatedAt DESC) INCLUDE (Amount)
- [ ] Add index on IEPGoals (NextReviewDate) WHERE IsActive = 1
- [ ] Add index on ProgressData (StudentId, CollectedAt DESC)
- [ ] Add index on CommunicationLogs (StudentId, SentAt DESC)
- [ ] Add full-text index on Resources (Title, Description, Tags)
- [ ] Add full-text index on MarketplaceProducts (Title, Description, Tags)

## Create Data Seeding Migration

- [ ] Run dotnet ef migrations add AddSeedData
- [ ] Open the AddSeedData migration file
- [ ] Add INSERT for default ResourceCategories in Up method
- [ ] Insert 'Fine Motor Skills' category
- [ ] Insert 'Gross Motor Skills' category
- [ ] Insert 'Speech & Language' category
- [ ] Insert 'Social Skills' category
- [ ] Insert 'Sensory Integration' category
- [ ] Insert 'Academic Support' category
- [ ] Insert 'Daily Living Skills' category
- [ ] Insert 'Behavior Management' category
- [ ] Add INSERT for system users in Up method
- [ ] Insert system admin user
- [ ] Insert demo therapist user
- [ ] Insert demo parent user
- [ ] Add INSERT for demo organization in Up method
- [ ] Insert 'Demo School District' organization
- [ ] Add INSERT for subscription plans in Up method
- [ ] Insert 'Basic Individual' plan
- [ ] Insert 'Pro Individual' plan
- [ ] Insert 'Small Group' plan
- [ ] Insert 'Large Group' plan
- [ ] Insert 'Enterprise' plan
- [ ] Add DELETE statements in Down method for all seed data

## Apply Migrations

- [ ] Run dotnet ef database update to apply all migrations
- [ ] Check for any errors during migration
- [ ] Verify all tables created successfully
- [ ] Verify all indexes created successfully
- [ ] Verify all stored procedures created successfully
- [ ] Verify all views created successfully
- [ ] Verify seed data inserted successfully
- [ ] Run dotnet ef migrations list to verify all applied
- [ ] Test a simple query against each table
- [ ] Test each stored procedure manually
- [ ] Test each view returns data correctly