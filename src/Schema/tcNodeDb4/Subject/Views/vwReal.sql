CREATE VIEW [Subject].[vwReal]
AS
    SELECT
        s.[SubjectCode],
        s.[SubjectName] AS [ContactName],
        r.[FileAs],
        r.[OnMailingList],
        r.[NameTitle],
        r.[NickName],
        r.[JobTitle],
        r.[PhoneNumber],
        r.[MobileNumber],
        r.[EmailAddress],
        r.[Hobby],
        r.[DateOfBirth],
        r.[Department],
        r.[SpouseName],
        r.[HomeNumber],
        r.[Information],
        r.[Photo],
        s.[InsertedBy],
        s.[InsertedOn],
        s.[UpdatedBy],
        s.[UpdatedOn],
        r.[RowVer],
        Subject.fnSubjectNamespace(s.[SubjectCode]) AS [SubjectNamespace]
    FROM [Subject].[tbSubject] AS s
    INNER JOIN [Subject].[tbReal] AS r
        ON r.[SubjectCode] = s.[SubjectCode]
    INNER JOIN [Subject].[tbType] AS t
        ON t.[SubjectTypeCode] = s.[SubjectTypeCode]
    WHERE t.[SubjectClassCode] = 1;
GO
