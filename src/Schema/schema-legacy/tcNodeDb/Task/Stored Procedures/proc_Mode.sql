
CREATE   PROCEDURE Task.proc_Mode 
	(
	@TaskCode nvarchar(20)
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SELECT     Task.tbTask.AccountCode, Task.tbTask.ActivityCode, Task.tbTask.TaskStatusCode, Task.tbTask.ActionOn, Task.vwCashMode.CashModeCode
		FROM         Task.tbTask LEFT OUTER JOIN
							  Task.vwCashMode ON Task.tbTask.TaskCode = Task.vwCashMode.TaskCode
		WHERE     ( Task.tbTask.TaskCode = @TaskCode)
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
