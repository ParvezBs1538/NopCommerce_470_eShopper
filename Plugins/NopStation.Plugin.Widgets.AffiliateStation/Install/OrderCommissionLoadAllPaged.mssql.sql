CREATE PROCEDURE [dbo].[NS_OrderCommissionLoadAllPaged]
(
	@AffiliateFirstName			nvarchar(MAX) = null,
	@AffiliateLastName			nvarchar(MAX) = null,
	@AffiliateId				int = 0,
	@OrderId					int = 0,
	@StartDateUtc				datetime = null,	
	@EndDateUtc					datetime = null,
	@FilteredComssionStatusIds	nvarchar(MAX) = null,
	@FilteredOrderStatusIds		nvarchar(MAX) = null,
	@FilteredPaymentStatusIds	nvarchar(MAX) = null,
	@PageIndex					int = 0, 
	@PageSize					int = 2147483644,
	@LoadCommission				bit = 0,
	@TotalRecords				int = null OUTPUT,
	@TotalCommission			decimal(18, 6) = null OUTPUT,
	@PayableCommission			decimal(18, 6) = null OUTPUT,
	@PaidCommission				decimal(18, 6) = null OUTPUT
)
AS
BEGIN

	SET @AffiliateFirstName = ISNULL(@AffiliateFirstName, '')
	SET @AffiliateLastName = ISNULL(@AffiliateLastName, '')
	SET @FilteredComssionStatusIds = ISNULL(@FilteredComssionStatusIds, '')
	SET @FilteredOrderStatusIds = ISNULL(@FilteredOrderStatusIds, '')
	SET @FilteredPaymentStatusIds = ISNULL(@FilteredPaymentStatusIds, '')

	DECLARE @sql nvarchar(max)

	SET NOCOUNT ON
	
	CREATE TABLE #PageIndex 
	(
		[IndexId] int IDENTITY (1, 1) NOT NULL,
		[OrderCommissionId] int NOT NULL
	)

	SET @sql = 'INSERT INTO #PageIndex ([OrderCommissionId])
		SELECT oc.Id from NS_OrderCommission oc 
        INNER JOIN Affiliate af on af.Id = oc.AffiliateId 
		INNER JOIN [Order] o ON o.Id = oc.OrderId '

	if(@AffiliateFirstName != '' OR @AffiliateLastName != '')
		SET @sql = @sql + 'INNER JOIN [Address] ad on ad.Id = af.AddressId '

	SET @sql = @sql + 'WHERE af.Deleted = 0 '
	
	if(@AffiliateFirstName != '')
		SET @sql = @sql + 'AND ad.FirstName LIKE  ''%' + @AffiliateFirstName + '%'' '
	
	if(@AffiliateLastName != '')
		SET @sql = @sql + 'AND ad.LastName LIKE  ''%' + @AffiliateLastName + '%'' '
		
	if(@AffiliateId > 0)
		SET @sql = @sql + 'AND oc.AffiliateId = ' + CAST(@AffiliateId as NVARCHAR(MAX)) + ' '
		
	if(@OrderId > 0)
		SET @sql = @sql + 'AND oc.OrderId = ' + CAST(@OrderId as NVARCHAR(MAX)) + ' '
		
	if(@StartDateUtc IS NOT NULL)
		SET @sql = @sql + 'AND o.CreatedOnUtc >= ''' + CAST(@StartDateUtc as NVARCHAR(MAX)) + ''' '
		
	if(@EndDateUtc IS NOT NULL)
		SET @sql = @sql + 'AND o.CreatedOnUtc <= ''' + CAST(@EndDateUtc as NVARCHAR(MAX)) + ''' '
		
	if(@FilteredComssionStatusIds != '')
		SET @sql = @sql + 'AND oc.CommissionStatusId IN (' + @FilteredComssionStatusIds + ') '
		
	if(@FilteredOrderStatusIds != '')
		SET @sql = @sql + 'AND o.OrderStatusId IN (' + @FilteredOrderStatusIds + ') '
		
	if(@FilteredPaymentStatusIds != '')
		SET @sql = @sql + 'AND o.PaymentStatusId IN (' + @FilteredPaymentStatusIds + ') '

	SET @sql = @sql + 'ORDER BY o.CreatedOnUtc desc '
	
	--PRINT (@sql)
	EXEC sp_executesql @sql

	--total records
	SET @TotalRecords = @@rowcount

	if(@LoadCommission = 1)
		BEGIN
			SELECT @TotalCommission = Sum(oc.TotalCommissionAmount)
			FROM
				#PageIndex [pi]
				INNER JOIN NS_OrderCommission oc with (NOLOCK) on oc.Id = [pi].OrderCommissionId

			SELECT @PayableCommission = Sum(oc.TotalCommissionAmount)
			FROM
				#PageIndex [pi]
				INNER JOIN NS_OrderCommission oc with (NOLOCK) on oc.Id = [pi].OrderCommissionId
				INNER JOIN [Order] o with (NOLOCK) on o.Id = [oc].OrderId
				WHERE o.PaymentStatusId = 30
				
			SELECT @PaidCommission = Sum(
				CASE 
					WHEN oc.CommissionStatusId = 20 THEN oc.PartialPaidAmount
					ELSE oc.TotalCommissionAmount
				END)
			FROM
				#PageIndex [pi]
				INNER JOIN NS_OrderCommission oc with (NOLOCK) on oc.Id = [pi].OrderCommissionId
				INNER JOIN [Order] o with (NOLOCK) on o.Id = [oc].OrderId
				WHERE o.PaymentStatusId = 30 AND (oc.CommissionStatusId = 30 OR oc.CommissionStatusId = 20) 
		END

	--paging
	DECLARE @PageLowerBound int
	DECLARE @PageUpperBound int
	DECLARE @RowsToReturn int
	SET @RowsToReturn = @PageSize * (@PageIndex + 1)	
	SET @PageLowerBound = @PageSize * @PageIndex
	SET @PageUpperBound = @PageLowerBound + @PageSize + 1
	
	--return products
	SELECT TOP (@RowsToReturn)
		oc.*
	FROM
		#PageIndex [pi]
		INNER JOIN NS_OrderCommission oc with (NOLOCK) on oc.Id = [pi].OrderCommissionId
	WHERE
		[pi].IndexId > @PageLowerBound AND 
		[pi].IndexId < @PageUpperBound
	ORDER BY
		[oc].OrderId DESC
	
	DROP TABLE #PageIndex
END
