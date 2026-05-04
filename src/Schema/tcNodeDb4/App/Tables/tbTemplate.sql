CREATE TABLE [App].[tbTemplate] (
    [TemplateCode]        NVARCHAR (10)  NOT NULL,
    [TemplateName]        NVARCHAR (100) NOT NULL,
    [StoredProcedure]     NVARCHAR (100) NOT NULL,
    [TemplateDescription] NVARCHAR (MAX) NULL,
    [IsVatRegistered]     BIT            CONSTRAINT [DF_App_tbTemplate_IsVatRegistered] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_App_tbTemplate] PRIMARY KEY CLUSTERED ([TemplateCode] ASC) WITH (FILLFACTOR = 90)
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_App_tbTemplate_TemplateName]
    ON [App].[tbTemplate]([TemplateName] ASC) WITH (FILLFACTOR = 90);
GO
