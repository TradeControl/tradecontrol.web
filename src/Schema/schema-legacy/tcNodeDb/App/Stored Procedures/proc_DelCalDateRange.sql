
CREATE   PROCEDURE App.proc_DelCalDateRange
	(
		@CalendarCode nvarchar(10),
		@FromDate datetime,
		@ToDate datetime
	)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DELETE FROM App.tbCalendarHoliday
			WHERE UnavailableOn >= @FromDate
				AND UnavailableOn <= @ToDate
				AND CalendarCode = @CalendarCode
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
