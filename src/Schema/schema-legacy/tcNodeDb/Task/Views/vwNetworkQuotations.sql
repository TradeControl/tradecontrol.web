CREATE VIEW Task.vwNetworkQuotations
AS
	WITH requests AS
	(
		SELECT mirror.ActivityCode, alloc.AccountCode, alloc.TaskCode, alloc.ActionOn, 
			CASE CashModeCode WHEN 0 THEN 1 ELSE 0 END SupplyOrder, CAST(1 AS bit) IsAllocation, UnitCharge,
			CASE CashModeCode 
				WHEN 0 THEN (alloc.QuantityOrdered - alloc.QuantityDelivered) * -1
				WHEN 1 THEN (alloc.QuantityOrdered - alloc.QuantityDelivered)
			END Quantity,
			CASE CashModeCode 
				WHEN 0 THEN 1
				WHEN 1 THEN 0
			END CashModeCode			
		FROM Task.tbAllocation alloc
			JOIN Activity.tbMirror mirror ON alloc.AccountCode = mirror.AccountCode AND alloc.AllocationCode = mirror.AllocationCode
		WHERE TaskStatusCode = 0	
	), tasks AS
	(
		SELECT task.ActivityCode, task.AccountCode, TaskCode, ActionOn,  
			CASE CashModeCode WHEN 0 THEN 1 ELSE 0 END SupplyOrder, CAST(0 AS bit) IsAllocation, UnitCharge,
			CASE CashModeCode
					WHEN 0 THEN Quantity * -1
					WHEN 1 THEN Quantity 
				END Quantity, CashModeCode
		FROM Task.tbTask task
			JOIN Activity.tbMirror mirror ON task.AccountCode = mirror.AccountCode AND task.ActivityCode = mirror.ActivityCode
			JOIN Cash.tbCode cash_code ON task.CashCode = cash_code.CashCode
			JOIN Cash.tbCategory category ON cash_code.CategoryCode = category.CategoryCode
		WHERE TaskStatusCode = 0
	), quotes AS
	(
		SELECT * FROM requests
		UNION
		SELECT * FROM tasks
	), quotes_ordered AS
	(
			SELECT
				ActivityCode,
				ROW_NUMBER() OVER (PARTITION BY ActivityCode ORDER BY ActionOn, SupplyOrder) RowNumber,
				AccountCode, IsAllocation, TaskCode, CashModeCode, UnitCharge, ActionOn, Quantity
			FROM quotes
	), quotes_projection AS
	(
		SELECT
			ActivityCode, RowNumber, AccountCode, IsAllocation, TaskCode, CashModeCode, UnitCharge, ActionOn, Quantity,
			SUM(Quantity) OVER (PARTITION BY ActivityCode ORDER BY RowNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
		FROM quotes_ordered
	)
	SELECT CAST(ROW_NUMBER() OVER (ORDER BY quotes_projection.ActivityCode, RowNumber) AS int) AllocationId, quotes_projection.ActivityCode, activity.ActivityDescription, AccountCode, IsAllocation, 
		TaskCode, quotes_projection.CashModeCode, polarity.CashMode, quotes_projection.UnitCharge, ActionOn, Quantity, CAST(Balance AS decimal(18,2)) Balance
	FROM quotes_projection
		JOIN Activity.tbActivity activity ON quotes_projection.ActivityCode = activity.ActivityCode
		JOIN Cash.tbMode polarity ON quotes_projection.CashModeCode = polarity.CashModeCode;
