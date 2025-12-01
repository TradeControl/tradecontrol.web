CREATE VIEW Object.vwUnMirrored
AS
	WITH candidates AS
	(
		SELECT DISTINCT Project.tbAllocation.SubjectCode, Subject.tbSubject.SubjectName, Project.tbAllocation.AllocationCode, Project.tbAllocation.AllocationDescription, Project.tbAllocation.CashPolarityCode, Cash.tbPolarity.CashPolarity, Project.tbAllocation.UnitCharge, Project.tbAllocation.UnitOfMeasure
		FROM            Project.tbAllocation 
			INNER JOIN Cash.tbPolarity ON Project.tbAllocation.CashPolarityCode = Cash.tbPolarity.CashPolarityCode 
			INNER JOIN Subject.tbSubject ON Project.tbAllocation.SubjectCode = Subject.tbSubject.SubjectCode 
			LEFT OUTER JOIN Object.tbMirror ON Project.tbAllocation.SubjectCode = Object.tbMirror.SubjectCode AND Project.tbAllocation.AllocationCode = Object.tbMirror.AllocationCode
		WHERE        (Object.tbMirror.ObjectCode IS NULL)
	)
	SELECT CAST(ROW_NUMBER() OVER (ORDER BY SubjectCode, AllocationCode) AS int) CandidateId,
		candidates.SubjectCode, candidates.SubjectName, candidates.AllocationCode, candidates.AllocationDescription, candidates.CashPolarityCode, candidates.CashPolarity, candidates.UnitCharge, candidates.UnitOfMeasure,
		CASE WHEN act_code.ObjectCode IS NULL THEN 0 ELSE 1 END IsObject
	FROM candidates LEFT OUTER JOIN Object.tbObject act_code ON candidates.AllocationCode = act_code.ObjectCode;
