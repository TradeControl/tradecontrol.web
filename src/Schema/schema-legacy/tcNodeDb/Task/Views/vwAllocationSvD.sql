CREATE VIEW Task.vwAllocationSvD
AS
	WITH allocs AS
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
		WHERE TaskStatusCode BETWEEN 1 AND 2	
	), tasks AS
	(
		SELECT task.ActivityCode, task.AccountCode, TaskCode, ActionOn, Quantity, UnitCharge, CashModeCode
		FROM Task.tbTask task
			JOIN Activity.tbMirror mirror ON task.AccountCode = mirror.AccountCode AND task.ActivityCode = mirror.ActivityCode
			JOIN Cash.tbCode cash_code ON task.CashCode = cash_code.CashCode
			JOIN Cash.tbCategory category ON cash_code.CategoryCode = category.CategoryCode
		WHERE TaskStatusCode BETWEEN 1 AND 2
	), invoice_quantities AS
	(
		SELECT tasks.TaskCode, SUM(COALESCE(invoice_quantity.Quantity, 0)) InvoiceQuantity
		FROM tasks
		OUTER APPLY 
			(
				SELECT CASE invoice.InvoiceTypeCode 
							WHEN 1 THEN delivery.Quantity * -1 
							WHEN 3 THEN delivery.Quantity * -1 
							ELSE delivery.Quantity 
						END Quantity
				FROM Invoice.tbTask delivery 
					JOIN Invoice.tbInvoice invoice ON delivery.InvoiceNumber = invoice.InvoiceNumber
				WHERE delivery.TaskCode = tasks.TaskCode
			) invoice_quantity
		GROUP BY tasks.TaskCode
	), deliveries AS
	(
		SELECT tasks.*, invoice_quantities.InvoiceQuantity
		FROM tasks JOIN invoice_quantities ON tasks.TaskCode = invoice_quantities.TaskCode 
	
	), order_book AS
	(
		SELECT ActivityCode, AccountCode, TaskCode, ActionOn, CASE CashModeCode WHEN 0 THEN 1 ELSE 0 END SupplyOrder, CAST(0 AS bit) IsAllocation, UnitCharge,
			CASE CashModeCode
				WHEN 0 THEN (Quantity - InvoiceQuantity) * -1
				WHEN 1 THEN (Quantity - InvoiceQuantity)
			END Quantity,
			CashModeCode
		FROM deliveries
	), SvD AS
	(
		SELECT * FROM allocs
		UNION
		SELECT * FROM order_book
	), SvD_ordered AS
	(
		SELECT
			ActivityCode,
			ROW_NUMBER() OVER (PARTITION BY ActivityCode ORDER BY ActionOn, SupplyOrder) RowNumber,
			AccountCode, IsAllocation, TaskCode, CashModeCode, UnitCharge, ActionOn, Quantity
		FROM SvD
	), SvD_projection AS
	(
		SELECT
			ActivityCode, RowNumber, AccountCode, IsAllocation, TaskCode, CashModeCode, UnitCharge, ActionOn, Quantity,
			SUM(Quantity) OVER (PARTITION BY ActivityCode ORDER BY RowNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
		FROM SvD_ordered
	), SvD_scheduled AS
	(
		SELECT ActivityCode, RowNumber, AccountCode, TaskCode, IsAllocation, CashModeCode, UnitCharge, ActionOn, Quantity, Balance,
			CASE WHEN 
				LEAD(Balance, 1, Balance) OVER (PARTITION BY ActivityCode ORDER BY RowNumber) < 0 
					AND LAG(Balance, 1, 0) OVER (PARTITION BY ActivityCode ORDER BY RowNumber) >= 0 
					AND Balance < 0
				THEN ActionOn
				ELSE NULL END ScheduleOn
		FROM SvD_projection
	)
	SELECT CAST(ROW_NUMBER() OVER (ORDER BY SvD_scheduled.ActivityCode, RowNumber) AS int) AllocationId, SvD_scheduled.ActivityCode, activity.ActivityDescription, AccountCode, IsAllocation, TaskCode, SvD_scheduled.CashModeCode, polarity.CashMode, SvD_scheduled.UnitCharge, ActionOn, Quantity, CAST(Balance AS decimal(18,2)) Balance,
		MAX(ScheduleOn) OVER (PARTITION BY SvD_scheduled.ActivityCode ORDER BY RowNumber) ScheduleOn			
	FROM SvD_scheduled
		JOIN Activity.tbActivity activity ON SvD_scheduled.ActivityCode = activity.ActivityCode
		JOIN Cash.tbMode polarity ON SvD_scheduled.CashModeCode = polarity.CashModeCode;
