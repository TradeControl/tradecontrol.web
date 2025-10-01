

CREATE   VIEW Task.vwAttributesForOrder
AS
SELECT        TaskCode, Attribute, PrintOrder, AttributeDescription
FROM            Task.tbAttribute
WHERE        (AttributeTypeCode = 0);
