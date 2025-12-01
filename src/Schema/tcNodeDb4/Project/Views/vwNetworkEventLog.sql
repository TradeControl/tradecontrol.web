CREATE VIEW Project.vwNetworkEventLog
AS
	SELECT        Project.tbAllocationEvent.ContractAddress, Project.tbAllocationEvent.LogId, Project.tbAllocationEvent.EventTypeCode, Project.tbAllocationEvent.ProjectStatusCode, Project.tbAllocationEvent.ActionOn, Project.tbAllocationEvent.UnitCharge, 
							 Project.tbAllocationEvent.TaxRate, Project.tbAllocationEvent.QuantityOrdered, Project.tbAllocationEvent.QuantityDelivered, Project.tbAllocationEvent.InsertedOn, Project.tbAllocationEvent.RowVer, App.tbEventType.EventType, 
							 Project.tbStatus.ProjectStatus, Project.tbAllocation.SubjectCode, Subject.tbSubject.SubjectName, Object.tbMirror.ObjectCode, Project.tbAllocation.AllocationCode, Project.tbAllocation.AllocationDescription, Project.tbAllocation.ProjectCode, 
							 Project.tbAllocation.CashPolarityCode, Cash.tbPolarity.CashPolarity, Project.tbAllocation.UnitOfMeasure, Project.tbAllocation.UnitOfCharge
	FROM            Project.tbAllocationEvent INNER JOIN
							 Project.tbStatus ON Project.tbAllocationEvent.ProjectStatusCode = Project.tbStatus.ProjectStatusCode INNER JOIN
							 Project.tbAllocation ON Project.tbAllocationEvent.ContractAddress = Project.tbAllocation.ContractAddress AND Project.tbStatus.ProjectStatusCode = Project.tbAllocation.ProjectStatusCode AND 
							 Project.tbStatus.ProjectStatusCode = Project.tbAllocation.ProjectStatusCode INNER JOIN
							 Subject.tbSubject ON Project.tbAllocation.SubjectCode = Subject.tbSubject.SubjectCode AND Project.tbAllocation.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Cash.tbPolarity ON Project.tbAllocation.CashPolarityCode = Cash.tbPolarity.CashPolarityCode AND Project.tbAllocation.CashPolarityCode = Cash.tbPolarity.CashPolarityCode INNER JOIN
							 Object.tbMirror ON Project.tbAllocation.SubjectCode = Object.tbMirror.SubjectCode AND Project.tbAllocation.AllocationCode = Object.tbMirror.AllocationCode INNER JOIN
							 App.tbEventType ON Project.tbAllocationEvent.EventTypeCode = App.tbEventType.EventTypeCode;

