CREATE PROCEDURE `NS_OrderCommissionLoadAllPaged`
(
	AffiliateFirstName			longtext,
	AffiliateLastName			longtext,
	AffiliateId					int,
	OrderId						int,
	StartDateUtc				datetime,	
	EndDateUtc					datetime,
	FilteredComssionStatusIds	longtext,
	FilteredOrderStatusIds		longtext,
	FilteredPaymentStatusIds	longtext,
	PageIndex					int, 
	PageSize					int,
	LoadCommission				tinyint,
	OUT TotalRecords			int,
	OUT TotalCommission			decimal(18, 6),
	OUT PayableCommission		decimal(18, 6),
	OUT PaidCommission			decimal(18, 6)
)
BEGIN

	DECLARE `sql_command` longtext;
    DECLARE `PageLowerBound` int;
	DECLARE `PageUpperBound` int;
	DECLARE `RowsToReturn` int;
    
    set @sql_command = '';
	SET AffiliateFirstName = IFNULL(AffiliateFirstName, '');
	SET AffiliateLastName = IFNULL(AffiliateLastName, '');
	SET FilteredComssionStatusIds = IFNULL(FilteredComssionStatusIds, '');
	SET FilteredOrderStatusIds = IFNULL(FilteredOrderStatusIds, '');
	SET FilteredPaymentStatusIds = IFNULL(FilteredPaymentStatusIds, '');

	drop temporary table if EXISTS PageIndex;
	CREATE TEMPORARY TABLE PageIndex 
	(
		`IndexId` int NOT NULL AUTO_INCREMENT,
		`OrderCommissionId` int NOT NULL,
        primary key (`IndexId`)
	);

	SET @sql_command = 'INSERT INTO PageIndex (OrderCommissionId)
		SELECT oc.Id from NS_OrderCommission oc INNER JOIN Affiliate af on af.Id = oc.AffiliateId 
		INNER JOIN `Order` o ON o.Id = oc.OrderId ';

	if(AffiliateFirstName != '' OR AffiliateLastName != '') then
		SET @sql_command = Concat(@sql_command , 'INNER JOIN Address ad on ad.Id = af.AddressId ');
	end if;

	SET @sql_command = Concat(@sql_command , 'WHERE af.Deleted = 0 ');
	
	if(AffiliateFirstName != '') then
		SET @sql_command = Concat(@sql_command , 'AND ad.FirstName LIKE  ''%' , AffiliateFirstName , '%'' ');
	end if;
	
	if(AffiliateLastName != '') then
		SET @sql_command = Concat(@sql_command , 'AND ad.LastName LIKE  ''%' , AffiliateLastName , '%'' ');
	end if;
	
	if(AffiliateId > 0) then
		SET @sql_command = Concat(@sql_command , 'AND oc.AffiliateId = ' , CAST(AffiliateId as char) , ' ');
	end if;
	
	if(OrderId > 0) then
		SET @sql_command = Concat(@sql_command , 'AND oc.OrderId = ' , CAST(OrderId as char) , ' ');
	end if;
		
	if(StartDateUtc IS NOT NULL) then
		SET @sql_command = Concat(@sql_command , 'AND o.CreatedOnUtc >= ''' , CAST(StartDateUtc as char) , ''' ');
	end if;
		
	if(EndDateUtc IS NOT NULL) then
		SET @sql_command = Concat(@sql_command , 'AND o.CreatedOnUtc <= ''' , CAST(EndDateUtc as char) , ''' ');
	end if;

	if(FilteredComssionStatusIds != '') then
		SET @sql_command = Concat(@sql_command , 'AND oc.CommissionStatusId IN (' , FilteredComssionStatusIds , ') ');
	end if;
		
	if(FilteredOrderStatusIds != '') then
		SET @sql_command = Concat(@sql_command , 'AND o.OrderStatusId IN (' , FilteredOrderStatusIds , ') ');
	end if;
		
	if(FilteredPaymentStatusIds != '') then
		SET @sql_command = Concat(@sql_command , 'AND o.PaymentStatusId IN (' , FilteredPaymentStatusIds , ') ');
	end if;

	SET @sql_command = Concat(@sql_command , 'ORDER BY o.CreatedOnUtc desc ');
	
	-- PRINT (@sql)
	prepare stmt1 from @sql_command;
	EXECUTE stmt1;
    
	-- total records
	SET TotalRecords = Found_rows();

	if(LoadCommission = 1)
		THEN
			SELECT Sum(oc.TotalCommissionAmount) INTO TotalCommission
			FROM
				`PageIndex` pi
				INNER JOIN NS_OrderCommission oc on oc.Id = pi.OrderCommissionId;

			SELECT Sum(oc.TotalCommissionAmount) INTO PayableCommission
			FROM
				`PageIndex` pi
				INNER JOIN NS_OrderCommission oc on oc.Id = pi.OrderCommissionId
				INNER JOIN `Order` o  on o.Id = oc.OrderId
				WHERE o.PaymentStatusId = 30;
				
			SELECT Sum(
				CASE 
					WHEN oc.CommissionStatusId = 20 THEN oc.PartialPaidAmount
					ELSE oc.TotalCommissionAmount
				END) INTO PaidCommission
			FROM
				`PageIndex` pi
				INNER JOIN NS_OrderCommission oc on oc.Id = pi.OrderCommissionId
				INNER JOIN `Order` o on o.Id = `oc`.OrderId
				WHERE o.PaymentStatusId = 30 AND (oc.CommissionStatusId = 30 OR oc.CommissionStatusId = 20);
		END IF;

	-- paging

	SET RowsToReturn = PageSize * (PageIndex + 1);	
	SET PageLowerBound = PageSize * PageIndex;
	SET PageUpperBound = PageLowerBound + PageSize + 1;
	
	-- return products
	SELECT oc.*
	FROM
		`PageIndex` pi
		INNER JOIN NS_OrderCommission oc on oc.Id = pi.OrderCommissionId
	WHERE
		pi.IndexId > PageLowerBound AND 
		pi.IndexId < PageUpperBound
        
	ORDER BY oc.OrderId DESC
	limit `RowsToReturn`;
    
	DROP TEMPORARY TABLE PageIndex;
END;
