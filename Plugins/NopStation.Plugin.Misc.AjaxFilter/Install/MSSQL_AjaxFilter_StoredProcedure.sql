CREATE OR ALTER  PROCEDURE [dbo].[AjaxFilter]
(
	@CategoryIds		nvarchar(MAX) = null,	
	@ManufacturerIds	nvarchar(MAX) = null,
	@StoreId			int = 0,
	@VendorIds			nvarchar(MAX) = null,
	@ProductTagIds			nvarchar(MAX) = null,
	@ProductRatingIds			int = 0,
	@PriceMin			decimal(18, 4) = null,
	@PriceMax			decimal(18, 4) = null,	
	@FilteredSpecs nvarchar(MAX) = null,
	@AllowedCustomerRoleIds	nvarchar(MAX) = null,
	@AttributesIds nvarchar(MAX) = null,
	@ShowHidden			bit = 0,
	@ViewMore			bit = 0,
	@SelectedSpecificationAttributeId int = 0,
	@ReturnSize int = 10,
	@SpecificationAttributeIds nvarchar(MAX) = '',
	@SpecificationAttributeIdsSetting nvarchar(MAX) = '',
	@FilterFreeShipping	        bit = 0,
	@FilterTaxExcemptProduct        bit = 0,
	@FilterDiscountedProduct        bit = 0,
	@FilterNewProduct       bit = 0,
	@DefaultSize int = 5,
	@PageSize int = 30,
	@PageIndexAt int = 1,
	@FilterBy  nvarchar(100) = ''
)
AS
BEGIN
	DECLARE @sql nvarchar(max)
	DECLARE @sql2 nvarchar(max)

	SET @sql2 = ''

------------------------------------------------------------------------------------------------------
	-- Filters table to show available filters

	IF OBJECT_ID('tempdb..#FILTERS') IS NOT NULL DROP TABLE #FILTERS
	CREATE TABLE #FILTERS
	(
		[IndexId] int Identity(1, 1),
		[FilterType] nvarchar(3),
		[FilterId] int,
		[FilterCount] int,
		[Name] nvarchar(max),
		[Color] nvarchar(max),
		[SpecificationAttributeId] int,
		[CheckedSpecificationAttributeId] int,
		[ProductAttributeId] int,
		[CheckedProductAttributeId] int,
		[RN] int
	)

	IF OBJECT_ID('tempdb..#TEMP_FILTERS') IS NOT NULL DROP TABLE #TEMP_FILTERS
	CREATE TABLE #TEMP_FILTERS
	(
		[IndexId] int Identity(1, 1),
		[FilterType] nvarchar(3),
		[FilterId] int,
		[FilterCount] int,
		[Name] nvarchar(max),
		[Color] nvarchar(max),
		[SpecificationAttributeId] int,
		[CheckedSpecificationAttributeId] int,
		[RN] int
	)

------------------------------------------------------------------------------------------------------
	-- filter by category IDs
	SET @CategoryIds = isnull(@CategoryIds, '')	
	IF OBJECT_ID('tempdb..#CategoryIds') IS NOT NULL DROP TABLE #CategoryIds
	CREATE TABLE #CategoryIds
	(
		CategoryId int not null
	)
	INSERT INTO #CategoryIds (CategoryId)
    SELECT CAST(data AS int) FROM [nop_splitstring_to_table](@CategoryIds, ',')
	DECLARE @CategoryIdsCount int	
	SET @CategoryIdsCount = (SELECT COUNT(1) FROM #CategoryIds)

------------------------------------------------------------------------------------------------------
	-- filter by manufacturer IDs
	SET @ManufacturerIds = isnull(@ManufacturerIds, '')	
	IF OBJECT_ID('tempdb..#FilteredManufactureIds') IS NOT NULL DROP TABLE #FilteredManufactureIds
	CREATE TABLE #FilteredManufactureIds
	(
		ManufacturerId int not null
	)
	INSERT INTO #FilteredManufactureIds (ManufacturerId)
	SELECT CAST(data AS int) FROM [nop_splitstring_to_table](@ManufacturerIds, ',')	

------------------------------------------------------------------------------------------------------
	-- filter by vendor IDs
	SET @VendorIds = isnull(@VendorIds, '')	
	IF OBJECT_ID('tempdb..#FilteredVendors') IS NOT NULL DROP TABLE #FilteredVendors
	CREATE TABLE #FilteredVendors
	(
		VendorId int not null
	)
	INSERT INTO #FilteredVendors (VendorId)
	SELECT CAST(data AS int) FROM [nop_splitstring_to_table](@VendorIds, ',')	

------------------------------------------------------------------------------------------------------
	-- filter by product tag IDs

	SET @ProductTagIds = isnull(@ProductTagIds, '')	
	IF OBJECT_ID('tempdb..#FilteredProductTagIds') IS NOT NULL DROP TABLE #FilteredProductTagIds
	CREATE TABLE #FilteredProductTagIds
	(
		TagId int not null
	)
	INSERT INTO #FilteredProductTagIds (TagId)
	SELECT CAST(data AS int) FROM [nop_splitstring_to_table](@ProductTagIds, ',')	
	
------------------------------------------------------------------------------------------------------
	-- filter by product Ratings IDs
	
	--SET @ProductRatingIds = isnull(@ProductRatingIds, '')	
	IF OBJECT_ID('tempdb..#FilteredProductRatingIds') IS NOT NULL DROP TABLE #FilteredProductRatingIds
	CREATE TABLE #FilteredProductRatingIds
	(
		RatingId int ,
		ProductId int not null,
	)
	INSERT INTO #FilteredProductRatingIds(RatingId, ProductId) SELECT [ApprovedRatingSum]/NULLIF([ApprovedTotalReviews],0), Id FROM [dbo].[Product]
	
------------------------------------------------------------------------------------------------------
	-- filter by specification
	SET @FilteredSpecs = isnull(@FilteredSpecs, '')	
	IF OBJECT_ID('tempdb..#FilteredSpecs') IS NOT NULL DROP TABLE #FilteredSpecs

	CREATE TABLE #FilteredSpecs
	(
		SpecificationAttributeOptionId int not null
	)

	INSERT INTO #FilteredSpecs (SpecificationAttributeOptionId)
	SELECT CAST(data AS int) FROM [nop_splitstring_to_table](@FilteredSpecs, ',')
	DECLARE @SpecAttributesCount int	
	SET @SpecAttributesCount = (SELECT COUNT(*) FROM #FilteredSpecs)

    ------------------------------test product tag--------------------------------------------------

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
	IF OBJECT_ID('tempdb..##TempProduct') IS NOT NULL DROP TABLE ##TempProduct
					SET @sql = 'SELECT P.Id as ProductId, P.* INTO ##TempProduct
							FROM [dbo].[Product] P'
					SET @sql = @sql + ' inner join [dbo].[Product_ProductAttribute_Mapping] TA2 on TA2.ProductId = P.Id
										inner join [dbo].[ProductAttributeValue] TA3 on TA3.ProductAttributeMappingId = TA2.Id
										inner join #Attributes TA4 on TA4.AttributeId = TA2.ProductAttributeId and TA4.Name = TA3.Name '
					
					EXEC sp_executesql @sql
 
	-- Add price minimum, max filter
	IF OBJECT_ID('tempdb..#PageIndexP') IS NOT NULL DROP TABLE #PageIndexP
	CREATE TABLE #PageIndexP 
	(
		[IndexId] int IDENTITY (1, 1) NOT NULL,
		[ProductId] int NOT NULL,
		[ParentGroupedProductId] int NOT NULL,
		[Price] decimal (18, 4) not null
	)

	SET @sql = 'INSERT #PageIndexP ([ProductId],[ParentGroupedProductId],[Price] ) '
	SET @sql = @sql + ' SELECT p.Id, p.ParentGroupedProductId, p.Price FROM [dbo].[Product] p '
		
	IF(@CategoryIdsCount > 0)
		SET @sql = @sql + ' INNER JOIN Product_Category_Mapping pcm ON pcm.ProductId = p.Id '
	IF @StoreId > 0
	BEGIN
		SET @sql = @sql + '
		AND (p.LimitedToStores = 0 OR EXISTS (
			SELECT 1 FROM [StoreMapping] sm with (NOLOCK)
			WHERE [sm].EntityId = p.Id AND [sm].EntityName = ''Product'' AND [sm].StoreId=' + CAST(@StoreId AS nvarchar(max)) + '
			))'
	END

	IF(@CategoryIdsCount > 0)
	BEGIN
		Set @sql = @sql + ' AND pcm.CategoryId IN (' + @CategoryIds + ') '
	END

	EXEC sp_executesql @sql
    
	SET @sql = @sql + ' WHERE p.Deleted = 0 AND p.Published = 1 '
	INSERT #FILTERS (FilterType, FilterId, FilterCount, [Name]) SELECT 'MIN', isnull(MIN(p.Price), 0) AS nvarchar, 0, '' FROM #PageIndexP [pi] INNER JOIN Product p ON p.Id = [pi].ProductId
	INSERT #FILTERS (FilterType, FilterId, FilterCount, [Name]) SELECT 'MAX', isnull(Max(p.Price), 0) AS nvarchar, 0,  '' FROM #PageIndexP [pi] INNER JOIN Product p ON p.Id = [pi].ProductId

------------------------------------------------------------------------------------------------------
	-- Paging Table
	
	IF OBJECT_ID('tempdb..#PageIndex') IS NOT NULL DROP TABLE #PageIndex
	CREATE TABLE #PageIndex 
	(
		[IndexId] int IDENTITY (1, 1) NOT NULL,
		[ProductId] int NOT NULL,
		[ParentGroupedProductId] int NOT NULL,
		[Price] decimal (18, 4) not null
	)

------------------------------------------------------------------------------------------------------
	-- construct query and filter 

	SET @sql = 'INSERT #PageIndex ([ProductId],[ParentGroupedProductId],[Price] ) '
	SET @sql = @sql + ' SELECT p.Id, p.ParentGroupedProductId, p.Price FROM [dbo].[Product] p '
		
	IF(@CategoryIdsCount > 0)
		SET @sql = @sql + ' INNER JOIN Product_Category_Mapping pcm ON pcm.ProductId = p.Id '
	IF(@ProductRatingIds > 0)
		SET @sql = @sql + ' INNER JOIN #FilteredProductRatingIds PR ON PR.ProductId = p.Id '
	SET @sql = @sql + ' WHERE p.Deleted = 0 AND p.Published = 1 '

    IF @PriceMin is not null
		BEGIN
			SET @sql = @sql + '
			AND (
					p.Price >= ' + CAST(@PriceMin AS nvarchar(max)) + '
				) '
		END

	IF @PriceMax is not null
		BEGIN
			SET @sql = @sql + ' 
			AND (
					p.Price <= ' + CAST(@PriceMax AS nvarchar(max)) + '
				) '
		END

    IF @FilterFreeShipping > 0
	    BEGIN
		    SET @sql = @sql + ' AND (p.IsFreeShipping > 0) AND (p.IsShipEnabled > 0) '
	    END
    IF @FilterNewProduct > 0
	    BEGIN
		    SET @sql = @sql + ' AND (p.MarkAsNew > 0) '
	    END
    IF @FilterDiscountedProduct > 0
	    BEGIN
		    SET @sql = @sql + ' AND (p.HasDiscountsApplied > 0) '
	    END
    IF @FilterTaxExcemptProduct > 0
	    BEGIN
		    SET @sql = @sql + ' AND (p.IsTaxExempt > 0) '
	    END
	IF @ProductRatingIds > 0
	    BEGIN
		    SET @sql = @sql + ' AND (PR.RatingId >= ' + CAST(@ProductRatingIds AS nvarchar(max)) + ') '
	    END
	
	--filter by store
	IF @StoreId > 0
	BEGIN
		SET @sql = @sql + '
		AND (p.LimitedToStores = 0 OR EXISTS (
			SELECT 1 FROM [StoreMapping] sm with (NOLOCK)
			WHERE [sm].EntityId = p.Id AND [sm].EntityName = ''Product'' AND [sm].StoreId=' + CAST(@StoreId AS nvarchar(max)) + '
			))'
	END

	IF(@CategoryIdsCount > 0)
	BEGIN
		Set @sql = @sql + ' AND pcm.CategoryId IN (' + @CategoryIds + ') '
	END
	print @sql
	EXEC sp_executesql @sql
	
	-- Add specification filters
	DECLARE @maxSetting INT
	SET @maxSetting = 0

	IF @SpecAttributesCount > 0
	BEGIN
		declare @spec nvarchar(max)
		SET @spec = ''
		IF OBJECT_ID('tempdb..#FilteredSpecsWithAttributes') IS NOT NULL DROP TABLE #FilteredSpecsWithAttributes
		CREATE TABLE #FilteredSpecsWithAttributes
		(
			SpecificationAttributeId int not null,
			SpecificationAttributeOptionId int not null
		)
		INSERT INTO #FilteredSpecsWithAttributes (SpecificationAttributeId, SpecificationAttributeOptionId)
		SELECT sao.SpecificationAttributeId, fs.SpecificationAttributeOptionId
		FROM #FilteredSpecs fs INNER JOIN SpecificationAttributeOption sao 
		ON sao.Id = fs.SpecificationAttributeOptionId
		ORDER BY sao.SpecificationAttributeId 
	END

	IF OBJECT_ID('tempdb..##TempSpProduct') IS NOT NULL DROP TABLE ##TempSpProduct
	
	SET @sql = 'SELECT P.Id as ProductId, P.* INTO ##TempSpProduct
			FROM [dbo].[Product] P '
	IF(@SpecAttributesCount > 0)
		Begin
			SET @sql = @sql + ' INNER JOIN [dbo].[Product_SpecificationAttribute_Mapping] psm on (P.Id = psm.ProductId)
								Inner Join #FilteredSpecsWithAttributes fsa ON fsa.SpecificationAttributeOptionId = psm.SpecificationAttributeOptionId  '
		End
	EXEC sp_executesql @sql

	DECLARE @priceFilter nvarchar(200)

	-- Add vendors filters
	SET @sql = 'INSERT #FILTERS (FilterType, FilterId, FilterCount, Name, Color, SpecificationAttributeId, CheckedSpecificationAttributeId) 
	SELECT distinct ''VEN'' AS FilterType, V00.Id, 0 AS Qty, V00.Name, null, null, null
	FROM #PageIndex [pi] 
	INNER JOIN [dbo].[Product] T00 on (T00.Id = [pi].ProductId or T00.Id = [pi].ParentGroupedProductId)
	INNER JOIN [dbo].[Vendor] V00 on (V00.Id = T00.VendorId) '
	IF(@AttributesIds != '')
		Begin
			SET @sql = @sql + 'inner join ##TempProduct TP on (TP.ProductId = [pi].ProductId) '
		End
	IF(@SpecAttributesCount > 0)
		Begin
			SET @sql = @sql + ' inner join ##TempSpProduct TSP on (TSP.ProductId = [pi].ProductId)  '
		End
	IF(@ManufacturerIds != '')
		Begin
			SET @sql = @sql + ' inner join Product_Manufacturer_Mapping pmm on (T00.Id = pmm.ProductId)
								inner join #FilteredManufactureIds T30 on T30.ManufacturerId = pmm.ManufacturerId '
		End
	if(@ProductTagIds !='')
		Begin
			SET @sql = @sql + ' inner join [dbo].[Product_ProductTag_Mapping] T6 on T6.Product_Id = T00.Id 
								inner join #FilteredProductTagIds T60 on T60.TagId = T6.ProductTag_Id '
		End
	EXEC sp_executesql @sql

	-- Add product tags filters
	SET @sql = 'INSERT #FILTERS (FilterType, FilterId, FilterCount, Name, Color, SpecificationAttributeId, CheckedSpecificationAttributeId) 
	SELECT distinct ''TAG'' AS FilterType, PTM00.ProductTag_Id, count(distinct [pi].ProductId) Qty, PT00.Name, null, null, null
	FROM #PageIndex [pi] 
	INNER JOIN [dbo].[Product] P00 on (P00.Id = [pi].ProductId or P00.Id = [pi].ParentGroupedProductId)
	INNER JOIN [dbo].[Product_ProductTag_Mapping] PTM00 on (PTM00.Product_Id = P00.Id) 
	INNER JOIN [dbo].[ProductTag] PT00 on (PT00.Id = PTM00.ProductTag_Id) '
	IF(@AttributesIds != '')
		Begin
			SET @sql = @sql + 'inner join ##TempProduct TP on (TP.ProductId = [pi].ProductId) '
		End
	IF(@SpecAttributesCount > 0)
		Begin
			SET @sql = @sql + ' inner join ##TempSpProduct TSP on (TSP.ProductId = [pi].ProductId)  '
		End
	if(@VendorIds!='')
		Begin
			SET @sql = @sql + ' inner join #FilteredVendors T4 on T4.VendorId = P00.VendorId '
		End
	IF(@ManufacturerIds != '')
		Begin
			SET @sql = @sql + ' inner join Product_Manufacturer_Mapping pmm on (P00.Id = pmm.ProductId)
								inner join #FilteredManufactureIds T30 on T30.ManufacturerId = pmm.ManufacturerId '
		End
    SET @sql = @sql + ' Group by PTM00.ProductTag_Id, PT00.Name'
	EXEC sp_executesql @sql

    -- Add product rating filters
	SET @sql = 'INSERT #FILTERS (FilterType, FilterId, FilterCount, Name, Color, SpecificationAttributeId, CheckedSpecificationAttributeId) 
	SELECT distinct ''RAT'' AS FilterType, TPR.RatingId, count(TPR.ProductId) Qty, TPR.RatingId, null, null, null
	FROM #PageIndex [pi] 
	INNER JOIN [dbo].[Product] P00 on (P00.Id = [pi].ProductId or P00.Id = [pi].ParentGroupedProductId)
	INNER JOIN #FilteredProductRatingIds TPR ON TPR.ProductId = P00.Id '
	IF(@AttributesIds != '')
		Begin
			SET @sql = @sql + 'inner join ##TempProduct TP on (TP.ProductId = [pi].ProductId) '
		End
	IF(@SpecAttributesCount > 0)
		Begin
			SET @sql = @sql + ' inner join ##TempSpProduct TSP on (TSP.ProductId = [pi].ProductId)  '
		End
	if(@VendorIds!='')
		Begin
			SET @sql = @sql + ' inner join #FilteredVendors T4 on T4.VendorId = P00.VendorId '
		End
	IF(@ManufacturerIds != '')
		Begin
			SET @sql = @sql + ' inner join Product_Manufacturer_Mapping pmm on (P00.Id = pmm.ProductId)
								inner join #FilteredManufactureIds T30 on T30.ManufacturerId = pmm.ManufacturerId '
		End
	IF(@ProductTagIds !='')
		Begin
			SET @sql = @sql + ' inner join [dbo].[Product_ProductTag_Mapping] T6 on T6.Product_Id = P00.Id 
								inner join #FilteredProductTagIds T60 on T60.TagId = T6.ProductTag_Id '
		End
    SET @sql = @sql + ' Group by TPR.RatingId'
	
	EXEC sp_executesql @sql

	-- Add product attribute filters
	
	SET @sql = 'INSERT #FILTERS (FilterType, FilterId, FilterCount, Name, ProductAttributeId, CheckedProductAttributeId) 
		SELECT distinct ''PAT'' AS FilterType, PAV.Id as FilterId, count(distinct [pi].ProductId) AS Qty, PAV.Name, PA.Id, PAV.Id
		FROM #PageIndex [pi]
		INNER JOIN [dbo].[Product] P00 on (P00.Id = [pi].ProductId or P00.Id = [pi].ParentGroupedProductId)
		inner join Product_ProductAttribute_Mapping PM on (PM.ProductId = [pi].ProductId)
		inner join ProductAttribute PA on (PA.Id = Pm.ProductAttributeId)
		inner join ProductAttributeValue PAV on (PAV.ProductAttributeMappingId = PM.Id)
			'
	IF(@AttributesIds != '')
		Begin
			SET @sql = @sql + 'inner join ##TempProduct TP on (TP.ProductId = [pi].ProductId) '
		End
	IF(@SpecAttributesCount > 0)
		Begin
			SET @sql = @sql + ' inner join ##TempSpProduct TSP on (TSP.ProductId = [pi].ProductId)  '
		End
	if(@VendorIds!='')
		Begin
			SET @sql = @sql + ' inner join #FilteredVendors T4 on T4.VendorId = P00.VendorId '
		End
	IF(@ManufacturerIds != '')
		Begin
			SET @sql = @sql + ' inner join Product_Manufacturer_Mapping pmm on (P00.Id = pmm.ProductId)
								inner join #FilteredManufactureIds T30 on T30.ManufacturerId = pmm.ManufacturerId '
		End
	if(@ProductTagIds !='')
		Begin
			SET @sql = @sql + ' inner join [dbo].[Product_ProductTag_Mapping] T6 on T6.Product_Id = P00.Id 
								inner join #FilteredProductTagIds T60 on T60.TagId = T6.ProductTag_Id '
		End
	SET @sql = @sql + ' Group by PA.Name, PA.Id,PAV.Id,PAV.Name'

	EXEC sp_executesql @sql

	IF @FilteredSpecs = ''
		BEGIN
			SET @sql =  'INSERT #TEMP_FILTERS (FilterType, FilterId, FilterCount, Name, Color, SpecificationAttributeId, CheckedSpecificationAttributeId) 
							SELECT  distinct ''SPE'' AS FilterType, T00.SpecificationAttributeOptionId, count(distinct [pi].ProductId) AS Qty , Sao.Name, Sao.ColorSquaresRgb, Sao.SpecificationAttributeId,Sao.Id   FROM #PageIndex [pi]
							INNER JOIN [dbo].[Product_SpecificationAttribute_Mapping] T00
							on (T00.ProductId = [pi].ProductId or T00.ProductId = [pi].ParentGroupedProductId) AND T00.AllowFiltering = 1
							LEFT OUTER JOIN [dbo].[SpecificationAttributeOption] Sao ON Sao.Id = T00.SpecificationAttributeOptionId  '
			IF(@AttributesIds != '')
				Begin
					SET @sql = @sql + 'inner join ##TempProduct TP on (TP.ProductId = [pi].ProductId) '
				End
			IF(@ManufacturerIds != '')
				Begin
					SET @sql = @sql + ' inner join Product_Manufacturer_Mapping pmm on ([pi].ProductId = pmm.ProductId)
										inner join #FilteredManufactureIds T30 on T30.ManufacturerId = pmm.ManufacturerId '
				End
			if(@ProductTagIds !='')
				Begin
					SET @sql = @sql + ' inner join [dbo].[Product_ProductTag_Mapping] T6 on T6.Product_Id = [pi].ProductId  
										inner join #FilteredProductTagIds T60 on T60.TagId = T6.ProductTag_Id '
				End
				
			SET @sql = @sql + ' Group by SpecificationAttributeOptionId , Sao.Name, ColorSquaresRgb, SpecificationAttributeId,Sao.Id'
			EXEC sp_executesql @sql
			SET @sql = 'INSERT  #FILTERS (FilterType, FilterId, FilterCount, Name, Color, SpecificationAttributeId, CheckedSpecificationAttributeId, RN)
							SELECT FilterType, FilterId, FilterCount, Name, Color, SpecificationAttributeId, CheckedSpecificationAttributeId,
							ROW_NUMBER() OVER (PARTITION BY SpecificationAttributeId ORDER BY CheckedSpecificationAttributeId DESC) AS RN FROM #TEMP_FILTERS'
			EXEC sp_executesql @sql
		END 

	IF(@ViewMore = 0 and @SpecAttributesCount > 0 and @FilteredSpecs != '')
	
		BEGIN
				SET @sql =  'INSERT #TEMP_FILTERS (FilterType, FilterId, FilterCount, Name, Color, SpecificationAttributeId, CheckedSpecificationAttributeId) 
						SELECT  distinct ''SPE'' AS FilterType, T00.SpecificationAttributeOptionId, count(distinct [pi].ProductId) AS Qty , Sao.Name, Sao.ColorSquaresRgb, Sao.SpecificationAttributeId,Sao.Id   FROM #PageIndex [pi]
						INNER JOIN [dbo].[Product_SpecificationAttribute_Mapping] T00
						on (T00.ProductId = [pi].ProductId or T00.ProductId = [pi].ParentGroupedProductId) AND T00.AllowFiltering = 1
						LEFT OUTER JOIN [dbo].[SpecificationAttributeOption] Sao ON Sao.Id = T00.SpecificationAttributeOptionId
						inner join ##TempSpProduct TSP on (TSP.ProductId = [pi].ProductId)  '

				IF(@AttributesIds != '')
					Begin
						SET @sql = @sql + 'inner join ##TempProduct TP on (TP.ProductId = [pi].ProductId) '
					End
				if(@VendorIds!='')
					Begin
						SET @sql = @sql + ' inner join #FilteredVendors T4 on T4.VendorId = TSP.VendorId '
					End
				IF(@ManufacturerIds != '')
					Begin
						SET @sql = @sql + ' inner join Product_Manufacturer_Mapping pmm on ([pi].ProductId = pmm.ProductId)
											inner join #FilteredManufactureIds T30 on T30.ManufacturerId = pmm.ManufacturerId '
					End
				if(@ProductTagIds !='')
					Begin
						SET @sql = @sql + ' inner join [dbo].[Product_ProductTag_Mapping] T6 on T6.Product_Id = [pi].ProductId  
											inner join #FilteredProductTagIds T60 on T60.TagId = T6.ProductTag_Id '
					End

				SET @sql = @sql + ' Group by T00.SpecificationAttributeOptionId , Sao.Name, ColorSquaresRgb, Sao.SpecificationAttributeId,Sao.Id'

				EXEC sp_executesql @sql
			END
	
	IF(@SpecAttributesCount > 0 AND @ViewMore = 0)
		BEGIN
			SET @sql = 'INSERT  #FILTERS (FilterType, FilterId, FilterCount, Name, Color, SpecificationAttributeId, CheckedSpecificationAttributeId, RN)
						SELECT FilterType, FilterId, FilterCount, Name, Color, SpecificationAttributeId, CheckedSpecificationAttributeId,
						ROW_NUMBER() OVER (PARTITION BY SpecificationAttributeId ORDER BY CheckedSpecificationAttributeId DESC) AS RN FROM #TEMP_FILTERS'
			EXEC sp_executesql @sql
		END	

	--view more specification
	IF(@ViewMore = 1)
		BEGIN
			SET @sql = 'Insert into  #FILTERS (FilterType, FilterId, FilterCount, Name, Color, SpecificationAttributeId, CheckedSpecificationAttributeId, RN)
						SELECT  FilterType, FilterId, count(distinct FilterCount) Qty, Name, Color,SpecificationAttributeId,CheckedSpecificationAttributeId,0 AS RN  
						FROM #TEMP_FILTERS WHERE SpecificationAttributeId = '+convert(nvarchar(10), @SelectedSpecificationAttributeId)
				IF( @FilterBy != '')
					BEGIN
						SET @sql = @sql + ' AND (Name like ''%'+@FilterBy+'%'' OR CheckedSpecificationAttributeId > 0)'
					END
				SET @sql = @sql + ' GROUP BY  FilterType, FilterId, Name, Color, SpecificationAttributeId , CheckedSpecificationAttributeId  
				ORDER BY CheckedSpecificationAttributeId desc 
				offset '+convert(nvarchar(10), @PageSize * (@PageIndexAt - 1))+ 'rows
				fetch next '+convert(nvarchar(10), @PageSize)+' rows only'

				EXEC sp_executesql @sql
		END

	IF(@ViewMore = 0)
		BEGIN
			SET @sql = 'INSERT  #FILTERS (FilterType, FilterId, FilterCount)  
			 SELECT distinct ''MAN'' AS FilterType, pmm.ManufacturerId, count(distinct pcm.Id) Qty FROM [dbo].[Product] p 
             INNER JOIN #PageIndex [pi] ON [pi].ProductId = p.Id
			 LEFT OUTER JOIN [dbo].[Product_Category_Mapping] pcm ON pcm.ProductId = p.Id
			 LEFT OUTER JOIN [dbo].[Product_Manufacturer_Mapping] pmm ON  p.Id = pmm.ProductId
			 LEFT OUTER JOIN [dbo].[Product_ProductTag_Mapping] ptm ON  p.Id = ptm.Product_Id
			 LEFT OUTER JOIN [dbo].[Product_ProductAttribute_Mapping] PM on (PM.ProductId = p.Id) '
			
			IF(@AttributesIds!='')
				BEGIN
					SET @sql = @sql+' inner join ##TempProduct TP on (TP.ProductId = p.Id)'
				END
			IF(@SpecAttributesCount > 0)
				Begin
					SET @sql = @sql + ' inner join ##TempSpProduct TSP on (TSP.ProductId = [pi].ProductId)   '
				End
			if(@VendorIds!='')
					Begin
						SET @sql = @sql + ' inner join #FilteredVendors T4 on T4.VendorId = T0.VendorId '

					End
			if(@ProductTagIds !='')
				Begin
					SET @sql = @sql + ' inner join [dbo].[Product_ProductTag_Mapping] T6 on T6.Product_Id = p.Id 
										inner join #FilteredProductTagIds T60 on T60.TagId = T6.ProductTag_Id '
				End
			IF(@FilteredSpecs != '')
				BEGIN
					 SET @sql = @sql+' LEFT OUTER JOIN Product_SpecificationAttribute_Mapping psam
									ON p.Id = psam.ProductId'
				END
			SET @sql = @sql+' WHERE p.Deleted = 0 AND p.Published = 1'
			IF(@CategoryIds != '')
				BEGIN
					SET @sql = @sql+' AND pcm.CategoryId in (SELECT CategoryId FROM #CategoryIds)'
				END
			IF(@FilteredSpecs != '' and @ManufacturerIds != '')
				BEGIN
					 SET @sql = @sql+' AND (psam.SpecificationAttributeOptionId in ('+@FilteredSpecs+') OR pmm.ManufacturerId in (SELECT ManufacturerId FROM #FilteredManufactureIds))'
				END
			ELSE IF(@FilteredSpecs != '' and @ProductTagIds != '')
				BEGIN
					 SET @sql = @sql+' AND (psam.SpecificationAttributeOptionId in ('+@FilteredSpecs+') OR ptm.Product_Id in (SELECT Product_Id FROM #FilteredProductTagIds))'
				END
			ELSE IF(@FilteredSpecs != '')
				BEGIN
					 SET @sql = @sql+' AND psam.SpecificationAttributeOptionId in ('+@FilteredSpecs+') '
			END
			 SET @sql = @sql+' GROUP BY pmm.ManufacturerId'
		
			EXEC sp_executesql @sql
		END

	SELECT * FROM #FILTERS
END
