
CREATE   VIEW Task.vwAttributeDescriptions
AS
SELECT        Attribute, AttributeDescription
FROM            Task.tbAttribute
GROUP BY Attribute, AttributeDescription
HAVING        (AttributeDescription IS NOT NULL);
