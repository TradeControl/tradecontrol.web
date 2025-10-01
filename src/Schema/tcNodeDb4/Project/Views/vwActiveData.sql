CREATE VIEW Project.vwActiveData
AS
	SELECT        ProjectCode, UserId, SubjectCode, ContactName, ObjectCode, ProjectTitle, ProjectStatusCode, ActionById, ActionOn, ActionedOn, PaymentOn, SecondReference, ProjectNotes, TaxCode, Quantity, UnitCharge, TotalCharge, 
							 AddressCodeFrom, AddressCodeTo, Printed, Spooled, InsertedBy, InsertedOn, UpdatedBy, UpdatedOn, Period, BucketId, ProjectStatus, CashCode, CashDescription, OwnerName, ActionName, SubjectName, 
							 SubjectStatus, SubjectType, CashPolarityCode
	FROM            Project.vwProjects
	WHERE        (ProjectStatusCode = 1);
