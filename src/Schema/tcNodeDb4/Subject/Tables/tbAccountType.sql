CREATE TABLE [Subject].[tbAccountType] (
    [AccountTypeCode] SMALLINT      NOT NULL,
    [AccountType]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Subject_tbAccountType] PRIMARY KEY CLUSTERED ([AccountTypeCode] ASC)
);

