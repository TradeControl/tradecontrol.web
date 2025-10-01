CREATE   PROCEDURE Activity.proc_NextStepNumber 
	(
	@ActivityCode nvarchar(50),
	@StepNumber smallint = 10 output
	)
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT     TOP 1 StepNumber
				  FROM         Activity.tbFlow
				  WHERE     (ParentCode = @ActivityCode))
			BEGIN
			SELECT  @StepNumber = MAX(StepNumber) 
			FROM         Activity.tbFlow
			WHERE     (ParentCode = @ActivityCode)
			SET @StepNumber = @StepNumber - (@StepNumber % 10) + 10		
			END
		ELSE
			SET @StepNumber = 10
		
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
