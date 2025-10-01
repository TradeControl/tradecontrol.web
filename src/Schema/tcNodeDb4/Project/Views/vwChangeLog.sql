CREATE VIEW Project.vwChangeLog
AS
	SELECT        Project.tbChangeLog.LogId, Project.tbChangeLog.ProjectCode, Project.tbChangeLog.ChangedOn, Subject.tbTransmitStatus.TransmitStatusCode, Subject.tbTransmitStatus.TransmitStatus, Subject.tbSubject.SubjectCode, Subject.tbSubject.SubjectName, 
							 Project.tbChangeLog.ObjectCode, Project.tbChangeLog.ProjectStatusCode, Project.tbStatus.ProjectStatus, Project.tbChangeLog.ActionOn, Project.tbChangeLog.Quantity, Project.tbChangeLog.CashCode, Cash.tbCode.CashDescription, 
							 Project.tbChangeLog.UnitCharge, Project.tbChangeLog.UnitCharge * Project.tbChangeLog.Quantity AS TotalCharge, Project.tbChangeLog.TaxCode, App.tbTaxCode.TaxRate, Project.tbChangeLog.UpdatedBy
	FROM            Project.tbChangeLog INNER JOIN
							 Subject.tbTransmitStatus ON Project.tbChangeLog.TransmitStatusCode = Subject.tbTransmitStatus.TransmitStatusCode INNER JOIN
							 Subject.tbSubject ON Project.tbChangeLog.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Project.tbStatus ON Project.tbChangeLog.ProjectStatusCode = Project.tbStatus.ProjectStatusCode INNER JOIN
							 App.tbTaxCode ON Project.tbChangeLog.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
							 Cash.tbCode ON Project.tbChangeLog.CashCode = Cash.tbCode.CashCode;
