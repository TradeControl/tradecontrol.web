CREATE TABLE [Web].[tbTemplate] (
    [TemplateId]       INT            IDENTITY (1, 1) NOT NULL,
    [TemplateFileName] NVARCHAR (256) NULL,
    CONSTRAINT [PK_Web_tbTemplate] PRIMARY KEY CLUSTERED ([TemplateId] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Web_tbTemplate_TemplateFileName]
    ON [Web].[tbTemplate]([TemplateFileName] ASC);

