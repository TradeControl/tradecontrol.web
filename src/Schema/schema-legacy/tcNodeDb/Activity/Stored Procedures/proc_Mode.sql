
CREATE   PROCEDURE Activity.proc_Mode
	(
	@ActivityCode nvarchar(50)
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SELECT     Activity.tbActivity.ActivityCode, Activity.tbActivity.UnitOfMeasure, Task.tbStatus.TaskStatus, ISNULL(Cash.tbCategory.CashModeCode, 2) AS CashModeCode
		FROM         Activity.tbActivity INNER JOIN
							  Task.tbStatus ON Activity.tbActivity.TaskStatusCode = Task.tbStatus.TaskStatusCode LEFT OUTER JOIN
							  Cash.tbCode ON Activity.tbActivity.CashCode = Cash.tbCode.CashCode LEFT OUTER JOIN
							  Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
		WHERE     ( Activity.tbActivity.ActivityCode = @ActivityCode)
		 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
