CREATE TABLE [App].[tbRegister] (
    [RegisterName] NVARCHAR (50) NOT NULL,
    [NextNumber]   INT           CONSTRAINT [DF_App_tbRegister_NextNumber] DEFAULT ((1)) NOT NULL,
    [RowVer]       ROWVERSION    NOT NULL,
    CONSTRAINT [PK_App_tbRegister] PRIMARY KEY CLUSTERED ([RegisterName] ASC) WITH (FILLFACTOR = 90)
);

