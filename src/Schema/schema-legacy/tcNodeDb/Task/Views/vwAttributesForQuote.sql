

CREATE   VIEW Task.vwAttributesForQuote
AS
SELECT        TaskCode, Attribute, PrintOrder, AttributeDescription
FROM            Task.tbAttribute
WHERE        (AttributeTypeCode = 1);
