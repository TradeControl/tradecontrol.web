CREATE VIEW Cash.vwTaxBizAuditAccruals
AS
	SELECT     App.tbYear.YearNumber, CONCAT(App.tbYear.Description, ' ', App.tbMonth.MonthName) AS YearPeriod, Cash.vwTaxBizAccruals.StartOn, Project.tbProject.ProjectCode, Project.tbProject.SubjectCode, Subject.tbSubject.SubjectName, 
							 Project.tbProject.ProjectTitle, Object.tbObject.ObjectCode, Project.tbStatus.ProjectStatusCode, Project.tbStatus.ProjectStatus, Project.tbProject.CashCode, Cash.tbCode.CashDescription, Object.tbObject.UnitOfMeasure, 
							 Cash.vwTaxBizAccruals.QuantityRemaining, Cash.vwTaxBizAccruals.OrderValue, Cash.vwTaxBizAccruals.TaxDue
	FROM            Project.tbProject INNER JOIN
							 Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Cash.vwTaxBizAccruals ON Project.tbProject.ProjectCode = Cash.vwTaxBizAccruals.ProjectCode INNER JOIN
							 Project.tbStatus ON Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode AND Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode INNER JOIN
							 Object.tbObject ON Project.tbProject.ObjectCode = Object.tbObject.ObjectCode AND Project.tbProject.ObjectCode = Object.tbObject.ObjectCode INNER JOIN
							 Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode AND Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
							 App.tbYearPeriod ON Cash.vwTaxBizAccruals.StartOn = App.tbYearPeriod.StartOn INNER JOIN
							 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND 
							 App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND 
							 App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
							 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber AND App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber AND 
							 App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber AND App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber AND App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber AND 
							 App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber AND App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
