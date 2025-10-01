CREATE   PROCEDURE Activity.proc_WorkFlowMultiLevel
	(
	@ActivityCode nvarchar(50)
	)
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		IF EXISTS(SELECT * FROM Activity.tbFlow WHERE (ParentCode = @ActivityCode))
		BEGIN
			WITH workflow AS
			(
				SELECT  parent_flow.ParentCode, parent_flow.ChildCode, parent_flow.OffsetDays, parent_flow.UsedOnQuantity, 1 AS Depth
				FROM Activity.tbFlow parent_flow
				WHERE (parent_flow.ParentCode = @ActivityCode)

				UNION ALL

				SELECT  child_flow.ParentCode, child_flow.ChildCode, child_flow.OffsetDays, child_flow.UsedOnQuantity, workflow.Depth + 1 AS Depth
				FROM workflow 
					JOIN Activity.tbFlow child_flow ON workflow.ChildCode = child_flow.ParentCode
			)
			SELECT workflow.ParentCode, workflow.ChildCode,
						task_status.TaskStatus, ISNULL(cash_category.CashModeCode, 2) AS CashModeCode,
						activity.UnitOfMeasure, workflow.OffsetDays, workflow.UsedOnQuantity, Depth
			FROM workflow
					JOIN Activity.tbActivity activity ON workflow.ChildCode = activity.ActivityCode
					JOIN Task.tbStatus task_status ON activity.TaskStatusCode = task_status.TaskStatusCode 
					LEFT OUTER JOIN Cash.tbCode cash_code ON activity.CashCode = cash_code.CashCode 
					LEFT OUTER JOIN Cash.tbCategory cash_category ON cash_code.CategoryCode = cash_category.CategoryCode
			ORDER BY Depth, ParentCode, ChildCode;
		END
		ELSE
		BEGIN
			WITH workflow AS
			(
				SELECT  child_flow.ParentCode, child_flow.ChildCode, child_flow.OffsetDays, child_flow.UsedOnQuantity, -1 AS Depth
				FROM Activity.tbFlow child_flow
				WHERE (child_flow.ChildCode = @ActivityCode)

				UNION ALL

				SELECT  parent_flow.ParentCode, parent_flow.ChildCode, parent_flow.OffsetDays, parent_flow.UsedOnQuantity, workflow.Depth - 1 AS Depth
				FROM workflow 
					JOIN Activity.tbFlow parent_flow ON workflow.ParentCode = parent_flow.ChildCode
			)
			SELECT workflow.ChildCode AS ParentCode, workflow.ParentCode AS ChildCode, 
						task_status.TaskStatus, ISNULL(cash_category.CashModeCode, 2) AS CashModeCode,
						activity.UnitOfMeasure, workflow.OffsetDays, workflow.UsedOnQuantity, Depth
			FROM workflow
					JOIN Activity.tbActivity activity ON workflow.ParentCode = activity.ActivityCode
					JOIN Task.tbStatus task_status ON activity.TaskStatusCode = task_status.TaskStatusCode 
					LEFT OUTER JOIN Cash.tbCode cash_code ON activity.CashCode = cash_code.CashCode 
					LEFT OUTER JOIN Cash.tbCategory cash_category ON cash_code.CategoryCode = cash_category.CategoryCode
			ORDER BY Depth DESC, ParentCode, ChildCode;		
		END
			 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
