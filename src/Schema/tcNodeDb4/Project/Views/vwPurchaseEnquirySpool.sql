CREATE VIEW Project.vwPurchaseEnquirySpool
AS
SELECT        purchase_enquiry.ProjectCode, purchase_enquiry.ContactName, Subject.tbContact.NickName, Usr.tbUser.UserName, Subject.tbSubject.SubjectName, Subject.tbAddress.Address AS InvoiceAddress, 
                         Subject_tbAddress_1.Address AS DeliveryAddress, purchase_enquiry.SubjectCode, purchase_enquiry.ProjectNotes, purchase_enquiry.ObjectCode, purchase_enquiry.ActionOn, Object.tbObject.UnitOfMeasure, 
                         purchase_enquiry.Quantity, App.tbTaxCode.TaxCode, App.tbTaxCode.TaxRate, purchase_enquiry.UnitCharge, purchase_enquiry.TotalCharge, Usr.tbUser.MobileNumber, Usr.tbUser.Signature, 
                         purchase_enquiry.ProjectTitle
FROM            Usr.tbUser INNER JOIN
                         Object.tbObject INNER JOIN
                         Project.tbProject AS purchase_enquiry ON Object.tbObject.ObjectCode = purchase_enquiry.ObjectCode INNER JOIN
                         Subject.tbSubject ON purchase_enquiry.SubjectCode = Subject.tbSubject.SubjectCode LEFT OUTER JOIN
                         Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode ON Usr.tbUser.UserId = purchase_enquiry.ActionById LEFT OUTER JOIN
                         Subject.tbAddress AS Subject_tbAddress_1 ON purchase_enquiry.AddressCodeTo = Subject_tbAddress_1.AddressCode LEFT OUTER JOIN
                         App.tbTaxCode ON purchase_enquiry.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
                         Subject.tbContact ON purchase_enquiry.SubjectCode = Subject.tbContact.SubjectCode AND purchase_enquiry.ContactName = Subject.tbContact.ContactName
WHERE EXISTS (SELECT * FROM App.tbDocSpool AS doc WHERE DocTypeCode = 2 AND UserName = SUSER_SNAME() AND purchase_enquiry.ProjectCode = doc.DocumentNumber);
