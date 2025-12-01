
CREATE   PROCEDURE Project.proc_IsProject 
	(
	@ProjectCode nvarchar(20),
	@IsProject bit = 0 output
	)
  AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT     TOP 1 Attribute
				  FROM         Project.tbAttribute
				  WHERE     (ProjectCode = @ProjectCode))
			SET @IsProject = 1
		ELSE IF EXISTS (SELECT     TOP 1 ParentProjectCode, StepNumber
						FROM         Project.tbFlow
						WHERE     (ParentProjectCode = @ProjectCode))
			SET @IsProject = 1
		ELSE
			SET @IsProject = 0
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH	
