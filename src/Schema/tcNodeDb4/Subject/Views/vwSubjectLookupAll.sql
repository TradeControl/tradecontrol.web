CREATE   VIEW Subject.vwSubjectLookupAll
AS
	SELECT Subject.tbSubject.SubjectCode, Subject.tbSubject.SubjectName, Subject.tbSubject.SubjectTypeCode, Subject.tbType.SubjectType, Cash.tbPolarity.CashPolarity, Cash.tbPolarity.CashPolarityCode, Subject.tbSubject.SubjectStatusCode, Subject.tbStatus.SubjectStatus
	FROM Subject.tbSubject 
		JOIN Subject.tbType ON Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode
		JOIN Cash.tbPolarity ON Subject.tbType.CashPolarityCode = Cash.tbPolarity.CashPolarityCode 
		JOIN Subject.tbStatus ON Subject.tbSubject.SubjectStatusCode = Subject.tbStatus.SubjectStatusCode;

