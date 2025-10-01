CREATE VIEW Project.vwNetworkQuotations
AS
	WITH requests AS
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
		WHERE ProjectStatusCode = 0	
	), Projects AS
	(
		SELECT Project.ObjectCode, Project.SubjectCode, ProjectCode, ActionOn,  
			CASE CashPolarityCode WHEN 0 THEN 1 ELSE 0 END SupplyOrder, CAST(0 AS bit) IsAllocation, UnitCharge,
			CASE CashPolarityCode
					WHEN 0 THEN Quantity * -1
					WHEN 1 THEN Quantity 
				END Quantity, CashPolarityCode
		FROM Project.tbProject Project
			JOIN Object.tbMirror mirror ON Project.SubjectCode = mirror.SubjectCode AND Project.ObjectCode = mirror.ObjectCode
			JOIN Cash.tbCode cash_code ON Project.CashCode = cash_code.CashCode
			JOIN Cash.tbCategory category ON cash_code.CategoryCode = category.CategoryCode
		WHERE ProjectStatusCode = 0
	), quotes AS
	(
		SELECT * FROM requests
		UNION
		SELECT * FROM Projects
	), quotes_ordered AS
	(
			SELECT
				ObjectCode,
				ROW_NUMBER() OVER (PARTITION BY ObjectCode ORDER BY ActionOn, SupplyOrder) RowNumber,
				SubjectCode, IsAllocation, ProjectCode, CashPolarityCode, UnitCharge, ActionOn, Quantity
			FROM quotes
	), quotes_projection AS
	(
		SELECT
			ObjectCode, RowNumber, SubjectCode, IsAllocation, ProjectCode, CashPolarityCode, UnitCharge, ActionOn, Quantity,
			SUM(Quantity) OVER (PARTITION BY ObjectCode ORDER BY RowNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
		FROM quotes_ordered
	)
	SELECT CAST(ROW_NUMBER() OVER (ORDER BY quotes_projection.ObjectCode, RowNumber) AS int) AllocationId, quotes_projection.ObjectCode, Object.ObjectDescription, SubjectCode, IsAllocation, 
		ProjectCode, quotes_projection.CashPolarityCode, polarity.CashPolarity, quotes_projection.UnitCharge, ActionOn, Quantity, CAST(Balance AS decimal(18,2)) Balance
	FROM quotes_projection
		JOIN Object.tbObject Object ON quotes_projection.ObjectCode = Object.ObjectCode
		JOIN Cash.tbPolarity polarity ON quotes_projection.CashPolarityCode = polarity.CashPolarityCode;
