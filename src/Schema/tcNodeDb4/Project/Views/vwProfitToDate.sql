
CREATE   VIEW Project.vwProfitToDate
AS
	WITH ProjectProfitToDate AS 
		(SELECT        MAX(PaymentOn) AS LastPaymentOn
		 FROM            Project.tbProject)
	SELECT TOP (100) PERCENT App.tbYearPeriod.StartOn, App.tbYear.Description + SPACE(1) + App.tbMonth.MonthName AS Description
	FROM            ProjectProfitToDate INNER JOIN
							App.tbYearPeriod INNER JOIN
							App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
							App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber ON DATEADD(m, 1, ProjectProfitToDate.LastPaymentOn) > App.tbYearPeriod.StartOn
	WHERE        (App.tbYear.CashStatusCode < 3)
	ORDER BY App.tbYearPeriod.StartOn DESC;
