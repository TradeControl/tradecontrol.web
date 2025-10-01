CREATE TABLE [Project].[tbAttribute] (
    [ProjectCode]             NVARCHAR (20)  NOT NULL,
    [Attribute]            NVARCHAR (50)  NOT NULL,
    [PrintOrder]           SMALLINT       CONSTRAINT [DF_Project_tbAttribute_OrderBy] DEFAULT ((10)) NOT NULL,
    [AttributeTypeCode]    SMALLINT       CONSTRAINT [DF_Project_tbAttribute_AttributeTypeCode] DEFAULT ((0)) NOT NULL,
    [AttributeDescription] NVARCHAR (400) NULL,
    [InsertedBy]           NVARCHAR (50)  CONSTRAINT [DF_tbJobAttribute_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]           DATETIME       CONSTRAINT [DF_tbJobAttribute_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]            NVARCHAR (50)  CONSTRAINT [DF_tbJobAttribute_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]            DATETIME       CONSTRAINT [DF_tbJobAttribute_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]               ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Project_tbProjectAttribute] PRIMARY KEY CLUSTERED ([ProjectCode] ASC, [Attribute] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Project_tbAttrib_Project_tb] FOREIGN KEY ([ProjectCode]) REFERENCES [Project].[tbProject] ([ProjectCode]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Project_tbAttribute_Object_tbAttributeType] FOREIGN KEY ([AttributeTypeCode]) REFERENCES [Object].[tbAttributeType] ([AttributeTypeCode])
);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tbAttribute]
    ON [Project].[tbAttribute]([ProjectCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tbAttribute_Description]
    ON [Project].[tbAttribute]([Attribute] ASC, [AttributeDescription] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tbAttribute_OrderBy]
    ON [Project].[tbAttribute]([ProjectCode] ASC, [PrintOrder] ASC, [Attribute] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tbAttribute_Type_OrderBy]
    ON [Project].[tbAttribute]([ProjectCode] ASC, [AttributeTypeCode] ASC, [PrintOrder] ASC) WITH (FILLFACTOR = 90);


GO
CREATE   TRIGGER Project.Project_tbAttribute_TriggerUpdate 
   ON  Project.tbAttribute
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		UPDATE Project.tbAttribute
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Project.tbAttribute INNER JOIN inserted AS i ON tbAttribute.ProjectCode = i.ProjectCode AND tbAttribute.Attribute = i.Attribute;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
