CREATE VIEW Project.vwNetworkUpdates
AS
	WITH updates AS
	(
		SELECT DISTINCT ProjectCode FROM Project.tbChangeLog 
		WHERE TransmitStatusCode = 2
		EXCEPT
		SELECT DISTINCT ProjectCode FROM Project.tbChangeLog 
		WHERE TransmitStatusCode = 1
	)
	SELECT Project.tbProject.ProjectCode, Project.tbProject.SubjectCode, Project.tbProject.ObjectCode, Project.tbProject.ProjectStatusCode, Project.tbStatus.ProjectStatus, Project.tbProject.ActionOn, Project.tbProject.Quantity, App.tbTaxCode.TaxRate, Project.tbProject.UnitCharge
	FROM  updates 
		JOIN Project.tbProject ON updates.ProjectCode = Project.tbProject.ProjectCode 
		JOIN Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode 
		JOIN Project.tbStatus ON Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode
		JOIN App.tbTaxCode ON Project.tbProject.TaxCode = App.tbTaxCode.TaxCode AND Project.tbProject.TaxCode = App.tbTaxCode.TaxCode;
