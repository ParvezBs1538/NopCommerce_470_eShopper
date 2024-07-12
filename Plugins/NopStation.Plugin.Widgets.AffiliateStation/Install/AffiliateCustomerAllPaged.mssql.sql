CREATE PROCEDURE [dbo].[NS_AffiliateCustomerAllPaged]
(
	@CustomerEmail				nvarchar(MAX) = null,
	@AffiliateFirstName			nvarchar(MAX) = null,
	@AffiliateLastName			nvarchar(MAX) = null,
	@Active						bit = null,
	@CreatedFromUtc				datetime = null,	
	@CreatedToUtc				datetime = null,
	@ApplyStatusIdIds			nvarchar(MAX) = null,
	@PageIndex					int = 0, 
	@PageSize					int = 2147483644,
	@TotalRecords				int = null OUTPUT
)
AS
BEGIN

	SET @AffiliateFirstName = ISNULL(@AffiliateFirstName, '')
	SET @AffiliateLastName = ISNULL(@AffiliateLastName, '')
	SET @CustomerEmail = ISNULL(@CustomerEmail, '')
	SET @ApplyStatusIdIds = ISNULL(@ApplyStatusIdIds, '')

	DECLARE @sql nvarchar(max)

	SET NOCOUNT ON
	
	CREATE TABLE #PageIndex 
	(
		[IndexId] int IDENTITY (1, 1) NOT NULL,
		[AffiliateCustomerId] int NOT NULL
	)

	SET @sql = 'INSERT INTO #PageIndex ([AffiliateCustomerId])
		SELECT ac.Id from NS_AffiliateCustomer ac 
		INNER JOIN [Affiliate] af on af.Id = ac.AffiliateId
		INNER JOIN [Address] ad on ad.Id = af.AddressId 
		INNER JOIN [Customer] c on c.Id = ac.CustomerId 
		WHERE af.Deleted = 0 AND c.Deleted = 0'
		
	if(@CustomerEmail != '')
		SET @sql = @sql + 'AND c.Email LIKE  ''%' + @CustomerEmail + '%'' '
	
	if(@AffiliateFirstName != '')
		SET @sql = @sql + 'AND ad.FirstName LIKE  ''%' + @AffiliateFirstName + '%'' '
	
	if(@AffiliateLastName != '')
		SET @sql = @sql + 'AND ad.LastName LIKE  ''%' + @AffiliateLastName + '%'' '
	
	if(@Active IS NOT NULL)
		SET @sql = @sql + 'AND af.Active = ' + CAST(@Active as NVARCHAR(MAX)) + ' '
		
	if(@CreatedFromUtc IS NOT NULL)
		SET @sql = @sql + 'AND ac.CreatedOnUtc >= ''' + CAST(@CreatedFromUtc as NVARCHAR(MAX)) + ''' '
		
	if(@CreatedToUtc IS NOT NULL)
		SET @sql = @sql + 'AND ac.CreatedOnUtc <= ''' + CAST(@CreatedToUtc as NVARCHAR(MAX)) + ''' '
		
	if(@ApplyStatusIdIds != '')
		SET @sql = @sql + 'AND ac.ApplyStatusId IN (' + @ApplyStatusIdIds + ') '

	--PRINT (@sql)
	EXEC sp_executesql @sql

	--paging
	DECLARE @PageLowerBound int
	DECLARE @PageUpperBound int
	DECLARE @RowsToReturn int
	SET @RowsToReturn = @PageSize * (@PageIndex + 1)	
	SET @PageLowerBound = @PageSize * @PageIndex
	SET @PageUpperBound = @PageLowerBound + @PageSize + 1
	
	--total records
	SET @TotalRecords = @@rowcount
	
	--return products
	SELECT TOP (@RowsToReturn)
		oc.*
	FROM
		#PageIndex [pi]
		INNER JOIN NS_AffiliateCustomer oc with (NOLOCK) on oc.Id = [pi].AffiliateCustomerId
	WHERE
		[pi].IndexId > @PageLowerBound AND 
		[pi].IndexId < @PageUpperBound
	ORDER BY
		[pi].IndexId
	
	DROP TABLE #PageIndex
END
