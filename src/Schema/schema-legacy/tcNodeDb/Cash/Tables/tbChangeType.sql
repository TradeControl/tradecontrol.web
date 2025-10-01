CREATE TABLE [Cash].[tbChangeType] (
    [ChangeTypeCode] SMALLINT      NOT NULL,
    [ChangeType]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Cash_tbChangeType] PRIMARY KEY CLUSTERED ([ChangeTypeCode] ASC)
);

