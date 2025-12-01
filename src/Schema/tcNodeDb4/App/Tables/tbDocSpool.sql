CREATE TABLE [App].[tbDocSpool] (
    [UserName]       NVARCHAR (50) CONSTRAINT [DF_App_tbDocSpool_UserName] DEFAULT (suser_sname()) NOT NULL,
    [DocTypeCode]    SMALLINT      CONSTRAINT [DF_App_tbDocSpool_DocTypeCode] DEFAULT ((1)) NOT NULL,
    [DocumentNumber] NVARCHAR (25) NOT NULL,
    [SpooledOn]      DATETIME      CONSTRAINT [DF_App_tbDocSpool_SpooledOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]         ROWVERSION    NOT NULL,
    CONSTRAINT [PK_App_tbDocSpool] PRIMARY KEY CLUSTERED ([UserName] ASC, [DocTypeCode] ASC, [DocumentNumber] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_App_tbDocSpool_App_tbDocType] FOREIGN KEY ([DocTypeCode]) REFERENCES [App].[tbDocType] ([DocTypeCode])
);


GO
CREATE NONCLUSTERED INDEX [IX_App_tbDocSpool_DocTypeCode]
    ON [App].[tbDocSpool]([DocTypeCode] ASC) WITH (FILLFACTOR = 90);

