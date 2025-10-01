CREATE   VIEW Task.vwCostSetTasks
AS
	WITH task_flow AS
	(
		SELECT child.ParentTaskCode, child.ChildTaskCode
		FROM Task.tbFlow child 
			JOIN Task.vwCostSet cost_set ON child.ParentTaskCode = cost_set.TaskCode
			JOIN Task.tbTask child_task ON child.ChildTaskCode = child_task.TaskCode

		UNION ALL

		SELECT child.ParentTaskCode, child.ChildTaskCode
		FROM Task.tbFlow child 
			JOIN task_flow parent ON child.ParentTaskCode = parent.ChildTaskCode
			JOIN Task.tbTask child_task ON child.ChildTaskCode = child_task.TaskCode
	)
	SELECT TaskCode FROM Task.vwCostSet
	UNION
	SELECT quote.TaskCode
	FROM Task.tbTask quote 
		JOIN task_flow ON task_flow.ChildTaskCode = quote.TaskCode
		JOIN Cash.tbCode cash_code ON quote.CashCode = cash_code.CashCode
	WHERE quote.TaskStatusCode = 0;
