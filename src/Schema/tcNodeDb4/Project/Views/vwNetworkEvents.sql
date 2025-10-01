CREATE VIEW Project.vwNetworkEvents
AS
	SELECT        Project.tbAllocationEvent.ContractAddress, Project.tbAllocationEvent.LogId, App.tbEventType.EventTypeCode, App.tbEventType.EventType, 
							 Project.tbStatus.ProjectStatusCode, Project.tbStatus.ProjectStatus, Project.tbAllocationEvent.ActionOn, Project.tbAllocationEvent.UnitCharge, Project.tbAllocationEvent.TaxRate, Project.tbAllocationEvent.QuantityOrdered, 
							 Project.tbAllocationEvent.QuantityDelivered, Project.tbAllocationEvent.InsertedOn
	FROM            Project.tbAllocationEvent INNER JOIN
							 App.tbEventType ON Project.tbAllocationEvent.EventTypeCode = App.tbEventType.EventTypeCode INNER JOIN
							 Project.tbStatus ON Project.tbAllocationEvent.ProjectStatusCode = Project.tbStatus.ProjectStatusCode;
