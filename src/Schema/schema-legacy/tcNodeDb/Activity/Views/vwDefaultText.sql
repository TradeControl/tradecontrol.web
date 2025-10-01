
CREATE   VIEW Activity.vwDefaultText
AS
SELECT TOP 100 PERCENT  DefaultText
FROM            Activity.tbAttribute
GROUP BY DefaultText
HAVING        (DefaultText IS NOT NULL)
ORDER BY DefaultText;
