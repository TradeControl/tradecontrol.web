CREATE VIEW Subject.vwInvoiceSummary
AS
	WITH ois AS
	(
		SELECT        SubjectCode, StartOn, SUM(InvoiceValue) AS PeriodValue
		FROM            Invoice.vwRegister
		GROUP BY SubjectCode, StartOn
	), acc AS
	(
		SELECT Subject.tbSubject.SubjectCode, App.vwPeriods.StartOn
		FROM Subject.tbSubject CROSS JOIN App.vwPeriods
	)
	SELECT TOP (100) PERCENT acc.SubjectCode, acc.StartOn, ois.PeriodValue 
	FROM ois RIGHT OUTER JOIN acc ON ois.SubjectCode = acc.SubjectCode AND ois.StartOn = acc.StartOn
	ORDER BY acc.SubjectCode, acc.StartOn;

