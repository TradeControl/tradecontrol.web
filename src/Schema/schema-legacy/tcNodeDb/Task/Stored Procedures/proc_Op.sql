CREATE PROCEDURE Task.proc_Op (@TaskCode nvarchar(20))
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS (SELECT     TaskCode
				   FROM         Task.tbOp
				   WHERE     (TaskCode = @TaskCode))
			BEGIN
			SELECT     Task.tbOp.*
				   FROM         Task.tbOp
				   WHERE     (TaskCode = @TaskCode)
			END
		ELSE
			BEGIN
			SELECT     Task.tbOp.*
				   FROM         Task.tbFlow INNER JOIN
										 Task.tbOp ON Task.tbFlow.ParentTaskCode = Task.tbOp.TaskCode
				   WHERE     ( Task.tbFlow.ChildTaskCode = @TaskCode)
			END
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
