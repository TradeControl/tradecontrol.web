CREATE VIEW Project.vwEdit
AS
	SELECT        Project.tbProject.ProjectCode, Project.tbProject.UserId, Project.tbProject.SubjectCode, Project.tbProject.ProjectTitle, Project.tbProject.ContactName, Project.tbProject.ObjectCode, Project.tbProject.ProjectStatusCode, Project.tbProject.ActionById, 
							 Project.tbProject.ActionOn, Project.tbProject.ActionedOn, Project.tbProject.ProjectNotes, Project.tbProject.Quantity, Project.tbProject.CashCode, Project.tbProject.TaxCode, Project.tbProject.UnitCharge, Project.tbProject.TotalCharge, 
							 Project.tbProject.AddressCodeFrom, Project.tbProject.AddressCodeTo, Project.tbProject.Printed, Project.tbProject.InsertedBy, Project.tbProject.InsertedOn, Project.tbProject.UpdatedBy, Project.tbProject.UpdatedOn, Project.tbProject.PaymentOn, 
							 Project.tbProject.SecondReference, Project.tbProject.Spooled, Object.tbObject.UnitOfMeasure, Project.tbStatus.ProjectStatus
	FROM            Project.tbProject INNER JOIN
							 Object.tbObject ON Project.tbProject.ObjectCode = Object.tbObject.ObjectCode INNER JOIN
							 Project.tbStatus ON Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode;
