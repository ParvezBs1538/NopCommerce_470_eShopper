CREATE PROCEDURE AjaxFilter(CategoryIds		LONGTEXT ,	
	ManufacturerIds	LONGTEXT ,
	StoreId			INT ,
	VendorIds			LONGTEXT ,
	ProductTagIds			LONGTEXT ,
    ProductRatingIds		INT ,
	PriceMin			DECIMAL(18,4) ,
	PriceMax			DECIMAL(18,4) ,	
	FilteredSpecs LONGTEXT ,
	AllowedCustomerRoleIds	LONGTEXT ,
	AttributesIds LONGTEXT ,
	ShowHidden			BOOLEAN ,
	ViewMore			BOOLEAN ,
	SelectedSpecificationAttributeId INT ,
	ReturnSize INT ,
	SpecificationAttributeIds LONGTEXT ,
	SpecificationAttributeIdsSetting LONGTEXT ,
	FilterFreeShipping	BOOLEAN ,
	FilterTaxExcemptProduct BOOLEAN ,
	FilterDiscountedProduct BOOLEAN ,
	FilterNewProduct BOOLEAN ,
	DefaultSize INT ,
	PageSize INT ,
	PageIndexAt INT ,
	FilterBy  NVARCHAR(100))
BEGIN

   DECLARE mysqlquery LONGTEXT;
   DECLARE sql2 LONGTEXT;
   DECLARE CategoryIdsCount INT;	
   DECLARE SpecAttributesCount INT;	
   DECLARE maxSetting INT;
   DECLARE priceFilter NVARCHAR(200);
   DECLARE spec LONGTEXT;

   IF StoreId is null then
      set StoreId = 0;
   END IF;
   IF ShowHidden is null then
      set ShowHidden = 0;
   END IF;
   IF ViewMore is null then
      set ViewMore = 0;
   END IF;
   IF SelectedSpecificationAttributeId is null then
      set SelectedSpecificationAttributeId = 0;
   END IF;
   IF ReturnSize is null then
      set ReturnSize = 10;
   END IF;
   IF SpecificationAttributeIds is null then
      set SpecificationAttributeIds = '';
   END IF;
   IF SpecificationAttributeIdsSetting is null then
      set SpecificationAttributeIdsSetting = '';
   END IF;
   IF FilterFreeShipping is null then
      set FilterFreeShipping = 0;
   END IF;
   IF FilterTaxExcemptProduct is null then
      set FilterTaxExcemptProduct = 0;
   END IF;
   IF FilterDiscountedProduct is null then
      set FilterDiscountedProduct = 0;
   END IF;
   IF FilterNewProduct is null then
      set FilterNewProduct = 0;
   END IF;
   IF DefaultSize is null then
      set DefaultSize = 5;
   END IF;
   IF PageSize is null then
      set PageSize = 30;
   END IF;
   IF PageIndexAt is null then
      set PageIndexAt = 1;
   END IF;
   IF FilterBy is null then
      set FilterBy = '';
   END IF;
   SET sql2 = '';

-- ----------------------------------------------------------------------------------------------------
	-- Filters table to show available filters

   DROP TEMPORARY TABLE IF EXISTS FILTERS;

   CREATE TEMPORARY TABLE FILTERS
   (
      IndexId INT   AUTO_INCREMENT PRIMARY KEY,
      FilterType NVARCHAR(3),
      FilterId INT,
      FilterCount INT,
      Name varchar(100),
      Index(Name),
      Color varchar(100),
      Index(Color),
      SpecificationAttributeId INT,
      CheckedSpecificationAttributeId INT,
      ProductAttributeId INT,
      CheckedProductAttributeId INT,
      RN INT
   )  AUTO_INCREMENT = 1;

   DROP TEMPORARY TABLE IF EXISTS TEMP_FILTERS;
   CREATE TEMPORARY TABLE TEMP_FILTERS
   (
      IndexId INT   AUTO_INCREMENT PRIMARY KEY,
      FilterType NVARCHAR(3),
      FilterId INT,
      FilterCount INT,
      Name varchar(100),
      Index(Name),
      Color varchar(100),
      Index(Color),
      SpecificationAttributeId INT,
      CheckedSpecificationAttributeId INT,
      RN INT
   )  AUTO_INCREMENT = 1;

-- ----------------------------------------------------------------------------------------------------
	-- filter by category IDs
   SET CategoryIds = IFNULL(CategoryIds,'');	
   DROP TEMPORARY TABLE IF EXISTS CategoryIds;
   CREATE TEMPORARY TABLE CategoryIds
   (
      CategoryId INT not null
   );
   CALL nop_splitstring_to_table(CategoryIds,',');
   INSERT INTO CategoryIds(CategoryId)
   SELECT cast(data as SIGNED INTEGER) FROM nop_splitstring_to_table_output;
   SET CategoryIdsCount =(SELECT COUNT(1) FROM CategoryIds);

-- ----------------------------------------------------------------------------------------------------
	-- filter by manufacturer IDs
   SET ManufacturerIds = IFNULL(ManufacturerIds,'');	
   DROP TEMPORARY TABLE IF EXISTS FilteredManufactureIds;
   CREATE TEMPORARY TABLE FilteredManufactureIds
   (
      ManufacturerId INT not null
   );
   CALL nop_splitstring_to_table(ManufacturerIds,',');
   INSERT INTO FilteredManufactureIds(ManufacturerId)
   SELECT cast(data as SIGNED INTEGER) FROM nop_splitstring_to_table_output;	

-- ----------------------------------------------------------------------------------------------------
	-- filter by vendor IDs
   SET VendorIds = IFNULL(VendorIds,'');	
   DROP TEMPORARY TABLE IF EXISTS FilteredVendors;
   CREATE TEMPORARY TABLE FilteredVendors
   (
      VendorId INT not null
   );
   CALL nop_splitstring_to_table(VendorIds,',');
   INSERT INTO FilteredVendors(VendorId)
   SELECT cast(data as SIGNED INTEGER) FROM nop_splitstring_to_table_output;	

-- ----------------------------------------------------------------------------------------------------
	-- filter by product Rating IDs

   IF ProductRatingIds is null then
      set ProductRatingIds = 0;
   END IF;
   DROP TEMPORARY TABLE IF EXISTS FilteredProductRatingIds;
   CREATE TEMPORARY TABLE FilteredProductRatingIds
   (
      Rating INT,
      ProductId INT not null
   );
   INSERT INTO FilteredProductRatingIds(Rating, ProductId)
   SELECT FLOOR(ApprovedRatingSum/NULLIF(ApprovedTotalReviews,0)), Id FROM Product;	

-- ----------------------------------------------------------------------------------------------------
	-- filter by product Tag IDs

   SET ProductTagIds = IFNULL(ProductTagIds,'');	
   DROP TEMPORARY TABLE IF EXISTS FilteredProductTagIds;
   CREATE TEMPORARY TABLE FilteredProductTagIds
   (
      TagId INT not null
   );
   CALL nop_splitstring_to_table(ProductTagIds,',');
   INSERT INTO FilteredProductTagIds(TagId)
   SELECT cast(data as SIGNED INTEGER) FROM nop_splitstring_to_table_output;
   
-- ----------------------------------------------------------------------------------------------------
	-- filter by specification
   SET FilteredSpecs = IFNULL(FilteredSpecs,'');	
   DROP TEMPORARY TABLE IF EXISTS FilteredSpecs;

   CREATE TEMPORARY TABLE FilteredSpecs
   (
      SpecificationAttributeOptionId INT not null
   );

   CALL nop_splitstring_to_table(FilteredSpecs,',');
   INSERT INTO FilteredSpecs(SpecificationAttributeOptionId)
   SELECT cast(data as SIGNED INTEGER) FROM nop_splitstring_to_table_output;
   SET SpecAttributesCount =(SELECT COUNT(*) FROM FilteredSpecs);
   
-- ----------------------------------------------------------------------------------------------------
	-- filter by attributes
   SET @AttributesIds = IFNULL(AttributesIds,'');	
	DROP TEMPORARY TABLE IF EXISTS Attributes;	
   CREATE TEMPORARY TABLE Attributes
   (
      Id INT   AUTO_INCREMENT PRIMARY KEY,
      AttributeId INT,
      Name varchar(100) not null,
      INDEX(Name)
   )  AUTO_INCREMENT = 1;

   if(CHAR_LENGTH(RTRIM(AttributesIds)) > 1) then
      CALL nop_splitstring_to_table(AttributesIds,',');
      INSERT INTO Attributes(AttributeId, Name)
      SELECT
      SUBSTRING(data,1,LOCATE('-',data) -1) as id,
	  SUBSTRING(data,LOCATE('-',data)+1,CHAR_LENGTH(RTRIM(data))) as Name
      FROM nop_splitstring_to_table_output T0;
   end if;
   DROP TEMPORARY TABLE IF EXISTS TempProduct;
   if(@AttributesIds != '') then
	   SET @sqlquery = 'CREATE TEMPORARY TABLE TempProduct
	   AS
	   select p.Id as ProductId, p.* from Product p 
	   inner join Product_ProductAttribute_Mapping TA2 on TA2.ProductId = p.Id
	   inner join ProductAttributeValue TA3 on TA3.ProductAttributeMappingId = TA2.Id
	   inner join Attributes TA4 on TA4.AttributeId = TA2.ProductAttributeId and TA4.Name = TA3.Name ';
	   
		PREPARE stmt FROM @sqlquery;
		EXECUTE stmt;
		DEALLOCATE PREPARE stmt;
	end if;
    
-- ----------------------------------------------------------------------------------------------------   
	-- Add price minimum, max filter
    DROP TEMPORARY TABLE IF EXISTS PageIndexP;
    CREATE TEMPORARY TABLE PageIndexP
   (
      IndexId INT NOT NULL  AUTO_INCREMENT PRIMARY KEY,
      ProductId INT NOT NULL,
      ParentGroupedProductId INT NOT NULL,
      Price DECIMAL(18,4) not null
   )  AUTO_INCREMENT = 1;
   
   SET @mysqlquery = 'INSERT INTO PageIndexP (ProductId,ParentGroupedProductId,Price ) ';
   SET @mysqlquery = concat(@mysqlquery,' SELECT p.Id, p.ParentGroupedProductId, p.Price FROM Product p ');
		
   IF(CategoryIdsCount > 0) then
      SET @mysqlquery = concat(@mysqlquery, ' INNER JOIN Product_Category_Mapping pcm ON pcm.ProductId = p.Id ');
   end if;
		
   SET @mysqlquery = concat(@mysqlquery , ' WHERE p.Deleted = 0 AND p.Published = 1 ');
   IF StoreId > 0 then
      SET @mysqlquery = concat(@mysqlquery , '
		AND (p.LimitedToStores = 0 OR EXISTS (
			SELECT 1 FROM StoreMapping sm with (NOLOCK)
			WHERE sm.EntityId = p.Id AND sm.EntityName = ''Product'' AND sm.StoreId=' , CAST(StoreId AS NCHAR) , '
			))');
   end if;

   IF(CategoryIdsCount > 0) then
      SET @mysqlquery = concat(@mysqlquery , ' AND pcm.CategoryId IN (' , @CategoryIds , ') ');
   end if;

   PREPARE stmt FROM @mysqlquery;
   EXECUTE stmt;
   DEALLOCATE PREPARE stmt;

   INSERT INTO FILTERS(FilterType, FilterId, FilterCount, Name) SELECT 'MIN', IFNULL(FLOOR(MIN(p.Price)),0) AS nvarchar, 0, '' FROM PageIndexP pi INNER JOIN Product p ON p.Id = pi.ProductId;
   INSERT INTO FILTERS(FilterType, FilterId, FilterCount, Name) SELECT 'MAX', IFNULL(Max(p.Price),0) AS nvarchar, 0,  '' FROM PageIndexP pi INNER JOIN Product p ON p.Id = pi.ProductId;

	-- Add specification filters
   SET maxSetting = 0;

   IF SpecAttributesCount > 0 then
      SET spec = '';
      DROP TEMPORARY TABLE IF EXISTS FilteredSpecsWithAttributes;
      CREATE TEMPORARY TABLE FilteredSpecsWithAttributes
      (
         SpecificationAttributeId INT not null,
         SpecificationAttributeOptionId INT not null
      );
      INSERT INTO FilteredSpecsWithAttributes(SpecificationAttributeId, SpecificationAttributeOptionId)
      SELECT sao.SpecificationAttributeId, fs.SpecificationAttributeOptionId
      FROM FilteredSpecs fs INNER JOIN SpecificationAttributeOption sao
      ON sao.Id = fs.SpecificationAttributeOptionId
      ORDER BY sao.SpecificationAttributeId;
   end if;
   DROP TEMPORARY TABLE IF EXISTS TempSpProduct;
   SET @sqlquery = 'CREATE TEMPORARY TABLE TempSpProduct
   AS
   select p.Id as ProductId, p.* from Product p';
   IF SpecAttributesCount > 0 then
	SET @sqlquery = concat(@sqlquery,' INNER JOIN Product_SpecificationAttribute_Mapping PSM on (p.Id = PSM.ProductId) 
									   INNER JOIN FilteredSpecsWithAttributes fsa ON fsa.SpecificationAttributeOptionId = PSM.SpecificationAttributeOptionId  ');
   end if;
	PREPARE stmt FROM @sqlquery;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;

    -- Add product rating filters
   SET @mysqlquery =  'INSERT INTO FILTERS(FilterType, FilterId, FilterCount, Name, Color, SpecificationAttributeId, CheckedSpecificationAttributeId)
   SELECT distinct ''RAT'' AS FilterType, TPR.Rating, count(distinct TPR.ProductId) AS Qty, TPR.Rating, null, null, null
   FROM Product p
   INNER JOIN FilteredProductRatingIds TPR ON TPR.ProductId = p.Id  ';
   IF(CategoryIdsCount > 0) then
      SET @mysqlquery = concat(@mysqlquery, ' INNER JOIN Product_Category_Mapping pcm ON pcm.ProductId = p.Id ');
   end if;

   IF(@AttributesIds != '') then
		SET @mysqlquery = concat(@mysqlquery , 'inner join TempProduct TP on (TP.ProductId = p.Id) ');
   end if;
   IF SpecAttributesCount > 0 then
		SET @mysqlquery = concat(@mysqlquery , 'inner join TempSpProduct TSP on (TSP.ProductId = p.Id) ');
   end if;
   
   if(@VendorIds!='') then
		SET @mysqlquery = concat(@mysqlquery , ' inner join FilteredVendors T4 on T4.VendorId = p.VendorId ');
   End if;
   IF(@ManufacturerIds != '') then
			SET @mysqlquery = concat(@mysqlquery , ' inner join Product_Manufacturer_Mapping pmm on (p.Id = pmm.ProductId)
								inner join FilteredManufactureIds T30 on T30.ManufacturerId = pmm.ManufacturerId ');
   End if;
	if(@ProductTagIds !='') then
		SET @mysqlquery = concat(@mysqlquery , ' inner join Product_ProductTag_Mapping T6 on T6.Product_Id = p.Id 
								inner join FilteredProductTagIds T60 on T60.TagId = T6.ProductTag_Id ');
   End if;
   
   SET @mysqlquery = concat(@mysqlquery , ' WHERE p.Deleted = 0 AND p.Published = 1 ');
   IF PriceMin > 0 then
      SET @mysqlquery = concat(@mysqlquery, ' AND (p.Price >= ', CAST(PriceMin AS NCHAR), ') ');
   end if;
   IF PriceMax > 0 then
      SET @mysqlquery = concat(@mysqlquery, ' AND (p.Price <= ' , CAST(PriceMax AS NCHAR), ') ');
   end if;
   IF FilterFreeShipping > 0 then
      SET @mysqlquery = concat(@mysqlquery , ' AND (p.IsFreeShipping > 0) AND (p.IsShipEnabled > 0)  ');
   end if;
   IF FilterNewProduct > 0 then
      SET @mysqlquery = concat(@mysqlquery , ' AND (p.MarkAsNew > 0) ');
   end if;
   IF FilterDiscountedProduct > 0 then
      SET @mysqlquery = concat(@mysqlquery , ' AND (p.HasDiscountsApplied > 0) ');
   end if;
   IF FilterTaxExcemptProduct > 0 then
      SET @mysqlquery = concat(@mysqlquery , ' AND (p.IsTaxExempt > 0) ');
   end if;

	
	-- filter by store
   IF StoreId > 0 then
      SET @mysqlquery = concat(@mysqlquery , '
		AND (p.LimitedToStores = 0 OR EXISTS (
			SELECT 1 FROM StoreMapping sm with (NOLOCK)
			WHERE sm.EntityId = p.Id AND sm.EntityName = ''Product'' AND sm.StoreId=' , CAST(StoreId AS NCHAR) , '
			))');
   end if;

   IF(CategoryIdsCount > 0) then
      SET @mysqlquery = concat(@mysqlquery , ' AND pcm.CategoryId IN (' , @CategoryIds , ') ');
   end if;
   SET @mysqlquery = CONCAT(@mysqlquery , '  Group by TPR.Rating ');
   PREPARE stmt FROM @mysqlquery;
   EXECUTE stmt;
   DEALLOCATE PREPARE stmt;

	-- Add product tags filters
    SET @mysqlquery =  'INSERT INTO FILTERS(FilterType, FilterId, FilterCount, Name, Color, SpecificationAttributeId, CheckedSpecificationAttributeId)
   SELECT distinct ''TAG'' AS FilterType, PTM00.ProductTag_Id, count(distinct p.Id) AS Qty, PT00.Name, null, null, null
   FROM Product p
   INNER JOIN Product_ProductTag_Mapping PTM00 on (PTM00.Product_Id = p.Id)
   INNER JOIN ProductTag PT00 on (PT00.Id = PTM00.ProductTag_Id) ';
   IF(CategoryIdsCount > 0) then
      SET @mysqlquery = concat(@mysqlquery, ' INNER JOIN Product_Category_Mapping pcm ON pcm.ProductId = p.Id ');
   end if;
   IF(ProductRatingIds > 0) then
     SET @mysqlquery = concat(@mysqlquery, ' INNER JOIN FilteredProductRatingIds FPR ON FPR.ProductId = p.Id ');
   end if; 
   
   
   IF(@AttributesIds != '') then
		SET @mysqlquery = concat(@mysqlquery , 'inner join TempProduct TP on (TP.ProductId = p.Id) ');
   end if;
   IF SpecAttributesCount > 0 then
		SET @mysqlquery = concat(@mysqlquery , 'inner join TempSpProduct TSP on (TSP.ProductId = p.Id) ');
   end if;
   
   if(@VendorIds!='') then
		SET @mysqlquery = concat(@mysqlquery , ' inner join FilteredVendors T4 on T4.VendorId = p.VendorId ');
   End if;
   IF(@ManufacturerIds != '') then
			SET @mysqlquery = concat(@mysqlquery , ' inner join Product_Manufacturer_Mapping pmm on (p.Id = pmm.ProductId)
								inner join FilteredManufactureIds T30 on T30.ManufacturerId = pmm.ManufacturerId ');
   End if;
   
   
   SET @mysqlquery = concat(@mysqlquery , ' WHERE p.Deleted = 0 AND p.Published = 1 ');
   IF PriceMin > 0 then
      SET @mysqlquery = concat(@mysqlquery, ' AND (p.Price >= ', CAST(PriceMin AS NCHAR), ') ');
   end if;
   IF PriceMax > 0 then
      SET @mysqlquery = concat(@mysqlquery, ' AND (p.Price <= ' , CAST(PriceMax AS NCHAR), ') ');
   end if;
   IF FilterFreeShipping > 0 then
      SET @mysqlquery = concat(@mysqlquery , ' AND (p.IsFreeShipping > 0) AND (p.IsShipEnabled > 0)  ');
   end if;
   IF FilterNewProduct > 0 then
      SET @mysqlquery = concat(@mysqlquery , ' AND (p.MarkAsNew > 0) ');
   end if;
   IF FilterDiscountedProduct > 0 then
      SET @mysqlquery = concat(@mysqlquery , ' AND (p.HasDiscountsApplied > 0) ');
   end if;
   IF FilterTaxExcemptProduct > 0 then
      SET @mysqlquery = concat(@mysqlquery , ' AND (p.IsTaxExempt > 0) ');
   end if;
   IF(ProductRatingIds > 0) then
     SET @mysqlquery = concat(@mysqlquery, ' AND (FPR.Rating >= ', CAST(ProductRatingIds AS NCHAR), ') ');
   end if;
	
	-- filter by store
   IF StoreId > 0 then
      SET @mysqlquery = concat(@mysqlquery , '
		AND (p.LimitedToStores = 0 OR EXISTS (
			SELECT 1 FROM StoreMapping sm with (NOLOCK)
			WHERE sm.EntityId = p.Id AND sm.EntityName = ''Product'' AND sm.StoreId=' , CAST(StoreId AS NCHAR) , '
			))');
   end if;

   IF(CategoryIdsCount > 0) then
      SET @mysqlquery = concat(@mysqlquery , ' AND pcm.CategoryId IN (' , @CategoryIds , ') ');
   end if;
    SET @mysqlquery = CONCAT(@mysqlquery , ' Group by PTM00.ProductTag_Id, PT00.Name ');
   PREPARE stmt FROM @mysqlquery;
   EXECUTE stmt;
   DEALLOCATE PREPARE stmt;

	-- Add product attribute filters
    SET @mysqlquery =  'INSERT INTO FILTERS(FilterType, FilterId, FilterCount, Name, ProductAttributeId, CheckedProductAttributeId)
   SELECT distinct ''PAT'' AS FilterType, PAV.Id as FilterId, count(distinct p.Id) AS Qty, PAV.Name, PA.Id, PAV.Id
   From Product p
   inner join Product_ProductAttribute_Mapping PM on (PM.ProductId = p.Id)
   inner join ProductAttribute PA on (PA.Id = PM.ProductAttributeId)
   inner join ProductAttributeValue PAV on (PAV.ProductAttributeMappingId = PM.Id)  '; 
   IF(@AttributesIds != '') then
		SET @mysqlquery = concat(@mysqlquery , 'inner join TempProduct TP on (TP.ProductId = p.Id) ');
   end if;
   IF SpecAttributesCount > 0 then
		SET @mysqlquery = concat(@mysqlquery , 'inner join TempSpProduct TSP on (TSP.ProductId = p.Id) ');
   end if;
   
   if(@VendorIds!='') then
		SET @mysqlquery = concat(@mysqlquery , ' inner join FilteredVendors T4 on T4.VendorId = p.VendorId ');
   End if;
   IF(@ManufacturerIds != '') then
			SET @mysqlquery = concat(@mysqlquery , ' inner join Product_Manufacturer_Mapping pmm on (p.Id = pmm.ProductId)
								inner join FilteredManufactureIds T30 on T30.ManufacturerId = pmm.ManufacturerId ');
   End if;
   if(@ProductTagIds !='') then
		SET @mysqlquery = concat(@mysqlquery , ' inner join Product_ProductTag_Mapping T6 on T6.Product_Id = p.Id 
								inner join FilteredProductTagIds T60 on T60.TagId = T6.ProductTag_Id ');
   End if;
   IF(CategoryIdsCount > 0) then
      SET @mysqlquery = concat(@mysqlquery, ' INNER JOIN Product_Category_Mapping pcm ON pcm.ProductId = p.Id ');
   end if;
   IF(ProductRatingIds > 0) then
     SET @mysqlquery = concat(@mysqlquery, ' INNER JOIN FilteredProductRatingIds FPR ON FPR.ProductId = p.Id ');
   end if;
   SET @mysqlquery = concat(@mysqlquery , ' WHERE p.Deleted = 0  AND p.Published = 1 ');
   IF PriceMin > 0 then
      SET @mysqlquery = concat(@mysqlquery, ' AND (p.Price >= ', CAST(PriceMin AS NCHAR), ') ');
   end if;
   IF PriceMax > 0 then
      SET @mysqlquery = concat(@mysqlquery, ' AND (p.Price <= ' , CAST(PriceMax AS NCHAR), ') ');
   end if;
   IF FilterFreeShipping > 0 then
      SET @mysqlquery = concat(@mysqlquery , ' AND (p.IsFreeShipping > 0) AND (p.IsShipEnabled > 0) ');
   end if;
   IF FilterNewProduct > 0 then
      SET @mysqlquery = concat(@mysqlquery , ' AND (p.MarkAsNew > 0) ');
   end if;
   IF FilterDiscountedProduct > 0 then
      SET @mysqlquery = concat(@mysqlquery , ' AND (p.HasDiscountsApplied > 0) ');
   end if;
   IF FilterTaxExcemptProduct > 0 then
      SET @mysqlquery = concat(@mysqlquery , ' AND (p.IsTaxExempt > 0) ');
   end if;
   IF(ProductRatingIds > 0) then
     SET @mysqlquery = concat(@mysqlquery, ' AND (FPR.Rating >= ', CAST(ProductRatingIds AS NCHAR), ') ');
   end if;
	
	-- filter by store
   IF StoreId > 0 then
      SET @mysqlquery = concat(@mysqlquery , '
		AND (p.LimitedToStores = 0 OR EXISTS (
			SELECT 1 FROM StoreMapping sm with (NOLOCK)
			WHERE sm.EntityId = p.Id AND sm.EntityName = ''Product'' AND sm.StoreId=' , CAST(StoreId AS NCHAR) , '
			))');
   end if;

   IF(CategoryIdsCount > 0) then
      SET @mysqlquery = concat(@mysqlquery , ' AND pcm.CategoryId IN (' , @CategoryIds , ') ');
   end if;
    
   SET @mysqlquery = concat(@mysqlquery , ' Group by PA.Name,PA.Id,PAV.Id,PAV.Name  ');
   PREPARE stmt FROM @mysqlquery;
   EXECUTE stmt;
   DEALLOCATE PREPARE stmt; 
   
	-- Add product Specification filters
    IF FilteredSpecs = '' then
      SET @mysqlquery =  '	INSERT INTO TEMP_FILTERS (FilterType, FilterId, FilterCount, Name, Color, SpecificationAttributeId, CheckedSpecificationAttributeId)
							SELECT  distinct ''SPE'' AS FilterType, T00.SpecificationAttributeOptionId, count(distinct p.Id) AS Qty , Sao.Name, Sao.ColorSquaresRgb, Sao.SpecificationAttributeId,Sao.Id   FROM Product p
							INNER JOIN Product_SpecificationAttribute_Mapping T00 on (T00.ProductId = p.Id ) AND T00.AllowFiltering = 1
							INNER JOIN SpecificationAttributeOption Sao ON Sao.Id = T00.SpecificationAttributeOptionId   ';
	  IF(@AttributesIds != '') then
			SET @mysqlquery = concat(@mysqlquery , ' inner join TempProduct TP on (TP.ProductId = p.Id) ');
	  End if;
	   
	   IF(@ManufacturerIds != '') then
				SET @mysqlquery = concat(@mysqlquery , ' inner join Product_Manufacturer_Mapping pmm on (p.Id = pmm.ProductId)
									inner join FilteredManufactureIds T30 on T30.ManufacturerId = pmm.ManufacturerId ');
	   End if;
	   if(@ProductTagIds !='') then
			SET @mysqlquery = concat(@mysqlquery , ' inner join Product_ProductTag_Mapping T6 on T6.Product_Id = p.Id 
									inner join FilteredProductTagIds T60 on T60.TagId = T6.ProductTag_Id ');
	   End if;
       
       IF(CategoryIdsCount > 0) then
		  SET @mysqlquery = concat(@mysqlquery, ' INNER JOIN Product_Category_Mapping pcm ON pcm.ProductId = p.Id ');
	   end if;
	   IF(ProductRatingIds > 0) then
		 SET @mysqlquery = concat(@mysqlquery, ' INNER JOIN FilteredProductRatingIds FPR ON FPR.ProductId = p.Id ');
	   end if;
	   SET @mysqlquery = concat(@mysqlquery , ' WHERE p.Deleted = 0 AND p.Published = 1 ');
	   IF PriceMin > 0 then
		  SET @mysqlquery = concat(@mysqlquery, ' AND (p.Price >= ', CAST(PriceMin AS NCHAR), ') ');
	   end if;
	   IF PriceMax > 0 then
		  SET @mysqlquery = concat(@mysqlquery, ' AND (p.Price <= ' , CAST(PriceMax AS NCHAR), ') ');
	   end if;
	   IF FilterFreeShipping > 0 then
		  SET @mysqlquery = concat(@mysqlquery , ' AND (p.IsFreeShipping > 0) AND (p.IsShipEnabled > 0) ');
	   end if;
	   IF FilterNewProduct > 0 then
		  SET @mysqlquery = concat(@mysqlquery , ' AND (p.MarkAsNew > 0) ');
	   end if;
	   IF FilterDiscountedProduct > 0 then
		  SET @mysqlquery = concat(@mysqlquery , ' AND (p.HasDiscountsApplied > 0) ');
	   end if;
	   IF FilterTaxExcemptProduct > 0 then
		  SET @mysqlquery = concat(@mysqlquery , ' AND (p.IsTaxExempt > 0) ');
	   end if;
	   IF(ProductRatingIds > 0) then
		 SET @mysqlquery = concat(@mysqlquery, ' AND (FPR.Rating >= ', CAST(ProductRatingIds AS NCHAR), ') ');
	   end if;
		
		-- filter by store
	   IF StoreId > 0 then
		  SET @mysqlquery = concat(@mysqlquery , '
			AND (p.LimitedToStores = 0 OR EXISTS (
				SELECT 1 FROM StoreMapping sm with (NOLOCK)
				WHERE sm.EntityId = p.Id AND sm.EntityName = ''Product'' AND sm.StoreId=' , CAST(StoreId AS NCHAR) , '
				))');
	   end if;

	   IF(CategoryIdsCount > 0) then
		  SET @mysqlquery = concat(@mysqlquery , ' AND pcm.CategoryId IN (' , @CategoryIds , ') ');
	   end if;
	   
      SET @mysqlquery = concat(@mysqlquery , '  Group by SpecificationAttributeOptionId , Sao.Name, ColorSquaresRgb, SpecificationAttributeId, Sao.Id limit 40 ' );
     
      PREPARE stmt FROM @mysqlquery;
      EXECUTE stmt;
      DEALLOCATE PREPARE stmt;
      
      SET @mysqlquery = 'INSERT INTO  FILTERS (FilterType, FilterId, FilterCount, Name, Color, SpecificationAttributeId, CheckedSpecificationAttributeId, RN)
							SELECT FilterType, FilterId, FilterCount, Name, Color, SpecificationAttributeId, CheckedSpecificationAttributeId,
							ROW_NUMBER() OVER (PARTITION BY SpecificationAttributeId ORDER BY CheckedSpecificationAttributeId DESC) AS RN FROM TEMP_FILTERS';
      
      PREPARE stmt FROM @mysqlquery;
      EXECUTE stmt;
      DEALLOCATE PREPARE stmt;
   end if;
    
   IF(ViewMore = 0 and SpecAttributesCount > 0 and FilteredSpecs != '') then
		
      SET @mysqlquery =  'INSERT INTO TEMP_FILTERS (FilterType, FilterId, FilterCount, Name, Color, SpecificationAttributeId, CheckedSpecificationAttributeId) 
						SELECT  distinct ''SPE'' AS FilterType, T00.SpecificationAttributeOptionId, count(distinct p.Id) AS Qty , Sao.Name, Sao.ColorSquaresRgb, Sao.SpecificationAttributeId,Sao.Id   FROM Product p
						INNER JOIN Product_SpecificationAttribute_Mapping T00
						on (T00.ProductId = p.Id ) AND T00.AllowFiltering = 1
						INNER JOIN SpecificationAttributeOption Sao ON Sao.Id = T00.SpecificationAttributeOptionId ';
	  IF SpecAttributesCount > 0 then
			SET @mysqlquery = concat(@mysqlquery , 'inner join TempSpProduct TSP on (TSP.ProductId = p.Id) ');
	   end if;
       IF(@AttributesIds != '') then
			SET @mysqlquery = concat(@mysqlquery , ' inner join TempProduct TP on (TP.ProductId = p.Id) ');
	   End if;
	   
       if(@ProductTagIds !='') then
			SET @mysqlquery = concat(@mysqlquery , ' inner join Product_ProductTag_Mapping T6 on T6.Product_Id = p.Id 
													inner join FilteredProductTagIds T60 on T60.TagId = T6.ProductTag_Id ');
	   End if;
      IF(@ManufacturerIds != '') then
				SET @mysqlquery = concat(@mysqlquery , ' inner join Product_Manufacturer_Mapping pmm on (p.Id = pmm.ProductId)
									inner join FilteredManufactureIds T30 on T30.ManufacturerId = pmm.ManufacturerId ');
      end if;
      IF(CategoryIdsCount > 0) then
		  SET @mysqlquery = concat(@mysqlquery, ' INNER JOIN Product_Category_Mapping pcm ON pcm.ProductId = p.Id ');
	   end if;
	   IF(ProductRatingIds > 0) then
		 SET @mysqlquery = concat(@mysqlquery, ' INNER JOIN FilteredProductRatingIds FPR ON FPR.ProductId = p.Id ');
	   end if;
	   SET @mysqlquery = concat(@mysqlquery , ' WHERE p.Deleted = 0  AND p.Published = 1 ');
	   IF PriceMin > 0 then
		  SET @mysqlquery = concat(@mysqlquery, ' AND (p.Price >= ', CAST(PriceMin AS NCHAR), ') ');
	   end if;
	   IF PriceMax > 0 then
		  SET @mysqlquery = concat(@mysqlquery, ' AND (p.Price <= ' , CAST(PriceMax AS NCHAR), ') ');
	   end if;
	   IF FilterFreeShipping > 0 then
		  SET @mysqlquery = concat(@mysqlquery , ' AND (p.IsFreeShipping > 0) AND (p.IsShipEnabled > 0) ');
	   end if;
	   IF FilterNewProduct > 0 then
		  SET @mysqlquery = concat(@mysqlquery , ' AND (p.MarkAsNew > 0) ');
	   end if;
	   IF FilterDiscountedProduct > 0 then
		  SET @mysqlquery = concat(@mysqlquery , ' AND (p.HasDiscountsApplied > 0) ');
	   end if;
	   IF FilterTaxExcemptProduct > 0 then
		  SET @mysqlquery = concat(@mysqlquery , ' AND (p.IsTaxExempt > 0) ');
	   end if;
	   IF(ProductRatingIds > 0) then
		 SET @mysqlquery = concat(@mysqlquery, ' AND (FPR.Rating >= ', CAST(ProductRatingIds AS NCHAR), ') ');
	   end if;
		
		-- filter by store
	   IF StoreId > 0 then
		  SET @mysqlquery = concat(@mysqlquery , '
			AND (p.LimitedToStores = 0 OR EXISTS (
				SELECT 1 FROM StoreMapping sm with (NOLOCK)
				WHERE sm.EntityId = p.Id AND sm.EntityName = ''Product'' AND sm.StoreId=' , CAST(StoreId AS NCHAR) , '
				))');
	   end if;

	   IF(CategoryIdsCount > 0) then
		  SET @mysqlquery = concat(@mysqlquery , ' AND pcm.CategoryId IN (' , @CategoryIds , ') ');
	   end if;
      
      SET @mysqlquery = concat(@mysqlquery , ' Group by SpecificationAttributeOptionId , Name, ColorSquaresRgb, SpecificationAttributeId,Sao.Id  ');
      
      PREPARE stmt FROM @mysqlquery;
      EXECUTE stmt;
      DEALLOCATE PREPARE stmt;
   end if;

   IF(SpecAttributesCount > 0 AND ViewMore = 0) then
      SET @mysqlquery = 'INSERT INTO  FILTERS (FilterType, FilterId, FilterCount, Name, Color, SpecificationAttributeId, CheckedSpecificationAttributeId, RN)
						SELECT FilterType, FilterId, FilterCount, Name, Color, SpecificationAttributeId, CheckedSpecificationAttributeId,
						ROW_NUMBER() OVER (PARTITION BY SpecificationAttributeId ORDER BY CheckedSpecificationAttributeId DESC) AS RN FROM TEMP_FILTERS';
      
      PREPARE stmt FROM @mysqlquery;
      EXECUTE stmt;
      DEALLOCATE PREPARE stmt;
   end if;	

	-- view more specification
   IF(ViewMore = 1) then	
      SET @mysqlquery = concat('Insert into  FILTERS (FilterType, FilterId, FilterCount, Name, Color, SpecificationAttributeId, CheckedSpecificationAttributeId, RN)
						SELECT  FilterType, FilterId, count(FilterCount) Qty, Name, Color,SpecificationAttributeId,CheckedSpecificationAttributeId,0 AS RN  
						FROM TEMP_FILTERS WHERE SpecificationAttributeId = ' , convert(SelectedSpecificationAttributeId,NCHAR(10)));
      IF(FilterBy != '') then	
         SET @mysqlquery = concat(@mysqlquery , ' AND (Name like ''%' , FilterBy , '%'' OR CheckedSpecificationAttributeId > 0)');
      end if;
      SET @mysqlquery = concat(@mysqlquery , ' GROUP BY  FilterType, FilterId, Name, Color, SpecificationAttributeId , CheckedSpecificationAttributeId  
				ORDER BY CheckedSpecificationAttributeId desc 
				offset ' , convert(PageSize*(PageIndexAt -1),NCHAR(10)) , 'rows
				fetch next ' , convert(PageSize,NCHAR(10)) , ' rows only');

      
      PREPARE stmt FROM @mysqlquery;
      EXECUTE stmt;
      DEALLOCATE PREPARE stmt;
   end if;
   
   IF(ViewMore = 0) then
		
      SET @mysqlquery = 'INSERT INTO  FILTERS (FilterType, FilterId, FilterCount)  
			 SELECT distinct ''MAN'' AS FilterType, pmm.ManufacturerId, count(distinct pcm.Id) Qty FROM Product p
			 INNER JOIN Product_Category_Mapping pcm ON pcm.ProductId = p.Id
			 INNER JOIN Product_Manufacturer_Mapping pmm ON  p.Id = pmm.ProductId
			 INNER JOIN Product_ProductTag_Mapping ptm ON  p.Id = ptm.Product_Id';
      IF(FilteredSpecs != '') then
				
         SET @mysqlquery = concat(@mysqlquery , ' INNER JOIN Product_SpecificationAttribute_Mapping psam
									ON p.Id = psam.ProductId');
      end if;
      
      IF(AttributesIds != '') then
				
         SET @mysqlquery = concat(@mysqlquery , ' INNER JOIN TempProduct TP
									ON p.Id = TP.ProductId');
      end if;
      if(@VendorIds!='') then
		 SET @mysqlquery = concat(@mysqlquery , ' inner join FilteredVendors T4 on T4.VendorId = p.VendorId ');
	  end if;
	  if(@ProductTagIds !='') then
		 SET @mysqlquery = concat(@mysqlquery , ' inner join Product_ProductTag_Mapping T6 on T6.Product_Id = p.Id 
								inner join FilteredProductTagIds T60 on T60.TagId = T6.ProductTag_Id ');
	  end if;
	   IF(ProductRatingIds > 0) then
		 SET @mysqlquery = concat(@mysqlquery, ' INNER JOIN FilteredProductRatingIds FPR ON FPR.ProductId = p.Id ');
	   end if;
	   SET @mysqlquery = concat(@mysqlquery , ' WHERE p.Deleted = 0  AND p.Published = 1 ');
	   IF PriceMin > 0 then
		  SET @mysqlquery = concat(@mysqlquery, ' AND (p.Price >= ', CAST(PriceMin AS NCHAR), ') ');
	   end if;
	   IF PriceMax > 0 then
		  SET @mysqlquery = concat(@mysqlquery, ' AND (p.Price <= ' , CAST(PriceMax AS NCHAR), ') ');
	   end if;
	   IF FilterFreeShipping > 0 then
		  SET @mysqlquery = concat(@mysqlquery , ' AND (p.IsFreeShipping > 0) AND (p.IsShipEnabled > 0)  ');
	   end if;
	   IF FilterNewProduct > 0 then
		  SET @mysqlquery = concat(@mysqlquery , ' AND (p.MarkAsNew > 0) ');
	   end if;
	   IF FilterDiscountedProduct > 0 then
		  SET @mysqlquery = concat(@mysqlquery , ' AND (p.HasDiscountsApplied > 0) ');
	   end if;
	   IF FilterTaxExcemptProduct > 0 then
		  SET @mysqlquery = concat(@mysqlquery , ' AND (p.IsTaxExempt > 0) ');
	   end if;
	   IF(ProductRatingIds > 0) then
		 SET @mysqlquery = concat(@mysqlquery, ' AND (FPR.Rating >= ', CAST(ProductRatingIds AS NCHAR), ') ');
	   end if;
		
		-- filter by store
	   IF StoreId > 0 then
		  SET @mysqlquery = concat(@mysqlquery , '
			AND (p.LimitedToStores = 0 OR EXISTS (
				SELECT 1 FROM StoreMapping sm with (NOLOCK)
				WHERE sm.EntityId = p.Id AND sm.EntityName = ''Product'' AND sm.StoreId=' , CAST(StoreId AS NCHAR) , '
				))');
	   end if;

	   IF(CategoryIdsCount > 0) then
		  SET @mysqlquery = concat(@mysqlquery , ' AND pcm.CategoryId IN (' , @CategoryIds , ') ');
	   end if;
      
      SET @mysqlquery = concat(@mysqlquery , ' GROUP BY pmm.ManufacturerId');
      
      PREPARE stmt FROM @mysqlquery;
      EXECUTE stmt;
      DEALLOCATE PREPARE stmt;
   end if;
   
   SELECT * FROM FILTERS;
END