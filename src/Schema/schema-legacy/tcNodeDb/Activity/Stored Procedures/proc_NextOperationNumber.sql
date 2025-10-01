
CREATE   PROCEDURE Activity.proc_NextOperationNumber 
	(
	@ActivityCode nvarchar(50),
	@OperationNumber smallint = 10 output
	)
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT     TOP 1 OperationNumber
				  FROM         Activity.tbOp
				  WHERE     (ActivityCode = @ActivityCode))
			BEGIN
			SELECT  @OperationNumber = MAX(OperationNumber) 
			FROM         Activity.tbOp
			WHERE     (ActivityCode = @ActivityCode)
			SET @OperationNumber = @OperationNumber - (@OperationNumber % 10) + 10		
			END
		ELSE
			SET @OperationNumber = 10
		
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
