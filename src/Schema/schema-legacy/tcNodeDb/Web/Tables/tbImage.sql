CREATE TABLE [Web].[tbImage] (
    [ImageTag]      NVARCHAR (50)  NOT NULL,
    [ImageFileName] NVARCHAR (256) NOT NULL,
    CONSTRAINT [PK_Web_tbImage] PRIMARY KEY CLUSTERED ([ImageTag] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Web_tbImage_ImageFileName]
    ON [Web].[tbImage]([ImageFileName] ASC);

