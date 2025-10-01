CREATE VIEW Task.vwDoc
AS
	SELECT     Task.fnEmailAddress(Task.tbTask.TaskCode) AS EmailAddress, Task.tbTask.TaskCode, Task.tbTask.TaskStatusCode, Task.tbStatus.TaskStatus, 
						  Task.tbTask.ContactName, Org.tbContact.NickName, Usr.tbUser.UserName, Org.tbOrg.AccountName, Org.tbAddress.Address AS InvoiceAddress, 
						  Org_tb1.AccountName AS DeliveryAccountName, Org_tbAddress1.Address AS DeliveryAddress, Org_tb2.AccountName AS CollectionAccountName, 
						  Org_tbAddress2.Address AS CollectionAddress, Task.tbTask.AccountCode, Task.tbTask.TaskNotes, Task.tbTask.ActivityCode, Task.tbTask.ActionOn, 
						  Activity.tbActivity.UnitOfMeasure, Task.tbTask.Quantity, App.tbTaxCode.TaxCode, App.tbTaxCode.TaxRate, Task.tbTask.UnitCharge, Task.tbTask.TotalCharge, 
						  Usr.tbUser.MobileNumber, Usr.tbUser.Signature, Task.tbTask.TaskTitle, Task.tbTask.PaymentOn, Task.tbTask.SecondReference, Org.tbOrg.PaymentTerms
	FROM         Org.tbOrg AS Org_tb2 RIGHT OUTER JOIN
						  Org.tbAddress AS Org_tbAddress2 ON Org_tb2.AccountCode = Org_tbAddress2.AccountCode RIGHT OUTER JOIN
						  Task.tbStatus INNER JOIN
						  Usr.tbUser INNER JOIN
						  Activity.tbActivity INNER JOIN
						  Task.tbTask ON Activity.tbActivity.ActivityCode = Task.tbTask.ActivityCode INNER JOIN
						  Org.tbOrg ON Task.tbTask.AccountCode = Org.tbOrg.AccountCode LEFT OUTER JOIN
						  Org.tbAddress ON Org.tbOrg.AddressCode = Org.tbAddress.AddressCode ON Usr.tbUser.UserId = Task.tbTask.ActionById ON 
						  Task.tbStatus.TaskStatusCode = Task.tbTask.TaskStatusCode LEFT OUTER JOIN
						  Org.tbAddress AS Org_tbAddress1 LEFT OUTER JOIN
						  Org.tbOrg AS Org_tb1 ON Org_tbAddress1.AccountCode = Org_tb1.AccountCode ON Task.tbTask.AddressCodeTo = Org_tbAddress1.AddressCode ON 
						  Org_tbAddress2.AddressCode = Task.tbTask.AddressCodeFrom LEFT OUTER JOIN
						  Org.tbContact ON Task.tbTask.ContactName = Org.tbContact.ContactName AND Task.tbTask.AccountCode = Org.tbContact.AccountCode LEFT OUTER JOIN
						  App.tbTaxCode ON Task.tbTask.TaxCode = App.tbTaxCode.TaxCode
