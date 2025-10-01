CREATE TABLE [Org].[tbAccountKey] (
    [CashAccountCode] NVARCHAR (10)       NOT NULL,
    [HDPath]          [sys].[hierarchyid] NOT NULL,
    [KeyName]         NVARCHAR (50)       NOT NULL,
    [HDLevel]         AS                  ([HDPath].[GetLevel]()),
    CONSTRAINT [PK_Org_tbAccountKey] PRIMARY KEY NONCLUSTERED ([CashAccountCode] ASC, [HDPath] ASC),
    CONSTRAINT [FK_Org_tbAccountKey_Org_tbAccount] FOREIGN KEY ([CashAccountCode]) REFERENCES [Org].[tbAccount] ([CashAccountCode]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Org_tbAccountKey_HDLevel]
    ON [Org].[tbAccountKey]([CashAccountCode] ASC, [HDLevel] ASC, [HDPath] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Org_tbAccountKey_KeyName]
    ON [Org].[tbAccountKey]([CashAccountCode] ASC, [KeyName] ASC);

