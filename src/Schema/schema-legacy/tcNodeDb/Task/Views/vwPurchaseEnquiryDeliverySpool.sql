CREATE VIEW Task.vwPurchaseEnquiryDeliverySpool
AS
	SELECT        purchase_enquiry.TaskCode, purchase_enquiry.ContactName, Org.tbContact.NickName, Usr.tbUser.UserName, Org.tbOrg.AccountName, Org.tbAddress.Address AS InvoiceAddress, 
							 collection_account.AccountName AS CollectAccount, collection_address.Address AS CollectAddress, delivery_account.AccountName AS DeliveryAccount, delivery_address.Address AS DeliveryAddress, 
							 purchase_enquiry.AccountCode, purchase_enquiry.TaskNotes, purchase_enquiry.ActivityCode, purchase_enquiry.ActionOn, Activity.tbActivity.UnitOfMeasure, purchase_enquiry.Quantity, App.tbTaxCode.TaxCode, 
							 App.tbTaxCode.TaxRate, purchase_enquiry.UnitCharge, purchase_enquiry.TotalCharge, Usr.tbUser.MobileNumber, Usr.tbUser.Signature, purchase_enquiry.TaskTitle
	FROM            Org.tbOrg AS delivery_account INNER JOIN
							 Org.tbOrg AS collection_account INNER JOIN
							 Usr.tbUser INNER JOIN
							 Activity.tbActivity INNER JOIN
							 Task.tbTask AS purchase_enquiry ON Activity.tbActivity.ActivityCode = purchase_enquiry.ActivityCode INNER JOIN
							 Org.tbOrg ON purchase_enquiry.AccountCode = Org.tbOrg.AccountCode LEFT OUTER JOIN
							 Org.tbAddress ON Org.tbOrg.AddressCode = Org.tbAddress.AddressCode ON Usr.tbUser.UserId = purchase_enquiry.ActionById INNER JOIN
							 Org.tbAddress AS delivery_address ON purchase_enquiry.AddressCodeTo = delivery_address.AddressCode LEFT OUTER JOIN
							 App.tbTaxCode ON purchase_enquiry.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
							 Org.tbContact ON purchase_enquiry.ContactName = Org.tbContact.ContactName AND purchase_enquiry.AccountCode = Org.tbContact.AccountCode INNER JOIN
							 Org.tbAddress AS collection_address ON purchase_enquiry.AddressCodeFrom = collection_address.AddressCode ON collection_account.AccountCode = collection_address.AccountCode ON 
							 delivery_account.AccountCode = delivery_address.AccountCode
	WHERE EXISTS (SELECT * FROM App.tbDocSpool AS doc WHERE DocTypeCode = 2 AND UserName = SUSER_SNAME() AND purchase_enquiry.TaskCode = doc.DocumentNumber);
