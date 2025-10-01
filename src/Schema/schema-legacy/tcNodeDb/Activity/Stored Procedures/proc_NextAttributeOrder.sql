CREATE   PROCEDURE Activity.proc_NextAttributeOrder 
	(
	@ActivityCode nvarchar(50),
	@PrintOrder smallint = 10 output
	)
  AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT     TOP 1 PrintOrder
				  FROM         Activity.tbAttribute
				  WHERE     (ActivityCode = @ActivityCode))
			BEGIN
			SELECT  @PrintOrder = MAX(PrintOrder) 
			FROM         Activity.tbAttribute
			WHERE     (ActivityCode = @ActivityCode)
			SET @PrintOrder = @PrintOrder - (@PrintOrder % 10) + 10		
			END
		ELSE
			SET @PrintOrder = 10
		
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
