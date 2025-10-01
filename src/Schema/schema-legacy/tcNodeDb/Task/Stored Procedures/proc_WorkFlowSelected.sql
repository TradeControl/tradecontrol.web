
CREATE   PROCEDURE Task.proc_WorkFlowSelected 
	(
	@ChildTaskCode nvarchar(20),
	@ParentTaskCode nvarchar(20) = NULL
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF NOT (@ParentTaskCode IS NULL)
			SELECT        Task.tbTask.AccountCode, Task.tbTask.ActivityCode, Task.tbTask.TaskStatusCode, Task.tbTask.ActionOn, Task.vwCashMode.CashModeCode, Task.tbFlow.OffsetDays
			FROM            Task.tbTask INNER JOIN
									 Task.tbFlow ON Task.tbTask.TaskCode = Task.tbFlow.ChildTaskCode LEFT OUTER JOIN
									 Task.vwCashMode ON Task.tbTask.TaskCode = Task.vwCashMode.TaskCode
			WHERE        (Task.tbFlow.ParentTaskCode = @ParentTaskCode) AND (Task.tbFlow.ChildTaskCode = @ChildTaskCode)
		ELSE
			SELECT        Task.tbTask.AccountCode, Task.tbTask.ActivityCode, Task.tbTask.TaskStatusCode, Task.tbTask.ActionOn, Task.vwCashMode.CashModeCode, 0 AS OffsetDays
			FROM            Task.tbTask LEFT OUTER JOIN
									 Task.vwCashMode ON Task.tbTask.TaskCode = Task.vwCashMode.TaskCode
			WHERE        (Task.tbTask.TaskCode = @ChildTaskCode)
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
