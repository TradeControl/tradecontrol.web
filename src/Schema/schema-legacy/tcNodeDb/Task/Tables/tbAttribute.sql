CREATE TABLE [Task].[tbAttribute] (
    [TaskCode]             NVARCHAR (20)  NOT NULL,
    [Attribute]            NVARCHAR (50)  NOT NULL,
    [PrintOrder]           SMALLINT       CONSTRAINT [DF_Task_tbAttribute_OrderBy] DEFAULT ((10)) NOT NULL,
    [AttributeTypeCode]    SMALLINT       CONSTRAINT [DF_Task_tbAttribute_AttributeTypeCode] DEFAULT ((0)) NOT NULL,
    [AttributeDescription] NVARCHAR (400) NULL,
    [InsertedBy]           NVARCHAR (50)  CONSTRAINT [DF_tbJobAttribute_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]           DATETIME       CONSTRAINT [DF_tbJobAttribute_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]            NVARCHAR (50)  CONSTRAINT [DF_tbJobAttribute_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]            DATETIME       CONSTRAINT [DF_tbJobAttribute_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]               ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Task_tbTaskAttribute] PRIMARY KEY CLUSTERED ([TaskCode] ASC, [Attribute] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Task_tbAttrib_Task_tb] FOREIGN KEY ([TaskCode]) REFERENCES [Task].[tbTask] ([TaskCode]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Task_tbAttribute_Activity_tbAttributeType] FOREIGN KEY ([AttributeTypeCode]) REFERENCES [Activity].[tbAttributeType] ([AttributeTypeCode])
);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tbAttribute]
    ON [Task].[tbAttribute]([TaskCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tbAttribute_Description]
    ON [Task].[tbAttribute]([Attribute] ASC, [AttributeDescription] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tbAttribute_OrderBy]
    ON [Task].[tbAttribute]([TaskCode] ASC, [PrintOrder] ASC, [Attribute] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tbAttribute_Type_OrderBy]
    ON [Task].[tbAttribute]([TaskCode] ASC, [AttributeTypeCode] ASC, [PrintOrder] ASC) WITH (FILLFACTOR = 90);


GO
CREATE   TRIGGER Task.Task_tbAttribute_TriggerUpdate 
   ON  Task.tbAttribute
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		UPDATE Task.tbAttribute
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Task.tbAttribute INNER JOIN inserted AS i ON tbAttribute.TaskCode = i.TaskCode AND tbAttribute.Attribute = i.Attribute;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
