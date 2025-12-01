CREATE VIEW App.vwGraphProjectObject
AS
SELECT        CONCAT(Project.tbStatus.ProjectStatus, SPACE(1), Cash.tbPolarity.CashPolarity) AS Category, SUM(Project.tbProject.TotalCharge) AS SumOfTotalCharge
FROM            Project.tbProject INNER JOIN
                         Project.tbStatus ON Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode INNER JOIN
                         Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
                         Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
                         Cash.tbPolarity ON Cash.tbCategory.CashPolarityCode = Cash.tbPolarity.CashPolarityCode
WHERE        (Project.tbProject.ProjectStatusCode < 3) AND (Project.tbProject.ProjectStatusCode > 0)
GROUP BY CONCAT(Project.tbStatus.ProjectStatus, SPACE(1), Cash.tbPolarity.CashPolarity);
