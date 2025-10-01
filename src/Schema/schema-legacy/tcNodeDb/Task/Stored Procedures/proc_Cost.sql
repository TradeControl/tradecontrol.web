CREATE   PROCEDURE Task.proc_Cost 
	(
	@ParentTaskCode nvarchar(20),
	@TotalCost decimal(18, 5) = 0 OUTPUT
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		WITH task_flow AS
		(
			SELECT parent_task.TaskCode, child.ParentTaskCode, child.ChildTaskCode, 
				CASE WHEN child.UsedOnQuantity <> 0 THEN CAST(parent_task.Quantity * child.UsedOnQuantity AS decimal(18, 4)) ELSE child_task.Quantity END AS Quantity, 
				1 AS Depth				
			FROM Task.tbFlow child 
				JOIN Task.tbTask parent_task ON child.ParentTaskCode = parent_task.TaskCode
				JOIN Task.tbTask child_task ON child.ChildTaskCode = child_task.TaskCode
			WHERE parent_task.TaskCode = @ParentTaskCode

			UNION ALL

			SELECT parent.TaskCode, child.ParentTaskCode, child.ChildTaskCode, 
				CASE WHEN child.UsedOnQuantity <> 0 THEN CAST(parent.Quantity * child.UsedOnQuantity AS decimal(18, 4)) ELSE child_task.Quantity END AS Quantity, 
				parent.Depth + 1 AS Depth
			FROM Task.tbFlow child 
				JOIN task_flow parent ON child.ParentTaskCode = parent.ChildTaskCode
				JOIN Task.tbTask child_task ON child.ChildTaskCode = child_task.TaskCode
		)
		, tasks AS
		(
			SELECT task_flow.TaskCode, task.Quantity,
				CASE category.CashModeCode 
					WHEN NULL THEN 0 
					WHEN 0 THEN task.UnitCharge * -1 
					ELSE task.UnitCharge 
				END AS UnitCharge
			FROM task_flow
				JOIN Task.tbTask task ON task_flow.ChildTaskCode = task.TaskCode
				LEFT OUTER JOIN Cash.tbCode cashcode ON cashcode.CashCode = task.CashCode 
				LEFT OUTER JOIN Cash.tbCategory category ON category.CategoryCode = cashcode.CategoryCode
		), task_costs AS
		(
			SELECT TaskCode, SUM(Quantity * UnitCharge) AS TotalCost
			FROM tasks
			GROUP BY TaskCode
		)
		SELECT @TotalCost = TotalCost
		FROM task_costs;		

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
