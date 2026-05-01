CREATE VIEW Project.vwDoc
AS
    SELECT
        Project.fnEmailAddress(p.ProjectCode) AS EmailAddress,
        p.ProjectCode,
        p.ProjectStatusCode,
        ps.ProjectStatus,
        p.ContactName,
        rv.NickName,
        u.UserName,
        s.SubjectName,
        invoiceAddress.Address AS InvoiceAddress,
        deliverySubject.SubjectName AS DeliveryAccountName,
        deliveryAddress.Address AS DeliveryAddress,
        collectionSubject.SubjectName AS CollectionAccountName,
        collectionAddress.Address AS CollectionAddress,
        p.SubjectCode,
        p.ProjectNotes,
        p.ObjectCode,
        p.ActionOn,
        o.UnitOfMeasure,
        p.Quantity,
        tax.TaxCode,
        tax.TaxRate,
        p.UnitCharge,
        p.TotalCharge,
        u.MobileNumber,
        u.Signature,
        p.ProjectTitle,
        p.PaymentOn,
        p.SecondReference,
        s.PaymentTerms
    FROM Project.tbProject AS p
    INNER JOIN Project.tbStatus AS ps
        ON ps.ProjectStatusCode = p.ProjectStatusCode
    INNER JOIN Usr.tbUser AS u
        ON u.UserId = p.ActionById
    INNER JOIN Object.tbObject AS o
        ON o.ObjectCode = p.ObjectCode
    INNER JOIN Subject.tbSubject AS s
        ON s.SubjectCode = p.SubjectCode
    LEFT JOIN Subject.tbAddress AS invoiceAddress
        ON invoiceAddress.AddressCode = s.AddressCode
    LEFT JOIN Subject.tbAddress AS deliveryAddress
        ON deliveryAddress.AddressCode = p.AddressCodeTo
    LEFT JOIN Subject.tbSubject AS deliverySubject
        ON deliverySubject.SubjectCode = deliveryAddress.SubjectCode
    LEFT JOIN Subject.tbAddress AS collectionAddress
        ON collectionAddress.AddressCode = p.AddressCodeFrom
    LEFT JOIN Subject.tbSubject AS collectionSubject
        ON collectionSubject.SubjectCode = collectionAddress.SubjectCode
    LEFT JOIN Subject.vwReal AS rv
        ON rv.SubjectCode = p.SubjectCode
       AND rv.ContactName = p.ContactName
    LEFT JOIN App.tbTaxCode AS tax
        ON tax.TaxCode = p.TaxCode;
GO
