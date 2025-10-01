CREATE TABLE [Org].[tbAccountType] (
    [AccountTypeCode] SMALLINT      NOT NULL,
    [AccountType]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Org_tbAccountType] PRIMARY KEY CLUSTERED ([AccountTypeCode] ASC)
);

