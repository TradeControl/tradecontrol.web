CREATE VIEW Task.vwNetworkEvents
AS
	SELECT        Task.tbAllocationEvent.ContractAddress, Task.tbAllocationEvent.LogId, App.tbEventType.EventTypeCode, App.tbEventType.EventType, 
							 Task.tbStatus.TaskStatusCode, Task.tbStatus.TaskStatus, Task.tbAllocationEvent.ActionOn, Task.tbAllocationEvent.UnitCharge, Task.tbAllocationEvent.TaxRate, Task.tbAllocationEvent.QuantityOrdered, 
							 Task.tbAllocationEvent.QuantityDelivered, Task.tbAllocationEvent.InsertedOn
	FROM            Task.tbAllocationEvent INNER JOIN
							 App.tbEventType ON Task.tbAllocationEvent.EventTypeCode = App.tbEventType.EventTypeCode INNER JOIN
							 Task.tbStatus ON Task.tbAllocationEvent.TaskStatusCode = Task.tbStatus.TaskStatusCode;
