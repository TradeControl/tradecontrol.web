CREATE TABLE [Web].[tbTemplateImage] (
    [TemplateId] INT           NOT NULL,
    [ImageTag]   NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_Web_tbTemplateImage] PRIMARY KEY CLUSTERED ([TemplateId] ASC, [ImageTag] ASC),
    CONSTRAINT [FK_tbTemplateImage_tbImage] FOREIGN KEY ([ImageTag]) REFERENCES [Web].[tbImage] ([ImageTag]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbTemplateImage_tbTemplate] FOREIGN KEY ([TemplateId]) REFERENCES [Web].[tbTemplate] ([TemplateId]) ON DELETE CASCADE ON UPDATE CASCADE
);

