CREATE   VIEW Project.vwCostSetProjects
AS
	WITH Project_flow AS
	(
		SELECT child.ParentProjectCode, child.ChildProjectCode
		FROM Project.tbFlow child 
			JOIN Project.vwCostSet cost_set ON child.ParentProjectCode = cost_set.ProjectCode
			JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode

		UNION ALL

		SELECT child.ParentProjectCode, child.ChildProjectCode
		FROM Project.tbFlow child 
			JOIN Project_flow parent ON child.ParentProjectCode = parent.ChildProjectCode
			JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode
	)
	SELECT ProjectCode FROM Project.vwCostSet
	UNION
	SELECT quote.ProjectCode
	FROM Project.tbProject quote 
		JOIN Project_flow ON Project_flow.ChildProjectCode = quote.ProjectCode
		JOIN Cash.tbCode cash_code ON quote.CashCode = cash_code.CashCode
	WHERE quote.ProjectStatusCode = 0;
