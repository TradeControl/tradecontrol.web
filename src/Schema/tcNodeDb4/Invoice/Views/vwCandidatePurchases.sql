CREATE VIEW Invoice.vwCandidatePurchases
AS
	SELECT TOP 100 PERCENT  ProjectCode, SubjectCode, ContactName, ObjectCode, ActionOn, ActionedOn, Quantity, UnitCharge, TotalCharge, ProjectTitle, ProjectNotes, CashDescription, ActionName, OwnerName, ProjectStatus, InsertedBy, 
							 InsertedOn, UpdatedBy, UpdatedOn, ProjectStatusCode
	FROM            Project.vwProjects
	WHERE        (ProjectStatusCode = 1 OR
							 ProjectStatusCode = 2) AND (CashPolarityCode = 0) AND (CashCode IS NOT NULL)
	ORDER BY ActionOn;
