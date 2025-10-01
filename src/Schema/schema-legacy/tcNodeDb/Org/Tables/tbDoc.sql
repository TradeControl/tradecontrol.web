CREATE TABLE [Org].[tbDoc] (
    [AccountCode]         NVARCHAR (10)  NOT NULL,
    [DocumentName]        NVARCHAR (255) NOT NULL,
    [DocumentDescription] NTEXT          NULL,
    [DocumentImage]       IMAGE          NULL,
    [InsertedBy]          NVARCHAR (50)  CONSTRAINT [DF_Org_tbDoc_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]          DATETIME       CONSTRAINT [DF_Org_tbDoc_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]           NVARCHAR (50)  CONSTRAINT [DF_Org_tbDoc_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]           DATETIME       CONSTRAINT [DF_Org_tbDoc_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]              ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Org_tbDoc] PRIMARY KEY NONCLUSTERED ([AccountCode] ASC, [DocumentName] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Org_tbDoc_AccountCode] FOREIGN KEY ([AccountCode]) REFERENCES [Org].[tbOrg] ([AccountCode]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Org_tbDoc_DocName_AccountCode]
    ON [Org].[tbDoc]([DocumentName] ASC, [AccountCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Org_tbDoc_AccountCode]
    ON [Org].[tbDoc]([AccountCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE   TRIGGER Org.Org_tbDoc_TriggerUpdate 
   ON  Org.tbDoc
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		UPDATE Org.tbDoc
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Org.tbDoc INNER JOIN inserted AS i ON tbDoc.AccountCode = i.AccountCode AND tbDoc.DocumentName = i.DocumentName;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
