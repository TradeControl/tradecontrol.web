CREATE TABLE [Cash].[tbCategoryExprFormat] (
    [TemplateCode]        NVARCHAR (10)  NOT NULL,
    [Template]            NVARCHAR (50)  NOT NULL,
    [TemplateDescription] NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_Cash_tbCategoryExprFormat] PRIMARY KEY CLUSTERED ([TemplateCode] ASC)
);

