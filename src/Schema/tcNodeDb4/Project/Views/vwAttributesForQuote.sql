

CREATE   VIEW Project.vwAttributesForQuote
AS
SELECT        ProjectCode, Attribute, PrintOrder, AttributeDescription
FROM            Project.tbAttribute
WHERE        (AttributeTypeCode = 1);
