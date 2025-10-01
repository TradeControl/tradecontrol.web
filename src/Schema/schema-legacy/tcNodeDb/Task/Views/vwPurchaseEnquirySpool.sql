CREATE VIEW Task.vwPurchaseEnquirySpool
AS
SELECT        purchase_enquiry.TaskCode, purchase_enquiry.ContactName, Org.tbContact.NickName, Usr.tbUser.UserName, Org.tbOrg.AccountName, Org.tbAddress.Address AS InvoiceAddress, 
                         Org_tbAddress_1.Address AS DeliveryAddress, purchase_enquiry.AccountCode, purchase_enquiry.TaskNotes, purchase_enquiry.ActivityCode, purchase_enquiry.ActionOn, Activity.tbActivity.UnitOfMeasure, 
                         purchase_enquiry.Quantity, App.tbTaxCode.TaxCode, App.tbTaxCode.TaxRate, purchase_enquiry.UnitCharge, purchase_enquiry.TotalCharge, Usr.tbUser.MobileNumber, Usr.tbUser.Signature, 
                         purchase_enquiry.TaskTitle
FROM            Usr.tbUser INNER JOIN
                         Activity.tbActivity INNER JOIN
                         Task.tbTask AS purchase_enquiry ON Activity.tbActivity.ActivityCode = purchase_enquiry.ActivityCode INNER JOIN
                         Org.tbOrg ON purchase_enquiry.AccountCode = Org.tbOrg.AccountCode LEFT OUTER JOIN
                         Org.tbAddress ON Org.tbOrg.AddressCode = Org.tbAddress.AddressCode ON Usr.tbUser.UserId = purchase_enquiry.ActionById LEFT OUTER JOIN
                         Org.tbAddress AS Org_tbAddress_1 ON purchase_enquiry.AddressCodeTo = Org_tbAddress_1.AddressCode LEFT OUTER JOIN
                         App.tbTaxCode ON purchase_enquiry.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
                         Org.tbContact ON purchase_enquiry.AccountCode = Org.tbContact.AccountCode AND purchase_enquiry.ContactName = Org.tbContact.ContactName
WHERE EXISTS (SELECT * FROM App.tbDocSpool AS doc WHERE DocTypeCode = 2 AND UserName = SUSER_SNAME() AND purchase_enquiry.TaskCode = doc.DocumentNumber);
