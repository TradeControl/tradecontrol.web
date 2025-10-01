
CREATE   PROCEDURE Task.proc_DefaultDocType
	(
		@TaskCode nvarchar(20),
		@DocTypeCode smallint OUTPUT
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@CashModeCode smallint
			, @TaskStatusCode smallint

		IF EXISTS(SELECT     CashModeCode
				  FROM         Task.vwCashMode
				  WHERE     (TaskCode = @TaskCode))
			SELECT   @CashModeCode = CashModeCode
			FROM         Task.vwCashMode
			WHERE     (TaskCode = @TaskCode)			          
		ELSE
			SET @CashModeCode = 1

		SELECT  @TaskStatusCode =TaskStatusCode
		FROM         Task.tbTask
		WHERE     (TaskCode = @TaskCode)		
	
		IF @CashModeCode = 0
			SET @DocTypeCode = CASE @TaskStatusCode WHEN 0 THEN 2 ELSE 3 END								
		ELSE
			SET @DocTypeCode = CASE @TaskStatusCode WHEN 0 THEN 0 ELSE 1 END 
		 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
