CREATE   FUNCTION Invoice.fnEditProjects (@InvoiceNumber nvarchar(20), @SubjectCode nvarchar(10))
RETURNS TABLE
AS
	RETURN 
	(		
		WITH InvoiceEditProjects AS 
		(	SELECT        ProjectCode
			FROM            Invoice.tbProject
			WHERE        (InvoiceNumber = @InvoiceNumber)
		)
		SELECT TOP (100) PERCENT Project.tbProject.ProjectCode, Project.tbProject.ObjectCode, Project.tbStatus.ProjectStatus, Usr.tbUser.UserName, Project.tbProject.ActionOn, Project.tbProject.ActionedOn, Project.tbProject.ProjectTitle
		FROM            Usr.tbUser INNER JOIN
								Project.tbProject INNER JOIN
								Project.tbStatus ON Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode ON Usr.tbUser.UserId = Project.tbProject.ActionById LEFT OUTER JOIN
								InvoiceEditProjects ON Project.tbProject.ProjectCode = InvoiceEditProjects.ProjectCode
		WHERE        (Project.tbProject.SubjectCode = @SubjectCode) AND (Project.tbProject.ProjectStatusCode = 1 OR
								Project.tbProject.ProjectStatusCode = 2) AND (Project.tbProject.CashCode IS NOT NULL) AND (InvoiceEditProjects.ProjectCode IS NULL)
		ORDER BY Project.tbProject.ActionOn DESC
	);
