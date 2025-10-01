CREATE TABLE [Cash].[tbChangeStatus] (
    [ChangeStatusCode] SMALLINT      NOT NULL,
    [ChangeStatus]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Cash_tbChangeStatus] PRIMARY KEY CLUSTERED ([ChangeStatusCode] ASC)
);

