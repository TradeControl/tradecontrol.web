CREATE   PROCEDURE Object.proc_WorkFlowMultiLevel
	(
	@ObjectCode nvarchar(50)
	)
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		IF EXISTS(SELECT * FROM Object.tbFlow WHERE (ParentCode = @ObjectCode))
		BEGIN
			WITH workflow AS
			(
				SELECT  parent_flow.ParentCode, parent_flow.ChildCode, parent_flow.OffsetDays, parent_flow.UsedOnQuantity, 1 AS Depth
				FROM Object.tbFlow parent_flow
				WHERE (parent_flow.ParentCode = @ObjectCode)

				UNION ALL

				SELECT  child_flow.ParentCode, child_flow.ChildCode, child_flow.OffsetDays, child_flow.UsedOnQuantity, workflow.Depth + 1 AS Depth
				FROM workflow 
					JOIN Object.tbFlow child_flow ON workflow.ChildCode = child_flow.ParentCode
			)
			SELECT workflow.ParentCode, workflow.ChildCode,
						Project_status.ProjectStatus, ISNULL(cash_category.CashPolarityCode, 2) AS CashPolarityCode,
						Object.UnitOfMeasure, workflow.OffsetDays, workflow.UsedOnQuantity, Depth
			FROM workflow
					JOIN Object.tbObject Object ON workflow.ChildCode = Object.ObjectCode
					JOIN Project.tbStatus Project_status ON Object.ProjectStatusCode = Project_status.ProjectStatusCode 
					LEFT OUTER JOIN Cash.tbCode cash_code ON Object.CashCode = cash_code.CashCode 
					LEFT OUTER JOIN Cash.tbCategory cash_category ON cash_code.CategoryCode = cash_category.CategoryCode
			ORDER BY Depth, ParentCode, ChildCode;
		END
		ELSE
		BEGIN
			WITH workflow AS
			(
				SELECT  child_flow.ParentCode, child_flow.ChildCode, child_flow.OffsetDays, child_flow.UsedOnQuantity, -1 AS Depth
				FROM Object.tbFlow child_flow
				WHERE (child_flow.ChildCode = @ObjectCode)

				UNION ALL

				SELECT  parent_flow.ParentCode, parent_flow.ChildCode, parent_flow.OffsetDays, parent_flow.UsedOnQuantity, workflow.Depth - 1 AS Depth
				FROM workflow 
					JOIN Object.tbFlow parent_flow ON workflow.ParentCode = parent_flow.ChildCode
			)
			SELECT workflow.ChildCode AS ParentCode, workflow.ParentCode AS ChildCode, 
						Project_status.ProjectStatus, ISNULL(cash_category.CashPolarityCode, 2) AS CashPolarityCode,
						Object.UnitOfMeasure, workflow.OffsetDays, workflow.UsedOnQuantity, Depth
			FROM workflow
					JOIN Object.tbObject Object ON workflow.ParentCode = Object.ObjectCode
					JOIN Project.tbStatus Project_status ON Object.ProjectStatusCode = Project_status.ProjectStatusCode 
					LEFT OUTER JOIN Cash.tbCode cash_code ON Object.CashCode = cash_code.CashCode 
					LEFT OUTER JOIN Cash.tbCategory cash_category ON cash_code.CategoryCode = cash_category.CategoryCode
			ORDER BY Depth DESC, ParentCode, ChildCode;		
		END
			 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
