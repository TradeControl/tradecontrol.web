CREATE   VIEW Subject.vwProjects
AS
	SELECT        Project.vwProjects.SubjectCode, Project.vwProjects.ProjectCode, Project.vwProjects.UserId, Project.vwProjects.ContactName, Project.vwProjects.ObjectCode, Project.vwProjects.ProjectTitle, Project.vwProjects.ProjectStatusCode, Project.vwProjects.ActionById, 
							 Project.vwProjects.ActionOn, Project.vwProjects.ActionedOn, Project.vwProjects.PaymentOn, Project.vwProjects.SecondReference, Project.vwProjects.ProjectNotes, Project.vwProjects.TaxCode, Project.vwProjects.Quantity, Project.vwProjects.UnitCharge, 
							 Project.vwProjects.TotalCharge, Project.vwProjects.AddressCodeFrom, Project.vwProjects.AddressCodeTo, Project.vwProjects.Printed, Project.vwProjects.Spooled, Project.vwProjects.InsertedBy, Project.vwProjects.InsertedOn, Project.vwProjects.UpdatedBy, 
							 Project.vwProjects.UpdatedOn, Project.vwProjects.Period, Project.vwProjects.BucketId, Project.vwProjects.ProjectStatus, Project.vwProjects.CashCode, Project.vwProjects.CashDescription, Project.vwProjects.OwnerName, Project.vwProjects.ActionName, 
							 Project.vwProjects.SubjectName, Project.vwProjects.SubjectStatus, Project.vwProjects.SubjectType, Project.vwProjects.CashPolarityCode, Cash.tbPolarity.CashPolarity
	FROM            Project.vwProjects INNER JOIN
							 Cash.tbPolarity ON Project.vwProjects.CashPolarityCode = Cash.tbPolarity.CashPolarityCode
	WHERE        (Project.vwProjects.CashCode IS NOT NULL)

