CREATE VIEW Project.vwFlow
AS
	SELECT        Project.tbFlow.ParentProjectCode, Project.tbFlow.StepNumber, Project.tbProject.ProjectCode, Project.tbProject.ObjectCode, Project.tbProject.ProjectTitle, Project.tbProject.ProjectNotes, Project.tbStatus.ProjectStatus, Project.tbProject.ActionOn, 
							 Project.tbProject.Quantity, Project.tbProject.ActionedOn, Subject.tbSubject.SubjectCode, Usr.tbUser.UserName AS Owner, tbUser_1.UserName AS ActionBy, Subject.tbSubject.SubjectName, Project.tbProject.UnitCharge, 
							 Project.tbProject.TotalCharge, Project.tbProject.InsertedBy, Project.tbProject.InsertedOn, Project.tbProject.UpdatedBy, Project.tbProject.UpdatedOn, Project.tbProject.ProjectStatusCode
	FROM            Usr.tbUser AS tbUser_1 INNER JOIN
							 Project.tbProject INNER JOIN
							 Project.tbStatus ON Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode INNER JOIN
							 Usr.tbUser ON Project.tbProject.UserId = Usr.tbUser.UserId INNER JOIN
							 Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode ON tbUser_1.UserId = Project.tbProject.ActionById INNER JOIN
							 Project.tbFlow ON Project.tbProject.ProjectCode = Project.tbFlow.ChildProjectCode;
