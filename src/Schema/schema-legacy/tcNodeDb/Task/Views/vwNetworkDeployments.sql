CREATE VIEW Task.vwNetworkDeployments
AS
	SELECT DISTINCT Task.tbTask.TaskCode, Task.tbTask.AccountCode, Task.tbTask.ActivityCode, Activity.tbActivity.ActivityDescription, Task.tbTask.TaskTitle, Task.tbTask.TaskStatusCode, Task.tbStatus.TaskStatus, Task.tbTask.ActionOn, Task.tbTask.Quantity, 
							 Cash.tbCategory.CashModeCode, Cash.tbMode.CashMode, App.tbTaxCode.TaxRate, Task.tbTask.UnitCharge, Activity.tbActivity.UnitOfMeasure,
								 (SELECT        UnitOfCharge
								   FROM            App.tbOptions) AS UnitOfCharge
	FROM            Task.tbChangeLog INNER JOIN
							 Task.tbTask ON Task.tbChangeLog.TaskCode = Task.tbTask.TaskCode INNER JOIN
							 Cash.tbCode ON Task.tbTask.CashCode = Cash.tbCode.CashCode INNER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode AND Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
							 App.tbTaxCode ON Task.tbTask.TaxCode = App.tbTaxCode.TaxCode AND Task.tbTask.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
							 Activity.tbActivity ON Task.tbTask.ActivityCode = Activity.tbActivity.ActivityCode AND Task.tbTask.ActivityCode = Activity.tbActivity.ActivityCode INNER JOIN
							 Task.tbStatus ON Task.tbTask.TaskStatusCode = Task.tbStatus.TaskStatusCode INNER JOIN
							 Cash.tbMode ON Cash.tbCategory.CashModeCode = Cash.tbMode.CashModeCode AND Cash.tbCategory.CashModeCode = Cash.tbMode.CashModeCode
	WHERE        (Task.tbChangeLog.TransmitStatusCode = 1)
