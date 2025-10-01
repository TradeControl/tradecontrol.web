
CREATE   PROCEDURE Object.proc_NextOperationNumber 
	(
	@ObjectCode nvarchar(50),
	@OperationNumber smallint = 10 output
	)
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT     TOP 1 OperationNumber
				  FROM         Object.tbOp
				  WHERE     (ObjectCode = @ObjectCode))
			BEGIN
			SELECT  @OperationNumber = MAX(OperationNumber) 
			FROM         Object.tbOp
			WHERE     (ObjectCode = @ObjectCode)
			SET @OperationNumber = @OperationNumber - (@OperationNumber % 10) + 10		
			END
		ELSE
			SET @OperationNumber = 10
		
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
