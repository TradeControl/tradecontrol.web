

CREATE   VIEW Project.vwAttributesForOrder
AS
SELECT        ProjectCode, Attribute, PrintOrder, AttributeDescription
FROM            Project.tbAttribute
WHERE        (AttributeTypeCode = 0);
