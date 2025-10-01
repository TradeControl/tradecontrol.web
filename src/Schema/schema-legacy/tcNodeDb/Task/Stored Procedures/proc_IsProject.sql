
CREATE   PROCEDURE Task.proc_IsProject 
	(
	@TaskCode nvarchar(20),
	@IsProject bit = 0 output
	)
  AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT     TOP 1 Attribute
				  FROM         Task.tbAttribute
				  WHERE     (TaskCode = @TaskCode))
			SET @IsProject = 1
		ELSE IF EXISTS (SELECT     TOP 1 ParentTaskCode, StepNumber
						FROM         Task.tbFlow
						WHERE     (ParentTaskCode = @TaskCode))
			SET @IsProject = 1
		ELSE
			SET @IsProject = 0
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH	
