CREATE TABLE [App].[tbUoc] (
    [UnitOfCharge] NVARCHAR (5)   NOT NULL,
    [UocSymbol]    NVARCHAR (10)  NOT NULL,
    [UocName]      NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_tbTag] PRIMARY KEY CLUSTERED ([UnitOfCharge] ASC)
);

