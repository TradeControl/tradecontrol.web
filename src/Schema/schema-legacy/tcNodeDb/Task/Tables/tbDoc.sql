CREATE TABLE [Task].[tbDoc] (
    [TaskCode]            NVARCHAR (20)  NOT NULL,
    [DocumentName]        NVARCHAR (255) NOT NULL,
    [DocumentDescription] NTEXT          NULL,
    [DocumentImage]       IMAGE          NOT NULL,
    [InsertedBy]          NVARCHAR (50)  CONSTRAINT [DF_Task_tbDoc_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]          DATETIME       CONSTRAINT [DF_Task_tbDoc_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]           NVARCHAR (50)  CONSTRAINT [DF_Task_tbDoc_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]           DATETIME       CONSTRAINT [DF_Task_tbDoc_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]              ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Task_tbDoc] PRIMARY KEY CLUSTERED ([TaskCode] ASC, [DocumentName] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Task_tbDoc_Task_tb] FOREIGN KEY ([TaskCode]) REFERENCES [Task].[tbTask] ([TaskCode])
);


GO
CREATE   TRIGGER Task.Task_tbDoc_TriggerUpdate 
   ON  Task.tbDoc
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		UPDATE Task.tbDoc
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Task.tbDoc INNER JOIN inserted AS i ON tbDoc.TaskCode = i.TaskCode AND tbDoc.DocumentName = i.DocumentName;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
