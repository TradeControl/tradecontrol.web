

CREATE   VIEW Org.vwPaymentTerms
AS
SELECT        PaymentTerms
FROM            Org.tbOrg
GROUP BY PaymentTerms
HAVING         LEN(ISNULL(PaymentTerms, '')) > 0;
