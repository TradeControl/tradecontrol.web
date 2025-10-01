CREATE TABLE [App].[tbEth] (
    [NetworkProvider]   NVARCHAR (200) NOT NULL,
    [PublicKey]         NVARCHAR (42)  NOT NULL,
    [PrivateKey]        NVARCHAR (64)  NULL,
    [ConsortiumAddress] NVARCHAR (42)  NULL,
    CONSTRAINT [PK_App_tbEth] PRIMARY KEY CLUSTERED ([NetworkProvider] ASC)
);

