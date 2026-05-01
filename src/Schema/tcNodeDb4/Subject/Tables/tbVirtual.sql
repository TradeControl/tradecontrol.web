CREATE TABLE [Subject].[tbVirtual] (
    [SubjectCode]        NVARCHAR (50)  NOT NULL,
    [NumberOfEmployees]  INT            CONSTRAINT [DF_Subject_tbVirtual_NumberOfEmployees] DEFAULT ((0)) NOT NULL,
    [CompanyNumber]      NVARCHAR (20)  NULL,
    [VatNumber]          NVARCHAR (50)  NULL,
    [EUJurisdiction]     BIT            CONSTRAINT [DF_Subject_tbVirtual_EUJurisdiction] DEFAULT ((0)) NOT NULL,
    [BusinessDescription] NVARCHAR(MAX) NULL,
    [Logo]               VARBINARY(MAX) NULL,
    [Turnover]           DECIMAL (18, 5) CONSTRAINT [DF_Subject_tbVirtual_Turnover] DEFAULT ((0)) NOT NULL,
    [WebSite]            NVARCHAR (255)  NULL,
    [SubjectSource]      NVARCHAR (100)  NULL,
    [RowVer]             ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Subject_tbVirtual] PRIMARY KEY NONCLUSTERED ([SubjectCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Subject_tbVirtual_tbSubject] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO
CREATE TRIGGER [Subject].[Subject_tbVirtual_TriggerUpdate]
ON [Subject].[tbVirtual]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
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
