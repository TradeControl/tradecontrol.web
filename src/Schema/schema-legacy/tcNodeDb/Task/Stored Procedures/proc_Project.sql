CREATE   PROCEDURE Task.proc_Project 
	(
	@TaskCode nvarchar(20),
	@ParentTaskCode nvarchar(20) output
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SET @ParentTaskCode = @TaskCode
		WHILE EXISTS(SELECT     ParentTaskCode
					 FROM         Task.tbFlow
					 WHERE     (ChildTaskCode = @ParentTaskCode))
			SELECT @ParentTaskCode = ParentTaskCode
					 FROM         Task.tbFlow
					 WHERE     (ChildTaskCode = @ParentTaskCode)
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
