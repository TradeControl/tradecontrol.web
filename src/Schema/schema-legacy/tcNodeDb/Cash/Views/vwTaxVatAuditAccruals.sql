CREATE VIEW Cash.vwTaxVatAuditAccruals
AS
SELECT       App.tbYear.YearNumber, CONCAT(App.tbYear.Description, ' ', App.tbMonth.MonthName) AS YearPeriod, vat_accruals.StartOn, Task.tbTask.ActionOn, Task.tbTask.TaskTitle, Task.tbTask.TaskCode, Cash.tbCode.CashCode, 
                         Cash.tbCode.CashDescription, Activity.tbActivity.ActivityCode, Task.tbStatus.TaskStatus, Task.tbStatus.TaskStatusCode, vat_accruals.TaxCode, vat_accruals.TaxRate, vat_accruals.TotalValue, 
                         vat_accruals.TaxValue, vat_accruals.QuantityRemaining, Activity.tbActivity.UnitOfMeasure, vat_accruals.HomePurchases, vat_accruals.ExportSales, vat_accruals.ExportPurchases, vat_accruals.HomeSalesVat, 
                         vat_accruals.HomePurchasesVat, vat_accruals.ExportSalesVat, vat_accruals.ExportPurchasesVat, vat_accruals.VatDue, vat_accruals.HomeSales
FROM            Cash.vwTaxVatAccruals AS vat_accruals INNER JOIN
                         App.tbYearPeriod AS year_period ON vat_accruals.StartOn = year_period.StartOn INNER JOIN
                         App.tbYear ON year_period.YearNumber = App.tbYear.YearNumber INNER JOIN
                         App.tbMonth ON year_period.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
                         Task.tbTask ON vat_accruals.TaskCode = Task.tbTask.TaskCode INNER JOIN
                         Task.tbStatus ON Task.tbTask.TaskStatusCode = Task.tbStatus.TaskStatusCode AND Task.tbTask.TaskStatusCode = Task.tbStatus.TaskStatusCode AND 
                         Task.tbTask.TaskStatusCode = Task.tbStatus.TaskStatusCode AND Task.tbTask.TaskStatusCode = Task.tbStatus.TaskStatusCode AND Task.tbTask.TaskStatusCode = Task.tbStatus.TaskStatusCode AND 
                         Task.tbTask.TaskStatusCode = Task.tbStatus.TaskStatusCode INNER JOIN
                         Org.tbOrg ON Task.tbTask.AccountCode = Org.tbOrg.AccountCode AND Task.tbTask.AccountCode = Org.tbOrg.AccountCode AND Task.tbTask.AccountCode = Org.tbOrg.AccountCode AND 
                         Task.tbTask.AccountCode = Org.tbOrg.AccountCode AND Task.tbTask.AccountCode = Org.tbOrg.AccountCode AND Task.tbTask.AccountCode = Org.tbOrg.AccountCode INNER JOIN
                         Activity.tbActivity ON Task.tbTask.ActivityCode = Activity.tbActivity.ActivityCode AND Task.tbTask.ActivityCode = Activity.tbActivity.ActivityCode AND Task.tbTask.ActivityCode = Activity.tbActivity.ActivityCode AND 
                         Task.tbTask.ActivityCode = Activity.tbActivity.ActivityCode AND Task.tbTask.ActivityCode = Activity.tbActivity.ActivityCode AND Task.tbTask.ActivityCode = Activity.tbActivity.ActivityCode INNER JOIN
                         Cash.tbCode ON Task.tbTask.CashCode = Cash.tbCode.CashCode AND Task.tbTask.CashCode = Cash.tbCode.CashCode AND Task.tbTask.CashCode = Cash.tbCode.CashCode AND 
                         Task.tbTask.CashCode = Cash.tbCode.CashCode AND Task.tbTask.CashCode = Cash.tbCode.CashCode AND Task.tbTask.CashCode = Cash.tbCode.CashCode

