CREATE TABLE [App].[tbMonth] (
    [MonthNumber] SMALLINT      NOT NULL,
    [MonthName]   NVARCHAR (10) NOT NULL,
    CONSTRAINT [PK_App_tbMonth] PRIMARY KEY CLUSTERED ([MonthNumber] ASC) WITH (FILLFACTOR = 90)
);

