CREATE TABLE [Subject].[tbDoc] (
    [SubjectCode]         NVARCHAR (10)  NOT NULL,
    [DocumentName]        NVARCHAR (255) NOT NULL,
    [DocumentDescription] NTEXT          NULL,
    [DocumentImage]       IMAGE          NULL,
    [InsertedBy]          NVARCHAR (50)  CONSTRAINT [DF_Subject_tbDoc_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]          DATETIME       CONSTRAINT [DF_Subject_tbDoc_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]           NVARCHAR (50)  CONSTRAINT [DF_Subject_tbDoc_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]           DATETIME       CONSTRAINT [DF_Subject_tbDoc_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]              ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Subject_tbDoc] PRIMARY KEY NONCLUSTERED ([SubjectCode] ASC, [DocumentName] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Subject_tbDoc_AccountCode] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Subject_tbDoc_DocName_AccountCode]
    ON [Subject].[tbDoc]([DocumentName] ASC, [SubjectCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Subject_tbDoc_AccountCode]
    ON [Subject].[tbDoc]([SubjectCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE   TRIGGER Subject.Subject_tbDoc_TriggerUpdate 
   ON  Subject.tbDoc
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		UPDATE Subject.tbDoc
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Subject.tbDoc INNER JOIN inserted AS i ON tbDoc.SubjectCode = i.SubjectCode AND tbDoc.DocumentName = i.DocumentName;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
