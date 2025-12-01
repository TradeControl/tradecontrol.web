CREATE TABLE [Subject].[tbAccountKey] (
    [AccountCode] NVARCHAR (10)       NOT NULL,
    [HDPath]          [sys].[hierarchyid] NOT NULL,
    [KeyName]         NVARCHAR (50)       NOT NULL,
    [HDLevel]         AS                  ([HDPath].[GetLevel]()),
    CONSTRAINT [PK_Subject_tbAccountKey] PRIMARY KEY NONCLUSTERED ([AccountCode] ASC, [HDPath] ASC),
    CONSTRAINT [FK_Subject_tbAccountKey_Subject_tbAccount] FOREIGN KEY ([AccountCode]) REFERENCES [Subject].[tbAccount] ([AccountCode]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Subject_tbAccountKey_HDLevel]
    ON [Subject].[tbAccountKey]([AccountCode] ASC, [HDLevel] ASC, [HDPath] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Subject_tbAccountKey_KeyName]
    ON [Subject].[tbAccountKey]([AccountCode] ASC, [KeyName] ASC);

