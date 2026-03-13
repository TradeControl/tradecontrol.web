CREATE TABLE [App].[tbTemplate] (
    [TemplateName]        NVARCHAR (100) NOT NULL,
    [StoredProcedure]     NVARCHAR (100) NOT NULL,
    [TemplateDescription] NVARCHAR (MAX) NULL,
    [IsVatRegistered]     BIT NOT NULL CONSTRAINT [DF_App_tbTemplate_IsVatRegistered] DEFAULT (0),
    CONSTRAINT [PK_App_tbTemplateName] PRIMARY KEY CLUSTERED ([TemplateName] ASC)
);
