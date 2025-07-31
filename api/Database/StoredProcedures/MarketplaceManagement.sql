-- =============================================
-- Marketplace Management Stored Procedures
-- =============================================

-- Get marketplace transaction by ID
CREATE OR ALTER PROCEDURE [dbo].[sp_GetMarketplaceTransactionById]
    @TransactionId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT mt.*, 
           b.Email as BuyerEmail, b.FirstName as BuyerFirstName, b.LastName as BuyerLastName,
           s.Email as SellerEmail, s.FirstName as SellerFirstName, s.LastName as SellerLastName
    FROM MarketplaceTransactions mt
    INNER JOIN Users b ON mt.BuyerId = b.UserId
    INNER JOIN Users s ON mt.SellerId = s.UserId
    WHERE mt.TransactionId = @TransactionId;
END
GO

-- Get marketplace transactions
CREATE OR ALTER PROCEDURE [dbo].[sp_GetMarketplaceTransactions]
    @BuyerId UNIQUEIDENTIFIER = NULL,
    @SellerId UNIQUEIDENTIFIER = NULL,
    @ResourceId UNIQUEIDENTIFIER = NULL,
    @PaymentStatus INT = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Count total matching records
    SELECT @TotalCount = COUNT(*)
    FROM MarketplaceTransactions mt
    WHERE (@BuyerId IS NULL OR mt.BuyerId = @BuyerId)
        AND (@SellerId IS NULL OR mt.SellerId = @SellerId)
        AND (@ResourceId IS NULL OR JSON_VALUE(mt.ResourceIds, '$') LIKE '%' + CAST(@ResourceId AS NVARCHAR(36)) + '%')
        AND (@PaymentStatus IS NULL OR mt.PaymentStatus = @PaymentStatus)
        AND (@StartDate IS NULL OR mt.CreatedAt >= @StartDate)
        AND (@EndDate IS NULL OR mt.CreatedAt <= @EndDate);
    
    -- Return paginated results
    SELECT mt.*, 
           b.Email as BuyerEmail, b.FirstName as BuyerFirstName, b.LastName as BuyerLastName,
           s.Email as SellerEmail, s.FirstName as SellerFirstName, s.LastName as SellerLastName
    FROM MarketplaceTransactions mt
    INNER JOIN Users b ON mt.BuyerId = b.UserId
    INNER JOIN Users s ON mt.SellerId = s.UserId
    WHERE (@BuyerId IS NULL OR mt.BuyerId = @BuyerId)
        AND (@SellerId IS NULL OR mt.SellerId = @SellerId)
        AND (@ResourceId IS NULL OR JSON_VALUE(mt.ResourceIds, '$') LIKE '%' + CAST(@ResourceId AS NVARCHAR(36)) + '%')
        AND (@PaymentStatus IS NULL OR mt.PaymentStatus = @PaymentStatus)
        AND (@StartDate IS NULL OR mt.CreatedAt >= @StartDate)
        AND (@EndDate IS NULL OR mt.CreatedAt <= @EndDate)
    ORDER BY mt.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Create marketplace transaction
CREATE OR ALTER PROCEDURE [dbo].[sp_CreateMarketplaceTransaction]
    @TransactionId UNIQUEIDENTIFIER OUTPUT,
    @BuyerId UNIQUEIDENTIFIER,
    @SellerId UNIQUEIDENTIFIER,
    @ResourceIds NVARCHAR(MAX),
    @Amount DECIMAL(10,2),
    @Commission DECIMAL(10,2),
    @PaymentStatus INT,
    @PaymentIntentId NVARCHAR(255) = NULL,
    @PaymentMethod NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @TransactionId IS NULL OR @TransactionId = '00000000-0000-0000-0000-000000000000'
        SET @TransactionId = NEWID();
    
    INSERT INTO MarketplaceTransactions (
        TransactionId, BuyerId, SellerId, ResourceIds,
        Amount, Commission, PaymentStatus, PaymentIntentId,
        PaymentMethod, CreatedAt, UpdatedAt
    )
    VALUES (
        @TransactionId, @BuyerId, @SellerId, @ResourceIds,
        @Amount, @Commission, @PaymentStatus, @PaymentIntentId,
        @PaymentMethod, GETUTCDATE(), GETUTCDATE()
    );
    
    -- Update resource download counts if payment successful
    IF @PaymentStatus = 2 -- Assuming 2 = Completed
    BEGIN
        -- Parse JSON array of resource IDs and update download counts
        -- This would need proper JSON parsing in production
        UPDATE Resources
        SET DownloadCount = DownloadCount + 1
        WHERE ResourceId IN (
            SELECT value FROM OPENJSON(@ResourceIds)
        );
    END
END
GO

-- Update marketplace transaction status
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateMarketplaceTransactionStatus]
    @TransactionId UNIQUEIDENTIFIER,
    @PaymentStatus INT,
    @PaymentIntentId NVARCHAR(255) = NULL,
    @ProcessedAt DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE MarketplaceTransactions
    SET PaymentStatus = @PaymentStatus,
        PaymentIntentId = ISNULL(@PaymentIntentId, PaymentIntentId),
        ProcessedAt = ISNULL(@ProcessedAt, ProcessedAt),
        UpdatedAt = GETUTCDATE()
    WHERE TransactionId = @TransactionId;
    
    -- Update resource download counts if payment just completed
    IF @PaymentStatus = 2 -- Assuming 2 = Completed
    BEGIN
        DECLARE @ResourceIds NVARCHAR(MAX);
        SELECT @ResourceIds = ResourceIds 
        FROM MarketplaceTransactions 
        WHERE TransactionId = @TransactionId;
        
        UPDATE Resources
        SET DownloadCount = DownloadCount + 1
        WHERE ResourceId IN (
            SELECT value FROM OPENJSON(@ResourceIds)
        );
    END
END
GO

-- Get seller revenue statistics
CREATE OR ALTER PROCEDURE [dbo].[sp_GetSellerRevenueStatistics]
    @SellerId UNIQUEIDENTIFIER,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Set default date range if not provided (last 30 days)
    IF @StartDate IS NULL
        SET @StartDate = DATEADD(DAY, -30, GETUTCDATE());
    IF @EndDate IS NULL
        SET @EndDate = GETUTCDATE();
    
    -- Summary statistics
    SELECT 
        COUNT(*) as TotalTransactions,
        SUM(Amount) as GrossRevenue,
        SUM(Amount - Commission) as NetRevenue,
        SUM(Commission) as TotalCommission,
        AVG(Amount) as AverageTransactionValue,
        COUNT(DISTINCT BuyerId) as UniqueBuyers
    FROM MarketplaceTransactions
    WHERE SellerId = @SellerId
        AND PaymentStatus = 2 -- Completed
        AND CreatedAt BETWEEN @StartDate AND @EndDate;
    
    -- Revenue by resource
    SELECT 
        r.ResourceId,
        r.Title,
        COUNT(*) as SalesCount,
        SUM(mt.Amount / resourceCount.cnt) as ResourceRevenue
    FROM MarketplaceTransactions mt
    CROSS APPLY OPENJSON(mt.ResourceIds) as rids
    INNER JOIN Resources r ON r.ResourceId = CAST(rids.value AS UNIQUEIDENTIFIER)
    CROSS APPLY (
        SELECT COUNT(*) as cnt 
        FROM OPENJSON(mt.ResourceIds)
    ) as resourceCount
    WHERE mt.SellerId = @SellerId
        AND mt.PaymentStatus = 2
        AND mt.CreatedAt BETWEEN @StartDate AND @EndDate
    GROUP BY r.ResourceId, r.Title
    ORDER BY ResourceRevenue DESC;
    
    -- Daily revenue trend
    SELECT 
        CAST(CreatedAt AS DATE) as Date,
        COUNT(*) as TransactionCount,
        SUM(Amount) as DailyGrossRevenue,
        SUM(Amount - Commission) as DailyNetRevenue
    FROM MarketplaceTransactions
    WHERE SellerId = @SellerId
        AND PaymentStatus = 2
        AND CreatedAt BETWEEN @StartDate AND @EndDate
    GROUP BY CAST(CreatedAt AS DATE)
    ORDER BY Date;
END
GO

-- Get buyer purchase history
CREATE OR ALTER PROCEDURE [dbo].[sp_GetBuyerPurchaseHistory]
    @BuyerId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Count total purchases
    SELECT @TotalCount = COUNT(*)
    FROM MarketplaceTransactions
    WHERE BuyerId = @BuyerId
        AND PaymentStatus = 2; -- Completed
    
    -- Return paginated purchase history with resource details
    SELECT 
        mt.TransactionId,
        mt.CreatedAt as PurchaseDate,
        mt.Amount,
        mt.ResourceIds,
        s.UserId as SellerId,
        s.FirstName + ' ' + s.LastName as SellerName,
        s.Email as SellerEmail
    FROM MarketplaceTransactions mt
    INNER JOIN Users s ON mt.SellerId = s.UserId
    WHERE mt.BuyerId = @BuyerId
        AND mt.PaymentStatus = 2
    ORDER BY mt.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Get marketplace analytics
CREATE OR ALTER PROCEDURE [dbo].[sp_GetMarketplaceAnalytics]
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Set default date range if not provided (last 30 days)
    IF @StartDate IS NULL
        SET @StartDate = DATEADD(DAY, -30, GETUTCDATE());
    IF @EndDate IS NULL
        SET @EndDate = GETUTCDATE();
    
    -- Overall marketplace statistics
    SELECT 
        COUNT(*) as TotalTransactions,
        COUNT(DISTINCT BuyerId) as UniqueBuyers,
        COUNT(DISTINCT SellerId) as ActiveSellers,
        SUM(Amount) as TotalGrossRevenue,
        SUM(Commission) as TotalCommission,
        AVG(Amount) as AverageTransactionValue
    FROM MarketplaceTransactions
    WHERE PaymentStatus = 2
        AND CreatedAt BETWEEN @StartDate AND @EndDate;
    
    -- Top sellers by revenue
    SELECT TOP 10
        s.UserId,
        s.FirstName + ' ' + s.LastName as SellerName,
        COUNT(*) as TransactionCount,
        SUM(mt.Amount) as GrossRevenue,
        SUM(mt.Amount - mt.Commission) as NetRevenue
    FROM MarketplaceTransactions mt
    INNER JOIN Users s ON mt.SellerId = s.UserId
    WHERE mt.PaymentStatus = 2
        AND mt.CreatedAt BETWEEN @StartDate AND @EndDate
    GROUP BY s.UserId, s.FirstName, s.LastName
    ORDER BY GrossRevenue DESC;
    
    -- Top resources by sales
    SELECT TOP 10
        r.ResourceId,
        r.Title,
        COUNT(*) as SalesCount,
        SUM(mt.Amount / resourceCount.cnt) as ResourceRevenue
    FROM MarketplaceTransactions mt
    CROSS APPLY OPENJSON(mt.ResourceIds) as rids
    INNER JOIN Resources r ON r.ResourceId = CAST(rids.value AS UNIQUEIDENTIFIER)
    CROSS APPLY (
        SELECT COUNT(*) as cnt 
        FROM OPENJSON(mt.ResourceIds)
    ) as resourceCount
    WHERE mt.PaymentStatus = 2
        AND mt.CreatedAt BETWEEN @StartDate AND @EndDate
    GROUP BY r.ResourceId, r.Title
    ORDER BY SalesCount DESC;
    
    -- Revenue trend by day
    SELECT 
        CAST(CreatedAt AS DATE) as Date,
        COUNT(*) as TransactionCount,
        SUM(Amount) as DailyRevenue,
        SUM(Commission) as DailyCommission
    FROM MarketplaceTransactions
    WHERE PaymentStatus = 2
        AND CreatedAt BETWEEN @StartDate AND @EndDate
    GROUP BY CAST(CreatedAt AS DATE)
    ORDER BY Date;
END
GO

-- Process seller payout
CREATE OR ALTER PROCEDURE [dbo].[sp_ProcessSellerPayout]
    @PayoutId UNIQUEIDENTIFIER OUTPUT,
    @SellerId UNIQUEIDENTIFIER,
    @Amount DECIMAL(10,2),
    @PayoutMethod NVARCHAR(50),
    @PayoutReference NVARCHAR(255) = NULL,
    @TransactionIds NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @PayoutId IS NULL OR @PayoutId = '00000000-0000-0000-0000-000000000000'
        SET @PayoutId = NEWID();
    
    -- Create payout record
    INSERT INTO SellerPayouts (
        PayoutId, SellerId, Amount, PayoutMethod,
        PayoutReference, TransactionIds, Status,
        CreatedAt, ProcessedAt
    )
    VALUES (
        @PayoutId, @SellerId, @Amount, @PayoutMethod,
        @PayoutReference, @TransactionIds, 'Processed',
        GETUTCDATE(), GETUTCDATE()
    );
    
    -- Mark related transactions as paid out
    IF @TransactionIds IS NOT NULL
    BEGIN
        UPDATE MarketplaceTransactions
        SET PayoutId = @PayoutId,
            UpdatedAt = GETUTCDATE()
        WHERE TransactionId IN (
            SELECT value FROM OPENJSON(@TransactionIds)
        );
    END
END
GO

-- Get seller payout history
CREATE OR ALTER PROCEDURE [dbo].[sp_GetSellerPayoutHistory]
    @SellerId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Count total payouts
    SELECT @TotalCount = COUNT(*)
    FROM SellerPayouts
    WHERE SellerId = @SellerId;
    
    -- Return paginated payout history
    SELECT *
    FROM SellerPayouts
    WHERE SellerId = @SellerId
    ORDER BY CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO