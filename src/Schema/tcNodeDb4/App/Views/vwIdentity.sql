CREATE VIEW App.vwIdentity
AS
    SELECT TOP (1)
        s.SubjectName,
        a.Address,
        s.PhoneNumber,
        s.EmailAddress,
        v.WebSite,
        v.Logo,
        u.UserName,
        u.LogonName,
        u.Avatar,
        v.CompanyNumber,
        v.VatNumber,
        uoc.UocName,
        uoc.UocSymbol
    FROM App.tbOptions AS o
        INNER JOIN Subject.tbSubject AS s
            ON s.SubjectCode = o.SubjectCode
        INNER JOIN Subject.tbVirtual AS v
            ON v.SubjectCode = s.SubjectCode
        INNER JOIN App.tbUoc AS uoc
            ON uoc.UnitOfCharge = o.UnitOfCharge
        LEFT OUTER JOIN Subject.tbAddress AS a
            ON a.AddressCode = s.AddressCode
        CROSS JOIN Usr.vwCredentials AS c
        INNER JOIN Usr.tbUser AS u
            ON u.UserId = c.UserId;
GO
