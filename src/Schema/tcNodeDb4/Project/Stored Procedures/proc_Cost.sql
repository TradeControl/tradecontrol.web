CREATE   PROCEDURE Project.proc_Cost 
	(
	@ParentProjectCode nvarchar(20),
	@TotalCost decimal(18, 5) = 0 OUTPUT
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		WITH Project_flow AS
		(
			SELECT parent_Project.ProjectCode, child.ParentProjectCode, child.ChildProjectCode, 
				CASE WHEN child.UsedOnQuantity <> 0 THEN CAST(parent_Project.Quantity * child.UsedOnQuantity AS decimal(18, 4)) ELSE child_Project.Quantity END AS Quantity, 
				1 AS Depth				
			FROM Project.tbFlow child 
				JOIN Project.tbProject parent_Project ON child.ParentProjectCode = parent_Project.ProjectCode
				JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode
			WHERE parent_Project.ProjectCode = @ParentProjectCode

			UNION ALL

			SELECT parent.ProjectCode, child.ParentProjectCode, child.ChildProjectCode, 
				CASE WHEN child.UsedOnQuantity <> 0 THEN CAST(parent.Quantity * child.UsedOnQuantity AS decimal(18, 4)) ELSE child_Project.Quantity END AS Quantity, 
				parent.Depth + 1 AS Depth
			FROM Project.tbFlow child 
				JOIN Project_flow parent ON child.ParentProjectCode = parent.ChildProjectCode
				JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode
		)
		, Projects AS
		(
			SELECT Project_flow.ProjectCode, Project.Quantity,
				CASE category.CashPolarityCode 
					WHEN NULL THEN 0 
					WHEN 0 THEN Project.UnitCharge * -1 
					ELSE Project.UnitCharge 
				END AS UnitCharge
			FROM Project_flow
				JOIN Project.tbProject Project ON Project_flow.ChildProjectCode = Project.ProjectCode
				LEFT OUTER JOIN Cash.tbCode cashcode ON cashcode.CashCode = Project.CashCode 
				LEFT OUTER JOIN Cash.tbCategory category ON category.CategoryCode = cashcode.CategoryCode
		), Project_costs AS
		(
			SELECT ProjectCode, SUM(Quantity * UnitCharge) AS TotalCost
			FROM Projects
			GROUP BY ProjectCode
		)
		SELECT @TotalCost = TotalCost
		FROM Project_costs;		

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
