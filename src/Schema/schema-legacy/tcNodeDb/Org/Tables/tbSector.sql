CREATE TABLE [Org].[tbSector] (
    [AccountCode]    NVARCHAR (10) NOT NULL,
    [IndustrySector] NVARCHAR (50) NOT NULL,
    [RowVer]         ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Org_tbSector] PRIMARY KEY CLUSTERED ([AccountCode] ASC, [IndustrySector] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Org_tbSector_Org_tb] FOREIGN KEY ([AccountCode]) REFERENCES [Org].[tbOrg] ([AccountCode]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Org_tbSector_IndustrySector]
    ON [Org].[tbSector]([IndustrySector] ASC) WITH (FILLFACTOR = 90);

