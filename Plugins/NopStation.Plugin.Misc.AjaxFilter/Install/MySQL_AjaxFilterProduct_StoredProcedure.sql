CREATE PROCEDURE AjaxFilterProduct(
    CategoryIds LONGTEXT ,	
	ManufacturerIds	LONGTEXT ,
	StoreId			INT ,
	VendorIds			LONGTEXT ,
	PriceMin			DECIMAL(18,4) ,
	PriceMax			DECIMAL(18,4) ,	
	FilteredSpecs LONGTEXT ,
	AllowedCustomerRoleIds	LONGTEXT ,	
	AttributesIds LONGTEXT ,
	ShowHidden			BOOLEAN ,
	OrderBy			INT ,
	PageIndex			INT , 
	PageSize			INT ,
	FilterInStock		BOOLEAN ,
	FilterNotInStock	BOOLEAN ,
	StockQuantity		INT ,
	FilterFreeShipping	BOOLEAN ,
	FilterTaxExcemptProduct BOOLEAN ,
	FilterDiscountedProduct BOOLEAN ,
	FilterNewProduct BOOLEAN ,
	ProductRatingIds INT ,
	FilterProductTag  BOOLEAN ,
	ProductTagIds	LONGTEXT ,
	INOUT TotalRecords		INT)
BEGIN
   DECLARE sqlquery LONGTEXT;
   DECLARE sqlquery_orderby LONGTEXT;
   DECLARE CategoryIdsCount INT;	
   DECLARE PageLowerBound INT;
   DECLARE PageUpperBound INT;
   DECLARE RowsToReturn INT;
   DECLARE spec LONGTEXT;
   DECLARE normalSpec NVARCHAR(255);

-- ----------------------------------------------------------------------------------------------------
	-- Filters table

   IF StoreId is null then
      set StoreId = 0;
   END IF;
   IF ShowHidden is null then
      set ShowHidden = 0;
   END IF;
   IF OrderBy is null then
      set OrderBy = 0;
   END IF;
   IF PageIndex is null then
      set PageIndex = 0;
   END IF;
   IF PageSize is null then
      set PageSize = 2147483644;
   END IF;
   IF FilterInStock is null then
      set FilterInStock = 0;
   END IF;
   IF FilterNotInStock is null then
      set FilterNotInStock = 0;
   END IF;
   IF StockQuantity is null then
      set StockQuantity = 0;
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
   IF FilterProductTag is null then
      set FilterProductTag = 0;
   END IF;
   
   DROP TEMPORARY TABLE IF EXISTS PageIndex;
   DROP TEMPORARY TABLE IF EXISTS PageIndexHelper;
   
   CREATE TEMPORARY TABLE PageIndex
   (
      IndexId INT NOT NULL  AUTO_INCREMENT PRIMARY KEY,
      ProductId INT NOT NULL
   )  AUTO_INCREMENT = 1;

   CREATE TEMPORARY TABLE PageIndexHelper
   (
      IndexId INT NOT NULL  AUTO_INCREMENT PRIMARY KEY,
      ProductId INT NOT NULL
   )  AUTO_INCREMENT = 1;

-- ----------------------------------------------------------------------------------------------------
	-- filter by category IDs
    
   SET @CategoryIds = IFNULL(CategoryIds,'');	

   DROP TEMPORARY TABLE IF EXISTS FilteredCategoryIds;

   CREATE TEMPORARY TABLE FilteredCategoryIds
   (
      CategoryId INT not null
   );

   CALL nop_splitstring_to_table(CategoryIds,',');
   INSERT INTO FilteredCategoryIds(CategoryId)
   SELECT cast(data as SIGNED INTEGER) FROM nop_splitstring_to_table_output;	

   SET @CategoryIdsCount =(SELECT COUNT(1) FROM FilteredCategoryIds);

-- ----------------------------------------------------------------------------------------------------
	-- filter by manufacturer IDs
   SET @ManufacturerIds = IFNULL(ManufacturerIds,'');	
   DROP TEMPORARY TABLE IF EXISTS FilteredManufactureIds;	
   CREATE TEMPORARY TABLE FilteredManufactureIds
   (
      ManufacturerId INT not null
   );
   CALL nop_splitstring_to_table(ManufacturerIds,',');
   INSERT INTO FilteredManufactureIds(ManufacturerId)
   SELECT cast(data as SIGNED INTEGER) FROM nop_splitstring_to_table_output;	

-- ----------------------------------------------------------------------------------------------------
	-- filter by product tag IDs
   SET @ProductTagIds = IFNULL(ProductTagIds,'');	
   DROP TEMPORARY TABLE IF EXISTS FilteredProductTagIds;	
   CREATE TEMPORARY TABLE FilteredProductTagIds
   (
      TagId INT not null
   );
   CALL nop_splitstring_to_table(ProductTagIds,',');
   INSERT INTO FilteredProductTagIds(TagId)
   SELECT cast(data as SIGNED INTEGER) FROM nop_splitstring_to_table_output;	
   
-- ----------------------------------------------------------------------------------------------------
	-- filter by product ratings
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
	-- filter by vendor
   SET @VendorIds = IFNULL(VendorIds,'');	
   DROP TEMPORARY TABLE IF EXISTS FilteredVendorIds;	
   CREATE TEMPORARY TABLE FilteredVendorIds
   (
      VendorId INT not null
   );
   CALL nop_splitstring_to_table(VendorIds,',');
   INSERT INTO FilteredVendorIds(VendorId)
   SELECT cast(data as SIGNED INTEGER) FROM nop_splitstring_to_table_output;	

-- ----------------------------------------------------------------------------------------------------
	-- filter by specification
   SET @FilteredSpecs = IFNULL(FilteredSpecs,'');	
   DROP TEMPORARY TABLE IF EXISTS FilteredSpecs;	
   CREATE TEMPORARY TABLE FilteredSpecs
   (
      SpecificationAttributeOptionId INT not null
   );
   CALL nop_splitstring_to_table(FilteredSpecs,',');
   INSERT INTO FilteredSpecs(SpecificationAttributeOptionId)
   SELECT cast(data as SIGNED INTEGER) FROM nop_splitstring_to_table_output;

-- ----------------------------------------------------------------------------------------------------
	-- filter by attributes
   SET @AttributesIds = IFNULL(AttributesIds,'');	
   DROP TEMPORARY TABLE IF EXISTS Attributes;	
   CREATE TEMPORARY TABLE Attributes
   (
      Id INT   AUTO_INCREMENT PRIMARY KEY,
      AttributeId INT,
      Name LONGTEXT not null
   )  AUTO_INCREMENT = 1;

   if(CHAR_LENGTH(RTRIM(AttributesIds)) > 1) then
      CALL nop_splitstring_to_table(AttributesIds,',');
      INSERT INTO Attributes(AttributeId, Name)
      SELECT
      SUBSTRING(data,1,LOCATE('-',data) -1) as id,
	  SUBSTRING(data,LOCATE('-',data)+1,CHAR_LENGTH(RTRIM(data))) as Name
      FROM nop_splitstring_to_table_output T0;
   end if;

	-- paging
   SET RowsToReturn = PageSize*(PageIndex+1);	
   SET PageLowerBound = PageSize*PageIndex;
   SET PageUpperBound = PageLowerBound+(PageSize+1);
   
   SET @sqlquery = 'INSERT INTO PageIndexHelper (ProductId) select T0.Id from Product T0 ';

-- ----------------------------------------------------------------------------------------------------
	-- join by category ids
   if(CategoryIds != '') then
      SET @sqlquery = concat( @sqlquery, ' inner join Product_Category_Mapping T1 on T1.ProductId = T0.Id
						inner join FilteredCategoryIds T2 on T2.CategoryId = T1.CategoryId ');
     
   end if;
   
-- ----------------------------------------------------------------------------------------------------
	-- join by manufacture ids

   if(ManufacturerIds != '') then
	
      SET @sqlquery = concat(@sqlquery, ' inner join Product_Manufacturer_Mapping T3 on T3.ProductId = T0.Id
						   inner join FilteredManufactureIds T30 on T30.ManufacturerId = T3.ManufacturerId ');
   end if;	

-- ----------------------------------------------------------------------------------------------------
	-- join by product rating

   if(ProductRatingIds > 0) then
	
      SET @sqlquery = concat(@sqlquery,' inner join FilteredProductRatingIds FPR on FPR.ProductId = T0.Id ');
   end if;

-- ----------------------------------------------------------------------------------------------------
	-- join by vendor ids

   if(VendorIds != '') then
	
      SET @sqlquery = concat(@sqlquery, ' inner join FilteredVendorIds T4 on T4.VendorId = T0.VendorId ');
   end if;

-- ----------------------------------------------------------------------------------------------------
	-- join by product tag

   if(ProductTagIds != '') then
      SET @sqlquery = concat(@sqlquery, ' inner join Product_ProductTag_Mapping T6 on T6.Product_Id = T0.Id 
							inner join FilteredProductTagIds T60 on T60.TagId = T6.ProductTag_Id');
   end if;

-- ----------------------------------------------------------------------------------------------------
	-- join by specs
   IF FilteredSpecs != '' then
      DROP TEMPORARY TABLE IF EXISTS FilteredSpecsWithAttributes;
      CREATE TEMPORARY TABLE FilteredSpecsWithAttributes
      (
         SpecificationAttributeId INT not null,
         SpecificationAttributeOptionId INT not null
      );
      INSERT INTO FilteredSpecsWithAttributes(SpecificationAttributeId, SpecificationAttributeOptionId)
      SELECT sao.SpecificationAttributeId, fs.SpecificationAttributeOptionId
      FROM FilteredSpecs fs INNER JOIN SpecificationAttributeOption sao ON sao.Id = fs.SpecificationAttributeOptionId
      ORDER BY sao.SpecificationAttributeId;
      select   CONCAT(coalesce(CONCAT(normalSpec,','),''),cast(SpecificationAttributeOptionId as NCHAR(5))) INTO normalSpec from FilteredSpecsWithAttributes fsa limit 1;
      IF(normalSpec != '') then
         set spec =  CONCAT(' inner join Product_SpecificationAttribute_Mapping TS on TS.ProductId = T0.Id 
							  inner join FilteredSpecsWithAttributes FSA  on TS.SpecificationAttributeOptionId = FSA.SpecificationAttributeOptionId  ');
         SET @sqlquery = Concat(@sqlquery, spec);
      end if;
   end if;

-- ----------------------------------------------------------------------------------------------------
	-- join by attributes

   if(AttributesIds != '') then
	  SET @sqlquery = concat(@sqlquery , ' inner join Product_ProductAttribute_Mapping TA2 on TA2.ProductId = T0.Id
	   inner join ProductAttributeValue TA3 on TA3.ProductAttributeMappingId = TA2.Id
	   inner join Attributes TA4 on TA4.AttributeId = TA2.ProductAttributeId and TA4.Name = TA3.Name ');
   end if;
   SET @sqlquery = concat(@sqlquery, ' where T0.Deleted = 0 and T0.Published = 1 ');
   
-- ----------------------------------------------------------------------------------------------------
	-- filter by store id

   IF StoreId > 0 then
	
      SET @sqlquery = concat(@sqlquery , ' 
		AND (T0.LimitedToStores = 0 OR EXISTS (
			SELECT 1 FROM StoreMapping sm with (NOLOCK)
			WHERE sm.EntityId = T0.Id AND sm.EntityName = ''Product'' and sm.StoreId=' , CAST(StoreId AS NCHAR) , '
			)) ');
   end if;

-- ----------------------------------------------------------------------------------------------------
	-- filter by stock

   IF FilterInStock > 0 then
      IF StockQuantity > 0 then
         SET @sqlquery = concat(@sqlquery ,' AND T0.StockQuantity >=' ,  CAST(StockQuantity AS NCHAR));
      ELSE
         SET @sqlquery = concat(@sqlquery , ' AND (T0.StockQuantity > 0) ');
      end if;
   end if;

-- ----------------------------------------------------------------------------------------------------
	-- filter by not in stock

   IF FilterNotInStock  > 0 then
      SET @sqlquery = concat(@sqlquery , ' AND (T0.StockQuantity <= 0) ');
   end if;

-- ----------------------------------------------------------------------------------------------------
	-- filter by product rating

   IF ProductRatingIds > 0 then
	
      SET @sqlquery = concat(@sqlquery , ' AND (FPR.Rating >= ', CAST(ProductRatingIds AS NCHAR), ') ');
      
end if;


-- ----------------------------------------------------------------------------------------------------
	-- filter by free shipping

   IF FilterFreeShipping > 0 then
	
      SET @sqlquery = concat(@sqlquery , ' AND (T0.IsFreeShipping > 0) AND (T0.IsShipEnabled > 0) ');
   end if;

	
-- ----------------------------------------------------------------------------------------------------
	-- filter by new product

   IF FilterNewProduct > 0 then
	
      SET @sqlquery = concat(@sqlquery , ' AND (T0.MarkAsNew > 0) ');
   end if;

-- ----------------------------------------------------------------------------------------------------
	-- filter by discounted product

   IF FilterDiscountedProduct > 0 then
	
      SET @sqlquery = concat(@sqlquery , ' AND (T0.HasDiscountsApplied > 0) ');
   end if;

-- ----------------------------------------------------------------------------------------------------
	-- filter by tax exempt product

   IF FilterTaxExcemptProduct > 0 then
	
      SET @sqlquery = concat(@sqlquery , ' AND (T0.IsTaxExempt > 0) ');
   end if;

-- ----------------------------------------------------------------------------------------------------
	-- filter by minimum price

   IF PriceMin > 0 then
      SET @sqlquery = concat(@sqlquery, ' AND (T0.Price >= ', CAST(PriceMin AS NCHAR), ') ');
   end if;
	
-- ----------------------------------------------------------------------------------------------------
	-- filter by max price

   IF PriceMax > 0 then
      SET @sqlquery = concat(@sqlquery, ' AND (T0.Price <= ' , CAST(PriceMax AS NCHAR), ') ');
   end if;
-- ----------------------------------------------------------------------------------------------------

	-- sorting
	SET @sqlquery_orderby = ''	;
	IF OrderBy = 5 then /* Name: A to Z */
		SET @sqlquery_orderby = ' T0.Name ASC';
	ELSEIF OrderBy = 6  then /* Name: Z to A */
		SET @sqlquery_orderby = ' T0.Name DESC';
	ELSEIF OrderBy = 10  then /* Price: Low to High */
		SET @sqlquery_orderby = ' T0.Price ASC';
	ELSEIF OrderBy = 11  then /* Price: High to Low */
		SET @sqlquery_orderby = ' T0.Price DESC';
	ELSEIF OrderBy = 15  then /* creation date */
		SET @sqlquery_orderby = ' T0.CreatedOnUtc DESC';
    ELSEIF OrderBy = 20  then /* creation date */
		SET @sqlquery_orderby = ' T0.CreatedOnUtc ASC';
    ELSEIF OrderBy = 21  then /* creation date */
		SET @sqlquery_orderby = ' T0.CreatedOnUtc DESC';
	ELSE /* default sorting, 0 (position) */
        BEGIN
            -- category position (display order)
            IF @CategoryIdsCount > 0 then SET @sqlquery_orderby = ' T1.DisplayOrder ASC';
            END IF;
            -- name
            IF CHAR_LENGTH(@sqlquery_orderby) > 0 then SET @sqlquery_orderby = CONCAT(@sqlquery_orderby, ', ');
            END IF;

            SET @sqlquery_orderby = CONCAT(@sqlquery_orderby, ' T0.Name ASC');
        END;
	END IF;

    SET @sqlquery = concat(@sqlquery,'ORDER BY ',@sqlquery_orderby,' ');
	
    PREPARE stmt FROM @sqlquery;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt; 

    set @sqlquery = 'INSERT INTO PageIndex(ProductId) select ProductId from PageIndexHelper GROUP BY ProductId ORDER BY MAX(IndexId) ';
 
    PREPARE stmt FROM @sqlquery;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt; 

-- ----------------------------------------------------------------------------------------------------
	-- total records
	SET TotalRecords = found_rows();

-- ----------------------------------------------------------------------------------------------------
	-- return products
   SET SQL_SELECT_LIMIT = RowsToReturn;
   SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
   SELECT p.* FROM
   PageIndex pi
   INNER JOIN Product p  on p.Id = pi.ProductId
   WHERE
   pi.IndexId > PageLowerBound AND
   pi.IndexId < PageUpperBound
   ORDER BY
   pi.IndexId;
   SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ;
   SET SQL_SELECT_LIMIT = DEFAULT;

END