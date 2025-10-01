CREATE TABLE [Project].[tbDoc] (
    [ProjectCode]            NVARCHAR (20)  NOT NULL,
    [DocumentName]        NVARCHAR (255) NOT NULL,
    [DocumentDescription] NTEXT          NULL,
    [DocumentImage]       IMAGE          NOT NULL,
    [InsertedBy]          NVARCHAR (50)  CONSTRAINT [DF_Project_tbDoc_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]          DATETIME       CONSTRAINT [DF_Project_tbDoc_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]           NVARCHAR (50)  CONSTRAINT [DF_Project_tbDoc_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]           DATETIME       CONSTRAINT [DF_Project_tbDoc_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]              ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Project_tbDoc] PRIMARY KEY CLUSTERED ([ProjectCode] ASC, [DocumentName] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Project_tbDoc_Project_tb] FOREIGN KEY ([ProjectCode]) REFERENCES [Project].[tbProject] ([ProjectCode])
);


GO
CREATE   TRIGGER Project.Project_tbDoc_TriggerUpdate 
   ON  Project.tbDoc
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		UPDATE Project.tbDoc
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Project.tbDoc INNER JOIN inserted AS i ON tbDoc.ProjectCode = i.ProjectCode AND tbDoc.DocumentName = i.DocumentName;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
