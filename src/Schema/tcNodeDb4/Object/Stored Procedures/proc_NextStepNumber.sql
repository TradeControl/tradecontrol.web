CREATE   PROCEDURE Object.proc_NextStepNumber 
	(
	@ObjectCode nvarchar(50),
	@StepNumber smallint = 10 output
	)
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT     TOP 1 StepNumber
				  FROM         Object.tbFlow
				  WHERE     (ParentCode = @ObjectCode))
			BEGIN
			SELECT  @StepNumber = MAX(StepNumber) 
			FROM         Object.tbFlow
			WHERE     (ParentCode = @ObjectCode)
			SET @StepNumber = @StepNumber - (@StepNumber % 10) + 10		
			END
		ELSE
			SET @StepNumber = 10
		
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
