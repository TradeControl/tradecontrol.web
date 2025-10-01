CREATE VIEW Task.vwChangeLog
AS
	SELECT        Task.tbChangeLog.LogId, Task.tbChangeLog.TaskCode, Task.tbChangeLog.ChangedOn, Org.tbTransmitStatus.TransmitStatusCode, Org.tbTransmitStatus.TransmitStatus, Org.tbOrg.AccountCode, Org.tbOrg.AccountName, 
							 Task.tbChangeLog.ActivityCode, Task.tbChangeLog.TaskStatusCode, Task.tbStatus.TaskStatus, Task.tbChangeLog.ActionOn, Task.tbChangeLog.Quantity, Task.tbChangeLog.CashCode, Cash.tbCode.CashDescription, 
							 Task.tbChangeLog.UnitCharge, Task.tbChangeLog.UnitCharge * Task.tbChangeLog.Quantity AS TotalCharge, Task.tbChangeLog.TaxCode, App.tbTaxCode.TaxRate, Task.tbChangeLog.UpdatedBy
	FROM            Task.tbChangeLog INNER JOIN
							 Org.tbTransmitStatus ON Task.tbChangeLog.TransmitStatusCode = Org.tbTransmitStatus.TransmitStatusCode INNER JOIN
							 Org.tbOrg ON Task.tbChangeLog.AccountCode = Org.tbOrg.AccountCode INNER JOIN
							 Task.tbStatus ON Task.tbChangeLog.TaskStatusCode = Task.tbStatus.TaskStatusCode INNER JOIN
							 App.tbTaxCode ON Task.tbChangeLog.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
							 Cash.tbCode ON Task.tbChangeLog.CashCode = Cash.tbCode.CashCode;
