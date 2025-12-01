CREATE TABLE [Object].[tbAttribute] (
    [ObjectCode]      NVARCHAR (50)  NOT NULL,
    [Attribute]         NVARCHAR (50)  NOT NULL,
    [PrintOrder]        SMALLINT       CONSTRAINT [DF_Object_tbAttribute_OrderBy] DEFAULT ((10)) NOT NULL,
    [AttributeTypeCode] SMALLINT       CONSTRAINT [DF_Object_tbAttribute_AttributeTypeCode] DEFAULT ((0)) NOT NULL,
    [DefaultText]       NVARCHAR (400) NULL,
    [InsertedBy]        NVARCHAR (50)  CONSTRAINT [DF_tbTemplateAttribute_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]        DATETIME       CONSTRAINT [DF_tbTemplateAttribute_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]         NVARCHAR (50)  CONSTRAINT [DF_tbTemplateAttribute_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]         DATETIME       CONSTRAINT [DF_tbTemplateAttribute_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]            ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Object_tbAttribute] PRIMARY KEY CLUSTERED ([ObjectCode] ASC, [Attribute] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Object_tbAttribute_Object_tbAttributeType] FOREIGN KEY ([AttributeTypeCode]) REFERENCES [Object].[tbAttributeType] ([AttributeTypeCode]),
    CONSTRAINT [FK_Object_tbAttribute_tbObject] FOREIGN KEY ([ObjectCode]) REFERENCES [Object].[tbObject] ([ObjectCode]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Object_tbAttribute]
    ON [Object].[tbAttribute]([Attribute] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Object_tbAttribute_DefaultText]
    ON [Object].[tbAttribute]([DefaultText] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Object_tbAttribute_OrderBy]
    ON [Object].[tbAttribute]([ObjectCode] ASC, [PrintOrder] ASC, [Attribute] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Object_tbAttribute_Type_OrderBy]
    ON [Object].[tbAttribute]([ObjectCode] ASC, [AttributeTypeCode] ASC, [PrintOrder] ASC) WITH (FILLFACTOR = 90);


GO
CREATE   TRIGGER Object.Object_tbAttribute_TriggerUpdate 
   ON  Object.tbAttribute
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		UPDATE Object.tbAttribute
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Object.tbAttribute INNER JOIN inserted AS i ON tbAttribute.ObjectCode = i.ObjectCode AND tbAttribute.Attribute = i.Attribute;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
