
CREATE   PROCEDURE Activity.proc_WorkFlow
	(
	@ParentActivityCode nvarchar(50),
	@ActivityCode nvarchar(50)
	)
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		IF EXISTS(SELECT * FROM Activity.tbFlow WHERE (ParentCode = @ParentActivityCode))
			AND NOT EXISTS(SELECT COUNT(*) FROM Activity.tbFlow WHERE ChildCode = @ParentActivityCode GROUP BY ChildCode HAVING COUNT(*) > 1)			
		BEGIN
			SELECT     Activity.tbActivity.ActivityCode, Task.tbStatus.TaskStatus, ISNULL(Cash.tbCategory.CashModeCode, 2) AS CashModeCode, Activity.tbActivity.UnitOfMeasure, Activity.tbFlow.OffsetDays, Activity.tbFlow.UsedOnQuantity
			FROM         Activity.tbActivity INNER JOIN
								  Task.tbStatus ON Activity.tbActivity.TaskStatusCode = Task.tbStatus.TaskStatusCode INNER JOIN
								  Activity.tbFlow ON Activity.tbActivity.ActivityCode = Activity.tbFlow.ChildCode LEFT OUTER JOIN
								  Cash.tbCode ON Activity.tbActivity.CashCode = Cash.tbCode.CashCode LEFT OUTER JOIN
								  Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE     ( Activity.tbFlow.ParentCode = @ActivityCode)
			ORDER BY Activity.tbFlow.StepNumber	
		END
		ELSE
		BEGIN
			SELECT     Activity.tbActivity.ActivityCode, Task.tbStatus.TaskStatus, ISNULL(Cash.tbCategory.CashModeCode, 2) AS CashModeCode, Activity.tbActivity.UnitOfMeasure, Activity.tbFlow.OffsetDays, Activity.tbFlow.UsedOnQuantity
			FROM         Activity.tbActivity INNER JOIN
								  Task.tbStatus ON Activity.tbActivity.TaskStatusCode = Task.tbStatus.TaskStatusCode INNER JOIN
								  Activity.tbFlow ON Activity.tbActivity.ActivityCode = Activity.tbFlow.ParentCode LEFT OUTER JOIN
								  Cash.tbCode ON Activity.tbActivity.CashCode = Cash.tbCode.CashCode LEFT OUTER JOIN
								  Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE     ( Activity.tbFlow.ChildCode = @ActivityCode)
			ORDER BY Activity.tbFlow.StepNumber	
		END
			 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
