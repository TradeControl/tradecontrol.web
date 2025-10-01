CREATE TABLE [Org].[tbStatus] (
    [OrganisationStatusCode] SMALLINT       CONSTRAINT [DF_Org_tbStatus_OrganisationStatusCode] DEFAULT ((1)) NOT NULL,
    [OrganisationStatus]     NVARCHAR (255) NULL,
    CONSTRAINT [PK_Org_tbStatus] PRIMARY KEY NONCLUSTERED ([OrganisationStatusCode] ASC) WITH (FILLFACTOR = 90)
);

