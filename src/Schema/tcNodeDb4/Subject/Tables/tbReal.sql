CREATE TABLE [Subject].[tbReal] (
    [SubjectCode]   NVARCHAR (50)  NOT NULL,
    [FileAs]        NVARCHAR (100) NULL,
    [OnMailingList] BIT            CONSTRAINT [DF_Subject_tbReal_OnMailingList] DEFAULT ((1)) NOT NULL,
    [NameTitle]     NVARCHAR (25)  NULL,
    [NickName]      NVARCHAR (100) NULL,
    [JobTitle]      NVARCHAR (100) NULL,
    [PhoneNumber]   NVARCHAR (50)  NULL,
    [MobileNumber]  NVARCHAR (50)  NULL,
    [EmailAddress]  NVARCHAR (255) NULL,
    [Hobby]         NVARCHAR (50)  NULL,
    [DateOfBirth]   DATETIME       NULL,
    [Department]    NVARCHAR (50)  NULL,
    [SpouseName]    NVARCHAR (50)  NULL,
    [HomeNumber]    NVARCHAR (50)  NULL,
    [Information]   NVARCHAR(MAX)  NULL,
    [Photo]         VARBINARY(MAX) NULL,
    [RowVer]        ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Subject_tbReal] PRIMARY KEY NONCLUSTERED ([SubjectCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Subject_tbReal_tbSubject] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO
CREATE NONCLUSTERED INDEX [IX_Subject_tbRealDepartment]
    ON [Subject].[tbReal]([Department] ASC) WITH (FILLFACTOR = 90);
GO
CREATE NONCLUSTERED INDEX [IX_Subject_tbRealJobTitle]
    ON [Subject].[tbReal]([JobTitle] ASC) WITH (FILLFACTOR = 90);
GO
CREATE NONCLUSTERED INDEX [IX_Subject_tbRealNameTitle]
    ON [Subject].[tbReal]([NameTitle] ASC) WITH (FILLFACTOR = 90);
GO
CREATE NONCLUSTERED INDEX [IX_Subject_tbReal_AccountCode]
    ON [Subject].[tbReal]([SubjectCode] ASC) WITH (FILLFACTOR = 90);
GO
CREATE TRIGGER [Subject].[Subject_tbReal_TriggerInsert]
ON [Subject].[tbReal]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        UPDATE r
        SET NickName = RTRIM(CASE
                                WHEN LEN(ISNULL(i.NickName, '')) > 0 THEN i.NickName
                                WHEN CHARINDEX(' ', s.SubjectName, 0) = 0 THEN s.SubjectName
                                ELSE LEFT(s.SubjectName, CHARINDEX(' ', s.SubjectName, 0))
                             END),
            FileAs = Subject.fnContactFileAs(s.SubjectName)
        FROM Subject.tbReal AS r
        INNER JOIN inserted AS i
            ON r.SubjectCode = i.SubjectCode
        INNER JOIN Subject.tbSubject AS s
            ON s.SubjectCode = i.SubjectCode;

        UPDATE s
        SET UpdatedBy = SUSER_SNAME(),
            UpdatedOn = CURRENT_TIMESTAMP
        FROM Subject.tbSubject AS s
        INNER JOIN inserted AS i
            ON s.SubjectCode = i.SubjectCode;
    END TRY
    BEGIN CATCH
        EXEC App.proc_ErrorLog;
    END CATCH
END
GO
CREATE TRIGGER [Subject].[Subject_tbReal_TriggerUpdate]
ON [Subject].[tbReal]
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF EXISTS (SELECT 1 FROM inserted)
        BEGIN
            UPDATE r
            SET FileAs = Subject.fnContactFileAs(s.SubjectName)
            FROM Subject.tbReal AS r
            INNER JOIN inserted AS i
                ON r.SubjectCode = i.SubjectCode
            INNER JOIN Subject.tbSubject AS s
                ON s.SubjectCode = i.SubjectCode;
        END

        UPDATE s
        SET UpdatedBy = SUSER_SNAME(),
            UpdatedOn = CURRENT_TIMESTAMP
        FROM Subject.tbSubject AS s
        INNER JOIN
        (
            SELECT SubjectCode FROM inserted
            UNION
            SELECT SubjectCode FROM deleted
        ) AS d
            ON d.SubjectCode = s.SubjectCode;
    END TRY
    BEGIN CATCH
        EXEC App.proc_ErrorLog;
    END CATCH
END
