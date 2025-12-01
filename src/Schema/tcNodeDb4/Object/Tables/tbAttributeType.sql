CREATE TABLE [Object].[tbAttributeType] (
    [AttributeTypeCode] SMALLINT      NOT NULL,
    [AttributeType]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Object_tbAttributeType] PRIMARY KEY CLUSTERED ([AttributeTypeCode] ASC) WITH (FILLFACTOR = 90)
);

