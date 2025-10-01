CREATE VIEW Task.vwNetworkEventLog
AS
	SELECT        Task.tbAllocationEvent.ContractAddress, Task.tbAllocationEvent.LogId, Task.tbAllocationEvent.EventTypeCode, Task.tbAllocationEvent.TaskStatusCode, Task.tbAllocationEvent.ActionOn, Task.tbAllocationEvent.UnitCharge, 
							 Task.tbAllocationEvent.TaxRate, Task.tbAllocationEvent.QuantityOrdered, Task.tbAllocationEvent.QuantityDelivered, Task.tbAllocationEvent.InsertedOn, Task.tbAllocationEvent.RowVer, App.tbEventType.EventType, 
							 Task.tbStatus.TaskStatus, Task.tbAllocation.AccountCode, Org.tbOrg.AccountName, Activity.tbMirror.ActivityCode, Task.tbAllocation.AllocationCode, Task.tbAllocation.AllocationDescription, Task.tbAllocation.TaskCode, 
							 Task.tbAllocation.CashModeCode, Cash.tbMode.CashMode, Task.tbAllocation.UnitOfMeasure, Task.tbAllocation.UnitOfCharge
	FROM            Task.tbAllocationEvent INNER JOIN
							 Task.tbStatus ON Task.tbAllocationEvent.TaskStatusCode = Task.tbStatus.TaskStatusCode INNER JOIN
							 Task.tbAllocation ON Task.tbAllocationEvent.ContractAddress = Task.tbAllocation.ContractAddress AND Task.tbStatus.TaskStatusCode = Task.tbAllocation.TaskStatusCode AND 
							 Task.tbStatus.TaskStatusCode = Task.tbAllocation.TaskStatusCode INNER JOIN
							 Org.tbOrg ON Task.tbAllocation.AccountCode = Org.tbOrg.AccountCode AND Task.tbAllocation.AccountCode = Org.tbOrg.AccountCode INNER JOIN
							 Cash.tbMode ON Task.tbAllocation.CashModeCode = Cash.tbMode.CashModeCode AND Task.tbAllocation.CashModeCode = Cash.tbMode.CashModeCode INNER JOIN
							 Activity.tbMirror ON Task.tbAllocation.AccountCode = Activity.tbMirror.AccountCode AND Task.tbAllocation.AllocationCode = Activity.tbMirror.AllocationCode INNER JOIN
							 App.tbEventType ON Task.tbAllocationEvent.EventTypeCode = App.tbEventType.EventTypeCode;

