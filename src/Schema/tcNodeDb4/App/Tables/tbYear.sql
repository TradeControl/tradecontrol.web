CREATE TABLE [App].[tbYear] (
    [YearNumber]     SMALLINT      NOT NULL,
    [StartMonth]     SMALLINT      CONSTRAINT [DF_App_tbYear_StartMonth] DEFAULT ((1)) NOT NULL,
    [CashStatusCode] SMALLINT      CONSTRAINT [DF_App_tbYear_CashStatusCode] DEFAULT ((1)) NOT NULL,
    [Description]    NVARCHAR (10) NOT NULL,
    [InsertedBy]     NVARCHAR (50) CONSTRAINT [DF_App_tbYear_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]     DATETIME      CONSTRAINT [DF_App_tbYear_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]         ROWVERSION    NOT NULL,
    CONSTRAINT [PK_App_tbYear] PRIMARY KEY CLUSTERED ([YearNumber] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_App_tbYear_App_tbMonth] FOREIGN KEY ([StartMonth]) REFERENCES [App].[tbMonth] ([MonthNumber])
);

