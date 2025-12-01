CREATE VIEW Project.vwNetworkAllocations
AS
	SELECT        Project.tbAllocation.ContractAddress, Project.tbAllocation.SubjectCode, Subject.tbSubject.SubjectName, Object.tbMirror.ObjectCode, Project.tbAllocation.AllocationCode, Project.tbAllocation.AllocationDescription, Project.tbAllocation.ProjectCode, 
							 Project.tbAllocation.ProjectTitle, Project.tbAllocation.CashPolarityCode, Cash.tbPolarity.CashPolarity, Project.tbAllocation.UnitOfMeasure, Project.tbAllocation.UnitOfCharge, Project.tbAllocation.ProjectStatusCode, Project.tbStatus.ProjectStatus, 
							 Project.tbAllocation.ActionOn, Project.tbAllocation.UnitCharge, Project.tbAllocation.TaxRate, Project.tbAllocation.QuantityOrdered, Project.tbAllocation.QuantityDelivered, Project.tbAllocation.InsertedOn, Project.tbAllocation.RowVer
	FROM            Project.tbAllocation INNER JOIN
							 Object.tbMirror ON Project.tbAllocation.SubjectCode = Object.tbMirror.SubjectCode AND Project.tbAllocation.AllocationCode = Object.tbMirror.AllocationCode INNER JOIN
							 Subject.tbSubject ON Project.tbAllocation.SubjectCode = Subject.tbSubject.SubjectCode AND Project.tbAllocation.SubjectCode = Subject.tbSubject.SubjectCode AND Project.tbAllocation.SubjectCode = Subject.tbSubject.SubjectCode AND 
							 Project.tbAllocation.SubjectCode = Subject.tbSubject.SubjectCode AND Project.tbAllocation.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Cash.tbPolarity ON Project.tbAllocation.CashPolarityCode = Cash.tbPolarity.CashPolarityCode AND Project.tbAllocation.CashPolarityCode = Cash.tbPolarity.CashPolarityCode AND 
							 Project.tbAllocation.CashPolarityCode = Cash.tbPolarity.CashPolarityCode AND Project.tbAllocation.CashPolarityCode = Cash.tbPolarity.CashPolarityCode AND Project.tbAllocation.CashPolarityCode = Cash.tbPolarity.CashPolarityCode INNER JOIN
							 Project.tbStatus ON Project.tbAllocation.ProjectStatusCode = Project.tbStatus.ProjectStatusCode AND Project.tbAllocation.ProjectStatusCode = Project.tbStatus.ProjectStatusCode AND 
							 Project.tbAllocation.ProjectStatusCode = Project.tbStatus.ProjectStatusCode AND Project.tbAllocation.ProjectStatusCode = Project.tbStatus.ProjectStatusCode AND Project.tbAllocation.ProjectStatusCode = Project.tbStatus.ProjectStatusCode;
