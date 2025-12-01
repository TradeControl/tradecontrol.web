CREATE VIEW [Project].[vwProfit] 
AS
	WITH orders AS
	(
		SELECT        Project.ProjectCode, Project.Quantity, Project.UnitCharge,
									 (SELECT        TOP (1) StartOn
									   FROM            App.tbYearPeriod AS p
									   WHERE        (StartOn <= Project.ActionOn)
									   ORDER BY StartOn DESC) AS StartOn
		FROM            Project.tbFlow RIGHT OUTER JOIN
								 Project.tbProject ON Project.tbFlow.ParentProjectCode = Project.tbProject.ProjectCode AND Project.tbFlow.ParentProjectCode = Project.tbProject.ProjectCode AND Project.tbFlow.ParentProjectCode = Project.tbProject.ProjectCode RIGHT OUTER JOIN
								 Project.tbProject AS Project INNER JOIN
								 Cash.tbCode AS cashcode ON Project.CashCode = cashcode.CashCode INNER JOIN
								 Cash.tbCategory AS category ON category.CategoryCode = cashcode.CategoryCode ON Project.tbFlow.ChildProjectCode = Project.ProjectCode AND Project.tbFlow.ChildProjectCode = Project.ProjectCode
		WHERE        (category.CashPolarityCode = 1) AND (Project.ProjectStatusCode BETWEEN 1 AND 3) AND 
			(Project.ActionOn >= (SELECT        MIN(StartOn)
											FROM            App.tbYearPeriod p JOIN
																	  App.tbYear y ON p.YearNumber = y.YearNumber
											WHERE        y.CashStatusCode < 3)) AND	
			((Project.tbFlow.ParentProjectCode IS NULL) OR (Project.tbProject.CashCode IS NULL))

	), invoices AS
	(
		SELECT Projects.ProjectCode, ISNULL(invoice.InvoiceValue, 0) AS InvoiceValue, ISNULL(invoice.InvoicePaid, 0) AS InvoicePaid 
		FROM Project.tbProject Projects LEFT OUTER JOIN 
			(
				SELECT Invoice.tbProject.ProjectCode, 
					SUM(CASE CashPolarityCode WHEN 0 THEN Invoice.tbProject.InvoiceValue * -1 ELSE Invoice.tbProject.InvoiceValue END) AS InvoiceValue, 
					CASE InvoiceStatusCode WHEN 3 THEN 
						SUM(CASE CashPolarityCode WHEN 0 THEN Invoice.tbProject.InvoiceValue * -1 ELSE Invoice.tbProject.InvoiceValue END)
					ELSE 0
					END AS InvoicePaid
				FROM Invoice.tbProject 
					INNER JOIN Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
					INNER JOIN Invoice.tbType ON Invoice.tbType.InvoiceTypeCode = Invoice.tbInvoice.InvoiceTypeCode 
				GROUP BY Invoice.tbProject.ProjectCode, Invoice.tbInvoice.InvoiceStatusCode
			) invoice 
		ON Projects.ProjectCode = invoice.ProjectCode
	), Project_flow AS
	(
		SELECT orders.ProjectCode, child.ParentProjectCode, child.ChildProjectCode, 
			CASE WHEN child.UsedOnQuantity <> 0 THEN CAST(orders.Quantity * child.UsedOnQuantity AS decimal(18, 4)) ELSE Project.Quantity END AS Quantity
		FROM Project.tbFlow child 
			JOIN orders ON child.ParentProjectCode = orders.ProjectCode
			JOIN Project.tbProject Project ON child.ChildProjectCode = Project.ProjectCode

		UNION ALL

		SELECT parent.ProjectCode, child.ParentProjectCode, child.ChildProjectCode, 
			CASE WHEN child.UsedOnQuantity <> 0 THEN CAST(parent.Quantity * child.UsedOnQuantity AS decimal(18, 4)) ELSE Project.Quantity END AS Quantity
		FROM Project.tbFlow child 
			JOIN Project_flow parent ON child.ParentProjectCode = parent.ChildProjectCode
			JOIN Project.tbProject Project ON child.ChildProjectCode = Project.ProjectCode

	), Projects AS
	(
		SELECT Project_flow.ProjectCode, Project.Quantity,
				CASE category.CashPolarityCode 
					WHEN NULL THEN 0 
					WHEN 0 THEN Project.UnitCharge * -1 
					ELSE Project.UnitCharge 
				END AS UnitCharge,
				invoices.InvoiceValue, invoices.InvoicePaid
		FROM Project_flow
			JOIN Project.tbProject Project ON Project_flow.ChildProjectCode = Project.ProjectCode
			JOIN invoices ON invoices.ProjectCode = Project.ProjectCode
			LEFT OUTER JOIN Cash.tbCode cashcode ON cashcode.CashCode = Project.CashCode 
			LEFT OUTER JOIN Cash.tbCategory category ON category.CategoryCode = cashcode.CategoryCode
	)
	, Project_costs AS
	(
		SELECT ProjectCode, ROUND(SUM(Quantity * UnitCharge), 2) AS TotalCost, 
				ROUND(SUM(InvoiceValue), 2) AS InvoicedCost, ROUND(SUM(InvoicePaid), 2) AS InvoicedCostPaid
		FROM Projects
		GROUP BY ProjectCode
		UNION
		SELECT ProjectCode, 0 AS TotalCost, 0 AS InvoicedCost, 0 AS InvoicedCostPaid
		FROM orders LEFT OUTER JOIN Project.tbFlow AS flow ON orders.ProjectCode = flow.ParentProjectCode
		WHERE (flow.ParentProjectCode IS NULL)
	), profits AS
	(
		SELECT orders.StartOn, Project.SubjectCode, orders.ProjectCode, 
			yearperiod.YearNumber, yr.Description, 
			CONCAT(mn.MonthName, ' ', YEAR(yearperiod.StartOn)) AS Period,
			Project.ObjectCode, cashcode.CashCode, Project.ProjectTitle, Subject.SubjectName, cashcode.CashDescription,
			Projectstatus.ProjectStatus, Project.ProjectStatusCode, Project.TotalCharge, invoices.InvoiceValue AS InvoicedCharge,
			invoices.InvoicePaid AS InvoicedChargePaid,
			Project_costs.TotalCost, Project_costs.InvoicedCost, Project_costs.InvoicedCostPaid,
			Project.TotalCharge + Project_costs.TotalCost AS Profit,
			Project.TotalCharge - invoices.InvoiceValue AS UninvoicedCharge,
			invoices.InvoiceValue - invoices.InvoicePaid AS UnpaidCharge,
			Project_costs.TotalCost - Project_costs.InvoicedCost AS UninvoicedCost,
			Project_costs.InvoicedCost - Project_costs.InvoicedCostPaid AS UnpaidCost,
			Project.ActionOn, Project.ActionedOn, Project.PaymentOn
		FROM orders 
			JOIN Project.tbProject Project ON Project.ProjectCode = orders.ProjectCode
			JOIN invoices ON invoices.ProjectCode = Project.ProjectCode
			JOIN Project_costs ON orders.ProjectCode = Project_costs.ProjectCode	
			JOIN Cash.tbCode cashcode ON Project.CashCode = cashcode.CashCode
			JOIN Project.tbStatus Projectstatus ON Projectstatus.ProjectStatusCode = Project.ProjectStatusCode
			JOIN Subject.tbSubject Subject ON Subject.SubjectCode = Project.SubjectCode
			JOIN App.tbYearPeriod yearperiod ON yearperiod.StartOn = orders.StartOn
			JOIN App.tbYear yr ON yr.YearNumber = yearperiod.YearNumber
			JOIN App.tbMonth mn ON mn.MonthNumber = yearperiod.MonthNumber
		)
		SELECT StartOn, SubjectCode, ProjectCode, YearNumber, [Description], [Period], ObjectCode, CashCode,
			ProjectTitle, SubjectName, CashDescription, ProjectStatus, ProjectStatusCode, CAST(TotalCharge as float) TotalCharge, CAST(InvoicedCharge as float) InvoicedCharge, CAST(InvoicedChargePaid as float) InvoicedChargePaid,
			CAST(TotalCost AS float) TotalCost, CAST(InvoicedCost as float) InvoicedCost, CAST(InvoicedCostPaid as float) InvoicedCostPaid, CAST(Profit AS float) Profit,
			CAST(UninvoicedCharge AS float) UninvoicedCharge, CAST(UnpaidCharge AS float) UnpaidCharge,
			CAST(UninvoicedCost AS float) UninvoicedCost, CAST(UnpaidCost AS float) UnpaidCost,
			ActionOn, ActionedOn, PaymentOn
		FROM profits;
