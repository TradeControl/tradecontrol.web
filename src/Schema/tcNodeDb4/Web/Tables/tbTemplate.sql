CREATE TABLE [Web].[tbTemplate] (
    [TemplateId]         INT            IDENTITY (1, 1) NOT NULL,
    [TemplateFileName]   NVARCHAR (256)  NULL,

    [TemplateStatusCode] SMALLINT       CONSTRAINT [DF_Web_tbTemplate_TemplateStatusCode] DEFAULT ((0)) NOT NULL,
    [ParsedOn]           DATETIME       NULL,
    [ParseMessage]       NVARCHAR (512) NULL,

    CONSTRAINT [PK_Web_tbTemplate] PRIMARY KEY CLUSTERED ([TemplateId] ASC),
    CONSTRAINT [FK_Web_tbTemplate_tbTemplateStatus]
        FOREIGN KEY ([TemplateStatusCode]) REFERENCES [Web].[tbTemplateStatus] ([TemplateStatusCode])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Web_tbTemplate_TemplateFileName]
    ON [Web].[tbTemplate]([TemplateFileName] ASC);
