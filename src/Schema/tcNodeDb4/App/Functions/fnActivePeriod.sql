CREATE   FUNCTION App.fnActivePeriod	()
RETURNS @tbSystemYearPeriod TABLE (YearNumber smallint, StartOn datetime, EndOn datetime, MonthName nvarchar(10), Description nvarchar(10), MonthNumber smallint) 
   AS
	BEGIN
	DECLARE @StartOn datetime
	DECLARE @EndOn datetime
	
	IF EXISTS (	SELECT     StartOn	FROM App.tbYearPeriod WHERE (CashStatusCode < 2))
		BEGIN
		SELECT @StartOn = MIN(StartOn)
		FROM         App.tbYearPeriod
		WHERE     (CashStatusCode < 2)
		
		IF EXISTS (SELECT StartOn FROM App.tbYearPeriod WHERE StartOn > @StartOn)
			SELECT TOP 1 @EndOn = StartOn FROM App.tbYearPeriod WHERE StartOn > @StartOn order by StartOn
		ELSE
			SET @EndOn = DATEADD(m, 1, @StartOn)
			
		INSERT INTO @tbSystemYearPeriod (YearNumber, StartOn, EndOn, MonthName, Description, MonthNumber)
		SELECT     App.tbYearPeriod.YearNumber, App.tbYearPeriod.StartOn, @EndOn, App.tbMonth.MonthName, App.tbYear.Description, App.tbMonth.MonthNumber
		FROM         App.tbYearPeriod INNER JOIN
		                      App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
		                      App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
		WHERE     ( App.tbYearPeriod.StartOn = @StartOn)
		END	
	RETURN
	END
