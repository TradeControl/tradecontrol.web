

CREATE   PROCEDURE App.proc_YearPeriods
	(
	@YearNumber int
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SELECT     App.tbYear.Description, App.tbMonth.MonthName
					FROM         App.tbYearPeriod INNER JOIN
										App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
										App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
					WHERE     ( App.tbYearPeriod.YearNumber = @YearNumber)
					ORDER BY App.tbYearPeriod.YearNumber, App.tbYearPeriod.StartOn
    END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
