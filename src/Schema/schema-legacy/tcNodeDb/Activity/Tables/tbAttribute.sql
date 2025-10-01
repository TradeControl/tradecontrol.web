CREATE TABLE [Activity].[tbAttribute] (
    [ActivityCode]      NVARCHAR (50)  NOT NULL,
    [Attribute]         NVARCHAR (50)  NOT NULL,
    [PrintOrder]        SMALLINT       CONSTRAINT [DF_Activity_tbAttribute_OrderBy] DEFAULT ((10)) NOT NULL,
    [AttributeTypeCode] SMALLINT       CONSTRAINT [DF_Activity_tbAttribute_AttributeTypeCode] DEFAULT ((0)) NOT NULL,
    [DefaultText]       NVARCHAR (400) NULL,
    [InsertedBy]        NVARCHAR (50)  CONSTRAINT [DF_tbTemplateAttribute_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]        DATETIME       CONSTRAINT [DF_tbTemplateAttribute_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]         NVARCHAR (50)  CONSTRAINT [DF_tbTemplateAttribute_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]         DATETIME       CONSTRAINT [DF_tbTemplateAttribute_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]            ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Activity_tbAttribute] PRIMARY KEY CLUSTERED ([ActivityCode] ASC, [Attribute] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Activity_tbAttribute_Activity_tbAttributeType] FOREIGN KEY ([AttributeTypeCode]) REFERENCES [Activity].[tbAttributeType] ([AttributeTypeCode]),
    CONSTRAINT [FK_Activity_tbAttribute_tbActivity] FOREIGN KEY ([ActivityCode]) REFERENCES [Activity].[tbActivity] ([ActivityCode]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Activity_tbAttribute]
    ON [Activity].[tbAttribute]([Attribute] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Activity_tbAttribute_DefaultText]
    ON [Activity].[tbAttribute]([DefaultText] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Activity_tbAttribute_OrderBy]
    ON [Activity].[tbAttribute]([ActivityCode] ASC, [PrintOrder] ASC, [Attribute] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Activity_tbAttribute_Type_OrderBy]
    ON [Activity].[tbAttribute]([ActivityCode] ASC, [AttributeTypeCode] ASC, [PrintOrder] ASC) WITH (FILLFACTOR = 90);


GO
CREATE   TRIGGER Activity.Activity_tbAttribute_TriggerUpdate 
   ON  Activity.tbAttribute
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		UPDATE Activity.tbAttribute
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Activity.tbAttribute INNER JOIN inserted AS i ON tbAttribute.ActivityCode = i.ActivityCode AND tbAttribute.Attribute = i.Attribute;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
