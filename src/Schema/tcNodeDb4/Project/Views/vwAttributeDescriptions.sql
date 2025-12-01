
CREATE   VIEW Project.vwAttributeDescriptions
AS
SELECT        Attribute, AttributeDescription
FROM            Project.tbAttribute
GROUP BY Attribute, AttributeDescription
HAVING        (AttributeDescription IS NOT NULL);
