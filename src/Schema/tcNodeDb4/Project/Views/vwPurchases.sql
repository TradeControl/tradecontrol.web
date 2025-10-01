CREATE VIEW Project.vwPurchases
AS
	SELECT        Project.vwProjects.ProjectCode, Project.vwProjects.ObjectCode, Project.vwProjects.ProjectStatusCode, Project.vwProjects.ActionOn, Project.vwProjects.ActionById, Project.vwProjects.ProjectTitle, Project.vwProjects.Period, Project.vwProjects.BucketId, 
							 Project.vwProjects.SubjectCode, Project.vwProjects.ContactName, Project.vwProjects.ProjectStatus, Project.vwProjects.ProjectNotes, Project.vwProjects.ActionedOn, Project.vwProjects.OwnerName, Project.vwProjects.CashCode, 
							 Project.vwProjects.CashDescription, Project.vwProjects.Quantity, Object.tbObject.UnitOfMeasure, Project.vwProjects.UnitCharge, Project.vwProjects.TotalCharge, Subject_tbAddress_1.Address AS FromAddress, 
							 Subject.tbAddress.Address AS ToAddress, Project.vwProjects.Printed, Project.vwProjects.InsertedBy, Project.vwProjects.InsertedOn, Project.vwProjects.UpdatedBy, Project.vwProjects.UpdatedOn, Project.vwProjects.SubjectName, 
							 Project.vwProjects.ActionName, Project.vwProjects.SecondReference
	FROM            Project.vwProjects LEFT OUTER JOIN
							 Subject.tbAddress AS Subject_tbAddress_1 ON Project.vwProjects.AddressCodeFrom = Subject_tbAddress_1.AddressCode LEFT OUTER JOIN
							 Subject.tbAddress ON Project.vwProjects.AddressCodeTo = Subject.tbAddress.AddressCode INNER JOIN
							 Object.tbObject ON Project.vwProjects.ObjectCode = Object.tbObject.ObjectCode
	WHERE        (Project.vwProjects.CashCode IS NOT NULL) AND (Project.vwProjects.CashPolarityCode = 0);
