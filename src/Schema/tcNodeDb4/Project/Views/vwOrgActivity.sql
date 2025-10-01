CREATE VIEW Project.vwSubjectObject
AS
	SELECT SubjectCode, ProjectStatusCode, ActionOn, ProjectTitle, ObjectCode, ActionById, ProjectCode, Period, BucketId, ContactName, ProjectStatus, ProjectNotes, ActionedOn, OwnerName, CashCode, CashDescription, Quantity, 
							 UnitCharge, TotalCharge, AddressCodeFrom, AddressCodeTo, Printed, InsertedBy, InsertedOn, UpdatedBy, UpdatedOn, SubjectName, ActionName
	FROM            Project.vwProjects
	WHERE        (ProjectStatusCode < 2);
