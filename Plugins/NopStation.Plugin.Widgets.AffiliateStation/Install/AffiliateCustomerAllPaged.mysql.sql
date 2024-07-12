CREATE PROCEDURE `NS_AffiliateCustomerAllPaged`
(
	CustomerEmail				longtext,
	AffiliateFirstName			longtext,
	AffiliateLastName			longtext,
	`Active`						tinyint,
	CreatedFromUtc				datetime,	
	CreatedToUtc				datetime,
	ApplyStatusIdIds			longtext,
	PageIndex					int, 
	PageSize					int,
	OUT TotalRecords			int
)
BEGIN
	DECLARE `sql_command` longtext;
    DECLARE `PageLowerBound` int;
	DECLARE `PageUpperBound` int;
	DECLARE `RowsToReturn` int;
    
	SET AffiliateFirstName = IFNULL(AffiliateFirstName, '');
	SET AffiliateLastName = IFNULL(AffiliateLastName, '');
	SET CustomerEmail = IFNULL(CustomerEmail, '');
	SET ApplyStatusIdIds = IFNULL(ApplyStatusIdIds, '');

	drop temporary table if EXISTS PageIndex;
	CREATE TEMPORARY TABLE PageIndex 
	(
		`IndexId` int NOT NULL AUTO_INCREMENT,
		`AffiliateCustomerId` int NOT NULL,
		primary key (`IndexId`)
	);

	SET @sql_command = 'INSERT INTO PageIndex (AffiliateCustomerId)
		SELECT ac.Id from NS_AffiliateCustomer ac 
		INNER JOIN Affiliate af on af.Id = ac.AffiliateId
		INNER JOIN Address ad on ad.Id = af.AddressId 
		INNER JOIN Customer c on c.Id = ac.CustomerId 
		WHERE af.Deleted = 0 AND c.Deleted = 0 ';
		
	if(CustomerEmail != '') then
		SET @sql_command = Concat(@sql_command , 'AND c.Email LIKE  ''%' , CustomerEmail , '%'' ');
	end if;
	
	if(AffiliateFirstName != '') then
		SET @sql_command = Concat(@sql_command , 'AND ad.FirstName LIKE  ''%' , AffiliateFirstName , '%'' ');
	end if;
	
	if(AffiliateLastName != '') then
		SET @sql_command = Concat(@sql_command , 'AND ad.LastName LIKE  ''%' , AffiliateLastName , '%'' ');
	end if;

	if(`Active` IS NOT NULL) then
		SET @sql_command = Concat(@sql_command , 'AND af.Active = ' , CAST(`Active` as char) , ' ');
	end if;
		
	if(CreatedFromUtc IS NOT NULL) then
		SET @sql_command = Concat(@sql_command , 'AND ac.CreatedOnUtc >= ''' , CAST(CreatedFromUtc as char) , ''' ');
	end if;
		
	if(CreatedToUtc IS NOT NULL) then
		SET @sql_command = Concat(@sql_command , 'AND ac.CreatedOnUtc <= ''' , CAST(CreatedToUtc as char) , ''' ');
	end if;
	
	if(ApplyStatusIdIds != '') then
		SET @sql_command = Concat(@sql_command , 'AND ac.ApplyStatusId IN (' , ApplyStatusIdIds , ') ');
	end if;

	-- PRINT (@sql)
	prepare stmt1 from @sql_command;
		EXECUTE stmt1;

	-- paging
	SET RowsToReturn = PageSize * (PageIndex + 1);	
	SET PageLowerBound = PageSize * PageIndex;
	SET PageUpperBound = PageLowerBound + PageSize + 1;
	
	-- total records
	SET TotalRecords = Found_rows();
	
	-- return products
	SELECT oc.*
	FROM
		`PageIndex` pi
		INNER JOIN NS_AffiliateCustomer oc on oc.Id = pi.AffiliateCustomerId
	WHERE
		pi.IndexId > PageLowerBound AND 
		pi.IndexId < PageUpperBound
	ORDER BY pi.IndexId
            limit `RowsToReturn`;
	
	DROP TABLE PageIndex;
END;


