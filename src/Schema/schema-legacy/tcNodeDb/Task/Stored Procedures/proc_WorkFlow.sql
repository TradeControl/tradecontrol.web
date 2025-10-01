
CREATE   PROCEDURE Task.proc_WorkFlow 
	(
	@TaskCode nvarchar(20)
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SELECT     Task.tbFlow.ParentTaskCode, Task.tbFlow.StepNumber, Task.tbTask.TaskCode, Task.tbTask.AccountCode, Task.tbTask.ActivityCode, Task.tbTask.TaskStatusCode, 
							  Task.tbTask.ActionOn, Task.vwCashMode.CashModeCode, Task.tbFlow.OffsetDays
		FROM         Task.tbTask INNER JOIN
							  Task.tbFlow ON Task.tbTask.TaskCode = Task.tbFlow.ChildTaskCode LEFT OUTER JOIN
							  Task.vwCashMode ON Task.tbTask.TaskCode = Task.vwCashMode.TaskCode
		WHERE     ( Task.tbFlow.ParentTaskCode = @TaskCode)
		ORDER BY Task.tbFlow.StepNumber, Task.tbFlow.ParentTaskCode
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
