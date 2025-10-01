
 CREATE   PROCEDURE Task.proc_Parent 
	(
	@TaskCode nvarchar(20),
	@ParentTaskCode nvarchar(20) output
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SET @ParentTaskCode = @TaskCode
		IF EXISTS(SELECT     ParentTaskCode
					 FROM         Task.tbFlow
					 WHERE     (ChildTaskCode = @ParentTaskCode))
			SELECT @ParentTaskCode = ParentTaskCode
					 FROM         Task.tbFlow
					 WHERE     (ChildTaskCode = @ParentTaskCode)
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH


