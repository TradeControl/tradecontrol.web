CREATE VIEW Project.vwProjects
AS
	SELECT        Project.tbProject.ProjectCode, Project.tbProject.UserId, Project.tbProject.SubjectCode, Project.tbProject.ContactName, Project.tbProject.ObjectCode, Project.tbProject.ProjectTitle, Project.tbProject.ProjectStatusCode, Project.tbProject.ActionById, 
							 Project.tbProject.ActionOn, Project.tbProject.ActionedOn, Project.tbProject.PaymentOn, Project.tbProject.SecondReference, Project.tbProject.ProjectNotes, Project.tbProject.TaxCode, Project.tbProject.Quantity, Project.tbProject.UnitCharge, 
							 Project.tbProject.TotalCharge, Project.tbProject.AddressCodeFrom, Project.tbProject.AddressCodeTo, Project.tbProject.Printed, Project.tbProject.Spooled, Project.tbProject.InsertedBy, Project.tbProject.InsertedOn, Project.tbProject.UpdatedBy, 
							 Project.tbProject.UpdatedOn, Project.vwBucket.Period, Project.vwBucket.BucketId, ProjectStatus.ProjectStatus, Project.tbProject.CashCode, Cash.tbCode.CashDescription, tbUser_1.UserName AS OwnerName, 
							 Usr.tbUser.UserName AS ActionName, Subject.tbSubject.SubjectName, SubjectStatus.SubjectStatus, Subject.tbType.SubjectType, CASE WHEN Cash.tbCategory.CategoryCode IS NULL 
							 THEN Subject.tbType.CashPolarityCode ELSE Cash.tbCategory.CashPolarityCode END AS CashPolarityCode, Project.tbProject.RowVer
	FROM            Usr.tbUser INNER JOIN
							 Project.tbStatus AS ProjectStatus INNER JOIN
							 Subject.tbType INNER JOIN
							 Subject.tbSubject ON Subject.tbType.SubjectTypeCode = Subject.tbSubject.SubjectTypeCode INNER JOIN
							 Subject.tbStatus AS SubjectStatus ON Subject.tbSubject.SubjectStatusCode = SubjectStatus.SubjectStatusCode INNER JOIN
							 Project.tbProject ON Subject.tbSubject.SubjectCode = Project.tbProject.SubjectCode ON ProjectStatus.ProjectStatusCode = Project.tbProject.ProjectStatusCode ON Usr.tbUser.UserId = Project.tbProject.ActionById INNER JOIN
							 Usr.tbUser AS tbUser_1 ON Project.tbProject.UserId = tbUser_1.UserId INNER JOIN
							 Project.vwBucket ON Project.tbProject.ProjectCode = Project.vwBucket.ProjectCode LEFT OUTER JOIN
							 Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode LEFT OUTER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
