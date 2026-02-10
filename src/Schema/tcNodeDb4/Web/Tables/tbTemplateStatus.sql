CREATE TABLE [Web].[tbTemplateStatus] (
    [TemplateStatusCode] SMALLINT      NOT NULL,
    [TemplateStatus]     NVARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_Web_tbTemplateStatus] PRIMARY KEY CLUSTERED ([TemplateStatusCode] ASC)
);
