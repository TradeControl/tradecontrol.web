
CREATE   PROCEDURE App.proc_PeriodGetYear
	(
	@StartOn DATETIME,
	@YearNumber INTEGER OUTPUT
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SELECT @YearNumber = YearNumber
		FROM            App.tbYearPeriod
		WHERE        (StartOn = @StartOn)
	
		IF @YearNumber IS NULL
			SELECT @YearNumber = YearNumber FROM App.fnActivePeriod()
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH	 
