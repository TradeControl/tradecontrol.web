CREATE VIEW Invoice.vwCandidateSales
AS
	SELECT TOP 100 PERCENT ProjectCode, SubjectCode, ContactName, ObjectCode, ActionOn, ActionedOn, ProjectTitle, Quantity, UnitCharge, TotalCharge, ProjectNotes, CashDescription, ActionName, OwnerName, ProjectStatus, InsertedBy, 
							 InsertedOn, UpdatedBy, UpdatedOn, ProjectStatusCode
	FROM            Project.vwProjects
	WHERE        (ProjectStatusCode = 1 OR
							 ProjectStatusCode = 2) AND (CashPolarityCode = 1) AND (CashCode IS NOT NULL)
	ORDER BY ActionOn;
