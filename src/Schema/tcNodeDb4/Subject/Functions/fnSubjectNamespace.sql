CREATE FUNCTION Subject.fnSubjectNamespace
(
	@SubjectCode nvarchar(50)
)
RETURNS nvarchar(max)
AS
BEGIN
	DECLARE @SubjectNamespace nvarchar(max);

	IF NOT EXISTS
	(
		SELECT 1
		FROM Subject.tbSubject
		WHERE SubjectCode = @SubjectCode
	)
		RETURN NULL;

	WITH namespace_path AS
	(
		SELECT
			s.SubjectCode AS CurrentSubjectCode,
			CAST(s.SubjectCode AS nvarchar(max)) AS NamespacePath
		FROM Subject.tbSubject AS s
		WHERE s.SubjectCode = @SubjectCode

		UNION ALL

		SELECT
			n.ParentSubjectCode AS CurrentSubjectCode,
			CAST(CONCAT(n.ParentSubjectCode, N'.', p.NamespacePath) AS nvarchar(max)) AS NamespacePath
		FROM Subject.tbNamespace AS n
			INNER JOIN namespace_path AS p
				ON n.ChildSubjectCode = p.CurrentSubjectCode
	)
	SELECT
		@SubjectNamespace =
			STRING_AGG(NamespacePath, N'; ')
			WITHIN GROUP (ORDER BY NamespacePath)
	from namespace_path AS p
	WHERE NOT EXISTS
	(
		SELECT 1
		FROM Subject.tbNamespace AS n
		WHERE n.ChildSubjectCode = p.CurrentSubjectCode
	);

	RETURN @SubjectNamespace;
END
GO
