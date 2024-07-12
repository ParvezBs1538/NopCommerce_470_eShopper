CREATE OR ALTER PROCEDURE [dbo].[AjaxFilterProduct]
(
	@CategoryIds		nvarchar(MAX) = null,	
	@ManufacturerIds	nvarchar(MAX) = null,
	@StoreId			int = 0,
	@VendorIds			nvarchar(MAX) = null,
	@PriceMin			decimal(18, 4) = null,
	@PriceMax			decimal(18, 4) = null,	
	@FilteredSpecs nvarchar(MAX) = null,
	@AllowedCustomerRoleIds	nvarchar(MAX) = null,	
	@AttributesIds nvarchar(MAX) = null,
	@ShowHidden			bit = 0,
	@OrderBy			int = 0,
	@PageIndex			int = 0, 
	@PageSize			int = 2147483644,
	@FilterInStock		bit = 0,
	@FilterNotInStock	bit = 0,
	@StockQuantity		int = 0,
	@FilterFreeShipping	bit = 0,
	@FilterTaxExcemptProduct bit = 0,
	@FilterDiscountedProduct bit = 0,
	@FilterNewProduct bit = 0,
	@ProductRatingIds int = 0,
	@FilterProductTag  bit = 0,
	@ProductTagIds 	nvarchar(MAX) = null,
	@TotalRecords		int = null OUTPUT
)
AS
BEGIN

	DECLARE @sql nvarchar(max)
	DECLARE @sql_orderby nvarchar(max)


------------------------------------------------------------------------------------------------------
	-- Filters table

	CREATE TABLE #PageIndex 
	(
		[IndexId] int IDENTITY (1, 1) NOT NULL,
		[ProductId] int NOT NULL
	)
	CREATE TABLE #PageIndexHelper 
	(
		[IndexId] int IDENTITY (1, 1) NOT NULL,
		[ProductId] int NOT NULL
	)

------------------------------------------------------------------------------------------------------
	--filter by category IDs
	SET @CategoryIds = isnull(@CategoryIds, '')	
	CREATE TABLE #FilteredCategoryIds
	(
		CategoryId int not null
	)
	INSERT INTO #FilteredCategoryIds (CategoryId)
	SELECT CAST(data as int) FROM [nop_splitstring_to_table](@CategoryIds, ',')	

	DECLARE @CategoryIdsCount int	
	SET @CategoryIdsCount = (SELECT COUNT(1) FROM #FilteredCategoryIds)

------------------------------------------------------------------------------------------------------
	-- filter by manufacturer IDs
	SET @ManufacturerIds = isnull(@ManufacturerIds, '')	
	CREATE TABLE #FilteredManufactureIds
	(
		ManufacturerId int not null
	)
	INSERT INTO #FilteredManufactureIds (ManufacturerId)
	SELECT CAST(data as int) FROM [nop_splitstring_to_table](@ManufacturerIds, ',')	

------------------------------------------------------------------------------------------------------
	-- filter by product Rating
	IF OBJECT_ID('tempdb..#FilteredProductRatingIds') IS NOT NULL DROP TABLE #FilteredProductRatingIds
	CREATE TABLE #FilteredProductRatingIds
	(
		RatingId int ,
		ProductId int not null,
	)
	INSERT INTO #FilteredProductRatingIds(RatingId, ProductId) SELECT [ApprovedRatingSum]/NULLIF([ApprovedTotalReviews],0), Id FROM [dbo].[Product]
	
------------------------------------------------------------------------------------------------------
	-- filter by product tag IDs
	SET @ProductTagIds  = isnull(@ProductTagIds , '')	
	CREATE TABLE #FilteredProductTagIds
	(
		TagId int not null
	)
	INSERT INTO #FilteredProductTagIds (TagId)
	SELECT CAST(data as int) FROM [nop_splitstring_to_table](@ProductTagIds , ',')	

------------------------------------------------------------------------------------------------------
	-- filter by vendor
	SET @VendorIds = isnull(@VendorIds, '')	
	CREATE TABLE #FilteredVendorIds
	(
		VendorId int not null
	)
	INSERT INTO #FilteredVendorIds (VendorId)
	SELECT CAST(data as int) FROM [nop_splitstring_to_table](@VendorIds, ',')	

------------------------------------------------------------------------------------------------------
	--filter by specification
	SET @FilteredSpecs = isnull(@FilteredSpecs, '')	
	CREATE TABLE #FilteredSpecs
	(
		SpecificationAttributeOptionId int not null
	)
	INSERT INTO #FilteredSpecs (SpecificationAttributeOptionId)
	SELECT CAST(data as int) FROM [nop_splitstring_to_table](@FilteredSpecs, ',')
	
------------------------------------------------------------------------------------------------------
	--filter by attributes
	SET @AttributesIds = isnull(@AttributesIds, '')	
	CREATE TABLE #Attributes
	(
		Id int identity(1,1),
		AttributeId int,
		Name nvarchar(max) not null
	)
	if(LEN(@AttributesIds)>1)
	Begin
		INSERT INTO #Attributes (AttributeId, Name)
		SELECT 
			SUBSTRING(data, 1, CHARINDEX('-', data) - 1) as id, 
			SUBSTRING(data, CHARINDEX('-', data) + 1, LEN(data)) as Name
		FROM [nop_splitstring_to_table](@AttributesIds, ',')T0

	End
	--paging
	DECLARE @PageLowerBound int
	DECLARE @PageUpperBound int
	DECLARE @RowsToReturn int
	SET @RowsToReturn = @PageSize * (@PageIndex + 1)	
	SET @PageLowerBound = @PageSize * @PageIndex
	SET @PageUpperBound = @PageLowerBound + @PageSize + 1

		
	SET @sql = 'INSERT INTO #PageIndexHelper ([ProductId]) '
	SET @sql = @sql + 'select T0.Id from [dbo].[Product] T0 '

------------------------------------------------------------------------------------------------------
	-- join by category ids

	if(@CategoryIds!='')
	Begin
		SET @sql = @sql + ' inner join [dbo].[Product_Category_Mapping] T1 on T1.ProductId = T0.Id
						inner join #FilteredCategoryIds T2 on T2.CategoryId = T1.CategoryId '
	End

------------------------------------------------------------------------------------------------------
	-- join by manufacture ids

	if(@ManufacturerIds!='')
	Begin
		SET @sql = @sql + ' inner join [dbo].[Product_Manufacturer_Mapping] T3 on T3.ProductId = T0.Id
						   inner join #FilteredManufactureIds T30 on T30.ManufacturerId = T3.ManufacturerId '
	End	

------------------------------------------------------------------------------------------------------
	-- join by product rating

	if(@ProductRatingIds != 0)
	Begin
		SET @sql = @sql + ' inner join #FilteredProductRatingIds FPR on FPR.ProductId = T0.Id '

	End

------------------------------------------------------------------------------------------------------
	-- join by vendor ids

	if(@VendorIds!='')
	Begin
		SET @sql = @sql + ' inner join #FilteredVendorIds T4 on T4.VendorId = T0.VendorId '

	End

------------------------------------------------------------------------------------------------------
	-- join by product tag

	if(@ProductTagIds !='')
	Begin
		SET @sql = @sql + ' inner join [dbo].[Product_ProductTag_Mapping] T6 on T6.Product_Id = T0.Id 
							inner join #FilteredProductTagIds T60 on T60.TagId = T6.ProductTag_Id'
	End

------------------------------------------------------------------------------------------------------
	-- join by specs
	IF @FilteredSpecs != ''
	BEGIN

		declare @spec nvarchar(max)

			CREATE TABLE #FilteredSpecsWithAttributes
			(
				SpecificationAttributeId int not null,
				SpecificationAttributeOptionId int not null
			)
			INSERT INTO #FilteredSpecsWithAttributes (SpecificationAttributeId, SpecificationAttributeOptionId)
			SELECT sao.SpecificationAttributeId, fs.SpecificationAttributeOptionId
			FROM #FilteredSpecs fs INNER JOIN SpecificationAttributeOption sao ON sao.Id = fs.SpecificationAttributeOptionId
			ORDER BY sao.SpecificationAttributeId 

			declare @normalSpec nvarchar(255)
			select @normalSpec = coalesce(@normalSpec + ',', '') + cast(SpecificationAttributeOptionId as nvarchar(5)) from #FilteredSpecsWithAttributes fsa 
			IF(@normalSpec != '')
			BEGIN
				set @spec =  '
				 inner join [dbo].[Product_SpecificationAttribute_Mapping] TS
				 on TS.ProductId = T0.Id and 
				 TS.SpecificationAttributeOptionId  in ('+@normalSpec+')'
				SET @sql = @sql + @spec
			END
	END

------------------------------------------------------------------------------------------------------
	-- join by attributes

	if(@AttributesIds!='')
	Begin
		SET @sql = @sql + ' inner join [dbo].[Product_ProductAttribute_Mapping] TA2 on TA2.ProductId = T0.Id
							inner join [dbo].[ProductAttributeValue] TA3 on TA3.ProductAttributeMappingId = TA2.Id
							inner join #Attributes TA4 on TA4.AttributeId = TA2.ProductAttributeId and TA4.Name = TA3.Name '
	End

	SET @sql = @sql + ' where T0.Deleted = 0 and T0.Published = 1'
	
------------------------------------------------------------------------------------------------------
	-- filter by store id

	IF @StoreId > 0
	BEGIN
		SET @sql = @sql + ' 
		AND (T0.LimitedToStores = 0 OR EXISTS (
			SELECT 1 FROM [StoreMapping] sm with (NOLOCK)
			WHERE [sm].EntityId = T0.Id AND [sm].EntityName = ''Product'' and [sm].StoreId=' + CAST(@StoreId AS nvarchar(max)) + '
			)) '
	END

------------------------------------------------------------------------------------------------------
	-- filter by stock

	IF @FilterInStock > 0
	BEGIN
		IF @StockQuantity > 0
			SET @sql = @sql + ' AND T0.StockQuantity >=' +  CAST(@StockQuantity AS nvarchar(max))
		ELSE
			SET @sql = @sql + ' AND (T0.StockQuantity > 0) '
	END

------------------------------------------------------------------------------------------------------
	-- filter by not in stock

	IF @FilterNotInStock  > 0
	BEGIN
		SET @sql = @sql + ' AND (T0.StockQuantity <= 0) '
	END

------------------------------------------------------------------------------------------------------
	-- filter by product rating

	IF @ProductRatingIds != 0
	BEGIN
		SET @sql = @sql + ' AND (FPR.RatingId >= ' + CAST(@ProductRatingIds AS nvarchar(max)) + ') '
		print @sql
	END

------------------------------------------------------------------------------------------------------
	-- filter by free shipping

	IF @FilterFreeShipping > 0
	BEGIN
		SET @sql = @sql + ' AND (T0.IsFreeShipping > 0) AND (T0.IsShipEnabled > 0) '
	END

------------------------------------------------------------------------------------------------------
	-- filter by new product

	IF @FilterNewProduct > 0
	BEGIN
		SET @sql = @sql + ' AND (T0.MarkAsNew > 0) '
	END

------------------------------------------------------------------------------------------------------
	-- filter by discounted product

	IF @FilterDiscountedProduct > 0
	BEGIN
		SET @sql = @sql + ' AND (T0.HasDiscountsApplied > 0) '
	END

------------------------------------------------------------------------------------------------------
	-- filter by tax exempt product

	IF @FilterTaxExcemptProduct > 0
	BEGIN
		SET @sql = @sql + ' AND (T0.IsTaxExempt > 0) '
	END

------------------------------------------------------------------------------------------------------
	-- filter by minimum price

	IF @PriceMin is not null
	BEGIN
		SET @sql = @sql + '
		AND (
				T0.Price >= ' + CAST(@PriceMin AS nvarchar(max)) + '
			) '
	END
	
------------------------------------------------------------------------------------------------------
	-- filter by max price

	IF @PriceMax is not null
	BEGIN
		SET @sql = @sql + ' 
		AND (
				T0.Price <= ' + CAST(@PriceMax AS nvarchar(max)) + '
			) '
	END
	
------------------------------------------------------------------------------------------------------
	--sorting
	SET @sql_orderby = ''	
	IF @OrderBy = 5 /* Name: A to Z */
		SET @sql_orderby = ' T0.[Name] ASC'
	ELSE IF @OrderBy = 6 /* Name: Z to A */
		SET @sql_orderby = ' T0.[Name] DESC'
	ELSE IF @OrderBy = 10 /* Price: Low to High */
		SET @sql_orderby = ' T0.[Price] ASC'
	ELSE IF @OrderBy = 11 /* Price: High to Low */
		SET @sql_orderby = ' T0.[Price] DESC'
	ELSE IF @OrderBy = 15 /* creation date */
		SET @sql_orderby = ' T0.[CreatedOnUtc] DESC'
    ELSE IF @OrderBy = 20 /* creation date */
		SET @sql_orderby = ' T0.[CreatedOnUtc] ASC'
    ELSE IF @OrderBy = 21 /* creation date */
		SET @sql_orderby = ' T0.[CreatedOnUtc] DESC'
	ELSE /* default sorting, 0 (position) */
	BEGIN
		--category position (display order)
		IF @CategoryIdsCount > 0 SET @sql_orderby = ' T1.DisplayOrder ASC'

		--name
		IF LEN(@sql_orderby) > 0 SET @sql_orderby = @sql_orderby + ', '
		SET @sql_orderby = @sql_orderby + ' T0.[Name] ASC'
	END
	
	SET @sql = @sql + '
	ORDER BY ' + @sql_orderby + ' '
	EXEC sp_executesql @sql
	set @sql = 'INSERT INTO #PageIndex(ProductId) select ProductId from #PageIndexHelper GROUP BY ProductId ORDER BY MAX(IndexId) '
	EXEC sp_executesql @sql


------------------------------------------------------------------------------------------------------
	--total records
	SET @TotalRecords = @@rowcount

------------------------------------------------------------------------------------------------------
	--return products
	SELECT TOP (@RowsToReturn)
		p.*
	FROM
		#PageIndex [pi]
		INNER JOIN Product p with (NOLOCK) on p.Id = [pi].[ProductId]
	WHERE
		[pi].IndexId > @PageLowerBound AND 
		[pi].IndexId < @PageUpperBound
	ORDER BY
		[pi].IndexId
	
	DROP TABLE #PageIndex
	DROP TABLE #PageIndexHelper

END