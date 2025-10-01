
CREATE   VIEW Object.vwDefaultText
AS
SELECT TOP 100 PERCENT  DefaultText
FROM            Object.tbAttribute
GROUP BY DefaultText
HAVING        (DefaultText IS NOT NULL)
ORDER BY DefaultText;
