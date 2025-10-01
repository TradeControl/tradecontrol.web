CREATE VIEW Task.vwNetworkChangeLog
AS
	SELECT Task.tbTask.AccountCode, Org.tbOrg.AccountName, Task.tbTask.TaskCode, Task.tbChangeLog.LogId, Task.tbChangeLog.ChangedOn, Task.tbChangeLog.TransmitStatusCode, Org.tbTransmitStatus.TransmitStatus, 
				Task.tbChangeLog.ActivityCode, Activity.tbMirror.AllocationCode, Task.tbChangeLog.TaskStatusCode, Task.tbStatus.TaskStatus, Cash.tbMode.CashModeCode, Cash.tbMode.CashMode, Task.tbChangeLog.ActionOn, 
				Task.tbChangeLog.TaxCode, Task.tbChangeLog.Quantity, Task.tbChangeLog.UnitCharge, Task.tbChangeLog.UpdatedBy, Task.tbChangeLog.RowVer
	FROM Task.tbChangeLog 
		INNER JOIN Task.tbTask ON Task.tbChangeLog.TaskCode = Task.tbTask.TaskCode INNER JOIN
				Cash.tbCode ON Task.tbTask.CashCode = Cash.tbCode.CashCode INNER JOIN
				Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode AND Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
				Cash.tbMode ON Cash.tbCategory.CashModeCode = Cash.tbMode.CashModeCode AND Cash.tbCategory.CashModeCode = Cash.tbMode.CashModeCode INNER JOIN
				Org.tbOrg ON Task.tbTask.AccountCode = Org.tbOrg.AccountCode AND Task.tbTask.AccountCode = Org.tbOrg.AccountCode AND Task.tbTask.AccountCode = Org.tbOrg.AccountCode INNER JOIN
				Org.tbTransmitStatus ON Task.tbChangeLog.TransmitStatusCode = Org.tbTransmitStatus.TransmitStatusCode AND Task.tbChangeLog.TransmitStatusCode = Org.tbTransmitStatus.TransmitStatusCode AND 
				Task.tbChangeLog.TransmitStatusCode = Org.tbTransmitStatus.TransmitStatusCode AND Task.tbChangeLog.TransmitStatusCode = Org.tbTransmitStatus.TransmitStatusCode AND 
				Task.tbChangeLog.TransmitStatusCode = Org.tbTransmitStatus.TransmitStatusCode INNER JOIN
				Task.tbStatus ON Task.tbChangeLog.TaskStatusCode = Task.tbStatus.TaskStatusCode LEFT OUTER JOIN
				Activity.tbMirror ON Task.tbChangeLog.AccountCode = Activity.tbMirror.AccountCode AND Task.tbChangeLog.ActivityCode = Activity.tbMirror.ActivityCode
	WHERE Task.tbChangeLog.TransmitStatusCode > 0
