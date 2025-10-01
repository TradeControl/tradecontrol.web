CREATE VIEW Task.vwNetworkAllocations
AS
	SELECT        Task.tbAllocation.ContractAddress, Task.tbAllocation.AccountCode, Org.tbOrg.AccountName, Activity.tbMirror.ActivityCode, Task.tbAllocation.AllocationCode, Task.tbAllocation.AllocationDescription, Task.tbAllocation.TaskCode, 
							 Task.tbAllocation.TaskTitle, Task.tbAllocation.CashModeCode, Cash.tbMode.CashMode, Task.tbAllocation.UnitOfMeasure, Task.tbAllocation.UnitOfCharge, Task.tbAllocation.TaskStatusCode, Task.tbStatus.TaskStatus, 
							 Task.tbAllocation.ActionOn, Task.tbAllocation.UnitCharge, Task.tbAllocation.TaxRate, Task.tbAllocation.QuantityOrdered, Task.tbAllocation.QuantityDelivered, Task.tbAllocation.InsertedOn, Task.tbAllocation.RowVer
	FROM            Task.tbAllocation INNER JOIN
							 Activity.tbMirror ON Task.tbAllocation.AccountCode = Activity.tbMirror.AccountCode AND Task.tbAllocation.AllocationCode = Activity.tbMirror.AllocationCode INNER JOIN
							 Org.tbOrg ON Task.tbAllocation.AccountCode = Org.tbOrg.AccountCode AND Task.tbAllocation.AccountCode = Org.tbOrg.AccountCode AND Task.tbAllocation.AccountCode = Org.tbOrg.AccountCode AND 
							 Task.tbAllocation.AccountCode = Org.tbOrg.AccountCode AND Task.tbAllocation.AccountCode = Org.tbOrg.AccountCode INNER JOIN
							 Cash.tbMode ON Task.tbAllocation.CashModeCode = Cash.tbMode.CashModeCode AND Task.tbAllocation.CashModeCode = Cash.tbMode.CashModeCode AND 
							 Task.tbAllocation.CashModeCode = Cash.tbMode.CashModeCode AND Task.tbAllocation.CashModeCode = Cash.tbMode.CashModeCode AND Task.tbAllocation.CashModeCode = Cash.tbMode.CashModeCode INNER JOIN
							 Task.tbStatus ON Task.tbAllocation.TaskStatusCode = Task.tbStatus.TaskStatusCode AND Task.tbAllocation.TaskStatusCode = Task.tbStatus.TaskStatusCode AND 
							 Task.tbAllocation.TaskStatusCode = Task.tbStatus.TaskStatusCode AND Task.tbAllocation.TaskStatusCode = Task.tbStatus.TaskStatusCode AND Task.tbAllocation.TaskStatusCode = Task.tbStatus.TaskStatusCode;
