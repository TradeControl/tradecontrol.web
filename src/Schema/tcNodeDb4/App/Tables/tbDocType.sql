CREATE TABLE [App].[tbDocType] (
    [DocTypeCode]  SMALLINT      NOT NULL,
    [DocType]      NVARCHAR (50) NOT NULL,
    [DocClassCode] SMALLINT      CONSTRAINT [DF_App_tbDocType_DocClassCode] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_App_tbDocType] PRIMARY KEY CLUSTERED ([DocTypeCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_App_tbDocType_App_tbDocClass] FOREIGN KEY ([DocClassCode]) REFERENCES [App].[tbDocClass] ([DocClassCode])
);

