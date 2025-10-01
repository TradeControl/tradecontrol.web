CREATE VIEW Task.vwPurchaseOrderDeliverySpool
AS
	SELECT        purchase_order.TaskCode, purchase_order.ContactName, Org.tbContact.NickName, Usr.tbUser.UserName, Org.tbOrg.AccountName, invoice_address.Address AS InvoiceAddress, 
							 delivery_account.AccountName AS CollectAccount, delivery_address.Address AS CollectAddress, collection_account.AccountName AS DeliveryAccount, collection_address.Address AS DeliveryAddress, 
							 purchase_order.AccountCode, purchase_order.TaskNotes, purchase_order.ActivityCode, purchase_order.ActionOn, Activity.tbActivity.UnitOfMeasure, purchase_order.Quantity, App.tbTaxCode.TaxCode, 
							 App.tbTaxCode.TaxRate, purchase_order.UnitCharge, purchase_order.TotalCharge, Usr.tbUser.MobileNumber, Usr.tbUser.Signature, purchase_order.TaskTitle
	FROM            Org.tbOrg AS collection_account INNER JOIN
							 Org.tbOrg AS delivery_account INNER JOIN
							 Usr.tbUser INNER JOIN
							 Activity.tbActivity INNER JOIN
							 Task.tbTask AS purchase_order ON Activity.tbActivity.ActivityCode = purchase_order.ActivityCode INNER JOIN
							 Org.tbOrg ON purchase_order.AccountCode = Org.tbOrg.AccountCode LEFT OUTER JOIN
							 Org.tbAddress AS invoice_address ON Org.tbOrg.AddressCode = invoice_address.AddressCode ON Usr.tbUser.UserId = purchase_order.ActionById INNER JOIN
							 Org.tbAddress AS collection_address ON purchase_order.AddressCodeTo = collection_address.AddressCode LEFT OUTER JOIN
							 App.tbTaxCode ON purchase_order.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
							 Org.tbContact ON purchase_order.ContactName = Org.tbContact.ContactName AND purchase_order.AccountCode = Org.tbContact.AccountCode INNER JOIN
							 Org.tbAddress AS delivery_address ON purchase_order.AddressCodeFrom = delivery_address.AddressCode ON delivery_account.AccountCode = delivery_address.AccountCode ON 
							 collection_account.AccountCode = collection_address.AccountCode
	WHERE EXISTS (
		SELECT        UserName, DocTypeCode, DocumentNumber, SpooledOn
		FROM            App.tbDocSpool AS doc
		WHERE        (DocTypeCode = 3) AND (UserName = SUSER_SNAME()) AND (purchase_order.TaskCode = DocumentNumber));
