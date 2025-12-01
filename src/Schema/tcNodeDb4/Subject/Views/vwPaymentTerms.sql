

CREATE   VIEW Subject.vwPaymentTerms
AS
SELECT        PaymentTerms
FROM            Subject.tbSubject
GROUP BY PaymentTerms
HAVING         LEN(ISNULL(PaymentTerms, '')) > 0;
