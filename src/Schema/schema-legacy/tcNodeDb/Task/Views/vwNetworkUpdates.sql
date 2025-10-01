CREATE VIEW Task.vwNetworkUpdates
AS
	WITH updates AS
	(
		SELECT DISTINCT TaskCode FROM Task.tbChangeLog 
		WHERE TransmitStatusCode = 2
		EXCEPT
		SELECT DISTINCT TaskCode FROM Task.tbChangeLog 
		WHERE TransmitStatusCode = 1
	)
	SELECT Task.tbTask.TaskCode, Task.tbTask.AccountCode, Task.tbTask.ActivityCode, Task.tbTask.TaskStatusCode, Task.tbStatus.TaskStatus, Task.tbTask.ActionOn, Task.tbTask.Quantity, App.tbTaxCode.TaxRate, Task.tbTask.UnitCharge
	FROM  updates 
		JOIN Task.tbTask ON updates.TaskCode = Task.tbTask.TaskCode 
		JOIN Cash.tbCode ON Task.tbTask.CashCode = Cash.tbCode.CashCode 
		JOIN Task.tbStatus ON Task.tbTask.TaskStatusCode = Task.tbStatus.TaskStatusCode
		JOIN App.tbTaxCode ON Task.tbTask.TaxCode = App.tbTaxCode.TaxCode AND Task.tbTask.TaxCode = App.tbTaxCode.TaxCode;
