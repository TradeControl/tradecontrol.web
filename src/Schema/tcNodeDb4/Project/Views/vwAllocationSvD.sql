CREATE VIEW Project.vwAllocationSvD
AS
	WITH allocs AS
	(
		SELECT mirror.ObjectCode, alloc.SubjectCode, alloc.ProjectCode, alloc.ActionOn, 
			CASE CashPolarityCode WHEN 0 THEN 1 ELSE 0 END SupplyOrder, CAST(1 AS bit) IsAllocation, UnitCharge,
			CASE CashPolarityCode 
				WHEN 0 THEN (alloc.QuantityOrdered - alloc.QuantityDelivered) * -1
				WHEN 1 THEN (alloc.QuantityOrdered - alloc.QuantityDelivered)
			END Quantity,
			CASE CashPolarityCode 
				WHEN 0 THEN 1
				WHEN 1 THEN 0
			END CashPolarityCode			
		FROM Project.tbAllocation alloc
			JOIN Object.tbMirror mirror ON alloc.SubjectCode = mirror.SubjectCode AND alloc.AllocationCode = mirror.AllocationCode
		WHERE ProjectStatusCode BETWEEN 1 AND 2	
	), Projects AS
	(
		SELECT Project.ObjectCode, Project.SubjectCode, ProjectCode, ActionOn, Quantity, UnitCharge, CashPolarityCode
		FROM Project.tbProject Project
			JOIN Object.tbMirror mirror ON Project.SubjectCode = mirror.SubjectCode AND Project.ObjectCode = mirror.ObjectCode
			JOIN Cash.tbCode cash_code ON Project.CashCode = cash_code.CashCode
			JOIN Cash.tbCategory category ON cash_code.CategoryCode = category.CategoryCode
		WHERE ProjectStatusCode BETWEEN 1 AND 2
	), invoice_quantities AS
	(
		SELECT Projects.ProjectCode, SUM(COALESCE(invoice_quantity.Quantity, 0)) InvoiceQuantity
		FROM Projects
		OUTER APPLY 
			(
				SELECT CASE invoice.InvoiceTypeCode 
							WHEN 1 THEN delivery.Quantity * -1 
							WHEN 3 THEN delivery.Quantity * -1 
							ELSE delivery.Quantity 
						END Quantity
				FROM Invoice.tbProject delivery 
					JOIN Invoice.tbInvoice invoice ON delivery.InvoiceNumber = invoice.InvoiceNumber
				WHERE delivery.ProjectCode = Projects.ProjectCode
			) invoice_quantity
		GROUP BY Projects.ProjectCode
	), deliveries AS
	(
		SELECT Projects.*, invoice_quantities.InvoiceQuantity
		FROM Projects JOIN invoice_quantities ON Projects.ProjectCode = invoice_quantities.ProjectCode 
	
	), order_book AS
	(
		SELECT ObjectCode, SubjectCode, ProjectCode, ActionOn, CASE CashPolarityCode WHEN 0 THEN 1 ELSE 0 END SupplyOrder, CAST(0 AS bit) IsAllocation, UnitCharge,
			CASE CashPolarityCode
				WHEN 0 THEN (Quantity - InvoiceQuantity) * -1
				WHEN 1 THEN (Quantity - InvoiceQuantity)
			END Quantity,
			CashPolarityCode
		FROM deliveries
	), SvD AS
	(
		SELECT * FROM allocs
		UNION
		SELECT * FROM order_book
	), SvD_ordered AS
	(
		SELECT
			ObjectCode,
			ROW_NUMBER() OVER (PARTITION BY ObjectCode ORDER BY ActionOn, SupplyOrder) RowNumber,
			SubjectCode, IsAllocation, ProjectCode, CashPolarityCode, UnitCharge, ActionOn, Quantity
		FROM SvD
	), SvD_projection AS
	(
		SELECT
			ObjectCode, RowNumber, SubjectCode, IsAllocation, ProjectCode, CashPolarityCode, UnitCharge, ActionOn, Quantity,
			SUM(Quantity) OVER (PARTITION BY ObjectCode ORDER BY RowNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
		FROM SvD_ordered
	), SvD_scheduled AS
	(
		SELECT ObjectCode, RowNumber, SubjectCode, ProjectCode, IsAllocation, CashPolarityCode, UnitCharge, ActionOn, Quantity, Balance,
			CASE WHEN 
				LEAD(Balance, 1, Balance) OVER (PARTITION BY ObjectCode ORDER BY RowNumber) < 0 
					AND LAG(Balance, 1, 0) OVER (PARTITION BY ObjectCode ORDER BY RowNumber) >= 0 
					AND Balance < 0
				THEN ActionOn
				ELSE NULL END ScheduleOn
		FROM SvD_projection
	)
	SELECT CAST(ROW_NUMBER() OVER (ORDER BY SvD_scheduled.ObjectCode, RowNumber) AS int) AllocationId, SvD_scheduled.ObjectCode, Object.ObjectDescription, SubjectCode, IsAllocation, ProjectCode, SvD_scheduled.CashPolarityCode, polarity.CashPolarity, SvD_scheduled.UnitCharge, ActionOn, Quantity, CAST(Balance AS decimal(18,2)) Balance,
		MAX(ScheduleOn) OVER (PARTITION BY SvD_scheduled.ObjectCode ORDER BY RowNumber) ScheduleOn			
	FROM SvD_scheduled
		JOIN Object.tbObject Object ON SvD_scheduled.ObjectCode = Object.ObjectCode
		JOIN Cash.tbPolarity polarity ON SvD_scheduled.CashPolarityCode = polarity.CashPolarityCode;
