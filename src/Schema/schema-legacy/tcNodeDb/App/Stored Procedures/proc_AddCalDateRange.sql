
CREATE   PROCEDURE App.proc_AddCalDateRange
	(
		@CalendarCode nvarchar(10),
		@FromDate datetime,
		@ToDate datetime
	)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @UnavailableDate datetime

		SELECT @UnavailableDate = @FromDate
	
		BEGIN TRANSACTION

		WHILE @UnavailableDate <= @ToDate
		BEGIN
			INSERT INTO App.tbCalendarHoliday (CalendarCode, UnavailableOn)
			VALUES (@CalendarCode, @UnavailableDate)
			SELECT @UnavailableDate = DateAdd(d, 1, @UnavailableDate)
		END

		COMMIT TRANSACTION

		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
