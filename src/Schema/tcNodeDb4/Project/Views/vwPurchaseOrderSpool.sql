CREATE VIEW Project.vwPurchaseOrderSpool
AS
	SELECT        purchase_order.ProjectCode, purchase_order.ContactName, Subject.tbContact.NickName, Usr.tbUser.UserName, Subject.tbSubject.SubjectName, invoice_address.Address AS InvoiceAddress, 
							 delivery_address.Address AS DeliveryAddress, purchase_order.SubjectCode, purchase_order.ProjectNotes, purchase_order.ObjectCode, purchase_order.ActionOn, Object.tbObject.UnitOfMeasure, 
							 purchase_order.Quantity, App.tbTaxCode.TaxCode, App.tbTaxCode.TaxRate, purchase_order.UnitCharge, purchase_order.TotalCharge, Usr.tbUser.MobileNumber, Usr.tbUser.Signature, 
							 purchase_order.ProjectTitle
	FROM            Usr.tbUser INNER JOIN
							 Object.tbObject INNER JOIN
							 Project.tbProject AS purchase_order ON Object.tbObject.ObjectCode = purchase_order.ObjectCode INNER JOIN
							 Subject.tbSubject ON purchase_order.SubjectCode = Subject.tbSubject.SubjectCode LEFT OUTER JOIN
							 Subject.tbAddress AS invoice_address ON Subject.tbSubject.AddressCode = invoice_address.AddressCode ON Usr.tbUser.UserId = purchase_order.ActionById LEFT OUTER JOIN
							 Subject.tbAddress AS delivery_address ON purchase_order.AddressCodeTo = delivery_address.AddressCode LEFT OUTER JOIN
							 App.tbTaxCode ON purchase_order.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
							 Subject.tbContact ON purchase_order.SubjectCode = Subject.tbContact.SubjectCode AND purchase_order.ContactName = Subject.tbContact.ContactName
	WHERE EXISTS (SELECT * FROM App.tbDocSpool AS doc WHERE DocTypeCode = 3 AND UserName = SUSER_SNAME() AND purchase_order.ProjectCode = doc.DocumentNumber);
