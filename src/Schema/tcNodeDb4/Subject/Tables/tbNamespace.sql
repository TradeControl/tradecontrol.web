CREATE TABLE [Subject].[tbNamespace] (
    [ParentSubjectCode] NVARCHAR (50) NOT NULL,
    [ChildSubjectCode]  NVARCHAR (50) NOT NULL,
    [Ordinal]           INT NOT NULL,
    [IsDefault]         BIT CONSTRAINT [DF_Subject_tbNamespace_IsDefault] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Subject_tbNamespace] PRIMARY KEY CLUSTERED ([ParentSubjectCode] ASC, [ChildSubjectCode] ASC) WITH (FILLFACTOR = 90)
);
GO

CREATE NONCLUSTERED INDEX [IX_Subject_tbNamespace_Child]
    ON [Subject].[tbNamespace]([ChildSubjectCode] ASC) WITH (FILLFACTOR = 90);
GO

CREATE NONCLUSTERED INDEX [IX_Subject_tbNamespace_Parent]
    ON [Subject].[tbNamespace]([ParentSubjectCode] ASC) WITH (FILLFACTOR = 90);
GO

CREATE NONCLUSTERED INDEX [IX_Subject_tbNamespace_Parent_IsDefault]
    ON [Subject].[tbNamespace]([ParentSubjectCode] ASC, [IsDefault] ASC) WITH (FILLFACTOR = 90);
GO

CREATE TRIGGER [Subject].[Subject_tbNamespace_TriggerInsertUpdate]
ON [Subject].[tbNamespace]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF EXISTS
        (
            SELECT 1
            FROM inserted AS i
            WHERE i.ParentSubjectCode = i.ChildSubjectCode
        )
        BEGIN
            RAISERROR ('ParentSubjectCode and ChildSubjectCode cannot be the same.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF EXISTS
        (
            SELECT 1
            FROM inserted AS i
            LEFT JOIN Subject.tbSubject AS p
                ON p.SubjectCode = i.ParentSubjectCode
            LEFT JOIN Subject.tbSubject AS c
                ON c.SubjectCode = i.ChildSubjectCode
            WHERE p.SubjectCode IS NULL
               OR c.SubjectCode IS NULL
        )
        BEGIN
            RAISERROR ('Namespace relationships must reference existing Subjects.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        UPDATE n
        SET IsDefault = 0
        FROM Subject.tbNamespace AS n
        JOIN
        (
            SELECT DISTINCT
                i.ParentSubjectCode,
                i.ChildSubjectCode
            FROM inserted AS i
            WHERE i.IsDefault <> 0
        ) AS d
            ON d.ParentSubjectCode = n.ParentSubjectCode
        WHERE n.ChildSubjectCode <> d.ChildSubjectCode
          AND n.IsDefault <> 0;

        UPDATE n
        SET IsDefault = 1
        FROM Subject.tbNamespace AS n
        JOIN
        (
            SELECT ParentSubjectCode
            FROM Subject.tbNamespace
            GROUP BY ParentSubjectCode
            HAVING COUNT(*) = 1
        ) AS s
            ON s.ParentSubjectCode = n.ParentSubjectCode
        WHERE n.IsDefault = 0;

        IF EXISTS
        (
            SELECT n.ParentSubjectCode
            FROM Subject.tbNamespace AS n
            JOIN
            (
                SELECT ParentSubjectCode FROM inserted
                UNION
                SELECT ParentSubjectCode FROM deleted
            ) AS a
                ON a.ParentSubjectCode = n.ParentSubjectCode
            WHERE n.IsDefault <> 0
            GROUP BY n.ParentSubjectCode
            HAVING COUNT(*) > 1
        )
        BEGIN
            RAISERROR ('Only one default namespace child is permitted for each parent Subject.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
    END TRY
    BEGIN CATCH
        EXEC App.proc_ErrorLog;
    END CATCH
END
GO
