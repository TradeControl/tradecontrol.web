CREATE VIEW Project.vwDoc
AS
	SELECT     Project.fnEmailAddress(Project.tbProject.ProjectCode) AS EmailAddress, Project.tbProject.ProjectCode, Project.tbProject.ProjectStatusCode, Project.tbStatus.ProjectStatus, 
						  Project.tbProject.ContactName, Subject.tbContact.NickName, Usr.tbUser.UserName, Subject.tbSubject.SubjectName, Subject.tbAddress.Address AS InvoiceAddress, 
						  Subject_tb1.SubjectName AS DeliveryAccountName, Subject_tbAddress1.Address AS DeliveryAddress, Subject_tb2.SubjectName AS CollectionAccountName, 
						  Subject_tbAddress2.Address AS CollectionAddress, Project.tbProject.SubjectCode, Project.tbProject.ProjectNotes, Project.tbProject.ObjectCode, Project.tbProject.ActionOn, 
						  Object.tbObject.UnitOfMeasure, Project.tbProject.Quantity, App.tbTaxCode.TaxCode, App.tbTaxCode.TaxRate, Project.tbProject.UnitCharge, Project.tbProject.TotalCharge, 
						  Usr.tbUser.MobileNumber, Usr.tbUser.Signature, Project.tbProject.ProjectTitle, Project.tbProject.PaymentOn, Project.tbProject.SecondReference, Subject.tbSubject.PaymentTerms
	FROM         Subject.tbSubject AS Subject_tb2 RIGHT OUTER JOIN
						  Subject.tbAddress AS Subject_tbAddress2 ON Subject_tb2.SubjectCode = Subject_tbAddress2.SubjectCode RIGHT OUTER JOIN
						  Project.tbStatus INNER JOIN
						  Usr.tbUser INNER JOIN
						  Object.tbObject INNER JOIN
						  Project.tbProject ON Object.tbObject.ObjectCode = Project.tbProject.ObjectCode INNER JOIN
						  Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode LEFT OUTER JOIN
						  Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode ON Usr.tbUser.UserId = Project.tbProject.ActionById ON 
						  Project.tbStatus.ProjectStatusCode = Project.tbProject.ProjectStatusCode LEFT OUTER JOIN
						  Subject.tbAddress AS Subject_tbAddress1 LEFT OUTER JOIN
						  Subject.tbSubject AS Subject_tb1 ON Subject_tbAddress1.SubjectCode = Subject_tb1.SubjectCode ON Project.tbProject.AddressCodeTo = Subject_tbAddress1.AddressCode ON 
						  Subject_tbAddress2.AddressCode = Project.tbProject.AddressCodeFrom LEFT OUTER JOIN
						  Subject.tbContact ON Project.tbProject.ContactName = Subject.tbContact.ContactName AND Project.tbProject.SubjectCode = Subject.tbContact.SubjectCode LEFT OUTER JOIN
						  App.tbTaxCode ON Project.tbProject.TaxCode = App.tbTaxCode.TaxCode
