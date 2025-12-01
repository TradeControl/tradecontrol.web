CREATE TABLE [App].[tbDocClass] (
    [DocClassCode] SMALLINT      NOT NULL,
    [DocClass]     NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_App_tbDocClass] PRIMARY KEY CLUSTERED ([DocClassCode] ASC)
);

