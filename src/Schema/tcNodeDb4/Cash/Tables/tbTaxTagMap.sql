CREATE TABLE Cash.tbTaxTagMap
(
    TaxSourceCode  NVARCHAR(20)  NOT NULL,
    TagCode        NVARCHAR(64)  NOT NULL,

    MapTypeCode    TINYINT       NOT NULL,   -- 0 = Category, 1 = CashCode
    CategoryCode   NVARCHAR(10)  NOT NULL CONSTRAINT DF_Cash_tbTaxTagMap_CategoryCode DEFAULT '',
    CashCode       NVARCHAR(50)  NOT NULL CONSTRAINT DF_Cash_tbTaxTagMap_CashCode DEFAULT '',
    IsEnabled      BIT           NOT NULL CONSTRAINT DF_Cash_tbTaxTagMap_IsEnabled DEFAULT 1,

    CONSTRAINT PK_Cash_tbTaxTagMap
        PRIMARY KEY CLUSTERED (TaxSourceCode, TagCode, MapTypeCode, CategoryCode, CashCode),

    CONSTRAINT CK_Cash_tbTaxTagMap_MapType
        CHECK
        (
            (MapTypeCode = 0 AND CategoryCode <> '' AND CashCode = '') OR
            (MapTypeCode = 1 AND CashCode <> '' AND CategoryCode = '')
        ),

    CONSTRAINT FK_Cash_tbTaxTagMap_TaxTag
        FOREIGN KEY (TaxSourceCode, TagCode)
        REFERENCES Cash.tbTaxTag (TaxSourceCode, TagCode)
        ON DELETE CASCADE,

    CONSTRAINT FK_Cash_tbTaxTagMap_TaxTagMapType
        FOREIGN KEY (MapTypeCode)
        REFERENCES Cash.tbTaxTagMapType (MapTypeCode)
);
GO

CREATE TRIGGER Cash.trgTaxTagMap_Validate
ON Cash.tbTaxTagMap
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        WHERE i.MapTypeCode = 0
          AND NOT EXISTS
          (
              SELECT 1
              FROM Cash.tbCategory c
              WHERE c.CategoryCode = i.CategoryCode
          )
    )
    BEGIN
        RAISERROR ('Invalid CategoryCode for MapTypeCode = 0', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        WHERE i.MapTypeCode = 1
          AND NOT EXISTS
          (
              SELECT 1
              FROM Cash.tbCode cc
              WHERE cc.CashCode = i.CashCode
          )
    )
    BEGIN
        RAISERROR ('Invalid CashCode for MapTypeCode = 1', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;
END;
GO
