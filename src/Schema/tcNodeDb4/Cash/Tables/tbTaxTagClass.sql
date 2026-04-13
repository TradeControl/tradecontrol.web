CREATE TABLE [Cash].[tbTaxTagClass]
(
    [TagClassCode] TINYINT       NOT NULL,
    [TagClass]     NVARCHAR(20)  NOT NULL,

    CONSTRAINT [PK_Cash_tbTaxTagClass]
        PRIMARY KEY CLUSTERED ([TagClassCode] ASC)
);
GO
