CREATE VIEW Task.vwQuotationSpool
AS
	SELECT        sales_order.TaskCode, sales_order.ContactName, Org.tbContact.NickName, Usr.tbUser.UserName, Org.tbOrg.AccountName, invoice_address.Address AS InvoiceAddress, 
							 delivery_address.Address AS DeliveryAddress, sales_order.AccountCode, sales_order.TaskNotes, sales_order.ActivityCode, sales_order.ActionOn, Activity.tbActivity.UnitOfMeasure, sales_order.Quantity, 
							 App.tbTaxCode.TaxCode, App.tbTaxCode.TaxRate, sales_order.UnitCharge, sales_order.TotalCharge, Usr.tbUser.MobileNumber, Usr.tbUser.Signature, sales_order.TaskTitle
	FROM            Usr.tbUser INNER JOIN
							 Activity.tbActivity INNER JOIN
							 Task.tbTask AS sales_order ON Activity.tbActivity.ActivityCode = sales_order.ActivityCode INNER JOIN
							 Org.tbOrg ON sales_order.AccountCode = Org.tbOrg.AccountCode LEFT OUTER JOIN
							 Org.tbAddress AS invoice_address ON Org.tbOrg.AddressCode = invoice_address.AddressCode ON Usr.tbUser.UserId = sales_order.ActionById LEFT OUTER JOIN
							 Org.tbAddress AS delivery_address ON sales_order.AddressCodeTo = delivery_address.AddressCode LEFT OUTER JOIN
							 App.tbTaxCode ON sales_order.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
							 Org.tbContact ON sales_order.AccountCode = Org.tbContact.AccountCode AND sales_order.ContactName = Org.tbContact.ContactName
	WHERE EXISTS (
		SELECT        UserName, DocTypeCode, DocumentNumber, SpooledOn
		FROM            App.tbDocSpool AS doc
		WHERE        (DocTypeCode = 0) AND (UserName = SUSER_SNAME()) AND (sales_order.TaskCode = DocumentNumber));
