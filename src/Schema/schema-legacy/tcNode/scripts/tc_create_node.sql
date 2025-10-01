/**************************************************************************************
Trade Control
Node Creation Script - SCHEMA + LOGIC
Release: 4.1.1

Date: 5 August 2023
Author: IAM
Project: tcNodeDb4

Trade Control by Trade Control Ltd is licensed under GNU General Public License v3.0. 

You may obtain a copy of the License at

	https://www.gnu.org/licenses/gpl-3.0.en.html

***********************************************************************************/
go
PRINT N'Creating Schema [Web]...';


go
CREATE SCHEMA [Web]
    AUTHORIZATION [dbo];


go
PRINT N'Creating Schema [Usr]...';


go
CREATE SCHEMA [Usr]
    AUTHORIZATION [dbo];


go
PRINT N'Creating Schema [Invoice]...';


go
CREATE SCHEMA [Invoice]
    AUTHORIZATION [dbo];


go
PRINT N'Creating Schema [Cash]...';


go
CREATE SCHEMA [Cash]
    AUTHORIZATION [dbo];


go
PRINT N'Creating Schema [App]...';


go
CREATE SCHEMA [App]
    AUTHORIZATION [dbo];


go
PRINT N'Creating Schema [Object]...';


go
CREATE SCHEMA [Object]
    AUTHORIZATION [dbo];


go
PRINT N'Creating Schema [Subject]...';


go
CREATE SCHEMA [Subject]
    AUTHORIZATION [dbo];


go
PRINT N'Creating Schema [Project]...';


go
CREATE SCHEMA [Project]
    AUTHORIZATION [dbo];


go
PRINT N'Creating Table [dbo].[AspNetUserTokens]...';


go
CREATE TABLE [dbo].[AspNetUserTokens] (
    [UserId]        NVARCHAR (450) NOT NULL,
    [LoginProvider] NVARCHAR (128) NOT NULL,
    [Name]          NVARCHAR (128) NOT NULL,
    [Value]         NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED ([UserId] ASC, [LoginProvider] ASC, [Name] ASC)
);


go
PRINT N'Creating Table [dbo].[AspNetUsers]...';


go
CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                   NVARCHAR (450)     NOT NULL,
    [UserName]             NVARCHAR (256)     NULL,
    [NormalizedUserName]   NVARCHAR (256)     NULL,
    [Email]                NVARCHAR (256)     NULL,
    [NormalizedEmail]      NVARCHAR (256)     NULL,
    [EmailConfirmed]       BIT                NOT NULL,
    [PasswordHash]         NVARCHAR (MAX)     NULL,
    [SecurityStamp]        NVARCHAR (MAX)     NULL,
    [ConcurrencyStamp]     NVARCHAR (MAX)     NULL,
    [PhoneNumber]          NVARCHAR (MAX)     NULL,
    [PhoneNumberConfirmed] BIT                NOT NULL,
    [TwoFactorEnabled]     BIT                NOT NULL,
    [LockoutEnd]           DATETIMEOFFSET (7) NULL,
    [LockoutEnabled]       BIT                NOT NULL,
    [AccessFailedCount]    INT                NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
);


go
PRINT N'Creating Index [dbo].[AspNetUsers].[EmailIndex]...';


go
CREATE NONCLUSTERED INDEX [EmailIndex]
    ON [dbo].[AspNetUsers]([NormalizedEmail] ASC);


go
PRINT N'Creating Index [dbo].[AspNetUsers].[UserNameIndex]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
    ON [dbo].[AspNetUsers]([NormalizedUserName] ASC) WHERE ([NormalizedUserName] IS NOT NULL);


go
PRINT N'Creating Table [dbo].[AspNetUserRoles]...';


go
CREATE TABLE [dbo].[AspNetUserRoles] (
    [UserId] NVARCHAR (450) NOT NULL,
    [RoleId] NVARCHAR (450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC)
);


go
PRINT N'Creating Index [dbo].[AspNetUserRoles].[IX_AspNetUserRoles_RoleId]...';


go
CREATE NONCLUSTERED INDEX [IX_AspNetUserRoles_RoleId]
    ON [dbo].[AspNetUserRoles]([RoleId] ASC);


go
PRINT N'Creating Table [dbo].[AspNetUserLogins]...';


go
CREATE TABLE [dbo].[AspNetUserLogins] (
    [LoginProvider]       NVARCHAR (128) NOT NULL,
    [ProviderKey]         NVARCHAR (128) NOT NULL,
    [ProviderDisplayName] NVARCHAR (MAX) NULL,
    [UserId]              NVARCHAR (450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC)
);


go
PRINT N'Creating Index [dbo].[AspNetUserLogins].[IX_AspNetUserLogins_UserId]...';


go
CREATE NONCLUSTERED INDEX [IX_AspNetUserLogins_UserId]
    ON [dbo].[AspNetUserLogins]([UserId] ASC);


go
PRINT N'Creating Table [dbo].[AspNetUserClaims]...';


go
CREATE TABLE [dbo].[AspNetUserClaims] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [UserId]     NVARCHAR (450) NOT NULL,
    [ClaimType]  NVARCHAR (MAX) NULL,
    [ClaimValue] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED ([Id] ASC)
);


go
PRINT N'Creating Index [dbo].[AspNetUserClaims].[IX_AspNetUserClaims_UserId]...';


go
CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId]
    ON [dbo].[AspNetUserClaims]([UserId] ASC);


go
PRINT N'Creating Table [dbo].[AspNetRoles]...';


go
CREATE TABLE [dbo].[AspNetRoles] (
    [Id]               NVARCHAR (450) NOT NULL,
    [Name]             NVARCHAR (256) NULL,
    [NormalizedName]   NVARCHAR (256) NULL,
    [ConcurrencyStamp] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
);


go
PRINT N'Creating Index [dbo].[AspNetRoles].[RoleNameIndex]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex]
    ON [dbo].[AspNetRoles]([NormalizedName] ASC) WHERE ([NormalizedName] IS NOT NULL);


go
PRINT N'Creating Table [dbo].[AspNetRoleClaims]...';


go
CREATE TABLE [dbo].[AspNetRoleClaims] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [RoleId]     NVARCHAR (450) NOT NULL,
    [ClaimType]  NVARCHAR (MAX) NULL,
    [ClaimValue] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED ([Id] ASC)
);


go
PRINT N'Creating Index [dbo].[AspNetRoleClaims].[IX_AspNetRoleClaims_RoleId]...';


go
CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId]
    ON [dbo].[AspNetRoleClaims]([RoleId] ASC);


go
PRINT N'Creating Table [Web].[tbTemplateImage]...';


go
CREATE TABLE [Web].[tbTemplateImage] (
    [TemplateId] INT           NOT NULL,
    [ImageTag]   NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_Web_tbTemplateImage] PRIMARY KEY CLUSTERED ([TemplateId] ASC, [ImageTag] ASC)
);


go
PRINT N'Creating Table [Web].[tbTemplate]...';


go
CREATE TABLE [Web].[tbTemplate] (
    [TemplateId]       INT            IDENTITY (1, 1) NOT NULL,
    [TemplateFileName] NVARCHAR (256) NULL,
    CONSTRAINT [PK_Web_tbTemplate] PRIMARY KEY CLUSTERED ([TemplateId] ASC)
);


go
PRINT N'Creating Index [Web].[tbTemplate].[IX_Web_tbTemplate_TemplateFileName]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Web_tbTemplate_TemplateFileName]
    ON [Web].[tbTemplate]([TemplateFileName] ASC);


go
PRINT N'Creating Table [Web].[tbImage]...';


go
CREATE TABLE [Web].[tbImage] (
    [ImageTag]      NVARCHAR (50)  NOT NULL,
    [ImageFileName] NVARCHAR (256) NOT NULL,
    CONSTRAINT [PK_Web_tbImage] PRIMARY KEY CLUSTERED ([ImageTag] ASC)
);


go
PRINT N'Creating Index [Web].[tbImage].[IX_Web_tbImage_ImageFileName]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Web_tbImage_ImageFileName]
    ON [Web].[tbImage]([ImageFileName] ASC);


go
PRINT N'Creating Table [Web].[tbAttachmentInvoice]...';


go
CREATE TABLE [Web].[tbAttachmentInvoice] (
    [InvoiceTypeCode] SMALLINT NOT NULL,
    [AttachmentId]    INT      NOT NULL,
    CONSTRAINT [PK_Web_tbInvoiceAttachment] PRIMARY KEY CLUSTERED ([InvoiceTypeCode] ASC, [AttachmentId] ASC)
);


go
PRINT N'Creating Index [Web].[tbAttachmentInvoice].[IX_Web_tbAttachmentInvoice]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Web_tbAttachmentInvoice]
    ON [Web].[tbAttachmentInvoice]([AttachmentId] ASC, [InvoiceTypeCode] ASC);


go
PRINT N'Creating Table [Web].[tbAttachment]...';


go
CREATE TABLE [Web].[tbAttachment] (
    [AttachmentId]       INT            IDENTITY (1, 1) NOT NULL,
    [AttachmentFileName] NVARCHAR (256) NOT NULL,
    CONSTRAINT [PK_Web_tbAttachment] PRIMARY KEY CLUSTERED ([AttachmentId] ASC)
);


go
PRINT N'Creating Index [Web].[tbAttachment].[IX_Web_tbAttachment_AttachmentFileName]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Web_tbAttachment_AttachmentFileName]
    ON [Web].[tbAttachment]([AttachmentFileName] ASC);


go
PRINT N'Creating Table [Web].[tbTemplateInvoice]...';


go
CREATE TABLE [Web].[tbTemplateInvoice] (
    [InvoiceTypeCode] SMALLINT NOT NULL,
    [TemplateId]      INT      NOT NULL,
    [LastUsedOn]      DATETIME NULL,
    CONSTRAINT [PK_Web_tbTemplateInvoice] PRIMARY KEY CLUSTERED ([InvoiceTypeCode] ASC, [TemplateId] ASC)
);


go
PRINT N'Creating Index [Web].[tbTemplateInvoice].[IX_Web_tbTemplateInvoice]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Web_tbTemplateInvoice]
    ON [Web].[tbTemplateInvoice]([TemplateId] ASC, [InvoiceTypeCode] ASC);


go
PRINT N'Creating Index [Web].[tbTemplateInvoice].[IX_Web_tbTemplateInvoice_LastUsedOn]...';


go
CREATE NONCLUSTERED INDEX [IX_Web_tbTemplateInvoice_LastUsedOn]
    ON [Web].[tbTemplateInvoice]([InvoiceTypeCode] ASC, [LastUsedOn] DESC);


go
PRINT N'Creating Table [Usr].[tbUser]...';


go
CREATE TABLE [Usr].[tbUser] (
    [UserId]            NVARCHAR (10)  NOT NULL,
    [UserName]          NVARCHAR (50)  NOT NULL,
    [LogonName]         NVARCHAR (50)  NOT NULL,
    [CalendarCode]      NVARCHAR (10)  NULL,
    [PhoneNumber]       NVARCHAR (50)  NULL,
    [MobileNumber]      NVARCHAR (50)  NULL,
    [EmailAddress]      NVARCHAR (255) NULL,
    [Address]           NTEXT          NULL,
    [Avatar]            IMAGE          NULL,
    [Signature]         IMAGE          NULL,
    [IsAdministrator]   BIT            NOT NULL,
    [IsEnabled]         SMALLINT       NOT NULL,
    [NextProjectNumber] INT            NOT NULL,
    [InsertedBy]        NVARCHAR (50)  NOT NULL,
    [InsertedOn]        DATETIME       NOT NULL,
    [UpdatedBy]         NVARCHAR (50)  NOT NULL,
    [UpdatedOn]         DATETIME       NOT NULL,
    [RowVer]            ROWVERSION     NOT NULL,
    [MenuViewCode]      SMALLINT       NOT NULL,
    CONSTRAINT [PK_Usr_tbUser] PRIMARY KEY CLUSTERED ([UserId] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Usr].[tbUser].[IX_Usr_tbUser_IsEnabled_LogonName]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Usr_tbUser_IsEnabled_LogonName]
    ON [Usr].[tbUser]([IsEnabled] ASC, [LogonName] ASC);


go
PRINT N'Creating Index [Usr].[tbUser].[IX_Usr_tbUser_IsEnabled_UserName]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Usr_tbUser_IsEnabled_UserName]
    ON [Usr].[tbUser]([IsEnabled] ASC, [UserName] ASC);


go
PRINT N'Creating Index [Usr].[tbUser].[IX_Usr_tbUser_LogonName]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Usr_tbUser_LogonName]
    ON [Usr].[tbUser]([LogonName] ASC);


go
PRINT N'Creating Index [Usr].[tbUser].[IX_Usr_tbUser_UserName]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Usr_tbUser_UserName]
    ON [Usr].[tbUser]([UserName] ASC);


go
PRINT N'Creating Table [Usr].[tbMenuUser]...';


go
CREATE TABLE [Usr].[tbMenuUser] (
    [UserId] NVARCHAR (10) NOT NULL,
    [MenuId] SMALLINT      NOT NULL,
    [RowVer] ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Usr_tbMenuUser] PRIMARY KEY CLUSTERED ([UserId] ASC, [MenuId] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Usr].[tbMenuOpenMode]...';


go
CREATE TABLE [Usr].[tbMenuOpenMode] (
    [OpenMode]            SMALLINT      NOT NULL,
    [OpenModeDescription] NVARCHAR (20) NULL,
    CONSTRAINT [PK_Usr_tbMenuOpenMode] PRIMARY KEY CLUSTERED ([OpenMode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Usr].[tbMenuCommand]...';


go
CREATE TABLE [Usr].[tbMenuCommand] (
    [Command]     SMALLINT      NOT NULL,
    [CommandText] NVARCHAR (50) NULL,
    CONSTRAINT [PK_Usr_tbMenuCommand] PRIMARY KEY CLUSTERED ([Command] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Usr].[tbMenu]...';


go
CREATE TABLE [Usr].[tbMenu] (
    [MenuId]        SMALLINT      IDENTITY (1, 1) NOT NULL,
    [MenuName]      NVARCHAR (50) NOT NULL,
    [InsertedOn]    DATETIME      NOT NULL,
    [InsertedBy]    NVARCHAR (50) NOT NULL,
    [RowVer]        ROWVERSION    NOT NULL,
    [InterfaceCode] SMALLINT      NOT NULL,
    CONSTRAINT [PK_Usr_tbMenu] PRIMARY KEY CLUSTERED ([MenuId] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [IX_Usr_tbMenu] UNIQUE NONCLUSTERED ([MenuName] ASC, [MenuId] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Usr].[tbMenuView]...';


go
CREATE TABLE [Usr].[tbMenuView] (
    [MenuViewCode] SMALLINT      NOT NULL,
    [MenuView]     NVARCHAR (30) NOT NULL,
    CONSTRAINT [PK_tbMenuView] PRIMARY KEY CLUSTERED ([MenuViewCode] ASC)
);


go
PRINT N'Creating Table [Usr].[tbInterface]...';


go
CREATE TABLE [Usr].[tbInterface] (
    [InterfaceCode] SMALLINT      NOT NULL,
    [Interface]     NVARCHAR (30) NOT NULL,
    CONSTRAINT [PK_Usr_tbInterface] PRIMARY KEY CLUSTERED ([InterfaceCode] ASC)
);


go
PRINT N'Creating Table [Usr].[tbMenuEntry]...';


go
CREATE TABLE [Usr].[tbMenuEntry] (
    [MenuId]      SMALLINT       NOT NULL,
    [EntryId]     INT            IDENTITY (1, 1) NOT NULL,
    [FolderId]    SMALLINT       NOT NULL,
    [ItemId]      SMALLINT       NOT NULL,
    [ItemText]    NVARCHAR (255) NULL,
    [Command]     SMALLINT       NULL,
    [ProjectName] NVARCHAR (50)  NULL,
    [Argument]    NVARCHAR (50)  NULL,
    [OpenMode]    SMALLINT       NULL,
    [UpdatedOn]   DATETIME       NOT NULL,
    [InsertedOn]  DATETIME       NOT NULL,
    [UpdatedBy]   NVARCHAR (50)  NOT NULL,
    [RowVer]      ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Usr_tbMenuEntry] PRIMARY KEY CLUSTERED ([MenuId] ASC, [EntryId] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [IX_Usr_tbMenuEntry_MenuFolderItem] UNIQUE NONCLUSTERED ([MenuId] ASC, [FolderId] ASC, [ItemId] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Usr].[tbMenuEntry].[IX_Usr_tbMenuEntry_Command]...';


go
CREATE NONCLUSTERED INDEX [IX_Usr_tbMenuEntry_Command]
    ON [Usr].[tbMenuEntry]([Command] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Usr].[tbMenuEntry].[IX_Usr_tbMenuEntry_OpenMode]...';


go
CREATE NONCLUSTERED INDEX [IX_Usr_tbMenuEntry_OpenMode]
    ON [Usr].[tbMenuEntry]([OpenMode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Table [Invoice].[tbMirrorEvent]...';


go
CREATE TABLE [Invoice].[tbMirrorEvent] (
    [ContractAddress]   NVARCHAR (42)   NOT NULL,
    [LogId]             INT             IDENTITY (1, 1) NOT NULL,
    [EventTypeCode]     SMALLINT        NULL,
    [InvoiceStatusCode] SMALLINT        NULL,
    [DueOn]             DATETIME        NULL,
    [InsertedOn]        DATETIME        NOT NULL,
    [RowVer]            ROWVERSION      NOT NULL,
    [PaidValue]         DECIMAL (18, 5) NOT NULL,
    [PaidTaxValue]      DECIMAL (18, 5) NOT NULL,
    [PaymentAddress]    NVARCHAR (42)   NULL,
    CONSTRAINT [PK_Invoice_tbMirrorEvent] PRIMARY KEY CLUSTERED ([ContractAddress] ASC, [LogId] ASC)
);


go
PRINT N'Creating Index [Invoice].[tbMirrorEvent].[IX_Invoice_tbMirrorEvent_EventTypeCode]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Invoice_tbMirrorEvent_EventTypeCode]
    ON [Invoice].[tbMirrorEvent]([EventTypeCode] ASC, [InvoiceStatusCode] ASC, [InsertedOn] ASC);


go
PRINT N'Creating Table [Invoice].[tbMirrorItem]...';


go
CREATE TABLE [Invoice].[tbMirrorItem] (
    [ContractAddress]   NVARCHAR (42)   NOT NULL,
    [ChargeCode]        NVARCHAR (50)   NOT NULL,
    [ChargeDescription] NVARCHAR (100)  NULL,
    [TaxCode]           NVARCHAR (10)   NULL,
    [RowVer]            ROWVERSION      NOT NULL,
    [InvoiceValue]      DECIMAL (18, 5) NOT NULL,
    [TaxValue]          DECIMAL (18, 5) NOT NULL,
    CONSTRAINT [PK_Invoice_tbMirrorItem] PRIMARY KEY CLUSTERED ([ContractAddress] ASC, [ChargeCode] ASC)
);


go
PRINT N'Creating Index [Invoice].[tbMirrorItem].[IX_Invoice_tbMirrorItem_InvoiceNumber]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbMirrorItem_InvoiceNumber]
    ON [Invoice].[tbMirrorItem]([ChargeCode] ASC, [ContractAddress] ASC);


go
PRINT N'Creating Table [Invoice].[tbChangeLog]...';


go
CREATE TABLE [Invoice].[tbChangeLog] (
    [InvoiceNumber]      NVARCHAR (20)   NOT NULL,
    [LogId]              INT             IDENTITY (1, 1) NOT NULL,
    [ChangedOn]          DATETIME        NOT NULL,
    [TransmitStatusCode] SMALLINT        NOT NULL,
    [InvoiceStatusCode]  SMALLINT        NOT NULL,
    [DueOn]              DATETIME        NOT NULL,
    [UpdatedBy]          NVARCHAR (50)   NOT NULL,
    [RowVer]             ROWVERSION      NOT NULL,
    [InvoiceValue]       DECIMAL (18, 5) NOT NULL,
    [TaxValue]           DECIMAL (18, 5) NOT NULL,
    [PaidValue]          DECIMAL (18, 5) NOT NULL,
    [PaidTaxValue]       DECIMAL (18, 5) NOT NULL,
    CONSTRAINT [PK_Invoice_tbChangeLog] PRIMARY KEY CLUSTERED ([InvoiceNumber] ASC, [LogId] DESC)
);


go
PRINT N'Creating Index [Invoice].[tbChangeLog].[IX_Invoice_tbChangeLog_LogId]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Invoice_tbChangeLog_LogId]
    ON [Invoice].[tbChangeLog]([LogId] DESC);


go
PRINT N'Creating Index [Invoice].[tbChangeLog].[IX_Invoice_tbChangeLog_ChangedOn]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbChangeLog_ChangedOn]
    ON [Invoice].[tbChangeLog]([ChangedOn] DESC);


go
PRINT N'Creating Index [Invoice].[tbChangeLog].[IX_Invoice_tbChangeLog_TransmitStatus]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbChangeLog_TransmitStatus]
    ON [Invoice].[tbChangeLog]([TransmitStatusCode] ASC, [ChangedOn] ASC);


go
PRINT N'Creating Table [Invoice].[tbType]...';


go
CREATE TABLE [Invoice].[tbType] (
    [InvoiceTypeCode]  SMALLINT      NOT NULL,
    [InvoiceType]      NVARCHAR (20) NOT NULL,
    [CashPolarityCode] SMALLINT      NOT NULL,
    [NextNumber]       INT           NOT NULL,
    [RowVer]           ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Invoice_tbType] PRIMARY KEY CLUSTERED ([InvoiceTypeCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Invoice].[tbMirrorReference]...';


go
CREATE TABLE [Invoice].[tbMirrorReference] (
    [ContractAddress] NVARCHAR (42) NOT NULL,
    [InvoiceNumber]   NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Invoice_tbMirrorReference] PRIMARY KEY CLUSTERED ([ContractAddress] ASC)
);


go
PRINT N'Creating Index [Invoice].[tbMirrorReference].[IX_Invoice_tbMirrorReference_InvoiceNumber]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Invoice_tbMirrorReference_InvoiceNumber]
    ON [Invoice].[tbMirrorReference]([InvoiceNumber] ASC)
    INCLUDE([ContractAddress]);


go
PRINT N'Creating Table [Invoice].[tbProject]...';


go
CREATE TABLE [Invoice].[tbProject] (
    [InvoiceNumber] NVARCHAR (20)   NOT NULL,
    [ProjectCode]   NVARCHAR (20)   NOT NULL,
    [CashCode]      NVARCHAR (50)   NOT NULL,
    [TaxCode]       NVARCHAR (10)   NULL,
    [RowVer]        ROWVERSION      NOT NULL,
    [Quantity]      DECIMAL (18, 4) NOT NULL,
    [TotalValue]    DECIMAL (18, 5) NOT NULL,
    [InvoiceValue]  DECIMAL (18, 5) NOT NULL,
    [TaxValue]      DECIMAL (18, 5) NOT NULL,
    CONSTRAINT [PK_Invoice_tbProject] PRIMARY KEY CLUSTERED ([InvoiceNumber] ASC, [ProjectCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Invoice].[tbProject].[IX_Invoice_tbProject_CashCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbProject_CashCode]
    ON [Invoice].[tbProject]([CashCode] ASC, [InvoiceNumber] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Invoice].[tbProject].[IX_Invoice_tbProject_Full]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbProject_Full]
    ON [Invoice].[tbProject]([InvoiceNumber] ASC, [CashCode] ASC, [InvoiceValue] ASC, [TaxValue] ASC, [TaxCode] ASC);


go
PRINT N'Creating Index [Invoice].[tbProject].[IX_Invoice_tbProject_InvoiceNumber_TaxCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbProject_InvoiceNumber_TaxCode]
    ON [Invoice].[tbProject]([InvoiceNumber] ASC, [TaxCode] ASC)
    INCLUDE([CashCode], [InvoiceValue], [TaxValue]);


go
PRINT N'Creating Index [Invoice].[tbProject].[IX_Invoice_tbProject_ProjectCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbProject_ProjectCode]
    ON [Invoice].[tbProject]([ProjectCode] ASC, [InvoiceNumber] ASC)
    INCLUDE([InvoiceValue], [TaxValue]);


go
PRINT N'Creating Index [Invoice].[tbProject].[IX_Invoice_tbProject_TaxCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbProject_TaxCode]
    ON [Invoice].[tbProject]([TaxCode] ASC)
    INCLUDE([InvoiceValue], [TaxValue]);


go
PRINT N'Creating Table [Invoice].[tbStatus]...';


go
CREATE TABLE [Invoice].[tbStatus] (
    [InvoiceStatusCode] SMALLINT      NOT NULL,
    [InvoiceStatus]     NVARCHAR (50) NULL,
    CONSTRAINT [PK_Invoice_tbStatus] PRIMARY KEY NONCLUSTERED ([InvoiceStatusCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Invoice].[tbItem]...';


go
CREATE TABLE [Invoice].[tbItem] (
    [InvoiceNumber] NVARCHAR (20)   NOT NULL,
    [CashCode]      NVARCHAR (50)   NOT NULL,
    [TaxCode]       NVARCHAR (10)   NULL,
    [ItemReference] NTEXT           NULL,
    [RowVer]        ROWVERSION      NOT NULL,
    [TotalValue]    DECIMAL (18, 5) NOT NULL,
    [InvoiceValue]  DECIMAL (18, 5) NOT NULL,
    [TaxValue]      DECIMAL (18, 5) NOT NULL,
    CONSTRAINT [PK_Invoice_tbItem] PRIMARY KEY CLUSTERED ([InvoiceNumber] ASC, [CashCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Invoice].[tbItem].[IX_Invoice_tbItem_Full]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbItem_Full]
    ON [Invoice].[tbItem]([InvoiceNumber] ASC, [CashCode] ASC, [InvoiceValue] ASC, [TaxValue] ASC, [TaxCode] ASC);


go
PRINT N'Creating Index [Invoice].[tbItem].[IX_Invoice_tbItem_InvoiceNumber_TaxCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbItem_InvoiceNumber_TaxCode]
    ON [Invoice].[tbItem]([InvoiceNumber] ASC, [TaxCode] ASC)
    INCLUDE([CashCode], [InvoiceValue], [TaxValue]);


go
PRINT N'Creating Index [Invoice].[tbItem].[IX_Invoice_tbItem_CashCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbItem_CashCode]
    ON [Invoice].[tbItem]([CashCode] ASC, [InvoiceNumber] ASC)
    INCLUDE([InvoiceValue], [TaxValue]);


go
PRINT N'Creating Index [Invoice].[tbItem].[IX_Invoice_tbItem_TaxCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbItem_TaxCode]
    ON [Invoice].[tbItem]([TaxCode] ASC)
    INCLUDE([InvoiceValue], [TaxValue]);


go
PRINT N'Creating Table [Invoice].[tbInvoice]...';


go
CREATE TABLE [Invoice].[tbInvoice] (
    [InvoiceNumber]     NVARCHAR (20)   NOT NULL,
    [UserId]            NVARCHAR (10)   NOT NULL,
    [SubjectCode]       NVARCHAR (10)   NOT NULL,
    [InvoiceTypeCode]   SMALLINT        NOT NULL,
    [InvoiceStatusCode] SMALLINT        NOT NULL,
    [InvoicedOn]        DATETIME        NOT NULL,
    [ExpectedOn]        DATETIME        NOT NULL,
    [DueOn]             DATETIME        NOT NULL,
    [PaymentTerms]      NVARCHAR (100)  NULL,
    [Notes]             NTEXT           NULL,
    [Printed]           BIT             NOT NULL,
    [Spooled]           BIT             NOT NULL,
    [RowVer]            ROWVERSION      NOT NULL,
    [InvoiceValue]      DECIMAL (18, 5) NOT NULL,
    [TaxValue]          DECIMAL (18, 5) NOT NULL,
    [PaidValue]         DECIMAL (18, 5) NOT NULL,
    [PaidTaxValue]      DECIMAL (18, 5) NOT NULL,
    CONSTRAINT [PK_Invoice_tbInvoicePK] PRIMARY KEY CLUSTERED ([InvoiceNumber] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Invoice].[tbInvoice].[IX_Invoice_tb_AccountCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tb_AccountCode]
    ON [Invoice].[tbInvoice]([SubjectCode] ASC, [InvoicedOn] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Invoice].[tbInvoice].[IX_Invoice_tb_Status]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tb_Status]
    ON [Invoice].[tbInvoice]([InvoiceStatusCode] ASC, [InvoicedOn] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Invoice].[tbInvoice].[IX_Invoice_tb_UserId]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tb_UserId]
    ON [Invoice].[tbInvoice]([UserId] ASC, [InvoiceNumber] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Invoice].[tbInvoice].[IX_Invoice_tbInvoice_AccountCode_DueOn]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbInvoice_AccountCode_DueOn]
    ON [Invoice].[tbInvoice]([SubjectCode] ASC, [InvoiceTypeCode] ASC, [DueOn] ASC)
    INCLUDE([InvoiceNumber]);


go
PRINT N'Creating Index [Invoice].[tbInvoice].[IX_Invoice_tbInvoice_AccountCode_Status]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbInvoice_AccountCode_Status]
    ON [Invoice].[tbInvoice]([SubjectCode] ASC, [InvoiceStatusCode] ASC, [InvoiceNumber] ASC);


go
PRINT N'Creating Index [Invoice].[tbInvoice].[IX_Invoice_tbInvoice_AccountCode_Type]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbInvoice_AccountCode_Type]
    ON [Invoice].[tbInvoice]([SubjectCode] ASC, [InvoiceNumber] ASC, [InvoiceTypeCode] ASC)
    INCLUDE([InvoiceValue], [TaxValue]);


go
PRINT N'Creating Index [Invoice].[tbInvoice].[IX_Invoice_tbInvoice_AccountValues]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbInvoice_AccountValues]
    ON [Invoice].[tbInvoice]([SubjectCode] ASC, [InvoiceStatusCode] ASC, [InvoiceNumber] ASC)
    INCLUDE([InvoiceValue], [TaxValue]);


go
PRINT N'Creating Index [Invoice].[tbInvoice].[IX_Invoice_tbInvoice_ExpectedOn]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbInvoice_ExpectedOn]
    ON [Invoice].[tbInvoice]([ExpectedOn] ASC, [InvoiceTypeCode] ASC, [InvoiceStatusCode] ASC);


go
PRINT N'Creating Index [Invoice].[tbInvoice].[IX_Invoice_tbInvoice_FlowInitialise]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbInvoice_FlowInitialise]
    ON [Invoice].[tbInvoice]([InvoiceTypeCode] ASC, [UserId] ASC, [InvoiceStatusCode] ASC, [SubjectCode] ASC, [InvoiceNumber] ASC, [InvoicedOn] ASC, [PaymentTerms] ASC, [Printed] ASC);


go
PRINT N'Creating Table [Invoice].[tbEntry]...';


go
CREATE TABLE [Invoice].[tbEntry] (
    [UserId]          NVARCHAR (10)   NOT NULL,
    [SubjectCode]     NVARCHAR (10)   NOT NULL,
    [CashCode]        NVARCHAR (50)   NOT NULL,
    [InvoiceTypeCode] SMALLINT        NOT NULL,
    [InvoicedOn]      DATETIME        NOT NULL,
    [TaxCode]         NVARCHAR (10)   NULL,
    [ItemReference]   NTEXT           NULL,
    [TotalValue]      DECIMAL (18, 5) NOT NULL,
    [InvoiceValue]    DECIMAL (18, 5) NOT NULL,
    [RowVer]          ROWVERSION      NOT NULL,
    CONSTRAINT [PK_Invoice_tbEntry] PRIMARY KEY CLUSTERED ([SubjectCode] ASC, [CashCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Invoice].[tbEntry].[IX_Invoice_tbEntry_UserId]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbEntry_UserId]
    ON [Invoice].[tbEntry]([UserId] ASC);


go
PRINT N'Creating Table [Invoice].[tbMirrorProject]...';


go
CREATE TABLE [Invoice].[tbMirrorProject] (
    [ContractAddress] NVARCHAR (42)   NOT NULL,
    [ProjectCode]     NVARCHAR (20)   NOT NULL,
    [Quantity]        DECIMAL (18, 4) NOT NULL,
    [TaxCode]         NVARCHAR (10)   NULL,
    [RowVer]          ROWVERSION      NULL,
    [InvoiceValue]    DECIMAL (18, 5) NOT NULL,
    [TaxValue]        DECIMAL (18, 5) NOT NULL,
    CONSTRAINT [PK_Invoice_tbMirrorProject] PRIMARY KEY CLUSTERED ([ContractAddress] ASC, [ProjectCode] ASC)
);


go
PRINT N'Creating Index [Invoice].[tbMirrorProject].[IX_Invoice_tbMirrorProject_ProjectCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Invoice_tbMirrorProject_ProjectCode]
    ON [Invoice].[tbMirrorProject]([ProjectCode] ASC, [ContractAddress] ASC);


go
PRINT N'Creating Table [Invoice].[tbMirror]...';


go
CREATE TABLE [Invoice].[tbMirror] (
    [ContractAddress]   NVARCHAR (42)   NOT NULL,
    [SubjectCode]       NVARCHAR (10)   NOT NULL,
    [InvoiceNumber]     NVARCHAR (50)   NOT NULL,
    [InvoiceTypeCode]   SMALLINT        NOT NULL,
    [InvoiceStatusCode] SMALLINT        NOT NULL,
    [InvoicedOn]        DATETIME        NOT NULL,
    [DueOn]             DATETIME        NOT NULL,
    [UnitOfCharge]      NVARCHAR (5)    NULL,
    [PaymentTerms]      NVARCHAR (100)  NULL,
    [InsertedOn]        DATETIME        NOT NULL,
    [RowVer]            ROWVERSION      NOT NULL,
    [InvoiceValue]      DECIMAL (18, 5) NOT NULL,
    [InvoiceTax]        DECIMAL (18, 5) NOT NULL,
    [PaidValue]         DECIMAL (18, 5) NOT NULL,
    [PaidTaxValue]      DECIMAL (18, 5) NOT NULL,
    [PaymentAddress]    NVARCHAR (42)   NULL,
    CONSTRAINT [PK_Invoice_tbMirror] PRIMARY KEY CLUSTERED ([ContractAddress] ASC)
);


go
PRINT N'Creating Index [Invoice].[tbMirror].[IX_Invoice_tbMirror_InvoiceNumber]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Invoice_tbMirror_InvoiceNumber]
    ON [Invoice].[tbMirror]([SubjectCode] ASC, [InvoiceNumber] ASC);


go
PRINT N'Creating Table [Cash].[tbTx]...';


go
CREATE TABLE [Cash].[tbTx] (
    [TxNumber]       INT             IDENTITY (1, 1) NOT NULL,
    [PaymentAddress] NVARCHAR (42)   NOT NULL,
    [TxId]           NVARCHAR (64)   NOT NULL,
    [TransactedOn]   DATETIME        NOT NULL,
    [TxStatusCode]   SMALLINT        NOT NULL,
    [MoneyIn]        DECIMAL (18, 5) NOT NULL,
    [MoneyOut]       DECIMAL (18, 5) NOT NULL,
    [Confirmations]  INT             NOT NULL,
    [TxMessage]      NVARCHAR (50)   NULL,
    [InsertedBy]     NVARCHAR (50)   NOT NULL,
    CONSTRAINT [PK_Cash_tbTx] PRIMARY KEY CLUSTERED ([TxNumber] ASC)
);


go
PRINT N'Creating Index [Cash].[tbTx].[IX_Cash_tbTx_PaymentAddress]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Cash_tbTx_PaymentAddress]
    ON [Cash].[tbTx]([PaymentAddress] ASC, [TxId] ASC);


go
PRINT N'Creating Index [Cash].[tbTx].[IX_Cash_tbTx_TxStatusCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Cash_tbTx_TxStatusCode]
    ON [Cash].[tbTx]([TxStatusCode] ASC, [TransactedOn] ASC);


go
PRINT N'Creating Table [Cash].[tbTxStatus]...';


go
CREATE TABLE [Cash].[tbTxStatus] (
    [TxStatusCode] SMALLINT      NOT NULL,
    [TxStatus]     NVARCHAR (10) NOT NULL,
    CONSTRAINT [PK_Cash_tbTxStatus] PRIMARY KEY CLUSTERED ([TxStatusCode] ASC)
);


go
PRINT N'Creating Table [Cash].[tbChangeReference]...';


go
CREATE TABLE [Cash].[tbChangeReference] (
    [PaymentAddress] NVARCHAR (42) NOT NULL,
    [InvoiceNumber]  NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Cash_tbChangeReference] PRIMARY KEY CLUSTERED ([PaymentAddress] ASC)
);


go
PRINT N'Creating Index [Cash].[tbChangeReference].[IX_Cash_tbChangeReference_InvoiceNumber]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Cash_tbChangeReference_InvoiceNumber]
    ON [Cash].[tbChangeReference]([InvoiceNumber] ASC);


go
PRINT N'Creating Table [Cash].[tbTxReference]...';


go
CREATE TABLE [Cash].[tbTxReference] (
    [TxNumber]     INT           NOT NULL,
    [TxStatusCode] SMALLINT      NOT NULL,
    [PaymentCode]  NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Cash_tbTxReference] PRIMARY KEY CLUSTERED ([TxNumber] ASC, [TxStatusCode] ASC)
);


go
PRINT N'Creating Index [Cash].[tbTxReference].[IX_Cash_tbTxReference_PaymentCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Cash_tbTxReference_PaymentCode]
    ON [Cash].[tbTxReference]([PaymentCode] ASC, [TxNumber] ASC);


go
PRINT N'Creating Table [Cash].[tbAssetType]...';


go
CREATE TABLE [Cash].[tbAssetType] (
    [AssetTypeCode] SMALLINT      NOT NULL,
    [AssetType]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Cash_tbAssetType] PRIMARY KEY CLUSTERED ([AssetTypeCode] ASC)
);


go
PRINT N'Creating Table [Cash].[tbCode]...';


go
CREATE TABLE [Cash].[tbCode] (
    [CashCode]        NVARCHAR (50)  NOT NULL,
    [CashDescription] NVARCHAR (100) NOT NULL,
    [CategoryCode]    NVARCHAR (10)  NOT NULL,
    [TaxCode]         NVARCHAR (10)  NOT NULL,
    [IsEnabled]       SMALLINT       NOT NULL,
    [InsertedBy]      NVARCHAR (50)  NOT NULL,
    [InsertedOn]      DATETIME       NOT NULL,
    [UpdatedBy]       NVARCHAR (50)  NOT NULL,
    [UpdatedOn]       DATETIME       NOT NULL,
    [RowVer]          ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Cash_tbCode] PRIMARY KEY CLUSTERED ([CashCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [IX_Cash_tbCodeDescription] UNIQUE NONCLUSTERED ([CashDescription] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Cash].[tbCode].[IX_Cash_tbCode_Category_IsEnabled_Code]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Cash_tbCode_Category_IsEnabled_Code]
    ON [Cash].[tbCode]([CategoryCode] ASC, [IsEnabled] ASC, [CashCode] ASC);


go
PRINT N'Creating Index [Cash].[tbCode].[IX_Cash_tbCode_IsEnabled_Code]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Cash_tbCode_IsEnabled_Code]
    ON [Cash].[tbCode]([IsEnabled] ASC, [CashCode] ASC);


go
PRINT N'Creating Index [Cash].[tbCode].[IX_Cash_tbCode_IsEnabled_Description]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Cash_tbCode_IsEnabled_Description]
    ON [Cash].[tbCode]([IsEnabled] ASC, [CashDescription] ASC);


go
PRINT N'Creating Table [Cash].[tbCategoryType]...';


go
CREATE TABLE [Cash].[tbCategoryType] (
    [CategoryTypeCode] SMALLINT      NOT NULL,
    [CategoryType]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Cash_tbCategoryType] PRIMARY KEY CLUSTERED ([CategoryTypeCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Cash].[tbCategoryTotal]...';


go
CREATE TABLE [Cash].[tbCategoryTotal] (
    [ParentCode] NVARCHAR (10) NOT NULL,
    [ChildCode]  NVARCHAR (10) NOT NULL,
    [RowVer]     ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Cash_tbCategoryTotal] PRIMARY KEY CLUSTERED ([ParentCode] ASC, [ChildCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Cash].[tbCategoryExp]...';


go
CREATE TABLE [Cash].[tbCategoryExp] (
    [CategoryCode] NVARCHAR (10)  NOT NULL,
    [Expression]   NVARCHAR (256) NOT NULL,
    [Format]       NVARCHAR (100) NOT NULL,
    [RowVer]       ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Cash_tbCategoryExp] PRIMARY KEY CLUSTERED ([CategoryCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Cash].[tbCategory]...';


go
CREATE TABLE [Cash].[tbCategory] (
    [CategoryCode]     NVARCHAR (10) NOT NULL,
    [Category]         NVARCHAR (50) NOT NULL,
    [CategoryTypeCode] SMALLINT      NOT NULL,
    [CashPolarityCode] SMALLINT      NULL,
    [CashTypeCode]     SMALLINT      NULL,
    [DisplayOrder]     SMALLINT      NOT NULL,
    [IsEnabled]        SMALLINT      NOT NULL,
    [InsertedBy]       NVARCHAR (50) NOT NULL,
    [InsertedOn]       DATETIME      NOT NULL,
    [UpdatedBy]        NVARCHAR (50) NOT NULL,
    [UpdatedOn]        DATETIME      NOT NULL,
    [RowVer]           ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Cash_tbCategory] PRIMARY KEY CLUSTERED ([CategoryCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Cash].[tbCategory].[IX_Cash_tbCategory_DisplayOrder]...';


go
CREATE NONCLUSTERED INDEX [IX_Cash_tbCategory_DisplayOrder]
    ON [Cash].[tbCategory]([DisplayOrder] ASC, [Category] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Cash].[tbCategory].[IX_Cash_tbCategory_IsEnabled_Category]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Cash_tbCategory_IsEnabled_Category]
    ON [Cash].[tbCategory]([IsEnabled] ASC, [Category] ASC);


go
PRINT N'Creating Index [Cash].[tbCategory].[IX_Cash_tbCategory_IsEnabled_CategoryCode]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Cash_tbCategory_IsEnabled_CategoryCode]
    ON [Cash].[tbCategory]([IsEnabled] ASC, [CategoryCode] ASC);


go
PRINT N'Creating Index [Cash].[tbCategory].[IX_Cash_tbCategory_Name]...';


go
CREATE NONCLUSTERED INDEX [IX_Cash_tbCategory_Name]
    ON [Cash].[tbCategory]([Category] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Cash].[tbCategory].[IX_Cash_tbCategory_TypeCategory]...';


go
CREATE NONCLUSTERED INDEX [IX_Cash_tbCategory_TypeCategory]
    ON [Cash].[tbCategory]([CategoryTypeCode] ASC, [Category] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Cash].[tbCategory].[IX_Cash_tbCategory_TypeOrderCategory]...';


go
CREATE NONCLUSTERED INDEX [IX_Cash_tbCategory_TypeOrderCategory]
    ON [Cash].[tbCategory]([CategoryTypeCode] ASC, [DisplayOrder] ASC, [Category] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Table [Cash].[tbPaymentStatus]...';


go
CREATE TABLE [Cash].[tbPaymentStatus] (
    [PaymentStatusCode] SMALLINT      NOT NULL,
    [PaymentStatus]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Cash_tbPaymentStatus] PRIMARY KEY CLUSTERED ([PaymentStatusCode] ASC)
);


go
PRINT N'Creating Table [Cash].[tbChangeStatus]...';


go
CREATE TABLE [Cash].[tbChangeStatus] (
    [ChangeStatusCode] SMALLINT      NOT NULL,
    [ChangeStatus]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Cash_tbChangeStatus] PRIMARY KEY CLUSTERED ([ChangeStatusCode] ASC)
);


go
PRINT N'Creating Table [Cash].[tbChangeType]...';


go
CREATE TABLE [Cash].[tbChangeType] (
    [ChangeTypeCode] SMALLINT      NOT NULL,
    [ChangeType]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Cash_tbChangeType] PRIMARY KEY CLUSTERED ([ChangeTypeCode] ASC)
);


go
PRINT N'Creating Table [Cash].[tbType]...';


go
CREATE TABLE [Cash].[tbType] (
    [CashTypeCode] SMALLINT      NOT NULL,
    [CashType]     NVARCHAR (25) NULL,
    CONSTRAINT [PK_Cash_tbType] PRIMARY KEY CLUSTERED ([CashTypeCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Cash].[tbStatus]...';


go
CREATE TABLE [Cash].[tbStatus] (
    [CashStatusCode] SMALLINT      NOT NULL,
    [CashStatus]     NVARCHAR (15) NOT NULL,
    CONSTRAINT [PK_Cash_tbStatus] PRIMARY KEY CLUSTERED ([CashStatusCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Cash].[tbPeriod]...';


go
CREATE TABLE [Cash].[tbPeriod] (
    [CashCode]      NVARCHAR (50)   NOT NULL,
    [StartOn]       DATETIME        NOT NULL,
    [Note]          NTEXT           NULL,
    [RowVer]        ROWVERSION      NOT NULL,
    [InvoiceValue]  DECIMAL (18, 5) NOT NULL,
    [InvoiceTax]    DECIMAL (18, 5) NOT NULL,
    [ForecastValue] DECIMAL (18, 5) NOT NULL,
    [ForecastTax]   DECIMAL (18, 5) NOT NULL,
    CONSTRAINT [PK_Cash_tbPeriod] PRIMARY KEY CLUSTERED ([CashCode] ASC, [StartOn] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Cash].[tbPolarity]...';


go
CREATE TABLE [Cash].[tbPolarity] (
    [CashPolarityCode] SMALLINT      NOT NULL,
    [CashPolarity]     NVARCHAR (10) NULL,
    CONSTRAINT [PK_Cash_tbPolarity] PRIMARY KEY CLUSTERED ([CashPolarityCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Cash].[tbEntryType]...';


go
CREATE TABLE [Cash].[tbEntryType] (
    [CashEntryTypeCode] SMALLINT      NOT NULL,
    [CashEntryType]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Cash_tbEntryType] PRIMARY KEY CLUSTERED ([CashEntryTypeCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Cash].[tbCoinType]...';


go
CREATE TABLE [Cash].[tbCoinType] (
    [CoinTypeCode] SMALLINT      NOT NULL,
    [CoinType]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Cash_tbCoinType] PRIMARY KEY CLUSTERED ([CoinTypeCode] ASC)
);


go
PRINT N'Creating Table [Cash].[tbMirror]...';


go
CREATE TABLE [Cash].[tbMirror] (
    [CashCode]           NVARCHAR (50) NOT NULL,
    [SubjectCode]        NVARCHAR (10) NOT NULL,
    [ChargeCode]         NVARCHAR (50) NOT NULL,
    [TransmitStatusCode] SMALLINT      NOT NULL,
    [InsertedBy]         NVARCHAR (50) NOT NULL,
    [InsertedOn]         DATETIME      NOT NULL,
    [UpdatedBy]          NVARCHAR (50) NOT NULL,
    [UpdatedOn]          DATETIME      NOT NULL,
    [RowVer]             ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Cash_tbMirror] PRIMARY KEY CLUSTERED ([CashCode] ASC, [SubjectCode] ASC, [ChargeCode] ASC)
);


go
PRINT N'Creating Index [Cash].[tbMirror].[IX_Cash_tbMirror_ChargeCode]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Cash_tbMirror_ChargeCode]
    ON [Cash].[tbMirror]([SubjectCode] ASC, [ChargeCode] ASC)
    INCLUDE([CashCode]);


go
PRINT N'Creating Index [Cash].[tbMirror].[IX_Cash_tbMirror_TransmitStatusCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Cash_tbMirror_TransmitStatusCode]
    ON [Cash].[tbMirror]([TransmitStatusCode] ASC, [ChargeCode] ASC);


go
PRINT N'Creating Table [Cash].[tbTaxType]...';


go
CREATE TABLE [Cash].[tbTaxType] (
    [TaxTypeCode]    SMALLINT      NOT NULL,
    [TaxType]        NVARCHAR (20) NOT NULL,
    [CashCode]       NVARCHAR (50) NULL,
    [MonthNumber]    SMALLINT      NOT NULL,
    [RecurrenceCode] SMALLINT      NOT NULL,
    [SubjectCode]    NVARCHAR (10) NULL,
    [OffsetDays]     SMALLINT      NOT NULL,
    [RowVer]         ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Cash_tbTaxType] PRIMARY KEY CLUSTERED ([TaxTypeCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Cash].[tbTaxType].[IX_tbTaxType_CashCode]...';


go
CREATE NONCLUSTERED INDEX [IX_tbTaxType_CashCode]
    ON [Cash].[tbTaxType]([CashCode] ASC);


go
PRINT N'Creating Table [Cash].[tbChange]...';


go
CREATE TABLE [Cash].[tbChange] (
    [PaymentAddress]   NVARCHAR (42)       NOT NULL,
    [AccountCode]      NVARCHAR (10)       NOT NULL,
    [HDPath]           [sys].[hierarchyid] NOT NULL,
    [ChangeTypeCode]   SMALLINT            NOT NULL,
    [ChangeStatusCode] SMALLINT            NOT NULL,
    [AddressIndex]     INT                 NOT NULL,
    [Note]             NVARCHAR (256)      NULL,
    [UpdatedOn]        DATETIME            NOT NULL,
    [UpdatedBy]        NVARCHAR (50)       NOT NULL,
    [InsertedOn]       DATETIME            NOT NULL,
    [InsertedBy]       NVARCHAR (50)       NOT NULL,
    [RowVer]           ROWVERSION          NOT NULL,
    CONSTRAINT [PK_Cash_tbChange] PRIMARY KEY CLUSTERED ([PaymentAddress] ASC)
);


go
PRINT N'Creating Index [Cash].[tbChange].[IX_Cash_tbChange_ChangeTypeCode]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Cash_tbChange_ChangeTypeCode]
    ON [Cash].[tbChange]([AccountCode] ASC, [HDPath] ASC, [ChangeTypeCode] ASC, [ChangeStatusCode] ASC, [AddressIndex] ASC);


go
PRINT N'Creating Index [Cash].[tbChange].[IX_Cash_tbChange_ChangeStatusCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Cash_tbChange_ChangeStatusCode]
    ON [Cash].[tbChange]([AccountCode] ASC, [ChangeStatusCode] ASC, [AddressIndex] ASC);


go
PRINT N'Creating Index [Cash].[tbChange].[IX_Cash_tbChange_UpdatedOn]...';


go
CREATE NONCLUSTERED INDEX [IX_Cash_tbChange_UpdatedOn]
    ON [Cash].[tbChange]([AccountCode] ASC, [HDPath] ASC, [UpdatedOn] DESC);


go
PRINT N'Creating Table [Cash].[tbPayment]...';


go
CREATE TABLE [Cash].[tbPayment] (
    [PaymentCode]       NVARCHAR (20)   NOT NULL,
    [UserId]            NVARCHAR (10)   NOT NULL,
    [PaymentStatusCode] SMALLINT        NOT NULL,
    [SubjectCode]       NVARCHAR (10)   NOT NULL,
    [AccountCode]       NVARCHAR (10)   NOT NULL,
    [CashCode]          NVARCHAR (50)   NULL,
    [TaxCode]           NVARCHAR (10)   NULL,
    [PaidOn]            DATETIME        NOT NULL,
    [PaidInValue]       DECIMAL (18, 5) NOT NULL,
    [PaidOutValue]      DECIMAL (18, 5) NOT NULL,
    [PaymentReference]  NVARCHAR (50)   NULL,
    [InsertedBy]        NVARCHAR (50)   NOT NULL,
    [InsertedOn]        DATETIME        NOT NULL,
    [UpdatedBy]         NVARCHAR (50)   NOT NULL,
    [UpdatedOn]         DATETIME        NOT NULL,
    [RowVer]            ROWVERSION      NOT NULL,
    [IsProfitAndLoss]   BIT             NOT NULL,
    CONSTRAINT [PK_Cash_tbPayment] PRIMARY KEY CLUSTERED ([PaymentCode] ASC)
);


go
PRINT N'Creating Index [Cash].[tbPayment].[IX_Cash_tbPayment]...';


go
CREATE NONCLUSTERED INDEX [IX_Cash_tbPayment]
    ON [Cash].[tbPayment]([PaymentReference] ASC);


go
PRINT N'Creating Index [Cash].[tbPayment].[IX_Cash_tbPayment_AccountCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Cash_tbPayment_AccountCode]
    ON [Cash].[tbPayment]([SubjectCode] ASC, [PaidOn] DESC);


go
PRINT N'Creating Index [Cash].[tbPayment].[IX_Cash_tbPayment_CashAccountCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Cash_tbPayment_CashAccountCode]
    ON [Cash].[tbPayment]([AccountCode] ASC, [PaidOn] ASC);


go
PRINT N'Creating Index [Cash].[tbPayment].[IX_Cash_tbPayment_CashCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Cash_tbPayment_CashCode]
    ON [Cash].[tbPayment]([CashCode] ASC, [PaidOn] ASC);


go
PRINT N'Creating Index [Cash].[tbPayment].[IX_Cash_tbPayment_PaymentCode_Status]...';


go
CREATE NONCLUSTERED INDEX [IX_Cash_tbPayment_PaymentCode_Status]
    ON [Cash].[tbPayment]([SubjectCode] ASC, [PaymentStatusCode] ASC, [PaymentCode] ASC)
    INCLUDE([PaidInValue], [PaidOutValue]);


go
PRINT N'Creating Index [Cash].[tbPayment].[IX_Cash_tbPayment_PaymentCode_TaxCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Cash_tbPayment_PaymentCode_TaxCode]
    ON [Cash].[tbPayment]([SubjectCode] ASC, [PaymentCode] ASC, [TaxCode] ASC)
    INCLUDE([PaymentStatusCode], [PaidInValue], [PaidOutValue]);


go
PRINT N'Creating Index [Cash].[tbPayment].[IX_Cash_tbPayment_Status]...';


go
CREATE NONCLUSTERED INDEX [IX_Cash_tbPayment_Status]
    ON [Cash].[tbPayment]([PaymentStatusCode] ASC)
    INCLUDE([AccountCode], [CashCode], [PaidOn], [PaidInValue], [PaidOutValue]);


go
PRINT N'Creating Index [Cash].[tbPayment].[IX_Cash_tbPayment_Status_AccountCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Cash_tbPayment_Status_AccountCode]
    ON [Cash].[tbPayment]([PaymentStatusCode] ASC, [SubjectCode] ASC);


go
PRINT N'Creating Index [Cash].[tbPayment].[IX_Cash_tbPayment_Status_CashAccount_PaidOn]...';


go
CREATE NONCLUSTERED INDEX [IX_Cash_tbPayment_Status_CashAccount_PaidOn]
    ON [Cash].[tbPayment]([PaymentStatusCode] ASC, [AccountCode] ASC, [PaidOn] ASC)
    INCLUDE([PaymentCode], [PaidInValue], [PaidOutValue]);


go
PRINT N'Creating Index [Cash].[tbPayment].[IX_tbPayment_TaxCode]...';


go
CREATE NONCLUSTERED INDEX [IX_tbPayment_TaxCode]
    ON [Cash].[tbPayment]([TaxCode] ASC)
    INCLUDE([PaidInValue], [PaidOutValue]);


go
PRINT N'Creating Table [App].[tbBucket]...';


go
CREATE TABLE [App].[tbBucket] (
    [Period]            SMALLINT      NOT NULL,
    [BucketId]          NVARCHAR (10) NOT NULL,
    [BucketDescription] NVARCHAR (50) NULL,
    [AllowForecasts]    BIT           NOT NULL,
    [RowVer]            ROWVERSION    NOT NULL,
    CONSTRAINT [PK_App_tbBucket] PRIMARY KEY CLUSTERED ([Period] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [App].[tbTemplate]...';


go
CREATE TABLE [App].[tbTemplate] (
    [TemplateName]    NVARCHAR (100) NOT NULL,
    [StoredProcedure] NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_App_tbTemplateName] PRIMARY KEY CLUSTERED ([TemplateName] ASC)
);


go
PRINT N'Creating Table [App].[tbEventType]...';


go
CREATE TABLE [App].[tbEventType] (
    [EventTypeCode] SMALLINT      NOT NULL,
    [EventType]     NVARCHAR (15) NOT NULL,
    CONSTRAINT [PK_tbFeedLogEventCode] PRIMARY KEY CLUSTERED ([EventTypeCode] ASC)
);


go
PRINT N'Creating Table [App].[tbEventLog]...';


go
CREATE TABLE [App].[tbEventLog] (
    [LogCode]       NVARCHAR (20)  NOT NULL,
    [LoggedOn]      DATETIME       NOT NULL,
    [EventTypeCode] SMALLINT       NOT NULL,
    [EventMessage]  NVARCHAR (MAX) NULL,
    [InsertedBy]    NVARCHAR (50)  NOT NULL,
    [RowVer]        ROWVERSION     NOT NULL,
    CONSTRAINT [PK_App_tbEventLog_LogCode] PRIMARY KEY CLUSTERED ([LogCode] ASC)
);


go
PRINT N'Creating Index [App].[tbEventLog].[IX_App_tbEventLog_EventType]...';


go
CREATE NONCLUSTERED INDEX [IX_App_tbEventLog_EventType]
    ON [App].[tbEventLog]([EventTypeCode] ASC, [LoggedOn] ASC);


go
PRINT N'Creating Index [App].[tbEventLog].[IX_App_tbEventLog_LoggedOn]...';


go
CREATE NONCLUSTERED INDEX [IX_App_tbEventLog_LoggedOn]
    ON [App].[tbEventLog]([LoggedOn] DESC);


go
PRINT N'Creating Table [App].[tbDocType]...';


go
CREATE TABLE [App].[tbDocType] (
    [DocTypeCode]  SMALLINT      NOT NULL,
    [DocType]      NVARCHAR (50) NOT NULL,
    [DocClassCode] SMALLINT      NOT NULL,
    CONSTRAINT [PK_App_tbDocType] PRIMARY KEY CLUSTERED ([DocTypeCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [App].[tbEth]...';


go
CREATE TABLE [App].[tbEth] (
    [NetworkProvider]   NVARCHAR (200) NOT NULL,
    [PublicKey]         NVARCHAR (42)  NOT NULL,
    [PrivateKey]        NVARCHAR (64)  NULL,
    [ConsortiumAddress] NVARCHAR (42)  NULL,
    CONSTRAINT [PK_App_tbEth] PRIMARY KEY CLUSTERED ([NetworkProvider] ASC)
);


go
PRINT N'Creating Table [App].[tbDocSpool]...';


go
CREATE TABLE [App].[tbDocSpool] (
    [UserName]       NVARCHAR (50) NOT NULL,
    [DocTypeCode]    SMALLINT      NOT NULL,
    [DocumentNumber] NVARCHAR (25) NOT NULL,
    [SpooledOn]      DATETIME      NOT NULL,
    [RowVer]         ROWVERSION    NOT NULL,
    CONSTRAINT [PK_App_tbDocSpool] PRIMARY KEY CLUSTERED ([UserName] ASC, [DocTypeCode] ASC, [DocumentNumber] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [App].[tbDocSpool].[IX_App_tbDocSpool_DocTypeCode]...';


go
CREATE NONCLUSTERED INDEX [IX_App_tbDocSpool_DocTypeCode]
    ON [App].[tbDocSpool]([DocTypeCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Table [App].[tbUoc]...';


go
CREATE TABLE [App].[tbUoc] (
    [UnitOfCharge] NVARCHAR (5)   NOT NULL,
    [UocSymbol]    NVARCHAR (10)  NOT NULL,
    [UocName]      NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_tbTag] PRIMARY KEY CLUSTERED ([UnitOfCharge] ASC)
);


go
PRINT N'Creating Table [App].[tbDocClass]...';


go
CREATE TABLE [App].[tbDocClass] (
    [DocClassCode] SMALLINT      NOT NULL,
    [DocClass]     NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_App_tbDocClass] PRIMARY KEY CLUSTERED ([DocClassCode] ASC)
);


go
PRINT N'Creating Table [App].[tbDoc]...';


go
CREATE TABLE [App].[tbDoc] (
    [DocTypeCode] SMALLINT      NOT NULL,
    [ReportName]  NVARCHAR (50) NOT NULL,
    [OpenMode]    SMALLINT      NOT NULL,
    [Description] NVARCHAR (50) NOT NULL,
    [RowVer]      ROWVERSION    NOT NULL,
    CONSTRAINT [PK_App_tbDoc] PRIMARY KEY CLUSTERED ([DocTypeCode] ASC, [ReportName] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [App].[tbCodeExclusion]...';


go
CREATE TABLE [App].[tbCodeExclusion] (
    [ExcludedTag] NVARCHAR (100) NOT NULL,
    [RowVer]      ROWVERSION     NOT NULL,
    CONSTRAINT [PK_App_tbCodeExclusion] PRIMARY KEY CLUSTERED ([ExcludedTag] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [App].[tbHost]...';


go
CREATE TABLE [App].[tbHost] (
    [HostId]          INT            IDENTITY (1, 1) NOT NULL,
    [HostDescription] NVARCHAR (50)  NOT NULL,
    [EmailAddress]    VARCHAR (256)  NOT NULL,
    [EmailPassword]   NVARCHAR (50)  NOT NULL,
    [HostName]        NVARCHAR (256) NOT NULL,
    [HostPort]        INT            NULL,
    [InsertedBy]      NVARCHAR (50)  NOT NULL,
    [InsertedOn]      DATETIME       NOT NULL,
    CONSTRAINT [PK_App_tbHost] PRIMARY KEY NONCLUSTERED ([HostId] ASC)
);


go
PRINT N'Creating Index [App].[tbHost].[IX_App_tbHost_HostDescription]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_App_tbHost_HostDescription]
    ON [App].[tbHost]([HostDescription] ASC);


go
PRINT N'Creating Table [App].[tbCalendar]...';


go
CREATE TABLE [App].[tbCalendar] (
    [CalendarCode] NVARCHAR (10) NOT NULL,
    [Monday]       BIT           NOT NULL,
    [Tuesday]      BIT           NOT NULL,
    [Wednesday]    BIT           NOT NULL,
    [Thursday]     BIT           NOT NULL,
    [Friday]       BIT           NOT NULL,
    [Saturday]     BIT           NOT NULL,
    [Sunday]       BIT           NOT NULL,
    [RowVer]       ROWVERSION    NOT NULL,
    CONSTRAINT [PK_App_tbCalendar] PRIMARY KEY CLUSTERED ([CalendarCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [App].[tbBucketType]...';


go
CREATE TABLE [App].[tbBucketType] (
    [BucketTypeCode] SMALLINT      NOT NULL,
    [BucketType]     NVARCHAR (25) NOT NULL,
    CONSTRAINT [PK_App_tbBucketType] PRIMARY KEY CLUSTERED ([BucketTypeCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [App].[tbBucketInterval]...';


go
CREATE TABLE [App].[tbBucketInterval] (
    [BucketIntervalCode] SMALLINT      NOT NULL,
    [BucketInterval]     NVARCHAR (15) NOT NULL,
    [RowVer]             ROWVERSION    NOT NULL,
    CONSTRAINT [PK_App_tbBucketInterval] PRIMARY KEY CLUSTERED ([BucketIntervalCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [App].[tbYearPeriod]...';


go
CREATE TABLE [App].[tbYearPeriod] (
    [YearNumber]         SMALLINT        NOT NULL,
    [StartOn]            DATETIME        NOT NULL,
    [MonthNumber]        SMALLINT        NOT NULL,
    [CashStatusCode]     SMALLINT        NOT NULL,
    [InsertedBy]         NVARCHAR (50)   NOT NULL,
    [InsertedOn]         DATETIME        NOT NULL,
    [CorporationTaxRate] REAL            NOT NULL,
    [RowVer]             ROWVERSION      NOT NULL,
    [TaxAdjustment]      DECIMAL (18, 5) NOT NULL,
    [VatAdjustment]      DECIMAL (18, 5) NOT NULL,
    CONSTRAINT [PK_App_tbYearPeriod] PRIMARY KEY CLUSTERED ([YearNumber] ASC, [StartOn] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [IX_App_tbYearPeriod_StartOn] UNIQUE NONCLUSTERED ([StartOn] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [IX_App_tbYearPeriod_Year_MonthNumber] UNIQUE NONCLUSTERED ([YearNumber] ASC, [MonthNumber] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [App].[tbYear]...';


go
CREATE TABLE [App].[tbYear] (
    [YearNumber]     SMALLINT      NOT NULL,
    [StartMonth]     SMALLINT      NOT NULL,
    [CashStatusCode] SMALLINT      NOT NULL,
    [Description]    NVARCHAR (10) NOT NULL,
    [InsertedBy]     NVARCHAR (50) NOT NULL,
    [InsertedOn]     DATETIME      NOT NULL,
    [RowVer]         ROWVERSION    NOT NULL,
    CONSTRAINT [PK_App_tbYear] PRIMARY KEY CLUSTERED ([YearNumber] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [App].[tbUom]...';


go
CREATE TABLE [App].[tbUom] (
    [UnitOfMeasure] NVARCHAR (15) NOT NULL,
    [RowVer]        ROWVERSION    NOT NULL,
    CONSTRAINT [PK_App_tbUom] PRIMARY KEY CLUSTERED ([UnitOfMeasure] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [App].[tbText]...';


go
CREATE TABLE [App].[tbText] (
    [TextId]    INT        NOT NULL,
    [Message]   NTEXT      NOT NULL,
    [Arguments] SMALLINT   NOT NULL,
    [RowVer]    ROWVERSION NOT NULL,
    CONSTRAINT [PK_App_tbText] PRIMARY KEY CLUSTERED ([TextId] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [App].[tbTaxCode]...';


go
CREATE TABLE [App].[tbTaxCode] (
    [TaxCode]        NVARCHAR (10)   NOT NULL,
    [TaxDescription] NVARCHAR (50)   NOT NULL,
    [TaxTypeCode]    SMALLINT        NOT NULL,
    [RoundingCode]   SMALLINT        NOT NULL,
    [UpdatedBy]      NVARCHAR (50)   NOT NULL,
    [UpdatedOn]      DATETIME        NOT NULL,
    [RowVer]         ROWVERSION      NOT NULL,
    [TaxRate]        DECIMAL (18, 4) NOT NULL,
    [Decimals]       SMALLINT        NOT NULL,
    CONSTRAINT [PK_App_tbTaxCode] PRIMARY KEY CLUSTERED ([TaxCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [App].[tbTaxCode].[IX_App_tbTaxCodeByType]...';


go
CREATE NONCLUSTERED INDEX [IX_App_tbTaxCodeByType]
    ON [App].[tbTaxCode]([TaxTypeCode] ASC, [TaxCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Table [App].[tbRounding]...';


go
CREATE TABLE [App].[tbRounding] (
    [RoundingCode] SMALLINT      NOT NULL,
    [Rounding]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_tbRounding] PRIMARY KEY CLUSTERED ([RoundingCode] ASC)
);


go
PRINT N'Creating Table [App].[tbRegister]...';


go
CREATE TABLE [App].[tbRegister] (
    [RegisterName] NVARCHAR (50) NOT NULL,
    [NextNumber]   INT           NOT NULL,
    [RowVer]       ROWVERSION    NOT NULL,
    CONSTRAINT [PK_App_tbRegister] PRIMARY KEY CLUSTERED ([RegisterName] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [App].[tbRecurrence]...';


go
CREATE TABLE [App].[tbRecurrence] (
    [RecurrenceCode] SMALLINT      NOT NULL,
    [Recurrence]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_App_tbRecurrence] PRIMARY KEY CLUSTERED ([RecurrenceCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [App].[tbMonth]...';


go
CREATE TABLE [App].[tbMonth] (
    [MonthNumber] SMALLINT      NOT NULL,
    [MonthName]   NVARCHAR (10) NOT NULL,
    CONSTRAINT [PK_App_tbMonth] PRIMARY KEY CLUSTERED ([MonthNumber] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [App].[tbInstall]...';


go
CREATE TABLE [App].[tbInstall] (
    [InstallId]      INT           IDENTITY (1, 1) NOT NULL,
    [SQLDataVersion] REAL          NOT NULL,
    [SQLRelease]     INT           NOT NULL,
    [InsertedBy]     NVARCHAR (50) NOT NULL,
    [InsertedOn]     DATETIME      NOT NULL,
    [UpdatedBy]      NVARCHAR (50) NOT NULL,
    [UpdatedOn]      DATETIME      NOT NULL,
    CONSTRAINT [PK_App_tbInstall] PRIMARY KEY CLUSTERED ([InstallId] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [App].[tbCalendarHoliday]...';


go
CREATE TABLE [App].[tbCalendarHoliday] (
    [CalendarCode]  NVARCHAR (10) NOT NULL,
    [UnavailableOn] DATETIME      NOT NULL,
    [RowVer]        ROWVERSION    NOT NULL,
    CONSTRAINT [PK_App_tbCalendarHoliday] PRIMARY KEY CLUSTERED ([CalendarCode] ASC, [UnavailableOn] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [App].[tbCalendarHoliday].[IX_App_tbCalendarHoliday_CalendarCode]...';


go
CREATE NONCLUSTERED INDEX [IX_App_tbCalendarHoliday_CalendarCode]
    ON [App].[tbCalendarHoliday]([CalendarCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Table [App].[tbOptions]...';


go
CREATE TABLE [App].[tbOptions] (
    [Identifier]         NVARCHAR (4)  NOT NULL,
    [IsInitialised]      BIT           NOT NULL,
    [SubjectCode]        NVARCHAR (10) NOT NULL,
    [RegisterName]       NVARCHAR (50) NOT NULL,
    [DefaultPrintMode]   SMALLINT      NOT NULL,
    [BucketTypeCode]     SMALLINT      NOT NULL,
    [BucketIntervalCode] SMALLINT      NOT NULL,
    [NetProfitCode]      NVARCHAR (10) NULL,
    [VatCategoryCode]    NVARCHAR (10) NULL,
    [TaxHorizon]         SMALLINT      NOT NULL,
    [IsAutoOffsetDays]   BIT           NOT NULL,
    [InsertedBy]         NVARCHAR (50) NOT NULL,
    [InsertedOn]         DATETIME      NOT NULL,
    [UpdatedBy]          NVARCHAR (50) NOT NULL,
    [UpdatedOn]          DATETIME      NOT NULL,
    [RowVer]             ROWVERSION    NOT NULL,
    [UnitOfCharge]       NVARCHAR (5)  NULL,
    [MinerFeeCode]       NVARCHAR (50) NULL,
    [MinerAccountCode]   NVARCHAR (10) NULL,
    [CoinTypeCode]       SMALLINT      NOT NULL,
    [HostId]             INT           NULL,
    CONSTRAINT [PK_App_tbOptions] PRIMARY KEY CLUSTERED ([Identifier] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Object].[tbSyncType]...';


go
CREATE TABLE [Object].[tbSyncType] (
    [SyncTypeCode] SMALLINT      NOT NULL,
    [SyncType]     NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_Object_tbSyncType] PRIMARY KEY CLUSTERED ([SyncTypeCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Object].[tbOp]...';


go
CREATE TABLE [Object].[tbOp] (
    [ObjectCode]      NVARCHAR (50)   NOT NULL,
    [OperationNumber] SMALLINT        NOT NULL,
    [SyncTypeCode]    SMALLINT        NOT NULL,
    [Operation]       NVARCHAR (50)   NOT NULL,
    [OffsetDays]      SMALLINT        NOT NULL,
    [InsertedBy]      NVARCHAR (50)   NOT NULL,
    [InsertedOn]      DATETIME        NOT NULL,
    [UpdatedBy]       NVARCHAR (50)   NOT NULL,
    [UpdatedOn]       DATETIME        NOT NULL,
    [RowVer]          ROWVERSION      NOT NULL,
    [Duration]        DECIMAL (18, 4) NULL,
    CONSTRAINT [PK_Object_tbOp] PRIMARY KEY CLUSTERED ([ObjectCode] ASC, [OperationNumber] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Object].[tbOp].[IX_Object_tbOp_Operation]...';


go
CREATE NONCLUSTERED INDEX [IX_Object_tbOp_Operation]
    ON [Object].[tbOp]([Operation] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Table [Object].[tbFlow]...';


go
CREATE TABLE [Object].[tbFlow] (
    [ParentCode]     NVARCHAR (50)   NOT NULL,
    [StepNumber]     SMALLINT        NOT NULL,
    [ChildCode]      NVARCHAR (50)   NOT NULL,
    [SyncTypeCode]   SMALLINT        NOT NULL,
    [OffsetDays]     SMALLINT        NOT NULL,
    [InsertedBy]     NVARCHAR (50)   NOT NULL,
    [InsertedOn]     DATETIME        NOT NULL,
    [UpdatedBy]      NVARCHAR (50)   NOT NULL,
    [UpdatedOn]      DATETIME        NOT NULL,
    [RowVer]         ROWVERSION      NOT NULL,
    [UsedOnQuantity] DECIMAL (18, 6) NOT NULL,
    CONSTRAINT [PK_Object_tbFlow] PRIMARY KEY NONCLUSTERED ([ParentCode] ASC, [StepNumber] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Object].[tbFlow].[IX_Object_tbFlow_ChildParent]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Object_tbFlow_ChildParent]
    ON [Object].[tbFlow]([ChildCode] ASC, [ParentCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Object].[tbFlow].[IX_Object_tbFlow_ParentChild]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Object_tbFlow_ParentChild]
    ON [Object].[tbFlow]([ParentCode] ASC, [ChildCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Table [Object].[tbAttributeType]...';


go
CREATE TABLE [Object].[tbAttributeType] (
    [AttributeTypeCode] SMALLINT      NOT NULL,
    [AttributeType]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Object_tbAttributeType] PRIMARY KEY CLUSTERED ([AttributeTypeCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Object].[tbAttribute]...';


go
CREATE TABLE [Object].[tbAttribute] (
    [ObjectCode]        NVARCHAR (50)  NOT NULL,
    [Attribute]         NVARCHAR (50)  NOT NULL,
    [PrintOrder]        SMALLINT       NOT NULL,
    [AttributeTypeCode] SMALLINT       NOT NULL,
    [DefaultText]       NVARCHAR (400) NULL,
    [InsertedBy]        NVARCHAR (50)  NOT NULL,
    [InsertedOn]        DATETIME       NOT NULL,
    [UpdatedBy]         NVARCHAR (50)  NOT NULL,
    [UpdatedOn]         DATETIME       NOT NULL,
    [RowVer]            ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Object_tbAttribute] PRIMARY KEY CLUSTERED ([ObjectCode] ASC, [Attribute] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Object].[tbAttribute].[IX_Object_tbAttribute]...';


go
CREATE NONCLUSTERED INDEX [IX_Object_tbAttribute]
    ON [Object].[tbAttribute]([Attribute] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Object].[tbAttribute].[IX_Object_tbAttribute_DefaultText]...';


go
CREATE NONCLUSTERED INDEX [IX_Object_tbAttribute_DefaultText]
    ON [Object].[tbAttribute]([DefaultText] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Object].[tbAttribute].[IX_Object_tbAttribute_OrderBy]...';


go
CREATE NONCLUSTERED INDEX [IX_Object_tbAttribute_OrderBy]
    ON [Object].[tbAttribute]([ObjectCode] ASC, [PrintOrder] ASC, [Attribute] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Object].[tbAttribute].[IX_Object_tbAttribute_Type_OrderBy]...';


go
CREATE NONCLUSTERED INDEX [IX_Object_tbAttribute_Type_OrderBy]
    ON [Object].[tbAttribute]([ObjectCode] ASC, [AttributeTypeCode] ASC, [PrintOrder] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Table [Object].[tbObject]...';


go
CREATE TABLE [Object].[tbObject] (
    [ObjectCode]        NVARCHAR (50)   NOT NULL,
    [ProjectStatusCode] SMALLINT        NOT NULL,
    [UnitOfMeasure]     NVARCHAR (15)   NOT NULL,
    [CashCode]          NVARCHAR (50)   NULL,
    [Printed]           BIT             NOT NULL,
    [RegisterName]      NVARCHAR (50)   NULL,
    [InsertedBy]        NVARCHAR (50)   NOT NULL,
    [InsertedOn]        DATETIME        NOT NULL,
    [UpdatedBy]         NVARCHAR (50)   NOT NULL,
    [UpdatedOn]         DATETIME        NOT NULL,
    [RowVer]            ROWVERSION      NOT NULL,
    [ObjectDescription] NVARCHAR (100)  NULL,
    [UnitCharge]        DECIMAL (18, 7) NOT NULL,
    CONSTRAINT [PK_Object_tbObjectCode] PRIMARY KEY NONCLUSTERED ([ObjectCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Object].[tbMirror]...';


go
CREATE TABLE [Object].[tbMirror] (
    [ObjectCode]         NVARCHAR (50) NOT NULL,
    [SubjectCode]        NVARCHAR (10) NOT NULL,
    [AllocationCode]     NVARCHAR (50) NOT NULL,
    [TransmitStatusCode] SMALLINT      NOT NULL,
    [InsertedBy]         NVARCHAR (50) NOT NULL,
    [InsertedOn]         DATETIME      NOT NULL,
    [UpdatedBy]          NVARCHAR (50) NOT NULL,
    [UpdatedOn]          DATETIME      NOT NULL,
    [RowVer]             ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Object_tbMirror] PRIMARY KEY CLUSTERED ([ObjectCode] ASC, [SubjectCode] ASC, [AllocationCode] ASC)
);


go
PRINT N'Creating Index [Object].[tbMirror].[IX_Object_tbMirror_AllocationCode]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Object_tbMirror_AllocationCode]
    ON [Object].[tbMirror]([SubjectCode] ASC, [AllocationCode] ASC)
    INCLUDE([ObjectCode]);


go
PRINT N'Creating Index [Object].[tbMirror].[IX_Object_tbMirror_TransmitStatusCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Object_tbMirror_TransmitStatusCode]
    ON [Object].[tbMirror]([TransmitStatusCode] ASC, [AllocationCode] ASC);


go
PRINT N'Creating Table [Subject].[tbTransmitStatus]...';


go
CREATE TABLE [Subject].[tbTransmitStatus] (
    [TransmitStatusCode] SMALLINT      NOT NULL,
    [TransmitStatus]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_App_tbTransmitStatus] PRIMARY KEY CLUSTERED ([TransmitStatusCode] ASC)
);


go
PRINT N'Creating Table [Subject].[tbType]...';


go
CREATE TABLE [Subject].[tbType] (
    [SubjectTypeCode]  SMALLINT      NOT NULL,
    [CashPolarityCode] SMALLINT      NOT NULL,
    [SubjectType]      NVARCHAR (50) NOT NULL,
    [RowVer]           ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Subject_tbType] PRIMARY KEY NONCLUSTERED ([SubjectTypeCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Subject].[tbStatus]...';


go
CREATE TABLE [Subject].[tbStatus] (
    [SubjectStatusCode] SMALLINT       NOT NULL,
    [SubjectStatus]     NVARCHAR (255) NULL,
    CONSTRAINT [PK_Subject_tbStatus] PRIMARY KEY NONCLUSTERED ([SubjectStatusCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Subject].[tbAccountType]...';


go
CREATE TABLE [Subject].[tbAccountType] (
    [AccountTypeCode] SMALLINT      NOT NULL,
    [AccountType]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Subject_tbAccountType] PRIMARY KEY CLUSTERED ([AccountTypeCode] ASC)
);


go
PRINT N'Creating Table [Subject].[tbAddress]...';


go
CREATE TABLE [Subject].[tbAddress] (
    [AddressCode] NVARCHAR (15) NOT NULL,
    [SubjectCode] NVARCHAR (10) NOT NULL,
    [Address]     NTEXT         NOT NULL,
    [InsertedBy]  NVARCHAR (50) NOT NULL,
    [InsertedOn]  DATETIME      NOT NULL,
    [UpdatedBy]   NVARCHAR (50) NOT NULL,
    [UpdatedOn]   DATETIME      NOT NULL,
    [RowVer]      ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Subject_tbAddress] PRIMARY KEY CLUSTERED ([AddressCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Subject].[tbAddress].[IX_Subject_tbAddress]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Subject_tbAddress]
    ON [Subject].[tbAddress]([SubjectCode] ASC, [AddressCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Table [Subject].[tbDoc]...';


go
CREATE TABLE [Subject].[tbDoc] (
    [SubjectCode]         NVARCHAR (10)  NOT NULL,
    [DocumentName]        NVARCHAR (255) NOT NULL,
    [DocumentDescription] NTEXT          NULL,
    [DocumentImage]       IMAGE          NULL,
    [InsertedBy]          NVARCHAR (50)  NOT NULL,
    [InsertedOn]          DATETIME       NOT NULL,
    [UpdatedBy]           NVARCHAR (50)  NOT NULL,
    [UpdatedOn]           DATETIME       NOT NULL,
    [RowVer]              ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Subject_tbDoc] PRIMARY KEY NONCLUSTERED ([SubjectCode] ASC, [DocumentName] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Subject].[tbDoc].[IX_Subject_tbDoc_DocName_AccountCode]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Subject_tbDoc_DocName_AccountCode]
    ON [Subject].[tbDoc]([DocumentName] ASC, [SubjectCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Subject].[tbDoc].[IX_Subject_tbDoc_AccountCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Subject_tbDoc_AccountCode]
    ON [Subject].[tbDoc]([SubjectCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Table [Subject].[tbContact]...';


go
CREATE TABLE [Subject].[tbContact] (
    [SubjectCode]   NVARCHAR (10)  NOT NULL,
    [ContactName]   NVARCHAR (100) NOT NULL,
    [FileAs]        NVARCHAR (100) NULL,
    [OnMailingList] BIT            NOT NULL,
    [NameTitle]     NVARCHAR (25)  NULL,
    [NickName]      NVARCHAR (100) NULL,
    [JobTitle]      NVARCHAR (100) NULL,
    [PhoneNumber]   NVARCHAR (50)  NULL,
    [MobileNumber]  NVARCHAR (50)  NULL,
    [EmailAddress]  NVARCHAR (255) NULL,
    [Hobby]         NVARCHAR (50)  NULL,
    [DateOfBirth]   DATETIME       NULL,
    [Department]    NVARCHAR (50)  NULL,
    [SpouseName]    NVARCHAR (50)  NULL,
    [HomeNumber]    NVARCHAR (50)  NULL,
    [Information]   NTEXT          NULL,
    [Photo]         IMAGE          NULL,
    [InsertedBy]    NVARCHAR (50)  NOT NULL,
    [InsertedOn]    DATETIME       NOT NULL,
    [UpdatedBy]     NVARCHAR (50)  NOT NULL,
    [UpdatedOn]     DATETIME       NOT NULL,
    [RowVer]        ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Subject_tbContact] PRIMARY KEY NONCLUSTERED ([SubjectCode] ASC, [ContactName] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Subject].[tbContact].[IX_Subject_tbContactDepartment]...';


go
CREATE NONCLUSTERED INDEX [IX_Subject_tbContactDepartment]
    ON [Subject].[tbContact]([Department] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Subject].[tbContact].[IX_Subject_tbContactJobTitle]...';


go
CREATE NONCLUSTERED INDEX [IX_Subject_tbContactJobTitle]
    ON [Subject].[tbContact]([JobTitle] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Subject].[tbContact].[IX_Subject_tbContactNameTitle]...';


go
CREATE NONCLUSTERED INDEX [IX_Subject_tbContactNameTitle]
    ON [Subject].[tbContact]([NameTitle] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Subject].[tbContact].[IX_Subject_tbContact_AccountCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Subject_tbContact_AccountCode]
    ON [Subject].[tbContact]([SubjectCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Table [Subject].[tbSector]...';


go
CREATE TABLE [Subject].[tbSector] (
    [SubjectCode]    NVARCHAR (10) NOT NULL,
    [IndustrySector] NVARCHAR (50) NOT NULL,
    [RowVer]         ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Subject_tbSector] PRIMARY KEY CLUSTERED ([SubjectCode] ASC, [IndustrySector] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Subject].[tbSector].[IX_Subject_tbSector_IndustrySector]...';


go
CREATE NONCLUSTERED INDEX [IX_Subject_tbSector_IndustrySector]
    ON [Subject].[tbSector]([IndustrySector] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Table [Subject].[tbSubject]...';


go
CREATE TABLE [Subject].[tbSubject] (
    [SubjectCode]         NVARCHAR (10)   NOT NULL,
    [SubjectName]         NVARCHAR (255)  NOT NULL,
    [SubjectTypeCode]     SMALLINT        NOT NULL,
    [SubjectStatusCode]   SMALLINT        NOT NULL,
    [TaxCode]             NVARCHAR (10)   NULL,
    [AddressCode]         NVARCHAR (15)   NULL,
    [AreaCode]            NVARCHAR (50)   NULL,
    [PhoneNumber]         NVARCHAR (50)   NULL,
    [EmailAddress]        NVARCHAR (255)  NULL,
    [WebSite]             NVARCHAR (255)  NULL,
    [SubjectSource]       NVARCHAR (100)  NULL,
    [PaymentTerms]        NVARCHAR (100)  NULL,
    [ExpectedDays]        SMALLINT        NOT NULL,
    [PaymentDays]         SMALLINT        NOT NULL,
    [PayDaysFromMonthEnd] BIT             NOT NULL,
    [PayBalance]          BIT             NOT NULL,
    [NumberOfEmployees]   INT             NOT NULL,
    [CompanyNumber]       NVARCHAR (20)   NULL,
    [VatNumber]           NVARCHAR (50)   NULL,
    [EUJurisdiction]      BIT             NOT NULL,
    [BusinessDescription] NTEXT           NULL,
    [Logo]                IMAGE           NULL,
    [InsertedBy]          NVARCHAR (50)   NOT NULL,
    [InsertedOn]          DATETIME        NOT NULL,
    [UpdatedBy]           NVARCHAR (50)   NOT NULL,
    [UpdatedOn]           DATETIME        NOT NULL,
    [RowVer]              ROWVERSION      NOT NULL,
    [TransmitStatusCode]  SMALLINT        NOT NULL,
    [OpeningBalance]      DECIMAL (18, 5) NOT NULL,
    [Turnover]            DECIMAL (18, 5) NOT NULL,
    CONSTRAINT [PK_Subject_tbSubject] PRIMARY KEY NONCLUSTERED ([SubjectCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Subject].[tbSubject].[IX_Subject_tb_AccountName]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Subject_tb_AccountName]
    ON [Subject].[tbSubject]([SubjectName] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Subject].[tbSubject].[IX_Subject_tb_SubjectSource]...';


go
CREATE NONCLUSTERED INDEX [IX_Subject_tb_SubjectSource]
    ON [Subject].[tbSubject]([SubjectSource] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Subject].[tbSubject].[IX_Subject_tb_AreaCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Subject_tb_AreaCode]
    ON [Subject].[tbSubject]([AreaCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Subject].[tbSubject].[IX_Subject_tbSubject_OpeningBalance]...';


go
CREATE NONCLUSTERED INDEX [IX_Subject_tbSubject_OpeningBalance]
    ON [Subject].[tbSubject]([SubjectCode] ASC)
    INCLUDE([OpeningBalance]);


go
PRINT N'Creating Index [Subject].[tbSubject].[IX_Subject_tb_SubjectStatusCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Subject_tb_SubjectStatusCode]
    ON [Subject].[tbSubject]([SubjectStatusCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Subject].[tbSubject].[IX_Subject_tb_Status_AccountCode]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Subject_tb_Status_AccountCode]
    ON [Subject].[tbSubject]([SubjectStatusCode] ASC, [SubjectName] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Subject].[tbSubject].[IX_Subject_tb_SubjectTypeCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Subject_tb_SubjectTypeCode]
    ON [Subject].[tbSubject]([SubjectTypeCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Subject].[tbSubject].[IX_Subject_tb_PaymentTerms]...';


go
CREATE NONCLUSTERED INDEX [IX_Subject_tb_PaymentTerms]
    ON [Subject].[tbSubject]([PaymentTerms] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Subject].[tbSubject].[IX_tbSubject_tb_AccountCode]...';


go
CREATE NONCLUSTERED INDEX [IX_tbSubject_tb_AccountCode]
    ON [Subject].[tbSubject]([SubjectCode] ASC)
    INCLUDE([SubjectName]);


go
PRINT N'Creating Table [Subject].[tbAccountKey]...';


go
CREATE TABLE [Subject].[tbAccountKey] (
    [AccountCode] NVARCHAR (10)       NOT NULL,
    [HDPath]      [sys].[hierarchyid] NOT NULL,
    [KeyName]     NVARCHAR (50)       NOT NULL,
    [HDLevel]     AS                  ([HDPath].[GetLevel]()),
    CONSTRAINT [PK_Subject_tbAccountKey] PRIMARY KEY NONCLUSTERED ([AccountCode] ASC, [HDPath] ASC)
);


go
PRINT N'Creating Index [Subject].[tbAccountKey].[IX_Subject_tbAccountKey_HDLevel]...';


go
CREATE NONCLUSTERED INDEX [IX_Subject_tbAccountKey_HDLevel]
    ON [Subject].[tbAccountKey]([AccountCode] ASC, [HDLevel] ASC, [HDPath] ASC);


go
PRINT N'Creating Index [Subject].[tbAccountKey].[IX_Subject_tbAccountKey_KeyName]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Subject_tbAccountKey_KeyName]
    ON [Subject].[tbAccountKey]([AccountCode] ASC, [KeyName] ASC);


go
PRINT N'Creating Table [Subject].[tbAccount]...';


go
CREATE TABLE [Subject].[tbAccount] (
    [AccountCode]     NVARCHAR (10)   NOT NULL,
    [SubjectCode]     NVARCHAR (10)   NOT NULL,
    [AccountName]     NVARCHAR (50)   NOT NULL,
    [SortCode]        NVARCHAR (10)   NULL,
    [AccountNumber]   NVARCHAR (20)   NULL,
    [CashCode]        NVARCHAR (50)   NULL,
    [OpeningBalance]  DECIMAL (18, 5) NOT NULL,
    [CurrentBalance]  DECIMAL (18, 5) NOT NULL,
    [CoinTypeCode]    SMALLINT        NOT NULL,
    [AccountTypeCode] SMALLINT        NOT NULL,
    [LiquidityLevel]  SMALLINT        NOT NULL,
    [AccountClosed]   BIT             NOT NULL,
    [InsertedBy]      NVARCHAR (50)   NOT NULL,
    [InsertedOn]      DATETIME        NOT NULL,
    [UpdatedBy]       NVARCHAR (50)   NOT NULL,
    [UpdatedOn]       DATETIME        NOT NULL,
    [RowVer]          ROWVERSION      NOT NULL,
    CONSTRAINT [PK_Subject_tbAccount] PRIMARY KEY CLUSTERED ([AccountCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Subject].[tbAccount].[IX_Subject_tbAccount]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Subject_tbAccount]
    ON [Subject].[tbAccount]([SubjectCode] ASC, [AccountCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Subject].[tbAccount].[IX_tbAccount_AccountTypeCode]...';


go
CREATE NONCLUSTERED INDEX [IX_tbAccount_AccountTypeCode]
    ON [Subject].[tbAccount]([AccountTypeCode] ASC, [LiquidityLevel] DESC, [AccountCode] ASC);


go
PRINT N'Creating Index [Subject].[tbAccount].[IX_Subject_tbAccount_CashAccountName]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Subject_tbAccount_CashAccountName]
    ON [Subject].[tbAccount]([AccountName] ASC);


go
PRINT N'Creating Table [Project].[tbFlow]...';


go
CREATE TABLE [Project].[tbFlow] (
    [ParentProjectCode] NVARCHAR (20)   NOT NULL,
    [StepNumber]        SMALLINT        NOT NULL,
    [ChildProjectCode]  NVARCHAR (20)   NULL,
    [SyncTypeCode]      SMALLINT        NOT NULL,
    [OffsetDays]        REAL            NOT NULL,
    [InsertedBy]        NVARCHAR (50)   NOT NULL,
    [InsertedOn]        DATETIME        NOT NULL,
    [UpdatedBy]         NVARCHAR (50)   NOT NULL,
    [UpdatedOn]         DATETIME        NOT NULL,
    [RowVer]            ROWVERSION      NOT NULL,
    [UsedOnQuantity]    DECIMAL (18, 6) NOT NULL,
    CONSTRAINT [PK_Project_tbFlow] PRIMARY KEY CLUSTERED ([ParentProjectCode] ASC, [StepNumber] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Project].[tbFlow].[IX_Project_tbFlow_ChildParent]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Project_tbFlow_ChildParent]
    ON [Project].[tbFlow]([ChildProjectCode] ASC, [ParentProjectCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Project].[tbFlow].[IX_Project_tbFlow_ParentChild]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Project_tbFlow_ParentChild]
    ON [Project].[tbFlow]([ParentProjectCode] ASC, [ChildProjectCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Table [Project].[tbDoc]...';


go
CREATE TABLE [Project].[tbDoc] (
    [ProjectCode]         NVARCHAR (20)  NOT NULL,
    [DocumentName]        NVARCHAR (255) NOT NULL,
    [DocumentDescription] NTEXT          NULL,
    [DocumentImage]       IMAGE          NOT NULL,
    [InsertedBy]          NVARCHAR (50)  NOT NULL,
    [InsertedOn]          DATETIME       NOT NULL,
    [UpdatedBy]           NVARCHAR (50)  NOT NULL,
    [UpdatedOn]           DATETIME       NOT NULL,
    [RowVer]              ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Project_tbDoc] PRIMARY KEY CLUSTERED ([ProjectCode] ASC, [DocumentName] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Project].[tbAttribute]...';


go
CREATE TABLE [Project].[tbAttribute] (
    [ProjectCode]          NVARCHAR (20)  NOT NULL,
    [Attribute]            NVARCHAR (50)  NOT NULL,
    [PrintOrder]           SMALLINT       NOT NULL,
    [AttributeTypeCode]    SMALLINT       NOT NULL,
    [AttributeDescription] NVARCHAR (400) NULL,
    [InsertedBy]           NVARCHAR (50)  NOT NULL,
    [InsertedOn]           DATETIME       NOT NULL,
    [UpdatedBy]            NVARCHAR (50)  NOT NULL,
    [UpdatedOn]            DATETIME       NOT NULL,
    [RowVer]               ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Project_tbProjectAttribute] PRIMARY KEY CLUSTERED ([ProjectCode] ASC, [Attribute] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Project].[tbAttribute].[IX_Project_tbAttribute]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tbAttribute]
    ON [Project].[tbAttribute]([ProjectCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Project].[tbAttribute].[IX_Project_tbAttribute_Description]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tbAttribute_Description]
    ON [Project].[tbAttribute]([Attribute] ASC, [AttributeDescription] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Project].[tbAttribute].[IX_Project_tbAttribute_OrderBy]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tbAttribute_OrderBy]
    ON [Project].[tbAttribute]([ProjectCode] ASC, [PrintOrder] ASC, [Attribute] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Project].[tbAttribute].[IX_Project_tbAttribute_Type_OrderBy]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tbAttribute_Type_OrderBy]
    ON [Project].[tbAttribute]([ProjectCode] ASC, [AttributeTypeCode] ASC, [PrintOrder] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Table [Project].[tbStatus]...';


go
CREATE TABLE [Project].[tbStatus] (
    [ProjectStatusCode] SMALLINT       NOT NULL,
    [ProjectStatus]     NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_Project_tbStatus] PRIMARY KEY NONCLUSTERED ([ProjectStatusCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Project].[tbStatus].[IX_Project_tbStatus_ProjectStatus]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Project_tbStatus_ProjectStatus]
    ON [Project].[tbStatus]([ProjectStatus] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Table [Project].[tbQuote]...';


go
CREATE TABLE [Project].[tbQuote] (
    [ProjectCode]     NVARCHAR (20)   NOT NULL,
    [InsertedBy]      NVARCHAR (50)   NOT NULL,
    [InsertedOn]      DATETIME        NOT NULL,
    [UpdatedBy]       NVARCHAR (50)   NOT NULL,
    [UpdatedOn]       DATETIME        NOT NULL,
    [RowVer]          ROWVERSION      NOT NULL,
    [Quantity]        DECIMAL (18, 4) NOT NULL,
    [RunOnQuantity]   DECIMAL (18, 4) NOT NULL,
    [RunBackQuantity] DECIMAL (18, 4) NOT NULL,
    [TotalPrice]      DECIMAL (18, 5) NOT NULL,
    [RunOnPrice]      DECIMAL (18, 5) NOT NULL,
    [RunBackPrice]    DECIMAL (18, 5) NOT NULL,
    CONSTRAINT [PK_Project_tbQuote] PRIMARY KEY CLUSTERED ([ProjectCode] ASC, [Quantity] ASC)
);


go
PRINT N'Creating Table [Project].[tbOpStatus]...';


go
CREATE TABLE [Project].[tbOpStatus] (
    [OpStatusCode] SMALLINT      NOT NULL,
    [OpStatus]     NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_Project_tbOpStatus] PRIMARY KEY CLUSTERED ([OpStatusCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Table [Project].[tbOp]...';


go
CREATE TABLE [Project].[tbOp] (
    [ProjectCode]     NVARCHAR (20)   NOT NULL,
    [OperationNumber] SMALLINT        NOT NULL,
    [SyncTypeCode]    SMALLINT        NOT NULL,
    [OpStatusCode]    SMALLINT        NOT NULL,
    [UserId]          NVARCHAR (10)   NOT NULL,
    [Operation]       NVARCHAR (50)   NOT NULL,
    [Note]            NTEXT           NULL,
    [StartOn]         DATETIME        NOT NULL,
    [EndOn]           DATETIME        NOT NULL,
    [OffsetDays]      SMALLINT        NOT NULL,
    [InsertedBy]      NVARCHAR (50)   NOT NULL,
    [InsertedOn]      DATETIME        NOT NULL,
    [UpdatedBy]       NVARCHAR (50)   NOT NULL,
    [UpdatedOn]       DATETIME        NOT NULL,
    [RowVer]          ROWVERSION      NOT NULL,
    [Duration]        DECIMAL (18, 4) NULL,
    CONSTRAINT [PK_Project_tbOp] PRIMARY KEY CLUSTERED ([ProjectCode] ASC, [OperationNumber] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Project].[tbOp].[IX_Project_tbOp_OpStatusCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tbOp_OpStatusCode]
    ON [Project].[tbOp]([OpStatusCode] ASC, [StartOn] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Project].[tbOp].[IX_Project_tbOp_UserIdOpStatus]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tbOp_UserIdOpStatus]
    ON [Project].[tbOp]([UserId] ASC, [OpStatusCode] ASC, [StartOn] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Table [Project].[tbAllocationEvent]...';


go
CREATE TABLE [Project].[tbAllocationEvent] (
    [ContractAddress]   NVARCHAR (42)   NOT NULL,
    [LogId]             INT             IDENTITY (1, 1) NOT NULL,
    [EventTypeCode]     SMALLINT        NOT NULL,
    [ProjectStatusCode] SMALLINT        NOT NULL,
    [ActionOn]          DATETIME        NOT NULL,
    [TaxRate]           DECIMAL (18, 4) NOT NULL,
    [QuantityOrdered]   DECIMAL (18, 4) NOT NULL,
    [QuantityDelivered] DECIMAL (18, 4) NOT NULL,
    [InsertedOn]        DATETIME        NOT NULL,
    [RowVer]            ROWVERSION      NOT NULL,
    [UnitCharge]        DECIMAL (18, 7) NOT NULL,
    CONSTRAINT [PK_Project_tbAllocationEvent] PRIMARY KEY CLUSTERED ([ContractAddress] ASC, [LogId] ASC)
);


go
PRINT N'Creating Index [Project].[tbAllocationEvent].[IX_Project_tbAllocationEvent_EventTypeCide]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tbAllocationEvent_EventTypeCide]
    ON [Project].[tbAllocationEvent]([EventTypeCode] ASC, [ProjectStatusCode] ASC, [InsertedOn] DESC);


go
PRINT N'Creating Table [Project].[tbCostSet]...';


go
CREATE TABLE [Project].[tbCostSet] (
    [ProjectCode] NVARCHAR (20) NOT NULL,
    [UserId]      NVARCHAR (10) NOT NULL,
    [InsertedBy]  NVARCHAR (50) NOT NULL,
    [InsertedOn]  DATETIME      NOT NULL,
    [RowVer]      ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Project_tbCostSet] PRIMARY KEY CLUSTERED ([ProjectCode] ASC, [UserId] ASC)
);


go
PRINT N'Creating Index [Project].[tbCostSet].[IX_Project_tbCostSet_UserId]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Project_tbCostSet_UserId]
    ON [Project].[tbCostSet]([UserId] ASC, [ProjectCode] ASC);


go
PRINT N'Creating Table [Project].[tbChangeLog]...';


go
CREATE TABLE [Project].[tbChangeLog] (
    [ProjectCode]        NVARCHAR (20)   NOT NULL,
    [LogId]              INT             IDENTITY (1, 1) NOT NULL,
    [ChangedOn]          DATETIME        NOT NULL,
    [TransmitStatusCode] SMALLINT        NOT NULL,
    [SubjectCode]        NVARCHAR (10)   NOT NULL,
    [ObjectCode]         NVARCHAR (50)   NOT NULL,
    [ProjectStatusCode]  SMALLINT        NOT NULL,
    [ActionOn]           DATETIME        NOT NULL,
    [CashCode]           NVARCHAR (50)   NULL,
    [TaxCode]            NVARCHAR (10)   NULL,
    [UpdatedBy]          NVARCHAR (50)   NOT NULL,
    [RowVer]             ROWVERSION      NOT NULL,
    [Quantity]           DECIMAL (18, 4) NOT NULL,
    [UnitCharge]         DECIMAL (18, 7) NOT NULL,
    CONSTRAINT [PK_Project_tbChangeLog] PRIMARY KEY CLUSTERED ([ProjectCode] ASC, [LogId] DESC)
);


go
PRINT N'Creating Index [Project].[tbChangeLog].[IX_Project_tbChangeLog_LogId]...';


go
CREATE UNIQUE NONCLUSTERED INDEX [IX_Project_tbChangeLog_LogId]
    ON [Project].[tbChangeLog]([LogId] DESC);


go
PRINT N'Creating Index [Project].[tbChangeLog].[IX_Project_tbChangeLog_ChangedOn]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tbChangeLog_ChangedOn]
    ON [Project].[tbChangeLog]([ChangedOn] DESC);


go
PRINT N'Creating Index [Project].[tbChangeLog].[IX_Project_tbChangeLog_TransmitStatus]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tbChangeLog_TransmitStatus]
    ON [Project].[tbChangeLog]([TransmitStatusCode] ASC, [ChangedOn] ASC);


go
PRINT N'Creating Table [Project].[tbAllocation]...';


go
CREATE TABLE [Project].[tbAllocation] (
    [ContractAddress]       NVARCHAR (42)   NOT NULL,
    [SubjectCode]           NVARCHAR (10)   NOT NULL,
    [AllocationCode]        NVARCHAR (50)   NOT NULL,
    [AllocationDescription] NVARCHAR (256)  NULL,
    [ProjectCode]           NVARCHAR (20)   NOT NULL,
    [ProjectTitle]          NVARCHAR (100)  NULL,
    [CashPolarityCode]      SMALLINT        NOT NULL,
    [UnitOfMeasure]         NVARCHAR (15)   NULL,
    [UnitOfCharge]          NVARCHAR (5)    NULL,
    [ProjectStatusCode]     SMALLINT        NOT NULL,
    [ActionOn]              DATETIME        NOT NULL,
    [TaxRate]               DECIMAL (18, 4) NOT NULL,
    [QuantityOrdered]       DECIMAL (18, 4) NOT NULL,
    [QuantityDelivered]     DECIMAL (18, 4) NOT NULL,
    [InsertedOn]            DATETIME        NOT NULL,
    [RowVer]                ROWVERSION      NOT NULL,
    [UnitCharge]            DECIMAL (18, 7) NOT NULL,
    CONSTRAINT [PK_Project_tbAllocation] PRIMARY KEY CLUSTERED ([ContractAddress] ASC)
);


go
PRINT N'Creating Index [Project].[tbAllocation].[IX_Project_tbAllocation_ObjectCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tbAllocation_ObjectCode]
    ON [Project].[tbAllocation]([SubjectCode] ASC, [AllocationCode] ASC);


go
PRINT N'Creating Index [Project].[tbAllocation].[IX_Project_tbAllocation_ProjectStatusCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tbAllocation_ProjectStatusCode]
    ON [Project].[tbAllocation]([ProjectStatusCode] ASC, [SubjectCode] ASC, [AllocationCode] ASC, [ActionOn] ASC);


go
PRINT N'Creating Index [Project].[tbAllocation].[IX_Project_tbAllocation_ProjectCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tbAllocation_ProjectCode]
    ON [Project].[tbAllocation]([SubjectCode] ASC, [ProjectCode] ASC);


go
PRINT N'Creating Table [Project].[tbProject]...';


go
CREATE TABLE [Project].[tbProject] (
    [ProjectCode]       NVARCHAR (20)   NOT NULL,
    [UserId]            NVARCHAR (10)   NOT NULL,
    [SubjectCode]       NVARCHAR (10)   NOT NULL,
    [SecondReference]   NVARCHAR (20)   NULL,
    [ProjectTitle]      NVARCHAR (100)  NULL,
    [ContactName]       NVARCHAR (100)  NULL,
    [ObjectCode]        NVARCHAR (50)   NOT NULL,
    [ProjectStatusCode] SMALLINT        NOT NULL,
    [ActionById]        NVARCHAR (10)   NOT NULL,
    [ActionOn]          DATETIME        NOT NULL,
    [ActionedOn]        DATETIME        NULL,
    [PaymentOn]         DATETIME        NOT NULL,
    [ProjectNotes]      NVARCHAR (255)  NULL,
    [CashCode]          NVARCHAR (50)   NULL,
    [TaxCode]           NVARCHAR (10)   NULL,
    [AddressCodeFrom]   NVARCHAR (15)   NULL,
    [AddressCodeTo]     NVARCHAR (15)   NULL,
    [Spooled]           BIT             NOT NULL,
    [Printed]           BIT             NOT NULL,
    [InsertedBy]        NVARCHAR (50)   NOT NULL,
    [InsertedOn]        DATETIME        NOT NULL,
    [UpdatedBy]         NVARCHAR (50)   NOT NULL,
    [UpdatedOn]         DATETIME        NOT NULL,
    [RowVer]            ROWVERSION      NOT NULL,
    [Quantity]          DECIMAL (18, 4) NOT NULL,
    [TotalCharge]       DECIMAL (18, 5) NOT NULL,
    [UnitCharge]        DECIMAL (18, 7) NOT NULL,
    CONSTRAINT [PK_Project_tbProject] PRIMARY KEY CLUSTERED ([ProjectCode] ASC) WITH (FILLFACTOR = 90)
);


go
PRINT N'Creating Index [Project].[tbProject].[IX_Project_tb_AccountCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tb_AccountCode]
    ON [Project].[tbProject]([SubjectCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Project].[tbProject].[IX_Project_tb_AccountCodeByActionOn]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tb_AccountCodeByActionOn]
    ON [Project].[tbProject]([SubjectCode] ASC, [ActionOn] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Project].[tbProject].[IX_Project_tb_AccountCodeByStatus]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tb_AccountCodeByStatus]
    ON [Project].[tbProject]([SubjectCode] ASC, [ProjectStatusCode] ASC, [ActionOn] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Project].[tbProject].[IX_Project_tb_ActionBy]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tb_ActionBy]
    ON [Project].[tbProject]([ActionById] ASC, [ProjectStatusCode] ASC, [ActionOn] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Project].[tbProject].[IX_Project_tb_ActionById]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tb_ActionById]
    ON [Project].[tbProject]([ActionById] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Project].[tbProject].[IX_Project_tb_ActionOn]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tb_ActionOn]
    ON [Project].[tbProject]([ActionOn] DESC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Project].[tbProject].[IX_Project_tb_ActionOnStatus]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tb_ActionOnStatus]
    ON [Project].[tbProject]([ProjectStatusCode] ASC, [ActionOn] ASC, [SubjectCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Project].[tbProject].[IX_Project_tb_ObjectCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tb_ObjectCode]
    ON [Project].[tbProject]([ObjectCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Project].[tbProject].[IX_Project_tb_ObjectCodeProjectTitle]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tb_ObjectCodeProjectTitle]
    ON [Project].[tbProject]([ObjectCode] ASC, [ProjectTitle] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Project].[tbProject].[IX_Project_tb_CashCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tb_CashCode]
    ON [Project].[tbProject]([CashCode] ASC, [ProjectStatusCode] ASC, [ActionOn] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Project].[tbProject].[IX_Project_tb_ProjectStatusCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tb_ProjectStatusCode]
    ON [Project].[tbProject]([ProjectStatusCode] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Project].[tbProject].[IX_Project_tb_UserId]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tb_UserId]
    ON [Project].[tbProject]([UserId] ASC) WITH (FILLFACTOR = 90);


go
PRINT N'Creating Index [Project].[tbProject].[IX_Project_tbProject_ActionOn_Status_CashCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tbProject_ActionOn_Status_CashCode]
    ON [Project].[tbProject]([ActionOn] ASC, [ProjectStatusCode] ASC, [CashCode] ASC, [ProjectCode] ASC);


go
PRINT N'Creating Index [Project].[tbProject].[IX_Project_tbProject_ActionOn_ProjectCode_CashCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tbProject_ActionOn_ProjectCode_CashCode]
    ON [Project].[tbProject]([ActionOn] ASC, [ProjectCode] ASC, [CashCode] ASC, [ProjectStatusCode] ASC, [SubjectCode] ASC)
    INCLUDE([ProjectTitle], [ObjectCode], [ActionedOn], [Quantity], [UnitCharge], [TotalCharge], [PaymentOn]);


go
PRINT N'Creating Index [Project].[tbProject].[IX_Project_tbProject_Status_TaxCode_ProjectCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tbProject_Status_TaxCode_ProjectCode]
    ON [Project].[tbProject]([ProjectStatusCode] ASC, [TaxCode] ASC, [ProjectCode] ASC, [CashCode] ASC, [ActionOn] ASC)
    INCLUDE([TotalCharge]);


go
PRINT N'Creating Index [Project].[tbProject].[IX_Project_tbProject_ProjectCode_CashCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tbProject_ProjectCode_CashCode]
    ON [Project].[tbProject]([ProjectCode] ASC, [CashCode] ASC)
    INCLUDE([Quantity], [UnitCharge]);


go
PRINT N'Creating Index [Project].[tbProject].[IX_Project_tbProject_ProjectCode_TaxCode_CashCode]...';


go
CREATE NONCLUSTERED INDEX [IX_Project_tbProject_ProjectCode_TaxCode_CashCode]
    ON [Project].[tbProject]([ProjectCode] ASC, [TaxCode] ASC, [CashCode] ASC, [ActionOn] ASC)
    INCLUDE([TotalCharge]);


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tb_LogonName]...';


go
ALTER TABLE [Usr].[tbUser]
    ADD CONSTRAINT [DF_Usr_tb_LogonName] DEFAULT (suser_sname()) FOR [LogonName];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tbUser_IsAdministrator]...';


go
ALTER TABLE [Usr].[tbUser]
    ADD CONSTRAINT [DF_Usr_tbUser_IsAdministrator] DEFAULT ((0)) FOR [IsAdministrator];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tbUser_IsEnabled]...';


go
ALTER TABLE [Usr].[tbUser]
    ADD CONSTRAINT [DF_Usr_tbUser_IsEnabled] DEFAULT ((1)) FOR [IsEnabled];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tb_NextProjectNumber]...';


go
ALTER TABLE [Usr].[tbUser]
    ADD CONSTRAINT [DF_Usr_tb_NextProjectNumber] DEFAULT ((1)) FOR [NextProjectNumber];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tb_InsertedBy]...';


go
ALTER TABLE [Usr].[tbUser]
    ADD CONSTRAINT [DF_Usr_tb_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tb_InsertedOn]...';


go
ALTER TABLE [Usr].[tbUser]
    ADD CONSTRAINT [DF_Usr_tb_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tb_UpdatedBy]...';


go
ALTER TABLE [Usr].[tbUser]
    ADD CONSTRAINT [DF_Usr_tb_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tb_UpdatedOn]...';


go
ALTER TABLE [Usr].[tbUser]
    ADD CONSTRAINT [DF_Usr_tb_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tbUser_MenuViewCode]...';


go
ALTER TABLE [Usr].[tbUser]
    ADD CONSTRAINT [DF_Usr_tbUser_MenuViewCode] DEFAULT ((0)) FOR [MenuViewCode];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tbMenuOpenMode_OpenMode]...';


go
ALTER TABLE [Usr].[tbMenuOpenMode]
    ADD CONSTRAINT [DF_Usr_tbMenuOpenMode_OpenMode] DEFAULT ((0)) FOR [OpenMode];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tbMenuCommand_Command]...';


go
ALTER TABLE [Usr].[tbMenuCommand]
    ADD CONSTRAINT [DF_Usr_tbMenuCommand_Command] DEFAULT ((0)) FOR [Command];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tbMenu_InsertedOn]...';


go
ALTER TABLE [Usr].[tbMenu]
    ADD CONSTRAINT [DF_Usr_tbMenu_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tbMenu_InsertedBy]...';


go
ALTER TABLE [Usr].[tbMenu]
    ADD CONSTRAINT [DF_Usr_tbMenu_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tbMenu_InterfaceCode]...';


go
ALTER TABLE [Usr].[tbMenu]
    ADD CONSTRAINT [DF_Usr_tbMenu_InterfaceCode] DEFAULT ((0)) FOR [InterfaceCode];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tbMenuEntry_MenuId]...';


go
ALTER TABLE [Usr].[tbMenuEntry]
    ADD CONSTRAINT [DF_Usr_tbMenuEntry_MenuId] DEFAULT ((0)) FOR [MenuId];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tbMenuEntry_FolderId]...';


go
ALTER TABLE [Usr].[tbMenuEntry]
    ADD CONSTRAINT [DF_Usr_tbMenuEntry_FolderId] DEFAULT ((0)) FOR [FolderId];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tbMenuEntry_ItemId]...';


go
ALTER TABLE [Usr].[tbMenuEntry]
    ADD CONSTRAINT [DF_Usr_tbMenuEntry_ItemId] DEFAULT ((0)) FOR [ItemId];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tbMenuEntry_Command]...';


go
ALTER TABLE [Usr].[tbMenuEntry]
    ADD CONSTRAINT [DF_Usr_tbMenuEntry_Command] DEFAULT ((0)) FOR [Command];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tbMenuEntry_OpenMode]...';


go
ALTER TABLE [Usr].[tbMenuEntry]
    ADD CONSTRAINT [DF_Usr_tbMenuEntry_OpenMode] DEFAULT ((1)) FOR [OpenMode];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tbMenuEntry_UpdatedOn]...';


go
ALTER TABLE [Usr].[tbMenuEntry]
    ADD CONSTRAINT [DF_Usr_tbMenuEntry_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tbMenuEntry_InsertedOn]...';


go
ALTER TABLE [Usr].[tbMenuEntry]
    ADD CONSTRAINT [DF_Usr_tbMenuEntry_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Usr].[DF_Usr_tbMenuEntry_UpdatedBy]...';


go
ALTER TABLE [Usr].[tbMenuEntry]
    ADD CONSTRAINT [DF_Usr_tbMenuEntry_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Project_tbMirrorEvent_InsertedOn]...';


go
ALTER TABLE [Invoice].[tbMirrorEvent]
    ADD CONSTRAINT [DF_Project_tbMirrorEvent_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbMirrorEvent_PaidValue]...';


go
ALTER TABLE [Invoice].[tbMirrorEvent]
    ADD CONSTRAINT [DF_Invoice_tbMirrorEvent_PaidValue] DEFAULT ((0)) FOR [PaidValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbMirrorEvent_PaidTaxValue]...';


go
ALTER TABLE [Invoice].[tbMirrorEvent]
    ADD CONSTRAINT [DF_Invoice_tbMirrorEvent_PaidTaxValue] DEFAULT ((0)) FOR [PaidTaxValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbMirrorItem_InvoiceValue]...';


go
ALTER TABLE [Invoice].[tbMirrorItem]
    ADD CONSTRAINT [DF_Invoice_tbMirrorItem_InvoiceValue] DEFAULT ((0)) FOR [InvoiceValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbMirrorItem_TaxValue]...';


go
ALTER TABLE [Invoice].[tbMirrorItem]
    ADD CONSTRAINT [DF_Invoice_tbMirrorItem_TaxValue] DEFAULT ((0)) FOR [TaxValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbChangeLog_ChangedOn]...';


go
ALTER TABLE [Invoice].[tbChangeLog]
    ADD CONSTRAINT [DF_Invoice_tbChangeLog_ChangedOn] DEFAULT (dateadd(millisecond,datepart(millisecond,getdate())*(-1),getdate())) FOR [ChangedOn];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbChangeLog_TransmissionStatusCode]...';


go
ALTER TABLE [Invoice].[tbChangeLog]
    ADD CONSTRAINT [DF_Invoice_tbChangeLog_TransmissionStatusCode] DEFAULT ((0)) FOR [TransmitStatusCode];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbChangeLog_UpdatedBy]...';


go
ALTER TABLE [Invoice].[tbChangeLog]
    ADD CONSTRAINT [DF_Invoice_tbChangeLog_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbChangeLog_InvoiceValue]...';


go
ALTER TABLE [Invoice].[tbChangeLog]
    ADD CONSTRAINT [DF_Invoice_tbChangeLog_InvoiceValue] DEFAULT ((0)) FOR [InvoiceValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbChangeLog_TaxValue]...';


go
ALTER TABLE [Invoice].[tbChangeLog]
    ADD CONSTRAINT [DF_Invoice_tbChangeLog_TaxValue] DEFAULT ((0)) FOR [TaxValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbChangeLog_PaidValue]...';


go
ALTER TABLE [Invoice].[tbChangeLog]
    ADD CONSTRAINT [DF_Invoice_tbChangeLog_PaidValue] DEFAULT ((0)) FOR [PaidValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbChangeLog_PaidTaxValue]...';


go
ALTER TABLE [Invoice].[tbChangeLog]
    ADD CONSTRAINT [DF_Invoice_tbChangeLog_PaidTaxValue] DEFAULT ((0)) FOR [PaidTaxValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbType_NextNumber]...';


go
ALTER TABLE [Invoice].[tbType]
    ADD CONSTRAINT [DF_Invoice_tbType_NextNumber] DEFAULT ((1000)) FOR [NextNumber];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbProject_Quantity]...';


go
ALTER TABLE [Invoice].[tbProject]
    ADD CONSTRAINT [DF_Invoice_tbProject_Quantity] DEFAULT ((0)) FOR [Quantity];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbProject_TotalValue]...';


go
ALTER TABLE [Invoice].[tbProject]
    ADD CONSTRAINT [DF_Invoice_tbProject_TotalValue] DEFAULT ((0)) FOR [TotalValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbProject_InvoiceValue]...';


go
ALTER TABLE [Invoice].[tbProject]
    ADD CONSTRAINT [DF_Invoice_tbProject_InvoiceValue] DEFAULT ((0)) FOR [InvoiceValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbProject_TaxValue]...';


go
ALTER TABLE [Invoice].[tbProject]
    ADD CONSTRAINT [DF_Invoice_tbProject_TaxValue] DEFAULT ((0)) FOR [TaxValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbItem_TotalValue]...';


go
ALTER TABLE [Invoice].[tbItem]
    ADD CONSTRAINT [DF_Invoice_tbItem_TotalValue] DEFAULT ((0)) FOR [TotalValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbItem_InvoiceValue]...';


go
ALTER TABLE [Invoice].[tbItem]
    ADD CONSTRAINT [DF_Invoice_tbItem_InvoiceValue] DEFAULT ((0)) FOR [InvoiceValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbItem_TaxValue]...';


go
ALTER TABLE [Invoice].[tbItem]
    ADD CONSTRAINT [DF_Invoice_tbItem_TaxValue] DEFAULT ((0)) FOR [TaxValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tb_InvoicedOn]...';


go
ALTER TABLE [Invoice].[tbInvoice]
    ADD CONSTRAINT [DF_Invoice_tb_InvoicedOn] DEFAULT (CONVERT([date],getdate())) FOR [InvoicedOn];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbInvoice_ExpectedOn]...';


go
ALTER TABLE [Invoice].[tbInvoice]
    ADD CONSTRAINT [DF_Invoice_tbInvoice_ExpectedOn] DEFAULT (dateadd(day,(1),CONVERT([date],getdate()))) FOR [ExpectedOn];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbInvoice_DueOn]...';


go
ALTER TABLE [Invoice].[tbInvoice]
    ADD CONSTRAINT [DF_Invoice_tbInvoice_DueOn] DEFAULT (dateadd(day,(1),CONVERT([date],getdate()))) FOR [DueOn];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tb_Printed]...';


go
ALTER TABLE [Invoice].[tbInvoice]
    ADD CONSTRAINT [DF_Invoice_tb_Printed] DEFAULT ((0)) FOR [Printed];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tb_Spooled]...';


go
ALTER TABLE [Invoice].[tbInvoice]
    ADD CONSTRAINT [DF_Invoice_tb_Spooled] DEFAULT ((0)) FOR [Spooled];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tb_InvoiceValue]...';


go
ALTER TABLE [Invoice].[tbInvoice]
    ADD CONSTRAINT [DF_Invoice_tb_InvoiceValue] DEFAULT ((0)) FOR [InvoiceValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tb_TaxValue]...';


go
ALTER TABLE [Invoice].[tbInvoice]
    ADD CONSTRAINT [DF_Invoice_tb_TaxValue] DEFAULT ((0)) FOR [TaxValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tb_PaidValue]...';


go
ALTER TABLE [Invoice].[tbInvoice]
    ADD CONSTRAINT [DF_Invoice_tb_PaidValue] DEFAULT ((0)) FOR [PaidValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tb_PaidTaxValue]...';


go
ALTER TABLE [Invoice].[tbInvoice]
    ADD CONSTRAINT [DF_Invoice_tb_PaidTaxValue] DEFAULT ((0)) FOR [PaidTaxValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbEntry_InvoicedOn]...';


go
ALTER TABLE [Invoice].[tbEntry]
    ADD CONSTRAINT [DF_Invoice_tbEntry_InvoicedOn] DEFAULT (CONVERT([date],getdate())) FOR [InvoicedOn];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbEntry_TotalValue]...';


go
ALTER TABLE [Invoice].[tbEntry]
    ADD CONSTRAINT [DF_Invoice_tbEntry_TotalValue] DEFAULT ((0)) FOR [TotalValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbEntry_InvoiceValue]...';


go
ALTER TABLE [Invoice].[tbEntry]
    ADD CONSTRAINT [DF_Invoice_tbEntry_InvoiceValue] DEFAULT ((0)) FOR [InvoiceValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbMirrorProject_InvoiceValue]...';


go
ALTER TABLE [Invoice].[tbMirrorProject]
    ADD CONSTRAINT [DF_Invoice_tbMirrorProject_InvoiceValue] DEFAULT ((0)) FOR [InvoiceValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbMirrorProject_TaxValue]...';


go
ALTER TABLE [Invoice].[tbMirrorProject]
    ADD CONSTRAINT [DF_Invoice_tbMirrorProject_TaxValue] DEFAULT ((0)) FOR [TaxValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbMirror_InsertedOn]...';


go
ALTER TABLE [Invoice].[tbMirror]
    ADD CONSTRAINT [DF_Invoice_tbMirror_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbMirror_InvoiceValue]...';


go
ALTER TABLE [Invoice].[tbMirror]
    ADD CONSTRAINT [DF_Invoice_tbMirror_InvoiceValue] DEFAULT ((0)) FOR [InvoiceValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbMirror_InvoiceTax]...';


go
ALTER TABLE [Invoice].[tbMirror]
    ADD CONSTRAINT [DF_Invoice_tbMirror_InvoiceTax] DEFAULT ((0)) FOR [InvoiceTax];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbMirror_PaidValue]...';


go
ALTER TABLE [Invoice].[tbMirror]
    ADD CONSTRAINT [DF_Invoice_tbMirror_PaidValue] DEFAULT ((0)) FOR [PaidValue];


go
PRINT N'Creating Default Constraint [Invoice].[DF_Invoice_tbMirror_PaidTaxValue]...';


go
ALTER TABLE [Invoice].[tbMirror]
    ADD CONSTRAINT [DF_Invoice_tbMirror_PaidTaxValue] DEFAULT ((0)) FOR [PaidTaxValue];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbTx_TransactedOn]...';


go
ALTER TABLE [Cash].[tbTx]
    ADD CONSTRAINT [DF_Cash_tbTx_TransactedOn] DEFAULT (getdate()) FOR [TransactedOn];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbTx_TxStatusCode]...';


go
ALTER TABLE [Cash].[tbTx]
    ADD CONSTRAINT [DF_Cash_tbTx_TxStatusCode] DEFAULT ((0)) FOR [TxStatusCode];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbTx_MoneyIn]...';


go
ALTER TABLE [Cash].[tbTx]
    ADD CONSTRAINT [DF_Cash_tbTx_MoneyIn] DEFAULT ((0)) FOR [MoneyIn];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbTx_MoneyOut]...';


go
ALTER TABLE [Cash].[tbTx]
    ADD CONSTRAINT [DF_Cash_tbTx_MoneyOut] DEFAULT ((0)) FOR [MoneyOut];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbTx_Confirmations]...';


go
ALTER TABLE [Cash].[tbTx]
    ADD CONSTRAINT [DF_Cash_tbTx_Confirmations] DEFAULT ((0)) FOR [Confirmations];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbTx_InsertedBy]...';


go
ALTER TABLE [Cash].[tbTx]
    ADD CONSTRAINT [DF_Cash_tbTx_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbTxReference_TxStatusCode]...';


go
ALTER TABLE [Cash].[tbTxReference]
    ADD CONSTRAINT [DF_Cash_tbTxReference_TxStatusCode] DEFAULT ((0)) FOR [TxStatusCode];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbCode_IsEnabled]...';


go
ALTER TABLE [Cash].[tbCode]
    ADD CONSTRAINT [DF_Cash_tbCode_IsEnabled] DEFAULT ((1)) FOR [IsEnabled];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbCode_InsertedBy]...';


go
ALTER TABLE [Cash].[tbCode]
    ADD CONSTRAINT [DF_Cash_tbCode_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbCode_InsertedOn]...';


go
ALTER TABLE [Cash].[tbCode]
    ADD CONSTRAINT [DF_Cash_tbCode_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbCode_UpdatedBy]...';


go
ALTER TABLE [Cash].[tbCode]
    ADD CONSTRAINT [DF_Cash_tbCode_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbCode_UpdatedOn]...';


go
ALTER TABLE [Cash].[tbCode]
    ADD CONSTRAINT [DF_Cash_tbCode_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbCategory_CategoryTypeCode]...';


go
ALTER TABLE [Cash].[tbCategory]
    ADD CONSTRAINT [DF_Cash_tbCategory_CategoryTypeCode] DEFAULT ((1)) FOR [CategoryTypeCode];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbCategory_CashPolarityCode]...';


go
ALTER TABLE [Cash].[tbCategory]
    ADD CONSTRAINT [DF_Cash_tbCategory_CashPolarityCode] DEFAULT ((1)) FOR [CashPolarityCode];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbCategory_CashTypeCode]...';


go
ALTER TABLE [Cash].[tbCategory]
    ADD CONSTRAINT [DF_Cash_tbCategory_CashTypeCode] DEFAULT ((0)) FOR [CashTypeCode];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbCategory_DisplayOrder]...';


go
ALTER TABLE [Cash].[tbCategory]
    ADD CONSTRAINT [DF_Cash_tbCategory_DisplayOrder] DEFAULT ((0)) FOR [DisplayOrder];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbCategory_IsEnabled]...';


go
ALTER TABLE [Cash].[tbCategory]
    ADD CONSTRAINT [DF_Cash_tbCategory_IsEnabled] DEFAULT ((1)) FOR [IsEnabled];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbCategory_InsertedBy]...';


go
ALTER TABLE [Cash].[tbCategory]
    ADD CONSTRAINT [DF_Cash_tbCategory_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbCategory_InsertedOn]...';


go
ALTER TABLE [Cash].[tbCategory]
    ADD CONSTRAINT [DF_Cash_tbCategory_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbCategory_UpdatedBy]...';


go
ALTER TABLE [Cash].[tbCategory]
    ADD CONSTRAINT [DF_Cash_tbCategory_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbCategory_UpdatedOn]...';


go
ALTER TABLE [Cash].[tbCategory]
    ADD CONSTRAINT [DF_Cash_tbCategory_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbPeriod_InvoiceValue]...';


go
ALTER TABLE [Cash].[tbPeriod]
    ADD CONSTRAINT [DF_Cash_tbPeriod_InvoiceValue] DEFAULT ((0)) FOR [InvoiceValue];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbPeriod_InvoiceTax]...';


go
ALTER TABLE [Cash].[tbPeriod]
    ADD CONSTRAINT [DF_Cash_tbPeriod_InvoiceTax] DEFAULT ((0)) FOR [InvoiceTax];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbPeriod_ForecastValue]...';


go
ALTER TABLE [Cash].[tbPeriod]
    ADD CONSTRAINT [DF_Cash_tbPeriod_ForecastValue] DEFAULT ((0)) FOR [ForecastValue];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbPeriod_ForecastTax]...';


go
ALTER TABLE [Cash].[tbPeriod]
    ADD CONSTRAINT [DF_Cash_tbPeriod_ForecastTax] DEFAULT ((0)) FOR [ForecastTax];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbMirror_TransmitStatusCode]...';


go
ALTER TABLE [Cash].[tbMirror]
    ADD CONSTRAINT [DF_Cash_tbMirror_TransmitStatusCode] DEFAULT ((0)) FOR [TransmitStatusCode];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbMirror_InsertedBy]...';


go
ALTER TABLE [Cash].[tbMirror]
    ADD CONSTRAINT [DF_Cash_tbMirror_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbMirror_InsertedOn]...';


go
ALTER TABLE [Cash].[tbMirror]
    ADD CONSTRAINT [DF_Cash_tbMirror_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbMirror_UpdatedBy]...';


go
ALTER TABLE [Cash].[tbMirror]
    ADD CONSTRAINT [DF_Cash_tbMirror_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbMirror_UpdatedOn]...';


go
ALTER TABLE [Cash].[tbMirror]
    ADD CONSTRAINT [DF_Cash_tbMirror_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Cash].[DF_App_tbOptions_MonthNumber]...';


go
ALTER TABLE [Cash].[tbTaxType]
    ADD CONSTRAINT [DF_App_tbOptions_MonthNumber] DEFAULT ((1)) FOR [MonthNumber];


go
PRINT N'Creating Default Constraint [Cash].[DF_App_tbOptions_Recurrence]...';


go
ALTER TABLE [Cash].[tbTaxType]
    ADD CONSTRAINT [DF_App_tbOptions_Recurrence] DEFAULT ((1)) FOR [RecurrenceCode];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbTaxType_OffsetDays]...';


go
ALTER TABLE [Cash].[tbTaxType]
    ADD CONSTRAINT [DF_Cash_tbTaxType_OffsetDays] DEFAULT ((0)) FOR [OffsetDays];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbChange_ChangeTypeCode]...';


go
ALTER TABLE [Cash].[tbChange]
    ADD CONSTRAINT [DF_Cash_tbChange_ChangeTypeCode] DEFAULT ((0)) FOR [ChangeTypeCode];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbChange_ChangeStatusCode]...';


go
ALTER TABLE [Cash].[tbChange]
    ADD CONSTRAINT [DF_Cash_tbChange_ChangeStatusCode] DEFAULT ((0)) FOR [ChangeStatusCode];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbChange_AddressIndex]...';


go
ALTER TABLE [Cash].[tbChange]
    ADD CONSTRAINT [DF_Cash_tbChange_AddressIndex] DEFAULT ((0)) FOR [AddressIndex];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbChange_UpdatedOn]...';


go
ALTER TABLE [Cash].[tbChange]
    ADD CONSTRAINT [DF_Cash_tbChange_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbChange_UpdatedBy]...';


go
ALTER TABLE [Cash].[tbChange]
    ADD CONSTRAINT [DF_Cash_tbChange_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbChange_InsertedOn]...';


go
ALTER TABLE [Cash].[tbChange]
    ADD CONSTRAINT [DF_Cash_tbChange_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbChange_InsertedBy]...';


go
ALTER TABLE [Cash].[tbChange]
    ADD CONSTRAINT [DF_Cash_tbChange_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbPayment_PaymentStatusCode]...';


go
ALTER TABLE [Cash].[tbPayment]
    ADD CONSTRAINT [DF_Cash_tbPayment_PaymentStatusCode] DEFAULT ((0)) FOR [PaymentStatusCode];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbPayment_PaidOn]...';


go
ALTER TABLE [Cash].[tbPayment]
    ADD CONSTRAINT [DF_Cash_tbPayment_PaidOn] DEFAULT (CONVERT([date],getdate())) FOR [PaidOn];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbPayment_PaidInValue]...';


go
ALTER TABLE [Cash].[tbPayment]
    ADD CONSTRAINT [DF_Cash_tbPayment_PaidInValue] DEFAULT ((0)) FOR [PaidInValue];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbPayment_PaidOutValue]...';


go
ALTER TABLE [Cash].[tbPayment]
    ADD CONSTRAINT [DF_Cash_tbPayment_PaidOutValue] DEFAULT ((0)) FOR [PaidOutValue];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbPayment_InsertedBy]...';


go
ALTER TABLE [Cash].[tbPayment]
    ADD CONSTRAINT [DF_Cash_tbPayment_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbPayment_InsertedOn]...';


go
ALTER TABLE [Cash].[tbPayment]
    ADD CONSTRAINT [DF_Cash_tbPayment_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbPayment_UpdatedBy]...';


go
ALTER TABLE [Cash].[tbPayment]
    ADD CONSTRAINT [DF_Cash_tbPayment_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbPayment_UpdatedOn]...';


go
ALTER TABLE [Cash].[tbPayment]
    ADD CONSTRAINT [DF_Cash_tbPayment_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Cash].[DF_Cash_tbPayment_IsProfitAndLoss]...';


go
ALTER TABLE [Cash].[tbPayment]
    ADD CONSTRAINT [DF_Cash_tbPayment_IsProfitAndLoss] DEFAULT ((1)) FOR [IsProfitAndLoss];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbLog_LoggedOn]...';


go
ALTER TABLE [App].[tbEventLog]
    ADD CONSTRAINT [DF_App_tbLog_LoggedOn] DEFAULT (getdate()) FOR [LoggedOn];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbLog_EventTypeCode]...';


go
ALTER TABLE [App].[tbEventLog]
    ADD CONSTRAINT [DF_App_tbLog_EventTypeCode] DEFAULT ((2)) FOR [EventTypeCode];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbLog_InsertedBy]...';


go
ALTER TABLE [App].[tbEventLog]
    ADD CONSTRAINT [DF_App_tbLog_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbDocType_DocClassCode]...';


go
ALTER TABLE [App].[tbDocType]
    ADD CONSTRAINT [DF_App_tbDocType_DocClassCode] DEFAULT ((0)) FOR [DocClassCode];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbDocSpool_UserName]...';


go
ALTER TABLE [App].[tbDocSpool]
    ADD CONSTRAINT [DF_App_tbDocSpool_UserName] DEFAULT (suser_sname()) FOR [UserName];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbDocSpool_DocTypeCode]...';


go
ALTER TABLE [App].[tbDocSpool]
    ADD CONSTRAINT [DF_App_tbDocSpool_DocTypeCode] DEFAULT ((1)) FOR [DocTypeCode];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbDocSpool_SpooledOn]...';


go
ALTER TABLE [App].[tbDocSpool]
    ADD CONSTRAINT [DF_App_tbDocSpool_SpooledOn] DEFAULT (getdate()) FOR [SpooledOn];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbDoc_OpenMode]...';


go
ALTER TABLE [App].[tbDoc]
    ADD CONSTRAINT [DF_App_tbDoc_OpenMode] DEFAULT ((1)) FOR [OpenMode];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbHost_InsertedBy]...';


go
ALTER TABLE [App].[tbHost]
    ADD CONSTRAINT [DF_App_tbHost_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbHost_InsertedOn]...';


go
ALTER TABLE [App].[tbHost]
    ADD CONSTRAINT [DF_App_tbHost_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbCalendar_Monday]...';


go
ALTER TABLE [App].[tbCalendar]
    ADD CONSTRAINT [DF_App_tbCalendar_Monday] DEFAULT ((1)) FOR [Monday];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbCalendar_Tuesday]...';


go
ALTER TABLE [App].[tbCalendar]
    ADD CONSTRAINT [DF_App_tbCalendar_Tuesday] DEFAULT ((1)) FOR [Tuesday];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbCalendar_Wednesday]...';


go
ALTER TABLE [App].[tbCalendar]
    ADD CONSTRAINT [DF_App_tbCalendar_Wednesday] DEFAULT ((1)) FOR [Wednesday];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbCalendar_Thursday]...';


go
ALTER TABLE [App].[tbCalendar]
    ADD CONSTRAINT [DF_App_tbCalendar_Thursday] DEFAULT ((1)) FOR [Thursday];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbCalendar_Friday]...';


go
ALTER TABLE [App].[tbCalendar]
    ADD CONSTRAINT [DF_App_tbCalendar_Friday] DEFAULT ((1)) FOR [Friday];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbCalendar_Saturday]...';


go
ALTER TABLE [App].[tbCalendar]
    ADD CONSTRAINT [DF_App_tbCalendar_Saturday] DEFAULT ((0)) FOR [Saturday];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbCalendar_Sunday]...';


go
ALTER TABLE [App].[tbCalendar]
    ADD CONSTRAINT [DF_App_tbCalendar_Sunday] DEFAULT ((0)) FOR [Sunday];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbYearPeriod_CashStatusCode]...';


go
ALTER TABLE [App].[tbYearPeriod]
    ADD CONSTRAINT [DF_App_tbYearPeriod_CashStatusCode] DEFAULT ((1)) FOR [CashStatusCode];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbYearPeriod_InsertedBy]...';


go
ALTER TABLE [App].[tbYearPeriod]
    ADD CONSTRAINT [DF_App_tbYearPeriod_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbYearPeriod_InsertedOn]...';


go
ALTER TABLE [App].[tbYearPeriod]
    ADD CONSTRAINT [DF_App_tbYearPeriod_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbYearPeriod_CorporationTaxRate]...';


go
ALTER TABLE [App].[tbYearPeriod]
    ADD CONSTRAINT [DF_App_tbYearPeriod_CorporationTaxRate] DEFAULT ((0)) FOR [CorporationTaxRate];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbYearPeriod_TaxAdjustment]...';


go
ALTER TABLE [App].[tbYearPeriod]
    ADD CONSTRAINT [DF_App_tbYearPeriod_TaxAdjustment] DEFAULT ((0)) FOR [TaxAdjustment];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbYearPeriod_VatAdjustment]...';


go
ALTER TABLE [App].[tbYearPeriod]
    ADD CONSTRAINT [DF_App_tbYearPeriod_VatAdjustment] DEFAULT ((0)) FOR [VatAdjustment];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbYear_StartMonth]...';


go
ALTER TABLE [App].[tbYear]
    ADD CONSTRAINT [DF_App_tbYear_StartMonth] DEFAULT ((1)) FOR [StartMonth];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbYear_CashStatusCode]...';


go
ALTER TABLE [App].[tbYear]
    ADD CONSTRAINT [DF_App_tbYear_CashStatusCode] DEFAULT ((1)) FOR [CashStatusCode];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbYear_InsertedBy]...';


go
ALTER TABLE [App].[tbYear]
    ADD CONSTRAINT [DF_App_tbYear_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbYear_InsertedOn]...';


go
ALTER TABLE [App].[tbYear]
    ADD CONSTRAINT [DF_App_tbYear_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbTaxCode_TaxTypeCode]...';


go
ALTER TABLE [App].[tbTaxCode]
    ADD CONSTRAINT [DF_App_tbTaxCode_TaxTypeCode] DEFAULT ((2)) FOR [TaxTypeCode];


go
PRINT N'Creating Default Constraint [App].[DF_tbTaxCode_RoundingCode]...';


go
ALTER TABLE [App].[tbTaxCode]
    ADD CONSTRAINT [DF_tbTaxCode_RoundingCode] DEFAULT ((0)) FOR [RoundingCode];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbTaxCode_UpdatedBy]...';


go
ALTER TABLE [App].[tbTaxCode]
    ADD CONSTRAINT [DF_App_tbTaxCode_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbTaxCode_UpdatedOn]...';


go
ALTER TABLE [App].[tbTaxCode]
    ADD CONSTRAINT [DF_App_tbTaxCode_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbTaxCode_TaxRate]...';


go
ALTER TABLE [App].[tbTaxCode]
    ADD CONSTRAINT [DF_App_tbTaxCode_TaxRate] DEFAULT ((0)) FOR [TaxRate];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbTaxCode_Decimals]...';


go
ALTER TABLE [App].[tbTaxCode]
    ADD CONSTRAINT [DF_App_tbTaxCode_Decimals] DEFAULT ((2)) FOR [Decimals];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbRegister_NextNumber]...';


go
ALTER TABLE [App].[tbRegister]
    ADD CONSTRAINT [DF_App_tbRegister_NextNumber] DEFAULT ((1)) FOR [NextNumber];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbInstall_InsertedBy]...';


go
ALTER TABLE [App].[tbInstall]
    ADD CONSTRAINT [DF_App_tbInstall_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbInstall_InsertedOn]...';


go
ALTER TABLE [App].[tbInstall]
    ADD CONSTRAINT [DF_App_tbInstall_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbInstall_UpdatedBy]...';


go
ALTER TABLE [App].[tbInstall]
    ADD CONSTRAINT [DF_App_tbInstall_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbInstall_UpdatedOn]...';


go
ALTER TABLE [App].[tbInstall]
    ADD CONSTRAINT [DF_App_tbInstall_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbOptions_IsIntialised]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [DF_App_tbOptions_IsIntialised] DEFAULT ((0)) FOR [IsInitialised];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbOptions_DefaultPrintMode]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [DF_App_tbOptions_DefaultPrintMode] DEFAULT ((2)) FOR [DefaultPrintMode];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbOptions_BucketTypeCode]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [DF_App_tbOptions_BucketTypeCode] DEFAULT ((1)) FOR [BucketTypeCode];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbOptions_BucketIntervalCode]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [DF_App_tbOptions_BucketIntervalCode] DEFAULT ((1)) FOR [BucketIntervalCode];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbOptions_TaxHorizon]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [DF_App_tbOptions_TaxHorizon] DEFAULT ((90)) FOR [TaxHorizon];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbOptions_IsAutoOffsetDays]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [DF_App_tbOptions_IsAutoOffsetDays] DEFAULT ((0)) FOR [IsAutoOffsetDays];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbOptions_InsertedBy]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [DF_App_tbOptions_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbOptions_InsertedOn]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [DF_App_tbOptions_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbOptions_UpdatedBy]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [DF_App_tbOptions_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbOptions_UpdatedOn]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [DF_App_tbOptions_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [App].[DF_App_tbOptions_CoinTypeCode]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [DF_App_tbOptions_CoinTypeCode] DEFAULT ((2)) FOR [CoinTypeCode];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbOp_OperationNumber]...';


go
ALTER TABLE [Object].[tbOp]
    ADD CONSTRAINT [DF_Object_tbOp_OperationNumber] DEFAULT ((0)) FOR [OperationNumber];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbOp_SyncTypeCode]...';


go
ALTER TABLE [Object].[tbOp]
    ADD CONSTRAINT [DF_Object_tbOp_SyncTypeCode] DEFAULT ((1)) FOR [SyncTypeCode];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbOp_OffsetDays]...';


go
ALTER TABLE [Object].[tbOp]
    ADD CONSTRAINT [DF_Object_tbOp_OffsetDays] DEFAULT ((0)) FOR [OffsetDays];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbOp_InsertedBy]...';


go
ALTER TABLE [Object].[tbOp]
    ADD CONSTRAINT [DF_Object_tbOp_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbOp_InsertedOn]...';


go
ALTER TABLE [Object].[tbOp]
    ADD CONSTRAINT [DF_Object_tbOp_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbOp_UpdatedBy]...';


go
ALTER TABLE [Object].[tbOp]
    ADD CONSTRAINT [DF_Object_tbOp_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbOp_UpdatedOn]...';


go
ALTER TABLE [Object].[tbOp]
    ADD CONSTRAINT [DF_Object_tbOp_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbOp_Duration]...';


go
ALTER TABLE [Object].[tbOp]
    ADD CONSTRAINT [DF_Object_tbOp_Duration] DEFAULT ((0)) FOR [Duration];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbFlow_StepNumber]...';


go
ALTER TABLE [Object].[tbFlow]
    ADD CONSTRAINT [DF_Object_tbFlow_StepNumber] DEFAULT ((10)) FOR [StepNumber];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbFlow_SyncTypeCode]...';


go
ALTER TABLE [Object].[tbFlow]
    ADD CONSTRAINT [DF_Object_tbFlow_SyncTypeCode] DEFAULT ((0)) FOR [SyncTypeCode];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbFlow_OffsetDays]...';


go
ALTER TABLE [Object].[tbFlow]
    ADD CONSTRAINT [DF_Object_tbFlow_OffsetDays] DEFAULT ((0)) FOR [OffsetDays];


go
PRINT N'Creating Default Constraint [Object].[DF_tbTemplateObject_InsertedBy]...';


go
ALTER TABLE [Object].[tbFlow]
    ADD CONSTRAINT [DF_tbTemplateObject_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Object].[DF_tbTemplateObject_InsertedOn]...';


go
ALTER TABLE [Object].[tbFlow]
    ADD CONSTRAINT [DF_tbTemplateObject_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Object].[DF_tbTemplateObject_UpdatedBy]...';


go
ALTER TABLE [Object].[tbFlow]
    ADD CONSTRAINT [DF_tbTemplateObject_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Object].[DF_tbTemplateObject_UpdatedOn]...';


go
ALTER TABLE [Object].[tbFlow]
    ADD CONSTRAINT [DF_tbTemplateObject_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbFlow_UsedOnQuantity]...';


go
ALTER TABLE [Object].[tbFlow]
    ADD CONSTRAINT [DF_Object_tbFlow_UsedOnQuantity] DEFAULT ((1)) FOR [UsedOnQuantity];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbAttribute_OrderBy]...';


go
ALTER TABLE [Object].[tbAttribute]
    ADD CONSTRAINT [DF_Object_tbAttribute_OrderBy] DEFAULT ((10)) FOR [PrintOrder];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbAttribute_AttributeTypeCode]...';


go
ALTER TABLE [Object].[tbAttribute]
    ADD CONSTRAINT [DF_Object_tbAttribute_AttributeTypeCode] DEFAULT ((0)) FOR [AttributeTypeCode];


go
PRINT N'Creating Default Constraint [Object].[DF_tbTemplateAttribute_InsertedBy]...';


go
ALTER TABLE [Object].[tbAttribute]
    ADD CONSTRAINT [DF_tbTemplateAttribute_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Object].[DF_tbTemplateAttribute_InsertedOn]...';


go
ALTER TABLE [Object].[tbAttribute]
    ADD CONSTRAINT [DF_tbTemplateAttribute_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Object].[DF_tbTemplateAttribute_UpdatedBy]...';


go
ALTER TABLE [Object].[tbAttribute]
    ADD CONSTRAINT [DF_tbTemplateAttribute_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Object].[DF_tbTemplateAttribute_UpdatedOn]...';


go
ALTER TABLE [Object].[tbAttribute]
    ADD CONSTRAINT [DF_tbTemplateAttribute_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbObject_ProjectStatusCode]...';


go
ALTER TABLE [Object].[tbObject]
    ADD CONSTRAINT [DF_Object_tbObject_ProjectStatusCode] DEFAULT ((1)) FOR [ProjectStatusCode];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbObject_Printed]...';


go
ALTER TABLE [Object].[tbObject]
    ADD CONSTRAINT [DF_Object_tbObject_Printed] DEFAULT ((0)) FOR [Printed];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbObject_InsertedBy]...';


go
ALTER TABLE [Object].[tbObject]
    ADD CONSTRAINT [DF_Object_tbObject_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbObject_InsertedOn]...';


go
ALTER TABLE [Object].[tbObject]
    ADD CONSTRAINT [DF_Object_tbObject_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbObject_UpdatedBy]...';


go
ALTER TABLE [Object].[tbObject]
    ADD CONSTRAINT [DF_Object_tbObject_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbObject_UpdatedOn]...';


go
ALTER TABLE [Object].[tbObject]
    ADD CONSTRAINT [DF_Object_tbObject_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbObject_UnitCharge]...';


go
ALTER TABLE [Object].[tbObject]
    ADD CONSTRAINT [DF_Object_tbObject_UnitCharge] DEFAULT ((0)) FOR [UnitCharge];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbMirror_TransmitStatusCode]...';


go
ALTER TABLE [Object].[tbMirror]
    ADD CONSTRAINT [DF_Object_tbMirror_TransmitStatusCode] DEFAULT ((0)) FOR [TransmitStatusCode];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbMirror_InsertedBy]...';


go
ALTER TABLE [Object].[tbMirror]
    ADD CONSTRAINT [DF_Object_tbMirror_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbMirror_InsertedOn]...';


go
ALTER TABLE [Object].[tbMirror]
    ADD CONSTRAINT [DF_Object_tbMirror_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbMirror_UpdatedBy]...';


go
ALTER TABLE [Object].[tbMirror]
    ADD CONSTRAINT [DF_Object_tbMirror_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Object].[DF_Object_tbMirror_UpdatedOn]...';


go
ALTER TABLE [Object].[tbMirror]
    ADD CONSTRAINT [DF_Object_tbMirror_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbType_SubjectTypeCode]...';


go
ALTER TABLE [Subject].[tbType]
    ADD CONSTRAINT [DF_Subject_tbType_SubjectTypeCode] DEFAULT ((1)) FOR [SubjectTypeCode];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbStatus_SubjectStatusCode]...';


go
ALTER TABLE [Subject].[tbStatus]
    ADD CONSTRAINT [DF_Subject_tbStatus_SubjectStatusCode] DEFAULT ((1)) FOR [SubjectStatusCode];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbAddress_InsertedBy]...';


go
ALTER TABLE [Subject].[tbAddress]
    ADD CONSTRAINT [DF_Subject_tbAddress_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbAddress_InsertedOn]...';


go
ALTER TABLE [Subject].[tbAddress]
    ADD CONSTRAINT [DF_Subject_tbAddress_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbAddress_UpdatedBy]...';


go
ALTER TABLE [Subject].[tbAddress]
    ADD CONSTRAINT [DF_Subject_tbAddress_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbAddress_UpdatedOn]...';


go
ALTER TABLE [Subject].[tbAddress]
    ADD CONSTRAINT [DF_Subject_tbAddress_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbDoc_InsertedBy]...';


go
ALTER TABLE [Subject].[tbDoc]
    ADD CONSTRAINT [DF_Subject_tbDoc_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbDoc_InsertedOn]...';


go
ALTER TABLE [Subject].[tbDoc]
    ADD CONSTRAINT [DF_Subject_tbDoc_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbDoc_UpdatedBy]...';


go
ALTER TABLE [Subject].[tbDoc]
    ADD CONSTRAINT [DF_Subject_tbDoc_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbDoc_UpdatedOn]...';


go
ALTER TABLE [Subject].[tbDoc]
    ADD CONSTRAINT [DF_Subject_tbDoc_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbContact_OnMailingList]...';


go
ALTER TABLE [Subject].[tbContact]
    ADD CONSTRAINT [DF_Subject_tbContact_OnMailingList] DEFAULT ((1)) FOR [OnMailingList];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbContact_InsertedBy]...';


go
ALTER TABLE [Subject].[tbContact]
    ADD CONSTRAINT [DF_Subject_tbContact_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbContact_InsertedOn]...';


go
ALTER TABLE [Subject].[tbContact]
    ADD CONSTRAINT [DF_Subject_tbContact_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbContact_UpdatedBy]...';


go
ALTER TABLE [Subject].[tbContact]
    ADD CONSTRAINT [DF_Subject_tbContact_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbContact_UpdatedOn]...';


go
ALTER TABLE [Subject].[tbContact]
    ADD CONSTRAINT [DF_Subject_tbContact_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tb_SubjectTypeCode]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [DF_Subject_tb_SubjectTypeCode] DEFAULT ((1)) FOR [SubjectTypeCode];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tb_SubjectStatusCode]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [DF_Subject_tb_SubjectStatusCode] DEFAULT ((1)) FOR [SubjectStatusCode];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbSubject_ExpectedDays]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [DF_Subject_tbSubject_ExpectedDays] DEFAULT ((0)) FOR [ExpectedDays];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tb_PaymentDays]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [DF_Subject_tb_PaymentDays] DEFAULT ((0)) FOR [PaymentDays];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tb_PayDaysFromMonthEnd]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [DF_Subject_tb_PayDaysFromMonthEnd] DEFAULT ((0)) FOR [PayDaysFromMonthEnd];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbSubject_PayBalance]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [DF_Subject_tbSubject_PayBalance] DEFAULT ((1)) FOR [PayBalance];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tb_NumberOfEmployees]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [DF_Subject_tb_NumberOfEmployees] DEFAULT ((0)) FOR [NumberOfEmployees];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tb_EUJurisdiction]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [DF_Subject_tb_EUJurisdiction] DEFAULT ((0)) FOR [EUJurisdiction];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tb_InsertedBy]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [DF_Subject_tb_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tb_InsertedOn]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [DF_Subject_tb_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tb_UpdatedBy]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [DF_Subject_tb_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tb_UpdatedOn]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [DF_Subject_tb_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbSubject_TransmitStatusCode]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [DF_Subject_tbSubject_TransmitStatusCode] DEFAULT ((0)) FOR [TransmitStatusCode];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tb_OpeningBalance]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [DF_Subject_tb_OpeningBalance] DEFAULT ((0)) FOR [OpeningBalance];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tb_Turnover]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [DF_Subject_tb_Turnover] DEFAULT ((0)) FOR [Turnover];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbAccount_OpeningBalance]...';


go
ALTER TABLE [Subject].[tbAccount]
    ADD CONSTRAINT [DF_Subject_tbAccount_OpeningBalance] DEFAULT ((0)) FOR [OpeningBalance];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbAccount_CurrentBalance]...';


go
ALTER TABLE [Subject].[tbAccount]
    ADD CONSTRAINT [DF_Subject_tbAccount_CurrentBalance] DEFAULT ((0)) FOR [CurrentBalance];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbAccount_CoinTypeCode]...';


go
ALTER TABLE [Subject].[tbAccount]
    ADD CONSTRAINT [DF_Subject_tbAccount_CoinTypeCode] DEFAULT ((2)) FOR [CoinTypeCode];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbAccount_AccountTypeCode]...';


go
ALTER TABLE [Subject].[tbAccount]
    ADD CONSTRAINT [DF_Subject_tbAccount_AccountTypeCode] DEFAULT ((0)) FOR [AccountTypeCode];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbAccount_LiquidityLevel]...';


go
ALTER TABLE [Subject].[tbAccount]
    ADD CONSTRAINT [DF_Subject_tbAccount_LiquidityLevel] DEFAULT ((0)) FOR [LiquidityLevel];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbAccount_AccountClosed]...';


go
ALTER TABLE [Subject].[tbAccount]
    ADD CONSTRAINT [DF_Subject_tbAccount_AccountClosed] DEFAULT ((0)) FOR [AccountClosed];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbAccount_InsertedBy]...';


go
ALTER TABLE [Subject].[tbAccount]
    ADD CONSTRAINT [DF_Subject_tbAccount_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbAccount_InsertedOn]...';


go
ALTER TABLE [Subject].[tbAccount]
    ADD CONSTRAINT [DF_Subject_tbAccount_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbAccount_UpdatedBy]...';


go
ALTER TABLE [Subject].[tbAccount]
    ADD CONSTRAINT [DF_Subject_tbAccount_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Subject].[DF_Subject_tbAccount_UpdatedOn]...';


go
ALTER TABLE [Subject].[tbAccount]
    ADD CONSTRAINT [DF_Subject_tbAccount_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbFlow_StepNumber]...';


go
ALTER TABLE [Project].[tbFlow]
    ADD CONSTRAINT [DF_Project_tbFlow_StepNumber] DEFAULT ((10)) FOR [StepNumber];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbFlow_SyncTypeCode]...';


go
ALTER TABLE [Project].[tbFlow]
    ADD CONSTRAINT [DF_Project_tbFlow_SyncTypeCode] DEFAULT ((0)) FOR [SyncTypeCode];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbFlow_OffsetDays]...';


go
ALTER TABLE [Project].[tbFlow]
    ADD CONSTRAINT [DF_Project_tbFlow_OffsetDays] DEFAULT ((0)) FOR [OffsetDays];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbFlow_InsertedBy]...';


go
ALTER TABLE [Project].[tbFlow]
    ADD CONSTRAINT [DF_Project_tbFlow_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbFlow_InsertedOn]...';


go
ALTER TABLE [Project].[tbFlow]
    ADD CONSTRAINT [DF_Project_tbFlow_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbFlow_UpdatedBy]...';


go
ALTER TABLE [Project].[tbFlow]
    ADD CONSTRAINT [DF_Project_tbFlow_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbFlow_UpdatedOn]...';


go
ALTER TABLE [Project].[tbFlow]
    ADD CONSTRAINT [DF_Project_tbFlow_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbFlow_UsedOnQuantity]...';


go
ALTER TABLE [Project].[tbFlow]
    ADD CONSTRAINT [DF_Project_tbFlow_UsedOnQuantity] DEFAULT ((1)) FOR [UsedOnQuantity];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbDoc_InsertedBy]...';


go
ALTER TABLE [Project].[tbDoc]
    ADD CONSTRAINT [DF_Project_tbDoc_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbDoc_InsertedOn]...';


go
ALTER TABLE [Project].[tbDoc]
    ADD CONSTRAINT [DF_Project_tbDoc_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbDoc_UpdatedBy]...';


go
ALTER TABLE [Project].[tbDoc]
    ADD CONSTRAINT [DF_Project_tbDoc_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbDoc_UpdatedOn]...';


go
ALTER TABLE [Project].[tbDoc]
    ADD CONSTRAINT [DF_Project_tbDoc_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbAttribute_OrderBy]...';


go
ALTER TABLE [Project].[tbAttribute]
    ADD CONSTRAINT [DF_Project_tbAttribute_OrderBy] DEFAULT ((10)) FOR [PrintOrder];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbAttribute_AttributeTypeCode]...';


go
ALTER TABLE [Project].[tbAttribute]
    ADD CONSTRAINT [DF_Project_tbAttribute_AttributeTypeCode] DEFAULT ((0)) FOR [AttributeTypeCode];


go
PRINT N'Creating Default Constraint [Project].[DF_tbJobAttribute_InsertedBy]...';


go
ALTER TABLE [Project].[tbAttribute]
    ADD CONSTRAINT [DF_tbJobAttribute_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Project].[DF_tbJobAttribute_InsertedOn]...';


go
ALTER TABLE [Project].[tbAttribute]
    ADD CONSTRAINT [DF_tbJobAttribute_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Project].[DF_tbJobAttribute_UpdatedBy]...';


go
ALTER TABLE [Project].[tbAttribute]
    ADD CONSTRAINT [DF_tbJobAttribute_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Project].[DF_tbJobAttribute_UpdatedOn]...';


go
ALTER TABLE [Project].[tbAttribute]
    ADD CONSTRAINT [DF_tbJobAttribute_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbQuote_InsertedBy]...';


go
ALTER TABLE [Project].[tbQuote]
    ADD CONSTRAINT [DF_Project_tbQuote_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbQuote_InsertedOn]...';


go
ALTER TABLE [Project].[tbQuote]
    ADD CONSTRAINT [DF_Project_tbQuote_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbQuote_UpdatedBy]...';


go
ALTER TABLE [Project].[tbQuote]
    ADD CONSTRAINT [DF_Project_tbQuote_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbQuote_UpdatedOn]...';


go
ALTER TABLE [Project].[tbQuote]
    ADD CONSTRAINT [DF_Project_tbQuote_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbQuote_Quantity]...';


go
ALTER TABLE [Project].[tbQuote]
    ADD CONSTRAINT [DF_Project_tbQuote_Quantity] DEFAULT ((0)) FOR [Quantity];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbQuote_RunOnQuantity]...';


go
ALTER TABLE [Project].[tbQuote]
    ADD CONSTRAINT [DF_Project_tbQuote_RunOnQuantity] DEFAULT ((0)) FOR [RunOnQuantity];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbQuote_RunBackQuantity]...';


go
ALTER TABLE [Project].[tbQuote]
    ADD CONSTRAINT [DF_Project_tbQuote_RunBackQuantity] DEFAULT ((0)) FOR [RunBackQuantity];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbQuote_TotalPrice]...';


go
ALTER TABLE [Project].[tbQuote]
    ADD CONSTRAINT [DF_Project_tbQuote_TotalPrice] DEFAULT ((0)) FOR [TotalPrice];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbQuote_RunOnPrice]...';


go
ALTER TABLE [Project].[tbQuote]
    ADD CONSTRAINT [DF_Project_tbQuote_RunOnPrice] DEFAULT ((0)) FOR [RunOnPrice];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbQuote_RunBackPrice]...';


go
ALTER TABLE [Project].[tbQuote]
    ADD CONSTRAINT [DF_Project_tbQuote_RunBackPrice] DEFAULT ((0)) FOR [RunBackPrice];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbOp_SyncTypeCode]...';


go
ALTER TABLE [Project].[tbOp]
    ADD CONSTRAINT [DF_Project_tbOp_SyncTypeCode] DEFAULT ((0)) FOR [SyncTypeCode];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbOp_OpStatusCode]...';


go
ALTER TABLE [Project].[tbOp]
    ADD CONSTRAINT [DF_Project_tbOp_OpStatusCode] DEFAULT ((0)) FOR [OpStatusCode];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbOp_StartOn]...';


go
ALTER TABLE [Project].[tbOp]
    ADD CONSTRAINT [DF_Project_tbOp_StartOn] DEFAULT (getdate()) FOR [StartOn];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbOp_EndOn]...';


go
ALTER TABLE [Project].[tbOp]
    ADD CONSTRAINT [DF_Project_tbOp_EndOn] DEFAULT (getdate()) FOR [EndOn];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbOp_OffsetDays]...';


go
ALTER TABLE [Project].[tbOp]
    ADD CONSTRAINT [DF_Project_tbOp_OffsetDays] DEFAULT ((0)) FOR [OffsetDays];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbOp_InsertedBy]...';


go
ALTER TABLE [Project].[tbOp]
    ADD CONSTRAINT [DF_Project_tbOp_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbOp_InsertedOn]...';


go
ALTER TABLE [Project].[tbOp]
    ADD CONSTRAINT [DF_Project_tbOp_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbOp_UpdatedBy]...';


go
ALTER TABLE [Project].[tbOp]
    ADD CONSTRAINT [DF_Project_tbOp_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbOp_UpdatedOn]...';


go
ALTER TABLE [Project].[tbOp]
    ADD CONSTRAINT [DF_Project_tbOp_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbOp_Duration]...';


go
ALTER TABLE [Project].[tbOp]
    ADD CONSTRAINT [DF_Project_tbOp_Duration] DEFAULT ((0)) FOR [Duration];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbAllocationEvent_InsertedOn]...';


go
ALTER TABLE [Project].[tbAllocationEvent]
    ADD CONSTRAINT [DF_Project_tbAllocationEvent_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Project].[DF_tbAllocationEvent_UnitCharge]...';


go
ALTER TABLE [Project].[tbAllocationEvent]
    ADD CONSTRAINT [DF_tbAllocationEvent_UnitCharge] DEFAULT ((0)) FOR [UnitCharge];


go
PRINT N'Creating Default Constraint [Project].[Project_tbCostSet_InsertedBy]...';


go
ALTER TABLE [Project].[tbCostSet]
    ADD CONSTRAINT [Project_tbCostSet_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Project].[Project_tbCostSet_InsertedOn]...';


go
ALTER TABLE [Project].[tbCostSet]
    ADD CONSTRAINT [Project_tbCostSet_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbChangeLog_ChangedOn]...';


go
ALTER TABLE [Project].[tbChangeLog]
    ADD CONSTRAINT [DF_Project_tbChangeLog_ChangedOn] DEFAULT (dateadd(millisecond,datepart(millisecond,getdate())*(-1),getdate())) FOR [ChangedOn];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbChangeLog_TransmissionStatusCode]...';


go
ALTER TABLE [Project].[tbChangeLog]
    ADD CONSTRAINT [DF_Project_tbChangeLog_TransmissionStatusCode] DEFAULT ((0)) FOR [TransmitStatusCode];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbChangeLog_UpdatedBy]...';


go
ALTER TABLE [Project].[tbChangeLog]
    ADD CONSTRAINT [DF_Project_tbChangeLog_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbChangeLog_Quantity]...';


go
ALTER TABLE [Project].[tbChangeLog]
    ADD CONSTRAINT [DF_Project_tbChangeLog_Quantity] DEFAULT ((0)) FOR [Quantity];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbChangeLog_UnitCharge]...';


go
ALTER TABLE [Project].[tbChangeLog]
    ADD CONSTRAINT [DF_Project_tbChangeLog_UnitCharge] DEFAULT ((0)) FOR [UnitCharge];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbAllocation_InsertedOn]...';


go
ALTER TABLE [Project].[tbAllocation]
    ADD CONSTRAINT [DF_Project_tbAllocation_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbAllocation_UnitCharge]...';


go
ALTER TABLE [Project].[tbAllocation]
    ADD CONSTRAINT [DF_Project_tbAllocation_UnitCharge] DEFAULT ((0)) FOR [UnitCharge];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tbProject_ActionOn]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [DF_Project_tbProject_ActionOn] DEFAULT (getdate()) FOR [ActionOn];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tb_PaymentOn]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [DF_Project_tb_PaymentOn] DEFAULT (getdate()) FOR [PaymentOn];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tb_Spooled]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [DF_Project_tb_Spooled] DEFAULT ((0)) FOR [Spooled];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tb_Printed]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [DF_Project_tb_Printed] DEFAULT ((0)) FOR [Printed];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tb_InsertedBy]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [DF_Project_tb_InsertedBy] DEFAULT (suser_sname()) FOR [InsertedBy];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tb_InsertedOn]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [DF_Project_tb_InsertedOn] DEFAULT (getdate()) FOR [InsertedOn];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tb_UpdatedBy]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [DF_Project_tb_UpdatedBy] DEFAULT (suser_sname()) FOR [UpdatedBy];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tb_UpdatedOn]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [DF_Project_tb_UpdatedOn] DEFAULT (getdate()) FOR [UpdatedOn];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tb_Quantity]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [DF_Project_tb_Quantity] DEFAULT ((0)) FOR [Quantity];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tb_TotalCharge]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [DF_Project_tb_TotalCharge] DEFAULT ((0)) FOR [TotalCharge];


go
PRINT N'Creating Default Constraint [Project].[DF_Project_tb_UnitCharge]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [DF_Project_tb_UnitCharge] DEFAULT ((0)) FOR [UnitCharge];


go
PRINT N'Creating Foreign Key [dbo].[FK_AspNetUserTokens_AspNetUsers_UserId]...';


go
ALTER TABLE [dbo].[AspNetUserTokens]
    ADD CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;


go
PRINT N'Creating Foreign Key [dbo].[FK_AspNetUserRoles_AspNetRoles_RoleId]...';


go
ALTER TABLE [dbo].[AspNetUserRoles]
    ADD CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE;


go
PRINT N'Creating Foreign Key [dbo].[FK_AspNetUserRoles_AspNetUsers_UserId]...';


go
ALTER TABLE [dbo].[AspNetUserRoles]
    ADD CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;


go
PRINT N'Creating Foreign Key [dbo].[FK_AspNetUserLogins_AspNetUsers_UserId]...';


go
ALTER TABLE [dbo].[AspNetUserLogins]
    ADD CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;


go
PRINT N'Creating Foreign Key [dbo].[FK_AspNetUserClaims_AspNetUsers_UserId]...';


go
ALTER TABLE [dbo].[AspNetUserClaims]
    ADD CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;


go
PRINT N'Creating Foreign Key [dbo].[FK_AspNetRoleClaims_AspNetRoles_RoleId]...';


go
ALTER TABLE [dbo].[AspNetRoleClaims]
    ADD CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE;


go
PRINT N'Creating Foreign Key [Web].[FK_tbTemplateImage_tbImage]...';


go
ALTER TABLE [Web].[tbTemplateImage]
    ADD CONSTRAINT [FK_tbTemplateImage_tbImage] FOREIGN KEY ([ImageTag]) REFERENCES [Web].[tbImage] ([ImageTag]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Web].[FK_tbTemplateImage_tbTemplate]...';


go
ALTER TABLE [Web].[tbTemplateImage]
    ADD CONSTRAINT [FK_tbTemplateImage_tbTemplate] FOREIGN KEY ([TemplateId]) REFERENCES [Web].[tbTemplate] ([TemplateId]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Web].[FK_tbAttachmentInvoice_tbAttachment]...';


go
ALTER TABLE [Web].[tbAttachmentInvoice]
    ADD CONSTRAINT [FK_tbAttachmentInvoice_tbAttachment] FOREIGN KEY ([AttachmentId]) REFERENCES [Web].[tbAttachment] ([AttachmentId]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Web].[FK_tbAttachmentInvoice_tbType]...';


go
ALTER TABLE [Web].[tbAttachmentInvoice]
    ADD CONSTRAINT [FK_tbAttachmentInvoice_tbType] FOREIGN KEY ([InvoiceTypeCode]) REFERENCES [Invoice].[tbType] ([InvoiceTypeCode]);


go
PRINT N'Creating Foreign Key [Web].[FK_tbTemplateInvoice_tbTemplate]...';


go
ALTER TABLE [Web].[tbTemplateInvoice]
    ADD CONSTRAINT [FK_tbTemplateInvoice_tbTemplate] FOREIGN KEY ([TemplateId]) REFERENCES [Web].[tbTemplate] ([TemplateId]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Web].[FK_tbTemplateInvoice_tbType]...';


go
ALTER TABLE [Web].[tbTemplateInvoice]
    ADD CONSTRAINT [FK_tbTemplateInvoice_tbType] FOREIGN KEY ([InvoiceTypeCode]) REFERENCES [Invoice].[tbType] ([InvoiceTypeCode]);


go
PRINT N'Creating Foreign Key [Usr].[FK_Usr_tb_App_tbCalendar]...';


go
ALTER TABLE [Usr].[tbUser]
    ADD CONSTRAINT [FK_Usr_tb_App_tbCalendar] FOREIGN KEY ([CalendarCode]) REFERENCES [App].[tbCalendar] ([CalendarCode]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Usr].[FK_Usr_tbMenu_Usr_tbUser]...';


go
ALTER TABLE [Usr].[tbUser]
    ADD CONSTRAINT [FK_Usr_tbMenu_Usr_tbUser] FOREIGN KEY ([MenuViewCode]) REFERENCES [Usr].[tbMenuView] ([MenuViewCode]);


go
PRINT N'Creating Foreign Key [Usr].[FK_Usr_tbMenu_Usr_tb]...';


go
ALTER TABLE [Usr].[tbMenuUser]
    ADD CONSTRAINT [FK_Usr_tbMenu_Usr_tb] FOREIGN KEY ([UserId]) REFERENCES [Usr].[tbUser] ([UserId]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Usr].[FK_Usr_tbMenu_Usr_tbMenu]...';


go
ALTER TABLE [Usr].[tbMenuUser]
    ADD CONSTRAINT [FK_Usr_tbMenu_Usr_tbMenu] FOREIGN KEY ([MenuId]) REFERENCES [Usr].[tbMenu] ([MenuId]);


go
PRINT N'Creating Foreign Key [Usr].[FK_Usr_tbMenu_Usr_tbInterface]...';


go
ALTER TABLE [Usr].[tbMenu]
    ADD CONSTRAINT [FK_Usr_tbMenu_Usr_tbInterface] FOREIGN KEY ([InterfaceCode]) REFERENCES [Usr].[tbInterface] ([InterfaceCode]);


go
PRINT N'Creating Foreign Key [Usr].[FK_Usr_tbMenuEntry_Usr_tbMenu]...';


go
ALTER TABLE [Usr].[tbMenuEntry]
    ADD CONSTRAINT [FK_Usr_tbMenuEntry_Usr_tbMenu] FOREIGN KEY ([MenuId]) REFERENCES [Usr].[tbMenu] ([MenuId]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Usr].[FK_Usr_tbMenuEntry_tbMenuCommand]...';


go
ALTER TABLE [Usr].[tbMenuEntry]
    ADD CONSTRAINT [FK_Usr_tbMenuEntry_tbMenuCommand] FOREIGN KEY ([Command]) REFERENCES [Usr].[tbMenuCommand] ([Command]);


go
PRINT N'Creating Foreign Key [Usr].[FK_Usr_tbMenuEntry_tbMenuOpenMode]...';


go
ALTER TABLE [Usr].[tbMenuEntry]
    ADD CONSTRAINT [FK_Usr_tbMenuEntry_tbMenuOpenMode] FOREIGN KEY ([OpenMode]) REFERENCES [Usr].[tbMenuOpenMode] ([OpenMode]);


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbMirrorEvent_ContractAddress]...';


go
ALTER TABLE [Invoice].[tbMirrorEvent]
    ADD CONSTRAINT [FK_Invoice_tbMirrorEvent_ContractAddress] FOREIGN KEY ([ContractAddress]) REFERENCES [Invoice].[tbMirror] ([ContractAddress]);


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbMirrorEvent_EventTypeCode]...';


go
ALTER TABLE [Invoice].[tbMirrorEvent]
    ADD CONSTRAINT [FK_Invoice_tbMirrorEvent_EventTypeCode] FOREIGN KEY ([EventTypeCode]) REFERENCES [App].[tbEventType] ([EventTypeCode]);


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbMirrorItem_ContractAddress]...';


go
ALTER TABLE [Invoice].[tbMirrorItem]
    ADD CONSTRAINT [FK_Invoice_tbMirrorItem_ContractAddress] FOREIGN KEY ([ContractAddress]) REFERENCES [Invoice].[tbMirror] ([ContractAddress]) ON DELETE CASCADE;


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbChangeLog_tbInvoice]...';


go
ALTER TABLE [Invoice].[tbChangeLog]
    ADD CONSTRAINT [FK_Invoice_tbChangeLog_tbInvoice] FOREIGN KEY ([InvoiceNumber]) REFERENCES [Invoice].[tbInvoice] ([InvoiceNumber]) ON DELETE CASCADE;


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbChangeLog_TrasmitStatusCode]...';


go
ALTER TABLE [Invoice].[tbChangeLog]
    ADD CONSTRAINT [FK_Invoice_tbChangeLog_TrasmitStatusCode] FOREIGN KEY ([TransmitStatusCode]) REFERENCES [Subject].[tbTransmitStatus] ([TransmitStatusCode]);


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbType_Cash_tbPolarity]...';


go
ALTER TABLE [Invoice].[tbType]
    ADD CONSTRAINT [FK_Invoice_tbType_Cash_tbPolarity] FOREIGN KEY ([CashPolarityCode]) REFERENCES [Cash].[tbPolarity] ([CashPolarityCode]);


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbMirrorReference_tbInvoice]...';


go
ALTER TABLE [Invoice].[tbMirrorReference]
    ADD CONSTRAINT [FK_Invoice_tbMirrorReference_tbInvoice] FOREIGN KEY ([InvoiceNumber]) REFERENCES [Invoice].[tbInvoice] ([InvoiceNumber]) ON DELETE CASCADE;


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbMirrorReference_tbMirror]...';


go
ALTER TABLE [Invoice].[tbMirrorReference]
    ADD CONSTRAINT [FK_Invoice_tbMirrorReference_tbMirror] FOREIGN KEY ([ContractAddress]) REFERENCES [Invoice].[tbMirror] ([ContractAddress]) ON DELETE CASCADE;


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbProject_App_tbTaxCode]...';


go
ALTER TABLE [Invoice].[tbProject]
    ADD CONSTRAINT [FK_Invoice_tbProject_App_tbTaxCode] FOREIGN KEY ([TaxCode]) REFERENCES [App].[tbTaxCode] ([TaxCode]);


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbProject_Cash_tbCode]...';


go
ALTER TABLE [Invoice].[tbProject]
    ADD CONSTRAINT [FK_Invoice_tbProject_Cash_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]);


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbProject_Invoice_tb]...';


go
ALTER TABLE [Invoice].[tbProject]
    ADD CONSTRAINT [FK_Invoice_tbProject_Invoice_tb] FOREIGN KEY ([InvoiceNumber]) REFERENCES [Invoice].[tbInvoice] ([InvoiceNumber]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbProject_Project_tb]...';


go
ALTER TABLE [Invoice].[tbProject]
    ADD CONSTRAINT [FK_Invoice_tbProject_Project_tb] FOREIGN KEY ([ProjectCode]) REFERENCES [Project].[tbProject] ([ProjectCode]);


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbItem_App_tbTaxCode]...';


go
ALTER TABLE [Invoice].[tbItem]
    ADD CONSTRAINT [FK_Invoice_tbItem_App_tbTaxCode] FOREIGN KEY ([TaxCode]) REFERENCES [App].[tbTaxCode] ([TaxCode]);


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbItem_Cash_tbCode]...';


go
ALTER TABLE [Invoice].[tbItem]
    ADD CONSTRAINT [FK_Invoice_tbItem_Cash_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbItem_Invoice_tb]...';


go
ALTER TABLE [Invoice].[tbItem]
    ADD CONSTRAINT [FK_Invoice_tbItem_Invoice_tb] FOREIGN KEY ([InvoiceNumber]) REFERENCES [Invoice].[tbInvoice] ([InvoiceNumber]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tb_Invoice_tbStatus]...';


go
ALTER TABLE [Invoice].[tbInvoice]
    ADD CONSTRAINT [FK_Invoice_tb_Invoice_tbStatus] FOREIGN KEY ([InvoiceStatusCode]) REFERENCES [Invoice].[tbStatus] ([InvoiceStatusCode]);


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tb_Invoice_tbType]...';


go
ALTER TABLE [Invoice].[tbInvoice]
    ADD CONSTRAINT [FK_Invoice_tb_Invoice_tbType] FOREIGN KEY ([InvoiceTypeCode]) REFERENCES [Invoice].[tbType] ([InvoiceTypeCode]);


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tb_Subject_tb]...';


go
ALTER TABLE [Invoice].[tbInvoice]
    ADD CONSTRAINT [FK_Invoice_tb_Subject_tb] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]);


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tb_Usr_tb]...';


go
ALTER TABLE [Invoice].[tbInvoice]
    ADD CONSTRAINT [FK_Invoice_tb_Usr_tb] FOREIGN KEY ([UserId]) REFERENCES [Usr].[tbUser] ([UserId]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbEntry_App_tbTaxCode]...';


go
ALTER TABLE [Invoice].[tbEntry]
    ADD CONSTRAINT [FK_Invoice_tbEntry_App_tbTaxCode] FOREIGN KEY ([TaxCode]) REFERENCES [App].[tbTaxCode] ([TaxCode]);


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbEntry_Cash_tbCode]...';


go
ALTER TABLE [Invoice].[tbEntry]
    ADD CONSTRAINT [FK_Invoice_tbEntry_Cash_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbEntry_Invoice_tbType]...';


go
ALTER TABLE [Invoice].[tbEntry]
    ADD CONSTRAINT [FK_Invoice_tbEntry_Invoice_tbType] FOREIGN KEY ([InvoiceTypeCode]) REFERENCES [Invoice].[tbType] ([InvoiceTypeCode]);


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbEntry_Subject_tb]...';


go
ALTER TABLE [Invoice].[tbEntry]
    ADD CONSTRAINT [FK_Invoice_tbEntry_Subject_tb] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]);


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbEntry_Usr_tb]...';


go
ALTER TABLE [Invoice].[tbEntry]
    ADD CONSTRAINT [FK_Invoice_tbEntry_Usr_tb] FOREIGN KEY ([UserId]) REFERENCES [Usr].[tbUser] ([UserId]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbMirrorProject_ContractAddress]...';


go
ALTER TABLE [Invoice].[tbMirrorProject]
    ADD CONSTRAINT [FK_Invoice_tbMirrorProject_ContractAddress] FOREIGN KEY ([ContractAddress]) REFERENCES [Invoice].[tbMirror] ([ContractAddress]) ON DELETE CASCADE;


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbMirror_tbSubject]...';


go
ALTER TABLE [Invoice].[tbMirror]
    ADD CONSTRAINT [FK_Invoice_tbMirror_tbSubject] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]);


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbMirror_tbStatus]...';


go
ALTER TABLE [Invoice].[tbMirror]
    ADD CONSTRAINT [FK_Invoice_tbMirror_tbStatus] FOREIGN KEY ([InvoiceStatusCode]) REFERENCES [Invoice].[tbStatus] ([InvoiceStatusCode]);


go
PRINT N'Creating Foreign Key [Invoice].[FK_Invoice_tbMirror_tbType]...';


go
ALTER TABLE [Invoice].[tbMirror]
    ADD CONSTRAINT [FK_Invoice_tbMirror_tbType] FOREIGN KEY ([InvoiceTypeCode]) REFERENCES [Invoice].[tbType] ([InvoiceTypeCode]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbTx_Cash_tbChange]...';


go
ALTER TABLE [Cash].[tbTx]
    ADD CONSTRAINT [FK_Cash_tbTx_Cash_tbChange] FOREIGN KEY ([PaymentAddress]) REFERENCES [Cash].[tbChange] ([PaymentAddress]) ON DELETE CASCADE;


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbTx_Cash_tbTxStatus]...';


go
ALTER TABLE [Cash].[tbTx]
    ADD CONSTRAINT [FK_Cash_tbTx_Cash_tbTxStatus] FOREIGN KEY ([TxStatusCode]) REFERENCES [Cash].[tbTxStatus] ([TxStatusCode]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbChangeReferencee_Cash_tbChange]...';


go
ALTER TABLE [Cash].[tbChangeReference]
    ADD CONSTRAINT [FK_Cash_tbChangeReferencee_Cash_tbChange] FOREIGN KEY ([PaymentAddress]) REFERENCES [Cash].[tbChange] ([PaymentAddress]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbChangeReferencee_Invoice_tbInvoice]...';


go
ALTER TABLE [Cash].[tbChangeReference]
    ADD CONSTRAINT [FK_Cash_tbChangeReferencee_Invoice_tbInvoice] FOREIGN KEY ([InvoiceNumber]) REFERENCES [Invoice].[tbInvoice] ([InvoiceNumber]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbTxReference_Cash_tbPayment]...';


go
ALTER TABLE [Cash].[tbTxReference]
    ADD CONSTRAINT [FK_Cash_tbTxReference_Cash_tbPayment] FOREIGN KEY ([PaymentCode]) REFERENCES [Cash].[tbPayment] ([PaymentCode]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbTxReference_Cash_tbTx]...';


go
ALTER TABLE [Cash].[tbTxReference]
    ADD CONSTRAINT [FK_Cash_tbTxReference_Cash_tbTx] FOREIGN KEY ([TxNumber]) REFERENCES [Cash].[tbTx] ([TxNumber]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbTxReference_Cash_tbTxStatus]...';


go
ALTER TABLE [Cash].[tbTxReference]
    ADD CONSTRAINT [FK_Cash_tbTxReference_Cash_tbTxStatus] FOREIGN KEY ([TxStatusCode]) REFERENCES [Cash].[tbTxStatus] ([TxStatusCode]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbCode_App_tbTaxCode]...';


go
ALTER TABLE [Cash].[tbCode]
    ADD CONSTRAINT [FK_Cash_tbCode_App_tbTaxCode] FOREIGN KEY ([TaxCode]) REFERENCES [App].[tbTaxCode] ([TaxCode]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbCode_Cash_tbCategory1]...';


go
ALTER TABLE [Cash].[tbCode]
    ADD CONSTRAINT [FK_Cash_tbCode_Cash_tbCategory1] FOREIGN KEY ([CategoryCode]) REFERENCES [Cash].[tbCategory] ([CategoryCode]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbCategoryTotal_Cash_tbCategory_Child]...';


go
ALTER TABLE [Cash].[tbCategoryTotal]
    ADD CONSTRAINT [FK_Cash_tbCategoryTotal_Cash_tbCategory_Child] FOREIGN KEY ([ChildCode]) REFERENCES [Cash].[tbCategory] ([CategoryCode]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbCategoryTotal_Cash_tbCategory_Parent]...';


go
ALTER TABLE [Cash].[tbCategoryTotal]
    ADD CONSTRAINT [FK_Cash_tbCategoryTotal_Cash_tbCategory_Parent] FOREIGN KEY ([ParentCode]) REFERENCES [Cash].[tbCategory] ([CategoryCode]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbCategoryExp_Cash_tbCategory]...';


go
ALTER TABLE [Cash].[tbCategoryExp]
    ADD CONSTRAINT [FK_Cash_tbCategoryExp_Cash_tbCategory] FOREIGN KEY ([CategoryCode]) REFERENCES [Cash].[tbCategory] ([CategoryCode]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbCategory_Cash_tbCategoryType]...';


go
ALTER TABLE [Cash].[tbCategory]
    ADD CONSTRAINT [FK_Cash_tbCategory_Cash_tbCategoryType] FOREIGN KEY ([CategoryTypeCode]) REFERENCES [Cash].[tbCategoryType] ([CategoryTypeCode]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbCategory_Cash_tbPolarity]...';


go
ALTER TABLE [Cash].[tbCategory]
    ADD CONSTRAINT [FK_Cash_tbCategory_Cash_tbPolarity] FOREIGN KEY ([CashPolarityCode]) REFERENCES [Cash].[tbPolarity] ([CashPolarityCode]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbCategory_Cash_tbType]...';


go
ALTER TABLE [Cash].[tbCategory]
    ADD CONSTRAINT [FK_Cash_tbCategory_Cash_tbType] FOREIGN KEY ([CashTypeCode]) REFERENCES [Cash].[tbType] ([CashTypeCode]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbPeriod_App_tbYearPeriod]...';


go
ALTER TABLE [Cash].[tbPeriod]
    ADD CONSTRAINT [FK_Cash_tbPeriod_App_tbYearPeriod] FOREIGN KEY ([StartOn]) REFERENCES [App].[tbYearPeriod] ([StartOn]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbPeriod_Cash_tbCode]...';


go
ALTER TABLE [Cash].[tbPeriod]
    ADD CONSTRAINT [FK_Cash_tbPeriod_Cash_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbMirror_tbCode]...';


go
ALTER TABLE [Cash].[tbMirror]
    ADD CONSTRAINT [FK_Cash_tbMirror_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbMirror_tbSubject]...';


go
ALTER TABLE [Cash].[tbMirror]
    ADD CONSTRAINT [FK_Cash_tbMirror_tbSubject] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbMirror_tbTransmitStatus]...';


go
ALTER TABLE [Cash].[tbMirror]
    ADD CONSTRAINT [FK_Cash_tbMirror_tbTransmitStatus] FOREIGN KEY ([TransmitStatusCode]) REFERENCES [Subject].[tbTransmitStatus] ([TransmitStatusCode]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbTaxType_App_tbMonth]...';


go
ALTER TABLE [Cash].[tbTaxType]
    ADD CONSTRAINT [FK_Cash_tbTaxType_App_tbMonth] FOREIGN KEY ([MonthNumber]) REFERENCES [App].[tbMonth] ([MonthNumber]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbTaxType_App_tbRecurrence]...';


go
ALTER TABLE [Cash].[tbTaxType]
    ADD CONSTRAINT [FK_Cash_tbTaxType_App_tbRecurrence] FOREIGN KEY ([RecurrenceCode]) REFERENCES [App].[tbRecurrence] ([RecurrenceCode]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbTaxType_Cash_tbCode]...';


go
ALTER TABLE [Cash].[tbTaxType]
    ADD CONSTRAINT [FK_Cash_tbTaxType_Cash_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbTaxType_Subject_tb]...';


go
ALTER TABLE [Cash].[tbTaxType]
    ADD CONSTRAINT [FK_Cash_tbTaxType_Subject_tb] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Cash].[FK__Cash_tbChange_Cash_tbChangeType]...';


go
ALTER TABLE [Cash].[tbChange]
    ADD CONSTRAINT [FK__Cash_tbChange_Cash_tbChangeType] FOREIGN KEY ([ChangeTypeCode]) REFERENCES [Cash].[tbChangeType] ([ChangeTypeCode]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbChange_Subject_tbAccountKey]...';


go
ALTER TABLE [Cash].[tbChange]
    ADD CONSTRAINT [FK_Cash_tbChange_Subject_tbAccountKey] FOREIGN KEY ([AccountCode], [HDPath]) REFERENCES [Subject].[tbAccountKey] ([AccountCode], [HDPath]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbPayment_App_tbTaxCode]...';


go
ALTER TABLE [Cash].[tbPayment]
    ADD CONSTRAINT [FK_Cash_tbPayment_App_tbTaxCode] FOREIGN KEY ([TaxCode]) REFERENCES [App].[tbTaxCode] ([TaxCode]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbPayment_Cash_tbCode]...';


go
ALTER TABLE [Cash].[tbPayment]
    ADD CONSTRAINT [FK_Cash_tbPayment_Cash_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbPayment_Cash_tbPaymentStatus]...';


go
ALTER TABLE [Cash].[tbPayment]
    ADD CONSTRAINT [FK_Cash_tbPayment_Cash_tbPaymentStatus] FOREIGN KEY ([PaymentStatusCode]) REFERENCES [Cash].[tbPaymentStatus] ([PaymentStatusCode]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbPayment_Subject_tbAccount]...';


go
ALTER TABLE [Cash].[tbPayment]
    ADD CONSTRAINT [FK_Cash_tbPayment_Subject_tbAccount] FOREIGN KEY ([AccountCode]) REFERENCES [Subject].[tbAccount] ([AccountCode]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbPayment_tbSubject]...';


go
ALTER TABLE [Cash].[tbPayment]
    ADD CONSTRAINT [FK_Cash_tbPayment_tbSubject] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]);


go
PRINT N'Creating Foreign Key [Cash].[FK_Cash_tbPayment_Usr_tbUser]...';


go
ALTER TABLE [Cash].[tbPayment]
    ADD CONSTRAINT [FK_Cash_tbPayment_Usr_tbUser] FOREIGN KEY ([UserId]) REFERENCES [Usr].[tbUser] ([UserId]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key unnamed constraint on [App].[tbEventLog]...';


go
ALTER TABLE [App].[tbEventLog]
    ADD FOREIGN KEY ([EventTypeCode]) REFERENCES [App].[tbEventType] ([EventTypeCode]);


go
PRINT N'Creating Foreign Key [App].[FK_App_tbDocType_App_tbDocClass]...';


go
ALTER TABLE [App].[tbDocType]
    ADD CONSTRAINT [FK_App_tbDocType_App_tbDocClass] FOREIGN KEY ([DocClassCode]) REFERENCES [App].[tbDocClass] ([DocClassCode]);


go
PRINT N'Creating Foreign Key [App].[FK_App_tbDocSpool_App_tbDocType]...';


go
ALTER TABLE [App].[tbDocSpool]
    ADD CONSTRAINT [FK_App_tbDocSpool_App_tbDocType] FOREIGN KEY ([DocTypeCode]) REFERENCES [App].[tbDocType] ([DocTypeCode]);


go
PRINT N'Creating Foreign Key [App].[FK_App_tbDoc_Usr_tbMenuOpenMode]...';


go
ALTER TABLE [App].[tbDoc]
    ADD CONSTRAINT [FK_App_tbDoc_Usr_tbMenuOpenMode] FOREIGN KEY ([OpenMode]) REFERENCES [Usr].[tbMenuOpenMode] ([OpenMode]);


go
PRINT N'Creating Foreign Key [App].[FK_App_tbYearPeriod_App_tbMonth]...';


go
ALTER TABLE [App].[tbYearPeriod]
    ADD CONSTRAINT [FK_App_tbYearPeriod_App_tbMonth] FOREIGN KEY ([MonthNumber]) REFERENCES [App].[tbMonth] ([MonthNumber]);


go
PRINT N'Creating Foreign Key [App].[FK_App_tbYearPeriod_App_tbYear]...';


go
ALTER TABLE [App].[tbYearPeriod]
    ADD CONSTRAINT [FK_App_tbYearPeriod_App_tbYear] FOREIGN KEY ([YearNumber]) REFERENCES [App].[tbYear] ([YearNumber]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [App].[FK_App_tbYearPeriod_Cash_tbStatus]...';


go
ALTER TABLE [App].[tbYearPeriod]
    ADD CONSTRAINT [FK_App_tbYearPeriod_Cash_tbStatus] FOREIGN KEY ([CashStatusCode]) REFERENCES [Cash].[tbStatus] ([CashStatusCode]);


go
PRINT N'Creating Foreign Key [App].[FK_App_tbYear_App_tbMonth]...';


go
ALTER TABLE [App].[tbYear]
    ADD CONSTRAINT [FK_App_tbYear_App_tbMonth] FOREIGN KEY ([StartMonth]) REFERENCES [App].[tbMonth] ([MonthNumber]);


go
PRINT N'Creating Foreign Key [App].[FK_App_tbTaxCode_App_tbRounding]...';


go
ALTER TABLE [App].[tbTaxCode]
    ADD CONSTRAINT [FK_App_tbTaxCode_App_tbRounding] FOREIGN KEY ([RoundingCode]) REFERENCES [App].[tbRounding] ([RoundingCode]);


go
PRINT N'Creating Foreign Key [App].[FK_App_tbTaxCode_Cash_tbTaxType]...';


go
ALTER TABLE [App].[tbTaxCode]
    ADD CONSTRAINT [FK_App_tbTaxCode_Cash_tbTaxType] FOREIGN KEY ([TaxTypeCode]) REFERENCES [Cash].[tbTaxType] ([TaxTypeCode]);


go
PRINT N'Creating Foreign Key [App].[FK_App_tbCalendarHoliday_tbCalendar]...';


go
ALTER TABLE [App].[tbCalendarHoliday]
    ADD CONSTRAINT [FK_App_tbCalendarHoliday_tbCalendar] FOREIGN KEY ([CalendarCode]) REFERENCES [App].[tbCalendar] ([CalendarCode]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [App].[FK_App_tbOption_Cash_tbCategory]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [FK_App_tbOption_Cash_tbCategory] FOREIGN KEY ([NetProfitCode]) REFERENCES [Cash].[tbCategory] ([CategoryCode]);


go
PRINT N'Creating Foreign Key [App].[FK_App_tbOptions_App_tbBucketInterval]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [FK_App_tbOptions_App_tbBucketInterval] FOREIGN KEY ([BucketIntervalCode]) REFERENCES [App].[tbBucketInterval] ([BucketIntervalCode]);


go
PRINT N'Creating Foreign Key [App].[FK_App_tbOptions_App_tbBucketType]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [FK_App_tbOptions_App_tbBucketType] FOREIGN KEY ([BucketTypeCode]) REFERENCES [App].[tbBucketType] ([BucketTypeCode]);


go
PRINT N'Creating Foreign Key [App].[FK_App_tbOptions_App_tbHost]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [FK_App_tbOptions_App_tbHost] FOREIGN KEY ([HostId]) REFERENCES [App].[tbHost] ([HostId]);


go
PRINT N'Creating Foreign Key [App].[FK_App_tbOptions_App_tbRegister]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [FK_App_tbOptions_App_tbRegister] FOREIGN KEY ([RegisterName]) REFERENCES [App].[tbRegister] ([RegisterName]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [App].[FK_App_tbOptions_Cash_tbCode]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [FK_App_tbOptions_Cash_tbCode] FOREIGN KEY ([MinerFeeCode]) REFERENCES [Cash].[tbCode] ([CashCode]);


go
PRINT N'Creating Foreign Key [App].[FK_App_tbOptions_Cash_tbCoinType]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [FK_App_tbOptions_Cash_tbCoinType] FOREIGN KEY ([CoinTypeCode]) REFERENCES [Cash].[tbCoinType] ([CoinTypeCode]);


go
PRINT N'Creating Foreign Key [App].[FK_App_tbOptions_Subject_tb]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [FK_App_tbOptions_Subject_tb] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [App].[FK_App_tbOptions_Subject_tbSubject]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [FK_App_tbOptions_Subject_tbSubject] FOREIGN KEY ([MinerAccountCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]);


go
PRINT N'Creating Foreign Key [App].[FK_App_tbUoc_UnitOfCharge]...';


go
ALTER TABLE [App].[tbOptions]
    ADD CONSTRAINT [FK_App_tbUoc_UnitOfCharge] FOREIGN KEY ([UnitOfCharge]) REFERENCES [App].[tbUoc] ([UnitOfCharge]);


go
PRINT N'Creating Foreign Key [Object].[FK_Object_tbOp_Object_tbSyncType]...';


go
ALTER TABLE [Object].[tbOp]
    ADD CONSTRAINT [FK_Object_tbOp_Object_tbSyncType] FOREIGN KEY ([SyncTypeCode]) REFERENCES [Object].[tbSyncType] ([SyncTypeCode]);


go
PRINT N'Creating Foreign Key [Object].[FK_Object_tbOp_tbObject]...';


go
ALTER TABLE [Object].[tbOp]
    ADD CONSTRAINT [FK_Object_tbOp_tbObject] FOREIGN KEY ([ObjectCode]) REFERENCES [Object].[tbObject] ([ObjectCode]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Object].[FK_Object_tbFlow_Object_tbChild]...';


go
ALTER TABLE [Object].[tbFlow]
    ADD CONSTRAINT [FK_Object_tbFlow_Object_tbChild] FOREIGN KEY ([ChildCode]) REFERENCES [Object].[tbObject] ([ObjectCode]);


go
PRINT N'Creating Foreign Key [Object].[FK_Object_tbFlow_Object_tbSyncType]...';


go
ALTER TABLE [Object].[tbFlow]
    ADD CONSTRAINT [FK_Object_tbFlow_Object_tbSyncType] FOREIGN KEY ([SyncTypeCode]) REFERENCES [Object].[tbSyncType] ([SyncTypeCode]);


go
PRINT N'Creating Foreign Key [Object].[FK_Object_tbFlow_tbObjectParent]...';


go
ALTER TABLE [Object].[tbFlow]
    ADD CONSTRAINT [FK_Object_tbFlow_tbObjectParent] FOREIGN KEY ([ParentCode]) REFERENCES [Object].[tbObject] ([ObjectCode]);


go
PRINT N'Creating Foreign Key [Object].[FK_Object_tbAttribute_Object_tbAttributeType]...';


go
ALTER TABLE [Object].[tbAttribute]
    ADD CONSTRAINT [FK_Object_tbAttribute_Object_tbAttributeType] FOREIGN KEY ([AttributeTypeCode]) REFERENCES [Object].[tbAttributeType] ([AttributeTypeCode]);


go
PRINT N'Creating Foreign Key [Object].[FK_Object_tbAttribute_tbObject]...';


go
ALTER TABLE [Object].[tbAttribute]
    ADD CONSTRAINT [FK_Object_tbAttribute_tbObject] FOREIGN KEY ([ObjectCode]) REFERENCES [Object].[tbObject] ([ObjectCode]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Object].[FK_Object_tbObject_App_tbRegister]...';


go
ALTER TABLE [Object].[tbObject]
    ADD CONSTRAINT [FK_Object_tbObject_App_tbRegister] FOREIGN KEY ([RegisterName]) REFERENCES [App].[tbRegister] ([RegisterName]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Object].[FK_Object_tbObject_App_tbUom]...';


go
ALTER TABLE [Object].[tbObject]
    ADD CONSTRAINT [FK_Object_tbObject_App_tbUom] FOREIGN KEY ([UnitOfMeasure]) REFERENCES [App].[tbUom] ([UnitOfMeasure]);


go
PRINT N'Creating Foreign Key [Object].[FK_Object_tbObject_Cash_tbCode]...';


go
ALTER TABLE [Object].[tbObject]
    ADD CONSTRAINT [FK_Object_tbObject_Cash_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Object].[FK_Object_tbMirror_tbObject]...';


go
ALTER TABLE [Object].[tbMirror]
    ADD CONSTRAINT [FK_Object_tbMirror_tbObject] FOREIGN KEY ([ObjectCode]) REFERENCES [Object].[tbObject] ([ObjectCode]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Object].[FK_Object_tbMirror_tbSubject]...';


go
ALTER TABLE [Object].[tbMirror]
    ADD CONSTRAINT [FK_Object_tbMirror_tbSubject] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Object].[FK_Object_tbMirror_tbTransmitStatus]...';


go
ALTER TABLE [Object].[tbMirror]
    ADD CONSTRAINT [FK_Object_tbMirror_tbTransmitStatus] FOREIGN KEY ([TransmitStatusCode]) REFERENCES [Subject].[tbTransmitStatus] ([TransmitStatusCode]);


go
PRINT N'Creating Foreign Key [Subject].[FK_Subject_tbType_Cash_tbPolarity]...';


go
ALTER TABLE [Subject].[tbType]
    ADD CONSTRAINT [FK_Subject_tbType_Cash_tbPolarity] FOREIGN KEY ([CashPolarityCode]) REFERENCES [Cash].[tbPolarity] ([CashPolarityCode]);


go
PRINT N'Creating Foreign Key [Subject].[FK_Subject_tbAddress_Subject_tb]...';


go
ALTER TABLE [Subject].[tbAddress]
    ADD CONSTRAINT [FK_Subject_tbAddress_Subject_tb] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Subject].[FK_Subject_tbDoc_AccountCode]...';


go
ALTER TABLE [Subject].[tbDoc]
    ADD CONSTRAINT [FK_Subject_tbDoc_AccountCode] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Subject].[FK_Subject_tbContact_AccountCode]...';


go
ALTER TABLE [Subject].[tbContact]
    ADD CONSTRAINT [FK_Subject_tbContact_AccountCode] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Subject].[FK_Subject_tbSector_Subject_tb]...';


go
ALTER TABLE [Subject].[tbSector]
    ADD CONSTRAINT [FK_Subject_tbSector_Subject_tb] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Subject].[FK_Subject_tb_App_tbTaxCode]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [FK_Subject_tb_App_tbTaxCode] FOREIGN KEY ([TaxCode]) REFERENCES [App].[tbTaxCode] ([TaxCode]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Subject].[FK_Subject_tb_Subject_tbAddress]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [FK_Subject_tb_Subject_tbAddress] FOREIGN KEY ([AddressCode]) REFERENCES [Subject].[tbAddress] ([AddressCode]) NOT FOR REPLICATION;


go
ALTER TABLE [Subject].[tbSubject] NOCHECK CONSTRAINT [FK_Subject_tb_Subject_tbAddress];


go
PRINT N'Creating Foreign Key [Subject].[FK_Subject_tbSubject_tbTransmitStatus]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [FK_Subject_tbSubject_tbTransmitStatus] FOREIGN KEY ([TransmitStatusCode]) REFERENCES [Subject].[tbTransmitStatus] ([TransmitStatusCode]);


go
PRINT N'Creating Foreign Key [Subject].[FK_Subject_tbSubject_tbStatus]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [FK_Subject_tbSubject_tbStatus] FOREIGN KEY ([SubjectStatusCode]) REFERENCES [Subject].[tbStatus] ([SubjectStatusCode]);


go
PRINT N'Creating Foreign Key [Subject].[FK_Subject_tbSubject_tbType]...';


go
ALTER TABLE [Subject].[tbSubject]
    ADD CONSTRAINT [FK_Subject_tbSubject_tbType] FOREIGN KEY ([SubjectTypeCode]) REFERENCES [Subject].[tbType] ([SubjectTypeCode]);


go
PRINT N'Creating Foreign Key [Subject].[FK_Subject_tbAccountKey_Subject_tbAccount]...';


go
ALTER TABLE [Subject].[tbAccountKey]
    ADD CONSTRAINT [FK_Subject_tbAccountKey_Subject_tbAccount] FOREIGN KEY ([AccountCode]) REFERENCES [Subject].[tbAccount] ([AccountCode]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Subject].[FK_Subject_tbAccount_Cash_tbCode]...';


go
ALTER TABLE [Subject].[tbAccount]
    ADD CONSTRAINT [FK_Subject_tbAccount_Cash_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]);


go
PRINT N'Creating Foreign Key [Subject].[FK_Subject_tbAccount_Cash_tbCoinType]...';


go
ALTER TABLE [Subject].[tbAccount]
    ADD CONSTRAINT [FK_Subject_tbAccount_Cash_tbCoinType] FOREIGN KEY ([CoinTypeCode]) REFERENCES [Cash].[tbCoinType] ([CoinTypeCode]);


go
PRINT N'Creating Foreign Key [Subject].[FK_Subject_tbAccount_Subject_tb]...';


go
ALTER TABLE [Subject].[tbAccount]
    ADD CONSTRAINT [FK_Subject_tbAccount_Subject_tb] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Subject].[FK_Subject_tbAccount_Subject_tbAccountType]...';


go
ALTER TABLE [Subject].[tbAccount]
    ADD CONSTRAINT [FK_Subject_tbAccount_Subject_tbAccountType] FOREIGN KEY ([AccountTypeCode]) REFERENCES [Subject].[tbAccountType] ([AccountTypeCode]);


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbFlow_Object_tbSyncType]...';


go
ALTER TABLE [Project].[tbFlow]
    ADD CONSTRAINT [FK_Project_tbFlow_Object_tbSyncType] FOREIGN KEY ([SyncTypeCode]) REFERENCES [Object].[tbSyncType] ([SyncTypeCode]);


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbFlow_Project_tb_Child]...';


go
ALTER TABLE [Project].[tbFlow]
    ADD CONSTRAINT [FK_Project_tbFlow_Project_tb_Child] FOREIGN KEY ([ChildProjectCode]) REFERENCES [Project].[tbProject] ([ProjectCode]);


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbFlow_Project_tb_Parent]...';


go
ALTER TABLE [Project].[tbFlow]
    ADD CONSTRAINT [FK_Project_tbFlow_Project_tb_Parent] FOREIGN KEY ([ParentProjectCode]) REFERENCES [Project].[tbProject] ([ProjectCode]);


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbDoc_Project_tb]...';


go
ALTER TABLE [Project].[tbDoc]
    ADD CONSTRAINT [FK_Project_tbDoc_Project_tb] FOREIGN KEY ([ProjectCode]) REFERENCES [Project].[tbProject] ([ProjectCode]);


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbAttrib_Project_tb]...';


go
ALTER TABLE [Project].[tbAttribute]
    ADD CONSTRAINT [FK_Project_tbAttrib_Project_tb] FOREIGN KEY ([ProjectCode]) REFERENCES [Project].[tbProject] ([ProjectCode]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbAttribute_Object_tbAttributeType]...';


go
ALTER TABLE [Project].[tbAttribute]
    ADD CONSTRAINT [FK_Project_tbAttribute_Object_tbAttributeType] FOREIGN KEY ([AttributeTypeCode]) REFERENCES [Object].[tbAttributeType] ([AttributeTypeCode]);


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbQuote_Project_tb]...';


go
ALTER TABLE [Project].[tbQuote]
    ADD CONSTRAINT [FK_Project_tbQuote_Project_tb] FOREIGN KEY ([ProjectCode]) REFERENCES [Project].[tbProject] ([ProjectCode]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbOp_Object_tbSyncType]...';


go
ALTER TABLE [Project].[tbOp]
    ADD CONSTRAINT [FK_Project_tbOp_Object_tbSyncType] FOREIGN KEY ([SyncTypeCode]) REFERENCES [Object].[tbSyncType] ([SyncTypeCode]);


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbOp_Project_tb]...';


go
ALTER TABLE [Project].[tbOp]
    ADD CONSTRAINT [FK_Project_tbOp_Project_tb] FOREIGN KEY ([ProjectCode]) REFERENCES [Project].[tbProject] ([ProjectCode]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbOp_Project_tbOpStatus]...';


go
ALTER TABLE [Project].[tbOp]
    ADD CONSTRAINT [FK_Project_tbOp_Project_tbOpStatus] FOREIGN KEY ([OpStatusCode]) REFERENCES [Project].[tbOpStatus] ([OpStatusCode]);


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbOp_Usr_tb]...';


go
ALTER TABLE [Project].[tbOp]
    ADD CONSTRAINT [FK_Project_tbOp_Usr_tb] FOREIGN KEY ([UserId]) REFERENCES [Usr].[tbUser] ([UserId]);


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbAllocationEvent_App_tbEventType]...';


go
ALTER TABLE [Project].[tbAllocationEvent]
    ADD CONSTRAINT [FK_Project_tbAllocationEvent_App_tbEventType] FOREIGN KEY ([EventTypeCode]) REFERENCES [App].[tbEventType] ([EventTypeCode]);


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbAllocationEvent_Project_tbStatus]...';


go
ALTER TABLE [Project].[tbAllocationEvent]
    ADD CONSTRAINT [FK_Project_tbAllocationEvent_Project_tbStatus] FOREIGN KEY ([ProjectStatusCode]) REFERENCES [Project].[tbStatus] ([ProjectStatusCode]);


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbAllocationEvent_tbAllocation]...';


go
ALTER TABLE [Project].[tbAllocationEvent]
    ADD CONSTRAINT [FK_Project_tbAllocationEvent_tbAllocation] FOREIGN KEY ([ContractAddress]) REFERENCES [Project].[tbAllocation] ([ContractAddress]) ON DELETE CASCADE;


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbCostSet_Project_tbProject]...';


go
ALTER TABLE [Project].[tbCostSet]
    ADD CONSTRAINT [FK_Project_tbCostSet_Project_tbProject] FOREIGN KEY ([ProjectCode]) REFERENCES [Project].[tbProject] ([ProjectCode]) ON DELETE CASCADE;


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbCostSet_Usr_tbUser]...';


go
ALTER TABLE [Project].[tbCostSet]
    ADD CONSTRAINT [FK_Project_tbCostSet_Usr_tbUser] FOREIGN KEY ([UserId]) REFERENCES [Usr].[tbUser] ([UserId]) ON DELETE CASCADE;


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbChangeLog_TrasmitStatusCode]...';


go
ALTER TABLE [Project].[tbChangeLog]
    ADD CONSTRAINT [FK_Project_tbChangeLog_TrasmitStatusCode] FOREIGN KEY ([TransmitStatusCode]) REFERENCES [Subject].[tbTransmitStatus] ([TransmitStatusCode]);


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbAllocation_AccountCode]...';


go
ALTER TABLE [Project].[tbAllocation]
    ADD CONSTRAINT [FK_Project_tbAllocation_AccountCode] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON DELETE CASCADE ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbAllocation_CashPolarityCode]...';


go
ALTER TABLE [Project].[tbAllocation]
    ADD CONSTRAINT [FK_Project_tbAllocation_CashPolarityCode] FOREIGN KEY ([CashPolarityCode]) REFERENCES [Cash].[tbPolarity] ([CashPolarityCode]);


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tbAllocation_ProjectStatusCode]...';


go
ALTER TABLE [Project].[tbAllocation]
    ADD CONSTRAINT [FK_Project_tbAllocation_ProjectStatusCode] FOREIGN KEY ([ProjectStatusCode]) REFERENCES [Project].[tbStatus] ([ProjectStatusCode]);


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tb_tbObject]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [FK_Project_tb_tbObject] FOREIGN KEY ([ObjectCode]) REFERENCES [Object].[tbObject] ([ObjectCode]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tb_tbStatus]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [FK_Project_tb_tbStatus] FOREIGN KEY ([ProjectStatusCode]) REFERENCES [Project].[tbStatus] ([ProjectStatusCode]);


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tb_tbSubject]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [FK_Project_tb_tbSubject] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tb_App_tbTaxCode]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [FK_Project_tb_App_tbTaxCode] FOREIGN KEY ([TaxCode]) REFERENCES [App].[tbTaxCode] ([TaxCode]);


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tb_Cash_tbCode]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [FK_Project_tb_Cash_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]);


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tb_Subject_tbAddress_From]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [FK_Project_tb_Subject_tbAddress_From] FOREIGN KEY ([AddressCodeFrom]) REFERENCES [Subject].[tbAddress] ([AddressCode]);


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tb_Subject_tbAddress_To]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [FK_Project_tb_Subject_tbAddress_To] FOREIGN KEY ([AddressCodeTo]) REFERENCES [Subject].[tbAddress] ([AddressCode]);


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tb_Usr_tb]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [FK_Project_tb_Usr_tb] FOREIGN KEY ([UserId]) REFERENCES [Usr].[tbUser] ([UserId]) ON UPDATE CASCADE;


go
PRINT N'Creating Foreign Key [Project].[FK_Project_tb_Usr_tb_ActionById]...';


go
ALTER TABLE [Project].[tbProject]
    ADD CONSTRAINT [FK_Project_tb_Usr_tb_ActionById] FOREIGN KEY ([ActionById]) REFERENCES [Usr].[tbUser] ([UserId]);


go
PRINT N'Creating View [dbo].[AspNetUserRegistrations]...';


go
CREATE   VIEW dbo.AspNetUserRegistrations
AS
	SELECT asp.Id, asp.UserName EmailAddress, u.UserName,
		asp.EmailConfirmed IsConfirmed, 
		CAST(CASE WHEN u.EmailAddress IS NULL THEN 0 ELSE 1 END as bit) IsRegistered,
		CAST(CASE WHEN 
			(SELECT COUNT(*) FROM AspNetUserRoles 
				JOIN AspNetRoles ON AspNetRoles.Id = AspNetUserRoles.RoleId 
				WHERE AspNetRoles.Name = 'Administrators' AND AspNetUserRoles.UserId = asp.Id) = 0 
		THEN 0 
		ELSE 1 
		END as bit) IsAdministrator,
		CAST(CASE WHEN 
			(SELECT COUNT(*) FROM AspNetUserRoles 
				JOIN AspNetRoles ON AspNetRoles.Id = AspNetUserRoles.RoleId 
				WHERE AspNetRoles.Name = 'Managers' AND AspNetUserRoles.UserId = asp.Id) = 0 
		THEN 0 
		ELSE 1 
		END as bit) IsManager
	FROM AspNetUsers asp
		LEFT OUTER JOIN Usr.tbUser u ON asp.Email = u.EmailAddress;
go
PRINT N'Creating View [Web].[vwAttachmentInvoices]...';


go
CREATE   VIEW Web.vwAttachmentInvoices
AS
	SELECT Web.tbAttachmentInvoice.InvoiceTypeCode, Invoice.tbType.InvoiceType, Web.tbAttachmentInvoice.AttachmentId, Web.tbAttachment.AttachmentFileName
	FROM Web.tbAttachmentInvoice 
		JOIN Invoice.tbType ON Web.tbAttachmentInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode 
		JOIN Web.tbAttachment ON Web.tbAttachmentInvoice.AttachmentId = Web.tbAttachment.AttachmentId;
go
PRINT N'Creating View [Web].[vwTemplateImages]...';


go
CREATE   VIEW Web.vwTemplateImages
AS
	SELECT Web.tbTemplateImage.TemplateId, Web.tbTemplate.TemplateFileName, Web.tbTemplateImage.ImageTag, Web.tbImage.ImageFileName
	FROM Web.tbTemplateImage 
		JOIN Web.tbTemplate ON Web.tbTemplateImage.TemplateId = Web.tbTemplate.TemplateId 
		JOIN Web.tbImage ON Web.tbTemplateImage.ImageTag = Web.tbImage.ImageTag;
go
PRINT N'Creating View [Web].[vwTemplateInvoices]...';


go
CREATE   VIEW Web.vwTemplateInvoices
AS
	SELECT Web.tbTemplateInvoice.InvoiceTypeCode, Invoice.tbType.InvoiceType, Web.tbTemplateInvoice.TemplateId, Web.tbTemplate.TemplateFileName, Web.tbTemplateInvoice.LastUsedOn
	FROM Web.tbTemplateInvoice 
		JOIN Invoice.tbType ON Web.tbTemplateInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode 
		JOIN Web.tbTemplate ON Web.tbTemplateInvoice.TemplateId = Web.tbTemplate.TemplateId;
go
PRINT N'Creating View [Usr].[vwMenuItemReportMode]...';


go

CREATE   VIEW Usr.vwMenuItemReportMode
AS
	SELECT        OpenMode, OpenModeDescription
	FROM            Usr.tbMenuOpenMode
	WHERE        (OpenMode > 1) AND (OpenMode < 5);
go
PRINT N'Creating View [Usr].[vwMenuItemFormMode]...';


go

CREATE   VIEW Usr.vwMenuItemFormMode
AS
	SELECT        OpenMode, OpenModeDescription
	FROM            Usr.tbMenuOpenMode
	WHERE        (OpenMode < 2);
go
PRINT N'Creating View [Usr].[vwCredentials]...';


go
CREATE   VIEW Usr.vwCredentials
  AS
SELECT     UserId, UserName, LogonName, IsAdministrator
FROM         Usr.tbUser
WHERE     (LogonName = SUSER_SNAME()) AND (IsEnabled <> 0)
go
PRINT N'Creating View [Usr].[vwUserMenuList]...';


go
CREATE   VIEW Usr.vwUserMenuList
AS
	WITH user_menus AS
	(
		SELECT MenuId
		FROM Usr.tbMenuUser
		WHERE UserId = (SELECT UserId FROM Usr.vwCredentials)
	), folders AS
	(
		SELECT folder.MenuId, folder.Argument FolderId , folder.ItemText 
			, (SELECT parent_folder.FolderId FROM Usr.tbMenuEntry parent_folder WHERE parent_folder.MenuId = folder.MenuId and parent_folder.FolderId = folder.FolderId and Command = 0) ParentFolderId 
		FROM Usr.tbMenuEntry folder
			JOIN user_menus ON folder.MenuId = user_menus.MenuId
		WHERE Command = 1
	), return_commands AS
	(
		SELECT folders.MenuId, folders.FolderId,
			(SELECT MAX(ItemId) + 1 FROM Usr.tbMenuEntry WHERE MenuId = folders.MenuId and FolderId = folders.FolderId) ItemId,
			(SELECT CONCAT('Return to ', CASE Argument WHEN 'Root' THEN 'Main Menu' ELSE ItemText END) FROM Usr.tbMenuEntry WHERE MenuId = folders.MenuId and FolderId = folders.ParentFolderId and ItemId = 0) ItemText,
			CAST(1 AS smallint) Command,
			NULL ProjectName,
			CAST(ParentFolderId as nvarchar(50)) Argument,
			CAST(0 AS smallint) OpenMode
		FROM folders
	), menu_items AS
	(
		SELECT menu_entries.MenuId, FolderId, 
			ROW_NUMBER() OVER (PARTITION BY menu_entries.MenuId, FolderId ORDER BY ItemText DESC) RowNumber,
			ItemId, ItemText, Command, ProjectName, Argument, OpenMode
		FROM Usr.tbMenuEntry menu_entries
			JOIN user_menus ON menu_entries.MenuId = user_menus.MenuId
		UNION
		SELECT MenuId, FolderId, 0 RowNumber, ItemId, ItemText, Command, ProjectName, Argument, OpenMode
		FROM return_commands
	)
	SELECT menu.MenuId, menu.InterfaceCode, FolderId, RowNumber, ItemId, ItemText, Command, ProjectName, Argument, OpenMode
	FROM menu_items
		JOIN Usr.tbMenu menu ON menu_items.MenuId = menu.MenuId;
go
PRINT N'Creating View [Usr].[vwDoc]...';


go
CREATE VIEW Usr.vwDoc
AS
	WITH bank AS 
	(
		SELECT TOP (1) (SELECT SubjectCode FROM App.tbOptions) AS SubjectCode, 
			Subject.tbSubject.SubjectName AS BankName,
			Subject.tbAccount.AccountName AS CurrentAccountName,
			CONCAT(Subject.tbSubject.SubjectName, SPACE(1), Subject.tbAccount.AccountName) AS BankAccount, 
			Subject.tbAccount.SortCode AS BankSortCode, Subject.tbAccount.AccountNumber AS BankAccountNumber
		FROM Subject.tbAccount 
			INNER JOIN Subject.tbSubject ON Subject.tbAccount.SubjectCode = Subject.tbSubject.SubjectCode
		WHERE (NOT (Subject.tbAccount.CashCode IS NULL)) AND (Subject.tbAccount.AccountTypeCode = 0)
	)
    SELECT        TOP (1) company.SubjectName AS CompanyName, Subject.tbAddress.Address AS CompanyAddress, company.PhoneNumber AS CompanyPhoneNumber,  
                              company.EmailAddress AS CompanyEmailAddress, company.WebSite AS CompanyWebsite, company.CompanyNumber, company.VatNumber, company.Logo, 
							  bank_details.BankName, bank_details.CurrentAccountName,
							  bank_details.BankAccount, bank_details.BankAccountNumber, bank_details.BankSortCode
     FROM            Subject.tbSubject AS company INNER JOIN
                              App.tbOptions ON company.SubjectCode = App.tbOptions.SubjectCode LEFT OUTER JOIN
                              bank AS bank_details ON company.SubjectCode = bank_details.SubjectCode LEFT OUTER JOIN
                              Subject.tbAddress ON company.AddressCode = Subject.tbAddress.AddressCode;
go
PRINT N'Creating View [Invoice].[vwNetworkDeploymentItems]...';


go
CREATE VIEW Invoice.vwNetworkDeploymentItems
AS
	SELECT Invoice.tbItem.InvoiceNumber, Invoice.tbItem.CashCode ChargeCode, 
		CASE WHEN LEN(COALESCE(CAST(Invoice.tbItem.ItemReference AS NVARCHAR), '')) > 0 THEN Invoice.tbItem.ItemReference ELSE Cash.tbCode.CashDescription END ChargeDescription, 
			Invoice.tbItem.InvoiceValue, Invoice.tbItem.TaxValue, 0 AS InvoiceQuantity, Invoice.tbItem.TaxCode
	FROM  Invoice.tbItem 
		INNER JOIN Cash.tbCode ON Invoice.tbItem.CashCode = Cash.tbCode.CashCode;
go
PRINT N'Creating View [Invoice].[vwTypes]...';


go
CREATE   VIEW Invoice.vwTypes
AS
	SELECT Invoice.tbType.InvoiceTypeCode, Invoice.tbType.InvoiceType, Invoice.tbType.CashPolarityCode, Cash.tbPolarity.CashPolarity, Invoice.tbType.NextNumber
	FROM Invoice.tbType 
		JOIN Cash.tbPolarity ON Invoice.tbType.CashPolarityCode = Cash.tbPolarity.CashPolarityCode;
go
PRINT N'Creating View [Invoice].[vwItems]...';


go
CREATE VIEW Invoice.vwItems
AS
SELECT        Invoice.tbItem.InvoiceNumber, Invoice.tbItem.CashCode, Cash.tbCode.CashDescription, Invoice.tbItem.TaxCode, Invoice.tbItem.TaxValue, Invoice.tbItem.InvoiceValue, Invoice.tbItem.ItemReference, 
                         Invoice.tbInvoice.InvoicedOn
FROM            Invoice.tbItem INNER JOIN
                         Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
                         Cash.tbCode ON Invoice.tbItem.CashCode = Cash.tbCode.CashCode;
go
PRINT N'Creating View [Invoice].[vwDocProject]...';


go
CREATE VIEW Invoice.vwDocProject
AS
SELECT        tbProjectInvoice.InvoiceNumber, tbProjectInvoice.ProjectCode, Project.tbProject.ProjectTitle, Project.tbProject.ObjectCode, tbProjectInvoice.CashCode, Cash.tbCode.CashDescription, Project.tbProject.ActionedOn, tbProjectInvoice.Quantity, 
                         Object.tbObject.UnitOfMeasure, tbProjectInvoice.InvoiceValue, tbProjectInvoice.TaxValue, tbProjectInvoice.TaxCode, Project.tbProject.SecondReference
FROM            Invoice.tbProject AS tbProjectInvoice INNER JOIN
                         Project.tbProject ON tbProjectInvoice.ProjectCode = Project.tbProject.ProjectCode AND tbProjectInvoice.ProjectCode = Project.tbProject.ProjectCode INNER JOIN
                         Cash.tbCode ON tbProjectInvoice.CashCode = Cash.tbCode.CashCode INNER JOIN
                         Object.tbObject ON Project.tbProject.ObjectCode = Object.tbObject.ObjectCode
go
PRINT N'Creating View [Invoice].[vwDocItem]...';


go
CREATE VIEW Invoice.vwDocItem
AS
SELECT     Invoice.tbItem.InvoiceNumber, Invoice.tbItem.CashCode, Cash.tbCode.CashDescription, Invoice.tbInvoice.InvoicedOn AS ActionedOn, 
                      Invoice.tbItem.TaxCode, Invoice.tbItem.InvoiceValue, Invoice.tbItem.TaxValue, Invoice.tbItem.ItemReference
FROM         Invoice.tbItem INNER JOIN
                      Cash.tbCode ON Invoice.tbItem.CashCode = Cash.tbCode.CashCode INNER JOIN
                      Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
go
PRINT N'Creating View [Invoice].[vwTaxSummary]...';


go
CREATE VIEW Invoice.vwTaxSummary
AS
	WITH base AS
	(
		SELECT        InvoiceNumber, TaxCode, SUM(InvoiceValue) AS InvoiceValueTotal, SUM(TaxValue) AS TaxValueTotal
		FROM            Invoice.tbItem
		GROUP BY InvoiceNumber, TaxCode
		HAVING        (NOT (TaxCode IS NULL))
		UNION
		SELECT        InvoiceNumber, TaxCode, SUM(InvoiceValue) AS InvoiceValueTotal, SUM(TaxValue) AS TaxValueTotal
		FROM            Invoice.tbProject
		GROUP BY InvoiceNumber, TaxCode
		HAVING        (NOT (TaxCode IS NULL))
	)
	SELECT        InvoiceNumber, TaxCode, CAST(SUM(InvoiceValueTotal) as decimal(18, 5)) AS InvoiceValueTotal, CAST(SUM(TaxValueTotal) as decimal(18, 5)) AS TaxValueTotal, 
	 CASE WHEN SUM(InvoiceValueTotal) <> 0 THEN CAST((SUM(TaxValueTotal) / SUM(InvoiceValueTotal)) as decimal(18, 5)) ELSE 0 END AS TaxRate
	FROM            base
	GROUP BY InvoiceNumber, TaxCode;
go
PRINT N'Creating View [Invoice].[vwSummary]...';


go
CREATE VIEW Invoice.vwSummary
AS
	WITH Projects AS
	(
		SELECT  (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= Invoice.tbInvoice.InvoicedOn) ORDER BY p.StartOn DESC) AS StartOn,  
								 CASE WHEN Invoice.tbInvoice.InvoiceTypeCode = 1 THEN 0 ELSE CASE WHEN Invoice.tbInvoice.InvoiceTypeCode = 3 THEN 2 ELSE Invoice.tbInvoice.InvoiceTypeCode END END AS InvoiceTypeCode, 
								 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbProject.InvoiceValue * - 1 ELSE Invoice.tbProject.InvoiceValue END AS InvoiceValue, 
								 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbProject.TaxValue * - 1 ELSE Invoice.tbProject.TaxValue END AS TaxValue
		FROM            Invoice.tbProject INNER JOIN
								 Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
								 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
		WHERE        (Invoice.tbInvoice.InvoicedOn >= (	
						SELECT MIN( App.tbYearPeriod.StartOn) FROM App.tbYear 
						INNER JOIN App.tbYearPeriod ON App.tbYear.YearNumber = App.tbYearPeriod.YearNumber 
						WHERE ( App.tbYear.CashStatusCode < 3)))
	), items AS
	(
		SELECT  (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= Invoice.tbInvoice.InvoicedOn) ORDER BY p.StartOn DESC) AS StartOn,  
								 CASE WHEN Invoice.tbInvoice.InvoiceTypeCode = 1 THEN 0 ELSE CASE WHEN Invoice.tbInvoice.InvoiceTypeCode = 3 THEN 2 ELSE Invoice.tbInvoice.InvoiceTypeCode END END AS InvoiceTypeCode, 
								 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbItem.InvoiceValue * - 1 ELSE Invoice.tbItem.InvoiceValue END AS InvoiceValue, 
								 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbItem.TaxValue * - 1 ELSE Invoice.tbItem.TaxValue END AS TaxValue
		FROM            Invoice.tbItem INNER JOIN
								 Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
								 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
		WHERE        (Invoice.tbInvoice.InvoicedOn >= (	
						SELECT MIN( App.tbYearPeriod.StartOn) FROM App.tbYear 
						INNER JOIN App.tbYearPeriod ON App.tbYear.YearNumber = App.tbYearPeriod.YearNumber 
						WHERE ( App.tbYear.CashStatusCode < 3)))
	), invoice_entries AS
	(
		SELECT     StartOn, InvoiceTypeCode, InvoiceValue, TaxValue
		FROM         items
		UNION
		SELECT     StartOn, InvoiceTypeCode, InvoiceValue, TaxValue
		FROM         Projects
	), invoice_totals AS
	(
		SELECT     invoice_entries.StartOn, invoice_entries.InvoiceTypeCode, Invoice.tbType.InvoiceType, 
							  SUM(invoice_entries.InvoiceValue) AS TotalInvoiceValue, SUM(invoice_entries.TaxValue) AS TotalTaxValue
		FROM         invoice_entries INNER JOIN
							  Invoice.tbType ON invoice_entries.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
		GROUP BY invoice_entries.StartOn, invoice_entries.InvoiceTypeCode, Invoice.tbType.InvoiceType
	), invoice_margin AS
	(
		SELECT     StartOn, 4 AS InvoiceTypeCode, (SELECT CAST(Message AS NVARCHAR(10)) FROM App.tbText WHERE TextId = 3004) AS InvoiceType, SUM(TotalInvoiceValue) AS TotalInvoiceValue, SUM(TotalTaxValue) 
							  AS TotalTaxValue
		FROM         invoice_totals
		GROUP BY StartOn
	)
	SELECT     CONCAT(DATENAME(yyyy, StartOn), '/', FORMAT(MONTH(StartOn), '00')) AS PeriodOn, StartOn, InvoiceTypeCode, InvoiceType AS InvoiceType, 
						  ABS(TotalInvoiceValue) AS TotalInvoiceValue, ABS(TotalTaxValue) AS TotalTaxValue
	FROM         invoice_totals
	UNION
	SELECT     CONCAT(DATENAME(yyyy, StartOn), '/', FORMAT(MONTH(StartOn), '00')) AS PeriodOn, StartOn, InvoiceTypeCode, InvoiceType AS InvoiceType, 
						  TotalInvoiceValue, TotalTaxValue
	FROM         invoice_margin;
go
PRINT N'Creating View [Invoice].[vwAccountsMode]...';


go
CREATE   VIEW Invoice.vwAccountsMode
AS
	SELECT        Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.UserId, Invoice.tbInvoice.SubjectCode, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.Notes, 
							 Invoice.tbItem.CashCode, Invoice.tbItem.TaxCode, Invoice.tbItem.ItemReference, Invoice.tbInvoice.RowVer AS InvoiceRowVer, Invoice.tbItem.RowVer AS ItemRowVer, Invoice.tbItem.TotalValue, Invoice.tbItem.InvoiceValue, 
							 Invoice.tbInvoice.PaidValue, Invoice.tbInvoice.PaidTaxValue, Invoice.tbInvoice.ExpectedOn, Invoice.tbInvoice.DueOn, Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Printed, Invoice.tbInvoice.Spooled
	FROM            Invoice.tbInvoice INNER JOIN
							 Invoice.tbItem ON Invoice.tbInvoice.InvoiceNumber = Invoice.tbItem.InvoiceNumber;
go
PRINT N'Creating View [Invoice].[vwCandidateDebits]...';


go
CREATE VIEW Invoice.vwCandidateDebits
AS
SELECT TOP 100 PERCENT Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.UserId, Invoice.tbInvoice.SubjectCode, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, Invoice.tbInvoice.InvoicedOn, 
                         Invoice.tbInvoice.InvoiceValue, Invoice.tbInvoice.TaxValue, Invoice.tbInvoice.PaidValue, Invoice.tbInvoice.PaidTaxValue, Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Notes, Invoice.tbInvoice.Printed, 
                         Invoice.tbInvoice.DueOn, Invoice.tbInvoice.Spooled, Usr.tbUser.UserName, Invoice.tbStatus.InvoiceStatus, Invoice.tbType.InvoiceType
FROM            Invoice.tbInvoice INNER JOIN
                         Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
                         Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
                         Usr.tbUser ON Invoice.tbInvoice.UserId = Usr.tbUser.UserId
WHERE        (Invoice.tbInvoice.InvoiceTypeCode = 2)
ORDER BY Invoice.tbInvoice.SubjectCode, Invoice.tbInvoice.InvoicedOn DESC
go
PRINT N'Creating View [Invoice].[vwCandidateCredits]...';


go
CREATE VIEW Invoice.vwCandidateCredits
AS
SELECT TOP 100 PERCENT Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.UserId, Invoice.tbInvoice.SubjectCode, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, Invoice.tbInvoice.InvoicedOn, 
                         Invoice.tbInvoice.InvoiceValue, Invoice.tbInvoice.TaxValue, Invoice.tbInvoice.PaidValue, Invoice.tbInvoice.PaidTaxValue, Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Notes, Invoice.tbInvoice.Printed, 
                         Invoice.tbInvoice.DueOn, Invoice.tbInvoice.Spooled, Usr.tbUser.UserName, Invoice.tbStatus.InvoiceStatus, Invoice.tbType.InvoiceType
FROM            Invoice.tbInvoice INNER JOIN
                         Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
                         Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
                         Usr.tbUser ON Invoice.tbInvoice.UserId = Usr.tbUser.UserId
WHERE        (Invoice.tbInvoice.InvoiceTypeCode = 0)
ORDER BY Invoice.tbInvoice.SubjectCode, Invoice.tbInvoice.InvoicedOn DESC
go
PRINT N'Creating View [Invoice].[vwMirrorDetails]...';


go
CREATE VIEW Invoice.vwMirrorDetails
AS
	SELECT invoice_Project.ContractAddress, invoice_Project.ProjectCode DetailRef, mirror.ObjectCode DetailCode, alloc.AllocationDescription DetailDescription,
		invoice_Project.Quantity, invoice_Project.InvoiceValue, invoice_Project.TaxValue, invoice_Project.TaxCode, invoice_Project.RowVer 
	FROM Invoice.tbMirrorProject invoice_Project
		JOIN Invoice.tbMirror invoice ON invoice.ContractAddress = invoice_Project.ContractAddress
		JOIN Project.tbAllocation alloc ON alloc.SubjectCode = invoice.SubjectCode AND alloc.ProjectCode = invoice_Project.ProjectCode
		JOIN Object.tbMirror mirror ON alloc.SubjectCode = mirror.SubjectCode AND alloc.AllocationCode = mirror.AllocationCode
	UNION
	SELECT invoice_item.ContractAddress, invoice_item.ChargeCode DetailRef, mirror.CashCode DetailCode, invoice_item.ChargeDescription DetailDescription,
		0 Quantity, invoice_item.InvoiceValue, invoice_item.TaxValue, invoice_item.TaxCode, invoice_item.RowVer
	FROM Invoice.tbMirrorItem invoice_item
		JOIN Invoice.tbMirror invoice ON invoice.ContractAddress = invoice_item.ContractAddress
		JOIN Cash.tbMirror mirror ON invoice_item.ChargeCode = mirror.ChargeCode AND invoice.SubjectCode = mirror.SubjectCode;
go
PRINT N'Creating View [Invoice].[vwNetworkDeployments]...';


go
CREATE VIEW Invoice.vwNetworkDeployments
AS
	SELECT Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.SubjectCode, 
		Invoice.tbType.CashPolarityCode AS PaymentPolarity, 
		CASE Invoice.tbType.InvoiceTypeCode 
			WHEN 0 THEN Invoice.tbType.CashPolarityCode 
			WHEN 1 THEN 1
			WHEN 2 THEN Invoice.tbType.CashPolarityCode 
			WHEN 3 THEN 0
		END InvoicePolarity, 
		Invoice.tbInvoice.InvoiceStatusCode,
		Invoice.tbInvoice.DueOn, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.InvoiceValue, Invoice.tbInvoice.TaxValue, 
		Invoice.tbInvoice.PaymentTerms, (SELECT TOP 1 UnitOfCharge FROM App.tbOptions) UnitOfCharge, Cash.tbChangeReference.PaymentAddress,
		Invoice.tbMirrorReference.ContractAddress,
		Invoice.tbMirror.InvoiceNumber ContractNumber
	FROM Invoice.tbMirrorReference 
		RIGHT OUTER JOIN Invoice.tbChangeLog 
		INNER JOIN Invoice.tbInvoice ON Invoice.tbChangeLog.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		INNER JOIN Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode ON Invoice.tbMirrorReference.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber 
		LEFT OUTER JOIN Invoice.tbMirror ON Invoice.tbMirrorReference.ContractAddress = Invoice.tbMirror.ContractAddress AND Invoice.tbMirrorReference.ContractAddress = Invoice.tbMirror.ContractAddress
		LEFT OUTER JOIN Cash.tbChangeReference ON Invoice.tbInvoice.InvoiceNumber = Cash.tbChangeReference.InvoiceNumber
	WHERE        (Invoice.tbChangeLog.TransmitStatusCode = 1)
go
PRINT N'Creating View [Invoice].[vwNetworkUpdates]...';


go
CREATE VIEW Invoice.vwNetworkUpdates
AS
	WITH updates AS
	(
		SELECT DISTINCT InvoiceNumber FROM Invoice.tbChangeLog 
		WHERE TransmitStatusCode = 2
		EXCEPT
		SELECT DISTINCT InvoiceNumber FROM Invoice.tbChangeLog 
		WHERE TransmitStatusCode = 1
	)
	SELECT Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.SubjectCode, Invoice.tbInvoice.InvoiceStatusCode,
			Invoice.tbInvoice.DueOn, Invoice.tbInvoice.PaidValue, Invoice.tbInvoice.PaidTaxValue, Cash.tbChangeReference.PaymentAddress
	FROM updates 
		JOIN Invoice.tbInvoice ON updates.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber 
		JOIN Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
		LEFT OUTER JOIN Cash.tbChangeReference ON Invoice.tbInvoice.InvoiceNumber = Cash.tbChangeReference.InvoiceNumber
go
PRINT N'Creating View [Invoice].[vwRegisterOverdue]...';


go
CREATE   VIEW Invoice.vwRegisterOverdue
AS
	SELECT Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, Invoice.tbStatus.InvoiceStatus, 
							 Invoice.tbType.InvoiceType, DATEDIFF(DD, CURRENT_TIMESTAMP, Invoice.tbInvoice.InvoicedOn) AS UnpaidDays, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.DueOn, Invoice.tbInvoice.ExpectedOn, 
							 CASE Invoice.tbType.CashPolarityCode WHEN 0 THEN Invoice.tbInvoice.InvoiceValue ELSE Invoice.tbInvoice.InvoiceValue * - 1 END AS InvoiceValue, 
							 CASE Invoice.tbType.CashPolarityCode WHEN 0 THEN Invoice.tbInvoice.TaxValue ELSE Invoice.tbInvoice.TaxValue * - 1 END AS TaxValue, 
							 CASE Invoice.tbType.CashPolarityCode WHEN 0 THEN (Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue) - (Invoice.tbInvoice.PaidValue + Invoice.tbInvoice.PaidTaxValue) 
							 ELSE ((Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue) - (Invoice.tbInvoice.PaidValue + Invoice.tbInvoice.PaidTaxValue)) * - 1 END AS UnpaidValue, 
							 Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Notes
	FROM            Invoice.tbInvoice INNER JOIN
							 Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode
	WHERE    (Invoice.tbInvoice.InvoiceStatusCode < 3);
go
PRINT N'Creating View [Invoice].[vwRegisterItems]...';


go
CREATE VIEW Invoice.vwRegisterItems
AS
	SELECT       (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= Invoice.tbInvoice.InvoicedOn) ORDER BY p.StartOn DESC) AS StartOn,  
					Invoice.tbInvoice.InvoiceNumber, Invoice.tbItem.CashCode AS ProjectCode, Cash.tbCode.CashCode, Cash.tbCode.CashDescription, 
							 Invoice.tbItem.TaxCode, App.tbTaxCode.TaxDescription, Invoice.tbInvoice.SubjectCode, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, 
							 Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.DueOn, Invoice.tbInvoice.ExpectedOn,
							 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbItem.InvoiceValue * - 1 ELSE Invoice.tbItem.InvoiceValue END AS InvoiceValue, 
							 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbItem.TaxValue * - 1 ELSE Invoice.tbItem.TaxValue END AS TaxValue, 
							 CAST(Invoice.tbItem.ItemReference as nvarchar(100)) ItemReference, Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Printed, 
							 Subject.tbSubject.SubjectName, Usr.tbUser.UserName, Invoice.tbInvoice.UserId, Invoice.tbStatus.InvoiceStatus, Invoice.tbType.CashPolarityCode, Invoice.tbType.InvoiceType
	FROM            Invoice.tbInvoice INNER JOIN
							 Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							 Usr.tbUser ON Invoice.tbInvoice.UserId = Usr.tbUser.UserId INNER JOIN
							 Invoice.tbItem ON Invoice.tbInvoice.InvoiceNumber = Invoice.tbItem.InvoiceNumber INNER JOIN
							 Cash.tbCode ON Invoice.tbItem.CashCode = Cash.tbCode.CashCode LEFT OUTER JOIN
							 App.tbTaxCode ON Invoice.tbItem.TaxCode = App.tbTaxCode.TaxCode;
go
PRINT N'Creating View [Invoice].[vwSalesInvoiceSpool]...';


go
CREATE VIEW Invoice.vwSalesInvoiceSpool
AS
SELECT        sales_invoice.InvoiceNumber, Invoice.tbType.InvoiceType, sales_invoice.InvoiceStatusCode, Usr.tbUser.UserName, sales_invoice.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbStatus.InvoiceStatus, 
                         sales_invoice.InvoicedOn, sales_invoice.InvoiceValue AS InvoiceValueTotal, sales_invoice.TaxValue AS TaxValueTotal, sales_invoice.PaymentTerms, sales_invoice.DueOn, sales_invoice.Notes, Subject.tbSubject.EmailAddress, 
                         Subject.tbAddress.Address AS InvoiceAddress, tbInvoiceProject.ProjectCode, Project.tbProject.ProjectTitle, Project.tbProject.ObjectCode, Project.tbProject.ActionedOn, tbInvoiceProject.Quantity, Object.tbObject.UnitOfMeasure, tbInvoiceProject.TaxCode, 
                         tbInvoiceProject.InvoiceValue, tbInvoiceProject.TaxValue
FROM            Invoice.tbInvoice AS sales_invoice INNER JOIN
                         Invoice.tbStatus ON sales_invoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
                         Subject.tbSubject ON sales_invoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
                         Usr.tbUser ON sales_invoice.UserId = Usr.tbUser.UserId LEFT OUTER JOIN
                         Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode INNER JOIN
                         Invoice.tbProject AS tbInvoiceProject ON sales_invoice.InvoiceNumber = tbInvoiceProject.InvoiceNumber INNER JOIN
                         Project.tbProject ON tbInvoiceProject.ProjectCode = Project.tbProject.ProjectCode INNER JOIN
                         Object.tbObject ON Project.tbProject.ObjectCode = Object.tbObject.ObjectCode INNER JOIN
                         Invoice.tbType ON sales_invoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
WHERE        (sales_invoice.InvoiceTypeCode = 0) AND EXISTS
                             (SELECT        UserName, DocTypeCode, DocumentNumber, SpooledOn, RowVer
                               FROM            App.tbDocSpool AS doc
                               WHERE        (DocTypeCode = 4) AND (UserName = SUSER_SNAME()) AND (sales_invoice.InvoiceNumber = DocumentNumber))
go
PRINT N'Creating View [Invoice].[vwRegisterProjects]...';


go
CREATE VIEW Invoice.vwRegisterProjects
AS
	SELECT (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= Invoice.tbInvoice.InvoicedOn) ORDER BY p.StartOn DESC) AS StartOn,  
			Invoice.tbInvoice.InvoiceNumber, InvoiceProject.ProjectCode, Project.ProjectTitle, Cash.tbCode.CashCode, Cash.tbCode.CashDescription, 
							 InvoiceProject.TaxCode, App.tbTaxCode.TaxDescription, Invoice.tbInvoice.SubjectCode, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, 
							 Invoice.tbInvoice.InvoicedOn,  Invoice.tbInvoice.DueOn, Invoice.tbInvoice.ExpectedOn, InvoiceProject.Quantity,
							 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN InvoiceProject.InvoiceValue * - 1 ELSE InvoiceProject.InvoiceValue END AS InvoiceValue, 
							 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN InvoiceProject.TaxValue * - 1 ELSE InvoiceProject.TaxValue END AS TaxValue, 
							 Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Printed, 
							 Subject.tbSubject.SubjectName, Usr.tbUser.UserName, Invoice.tbInvoice.UserId, Invoice.tbStatus.InvoiceStatus, Invoice.tbType.CashPolarityCode, Invoice.tbType.InvoiceType
	FROM            Invoice.tbInvoice INNER JOIN
							 Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							 Usr.tbUser ON Invoice.tbInvoice.UserId = Usr.tbUser.UserId INNER JOIN
							 Invoice.tbProject AS InvoiceProject ON Invoice.tbInvoice.InvoiceNumber = InvoiceProject.InvoiceNumber INNER JOIN
							 Cash.tbCode ON InvoiceProject.CashCode = Cash.tbCode.CashCode INNER JOIN
							 Project.tbProject AS Project ON InvoiceProject.ProjectCode = Project.ProjectCode AND InvoiceProject.ProjectCode = Project.ProjectCode LEFT OUTER JOIN
							 App.tbTaxCode ON InvoiceProject.TaxCode = App.tbTaxCode.TaxCode;
go
PRINT N'Creating View [Invoice].[vwRegisterSalesOverdue]...';


go
CREATE VIEW Invoice.vwRegisterSalesOverdue
AS
	SELECT        TOP (100) PERCENT Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, Invoice.tbStatus.InvoiceStatus, 
							 Invoice.tbType.InvoiceType, DATEDIFF(DD, CURRENT_TIMESTAMP, Invoice.tbInvoice.InvoicedOn) AS UnpaidDays, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.DueOn, Invoice.tbInvoice.ExpectedOn, 
							 CASE CashPolarityCode WHEN 1 THEN Invoice.tbInvoice.InvoiceValue ELSE Invoice.tbInvoice.InvoiceValue * - 1 END AS InvoiceValue, 
							 CASE CashPolarityCode WHEN 1 THEN Invoice.tbInvoice.TaxValue ELSE Invoice.tbInvoice.TaxValue * - 1 END AS TaxValue, CASE CashPolarityCode WHEN 1 THEN (Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue) 
							 - (Invoice.tbInvoice.PaidValue + Invoice.tbInvoice.PaidTaxValue) ELSE ((Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue) - (Invoice.tbInvoice.PaidValue + Invoice.tbInvoice.PaidTaxValue)) * - 1 END AS UnpaidValue, 
							 Cash.tbChangeReference.PaymentAddress, Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Notes
	FROM            Invoice.tbInvoice INNER JOIN
							 Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode LEFT OUTER JOIN
							 Cash.tbChangeReference ON Invoice.tbInvoice.InvoiceNumber = Cash.tbChangeReference.InvoiceNumber
	WHERE        (Invoice.tbInvoice.InvoiceTypeCode < 2) AND (Invoice.tbInvoice.InvoiceStatusCode < 3)
go
PRINT N'Creating View [Invoice].[vwSalesInvoiceSpoolByObject]...';


go
CREATE VIEW Invoice.vwSalesInvoiceSpoolByObject
AS
WITH invoice AS 
(
	SELECT        sales_invoice.InvoiceNumber, Invoice.tbType.InvoiceType, sales_invoice.InvoiceStatusCode, Usr.tbUser.UserName, sales_invoice.SubjectCode, Subject.tbSubject.SubjectName, 
							Invoice.tbStatus.InvoiceStatus, sales_invoice.InvoicedOn, sales_invoice.InvoiceValue AS InvoiceValueTotal, sales_invoice.TaxValue AS TaxValueTotal, sales_invoice.PaymentTerms, 
							Subject.tbSubject.EmailAddress, Subject.tbSubject.AddressCode, Object.tbObject.ObjectCode, Object.tbObject.UnitOfMeasure, MIN(Project.tbProject.ActionedOn) AS FirstActionedOn, 
							SUM(tbInvoiceProject.Quantity) AS ObjectQuantity, tbInvoiceProject.TaxCode, SUM(tbInvoiceProject.InvoiceValue) AS ObjectInvoiceValue, SUM(tbInvoiceProject.TaxValue) AS ObjectTaxValue
	FROM            Invoice.tbInvoice AS sales_invoice INNER JOIN
							Invoice.tbStatus ON sales_invoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							Subject.tbSubject ON sales_invoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							Usr.tbUser ON sales_invoice.UserId = Usr.tbUser.UserId INNER JOIN
							Invoice.tbProject AS tbInvoiceProject ON sales_invoice.InvoiceNumber = tbInvoiceProject.InvoiceNumber INNER JOIN
							Project.tbProject ON tbInvoiceProject.ProjectCode = Project.tbProject.ProjectCode INNER JOIN
							Object.tbObject ON Project.tbProject.ObjectCode = Object.tbObject.ObjectCode INNER JOIN
							Invoice.tbType ON sales_invoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
	WHERE        EXISTS
								(SELECT        UserName, DocTypeCode, DocumentNumber, SpooledOn
									FROM            App.tbDocSpool AS doc
									WHERE        (DocTypeCode = 4) AND (UserName = SUSER_SNAME()) AND (sales_invoice.InvoiceNumber = DocumentNumber))
	GROUP BY sales_invoice.InvoiceNumber, Invoice.tbType.InvoiceType, sales_invoice.InvoiceStatusCode, Usr.tbUser.UserName, sales_invoice.SubjectCode, Subject.tbSubject.SubjectName, 
							Invoice.tbStatus.InvoiceStatus, sales_invoice.InvoicedOn, sales_invoice.InvoiceValue, sales_invoice.TaxValue, sales_invoice.PaymentTerms, Subject.tbSubject.EmailAddress, Subject.tbSubject.AddressCode, 
							Object.tbObject.ObjectCode, Object.tbObject.UnitOfMeasure, tbInvoiceProject.TaxCode
)
SELECT        invoice_1.InvoiceNumber, invoice_1.InvoiceType, invoice_1.InvoiceStatusCode, invoice_1.UserName, invoice_1.SubjectCode, invoice_1.SubjectName, invoice_1.InvoiceStatus, invoice_1.InvoicedOn, 
                        Invoice.tbInvoice.Notes, Subject.tbAddress.Address AS InvoiceAddress, invoice_1.InvoiceValueTotal, invoice_1.TaxValueTotal, invoice_1.PaymentTerms, invoice_1.EmailAddress, invoice_1.AddressCode, 
                        invoice_1.ObjectCode, invoice_1.UnitOfMeasure, invoice_1.FirstActionedOn, invoice_1.ObjectQuantity, invoice_1.TaxCode, invoice_1.ObjectInvoiceValue, invoice_1.ObjectTaxValue
FROM            invoice AS invoice_1 INNER JOIN
                        Invoice.tbInvoice ON invoice_1.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber LEFT OUTER JOIN
                        Subject.tbAddress ON invoice_1.AddressCode = Subject.tbAddress.AddressCode;
go
PRINT N'Creating View [Invoice].[vwSalesInvoiceSpoolByItem]...';


go
CREATE   VIEW Invoice.vwSalesInvoiceSpoolByItem
AS
	SELECT  sales_invoice.InvoiceNumber, Invoice.tbType.InvoiceType, sales_invoice.InvoiceStatusCode, Usr.tbUser.UserName, sales_invoice.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbStatus.InvoiceStatus, 
							 sales_invoice.InvoicedOn, sales_invoice.InvoiceValue AS InvoiceValueTotal, sales_invoice.TaxValue AS TaxValueTotal, sales_invoice.PaymentTerms, sales_invoice.DueOn, sales_invoice.Notes, Subject.tbSubject.EmailAddress, 
							 Subject.tbAddress.Address AS InvoiceAddress, tbInvoiceItem.CashCode, Cash.tbCode.CashDescription, tbInvoiceItem.ItemReference, tbInvoiceItem.TaxCode, tbInvoiceItem.InvoiceValue, tbInvoiceItem.TaxValue
	FROM            Invoice.tbInvoice AS sales_invoice INNER JOIN
							 Invoice.tbStatus ON sales_invoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							 Subject.tbSubject ON sales_invoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Usr.tbUser ON sales_invoice.UserId = Usr.tbUser.UserId LEFT OUTER JOIN
							 Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode INNER JOIN
							 Invoice.tbItem AS tbInvoiceItem ON sales_invoice.InvoiceNumber = tbInvoiceItem.InvoiceNumber INNER JOIN
							 Invoice.tbType ON sales_invoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 Cash.tbCode ON tbInvoiceItem.CashCode = Cash.tbCode.CashCode
	WHERE        (sales_invoice.InvoiceTypeCode = 0) AND EXISTS
								 (SELECT        UserName, DocTypeCode, DocumentNumber, SpooledOn, RowVer
								   FROM            App.tbDocSpool AS doc
								   WHERE        (DocTypeCode = 4) AND (UserName = SUSER_SNAME()) AND (sales_invoice.InvoiceNumber = DocumentNumber))
go
PRINT N'Creating View [Invoice].[vwRegisterPurchasesOverdue]...';


go
CREATE VIEW Invoice.vwRegisterPurchasesOverdue
AS
SELECT        TOP (100) PERCENT Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, Invoice.tbStatus.InvoiceStatus, 
                         Invoice.tbType.InvoiceType, DATEDIFF(DD, CURRENT_TIMESTAMP, Invoice.tbInvoice.InvoicedOn) AS UnpaidDays, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.DueOn, Invoice.tbInvoice.ExpectedOn, 
                         CASE Invoice.tbType.CashPolarityCode WHEN 0 THEN Invoice.tbInvoice.InvoiceValue ELSE Invoice.tbInvoice.InvoiceValue * - 1 END AS InvoiceValue, 
                         CASE Invoice.tbType.CashPolarityCode WHEN 0 THEN Invoice.tbInvoice.TaxValue ELSE Invoice.tbInvoice.TaxValue * - 1 END AS TaxValue, 
                         CASE Invoice.tbType.CashPolarityCode WHEN 0 THEN (Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue) - (Invoice.tbInvoice.PaidValue + Invoice.tbInvoice.PaidTaxValue) 
                         ELSE ((Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue) - (Invoice.tbInvoice.PaidValue + Invoice.tbInvoice.PaidTaxValue)) * - 1 END AS UnpaidValue, Invoice.tbMirror.PaymentAddress, 
                         Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Notes
FROM            Invoice.tbInvoice INNER JOIN
                         Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
                         Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
                         Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode LEFT OUTER JOIN
                         Invoice.tbMirrorReference ON Invoice.tbInvoice.InvoiceNumber = Invoice.tbMirrorReference.InvoiceNumber LEFT OUTER JOIN
                         Invoice.tbMirror ON Invoice.tbMirrorReference.ContractAddress = Invoice.tbMirror.ContractAddress
WHERE        (Invoice.tbInvoice.InvoiceTypeCode > 1) AND (Invoice.tbInvoice.InvoiceStatusCode < 3)
go
PRINT N'Creating View [Invoice].[vwAgedDebtSales]...';


go
CREATE VIEW Invoice.vwAgedDebtSales
AS
SELECT TOP 100 PERCENT  Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, Invoice.tbStatus.InvoiceStatus, 
                         Invoice.tbType.InvoiceType, (Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue) - (Invoice.tbInvoice.PaidValue + Invoice.tbInvoice.PaidTaxValue) AS UnpaidValue, DATEDIFF(DD, CURRENT_TIMESTAMP, 
                         Invoice.tbInvoice.InvoicedOn) AS UnpaidDays, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.DueOn, Invoice.tbInvoice.ExpectedOn, Invoice.tbInvoice.InvoiceValue, Invoice.tbInvoice.TaxValue, Invoice.tbInvoice.PaymentTerms, 
                         Invoice.tbInvoice.Notes
FROM            Invoice.tbInvoice INNER JOIN
                         Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
                         Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
                         Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode
WHERE        (Invoice.tbInvoice.InvoiceTypeCode < 2) AND (Invoice.tbInvoice.InvoiceStatusCode < 3)
ORDER BY Invoice.tbInvoice.ExpectedOn;
go
PRINT N'Creating View [Invoice].[vwAgedDebtPurchases]...';


go
CREATE VIEW Invoice.vwAgedDebtPurchases
AS
SELECT TOP 100 PERCENT Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, Invoice.tbStatus.InvoiceStatus, 
                         Invoice.tbType.InvoiceType, (Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue) - (Invoice.tbInvoice.PaidValue + Invoice.tbInvoice.PaidTaxValue) AS UnpaidValue, DATEDIFF(DD, CURRENT_TIMESTAMP, 
                         Invoice.tbInvoice.InvoicedOn) AS UnpaidDays, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.DueOn, Invoice.tbInvoice.ExpectedOn, Invoice.tbInvoice.InvoiceValue, Invoice.tbInvoice.TaxValue, Invoice.tbInvoice.PaymentTerms, 
                         Invoice.tbInvoice.Notes
FROM            Invoice.tbInvoice INNER JOIN
                         Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
                         Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
                         Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode
WHERE        (Invoice.tbInvoice.InvoiceTypeCode > 1) AND (Invoice.tbInvoice.InvoiceStatusCode < 3)
ORDER BY Invoice.tbInvoice.ExpectedOn;
go
PRINT N'Creating View [Invoice].[vwChangeLog]...';


go
CREATE VIEW Invoice.vwChangeLog
AS
	SELECT        changelog.LogId, changelog.InvoiceNumber, Subject.tbSubject.SubjectCode, Subject.tbSubject.SubjectName, changelog.ChangedOn, changelog.TransmitStatusCode, transmit.TransmitStatus, changelog.InvoiceStatusCode, 
							 invoicestatus.InvoiceStatus, changelog.DueOn, changelog.InvoiceValue, changelog.TaxValue, changelog.PaidValue, changelog.PaidTaxValue, changelog.UpdatedBy
	FROM            Invoice.tbChangeLog AS changelog INNER JOIN
							 Subject.tbTransmitStatus AS transmit ON changelog.TransmitStatusCode = transmit.TransmitStatusCode INNER JOIN
							 Invoice.tbStatus AS invoicestatus ON changelog.InvoiceStatusCode = invoicestatus.InvoiceStatusCode INNER JOIN
							 Invoice.tbInvoice ON changelog.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber AND changelog.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
							 Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode AND Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode;
go
PRINT N'Creating View [Invoice].[vwCreditSpoolByItem]...';


go
CREATE   VIEW Invoice.vwCreditSpoolByItem
AS
	SELECT        credit_note.InvoiceNumber, Invoice.tbType.InvoiceType, credit_note.InvoiceStatusCode, Usr.tbUser.UserName, credit_note.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbStatus.InvoiceStatus, credit_note.InvoicedOn, 
							 credit_note.InvoiceValue AS InvoiceValueTotal, credit_note.TaxValue AS TaxValueTotal, credit_note.PaymentTerms, credit_note.DueOn, credit_note.Notes, Subject.tbSubject.EmailAddress, Subject.tbAddress.Address AS InvoiceAddress, 
							 tbInvoiceItem.CashCode, Cash.tbCode.CashDescription, tbInvoiceItem.ItemReference, tbInvoiceItem.TaxCode, tbInvoiceItem.InvoiceValue, tbInvoiceItem.TaxValue
	FROM            Invoice.tbInvoice AS credit_note INNER JOIN
							 Invoice.tbStatus ON credit_note.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							 Subject.tbSubject ON credit_note.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Usr.tbUser ON credit_note.UserId = Usr.tbUser.UserId LEFT OUTER JOIN
							 Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode INNER JOIN
							 Invoice.tbItem AS tbInvoiceItem ON credit_note.InvoiceNumber = tbInvoiceItem.InvoiceNumber INNER JOIN
							 Invoice.tbType ON credit_note.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 Cash.tbCode ON tbInvoiceItem.CashCode = Cash.tbCode.CashCode
	WHERE        (credit_note.InvoiceTypeCode = 1 OR
							 credit_note.InvoiceTypeCode = 3) AND EXISTS
								 (SELECT * FROM  App.tbDocSpool AS doc
								   WHERE (DocTypeCode BETWEEN 5 AND 6) AND (UserName = SUSER_SNAME()) AND (credit_note.InvoiceNumber = DocumentNumber))
go
PRINT N'Creating View [Invoice].[vwCreditNoteSpool]...';


go
CREATE VIEW Invoice.vwCreditNoteSpool
AS
SELECT        credit_note.InvoiceNumber, credit_note.Printed, Invoice.tbType.InvoiceType, credit_note.InvoiceStatusCode, Usr.tbUser.UserName, credit_note.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbStatus.InvoiceStatus, 
                         credit_note.InvoicedOn, credit_note.InvoiceValue AS InvoiceValueTotal, credit_note.TaxValue AS TaxValueTotal, credit_note.PaymentTerms, credit_note.Notes, Subject.tbSubject.EmailAddress, 
                         Subject.tbAddress.Address AS InvoiceAddress, tbInvoiceProject.ProjectCode, Project.tbProject.ProjectTitle, Project.tbProject.ActionedOn, tbInvoiceProject.Quantity, Object.tbObject.UnitOfMeasure, tbInvoiceProject.TaxCode, 
                         tbInvoiceProject.InvoiceValue, tbInvoiceProject.TaxValue
FROM            Invoice.tbInvoice AS credit_note INNER JOIN
                         Invoice.tbStatus ON credit_note.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
                         Subject.tbSubject ON credit_note.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
                         Usr.tbUser ON credit_note.UserId = Usr.tbUser.UserId LEFT OUTER JOIN
                         Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode INNER JOIN
                         Invoice.tbProject AS tbInvoiceProject ON credit_note.InvoiceNumber = tbInvoiceProject.InvoiceNumber INNER JOIN
                         Project.tbProject ON tbInvoiceProject.ProjectCode = Project.tbProject.ProjectCode INNER JOIN
                         Object.tbObject ON Project.tbProject.ObjectCode = Object.tbObject.ObjectCode INNER JOIN
                         Invoice.tbType ON credit_note.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
WHERE credit_note.InvoiceTypeCode = 1 
	AND EXISTS (SELECT * FROM App.tbDocSpool AS doc WHERE DocTypeCode = 5 AND UserName = SUSER_SNAME() AND credit_note.InvoiceNumber = doc.DocumentNumber);
go
PRINT N'Creating View [Invoice].[vwEntry]...';


go
CREATE   VIEW Invoice.vwEntry
AS
	SELECT        Invoice.tbEntry.UserId, Usr.tbUser.UserName, Invoice.tbEntry.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbEntry.CashCode, Cash.tbCode.CashDescription, Invoice.tbEntry.InvoiceTypeCode, Invoice.tbType.InvoiceType, 
							 Invoice.tbEntry.InvoicedOn, Invoice.tbEntry.TaxCode, App.tbTaxCode.TaxDescription, Cash.tbTaxType.TaxType, Invoice.tbEntry.ItemReference, Invoice.tbEntry.TotalValue, Invoice.tbEntry.InvoiceValue, 
							 Invoice.tbEntry.InvoiceValue + Invoice.tbEntry.TotalValue AS EntryValue
	FROM            Invoice.tbEntry INNER JOIN
							 Subject.tbSubject ON Invoice.tbEntry.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Cash.tbCode ON Invoice.tbEntry.CashCode = Cash.tbCode.CashCode INNER JOIN
							 Invoice.tbType ON Invoice.tbEntry.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 Usr.tbUser ON Invoice.tbEntry.UserId = Usr.tbUser.UserId INNER JOIN
							 App.tbTaxCode ON Invoice.tbEntry.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
							 Cash.tbTaxType ON App.tbTaxCode.TaxTypeCode = Cash.tbTaxType.TaxTypeCode AND App.tbTaxCode.TaxTypeCode = Cash.tbTaxType.TaxTypeCode AND App.tbTaxCode.TaxTypeCode = Cash.tbTaxType.TaxTypeCode AND 
							 App.tbTaxCode.TaxTypeCode = Cash.tbTaxType.TaxTypeCode AND App.tbTaxCode.TaxTypeCode = Cash.tbTaxType.TaxTypeCode
go
PRINT N'Creating View [Invoice].[vwDoc]...';


go
CREATE VIEW Invoice.vwDoc
AS
	SELECT     Subject.tbSubject.EmailAddress, Usr.tbUser.UserName, Subject.tbSubject.SubjectCode, Subject.tbSubject.SubjectName, Subject.tbAddress.Address AS InvoiceAddress, 
						  Invoice.tbInvoice.InvoiceNumber, Invoice.tbType.InvoiceType, Invoice.tbStatus.InvoiceStatus, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.DueOn, 
						  Invoice.tbInvoice.InvoiceValue, Invoice.tbInvoice.TaxValue, 
						  Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue AS TotalValue, 
						  Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Notes
	FROM         Invoice.tbInvoice INNER JOIN
						  Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
						  Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
						  Usr.tbUser ON Invoice.tbInvoice.UserId = Usr.tbUser.UserId INNER JOIN
						  Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode LEFT OUTER JOIN
						  Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode
go
PRINT N'Creating View [Invoice].[vwDebitNoteSpool]...';


go
CREATE VIEW Invoice.vwDebitNoteSpool
AS
SELECT        debit_note.Printed, debit_note.InvoiceNumber, Invoice.tbType.InvoiceType, debit_note.InvoiceStatusCode, Usr.tbUser.UserName, debit_note.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbStatus.InvoiceStatus, 
                         debit_note.InvoicedOn, debit_note.InvoiceValue AS InvoiceValueTotal, debit_note.TaxValue AS TaxValueTotal, debit_note.PaymentTerms, debit_note.Notes, Subject.tbSubject.EmailAddress, 
                         Subject.tbAddress.Address AS InvoiceAddress, tbInvoiceProject.ProjectCode, Project.tbProject.ProjectTitle, Project.tbProject.ActionedOn, tbInvoiceProject.Quantity, Object.tbObject.UnitOfMeasure, tbInvoiceProject.TaxCode, 
                         tbInvoiceProject.InvoiceValue, tbInvoiceProject.TaxValue
FROM            Invoice.tbInvoice AS debit_note INNER JOIN
                         Invoice.tbStatus ON debit_note.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
                         Subject.tbSubject ON debit_note.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
                         Usr.tbUser ON debit_note.UserId = Usr.tbUser.UserId LEFT OUTER JOIN
                         Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode INNER JOIN
                         Invoice.tbProject AS tbInvoiceProject ON debit_note.InvoiceNumber = tbInvoiceProject.InvoiceNumber INNER JOIN
                         Project.tbProject ON tbInvoiceProject.ProjectCode = Project.tbProject.ProjectCode INNER JOIN
                         Object.tbObject ON Project.tbProject.ObjectCode = Object.tbObject.ObjectCode INNER JOIN
                         Invoice.tbType ON debit_note.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
WHERE debit_note.InvoiceTypeCode = 3 AND
	EXISTS (SELECT * FROM App.tbDocSpool AS doc WHERE DocTypeCode = 6 AND UserName = SUSER_SNAME() AND debit_note.InvoiceNumber = doc.DocumentNumber);
go
PRINT N'Creating View [Invoice].[vwMirrors]...';


go

CREATE VIEW Invoice.vwMirrors
AS
SELECT        Invoice.tbMirror.ContractAddress, Invoice.tbMirror.SubjectCode, Subject.tbSubject.SubjectName, CASE WHEN tbMirrorReference.ContractAddress IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END AS IsMirrored, 
                         Invoice.tbMirrorReference.InvoiceNumber, Invoice.tbMirror.InvoiceNumber AS MirrorNumber, Invoice.tbMirror.InvoiceTypeCode, Invoice.tbType.InvoiceType, Invoice.tbType.CashPolarityCode, Invoice.tbMirror.InvoiceStatusCode, 
                         Invoice.tbStatus.InvoiceStatus, Invoice.tbMirror.InvoicedOn, Invoice.tbMirror.DueOn, Invoice.tbMirror.UnitOfCharge, CASE CashPolarityCode WHEN 0 THEN InvoiceValue * - 1 ELSE InvoiceValue END AS InvoiceValue, 
                         CASE CashPolarityCode WHEN 0 THEN InvoiceTax * - 1 ELSE InvoiceTax END AS InvoiceTax, CASE CashPolarityCode WHEN 0 THEN PaidValue ELSE PaidValue * - 1 END AS PaidValue, 
                         CASE CashPolarityCode WHEN 0 THEN PaidTaxValue ELSE PaidTaxValue * - 1 END AS PaidTaxValue, Invoice.tbMirror.PaymentAddress, Invoice.tbMirror.PaymentTerms, Invoice.tbMirror.InsertedOn, Invoice.tbMirror.RowVer
FROM            Invoice.tbMirror INNER JOIN
                         Invoice.tbType ON Invoice.tbMirror.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
                         Invoice.tbStatus ON Invoice.tbMirror.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
                         Subject.tbSubject ON Invoice.tbMirror.SubjectCode = Subject.tbSubject.SubjectCode LEFT OUTER JOIN
                         Invoice.tbMirrorReference ON Invoice.tbMirror.ContractAddress = Invoice.tbMirrorReference.ContractAddress
go
PRINT N'Creating View [Invoice].[vwMirrorEvents]...';


go
CREATE VIEW Invoice.vwMirrorEvents
AS
	SELECT        Invoice.tbMirrorEvent.ContractAddress, Invoice.tbMirror.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbMirror.InvoiceNumber, Invoice.tbMirrorEvent.LogId, Invoice.tbMirrorEvent.EventTypeCode, App.tbEventType.EventType, 
							 Invoice.tbMirrorEvent.InvoiceStatusCode, Invoice.tbStatus.InvoiceStatus, Invoice.tbMirrorEvent.DueOn, Invoice.tbMirrorEvent.PaidValue, Invoice.tbMirrorEvent.PaidTaxValue, 
							 Invoice.tbMirrorEvent.PaymentAddress, Invoice.tbMirrorEvent.InsertedOn, Invoice.tbMirrorEvent.RowVer
	FROM            Invoice.tbMirrorEvent INNER JOIN
							 Invoice.tbMirror ON Invoice.tbMirrorEvent.ContractAddress = Invoice.tbMirror.ContractAddress INNER JOIN
							 Invoice.tbType ON Invoice.tbMirror.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 App.tbEventType ON Invoice.tbMirrorEvent.EventTypeCode = App.tbEventType.EventTypeCode INNER JOIN
							 Invoice.tbStatus ON Invoice.tbMirrorEvent.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							 Subject.tbSubject ON Invoice.tbMirror.SubjectCode = Subject.tbSubject.SubjectCode AND Invoice.tbMirror.SubjectCode = Subject.tbSubject.SubjectCode AND Invoice.tbMirror.SubjectCode = Subject.tbSubject.SubjectCode AND 
							 Invoice.tbMirror.SubjectCode = Subject.tbSubject.SubjectCode AND Invoice.tbMirror.SubjectCode = Subject.tbSubject.SubjectCode AND Invoice.tbMirror.SubjectCode = Subject.tbSubject.SubjectCode;
go
PRINT N'Creating View [Invoice].[vwNetworkChangeLog]...';


go
CREATE VIEW Invoice.vwNetworkChangeLog
AS
	SELECT        Invoice.tbChangeLog.LogId, Invoice.tbInvoice.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbType.InvoiceType, Invoice.tbChangeLog.InvoiceStatusCode, 
							 Invoice.tbStatus.InvoiceStatus, Invoice.tbChangeLog.TransmitStatusCode, Subject.tbTransmitStatus.TransmitStatus, Invoice.tbType.CashPolarityCode, Cash.tbPolarity.CashPolarity, Invoice.tbChangeLog.DueOn, 
							 Invoice.tbChangeLog.InvoiceValue, Invoice.tbChangeLog.TaxValue, Invoice.tbChangeLog.PaidValue, Invoice.tbChangeLog.PaidTaxValue, Invoice.tbChangeLog.UpdatedBy, Invoice.tbChangeLog.ChangedOn, 
							 Invoice.tbChangeLog.RowVer
	FROM            Invoice.tbChangeLog INNER JOIN
							 Invoice.tbInvoice ON Invoice.tbChangeLog.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
							 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 Cash.tbPolarity ON Invoice.tbType.CashPolarityCode = Cash.tbPolarity.CashPolarityCode INNER JOIN
							 Invoice.tbStatus ON Invoice.tbChangeLog.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							 Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode AND Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Subject.tbTransmitStatus ON Invoice.tbChangeLog.TransmitStatusCode = Subject.tbTransmitStatus.TransmitStatusCode;
go
PRINT N'Creating View [Invoice].[vwRegisterDetail]...';


go
CREATE VIEW Invoice.vwRegisterDetail
AS
	WITH register AS
	(
		SELECT     StartOn, InvoiceNumber, ProjectCode, CashCode, CashDescription, TaxCode, TaxDescription, SubjectCode, InvoiceTypeCode, InvoiceStatusCode, 
							  InvoicedOn, DueOn, ExpectedOn, CAST(Quantity as float) Quantity, CAST(InvoiceValue as float) InvoiceValue, CAST(TaxValue as float) TaxValue, PaymentTerms, Printed, SubjectName, UserName, UserId, InvoiceStatus, CashPolarityCode, 
							  InvoiceType, CAST(1 as bit) IsProject, NULL ItemReference
		FROM         Invoice.vwRegisterProjects
		UNION
		SELECT     StartOn, InvoiceNumber, ProjectCode, CashCode, CashDescription, TaxCode, TaxDescription, SubjectCode, InvoiceTypeCode, InvoiceStatusCode, 
							  InvoicedOn, DueOn, ExpectedOn, CAST(0 as float) Quantity, CAST(InvoiceValue as float) InvoiceValue, CAST(TaxValue as float) TaxValue, PaymentTerms, Printed, SubjectName, UserName, UserId, InvoiceStatus, CashPolarityCode, 
							  InvoiceType, CAST(0 as bit) IsProject, ItemReference
		FROM         Invoice.vwRegisterItems
	)
	SELECT StartOn, InvoiceNumber, ProjectCode, CashCode, CashDescription, TaxCode, TaxDescription, SubjectCode, InvoiceTypeCode, InvoiceStatusCode, 
		InvoicedOn, DueOn, ExpectedOn, PaymentTerms, Printed, SubjectName, UserName, UserId, InvoiceStatus, CashPolarityCode, InvoiceType,
		Quantity, InvoiceValue, TaxValue, (InvoiceValue + TaxValue) TotalValue, IsProject, ItemReference
	FROM register;
go
PRINT N'Creating View [Invoice].[vwRegister]...';


go
CREATE VIEW Invoice.vwRegister
AS
	WITH register AS 
	(
		SELECT       (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= Invoice.tbInvoice.InvoicedOn) ORDER BY p.StartOn DESC) AS StartOn,  
				Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.SubjectCode, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, 
								 Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.DueOn, Invoice.tbInvoice.ExpectedOn, CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbInvoice.InvoiceValue * - 1 ELSE Invoice.tbInvoice.InvoiceValue END AS InvoiceValue, 
								 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbInvoice.TaxValue * - 1 ELSE Invoice.tbInvoice.TaxValue END AS TaxValue, 
								 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbInvoice.PaidValue * - 1 ELSE Invoice.tbInvoice.PaidValue END AS PaidValue, 
								 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbInvoice.PaidTaxValue * - 1 ELSE Invoice.tbInvoice.PaidTaxValue END AS PaidTaxValue, Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Notes, 
								 Invoice.tbInvoice.Printed, Subject.tbSubject.SubjectName, Usr.tbUser.UserName, Invoice.tbInvoice.UserId, Invoice.tbStatus.InvoiceStatus, Invoice.tbType.CashPolarityCode, Invoice.tbType.InvoiceType
		FROM            Invoice.tbInvoice INNER JOIN
								 Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
								 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
								 Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
								 Usr.tbUser ON Invoice.tbInvoice.UserId = Usr.tbUser.UserId
	)
	SELECT COALESCE(StartOn, CAST(getdate() as date)) StartOn, InvoiceNumber, SubjectCode, InvoiceTypeCode, InvoiceStatusCode, InvoicedOn, DueOn, ExpectedOn,
		CAST(InvoiceValue as float) InvoiceValue, CAST(TaxValue as float) TaxValue, CAST((InvoiceValue + TaxValue) as float) TotalInvoiceValue, 
		CAST(PaidValue as float) PaidValue, CAST(PaidTaxValue as float) PaidTaxValue, CAST((PaidValue + PaidTaxValue) as float) TotalPaidValue,
		PaymentTerms, Notes, Printed, SubjectName, UserName, UserId, InvoiceStatus, CashPolarityCode, InvoiceType
	FROM register;
go
PRINT N'Creating View [Invoice].[vwHistorySalesItems]...';


go
CREATE VIEW Invoice.vwHistorySalesItems
AS
	SELECT        App.tbYearPeriod.YearNumber, CONCAT(App.tbMonth.MonthName, SPACE(1), YEAR(App.tbYearPeriod.StartOn)) AS PeriodName, Invoice.vwRegisterDetail.StartOn, Invoice.vwRegisterDetail.InvoiceNumber, 
							 Invoice.vwRegisterDetail.ProjectCode, Invoice.vwRegisterDetail.CashCode, Invoice.vwRegisterDetail.CashDescription, Invoice.vwRegisterDetail.TaxCode, Invoice.vwRegisterDetail.SubjectCode, Invoice.vwRegisterDetail.InvoicedOn, 
							 Invoice.vwRegisterDetail.InvoiceValue, Invoice.vwRegisterDetail.TaxValue, Invoice.vwRegisterDetail.PaymentTerms, 
							 Invoice.vwRegisterDetail.SubjectName, Invoice.vwRegisterDetail.InvoiceStatus, Invoice.vwRegisterDetail.InvoiceType, Invoice.vwRegisterDetail.InvoiceTypeCode, 
							 Invoice.vwRegisterDetail.InvoiceStatusCode
	FROM            Invoice.vwRegisterDetail INNER JOIN
							 App.tbYearPeriod ON Invoice.vwRegisterDetail.StartOn = App.tbYearPeriod.StartOn INNER JOIN
							 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
							 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
	WHERE        (Invoice.vwRegisterDetail.InvoiceTypeCode < 2);
go
PRINT N'Creating View [Cash].[vwTaxCorpAccruals]...';


go
CREATE VIEW Cash.vwTaxCorpAccruals
AS
	WITH corptax_ordered_confirmed AS
	(
		SELECT        Project.ProjectCode, Project.ActionOn, Project.Quantity, CASE WHEN Cash.tbCategory.CashPolarityCode = 0 THEN Project.TotalCharge * - 1 ELSE Project.TotalCharge END AS TotalCharge
		FROM            Project.tbProject AS Project INNER JOIN
								 Cash.tbCode ON Project.CashCode = Cash.tbCode.CashCode INNER JOIN
								 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode AND Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
		WHERE        (Project.ProjectStatusCode BETWEEN 1 AND 2) AND (Project.ActionOn <= (SELECT DATEADD(d, TaxHorizon, CURRENT_TIMESTAMP) AS HorizonOn FROM App.tbOptions))
	), corptax_ordered_invoices AS
	(
		SELECT corptax_ordered_confirmed.ProjectCode, Project_invoice.Quantity,
			CASE WHEN invoice_type.CashPolarityCode = 0 THEN Project_invoice.InvoiceValue * -1 ELSE Project_invoice.InvoiceValue END AS InvoiceValue
		FROM corptax_ordered_confirmed JOIN Invoice.tbProject Project_invoice ON corptax_ordered_confirmed.ProjectCode = Project_invoice.ProjectCode
			JOIN Invoice.tbInvoice invoice ON Project_invoice.InvoiceNumber = invoice.InvoiceNumber
			JOIN Invoice.tbType invoice_type ON invoice_type.InvoiceTypeCode = invoice.InvoiceTypeCode
	), corptax_ordered AS
	(
		SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod WHERE (StartOn <= corptax_ordered_confirmed.ActionOn) ORDER BY StartOn DESC) AS StartOn, 
			corptax_ordered_confirmed.ProjectCode,
			corptax_ordered_confirmed.Quantity - ISNULL(corptax_ordered_invoices.Quantity, 0) AS QuantityRemaining,
			corptax_ordered_confirmed.TotalCharge - ISNULL(corptax_ordered_invoices.InvoiceValue, 0) AS OrderValue
		FROM corptax_ordered_confirmed 
			LEFT JOIN corptax_ordered_invoices ON corptax_ordered_confirmed.ProjectCode = corptax_ordered_invoices.ProjectCode
	)
	SELECT corptax_ordered.StartOn, ProjectCode, QuantityRemaining, OrderValue, OrderValue * CorporationTaxRate AS TaxDue
	FROM corptax_ordered JOIN App.tbYearPeriod year_period ON corptax_ordered.StartOn = year_period.StartOn;
go
PRINT N'Creating View [Cash].[vwProfitAndLossData]...';


go
CREATE   VIEW Cash.vwProfitAndLossData
AS
	WITH category_relations AS
	(
		SELECT Cash.tbCategoryTotal.ParentCode, Cash.tbCategoryTotal.ChildCode, 
			Cash.tbCategory.CategoryTypeCode, Cash.tbCode.CashCode, Cash.tbCategory.CashTypeCode, Cash.tbCategory.CashPolarityCode
		FROM  Cash.tbCategoryTotal 
			INNER JOIN Cash.tbCategory ON Cash.tbCategoryTotal.ChildCode = Cash.tbCategory.CategoryCode 
			LEFT OUTER JOIN Cash.tbCode ON Cash.tbCategory.CategoryCode = Cash.tbCode.CategoryCode
	), categories AS
	(
		SELECT CategoryCode, CashTypeCode
		FROM  Cash.tbCategory category 
		WHERE (CategoryTypeCode = 1)
			AND NOT EXISTS (SELECT * FROM App.tbOptions o WHERE o.VatCategoryCode = category.CategoryCode) 
			
	), cashcode_candidates AS
	(
		SELECT categories.CategoryCode, ChildCode, CashCode, CashPolarityCode
		FROM category_relations
			JOIN categories ON category_relations.ParentCode = categories.CategoryCode		

		UNION ALL

		SELECT  cashcode_candidates.CategoryCode, category_relations.ChildCode, category_relations.CashCode, category_relations.CashPolarityCode
		FROM  category_relations JOIN cashcode_candidates ON category_relations.ParentCode = cashcode_candidates.ChildCode
	), cashcode_selected AS
	(
		SELECT CategoryCode, CashCode, CashPolarityCode FROM cashcode_candidates
		UNION
		SELECT ParentCode CategoryCode, CashCode, CashPolarityCode FROM category_relations WHERE ParentCode = (SELECT NetProfitCode FROM App.tbOptions)
	), category_cash_codes AS
	(
		SELECT DISTINCT CategoryCode, CashCode, CashPolarityCode
		FROM cashcode_selected WHERE NOT CashCode IS NULL
	), active_periods AS
	(
		SELECT yr.YearNumber, pd.StartOn
		FROM App.tbYear yr
			JOIN App.tbYearPeriod pd ON yr.YearNumber = pd.YearNumber
		WHERE (yr.CashStatusCode BETWEEN 1 AND 2)
	), category_data AS
	(
		SELECT category_cash_codes.CategoryCode, CashTypeCode, periods.CashCode, periods.StartOn, 
			CASE category_cash_codes.CashPolarityCode WHEN 0 THEN periods.InvoiceValue * -1 ELSE InvoiceValue END InvoiceValue
		FROM category_cash_codes 
			JOIN Cash.tbCategory category ON category_cash_codes.CategoryCode = category.CategoryCode
			JOIN Cash.tbPeriod periods ON category_cash_codes.CashCode = periods.CashCode
			JOIN active_periods ON active_periods.StartOn = periods.StartOn
	)
	SELECT CategoryCode, CashTypeCode, StartOn, SUM(InvoiceValue) InvoiceValue
	FROM category_data
	GROUP BY CategoryCode, CashTypeCode, StartOn;
go
PRINT N'Creating View [Cash].[vwCode]...';


go
CREATE   VIEW Cash.vwCode
AS
	SELECT        Cash.tbCode.CashCode, Cash.tbCode.CashDescription, Cash.tbCode.CategoryCode, Cash.tbCategory.Category, Cash.tbPolarity.CashPolarityCode, Cash.tbPolarity.CashPolarity, App.tbTaxCode.TaxDescription, 
							 Cash.tbCategory.CashTypeCode, Cash.tbType.CashType, CAST(Cash.tbCode.IsEnabled AS bit) AS IsCashEnabled, CAST(Cash.tbCategory.IsEnabled AS bit) AS IsCategoryEnabled, Cash.tbCode.InsertedBy, 
							 Cash.tbCode.InsertedOn, Cash.tbCode.UpdatedBy, Cash.tbCode.UpdatedOn
	FROM            Cash.tbCode INNER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
							 Cash.tbPolarity ON Cash.tbCategory.CashPolarityCode = Cash.tbPolarity.CashPolarityCode INNER JOIN
							 Cash.tbType ON Cash.tbCategory.CashTypeCode = Cash.tbType.CashTypeCode INNER JOIN
							 App.tbTaxCode ON Cash.tbCode.TaxCode = App.tbTaxCode.TaxCode
go
PRINT N'Creating View [Cash].[vwFlowCategoryByPeriod]...';


go
CREATE   VIEW Cash.vwFlowCategoryByPeriod
AS
	SELECT cats.CategoryCode, cash_codes.CashCode, cash_codes.CashDescription,	
		YearNumber, year_period.StartOn, year_period.MonthNumber, CASE cats.CashPolarityCode WHEN 0 THEN InvoiceValue * -1 ELSE InvoiceValue END InvoiceValue
	FROM Cash.tbCategory cats
		JOIN Cash.tbCode cash_codes ON cats.CategoryCode = cash_codes.CategoryCode
		JOIN Cash.tbPeriod cash_periods ON cash_codes.CashCode = cash_periods.CashCode
		JOIN App.tbYearPeriod year_period ON cash_periods.StartOn = year_period.StartOn
	WHERE cash_codes.IsEnabled <> 0
go
PRINT N'Creating View [Cash].[vwTxReference]...';


go
CREATE   VIEW Cash.vwTxReference
AS
	WITH tx AS
	(
		SELECT TxNumber
		FROM Cash.tbTx
	), pay_in AS
	(
		SELECT TxNumber, PaymentCode PaymentCodeIn
		FROM Cash.tbTxReference
		WHERE TxStatusCode = 1
	), pay_out AS
	(
		SELECT TxNumber, PaymentCode PaymentCodeOut
		FROM Cash.tbTxReference
		WHERE TxStatusCode = 2
	)
	SELECT tx.TxNumber, PaymentCodeIn, PaymentCodeOut
	FROM tx 
		LEFT OUTER JOIN pay_in ON tx.TxNumber = pay_in.TxNumber
		LEFT OUTER JOIN pay_out ON tx.TxNumber = pay_out.TxNumber;
go
PRINT N'Creating View [Cash].[vwProfitAndLossByYear]...';


go
CREATE   VIEW Cash.vwProfitAndLossByYear
AS
	SELECT financial_year.YearNumber, financial_year.Description, category.DisplayOrder, category.CategoryCode, category.Category, category.CashTypeCode, SUM(profit_data.InvoiceValue) InvoiceValue
	FROM Cash.vwProfitAndLossData profit_data
		JOIN Cash.tbCategory category ON profit_data.CategoryCode = category.CategoryCode
		JOIN App.tbYearPeriod periods ON profit_data.StartOn = periods.StartOn
		JOIN App.tbYear financial_year ON periods.YearNumber = financial_year.YearNumber
	GROUP BY financial_year.YearNumber, financial_year.Description, category.DisplayOrder, category.CategoryCode, category.Category, category.CashTypeCode;
go
PRINT N'Creating View [Cash].[vwBudgetDataEntry]...';


go
CREATE VIEW Cash.vwBudgetDataEntry
AS
SELECT        TOP (100) PERCENT App.tbYearPeriod.YearNumber, Cash.tbPeriod.CashCode, Cash.tbPeriod.StartOn, App.tbMonth.MonthName, Cash.tbPeriod.ForecastValue, Cash.tbPeriod.ForecastTax, Cash.tbPeriod.Note, 
                         Cash.tbPeriod.RowVer
FROM            App.tbYearPeriod INNER JOIN
                         Cash.tbPeriod ON App.tbYearPeriod.StartOn = Cash.tbPeriod.StartOn INNER JOIN
                         App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
go
PRINT N'Creating View [Cash].[vwBudget]...';


go
CREATE VIEW Cash.vwBudget
AS
SELECT TOP 100 PERCENT Cash.tbCode.CategoryCode, Cash.tbPeriod.CashCode, Cash.tbCode.CashDescription, 
	Cash.tbPeriod.StartOn, App.tbYearPeriod.YearNumber, App.tbMonth.MonthName, Format(App.tbYearPeriod.StartOn, 'yy-MM') AS Period,  
	Cash.tbPeriod.ForecastValue, Cash.tbPeriod.ForecastTax, Cash.tbPeriod.InvoiceValue, Cash.tbPeriod.InvoiceTax, Cash.tbPeriod.Note, Cash.tbPolarity.CashPolarity, App.tbTaxCode.TaxRate
FROM            App.tbYearPeriod INNER JOIN
                         Cash.tbPeriod ON App.tbYearPeriod.StartOn = Cash.tbPeriod.StartOn INNER JOIN
                         App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
                         Cash.tbCode ON Cash.tbPeriod.CashCode = Cash.tbCode.CashCode INNER JOIN
                         Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
						 Cash.tbPolarity ON Cash.tbCategory.CashPolarityCode = Cash.tbPolarity.CashPolarityCode INNER JOIN
                         App.tbTaxCode ON Cash.tbCode.TaxCode = App.tbTaxCode.TaxCode
go
PRINT N'Creating View [Cash].[vwCategoryCapital]...';


go
CREATE   VIEW Cash.vwCategoryCapital
AS
	SELECT DISTINCT category.CategoryCode, category.Category, category.DisplayOrder, cat_type.CategoryType, cash_type.CashType, cash_mode.CashPolarity,
		cat_type.CategoryTypeCode, cash_type.CashTypeCode, cash_mode.CashPolarityCode
	FROM Subject.tbAccount account
		JOIN Cash.tbCode cash_code ON account.CashCode = cash_code.CashCode
		JOIN Cash.tbCategory category ON cash_code.CategoryCode = category.CategoryCode
		JOIN Cash.tbType cash_type ON category.CashTypeCode = cash_type.CashTypeCode
		JOIN Cash.tbCategoryType cat_type ON category.CategoryTypeCode = cat_type.CategoryTypeCode
		JOIN Cash.tbPolarity cash_mode ON category.CashPolarityCode = cash_mode.CashPolarityCode
	WHERE (AccountTypeCode = 2);
go
PRINT N'Creating View [Cash].[vwCodeLookup]...';


go
CREATE VIEW Cash.vwCodeLookup
AS
	SELECT        Cash.tbCode.CashCode, Cash.tbCode.CashDescription, Cash.tbCategory.Category, Cash.tbPolarity.CashPolarityCode, Cash.tbPolarity.CashPolarity, Cash.tbCode.TaxCode, Cash.tbCategory.CashTypeCode, Cash.tbType.CashType
	FROM            Cash.tbCode INNER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
							 Cash.tbPolarity ON Cash.tbCategory.CashPolarityCode = Cash.tbPolarity.CashPolarityCode INNER JOIN
							 Cash.tbType ON Cash.tbCategory.CashTypeCode = Cash.tbType.CashTypeCode
	WHERE        (Cash.tbCode.IsEnabled <> 0) AND (Cash.tbCategory.IsEnabled <> 0)
go
PRINT N'Creating View [Cash].[vwCategoryTrade]...';


go

CREATE   VIEW Cash.vwCategoryTrade
AS
SELECT        CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled, InsertedBy, InsertedOn, UpdatedBy, UpdatedOn, RowVer
FROM            Cash.tbCategory
WHERE        (CategoryTypeCode = 0) AND (CashTypeCode = 0)
go
PRINT N'Creating View [Cash].[vwCategoryTotals]...';


go

CREATE   VIEW Cash.vwCategoryTotals
AS
	SELECT CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled, InsertedBy, InsertedOn, UpdatedBy, UpdatedOn, RowVer
	FROM            Cash.tbCategory
	WHERE       (CategoryTypeCode = 1)
go
PRINT N'Creating View [Cash].[vwCategoryTotalCandidates]...';


go
CREATE VIEW Cash.vwCategoryTotalCandidates
AS
	SELECT Cash.tbCategory.CategoryCode, Cash.tbCategory.Category, Cash.tbCategoryType.CategoryType, Cash.tbType.CashType, Cash.tbPolarity.CashPolarity
	FROM   Cash.tbCategory INNER JOIN
				Cash.tbCategoryType ON Cash.tbCategory.CategoryTypeCode = Cash.tbCategoryType.CategoryTypeCode INNER JOIN
				Cash.tbType ON Cash.tbCategory.CashTypeCode = Cash.tbType.CashTypeCode INNER JOIN
				Cash.tbPolarity ON Cash.tbCategory.CashPolarityCode = Cash.tbPolarity.CashPolarityCode
	WHERE        (Cash.tbCategory.CashTypeCode < 2) AND (Cash.tbCategory.IsEnabled <> 0)
	UNION
	SELECT CategoryCode, Category, CategoryType, CashType, CashPolarity
	FROM Cash.vwCategoryCapital
go
PRINT N'Creating View [Cash].[vwCategoryExpressions]...';


go

CREATE   VIEW Cash.vwCategoryExpressions
AS
	SELECT     TOP 100 PERCENT Cash.tbCategory.DisplayOrder, Cash.tbCategory.CategoryCode, Cash.tbCategory.Category, Cash.tbCategoryExp.Expression, 
						  Cash.tbCategoryExp.Format
	FROM         Cash.tbCategory INNER JOIN
						  Cash.tbCategoryExp ON Cash.tbCategory.CategoryCode = Cash.tbCategoryExp.CategoryCode
	WHERE     (Cash.tbCategory.CategoryTypeCode = 2)
go
PRINT N'Creating View [Cash].[vwCategoryCodesTrade]...';


go

CREATE   VIEW Cash.vwCategoryCodesTrade
AS
SELECT        CategoryCode, Category, DisplayOrder, CategoryTypeCode, CashPolarityCode, CashTypeCode
FROM            Cash.tbCategory
WHERE        (CategoryTypeCode = 0);
go
PRINT N'Creating View [Cash].[vwCategoryCodesTotals]...';


go


CREATE   VIEW Cash.vwCategoryCodesTotals
AS
SELECT        CategoryCode, Category, DisplayOrder, CategoryTypeCode, CashPolarityCode, CashTypeCode
FROM            Cash.tbCategory
WHERE        (CategoryTypeCode = 1);
go
PRINT N'Creating View [Cash].[vwBalanceStartOn]...';


go
CREATE   VIEW Cash.vwBalanceStartOn
AS
	SELECT MIN(App.tbYearPeriod.StartOn) StartOn
	FROM  App.tbYearPeriod 
		JOIN App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
	WHERE (App.tbYear.CashStatusCode < 3) AND (App.tbYearPeriod.CashStatusCode < 3)
go
PRINT N'Creating View [Cash].[vwCategoryCodesExpressions]...';


go

CREATE   VIEW Cash.vwCategoryCodesExpressions
AS
SELECT        CategoryCode, Category, DisplayOrder, CategoryTypeCode, CashPolarityCode, CashTypeCode
FROM            Cash.tbCategory
WHERE        (CategoryTypeCode = 2);
go
PRINT N'Creating View [Cash].[vwCategoryBudget]...';


go

CREATE   VIEW Cash.vwCategoryBudget
AS
	SELECT CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled, InsertedBy, InsertedOn, UpdatedBy, UpdatedOn, RowVer
	FROM            Cash.tbCategory
	WHERE        (CategoryTypeCode = 0) AND (CashTypeCode = 0) AND (IsEnabled <> 0)
go
PRINT N'Creating View [Cash].[vwCashFlowTypes]...';


go

CREATE   VIEW Cash.vwCashFlowTypes
AS
SELECT        CashTypeCode, CashType
FROM            Cash.tbType
WHERE        (CashTypeCode < 2)
go
PRINT N'Creating View [Cash].[vwVATCodes]...';


go

CREATE   VIEW Cash.vwVATCodes
AS
SELECT        TaxCode, TaxDescription
FROM            App.tbTaxCode
WHERE        (TaxTypeCode = 1);
go
PRINT N'Creating View [Cash].[vwPeriods]...';


go

CREATE   VIEW Cash.vwPeriods
   AS
SELECT     Cash.tbCode.CashCode, App.tbYearPeriod.StartOn
FROM         App.tbYearPeriod CROSS JOIN
                      Cash.tbCode
go
PRINT N'Creating View [Cash].[vwExternalCodesLookup]...';


go

CREATE   VIEW Cash.vwExternalCodesLookup
AS
SELECT        Cash.tbCode.CashCode, Cash.tbCode.CashDescription, Cash.tbCategory.Category
FROM            Cash.tbCode INNER JOIN
                         Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
WHERE        (Cash.tbCategory.CashTypeCode = 1);
go
PRINT N'Creating View [Cash].[vwPaymentCode]...';


go
CREATE VIEW Cash.vwPaymentCode
AS
	SELECT CONCAT(LEFT((SELECT UserId FROM Usr.vwCredentials), 2), '_', FORMAT(CURRENT_TIMESTAMP, 'yyMMdd_HHmmss'), '_', DATEPART(MILLISECOND, CURRENT_TIMESTAMP)) AS PaymentCode
go
PRINT N'Creating View [Cash].[vwTaxVatAccruals]...';


go
CREATE VIEW Cash.vwTaxVatAccruals
AS
	WITH Project_invoiced_quantity AS
	(
		SELECT        Invoice.tbProject.ProjectCode, SUM(Invoice.tbProject.Quantity) AS InvoiceQuantity
		FROM            Invoice.tbProject INNER JOIN
								 Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE        (Invoice.tbInvoice.InvoiceTypeCode = 0) OR
								 (Invoice.tbInvoice.InvoiceTypeCode = 2)
		GROUP BY Invoice.tbProject.ProjectCode
	), Project_transactions AS
	(
		SELECT  (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= Project.tbProject.ActionOn) ORDER BY p.StartOn DESC) AS StartOn,  
				Project.tbProject.ProjectCode, Project.tbProject.TaxCode,
				Project.tbProject.Quantity - ISNULL(Project_invoiced_quantity.InvoiceQuantity, 0) AS QuantityRemaining,
				Project.tbProject.UnitCharge * (Project.tbProject.Quantity - ISNULL(Project_invoiced_quantity.InvoiceQuantity, 0)) AS TotalValue, 
				Project.tbProject.UnitCharge * (Project.tbProject.Quantity - ISNULL(Project_invoiced_quantity.InvoiceQuantity, 0)) * App.tbTaxCode.TaxRate AS TaxValue,
				App.tbTaxCode.TaxRate,
				Subject.tbSubject.EUJurisdiction,
				Cash.tbCategory.CashPolarityCode
		FROM    Project.tbProject INNER JOIN
				Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
				Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
				Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
				App.tbTaxCode ON Project.tbProject.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
				Project_invoiced_quantity ON Project.tbProject.ProjectCode = Project_invoiced_quantity.ProjectCode
		WHERE        (App.tbTaxCode.TaxTypeCode = 1) AND (Project.tbProject.ProjectStatusCode > 0) AND (Project.tbProject.ProjectStatusCode < 3) AND (App.tbTaxCode.TaxTypeCode = 1)
			AND (Project.tbProject.ActionOn <= (SELECT DATEADD(d, TaxHorizon, CURRENT_TIMESTAMP) FROM App.tbOptions))
	), Project_dataset AS
	(
		SELECT StartOn, ProjectCode, TaxCode, QuantityRemaining, TotalValue, TaxValue, TaxRate,
					CAST(CASE WHEN EUJurisdiction = 0 THEN (CASE CashPolarityCode WHEN 1 THEN TotalValue ELSE 0 END) ELSE 0 END as float) AS HomeSales, 
					CAST(CASE WHEN EUJurisdiction = 0 THEN (CASE CashPolarityCode WHEN 0 THEN TotalValue ELSE 0 END) ELSE 0 END as float) AS HomePurchases, 
					CAST(CASE WHEN EUJurisdiction != 0 THEN (CASE CashPolarityCode WHEN 1 THEN TotalValue ELSE 0 END) ELSE 0 END as float) AS ExportSales, 
					CAST(CASE WHEN EUJurisdiction != 0 THEN (CASE CashPolarityCode WHEN 0 THEN TotalValue ELSE 0 END) ELSE 0 END as float) AS ExportPurchases, 
					CAST(CASE WHEN EUJurisdiction = 0 THEN (CASE CashPolarityCode WHEN 1 THEN TaxValue ELSE 0 END) ELSE 0 END as float) AS HomeSalesVat, 
					CAST(CASE WHEN EUJurisdiction = 0 THEN (CASE CashPolarityCode WHEN 0 THEN TaxValue ELSE 0 END) ELSE 0 END as float) AS HomePurchasesVat, 
					CAST(CASE WHEN EUJurisdiction != 0 THEN (CASE CashPolarityCode WHEN 1 THEN TaxValue ELSE 0 END) ELSE 0 END as float) AS ExportSalesVat, 
					CAST(CASE WHEN EUJurisdiction != 0 THEN (CASE CashPolarityCode WHEN 0 THEN TaxValue ELSE 0 END)  ELSE 0 END as float) AS ExportPurchasesVat
		FROM Project_transactions
	)
	SELECT Project_dataset.*,
		 (HomeSalesVat + ExportSalesVat) - (HomePurchasesVat + ExportPurchasesVat) AS VatDue
	FROM Project_dataset
		JOIN App.tbYearPeriod AS year_period ON Project_dataset.StartOn = year_period.StartOn INNER JOIN
                         App.tbYear ON year_period.YearNumber = App.tbYear.YearNumber INNER JOIN
                         App.tbMonth ON year_period.MonthNumber = App.tbMonth.MonthNumber;
go
PRINT N'Creating View [Cash].[vwTaxVatAuditAccruals]...';


go
CREATE VIEW Cash.vwTaxVatAuditAccruals
AS
SELECT       App.tbYear.YearNumber, CONCAT(App.tbYear.Description, ' ', App.tbMonth.MonthName) AS YearPeriod, vat_accruals.StartOn, Project.tbProject.ActionOn, Project.tbProject.ProjectTitle, Project.tbProject.ProjectCode, Cash.tbCode.CashCode, 
                         Cash.tbCode.CashDescription, Object.tbObject.ObjectCode, Project.tbStatus.ProjectStatus, Project.tbStatus.ProjectStatusCode, vat_accruals.TaxCode, vat_accruals.TaxRate, vat_accruals.TotalValue, 
                         vat_accruals.TaxValue, vat_accruals.QuantityRemaining, Object.tbObject.UnitOfMeasure, vat_accruals.HomePurchases, vat_accruals.ExportSales, vat_accruals.ExportPurchases, vat_accruals.HomeSalesVat, 
                         vat_accruals.HomePurchasesVat, vat_accruals.ExportSalesVat, vat_accruals.ExportPurchasesVat, vat_accruals.VatDue, vat_accruals.HomeSales
FROM            Cash.vwTaxVatAccruals AS vat_accruals INNER JOIN
                         App.tbYearPeriod AS year_period ON vat_accruals.StartOn = year_period.StartOn INNER JOIN
                         App.tbYear ON year_period.YearNumber = App.tbYear.YearNumber INNER JOIN
                         App.tbMonth ON year_period.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
                         Project.tbProject ON vat_accruals.ProjectCode = Project.tbProject.ProjectCode INNER JOIN
                         Project.tbStatus ON Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode AND Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode AND 
                         Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode AND Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode AND Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode AND 
                         Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode INNER JOIN
                         Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode AND Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode AND Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode AND 
                         Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode AND Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode AND Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
                         Object.tbObject ON Project.tbProject.ObjectCode = Object.tbObject.ObjectCode AND Project.tbProject.ObjectCode = Object.tbObject.ObjectCode AND Project.tbProject.ObjectCode = Object.tbObject.ObjectCode AND 
                         Project.tbProject.ObjectCode = Object.tbObject.ObjectCode AND Project.tbProject.ObjectCode = Object.tbObject.ObjectCode AND Project.tbProject.ObjectCode = Object.tbObject.ObjectCode INNER JOIN
                         Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode AND Project.tbProject.CashCode = Cash.tbCode.CashCode AND Project.tbProject.CashCode = Cash.tbCode.CashCode AND 
                         Project.tbProject.CashCode = Cash.tbCode.CashCode AND Project.tbProject.CashCode = Cash.tbCode.CashCode AND Project.tbProject.CashCode = Cash.tbCode.CashCode
go
PRINT N'Creating View [Cash].[vwFlowTaxType]...';


go

CREATE   VIEW Cash.vwFlowTaxType
AS
	SELECT       Cash.tbTaxType.TaxTypeCode, Cash.tbTaxType.TaxType, Cash.tbTaxType.RecurrenceCode, App.tbRecurrence.Recurrence, Cash.tbTaxType.CashCode, Cash.tbCode.CashDescription, Cash.tbTaxType.MonthNumber, App.tbMonth.MonthName, Cash.tbTaxType.SubjectCode, 
								Cash.tbTaxType.OffsetDays
	FROM            Cash.tbTaxType INNER JOIN
								App.tbRecurrence ON Cash.tbTaxType.RecurrenceCode = App.tbRecurrence.RecurrenceCode INNER JOIN
								Cash.tbCode ON Cash.tbTaxType.CashCode = Cash.tbCode.CashCode INNER JOIN
								App.tbMonth ON Cash.tbTaxType.MonthNumber = App.tbMonth.MonthNumber
go
PRINT N'Creating View [Cash].[vwNetworkMirrors]...';


go
CREATE   VIEW Cash.vwNetworkMirrors
AS
	SELECT SubjectCode, CashCode, ChargeCode, TransmitStatusCode FROM Cash.tbMirror WHERE TransmitStatusCode BETWEEN 1 AND 2;
go
PRINT N'Creating View [Cash].[vwTaxCorpAuditAccruals]...';


go
CREATE VIEW Cash.vwTaxCorpAuditAccruals
AS
	SELECT     App.tbYear.YearNumber, CONCAT(App.tbYear.Description, ' ', App.tbMonth.MonthName) AS YearPeriod, Cash.vwTaxCorpAccruals.StartOn, Project.tbProject.ProjectCode, Project.tbProject.SubjectCode, Subject.tbSubject.SubjectName, 
							 Project.tbProject.ProjectTitle, Object.tbObject.ObjectCode, Project.tbStatus.ProjectStatusCode, Project.tbStatus.ProjectStatus, Project.tbProject.CashCode, Cash.tbCode.CashDescription, Object.tbObject.UnitOfMeasure, 
							 Cash.vwTaxCorpAccruals.QuantityRemaining, Cash.vwTaxCorpAccruals.OrderValue, Cash.vwTaxCorpAccruals.TaxDue
	FROM            Project.tbProject INNER JOIN
							 Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Cash.vwTaxCorpAccruals ON Project.tbProject.ProjectCode = Cash.vwTaxCorpAccruals.ProjectCode INNER JOIN
							 Project.tbStatus ON Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode AND Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode INNER JOIN
							 Object.tbObject ON Project.tbProject.ObjectCode = Object.tbObject.ObjectCode AND Project.tbProject.ObjectCode = Object.tbObject.ObjectCode INNER JOIN
							 Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode AND Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
							 App.tbYearPeriod ON Cash.vwTaxCorpAccruals.StartOn = App.tbYearPeriod.StartOn INNER JOIN
							 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND 
							 App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND 
							 App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
							 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber AND App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber AND 
							 App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber AND App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber AND App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber AND 
							 App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber AND App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
go
PRINT N'Creating View [Cash].[vwUnMirrored]...';


go
CREATE VIEW Cash.vwUnMirrored
AS
	WITH charge_codes AS
	(
		SELECT DISTINCT Invoice.tbMirror.SubjectCode, Invoice.tbMirrorItem.ChargeCode, Subject.tbSubject.SubjectName, Invoice.tbMirrorItem.ChargeDescription, Invoice.tbType.CashPolarityCode, Cash.tbPolarity.CashPolarity, 
			Invoice.tbMirrorItem.TaxCode, ROUND(Invoice.tbMirrorItem.TaxValue / Invoice.tbMirrorItem.InvoiceValue, 3) AS TaxRate
		FROM            Invoice.tbMirrorItem INNER JOIN
								 Invoice.tbMirror ON Invoice.tbMirrorItem.ContractAddress = Invoice.tbMirror.ContractAddress INNER JOIN
								 Subject.tbSubject ON Invoice.tbMirror.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
								 Invoice.tbType ON Invoice.tbMirror.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
								 Cash.tbPolarity ON Invoice.tbType.CashPolarityCode = Cash.tbPolarity.CashPolarityCode AND Invoice.tbType.CashPolarityCode = Cash.tbPolarity.CashPolarityCode
		WHERE        (Invoice.tbMirror.InvoiceTypeCode = 0) OR
								 (Invoice.tbMirror.InvoiceTypeCode = 2)
	)
	SELECT CAST(ROW_NUMBER() OVER (ORDER BY charge_codes.SubjectCode, charge_codes.ChargeCode) AS int) CandidateId, charge_codes.*
	FROM charge_codes 
		LEFT OUTER JOIN Cash.tbMirror mirror ON charge_codes.SubjectCode = mirror.SubjectCode AND charge_codes.ChargeCode = mirror.ChargeCode
	WHERE mirror.ChargeCode IS NULL;
go
PRINT N'Creating View [Cash].[vwTaxTypes]...';


go
CREATE   VIEW Cash.vwTaxTypes
AS
	SELECT        Cash.tbTaxType.TaxTypeCode, Cash.tbTaxType.TaxType, Cash.tbTaxType.CashCode, Cash.tbCode.CashDescription, Cash.tbTaxType.MonthNumber, App.tbMonth.[MonthName], Cash.tbTaxType.RecurrenceCode, 
							 App.tbRecurrence.Recurrence, Cash.tbTaxType.SubjectCode, Subject.tbSubject.SubjectName, Cash.tbTaxType.OffsetDays
	FROM            Cash.tbTaxType INNER JOIN
							 Cash.tbCode ON Cash.tbTaxType.CashCode = Cash.tbCode.CashCode INNER JOIN
							 App.tbMonth ON Cash.tbTaxType.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
							 App.tbRecurrence ON Cash.tbTaxType.RecurrenceCode = App.tbRecurrence.RecurrenceCode INNER JOIN
							 Subject.tbSubject ON Cash.tbTaxType.SubjectCode = Subject.tbSubject.SubjectCode;
go
PRINT N'Creating View [Cash].[vwTransfersUnposted]...';


go
CREATE VIEW Cash.vwTransfersUnposted
AS
	SELECT        PaymentCode, UserId, PaymentStatusCode, SubjectCode, AccountCode, CashCode, TaxCode, PaidOn, PaidInValue, PaidOutValue, PaymentReference, InsertedBy, InsertedOn, 
							 UpdatedBy, UpdatedOn, RowVer
	FROM            Cash.tbPayment
	WHERE        (PaymentStatusCode = 2)
go
PRINT N'Creating View [Cash].[vwTransferCodeLookup]...';


go
CREATE VIEW Cash.vwTransferCodeLookup
AS
	SELECT        Cash.tbCode.CashCode, Cash.tbCode.CashDescription, Cash.tbCategory.Category, Cash.tbPolarity.CashPolarity, Cash.tbPolarity.CashPolarityCode
	FROM            Cash.tbCode INNER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
							 Cash.tbPolarity ON Cash.tbCategory.CashPolarityCode = Cash.tbPolarity.CashPolarityCode LEFT OUTER JOIN
							 Subject.tbAccount ON Cash.tbCode.CashCode = Subject.tbAccount.CashCode
	WHERE        (Cash.tbCode.IsEnabled <> 0) AND (Cash.tbCategory.IsEnabled <> 0) AND (Cash.tbCategory.CashTypeCode = 2) AND (Cash.tbPolarity.CashPolarityCode < 2) AND (Subject.tbAccount.AccountCode IS NULL)
go
PRINT N'Creating View [Cash].[vwAccountRebuild]...';


go
CREATE VIEW Cash.vwAccountRebuild
AS
	SELECT     Cash.tbPayment.AccountCode, Subject.tbAccount.OpeningBalance, 
						  Subject.tbAccount.OpeningBalance + SUM(Cash.tbPayment.PaidInValue - Cash.tbPayment.PaidOutValue) AS CurrentBalance
	FROM         Cash.tbPayment INNER JOIN
						  Subject.tbAccount ON Cash.tbPayment.AccountCode = Subject.tbAccount.AccountCode
	WHERE     (Cash.tbPayment.PaymentStatusCode = 1) 
	GROUP BY Cash.tbPayment.AccountCode, Subject.tbAccount.OpeningBalance
go
PRINT N'Creating View [Cash].[vwPaymentsUnposted]...';


go
CREATE VIEW Cash.vwPaymentsUnposted
AS
	SELECT        PaymentCode, UserId, PaymentStatusCode, SubjectCode, AccountCode, CashCode, TaxCode, PaidOn, PaidInValue, PaidOutValue, PaymentReference, IsProfitAndLoss, InsertedBy, InsertedOn, 
							 UpdatedBy, UpdatedOn, RowVer
	FROM            Cash.tbPayment
	WHERE        (PaymentStatusCode = 0);
go
PRINT N'Creating View [Cash].[vwStatementReserves]...';


go
CREATE VIEW Cash.vwStatementReserves
AS
	WITH reserve_account AS
	(
		SELECT  Subject.tbAccount.AccountCode, Subject.tbAccount.AccountName, Subject.tbAccount.CurrentBalance
		FROM            Subject.tbAccount LEFT OUTER JOIN
								 Cash.tbCode ON Subject.tbAccount.CashCode = Cash.tbCode.CashCode 
		WHERE        (Subject.tbAccount.SubjectCode <> (SELECT SubjectCode FROM App.tbOptions))
			AND (Cash.tbCode.CashCode IS NULL) AND (Subject.tbAccount.AccountTypeCode = 0)
	), last_payment AS
	(
		SELECT MAX( payments.PaidOn) AS TransactOn
		FROM reserve_account JOIN Cash.tbPayment payments 
						ON reserve_account.AccountCode = payments.AccountCode 
		WHERE payments.PaymentStatusCode = 1
	
	), opening_balance AS
	(
		SELECT 	
			(SELECT SubjectCode FROM App.tbOptions) AS SubjectCode,		
			(SELECT TransactOn FROM last_payment) AS TransactOn,
			0 AS CashEntryTypeCode,
			(SELECT CAST(Message AS NVARCHAR) FROM App.tbText WHERE TextId = 1219) AS ReferenceCode,
			CASE WHEN SUM(CurrentBalance) > 0 THEN SUM(CurrentBalance) ELSE 0 END AS PayIn, 
			CASE WHEN SUM(CurrentBalance) < 0 THEN SUM(CurrentBalance) ELSE 0 END  AS PayOut
		FROM reserve_account 

	), unbalanced_reserves AS
	(
		SELECT  0 AS RowNumber, Subject.SubjectCode, Subject.SubjectName, TransactOn, CashEntryTypeCode, ReferenceCode, 
					PayOut, PayIn, NULL AS CashCode, NULL AS CashDescription
		FROM opening_balance
			JOIN Subject.tbSubject Subject ON opening_balance.SubjectCode = Subject.SubjectCode

		UNION
	
		SELECT ROW_NUMBER() OVER (ORDER BY payments.PaidOn) AS RowNumber, reserve_account.AccountCode AS SubjectCode,
			reserve_account.AccountName AS SubjectName,
			payments.PaidOn AS TransactOn, 6 AS CashEntryTypeCode, payments.PaymentCode AS ReferenceCode,  
			payments.PaidOutValue, payments.PaidInValue, payments.CashCode, cash_code.CashDescription 
		FROM reserve_account 
			JOIN Cash.tbPayment payments ON reserve_account.AccountCode = payments.AccountCode
			JOIN Cash.tbCode cash_code ON payments.CashCode = cash_code.CashCode
		WHERE payments.PaymentStatusCode = 2
	)
	SELECT RowNumber, TransactOn, entry_type.CashEntryTypeCode, entry_type.CashEntryType, ReferenceCode, unbalanced_reserves.SubjectCode, unbalanced_reserves.SubjectName,
		CAST(PayOut as float) PayOut, CAST(PayIn as float) PayIn,
		CAST(SUM(PayIn + (PayOut * -1)) OVER (ORDER BY RowNumber) as float) Balance,
		CashCode, CashDescription
	FROM unbalanced_reserves 
		JOIN Cash.tbEntryType entry_type ON unbalanced_reserves.CashEntryTypeCode = entry_type.CashEntryTypeCode
go
PRINT N'Creating View [Cash].[vwReserveAccount]...';


go
CREATE VIEW Cash.vwReserveAccount
AS
	SELECT TOP 1 AccountCode, LiquidityLevel, AccountName, AccountNumber, SortCode 
	FROM Subject.tbAccount 
			LEFT OUTER JOIN Cash.tbCode ON Subject.tbAccount.CashCode = Cash.tbCode.CashCode 
	WHERE (Cash.tbCode.CashCode) IS NULL AND (Subject.tbAccount.AccountTypeCode = 0) AND (Subject.tbAccount.AccountClosed = 0)
	ORDER BY AccountCode
go
PRINT N'Creating View [Cash].[vwPaymentsListing]...';


go
CREATE VIEW Cash.vwPaymentsListing
AS
	SELECT Subject.tbSubject.SubjectCode, Subject.tbSubject.SubjectName, Subject.tbType.SubjectType, Subject.tbStatus.SubjectStatus, Cash.tbPayment.PaymentCode, Usr.tbUser.UserName, 
							 App.tbTaxCode.TaxDescription AS PaymentTaxDescription, Subject.tbAccount.AccountName, Cash.tbCode.CashDescription, Cash.tbPayment.UserId, Cash.tbPayment.AccountCode, Cash.tbPayment.CashCode, 
							 Cash.tbPayment.TaxCode, CONCAT(YEAR(Cash.tbPayment.PaidOn), Format(MONTH(Cash.tbPayment.PaidOn), '00')) AS Period, Cash.tbPayment.PaidOn, Cash.tbPayment.PaidInValue, Cash.tbPayment.PaidOutValue, 
							 Cash.tbPayment.InsertedBy, Cash.tbPayment.InsertedOn, Cash.tbPayment.UpdatedBy, Cash.tbPayment.UpdatedOn, Cash.tbPayment.PaymentReference
	FROM            Cash.tbPayment INNER JOIN
							 Usr.tbUser ON Cash.tbPayment.UserId = Usr.tbUser.UserId INNER JOIN
							 Subject.tbAccount ON Cash.tbPayment.AccountCode = Subject.tbAccount.AccountCode INNER JOIN
							 Cash.tbCode ON Cash.tbPayment.CashCode = Cash.tbCode.CashCode LEFT OUTER JOIN
							 App.tbTaxCode ON Cash.tbPayment.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
							 Subject.tbSubject ON Cash.tbPayment.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Subject.tbType ON Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode INNER JOIN
							 Subject.tbStatus ON Subject.tbSubject.SubjectStatusCode = Subject.tbStatus.SubjectStatusCode
	WHERE        (Cash.tbPayment.PaymentStatusCode = 1);
go
PRINT N'Creating View [Cash].[vwPayments]...';


go
CREATE   VIEW Cash.vwPayments
AS
	SELECT        Cash.tbPayment.PaymentCode, Cash.tbPayment.PaymentStatusCode, Cash.tbPayment.UserId, Usr.tbUser.UserName, Subject.tbSubject.SubjectName, Cash.tbPayment.SubjectCode, Cash.tbPayment.AccountCode, Subject.tbAccount.AccountName, 
							 Cash.tbPayment.CashCode, Cash.tbCode.CashDescription, Cash.tbPayment.TaxCode, App.tbTaxCode.TaxDescription, Cash.tbPayment.PaidOn, Cash.tbPayment.PaidInValue, Cash.tbPayment.PaidOutValue, 
							 Cash.tbPayment.PaymentReference, Cash.tbPayment.IsProfitAndLoss, Cash.tbPayment.InsertedBy, Cash.tbPayment.InsertedOn, Cash.tbPayment.UpdatedBy, Cash.tbPayment.UpdatedOn
	FROM            Cash.tbPayment INNER JOIN
							 Usr.tbUser ON Cash.tbPayment.UserId = Usr.tbUser.UserId INNER JOIN
							 Subject.tbSubject ON Cash.tbPayment.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Subject.tbAccount ON Cash.tbPayment.AccountCode = Subject.tbAccount.AccountCode LEFT OUTER JOIN
							 App.tbTaxCode ON Cash.tbPayment.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
							 Cash.tbCode ON Cash.tbPayment.CashCode = Cash.tbCode.CashCode
go
PRINT N'Creating View [Cash].[vwCurrentAccount]...';


go
CREATE VIEW Cash.vwCurrentAccount
AS
	SELECT TOP (1) Subject.tbAccount.AccountCode, Subject.tbAccount.LiquidityLevel, Subject.tbAccount.AccountName, Subject.tbAccount.AccountNumber, Subject.tbAccount.SortCode, Subject.tbAccount.SubjectCode, Subject.tbSubject.SubjectName
	FROM            Subject.tbAccount INNER JOIN
							 Cash.tbCode ON Subject.tbAccount.CashCode = Cash.tbCode.CashCode INNER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
							 Subject.tbSubject ON Subject.tbAccount.SubjectCode = Subject.tbSubject.SubjectCode
	WHERE        (Cash.tbCategory.CashTypeCode = 2) AND (Subject.tbAccount.AccountTypeCode = 0) AND (Subject.tbAccount.AccountClosed = 0)
	ORDER BY Subject.tbAccount.AccountCode;
go
PRINT N'Creating View [Cash].[vwBalanceSheetPeriods]...';


go
CREATE VIEW Cash.vwBalanceSheetPeriods
AS
	WITH financial_periods AS
	(
		SELECT yr.YearNumber, pd.StartOn
		FROM App.tbYear yr
			JOIN App.tbYearPeriod pd ON yr.YearNumber = pd.YearNumber
		WHERE (yr.CashStatusCode BETWEEN 1 AND 2)
	), assets AS
	(
		SELECT AccountCode AssetCode, AccountName AssetName, LiquidityLevel, CAST(4 as smallint) AssetTypeCode, 
			category.CashPolarityCode,
			YearNumber, StartOn
		FROM Subject.tbAccount account
			JOIN Cash.tbCode cash_code ON account.CashCode = cash_code.CashCode
			JOIN Cash.tbCategory category ON cash_code.CategoryCode = category.CategoryCode
			CROSS JOIN financial_periods
		WHERE (AccountTypeCode= 2) AND (AccountClosed = 0)
	), cash AS
	(
		SELECT AccountCode AssetCode, AssetType AssetName, LiquidityLevel, AssetTypeCode, CAST(1 as smallint) CashPolarityCode, YearNumber, StartOn
		FROM Cash.tbAssetType
			CROSS JOIN Cash.vwCurrentAccount 
			CROSS JOIN financial_periods
		WHERE AssetTypeCode = 3
	), bank AS
	(
		SELECT AccountCode AssetCode, AssetType AssetName, LiquidityLevel, AssetTypeCode, CAST(1 as smallint) CashPolarityCode, YearNumber, StartOn
		FROM Cash.tbAssetType
			CROSS JOIN Cash.vwReserveAccount 
			CROSS JOIN financial_periods
		WHERE AssetTypeCode = 2
	), Subjects AS
	(
		SELECT AccountCode AssetCode, AssetType AssetName, LiquidityLevel, AssetTypeCode,
			CAST(CASE AssetTypeCode WHEN 0 THEN 1 ELSE 0 END as smallint) CashPolarityCode,
			YearNumber, StartOn
		FROM Cash.tbAssetType
			CROSS JOIN Cash.vwCurrentAccount
			CROSS JOIN financial_periods
		WHERE AssetTypeCode BETWEEN 0 AND 1
	), tax AS
	(
		SELECT UPPER(LEFT(TaxType, 3)) AssetCode, UPPER(TaxType) AssetName, CAST(1 as smallint) LiquidityLevel, CAST(1 as smallint) AssetTypeCode, CAST(0 as smallint) CashPolarityCode,
			YearNumber, StartOn
		FROM Cash.tbTaxType
			CROSS JOIN financial_periods
		WHERE TaxTypeCode BETWEEN 0 AND 1

	), asset_code_periods AS
	(
		SELECT AssetCode, AssetName, CashPolarityCode, LiquidityLevel, AssetTypeCode, YearNumber, StartOn FROM assets
		UNION 
		SELECT AssetCode, AssetName, CashPolarityCode, LiquidityLevel, AssetTypeCode, YearNumber, StartOn FROM cash
		UNION
		SELECT AssetCode, AssetName, CashPolarityCode, LiquidityLevel, AssetTypeCode, YearNumber, StartOn FROM bank
		UNION
		SELECT AssetCode, AssetName, CashPolarityCode, LiquidityLevel, AssetTypeCode, YearNumber, StartOn FROM Subjects
		UNION
		SELECT AssetCode, AssetName, CashPolarityCode, LiquidityLevel, AssetTypeCode, YearNumber, StartOn FROM tax
	)
	SELECT AssetCode, AssetName, CashPolarityCode, LiquidityLevel, AssetTypeCode, YearNumber, StartOn, CAST(0 as bit) IsEntry
	FROM asset_code_periods;
go
PRINT N'Creating View [Cash].[vwBankAccounts]...';


go
CREATE VIEW Cash.vwBankAccounts
AS
	SELECT AccountCode, AccountName, OpeningBalance, CASE WHEN NOT CashCode IS NULL THEN 0 ELSE 1 END AS DisplayOrder
	FROM Subject.tbAccount  
	WHERE (AccountTypeCode = 0)
go
PRINT N'Creating View [App].[vwCorpTaxCashCodes]...';


go
CREATE VIEW App.vwCorpTaxCashCodes
AS
	WITH category_relations AS
	(
		SELECT Cash.tbCategoryTotal.ParentCode, Cash.tbCategoryTotal.ChildCode, 
			Cash.tbCategory.CategoryTypeCode, Cash.tbCode.CashCode, Cash.tbCategory.CashTypeCode, Cash.tbCategory.CashPolarityCode
		FROM  Cash.tbCategoryTotal 
			INNER JOIN Cash.tbCategory ON Cash.tbCategoryTotal.ChildCode = Cash.tbCategory.CategoryCode 
			LEFT OUTER JOIN Cash.tbCode ON Cash.tbCategory.CategoryCode = Cash.tbCode.CategoryCode
	), cashcode_candidates AS
	(
		SELECT     ChildCode, CashCode, CashTypeCode, CashPolarityCode
		FROM category_relations
		WHERE     ( CategoryTypeCode = 1) AND ( ParentCode = (SELECT NetProfitCode FROM App.tbOptions))

		UNION ALL

		SELECT     category_relations.ChildCode, category_relations.CashCode, category_relations.CashTypeCode, category_relations.CashPolarityCode
		FROM  category_relations JOIN cashcode_candidates ON category_relations.ParentCode = cashcode_candidates.ChildCode
	), cashcode_selected AS
	(
		SELECT CashCode, CashTypeCode, CashPolarityCode FROM cashcode_candidates
		UNION
		SELECT CashCode, CashTypeCode, CashPolarityCode FROM category_relations WHERE ParentCode = (SELECT NetProfitCode FROM App.tbOptions)
	)
	SELECT CashCode, CashTypeCode, CashPolarityCode
	FROM cashcode_selected WHERE NOT CashCode IS NULL;
go
PRINT N'Creating View [App].[vwPeriods]...';


go
CREATE VIEW App.vwPeriods
AS
	SELECT TOP (100) PERCENT App.tbYear.YearNumber, App.tbYearPeriod.MonthNumber, App.tbYearPeriod.StartOn, App.tbYear.Description + SPACE(1) + App.tbMonth.MonthName AS Description, App.tbYearPeriod.CashStatusCode, App.tbYearPeriod.RowVer
	FROM            App.tbYearPeriod INNER JOIN
							 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
							 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
	WHERE        (App.tbYear.CashStatusCode < 3) AND (App.tbYearPeriod.CashStatusCode < 3)
go
PRINT N'Creating View [App].[vwVatTaxCashCodes]...';


go

CREATE   VIEW App.vwVatTaxCashCodes
AS
	WITH category_relations AS
	(
		SELECT Cash.tbCategoryTotal.ParentCode, Cash.tbCategoryTotal.ChildCode, Cash.tbCategory.CategoryTypeCode, Cash.tbCode.CashCode
		FROM  Cash.tbCategoryTotal 
			INNER JOIN Cash.tbCategory ON Cash.tbCategoryTotal.ChildCode = Cash.tbCategory.CategoryCode 
			LEFT OUTER JOIN Cash.tbCode ON Cash.tbCategory.CategoryCode = Cash.tbCode.CategoryCode
		WHERE Cash.tbCategory.CashTypeCode = 0
	), cashcode_candidates AS
	(
		SELECT     ChildCode, CashCode
		FROM category_relations
		WHERE     ( CategoryTypeCode = 1) AND ( ParentCode = (SELECT VatCategoryCode FROM App.tbOptions))

		UNION ALL

		SELECT     category_relations.ChildCode, category_relations.CashCode
		FROM  category_relations JOIN cashcode_candidates ON category_relations.ParentCode = cashcode_candidates.ChildCode
	), cashcode_selected AS
	(
		SELECT CashCode FROM cashcode_candidates
		UNION
		SELECT CashCode FROM category_relations WHERE ParentCode = (SELECT VatCategoryCode FROM App.tbOptions)
	)
	SELECT CashCode FROM cashcode_selected WHERE NOT CashCode IS NULL;
go
PRINT N'Creating View [App].[vwCandidateCategoryCodes]...';


go

CREATE   VIEW App.vwCandidateCategoryCodes
AS
	SELECT TOP 100 PERCENT CategoryCode, Category
	FROM            Cash.tbCategory
	WHERE        (CategoryTypeCode = 1)
	ORDER BY CategoryCode;
go
PRINT N'Creating View [App].[vwActiveYears]...';


go

CREATE   VIEW App.vwActiveYears
   AS
SELECT     TOP 100 PERCENT App.tbYear.YearNumber, App.tbYear.Description, Cash.tbStatus.CashStatus
FROM         App.tbYear INNER JOIN
                      Cash.tbStatus ON App.tbYear.CashStatusCode = Cash.tbStatus.CashStatusCode
WHERE     (App.tbYear.CashStatusCode < 3)
ORDER BY App.tbYear.YearNumber
go
PRINT N'Creating View [App].[vwYears]...';


go
CREATE   VIEW App.vwYears
AS
	SELECT App.tbYear.YearNumber, CONCAT(App.tbMonth.MonthName, ' ', App.tbYear.YearNumber) StartMonth, App.tbYear.CashStatusCode, Cash.tbStatus.CashStatus, App.tbYear.Description, App.tbYear.InsertedBy, App.tbYear.InsertedOn
	FROM App.tbYear 
		JOIN Cash.tbStatus ON App.tbYear.CashStatusCode = Cash.tbStatus.CashStatusCode 
		JOIN App.tbMonth ON App.tbYear.StartMonth = App.tbMonth.MonthNumber AND App.tbYear.StartMonth = App.tbMonth.MonthNumber;
go
PRINT N'Creating View [App].[vwYearPeriod]...';


go
CREATE VIEW App.vwYearPeriod
AS
	SELECT App.tbYear.Description, App.tbMonth.MonthName, App.tbYearPeriod.CashStatusCode, Cash.tbStatus.CashStatus, App.tbYearPeriod.YearNumber, App.tbYearPeriod.MonthNumber, App.tbYearPeriod.StartOn, App.tbYearPeriod.RowVer
	FROM App.tbYearPeriod INNER JOIN
		App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
		App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
		Cash.tbStatus ON App.tbYearPeriod.CashStatusCode = Cash.tbStatus.CashStatusCode;
go
PRINT N'Creating View [App].[vwTaxCodeTypes]...';


go

CREATE   VIEW App.vwTaxCodeTypes
AS
SELECT        TaxTypeCode, TaxType
FROM            Cash.tbTaxType
WHERE        (TaxTypeCode > 0);
go
PRINT N'Creating View [App].[vwTaxCodes]...';


go
CREATE VIEW App.vwTaxCodes
AS
	SELECT        App.tbTaxCode.TaxCode, App.tbTaxCode.TaxDescription, Cash.tbTaxType.TaxType, App.tbTaxCode.TaxTypeCode, App.tbTaxCode.RoundingCode, App.tbRounding.Rounding, App.tbTaxCode.TaxRate, App.tbTaxCode.Decimals, 
							 App.tbTaxCode.UpdatedBy, App.tbTaxCode.UpdatedOn
	FROM            App.tbTaxCode INNER JOIN
							 Cash.tbTaxType ON App.tbTaxCode.TaxTypeCode = Cash.tbTaxType.TaxTypeCode INNER JOIN
							 App.tbRounding ON App.tbTaxCode.RoundingCode = App.tbRounding.RoundingCode
go
PRINT N'Creating View [App].[vwPeriodEndListing]...';


go

CREATE   VIEW App.vwPeriodEndListing
AS
SELECT        TOP (100) PERCENT App.tbYear.YearNumber, App.tbYear.Description, App.tbYear.InsertedBy AS YearInsertedBy, App.tbYear.InsertedOn AS YearInsertedOn, App.tbYearPeriod.StartOn, App.tbMonth.MonthName, 
                         App.tbYearPeriod.InsertedBy AS PeriodInsertedBy, App.tbYearPeriod.InsertedOn AS PeriodInsertedOn, Cash.tbStatus.CashStatus
FROM            Cash.tbStatus INNER JOIN
                         App.tbYear INNER JOIN
                         App.tbYearPeriod ON App.tbYear.YearNumber = App.tbYearPeriod.YearNumber INNER JOIN
                         App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber ON Cash.tbStatus.CashStatusCode = App.tbYearPeriod.CashStatusCode
ORDER BY App.tbYearPeriod.StartOn;
go
PRINT N'Creating View [App].[vwGraphProjectObject]...';


go
CREATE VIEW App.vwGraphProjectObject
AS
SELECT        CONCAT(Project.tbStatus.ProjectStatus, SPACE(1), Cash.tbPolarity.CashPolarity) AS Category, SUM(Project.tbProject.TotalCharge) AS SumOfTotalCharge
FROM            Project.tbProject INNER JOIN
                         Project.tbStatus ON Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode INNER JOIN
                         Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
                         Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
                         Cash.tbPolarity ON Cash.tbCategory.CashPolarityCode = Cash.tbPolarity.CashPolarityCode
WHERE        (Project.tbProject.ProjectStatusCode < 3) AND (Project.tbProject.ProjectStatusCode > 0)
GROUP BY CONCAT(Project.tbStatus.ProjectStatus, SPACE(1), Cash.tbPolarity.CashPolarity);
go
PRINT N'Creating View [App].[vwEventLog]...';


go

CREATE   VIEW App.vwEventLog
AS
	SELECT        App.tbEventLog.LogCode, App.tbEventLog.LoggedOn, App.tbEventLog.EventTypeCode, App.tbEventType.EventType, App.tbEventLog.EventMessage, App.tbEventLog.InsertedBy, App.tbEventLog.RowVer
	FROM            App.tbEventLog INNER JOIN
							 App.tbEventType ON App.tbEventLog.EventTypeCode = App.tbEventType.EventTypeCode
go
PRINT N'Creating View [App].[vwDocSpool]...';


go

CREATE   VIEW App.vwDocSpool
 AS
SELECT     DocTypeCode, DocumentNumber
FROM         App.tbDocSpool
WHERE     (UserName = SUSER_SNAME())
go
PRINT N'Creating View [App].[vwDocOpenModes]...';


go


CREATE   VIEW App.vwDocOpenModes
AS
SELECT TOP 100 PERCENT OpenMode, OpenModeDescription
FROM            Usr.tbMenuOpenMode
WHERE        (OpenMode > 1)
ORDER BY OpenMode;
go
PRINT N'Creating View [App].[vwHost]...';


go
CREATE   VIEW App.vwHost
AS
	SELECT App.tbHost.HostId, App.tbHost.HostDescription, App.tbHost.EmailAddress, App.tbHost.EmailPassword, App.tbHost.HostName, App.tbHost.HostPort
	FROM App.tbOptions 
		JOIN App.tbHost ON App.tbOptions.HostId = App.tbHost.HostId;
go
PRINT N'Creating View [App].[vwVersion]...';


go
CREATE VIEW App.vwVersion
AS
	SELECT CONCAT(ROUND(SQLDataVersion, 3), '.', SQLRelease) AS VersionString, ROUND(SQLDataVersion, 3) SQLDataVersion, SQLRelease
	FROM App.tbInstall
	WHERE InstallId = (SELECT MAX(InstallId) FROM App.tbInstall)
go
PRINT N'Creating View [App].[vwWarehouseProject]...';


go

CREATE   VIEW App.vwWarehouseProject
AS
SELECT TOP (100) PERCENT Project.tbDoc.ProjectCode, Project.tbDoc.DocumentName, Subject.tbSubject.SubjectName, Project.tbProject.ProjectTitle, Project.tbDoc.DocumentImage, Project.tbDoc.DocumentDescription, Project.tbDoc.InsertedBy, Project.tbDoc.InsertedOn, 
                         Project.tbDoc.UpdatedBy, Project.tbDoc.UpdatedOn, Project.tbDoc.RowVer
FROM            Subject.tbSubject INNER JOIN
                         Project.tbProject ON Subject.tbSubject.SubjectCode = Project.tbProject.SubjectCode INNER JOIN
                         Project.tbDoc ON Project.tbProject.ProjectCode = Project.tbDoc.ProjectCode
ORDER BY Project.tbDoc.ProjectCode, Project.tbDoc.DocumentName;
go
PRINT N'Creating View [App].[vwWarehouseSubject]...';


go

CREATE   VIEW App.vwWarehouseSubject
AS
SELECT TOP (100) PERCENT Subject.tbSubject.SubjectCode, Subject.tbDoc.DocumentName, Subject.tbSubject.SubjectName, Subject.tbDoc.DocumentImage, Subject.tbDoc.DocumentDescription, Subject.tbDoc.InsertedBy, Subject.tbDoc.InsertedOn, Subject.tbDoc.UpdatedBy, 
                         Subject.tbDoc.UpdatedOn, Subject.tbDoc.RowVer
FROM            Subject.tbSubject INNER JOIN
                         Subject.tbDoc ON Subject.tbSubject.SubjectCode = Subject.tbDoc.SubjectCode
ORDER BY Subject.tbDoc.SubjectCode, Subject.tbDoc.DocumentName;
go
PRINT N'Creating View [App].[vwHomeAccount]...';


go

CREATE   VIEW App.vwHomeAccount
AS
	SELECT     Subject.tbSubject.SubjectCode, Subject.tbSubject.SubjectName
	FROM            App.tbOptions INNER JOIN
							 Subject.tbSubject ON App.tbOptions.SubjectCode = Subject.tbSubject.SubjectCode
go
PRINT N'Creating View [App].[vwCandidateHomeAccounts]...';


go

CREATE   VIEW App.vwCandidateHomeAccounts
AS
SELECT        Subject.tbSubject.SubjectCode, Subject.tbSubject.SubjectName, Subject.tbType.SubjectType, Cash.tbPolarity.CashPolarity
FROM            Subject.tbSubject INNER JOIN
                         Subject.tbType ON Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode INNER JOIN
                         Cash.tbPolarity ON Subject.tbType.CashPolarityCode = Cash.tbPolarity.CashPolarityCode
WHERE        (Subject.tbSubject.SubjectStatusCode < 3);
go
PRINT N'Creating View [App].[vwDocDebitNote]...';


go
CREATE VIEW App.vwDocDebitNote
AS
SELECT        TOP (100) PERCENT Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.Printed, Invoice.tbInvoice.Spooled, Invoice.tbInvoice.InvoiceStatusCode, Usr.tbUser.UserName, Invoice.tbInvoice.SubjectCode, 
                         Subject.tbSubject.SubjectName, Invoice.tbStatus.InvoiceStatus, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.InvoiceValue, Invoice.tbInvoice.TaxValue, Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Notes, 
                         Subject.tbSubject.EmailAddress, Invoice.tbInvoice.RowVer
FROM            Invoice.tbInvoice INNER JOIN
                         Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
                         Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
                         Usr.tbUser ON Invoice.tbInvoice.UserId = Usr.tbUser.UserId
WHERE        (Invoice.tbInvoice.InvoiceTypeCode = 3);
go
PRINT N'Creating View [App].[vwDocCreditNote]...';


go

CREATE VIEW App.vwDocCreditNote
AS
SELECT        TOP (100) PERCENT Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.Printed, Invoice.tbInvoice.Spooled, Invoice.tbInvoice.InvoiceStatusCode, Usr.tbUser.UserName, Invoice.tbInvoice.SubjectCode, 
                         Subject.tbSubject.SubjectName, Invoice.tbStatus.InvoiceStatus, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.InvoiceValue, Invoice.tbInvoice.TaxValue, Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Notes, 
                         Subject.tbSubject.EmailAddress, Invoice.tbInvoice.RowVer
FROM            Invoice.tbInvoice INNER JOIN
                         Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
                         Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
                         Usr.tbUser ON Invoice.tbInvoice.UserId = Usr.tbUser.UserId
WHERE        (Invoice.tbInvoice.InvoiceTypeCode = 1);
go
PRINT N'Creating View [App].[vwIdentity]...';


go
CREATE VIEW App.vwIdentity
AS
	SELECT TOP (1) Subject.tbSubject.SubjectName, Subject.tbAddress.Address, Subject.tbSubject.PhoneNumber, Subject.tbSubject.EmailAddress, Subject.tbSubject.WebSite, Subject.tbSubject.Logo, Usr.tbUser.UserName, Usr.tbUser.LogonName, Usr.tbUser.Avatar, 
							 Subject.tbSubject.CompanyNumber, Subject.tbSubject.VatNumber, App.tbUoc.UocName, App.tbUoc.UocSymbol
	FROM  Subject.tbSubject INNER JOIN
		App.tbOptions ON Subject.tbSubject.SubjectCode = App.tbOptions.SubjectCode INNER JOIN
		App.tbUoc ON App.tbOptions.UnitOfCharge = App.tbUoc.UnitOfCharge LEFT OUTER JOIN
		Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode CROSS JOIN
		Usr.vwCredentials INNER JOIN
		Usr.tbUser ON Usr.vwCredentials.UserId = Usr.tbUser.UserId
go
PRINT N'Creating View [App].[vwDocSalesInvoice]...';


go
CREATE VIEW App.vwDocSalesInvoice
AS
SELECT        TOP (100) PERCENT Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.Printed, Invoice.tbInvoice.Spooled, Invoice.tbInvoice.InvoiceStatusCode, Usr.tbUser.UserName, Invoice.tbInvoice.SubjectCode, 
                         Subject.tbSubject.SubjectName, Invoice.tbStatus.InvoiceStatus, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.InvoiceValue, Invoice.tbInvoice.TaxValue, Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Notes, 
                         Subject.tbSubject.EmailAddress, Invoice.tbInvoice.RowVer
FROM            Invoice.tbInvoice INNER JOIN
                         Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
                         Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
                         Usr.tbUser ON Invoice.tbInvoice.UserId = Usr.tbUser.UserId
WHERE        (Invoice.tbInvoice.InvoiceTypeCode = 0);
go
PRINT N'Creating View [Object].[vwDefaultText]...';


go

CREATE   VIEW Object.vwDefaultText
AS
SELECT TOP 100 PERCENT  DefaultText
FROM            Object.tbAttribute
GROUP BY DefaultText
HAVING        (DefaultText IS NOT NULL)
ORDER BY DefaultText;
go
PRINT N'Creating View [Object].[vwCodes]...';


go

CREATE   VIEW Object.vwCodes
AS
SELECT        Object.tbObject.ObjectCode, Object.tbObject.UnitOfMeasure, Object.tbObject.CashCode
FROM            Object.tbObject LEFT OUTER JOIN
                         Cash.tbCode ON Object.tbObject.CashCode = Cash.tbCode.CashCode;
go
PRINT N'Creating View [Object].[vwCandidateCashCodes]...';


go
CREATE VIEW Object.vwCandidateCashCodes
AS
	SELECT Cash.tbCode.CashCode, Cash.tbCode.CashDescription, Cash.tbCategory.Category, Cash.tbCategory.CashPolarityCode, Cash.tbCategory.CashTypeCode
	FROM            Cash.tbCode INNER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
	WHERE        (Cash.tbCategory.CashTypeCode < 2)  AND (Cash.tbCategory.IsEnabled <> 0) AND (Cash.tbCode.IsEnabled <> 0)
go
PRINT N'Creating View [Object].[vwIncomeCashCodes]...';


go
CREATE   VIEW Object.vwIncomeCashCodes
AS
	SELECT CashCode, CashDescription, Category
	FROM Object.vwCandidateCashCodes
	WHERE CashPolarityCode = 1 AND CashTypeCode = 0
go
PRINT N'Creating View [Object].[vwExpenseCashCodes]...';


go
CREATE   VIEW Object.vwExpenseCashCodes
AS
	SELECT CashCode, CashDescription, Category
	FROM Object.vwCandidateCashCodes
	WHERE CashPolarityCode = 0 AND CashTypeCode = 0
go
PRINT N'Creating View [Object].[vwNetworkMirrors]...';


go
CREATE   VIEW Object.vwNetworkMirrors
AS
	SELECT SubjectCode, ObjectCode, AllocationCode, TransmitStatusCode FROM Object.tbMirror WHERE TransmitStatusCode BETWEEN 1 AND 2;
go
PRINT N'Creating View [Object].[vwUnMirrored]...';


go
CREATE VIEW Object.vwUnMirrored
AS
	WITH candidates AS
	(
		SELECT DISTINCT Project.tbAllocation.SubjectCode, Subject.tbSubject.SubjectName, Project.tbAllocation.AllocationCode, Project.tbAllocation.AllocationDescription, Project.tbAllocation.CashPolarityCode, Cash.tbPolarity.CashPolarity, Project.tbAllocation.UnitCharge, Project.tbAllocation.UnitOfMeasure
		FROM            Project.tbAllocation 
			INNER JOIN Cash.tbPolarity ON Project.tbAllocation.CashPolarityCode = Cash.tbPolarity.CashPolarityCode 
			INNER JOIN Subject.tbSubject ON Project.tbAllocation.SubjectCode = Subject.tbSubject.SubjectCode 
			LEFT OUTER JOIN Object.tbMirror ON Project.tbAllocation.SubjectCode = Object.tbMirror.SubjectCode AND Project.tbAllocation.AllocationCode = Object.tbMirror.AllocationCode
		WHERE        (Object.tbMirror.ObjectCode IS NULL)
	)
	SELECT CAST(ROW_NUMBER() OVER (ORDER BY SubjectCode, AllocationCode) AS int) CandidateId,
		candidates.SubjectCode, candidates.SubjectName, candidates.AllocationCode, candidates.AllocationDescription, candidates.CashPolarityCode, candidates.CashPolarity, candidates.UnitCharge, candidates.UnitOfMeasure,
		CASE WHEN act_code.ObjectCode IS NULL THEN 0 ELSE 1 END IsObject
	FROM candidates LEFT OUTER JOIN Object.tbObject act_code ON candidates.AllocationCode = act_code.ObjectCode;
go
PRINT N'Creating View [Subject].[vwDepartments]...';


go

CREATE   VIEW Subject.vwDepartments
AS
SELECT        Department
FROM            Subject.tbContact
GROUP BY Department
HAVING        (Department IS NOT NULL);
go
PRINT N'Creating View [Subject].[vwAreaCodes]...';


go

CREATE   VIEW Subject.vwAreaCodes
AS
SELECT        AreaCode
FROM            Subject.tbSubject
GROUP BY AreaCode
HAVING        (AreaCode IS NOT NULL);
go
PRINT N'Creating View [Subject].[vwTypeLookup]...';


go

CREATE   VIEW Subject.vwTypeLookup
AS
SELECT        Subject.tbType.SubjectTypeCode, Subject.tbType.SubjectType, Cash.tbPolarity.CashPolarity
FROM            Subject.tbType INNER JOIN
                         Cash.tbPolarity ON Subject.tbType.CashPolarityCode = Cash.tbPolarity.CashPolarityCode;
go
PRINT N'Creating View [Subject].[vwPaymentTerms]...';


go


CREATE   VIEW Subject.vwPaymentTerms
AS
SELECT        PaymentTerms
FROM            Subject.tbSubject
GROUP BY PaymentTerms
HAVING         LEN(ISNULL(PaymentTerms, '')) > 0;
go
PRINT N'Creating View [Subject].[vwNameTitles]...';


go

CREATE   VIEW Subject.vwNameTitles
AS
SELECT        NameTitle
FROM            Subject.tbContact
GROUP BY NameTitle
HAVING        (NameTitle IS NOT NULL);
go
PRINT N'Creating View [Subject].[vwJobTitles]...';


go

CREATE   VIEW Subject.vwJobTitles
AS
SELECT        JobTitle
FROM            Subject.tbContact
GROUP BY JobTitle
HAVING        (JobTitle IS NOT NULL);
go
PRINT N'Creating View [Subject].[vwBalanceOutstanding]...';


go
CREATE VIEW Subject.vwBalanceOutstanding
AS
	WITH invoices_unpaid AS
	(
		SELECT        Invoice.tbInvoice.SubjectCode, 
			CASE Invoice.tbType.CashPolarityCode 
				WHEN 0 THEN ((InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue)) * - 1 
				WHEN 1 THEN (InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue) END AS OutstandingValue
		FROM            Invoice.tbInvoice INNER JOIN
								 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
		WHERE        (Invoice.tbInvoice.InvoiceStatusCode > 0) AND (Invoice.tbInvoice.InvoiceStatusCode < 3) 
	), current_balance AS
	(
		SELECT SubjectCode, SUM(OutstandingValue) AS Balance
		FROM   invoices_unpaid	
		GROUP BY SubjectCode
	)
	SELECT Subject.SubjectCode, ISNULL(current_balance.Balance, 0) AS Balance
	FROM Subject.tbSubject Subject 
		LEFT OUTER JOIN current_balance ON Subject.SubjectCode = current_balance.SubjectCode;
go
PRINT N'Creating View [Subject].[vwCompanyLogo]...';


go

CREATE   VIEW Subject.vwCompanyLogo
AS
SELECT        TOP (1) Subject.tbSubject.Logo
FROM            Subject.tbSubject INNER JOIN
                         App.tbOptions ON Subject.tbSubject.SubjectCode = App.tbOptions.SubjectCode;
go
PRINT N'Creating View [Subject].[vwInvoiceSummary]...';


go
CREATE VIEW Subject.vwInvoiceSummary
AS
	WITH ois AS
	(
		SELECT        SubjectCode, StartOn, SUM(InvoiceValue) AS PeriodValue
		FROM            Invoice.vwRegister
		GROUP BY SubjectCode, StartOn
	), acc AS
	(
		SELECT Subject.tbSubject.SubjectCode, App.vwPeriods.StartOn
		FROM Subject.tbSubject CROSS JOIN App.vwPeriods
	)
	SELECT TOP (100) PERCENT acc.SubjectCode, acc.StartOn, ois.PeriodValue 
	FROM ois RIGHT OUTER JOIN acc ON ois.SubjectCode = acc.SubjectCode AND ois.StartOn = acc.StartOn
	ORDER BY acc.SubjectCode, acc.StartOn;
go
PRINT N'Creating View [Subject].[vwMailContacts]...';


go

CREATE   VIEW Subject.vwMailContacts
  AS
SELECT     SubjectCode, ContactName, NickName, NameTitle + N' ' + ContactName AS FormalName, JobTitle, Department
FROM         Subject.tbContact
WHERE     (OnMailingList <> 0)
go
PRINT N'Creating View [Subject].[vwAddressList]...';


go
CREATE   VIEW Subject.vwAddressList
AS
	SELECT        Subject.tbSubject.SubjectCode, Subject.tbAddress.AddressCode, Subject.tbSubject.SubjectName, Subject.tbStatus.SubjectStatusCode, Subject.tbStatus.SubjectStatus, Subject.tbType.SubjectTypeCode, Subject.tbType.SubjectType, 
							 Subject.tbAddress.Address, Subject.tbAddress.InsertedBy, Subject.tbAddress.InsertedOn, CAST(CASE WHEN Subject.tbAddress.AddressCode = Subject.tbSubject.AddressCode THEN 1 ELSE 0 END AS bit) AS IsAdminAddress
	FROM            Subject.tbSubject INNER JOIN
							 Subject.tbAddress ON Subject.tbSubject.SubjectCode = Subject.tbAddress.SubjectCode INNER JOIN
							 Subject.tbStatus ON Subject.tbSubject.SubjectStatusCode = Subject.tbStatus.SubjectStatusCode INNER JOIN
							 Subject.tbType ON Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode
go
PRINT N'Creating View [Subject].[vwAddresses]...';


go

CREATE   VIEW Subject.vwAddresses
  AS
SELECT     TOP 100 PERCENT Subject.tbSubject.SubjectName, Subject.tbAddress.Address, Subject.tbSubject.SubjectTypeCode, Subject.tbSubject.SubjectStatusCode, 
                      Subject.tbType.SubjectType, Subject.tbStatus.SubjectStatus, Subject.vwMailContacts.ContactName, Subject.vwMailContacts.NickName, 
                      Subject.vwMailContacts.FormalName, Subject.vwMailContacts.JobTitle, Subject.vwMailContacts.Department
FROM         Subject.tbSubject INNER JOIN
                      Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode INNER JOIN
                      Subject.tbType ON Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode INNER JOIN
                      Subject.tbStatus ON Subject.tbSubject.SubjectStatusCode = Subject.tbStatus.SubjectStatusCode LEFT OUTER JOIN
                      Subject.vwMailContacts ON Subject.tbSubject.SubjectCode = Subject.vwMailContacts.SubjectCode
ORDER BY Subject.tbSubject.SubjectName
go
PRINT N'Creating View [Subject].[vwEmailAddresses]...';


go
CREATE   VIEW Subject.vwEmailAddresses
AS
	SELECT SubjectCode, SubjectName ContactName, EmailAddress, CAST(1 as bit) IsAdmin
	FROM Subject.tbSubject
	WHERE (NOT (EmailAddress IS NULL))
	UNION
	SELECT SubjectCode, ContactName, EmailAddress, CAST(0 as bit) IsAdmin
	FROM            Subject.tbContact
	WHERE        (NOT (EmailAddress IS NULL))
go
PRINT N'Creating View [Subject].[vwCompanyHeader]...';


go
CREATE   VIEW Subject.vwCompanyHeader
AS
SELECT        TOP (1) Subject.tbSubject.SubjectName AS CompanyName, Subject.tbAddress.Address AS CompanyAddress, Subject.tbSubject.PhoneNumber AS CompanyPhoneNumber, 
                         Subject.tbSubject.EmailAddress AS CompanyEmailAddress, Subject.tbSubject.WebSite AS CompanyWebsite, Subject.tbSubject.CompanyNumber, Subject.tbSubject.VatNumber
FROM            Subject.tbSubject INNER JOIN
                         App.tbOptions ON Subject.tbSubject.SubjectCode = App.tbOptions.SubjectCode LEFT OUTER JOIN
                         Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode;
go
PRINT N'Creating View [Subject].[vwListActive]...';


go

CREATE   VIEW Subject.vwListActive
AS
	SELECT        TOP (100) PERCENT Subject.tbSubject.SubjectCode, Subject.tbSubject.SubjectName, Subject.tbType.CashPolarityCode
	FROM            Subject.tbSubject INNER JOIN
							 Project.tbProject ON Subject.tbSubject.SubjectCode = Project.tbProject.SubjectCode INNER JOIN
							 Subject.tbType ON Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode
	WHERE        (Project.tbProject.ProjectStatusCode = 1 OR
							 Project.tbProject.ProjectStatusCode = 2) AND (Project.tbProject.CashCode IS NOT NULL)
	GROUP BY Subject.tbSubject.SubjectCode, Subject.tbSubject.SubjectName, Subject.tbType.CashPolarityCode
	ORDER BY Subject.tbSubject.SubjectName;
go
PRINT N'Creating View [Subject].[vwContacts]...';


go
CREATE VIEW Subject.vwContacts
AS
	WITH ContactCount AS 
	(
		SELECT ContactName, COUNT(ProjectCode) AS Projects
        FROM Project.tbProject
        WHERE (ProjectStatusCode < 2)
        GROUP BY ContactName
        HAVING (ContactName IS NOT NULL)
	)
    SELECT Subject.tbContact.ContactName, Subject.tbSubject.SubjectCode, COALESCE(ContactCount.Projects, 0) Projects, Subject.tbContact.PhoneNumber, Subject.tbContact.HomeNumber, Subject.tbContact.MobileNumber,  
                              Subject.tbContact.EmailAddress, Subject.tbSubject.SubjectName, Subject.tbType.SubjectType, Subject.tbStatus.SubjectStatus, Subject.tbContact.NameTitle, Subject.tbContact.NickName, Subject.tbContact.JobTitle, 
                              Subject.tbContact.Department, Subject.tbContact.Information, Subject.tbContact.InsertedBy, Subject.tbContact.InsertedOn
     FROM            Subject.tbSubject INNER JOIN
                              Subject.tbType ON Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode INNER JOIN
                              Subject.tbStatus ON Subject.tbSubject.SubjectStatusCode = Subject.tbStatus.SubjectStatusCode INNER JOIN
                              Subject.tbContact ON Subject.tbSubject.SubjectCode = Subject.tbContact.SubjectCode LEFT OUTER JOIN
                              ContactCount ON Subject.tbContact.ContactName = ContactCount.ContactName
     WHERE        (Subject.tbSubject.SubjectStatusCode < 3);
go
PRINT N'Creating View [Subject].[vwInvoiceItems]...';


go
CREATE VIEW Subject.vwInvoiceItems
AS
SELECT        Invoice.tbInvoice.SubjectCode, Invoice.tbItem.InvoiceNumber, Invoice.tbItem.CashCode, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbStatus.InvoiceStatus, 
                         Cash.tbCode.CashDescription, Subject.tbSubject.SubjectName, Invoice.tbInvoice.InvoiceStatusCode, Invoice.tbType.InvoiceType, Invoice.tbItem.TaxCode, Invoice.tbItem.TaxValue, 
                         Invoice.tbItem.InvoiceValue, Invoice.tbInvoice.PaidValue, Invoice.tbInvoice.PaidTaxValue, Invoice.tbItem.ItemReference
FROM            Invoice.tbInvoice INNER JOIN
                         Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
                         Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
                         Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
                         Invoice.tbItem ON Invoice.tbInvoice.InvoiceNumber = Invoice.tbItem.InvoiceNumber INNER JOIN
                         Cash.tbCode ON Invoice.tbItem.CashCode = Cash.tbCode.CashCode
WHERE        (Invoice.tbInvoice.InvoiceStatusCode > 0);
go
PRINT N'Creating View [Subject].[vwListAll]...';


go
CREATE   VIEW Subject.vwListAll
AS
	WITH accounts AS
	(
		SELECT SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode, TaxCode,
			(SELECT TOP 1 CashCode FROM Project.tbProject WHERE SubjectCode = Subjects.SubjectCode ORDER BY ActionOn DESC) ProjectCashCode,
			(SELECT TOP 1 CashCode FROM Cash.tbPayment WHERE SubjectCode = Subjects.SubjectCode ORDER BY PaidOn DESC) PaymentCashCode
		FROM  Subject.tbSubject Subjects
	)
		SELECT accounts.SubjectCode, accounts.SubjectName, Subject_type.SubjectType, accounts.TaxCode, Subject_type.CashPolarityCode, accounts.SubjectStatusCode,
			COALESCE(accounts.ProjectCashCode, accounts.PaymentCashCode) CashCode
		FROM accounts 
			INNER JOIN Subject.tbType AS Subject_type ON accounts.SubjectTypeCode = Subject_type.SubjectTypeCode
go
PRINT N'Creating View [Subject].[vwInvoiceProjects]...';


go
CREATE   VIEW Subject.vwInvoiceProjects
AS
	SELECT        Invoice.tbInvoice.SubjectCode, tbInvoiceProject.InvoiceNumber, tbInvoiceProject.ProjectCode, Project.tbProject.ContactName, Invoice.tbInvoice.InvoicedOn, tbInvoiceProject.Quantity, tbInvoiceProject.InvoiceValue, tbInvoiceProject.TaxValue, 
							 tbInvoiceProject.CashCode, tbInvoiceProject.TaxCode, Invoice.tbStatus.InvoiceStatus, Project.tbProject.ProjectNotes, Cash.tbCode.CashDescription, Invoice.tbInvoice.InvoiceStatusCode, Project.tbProject.ProjectTitle, Subject.tbSubject.SubjectName, 
							 Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbType.InvoiceType, Invoice.tbType.CashPolarityCode, Invoice.tbInvoice.PaidTaxValue, Invoice.tbInvoice.PaidValue
	FROM            Invoice.tbInvoice INNER JOIN
							 Invoice.tbProject AS tbInvoiceProject ON Invoice.tbInvoice.InvoiceNumber = tbInvoiceProject.InvoiceNumber INNER JOIN
							 Project.tbProject ON tbInvoiceProject.ProjectCode = Project.tbProject.ProjectCode INNER JOIN
							 Cash.tbCode ON tbInvoiceProject.CashCode = Cash.tbCode.CashCode INNER JOIN
							 Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							 Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
	WHERE        (Invoice.tbInvoice.InvoiceStatusCode > 0);
go
PRINT N'Creating View [Subject].[vwSubjectLookupAll]...';


go
CREATE   VIEW Subject.vwSubjectLookupAll
AS
	SELECT Subject.tbSubject.SubjectCode, Subject.tbSubject.SubjectName, Subject.tbSubject.SubjectTypeCode, Subject.tbType.SubjectType, Cash.tbPolarity.CashPolarity, Cash.tbPolarity.CashPolarityCode, Subject.tbSubject.SubjectStatusCode, Subject.tbStatus.SubjectStatus
	FROM Subject.tbSubject 
		JOIN Subject.tbType ON Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode
		JOIN Cash.tbPolarity ON Subject.tbType.CashPolarityCode = Cash.tbPolarity.CashPolarityCode 
		JOIN Subject.tbStatus ON Subject.tbSubject.SubjectStatusCode = Subject.tbStatus.SubjectStatusCode;
go
PRINT N'Creating View [Subject].[vwSubjectLookup]...';


go
CREATE VIEW Subject.vwSubjectLookup
AS
SELECT        Subject.tbSubject.SubjectCode, Subject.tbSubject.SubjectName, Subject.tbType.SubjectType, Cash.tbPolarity.CashPolarity, Cash.tbPolarity.CashPolarityCode
FROM            Subject.tbSubject INNER JOIN
                         Subject.tbType ON Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode INNER JOIN
                         Cash.tbPolarity ON Subject.tbType.CashPolarityCode = Cash.tbPolarity.CashPolarityCode
WHERE        (Subject.tbSubject.SubjectStatusCode < 3);
go
PRINT N'Creating View [Subject].[vwDatasheet]...';


go
CREATE VIEW Subject.vwDatasheet
AS
	With Project_count AS
	(
		SELECT        SubjectCode, COUNT(ProjectCode) AS ProjectCount
		FROM            Project.tbProject
		WHERE        (ProjectStatusCode = 1)
		GROUP BY SubjectCode
	)
	SELECT        o.SubjectCode, o.SubjectName, ISNULL(Project_count.ProjectCount, 0) AS Projects, o.SubjectTypeCode, Subject.tbType.SubjectType, Subject.tbType.CashPolarityCode, o.SubjectStatusCode, 
							 Subject.tbStatus.SubjectStatus, Subject.tbTransmitStatus.TransmitStatus, Subject.tbAddress.Address, App.tbTaxCode.TaxDescription, o.TaxCode, o.AddressCode, o.AreaCode, o.PhoneNumber, o.EmailAddress, o.WebSite,
								 (SELECT        TOP (1) IndustrySector
								   FROM            Subject.tbSector AS sector
								   WHERE        (SubjectCode = o.SubjectCode)) AS IndustrySector, o.SubjectSource, o.PaymentTerms, o.PaymentDays, o.ExpectedDays, o.PayDaysFromMonthEnd, o.PayBalance, o.NumberOfEmployees, o.CompanyNumber, o.VatNumber, o.Turnover, 
							 o.OpeningBalance, o.EUJurisdiction, o.BusinessDescription, o.InsertedBy, o.InsertedOn, o.UpdatedBy, o.UpdatedOn 
	FROM            Subject.tbSubject AS o 
		JOIN Subject.tbStatus ON o.SubjectStatusCode = Subject.tbStatus.SubjectStatusCode 
		JOIN Subject.tbType ON o.SubjectTypeCode = Subject.tbType.SubjectTypeCode 
		JOIN Subject.tbTransmitStatus ON o.TransmitStatusCode = Subject.tbTransmitStatus.TransmitStatusCode
		LEFT OUTER JOIN App.tbTaxCode ON o.TaxCode = App.tbTaxCode.TaxCode 
		LEFT OUTER JOIN Subject.tbAddress ON o.AddressCode = Subject.tbAddress.AddressCode 
		LEFT OUTER JOIN Project_count ON o.SubjectCode = Project_count.SubjectCode
go
PRINT N'Creating View [Subject].[vwSubjectSources]...';


go

CREATE   VIEW Subject.vwSubjectSources
AS
SELECT        SubjectSource
FROM            Subject.tbSubject
GROUP BY SubjectSource
HAVING        (SubjectSource IS NOT NULL);
go
PRINT N'Creating View [Subject].[vwStatement]...';


go
CREATE VIEW Subject.vwStatement 
AS
	WITH payment_data AS
	(
		SELECT Cash.tbPayment.SubjectCode, Cash.tbPayment.PaidOn AS TransactedOn, 2 AS OrderBy, 
						CASE WHEN LEN(COALESCE(Cash.tbPayment.PaymentReference, '')) = 0 THEN Cash.tbPayment.PaymentCode ELSE Cash.tbPayment.PaymentReference END AS Reference, 
						Cash.tbPaymentStatus.PaymentStatus AS StatementType, 
						CASE WHEN PaidInValue > 0 THEN PaidInValue ELSE PaidOutValue * - 1 END AS Charge
		FROM Cash.tbPayment 
			JOIN Subject.tbAccount ON Cash.tbPayment.AccountCode = Subject.tbAccount.AccountCode
			JOIN Cash.tbPaymentStatus ON Cash.tbPayment.PaymentStatusCode = Cash.tbPaymentStatus.PaymentStatusCode
		WHERE Subject.tbAccount.AccountTypeCode < 2 AND Cash.tbPayment.PaymentStatusCode = 1
	), payments AS
	(
		SELECT     SubjectCode, TransactedOn, OrderBy, Reference, StatementType, SUM(Charge) AS Charge
		FROM     payment_data
		GROUP BY SubjectCode, TransactedOn, OrderBy, Reference, StatementType
	), invoices AS
	(
		SELECT Invoice.tbInvoice.SubjectCode, Invoice.tbInvoice.InvoicedOn AS TransactedOn, 1 AS OrderBy, Invoice.tbInvoice.InvoiceNumber AS Reference, Invoice.tbType.InvoiceType AS StatementType, 
			CASE CashPolarityCode 
				WHEN 0 THEN Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue 
				WHEN 1 THEN (Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue) * - 1 
			END AS Charge
		FROM Invoice.tbInvoice 
			JOIN Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
	), transactions_union AS
	(
		SELECT     SubjectCode, TransactedOn, OrderBy, Reference, StatementType, Charge
		FROM         payments
		UNION ALL
		SELECT     SubjectCode, TransactedOn, OrderBy, Reference, StatementType, Charge
		FROM         invoices
	), transactions AS
	(
		SELECT SubjectCode, ROW_NUMBER() OVER (PARTITION BY SubjectCode ORDER BY TransactedOn, OrderBy, Reference) AS RowNumber, 
			TransactedOn, Reference, StatementType, Charge
		FROM transactions_union
	), opening_balance AS
	(
		SELECT SubjectCode, 0 AS RowNumber, InsertedOn AS TransactedOn, NULL AS Reference, 
			(SELECT CAST(Message AS NVARCHAR) FROM App.tbText WHERE TextId = 3005) AS StatementType, OpeningBalance AS Charge
		FROM Subject.tbSubject Subject
	), statement_data AS
	( 
		SELECT SubjectCode, RowNumber, TransactedOn, Reference, StatementType, Charge FROM transactions
		UNION
		SELECT SubjectCode, RowNumber, TransactedOn, Reference, StatementType, Charge FROM opening_balance
	)
	SELECT SubjectCode, CAST(RowNumber AS INT) AS RowNumber, 
		CASE RowNumber 
			WHEN 0 THEN 
				DATEADD(DAY, -1, COALESCE(LEAD(TransactedOn) OVER (PARTITION BY SubjectCode ORDER BY RowNumber), 0)) 
			ELSE 
				TransactedOn 
		END TransactedOn, 
		Reference, StatementType, CAST(Charge as float) AS Charge, 
		CAST(SUM(Charge) OVER (PARTITION BY SubjectCode ORDER BY RowNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS float) AS Balance
	FROM statement_data;
go
PRINT N'Creating View [Subject].[vwNamespace]...';


go
CREATE   VIEW Subject.vwNamespace
AS

	WITH ancestors AS
	(
		SELECT AccountCode, HDPath.GetAncestor(1) Ancestor, HDPath, KeyName
		FROM Subject.tbAccountKey
	), parent_child AS
	(
		SELECT nspace.AccountCode, nspace.HDPath parent, nspace.KeyName parentLoc, ancestors.HDPath child, ancestors.KeyName childLoc, ancestors.HDPath.GetLevel() KeyLevel
		FROM ancestors JOIN Subject.tbAccountKey nspace ON ancestors.AccountCode = nspace.AccountCode AND ancestors.Ancestor = nspace.HDPath
	), namespaced AS
	(
		SELECT AccountCode, cast(NULL AS hierarchyid) ParentHDPath, HDPath ChildHDPath, cast(KeyName AS nvarchar(1024)) KeyNamespace, HDPath.GetLevel() KeyLevel
		FROM Subject.tbAccountKey
		WHERE HDPath = (SELECT DISTINCT hierarchyid::GetRoot() r FROM Subject.tbAccountKey)

		UNION ALL

		SELECT parent_child.AccountCode, parent_child.parent ParentHDPath, parent_child.child ChildHDPath, cast(namespaced.KeyNamespace + '.' + parent_child.childLoc AS nvarchar(1024)) KeyNamespace, parent_child.KeyLevel
		FROM parent_child JOIN namespaced ON parent_child.AccountCode = namespaced.AccountCode AND parent_child.parent = namespaced.ChildHDPath
	)
	, hardened AS
	(
		SELECT namespaced.AccountCode, account.CoinTypeCode, namespaced.ChildHDPath HDPath, 
			REPLACE(namespaced.ParentHDPath.ToString(), '/', '''/') ParentHDPath, 
			REPLACE(namespaced.ChildHDPath.ToString(), '/', '''/') ChildHDPath, 
			KeyName, 
			REPLACE(UPPER(KeyNamespace), ' ', '_') KeyNamespace, 
			KeyLevel
		FROM namespaced
			JOIN Subject.tbAccount account ON namespaced.AccountCode = account.AccountCode
			JOIN  Subject.tbAccountKey ON namespaced.AccountCode = Subject.tbAccountKey.AccountCode 
				AND namespaced.ChildHDPath = Subject.tbAccountKey.HDPath
	)
	SELECT AccountCode,  -- HDPath, not supported VS
		CONCAT('44', '''', '/', CoinTypeCode, '''', CAST(RIGHT(ParentHDPath, LEN(ParentHDPath) - 1) AS nvarchar(50))) ParentHDPath, 
		CONCAT('44', '''', '/', CoinTypeCode, '''', CAST(RIGHT(ChildHDPath, LEN(ChildHDPath) - 1) AS nvarchar(50))) ChildHDPath, 
		KeyName,
		CAST(KeyNamespace AS nvarchar(1024)) KeyNamespace, KeyLevel, COALESCE(ReceiptIndex, 0) ReceiptIndex, COALESCE(ChangeIndex, 0) ChangeIndex 
	FROM hardened
		OUTER APPLY
		(
			SELECT COUNT(*) ReceiptIndex 
			FROM Cash.tbChange change
			WHERE change.AccountCode = hardened.AccountCode AND change.HDPath = hardened.HDPath AND ChangeTypeCode = 0
		) receipts
		OUTER APPLY
		(
			SELECT COUNT(*) ChangeIndex 
			FROM Cash.tbChange change
			WHERE change.AccountCode = hardened.AccountCode AND change.HDPath = hardened.HDPath AND ChangeTypeCode = 1
		) change;
go
PRINT N'Creating View [Subject].[vwWallets]...';


go
CREATE VIEW Subject.vwWallets
AS
	SELECT        Subject.tbAccount.AccountCode, Subject.tbAccount.AccountName, Subject.tbAccount.CashCode, Subject.tbAccount.CoinTypeCode
	FROM            Subject.tbAccount INNER JOIN
							 App.tbOptions ON Subject.tbAccount.SubjectCode = App.tbOptions.SubjectCode LEFT OUTER JOIN
							 Cash.tbCode ON Subject.tbAccount.CashCode = Cash.tbCode.CashCode
	WHERE        (Subject.tbAccount.AccountTypeCode = 0) AND Subject.tbAccount.CoinTypeCode < 2;
go
PRINT N'Creating View [Subject].[vwStatusReport]...';


go
CREATE VIEW Subject.vwStatusReport
AS
	SELECT        Subject.vwDatasheet.SubjectCode, Subject.vwDatasheet.SubjectName, Subject.vwDatasheet.SubjectType, Subject.vwDatasheet.SubjectStatus, Subject.vwDatasheet.TaxDescription, Subject.vwDatasheet.Address, 
							 Subject.vwDatasheet.AreaCode, Subject.vwDatasheet.PhoneNumber, Subject.vwDatasheet.EmailAddress, Subject.vwDatasheet.WebSite, Subject.vwDatasheet.IndustrySector, 
							 Subject.vwDatasheet.SubjectSource, Subject.vwDatasheet.PaymentTerms, Subject.vwDatasheet.PaymentDays, Subject.vwDatasheet.ExpectedDays, Subject.vwDatasheet.NumberOfEmployees, Subject.vwDatasheet.CompanyNumber, Subject.vwDatasheet.VatNumber, 
							 Subject.vwDatasheet.Turnover, Subject.vwDatasheet.OpeningBalance, Subject.vwDatasheet.EUJurisdiction, Subject.vwDatasheet.BusinessDescription, 
							 Cash.tbPayment.PaymentCode, Usr.tbUser.UserName, App.tbTaxCode.TaxDescription AS PaymentTaxDescription, Subject.tbAccount.AccountName, Cash.tbCode.CashDescription, Cash.tbPayment.UserId, 
							 Cash.tbPayment.AccountCode, Cash.tbPayment.CashCode, Cash.tbPayment.TaxCode, Cash.tbPayment.PaidOn, Cash.tbPayment.PaidInValue, Cash.tbPayment.PaidOutValue, 
							 Cash.tbPayment.InsertedBy, Cash.tbPayment.InsertedOn, Cash.tbPayment.UpdatedBy, Cash.tbPayment.UpdatedOn, Cash.tbPayment.PaymentReference
	FROM            Cash.tbPayment INNER JOIN
							 Usr.tbUser ON Cash.tbPayment.UserId = Usr.tbUser.UserId INNER JOIN
							 Subject.tbAccount ON Cash.tbPayment.AccountCode = Subject.tbAccount.AccountCode INNER JOIN
							 Cash.tbCode ON Cash.tbPayment.CashCode = Cash.tbCode.CashCode LEFT OUTER JOIN
							 App.tbTaxCode ON Cash.tbPayment.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
							 Subject.vwDatasheet ON Cash.tbPayment.SubjectCode = Subject.vwDatasheet.SubjectCode
	WHERE        (Cash.tbPayment.PaymentStatusCode = 1);
go
PRINT N'Creating View [Subject].[vwCashAccounts]...';


go
CREATE VIEW Subject.vwCashAccounts
AS
SELECT        Subject.tbAccount.AccountCode, Subject.tbSubject.SubjectCode, Subject.tbAccount.AccountName, Subject.tbSubject.SubjectName, Subject.tbType.SubjectType, Subject.tbAccount.OpeningBalance, Subject.tbAccount.CurrentBalance, 
                         Subject.tbAccount.SortCode, Subject.tbAccount.AccountNumber, Subject.tbAccount.AccountClosed, Subject.tbAccount.AccountTypeCode, Subject.tbAccountType.AccountType, Subject.tbAccount.CashCode, Cash.tbCode.CashDescription, 
                         Subject.tbAccount.InsertedBy, Subject.tbAccount.InsertedOn, Subject.tbAccount.LiquidityLevel
FROM            Subject.tbSubject INNER JOIN
                         Subject.tbAccount ON Subject.tbSubject.SubjectCode = Subject.tbAccount.SubjectCode INNER JOIN
                         Subject.tbType ON Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode INNER JOIN
                         Subject.tbAccountType ON Subject.tbAccount.AccountTypeCode = Subject.tbAccountType.AccountTypeCode LEFT OUTER JOIN
                         Cash.tbCode ON Subject.tbAccount.CashCode = Cash.tbCode.CashCode AND Subject.tbAccount.CashCode = Cash.tbCode.CashCode AND Subject.tbAccount.CashCode = Cash.tbCode.CashCode
go
PRINT N'Creating View [Subject].[vwCashAccountAssets]...';


go
CREATE VIEW Subject.vwCashAccountAssets
AS
	SELECT        Subject.tbAccount.AccountCode, Subject.tbAccount.LiquidityLevel, Subject.tbAccount.AccountName, Subject.tbAccount.SubjectCode, Cash.tbCode.CashCode, Cash.tbCode.TaxCode, Subject.tbAccount.AccountClosed
	FROM            Subject.tbAccount INNER JOIN
							 Cash.tbCode ON Subject.tbAccount.CashCode = Cash.tbCode.CashCode
	WHERE        (Subject.tbAccount.AccountTypeCode = 2);
go
PRINT N'Creating View [Project].[vwProfitToDate]...';


go

CREATE   VIEW Project.vwProfitToDate
AS
	WITH ProjectProfitToDate AS 
		(SELECT        MAX(PaymentOn) AS LastPaymentOn
		 FROM            Project.tbProject)
	SELECT TOP (100) PERCENT App.tbYearPeriod.StartOn, App.tbYear.Description + SPACE(1) + App.tbMonth.MonthName AS Description
	FROM            ProjectProfitToDate INNER JOIN
							App.tbYearPeriod INNER JOIN
							App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
							App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber ON DATEADD(m, 1, ProjectProfitToDate.LastPaymentOn) > App.tbYearPeriod.StartOn
	WHERE        (App.tbYear.CashStatusCode < 3)
	ORDER BY App.tbYearPeriod.StartOn DESC;
go
PRINT N'Creating View [Project].[vwAttributesForQuote]...';


go


CREATE   VIEW Project.vwAttributesForQuote
AS
SELECT        ProjectCode, Attribute, PrintOrder, AttributeDescription
FROM            Project.tbAttribute
WHERE        (AttributeTypeCode = 1);
go
PRINT N'Creating View [Project].[vwAttributesForOrder]...';


go


CREATE   VIEW Project.vwAttributesForOrder
AS
SELECT        ProjectCode, Attribute, PrintOrder, AttributeDescription
FROM            Project.tbAttribute
WHERE        (AttributeTypeCode = 0);
go
PRINT N'Creating View [Project].[vwAttributeDescriptions]...';


go

CREATE   VIEW Project.vwAttributeDescriptions
AS
SELECT        Attribute, AttributeDescription
FROM            Project.tbAttribute
GROUP BY Attribute, AttributeDescription
HAVING        (AttributeDescription IS NOT NULL);
go
PRINT N'Creating View [Project].[vwTitles]...';


go

CREATE   VIEW Project.vwTitles
AS
SELECT        ObjectCode, ProjectTitle
FROM            Project.tbProject
GROUP BY ProjectTitle, ObjectCode
HAVING        (ProjectTitle IS NOT NULL);
go
PRINT N'Creating View [Project].[vwNetworkEvents]...';


go
CREATE VIEW Project.vwNetworkEvents
AS
	SELECT        Project.tbAllocationEvent.ContractAddress, Project.tbAllocationEvent.LogId, App.tbEventType.EventTypeCode, App.tbEventType.EventType, 
							 Project.tbStatus.ProjectStatusCode, Project.tbStatus.ProjectStatus, Project.tbAllocationEvent.ActionOn, Project.tbAllocationEvent.UnitCharge, Project.tbAllocationEvent.TaxRate, Project.tbAllocationEvent.QuantityOrdered, 
							 Project.tbAllocationEvent.QuantityDelivered, Project.tbAllocationEvent.InsertedOn
	FROM            Project.tbAllocationEvent INNER JOIN
							 App.tbEventType ON Project.tbAllocationEvent.EventTypeCode = App.tbEventType.EventTypeCode INNER JOIN
							 Project.tbStatus ON Project.tbAllocationEvent.ProjectStatusCode = Project.tbStatus.ProjectStatusCode;
go
PRINT N'Creating View [Project].[vwCostSet]...';


go
CREATE   VIEW Project.vwCostSet
AS
	SELECT ProjectCode, UserId, InsertedBy, InsertedOn, RowVer
	FROM Project.tbCostSet
	WHERE (UserId = (SELECT UserId FROM Usr.vwCredentials));
go
PRINT N'Creating View [Project].[vwActiveStatusCodes]...';


go

CREATE   VIEW Project.vwActiveStatusCodes
AS
SELECT        ProjectStatusCode, ProjectStatus
FROM            Project.tbStatus
WHERE        (ProjectStatusCode < 3);
go
PRINT N'Creating View [Project].[vwAllocationSvD]...';


go
CREATE VIEW Project.vwAllocationSvD
AS
	WITH allocs AS
	(
		SELECT mirror.ObjectCode, alloc.SubjectCode, alloc.ProjectCode, alloc.ActionOn, 
			CASE CashPolarityCode WHEN 0 THEN 1 ELSE 0 END SupplyOrder, CAST(1 AS bit) IsAllocation, UnitCharge,
			CASE CashPolarityCode 
				WHEN 0 THEN (alloc.QuantityOrdered - alloc.QuantityDelivered) * -1
				WHEN 1 THEN (alloc.QuantityOrdered - alloc.QuantityDelivered)
			END Quantity,
			CASE CashPolarityCode 
				WHEN 0 THEN 1
				WHEN 1 THEN 0
			END CashPolarityCode			
		FROM Project.tbAllocation alloc
			JOIN Object.tbMirror mirror ON alloc.SubjectCode = mirror.SubjectCode AND alloc.AllocationCode = mirror.AllocationCode
		WHERE ProjectStatusCode BETWEEN 1 AND 2	
	), Projects AS
	(
		SELECT Project.ObjectCode, Project.SubjectCode, ProjectCode, ActionOn, Quantity, UnitCharge, CashPolarityCode
		FROM Project.tbProject Project
			JOIN Object.tbMirror mirror ON Project.SubjectCode = mirror.SubjectCode AND Project.ObjectCode = mirror.ObjectCode
			JOIN Cash.tbCode cash_code ON Project.CashCode = cash_code.CashCode
			JOIN Cash.tbCategory category ON cash_code.CategoryCode = category.CategoryCode
		WHERE ProjectStatusCode BETWEEN 1 AND 2
	), invoice_quantities AS
	(
		SELECT Projects.ProjectCode, SUM(COALESCE(invoice_quantity.Quantity, 0)) InvoiceQuantity
		FROM Projects
		OUTER APPLY 
			(
				SELECT CASE invoice.InvoiceTypeCode 
							WHEN 1 THEN delivery.Quantity * -1 
							WHEN 3 THEN delivery.Quantity * -1 
							ELSE delivery.Quantity 
						END Quantity
				FROM Invoice.tbProject delivery 
					JOIN Invoice.tbInvoice invoice ON delivery.InvoiceNumber = invoice.InvoiceNumber
				WHERE delivery.ProjectCode = Projects.ProjectCode
			) invoice_quantity
		GROUP BY Projects.ProjectCode
	), deliveries AS
	(
		SELECT Projects.*, invoice_quantities.InvoiceQuantity
		FROM Projects JOIN invoice_quantities ON Projects.ProjectCode = invoice_quantities.ProjectCode 
	
	), order_book AS
	(
		SELECT ObjectCode, SubjectCode, ProjectCode, ActionOn, CASE CashPolarityCode WHEN 0 THEN 1 ELSE 0 END SupplyOrder, CAST(0 AS bit) IsAllocation, UnitCharge,
			CASE CashPolarityCode
				WHEN 0 THEN (Quantity - InvoiceQuantity) * -1
				WHEN 1 THEN (Quantity - InvoiceQuantity)
			END Quantity,
			CashPolarityCode
		FROM deliveries
	), SvD AS
	(
		SELECT * FROM allocs
		UNION
		SELECT * FROM order_book
	), SvD_ordered AS
	(
		SELECT
			ObjectCode,
			ROW_NUMBER() OVER (PARTITION BY ObjectCode ORDER BY ActionOn, SupplyOrder) RowNumber,
			SubjectCode, IsAllocation, ProjectCode, CashPolarityCode, UnitCharge, ActionOn, Quantity
		FROM SvD
	), SvD_projection AS
	(
		SELECT
			ObjectCode, RowNumber, SubjectCode, IsAllocation, ProjectCode, CashPolarityCode, UnitCharge, ActionOn, Quantity,
			SUM(Quantity) OVER (PARTITION BY ObjectCode ORDER BY RowNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
		FROM SvD_ordered
	), SvD_scheduled AS
	(
		SELECT ObjectCode, RowNumber, SubjectCode, ProjectCode, IsAllocation, CashPolarityCode, UnitCharge, ActionOn, Quantity, Balance,
			CASE WHEN 
				LEAD(Balance, 1, Balance) OVER (PARTITION BY ObjectCode ORDER BY RowNumber) < 0 
					AND LAG(Balance, 1, 0) OVER (PARTITION BY ObjectCode ORDER BY RowNumber) >= 0 
					AND Balance < 0
				THEN ActionOn
				ELSE NULL END ScheduleOn
		FROM SvD_projection
	)
	SELECT CAST(ROW_NUMBER() OVER (ORDER BY SvD_scheduled.ObjectCode, RowNumber) AS int) AllocationId, SvD_scheduled.ObjectCode, Object.ObjectDescription, SubjectCode, IsAllocation, ProjectCode, SvD_scheduled.CashPolarityCode, polarity.CashPolarity, SvD_scheduled.UnitCharge, ActionOn, Quantity, CAST(Balance AS decimal(18,2)) Balance,
		MAX(ScheduleOn) OVER (PARTITION BY SvD_scheduled.ObjectCode ORDER BY RowNumber) ScheduleOn			
	FROM SvD_scheduled
		JOIN Object.tbObject Object ON SvD_scheduled.ObjectCode = Object.ObjectCode
		JOIN Cash.tbPolarity polarity ON SvD_scheduled.CashPolarityCode = polarity.CashPolarityCode;
go
PRINT N'Creating View [Project].[vwCashPolarity]...';


go

CREATE   VIEW Project.vwCashPolarity
  AS
SELECT     Project.tbProject.ProjectCode, CASE WHEN Cash.tbCategory.CategoryCode IS NULL 
                      THEN Subject.tbType.CashPolarityCode ELSE Cash.tbCategory.CashPolarityCode END AS CashPolarityCode
FROM         Project.tbProject INNER JOIN
                      Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
                      Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
                      Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
                      Subject.tbType ON Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode
go
PRINT N'Creating View [Project].[vwEdit]...';


go
CREATE VIEW Project.vwEdit
AS
	SELECT        Project.tbProject.ProjectCode, Project.tbProject.UserId, Project.tbProject.SubjectCode, Project.tbProject.ProjectTitle, Project.tbProject.ContactName, Project.tbProject.ObjectCode, Project.tbProject.ProjectStatusCode, Project.tbProject.ActionById, 
							 Project.tbProject.ActionOn, Project.tbProject.ActionedOn, Project.tbProject.ProjectNotes, Project.tbProject.Quantity, Project.tbProject.CashCode, Project.tbProject.TaxCode, Project.tbProject.UnitCharge, Project.tbProject.TotalCharge, 
							 Project.tbProject.AddressCodeFrom, Project.tbProject.AddressCodeTo, Project.tbProject.Printed, Project.tbProject.InsertedBy, Project.tbProject.InsertedOn, Project.tbProject.UpdatedBy, Project.tbProject.UpdatedOn, Project.tbProject.PaymentOn, 
							 Project.tbProject.SecondReference, Project.tbProject.Spooled, Object.tbObject.UnitOfMeasure, Project.tbStatus.ProjectStatus
	FROM            Project.tbProject INNER JOIN
							 Object.tbObject ON Project.tbProject.ObjectCode = Object.tbObject.ObjectCode INNER JOIN
							 Project.tbStatus ON Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode;
go
PRINT N'Creating View [Project].[vwNetworkDeployments]...';


go
CREATE VIEW Project.vwNetworkDeployments
AS
	SELECT DISTINCT Project.tbProject.ProjectCode, Project.tbProject.SubjectCode, Project.tbProject.ObjectCode, Object.tbObject.ObjectDescription, Project.tbProject.ProjectTitle, Project.tbProject.ProjectStatusCode, Project.tbStatus.ProjectStatus, Project.tbProject.ActionOn, Project.tbProject.Quantity, 
							 Cash.tbCategory.CashPolarityCode, Cash.tbPolarity.CashPolarity, App.tbTaxCode.TaxRate, Project.tbProject.UnitCharge, Object.tbObject.UnitOfMeasure,
								 (SELECT        UnitOfCharge
								   FROM            App.tbOptions) AS UnitOfCharge
	FROM            Project.tbChangeLog INNER JOIN
							 Project.tbProject ON Project.tbChangeLog.ProjectCode = Project.tbProject.ProjectCode INNER JOIN
							 Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode AND Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
							 App.tbTaxCode ON Project.tbProject.TaxCode = App.tbTaxCode.TaxCode AND Project.tbProject.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
							 Object.tbObject ON Project.tbProject.ObjectCode = Object.tbObject.ObjectCode AND Project.tbProject.ObjectCode = Object.tbObject.ObjectCode INNER JOIN
							 Project.tbStatus ON Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode INNER JOIN
							 Cash.tbPolarity ON Cash.tbCategory.CashPolarityCode = Cash.tbPolarity.CashPolarityCode AND Cash.tbCategory.CashPolarityCode = Cash.tbPolarity.CashPolarityCode
	WHERE        (Project.tbChangeLog.TransmitStatusCode = 1)
go
PRINT N'Creating View [Project].[vwNetworkUpdates]...';


go
CREATE VIEW Project.vwNetworkUpdates
AS
	WITH updates AS
	(
		SELECT DISTINCT ProjectCode FROM Project.tbChangeLog 
		WHERE TransmitStatusCode = 2
		EXCEPT
		SELECT DISTINCT ProjectCode FROM Project.tbChangeLog 
		WHERE TransmitStatusCode = 1
	)
	SELECT Project.tbProject.ProjectCode, Project.tbProject.SubjectCode, Project.tbProject.ObjectCode, Project.tbProject.ProjectStatusCode, Project.tbStatus.ProjectStatus, Project.tbProject.ActionOn, Project.tbProject.Quantity, App.tbTaxCode.TaxRate, Project.tbProject.UnitCharge
	FROM  updates 
		JOIN Project.tbProject ON updates.ProjectCode = Project.tbProject.ProjectCode 
		JOIN Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode 
		JOIN Project.tbStatus ON Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode
		JOIN App.tbTaxCode ON Project.tbProject.TaxCode = App.tbTaxCode.TaxCode AND Project.tbProject.TaxCode = App.tbTaxCode.TaxCode;
go
PRINT N'Creating View [Project].[vwNetworkQuotations]...';


go
CREATE VIEW Project.vwNetworkQuotations
AS
	WITH requests AS
	(
		SELECT mirror.ObjectCode, alloc.SubjectCode, alloc.ProjectCode, alloc.ActionOn, 
			CASE CashPolarityCode WHEN 0 THEN 1 ELSE 0 END SupplyOrder, CAST(1 AS bit) IsAllocation, UnitCharge,
			CASE CashPolarityCode 
				WHEN 0 THEN (alloc.QuantityOrdered - alloc.QuantityDelivered) * -1
				WHEN 1 THEN (alloc.QuantityOrdered - alloc.QuantityDelivered)
			END Quantity,
			CASE CashPolarityCode 
				WHEN 0 THEN 1
				WHEN 1 THEN 0
			END CashPolarityCode			
		FROM Project.tbAllocation alloc
			JOIN Object.tbMirror mirror ON alloc.SubjectCode = mirror.SubjectCode AND alloc.AllocationCode = mirror.AllocationCode
		WHERE ProjectStatusCode = 0	
	), Projects AS
	(
		SELECT Project.ObjectCode, Project.SubjectCode, ProjectCode, ActionOn,  
			CASE CashPolarityCode WHEN 0 THEN 1 ELSE 0 END SupplyOrder, CAST(0 AS bit) IsAllocation, UnitCharge,
			CASE CashPolarityCode
					WHEN 0 THEN Quantity * -1
					WHEN 1 THEN Quantity 
				END Quantity, CashPolarityCode
		FROM Project.tbProject Project
			JOIN Object.tbMirror mirror ON Project.SubjectCode = mirror.SubjectCode AND Project.ObjectCode = mirror.ObjectCode
			JOIN Cash.tbCode cash_code ON Project.CashCode = cash_code.CashCode
			JOIN Cash.tbCategory category ON cash_code.CategoryCode = category.CategoryCode
		WHERE ProjectStatusCode = 0
	), quotes AS
	(
		SELECT * FROM requests
		UNION
		SELECT * FROM Projects
	), quotes_ordered AS
	(
			SELECT
				ObjectCode,
				ROW_NUMBER() OVER (PARTITION BY ObjectCode ORDER BY ActionOn, SupplyOrder) RowNumber,
				SubjectCode, IsAllocation, ProjectCode, CashPolarityCode, UnitCharge, ActionOn, Quantity
			FROM quotes
	), quotes_projection AS
	(
		SELECT
			ObjectCode, RowNumber, SubjectCode, IsAllocation, ProjectCode, CashPolarityCode, UnitCharge, ActionOn, Quantity,
			SUM(Quantity) OVER (PARTITION BY ObjectCode ORDER BY RowNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
		FROM quotes_ordered
	)
	SELECT CAST(ROW_NUMBER() OVER (ORDER BY quotes_projection.ObjectCode, RowNumber) AS int) AllocationId, quotes_projection.ObjectCode, Object.ObjectDescription, SubjectCode, IsAllocation, 
		ProjectCode, quotes_projection.CashPolarityCode, polarity.CashPolarity, quotes_projection.UnitCharge, ActionOn, Quantity, CAST(Balance AS decimal(18,2)) Balance
	FROM quotes_projection
		JOIN Object.tbObject Object ON quotes_projection.ObjectCode = Object.ObjectCode
		JOIN Cash.tbPolarity polarity ON quotes_projection.CashPolarityCode = polarity.CashPolarityCode;
go
PRINT N'Creating View [Project].[vwSalesOrderSpool]...';


go
CREATE VIEW Project.vwSalesOrderSpool
AS
	SELECT        sales_order.ProjectCode, sales_order.ContactName, Subject.tbContact.NickName, Usr.tbUser.UserName, Subject.tbSubject.SubjectName, invoice_address.Address AS InvoiceAddress, 
							 delivery_address.Address AS DeliveryAddress, sales_order.SubjectCode, sales_order.ProjectNotes, sales_order.ProjectTitle, sales_order.ObjectCode, sales_order.ActionOn, Object.tbObject.UnitOfMeasure, 
							 sales_order.Quantity, App.tbTaxCode.TaxCode, App.tbTaxCode.TaxRate, sales_order.UnitCharge, sales_order.TotalCharge, Usr.tbUser.MobileNumber, Usr.tbUser.Signature
	FROM            Usr.tbUser INNER JOIN
							 Object.tbObject INNER JOIN
							 Project.tbProject AS sales_order ON Object.tbObject.ObjectCode = sales_order.ObjectCode INNER JOIN
							 Subject.tbSubject ON sales_order.SubjectCode = Subject.tbSubject.SubjectCode LEFT OUTER JOIN
							 Subject.tbAddress AS invoice_address ON Subject.tbSubject.AddressCode = invoice_address.AddressCode ON Usr.tbUser.UserId = sales_order.ActionById LEFT OUTER JOIN
							 Subject.tbAddress AS delivery_address ON sales_order.AddressCodeTo = delivery_address.AddressCode LEFT OUTER JOIN
							 App.tbTaxCode ON sales_order.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
							 Subject.tbContact ON sales_order.SubjectCode = Subject.tbContact.SubjectCode AND sales_order.ContactName = Subject.tbContact.ContactName
	WHERE EXISTS (
		SELECT        UserName, DocTypeCode, DocumentNumber, SpooledOn
		FROM            App.tbDocSpool AS doc
		WHERE        (DocTypeCode = 1) AND (UserName = SUSER_SNAME()) AND (sales_order.ProjectCode = DocumentNumber));
go
PRINT N'Creating View [Project].[vwFlow]...';


go
CREATE VIEW Project.vwFlow
AS
	SELECT        Project.tbFlow.ParentProjectCode, Project.tbFlow.StepNumber, Project.tbProject.ProjectCode, Project.tbProject.ObjectCode, Project.tbProject.ProjectTitle, Project.tbProject.ProjectNotes, Project.tbStatus.ProjectStatus, Project.tbProject.ActionOn, 
							 Project.tbProject.Quantity, Project.tbProject.ActionedOn, Subject.tbSubject.SubjectCode, Usr.tbUser.UserName AS Owner, tbUser_1.UserName AS ActionBy, Subject.tbSubject.SubjectName, Project.tbProject.UnitCharge, 
							 Project.tbProject.TotalCharge, Project.tbProject.InsertedBy, Project.tbProject.InsertedOn, Project.tbProject.UpdatedBy, Project.tbProject.UpdatedOn, Project.tbProject.ProjectStatusCode
	FROM            Usr.tbUser AS tbUser_1 INNER JOIN
							 Project.tbProject INNER JOIN
							 Project.tbStatus ON Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode INNER JOIN
							 Usr.tbUser ON Project.tbProject.UserId = Usr.tbUser.UserId INNER JOIN
							 Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode ON tbUser_1.UserId = Project.tbProject.ActionById INNER JOIN
							 Project.tbFlow ON Project.tbProject.ProjectCode = Project.tbFlow.ChildProjectCode;
go
PRINT N'Creating View [Project].[vwChangeLog]...';


go
CREATE VIEW Project.vwChangeLog
AS
	SELECT        Project.tbChangeLog.LogId, Project.tbChangeLog.ProjectCode, Project.tbChangeLog.ChangedOn, Subject.tbTransmitStatus.TransmitStatusCode, Subject.tbTransmitStatus.TransmitStatus, Subject.tbSubject.SubjectCode, Subject.tbSubject.SubjectName, 
							 Project.tbChangeLog.ObjectCode, Project.tbChangeLog.ProjectStatusCode, Project.tbStatus.ProjectStatus, Project.tbChangeLog.ActionOn, Project.tbChangeLog.Quantity, Project.tbChangeLog.CashCode, Cash.tbCode.CashDescription, 
							 Project.tbChangeLog.UnitCharge, Project.tbChangeLog.UnitCharge * Project.tbChangeLog.Quantity AS TotalCharge, Project.tbChangeLog.TaxCode, App.tbTaxCode.TaxRate, Project.tbChangeLog.UpdatedBy
	FROM            Project.tbChangeLog INNER JOIN
							 Subject.tbTransmitStatus ON Project.tbChangeLog.TransmitStatusCode = Subject.tbTransmitStatus.TransmitStatusCode INNER JOIN
							 Subject.tbSubject ON Project.tbChangeLog.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Project.tbStatus ON Project.tbChangeLog.ProjectStatusCode = Project.tbStatus.ProjectStatusCode INNER JOIN
							 App.tbTaxCode ON Project.tbChangeLog.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
							 Cash.tbCode ON Project.tbChangeLog.CashCode = Cash.tbCode.CashCode;
go
PRINT N'Creating View [Project].[vwNetworkEventLog]...';


go
CREATE VIEW Project.vwNetworkEventLog
AS
	SELECT        Project.tbAllocationEvent.ContractAddress, Project.tbAllocationEvent.LogId, Project.tbAllocationEvent.EventTypeCode, Project.tbAllocationEvent.ProjectStatusCode, Project.tbAllocationEvent.ActionOn, Project.tbAllocationEvent.UnitCharge, 
							 Project.tbAllocationEvent.TaxRate, Project.tbAllocationEvent.QuantityOrdered, Project.tbAllocationEvent.QuantityDelivered, Project.tbAllocationEvent.InsertedOn, Project.tbAllocationEvent.RowVer, App.tbEventType.EventType, 
							 Project.tbStatus.ProjectStatus, Project.tbAllocation.SubjectCode, Subject.tbSubject.SubjectName, Object.tbMirror.ObjectCode, Project.tbAllocation.AllocationCode, Project.tbAllocation.AllocationDescription, Project.tbAllocation.ProjectCode, 
							 Project.tbAllocation.CashPolarityCode, Cash.tbPolarity.CashPolarity, Project.tbAllocation.UnitOfMeasure, Project.tbAllocation.UnitOfCharge
	FROM            Project.tbAllocationEvent INNER JOIN
							 Project.tbStatus ON Project.tbAllocationEvent.ProjectStatusCode = Project.tbStatus.ProjectStatusCode INNER JOIN
							 Project.tbAllocation ON Project.tbAllocationEvent.ContractAddress = Project.tbAllocation.ContractAddress AND Project.tbStatus.ProjectStatusCode = Project.tbAllocation.ProjectStatusCode AND 
							 Project.tbStatus.ProjectStatusCode = Project.tbAllocation.ProjectStatusCode INNER JOIN
							 Subject.tbSubject ON Project.tbAllocation.SubjectCode = Subject.tbSubject.SubjectCode AND Project.tbAllocation.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Cash.tbPolarity ON Project.tbAllocation.CashPolarityCode = Cash.tbPolarity.CashPolarityCode AND Project.tbAllocation.CashPolarityCode = Cash.tbPolarity.CashPolarityCode INNER JOIN
							 Object.tbMirror ON Project.tbAllocation.SubjectCode = Object.tbMirror.SubjectCode AND Project.tbAllocation.AllocationCode = Object.tbMirror.AllocationCode INNER JOIN
							 App.tbEventType ON Project.tbAllocationEvent.EventTypeCode = App.tbEventType.EventTypeCode;
go
PRINT N'Creating View [Project].[vwNetworkChangeLog]...';


go
CREATE VIEW Project.vwNetworkChangeLog
AS
	SELECT Project.tbProject.SubjectCode, Subject.tbSubject.SubjectName, Project.tbProject.ProjectCode, Project.tbChangeLog.LogId, Project.tbChangeLog.ChangedOn, Project.tbChangeLog.TransmitStatusCode, Subject.tbTransmitStatus.TransmitStatus, 
				Project.tbChangeLog.ObjectCode, Object.tbMirror.AllocationCode, Project.tbChangeLog.ProjectStatusCode, Project.tbStatus.ProjectStatus, Cash.tbPolarity.CashPolarityCode, Cash.tbPolarity.CashPolarity, Project.tbChangeLog.ActionOn, 
				Project.tbChangeLog.TaxCode, Project.tbChangeLog.Quantity, Project.tbChangeLog.UnitCharge, Project.tbChangeLog.UpdatedBy, Project.tbChangeLog.RowVer
	FROM Project.tbChangeLog 
		INNER JOIN Project.tbProject ON Project.tbChangeLog.ProjectCode = Project.tbProject.ProjectCode INNER JOIN
				Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
				Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode AND Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
				Cash.tbPolarity ON Cash.tbCategory.CashPolarityCode = Cash.tbPolarity.CashPolarityCode AND Cash.tbCategory.CashPolarityCode = Cash.tbPolarity.CashPolarityCode INNER JOIN
				Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode AND Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode AND Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
				Subject.tbTransmitStatus ON Project.tbChangeLog.TransmitStatusCode = Subject.tbTransmitStatus.TransmitStatusCode AND Project.tbChangeLog.TransmitStatusCode = Subject.tbTransmitStatus.TransmitStatusCode AND 
				Project.tbChangeLog.TransmitStatusCode = Subject.tbTransmitStatus.TransmitStatusCode AND Project.tbChangeLog.TransmitStatusCode = Subject.tbTransmitStatus.TransmitStatusCode AND 
				Project.tbChangeLog.TransmitStatusCode = Subject.tbTransmitStatus.TransmitStatusCode INNER JOIN
				Project.tbStatus ON Project.tbChangeLog.ProjectStatusCode = Project.tbStatus.ProjectStatusCode LEFT OUTER JOIN
				Object.tbMirror ON Project.tbChangeLog.SubjectCode = Object.tbMirror.SubjectCode AND Project.tbChangeLog.ObjectCode = Object.tbMirror.ObjectCode
	WHERE Project.tbChangeLog.TransmitStatusCode > 0
go
PRINT N'Creating View [Project].[vwNetworkAllocations]...';


go
CREATE VIEW Project.vwNetworkAllocations
AS
	SELECT        Project.tbAllocation.ContractAddress, Project.tbAllocation.SubjectCode, Subject.tbSubject.SubjectName, Object.tbMirror.ObjectCode, Project.tbAllocation.AllocationCode, Project.tbAllocation.AllocationDescription, Project.tbAllocation.ProjectCode, 
							 Project.tbAllocation.ProjectTitle, Project.tbAllocation.CashPolarityCode, Cash.tbPolarity.CashPolarity, Project.tbAllocation.UnitOfMeasure, Project.tbAllocation.UnitOfCharge, Project.tbAllocation.ProjectStatusCode, Project.tbStatus.ProjectStatus, 
							 Project.tbAllocation.ActionOn, Project.tbAllocation.UnitCharge, Project.tbAllocation.TaxRate, Project.tbAllocation.QuantityOrdered, Project.tbAllocation.QuantityDelivered, Project.tbAllocation.InsertedOn, Project.tbAllocation.RowVer
	FROM            Project.tbAllocation INNER JOIN
							 Object.tbMirror ON Project.tbAllocation.SubjectCode = Object.tbMirror.SubjectCode AND Project.tbAllocation.AllocationCode = Object.tbMirror.AllocationCode INNER JOIN
							 Subject.tbSubject ON Project.tbAllocation.SubjectCode = Subject.tbSubject.SubjectCode AND Project.tbAllocation.SubjectCode = Subject.tbSubject.SubjectCode AND Project.tbAllocation.SubjectCode = Subject.tbSubject.SubjectCode AND 
							 Project.tbAllocation.SubjectCode = Subject.tbSubject.SubjectCode AND Project.tbAllocation.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Cash.tbPolarity ON Project.tbAllocation.CashPolarityCode = Cash.tbPolarity.CashPolarityCode AND Project.tbAllocation.CashPolarityCode = Cash.tbPolarity.CashPolarityCode AND 
							 Project.tbAllocation.CashPolarityCode = Cash.tbPolarity.CashPolarityCode AND Project.tbAllocation.CashPolarityCode = Cash.tbPolarity.CashPolarityCode AND Project.tbAllocation.CashPolarityCode = Cash.tbPolarity.CashPolarityCode INNER JOIN
							 Project.tbStatus ON Project.tbAllocation.ProjectStatusCode = Project.tbStatus.ProjectStatusCode AND Project.tbAllocation.ProjectStatusCode = Project.tbStatus.ProjectStatusCode AND 
							 Project.tbAllocation.ProjectStatusCode = Project.tbStatus.ProjectStatusCode AND Project.tbAllocation.ProjectStatusCode = Project.tbStatus.ProjectStatusCode AND Project.tbAllocation.ProjectStatusCode = Project.tbStatus.ProjectStatusCode;
go
PRINT N'Creating View [Project].[vwProfit]...';


go
CREATE VIEW [Project].[vwProfit] 
AS
	WITH orders AS
	(
		SELECT        Project.ProjectCode, Project.Quantity, Project.UnitCharge,
									 (SELECT        TOP (1) StartOn
									   FROM            App.tbYearPeriod AS p
									   WHERE        (StartOn <= Project.ActionOn)
									   ORDER BY StartOn DESC) AS StartOn
		FROM            Project.tbFlow RIGHT OUTER JOIN
								 Project.tbProject ON Project.tbFlow.ParentProjectCode = Project.tbProject.ProjectCode AND Project.tbFlow.ParentProjectCode = Project.tbProject.ProjectCode AND Project.tbFlow.ParentProjectCode = Project.tbProject.ProjectCode RIGHT OUTER JOIN
								 Project.tbProject AS Project INNER JOIN
								 Cash.tbCode AS cashcode ON Project.CashCode = cashcode.CashCode INNER JOIN
								 Cash.tbCategory AS category ON category.CategoryCode = cashcode.CategoryCode ON Project.tbFlow.ChildProjectCode = Project.ProjectCode AND Project.tbFlow.ChildProjectCode = Project.ProjectCode
		WHERE        (category.CashPolarityCode = 1) AND (Project.ProjectStatusCode BETWEEN 1 AND 3) AND 
			(Project.ActionOn >= (SELECT        MIN(StartOn)
											FROM            App.tbYearPeriod p JOIN
																	  App.tbYear y ON p.YearNumber = y.YearNumber
											WHERE        y.CashStatusCode < 3)) AND	
			((Project.tbFlow.ParentProjectCode IS NULL) OR (Project.tbProject.CashCode IS NULL))

	), invoices AS
	(
		SELECT Projects.ProjectCode, ISNULL(invoice.InvoiceValue, 0) AS InvoiceValue, ISNULL(invoice.InvoicePaid, 0) AS InvoicePaid 
		FROM Project.tbProject Projects LEFT OUTER JOIN 
			(
				SELECT Invoice.tbProject.ProjectCode, 
					SUM(CASE CashPolarityCode WHEN 0 THEN Invoice.tbProject.InvoiceValue * -1 ELSE Invoice.tbProject.InvoiceValue END) AS InvoiceValue, 
					CASE InvoiceStatusCode WHEN 3 THEN 
						SUM(CASE CashPolarityCode WHEN 0 THEN Invoice.tbProject.InvoiceValue * -1 ELSE Invoice.tbProject.InvoiceValue END)
					ELSE 0
					END AS InvoicePaid
				FROM Invoice.tbProject 
					INNER JOIN Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
					INNER JOIN Invoice.tbType ON Invoice.tbType.InvoiceTypeCode = Invoice.tbInvoice.InvoiceTypeCode 
				GROUP BY Invoice.tbProject.ProjectCode, Invoice.tbInvoice.InvoiceStatusCode
			) invoice 
		ON Projects.ProjectCode = invoice.ProjectCode
	), Project_flow AS
	(
		SELECT orders.ProjectCode, child.ParentProjectCode, child.ChildProjectCode, 
			CASE WHEN child.UsedOnQuantity <> 0 THEN CAST(orders.Quantity * child.UsedOnQuantity AS decimal(18, 4)) ELSE Project.Quantity END AS Quantity
		FROM Project.tbFlow child 
			JOIN orders ON child.ParentProjectCode = orders.ProjectCode
			JOIN Project.tbProject Project ON child.ChildProjectCode = Project.ProjectCode

		UNION ALL

		SELECT parent.ProjectCode, child.ParentProjectCode, child.ChildProjectCode, 
			CASE WHEN child.UsedOnQuantity <> 0 THEN CAST(parent.Quantity * child.UsedOnQuantity AS decimal(18, 4)) ELSE Project.Quantity END AS Quantity
		FROM Project.tbFlow child 
			JOIN Project_flow parent ON child.ParentProjectCode = parent.ChildProjectCode
			JOIN Project.tbProject Project ON child.ChildProjectCode = Project.ProjectCode

	), Projects AS
	(
		SELECT Project_flow.ProjectCode, Project.Quantity,
				CASE category.CashPolarityCode 
					WHEN NULL THEN 0 
					WHEN 0 THEN Project.UnitCharge * -1 
					ELSE Project.UnitCharge 
				END AS UnitCharge,
				invoices.InvoiceValue, invoices.InvoicePaid
		FROM Project_flow
			JOIN Project.tbProject Project ON Project_flow.ChildProjectCode = Project.ProjectCode
			JOIN invoices ON invoices.ProjectCode = Project.ProjectCode
			LEFT OUTER JOIN Cash.tbCode cashcode ON cashcode.CashCode = Project.CashCode 
			LEFT OUTER JOIN Cash.tbCategory category ON category.CategoryCode = cashcode.CategoryCode
	)
	, Project_costs AS
	(
		SELECT ProjectCode, ROUND(SUM(Quantity * UnitCharge), 2) AS TotalCost, 
				ROUND(SUM(InvoiceValue), 2) AS InvoicedCost, ROUND(SUM(InvoicePaid), 2) AS InvoicedCostPaid
		FROM Projects
		GROUP BY ProjectCode
		UNION
		SELECT ProjectCode, 0 AS TotalCost, 0 AS InvoicedCost, 0 AS InvoicedCostPaid
		FROM orders LEFT OUTER JOIN Project.tbFlow AS flow ON orders.ProjectCode = flow.ParentProjectCode
		WHERE (flow.ParentProjectCode IS NULL)
	), profits AS
	(
		SELECT orders.StartOn, Project.SubjectCode, orders.ProjectCode, 
			yearperiod.YearNumber, yr.Description, 
			CONCAT(mn.MonthName, ' ', YEAR(yearperiod.StartOn)) AS Period,
			Project.ObjectCode, cashcode.CashCode, Project.ProjectTitle, Subject.SubjectName, cashcode.CashDescription,
			Projectstatus.ProjectStatus, Project.ProjectStatusCode, Project.TotalCharge, invoices.InvoiceValue AS InvoicedCharge,
			invoices.InvoicePaid AS InvoicedChargePaid,
			Project_costs.TotalCost, Project_costs.InvoicedCost, Project_costs.InvoicedCostPaid,
			Project.TotalCharge + Project_costs.TotalCost AS Profit,
			Project.TotalCharge - invoices.InvoiceValue AS UninvoicedCharge,
			invoices.InvoiceValue - invoices.InvoicePaid AS UnpaidCharge,
			Project_costs.TotalCost - Project_costs.InvoicedCost AS UninvoicedCost,
			Project_costs.InvoicedCost - Project_costs.InvoicedCostPaid AS UnpaidCost,
			Project.ActionOn, Project.ActionedOn, Project.PaymentOn
		FROM orders 
			JOIN Project.tbProject Project ON Project.ProjectCode = orders.ProjectCode
			JOIN invoices ON invoices.ProjectCode = Project.ProjectCode
			JOIN Project_costs ON orders.ProjectCode = Project_costs.ProjectCode	
			JOIN Cash.tbCode cashcode ON Project.CashCode = cashcode.CashCode
			JOIN Project.tbStatus Projectstatus ON Projectstatus.ProjectStatusCode = Project.ProjectStatusCode
			JOIN Subject.tbSubject Subject ON Subject.SubjectCode = Project.SubjectCode
			JOIN App.tbYearPeriod yearperiod ON yearperiod.StartOn = orders.StartOn
			JOIN App.tbYear yr ON yr.YearNumber = yearperiod.YearNumber
			JOIN App.tbMonth mn ON mn.MonthNumber = yearperiod.MonthNumber
		)
		SELECT StartOn, SubjectCode, ProjectCode, YearNumber, [Description], [Period], ObjectCode, CashCode,
			ProjectTitle, SubjectName, CashDescription, ProjectStatus, ProjectStatusCode, CAST(TotalCharge as float) TotalCharge, CAST(InvoicedCharge as float) InvoicedCharge, CAST(InvoicedChargePaid as float) InvoicedChargePaid,
			CAST(TotalCost AS float) TotalCost, CAST(InvoicedCost as float) InvoicedCost, CAST(InvoicedCostPaid as float) InvoicedCostPaid, CAST(Profit AS float) Profit,
			CAST(UninvoicedCharge AS float) UninvoicedCharge, CAST(UnpaidCharge AS float) UnpaidCharge,
			CAST(UninvoicedCost AS float) UninvoicedCost, CAST(UnpaidCost AS float) UnpaidCost,
			ActionOn, ActionedOn, PaymentOn
		FROM profits;
go
PRINT N'Creating View [Project].[vwPurchaseEnquirySpool]...';


go
CREATE VIEW Project.vwPurchaseEnquirySpool
AS
SELECT        purchase_enquiry.ProjectCode, purchase_enquiry.ContactName, Subject.tbContact.NickName, Usr.tbUser.UserName, Subject.tbSubject.SubjectName, Subject.tbAddress.Address AS InvoiceAddress, 
                         Subject_tbAddress_1.Address AS DeliveryAddress, purchase_enquiry.SubjectCode, purchase_enquiry.ProjectNotes, purchase_enquiry.ObjectCode, purchase_enquiry.ActionOn, Object.tbObject.UnitOfMeasure, 
                         purchase_enquiry.Quantity, App.tbTaxCode.TaxCode, App.tbTaxCode.TaxRate, purchase_enquiry.UnitCharge, purchase_enquiry.TotalCharge, Usr.tbUser.MobileNumber, Usr.tbUser.Signature, 
                         purchase_enquiry.ProjectTitle
FROM            Usr.tbUser INNER JOIN
                         Object.tbObject INNER JOIN
                         Project.tbProject AS purchase_enquiry ON Object.tbObject.ObjectCode = purchase_enquiry.ObjectCode INNER JOIN
                         Subject.tbSubject ON purchase_enquiry.SubjectCode = Subject.tbSubject.SubjectCode LEFT OUTER JOIN
                         Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode ON Usr.tbUser.UserId = purchase_enquiry.ActionById LEFT OUTER JOIN
                         Subject.tbAddress AS Subject_tbAddress_1 ON purchase_enquiry.AddressCodeTo = Subject_tbAddress_1.AddressCode LEFT OUTER JOIN
                         App.tbTaxCode ON purchase_enquiry.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
                         Subject.tbContact ON purchase_enquiry.SubjectCode = Subject.tbContact.SubjectCode AND purchase_enquiry.ContactName = Subject.tbContact.ContactName
WHERE EXISTS (SELECT * FROM App.tbDocSpool AS doc WHERE DocTypeCode = 2 AND UserName = SUSER_SNAME() AND purchase_enquiry.ProjectCode = doc.DocumentNumber);
go
PRINT N'Creating View [Project].[vwPurchaseEnquiryDeliverySpool]...';


go
CREATE VIEW Project.vwPurchaseEnquiryDeliverySpool
AS
	SELECT        purchase_enquiry.ProjectCode, purchase_enquiry.ContactName, Subject.tbContact.NickName, Usr.tbUser.UserName, Subject.tbSubject.SubjectName, Subject.tbAddress.Address AS InvoiceAddress, 
							 collection_account.SubjectName AS CollectAccount, collection_address.Address AS CollectAddress, delivery_account.SubjectName AS DeliveryAccount, delivery_address.Address AS DeliveryAddress, 
							 purchase_enquiry.SubjectCode, purchase_enquiry.ProjectNotes, purchase_enquiry.ObjectCode, purchase_enquiry.ActionOn, Object.tbObject.UnitOfMeasure, purchase_enquiry.Quantity, App.tbTaxCode.TaxCode, 
							 App.tbTaxCode.TaxRate, purchase_enquiry.UnitCharge, purchase_enquiry.TotalCharge, Usr.tbUser.MobileNumber, Usr.tbUser.Signature, purchase_enquiry.ProjectTitle
	FROM            Subject.tbSubject AS delivery_account INNER JOIN
							 Subject.tbSubject AS collection_account INNER JOIN
							 Usr.tbUser INNER JOIN
							 Object.tbObject INNER JOIN
							 Project.tbProject AS purchase_enquiry ON Object.tbObject.ObjectCode = purchase_enquiry.ObjectCode INNER JOIN
							 Subject.tbSubject ON purchase_enquiry.SubjectCode = Subject.tbSubject.SubjectCode LEFT OUTER JOIN
							 Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode ON Usr.tbUser.UserId = purchase_enquiry.ActionById INNER JOIN
							 Subject.tbAddress AS delivery_address ON purchase_enquiry.AddressCodeTo = delivery_address.AddressCode LEFT OUTER JOIN
							 App.tbTaxCode ON purchase_enquiry.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
							 Subject.tbContact ON purchase_enquiry.ContactName = Subject.tbContact.ContactName AND purchase_enquiry.SubjectCode = Subject.tbContact.SubjectCode INNER JOIN
							 Subject.tbAddress AS collection_address ON purchase_enquiry.AddressCodeFrom = collection_address.AddressCode ON collection_account.SubjectCode = collection_address.SubjectCode ON 
							 delivery_account.SubjectCode = delivery_address.SubjectCode
	WHERE EXISTS (SELECT * FROM App.tbDocSpool AS doc WHERE DocTypeCode = 2 AND UserName = SUSER_SNAME() AND purchase_enquiry.ProjectCode = doc.DocumentNumber);
go
PRINT N'Creating View [Project].[vwQuotationSpool]...';


go
CREATE VIEW Project.vwQuotationSpool
AS
	SELECT        sales_order.ProjectCode, sales_order.ContactName, Subject.tbContact.NickName, Usr.tbUser.UserName, Subject.tbSubject.SubjectName, invoice_address.Address AS InvoiceAddress, 
							 delivery_address.Address AS DeliveryAddress, sales_order.SubjectCode, sales_order.ProjectNotes, sales_order.ObjectCode, sales_order.ActionOn, Object.tbObject.UnitOfMeasure, sales_order.Quantity, 
							 App.tbTaxCode.TaxCode, App.tbTaxCode.TaxRate, sales_order.UnitCharge, sales_order.TotalCharge, Usr.tbUser.MobileNumber, Usr.tbUser.Signature, sales_order.ProjectTitle
	FROM            Usr.tbUser INNER JOIN
							 Object.tbObject INNER JOIN
							 Project.tbProject AS sales_order ON Object.tbObject.ObjectCode = sales_order.ObjectCode INNER JOIN
							 Subject.tbSubject ON sales_order.SubjectCode = Subject.tbSubject.SubjectCode LEFT OUTER JOIN
							 Subject.tbAddress AS invoice_address ON Subject.tbSubject.AddressCode = invoice_address.AddressCode ON Usr.tbUser.UserId = sales_order.ActionById LEFT OUTER JOIN
							 Subject.tbAddress AS delivery_address ON sales_order.AddressCodeTo = delivery_address.AddressCode LEFT OUTER JOIN
							 App.tbTaxCode ON sales_order.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
							 Subject.tbContact ON sales_order.SubjectCode = Subject.tbContact.SubjectCode AND sales_order.ContactName = Subject.tbContact.ContactName
	WHERE EXISTS (
		SELECT        UserName, DocTypeCode, DocumentNumber, SpooledOn
		FROM            App.tbDocSpool AS doc
		WHERE        (DocTypeCode = 0) AND (UserName = SUSER_SNAME()) AND (sales_order.ProjectCode = DocumentNumber));
go
PRINT N'Creating View [Project].[vwPurchaseOrderDeliverySpool]...';


go
CREATE VIEW Project.vwPurchaseOrderDeliverySpool
AS
	SELECT        purchase_order.ProjectCode, purchase_order.ContactName, Subject.tbContact.NickName, Usr.tbUser.UserName, Subject.tbSubject.SubjectName, invoice_address.Address AS InvoiceAddress, 
							 delivery_account.SubjectName AS CollectAccount, delivery_address.Address AS CollectAddress, collection_account.SubjectName AS DeliveryAccount, collection_address.Address AS DeliveryAddress, 
							 purchase_order.SubjectCode, purchase_order.ProjectNotes, purchase_order.ObjectCode, purchase_order.ActionOn, Object.tbObject.UnitOfMeasure, purchase_order.Quantity, App.tbTaxCode.TaxCode, 
							 App.tbTaxCode.TaxRate, purchase_order.UnitCharge, purchase_order.TotalCharge, Usr.tbUser.MobileNumber, Usr.tbUser.Signature, purchase_order.ProjectTitle
	FROM            Subject.tbSubject AS collection_account INNER JOIN
							 Subject.tbSubject AS delivery_account INNER JOIN
							 Usr.tbUser INNER JOIN
							 Object.tbObject INNER JOIN
							 Project.tbProject AS purchase_order ON Object.tbObject.ObjectCode = purchase_order.ObjectCode INNER JOIN
							 Subject.tbSubject ON purchase_order.SubjectCode = Subject.tbSubject.SubjectCode LEFT OUTER JOIN
							 Subject.tbAddress AS invoice_address ON Subject.tbSubject.AddressCode = invoice_address.AddressCode ON Usr.tbUser.UserId = purchase_order.ActionById INNER JOIN
							 Subject.tbAddress AS collection_address ON purchase_order.AddressCodeTo = collection_address.AddressCode LEFT OUTER JOIN
							 App.tbTaxCode ON purchase_order.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
							 Subject.tbContact ON purchase_order.ContactName = Subject.tbContact.ContactName AND purchase_order.SubjectCode = Subject.tbContact.SubjectCode INNER JOIN
							 Subject.tbAddress AS delivery_address ON purchase_order.AddressCodeFrom = delivery_address.AddressCode ON delivery_account.SubjectCode = delivery_address.SubjectCode ON 
							 collection_account.SubjectCode = collection_address.SubjectCode
	WHERE EXISTS (
		SELECT        UserName, DocTypeCode, DocumentNumber, SpooledOn
		FROM            App.tbDocSpool AS doc
		WHERE        (DocTypeCode = 3) AND (UserName = SUSER_SNAME()) AND (purchase_order.ProjectCode = DocumentNumber));
go
PRINT N'Creating View [Project].[vwPurchaseOrderSpool]...';


go
CREATE VIEW Project.vwPurchaseOrderSpool
AS
	SELECT        purchase_order.ProjectCode, purchase_order.ContactName, Subject.tbContact.NickName, Usr.tbUser.UserName, Subject.tbSubject.SubjectName, invoice_address.Address AS InvoiceAddress, 
							 delivery_address.Address AS DeliveryAddress, purchase_order.SubjectCode, purchase_order.ProjectNotes, purchase_order.ObjectCode, purchase_order.ActionOn, Object.tbObject.UnitOfMeasure, 
							 purchase_order.Quantity, App.tbTaxCode.TaxCode, App.tbTaxCode.TaxRate, purchase_order.UnitCharge, purchase_order.TotalCharge, Usr.tbUser.MobileNumber, Usr.tbUser.Signature, 
							 purchase_order.ProjectTitle
	FROM            Usr.tbUser INNER JOIN
							 Object.tbObject INNER JOIN
							 Project.tbProject AS purchase_order ON Object.tbObject.ObjectCode = purchase_order.ObjectCode INNER JOIN
							 Subject.tbSubject ON purchase_order.SubjectCode = Subject.tbSubject.SubjectCode LEFT OUTER JOIN
							 Subject.tbAddress AS invoice_address ON Subject.tbSubject.AddressCode = invoice_address.AddressCode ON Usr.tbUser.UserId = purchase_order.ActionById LEFT OUTER JOIN
							 Subject.tbAddress AS delivery_address ON purchase_order.AddressCodeTo = delivery_address.AddressCode LEFT OUTER JOIN
							 App.tbTaxCode ON purchase_order.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
							 Subject.tbContact ON purchase_order.SubjectCode = Subject.tbContact.SubjectCode AND purchase_order.ContactName = Subject.tbContact.ContactName
	WHERE EXISTS (SELECT * FROM App.tbDocSpool AS doc WHERE DocTypeCode = 3 AND UserName = SUSER_SNAME() AND purchase_order.ProjectCode = doc.DocumentNumber);
go
PRINT N'Creating View [Usr].[vwUserMenus]...';


go
CREATE VIEW Usr.vwUserMenus
AS
	SELECT Usr.tbMenuUser.MenuId, Usr.tbMenu.InterfaceCode
	FROM Usr.vwCredentials 
		JOIN Usr.tbMenuUser ON Usr.vwCredentials.UserId = Usr.tbMenuUser.UserId
		JOIN Usr.tbMenu ON Usr.tbMenuUser.MenuId = Usr.tbMenu.MenuId;
go
PRINT N'Creating View [Invoice].[vwHistoryCashCodes]...';


go
CREATE VIEW Invoice.vwHistoryCashCodes
AS
SELECT        App.tbYearPeriod.YearNumber, CONCAT(App.tbMonth.MonthName, SPACE(1), YEAR(App.tbYearPeriod.StartOn)) AS Period, Invoice.vwRegisterDetail.StartOn, Invoice.vwRegisterDetail.CashCode, 
                         Invoice.vwRegisterDetail.CashDescription, SUM(Invoice.vwRegisterDetail.InvoiceValue) AS TotalInvoiceValue, SUM(Invoice.vwRegisterDetail.TaxValue) AS TotalTaxValue
FROM            Invoice.vwRegisterDetail INNER JOIN
                         App.tbYearPeriod INNER JOIN
                         App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber ON Invoice.vwRegisterDetail.StartOn = App.tbYearPeriod.StartOn INNER JOIN
                         App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
GROUP BY App.tbYearPeriod.YearNumber, CONCAT(App.tbMonth.MonthName, SPACE(1), YEAR(App.tbYearPeriod.StartOn)), Invoice.vwRegisterDetail.StartOn, Invoice.vwRegisterDetail.CashCode, 
                         Invoice.vwRegisterDetail.CashDescription;
go
PRINT N'Creating View [Invoice].[vwDocDetails]...';


go
CREATE   VIEW Invoice.vwDocDetails
AS
	SELECT 
		InvoiceNumber, 
		ProjectCode ItemCode,
		ObjectCode ItemDescription,
		CAST(SecondReference as nvarchar(MAX)) ItemReference,
		TaxCode,
		InvoiceValue,
		TaxValue,
		InvoiceValue + TaxValue AS TotalValue,
		CAST(1 as bit) IsProject,
		ActionedOn,
		Quantity,
		UnitOfMeasure
	FROM Invoice.vwDocProject

	UNION

	SELECT
		InvoiceNumber,
		CashCode ItemCode,
		CashDescription ItemDescription,
		CAST(ItemReference as nvarchar(MAX)) ItemReference,
		TaxCode,
		InvoiceValue,
		TaxValue,
		InvoiceValue + TaxValue AS TotalValue,
		CAST(0 as bit) IsProject,
		ActionedOn,
		1 Quantity,
		NULL UnitOfMeasure	
	FROM Invoice.vwDocItem;
go
PRINT N'Creating View [Invoice].[vwRegisterCashCodes]...';


go
CREATE VIEW Invoice.vwRegisterCashCodes
AS
	WITH cash_codes AS
	(
		SELECT StartOn, CashCode, CashDescription, CashPolarityCode, CAST(SUM(InvoiceValue) as float) AS TotalInvoiceValue, CAST(SUM(TaxValue) as float) AS TotalTaxValue
		FROM            Invoice.vwRegisterDetail
		GROUP BY StartOn, CashCode, CashDescription, CashPolarityCode	
	)
	SELECT cash_codes.StartOn, CONCAT(financial_year.[Description], ' ', app_month.MonthName) PeriodName, CashPolarity,
		CashCode, CashDescription, TotalInvoiceValue, TotalTaxValue, TotalInvoiceValue + TotalTaxValue as TotalValue		
	FROM cash_codes
		JOIN Cash.tbPolarity cash_mode ON cash_codes.CashPolarityCode = cash_mode.CashPolarityCode
		JOIN App.tbYearPeriod year_period ON cash_codes.StartOn = year_period.StartOn
		JOIN App.tbMonth app_month ON year_period.MonthNumber = app_month.MonthNumber
		JOIN App.tbYear financial_year ON year_period.YearNumber = financial_year.YearNumber;
go
PRINT N'Creating View [Invoice].[vwRegisterPurchases]...';


go
CREATE   VIEW Invoice.vwRegisterPurchases
AS
SELECT        StartOn, InvoiceNumber, SubjectCode, InvoiceTypeCode, InvoiceStatusCode, InvoicedOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue, PaymentTerms, Notes, Printed, SubjectName, UserName, 
                         InvoiceStatus, CashPolarityCode, InvoiceType, (InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue) AS UnpaidValue
FROM            Invoice.vwRegister
WHERE        (InvoiceTypeCode > 1);
go
PRINT N'Creating View [Invoice].[vwRegisterSales]...';


go
CREATE    VIEW Invoice.vwRegisterSales
AS
SELECT        StartOn, InvoiceNumber, SubjectCode, InvoiceTypeCode, InvoiceStatusCode, InvoicedOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue, PaymentTerms, Notes, Printed, SubjectName, UserName, 
                         InvoiceStatus, CashPolarityCode, InvoiceType, (InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue) AS UnpaidValue
FROM            Invoice.vwRegister
WHERE        (InvoiceTypeCode < 2);
go
PRINT N'Creating View [Invoice].[vwRegisterPurchaseProjects]...';


go
CREATE VIEW Invoice.vwRegisterPurchaseProjects
AS
	SELECT        StartOn, InvoiceNumber, ProjectCode, CashCode, CashDescription, TaxCode, TaxDescription, SubjectCode, InvoiceTypeCode, InvoiceStatusCode, InvoicedOn, InvoiceValue, TaxValue, PaymentTerms, Printed, SubjectName, 
							 UserName, InvoiceStatus, CashPolarityCode, InvoiceType
	FROM            Invoice.vwRegisterDetail
	WHERE        (InvoiceTypeCode > 1);
go
PRINT N'Creating View [Invoice].[vwRegisterSaleProjects]...';


go
CREATE VIEW Invoice.vwRegisterSaleProjects
AS
	SELECT        StartOn, InvoiceNumber, ProjectCode, CashCode, CashDescription, TaxCode, TaxDescription, SubjectCode, InvoiceTypeCode, InvoiceStatusCode, InvoicedOn, InvoiceValue, TaxValue,
							 PaymentTerms, Printed, SubjectName, UserName, InvoiceStatus, CashPolarityCode, InvoiceType
	FROM            Invoice.vwRegisterDetail
	WHERE        (InvoiceTypeCode < 2);
go
PRINT N'Creating View [Invoice].[vwHistoryPurchaseItems]...';


go
CREATE VIEW Invoice.vwHistoryPurchaseItems
AS
	SELECT        CONCAT(App.tbMonth.MonthName, SPACE(1), YEAR(App.tbYearPeriod.StartOn)) AS PeriodName, App.tbYearPeriod.YearNumber, Invoice.vwRegisterDetail.StartOn, Invoice.vwRegisterDetail.InvoiceNumber, 
							 Invoice.vwRegisterDetail.ProjectCode, Invoice.vwRegisterDetail.CashCode, Invoice.vwRegisterDetail.CashDescription, Invoice.vwRegisterDetail.TaxCode, Invoice.vwRegisterDetail.TaxDescription, 
							 Invoice.vwRegisterDetail.SubjectCode, Invoice.vwRegisterDetail.InvoiceTypeCode, Invoice.vwRegisterDetail.InvoiceStatusCode, Invoice.vwRegisterDetail.InvoicedOn, Invoice.vwRegisterDetail.InvoiceValue, 
							 Invoice.vwRegisterDetail.TaxValue, Invoice.vwRegisterDetail.PaymentTerms, Invoice.vwRegisterDetail.Printed, 
							 Invoice.vwRegisterDetail.SubjectName, Invoice.vwRegisterDetail.UserName, Invoice.vwRegisterDetail.InvoiceStatus, Invoice.vwRegisterDetail.CashPolarityCode, Invoice.vwRegisterDetail.InvoiceType
	FROM            Invoice.vwRegisterDetail INNER JOIN
							 App.tbYearPeriod ON Invoice.vwRegisterDetail.StartOn = App.tbYearPeriod.StartOn INNER JOIN
							 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
							 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
	WHERE        (Invoice.vwRegisterDetail.InvoiceTypeCode > 1);
go
PRINT N'Creating View [Invoice].[vwHistorySales]...';


go
CREATE   VIEW Invoice.vwHistorySales
AS
SELECT        App.tbYearPeriod.YearNumber, CONCAT(App.tbMonth.MonthName, SPACE(1), YEAR(App.tbYearPeriod.StartOn)) AS PeriodName, Invoice.vwRegister.StartOn, 
                         Invoice.vwRegister.InvoiceNumber, Invoice.vwRegister.SubjectCode, Invoice.vwRegister.InvoiceTypeCode, Invoice.vwRegister.InvoiceStatusCode, Invoice.vwRegister.InvoicedOn, 
                         Invoice.vwRegister.InvoiceValue, Invoice.vwRegister.TaxValue, Invoice.vwRegister.PaidValue, Invoice.vwRegister.PaidTaxValue, Invoice.vwRegister.PaymentTerms, Invoice.vwRegister.Notes, 
                         Invoice.vwRegister.Printed, Invoice.vwRegister.SubjectName, Invoice.vwRegister.UserName, Invoice.vwRegister.InvoiceStatus, Invoice.vwRegister.CashPolarityCode, Invoice.vwRegister.InvoiceType, 
                         (Invoice.vwRegister.InvoiceValue + Invoice.vwRegister.TaxValue) - (Invoice.vwRegister.PaidValue + Invoice.vwRegister.PaidTaxValue) AS UnpaidValue
FROM            App.tbYearPeriod INNER JOIN
                         App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
                         App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
                         Invoice.vwRegister ON App.tbYearPeriod.StartOn = Invoice.vwRegister.StartOn
WHERE        (Invoice.vwRegister.InvoiceTypeCode < 2);
go
PRINT N'Creating View [Invoice].[vwHistoryPurchases]...';


go
CREATE   VIEW Invoice.vwHistoryPurchases
AS
SELECT        App.tbYearPeriod.YearNumber, App.tbYear.Description, CONCAT(App.tbMonth.MonthName, SPACE(1), YEAR(App.tbYearPeriod.StartOn)) AS PeriodName, Invoice.vwRegister.StartOn, 
                         Invoice.vwRegister.InvoiceNumber, Invoice.vwRegister.SubjectCode, Invoice.vwRegister.InvoiceTypeCode, Invoice.vwRegister.InvoiceStatusCode, Invoice.vwRegister.InvoicedOn, 
                         Invoice.vwRegister.InvoiceValue, Invoice.vwRegister.TaxValue, Invoice.vwRegister.PaidValue, Invoice.vwRegister.PaidTaxValue, Invoice.vwRegister.PaymentTerms, Invoice.vwRegister.Notes, 
                         Invoice.vwRegister.Printed, Invoice.vwRegister.SubjectName, Invoice.vwRegister.UserName, Invoice.vwRegister.InvoiceStatus, Invoice.vwRegister.CashPolarityCode, Invoice.vwRegister.InvoiceType, 
                         (Invoice.vwRegister.InvoiceValue + Invoice.vwRegister.TaxValue) - (Invoice.vwRegister.PaidValue + Invoice.vwRegister.PaidTaxValue) AS UnpaidValue
FROM            App.tbYearPeriod INNER JOIN
                         App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
                         App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
                         Invoice.vwRegister ON App.tbYearPeriod.StartOn = Invoice.vwRegister.StartOn
WHERE        (Invoice.vwRegister.InvoiceTypeCode > 1);
go
PRINT N'Creating View [Cash].[vwBankCashCodes]...';


go
CREATE VIEW Cash.vwBankCashCodes
AS
	SELECT        Cash.tbCode.CashCode, Cash.tbCode.CashDescription, Cash.tbCode.TaxCode, Cash.tbCategory.CashPolarityCode
	FROM            Cash.tbCode INNER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode LEFT OUTER JOIN
							 Cash.vwTransferCodeLookup ON Cash.tbCode.CashCode = Cash.vwTransferCodeLookup.CashCode
	WHERE        (Cash.tbCategory.CashTypeCode = 2) AND (Cash.vwTransferCodeLookup.CashCode IS NULL)
go
PRINT N'Creating View [Cash].[vwTaxCorpTotalsByPeriod]...';


go
CREATE VIEW Cash.vwTaxCorpTotalsByPeriod
AS
	WITH invoiced_Projects AS
	(
		SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod WHERE (StartOn <= Invoice.tbInvoice.InvoicedOn) ORDER BY StartOn DESC) AS StartOn,  
								 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbProject.InvoiceValue * - 1 ELSE Invoice.tbProject.InvoiceValue END AS InvoiceValue
		FROM            Invoice.tbProject INNER JOIN
								 App.vwCorpTaxCashCodes CashCodes  ON Invoice.tbProject.CashCode = CashCodes.CashCode INNER JOIN
								 Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
								 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
		WHERE CashTypeCode < 3
	), invoiced_items AS
	(
		SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod WHERE (StartOn <= Invoice.tbInvoice.InvoicedOn) ORDER BY StartOn DESC) AS StartOn,  
							  CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbItem.InvoiceValue * - 1 ELSE Invoice.tbItem.InvoiceValue END AS InvoiceValue
		FROM         Invoice.tbItem INNER JOIN
							  App.vwCorpTaxCashCodes CashCodes ON Invoice.tbItem.CashCode = CashCodes.CashCode INNER JOIN
							  Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
							  Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
		WHERE CashTypeCode < 3
	), assets AS
	(
		SELECT cash_codes.CashCode, financial_periods.StartOn, 
			CASE cash_codes.CashPolarityCode WHEN 0 THEN financial_periods.InvoiceValue * -1 ELSE financial_periods.InvoiceValue END AssetValue
		FROM App.vwCorpTaxCashCodes cash_codes
			JOIN Cash.tbPeriod financial_periods
				ON cash_codes.CashCode = financial_periods.CashCode
		WHERE cash_codes.CashTypeCode = 2
	), netprofits AS	
	(
		SELECT StartOn, SUM(InvoiceValue) NetProfit 
		FROM invoiced_Projects 
		GROUP BY StartOn
		
		UNION
		
		SELECT StartOn, SUM(InvoiceValue) NetProfit 
		FROM invoiced_items 
		GROUP BY StartOn

		UNION

		SELECT StartOn, SUM(AssetValue) NetProfit
		FROM assets
		GROUP BY StartOn
	)
	, netprofit_consolidated AS
	(
		SELECT StartOn, SUM(NetProfit) AS NetProfit FROM netprofits GROUP BY StartOn
	)
	SELECT App.tbYearPeriod.StartOn, netprofit_consolidated.NetProfit, 
							netprofit_consolidated.NetProfit * App.tbYearPeriod.CorporationTaxRate + App.tbYearPeriod.TaxAdjustment AS CorporationTax, 
							App.tbYearPeriod.TaxAdjustment
	FROM         netprofit_consolidated INNER JOIN
							App.tbYearPeriod ON netprofit_consolidated.StartOn = App.tbYearPeriod.StartOn;
go
PRINT N'Creating View [Cash].[vwFlowVatPeriodAccruals]...';


go
CREATE   VIEW Cash.vwFlowVatPeriodAccruals
AS
	WITH active_periods AS
	(
		SELECT App.tbYear.YearNumber, App.tbYearPeriod.StartOn
		FROM            App.tbYearPeriod INNER JOIN
								 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
								 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
		WHERE        (App.tbYear.CashStatusCode < 3)
	),	 vat_accruals AS
	(
		SELECT   active_periods.YearNumber, active_periods.StartOn, ISNULL(SUM(vat_audit.HomeSales), 0) AS HomeSales, ISNULL(SUM(vat_audit.HomePurchases), 0) AS HomePurchases, ISNULL(SUM(vat_audit.ExportSales), 0) AS ExportSales, ISNULL(SUM(vat_audit.ExportPurchases), 0) 
								 AS ExportPurchases, ISNULL(SUM(vat_audit.HomeSalesVat), 0) AS HomeSalesVat, ISNULL(SUM(vat_audit.HomePurchasesVat), 0) AS HomePurchasesVat, ISNULL(SUM(vat_audit.ExportSalesVat), 0) AS ExportSalesVat, 
								 ISNULL(SUM(vat_audit.ExportPurchasesVat), 0) AS ExportPurchasesVat
		FROM            Cash.vwTaxVatAuditAccruals AS vat_audit RIGHT OUTER JOIN
								 active_periods ON active_periods.StartOn = vat_audit.StartOn
		GROUP BY active_periods.YearNumber, active_periods.StartOn
	)
	SELECT YearNumber, StartOn, CAST(HomeSales AS decimal(18,5)) HomeSales, CAST(HomePurchases AS decimal(18,5)) HomePurchases, CAST(ExportSales AS decimal(18,5)) AS ExportSales, 
		CAST(ExportPurchases AS decimal(18,5)) ExportPurchases, CAST(HomeSalesVat AS decimal(18,5)) HomeSalesVat, CAST(HomePurchasesVat AS decimal(18,5)) HomePurchasesVat, 
		CAST(ExportSalesVat AS decimal(18,5)) ExportSalesVat, CAST(ExportPurchasesVat AS decimal(18,5)) ExportPurchasesVat,
		CAST((HomeSalesVat + ExportSalesVat) - (HomePurchasesVat + ExportPurchasesVat) AS decimal(18,5)) VatDue
	FROM vat_accruals;
go
PRINT N'Creating View [Cash].[vwProfitAndLossByPeriod]...';


go
CREATE   VIEW Cash.vwProfitAndLossByPeriod
AS
	SELECT category.CategoryCode, category.Category, category.CashTypeCode, periods.YearNumber, periods.MonthNumber, category.DisplayOrder, financial_year.Description,
		year_month.MonthName, profit_data.StartOn, profit_data.InvoiceValue
	FROM Cash.vwProfitAndLossData profit_data
		JOIN Cash.tbCategory category ON profit_data.CategoryCode = category.CategoryCode
		JOIN App.tbYearPeriod periods ON profit_data.StartOn = periods.StartOn
		JOIN App.tbYear financial_year ON periods.YearNumber = financial_year.YearNumber
		JOIN App.tbMonth year_month ON periods.MonthNumber = year_month.MonthNumber;
go
PRINT N'Creating View [Cash].[vwTaxCorpTotals]...';


go
CREATE VIEW Cash.vwTaxCorpTotals
AS
	WITH totals AS
	(
		SELECT App.tbYearPeriod.YearNumber, netprofit_totals.StartOn, YEAR(App.tbYearPeriod.StartOn) AS PeriodYear, App.tbYear.Description, 
						  App.tbMonth.MonthName + ' ' + LTRIM(STR(YEAR(App.tbYearPeriod.StartOn))) AS Period, App.tbYearPeriod.CorporationTaxRate, 
						  App.tbYearPeriod.TaxAdjustment, SUM(netprofit_totals.NetProfit) AS NetProfit, SUM(netprofit_totals.CorporationTax) AS CorporationTax
		FROM       Cash.vwTaxCorpTotalsByPeriod  netprofit_totals INNER JOIN
							  App.tbYearPeriod ON netprofit_totals.StartOn = App.tbYearPeriod.StartOn INNER JOIN
							  App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
							  App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
		WHERE     (App.tbYear.CashStatusCode BETWEEN 1 AND 2)
		GROUP BY App.tbYearPeriod.YearNumber, App.tbYear.Description, App.tbMonth.MonthName, netprofit_totals.StartOn, YEAR(App.tbYearPeriod.StartOn), 
							  App.tbYearPeriod.CorporationTaxRate, App.tbYearPeriod.TaxAdjustment
	)
	SELECT YearNumber, StartOn, PeriodYear, Description, Period, CorporationTaxRate, TaxAdjustment, CAST(NetProfit AS decimal(18, 5)) NetProfit, CAST(CorporationTax AS decimal(18, 5)) CorporationTax
	FROM totals;
go
PRINT N'Creating View [Cash].[vwFlowCategoryByYear]...';


go
CREATE   VIEW Cash.vwFlowCategoryByYear
AS
	SELECT CategoryCode, CashCode, YearNumber, SUM(InvoiceValue) InvoiceValue
	FROM Cash.vwFlowCategoryByPeriod
	GROUP BY CategoryCode, CashCode, YearNumber
go
PRINT N'Creating View [Cash].[vwTaxVatSummary]...';


go
CREATE VIEW Cash.vwTaxVatSummary
AS
	WITH vat_transactions AS
	(	
		SELECT  (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= Invoice.tbInvoice.InvoicedOn) ORDER BY p.StartOn DESC) AS StartOn,  
				Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbItem.TaxCode, Invoice.tbItem.InvoiceValue, 
								 Invoice.tbItem.TaxValue, Subject.tbSubject.EUJurisdiction, Invoice.tbItem.CashCode AS IdentityCode
		FROM   App.vwVatTaxCashCodes cash_codes INNER JOIN  Invoice.tbItem ON cash_codes.CashCode = Invoice.tbItem.CashCode 
				INNER JOIN Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
								 Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
								 App.tbTaxCode ON Invoice.tbItem.TaxCode = App.tbTaxCode.TaxCode
		WHERE        (App.tbTaxCode.TaxTypeCode = 1)
		UNION
		SELECT  (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= Invoice.tbInvoice.InvoicedOn) ORDER BY p.StartOn DESC) AS StartOn,  
					Invoice.tbProject.InvoiceNumber, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbProject.TaxCode, Invoice.tbProject.InvoiceValue, 
								 Invoice.tbProject.TaxValue, Subject.tbSubject.EUJurisdiction, Invoice.tbProject.ProjectCode AS IdentityCode
		FROM    App.vwVatTaxCashCodes cash_codes INNER JOIN  Invoice.tbProject ON cash_codes.CashCode = Invoice.tbProject.CashCode 
					INNER JOIN Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
								 Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
								 App.tbTaxCode ON Invoice.tbProject.TaxCode = App.tbTaxCode.TaxCode
		WHERE        (App.tbTaxCode.TaxTypeCode = 1)
	), vat_detail AS
	(
		SELECT        StartOn, TaxCode, 
								 CASE WHEN EUJurisdiction = 0 THEN CASE InvoiceTypeCode WHEN 0 THEN InvoiceValue WHEN 1 THEN
								  InvoiceValue * - 1 ELSE 0 END ELSE 0 END AS HomeSales, 
								 CASE WHEN EUJurisdiction = 0 THEN CASE InvoiceTypeCode WHEN 2 THEN InvoiceValue WHEN 3 THEN
								  InvoiceValue * - 1 ELSE 0 END ELSE 0 END AS HomePurchases, 
								 CASE WHEN EUJurisdiction != 0 THEN CASE InvoiceTypeCode WHEN 0 THEN InvoiceValue WHEN 1 THEN
								  InvoiceValue * - 1 ELSE 0 END ELSE 0 END AS ExportSales, 
								 CASE WHEN EUJurisdiction != 0 THEN CASE InvoiceTypeCode WHEN 2 THEN InvoiceValue WHEN 3 THEN
								  InvoiceValue * - 1 ELSE 0 END ELSE 0 END AS ExportPurchases, 
								 CASE WHEN EUJurisdiction = 0 THEN CASE InvoiceTypeCode WHEN 0 THEN TaxValue WHEN 1 THEN TaxValue
								  * - 1 ELSE 0 END ELSE 0 END AS HomeSalesVat, 
								 CASE WHEN EUJurisdiction = 0 THEN CASE InvoiceTypeCode WHEN 2 THEN TaxValue WHEN 3 THEN TaxValue
								  * - 1 ELSE 0 END ELSE 0 END AS HomePurchasesVat, 
								 CASE WHEN EUJurisdiction != 0 THEN CASE InvoiceTypeCode WHEN 0 THEN TaxValue WHEN 1 THEN TaxValue
								  * - 1 ELSE 0 END ELSE 0 END AS ExportSalesVat, 
								 CASE WHEN EUJurisdiction != 0 THEN CASE InvoiceTypeCode WHEN 2 THEN TaxValue WHEN 3 THEN TaxValue
								  * - 1 ELSE 0 END ELSE 0 END AS ExportPurchasesVat
		FROM  vat_transactions
	), vatcode_summary AS
	(
		SELECT        StartOn, TaxCode, SUM(HomeSales) AS HomeSales, SUM(HomePurchases) AS HomePurchases, SUM(ExportSales) AS ExportSales, SUM(ExportPurchases) AS ExportPurchases, SUM(HomeSalesVat) 
								AS HomeSalesVat, SUM(HomePurchasesVat) AS HomePurchasesVat, SUM(ExportSalesVat) AS ExportSalesVat, SUM(ExportPurchasesVat) AS ExportPurchasesVat
		FROM            vat_detail
		GROUP BY StartOn, TaxCode
	)
	SELECT   StartOn, 
		TaxCode, CAST(HomeSales as float) HomeSales, CAST(HomePurchases as float) HomePurchases, CAST(ExportSales as float) ExportSales, CAST(ExportPurchases as float) ExportPurchases, 
		CAST(HomeSalesVat as float) HomeSalesVat, CAST(HomePurchasesVat as float) HomePurchasesVat, CAST(ExportSalesVat as float) ExportSalesVat, CAST(ExportPurchasesVat as float) ExportPurchasesVat,
		CAST((HomeSalesVat + ExportSalesVat) - (HomePurchasesVat + ExportPurchasesVat) as float) VatDue
	FROM vatcode_summary;
go
PRINT N'Creating View [Cash].[vwAccountStatement]...';


go
CREATE VIEW Cash.vwAccountStatement
AS
	WITH entries AS
	(
		SELECT  payment.AccountCode, payment.CashCode, ROW_NUMBER() OVER (PARTITION BY payment.AccountCode ORDER BY PaidOn) AS EntryNumber, PaymentCode, PaidOn, 
			CASE WHEN PaidInValue > 0 THEN PaidInValue ELSE PaidOutValue * - 1 END AS Paid
		FROM         Cash.tbPayment payment INNER JOIN Subject.tbAccount ON payment.AccountCode = Subject.tbAccount.AccountCode
		WHERE     (PaymentStatusCode = 1) AND (AccountClosed = 0)	
		UNION
		SELECT        
			AccountCode, 
			COALESCE(CashCode, (SELECT TOP 1 CashCode FROM Cash.vwBankCashCodes WHERE CashPolarityCode = 2)) CashCode,
			0 AS EntryNumber, 
			(SELECT CAST(Message AS NVARCHAR(30)) FROM App.tbText WHERE TextId = 3005) AS PaymentCode, 
			DATEADD(HOUR, - 1, (SELECT MIN(PaidOn) FROM Cash.tbPayment WHERE AccountCode = cash_account.AccountCode)) AS PaidOn, OpeningBalance AS Paid
		FROM            Subject.tbAccount cash_account 								 
		WHERE        (AccountClosed = 0) 
	), running_balance AS
	(
		SELECT AccountCode, CashCode, EntryNumber, PaymentCode, PaidOn, 
			SUM(Paid) OVER (PARTITION BY AccountCode ORDER BY EntryNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS PaidBalance
		FROM entries
	), payments AS
	(
		SELECT     Cash.tbPayment.PaymentCode, Cash.tbPayment.AccountCode, Usr.tbUser.UserName, Cash.tbPayment.SubjectCode, 
							  Subject.tbSubject.SubjectName, Cash.tbPayment.CashCode, Cash.tbCode.CashDescription, App.tbTaxCode.TaxDescription, 
							  Cash.tbPayment.PaidInValue, Cash.tbPayment.PaidOutValue, 
							  Cash.tbPayment.PaymentReference, Cash.tbPayment.InsertedBy, Cash.tbPayment.InsertedOn, 
							  Cash.tbPayment.UpdatedBy, Cash.tbPayment.UpdatedOn, Cash.tbPayment.TaxCode
		FROM         Cash.tbPayment INNER JOIN
							  Usr.tbUser ON Cash.tbPayment.UserId = Usr.tbUser.UserId INNER JOIN
							  Subject.tbSubject ON Cash.tbPayment.SubjectCode = Subject.tbSubject.SubjectCode LEFT OUTER JOIN
							  App.tbTaxCode ON Cash.tbPayment.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
							  Cash.tbCode ON Cash.tbPayment.CashCode = Cash.tbCode.CashCode
	)
		SELECT running_balance.AccountCode, 
			COALESCE((SELECT TOP 1 StartOn FROM App.tbYearPeriod WHERE (StartOn <= running_balance.PaidOn) ORDER BY StartOn DESC), 
				(SELECT MIN(StartOn) FROM App.tbYearPeriod) ) AS StartOn, 
			running_balance.EntryNumber, running_balance.PaymentCode, running_balance.PaidOn, 
			payments.SubjectName, payments.PaymentReference, COALESCE(payments.PaidInValue, 0) PaidInValue, 
			COALESCE(payments.PaidOutValue, 0) PaidOutValue, CAST(running_balance.PaidBalance as decimal(18,5)) PaidBalance, 
			payments.CashCode, payments.CashDescription, payments.TaxDescription, payments.UserName, 
			payments.SubjectCode, payments.TaxCode
		FROM   running_balance LEFT OUTER JOIN
								payments ON running_balance.PaymentCode = payments.PaymentCode
go
PRINT N'Creating View [Cash].[vwAccountStatementListing]...';


go
CREATE VIEW Cash.vwAccountStatementListing
AS
	SELECT        App.tbYear.YearNumber, Subject.tbSubject.SubjectName AS Bank, Subject.tbAccount.AccountCode, Subject.tbAccount.AccountName, Subject.tbAccount.SortCode, Subject.tbAccount.AccountNumber, CONCAT(App.tbYear.Description, SPACE(1), 
							 App.tbMonth.MonthName) AS PeriodName, Cash.vwAccountStatement.StartOn, CAST(Cash.vwAccountStatement.EntryNumber AS INT) EntryNumber, Cash.vwAccountStatement.PaymentCode, Cash.vwAccountStatement.PaidOn, 
							 Cash.vwAccountStatement.SubjectName, Cash.vwAccountStatement.PaymentReference, Cash.vwAccountStatement.PaidInValue, Cash.vwAccountStatement.PaidOutValue, 
							 Cash.vwAccountStatement.PaidBalance, Cash.vwAccountStatement.CashCode, 
							 Cash.vwAccountStatement.CashDescription, Cash.vwAccountStatement.TaxDescription, Cash.vwAccountStatement.UserName, Cash.vwAccountStatement.SubjectCode, 
							 Cash.vwAccountStatement.TaxCode
	FROM            App.tbYearPeriod INNER JOIN
							 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
							 Cash.vwAccountStatement INNER JOIN
							 Subject.tbAccount ON Cash.vwAccountStatement.AccountCode = Subject.tbAccount.AccountCode INNER JOIN
							 Subject.tbSubject ON Subject.tbAccount.SubjectCode = Subject.tbSubject.SubjectCode ON App.tbYearPeriod.StartOn = Cash.vwAccountStatement.StartOn INNER JOIN
							 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber;
go
PRINT N'Creating View [Cash].[vwBalanceSheetAssets]...';


go
CREATE VIEW Cash.vwBalanceSheetAssets
AS
	WITH asset_statements AS
	(
		SELECT account_statement.AccountCode, COALESCE(StartOn, (SELECT MIN(StartOn) FROM App.tbYearPeriod)) StartOn, EntryNumber, PaidBalance
		FROM Cash.vwAccountStatement account_statement
			JOIN Subject.tbAccount account ON account_statement.AccountCode = account.AccountCode
		WHERE account.AccountTypeCode = 2 AND account.AccountClosed = 0 
	), asset_last_tx AS
	(
		SELECT AccountCode, MAX(EntryNumber) EntryNumber
		FROM asset_statements
		GROUP BY AccountCode, StartOn
	)
	, asset_polarity AS
	(
		SELECT asset_statements.AccountCode, asset_statements.StartOn, SUM(asset_statements.PaidBalance) Balance, CAST(1 as bit) IsEntry
		FROM asset_statements
			JOIN asset_last_tx ON asset_statements.AccountCode = asset_last_tx.AccountCode AND asset_statements.EntryNumber = asset_last_tx.EntryNumber
		GROUP BY asset_statements.AccountCode, asset_statements.StartOn
	), asset_periods AS
	(
		SELECT AccountCode, StartOn,  0 Balance, CAST(0 as bit) IsEntry
		FROM App.tbYearPeriod year_periods
			CROSS JOIN Subject.tbAccount account
		WHERE account.AccountTypeCode = 2 AND account.AccountClosed = 0
	), asset_unordered AS
	(
		SELECT asset_periods.AccountCode, asset_periods.StartOn,
			CASE WHEN asset_polarity.AccountCode IS NULL THEN asset_periods.Balance ELSE asset_polarity.Balance END Balance,
			CASE WHEN asset_polarity.AccountCode IS NULL THEN asset_periods.IsEntry ELSE asset_polarity.IsEntry END IsEntry
		FROM asset_periods
			LEFT OUTER JOIN asset_polarity
				ON asset_periods.AccountCode = asset_polarity.AccountCode
					AND asset_periods.StartOn = asset_polarity.StartOn
	), asset_ordered AS
	(
		SELECT 
			ROW_NUMBER() OVER (ORDER BY AccountCode, StartOn) EntryNumber,
			AccountCode, StartOn, Balance, IsEntry
		FROM asset_unordered
	)
	, asset_ranked AS
	(
		SELECT *, 
		RANK() OVER (PARTITION BY AccountCode, IsEntry ORDER BY EntryNumber) RNK
		FROM asset_ordered
	)
	, asset_grouped AS
	(
		SELECT EntryNumber, AccountCode, StartOn, Balance, IsEntry,
		MAX(CASE IsEntry WHEN 0 THEN 0 ELSE RNK END) OVER (PARTITION BY AccountCode ORDER BY EntryNumber) RNK
		FROM asset_ranked
	), asset_base AS
	(
		SELECT EntryNumber, AccountCode, StartOn, IsEntry,
			CASE IsEntry WHEN 0 THEN
				MAX(Balance) OVER (PARTITION BY AccountCode, RNK ORDER BY EntryNumber) +
				MIN(Balance) OVER (PARTITION BY AccountCode, RNK ORDER BY EntryNumber) 
			ELSE
				Balance
			END AS Balance
		FROM asset_grouped
	), asset_accounts AS
	(
		SELECT AccountCode, AccountName, CashPolarityCode
		FROM Subject.tbAccount accounts
			JOIN Cash.tbCode cash_code ON accounts.CashCode = cash_code.CashCode
			JOIN Cash.tbCategory cash_category ON cash_code.CategoryCode = cash_category.CategoryCode
		WHERE AccountTypeCode = 2 AND AccountClosed = 0
	)
	SELECT asset_accounts.AccountCode AssetCode, AccountName AssetName, CashPolarityCode, 4 AssetTypeCode, StartOn, Balance
	FROM asset_base
		JOIN asset_accounts ON asset_base.AccountCode = asset_accounts.AccountCode;
go
PRINT N'Creating View [App].[vwGraphBankBalance]...';


go
CREATE VIEW App.vwGraphBankBalance
AS
	WITH last_entries AS
	(
		SELECT     AccountCode, StartOn, MAX(EntryNumber) AS LastEntry
		FROM         Cash.vwAccountStatement
		GROUP BY AccountCode, StartOn
		HAVING      (NOT (StartOn IS NULL))
	), closing_balance AS
	(
		SELECT        Subject.tbAccount.AccountCode, Subject.tbAccount.CashCode, last_entries.StartOn, SUM(Cash.vwAccountStatement.PaidBalance) AS ClosingBalance
		FROM            last_entries INNER JOIN
								 Cash.vwAccountStatement ON last_entries.AccountCode = Cash.vwAccountStatement.AccountCode AND 
								 last_entries.StartOn = Cash.vwAccountStatement.StartOn AND 
								 last_entries.LastEntry = Cash.vwAccountStatement.EntryNumber INNER JOIN
								 Subject.tbAccount ON last_entries.AccountCode = Subject.tbAccount.AccountCode
		WHERE Subject.tbAccount.AccountTypeCode = 0
		GROUP BY Subject.tbAccount.AccountCode, Subject.tbAccount.CashCode, last_entries.StartOn
	)
	SELECT        Format(closing_balance.StartOn, 'yyyy-MM') AS PeriodOn, SUM(closing_balance.ClosingBalance) AS SumOfClosingBalance
	FROM            closing_balance INNER JOIN
							 Cash.tbCode ON closing_balance.CashCode = Cash.tbCode.CashCode
	WHERE        (closing_balance.StartOn > DATEADD(m, - 6, CURRENT_TIMESTAMP))
	GROUP BY Format(closing_balance.StartOn, 'yyyy-MM');
go
PRINT N'Creating View [Subject].[vwAssetStatement]...';


go
CREATE VIEW Subject.vwAssetStatement
AS
	SELECT (SELECT TOP 1 StartOn FROM App.tbYearPeriod	WHERE (StartOn <= TransactedOn) ORDER BY StartOn DESC) AS StartOn, *
	FROM Subject.vwStatement
go
PRINT N'Creating View [Subject].[vwAssetBalances]...';


go
CREATE VIEW Subject.vwAssetBalances
AS
	WITH financial_periods AS
	(
		SELECT pd.StartOn
		FROM App.tbYear yr
			JOIN App.tbYearPeriod pd ON yr.YearNumber = pd.YearNumber
		WHERE (yr.CashStatusCode BETWEEN 1 AND 2)
	), Subject_periods AS
	(
		SELECT SubjectCode, StartOn
		FROM Subject.tbSubject Subjects
			CROSS JOIN financial_periods	
	)
	, Subject_statements AS
	(
		SELECT StartOn, 
			SubjectCode, os.RowNumber, TransactedOn, Balance,
			MAX(RowNumber) OVER (PARTITION BY SubjectCode, StartOn ORDER BY StartOn) LastRowNumber 
		FROM Subject.vwAssetStatement os
		WHERE TransactedOn >= (SELECT StartOn FROM Cash.vwBalanceStartOn)
	)
	, Subject_balances AS
	(
		SELECT SubjectCode, StartOn, Balance
		FROM Subject_statements
		WHERE RowNumber = LastRowNumber
	)
	, Subject_ordered AS
	(
		SELECT ROW_NUMBER() OVER (ORDER BY Subject_periods.SubjectCode, Subject_periods.StartOn) EntryNumber,
			Subject_periods.SubjectCode, Subject_periods.StartOn, 
			COALESCE(Balance, 0) Balance,
			CASE WHEN Subject_balances.StartOn IS NULL THEN 0 ELSE 1 END IsEntry
		FROM Subject_periods
			LEFT OUTER JOIN Subject_balances 
				ON Subject_periods.SubjectCode = Subject_balances.SubjectCode AND Subject_periods.StartOn = Subject_balances.StartOn
	), Subject_ranked AS
	(
		SELECT *,
			RANK() OVER (PARTITION BY SubjectCode, IsEntry ORDER BY EntryNumber) RNK
		FROM Subject_ordered
	), Subject_grouped AS
	(
		SELECT EntryNumber, SubjectCode, StartOn, IsEntry, Balance,
			MAX(CASE IsEntry WHEN 0 THEN 0 ELSE RNK END) OVER (PARTITION BY SubjectCode ORDER BY EntryNumber) RNK
		FROM Subject_ranked
	)
	, Subject_projected AS
	(
		SELECT EntryNumber, SubjectCode, StartOn, IsEntry,
			CASE IsEntry WHEN 0 THEN
				MAX(Balance) OVER (PARTITION BY SubjectCode, RNK ORDER BY EntryNumber) +
				MIN(Balance) OVER (PARTITION BY SubjectCode, RNK ORDER BY EntryNumber) 
			ELSE
				Balance
			END
			AS Balance
		FROM Subject_grouped	
	), Subject_entries AS
	(
		SELECT SubjectCode, EntryNumber, StartOn, Balance * -1 AS Balance,
			CASE 
				WHEN Balance < 0 THEN 0 
				ELSE 1
			END AS AssetTypeCode, 
			CASE WHEN Balance <> 0 THEN 1 ELSE IsEntry END AS IsEntry
		FROM Subject_projected
	)
	SELECT SubjectCode, StartOn, Balance, 
		CASE 
			WHEN Balance <> 0 THEN AssetTypeCode 
			ELSE
				COALESCE(LAG(AssetTypeCode) OVER (PARTITION BY SubjectCode ORDER BY EntryNumber), 0)
		END AssetTypeCode
	FROM Subject_entries WHERE IsEntry = 1;
go
PRINT N'Creating View [Subject].[vwCurrentBalance]...';


go
CREATE   VIEW Subject.vwCurrentBalance
AS
	WITH current_balance AS
	(
		SELECT SubjectCode, MAX(RowNumber) CurrentBalanceRow
		FROM Subject.vwStatement
		GROUP BY SubjectCode
	)
	SELECT Subject_statement.SubjectCode, Subject_statement.Balance
	FROM Subject.vwStatement Subject_statement
		JOIN current_balance ON Subject_statement.SubjectCode = current_balance.SubjectCode 
			AND Subject_statement.RowNumber = current_balance.CurrentBalanceRow
go
PRINT N'Creating View [Subject].[vwStatementReport]...';


go
CREATE   VIEW Subject.vwStatementReport
AS
	SELECT  asset.SubjectCode, o.SubjectName, asset.RowNumber, App.tbYear.YearNumber, App.tbYear.Description, App.tbMonth.MonthName, asset.StartOn, asset.TransactedOn, asset.Reference, asset.StatementType, asset.Charge, asset.Balance
	FROM            Subject.vwAssetStatement AS asset INNER JOIN
							 Subject.tbSubject AS o ON o.SubjectCode = asset.SubjectCode INNER JOIN
							 App.tbYearPeriod ON asset.StartOn = App.tbYearPeriod.StartOn INNER JOIN
							 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND 
							 App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
							 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber;
go
PRINT N'Creating View [Subject].[vwAssetStatementAudit]...';


go
CREATE   VIEW Subject.vwAssetStatementAudit
AS
	SELECT App.tbYear.YearNumber, App.tbYear.Description, App.tbMonth.MonthName, App.tbYearPeriod.StartOn, asset_statement.SubjectCode, account.SubjectName, asset_statement.RowNumber, asset_statement.TransactedOn, asset_statement.Charge, asset_statement.Balance
	FROM  Subject.vwAssetStatement AS asset_statement INNER JOIN
			Subject.tbSubject AS account ON asset_statement.SubjectCode = account.SubjectCode INNER JOIN
			App.tbYearPeriod ON asset_statement.StartOn = App.tbYearPeriod.StartOn INNER JOIN
			App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
			App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber;
go
PRINT N'Creating View [Project].[vwCostSetProjects]...';


go
CREATE   VIEW Project.vwCostSetProjects
AS
	WITH Project_flow AS
	(
		SELECT child.ParentProjectCode, child.ChildProjectCode
		FROM Project.tbFlow child 
			JOIN Project.vwCostSet cost_set ON child.ParentProjectCode = cost_set.ProjectCode
			JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode

		UNION ALL

		SELECT child.ParentProjectCode, child.ChildProjectCode
		FROM Project.tbFlow child 
			JOIN Project_flow parent ON child.ParentProjectCode = parent.ChildProjectCode
			JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode
	)
	SELECT ProjectCode FROM Project.vwCostSet
	UNION
	SELECT quote.ProjectCode
	FROM Project.tbProject quote 
		JOIN Project_flow ON Project_flow.ChildProjectCode = quote.ProjectCode
		JOIN Cash.tbCode cash_code ON quote.CashCode = cash_code.CashCode
	WHERE quote.ProjectStatusCode = 0;
go
PRINT N'Creating View [Invoice].[vwStatusLive]...';


go
CREATE VIEW Invoice.vwStatusLive
AS
	WITH nonzero_balance_Subjects AS
	(
		SELECT SubjectCode, ABS(Balance) Balance, CASE WHEN Balance > 0 THEN 0 ELSE 1 END CashPolarityCode 
		FROM Subject.vwCurrentBalance
	)
	, paid_invoices AS
	(
		SELECT SubjectCode, InvoiceNumber, 3 InvoiceStatusCode, TotalPaid, TaxRate
		FROM nonzero_balance_Subjects
			CROSS APPLY
				(
					SELECT InvoiceNumber,
						(InvoiceValue + TaxValue) TotalPaid,
						TaxValue / CASE InvoiceValue WHEN 0 THEN 1 ELSE InvoiceValue END TaxRate
					FROM Invoice.tbInvoice invoices
						INNER JOIN Invoice.tbType ON invoices.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
					WHERE (SubjectCode = nonzero_balance_Subjects.SubjectCode 
							AND Invoice.tbType.CashPolarityCode <> nonzero_balance_Subjects.CashPolarityCode)
				) invoices
	), candidates_invoices AS
	(
		SELECT SubjectCode, NULL InvoiceNumber, 0 RowNumber, Balance TotalCharge, 0 TaxRate
		FROM nonzero_balance_Subjects
		UNION
		SELECT SubjectCode, InvoiceNumber, RowNumber, TotalCharge, TaxRate
		FROM nonzero_balance_Subjects
			CROSS APPLY
				(
					SELECT InvoiceNumber, ROW_NUMBER() OVER (ORDER BY InvoicedOn DESC) RowNumber,
							(InvoiceValue + TaxValue) * - 1  TotalCharge,
							TaxValue / CASE InvoiceValue WHEN 0 THEN 1 ELSE InvoiceValue END TaxRate
					FROM Invoice.tbInvoice invoices
						INNER JOIN Invoice.tbType ON invoices.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
					WHERE SubjectCode = nonzero_balance_Subjects.SubjectCode 
						AND Invoice.tbType.CashPolarityCode = nonzero_balance_Subjects.CashPolarityCode
				) invoices
	)
	, candidate_balance AS
	(
		SELECT SubjectCode, InvoiceNumber, TotalCharge, TaxRate,
			CAST(SUM(TotalCharge) OVER (PARTITION BY SubjectCode ORDER BY RowNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS float) AS Balance
		FROM candidates_invoices
	), candidate_status AS
	(
		SELECT SubjectCode, InvoiceNumber,
			CASE 
				WHEN Balance >= 0 THEN 1 ELSE
				CASE WHEN TotalCharge < Balance THEN 2 ELSE 3 END
			END InvoiceStatusCode,
			CASE 
				WHEN Balance >= 0 THEN 0 ELSE
				CASE WHEN TotalCharge < Balance THEN ABS(Balance) ELSE ABS(TotalCharge) END
			END TotalPaid,
			TaxRate
		FROM candidate_balance
	), invoice_status AS
	(
		SELECT SubjectCode, InvoiceNumber, InvoiceStatusCode, TotalPaid, TaxRate 
		FROM paid_invoices
		UNION
		SELECT SubjectCode, InvoiceNumber, InvoiceStatusCode, TotalPaid, TaxRate 
		FROM candidate_status 
		WHERE NOT (InvoiceNumber IS NULL)
	)
	SELECT SubjectCode, InvoiceNumber, InvoiceStatusCode, 
		TotalPaid / (1 + TaxRate) PaidValue,
		TotalPaid - (TotalPaid / (1 + TaxRate)) PaidTaxValue
	FROM invoice_status;
go
PRINT N'Creating View [Cash].[vwFlowVatPeriodTotals]...';


go
CREATE VIEW Cash.vwFlowVatPeriodTotals
AS
	WITH active_periods AS
	(
		SELECT App.tbYear.YearNumber, App.tbYearPeriod.StartOn
		FROM            App.tbYearPeriod INNER JOIN
								 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
								 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
		WHERE        (App.tbYear.CashStatusCode < 3)
	)
	SELECT     active_periods.YearNumber, active_periods.StartOn,	
		CAST(ISNULL(SUM(vat.HomeSales), 0) as decimal(18, 5)) AS HomeSales, 
		CAST(ISNULL(SUM(vat.HomePurchases), 0) as decimal(18, 5)) AS HomePurchases, 
		CAST(ISNULL(SUM(vat.ExportSales), 0) as decimal(18, 5)) AS ExportSales, 
		CAST(ISNULL(SUM(vat.ExportPurchases), 0) as decimal(18, 5)) AS ExportPurchases, 
		CAST(ISNULL(SUM(vat.HomeSalesVat), 0) as decimal(18, 5)) AS HomeSalesVat, 
		CAST(ISNULL(SUM(vat.HomePurchasesVat), 0) as decimal(18, 5)) AS HomePurchasesVat, 
		CAST(ISNULL(SUM(vat.ExportSalesVat), 0) as decimal(18, 5)) AS ExportSalesVat, 
		CAST(ISNULL(SUM(vat.ExportPurchasesVat), 0) as decimal(18, 5)) AS ExportPurchasesVat, 
		CAST(ISNULL(SUM(vat.VatDue), 0) as decimal(18, 5)) AS VatDue
	FROM            active_periods LEFT OUTER JOIN
							 Cash.vwTaxVatSummary AS vat ON active_periods.StartOn = vat.StartOn
	GROUP BY active_periods.YearNumber, active_periods.StartOn;
go
PRINT N'Creating View [Cash].[vwTaxVatDetails]...';


go
CREATE VIEW Cash.vwTaxVatDetails
AS
SELECT        App.tbYearPeriod.YearNumber, App.tbYear.Description, CONCAT(App.tbMonth.MonthName, SPACE(1), YEAR(App.tbYearPeriod.StartOn)) AS PeriodName, Cash.vwTaxVatSummary.StartOn, 
                         Cash.vwTaxVatSummary.TaxCode, Cash.vwTaxVatSummary.HomeSales, Cash.vwTaxVatSummary.HomePurchases, Cash.vwTaxVatSummary.ExportSales, Cash.vwTaxVatSummary.ExportPurchases, 
                         Cash.vwTaxVatSummary.HomeSalesVat, Cash.vwTaxVatSummary.HomePurchasesVat, Cash.vwTaxVatSummary.ExportSalesVat, Cash.vwTaxVatSummary.ExportPurchasesVat, Cash.vwTaxVatSummary.VatDue                         
FROM            Cash.vwTaxVatSummary INNER JOIN
                         App.tbYearPeriod INNER JOIN
                         App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber ON Cash.vwTaxVatSummary.StartOn = App.tbYearPeriod.StartOn INNER JOIN
                         App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber;
go
PRINT N'Creating View [Cash].[vwBalanceSheetVat]...';


go
CREATE VIEW Cash.vwBalanceSheetVat
AS
	WITH vat_due AS 
	(	
		SELECT StartOn, SUM(VatDue) AS VatDue
		FROM Cash.vwTaxVatSummary 
		GROUP BY StartOn
	)
	, vat_paid AS
	(
		SELECT vat_due.StartOn, VatDue - VatAdjustment VatDue, 0 VatPaid
		FROM vat_due
			JOIN App.tbYearPeriod year_period ON vat_due.StartOn = year_period.StartOn

		UNION

		SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod WHERE (StartOn <= Cash.tbPayment.PaidOn) ORDER BY StartOn DESC) AS StartOn, 
			0 As VatDue, ( Cash.tbPayment.PaidOutValue * -1) + Cash.tbPayment.PaidInValue AS VatPaid
		FROM Cash.tbPayment 
			JOIN Cash.tbTaxType vat_codes ON Cash.tbPayment.CashCode = vat_codes.CashCode	
		WHERE (vat_codes.TaxTypeCode = 1)
	), vat_unordered AS
	(
		SELECT StartOn, SUM(VatDue) VatDue, SUM(VatPaid) VatPaid
		FROM vat_paid
		GROUP BY StartOn
	), vat_ordered AS
	(
		SELECT ROW_NUMBER() OVER (ORDER BY StartOn, VatDue) AS RowNumber,
			StartOn, VatDue, VatPaid
		FROM vat_unordered
	), vat_balance AS
	(
		SELECT RowNumber, StartOn, VatDue, VatPaid,
			SUM(VatDue+VatPaid) OVER (ORDER BY RowNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
		FROM vat_ordered
	)
	, vat_statement AS
	(
		SELECT RowNumber, StartOn, CAST(VatDue as float) VatDue, CAST(VatPaid as float) VatPaid, CAST(Balance as decimal(18,5)) Balance
		FROM vat_balance
		WHERE StartOn >= (SELECT MIN(StartOn) FROM App.tbYearPeriod p JOIN App.tbYear y ON p.YearNumber = y.YearNumber  WHERE y.CashStatusCode < 3)
	)
	SELECT tax_type.AssetCode, tax_type.AssetName, 
		CAST(0 as smallint) CashPolarityCode,  
		CAST(1 as smallint) AssetTypeCode,  
		StartOn, 
		Balance * -1 Balance 
	FROM vat_statement
		CROSS JOIN
		(
			SELECT UPPER(LEFT(TaxType, 3)) AssetCode, UPPER(TaxType) AssetName
			FROM Cash.tbTaxType
			WHERE TaxTypeCode = 1
		) tax_type;
go
PRINT N'Creating View [Cash].[vwBalanceSheetSubjects]...';


go
CREATE VIEW Cash.vwBalanceSheetSubjects
AS
	WITH asset_balances AS
	(
		SELECT AssetTypeCode, StartOn, SUM(Balance) Balance
		FROM Subject.vwAssetBalances
		GROUP BY AssetTypeCode, StartOn
	)
	SELECT (SELECT AccountCode FROM Cash.vwCurrentAccount) AssetCode, asset_type.AssetType AssetName, 
		asset_type.AssetTypeCode,
		CASE asset_type.AssetTypeCode WHEN 0 THEN 1 ELSE 0 END CashPolarityCode,
		StartOn, Balance
	FROM asset_balances
		JOIN Cash.tbAssetType asset_type ON asset_balances.AssetTypeCode = asset_type.AssetTypeCode;
go
PRINT N'Creating View [Cash].[vwBalanceSheetAccounts]...';


go
CREATE VIEW Cash.vwBalanceSheetAccounts
AS
	WITH cash_accounts AS
	(
		SELECT AccountCode, CashCode 
		FROM Subject.tbAccount
		WHERE AccountTypeCode = 0
	)
	, account_periods AS
	(
		SELECT AccountCode AS AccountCode, CashCode, App.tbYear.YearNumber, App.tbYearPeriod.StartOn
		FROM App.tbYearPeriod 
			JOIN App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
			CROSS JOIN  cash_accounts
		WHERE (App.tbYear.CashStatusCode BETWEEN 1 AND 2)
	), last_entries AS
	(
		SELECT account_statement.AccountCode, account_statement.StartOn, MAX(account_statement.EntryNumber) As EntryNumber
		FROM Cash.vwAccountStatement account_statement 
			JOIN cash_accounts ON account_statement.AccountCode = cash_accounts.AccountCode
		GROUP BY account_statement.AccountCode, account_statement.StartOn
	)
	, closing_balance AS
	(
		SELECT account_statement.AccountCode,  account_statement.StartOn, account_statement.PaidBalance 
		FROM last_entries 
			JOIN Cash.vwAccountStatement account_statement ON last_entries.AccountCode = account_statement.AccountCode
				AND last_entries.EntryNumber = account_statement.EntryNumber
	)
	, statement_ordered AS
	(
		SELECT 
			account_periods.AccountCode, account_periods.CashCode,
			ROW_NUMBER() OVER (PARTITION BY account_periods.AccountCode ORDER BY account_periods.StartOn) EntryNumber,
			account_periods.YearNumber, account_periods.StartOn, CAST(COALESCE(closing_balance.PaidBalance, 0) as float) Balance,
			CASE WHEN closing_balance.AccountCode IS NULL THEN CAST(0 as bit) ELSE CAST(1 as bit) END IsEntry
		FROM account_periods
			LEFT OUTER JOIN closing_balance 
				ON account_periods.AccountCode = closing_balance.AccountCode AND account_periods.StartOn = closing_balance.StartOn
	)
	, statement_ranked AS
	(
		SELECT *,
			RANK() OVER (PARTITION BY AccountCode ORDER BY EntryNumber) RNK
		FROM statement_ordered
	)
	, statement_grouped AS
	(
		SELECT EntryNumber, AccountCode, CashCode, YearNumber, StartOn, Balance, IsEntry,
			MAX(CASE IsEntry WHEN 0 THEN 0 ELSE RNK END) OVER (PARTITION BY AccountCode ORDER BY EntryNumber) RNK
		FROM statement_ranked
	), account_balances AS
	(
		SELECT AccountCode, CashCode, StartOn, 
			CASE IsEntry WHEN 0 THEN
				MAX(Balance) OVER (PARTITION BY AccountCode, RNK ORDER BY EntryNumber) +
				MIN(Balance) OVER (PARTITION BY AccountCode, RNK ORDER BY EntryNumber) 
			ELSE
				Balance
			END
			AS Balance		
		FROM statement_grouped
	), account_polarity AS
	(
		SELECT CashCode, StartOn, SUM(Balance) Balance
		FROM account_balances
		GROUP BY CashCode, StartOn
	), account_base AS
	(
		SELECT 
			CASE WHEN NOT (CashCode IS NULL) 
				THEN (SELECT AccountCode FROM Cash.vwCurrentAccount) 
				ELSE (SELECT AccountCode FROM Cash.vwReserveAccount) 
			END AS AssetCode,
			1 CashPolarityCode,
			CASE WHEN (CashCode IS NULL) THEN 2 ELSE 3 END AssetTypeCode, StartOn, Balance
		FROM account_polarity
	)
	SELECT AssetCode, asset_type.AssetType AssetName, CashPolarityCode, asset_type.AssetTypeCode, StartOn, Balance
	FROM account_base
		JOIN Cash.tbAssetType asset_type ON account_base.AssetTypeCode = asset_type.AssetTypeCode;
go
PRINT N'Creating Function [Cash].[fnChangeKeyPath]...';


go
CREATE   FUNCTION Cash.fnChangeKeyPath (@CoinTypeCode smallint, @HDPath nvarchar(256), @ChangeTypeCode smallint, @AddressIndex int)
RETURNS nvarchar(256)
AS
BEGIN
	DECLARE @KeyPath nvarchar(256) = CONCAT('44', '''', '/', @CoinTypeCode, REPLACE(@HDPath, '/', '''/'), @ChangeTypeCode, '/', @AddressIndex);
	RETURN @KeyPath;
END
go
PRINT N'Creating Function [Cash].[fnKeyNameBalance]...';


go
CREATE   FUNCTION Cash.fnKeyNameBalance(@AccountCode nvarchar(10), @KeyName nvarchar(50))
RETURNS float
AS
BEGIN
	DECLARE @Balance float;

	SELECT @Balance = SUM(COALESCE(change_balance.Balance, 0))
	FROM Subject.tbAccountKey accountKey
		JOIN Cash.tbChange change
			ON accountKey.HDPath = change.HDPath AND accountKey.AccountCode = change.AccountCode
		OUTER APPLY
		(
			SELECT PaymentAddress, SUM(MoneyIn) Balance
			FROM Cash.tbTx tx
			WHERE tx.PaymentAddress = change.PaymentAddress AND tx.TxStatusCode = 1
			GROUP BY PaymentAddress			
		) change_balance
	WHERE accountKey.AccountCode = @AccountCode AND accountKey.KeyName = @KeyName;

	RETURN @Balance;
END
go
PRINT N'Creating Function [App].[fnParsePrimaryKey]...';


go
CREATE   FUNCTION App.fnParsePrimaryKey(@PK NVARCHAR(50)) RETURNS BIT
AS
	BEGIN
		DECLARE @ParseOk BIT = 0;

		SET @ParseOk = CASE		
				WHEN CHARINDEX('"', @PK) > 0 THEN 0	
				WHEN CHARINDEX('''', @PK) > 0 THEN 0	
				WHEN CHARINDEX(',', @PK) > 0 THEN 0	
				WHEN CHARINDEX('<', @PK) > 0 THEN 0	
				WHEN CHARINDEX('>', @PK) > 0 THEN 0	
				WHEN CHARINDEX('@', @PK) > 0 THEN 0	
				WHEN CHARINDEX(':', @PK) > 0 THEN 0	
				WHEN CHARINDEX('*', @PK) > 0 THEN 0	
				WHEN CHARINDEX('', @PK) > 0 THEN 0	
				WHEN CHARINDEX('', @PK) > 0 THEN 0	
				WHEN CHARINDEX('{', @PK) > 0 THEN 0	
				WHEN CHARINDEX('}', @PK) > 0 THEN 0	
				--WHEN CHARINDEX('_', @PK) > 0 THEN 0	
				--WHEN CHARINDEX('&', @PK) > 0 THEN 0	
				--WHEN CHARINDEX('/', @PK) > 0 THEN 0	
				--WHEN CHARINDEX('\', @PK) > 0 THEN 0	
				--WHEN CHARINDEX(' ', @PK) > 0 THEN 0	
				--WHEN CHARINDEX('(', @PK) > 0 THEN 0	
				--WHEN CHARINDEX(')', @PK) > 0 THEN 0	
				ELSE 1 END;

		RETURN @ParseOk;
	END
go
PRINT N'Creating Function [App].[fnDocInvoiceType]...';


go
CREATE   FUNCTION App.fnDocInvoiceType
	(
	@InvoiceTypeCode SMALLINT
	)
RETURNS SMALLINT
AS
	BEGIN
	DECLARE @DocTypeCode SMALLINT
	
	SET @DocTypeCode = CASE @InvoiceTypeCode
		WHEN 0 THEN 4		--sales invoice
		WHEN 1 THEN 5		--credit note
		WHEN 3 THEN 6		--debit note
		ELSE 8				--error
		END
	
	RETURN @DocTypeCode
	END
go
PRINT N'Creating Function [App].[fnWeekDay]...';


go
CREATE   FUNCTION App.fnWeekDay
	(
	@Date datetime
	)
RETURNS smallint
    AS
	BEGIN
	DECLARE @CurrentDay smallint
	SET @CurrentDay = DATEPART(dw, @Date)
	RETURN 	CASE WHEN @CurrentDay > (7 - @@DATEFIRST + 1) THEN
				@CurrentDay - (7 - @@DATEFIRST + 1)
			ELSE
				@CurrentDay + (@@DATEFIRST - 1)
			END
	END
go
PRINT N'Creating Function [App].[fnAdjustDateToBucket]...';


go
CREATE   FUNCTION App.fnAdjustDateToBucket
	(
	@BucketDay smallint,
	@CurrentDate datetime
	)
RETURNS datetime
  AS
	BEGIN
	DECLARE @CurrentDay smallint
	DECLARE @Offset smallint
	DECLARE @AdjustedDay smallint
	
	SET @CurrentDay = DATEPART(dw, @CurrentDate)
	
	SET @AdjustedDay = CASE WHEN @CurrentDay > (7 - @@DATEFIRST + 1) THEN
				@CurrentDay - (7 - @@DATEFIRST + 1)
			ELSE
				@CurrentDay + (@@DATEFIRST - 1)
			END

	SET @Offset = CASE WHEN @BucketDay <= @AdjustedDay THEN
				@BucketDay - @AdjustedDay
			ELSE
				(7 - (@BucketDay - @AdjustedDay)) * -1
			END
	
		
	RETURN DATEADD(dd, @Offset, @CurrentDate)
	END
go
PRINT N'Creating Function [App].[fnVersion]...';


go

CREATE FUNCTION App.fnVersion()
RETURNS NVARCHAR(10)
AS
BEGIN
	DECLARE @Version NVARCHAR(10) = '0.0.0'
	SELECT @Version = VersionString
	FROM App.vwVersion
	RETURN @Version
END
go
PRINT N'Creating Function [Subject].[fnContactFileAs]...';


go
CREATE   FUNCTION Subject.fnContactFileAs(@ContactName nvarchar(100))
RETURNS NVARCHAR(100)
AS
BEGIN
	DECLARE @FileAs nvarchar(100)
		, @FirstNames nvarchar(100)
		, @LastName nvarchar(100)
		, @LastWordPos int;

	IF CHARINDEX(' ', @ContactName) = 0
		SET @FileAs = @ContactName
	ELSE
		BEGIN		
		SET @LastWordPos = CHARINDEX(' ', @ContactName) + 1
		WHILE CHARINDEX(' ', @ContactName, @LastWordPos) != 0
			SET @LastWordPos = CHARINDEX(' ', @ContactName, @LastWordPos) + 1
		
		SET @FirstNames = LEFT(@ContactName, @LastWordPos - 2)
		SET @LastName = RIGHT(@ContactName, LEN(@ContactName) - @LastWordPos + 1)
		SET @FileAs = @LastName + ', ' + @FirstNames
		END

	RETURN @FileAs
END
go
PRINT N'Creating Function [Subject].[fnAccountKeyNamespace]...';


go
CREATE   FUNCTION Subject.fnAccountKeyNamespace
(
	@AccountCode nvarchar(10),
	@HDPath hierarchyid
) RETURNS NVARCHAR(512)
AS
BEGIN
	DECLARE @KeyNamespace nvarchar(512);

	WITH key_namespace AS
	(
		SELECT HDPath, HDPath.GetAncestor(1) Ancestor, CAST(KeyName as nvarchar(512)) KeyNamespace
		FROM Subject.tbAccountKey
		WHERE AccountCode = @AccountCode AND HDPath = @HDPath

		UNION ALL

		SELECT parent_key.HDPath, parent_key.HDPath.GetAncestor(1) Ancestor, CAST(CONCAT(parent_key.KeyName, '.', key_namespace.KeyNamespace) as nvarchar(512)) KeyNamespace
		FROM Subject.tbAccountKey parent_key
			JOIN key_namespace ON parent_key.HDPath = key_namespace.Ancestor
		WHERE AccountCode = @AccountCode
	)
	SELECT @KeyNamespace = REPLACE(UPPER(KeyNamespace), ' ', '_')
	FROM key_namespace

	RETURN @KeyNamespace
END
go
PRINT N'Creating Function [Project].[fnIsExpense]...';


go
CREATE   FUNCTION Project.fnIsExpense
	(
	@ProjectCode nvarchar(20)
	)
RETURNS bit
AS
	BEGIN
	/* An expense is a Project assigned to an outgoing cash code that is not linked to a sale */
	DECLARE @IsExpense bit
	IF EXISTS (SELECT     Project.tbProject.ProjectCode
	           FROM         Project.tbProject INNER JOIN
	                                 Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
	                                 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
	           WHERE     ( Cash.tbCategory.CashPolarityCode = 1) AND ( Project.tbProject.ProjectCode = @ProjectCode))
		SET @IsExpense = 0			          
	ELSE IF EXISTS(SELECT     ParentProjectCode
	          FROM         Project.tbFlow
	          WHERE     (ChildProjectCode = @ProjectCode))
		BEGIN
		DECLARE @ParentProjectCode nvarchar(20)
		SELECT  @ParentProjectCode = ParentProjectCode
		FROM         Project.tbFlow
		WHERE     (ChildProjectCode = @ProjectCode)		
		SET @IsExpense = Project.fnIsExpense(@ParentProjectCode)		
		END	              
	ELSE
		SET @IsExpense = 1
			
	RETURN @IsExpense
	END
go
PRINT N'Creating Function [Project].[fnEmailAddress]...';


go
CREATE   FUNCTION Project.fnEmailAddress
	(
	@ProjectCode nvarchar(20)
	)
RETURNS nvarchar(255)
AS
	BEGIN
	DECLARE @EmailAddress nvarchar(255)

	IF EXISTS(SELECT     Subject.tbContact.EmailAddress
		  FROM         Subject.tbContact INNER JOIN
								tbProject ON Subject.tbContact.SubjectCode = Project.tbProject.SubjectCode AND Subject.tbContact.ContactName = Project.tbProject.ContactName
		  WHERE     ( Project.tbProject.ProjectCode = @ProjectCode)
		  GROUP BY Subject.tbContact.EmailAddress
		  HAVING      (NOT ( Subject.tbContact.EmailAddress IS NULL)))
		BEGIN
		SELECT    @EmailAddress = Subject.tbContact.EmailAddress
		FROM         Subject.tbContact INNER JOIN
							tbProject ON Subject.tbContact.SubjectCode = Project.tbProject.SubjectCode AND Subject.tbContact.ContactName = Project.tbProject.ContactName
		WHERE     ( Project.tbProject.ProjectCode = @ProjectCode)
		GROUP BY Subject.tbContact.EmailAddress
		HAVING      (NOT ( Subject.tbContact.EmailAddress IS NULL))	
		END
	ELSE
		BEGIN
		SELECT    @EmailAddress =  Subject.tbSubject.EmailAddress
		FROM         Subject.tbSubject INNER JOIN
							 Project.tbProject ON Subject.tbSubject.SubjectCode = Project.tbProject.SubjectCode
		WHERE     ( Project.tbProject.ProjectCode = @ProjectCode)
		END
	
	RETURN @EmailAddress
	END
go
PRINT N'Creating View [Invoice].[vwRegisterExpenses]...';


go
CREATE VIEW Invoice.vwRegisterExpenses
 AS
	SELECT     Invoice.vwRegisterProjects.StartOn, Invoice.vwRegisterProjects.InvoiceNumber, Invoice.vwRegisterProjects.ProjectCode, App.tbYearPeriod.YearNumber, 
						  App.tbYear.Description, App.tbMonth.MonthName + ' ' + LTRIM(STR(YEAR( App.tbYearPeriod.StartOn))) AS Period, Invoice.vwRegisterProjects.ProjectTitle, 
						  Invoice.vwRegisterProjects.CashCode, Invoice.vwRegisterProjects.CashDescription, Invoice.vwRegisterProjects.TaxCode, Invoice.vwRegisterProjects.TaxDescription, 
						  Invoice.vwRegisterProjects.SubjectCode, Invoice.vwRegisterProjects.InvoiceTypeCode, Invoice.vwRegisterProjects.InvoiceStatusCode, Invoice.vwRegisterProjects.InvoicedOn, 
						  Invoice.vwRegisterProjects.InvoiceValue, Invoice.vwRegisterProjects.TaxValue, 
						  Invoice.vwRegisterProjects.PaymentTerms, Invoice.vwRegisterProjects.Printed, Invoice.vwRegisterProjects.SubjectName, Invoice.vwRegisterProjects.UserName, 
						  Invoice.vwRegisterProjects.InvoiceStatus, Invoice.vwRegisterProjects.CashPolarityCode, Invoice.vwRegisterProjects.InvoiceType
	FROM         Invoice.vwRegisterProjects INNER JOIN
						  App.tbYearPeriod ON Invoice.vwRegisterProjects.StartOn = App.tbYearPeriod.StartOn INNER JOIN
						  App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
						  App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
	WHERE     (Project.fnIsExpense(Invoice.vwRegisterProjects.ProjectCode) = 1)
go
PRINT N'Creating View [Cash].[vwChangeCollection]...';


go
CREATE   VIEW Cash.vwChangeCollection
AS
	SELECT        change.PaymentAddress, Cash.fnChangeKeyPath(account.CoinTypeCode, account_key.HDPath.ToString(), change.ChangeTypeCode, change.AddressIndex)  FullHDPath
	FROM            Cash.tbChange AS change INNER JOIN
							 Subject.tbAccountKey AS account_key ON change.AccountCode = account_key.AccountCode AND change.HDPath = account_key.HDPath INNER JOIN
							 Subject.tbAccount AS account ON account_key.AccountCode = account.AccountCode
	WHERE        (change.ChangeStatusCode < 2);
go
PRINT N'Creating View [Project].[vwDoc]...';


go
CREATE VIEW Project.vwDoc
AS
	SELECT     Project.fnEmailAddress(Project.tbProject.ProjectCode) AS EmailAddress, Project.tbProject.ProjectCode, Project.tbProject.ProjectStatusCode, Project.tbStatus.ProjectStatus, 
						  Project.tbProject.ContactName, Subject.tbContact.NickName, Usr.tbUser.UserName, Subject.tbSubject.SubjectName, Subject.tbAddress.Address AS InvoiceAddress, 
						  Subject_tb1.SubjectName AS DeliveryAccountName, Subject_tbAddress1.Address AS DeliveryAddress, Subject_tb2.SubjectName AS CollectionAccountName, 
						  Subject_tbAddress2.Address AS CollectionAddress, Project.tbProject.SubjectCode, Project.tbProject.ProjectNotes, Project.tbProject.ObjectCode, Project.tbProject.ActionOn, 
						  Object.tbObject.UnitOfMeasure, Project.tbProject.Quantity, App.tbTaxCode.TaxCode, App.tbTaxCode.TaxRate, Project.tbProject.UnitCharge, Project.tbProject.TotalCharge, 
						  Usr.tbUser.MobileNumber, Usr.tbUser.Signature, Project.tbProject.ProjectTitle, Project.tbProject.PaymentOn, Project.tbProject.SecondReference, Subject.tbSubject.PaymentTerms
	FROM         Subject.tbSubject AS Subject_tb2 RIGHT OUTER JOIN
						  Subject.tbAddress AS Subject_tbAddress2 ON Subject_tb2.SubjectCode = Subject_tbAddress2.SubjectCode RIGHT OUTER JOIN
						  Project.tbStatus INNER JOIN
						  Usr.tbUser INNER JOIN
						  Object.tbObject INNER JOIN
						  Project.tbProject ON Object.tbObject.ObjectCode = Project.tbProject.ObjectCode INNER JOIN
						  Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode LEFT OUTER JOIN
						  Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode ON Usr.tbUser.UserId = Project.tbProject.ActionById ON 
						  Project.tbStatus.ProjectStatusCode = Project.tbProject.ProjectStatusCode LEFT OUTER JOIN
						  Subject.tbAddress AS Subject_tbAddress1 LEFT OUTER JOIN
						  Subject.tbSubject AS Subject_tb1 ON Subject_tbAddress1.SubjectCode = Subject_tb1.SubjectCode ON Project.tbProject.AddressCodeTo = Subject_tbAddress1.AddressCode ON 
						  Subject_tbAddress2.AddressCode = Project.tbProject.AddressCodeFrom LEFT OUTER JOIN
						  Subject.tbContact ON Project.tbProject.ContactName = Subject.tbContact.ContactName AND Project.tbProject.SubjectCode = Subject.tbContact.SubjectCode LEFT OUTER JOIN
						  App.tbTaxCode ON Project.tbProject.TaxCode = App.tbTaxCode.TaxCode
go
PRINT N'Creating Function [App].[fnOffsetDays]...';


go
CREATE   FUNCTION App.fnOffsetDays(@StartOn DATE, @EndOn DATE)
RETURNS SMALLINT
AS
BEGIN

	DECLARE 
		@OffsetDays SMALLINT = 0		  
		, @CalendarCode nvarchar(10)
		, @WorkingDay bit
		, @CurrentDay smallint
		, @Monday smallint
		, @Tuesday smallint
		, @Wednesday smallint
		, @Thursday smallint
		, @Friday smallint
		, @Saturday smallint
		, @Sunday smallint
			
	
	IF DATEDIFF(DAY, @StartOn, @EndOn) <= 0
		RETURN 0

	SELECT     @CalendarCode = App.tbCalendar.CalendarCode, @Monday = Monday, @Tuesday = Tuesday, @Wednesday = Wednesday, @Thursday = Thursday, @Friday = Friday, @Saturday = Saturday, @Sunday = Sunday
	FROM         App.tbCalendar INNER JOIN
							Usr.tbUser ON App.tbCalendar.CalendarCode = Usr.tbUser.CalendarCode
	WHERE UserId = (SELECT TOP (1) UserId FROM Usr.vwCredentials)
	
	WHILE @EndOn <> @StartOn
		BEGIN
		
		SET @CurrentDay = App.fnWeekDay(@EndOn)
		IF @CurrentDay = 1				
			SET @WorkingDay = CASE WHEN @Monday != 0 THEN 1 ELSE 0 END
		ELSE IF @CurrentDay = 2
			SET @WorkingDay = CASE WHEN @Tuesday != 0 THEN 1 ELSE 0 END
		ELSE IF @CurrentDay = 3
			SET @WorkingDay = CASE WHEN @Wednesday != 0 THEN 1 ELSE 0 END
		ELSE IF @CurrentDay = 4
			SET @WorkingDay = CASE WHEN @Thursday != 0 THEN 1 ELSE 0 END
		ELSE IF @CurrentDay = 5
			SET @WorkingDay = CASE WHEN @Friday != 0 THEN 1 ELSE 0 END
		ELSE IF @CurrentDay = 6
			SET @WorkingDay = CASE WHEN @Saturday != 0 THEN 1 ELSE 0 END
		ELSE IF @CurrentDay = 7
			SET @WorkingDay = CASE WHEN @Sunday != 0 THEN 1 ELSE 0 END
		
		IF @WorkingDay = 1
			BEGIN
			IF NOT EXISTS(SELECT     UnavailableOn
						FROM         App.tbCalendarHoliday
						WHERE     (CalendarCode = @CalendarCode) AND (UnavailableOn = @EndOn))
				SET @OffsetDays += 1
			END
			
		SET @EndOn = DATEADD(d, -1, @EndOn)
		END

	
	RETURN @OffsetDays

END
go
PRINT N'Creating Function [App].[fnAdjustToCalendar]...';


go
CREATE   FUNCTION App.fnAdjustToCalendar
	(
	@SourceDate datetime,
	@OffsetDays int
	)
RETURNS DATETIME
AS
BEGIN
	
	DECLARE 
		  @OutputDate datetime = @SourceDate
		, @CalendarCode nvarchar(10)
		, @WorkingDay bit
		, @CurrentDay smallint
		, @Monday smallint
		, @Tuesday smallint
		, @Wednesday smallint
		, @Thursday smallint
		, @Friday smallint
		, @Saturday smallint
		, @Sunday smallint
			

	SELECT     @CalendarCode = App.tbCalendar.CalendarCode, @Monday = Monday, @Tuesday = Tuesday, @Wednesday = Wednesday, @Thursday = Thursday, @Friday = Friday, @Saturday = Saturday, @Sunday = Sunday
	FROM         App.tbCalendar INNER JOIN
							Usr.tbUser ON App.tbCalendar.CalendarCode = Usr.tbUser.CalendarCode
	WHERE UserId = (SELECT TOP (1) UserId FROM Usr.vwCredentials)
	
	WHILE @OffsetDays > -1
		BEGIN
		SET @CurrentDay = App.fnWeekDay(@OutputDate)
		IF @CurrentDay = 1				
			SET @WorkingDay = CASE WHEN @Monday != 0 THEN 1 ELSE 0 END
		ELSE IF @CurrentDay = 2
			SET @WorkingDay = CASE WHEN @Tuesday != 0 THEN 1 ELSE 0 END
		ELSE IF @CurrentDay = 3
			SET @WorkingDay = CASE WHEN @Wednesday != 0 THEN 1 ELSE 0 END
		ELSE IF @CurrentDay = 4
			SET @WorkingDay = CASE WHEN @Thursday != 0 THEN 1 ELSE 0 END
		ELSE IF @CurrentDay = 5
			SET @WorkingDay = CASE WHEN @Friday != 0 THEN 1 ELSE 0 END
		ELSE IF @CurrentDay = 6
			SET @WorkingDay = CASE WHEN @Saturday != 0 THEN 1 ELSE 0 END
		ELSE IF @CurrentDay = 7
			SET @WorkingDay = CASE WHEN @Sunday != 0 THEN 1 ELSE 0 END
		
		IF @WorkingDay = 1
			BEGIN
			IF NOT EXISTS(SELECT     UnavailableOn
						FROM         App.tbCalendarHoliday
						WHERE     (CalendarCode = @CalendarCode) AND (UnavailableOn = @OutputDate))
				SET @OffsetDays -= 1
			END
			
		IF @OffsetDays > -1
			SET @OutputDate = DATEADD(d, -1, @OutputDate)
		END
	
	RETURN @OutputDate				
END
go
PRINT N'Creating Function [Invoice].[fnEditProjects]...';


go
CREATE   FUNCTION Invoice.fnEditProjects (@InvoiceNumber nvarchar(20), @SubjectCode nvarchar(10))
RETURNS TABLE
AS
	RETURN 
	(		
		WITH InvoiceEditProjects AS 
		(	SELECT        ProjectCode
			FROM            Invoice.tbProject
			WHERE        (InvoiceNumber = @InvoiceNumber)
		)
		SELECT TOP (100) PERCENT Project.tbProject.ProjectCode, Project.tbProject.ObjectCode, Project.tbStatus.ProjectStatus, Usr.tbUser.UserName, Project.tbProject.ActionOn, Project.tbProject.ActionedOn, Project.tbProject.ProjectTitle
		FROM            Usr.tbUser INNER JOIN
								Project.tbProject INNER JOIN
								Project.tbStatus ON Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode ON Usr.tbUser.UserId = Project.tbProject.ActionById LEFT OUTER JOIN
								InvoiceEditProjects ON Project.tbProject.ProjectCode = InvoiceEditProjects.ProjectCode
		WHERE        (Project.tbProject.SubjectCode = @SubjectCode) AND (Project.tbProject.ProjectStatusCode = 1 OR
								Project.tbProject.ProjectStatusCode = 2) AND (Project.tbProject.CashCode IS NOT NULL) AND (InvoiceEditProjects.ProjectCode IS NULL)
		ORDER BY Project.tbProject.ActionOn DESC
	);
go
PRINT N'Creating Function [Invoice].[fnEditDebitCandidates]...';


go
CREATE   FUNCTION Invoice.fnEditDebitCandidates (@InvoiceNumber nvarchar(20), @SubjectCode nvarchar(10))
RETURNS TABLE
AS
	RETURN 
	(		
		WITH InvoiceEditProjects AS 
		(
			SELECT        ProjectCode
			FROM            Invoice.tbProject
			WHERE        (InvoiceNumber = @InvoiceNumber)
		)
		SELECT TOP (100) PERCENT tbInvoiceProject.ProjectCode, tbInvoiceProject.InvoiceNumber, tbProject.ObjectCode, Invoice.tbStatus.InvoiceStatus, Usr.tbUser.UserName, Invoice.tbInvoice.InvoicedOn, tbInvoiceProject.InvoiceValue, 
								tbProject.ProjectTitle
		FROM            Usr.tbUser INNER JOIN
								Invoice.tbInvoice INNER JOIN
								Invoice.tbProject AS tbInvoiceProject ON Invoice.tbInvoice.InvoiceNumber = tbInvoiceProject.InvoiceNumber INNER JOIN
								Project.tbProject ON tbInvoiceProject.ProjectCode = tbProject.ProjectCode INNER JOIN
								Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode ON Usr.tbUser.UserId = Invoice.tbInvoice.UserId LEFT OUTER JOIN
								InvoiceEditProjects  ON tbProject.ProjectCode = InvoiceEditProjects.ProjectCode
		WHERE        (Invoice.tbInvoice.SubjectCode = @SubjectCode) AND (Invoice.tbInvoice.InvoiceTypeCode = 2) AND (InvoiceEditProjects.ProjectCode IS NULL)
		ORDER BY Invoice.tbInvoice.InvoicedOn DESC
	);
go
PRINT N'Creating Function [Invoice].[fnEditCreditCandidates]...';


go
CREATE   FUNCTION Invoice.fnEditCreditCandidates (@InvoiceNumber nvarchar(20), @SubjectCode nvarchar(10))
RETURNS TABLE
AS
	RETURN 
	(		
		WITH InvoiceEditProjects AS 
		(
			SELECT        ProjectCode
			FROM            Invoice.tbProject
			WHERE        (InvoiceNumber = @InvoiceNumber)
		)
		SELECT TOP (100) PERCENT tbInvoiceProject.ProjectCode, tbInvoiceProject.InvoiceNumber, tbProject.ObjectCode, Invoice.tbStatus.InvoiceStatus, Usr.tbUser.UserName, Invoice.tbInvoice.InvoicedOn, tbInvoiceProject.InvoiceValue, 
								tbProject.ProjectTitle
		FROM            Usr.tbUser INNER JOIN
								Invoice.tbInvoice INNER JOIN
								Invoice.tbProject AS tbInvoiceProject ON Invoice.tbInvoice.InvoiceNumber = tbInvoiceProject.InvoiceNumber INNER JOIN
								Project.tbProject AS tbProject ON tbInvoiceProject.ProjectCode = tbProject.ProjectCode INNER JOIN
								Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode ON Usr.tbUser.UserId = Invoice.tbInvoice.UserId LEFT OUTER JOIN
								InvoiceEditProjects AS InvoiceEditProjects ON tbProject.ProjectCode = InvoiceEditProjects.ProjectCode
		WHERE        (Invoice.tbInvoice.SubjectCode = @SubjectCode) AND (Invoice.tbInvoice.InvoiceTypeCode = 0) AND (InvoiceEditProjects.ProjectCode IS NULL)
		ORDER BY Invoice.tbInvoice.InvoicedOn DESC
	);
go
PRINT N'Creating Function [Cash].[fnChangeTx]...';


go
CREATE   FUNCTION Cash.fnChangeTx(@PaymentAddress nvarchar(42))
RETURNS TABLE
AS
	RETURN
	(
		SELECT Cash.tbTx.PaymentAddress, Cash.tbTx.TxId, Cash.tbTx.TransactedOn, Cash.tbTx.TxStatusCode, Cash.tbTxStatus.TxStatus, Cash.tbTx.MoneyIn, Cash.tbTx.MoneyOut, Cash.tbTx.Confirmations, Cash.tbTx.InsertedBy, payments.PaymentCodeIn, payments.PaymentCodeOut, Cash.tbTx.TxMessage
		FROM Cash.tbTx 
			INNER JOIN Cash.tbTxStatus ON Cash.tbTx.TxStatusCode = Cash.tbTxStatus.TxStatusCode 
			LEFT OUTER JOIN Cash.vwTxReference payments ON Cash.tbTx.TxNumber = payments.TxNumber
		WHERE        (Cash.tbTx.PaymentAddress = @PaymentAddress)		
	)
go
PRINT N'Creating Function [Cash].[fnFlowCategoryTotalCodes]...';


go
CREATE   FUNCTION Cash.fnFlowCategoryTotalCodes(@CategoryCode NVARCHAR(10))
RETURNS TABLE
AS
	RETURN
	(
		SELECT ChildCode AS CategoryCode FROM Cash.tbCategoryTotal WHERE ParentCode = @CategoryCode
	)
go
PRINT N'Creating Function [Cash].[fnFlowCategoryCashCodes]...';


go
CREATE   FUNCTION Cash.fnFlowCategoryCashCodes
	(
	@CategoryCode nvarchar(10)
	)
RETURNS TABLE
AS
	RETURN (
		SELECT     CashCode, CashDescription
		FROM         Cash.tbCode
		WHERE     (CategoryCode = @CategoryCode) AND (IsEnabled <> 0)			 
	)
go
PRINT N'Creating Function [Cash].[fnFlowCategoriesByType]...';


go
CREATE   FUNCTION Cash.fnFlowCategoriesByType
	(
	@CashTypeCode smallint,
	@CategoryTypeCode smallint = 1
	)
RETURNS TABLE
AS
	RETURN (
		SELECT     Cash.tbCategory.DisplayOrder, Cash.tbCategory.Category, Cash.tbType.CashType, Cash.tbCategory.CategoryCode
		FROM         Cash.tbCategory INNER JOIN
							  Cash.tbType ON Cash.tbCategory.CashTypeCode = Cash.tbType.CashTypeCode
		WHERE     ( Cash.tbCategory.CashTypeCode = @CashTypeCode) AND ( Cash.tbCategory.CategoryTypeCode = @CategoryTypeCode)
		)
go
PRINT N'Creating Function [Cash].[fnFlowBankBalances]...';


go
CREATE FUNCTION Cash.fnFlowBankBalances (@AccountCode NVARCHAR(10))
RETURNS TABLE
AS
	RETURN
	WITH account_periods AS
	(
		SELECT    @AccountCode AS AccountCode, App.tbYear.YearNumber, App.tbYearPeriod.StartOn
		FROM            App.tbYearPeriod INNER JOIN
								 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
		WHERE        (App.tbYear.CashStatusCode BETWEEN 1 AND 2)
	), last_entries AS
	(
		SELECT account_statement.AccountCode, account_statement.StartOn, MAX(account_statement.EntryNumber) As EntryNumber
		FROM Cash.vwAccountStatement account_statement 
		WHERE account_statement.AccountCode = @AccountCode
		GROUP BY account_statement.AccountCode, account_statement.StartOn
	), closing_balance AS
	(
		SELECT account_statement.AccountCode,  account_statement.StartOn, account_statement.PaidBalance 
		FROM last_entries 
			JOIN Cash.vwAccountStatement account_statement ON last_entries.AccountCode = account_statement.AccountCode
				AND last_entries.EntryNumber = account_statement.EntryNumber
	), statement_ordered AS
	(
		SELECT 
			ROW_NUMBER() OVER (ORDER BY account_periods.StartOn) EntryNumber,
			account_periods.AccountCode, account_periods.YearNumber, account_periods.StartOn, CAST(COALESCE(closing_balance.PaidBalance, 0) as float) Balance,
			CASE WHEN closing_balance.AccountCode IS NULL THEN CAST(0 as bit) ELSE CAST(1 as bit) END IsEntry
		FROM account_periods
			LEFT OUTER JOIN closing_balance 
				ON account_periods.AccountCode = closing_balance.AccountCode AND account_periods.StartOn = closing_balance.StartOn
	), statement_ranked AS
	(
		SELECT *,
			RANK() OVER (ORDER BY EntryNumber) RNK
		FROM statement_ordered
	), statement_grouped AS
	(
		SELECT EntryNumber, AccountCode, YearNumber, StartOn, Balance, IsEntry,
			MAX(CASE IsEntry WHEN 0 THEN 0 ELSE RNK END) OVER (ORDER BY EntryNumber) RNK
		FROM statement_ranked
	)
	SELECT AccountCode, YearNumber, StartOn, 
		CASE IsEntry WHEN 0 THEN
			MAX(Balance) OVER (PARTITION BY RNK ORDER BY EntryNumber) +
			MIN(Balance) OVER (PARTITION BY RNK ORDER BY EntryNumber) 
		ELSE
			Balance
		END
		AS Balance		
	FROM statement_grouped;
go
PRINT N'Creating Function [Cash].[fnChangeUnassigned]...';


go
CREATE   FUNCTION Cash.fnChangeUnassigned (@AccountCode nvarchar(10))
RETURNS TABLE
AS
	RETURN
	(
		SELECT change.AccountCode, Subject.fnAccountKeyNamespace(account_key.AccountCode, account_key.HDPath) AS KeyNamespace, 
			account_key.KeyName, change.PaymentAddress, change.Note, change.InsertedOn, change.UpdatedOn, COALESCE(change_balance.Balance, 0) Balance
		FROM Cash.tbChange AS change 
				OUTER APPLY
				(
					SELECT PaymentAddress, SUM(MoneyIn) Balance
					FROM Cash.tbTx tx
					WHERE tx.PaymentAddress = change.PaymentAddress AND tx.TxStatusCode = 1
					GROUP BY PaymentAddress			
				) change_balance
			JOIN Subject.tbAccountKey account_key ON change.AccountCode = account_key.AccountCode AND change.HDPath = account_key.HDPath
			LEFT OUTER JOIN Cash.tbChangeReference ON change.PaymentAddress = Cash.tbChangeReference.PaymentAddress
		WHERE  (change.AccountCode = @AccountCode)  AND (change.ChangeTypeCode = 0) AND (Cash.tbChangeReference.PaymentAddress IS NULL) AND (change.ChangeStatusCode = 0)
	)
go
PRINT N'Creating Function [Cash].[fnKeyAddresses]...';


go
CREATE   FUNCTION Cash.fnKeyAddresses(@AccountCode nvarchar(10), @KeyName nvarchar(50))
RETURNS TABLE
AS
	RETURN
	(
		SELECT        
			Cash.fnChangeKeyPath(cash_account.CoinTypeCode, key_name.HDPath.ToString(), change.ChangeTypeCode, AddressIndex)  HDPath, 
			change.PaymentAddress, change.AddressIndex
		FROM Cash.tbChange AS change 
			INNER JOIN Subject.tbAccountKey AS key_name 
				ON change.AccountCode = key_name.AccountCode AND change.HDPath = key_name.HDPath AND change.AccountCode = key_name.AccountCode AND change.HDPath = key_name.HDPath 
			INNER JOIN Subject.tbAccount AS cash_account 
				ON key_name.AccountCode = cash_account.AccountCode
		WHERE (change.ChangeStatusCode = 1) AND (key_name.AccountCode = @AccountCode) AND (key_name.KeyName = @KeyName)
	)
go
PRINT N'Creating Function [Subject].[fnKeyNamespace]...';


go
CREATE   FUNCTION Subject.fnKeyNamespace (@AccountCode nvarchar(10), @KeyName nvarchar(50))
RETURNS TABLE 
AS
	RETURN
	(
		WITH key_root AS
		(
			SELECT AccountCode, HDPath, HDLevel, KeyName
			FROM Subject.tbAccountKey
			WHERE AccountCode = @AccountCode AND KeyName = @KeyName
		), candidates AS
		(
			SELECT AccountCode, HDPath.GetAncestor(1) ParentHDPath, HDPath ChildHDPath, KeyName
			FROM Subject.tbAccountKey
			WHERE AccountCode = @AccountCode AND HDLevel > (SELECT HDLevel FROM key_root) 
		), namespace_set AS
		(
			SELECT AccountCode, cast(NULL AS hierarchyid) ParentHDPath, HDPath ChildHDPath, KeyName FROM key_root

			UNION ALL

			SELECT candidates.AccountCode, candidates.ParentHDPath, candidates.ChildHDPath, candidates.KeyName
			FROM candidates
				JOIN namespace_set ON candidates.ParentHDPath = namespace_set.ChildHDPath
		)
		SELECT AccountCode, ChildHDPath HDPath, KeyName, Subject.fnAccountKeyNamespace(AccountCode, ChildHDPath) KeyNamespace
		FROM namespace_set
	)
go
PRINT N'Creating Function [Cash].[fnNamespaceBalance]...';


go
CREATE   FUNCTION Cash.fnNamespaceBalance(@AccountCode nvarchar(10), @KeyName nvarchar(50))
RETURNS float
AS
BEGIN
	DECLARE @Balance float;

	SELECT @Balance = SUM(COALESCE(change_balance.Balance, 0))
	FROM Subject.fnKeyNamespace(@AccountCode, @KeyName) kn
		JOIN Cash.tbChange change
			ON kn.AccountCode = change.AccountCode AND kn.HDPath = change.HDPath
		OUTER APPLY
		(
			SELECT PaymentAddress, SUM(MoneyIn) Balance
			FROM Cash.tbTx tx
			WHERE tx.PaymentAddress = change.PaymentAddress AND tx.TxStatusCode = 1
			GROUP BY PaymentAddress			
		) change_balance

	RETURN @Balance;
END
go
PRINT N'Creating Function [Cash].[fnTx]...';


go
CREATE   FUNCTION Cash.fnTx(@AccountCode nvarchar(10), @KeyName nvarchar(50))
RETURNS TABLE
AS
	RETURN
	(
		WITH tx AS
		(
			SELECT        change.AccountCode, Subject.tbAccount.CoinTypeCode, change.PaymentAddress, change.HDPath, change.ChangeTypeCode, change_type.ChangeType, change.ChangeStatusCode, change_status.ChangeStatus, 
									 change.AddressIndex, tx.TxId, tx.TransactedOn, tx.TxStatusCode, tx_status.TxStatus, tx.MoneyIn, tx.MoneyOut, tx.Confirmations, tx.TxMessage, tx.InsertedBy, tx_ref.PaymentCodeIn, tx_ref.PaymentCodeOut
			FROM            Cash.tbTx AS tx INNER JOIN
                         Cash.tbTxStatus AS tx_status ON tx.TxStatusCode = tx_status.TxStatusCode AND tx.TxStatusCode = tx_status.TxStatusCode INNER JOIN
                         Cash.tbChange AS change ON tx.PaymentAddress = change.PaymentAddress AND tx.PaymentAddress = change.PaymentAddress INNER JOIN
                         Cash.tbChangeType AS change_type ON change.ChangeTypeCode = change_type.ChangeTypeCode AND change.ChangeTypeCode = change_type.ChangeTypeCode INNER JOIN
                         Cash.tbChangeStatus AS change_status ON change.ChangeStatusCode = change_status.ChangeStatusCode INNER JOIN
                         Subject.tbAccount ON change.AccountCode = Subject.tbAccount.AccountCode
						 LEFT OUTER JOIN vwTxReference tx_ref ON tx.TxNumber = tx_ref.TxNumber
		), key_namespace AS
		(
			SELECT AccountCode, HDPath, KeyNamespace, KeyName
			FROM Subject.fnKeyNamespace(@AccountCode, @KeyName) kn
		)
		SELECT tx.AccountCode, KeyNamespace, KeyName, PaymentAddress, ChangeTypeCode, ChangeType, ChangeStatusCode, ChangeStatus, 
			Cash.fnChangeKeyPath(tx.CoinTypeCode, key_namespace.HDPath.ToString(), tx.ChangeTypeCode, tx.AddressIndex)  FullHDPath,
			TxId, TransactedOn, TxStatusCode, TxStatus, MoneyIn, MoneyOut, Confirmations, TxMessage, InsertedBy, PaymentCodeIn, PaymentCodeOut
		FROM  key_namespace 
			JOIN tx ON key_namespace.HDPath = tx.HDPath
	)
go
PRINT N'Creating Function [Cash].[fnChange]...';


go
CREATE   FUNCTION Cash.fnChange(@AccountCode nvarchar(10), @KeyName nvarchar(50), @ChangeTypeCode smallint)
RETURNS TABLE
AS
	RETURN
	(
		WITH account_reference AS
		(
			SELECT        Cash.tbChangeReference.PaymentAddress, Cash.tbChangeReference.InvoiceNumber, Invoice.tbInvoice.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbType.InvoiceType, 
										Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue - Invoice.tbInvoice.PaidValue - Invoice.tbInvoice.PaidTaxValue AS AmountDue, Invoice.tbInvoice.ExpectedOn, Invoice.tbStatus.InvoiceStatus, Invoice.tbType.CashPolarityCode
			FROM            Cash.tbChangeReference INNER JOIN
										Invoice.tbInvoice ON Cash.tbChangeReference.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
										Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
										Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
										Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode
		), key_namespace AS
		(
			SELECT AccountCode, HDPath, KeyNamespace, KeyName
			FROM Subject.fnKeyNamespace(@AccountCode, @KeyName) kn
		), change AS
		(
			SELECT Cash.fnChangeKeyPath(cash_account.CoinTypeCode, key_namespace.HDPath.ToString(), change.ChangeTypeCode, AddressIndex)  FullHDPath, 
				change.AccountCode, key_namespace.KeyName, key_namespace.KeyNamespace, change.AddressIndex, change.PaymentAddress, change.ChangeTypeCode, change_type.ChangeType, change.ChangeStatusCode, change_Status.ChangeStatus,
				change.Note, account_reference.InvoiceNumber, account_reference.SubjectCode, account_reference.SubjectName, account_reference.InvoiceType, account_reference.InvoiceStatus, account_reference.CashPolarityCode,
				account_reference.AmountDue, account_reference.ExpectedOn, change.UpdatedOn, change.UpdatedBy, change.InsertedOn, change.InsertedBy, change.RowVer
			FROM  key_namespace 
				JOIN Subject.tbAccount AS cash_account ON key_namespace.AccountCode = cash_account.AccountCode AND key_namespace.AccountCode = cash_account.AccountCode 
				JOIN Cash.tbChange AS change ON key_namespace.AccountCode = change.AccountCode AND key_namespace.HDPath = change.HDPath 
				JOIN Cash.tbChangeType change_type ON change.ChangeTypeCode = change_type .ChangeTypeCode 
				JOIN Cash.tbChangeStatus change_status ON change.ChangeStatusCode = change_status .ChangeStatusCode 

				LEFT OUTER JOIN account_reference ON change.PaymentAddress = account_reference.PaymentAddress
			WHERE change.ChangeTypeCode = @ChangeTypeCode 
	)
	SELECT change.*, COALESCE(change_balance.Balance, 0) Balance
	FROM change
		OUTER APPLY
		(
			SELECT PaymentAddress, SUM(MoneyIn) Balance
			FROM Cash.tbTx tx
			WHERE tx.PaymentAddress = change.PaymentAddress AND tx.TxStatusCode = 1
			GROUP BY PaymentAddress
		) AS change_balance
	)
go
PRINT N'Creating Function [Cash].[fnTaxTypeDueDates]...';


go
CREATE FUNCTION Cash.fnTaxTypeDueDates(@TaxTypeCode smallint)
RETURNS @tbDueDate TABLE (PayOn datetime, PayFrom datetime, PayTo datetime)
 AS
	BEGIN
	DECLARE @MonthNumber smallint
			, @MonthInterval smallint
			, @StartOn datetime
	
		SELECT 
			@MonthNumber = MonthNumber, 
			@MonthInterval = CASE RecurrenceCode
								WHEN 0 THEN 1
								WHEN 1 THEN 1
								WHEN 2 THEN 3
								WHEN 3 THEN 6
								WHEN 4 THEN 12
							END
		FROM Cash.tbTaxType
		WHERE TaxTypeCode = @TaxTypeCode			

		SELECT   @StartOn = MIN(StartOn)
		FROM         App.tbYearPeriod
		WHERE     (MonthNumber = @MonthNumber)

		INSERT INTO @tbDueDate (PayOn) VALUES (@StartOn)
	
		SET @MonthNumber = CASE 			
			WHEN (@MonthNumber + @MonthInterval) <= 12 THEN @MonthNumber + @MonthInterval
			WHEN (@MonthNumber + @MonthInterval) % 12 = 0 THEN @MonthNumber
			ELSE (@MonthNumber + @MonthInterval) % 12
			END
	
		WHILE EXISTS(SELECT     *
					 FROM         App.tbYearPeriod
					 WHERE     (StartOn > @StartOn) AND (MonthNumber = @MonthNumber))
		BEGIN
			SELECT @StartOn = MIN(StartOn)
			FROM         App.tbYearPeriod
			WHERE     (StartOn > @StartOn) AND (MonthNumber = @MonthNumber)
			ORDER BY MIN(StartOn)		
			INSERT INTO @tbDueDate (PayOn) VALUES (@StartOn)
		
			SET @MonthNumber = CASE 
						WHEN (@MonthNumber + @MonthInterval) <= 12 THEN @MonthNumber + @MonthInterval
						WHEN (@MonthNumber + @MonthInterval) % 12 = 0 THEN @MonthNumber
						ELSE (@MonthNumber + @MonthInterval) % 12 
						END;	
		END;

		WITH dd AS
		(
			SELECT PayOn, LAG(PayOn) OVER (ORDER BY PayOn) AS PayFrom
			FROM @tbDueDate 
		)
		UPDATE @tbDueDate
		SET PayTo = dd.PayOn, PayFrom = dd.PayFrom
		FROM @tbDueDate tbDueDate JOIN dd ON tbDueDate.PayOn = dd.PayOn;

		UPDATE @tbDueDate
		SET PayFrom = DATEADD(MONTH, @MonthInterval * -1, PayTo)
		WHERE PayTo = (SELECT MIN(PayTo) FROM @tbDueDate);

		UPDATE @tbDueDate
		SET PayOn = DATEADD(DAY, (SELECT OffsetDays FROM Cash.tbTaxType WHERE TaxTypeCode = @TaxTypeCode), PayOn)

	RETURN	
	END
go
PRINT N'Creating Function [Cash].[fnFlowCategory]...';


go
CREATE   FUNCTION Cash.fnFlowCategory(@CashTypeCode smallint)
RETURNS @tbCategory TABLE (CategoryCode nvarchar(10), Category nvarchar(50), CashPolarityCode smallint, DisplayOrder smallint)
AS
BEGIN
	IF NOT EXISTS (SELECT * FROM Cash.vwCategoryCapital capital 
						JOIN Cash.tbCategory category ON capital.CategoryCode = category.CategoryCode 
						WHERE (category.CategoryTypeCode = 0) AND (category.CashTypeCode = @CashTypeCode) AND (category.IsEnabled <> 0))
	BEGIN
		INSERT INTO @tbCategory (CategoryCode, Category, CashPolarityCode, DisplayOrder)
		SELECT CategoryCode, Category, CashPolarityCode, DisplayOrder
		FROM Cash.tbCategory
		WHERE (CategoryTypeCode = 0) AND (CashTypeCode = @CashTypeCode) AND (IsEnabled <> 0)		
	END
	ELSE
	BEGIN
		INSERT INTO @tbCategory (CategoryCode, Category, CashPolarityCode, DisplayOrder)
		SELECT CategoryCode, Category, CashPolarityCode, DisplayOrder
		FROM Cash.vwCategoryCapital
	END

	RETURN
END
go
PRINT N'Creating Function [App].[fnActivePeriod]...';


go
CREATE   FUNCTION App.fnActivePeriod	()
RETURNS @tbSystemYearPeriod TABLE (YearNumber smallint, StartOn datetime, EndOn datetime, MonthName nvarchar(10), Description nvarchar(10), MonthNumber smallint) 
   AS
	BEGIN
	DECLARE @StartOn datetime
	DECLARE @EndOn datetime
	
	IF EXISTS (	SELECT     StartOn	FROM App.tbYearPeriod WHERE (CashStatusCode < 2))
		BEGIN
		SELECT @StartOn = MIN(StartOn)
		FROM         App.tbYearPeriod
		WHERE     (CashStatusCode < 2)
		
		IF EXISTS (SELECT StartOn FROM App.tbYearPeriod WHERE StartOn > @StartOn)
			SELECT TOP 1 @EndOn = StartOn FROM App.tbYearPeriod WHERE StartOn > @StartOn order by StartOn
		ELSE
			SET @EndOn = DATEADD(m, 1, @StartOn)
			
		INSERT INTO @tbSystemYearPeriod (YearNumber, StartOn, EndOn, MonthName, Description, MonthNumber)
		SELECT     App.tbYearPeriod.YearNumber, App.tbYearPeriod.StartOn, @EndOn, App.tbMonth.MonthName, App.tbYear.Description, App.tbMonth.MonthNumber
		FROM         App.tbYearPeriod INNER JOIN
		                      App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
		                      App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
		WHERE     ( App.tbYearPeriod.StartOn = @StartOn)
		END	
	RETURN
	END
go
PRINT N'Creating Function [App].[fnBuckets]...';


go
CREATE   FUNCTION App.fnBuckets
	(@CurrentDate datetime)
RETURNS  @tbBkn TABLE (Period smallint, BucketId nvarchar(10), StartDate datetime, EndDate datetime)
  AS
	BEGIN
	DECLARE @BucketTypeCode smallint
	DECLARE @UnitOfTimeCode smallint
	DECLARE @Period smallint	
	DECLARE @CurrentPeriod smallint
	DECLARE @Offset smallint
	
	DECLARE @StartDate datetime
	DECLARE @EndDate datetime
	DECLARE @BucketId nvarchar(10)
		
	SELECT     TOP 1 @BucketTypeCode = BucketTypeCode, @UnitOfTimeCode = BucketIntervalCode
	FROM         App.tbOptions
		
	SET @EndDate = 
		CASE @BucketTypeCode
			WHEN 0 THEN
				@CurrentDate
			WHEN 8 THEN
				DATEADD(d, Day(@CurrentDate) * -1 + 1, @CurrentDate)
			ELSE
				App.fnAdjustDateToBucket(@BucketTypeCode, @CurrentDate)
		END
			
	SET @EndDate = CAST(@EndDate AS date)
	SET @StartDate = DATEADD(yyyy, -100, @EndDate)
	SET @CurrentPeriod = 0
	
	DECLARE curBk cursor for			
		SELECT     Period, BucketId
		FROM         App.tbBucket
		ORDER BY Period

	OPEN curBk
	FETCH NEXT FROM curBk INTO @Period, @BucketId
	WHILE @@FETCH_STATUS = 0
		BEGIN
		IF @Period > 0
			BEGIN
			SET @StartDate = @EndDate
			SET @Offset = @Period - @CurrentPeriod
			SET @EndDate = CASE @UnitOfTimeCode
				WHEN 0 THEN		--day
					DATEADD(d, @Offset, @StartDate) 					
				WHEN 1 THEN		--week
					DATEADD(d, @Offset * 7, @StartDate)
				WHEN 2 THEN		--month
					DATEADD(m, @Offset, @StartDate)
				END
			END
		
		INSERT INTO @tbBkn(Period, BucketId, StartDate, EndDate)
		VALUES (@Period, @BucketId, @StartDate, @EndDate)
		
		SET @CurrentPeriod = @Period
		
		FETCH NEXT FROM curBk INTO @Period, @BucketId
		END		
			
	RETURN
	END
go
PRINT N'Creating View [Cash].[vwTaxCorpStatement]...';


go
CREATE VIEW Cash.vwTaxCorpStatement
AS
	WITH tax_dates AS
	(
		SELECT PayOn, PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(0)
	), period_totals AS
	(
		SELECT (SELECT PayOn FROM tax_dates WHERE totals.StartOn >= PayFrom AND totals.StartOn < PayTo) AS StartOn, CorporationTax
		FROM Cash.vwTaxCorpTotalsByPeriod totals
	), tax_entries AS
	(
		SELECT StartOn, SUM(CorporationTax) AS TaxDue, 0 AS TaxPaid
		FROM period_totals
		WHERE NOT StartOn IS NULL
		GROUP BY StartOn
		
		UNION

		SELECT Cash.tbPayment.PaidOn AS StartOn, 0 As TaxDue, ( Cash.tbPayment.PaidOutValue * -1) + Cash.tbPayment.PaidInValue AS TaxPaid
		FROM Cash.tbPayment 
			JOIN Cash.tbTaxType tt ON Cash.tbPayment.CashCode = tt.CashCode
		WHERE (tt.TaxTypeCode = 0)

	), tax_statement AS
	(
		SELECT StartOn, TaxDue, TaxPaid,
			SUM(TaxDue + TaxPaid) OVER (ORDER BY StartOn, TaxDue ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
		FROM tax_entries
	)
	SELECT StartOn, CAST(TaxDue AS decimal(18, 5)) TaxDue, CAST(TaxPaid AS decimal(18, 5)) TaxPaid, CAST(Balance AS decimal(18, 5)) Balance FROM tax_statement 
	WHERE StartOn >= (SELECT MIN(StartOn) FROM App.tbYearPeriod p JOIN App.tbYear y ON p.YearNumber = y.YearNumber  WHERE y.CashStatusCode < 3);
go
PRINT N'Creating View [Cash].[vwFlowVatRecurrenceAccruals]...';


go
CREATE   VIEW Cash.vwFlowVatRecurrenceAccruals
AS	
	WITH active_periods AS
	(
		SELECT App.tbYear.YearNumber, App.tbYearPeriod.StartOn
		FROM            App.tbYearPeriod INNER JOIN
								 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
								 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
		WHERE        (App.tbYear.CashStatusCode < 3)
	),	vat_dates AS
	(
		SELECT PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(1)
	), vatPeriod AS
	(
		SELECT        StartOn, y.YearNumber, p.MonthNumber,
			(SELECT PayTo FROM vat_dates WHERE p.StartOn >= PayFrom AND p.StartOn < PayTo) AS VatStartOn, VatAdjustment
		FROM            App.tbYearPeriod AS p JOIN App.tbYear AS y ON p.YearNumber = y.YearNumber 
	)
	, vat_accruals AS
	(
		SELECT  vatPeriod.VatStartOn AS StartOn,
				SUM(HomeSales) AS HomeSales, SUM(HomePurchases) AS HomePurchases, SUM(ExportSales) AS ExportSales, SUM(ExportPurchases) AS ExportPurchases, 
				SUM(HomeSalesVat) AS HomeSalesVat, SUM(HomePurchasesVat) AS HomePurchasesVat, 
				SUM(ExportSalesVat) AS ExportSalesVat, SUM(ExportPurchasesVat) AS ExportPurchasesVat, SUM(VatDue) AS VatDue
		FROM Cash.vwFlowVatPeriodAccruals accruals JOIN vatPeriod ON accruals.StartOn = vatPeriod.StartOn
		GROUP BY vatPeriod.VatStartOn
	)
	SELECT active_periods.YearNumber, active_periods.StartOn, CAST(ISNULL(HomeSales, 0) AS decimal(18,5)) AS HomeSales, CAST(ISNULL(HomePurchases, 0) AS decimal(18,5)) AS HomePurchases, 
		CAST(ISNULL(ExportSales, 0) AS decimal(18,5)) AS ExportSales, CAST(ISNULL(ExportPurchases, 0) AS decimal(18,5)) AS ExportPurchases, CAST(ISNULL(HomeSalesVat, 0) as decimal(18,5)) AS HomeSalesVat, 
		CAST(ISNULL(HomePurchasesVat, 0) AS decimal(18,5)) AS HomePurchasesVat, CAST(ISNULL(ExportSalesVat, 0) AS decimal(18,5)) AS ExportSalesVat, 
		CAST(ISNULL(ExportPurchasesVat, 0) AS decimal(18,5)) AS ExportPurchasesVat, CAST(ISNULL(VatDue, 0) AS decimal(18,5)) AS VatDue 
	FROM vat_accruals 
		RIGHT OUTER JOIN active_periods ON active_periods.StartOn = vat_accruals.StartOn;
go
PRINT N'Creating View [Cash].[vwTaxVatTotals]...';


go
CREATE VIEW Cash.vwTaxVatTotals
AS
	WITH vat_dates AS
	(
		SELECT PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(1)
	), vatPeriod AS
	(
		SELECT        StartOn, y.YearNumber, p.MonthNumber,
			(SELECT PayTo FROM vat_dates WHERE p.StartOn >= PayFrom AND p.StartOn < PayTo) AS VatStartOn, VatAdjustment
		FROM            App.tbYearPeriod AS p JOIN App.tbYear AS y ON p.YearNumber = y.YearNumber 
		WHERE     (y.CashStatusCode = 1) OR (y.CashStatusCode = 2)
	), vat_results AS
	(
		SELECT VatStartOn AS PayTo, DATEADD(MONTH, -1, VatStartOn) AS PostOn,
			SUM(HomeSales) AS HomeSales, SUM(HomePurchases) AS HomePurchases, SUM(ExportSales) AS ExportSales, SUM(ExportPurchases) AS ExportPurchases, 
			SUM(HomeSalesVat) AS HomeSalesVat, SUM(HomePurchasesVat) AS HomePurchasesVat, 
			SUM(ExportSalesVat) AS ExportSalesVat, SUM(ExportPurchasesVat) AS ExportPurchasesVat, SUM(VatDue) AS VatDue
		FROM Cash.vwTaxVatSummary vatCodeDue JOIN vatPeriod ON vatCodeDue.StartOn = vatPeriod.StartOn
		GROUP BY VatStartOn
	), vat_adjustments AS
	(
		SELECT VatStartOn AS PayTo, CAST(SUM(VatAdjustment) as float) AS VatAdjustment
		FROM vatPeriod p 
		GROUP BY VatStartOn
	)
	SELECT active_year.YearNumber, active_year.Description, active_month.MonthName AS Period, vat_results.PostOn AS StartOn, HomeSales, HomePurchases, ExportSales, ExportPurchases, HomeSalesVat, HomePurchasesVat, ExportSalesVat, ExportPurchasesVat,
		vat_adjustments.VatAdjustment, VatDue - vat_adjustments.VatAdjustment AS VatDue
	FROM vat_results JOIN vat_adjustments ON vat_results.PayTo = vat_adjustments.PayTo
		JOIN App.tbYearPeriod year_period ON vat_results.PostOn = year_period.StartOn
		JOIN App.tbMonth active_month ON year_period.MonthNumber = active_month.MonthNumber
		JOIN App.tbYear active_year ON year_period.YearNumber = active_year.YearNumber;
go
PRINT N'Creating View [Cash].[vwTaxVatStatement]...';


go
CREATE VIEW Cash.vwTaxVatStatement
AS
	WITH vat_dates AS
	(
		SELECT PayOn, PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(1)
	), vatPeriod AS
	(
		SELECT        StartOn, 
			(SELECT PayTo FROM vat_dates WHERE StartOn >= PayFrom AND StartOn < PayTo) AS VatStartOn, VatAdjustment
		FROM            App.tbYearPeriod 
	), vat_codes AS
	(
		SELECT     CashCode
		FROM         Cash.tbTaxType
		WHERE     (TaxTypeCode = 1)
	)
	, vat_results AS
	(
		SELECT VatStartOn AS StartOn, SUM(VatDue) AS VatDue
		FROM Cash.vwTaxVatSummary vatCodeDue JOIN vatPeriod ON vatCodeDue.StartOn = vatPeriod.StartOn
		GROUP BY VatStartOn
	), vat_adjustments AS
	(
		SELECT VatStartOn AS StartOn, SUM(VatAdjustment) AS VatAdjustment
		FROM vatPeriod
		GROUP BY VatStartOn
	), vat_unordered AS
	(
		SELECT vat_dates.PayOn AS StartOn, VatDue - a.VatAdjustment AS VatDue, 0 As VatPaid		
		FROM vat_results r JOIN vat_adjustments a ON r.StartOn = a.StartOn
			JOIN vat_dates ON r.StartOn = vat_dates.PayTo
			UNION
		SELECT     Cash.tbPayment.PaidOn AS StartOn, 0 As VatDue, ( Cash.tbPayment.PaidOutValue * -1) + Cash.tbPayment.PaidInValue AS VatPaid
		FROM         Cash.tbPayment INNER JOIN
							  vat_codes ON Cash.tbPayment.CashCode = vat_codes.CashCode	
	), vat_ordered AS
	(
		SELECT ROW_NUMBER() OVER (ORDER BY StartOn, VatDue) AS RowNumber,
			StartOn, VatDue, VatPaid
		FROM vat_unordered
	), vat_statement AS
	(
		SELECT RowNumber, StartOn, VatDue, VatPaid,
			SUM(VatDue+VatPaid) OVER (ORDER BY RowNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
		FROM vat_ordered
	)
	SELECT RowNumber, StartOn, CAST(VatDue as float) VatDue, CAST(VatPaid as float) VatPaid, CAST(Balance as decimal(18,5)) Balance
	FROM vat_statement
	WHERE StartOn >= (SELECT MIN(StartOn) FROM App.tbYearPeriod p JOIN App.tbYear y ON p.YearNumber = y.YearNumber  WHERE y.CashStatusCode < 3);
go
PRINT N'Creating View [Cash].[vwSummary]...';


go
CREATE VIEW Cash.vwSummary
AS
	WITH company AS
	(
		SELECT 0 AS SummaryId, SUM( Subject.tbAccount.CurrentBalance) AS CompanyBalance 
		FROM Subject.tbAccount WHERE ( Subject.tbAccount.AccountClosed = 0) AND (Subject.tbAccount.AccountTypeCode = 0)
	), corp_tax_invoiced AS
	(
		SELECT TOP (1)  0 AS SummaryId, Balance AS CorpTaxBalance 
		FROM Cash.vwTaxCorpStatement 
		ORDER BY StartOn DESC
	), corp_tax_ordered AS
	(
		SELECT 0 AS SummaryId, SUM(TaxDue) AS CorpTaxBalance
		FROM Cash.vwTaxCorpAccruals
	), vat_invoiced AS
	(
		SELECT TOP (1)  0 AS SummaryId, Balance AS VatBalance 
		FROM Cash.vwTaxVatStatement 
		ORDER BY StartOn DESC, VatDue DESC
	), vat_accruals AS
	(
		SELECT 0 AS SummaryId, SUM(VatDue) AS VatBalance
		FROM Cash.vwTaxVatAccruals
	), invoices AS
	(
		SELECT     Invoice.tbInvoice.InvoiceNumber, CASE Invoice.tbInvoice.InvoiceTypeCode WHEN 0 THEN (InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue) 
						  WHEN 3 THEN (InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue) ELSE 0 END AS ToCollect, 
						  CASE Invoice.tbInvoice.InvoiceTypeCode WHEN 1 THEN (InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue) WHEN 2 THEN (InvoiceValue + TaxValue) 
						  - (PaidValue + PaidTaxValue) ELSE 0 END AS ToPay, CASE Invoice.tbType.CashPolarityCode WHEN 0 THEN (TaxValue - PaidTaxValue) 
						  * - 1 WHEN 1 THEN TaxValue - PaidTaxValue END AS TaxValue
		FROM         Invoice.tbInvoice INNER JOIN
							  Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
		WHERE     (Invoice.tbInvoice.InvoiceStatusCode = 1) OR
						  (Invoice.tbInvoice.InvoiceStatusCode = 2)
	), invoice_totals AS
	(
		SELECT 0 AS SummaryId, ISNULL(SUM(ToCollect), 0) AS Collect, ISNULL(SUM(ToPay), 0) AS Pay, ISNULL(SUM(TaxValue), 0) AS TaxValue
		FROM  invoices
	), summary_base AS
	(
		SELECT Collect, Pay, TaxValue + vat_invoiced.VatBalance + vat_accruals.VatBalance + corp_tax_invoiced.CorpTaxBalance + corp_tax_ordered.CorpTaxBalance AS Tax, CompanyBalance
		FROM company 
				JOIN corp_tax_invoiced ON company.SummaryId = corp_tax_invoiced.SummaryId
				JOIN corp_tax_ordered ON company.SummaryId = corp_tax_ordered.SummaryId
				JOIN vat_invoiced ON company.SummaryId = vat_invoiced.SummaryId
				JOIN vat_accruals ON company.SummaryId = vat_accruals.SummaryId
				JOIN invoice_totals ON company.SummaryId = invoice_totals.SummaryId
	)
	SELECT CURRENT_TIMESTAMP AS Timestamp, Collect, Pay, Tax, CompanyBalance AS Cash, CompanyBalance + Collect - Pay - Tax AS Balance
	FROM    summary_base;
go
PRINT N'Creating View [Cash].[vwFlowVatRecurrence]...';


go
CREATE VIEW Cash.vwFlowVatRecurrence
AS
		WITH active_periods AS
	(
		SELECT App.tbYear.YearNumber, App.tbYearPeriod.StartOn
		FROM            App.tbYearPeriod INNER JOIN
								 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
								 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
		WHERE        (App.tbYear.CashStatusCode < 3)
	)
	SELECT active_periods.YearNumber, active_periods.StartOn, 
		CAST(ISNULL(SUM(vat.HomeSales), 0) as decimal(18, 5)) AS HomeSales, 
		CAST(ISNULL(SUM(vat.HomePurchases), 0) as decimal(18, 5)) AS HomePurchases, 
		CAST(ISNULL(SUM(vat.ExportSales), 0) as decimal(18, 5)) AS ExportSales, 
		CAST(ISNULL(SUM(vat.ExportPurchases), 0) as decimal(18, 5)) AS ExportPurchases, 
		CAST(ISNULL(SUM(vat.HomeSalesVat), 0) as decimal(18, 5)) AS HomeSalesVat, 
		CAST(ISNULL(SUM(vat.HomePurchasesVat), 0) as decimal(18, 5)) AS HomePurchasesVat, 
		CAST(ISNULL(SUM(vat.ExportSalesVat), 0) as decimal(18, 5)) AS ExportSalesVat, 
		CAST(ISNULL(SUM(vat.ExportPurchasesVat), 0) as decimal(18, 5)) AS ExportPurchasesVat, 
		CAST(ISNULL(SUM(vat.VatAdjustment), 0) as decimal(18, 5)) AS VatAdjustment, 
		CAST(ISNULL(SUM(vat.VatDue), 0) as decimal(18, 5)) AS VatDue
	FROM active_periods LEFT OUTER JOIN
							 Cash.vwTaxVatTotals AS vat ON active_periods.StartOn = vat.StartOn
	GROUP BY active_periods.YearNumber, active_periods.StartOn;
go
PRINT N'Creating View [Cash].[vwFlowCategories]...';


go
CREATE   VIEW Cash.vwFlowCategories
AS
	WITH trade_type AS
	(
		SELECT CashTypeCode, CashType FROM Cash.tbType
		WHERE CashTypeCode = 0
	), trade_cat AS
	(
		SELECT trade_type.CashTypeCode, trade_type.CashType, cats.CategoryCode, cats.Category, cats.CashPolarityCode, cats.DisplayOrder 
		FROM trade_type
			CROSS APPLY 
			(
				SELECT cat.* FROM Cash.fnFlowCategory(trade_type.CashTypeCode) cat
			) cats
	), cash_type AS
	(
		SELECT CashTypeCode, CashType FROM Cash.tbType
		WHERE CashTypeCode = 2
	), cash_cat AS
	(
		SELECT cash_type.CashTypeCode, 
		cash_type.CashType, cats.CategoryCode, cats.Category, cats.CashPolarityCode, cats.DisplayOrder
		FROM cash_type
			CROSS APPLY 
			(
				SELECT cat.* FROM Cash.fnFlowCategory(cash_type.CashTypeCode) cat
			) cats
	),  tax_type AS
	(
		SELECT CashTypeCode, CashType FROM Cash.tbType
		WHERE CashTypeCode = 1
	), tax_cat AS
	(
		SELECT tax_type.CashTypeCode, 
		tax_type.CashType, cats.CategoryCode, cats.Category, cats.CashPolarityCode, cats.DisplayOrder
		FROM tax_type
			CROSS APPLY 
			(
				SELECT cat.* FROM Cash.fnFlowCategory(tax_type.CashTypeCode) cat
			) cats
	), catagories_unsorted AS
	(
		SELECT CashTypeCode, DisplayOrder, CashType, CategoryCode, Category, CashPolarityCode 
		FROM trade_cat
		UNION
		SELECT 1 CashTypeCode, DisplayOrder, CashType, CategoryCode, Category, CashPolarityCode 
		FROM cash_cat
		UNION
		SELECT 2 CashTypeCode, DisplayOrder, CashType, CategoryCode, Category, CashPolarityCode 
		FROM tax_cat
	)
	SELECT CashTypeCode, ROW_NUMBER() OVER (ORDER BY CashTypeCode, DisplayOrder) EntryId,
		CashType, CategoryCode, Category, CashPolarityCode
	FROM catagories_unsorted;
go
PRINT N'Creating View [Cash].[vwBalanceSheetTax]...';


go
CREATE VIEW Cash.vwBalanceSheetTax
AS
	WITH tax_dates AS
	(
		SELECT (SELECT TOP 1 StartOn FROM App.tbYearPeriod WHERE StartOn < PayTo ORDER BY StartOn DESC) PayOn, 
			PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(0)
	), period_totals AS
	(
		SELECT (SELECT PayOn FROM tax_dates WHERE totals.StartOn >= PayFrom AND totals.StartOn < PayTo) AS StartOn, CorporationTax
		FROM Cash.vwTaxCorpTotalsByPeriod totals
	), tax_entries AS
	(
		SELECT StartOn, SUM(CorporationTax) AS TaxDue, 0 AS TaxPaid
		FROM period_totals
		WHERE NOT StartOn IS NULL
		GROUP BY StartOn
		
		UNION

		SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod WHERE (StartOn <= Cash.tbPayment.PaidOn) ORDER BY StartOn DESC) AS StartOn, 
			0 As TaxDue, ( Cash.tbPayment.PaidOutValue * -1) + Cash.tbPayment.PaidInValue AS TaxPaid
		FROM Cash.tbPayment 
			JOIN Cash.tbTaxType tt ON Cash.tbPayment.CashCode = tt.CashCode
		WHERE (tt.TaxTypeCode = 0)

	)
	, tax_balances AS
	(
		SELECT StartOn, TaxDue, TaxPaid,
			SUM(TaxDue + TaxPaid) OVER (ORDER BY StartOn, TaxDue ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
		FROM tax_entries
	), tax_statement AS
	(
		SELECT StartOn, CAST(TaxDue AS decimal(18, 5)) TaxDue, CAST(TaxPaid AS decimal(18, 5)) TaxPaid, CAST(Balance AS decimal(18, 5)) Balance FROM tax_balances 
		WHERE StartOn >= (SELECT MIN(StartOn) FROM App.tbYearPeriod p JOIN App.tbYear y ON p.YearNumber = y.YearNumber  WHERE y.CashStatusCode < 3)
	)
	SELECT tax_type.AssetCode, tax_type.AssetName, 
		CAST(0 as smallint) CashPolarityCode,  
		CAST(1 as smallint) AssetTypeCode,  
		StartOn, 		
		CASE WHEN Balance < 0 THEN 0 ELSE Balance * -1 END Balance 
	FROM tax_statement
		CROSS JOIN
		(
			SELECT UPPER(LEFT(TaxType, 3)) AssetCode, UPPER(TaxType) AssetName
			FROM Cash.tbTaxType
			WHERE TaxTypeCode = 0
		) tax_type;
go
PRINT N'Creating View [Cash].[vwTaxLossesCarriedForward]...';


go
CREATE VIEW Cash.vwTaxLossesCarriedForward
AS
	WITH tax_dates AS
	(
		SELECT PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(0)
	), period_totals AS
	(
		SELECT (SELECT PayTo FROM tax_dates WHERE totals.StartOn >= PayFrom AND totals.StartOn < PayTo) AS StartOn, CorporationTax
		FROM Cash.vwTaxCorpTotalsByPeriod totals
	), tax_entries AS
	(
		SELECT StartOn, SUM(CorporationTax) AS TaxDue, 0 AS TaxPaid
		FROM period_totals
		WHERE NOT StartOn IS NULL
		GROUP BY StartOn
		
		UNION

		SELECT Cash.tbPayment.PaidOn AS StartOn, 0 As TaxDue, ( Cash.tbPayment.PaidOutValue * -1) + Cash.tbPayment.PaidInValue AS TaxPaid
		FROM Cash.tbPayment 
			JOIN Cash.tbTaxType tt ON Cash.tbPayment.CashCode = tt.CashCode
		WHERE (tt.TaxTypeCode = 0)

	), tax_statement AS
	(
		SELECT StartOn, TaxDue, TaxPaid,
			SUM(TaxDue + TaxPaid) OVER (ORDER BY StartOn, TaxDue ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
		FROM tax_entries
	), profit_statement AS
	(
		SELECT tax_statement.StartOn, CAST(TaxDue AS decimal(18, 5)) TaxDue, CAST(Balance AS decimal(18, 5)) TaxBalance,  
			CAST(Balance / CorporationTaxRate AS decimal(18, 5)) LossesCarriedForward
		FROM tax_statement 
			JOIN App.tbYearPeriod yp ON tax_statement.StartOn = yp.StartOn
		WHERE tax_statement.StartOn >= (SELECT MIN(StartOn) FROM App.tbYearPeriod p JOIN App.tbYear y ON p.YearNumber = y.YearNumber  WHERE y.CashStatusCode < 3)
	)
	SELECT CONCAT(y.[Description], ' ', mn.MonthName) YearEndDescription,
		profit_statement.StartOn, TaxDue, TaxBalance, 
		CASE WHEN LossesCarriedForward < 0 THEN ABS(LossesCarriedForward) ELSE 0 END LossesCarriedForward		
	FROM profit_statement
		JOIN App.tbYearPeriod yp ON profit_statement.StartOn = yp.StartOn
		JOIN App.tbYear y ON yp.YearNumber - 1 = y.YearNumber
		JOIN App.tbMonth mn ON yp.MonthNumber = mn.MonthNumber;
go
PRINT N'Creating View [Cash].[vwTaxVatAuditInvoices]...';


go
CREATE VIEW Cash.vwTaxVatAuditInvoices
AS
	WITH vat_transactions AS
	(
		SELECT   Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbItem.TaxCode, Invoice.tbItem.InvoiceValue, Invoice.tbItem.TaxValue,
								  ROUND((Invoice.tbItem.TaxValue /  Invoice.tbItem.InvoiceValue), 3) As CalcRate,
								 App.tbTaxCode.TaxRate, Subject.tbSubject.EUJurisdiction, Invoice.tbItem.CashCode AS IdentityCode, Cash.tbCode.CashDescription As ItemDescription
		FROM            Invoice.tbItem INNER JOIN
								 Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
								 Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
								 App.tbTaxCode ON Invoice.tbItem.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
								 Cash.tbCode ON Invoice.tbItem.CashCode = Cash.tbCode.CashCode
		WHERE        (App.tbTaxCode.TaxTypeCode = 1) AND (Invoice.tbItem.InvoiceValue <> 0)
		UNION
		SELECT   Invoice.tbInvoice.InvoicedOn, Invoice.tbProject.InvoiceNumber, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbProject.TaxCode, Invoice.tbProject.InvoiceValue, Invoice.tbProject.TaxValue, 
								 ROUND(Invoice.tbProject.TaxValue / Invoice.tbProject.InvoiceValue, 3) AS CalcRate, App.tbTaxCode.TaxRate, Subject.tbSubject.EUJurisdiction, Invoice.tbProject.ProjectCode AS IdentityCode, tbProject_1.ProjectTitle As ItemDescription
		FROM            Invoice.tbProject INNER JOIN
								 Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
								 Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
								 App.tbTaxCode ON Invoice.tbProject.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
								 Project.tbProject AS tbProject_1 ON Invoice.tbProject.ProjectCode = tbProject_1.ProjectCode
		WHERE        (App.tbTaxCode.TaxTypeCode = 1) AND (Invoice.tbProject.InvoiceValue <> 0)
	)
	, vat_dataset AS
	(
		SELECT  (SELECT PayTo FROM Cash.fnTaxTypeDueDates(1) due_dates WHERE vat_transactions.InvoicedOn >= PayFrom AND vat_transactions.InvoicedOn < PayTo) AS StartOn,
		 vat_transactions.InvoicedOn, InvoiceNumber, invoice_type.InvoiceType, vat_transactions.InvoiceTypeCode, TaxCode, InvoiceValue, TaxValue, TaxRate, EUJurisdiction, IdentityCode, ItemDescription,
				CASE WHEN EUJurisdiction = 0 THEN CASE vat_transactions.InvoiceTypeCode WHEN 0 THEN InvoiceValue WHEN 1 THEN
				InvoiceValue * - 1 ELSE 0 END ELSE 0 END AS HomeSales, 
				CASE WHEN EUJurisdiction = 0 THEN CASE vat_transactions.InvoiceTypeCode WHEN 2 THEN InvoiceValue WHEN 3 THEN
				InvoiceValue * - 1 ELSE 0 END ELSE 0 END AS HomePurchases, 
				CASE WHEN EUJurisdiction != 0 THEN CASE vat_transactions.InvoiceTypeCode WHEN 0 THEN InvoiceValue WHEN 1 THEN
				InvoiceValue * - 1 ELSE 0 END ELSE 0 END AS ExportSales, 
				CASE WHEN EUJurisdiction != 0 THEN CASE vat_transactions.InvoiceTypeCode WHEN 2 THEN InvoiceValue WHEN 3 THEN
				InvoiceValue * - 1 ELSE 0 END ELSE 0 END AS ExportPurchases, 
				CASE WHEN EUJurisdiction = 0 THEN CASE vat_transactions.InvoiceTypeCode WHEN 0 THEN TaxValue WHEN 1 THEN TaxValue
				* - 1 ELSE 0 END ELSE 0 END AS HomeSalesVat, 
				CASE WHEN EUJurisdiction = 0 THEN CASE vat_transactions.InvoiceTypeCode WHEN 2 THEN TaxValue WHEN 3 THEN TaxValue
				* - 1 ELSE 0 END ELSE 0 END AS HomePurchasesVat, 
				CASE WHEN EUJurisdiction != 0 THEN CASE vat_transactions.InvoiceTypeCode WHEN 0 THEN TaxValue WHEN 1 THEN TaxValue
				* - 1 ELSE 0 END ELSE 0 END AS ExportSalesVat, 
				CASE WHEN EUJurisdiction != 0 THEN CASE vat_transactions.InvoiceTypeCode WHEN 2 THEN TaxValue WHEN 3 THEN TaxValue
				* - 1 ELSE 0 END ELSE 0 END AS ExportPurchasesVat
		FROM  vat_transactions 
			JOIN Invoice.tbType invoice_type ON vat_transactions.InvoiceTypeCode = invoice_type.InvoiceTypeCode
	)
	SELECT CONCAT(App.tbYear.Description, ' ', App.tbMonth.MonthName) AS YearPeriod, vat_dataset.*,
		 (HomeSalesVat + ExportSalesVat) - (HomePurchasesVat + ExportPurchasesVat) AS VatDue
	FROM vat_dataset
		JOIN App.tbYearPeriod AS year_period ON vat_dataset.StartOn = year_period.StartOn INNER JOIN
                         App.tbYear ON year_period.YearNumber = App.tbYear.YearNumber INNER JOIN
                         App.tbMonth ON year_period.MonthNumber = App.tbMonth.MonthNumber;
go
PRINT N'Creating View [Cash].[vwStatementBase]...';


go
CREATE   VIEW Cash.vwStatementBase
AS
	--invoiced taxes
	WITH corp_taxcode AS
	(
		SELECT TOP (1) SubjectCode, CashCode 
		FROM Cash.tbTaxType WHERE (TaxTypeCode = 0)
	), corptax_invoiced_entries AS
	(
		SELECT SubjectCode, CashCode, StartOn, TaxDue, Balance,
			ROW_NUMBER() OVER (ORDER BY StartOn) AS RowNumber 
		FROM Cash.vwTaxCorpStatement CROSS JOIN corp_taxcode
		WHERE (Balance <> 0) AND (StartOn >= (SELECT MIN(StartOn) FROM App.tbYearPeriod WHERE CashStatusCode < 2)) --AND (TaxDue > 0) 
	), corptax_invoiced_owing AS
	(
		SELECT SubjectCode, CashCode EntryDescription, StartOn AS TransactOn, 4 AS CashEntryTypeCode, 
			(SELECT CAST(Message AS NVARCHAR) FROM App.tbText WHERE TextId = 1214) ReferenceCode, 0 AS PayIn,
			CASE RowNumber WHEN 1 THEN Balance ELSE TaxDue END AS PayOut
		FROM corptax_invoiced_entries
	), vat_taxcode AS
	(
		SELECT TOP (1) SubjectCode, CashCode 
		FROM Cash.tbTaxType WHERE (TaxTypeCode = 1)
	), vat_totals AS
	(
		SELECT ROW_NUMBER() OVER (ORDER BY RowNumber DESC) AS Id, StartOn AS TransactOn, VatDue,
			CASE WHEN VatPaid  < 0 OR Balance <= 0 THEN NULL ELSE 1 END IsLive
		FROM Cash.vwTaxVatStatement
		--WHERE VatDue <> 0
	), vat_invoiced_owing AS
	(
		SELECT SubjectCode, CashCode EntryDescription, TransactOn, 5 AS CashEntryTypeCode, 
			(SELECT CAST(Message AS NVARCHAR) FROM App.tbText WHERE TextId = 1214) ReferenceCode, 
			CASE WHEN VatDue < 0 THEN ABS(VatDue) ELSE 0 END AS PayIn,
			CASE WHEN VatDue >= 0 THEN VatDue ELSE 0 END AS PayOut
		FROM vat_totals CROSS JOIN vat_taxcode
		WHERE Id <  COALESCE((SELECT TOP 1 t.Id FROM vat_totals t WHERE t.IsLive IS NULL ORDER BY Id), (SELECT MIN(Id) + 1 FROM vat_totals))
		--(SELECT TOP 1 t.Id FROM vat_totals t WHERE t.IsLive IS NULL ORDER BY Id)
	)
	--uninvoiced taxes
	,  corptax_dates AS
	(
		SELECT PayOn, PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(0)
	), corptax_accrual_entries AS
	(
		SELECT StartOn, SUM(TaxDue) AS TaxDue
		FROM Cash.vwTaxCorpAccruals
		GROUP BY StartOn
	), corptax_accrual_candidates AS
	(
			SELECT (SELECT PayOn FROM corptax_dates WHERE corptax_accrual_entries.StartOn >= PayFrom AND corptax_accrual_entries.StartOn < PayTo) AS TransactOn, TaxDue			
		FROM corptax_accrual_entries 
	), corptax_accrual_totals AS
	(
		SELECT TransactOn, SUM(TaxDue) AS TaxDue
		FROM corptax_accrual_candidates
		GROUP BY TransactOn
	)	
	, corptax_accruals AS
	(	
		SELECT SubjectCode, CashCode EntryDescription, TransactOn, 4 AS CashEntryTypeCode, 
				(SELECT CAST(Message AS NVARCHAR) FROM App.tbText WHERE TextId = 1215) ReferenceCode, 
				CASE WHEN TaxDue < 0 THEN ABS(TaxDue) ELSE 0 END AS PayIn,
				CASE WHEN TaxDue >= 0 THEN TaxDue ELSE 0 END AS PayOut
		FROM corptax_accrual_totals CROSS JOIN corp_taxcode
	), vat_dates AS
	(
		SELECT PayOn, PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(1)
	), vat_accrual_entries AS
	(
		SELECT StartOn, SUM(VatDue) AS TaxDue 
		FROM Cash.vwTaxVatAccruals vat_audit
		WHERE vat_audit.VatDue <> 0
		GROUP BY StartOn
	), vat_accrual_candidates AS
	(
		SELECT (SELECT PayOn FROM vat_dates WHERE vat_accrual_entries.StartOn >= PayFrom AND vat_accrual_entries.StartOn < PayTo) AS TransactOn, TaxDue			
		FROM vat_accrual_entries 
	), vat_accrual_totals AS
	(
		SELECT TransactOn, SUM(TaxDue) AS TaxDue
		FROM vat_accrual_candidates
		GROUP BY TransactOn
	), vat_accruals AS
	(
		SELECT vat_taxcode.SubjectCode, vat_taxcode.CashCode EntryDescription, TransactOn, 5 AS CashEntryTypeCode, 
				(SELECT CAST(Message AS NVARCHAR) FROM App.tbText WHERE TextId = 1215) ReferenceCode,
				CASE WHEN TaxDue < 0 THEN ABS(TaxDue) ELSE 0 END AS PayIn,
				CASE WHEN TaxDue >= 0 THEN TaxDue ELSE 0 END AS PayOut
		FROM vat_accrual_totals
			CROSS JOIN vat_taxcode
	)
	--unpaid invoices
	, invoice_desc_candidates AS
	(
		SELECT invoice_Projects.InvoiceNumber, 0 OrderBy, 
			FIRST_VALUE(invoiced_Project.ObjectCode) OVER (PARTITION BY invoice_Projects.InvoiceNumber ORDER BY invoice_Projects.ProjectCode) EntryDescription
		FROM Invoice.tbProject invoice_Projects 
			JOIN Project.tbProject invoiced_Project ON invoice_Projects.ProjectCode = invoiced_Project.ProjectCode
			JOIN Invoice.tbInvoice invoices ON invoices.InvoiceNumber = invoice_Projects.InvoiceNumber
		WHERE  (InvoiceStatusCode BETWEEN 1 AND 2)
		UNION
		SELECT invoice_items.InvoiceNumber, 1 OrderBy, 
			FIRST_VALUE(cash_code.CashDescription) OVER (PARTITION BY invoice_items.InvoiceNumber ORDER BY invoice_items.CashCode) EntryDescription
		FROM Invoice.tbItem invoice_items 
			JOIN Cash.tbCode cash_code ON invoice_items.CashCode = cash_code.CashCode
			JOIN Invoice.tbInvoice invoices ON invoices.InvoiceNumber = invoice_items.InvoiceNumber
		WHERE  (InvoiceStatusCode BETWEEN 1 AND 2)
	), invoice_desc AS
	(
		SELECT InvoiceNumber,
			FIRST_VALUE(EntryDescription) OVER (PARTITION BY InvoiceNumber ORDER BY OrderBy) EntryDescription
		FROM invoice_desc_candidates
	), invoices_outstanding AS
	(
		SELECT  invoices.SubjectCode, invoice_desc.EntryDescription, invoices.ExpectedOn AS TransactOn, 1 AS CashEntryTypeCode, invoices.InvoiceNumber AS ReferenceCode, 
					CASE CashPolarityCode WHEN 1 THEN InvoiceValue + TaxValue - (PaidValue + PaidTaxValue) ELSE 0 END AS PayIn, 
					CASE CashPolarityCode WHEN 0 THEN (InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue) ELSE 0 END AS PayOut
		FROM  Invoice.tbInvoice invoices
			JOIN Invoice.tbType invoice_type ON invoices.InvoiceTypeCode = invoice_type.InvoiceTypeCode
			JOIN invoice_desc ON invoices.InvoiceNumber = invoice_desc.InvoiceNumber
		WHERE  (InvoiceStatusCode < 3) AND ((InvoiceValue + TaxValue - PaidValue + PaidTaxValue) > 0)
	), Project_invoiced_quantity AS
	(
		SELECT        Invoice.tbProject.ProjectCode, SUM(Invoice.tbProject.Quantity) AS InvoiceQuantity
		FROM            Invoice.tbProject INNER JOIN
								 Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE        (Invoice.tbInvoice.InvoiceTypeCode = 0) OR
								 (Invoice.tbInvoice.InvoiceTypeCode = 2)
		GROUP BY Invoice.tbProject.ProjectCode
	), Projects_confirmed AS
	(
		SELECT Project.tbProject.ProjectCode AS ReferenceCode, Project.tbProject.SubjectCode, Project.tbProject.PaymentOn AS TransactOn, Project.tbProject.PaymentOn, 2 AS CashEntryTypeCode, 
								 CASE WHEN Cash.tbCategory.CashPolarityCode = 0 THEN (Project.tbProject.UnitCharge + Project.tbProject.UnitCharge * App.tbTaxCode.TaxRate) * (Project.tbProject.Quantity - ISNULL(Project_invoiced_quantity.InvoiceQuantity, 
								 0)) ELSE 0 END AS PayOut, CASE WHEN Cash.tbCategory.CashPolarityCode = 1 THEN (Project.tbProject.UnitCharge + Project.tbProject.UnitCharge * App.tbTaxCode.TaxRate) 
								 * (Project.tbProject.Quantity - ISNULL(Project_invoiced_quantity.InvoiceQuantity, 0)) ELSE 0 END AS PayIn, Project.tbProject.ObjectCode EntryDescription
		FROM            App.tbTaxCode INNER JOIN
								 Project.tbProject ON App.tbTaxCode.TaxCode = Project.tbProject.TaxCode INNER JOIN
								 Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
								 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode LEFT OUTER JOIN
								 Project_invoiced_quantity ON Project.tbProject.ProjectCode = Project_invoiced_quantity.ProjectCode
		WHERE        (Project.tbProject.ProjectStatusCode > 0) AND (Project.tbProject.ProjectStatusCode < 3) AND (Project.tbProject.Quantity - ISNULL(Project_invoiced_quantity.InvoiceQuantity, 0) > 0)
	)
	--interbank transfers
	, transfer_current_account AS
	(
		SELECT        Subject.tbAccount.AccountCode
		FROM            Subject.tbAccount INNER JOIN
								 Cash.tbCode ON Subject.tbAccount.CashCode = Cash.tbCode.CashCode INNER JOIN
								 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode AND Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
		WHERE        (Cash.tbCategory.CashTypeCode = 2)
	), transfer_accruals AS
	(
		SELECT        Cash.tbPayment.SubjectCode, Cash.tbPayment.CashCode EntryDescription, Cash.tbPayment.PaidOn AS TransactOn, Cash.tbPayment.PaymentCode AS ReferenceCode, 
			6 AS CashEntryTypeCode, Cash.tbPayment.PaidInValue AS PayIn, Cash.tbPayment.PaidOutValue AS PayOut
		FROM            transfer_current_account INNER JOIN
								 Cash.tbPayment ON transfer_current_account.AccountCode = Cash.tbPayment.AccountCode
		WHERE        (Cash.tbPayment.PaymentStatusCode = 2)
	)
	SELECT SubjectCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut FROM corptax_invoiced_owing
	UNION
	SELECT SubjectCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut FROM vat_invoiced_owing
	UNION
	SELECT SubjectCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut FROM corptax_accruals
	UNION
	SELECT SubjectCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut FROM vat_accruals
	UNION
	SELECT SubjectCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut FROM invoices_outstanding
	UNION 
	SELECT SubjectCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut FROM Projects_confirmed
	UNION
	SELECT SubjectCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut FROM transfer_accruals;
go
PRINT N'Creating View [App].[vwActivePeriod]...';


go

CREATE   VIEW App.vwActivePeriod
AS
SELECT App.tbYearPeriod.YearNumber, App.tbYearPeriod.StartOn, App.tbYear.Description, App.tbMonth.MonthNumber, App.tbMonth.MonthName, fnActivePeriod.EndOn
FROM            App.tbYear INNER JOIN
                         App.fnActivePeriod() AS fnActivePeriod INNER JOIN
                         App.tbYearPeriod INNER JOIN
                         App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber ON fnActivePeriod.StartOn = App.tbYearPeriod.StartOn AND fnActivePeriod.YearNumber = App.tbYearPeriod.YearNumber ON 
                         App.tbYear.YearNumber = App.tbYearPeriod.YearNumber
go
PRINT N'Creating View [App].[vwMonths]...';


go

CREATE     VIEW [App].[vwMonths]
AS
	SELECT DISTINCT CAST(App.tbYearPeriod.StartOn AS decimal) AS StartOn, App.tbMonth.MonthName, App.tbYearPeriod.MonthNumber
	FROM         App.tbYearPeriod INNER JOIN
						  App.fnActivePeriod() AS fnSystemActivePeriod ON App.tbYearPeriod.YearNumber = fnSystemActivePeriod.YearNumber INNER JOIN
						  App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
go
PRINT N'Creating View [Subject].[vwBalanceSheetAudit]...';


go
CREATE   VIEW Subject.vwBalanceSheetAudit
AS
	SELECT        App.tbYear.YearNumber, App.tbYear.Description, App.tbMonth.MonthName, Subject.tbSubject.SubjectCode, Subject.tbSubject.SubjectName, Subject.tbType.SubjectType, Cash.tbPolarity.CashPolarity, Cash.tbAssetType.AssetTypeCode, 
							 Cash.tbAssetType.AssetType, Subject.vwAssetBalances.StartOn, Subject.vwAssetBalances.Balance
	FROM            Subject.vwAssetBalances INNER JOIN
							 Cash.tbAssetType ON Subject.vwAssetBalances.AssetTypeCode = Cash.tbAssetType.AssetTypeCode INNER JOIN
							 Subject.tbSubject ON Subject.vwAssetBalances.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 App.tbYearPeriod ON Subject.vwAssetBalances.StartOn = App.tbYearPeriod.StartOn INNER JOIN
							 Subject.tbType ON Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode AND Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode INNER JOIN
							 Cash.tbPolarity ON Subject.tbType.CashPolarityCode = Cash.tbPolarity.CashPolarityCode AND Subject.tbType.CashPolarityCode = Cash.tbPolarity.CashPolarityCode INNER JOIN
							 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND App.tbYearPeriod.YearNumber = App.tbYear.YearNumber AND 
							 App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
							 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
	WHERE        (Subject.vwAssetBalances.Balance <> 0) AND (Subject.vwAssetBalances.StartOn <= (SELECT TOP (1) StartOn FROM App.vwActivePeriod));
go
PRINT N'Creating View [Project].[vwOpBucket]...';


go

CREATE   VIEW Project.vwOpBucket
AS
SELECT        op.ProjectCode, op.OperationNumber, op.EndOn, buckets.Period, buckets.BucketId
FROM            Project.tbOp AS op CROSS APPLY
			(	SELECT  buckets.Period, buckets.BucketId
				FROM        App.fnBuckets(CURRENT_TIMESTAMP) buckets 
				WHERE     (StartDate <= op.EndOn) AND (EndDate > op.EndOn)) AS buckets
go
PRINT N'Creating View [Project].[vwBucket]...';


go
CREATE   VIEW Project.vwBucket
AS
SELECT        Project.ProjectCode, Project.ActionOn, buckets.Period, buckets.BucketId
FROM            Project.tbProject AS Project CROSS APPLY
			(	SELECT  buckets.Period, buckets.BucketId
				FROM        App.fnBuckets(CURRENT_TIMESTAMP) buckets 
				WHERE     (StartDate <= Project.ActionOn) AND (EndDate > Project.ActionOn)) AS buckets
go
PRINT N'Creating View [Project].[vwOps]...';


go
CREATE VIEW Project.vwOps
AS
SELECT        Project.tbOp.ProjectCode, Project.tbProject.ObjectCode, Project.tbOp.OperationNumber, Project.vwOpBucket.Period, Project.vwOpBucket.BucketId, Project.tbOp.UserId, Project.tbOp.SyncTypeCode, Project.tbOp.OpStatusCode, 
                         Project.tbOp.Operation, Project.tbOp.Note, Project.tbOp.StartOn, Project.tbOp.EndOn, Project.tbOp.Duration, Project.tbOp.OffsetDays, Project.tbOp.InsertedBy, Project.tbOp.InsertedOn, Project.tbOp.UpdatedBy, Project.tbOp.UpdatedOn, 
                         Project.tbProject.ProjectTitle, Project.tbProject.ProjectStatusCode, Project.tbStatus.ProjectStatus, Project.tbProject.ActionOn, Project.tbProject.Quantity, Cash.tbCode.CashDescription, Project.tbProject.TotalCharge, Project.tbProject.SubjectCode, 
                         Subject.tbSubject.SubjectName, Project.tbOp.RowVer AS OpRowVer, Project.tbProject.RowVer AS ProjectRowVer
FROM            Project.tbOp INNER JOIN
                         Project.tbProject ON Project.tbOp.ProjectCode = Project.tbProject.ProjectCode INNER JOIN
                         Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
                         Project.tbStatus ON Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode INNER JOIN
                         Project.vwOpBucket ON Project.tbOp.ProjectCode = Project.vwOpBucket.ProjectCode AND Project.tbOp.OperationNumber = Project.vwOpBucket.OperationNumber LEFT OUTER JOIN
                         Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode
go
PRINT N'Creating View [Project].[vwProjects]...';


go
CREATE VIEW Project.vwProjects
AS
	SELECT        Project.tbProject.ProjectCode, Project.tbProject.UserId, Project.tbProject.SubjectCode, Project.tbProject.ContactName, Project.tbProject.ObjectCode, Project.tbProject.ProjectTitle, Project.tbProject.ProjectStatusCode, Project.tbProject.ActionById, 
							 Project.tbProject.ActionOn, Project.tbProject.ActionedOn, Project.tbProject.PaymentOn, Project.tbProject.SecondReference, Project.tbProject.ProjectNotes, Project.tbProject.TaxCode, Project.tbProject.Quantity, Project.tbProject.UnitCharge, 
							 Project.tbProject.TotalCharge, Project.tbProject.AddressCodeFrom, Project.tbProject.AddressCodeTo, Project.tbProject.Printed, Project.tbProject.Spooled, Project.tbProject.InsertedBy, Project.tbProject.InsertedOn, Project.tbProject.UpdatedBy, 
							 Project.tbProject.UpdatedOn, Project.vwBucket.Period, Project.vwBucket.BucketId, ProjectStatus.ProjectStatus, Project.tbProject.CashCode, Cash.tbCode.CashDescription, tbUser_1.UserName AS OwnerName, 
							 Usr.tbUser.UserName AS ActionName, Subject.tbSubject.SubjectName, SubjectStatus.SubjectStatus, Subject.tbType.SubjectType, CASE WHEN Cash.tbCategory.CategoryCode IS NULL 
							 THEN Subject.tbType.CashPolarityCode ELSE Cash.tbCategory.CashPolarityCode END AS CashPolarityCode, Project.tbProject.RowVer
	FROM            Usr.tbUser INNER JOIN
							 Project.tbStatus AS ProjectStatus INNER JOIN
							 Subject.tbType INNER JOIN
							 Subject.tbSubject ON Subject.tbType.SubjectTypeCode = Subject.tbSubject.SubjectTypeCode INNER JOIN
							 Subject.tbStatus AS SubjectStatus ON Subject.tbSubject.SubjectStatusCode = SubjectStatus.SubjectStatusCode INNER JOIN
							 Project.tbProject ON Subject.tbSubject.SubjectCode = Project.tbProject.SubjectCode ON ProjectStatus.ProjectStatusCode = Project.tbProject.ProjectStatusCode ON Usr.tbUser.UserId = Project.tbProject.ActionById INNER JOIN
							 Usr.tbUser AS tbUser_1 ON Project.tbProject.UserId = tbUser_1.UserId INNER JOIN
							 Project.vwBucket ON Project.tbProject.ProjectCode = Project.vwBucket.ProjectCode LEFT OUTER JOIN
							 Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode LEFT OUTER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
go
PRINT N'Creating View [Project].[vwPurchases]...';


go
CREATE VIEW Project.vwPurchases
AS
	SELECT        Project.vwProjects.ProjectCode, Project.vwProjects.ObjectCode, Project.vwProjects.ProjectStatusCode, Project.vwProjects.ActionOn, Project.vwProjects.ActionById, Project.vwProjects.ProjectTitle, Project.vwProjects.Period, Project.vwProjects.BucketId, 
							 Project.vwProjects.SubjectCode, Project.vwProjects.ContactName, Project.vwProjects.ProjectStatus, Project.vwProjects.ProjectNotes, Project.vwProjects.ActionedOn, Project.vwProjects.OwnerName, Project.vwProjects.CashCode, 
							 Project.vwProjects.CashDescription, Project.vwProjects.Quantity, Object.tbObject.UnitOfMeasure, Project.vwProjects.UnitCharge, Project.vwProjects.TotalCharge, Subject_tbAddress_1.Address AS FromAddress, 
							 Subject.tbAddress.Address AS ToAddress, Project.vwProjects.Printed, Project.vwProjects.InsertedBy, Project.vwProjects.InsertedOn, Project.vwProjects.UpdatedBy, Project.vwProjects.UpdatedOn, Project.vwProjects.SubjectName, 
							 Project.vwProjects.ActionName, Project.vwProjects.SecondReference
	FROM            Project.vwProjects LEFT OUTER JOIN
							 Subject.tbAddress AS Subject_tbAddress_1 ON Project.vwProjects.AddressCodeFrom = Subject_tbAddress_1.AddressCode LEFT OUTER JOIN
							 Subject.tbAddress ON Project.vwProjects.AddressCodeTo = Subject.tbAddress.AddressCode INNER JOIN
							 Object.tbObject ON Project.vwProjects.ObjectCode = Object.tbObject.ObjectCode
	WHERE        (Project.vwProjects.CashCode IS NOT NULL) AND (Project.vwProjects.CashPolarityCode = 0);
go
PRINT N'Creating View [Project].[vwQuotes]...';


go
CREATE   VIEW Project.vwQuotes
AS
	SELECT        Project.tbProject.UserId, Cash.tbCategory.CashPolarityCode, Cash.tbPolarity.CashPolarity, Project.tbProject.ActionOn, Project.tbProject.ProjectCode, Project.tbProject.SubjectCode, Project.tbProject.ContactName, Project.tbProject.ObjectCode, 
							 Project.tbProject.ProjectTitle, Project.tbProject.SecondReference, Project.tbProject.TaxCode, Project.tbProject.Quantity, Project.tbProject.UnitCharge, Project.tbProject.TotalCharge, Project.vwBucket.Period, Project.vwBucket.BucketId, Project.tbProject.CashCode, 
							 Cash.tbCode.CashDescription, tbUser_1.UserName AS OwnerName, Subject.tbSubject.SubjectName, Project.tbProject.RowVer
	FROM            Subject.tbSubject INNER JOIN
							 Project.tbProject ON Subject.tbSubject.SubjectCode = Project.tbProject.SubjectCode INNER JOIN
							 Usr.tbUser AS tbUser_1 ON Project.tbProject.UserId = tbUser_1.UserId INNER JOIN
							 Project.vwBucket ON Project.tbProject.ProjectCode = Project.vwBucket.ProjectCode INNER JOIN
							 Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
							 Cash.tbPolarity ON Cash.tbCategory.CashPolarityCode = Cash.tbPolarity.CashPolarityCode
	WHERE        (Project.tbProject.ProjectStatusCode = 0);
go
PRINT N'Creating View [Invoice].[vwCandidatePurchases]...';


go
CREATE VIEW Invoice.vwCandidatePurchases
AS
	SELECT TOP 100 PERCENT  ProjectCode, SubjectCode, ContactName, ObjectCode, ActionOn, ActionedOn, Quantity, UnitCharge, TotalCharge, ProjectTitle, ProjectNotes, CashDescription, ActionName, OwnerName, ProjectStatus, InsertedBy, 
							 InsertedOn, UpdatedBy, UpdatedOn, ProjectStatusCode
	FROM            Project.vwProjects
	WHERE        (ProjectStatusCode = 1 OR
							 ProjectStatusCode = 2) AND (CashPolarityCode = 0) AND (CashCode IS NOT NULL)
	ORDER BY ActionOn;
go
PRINT N'Creating View [Invoice].[vwCandidateSales]...';


go
CREATE VIEW Invoice.vwCandidateSales
AS
	SELECT TOP 100 PERCENT ProjectCode, SubjectCode, ContactName, ObjectCode, ActionOn, ActionedOn, ProjectTitle, Quantity, UnitCharge, TotalCharge, ProjectNotes, CashDescription, ActionName, OwnerName, ProjectStatus, InsertedBy, 
							 InsertedOn, UpdatedBy, UpdatedOn, ProjectStatusCode
	FROM            Project.vwProjects
	WHERE        (ProjectStatusCode = 1 OR
							 ProjectStatusCode = 2) AND (CashPolarityCode = 1) AND (CashCode IS NOT NULL)
	ORDER BY ActionOn;
go
PRINT N'Creating View [Cash].[vwBalanceSheet]...';


go
CREATE VIEW Cash.vwBalanceSheet
AS
	WITH balance_sheets AS
	(

		SELECT AssetCode, AssetName, CashPolarityCode, AssetTypeCode, StartOn, Balance FROM Cash.vwBalanceSheetSubjects
		UNION
		SELECT AssetCode, AssetName, CashPolarityCode, AssetTypeCode, StartOn, Balance FROM Cash.vwBalanceSheetAccounts
		UNION 
		SELECT AssetCode, AssetName, CashPolarityCode, AssetTypeCode, StartOn, Balance FROM Cash.vwBalanceSheetAssets
		UNION 
		SELECT AssetCode, AssetName, CashPolarityCode, AssetTypeCode, StartOn, Balance FROM Cash.vwBalanceSheetTax
		UNION
		SELECT AssetCode, AssetName, CashPolarityCode, AssetTypeCode, StartOn, Balance FROM Cash.vwBalanceSheetVat

	), balance_sheet_unordered AS
	(
		SELECT 
			balance_sheet_periods.AssetCode, balance_sheet_periods.AssetName,
			CASE WHEN balance_sheets.AssetCode IS NULL 
				THEN balance_sheet_periods.CashPolarityCode 
				ELSE balance_sheets.CashPolarityCode 
			END CashPolarityCode, LiquidityLevel,
			balance_sheet_periods.StartOn,
			CASE WHEN balance_sheets.AssetCode IS NULL 
				THEN 0 
				ELSE balance_sheets.Balance 
			END Balance,
			CASE WHEN balance_sheets.AssetCode IS NULL 
				THEN balance_sheet_periods.IsEntry 
				ELSE CAST(1 as bit) 
			END IsEntry
		FROM Cash.vwBalanceSheetPeriods balance_sheet_periods
			LEFT OUTER JOIN balance_sheets
				ON balance_sheet_periods.AssetCode = balance_sheets.AssetCode
					AND balance_sheet_periods.AssetName = balance_sheets.AssetName
					AND balance_sheet_periods.CashPolarityCode = balance_sheets.CashPolarityCode
					AND balance_sheet_periods.StartOn = balance_sheets.StartOn
	), balance_sheet_ordered AS
	(
		SELECT 
			ROW_NUMBER() OVER (ORDER BY CashPolarityCode desc, LiquidityLevel desc, AssetName, StartOn) EntryNumber,
			AssetCode, AssetName, CashPolarityCode, LiquidityLevel, StartOn, Balance, IsEntry
		FROM balance_sheet_unordered
	), balance_sheet_ranked AS
	(
		SELECT *, 
		RANK() OVER (PARTITION BY AssetName, CashPolarityCode, IsEntry ORDER BY EntryNumber) RNK
		FROM balance_sheet_ordered
	), balance_sheet_grouped AS
	(
		SELECT EntryNumber, AssetCode, AssetName, CashPolarityCode, LiquidityLevel, StartOn, Balance, IsEntry,
		MAX(CASE IsEntry WHEN 0 THEN 0 ELSE RNK END) OVER (PARTITION BY AssetName, CashPolarityCode ORDER BY EntryNumber) RNK
		FROM balance_sheet_ranked
	)
	SELECT EntryNumber, AssetCode, AssetName, CashPolarityCode, LiquidityLevel, balance_sheet_grouped.StartOn, 
		year_period.YearNumber, year_period.MonthNumber, IsEntry,
		CASE IsEntry WHEN 0 THEN
			MAX(Balance) OVER (PARTITION BY AssetName, CashPolarityCode, RNK ORDER BY EntryNumber) +
			MIN(Balance) OVER (PARTITION BY AssetName, CashPolarityCode, RNK ORDER BY EntryNumber) 
		ELSE
			Balance
		END AS Balance
	FROM balance_sheet_grouped
		JOIN App.tbYearPeriod year_period ON balance_sheet_grouped.StartOn = year_period.StartOn;
go
PRINT N'Creating View [Cash].[vwStatement]...';


go
CREATE VIEW Cash.vwStatement
AS
	WITH statement_base AS
	(
		SELECT ROW_NUMBER() OVER(ORDER BY TransactOn, CashEntryTypeCode DESC) AS RowNumber,
		 SubjectCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut 
		 FROM Cash.vwStatementBase
	), opening_balance AS
	(	
		SELECT SUM( Subject.tbAccount.CurrentBalance) AS OpeningBalance
		FROM         Subject.tbAccount INNER JOIN
							  Cash.tbCode ON Subject.tbAccount.CashCode = Cash.tbCode.CashCode
		WHERE     ( Subject.tbAccount.AccountClosed = 0) AND (Subject.tbAccount.AccountTypeCode = 0)
	), statement_data AS
	(
		SELECT 
			0 AS RowNumber,
			(SELECT TOP (1) SubjectCode FROM App.tbOptions) AS SubjectCode,
			NULL AS EntryDescription,
			NULL AS TransactOn,    
			(SELECT CAST(Message AS NVARCHAR) FROM App.tbText WHERE TextId = 3013) AS ReferenceCode,	
			1 AS CashEntryTypeCode,
			PayIn = (SELECT OpeningBalance FROM opening_balance),
			0 AS PayOut
		UNION 
		SELECT RowNumber, SubjectCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut 
		FROM statement_base
	), company_statement AS
	(
		SELECT RowNumber, SubjectCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut,
			SUM(PayIn + (PayOut * -1)) OVER (ORDER BY RowNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
		FROM statement_data
	)
	SELECT RowNumber, cs.SubjectCode, Subject.SubjectName, cs.EntryDescription,
			TransactOn, ReferenceCode, cs.CashEntryTypeCode, et.CashEntryType, CAST(PayIn AS decimal(18, 5)) PayIn, CAST(PayOut AS decimal(18, 5)) PayOut, CAST(Balance AS decimal(18, 5)) Balance
	FROM company_statement cs 
		JOIN Subject.tbSubject Subject ON cs.SubjectCode = Subject.SubjectCode
		JOIN Cash.tbEntryType et ON cs.CashEntryTypeCode = et.CashEntryTypeCode;
go
PRINT N'Creating View [Cash].[vwStatementWhatIf]...';


go
CREATE   VIEW Cash.vwStatementWhatIf
AS
	WITH quotes AS
	(
		SELECT Project.tbProject.ProjectCode AS ReferenceCode, 
			Project.tbProject.SubjectCode, Project.tbProject.PaymentOn AS TransactOn, 
			Project.tbProject.PaymentOn, 3 AS CashEntryTypeCode, 
			CASE WHEN Cash.tbCategory.CashPolarityCode = 0 
				THEN (Project.tbProject.UnitCharge + Project.tbProject.UnitCharge * App.tbTaxCode.TaxRate) * Project.tbProject.Quantity 
				ELSE 0 
			END AS PayOut, 
			CASE WHEN Cash.tbCategory.CashPolarityCode = 1 
				THEN (Project.tbProject.UnitCharge + Project.tbProject.UnitCharge * App.tbTaxCode.TaxRate) * Project.tbProject.Quantity ELSE 0 
			END AS PayIn, 
			Project.tbProject.ObjectCode EntryDescription
		FROM Project.vwCostSetProjects quoted_Projects 
			JOIN  Project.tbProject ON quoted_Projects.ProjectCode = Project.tbProject.ProjectCode 		
			JOIN App.tbTaxCode ON App.tbTaxCode.TaxCode = Project.tbProject.TaxCode 
			JOIN Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode 
			JOIN Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
	), cost_set_Project_vat AS
	(
		SELECT  (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= quotes.ActionOn) ORDER BY p.StartOn DESC) AS StartOn,  
				quotes.ProjectCode, quotes.TaxCode,
				quotes.Quantity AS QuantityRemaining,
				quotes.UnitCharge * quotes.Quantity AS TotalValue, 
				quotes.UnitCharge * quotes.Quantity * App.tbTaxCode.TaxRate AS TaxValue,
				App.tbTaxCode.TaxRate,
				Cash.tbCategory.CashPolarityCode
		FROM    Project.vwCostSetProjects cost_set INNER JOIN	Project.tbProject quotes ON cost_set.ProjectCode = quotes.ProjectCode INNER JOIN
				Subject.tbSubject ON quotes.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
				Cash.tbCode ON quotes.CashCode = Cash.tbCode.CashCode INNER JOIN
				Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
				App.tbTaxCode ON quotes.TaxCode = App.tbTaxCode.TaxCode 
		WHERE        (App.tbTaxCode.TaxTypeCode = 1) AND (App.tbTaxCode.TaxTypeCode = 1)
			AND (quotes.ActionOn <= (SELECT DATEADD(d, TaxHorizon, CURRENT_TIMESTAMP) FROM App.tbOptions))
	), cost_set_vat_accruals AS
	(
		SELECT StartOn, ProjectCode, TaxCode, QuantityRemaining, TotalValue, TaxValue, TaxRate,
			CASE CashPolarityCode WHEN 0 THEN TaxValue * -1 ELSE TaxValue END VatDue
		FROM cost_set_Project_vat
	), vat_dates AS
	(
		SELECT PayOn, PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(1)
	), vat_accrual_entries AS
	(
		SELECT StartOn, SUM(VatDue) AS TaxDue 
		FROM cost_set_vat_accruals
		WHERE VatDue <> 0
		GROUP BY StartOn
	), vat_accrual_candidates AS
	(
		SELECT (SELECT PayOn FROM vat_dates WHERE vat_accrual_entries.StartOn >= PayFrom AND vat_accrual_entries.StartOn < PayTo) AS TransactOn, TaxDue			
		FROM vat_accrual_entries 
	), vat_accrual_totals AS
	(
		SELECT TransactOn, SUM(TaxDue) AS TaxDue
		FROM vat_accrual_candidates
		GROUP BY TransactOn
	), vat_taxcode AS
	(
		SELECT TOP (1) SubjectCode, CashCode 
		FROM Cash.tbTaxType WHERE (TaxTypeCode = 1)
	), vat_accruals AS
	(
		SELECT vat_taxcode.SubjectCode, vat_taxcode.CashCode EntryDescription, TransactOn, 5 AS CashEntryTypeCode, 
				(SELECT CashEntryType FROM Cash.tbEntryType WHERE CashEntryTypeCode = 3) ReferenceCode,
				CASE WHEN TaxDue < 0 THEN ABS(TaxDue) ELSE 0 END AS PayIn,
				CASE WHEN TaxDue >= 0 THEN TaxDue ELSE 0 END AS PayOut
		FROM vat_accrual_totals
			CROSS JOIN vat_taxcode
	), cost_set_Project_tax AS
	(
		SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod WHERE (StartOn <= ActionOn) ORDER BY StartOn DESC) AS StartOn, 
			CASE WHEN Cash.tbCategory.CashPolarityCode = 0 THEN quote.TotalCharge * - 1 ELSE quote.TotalCharge END AS TotalCharge
		FROM Project.vwCostSetProjects cost_set INNER JOIN
			Project.tbProject AS quote ON cost_set.ProjectCode = quote.ProjectCode INNER JOIN
								 Cash.tbCode ON quote.CashCode = Cash.tbCode.CashCode INNER JOIN
								 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode AND Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
		WHERE    (quote.ActionOn <= (SELECT DATEADD(d, TaxHorizon, CURRENT_TIMESTAMP) AS HorizonOn FROM App.tbOptions))
	), cost_set_corptax AS
	(
		SELECT cost_set_Project_tax.StartOn, TotalCharge, TotalCharge * CorporationTaxRate AS TaxDue
		FROM cost_set_Project_tax JOIN App.tbYearPeriod year_period ON cost_set_Project_tax.StartOn = year_period.StartOn
	), corptax_dates AS
	(
		SELECT PayOn, PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(0)
	), corptax_accrual_entries AS
	(
		SELECT StartOn, SUM(TaxDue) AS TaxDue
		FROM cost_set_corptax
		GROUP BY StartOn
	), corptax_accrual_candidates AS
	(
			SELECT (SELECT PayOn FROM corptax_dates WHERE corptax_accrual_entries.StartOn >= PayFrom AND corptax_accrual_entries.StartOn < PayTo) AS TransactOn, TaxDue			
		FROM corptax_accrual_entries 
	), corptax_accrual_totals AS
	(
		SELECT TransactOn, SUM(TaxDue) AS TaxDue
		FROM corptax_accrual_candidates
		GROUP BY TransactOn
	), corp_taxcode AS
	(
		SELECT TOP (1) SubjectCode, CashCode 
		FROM Cash.tbTaxType WHERE (TaxTypeCode = 0)
	), corptax_accruals AS
	(	
		SELECT SubjectCode, CashCode EntryDescription, TransactOn, 4 AS CashEntryTypeCode, 
				(SELECT CashEntryType FROM Cash.tbEntryType WHERE CashEntryTypeCode = 3) ReferenceCode, 
				CASE WHEN TaxDue < 0 THEN ABS(TaxDue) ELSE 0 END AS PayIn,
				CASE WHEN TaxDue >= 0 THEN TaxDue ELSE 0 END AS PayOut
		FROM corptax_accrual_totals CROSS JOIN corp_taxcode
	), cost_statement AS
	(
		SELECT SubjectCode, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut, EntryDescription FROM Cash.vwStatementBase
		UNION
		SELECT SubjectCode, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut, EntryDescription FROM quotes
		UNION
		SELECT SubjectCode, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut, EntryDescription FROM vat_accruals
		UNION
		SELECT SubjectCode, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut, EntryDescription FROM corptax_accruals
	), statement_base AS
	(
		SELECT ROW_NUMBER() OVER(ORDER BY TransactOn, CashEntryTypeCode DESC) AS RowNumber,
		 SubjectCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut 
		 FROM cost_statement
	), opening_balance AS
	(	
		SELECT SUM( Subject.tbAccount.CurrentBalance) AS OpeningBalance
		FROM         Subject.tbAccount INNER JOIN
							  Cash.tbCode ON Subject.tbAccount.CashCode = Cash.tbCode.CashCode
		WHERE     ( Subject.tbAccount.AccountClosed = 0) AND (Subject.tbAccount.AccountTypeCode = 0)
	), statement_data AS
	(
		SELECT 
			0 AS RowNumber,
			(SELECT TOP (1) SubjectCode FROM App.tbOptions) AS SubjectCode,
			NULL AS EntryDescription,
			NULL AS TransactOn,    
			(SELECT CAST(Message AS NVARCHAR) FROM App.tbText WHERE TextId = 3013) AS ReferenceCode,	
			1 AS CashEntryTypeCode,
			PayIn = (SELECT OpeningBalance FROM opening_balance),
			0 AS PayOut
		UNION 
		SELECT RowNumber, SubjectCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut 
		FROM statement_base
	), company_statement AS
	(
		SELECT RowNumber, SubjectCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut,
			SUM(PayIn + (PayOut * -1)) OVER (ORDER BY RowNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
		FROM statement_data
	)
	SELECT RowNumber, cs.SubjectCode, Subject.SubjectName, cs.EntryDescription,
			TransactOn, ReferenceCode, cs.CashEntryTypeCode, et.CashEntryType, CAST(PayIn AS decimal(18, 5)) PayIn, CAST(PayOut AS decimal(18, 5)) PayOut, CAST(Balance AS decimal(18, 5)) Balance
	FROM company_statement cs 
		JOIN Subject.tbSubject Subject ON cs.SubjectCode = Subject.SubjectCode
		JOIN Cash.tbEntryType et ON cs.CashEntryTypeCode = et.CashEntryTypeCode;
go
PRINT N'Creating View [App].[vwDocPurchaseOrder]...';


go
CREATE VIEW App.vwDocPurchaseOrder
AS
	SELECT Project.vwProjects.ProjectCode, Project.vwProjects.ActionOn, Project.vwProjects.ObjectCode, Project.vwProjects.ActionById, Project.vwProjects.BucketId, Project.vwProjects.ProjectTitle, Project.vwProjects.SubjectCode, 
							 Project.vwProjects.ContactName, Project.vwProjects.ProjectNotes, Project.vwProjects.OwnerName, Project.vwProjects.CashCode, Project.vwProjects.CashDescription, Project.vwProjects.ProjectStatusCode, Project.vwProjects.ProjectStatus, Project.vwProjects.Quantity, Object.tbObject.UnitOfMeasure, 
							 Project.vwProjects.UnitCharge, Project.vwProjects.TotalCharge, Subject_tbAddress_1.Address AS FromAddress, Subject.tbAddress.Address AS ToAddress, Project.vwProjects.InsertedBy, Project.vwProjects.InsertedOn, 
							 Project.vwProjects.UpdatedBy, Project.vwProjects.UpdatedOn, Project.vwProjects.SubjectName, Project.vwProjects.ActionName, Project.vwProjects.Period, Project.vwProjects.Printed, Project.vwProjects.Spooled, Project.vwProjects.RowVer
	FROM            Project.vwProjects LEFT OUTER JOIN
							 Subject.tbAddress AS Subject_tbAddress_1 ON Project.vwProjects.AddressCodeFrom = Subject_tbAddress_1.AddressCode LEFT OUTER JOIN
							 Subject.tbAddress ON Project.vwProjects.AddressCodeTo = Subject.tbAddress.AddressCode INNER JOIN
							 Object.tbObject ON Project.vwProjects.ObjectCode = Object.tbObject.ObjectCode
	WHERE        (Project.vwProjects.CashCode IS NOT NULL) AND (Project.vwProjects.CashPolarityCode = 0) AND (Project.vwProjects.ProjectStatusCode > 0);
go
PRINT N'Creating View [App].[vwDocPurchaseEnquiry]...';


go
CREATE VIEW App.vwDocPurchaseEnquiry
AS
	SELECT Project.vwProjects.ProjectCode, Project.vwProjects.ActionOn, Project.vwProjects.ObjectCode, Project.vwProjects.ActionById, Project.vwProjects.BucketId, Project.vwProjects.ProjectTitle, Project.vwProjects.SubjectCode, 
							 Project.vwProjects.ContactName, Project.vwProjects.ProjectNotes, Project.vwProjects.OwnerName, Project.vwProjects.CashCode, Project.vwProjects.CashDescription, Project.vwProjects.ProjectStatusCode, Project.vwProjects.ProjectStatus, Project.vwProjects.Quantity, Object.tbObject.UnitOfMeasure, 
							 Project.vwProjects.UnitCharge, Project.vwProjects.TotalCharge, Subject_tbAddress_1.Address AS FromAddress, Subject.tbAddress.Address AS ToAddress, Project.vwProjects.InsertedBy, Project.vwProjects.InsertedOn, 
							 Project.vwProjects.UpdatedBy, Project.vwProjects.UpdatedOn, Project.vwProjects.SubjectName, Project.vwProjects.ActionName, Project.vwProjects.Period, Project.vwProjects.Printed, Project.vwProjects.Spooled, Project.vwProjects.RowVer
	FROM            Project.vwProjects LEFT OUTER JOIN
							 Subject.tbAddress AS Subject_tbAddress_1 ON Project.vwProjects.AddressCodeFrom = Subject_tbAddress_1.AddressCode LEFT OUTER JOIN
							 Subject.tbAddress ON Project.vwProjects.AddressCodeTo = Subject.tbAddress.AddressCode INNER JOIN
							 Object.tbObject ON Project.vwProjects.ObjectCode = Object.tbObject.ObjectCode
	WHERE        (Project.vwProjects.CashCode IS NOT NULL) AND (Project.vwProjects.CashPolarityCode = 0) AND (Project.vwProjects.ProjectStatusCode = 0);
go
PRINT N'Creating View [App].[vwDocSalesOrder]...';


go
CREATE VIEW App.vwDocSalesOrder
AS
	SELECT        Project.vwProjects.ProjectCode, Project.vwProjects.ActionOn, Project.vwProjects.ObjectCode, Project.vwProjects.ActionById, Project.vwProjects.BucketId, Project.vwProjects.SubjectCode, Project.vwProjects.ProjectTitle, Project.vwProjects.ContactName, 
							 Project.vwProjects.ProjectNotes, Project.vwProjects.OwnerName, Project.vwProjects.CashCode, Project.vwProjects.CashDescription, Project.vwProjects.ProjectStatusCode, Project.vwProjects.ProjectStatus, Project.vwProjects.Quantity, 
							 Object.tbObject.UnitOfMeasure, Project.vwProjects.UnitCharge, Project.vwProjects.TotalCharge, Subject_tbAddress_1.Address AS FromAddress, Subject.tbAddress.Address AS ToAddress, Project.vwProjects.InsertedBy, Project.vwProjects.InsertedOn, 
							 Project.vwProjects.UpdatedBy, Project.vwProjects.UpdatedOn, Project.vwProjects.SubjectName, Project.vwProjects.ActionName, Project.vwProjects.Period, Project.vwProjects.Printed, Project.vwProjects.Spooled, Project.vwProjects.RowVer
	FROM            Project.vwProjects LEFT OUTER JOIN
							 Subject.tbAddress AS Subject_tbAddress_1 ON Project.vwProjects.AddressCodeFrom = Subject_tbAddress_1.AddressCode LEFT OUTER JOIN
							 Subject.tbAddress ON Project.vwProjects.AddressCodeTo = Subject.tbAddress.AddressCode INNER JOIN
							 Object.tbObject ON Project.vwProjects.ObjectCode = Object.tbObject.ObjectCode
	WHERE        (Project.vwProjects.CashCode IS NOT NULL) AND (Project.vwProjects.CashPolarityCode = 1) AND (Project.vwProjects.ProjectStatusCode > 0);
go
PRINT N'Creating View [App].[vwDocQuotation]...';


go
CREATE VIEW App.vwDocQuotation
AS
	SELECT Project.vwProjects.ProjectCode, Project.vwProjects.ActionOn, Project.vwProjects.ObjectCode, Project.vwProjects.ActionById, Project.vwProjects.BucketId, Project.vwProjects.ProjectTitle, Project.vwProjects.SubjectCode, 
							 Project.vwProjects.ContactName, Project.vwProjects.ProjectNotes, Project.vwProjects.OwnerName, Project.vwProjects.CashCode, Project.vwProjects.CashDescription, Project.vwProjects.ProjectStatusCode, Project.vwProjects.ProjectStatus, Project.vwProjects.Quantity, Object.tbObject.UnitOfMeasure, 
							 Project.vwProjects.UnitCharge, Project.vwProjects.TotalCharge, Subject_tbAddress_1.Address AS FromAddress, Subject.tbAddress.Address AS ToAddress, Project.vwProjects.InsertedBy, Project.vwProjects.InsertedOn, 
							 Project.vwProjects.UpdatedBy, Project.vwProjects.UpdatedOn, Project.vwProjects.SubjectName, Project.vwProjects.ActionName, Project.vwProjects.Period, Project.vwProjects.Printed, Project.vwProjects.Spooled, Project.vwProjects.RowVer
	FROM            Project.vwProjects LEFT OUTER JOIN
							 Subject.tbAddress AS Subject_tbAddress_1 ON Project.vwProjects.AddressCodeFrom = Subject_tbAddress_1.AddressCode LEFT OUTER JOIN
							 Subject.tbAddress ON Project.vwProjects.AddressCodeTo = Subject.tbAddress.AddressCode INNER JOIN
							 Object.tbObject ON Project.vwProjects.ObjectCode = Object.tbObject.ObjectCode
	WHERE        (Project.vwProjects.CashCode IS NOT NULL) AND (Project.vwProjects.CashPolarityCode = 1) AND (Project.vwProjects.ProjectStatusCode = 0);
go
PRINT N'Creating View [Subject].[vwProjects]...';


go
CREATE   VIEW Subject.vwProjects
AS
	SELECT        Project.vwProjects.SubjectCode, Project.vwProjects.ProjectCode, Project.vwProjects.UserId, Project.vwProjects.ContactName, Project.vwProjects.ObjectCode, Project.vwProjects.ProjectTitle, Project.vwProjects.ProjectStatusCode, Project.vwProjects.ActionById, 
							 Project.vwProjects.ActionOn, Project.vwProjects.ActionedOn, Project.vwProjects.PaymentOn, Project.vwProjects.SecondReference, Project.vwProjects.ProjectNotes, Project.vwProjects.TaxCode, Project.vwProjects.Quantity, Project.vwProjects.UnitCharge, 
							 Project.vwProjects.TotalCharge, Project.vwProjects.AddressCodeFrom, Project.vwProjects.AddressCodeTo, Project.vwProjects.Printed, Project.vwProjects.Spooled, Project.vwProjects.InsertedBy, Project.vwProjects.InsertedOn, Project.vwProjects.UpdatedBy, 
							 Project.vwProjects.UpdatedOn, Project.vwProjects.Period, Project.vwProjects.BucketId, Project.vwProjects.ProjectStatus, Project.vwProjects.CashCode, Project.vwProjects.CashDescription, Project.vwProjects.OwnerName, Project.vwProjects.ActionName, 
							 Project.vwProjects.SubjectName, Project.vwProjects.SubjectStatus, Project.vwProjects.SubjectType, Project.vwProjects.CashPolarityCode, Cash.tbPolarity.CashPolarity
	FROM            Project.vwProjects INNER JOIN
							 Cash.tbPolarity ON Project.vwProjects.CashPolarityCode = Cash.tbPolarity.CashPolarityCode
	WHERE        (Project.vwProjects.CashCode IS NOT NULL)
go
PRINT N'Creating View [Project].[vwSales]...';


go
CREATE VIEW Project.vwSales
AS
	SELECT        Project.vwProjects.ProjectCode, Project.vwProjects.ObjectCode, Project.vwProjects.ProjectStatusCode, Project.vwProjects.ActionOn, Project.vwProjects.ActionById, Project.vwProjects.ProjectTitle, Project.vwProjects.Period, Project.vwProjects.BucketId, 
							 Project.vwProjects.SubjectCode, Project.vwProjects.ContactName, Project.vwProjects.ProjectStatus, Project.vwProjects.ProjectNotes, Project.vwProjects.ActionedOn, Project.vwProjects.OwnerName, Project.vwProjects.CashCode, 
							 Project.vwProjects.CashDescription, Project.vwProjects.Quantity, Object.tbObject.UnitOfMeasure, Project.vwProjects.UnitCharge, Project.vwProjects.TotalCharge, Subject_tbAddress_1.Address AS FromAddress, 
							 Subject.tbAddress.Address AS ToAddress, Project.vwProjects.Printed, Project.vwProjects.InsertedBy, Project.vwProjects.InsertedOn, Project.vwProjects.UpdatedBy, Project.vwProjects.UpdatedOn, Project.vwProjects.SubjectName, 
							 Project.vwProjects.ActionName, Project.vwProjects.SecondReference
	FROM            Project.vwProjects LEFT OUTER JOIN
							 Subject.tbAddress AS Subject_tbAddress_1 ON Project.vwProjects.AddressCodeFrom = Subject_tbAddress_1.AddressCode LEFT OUTER JOIN
							 Subject.tbAddress ON Project.vwProjects.AddressCodeTo = Subject.tbAddress.AddressCode INNER JOIN
							 Object.tbObject ON Project.vwProjects.ObjectCode = Object.tbObject.ObjectCode
	WHERE        (Project.vwProjects.CashCode IS NOT NULL) AND (Project.vwProjects.CashPolarityCode = 1);
go
PRINT N'Creating View [Project].[vwActiveData]...';


go
CREATE VIEW Project.vwActiveData
AS
	SELECT        ProjectCode, UserId, SubjectCode, ContactName, ObjectCode, ProjectTitle, ProjectStatusCode, ActionById, ActionOn, ActionedOn, PaymentOn, SecondReference, ProjectNotes, TaxCode, Quantity, UnitCharge, TotalCharge, 
							 AddressCodeFrom, AddressCodeTo, Printed, Spooled, InsertedBy, InsertedOn, UpdatedBy, UpdatedOn, Period, BucketId, ProjectStatus, CashCode, CashDescription, OwnerName, ActionName, SubjectName, 
							 SubjectStatus, SubjectType, CashPolarityCode
	FROM            Project.vwProjects
	WHERE        (ProjectStatusCode = 1);
go
PRINT N'Creating View [Project].[vwSubjectObject]...';


go
CREATE VIEW Project.vwSubjectObject
AS
	SELECT SubjectCode, ProjectStatusCode, ActionOn, ProjectTitle, ObjectCode, ActionById, ProjectCode, Period, BucketId, ContactName, ProjectStatus, ProjectNotes, ActionedOn, OwnerName, CashCode, CashDescription, Quantity, 
							 UnitCharge, TotalCharge, AddressCodeFrom, AddressCodeTo, Printed, InsertedBy, InsertedOn, UpdatedBy, UpdatedOn, SubjectName, ActionName
	FROM            Project.vwProjects
	WHERE        (ProjectStatusCode < 2);
go
PRINT N'Creating Procedure [App].[proc_EventLog]...';


go
CREATE   PROCEDURE App.proc_EventLog (@EventMessage NVARCHAR(MAX), @EventTypeCode SMALLINT = 0, @LogCode NVARCHAR(20) = NULL OUTPUT)
AS
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY

		DECLARE 
			@UserId nvarchar(10)
			, @LogNumber INT
			, @RegisterName nvarchar(50) = (SELECT RegisterName FROM App.tbOptions);
	
		SET @UserId = (SELECT TOP 1 Usr.tbUser.UserId FROM Usr.vwCredentials c INNER JOIN
								Usr.tbUser ON c.UserId = Usr.tbUser.UserId);

		BEGIN TRANSACTION;
		
		WHILE (1 = 1)
			BEGIN
			SET @LogNumber = FORMAT((SELECT TOP 1 r.NextNumber
						FROM App.tbRegister r
						WHERE r.RegisterName = @RegisterName), '00000');
				
			UPDATE App.tbRegister
			SET NextNumber += 1
			WHERE RegisterName = @RegisterName;

			SET @LogCode = CONCAT(@UserId, @LogNumber);

			IF NOT EXISTS (SELECT * FROM App.tbEventLog WHERE LogCode = @LogCode)
				BREAK;
			END

		INSERT INTO App.tbEventLog (LogCode, EventTypeCode, EventMessage)
		VALUES (@LogCode, @EventTypeCode, @EventMessage);

		COMMIT TRANSACTION;

		RETURN;
					
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_CompanyName]...';


go

CREATE   PROCEDURE App.proc_CompanyName
	(
	@SubjectName nvarchar(255) = null output
	)
  AS
	SELECT TOP 1 @SubjectName = Subject.tbSubject.SubjectName
	FROM         Subject.tbSubject INNER JOIN
	                      App.tbOptions ON Subject.tbSubject.SubjectCode = App.tbOptions.SubjectCode
go
PRINT N'Creating Procedure [App].[proc_ErrorLog]...';


go

CREATE   PROCEDURE App.proc_ErrorLog 
AS
DECLARE 
	@ErrorMessage NVARCHAR(MAX)
	, @ErrorSeverity TINYINT
	, @ErrorState TINYINT
	, @MessagePrefix nvarchar(4) = '*** ';
	
	IF @@TRANCOUNT > 0 
		ROLLBACK TRANSACTION;

	SET @ErrorSeverity = ERROR_SEVERITY();
	SET @ErrorState = ERROR_STATE();
	SET @ErrorMessage = ERROR_MESSAGE();

	IF @ErrorMessage NOT LIKE CONCAT(@MessagePrefix, '%')
		BEGIN
		SET @ErrorMessage = CONCAT(@MessagePrefix, ERROR_NUMBER(), ': ', QUOTENAME(ERROR_PROCEDURE()) + '.' + FORMAT(ERROR_LINE(), '0'),
			' Severity ', @ErrorSeverity, ', State ', @ErrorState, ' => ' + LEFT(ERROR_MESSAGE(), 1500));		

		EXEC App.proc_EventLog @ErrorMessage;
		END

	RAISERROR ('%s', @ErrorSeverity, @ErrorState, @ErrorMessage);
go
PRINT N'Creating Procedure [App].[proc_YearPeriods]...';


go


CREATE   PROCEDURE App.proc_YearPeriods
	(
	@YearNumber int
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SELECT     App.tbYear.Description, App.tbMonth.MonthName
					FROM         App.tbYearPeriod INNER JOIN
										App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
										App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
					WHERE     ( App.tbYearPeriod.YearNumber = @YearNumber)
					ORDER BY App.tbYearPeriod.YearNumber, App.tbYearPeriod.StartOn
    END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_ReassignUser]...';


go

CREATE   PROCEDURE App.proc_ReassignUser 
	(
	@UserId nvarchar(10)
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		UPDATE    Usr.tbUser
		SET       LogonName = (SUSER_SNAME())
		WHERE     (UserId = @UserId)
	
   	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_PeriodGetYear]...';


go

CREATE   PROCEDURE App.proc_PeriodGetYear
	(
	@StartOn DATETIME,
	@YearNumber INTEGER OUTPUT
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SELECT @YearNumber = YearNumber
		FROM            App.tbYearPeriod
		WHERE        (StartOn = @StartOn)
	
		IF @YearNumber IS NULL
			SELECT @YearNumber = YearNumber FROM App.fnActivePeriod()
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_EventLogCleardown]...';


go
CREATE   PROCEDURE App.proc_EventLogCleardown (@RetentionDays SMALLINT = 30)
AS
	SET NOCOUNT, XACT_ABORT OFF;

	BEGIN TRY
		DECLARE 
			@EventMessage nvarchar(max) = (SELECT [Message] FROM App.tbText WHERE TextId = 1221)
			, @EventTypeCode smallint = 2
			, @LogCode nvarchar(20)

		DELETE FROM App.tbEventLog
		WHERE LoggedOn < DATEADD(DAY, @RetentionDays * -1, CAST(CURRENT_TIMESTAMP AS DATE));
		
		EXECUTE App.proc_EventLog @EventMessage, @EventTypeCode, @LogCode OUTPUT

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_TaxRates]...';


go
CREATE   PROCEDURE App.proc_TaxRates(@StartOn datetime, @EndOn datetime, @CorporationTaxRate real)
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY	
		UPDATE App.tbYearPeriod
		SET CorporationTaxRate = @CorporationTaxRate
		WHERE StartOn >= @StartOn AND StartOn <= @EndOn;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_Initialised]...';


go

CREATE   PROCEDURE App.proc_Initialised
(@Setting bit)
  AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF @Setting = 1
			AND (EXISTS (SELECT     Subject.tbSubject.SubjectCode
						FROM         Subject.tbSubject INNER JOIN
											  App.tbOptions ON Subject.tbSubject.SubjectCode = App.tbOptions.SubjectCode)
			OR EXISTS (SELECT     Subject.tbAddress.AddressCode
						   FROM         Subject.tbSubject INNER JOIN
												 App.tbOptions ON Subject.tbSubject.SubjectCode = App.tbOptions.SubjectCode INNER JOIN
												 Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode)
			OR EXISTS (SELECT     TOP 1 UserId
							   FROM         Usr.tbUser))
			BEGIN
			UPDATE App.tbOptions Set IsInitialised = 1
			RETURN
			END
		ELSE
			BEGIN
			UPDATE App.tbOptions Set IsInitialised = 0
			RETURN 1
			END
 	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Object].[proc_WorkFlowMultiLevel]...';


go
CREATE   PROCEDURE Object.proc_WorkFlowMultiLevel
	(
	@ObjectCode nvarchar(50)
	)
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		IF EXISTS(SELECT * FROM Object.tbFlow WHERE (ParentCode = @ObjectCode))
		BEGIN
			WITH workflow AS
			(
				SELECT  parent_flow.ParentCode, parent_flow.ChildCode, parent_flow.OffsetDays, parent_flow.UsedOnQuantity, 1 AS Depth
				FROM Object.tbFlow parent_flow
				WHERE (parent_flow.ParentCode = @ObjectCode)

				UNION ALL

				SELECT  child_flow.ParentCode, child_flow.ChildCode, child_flow.OffsetDays, child_flow.UsedOnQuantity, workflow.Depth + 1 AS Depth
				FROM workflow 
					JOIN Object.tbFlow child_flow ON workflow.ChildCode = child_flow.ParentCode
			)
			SELECT workflow.ParentCode, workflow.ChildCode,
						Project_status.ProjectStatus, ISNULL(cash_category.CashPolarityCode, 2) AS CashPolarityCode,
						Object.UnitOfMeasure, workflow.OffsetDays, workflow.UsedOnQuantity, Depth
			FROM workflow
					JOIN Object.tbObject Object ON workflow.ChildCode = Object.ObjectCode
					JOIN Project.tbStatus Project_status ON Object.ProjectStatusCode = Project_status.ProjectStatusCode 
					LEFT OUTER JOIN Cash.tbCode cash_code ON Object.CashCode = cash_code.CashCode 
					LEFT OUTER JOIN Cash.tbCategory cash_category ON cash_code.CategoryCode = cash_category.CategoryCode
			ORDER BY Depth, ParentCode, ChildCode;
		END
		ELSE
		BEGIN
			WITH workflow AS
			(
				SELECT  child_flow.ParentCode, child_flow.ChildCode, child_flow.OffsetDays, child_flow.UsedOnQuantity, -1 AS Depth
				FROM Object.tbFlow child_flow
				WHERE (child_flow.ChildCode = @ObjectCode)

				UNION ALL

				SELECT  parent_flow.ParentCode, parent_flow.ChildCode, parent_flow.OffsetDays, parent_flow.UsedOnQuantity, workflow.Depth - 1 AS Depth
				FROM workflow 
					JOIN Object.tbFlow parent_flow ON workflow.ParentCode = parent_flow.ChildCode
			)
			SELECT workflow.ChildCode AS ParentCode, workflow.ParentCode AS ChildCode, 
						Project_status.ProjectStatus, ISNULL(cash_category.CashPolarityCode, 2) AS CashPolarityCode,
						Object.UnitOfMeasure, workflow.OffsetDays, workflow.UsedOnQuantity, Depth
			FROM workflow
					JOIN Object.tbObject Object ON workflow.ParentCode = Object.ObjectCode
					JOIN Project.tbStatus Project_status ON Object.ProjectStatusCode = Project_status.ProjectStatusCode 
					LEFT OUTER JOIN Cash.tbCode cash_code ON Object.CashCode = cash_code.CashCode 
					LEFT OUTER JOIN Cash.tbCategory cash_category ON cash_code.CategoryCode = cash_category.CategoryCode
			ORDER BY Depth DESC, ParentCode, ChildCode;		
		END
			 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Object].[proc_WorkFlow]...';


go

CREATE   PROCEDURE Object.proc_WorkFlow
	(
	@ParentObjectCode nvarchar(50),
	@ObjectCode nvarchar(50)
	)
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		IF EXISTS(SELECT * FROM Object.tbFlow WHERE (ParentCode = @ParentObjectCode))
			AND NOT EXISTS(SELECT COUNT(*) FROM Object.tbFlow WHERE ChildCode = @ParentObjectCode GROUP BY ChildCode HAVING COUNT(*) > 1)			
		BEGIN
			SELECT     Object.tbObject.ObjectCode, Project.tbStatus.ProjectStatus, ISNULL(Cash.tbCategory.CashPolarityCode, 2) AS CashPolarityCode, Object.tbObject.UnitOfMeasure, Object.tbFlow.OffsetDays, Object.tbFlow.UsedOnQuantity
			FROM         Object.tbObject INNER JOIN
								  Project.tbStatus ON Object.tbObject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode INNER JOIN
								  Object.tbFlow ON Object.tbObject.ObjectCode = Object.tbFlow.ChildCode LEFT OUTER JOIN
								  Cash.tbCode ON Object.tbObject.CashCode = Cash.tbCode.CashCode LEFT OUTER JOIN
								  Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE     ( Object.tbFlow.ParentCode = @ObjectCode)
			ORDER BY Object.tbFlow.StepNumber	
		END
		ELSE
		BEGIN
			SELECT     Object.tbObject.ObjectCode, Project.tbStatus.ProjectStatus, ISNULL(Cash.tbCategory.CashPolarityCode, 2) AS CashPolarityCode, Object.tbObject.UnitOfMeasure, Object.tbFlow.OffsetDays, Object.tbFlow.UsedOnQuantity
			FROM         Object.tbObject INNER JOIN
								  Project.tbStatus ON Object.tbObject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode INNER JOIN
								  Object.tbFlow ON Object.tbObject.ObjectCode = Object.tbFlow.ParentCode LEFT OUTER JOIN
								  Cash.tbCode ON Object.tbObject.CashCode = Cash.tbCode.CashCode LEFT OUTER JOIN
								  Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE     ( Object.tbFlow.ChildCode = @ObjectCode)
			ORDER BY Object.tbFlow.StepNumber	
		END
			 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Object].[proc_Parent]...';


go

CREATE   PROCEDURE Object.proc_Parent
	(
	@ObjectCode nvarchar(50),
	@ParentCode nvarchar(50) = null output
	)
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SET @ParentCode = @ObjectCode;
		
		IF EXISTS(SELECT ParentCode FROM Object.tbFlow WHERE (ParentCode = @ObjectCode))
			OR NOT EXISTS(SELECT COUNT(*) FROM Object.tbFlow WHERE ChildCode = @ObjectCode GROUP BY ChildCode HAVING COUNT(*) > 1)
		BEGIN		
			WHILE EXISTS (SELECT COUNT(*) FROM Object.tbFlow WHERE ChildCode = @ParentCode GROUP BY ChildCode HAVING COUNT(*) = 1)
				SELECT @ParentCode = ParentCode, @ObjectCode = ParentCode 
				FROM Object.tbFlow		
				WHERE ChildCode = @ObjectCode;	 
		END
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Object].[proc_NextStepNumber]...';


go
CREATE   PROCEDURE Object.proc_NextStepNumber 
	(
	@ObjectCode nvarchar(50),
	@StepNumber smallint = 10 output
	)
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT     TOP 1 StepNumber
				  FROM         Object.tbFlow
				  WHERE     (ParentCode = @ObjectCode))
			BEGIN
			SELECT  @StepNumber = MAX(StepNumber) 
			FROM         Object.tbFlow
			WHERE     (ParentCode = @ObjectCode)
			SET @StepNumber = @StepNumber - (@StepNumber % 10) + 10		
			END
		ELSE
			SET @StepNumber = 10
		
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Object].[proc_NextOperationNumber]...';


go

CREATE   PROCEDURE Object.proc_NextOperationNumber 
	(
	@ObjectCode nvarchar(50),
	@OperationNumber smallint = 10 output
	)
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT     TOP 1 OperationNumber
				  FROM         Object.tbOp
				  WHERE     (ObjectCode = @ObjectCode))
			BEGIN
			SELECT  @OperationNumber = MAX(OperationNumber) 
			FROM         Object.tbOp
			WHERE     (ObjectCode = @ObjectCode)
			SET @OperationNumber = @OperationNumber - (@OperationNumber % 10) + 10		
			END
		ELSE
			SET @OperationNumber = 10
		
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Object].[proc_NextAttributeOrder]...';


go
CREATE   PROCEDURE Object.proc_NextAttributeOrder 
	(
	@ObjectCode nvarchar(50),
	@PrintOrder smallint = 10 output
	)
  AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT     TOP 1 PrintOrder
				  FROM         Object.tbAttribute
				  WHERE     (ObjectCode = @ObjectCode))
			BEGIN
			SELECT  @PrintOrder = MAX(PrintOrder) 
			FROM         Object.tbAttribute
			WHERE     (ObjectCode = @ObjectCode)
			SET @PrintOrder = @PrintOrder - (@PrintOrder % 10) + 10		
			END
		ELSE
			SET @PrintOrder = 10
		
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Object].[proc_Mode]...';


go

CREATE   PROCEDURE Object.proc_Mode
	(
	@ObjectCode nvarchar(50)
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SELECT     Object.tbObject.ObjectCode, Object.tbObject.UnitOfMeasure, Project.tbStatus.ProjectStatus, ISNULL(Cash.tbCategory.CashPolarityCode, 2) AS CashPolarityCode
		FROM         Object.tbObject INNER JOIN
							  Project.tbStatus ON Object.tbObject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode LEFT OUTER JOIN
							  Cash.tbCode ON Object.tbObject.CashCode = Cash.tbCode.CashCode LEFT OUTER JOIN
							  Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
		WHERE     ( Object.tbObject.ObjectCode = @ObjectCode)
		 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Object].[proc_Mirror]...';


go
CREATE   PROCEDURE Object.proc_Mirror(@ObjectCode nvarchar(50), @SubjectCode nvarchar(10), @AllocationCode nvarchar(50))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF NOT EXISTS (SELECT * FROM Object.tbMirror WHERE ObjectCode = @ObjectCode AND SubjectCode = @SubjectCode AND AllocationCode = @AllocationCode)
		BEGIN
			INSERT INTO Object.tbMirror (ObjectCode, SubjectCode, AllocationCode)
			VALUES (@ObjectCode, @SubjectCode, @AllocationCode);
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Object].[proc_NetworkUpdated]...';


go
CREATE   PROCEDURE Object.proc_NetworkUpdated(@SubjectCode nvarchar(10), @AllocationCode nvarchar(50))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		UPDATE Object.tbMirror
		SET TransmitStatusCode = 3
		WHERE SubjectCode = @SubjectCode AND AllocationCode = @AllocationCode;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Subject].[proc_BalanceOutstanding]...';


go
CREATE PROCEDURE Subject.proc_BalanceOutstanding 
	(
	@SubjectCode nvarchar(10),
	@Balance decimal(18, 5) = 0 OUTPUT
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY		
		
		SELECT @Balance = ISNULL(Balance, 0) FROM Subject.vwBalanceOutstanding WHERE SubjectCode = @SubjectCode

		IF EXISTS(SELECT     SubjectCode
				  FROM         Cash.tbPayment
				  WHERE     (PaymentStatusCode = 0) AND (SubjectCode = @SubjectCode)) AND (@Balance <> 0)
			BEGIN
			SELECT  @Balance = @Balance - SUM(PaidInValue - PaidOutValue) 
			FROM         Cash.tbPayment
			WHERE     (PaymentStatusCode = 0) AND (SubjectCode = @SubjectCode)		
			END
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Subject].[proc_AddContact]...';


go

CREATE   PROCEDURE Subject.proc_AddContact 
	(
	@SubjectCode nvarchar(10),
	@ContactName nvarchar(100)	 
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	
		INSERT INTO Subject.tbContact
								(SubjectCode, ContactName, PhoneNumber, EmailAddress)
		SELECT     SubjectCode, @ContactName AS ContactName, PhoneNumber, EmailAddress
		FROM         Subject.tbSubject
		WHERE SubjectCode = @SubjectCode
	
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Subject].[proc_DefaultEmailAddress]...';


go
CREATE   PROCEDURE Subject.proc_DefaultEmailAddress 
	(
	@SubjectCode nvarchar(10),
	@EmailAddress nvarchar(255) OUTPUT
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

	SELECT @EmailAddress = COALESCE(EmailAddress, '') FROM Subject.tbSubject WHERE SubjectCode = @SubjectCode;

	IF (LEN(@EmailAddress) = 0)
		SELECT @EmailAddress = EmailAddress
		FROM Subject.tbContact
		WHERE SubjectCode = @SubjectCode AND NOT (EmailAddress IS NULL);

	SET @EmailAddress = COALESCE(@EmailAddress, '');

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Subject].[proc_DefaultTaxCode]...';


go
CREATE PROCEDURE Subject.proc_DefaultTaxCode 
	(
	@SubjectCode nvarchar(10),
	@TaxCode nvarchar(10) OUTPUT
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		IF EXISTS (SELECT * FROM Subject.tbSubject o JOIN App.tbTaxCode t ON o.TaxCode = t.TaxCode WHERE SubjectCode = @SubjectCode)
			SELECT @TaxCode = TaxCode FROM Subject.tbSubject WHERE SubjectCode = @SubjectCode
		ELSE IF EXISTS(SELECT * FROM  Subject.tbSubject JOIN App.tbOptions ON Subject.tbSubject.SubjectCode = App.tbOptions.SubjectCode)
			SELECT @TaxCode = Subject.tbSubject.TaxCode FROM  Subject.tbSubject JOIN App.tbOptions ON Subject.tbSubject.SubjectCode = App.tbOptions.SubjectCode		
		ELSE
			SET @TaxCode = ''

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Subject].[proc_NextAddressCode]...';


go

CREATE   PROCEDURE Subject.proc_NextAddressCode 
	(
	@SubjectCode nvarchar(10),
	@AddressCode nvarchar(15) OUTPUT
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @AddCount int

		SELECT @AddCount = ISNULL(COUNT(AddressCode), 0) 
		FROM         Subject.tbAddress
		WHERE     (SubjectCode = @SubjectCode)
	
		SET @AddCount += 1
		SET @AddressCode = CONCAT(UPPER(@SubjectCode), '_', FORMAT(@AddCount, '000'))
	
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Subject].[proc_Statement]...';


go

CREATE   PROCEDURE Subject.proc_Statement (@SubjectCode NVARCHAR(10))
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		SELECT *
		FROM Subject.vwStatement
		WHERE SubjectCode = @SubjectCode
		ORDER BY RowNumber DESC

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog
	END CATCH
go
PRINT N'Creating Procedure [Subject].[proc_Rebuild]...';


go
CREATE PROCEDURE Subject.proc_Rebuild(@SubjectCode NVARCHAR(10))
AS
  	SET NOCOUNT, XACT_ABORT ON;

	DECLARE @PaymentCode nvarchar(20);

	BEGIN TRY
		BEGIN TRANSACTION;

		UPDATE Invoice.tbItem
		SET 
			InvoiceValue =  ROUND(Invoice.tbItem.TotalValue / (1 + App.tbTaxCode.TaxRate), Decimals)
		FROM         Invoice.tbItem INNER JOIN
								App.tbTaxCode ON Invoice.tbItem.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
								Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode <> 0) AND Invoice.tbItem.TotalValue <> 0
			AND (Invoice.tbInvoice.SubjectCode = @SubjectCode);

		UPDATE Invoice.tbItem
		SET TaxValue = CASE App.tbTaxCode.RoundingCode 
				WHEN 0 THEN ROUND(Invoice.tbItem.InvoiceValue * App.tbTaxCode.TaxRate, Decimals)
				WHEN 1 THEN ROUND( Invoice.tbItem.InvoiceValue * App.tbTaxCode.TaxRate, Decimals, 1) END
		FROM         Invoice.tbItem INNER JOIN
								App.tbTaxCode ON Invoice.tbItem.TaxCode = App.tbTaxCode.TaxCode 
								INNER JOIN Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode <> 0)
			AND (Invoice.tbInvoice.SubjectCode = @SubjectCode);
                   
		UPDATE Invoice.tbProject
		SET InvoiceValue =  ROUND(Invoice.tbProject.TotalValue / (1 + App.tbTaxCode.TaxRate), Decimals)
		FROM         Invoice.tbProject INNER JOIN
								App.tbTaxCode ON Invoice.tbProject.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
								Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode <> 0) AND Invoice.tbProject.TotalValue <> 0
			AND (Invoice.tbInvoice.SubjectCode = @SubjectCode);

		UPDATE Invoice.tbProject
		SET TaxValue = CASE App.tbTaxCode.RoundingCode 
				WHEN 0 THEN ROUND(Invoice.tbProject.InvoiceValue * App.tbTaxCode.TaxRate, Decimals)
				WHEN 1 THEN ROUND( Invoice.tbProject.InvoiceValue * App.tbTaxCode.TaxRate, Decimals, 1) END,
			InvoiceValue = CASE WHEN Invoice.tbProject.TotalValue = 0 
								THEN Invoice.tbProject.InvoiceValue 
								ELSE ROUND(Invoice.tbProject.TotalValue / (1 + App.tbTaxCode.TaxRate), 2) 
							END
		FROM         Invoice.tbProject INNER JOIN
								App.tbTaxCode ON Invoice.tbProject.TaxCode = App.tbTaxCode.TaxCode 
								INNER JOIN Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode <> 0)
			AND (Invoice.tbInvoice.SubjectCode = @SubjectCode);
						   	
		WITH items AS
		(
			SELECT     Invoice.tbInvoice.InvoiceNumber, SUM( Invoice.tbItem.InvoiceValue) AS TotalInvoiceValue, SUM( Invoice.tbItem.TaxValue) AS TotalTaxValue
			FROM         Invoice.tbItem INNER JOIN
								Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
			GROUP BY Invoice.tbInvoice.InvoiceNumber
		), Projects AS
		(
			SELECT     Invoice.tbInvoice.InvoiceNumber, SUM( Invoice.tbProject.InvoiceValue) AS TotalInvoiceValue, SUM( Invoice.tbProject.TaxValue) AS TotalTaxValue
			FROM         Invoice.tbProject INNER JOIN
								Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
			GROUP BY Invoice.tbInvoice.InvoiceNumber
		), invoice_totals AS
		(
			SELECT invoices.InvoiceNumber, 
				COALESCE(items.TotalInvoiceValue, 0) + COALESCE(Projects.TotalInvoiceValue, 0) TotalInvoiceValue,
				COALESCE(items.TotalTaxValue, 0) + COALESCE(Projects.TotalTaxValue, 0) TotalTaxValue
			FROM Invoice.tbInvoice invoices
				LEFT OUTER JOIN Projects ON invoices.InvoiceNumber = Projects.InvoiceNumber
				LEFT OUTER JOIN items ON invoices.InvoiceNumber = items.InvoiceNumber
			WHERE ( invoices.InvoiceStatusCode > 0)
		)
		UPDATE invoices
		SET InvoiceValue = TotalInvoiceValue, 
			TaxValue = TotalTaxValue
		FROM  Invoice.tbInvoice invoices 
			JOIN invoice_totals ON invoices.InvoiceNumber = invoice_totals.InvoiceNumber
		WHERE SubjectCode = @SubjectCode AND (InvoiceValue <> TotalInvoiceValue OR TaxValue <> TotalTaxValue);



		WITH invoice_status AS
		(
			SELECT InvoiceNumber, InvoiceStatusCode, PaidValue, PaidTaxValue
			FROM Invoice.vwStatusLive
			WHERE SubjectCode = @SubjectCode
		)
		UPDATE invoices
		SET 
			InvoiceStatusCode = invoice_status.InvoiceStatusCode,
			PaidValue = invoice_status.PaidValue,
			PaidTaxValue = invoice_status.PaidTaxValue
		FROM Invoice.tbInvoice invoices	
			JOIN invoice_status ON invoices.InvoiceNumber = invoice_status.InvoiceNumber
		WHERE 
			invoices.InvoiceStatusCode <> invoice_status.InvoiceStatusCode 
			OR invoices.PaidValue <> invoice_status.PaidValue 
			OR invoices.PaidTaxValue <> invoice_status.PaidTaxValue;

		COMMIT TRANSACTION

		DECLARE @Msg NVARCHAR(MAX);
		SELECT @Msg = CONCAT(@SubjectCode, ' ', Message) FROM App.tbText WHERE TextId = 3006;
		EXEC App.proc_EventLog @EventMessage = @Msg, @EventTypeCode = 2;

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Subject].[proc_DefaultSubjectCode]...';


go

CREATE   PROCEDURE Subject.proc_DefaultSubjectCode 
	(
	@SubjectName nvarchar(100),
	@SubjectCode nvarchar(10) OUTPUT 
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@ParsedName nvarchar(100)
			, @FirstWord nvarchar(100)
			, @SecondWord nvarchar(100)
			, @ValidatedCode nvarchar(10)
			, @c char(1)
			, @ASCII smallint
			, @pos int
			, @ok bit
			, @Suffix smallint
			, @Rows int
		
		SET @pos = 1
		SET @ParsedName = ''

		WHILE @pos <= datalength(@SubjectName)
		BEGIN
			SET @ASCII = ASCII(SUBSTRING(@SubjectName, @pos, 1))
			SET @ok = CASE 
				WHEN @ASCII = 32 THEN 1
				WHEN @ASCII = 45 THEN 1
				WHEN (@ASCII >= 48 and @ASCII <= 57) THEN 1
				WHEN (@ASCII >= 65 and @ASCII <= 90) THEN 1
				WHEN (@ASCII >= 97 and @ASCII <= 122) THEN 1
				ELSE 0
			END
			IF @ok = 1
				SELECT @ParsedName = @ParsedName + char(ASCII(SUBSTRING(@SubjectName, @pos, 1)))
			SET @pos = @pos + 1
		END

		--print @ParsedName
		
		IF CHARINDEX(' ', @ParsedName) = 0
			BEGIN
			SET @FirstWord = @ParsedName
			SET @SecondWord = ''
			END
		ELSE
			BEGIN
			SET @FirstWord = left(@ParsedName, CHARINDEX(' ', @ParsedName) - 1)
			SET @SecondWord = right(@ParsedName, LEN(@ParsedName) - CHARINDEX(' ', @ParsedName))
			IF CHARINDEX(' ', @SecondWord) > 0
				SET @SecondWord = left(@SecondWord, CHARINDEX(' ', @SecondWord) - 1)
			END

		IF EXISTS(SELECT ExcludedTag FROM App.tbCodeExclusion WHERE ExcludedTag = @SecondWord)
			BEGIN
			SET @SecondWord = ''
			END

		--print @FirstWord
		--print @SecondWord

		IF LEN(@SecondWord) > 0
			SET @SubjectCode = UPPER(left(@FirstWord, 3)) + UPPER(left(@SecondWord, 3))		
		ELSE
			SET @SubjectCode = UPPER(left(@FirstWord, 6))

		SET @ValidatedCode = @SubjectCode
		SELECT @rows = COUNT(SubjectCode) FROM Subject.tbSubject WHERE SubjectCode = @ValidatedCode
		SET @Suffix = 0
	
		WHILE @rows > 0
		BEGIN
			SET @Suffix = @Suffix + 1
			SET @ValidatedCode = @SubjectCode + LTRIM(STR(@Suffix))
			SELECT @rows = COUNT(SubjectCode) FROM Subject.tbSubject WHERE SubjectCode = @ValidatedCode
		END
	
		SET @SubjectCode = @ValidatedCode
	
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Subject].[proc_AccountKeyDelete]...';


go
CREATE   PROCEDURE Subject.proc_AccountKeyDelete(@AccountCode nvarchar(10), @KeyName nvarchar(50))
AS
	SET NOCOUNT, XACT_ABORT OFF;

	BEGIN TRY	

		WITH root_level AS
		(
			SELECT AccountCode, CAST(NULL as hierarchyid) Ancestor, HDPath, HDPath.GetLevel() Lv
			FROM Subject.tbAccountKey 
			WHERE AccountCode = @AccountCode AND KeyName = @KeyName
		), candidates AS
		(
			SELECT ns.AccountCode, ns.HDPath.GetAncestor(1) Ancestor, ns.HDPath, ns.HDPath.GetLevel() Lv
			FROM Subject.tbAccountKey ns 
				JOIN root_level ON ns.AccountCode = root_level.AccountCode
			WHERE ns.HDPath.GetLevel() > root_level.Lv
		), selected AS
		(
			SELECT AccountCode, Ancestor, HDPath FROM root_level
		
			UNION ALL

			SELECT candidates.AccountCode, candidates.Ancestor, candidates.HDPath
			FROM candidates
				JOIN selected ON selected.HDPath = candidates.Ancestor
		)
		DELETE Subject.tbAccountKey
		FROM selected
			JOIN Subject.tbAccountKey ON Subject.tbAccountKey.AccountCode = selected.AccountCode AND Subject.tbAccountKey.HDPath = selected.HDPath;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Subject].[proc_AccountKeyRename]...';


go
CREATE   PROCEDURE Subject.proc_AccountKeyRename(@AccountCode nvarchar (10), @OldKeyName nvarchar(50), @NewKeyName nvarchar(50), @KeyNamespace nvarchar(1024) output)
AS
	SET NOCOUNT, XACT_ABORT OFF;

	BEGIN TRY

		UPDATE Subject.tbAccountKey
		SET KeyName = @NewKeyName
		WHERE AccountCode = @AccountCode AND KeyName = @OldKeyName;

		WITH namespaced AS
		(
			SELECT AccountCode, HDPath, CAST(KeyName as nvarchar(1024)) KeyNamespace, HDPath.GetLevel() HDLevel
			FROM Subject.tbAccountKey
			WHERE AccountCode = @AccountCode AND KeyName = @NewKeyName

			UNION ALL

			SELECT parent.AccountCode, parent.HDPath, CAST(CONCAT(parent.KeyName, '.', namespaced.KeyNamespace) as nvarchar(1024)) KeyNamespace, parent.HDPath.GetLevel() HDLevel
			FROM Subject.tbAccountKey parent
				JOIN namespaced ON parent.AccountCode = namespaced.AccountCode AND parent.HDPath = namespaced.HDPath.GetAncestor(1)
		)
		SELECT @KeyNamespace = REPLACE(UPPER(KeyNamespace), ' ', '_') 
		FROM namespaced
		WHERE HDLevel = 0;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Subject].[proc_AccountKeyAdd]...';


go
CREATE PROCEDURE Subject.proc_AccountKeyAdd (@AccountCode nvarchar (10), @ParentName nvarchar(50), @ChildName nvarchar(50), @ChildHDPath nvarchar(50) output)
AS
	SET NOCOUNT, XACT_ABORT OFF;

	BEGIN TRY

		DECLARE @ParentId hierarchyid = (SELECT HDPath FROM Subject.tbAccountKey WHERE AccountCode = @AccountCode AND KeyName = @ParentName);
		DECLARE @ChildId hierarchyId = (SELECT MAX(HDPath) FROM Subject.tbAccountKey WHERE HDPath.GetAncestor(1) = @ParentId);

		IF (App.fnParsePrimaryKey(@ChildName) <> 0 AND CHARINDEX('.', @ChildName) = 0)
			BEGIN
				SET @ChildId = @ParentId.GetDescendant(@ChildId, NULL);

				INSERT INTO Subject.tbAccountKey (AccountCode, HDPath, KeyName)
				SELECT @AccountCode AccountCode, 
					@ChildId HDPath, 
					@ChildName KeyName;

				SET @ChildHDPath = REPLACE(@ChildId.ToString(), '/', '''/'); 
				SET @ChildHDPath = RIGHT(@ChildHDPath, LEN(@ChildHDPath) - 1);
				SET @ChildHDPath = ( SELECT CONCAT('44/', CoinTypeCode, '/0', @ChildHDPath) FROM Subject.tbAccount WHERE AccountCode = @AccountCode)
				
			END
		ELSE
			BEGIN
				DECLARE @Msg nvarchar(MAX) = (SELECT TOP (1) [Message] FROM App.tbText WHERE TextId = 2004);
				THROW 50000, @Msg, 1;				
			END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Subject].[proc_WalletInitialise]...';


go
CREATE   PROCEDURE Subject.proc_WalletInitialise
AS
	SET NOCOUNT, XACT_ABORT OFF;

	BEGIN TRY
		WITH wallets AS
		(
			SELECT wallet.AccountCode
			FROM Subject.vwWallets AS wallet 
				LEFT OUTER JOIN Subject.tbAccountKey AS nspace ON wallet.AccountCode = nspace.AccountCode
			WHERE        (nspace.AccountCode IS NULL)
		), hdrootName AS
		(
			SELECT SubjectName KeyName
			FROM Subject.tbSubject Subjects
				JOIN App.tbOptions opts ON opts.SubjectCode = Subjects.SubjectCode
		)
		INSERT INTO Subject.tbAccountKey (AccountCode, HDPath, KeyName)
		SELECT AccountCode, '/' HDPath, (SELECT KeyName FROM hdrootName) KeyName
		FROM wallets;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_ChangeLogCleardown]...';


go
CREATE   PROCEDURE Project.proc_ChangeLogCleardown (@RetentionDays SMALLINT = 30)
AS
	SET NOCOUNT, XACT_ABORT OFF;

	BEGIN TRY					
		DECLARE 
			@EventMessage nvarchar(max) = (SELECT [Message] FROM App.tbText WHERE TextId = 1222)
			, @EventTypeCode smallint = 2
			, @LogCode nvarchar(20)

		DELETE FROM Project.tbChangeLog
		WHERE ChangedOn < DATEADD(DAY, @RetentionDays * -1, CAST(CURRENT_TIMESTAMP AS DATE)) 

		EXECUTE App.proc_EventLog @EventMessage, @EventTypeCode, @LogCode OUTPUT

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_Delete]...';


go

CREATE   PROCEDURE Project.proc_Delete 
	(
	@ProjectCode nvarchar(20)
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @ChildProjectCode nvarchar(20)

		IF @@NESTLEVEL = 1
			BEGIN TRANSACTION

		DELETE FROM Project.tbFlow
		WHERE     (ChildProjectCode = @ProjectCode)

		DECLARE curFlow cursor local for
			SELECT     ChildProjectCode
			FROM         Project.tbFlow
			WHERE     (ParentProjectCode = @ProjectCode)
	
		OPEN curFlow		
		FETCH NEXT FROM curFlow INTO @ChildProjectCode
		WHILE @@FETCH_STATUS = 0
			BEGIN
			EXEC Project.proc_Delete @ChildProjectCode
			FETCH NEXT FROM curFlow INTO @ChildProjectCode		
			END
	
		CLOSE curFlow
		DEALLOCATE curFlow
	
		DELETE FROM Project.tbProject
		WHERE (ProjectCode = @ProjectCode)
	
		IF @@NESTLEVEL = 1
			COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_DefaultInvoiceType]...';


go

CREATE   PROCEDURE Project.proc_DefaultInvoiceType
	(
		@ProjectCode nvarchar(20),
		@InvoiceTypeCode smallint OUTPUT
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		DECLARE @CashPolarityCode smallint

		IF EXISTS(SELECT     CashPolarityCode
				  FROM         Project.vwCashPolarity
				  WHERE     (ProjectCode = @ProjectCode))
			SELECT   @CashPolarityCode = CashPolarityCode
			FROM         Project.vwCashPolarity
			WHERE     (ProjectCode = @ProjectCode)			          
		ELSE
			SET @CashPolarityCode = 1
		
		IF @CashPolarityCode = 0
			SET @InvoiceTypeCode = 2
		ELSE
			SET @InvoiceTypeCode = 0
		 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_NetworkUpdated]...';


go
CREATE   PROCEDURE Project.proc_NetworkUpdated (@ProjectCode nvarchar(20))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		UPDATE Project.tbChangeLog
		SET TransmitStatusCode = 3
		WHERE ProjectCode = @ProjectCode AND TransmitStatusCode < 3;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_DefaultDocType]...';


go

CREATE   PROCEDURE Project.proc_DefaultDocType
	(
		@ProjectCode nvarchar(20),
		@DocTypeCode smallint OUTPUT
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@CashPolarityCode smallint
			, @ProjectStatusCode smallint

		IF EXISTS(SELECT     CashPolarityCode
				  FROM         Project.vwCashPolarity
				  WHERE     (ProjectCode = @ProjectCode))
			SELECT   @CashPolarityCode = CashPolarityCode
			FROM         Project.vwCashPolarity
			WHERE     (ProjectCode = @ProjectCode)			          
		ELSE
			SET @CashPolarityCode = 1

		SELECT  @ProjectStatusCode =ProjectStatusCode
		FROM         Project.tbProject
		WHERE     (ProjectCode = @ProjectCode)		
	
		IF @CashPolarityCode = 0
			SET @DocTypeCode = CASE @ProjectStatusCode WHEN 0 THEN 2 ELSE 3 END								
		ELSE
			SET @DocTypeCode = CASE @ProjectStatusCode WHEN 0 THEN 0 ELSE 1 END 
		 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_Cost]...';


go
CREATE   PROCEDURE Project.proc_Cost 
	(
	@ParentProjectCode nvarchar(20),
	@TotalCost decimal(18, 5) = 0 OUTPUT
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		WITH Project_flow AS
		(
			SELECT parent_Project.ProjectCode, child.ParentProjectCode, child.ChildProjectCode, 
				CASE WHEN child.UsedOnQuantity <> 0 THEN CAST(parent_Project.Quantity * child.UsedOnQuantity AS decimal(18, 4)) ELSE child_Project.Quantity END AS Quantity, 
				1 AS Depth				
			FROM Project.tbFlow child 
				JOIN Project.tbProject parent_Project ON child.ParentProjectCode = parent_Project.ProjectCode
				JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode
			WHERE parent_Project.ProjectCode = @ParentProjectCode

			UNION ALL

			SELECT parent.ProjectCode, child.ParentProjectCode, child.ChildProjectCode, 
				CASE WHEN child.UsedOnQuantity <> 0 THEN CAST(parent.Quantity * child.UsedOnQuantity AS decimal(18, 4)) ELSE child_Project.Quantity END AS Quantity, 
				parent.Depth + 1 AS Depth
			FROM Project.tbFlow child 
				JOIN Project_flow parent ON child.ParentProjectCode = parent.ChildProjectCode
				JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode
		)
		, Projects AS
		(
			SELECT Project_flow.ProjectCode, Project.Quantity,
				CASE category.CashPolarityCode 
					WHEN NULL THEN 0 
					WHEN 0 THEN Project.UnitCharge * -1 
					ELSE Project.UnitCharge 
				END AS UnitCharge
			FROM Project_flow
				JOIN Project.tbProject Project ON Project_flow.ChildProjectCode = Project.ProjectCode
				LEFT OUTER JOIN Cash.tbCode cashcode ON cashcode.CashCode = Project.CashCode 
				LEFT OUTER JOIN Cash.tbCategory category ON category.CategoryCode = cashcode.CategoryCode
		), Project_costs AS
		(
			SELECT ProjectCode, SUM(Quantity * UnitCharge) AS TotalCost
			FROM Projects
			GROUP BY ProjectCode
		)
		SELECT @TotalCost = TotalCost
		FROM Project_costs;		

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_Schedule]...';


go

CREATE   PROCEDURE Project.proc_Schedule (@ParentProjectCode nvarchar(20))
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF @@NESTLEVEL = 1
			BEGIN TRANSACTION;

		WITH ops_top_level AS
		(
			SELECT Project.ProjectCode, ops.OperationNumber, ops.OffsetDays, Project.ActionOn, ops.StartOn, ops.EndOn, Project.ProjectStatusCode, ops.OpStatusCode, ops.SyncTypeCode
			FROM Project.tbOp ops JOIN Project.tbProject Project ON ops.ProjectCode = Project.ProjectCode
			WHERE Project.ProjectCode = @ParentProjectCode
		), ops_candidates AS
		(
			SELECT *,
				ROW_NUMBER() OVER (PARTITION BY ProjectCode ORDER BY ProjectCode, OperationNumber DESC) AS LastOpRow,
				ROW_NUMBER() OVER (PARTITION BY ProjectCode ORDER BY ProjectCode, OperationNumber) AS FirstOpRow
			FROM ops_top_level
		), ops_unscheduled1 AS
		(
			SELECT ProjectCode, OperationNumber,
				CASE ProjectStatusCode 
					WHEN 0 THEN 0 
					WHEN 1 THEN 
						CASE WHEN FirstOpRow = 1 AND OpStatusCode < 1 THEN 1 ELSE OpStatusCode END				
					ELSE 2
					END AS OpStatusCode,
				CASE WHEN LastOpRow = 1 THEN App.fnAdjustToCalendar(ActionOn, OffsetDays) ELSE StartOn END AS StartOn,
				CASE WHEN LastOpRow = 1 THEN ActionOn ELSE EndOn END AS EndOn,
				LastOpRow,
				OffsetDays,
				CASE SyncTypeCode WHEN 1 THEN 0 ELSE OffsetDays END AS AsyncOffsetDays
			FROM ops_candidates
		)
		, ops_unscheduled2 AS
		(
			SELECT ProjectCode, OperationNumber, OpStatusCode, 
				FIRST_VALUE(EndOn) OVER (PARTITION BY ProjectCode ORDER BY OperationNumber DESC) AS ActionOn, 
				LAG(AsyncOffsetDays, 1, 0) OVER (PARTITION BY ProjectCode ORDER BY OperationNumber DESC) AS AsyncOffsetDays,
				OffsetDays
			FROM ops_unscheduled1
		), ops_scheduled AS
		(
			SELECT ProjectCode, OperationNumber, OpStatusCode,
				App.fnAdjustToCalendar(ActionOn, SUM(AsyncOffsetDays) OVER (PARTITION BY ProjectCode ORDER BY OperationNumber DESC)) AS EndOn,
				App.fnAdjustToCalendar(ActionOn, SUM(AsyncOffsetDays) OVER (PARTITION BY ProjectCode ORDER BY OperationNumber DESC) + OffsetDays) AS StartOn
			FROM ops_unscheduled2
		)
		UPDATE op
		SET OpStatusCode = ops_scheduled.OpStatusCode,
			StartOn = ops_scheduled.StartOn, EndOn = ops_scheduled.EndOn
		FROM Project.tbOp op JOIN ops_scheduled 
			ON op.ProjectCode = ops_scheduled.ProjectCode AND op.OperationNumber = ops_scheduled.OperationNumber;

		WITH first_op AS
		(
			SELECT Project.tbOp.ProjectCode, MIN(Project.tbOp.StartOn) EndOn
			FROM Project.tbOp
			WHERE  (Project.tbOp.ProjectCode = @ParentProjectCode)
			GROUP BY Project.tbOp.ProjectCode
		), parent_Project AS
		(
			SELECT  Project.tbProject.ProjectCode, ProjectStatusCode, Quantity, ISNULL(EndOn, Project.tbProject.ActionOn) AS EndOn, Project.tbProject.ActionOn
			FROM Project.tbProject LEFT OUTER JOIN first_op ON first_op.ProjectCode = Project.tbProject.ProjectCode
			WHERE  (Project.tbProject.ProjectCode = @ParentProjectCode)	
		), Project_flow AS
		(
			SELECT work_flow.ParentProjectCode, work_flow.ChildProjectCode, work_flow.StepNumber,
				CASE WHEN work_flow.UsedOnQuantity <> 0 THEN parent_Project.Quantity * work_flow.UsedOnQuantity ELSE child_Project.Quantity END AS Quantity, 
				CASE WHEN parent_Project.ProjectStatusCode < 3 AND child_Project.ProjectStatusCode < parent_Project.ProjectStatusCode 
					THEN parent_Project.ProjectStatusCode 
					ELSE child_Project.ProjectStatusCode 
					END AS ProjectStatusCode,
				CASE SyncTypeCode WHEN 2 THEN parent_Project.ActionOn ELSE parent_Project.EndOn END AS EndOn, 
				parent_Project.ActionOn,
				CASE SyncTypeCode WHEN 0 THEN 0 ELSE OffsetDays END  AS OffsetDays,
				CASE SyncTypeCode WHEN 1 THEN 0 ELSE OffsetDays END AS AsyncOffsetDays,
				SyncTypeCode
			FROM parent_Project 
				JOIN Project.tbFlow work_flow ON parent_Project.ProjectCode = work_flow.ParentProjectCode
				JOIN Project.tbProject child_Project ON work_flow.ChildProjectCode = child_Project.ProjectCode
				
		), calloff_Projects_lag AS
		(
			SELECT ParentProjectCode, ChildProjectCode, StepNumber, Quantity, ProjectStatusCode, ActionOn EndOn, OffsetDays, 
					LAG(AsyncOffsetDays, 1, 0) OVER (PARTITION BY ParentProjectCode ORDER BY StepNumber DESC) AS AsyncOffsetDays, 2SyncTypeCode	 
			FROM Project_flow
			WHERE EXISTS(SELECT * FROM Project_flow WHERE SyncTypeCode = 2)
				AND (StepNumber > (SELECT TOP 1 StepNumber FROM Project_flow WHERE SyncTypeCode = 0 ORDER BY StepNumber DESC)
					OR NOT EXISTS (SELECT * FROM Project_flow WHERE SyncTypeCode = 0))
		), calloff_Projects AS
		(
			SELECT ParentProjectCode, ChildProjectCode, StepNumber, Quantity, ProjectStatusCode, EndOn, OffsetDays, 
				SUM(AsyncOffsetDays) OVER (PARTITION BY ParentProjectCode ORDER BY StepNumber DESC) AS AsyncOffsetDays
			FROM calloff_Projects_lag
		), servicing_Projects_lag AS
		(
			SELECT ParentProjectCode, ChildProjectCode, StepNumber, Quantity, ProjectStatusCode, EndOn, OffsetDays, 
					LAG(AsyncOffsetDays, 1, 0) OVER (PARTITION BY ParentProjectCode ORDER BY StepNumber DESC) AS AsyncOffsetDays
			FROM Project_flow
			WHERE (StepNumber < (SELECT MIN(StepNumber) FROM calloff_Projects_lag))
				OR NOT EXISTS (SELECT * FROM Project_flow WHERE SyncTypeCode = 2)
		), servicing_Projects AS
		(
			SELECT ParentProjectCode, ChildProjectCode, StepNumber, Quantity, ProjectStatusCode, EndOn, OffsetDays, 
				SUM(AsyncOffsetDays) OVER (PARTITION BY ParentProjectCode ORDER BY StepNumber DESC) AS AsyncOffsetDays
			FROM servicing_Projects_lag
		), schedule AS
		(
			SELECT ChildProjectCode AS ProjectCode, Quantity, ProjectStatusCode, 
				DATEADD(DAY, (AsyncOffsetDays + OffsetDays) * -1, EndOn) AS ActionOn
			FROM calloff_Projects
			UNION
			SELECT ChildProjectCode AS ProjectCode, Quantity, ProjectStatusCode, 
				DATEADD(DAY, (AsyncOffsetDays + OffsetDays) * -1, EndOn) AS ActionOn
			FROM servicing_Projects
		)
		UPDATE Project
		SET
			Quantity = schedule.Quantity,
			ActionOn = schedule.ActionOn,
			ProjectStatusCode = schedule.ProjectStatusCode
		FROM Project.tbProject Project
			JOIN schedule ON Project.ProjectCode = schedule.ProjectCode;

		DECLARE child_Projects CURSOR LOCAL FOR
			SELECT ChildProjectCode FROM Project.tbFlow WHERE ParentProjectCode = @ParentProjectCode;

		DECLARE @ChildProjectCode NVARCHAR(20);

		OPEN child_Projects;

		FETCH NEXT FROM child_Projects INTO @ChildProjectCode
		
		WHILE (@@FETCH_STATUS = 0)
		BEGIN
			EXEC Project.proc_Schedule @ChildProjectCode
			FETCH NEXT FROM child_Projects INTO @ChildProjectCode
		END

		CLOSE child_Projects;
		DEALLOCATE child_Projects;

		IF @@NESTLEVEL = 1
			COMMIT TRANSACTION;

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_NextCode]...';


go

CREATE   PROCEDURE Project.proc_NextCode
	(
		@ObjectCode nvarchar(50),
		@ProjectCode nvarchar(20) OUTPUT
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@UserId nvarchar(10)
			, @NextProjectNumber int

		SELECT   @UserId = Usr.tbUser.UserId, @NextProjectNumber = Usr.tbUser.NextProjectNumber
		FROM         Usr.vwCredentials INNER JOIN
							Usr.tbUser ON Usr.vwCredentials.UserId = Usr.tbUser.UserId


		IF EXISTS(SELECT     App.tbRegister.NextNumber
				  FROM         Object.tbObject INNER JOIN
										App.tbRegister ON Object.tbObject.RegisterName = App.tbRegister.RegisterName
				  WHERE     ( Object.tbObject.ObjectCode = @ObjectCode))
			BEGIN
			DECLARE @RegisterName nvarchar(50)
			SELECT @RegisterName = App.tbRegister.RegisterName, @NextProjectNumber = App.tbRegister.NextNumber
			FROM         Object.tbObject INNER JOIN
										App.tbRegister ON Object.tbObject.RegisterName = App.tbRegister.RegisterName
			WHERE     ( Object.tbObject.ObjectCode = @ObjectCode)
			          
			UPDATE    App.tbRegister
			SET              NextNumber = NextNumber + 1
			WHERE     (RegisterName = @RegisterName)	
			END
		ELSE
			BEGIN	                      		
			UPDATE Usr.tbUser
			Set NextProjectNumber = NextProjectNumber + 1
			WHERE UserId = @UserId
			END
		                      
		SET @ProjectCode = CONCAT(@UserId, '_', FORMAT(@NextProjectNumber, '0000'))
			                      
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_AssignToParent]...';


go
CREATE PROCEDURE Project.proc_AssignToParent 
	(
	@ChildProjectCode nvarchar(20),
	@ParentProjectCode nvarchar(20)
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@ProjectTitle nvarchar(100)
			, @StepNumber smallint

		BEGIN TRANSACTION
		
		IF EXISTS (SELECT ParentProjectCode FROM Project.tbFlow WHERE ChildProjectCode = @ChildProjectCode)
			DELETE FROM Project.tbFlow WHERE ChildProjectCode = @ChildProjectCode

		IF EXISTS(SELECT     TOP 1 StepNumber
				  FROM         Project.tbFlow
				  WHERE     (ParentProjectCode = @ParentProjectCode))
			BEGIN
			SELECT  @StepNumber = MAX(StepNumber) 
			FROM         Project.tbFlow
			WHERE     (ParentProjectCode = @ParentProjectCode)
			SET @StepNumber = @StepNumber - (@StepNumber % 10) + 10		
			END
		ELSE
			SET @StepNumber = 10


		SELECT     @ProjectTitle = ProjectTitle
		FROM         Project.tbProject
		WHERE     (ProjectCode = @ParentProjectCode)		
	
		UPDATE    Project.tbProject
		SET              ProjectTitle = @ProjectTitle
		WHERE     (ProjectCode = @ChildProjectCode) AND ((ProjectTitle IS NULL) OR (ProjectTitle = ObjectCode))
	
		INSERT INTO Project.tbFlow
							  (ParentProjectCode, StepNumber, ChildProjectCode, UsedOnQuantity)
		VALUES     (@ParentProjectCode, @StepNumber, @ChildProjectCode, 0)
	
		COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_Project]...';


go
CREATE   PROCEDURE Project.proc_Project 
	(
	@ProjectCode nvarchar(20),
	@ParentProjectCode nvarchar(20) output
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SET @ParentProjectCode = @ProjectCode
		WHILE EXISTS(SELECT     ParentProjectCode
					 FROM         Project.tbFlow
					 WHERE     (ChildProjectCode = @ParentProjectCode))
			SELECT @ParentProjectCode = ParentProjectCode
					 FROM         Project.tbFlow
					 WHERE     (ChildProjectCode = @ParentProjectCode)
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_Parent]...';


go

 CREATE   PROCEDURE Project.proc_Parent 
	(
	@ProjectCode nvarchar(20),
	@ParentProjectCode nvarchar(20) output
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SET @ParentProjectCode = @ProjectCode
		IF EXISTS(SELECT     ParentProjectCode
					 FROM         Project.tbFlow
					 WHERE     (ChildProjectCode = @ParentProjectCode))
			SELECT @ParentProjectCode = ParentProjectCode
					 FROM         Project.tbFlow
					 WHERE     (ChildProjectCode = @ParentProjectCode)
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_Op]...';


go
CREATE PROCEDURE Project.proc_Op (@ProjectCode nvarchar(20))
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS (SELECT     ProjectCode
				   FROM         Project.tbOp
				   WHERE     (ProjectCode = @ProjectCode))
			BEGIN
			SELECT     Project.tbOp.*
				   FROM         Project.tbOp
				   WHERE     (ProjectCode = @ProjectCode)
			END
		ELSE
			BEGIN
			SELECT     Project.tbOp.*
				   FROM         Project.tbFlow INNER JOIN
										 Project.tbOp ON Project.tbFlow.ParentProjectCode = Project.tbOp.ProjectCode
				   WHERE     ( Project.tbFlow.ChildProjectCode = @ProjectCode)
			END
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_NextOperationNumber]...';


go

CREATE   PROCEDURE Project.proc_NextOperationNumber 
	(
	@ProjectCode nvarchar(20),
	@OperationNumber smallint = 10 output
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT     TOP 1 OperationNumber
				  FROM         Project.tbOp
				  WHERE     (ProjectCode = @ProjectCode))
			BEGIN
			SELECT  @OperationNumber = MAX(OperationNumber) 
			FROM         Project.tbOp
			WHERE     (ProjectCode = @ProjectCode)
			SET @OperationNumber = @OperationNumber - (@OperationNumber % 10) + 10		
			END
		ELSE
			SET @OperationNumber = 10
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_NextAttributeOrder]...';


go

CREATE   PROCEDURE Project.proc_NextAttributeOrder 
	(
	@ProjectCode nvarchar(20),
	@PrintOrder smallint = 10 output
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		IF EXISTS(SELECT     TOP 1 PrintOrder
				  FROM         Project.tbAttribute
				  WHERE     (ProjectCode = @ProjectCode))
			BEGIN
			SELECT  @PrintOrder = MAX(PrintOrder) 
			FROM         Project.tbAttribute
			WHERE     (ProjectCode = @ProjectCode)
			SET @PrintOrder = @PrintOrder - (@PrintOrder % 10) + 10		
			END
		ELSE
			SET @PrintOrder = 10
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_IsProject]...';


go

CREATE   PROCEDURE Project.proc_IsProject 
	(
	@ProjectCode nvarchar(20),
	@IsProject bit = 0 output
	)
  AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT     TOP 1 Attribute
				  FROM         Project.tbAttribute
				  WHERE     (ProjectCode = @ProjectCode))
			SET @IsProject = 1
		ELSE IF EXISTS (SELECT     TOP 1 ParentProjectCode, StepNumber
						FROM         Project.tbFlow
						WHERE     (ParentProjectCode = @ProjectCode))
			SET @IsProject = 1
		ELSE
			SET @IsProject = 0
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_FullyInvoiced]...';


go
CREATE   PROCEDURE Project.proc_FullyInvoiced
	(
	@ProjectCode nvarchar(20),
	@IsFullyInvoiced bit = 0 output
	)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@InvoiceValue decimal(18, 5)
			, @TotalCharge decimal(18, 5)

		SELECT @InvoiceValue = SUM(InvoiceValue)
		FROM         Invoice.tbProject
		WHERE     (ProjectCode = @ProjectCode)
	
	
		SELECT @TotalCharge = SUM(TotalCharge)
		FROM         Project.tbProject
		WHERE     (ProjectCode = @ProjectCode)
	
		IF (@TotalCharge = @InvoiceValue)
			SET @IsFullyInvoiced = 1
		ELSE
			SET @IsFullyInvoiced = 0	
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_CostSetAdd]...';


go
CREATE   PROCEDURE Project.proc_CostSetAdd(@ProjectCode nvarchar(20))
AS
	SET XACT_ABORT, NOCOUNT ON;
	BEGIN TRY
		DECLARE @UserId nvarchar(10) = (SELECT UserId FROM Usr.vwCredentials);
		IF NOT EXISTS (SELECT * FROM Project.tbCostSet WHERE UserId = @UserId AND ProjectCode = @ProjectCode)
		BEGIN
			INSERT INTO Project.tbCostSet (ProjectCode, UserId)
			VALUES (@ProjectCode, @UserId);
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_ResetChargedUninvoiced]...';


go

CREATE   PROCEDURE Project.proc_ResetChargedUninvoiced
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		UPDATE       Project
		SET                ProjectStatusCode = 2
		FROM            Cash.tbCode INNER JOIN
								 Project.tbProject AS Project ON Cash.tbCode.CashCode = Project.CashCode LEFT OUTER JOIN
								 Invoice.tbProject AS InvoiceProject ON Project.ProjectCode = InvoiceProject.ProjectCode AND Project.ProjectCode = InvoiceProject.ProjectCode
		WHERE        (InvoiceProject.InvoiceNumber IS NULL) AND (Project.ProjectStatusCode = 3)
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_ReconcileCharge]...';


go
CREATE   PROCEDURE Project.proc_ReconcileCharge
	(
	@ProjectCode nvarchar(20)
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @InvoiceValue decimal(18, 5)

		SELECT @InvoiceValue = SUM(InvoiceValue)
		FROM         Invoice.tbProject
		WHERE     (ProjectCode = @ProjectCode)

		UPDATE    Project.tbProject
		SET              TotalCharge = @InvoiceValue, UnitCharge = @InvoiceValue / Quantity
		WHERE     (ProjectCode = @ProjectCode)	
	
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_Copy]...';


go

CREATE   PROCEDURE Project.proc_Copy
	(
	@FromProjectCode nvarchar(20),
	@ParentProjectCode nvarchar(20) = null,
	@ToProjectCode nvarchar(20) = null output
	)
AS
	SET NOCOUNT, XACT_ABORT ON
	BEGIN TRY
		DECLARE 
			@ObjectCode nvarchar(50)
			, @Printed bit
			, @ChildProjectCode nvarchar(20)
			, @ProjectStatusCode smallint
			, @StepNumber smallint
			, @UserId nvarchar(10)
			, @SubjectCode nvarchar(10)

		SELECT @UserId = UserId FROM Usr.vwCredentials
	
		SELECT  
			@SubjectCode = Project.tbProject.SubjectCode,
			@ProjectStatusCode = Object.tbObject.ProjectStatusCode, 
			@ObjectCode = Project.tbProject.ObjectCode, 
			@Printed = CASE WHEN Object.tbObject.Printed = 0 THEN 1 ELSE 0 END
		FROM         Project.tbProject INNER JOIN
							  Object.tbObject ON Project.tbProject.ObjectCode = Object.tbObject.ObjectCode
		WHERE     ( Project.tbProject.ProjectCode = @FromProjectCode)
	
		EXEC Project.proc_NextCode @ObjectCode, @ToProjectCode output

		IF @@NESTLEVEL = 1
			BEGIN TRANSACTION;

		INSERT INTO Project.tbProject
							  (ProjectCode, UserId, SubjectCode, ProjectTitle, ContactName, ObjectCode, ProjectStatusCode, ActionById, ActionOn, ActionedOn, ProjectNotes, Quantity, 
							  SecondReference, CashCode, TaxCode, UnitCharge, TotalCharge, AddressCodeFrom, AddressCodeTo, Printed)
		SELECT     @ToProjectCode AS ToProjectCode, @UserId AS Owner, SubjectCode, ProjectTitle, ContactName, ObjectCode, @ProjectStatusCode AS ProjectStatus, 
							  @UserId AS ActionUserId, CAST(CURRENT_TIMESTAMP AS date) AS ActionOn, 
							  CASE WHEN @ProjectStatusCode > 1 THEN CAST(CURRENT_TIMESTAMP AS date) ELSE NULL END AS ActionedOn, ProjectNotes, 
							  Quantity, SecondReference, CashCode, TaxCode, UnitCharge, TotalCharge, AddressCodeFrom, AddressCodeTo, @Printed AS Printed
		FROM         Project.tbProject AS Project_tb1
		WHERE     (ProjectCode = @FromProjectCode)
	
		INSERT INTO Project.tbAttribute
							  (ProjectCode, Attribute, PrintOrder, AttributeTypeCode, AttributeDescription)
		SELECT     @ToProjectCode AS ToProjectCode, Attribute, PrintOrder, AttributeTypeCode, AttributeDescription
		FROM         Project.tbAttribute 
		WHERE     (ProjectCode = @FromProjectCode)
	
		INSERT INTO Project.tbQuote
							  (ProjectCode, Quantity, TotalPrice, RunOnQuantity, RunOnPrice, RunBackQuantity, RunBackPrice)
		SELECT     @ToProjectCode AS ToProjectCode, Quantity, TotalPrice, RunOnQuantity, RunOnPrice, RunBackQuantity, RunBackPrice
		FROM         Project.tbQuote 
		WHERE     (ProjectCode = @FromProjectCode)
	
		INSERT INTO Project.tbOp
							  (ProjectCode, OperationNumber, OpStatusCode, UserId, SyncTypeCode, Operation, Note, StartOn, EndOn, Duration, OffsetDays)
		SELECT     @ToProjectCode AS ToProjectCode, OperationNumber, 0 AS OpStatusCode, UserId, SyncTypeCode, Operation, Note, 
			CAST(CURRENT_TIMESTAMP AS date) AS StartOn, CAST(CURRENT_TIMESTAMP AS date) AS EndOn, Duration, OffsetDays
		FROM         Project.tbOp 
		WHERE     (ProjectCode = @FromProjectCode)
	
		IF (ISNULL(@ParentProjectCode, '') = '')
			BEGIN
			IF EXISTS(SELECT     ParentProjectCode
					FROM         Project.tbFlow
					WHERE     (ChildProjectCode = @FromProjectCode))
				BEGIN
				SELECT @ParentProjectCode = ParentProjectCode
				FROM         Project.tbFlow
				WHERE     (ChildProjectCode = @FromProjectCode)

				SELECT @StepNumber = MAX(StepNumber)
				FROM         Project.tbFlow
				WHERE     (ParentProjectCode = @ParentProjectCode)
				GROUP BY ParentProjectCode
				
				SET @StepNumber = @StepNumber - (@StepNumber % 10) + 10	
						
				INSERT INTO Project.tbFlow
				(ParentProjectCode, StepNumber, ChildProjectCode, SyncTypeCode, UsedOnQuantity, OffsetDays)
				SELECT TOP 1 ParentProjectCode, @StepNumber AS Step, @ToProjectCode AS ChildProject, SyncTypeCode, UsedOnQuantity, OffsetDays
				FROM         Project.tbFlow
				WHERE     (ChildProjectCode = @FromProjectCode)
				END
			END
		ELSE
			BEGIN		
			INSERT INTO Project.tbFlow
			(ParentProjectCode, StepNumber, ChildProjectCode, SyncTypeCode, UsedOnQuantity, OffsetDays)
			SELECT TOP 1 @ParentProjectCode As ParentProject, StepNumber, @ToProjectCode AS ChildProject, SyncTypeCode, UsedOnQuantity, OffsetDays
			FROM         Project.tbFlow 
			WHERE     (ChildProjectCode = @FromProjectCode)		
			END
	
		DECLARE curProject cursor local for			
			SELECT     ChildProjectCode
			FROM         Project.tbFlow
			WHERE     (ParentProjectCode = @FromProjectCode)
	
		OPEN curProject
	
		FETCH NEXT FROM curProject INTO @ChildProjectCode
		WHILE (@@FETCH_STATUS = 0)
			BEGIN
			EXEC Project.proc_Copy @ChildProjectCode, @ToProjectCode
			FETCH NEXT FROM curProject INTO @ChildProjectCode
			END
		
		CLOSE curProject
		DEALLOCATE curProject
		
		IF @@NESTLEVEL = 1
			BEGIN
			COMMIT TRANSACTION
			EXEC Project.proc_Schedule @ToProjectCode
			END

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_DefaultTaxCode]...';


go

CREATE   PROCEDURE Project.proc_DefaultTaxCode 
	(
	@SubjectCode nvarchar(10),
	@CashCode nvarchar(50),
	@TaxCode nvarchar(10) OUTPUT
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY		
		IF (NOT @SubjectCode IS NULL) and (NOT @CashCode IS NULL)
			BEGIN
			IF EXISTS(SELECT     TaxCode
				  FROM         Subject.tbSubject
				  WHERE     (SubjectCode = @SubjectCode) AND (NOT (TaxCode IS NULL)))
				BEGIN
				SELECT    @TaxCode = TaxCode
				FROM         Subject.tbSubject
				WHERE     (SubjectCode = @SubjectCode) AND (NOT (TaxCode IS NULL))
				END
			ELSE
				BEGIN
				SELECT    @TaxCode =  TaxCode
				FROM         Cash.tbCode
				WHERE     (CashCode = @CashCode)		
				END
			END
		ELSE
			SET @TaxCode = null
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_DefaultPaymentOn]...';


go

CREATE   PROCEDURE Project.proc_DefaultPaymentOn
	(
		@SubjectCode nvarchar(10),
		@ActionOn datetime,
		@PaymentOn datetime output
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		SELECT @ActionOn = CASE WHEN Subject.PayDaysFromMonthEnd <> 0 
				THEN 
					DATEADD(d, -1, DATEADD(d,  Subject.ExpectedDays, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, Subject.PaymentDays, @ActionOn), 'yyyyMM'), '01'))))												
				ELSE
					DATEADD(d, Subject.PaymentDays + Subject.ExpectedDays, @ActionOn)	
				END
		FROM Subject.tbSubject Subject 
		WHERE Subject.SubjectCode = @SubjectCode

		SELECT @PaymentOn = App.fnAdjustToCalendar(@ActionOn, 0) 					
	 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_EmailAddress]...';


go

CREATE   PROCEDURE Project.proc_EmailAddress 
	(
	@ProjectCode nvarchar(20),
	@EmailAddress nvarchar(255) OUTPUT
	)
AS
SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT     Subject.tbContact.EmailAddress
				  FROM         Subject.tbContact INNER JOIN
										Project.tbProject ON Subject.tbContact.SubjectCode = Project.tbProject.SubjectCode AND Subject.tbContact.ContactName = Project.tbProject.ContactName
				  WHERE     ( Project.tbProject.ProjectCode = @ProjectCode)
				  GROUP BY Subject.tbContact.EmailAddress
				  HAVING      (NOT ( Subject.tbContact.EmailAddress IS NULL)))
			BEGIN
			SELECT    @EmailAddress = Subject.tbContact.EmailAddress
			FROM         Subject.tbContact INNER JOIN
								tbProject ON Subject.tbContact.SubjectCode = Project.tbProject.SubjectCode AND Subject.tbContact.ContactName = Project.tbProject.ContactName
			WHERE     ( Project.tbProject.ProjectCode = @ProjectCode)
			GROUP BY Subject.tbContact.EmailAddress
			HAVING      (NOT ( Subject.tbContact.EmailAddress IS NULL))	
			END
		ELSE
			BEGIN
			SELECT    @EmailAddress =  Subject.tbSubject.EmailAddress
			FROM         Subject.tbSubject INNER JOIN
								 Project.tbProject ON Subject.tbSubject.SubjectCode = Project.tbProject.SubjectCode
			WHERE     ( Project.tbProject.ProjectCode = @ProjectCode)
			END
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_Mode]...';


go

CREATE   PROCEDURE Project.proc_Mode 
	(
	@ProjectCode nvarchar(20)
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SELECT     Project.tbProject.SubjectCode, Project.tbProject.ObjectCode, Project.tbProject.ProjectStatusCode, Project.tbProject.ActionOn, Project.vwCashPolarity.CashPolarityCode
		FROM         Project.tbProject LEFT OUTER JOIN
							  Project.vwCashPolarity ON Project.tbProject.ProjectCode = Project.vwCashPolarity.ProjectCode
		WHERE     ( Project.tbProject.ProjectCode = @ProjectCode)
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_WorkFlowSelected]...';


go

CREATE   PROCEDURE Project.proc_WorkFlowSelected 
	(
	@ChildProjectCode nvarchar(20),
	@ParentProjectCode nvarchar(20) = NULL
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF NOT (@ParentProjectCode IS NULL)
			SELECT        Project.tbProject.SubjectCode, Project.tbProject.ObjectCode, Project.tbProject.ProjectStatusCode, Project.tbProject.ActionOn, Project.vwCashPolarity.CashPolarityCode, Project.tbFlow.OffsetDays
			FROM            Project.tbProject INNER JOIN
									 Project.tbFlow ON Project.tbProject.ProjectCode = Project.tbFlow.ChildProjectCode LEFT OUTER JOIN
									 Project.vwCashPolarity ON Project.tbProject.ProjectCode = Project.vwCashPolarity.ProjectCode
			WHERE        (Project.tbFlow.ParentProjectCode = @ParentProjectCode) AND (Project.tbFlow.ChildProjectCode = @ChildProjectCode)
		ELSE
			SELECT        Project.tbProject.SubjectCode, Project.tbProject.ObjectCode, Project.tbProject.ProjectStatusCode, Project.tbProject.ActionOn, Project.vwCashPolarity.CashPolarityCode, 0 AS OffsetDays
			FROM            Project.tbProject LEFT OUTER JOIN
									 Project.vwCashPolarity ON Project.tbProject.ProjectCode = Project.vwCashPolarity.ProjectCode
			WHERE        (Project.tbProject.ProjectCode = @ChildProjectCode)
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_WorkFlow]...';


go

CREATE   PROCEDURE Project.proc_WorkFlow 
	(
	@ProjectCode nvarchar(20)
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SELECT     Project.tbFlow.ParentProjectCode, Project.tbFlow.StepNumber, Project.tbProject.ProjectCode, Project.tbProject.SubjectCode, Project.tbProject.ObjectCode, Project.tbProject.ProjectStatusCode, 
							  Project.tbProject.ActionOn, Project.vwCashPolarity.CashPolarityCode, Project.tbFlow.OffsetDays
		FROM         Project.tbProject INNER JOIN
							  Project.tbFlow ON Project.tbProject.ProjectCode = Project.tbFlow.ChildProjectCode LEFT OUTER JOIN
							  Project.vwCashPolarity ON Project.tbProject.ProjectCode = Project.vwCashPolarity.ProjectCode
		WHERE     ( Project.tbFlow.ParentProjectCode = @ProjectCode)
		ORDER BY Project.tbFlow.StepNumber, Project.tbFlow.ParentProjectCode
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_EmailFooter]...';


go

CREATE   PROCEDURE Project.proc_EmailFooter 
AS
--mod replace with view

	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SELECT        u.UserName, u.PhoneNumber, u.MobileNumber, o.SubjectName, o.WebSite
		FROM            Usr.vwCredentials AS c INNER JOIN
								 Usr.tbUser AS u ON c.UserId = u.UserId 
			CROSS JOIN
			(SELECT        TOP (1) Subject.tbSubject.SubjectName, Subject.tbSubject.WebSite
			FROM            Subject.tbSubject INNER JOIN
										App.tbOptions ON Subject.tbSubject.SubjectCode = App.tbOptions.SubjectCode) AS o

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_EmailDetail]...';


go

CREATE   PROCEDURE Project.proc_EmailDetail 
	(
	@ProjectCode nvarchar(20)
	)
AS
SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@NickName nvarchar(100)
			, @EmailAddress nvarchar(255)

		IF EXISTS(SELECT     Subject.tbContact.ContactName
				  FROM         Subject.tbContact INNER JOIN
										Project.tbProject ON Subject.tbContact.SubjectCode = Project.tbProject.SubjectCode AND Subject.tbContact.ContactName = Project.tbProject.ContactName
				  WHERE     ( Project.tbProject.ProjectCode = @ProjectCode))
			BEGIN
			SELECT  @NickName = CASE WHEN Subject.tbContact.NickName is null THEN Subject.tbContact.ContactName ELSE Subject.tbContact.NickName END
						  FROM         Subject.tbContact INNER JOIN
												tbProject ON Subject.tbContact.SubjectCode = Project.tbProject.SubjectCode AND Subject.tbContact.ContactName = Project.tbProject.ContactName
						  WHERE     ( Project.tbProject.ProjectCode = @ProjectCode)				
			END
		ELSE
			BEGIN
			SELECT @NickName = ContactName
			FROM         Project.tbProject
			WHERE     (ProjectCode = @ProjectCode)
			END
	
		EXEC Project.proc_EmailAddress	@ProjectCode, @EmailAddress output
	
		SELECT     Project.tbProject.ProjectCode, Project.tbProject.ProjectTitle, Subject.tbSubject.SubjectCode, Subject.tbSubject.SubjectName, @NickName AS NickName, @EmailAddress AS EmailAddress, 
							  Project.tbProject.ObjectCode, Project.tbStatus.ProjectStatus, Project.tbProject.ProjectNotes
		FROM         Project.tbProject INNER JOIN
							  Project.tbStatus ON Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode INNER JOIN
							  Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode
		WHERE     ( Project.tbProject.ProjectCode = @ProjectCode)

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_Configure]...';


go
CREATE   PROCEDURE Project.proc_Configure (@ParentProjectCode nvarchar(20))
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@StepNumber smallint
			, @ProjectCode nvarchar(20)
			, @UserId nvarchar(10)
			, @ObjectCode nvarchar(50)
			, @SubjectCode nvarchar(10)
			, @DefaultSubjectCode nvarchar(10)
			, @TaxCode nvarchar(10)

		IF @@NESTLEVEL = 1
			BEGIN TRANSACTION

		INSERT INTO Subject.tbContact 
			(SubjectCode, ContactName, FileAs, PhoneNumber, EmailAddress)
		SELECT Project.tbProject.SubjectCode, Project.tbProject.ContactName, Project.tbProject.ContactName AS NickName, Subject.tbSubject.PhoneNumber, Subject.tbSubject.EmailAddress
		FROM  Project.tbProject 
			INNER JOIN Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode
		WHERE LEN(ISNULL(Project.tbProject.ContactName, '')) > 0 AND (Project.tbProject.ProjectCode = @ParentProjectCode)
					AND EXISTS (SELECT *
								FROM Project.tbProject
								WHERE (ProjectCode = @ParentProjectCode) AND (NOT (ContactName IS NULL)) OR (ProjectCode = @ParentProjectCode) AND (ContactName <> N''))
				AND NOT EXISTS(SELECT *
								FROM  Project.tbProject 
									INNER JOIN Subject.tbContact ON Project.tbProject.SubjectCode = Subject.tbContact.SubjectCode AND Project.tbProject.ContactName = Subject.tbContact.ContactName
								WHERE     ( Project.tbProject.ProjectCode = @ParentProjectCode))
	
		UPDATE Subject.tbSubject
		SET SubjectStatusCode = 1
		FROM Subject.tbSubject INNER JOIN Project.tbProject ON Subject.tbSubject.SubjectCode = Project.tbProject.SubjectCode
		WHERE ( Project.tbProject.ProjectCode = @ParentProjectCode) AND ( Subject.tbSubject.SubjectStatusCode = 0)				
			AND EXISTS(SELECT *
				FROM  Subject.tbSubject INNER JOIN Project.tbProject ON Subject.tbSubject.SubjectCode = Project.tbProject.SubjectCode
				WHERE     ( Project.tbProject.ProjectCode = @ParentProjectCode) AND ( Subject.tbSubject.SubjectStatusCode = 0))
	          
		UPDATE    Project.tbProject
		SET  ActionedOn = ActionOn
		WHERE (ProjectCode = @ParentProjectCode)
			AND EXISTS(SELECT *
					  FROM Project.tbProject
					  WHERE (ProjectStatusCode = 2) AND (ProjectCode = @ParentProjectCode))

		UPDATE Project.tbProject
		SET ProjectTitle = ObjectCode
		WHERE (ProjectCode = @ParentProjectCode)
			AND EXISTS(SELECT *
				  FROM Project.tbProject
				  WHERE (ProjectCode = @ParentProjectCode) AND (ProjectTitle IS NULL))  	 				              
	     	
		INSERT INTO Project.tbAttribute
			(ProjectCode, Attribute, AttributeDescription, PrintOrder, AttributeTypeCode)
		SELECT Project.tbProject.ProjectCode, Object.tbAttribute.Attribute, Object.tbAttribute.DefaultText, Object.tbAttribute.PrintOrder, Object.tbAttribute.AttributeTypeCode
		FROM Object.tbAttribute 
			INNER JOIN Project.tbProject ON Object.tbAttribute.ObjectCode = Project.tbProject.ObjectCode
		WHERE     ( Project.tbProject.ProjectCode = @ParentProjectCode)
	
		INSERT INTO Project.tbOp
			(ProjectCode, UserId, OperationNumber, SyncTypeCode, Operation, Duration, OffsetDays, StartOn)
		SELECT Project.tbProject.ProjectCode, Project.tbProject.UserId, Object.tbOp.OperationNumber, Object.tbOp.SyncTypeCode, Object.tbOp.Operation, Object.tbOp.Duration,  Object.tbOp.OffsetDays, Project.tbProject.ActionOn
		FROM Object.tbOp INNER JOIN Project.tbProject ON Object.tbOp.ObjectCode = Project.tbProject.ObjectCode
		WHERE     ( Project.tbProject.ProjectCode = @ParentProjectCode)
	                   
	
		SELECT @UserId = UserId FROM Project.tbProject WHERE Project.tbProject.ProjectCode = @ParentProjectCode
	
		DECLARE curAct cursor local for
			SELECT Object.tbFlow.StepNumber
			FROM Object.tbFlow INNER JOIN Project.tbProject ON Object.tbFlow.ParentCode = Project.tbProject.ObjectCode
			WHERE     ( Project.tbProject.ProjectCode = @ParentProjectCode)
			ORDER BY Object.tbFlow.StepNumber	
	
		OPEN curAct
		FETCH NEXT FROM curAct INTO @StepNumber
		WHILE @@FETCH_STATUS = 0
			BEGIN
			SELECT  
				@ObjectCode = Object.tbObject.ObjectCode, 
				@SubjectCode = Project.tbProject.SubjectCode
			FROM Object.tbFlow 
				INNER JOIN Object.tbObject ON Object.tbFlow.ChildCode = Object.tbObject.ObjectCode 
				INNER JOIN Project.tbProject ON Object.tbFlow.ParentCode = Project.tbProject.ObjectCode
			WHERE     ( Object.tbFlow.StepNumber = @StepNumber) AND ( Project.tbProject.ProjectCode = @ParentProjectCode)
		
			EXEC Project.proc_NextCode @ObjectCode, @ProjectCode output

			INSERT INTO Project.tbProject
				(ProjectCode, UserId, SubjectCode, ContactName, ObjectCode, ProjectStatusCode, ActionById, ActionOn, ProjectNotes, Quantity, UnitCharge, AddressCodeFrom, AddressCodeTo, CashCode, Printed, ProjectTitle)
			SELECT  @ProjectCode AS NewProject, Project_tb1.UserId, Project_tb1.SubjectCode, Project_tb1.ContactName, Object.tbObject.ObjectCode, Object.tbObject.ProjectStatusCode, 
						Project_tb1.ActionById, Project_tb1.ActionOn, Object.tbObject.ObjectDescription, Project_tb1.Quantity * Object.tbFlow.UsedOnQuantity AS Quantity,
						Object.tbObject.UnitCharge, Subject.tbSubject.AddressCode AS AddressCodeFrom, Subject.tbSubject.AddressCode AS AddressCodeTo, 
						tbObject.CashCode, CASE WHEN Object.tbObject.Printed = 0 THEN 1 ELSE 0 END AS Printed, Project_tb1.ProjectTitle
			FROM  Object.tbFlow 
				INNER JOIN Object.tbObject ON Object.tbFlow.ChildCode = Object.tbObject.ObjectCode 
				INNER JOIN Project.tbProject Project_tb1 ON Object.tbFlow.ParentCode = Project_tb1.ObjectCode 
				INNER JOIN Subject.tbSubject ON Project_tb1.SubjectCode = Subject.tbSubject.SubjectCode
			WHERE     ( Object.tbFlow.StepNumber = @StepNumber) AND ( Project_tb1.ProjectCode = @ParentProjectCode)

			IF EXISTS (SELECT * FROM Project.tbProject 
							INNER JOIN  Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode 
							INNER JOIN App.tbTaxCode ON Subject.tbSubject.TaxCode = App.tbTaxCode.TaxCode AND Subject.tbSubject.TaxCode = App.tbTaxCode.TaxCode)
				BEGIN
				UPDATE Project.tbProject
				SET TaxCode = App.tbTaxCode.TaxCode
				FROM Project.tbProject 
					INNER JOIN Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode 
					INNER JOIN App.tbTaxCode ON Subject.tbSubject.TaxCode = App.tbTaxCode.TaxCode AND Subject.tbSubject.TaxCode = App.tbTaxCode.TaxCode
				WHERE (Project.tbProject.ProjectCode = @ProjectCode)
				END
			ELSE
				BEGIN
				UPDATE Project.tbProject
				SET TaxCode = Cash.tbCode.TaxCode
				FROM  Project.tbProject 
					INNER JOIN Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode
				WHERE  (Project.tbProject.ProjectCode = @ProjectCode)
				END			
			
			SELECT @DefaultSubjectCode = (SELECT TOP 1  SubjectCode FROM Project.tbProject
											WHERE   (ObjectCode = (SELECT ObjectCode FROM  Project.tbProject AS tbProject_1 WHERE (ProjectCode = @ProjectCode))) AND (ProjectCode <> @ProjectCode))

			IF NOT @DefaultSubjectCode IS NULL
				BEGIN
				UPDATE Project.tbProject
				SET SubjectCode = @DefaultSubjectCode
				WHERE (ProjectCode = @ProjectCode)
				END
					
			INSERT INTO Project.tbFlow
				(ParentProjectCode, StepNumber, ChildProjectCode, SyncTypeCode, UsedOnQuantity, OffsetDays)
			SELECT Project.tbProject.ProjectCode, Object.tbFlow.StepNumber, @ProjectCode AS ChildProjectCode, Object.tbFlow.SyncTypeCode, Object.tbFlow.UsedOnQuantity, Object.tbFlow.OffsetDays
			FROM Object.tbFlow 
				INNER JOIN Project.tbProject ON Object.tbFlow.ParentCode = Project.tbProject.ObjectCode
			WHERE (Project.tbProject.ProjectCode = @ParentProjectCode) AND ( Object.tbFlow.StepNumber = @StepNumber)
		
			EXEC Project.proc_Configure @ProjectCode

			FETCH NEXT FROM curAct INTO @StepNumber
			END
	
		CLOSE curAct
		DEALLOCATE curAct
		
		IF @@NESTLEVEL = 1
			COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [dbo].[AspNetUsers_TriggerInsert]...';


go

CREATE TRIGGER dbo.AspNetUsers_TriggerInsert 
   ON dbo.AspNetUsers
   AFTER INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		
		IF NOT EXISTS(SELECT * FROM Usr.tbUser)
		BEGIN
			UPDATE AspNetUsers
			SET EmailConfirmed = 1
			FROM AspNetUsers 
				JOIN inserted ON AspNetUsers.Id = inserted.Id;

			INSERT INTO AspNetUserRoles (UserId, RoleId)
			SELECT inserted.Id UserId, (SELECT Id FROM AspNetRoles WHERE [Name] = 'Administrators') RoleId 
			FROM inserted; 
		END
		ELSE IF EXISTS (SELECT * FROM inserted 
					JOIN Usr.tbUser ON inserted.UserName = Usr.tbUser.EmailAddress
					WHERE Usr.tbUser.IsAdministrator <> 0)
		BEGIN
			UPDATE AspNetUsers
			SET EmailConfirmed = 1
			FROM AspNetUsers 
				JOIN inserted ON AspNetUsers.Id = inserted.Id
				JOIN Usr.tbUser ON inserted.UserName = Usr.tbUser.EmailAddress
					WHERE Usr.tbUser.IsAdministrator <> 0;

			INSERT INTO AspNetUserRoles (UserId, RoleId)
			SELECT inserted.Id UserId, (SELECT Id FROM AspNetRoles WHERE [Name] = 'Administrators') RoleId 
				FROM inserted 
					JOIN Usr.tbUser ON inserted.UserName = Usr.tbUser.EmailAddress
			WHERE Usr.tbUser.IsAdministrator <> 0
		END

		UPDATE AspNetUsers
		SET PhoneNumber = Usr.tbUser.PhoneNumber, PhoneNumberConfirmed = 1
		FROM AspNetUsers 
			JOIN inserted ON AspNetUsers.Id = inserted.Id
			JOIN Usr.tbUser ON inserted.UserName = Usr.tbUser.EmailAddress;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

END
go
PRINT N'Creating Trigger [dbo].[AspNetUsers_TriggerUpdate]...';


go
CREATE TRIGGER dbo.AspNetUsers_TriggerUpdate 
   ON dbo.AspNetUsers
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY

		IF UPDATE (EmailConfirmed)
			AND NOT EXISTS (SELECT * FROM inserted  JOIN Usr.tbUser ON inserted.Email = Usr.tbUser.EmailAddress )
			AND EXISTS (SELECT * FROM Usr.tbUser)
		BEGIN			
			ROLLBACK TRANSACTION;
			EXEC App.proc_EventLog 'Unregistered ASP.NET users cannot be confirmed';
		END

		IF UPDATE (PhoneNumber)
		BEGIN
			UPDATE Usr.tbUser
			SET PhoneNumber = inserted.PhoneNumber
			FROM inserted
				JOIN Usr.tbUser u ON inserted.UserName = u.EmailAddress
		END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

END
go
PRINT N'Creating Trigger [Usr].[Usr_tbUser_TriggerUpdate]...';


go
CREATE TRIGGER Usr.Usr_tbUser_TriggerUpdate 
   ON  Usr.tbUser
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		IF NOT UPDATE(UserName)
		BEGIN
			UPDATE Usr.tbUser
			SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
			FROM Usr.tbUser INNER JOIN inserted AS i ON tbUser.UserId = i.UserId;
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Usr].[Usr_tbMenuEntry_TriggerUpdate]...';


go
CREATE   TRIGGER Usr.Usr_tbMenuEntry_TriggerUpdate 
   ON  Usr.tbMenuEntry
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE Usr.tbMenuEntry
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Usr.tbMenuEntry INNER JOIN inserted AS i ON tbMenuEntry.EntryId = i.EntryId AND tbMenuEntry.EntryId = i.EntryId;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Invoice].[Invoice_tbProject_TriggerDelete]...';


go
CREATE   TRIGGER Invoice.Invoice_tbProject_TriggerDelete
ON Invoice.tbProject
FOR DELETE
AS
	SET NOCOUNT ON;

	BEGIN TRY
		UPDATE Project.tbProject
		SET ProjectStatusCode = 2
		FROM deleted JOIN Project.tbProject ON deleted.ProjectCode = Project.tbProject.ProjectCode
		WHERE ProjectStatusCode = 3;		
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Invoice].[Invoice_tbProject_TriggerInsert]...';


go
CREATE TRIGGER Invoice.Invoice_tbProject_TriggerInsert
ON Invoice.tbProject
FOR INSERT, UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY

		UPDATE Project
		SET InvoiceValue = inserted.TotalValue / (1 + TaxRate),
			TaxValue = inserted.TotalValue - inserted.TotalValue / (1 + TaxRate)
		FROM inserted 
			INNER JOIN Invoice.tbProject Project ON inserted.InvoiceNumber = Project.InvoiceNumber 
					AND inserted.ProjectCode = Project.ProjectCode
				INNER JOIN App.tbTaxCode ON inserted.TaxCode = App.tbTaxCode.TaxCode 
		WHERE inserted.TotalValue <> 0;

		UPDATE Project
		SET TaxValue = CASE App.tbTaxCode.RoundingCode 
				WHEN 0 THEN ROUND(Project.InvoiceValue * App.tbTaxCode.TaxRate, Decimals)
				WHEN 1 THEN ROUND(Project.InvoiceValue * App.tbTaxCode.TaxRate, Decimals, 1) 
			END
		FROM Invoice.tbProject Project 
			INNER JOIN inserted ON inserted.InvoiceNumber = Project.InvoiceNumber
					 AND inserted.ProjectCode = Project.ProjectCode
				INNER JOIN App.tbTaxCode ON Project.TaxCode = App.tbTaxCode.TaxCode
		WHERE inserted.TotalValue = 0; 

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Invoice].[Invoice_tbItem_TriggerInsert]...';


go
CREATE TRIGGER Invoice.Invoice_tbItem_TriggerInsert
ON Invoice.tbItem
FOR INSERT, UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY

		UPDATE item
		SET InvoiceValue = inserted.TotalValue / (1 + TaxRate),
			TaxValue = inserted.TotalValue - inserted.TotalValue / (1 + TaxRate)
		FROM inserted 
			INNER JOIN Invoice.tbItem item ON inserted.InvoiceNumber = item.InvoiceNumber 
					AND inserted.CashCode = item.CashCode
				INNER JOIN App.tbTaxCode ON inserted.TaxCode = App.tbTaxCode.TaxCode 
		WHERE inserted.TotalValue <> 0;

		UPDATE item
		SET TaxValue = CASE App.tbTaxCode.RoundingCode 
				WHEN 0 THEN ROUND(item.InvoiceValue * App.tbTaxCode.TaxRate, Decimals)
				WHEN 1 THEN ROUND(item.InvoiceValue * App.tbTaxCode.TaxRate, Decimals, 1) 
			END
		FROM Invoice.tbItem item 
			INNER JOIN inserted ON inserted.InvoiceNumber = item.InvoiceNumber
					 AND inserted.CashCode = item.CashCode
				INNER JOIN App.tbTaxCode ON item.TaxCode = App.tbTaxCode.TaxCode
		WHERE inserted.TotalValue = 0; 

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Invoice].[Invoice_tbInvoice_TriggerDelete]...';


go
CREATE TRIGGER Invoice.Invoice_tbInvoice_TriggerDelete
ON Invoice.tbInvoice
FOR DELETE
AS
	SET NOCOUNT ON;

	BEGIN TRY

		IF EXISTS (SELECT * FROM deleted INNER JOIN Subject.tbSubject ON deleted.SubjectCode = Subject.tbSubject.SubjectCode WHERE Subject.tbSubject.TransmitStatusCode > 1)
		BEGIN
			DECLARE @Msg NVARCHAR(MAX);
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 1220;
			RAISERROR (@Msg, 10, 1)
		END
		
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Invoice].[Invoice_tbInvoice_TriggerUpdate]...';


go

CREATE TRIGGER Invoice.Invoice_tbInvoice_TriggerUpdate
ON Invoice.tbInvoice
FOR UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY
		IF UPDATE(Spooled)
		BEGIN
			INSERT INTO App.tbDocSpool (DocTypeCode, DocumentNumber)
			SELECT     App.fnDocInvoiceType(i.InvoiceTypeCode) AS DocTypeCode, i.InvoiceNumber
			FROM         inserted i 
			WHERE     (i.Spooled <> 0)

			DELETE App.tbDocSpool
			FROM         inserted i INNER JOIN
								  App.tbDocSpool ON i.InvoiceNumber = App.tbDocSpool.DocumentNumber
			WHERE    (i.Spooled = 0) AND ( App.tbDocSpool.DocTypeCode > 3)
		END


		IF UPDATE(InvoicedOn) AND EXISTS (
				SELECT * FROM inserted JOIN deleted 
					ON inserted.InvoiceNumber = deleted.InvoiceNumber AND inserted.DueOn = deleted.DueOn)
		BEGIN
			UPDATE invoice
			SET DueOn = App.fnAdjustToCalendar(CASE WHEN Subject.PayDaysFromMonthEnd <> 0 
													THEN 
														DATEADD(d, -1, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, Subject.PaymentDays, i.InvoicedOn), 'yyyyMM'), '01')))												
													ELSE
														DATEADD(d, Subject.PaymentDays, invoice.InvoicedOn)	
													END, 0)		
				FROM Invoice.tbInvoice invoice
					JOIN inserted i ON i.InvoiceNumber = invoice.InvoiceNumber
					JOIN Subject.tbSubject Subject ON i.SubjectCode = Subject.SubjectCode
				WHERE i.InvoiceTypeCode = 0;
		END;	

		IF UPDATE(InvoicedOn) AND EXISTS (
				SELECT * FROM inserted JOIN deleted 
					ON inserted.InvoiceNumber = deleted.InvoiceNumber AND inserted.ExpectedOn = deleted.ExpectedOn)
		BEGIN
			UPDATE invoice
			SET ExpectedOn = App.fnAdjustToCalendar(CASE WHEN Subject.PayDaysFromMonthEnd <> 0 
													THEN 
														DATEADD(d, -1, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, Subject.PaymentDays + Subject.ExpectedDays, i.InvoicedOn), 'yyyyMM'), '01')))												
													ELSE
														DATEADD(d, Subject.PaymentDays + Subject.ExpectedDays, invoice.InvoicedOn)	
													END, 0)		
				FROM Invoice.tbInvoice invoice
					JOIN inserted i ON i.InvoiceNumber = invoice.InvoiceNumber
					JOIN Subject.tbSubject Subject ON i.SubjectCode = Subject.SubjectCode
				WHERE i.InvoiceTypeCode = 0;
		END;	
		
		WITH invoices AS
		(
			SELECT inserted.InvoiceNumber, inserted.SubjectCode, inserted.InvoiceStatusCode, inserted.DueOn, inserted.InvoiceValue, inserted.TaxValue, inserted.PaidValue, inserted.PaidTaxValue FROM inserted JOIN deleted ON inserted.InvoiceNumber = deleted.InvoiceNumber WHERE inserted.InvoiceStatusCode = 1 AND deleted.InvoiceStatusCode = 0
		)
		INSERT INTO Invoice.tbChangeLog (InvoiceNumber, TransmitStatusCode, InvoiceStatusCode, DueOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue)
		SELECT InvoiceNumber, Subjects.TransmitStatusCode, InvoiceStatusCode, DueOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue
		FROM invoices JOIN Subject.tbSubject Subjects ON invoices.SubjectCode = Subjects.SubjectCode;

		IF UPDATE(InvoiceStatusCode) OR UPDATE(DueOn) OR UPDATE(PaidValue) OR UPDATE(PaidTaxValue) OR UPDATE(InvoiceValue) OR UPDATE (TaxValue)
		BEGIN
			WITH candidates AS
			(
				SELECT InvoiceNumber, SubjectCode, InvoiceStatusCode, DueOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue 
				FROM inserted
				WHERE EXISTS (SELECT * FROM Invoice.tbChangeLog WHERE InvoiceNumber = inserted.InvoiceNumber)
			)
			, logs AS
			(
				SELECT clog.LogId, clog.InvoiceNumber, clog.InvoiceStatusCode, clog.TransmitStatusCode, clog.DueOn, clog.InvoiceValue, clog.TaxValue, clog.PaidValue, clog.PaidTaxValue 
				FROM Invoice.tbChangeLog clog
				JOIN candidates ON clog.InvoiceNumber = candidates.InvoiceNumber AND LogId = (SELECT MAX(LogId) FROM Invoice.tbChangeLog WHERE InvoiceNumber = candidates.InvoiceNumber)		
			)
			INSERT INTO Invoice.tbChangeLog
									 (InvoiceNumber, TransmitStatusCode, InvoiceStatusCode, DueOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue)
			SELECT candidates.InvoiceNumber, CASE Subjects.TransmitStatusCode WHEN 1 THEN 2 ELSE 0 END TransmitStatusCode, candidates.InvoiceStatusCode,
				candidates.DueOn, candidates.InvoiceValue, candidates.TaxValue, candidates.PaidValue, candidates.PaidTaxValue
			FROM candidates 
				JOIN Subject.tbSubject Subjects ON candidates.SubjectCode = Subjects.SubjectCode 
				JOIN logs ON candidates.InvoiceNumber = logs.InvoiceNumber
			WHERE (logs.InvoiceStatusCode <> candidates.InvoiceStatusCode) 
				OR (logs.TransmitStatusCode < 2)
				OR (logs.DueOn <> candidates.DueOn) 
				OR ((logs.InvoiceValue + logs.TaxValue + logs.PaidValue + logs.PaidTaxValue) 
						<> (candidates.InvoiceValue + candidates.TaxValue + candidates.PaidValue + candidates.PaidTaxValue))
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Invoice].[Invoice_tbInvoice_TriggerInsert]...';


go
CREATE TRIGGER Invoice.Invoice_tbInvoice_TriggerInsert
ON Invoice.tbInvoice
FOR INSERT
AS
	SET NOCOUNT ON;

	BEGIN TRY
		UPDATE invoice
		SET DueOn = App.fnAdjustToCalendar(CASE WHEN Subject.PayDaysFromMonthEnd <> 0 
												THEN 
													DATEADD(d, -1, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, Subject.PaymentDays, i.InvoicedOn), 'yyyyMM'), '01')))												
												ELSE
													DATEADD(d, Subject.PaymentDays, invoice.InvoicedOn)	
												END, 0),
			ExpectedOn = App.fnAdjustToCalendar(CASE WHEN Subject.PayDaysFromMonthEnd <> 0 
												THEN 
													DATEADD(d, -1, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, Subject.PaymentDays + Subject.ExpectedDays, i.InvoicedOn), 'yyyyMM'), '01')))												
												ELSE
													DATEADD(d, Subject.PaymentDays + Subject.ExpectedDays, invoice.InvoicedOn)	
												END, 0)				 
		FROM Invoice.tbInvoice invoice
			JOIN inserted i ON i.InvoiceNumber = invoice.InvoiceNumber
			JOIN Subject.tbSubject Subject ON i.SubjectCode = Subject.SubjectCode
		WHERE i.InvoiceTypeCode = 0;

		INSERT INTO Invoice.tbChangeLog
								 (InvoiceNumber, TransmitStatusCode, InvoiceStatusCode, DueOn, InvoiceValue, TaxValue)
		SELECT      inserted.InvoiceNumber, Subject.tbSubject.TransmitStatusCode, inserted.InvoiceStatusCode, inserted.DueOn, inserted.InvoiceValue, inserted.TaxValue
		FROM            inserted INNER JOIN
								 Subject.tbSubject ON inserted.SubjectCode = Subject.tbSubject.SubjectCode
		WHERE InvoiceStatusCode > 0
								 
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Invoice].[Invoice_tbMirrorProject_TriggerInsert]...';


go
CREATE   TRIGGER Invoice.Invoice_tbMirrorProject_TriggerInsert
ON Invoice.tbMirrorProject
FOR INSERT
AS
	SET NOCOUNT ON;

	BEGIN TRY
		WITH deliveries AS
		(
			SELECT mirror.SubjectCode, inserted.ProjectCode, 
				CASE mirror.InvoiceTypeCode
					WHEN 0 THEN inserted.Quantity
					WHEN 1 THEN inserted.Quantity * -1
					WHEN 2 THEN inserted.Quantity
					WHEN 3 THEN inserted.Quantity * -1
					ELSE 0
				END QuantityDelivered
			FROM inserted
				JOIN Invoice.tbMirror mirror ON inserted.ContractAddress = mirror.ContractAddress
		)
		UPDATE allocs
		SET QuantityDelivered += deliveries.QuantityDelivered
		FROM Project.tbAllocation allocs
			JOIN deliveries ON allocs.SubjectCode = deliveries.SubjectCode AND allocs.ProjectCode = deliveries.ProjectCode;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Invoice].[Invoice_tbMirror_TriggerInsert]...';


go
CREATE   TRIGGER Invoice.Invoice_tbMirror_TriggerInsert
ON Invoice.tbMirror
FOR INSERT
AS
	SET NOCOUNT ON;

	BEGIN TRY
		INSERT INTO Invoice.tbMirrorEvent (ContractAddress, EventTypeCode, InvoiceStatusCode, DueOn, PaidValue, PaidTaxValue)
		SELECT ContractAddress, 2 EventTypeCode, InvoiceStatusCode, DueOn, PaidValue, PaidTaxValue
		FROM inserted;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Invoice].[Invoice_tbMirror_TriggerUpdate]...';


go

CREATE TRIGGER Invoice.Invoice_tbMirror_TriggerUpdate
ON Invoice.tbMirror
FOR UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY

		IF UPDATE(InvoiceStatusCode)
		BEGIN
			INSERT INTO Invoice.tbMirrorEvent (ContractAddress, EventTypeCode, InvoiceStatusCode, DueOn, PaidValue, PaidTaxValue)
			SELECT i.ContractAddress, 6 EventTypeCode, i.InvoiceStatusCode, i.DueOn, i.PaidValue, i.PaidTaxValue
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE d.InvoiceStatusCode <> i.InvoiceStatusCode;	
		END

		IF UPDATE(DueOn)
		BEGIN
			INSERT INTO Invoice.tbMirrorEvent (ContractAddress, EventTypeCode, InvoiceStatusCode, DueOn, PaidValue, PaidTaxValue)
			SELECT i.ContractAddress, 4 EventTypeCode, i.InvoiceStatusCode, i.DueOn, i.PaidValue, i.PaidTaxValue
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE d.DueOn <> i.DueOn;
		END

		IF UPDATE(PaidValue) OR UPDATE(PaidTaxValue)
		BEGIN
			INSERT INTO Invoice.tbMirrorEvent (ContractAddress, EventTypeCode, InvoiceStatusCode, DueOn, PaidValue, PaidTaxValue)
			SELECT i.ContractAddress, 7 EventTypeCode, i.InvoiceStatusCode, i.DueOn, i.PaidValue, i.PaidTaxValue
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE (d.PaidValue + d.PaidTaxValue) <> (i.PaidValue + i.PaidTaxValue);
		END

		IF UPDATE(PaymentAddress)
		BEGIN
			INSERT INTO Invoice.tbMirrorEvent (ContractAddress, EventTypeCode, InvoiceStatusCode, DueOn, PaidValue, PaidTaxValue, PaymentAddress)
			SELECT i.ContractAddress, 8 EventTypeCode, i.InvoiceStatusCode, i.DueOn, i.PaidValue, i.PaidTaxValue, i.PaymentAddress
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE d.PaymentAddress <> i.PaymentAddress;
		END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Cash].[Cash_tbTx_Trigger]...';


go
CREATE   TRIGGER Cash.Cash_tbTx_Trigger
   ON  Cash.tbTx
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY

		WITH payment AS
		(
			SELECT PaymentAddress
			FROM inserted tx
		), balance_base AS
		(
			SELECT tx.PaymentAddress, tx.TxStatusCode, SUM(CASE WHEN tx.TxStatusCode > 0 THEN tx.MoneyIn ELSE 0 END) Balance
			FROM Cash.tbTx tx JOIN payment ON tx.PaymentAddress = payment.PaymentAddress
			GROUP BY tx.PaymentAddress, tx.TxStatusCode
		), tx_balance AS
		(
			SELECT  PaymentAddress, MIN(TxStatusCode) TxStatusCode, SUM(Balance) Balance
			FROM balance_base
			GROUP BY PaymentAddress
		)
		UPDATE change
		SET	ChangeStatusCode =	CASE
									WHEN tx_balance.TxStatusCode = 2 THEN tx_balance.TxStatusCode
									WHEN tx_balance.Balance > 0 THEN 1
									ELSE tx_balance.TxStatusCode
								END 
		FROM tx_balance
			JOIN Cash.tbChange change ON tx_balance.PaymentAddress = change.PaymentAddress;		

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Cash].[Cash_tbTx_Trigger_Delete]...';


go
CREATE   TRIGGER [Cash].[Cash_tbTx_Trigger_Delete]
   ON  [Cash].[tbTx]
   AFTER DELETE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY

		WITH payment AS
		(
			SELECT PaymentAddress, 0 Balance
			FROM deleted tx
		), balance_base AS
		(
			SELECT tx.PaymentAddress, tx.TxStatusCode, SUM(CASE WHEN tx.TxStatusCode > 0 THEN tx.MoneyIn ELSE 0 END) Balance
			FROM Cash.tbTx tx JOIN payment ON tx.PaymentAddress = payment.PaymentAddress
			GROUP BY tx.PaymentAddress, tx.TxStatusCode
		), tx_balance AS
		(
			SELECT  PaymentAddress, MIN(TxStatusCode) TxStatusCode, SUM(Balance) Balance
			FROM balance_base
			GROUP BY PaymentAddress
		)
		UPDATE change
		SET	ChangeStatusCode =	CASE
									WHEN tx_balance.TxStatusCode = 2 THEN tx_balance.TxStatusCode
									WHEN tx_balance.Balance > 0 THEN 1
									ELSE tx_balance.TxStatusCode
								END 
		FROM tx_balance
			JOIN Cash.tbChange change ON tx_balance.PaymentAddress = change.PaymentAddress;	

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Cash].[Cash_tbChangeReference_TriggerInsert]...';


go
CREATE   TRIGGER Cash.Cash_tbChangeReference_TriggerInsert
ON Cash.tbChangeReference
FOR INSERT, UPDATE
AS
	SET NOCOUNT ON;
	BEGIN TRY
		INSERT INTO Invoice.tbChangeLog (InvoiceNumber, TransmitStatusCode, InvoiceStatusCode, DueOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue)
		SELECT invoices.InvoiceNumber, 2 TransmitStatusCode, InvoiceStatusCode, DueOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue
		FROM Cash.tbChangeReference inserted 
			JOIN Invoice.tbInvoice invoices ON inserted.InvoiceNumber = invoices.InvoiceNumber;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Cash].[Cash_tbTxReference_TriggerDelete]...';


go
CREATE   TRIGGER Cash.Cash_tbTxReference_TriggerDelete
   ON  Cash.tbTxReference
   AFTER DELETE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE Cash.tbTx
		SET 
			TxStatusCode = CASE change.ChangeTypeCode WHEN 0 THEN 0 ELSE 1 END
		FROM deleted 
			JOIN Cash.tbTx tx ON deleted.TxNumber = tx.TxNumber
			JOIN Cash.tbChange change ON tx.PaymentAddress = change.PaymentAddress
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Cash].[Cash_tbCode_TriggerUpdate]...';


go
CREATE TRIGGER Cash.Cash_tbCode_TriggerUpdate
   ON  Cash.tbCode
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		IF EXISTS (SELECT * FROM inserted i WHERE App.fnParsePrimaryKey(CashCode) = 0)
			BEGIN
			DECLARE @Msg NVARCHAR(MAX);
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 2004;
			RAISERROR (@Msg, 10, 1);
			ROLLBACK
			END
		ELSE IF NOT UPDATE(UpdatedBy)
			BEGIN
			UPDATE Cash.tbCode
			SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
			FROM Cash.tbCode INNER JOIN inserted AS i ON tbCode.CashCode = i.CashCode;
			END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Cash].[Cash_tbCategory_TriggerUpdate]...';


go
CREATE TRIGGER Cash.Cash_tbCategory_TriggerUpdate 
   ON  Cash.tbCategory
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		IF EXISTS (SELECT * FROM inserted i WHERE App.fnParsePrimaryKey(CategoryCode) = 0)
		BEGIN
			DECLARE @Msg NVARCHAR(MAX);
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 2004;
			RAISERROR (@Msg, 10, 1)
			ROLLBACK
		END

		IF UPDATE (IsEnabled)
		BEGIN
			UPDATE  Cash.tbCode
			SET     IsEnabled = 0
			FROM        inserted INNER JOIN
										Cash.tbCode ON inserted.CategoryCode = Cash.tbCode.CategoryCode
			WHERE        (inserted.IsEnabled = 0) AND (Cash.tbCode.IsEnabled <> 0);
		END

		IF NOT UPDATE(UpdatedBy)
		BEGIN
			UPDATE Cash.tbCategory
			SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
			FROM Cash.tbCategory INNER JOIN inserted AS i ON tbCategory.CategoryCode = i.CategoryCode;
		END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Cash].[Cash_tbPeriod_Trigger_Update]...';


go
CREATE   TRIGGER Cash.Cash_tbPeriod_Trigger_Update 
ON Cash.tbPeriod FOR INSERT, UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY
	IF UPDATE (ForecastValue)
		BEGIN
		UPDATE tbPeriod
		SET ForecastTax = inserted.ForecastValue * tax_code.TaxRate
		FROM inserted 
			JOIN Cash.tbPeriod tbPeriod ON inserted.CashCode = tbPeriod.CashCode AND inserted.StartOn = tbPeriod.StartOn
			JOIN Cash.tbCode cash_code ON tbPeriod.CashCode = cash_code.CashCode 
			JOIN Cash.tbCategory ON cash_code.CategoryCode = Cash.tbCategory.CategoryCode 
            JOIN App.tbTaxCode tax_code ON cash_code.TaxCode = tax_code.TaxCode
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Cash].[Cash_tbMirror_Trigger_Insert]...';


go
CREATE   TRIGGER [Cash].Cash_tbMirror_Trigger_Insert
ON Cash.tbMirror
FOR INSERT
AS
	SET NOCOUNT ON;

	BEGIN TRY

		UPDATE mirror
		SET TransmitStatusCode = Subject.TransmitStatusCode
		FROM Cash.tbMirror mirror 
			JOIN inserted ON mirror.SubjectCode = inserted.SubjectCode AND mirror.CashCode = inserted.CashCode
			JOIN Subject.tbSubject Subject ON inserted.SubjectCode = Subject.SubjectCode;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Cash].[Cash_tbMirror_Trigger_Update]...';


go
CREATE   TRIGGER [Cash].Cash_tbMirror_Trigger_Update
ON Cash.tbMirror
FOR UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY
		IF NOT UPDATE(TransmitStatusCode)
		BEGIN
			UPDATE mirror
			SET 
				TransmitStatusCode = CASE WHEN Subject.TransmitStatusCode = 1 THEN 2 ELSE 0 END,
				UpdatedBy = SUSER_NAME(),
				UpdatedOn = CURRENT_TIMESTAMP
			FROM Cash.tbMirror mirror 
				JOIN inserted ON mirror.SubjectCode = inserted.SubjectCode AND mirror.CashCode = inserted.CashCode
				JOIN Subject.tbSubject Subject ON inserted.SubjectCode = Subject.SubjectCode
			WHERE inserted.TransmitStatusCode <> 1;
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Cash].[Cash_tbChange_TriggerUpdate]...';


go
CREATE   TRIGGER Cash.Cash_tbChange_TriggerUpdate
   ON  Cash.tbChange
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE Cash.tbChange
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Cash.tbChange INNER JOIN inserted AS i ON Cash.tbChange.PaymentAddress = i.PaymentAddress;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Cash].[Cash_tbPayment_TriggerDelete]...';


go
CREATE   TRIGGER Cash.Cash_tbPayment_TriggerDelete
ON Cash.tbPayment
FOR DELETE
AS
	SET NOCOUNT ON;
	BEGIN TRY

		WITH assets AS
		(
			SELECT account.AccountCode FROM deleted d
				JOIN Subject.tbAccount account ON account.AccountCode = d.AccountCode
			WHERE AccountTypeCode > 1
		), balance AS
		(
			SELECT account.AccountCode, SUM(PaidInValue + (PaidOutValue * -1)) CurrentBalance
			FROM Subject.tbAccount account
				JOIN assets ON account.AccountCode = assets.AccountCode
				JOIN Cash.tbPayment payment ON account.AccountCode = payment.AccountCode
			WHERE payment.PaymentStatusCode = 1
			GROUP BY account.AccountCode
		)
		UPDATE account
		SET CurrentBalance = balance.CurrentBalance
		FROM Subject.tbAccount account
			JOIN balance ON account.AccountCode = balance.AccountCode;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Cash].[Cash_tbPayment_TriggerInsert]...';


go
CREATE TRIGGER Cash.Cash_tbPayment_TriggerInsert
ON Cash.tbPayment
FOR INSERT
AS
	SET NOCOUNT ON;
	BEGIN TRY

		UPDATE payment
		SET PaymentStatusCode = 2
		FROM inserted
			JOIN Cash.tbPayment payment ON inserted.PaymentCode = payment.PaymentCode
			JOIN Subject.tbAccount account ON payment.AccountCode = account.AccountCode
			JOIN Cash.tbCode ON inserted.CashCode = Cash.tbCode.CashCode 
			JOIN Cash.tbCategory category ON Cash.tbCode.CategoryCode = category.CategoryCode
		WHERE category.CashTypeCode = 2 AND inserted.PaymentStatusCode = 0 AND account.AccountTypeCode = 0;

		WITH assets AS
		(
			SELECT account.AccountCode FROM inserted i
				JOIN Subject.tbAccount account ON account.AccountCode = i.AccountCode
			WHERE AccountTypeCode = 2 AND PaymentStatusCode = 1
		), balance AS
		(
			SELECT account.AccountCode, SUM(PaidInValue + (PaidOutValue * -1)) CurrentBalance
			FROM Subject.tbAccount account
				JOIN assets ON account.AccountCode = assets.AccountCode
				JOIN Cash.tbPayment payment ON account.AccountCode = payment.AccountCode
			WHERE payment.PaymentStatusCode = 1
			GROUP BY account.AccountCode
		)
		UPDATE account
		SET CurrentBalance = balance.CurrentBalance + OpeningBalance
		FROM Subject.tbAccount account
			JOIN balance ON account.AccountCode = balance.AccountCode;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Cash].[Cash_tbPayment_TriggerUpdate]...';


go
CREATE TRIGGER Cash.Cash_tbPayment_TriggerUpdate
ON Cash.tbPayment
FOR UPDATE
AS
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE Cash.tbPayment
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Cash.tbPayment INNER JOIN inserted AS i ON tbPayment.PaymentCode = i.PaymentCode;

		IF UPDATE(PaidInValue) OR UPDATE(PaidOutValue)
		BEGIN
			IF EXISTS (SELECT * FROM inserted i
					JOIN Subject.tbAccount account ON i.AccountCode = account.AccountCode AND account.AccountTypeCode = 0
				WHERE i.PaymentStatusCode = 1)
			BEGIN
				DECLARE @SubjectCode NVARCHAR(10)
				DECLARE Subject CURSOR LOCAL FOR 
					SELECT i.SubjectCode 
					FROM inserted i
						JOIN Subject.tbAccount account ON i.AccountCode = account.AccountCode AND account.AccountTypeCode = 0
					WHERE i.PaymentStatusCode = 1

				OPEN Subject
				FETCH NEXT FROM Subject INTO @SubjectCode
				WHILE (@@FETCH_STATUS = 0)
					BEGIN		
					EXEC Subject.proc_Rebuild @SubjectCode
					FETCH NEXT FROM Subject INTO @SubjectCode
				END

				CLOSE Subject
				DEALLOCATE Subject
			END
		END

		IF UPDATE(PaymentStatusCode) OR UPDATE(PaidInValue) OR UPDATE(PaidOutValue)
		BEGIN
			WITH assets AS
			(
				SELECT account.AccountCode FROM inserted i
					JOIN Subject.tbAccount account ON account.AccountCode = i.AccountCode
				WHERE AccountTypeCode = 2
			), balance AS
			(
				SELECT account.AccountCode, SUM(PaidInValue + (PaidOutValue * -1)) AS CurrentBalance
				FROM Subject.tbAccount account
					JOIN assets ON account.AccountCode = assets.AccountCode
					JOIN Cash.tbPayment payment ON account.AccountCode = payment.AccountCode
				WHERE payment.PaymentStatusCode = 1
				GROUP BY account.AccountCode
			)
			UPDATE account
			SET CurrentBalance = balance.CurrentBalance + OpeningBalance
			FROM Subject.tbAccount account
				JOIN balance ON account.AccountCode = balance.AccountCode;
		END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [App].[App_tbCalendar_TriggerUpdate]...';


go
CREATE   TRIGGER App.App_tbCalendar_TriggerUpdate 
   ON  App.tbCalendar
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		IF EXISTS (SELECT * FROM inserted i WHERE App.fnParsePrimaryKey(CalendarCode) = 0)
			BEGIN
			DECLARE @Msg NVARCHAR(MAX);
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 2004;
			RAISERROR (@Msg, 10, 1);
			ROLLBACK
			END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [App].[App_tbUom_TriggerUpdate]...';


go
CREATE   TRIGGER App.App_tbUom_TriggerUpdate
   ON  App.tbUom
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		IF EXISTS (SELECT * FROM inserted i WHERE App.fnParsePrimaryKey(UnitOfMeasure) = 0)
			BEGIN
			DECLARE @Msg NVARCHAR(MAX);
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 2004;
			RAISERROR (@Msg, 10, 1)
			ROLLBACK
			END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [App].[App_tbTaxCode_TriggerUpdate]...';


go

CREATE TRIGGER App.App_tbTaxCode_TriggerUpdate ON App.tbTaxCode AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		IF EXISTS (SELECT * FROM inserted i WHERE App.fnParsePrimaryKey(TaxCode) = 0)
		BEGIN
			DECLARE @Msg NVARCHAR(MAX);
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 2004;
			RAISERROR (@Msg, 10, 1);
			ROLLBACK TRANSACTION;
		END
		ELSE IF NOT UPDATE(UpdatedBy)
		BEGIN
			UPDATE App.tbTaxCode
			SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
			FROM App.tbTaxCode INNER JOIN inserted AS i ON tbTaxCode.TaxCode = i.TaxCode;
		END
		
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [App].[App_tbOptions_TriggerUpdate]...';


go
CREATE TRIGGER App.App_tbOptions_TriggerUpdate 
   ON App.tbOptions
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE App.tbOptions
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM App.tbOptions INNER JOIN inserted AS i ON tbOptions.Identifier = i.Identifier;

		IF UPDATE(CoinTypeCode)
		BEGIN
			UPDATE Subject.tbAccount
			SET CoinTypeCode = (SELECT CoinTypeCode FROM inserted)
		END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Object].[Object_tbOp_TriggerUpdate]...';


go
CREATE   TRIGGER Object.Object_tbOp_TriggerUpdate 
   ON  Object.tbOp 
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE Object.tbOp
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Object.tbOp INNER JOIN inserted AS i ON tbOp.ObjectCode = i.ObjectCode AND tbOp.OperationNumber = i.OperationNumber;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Object].[Object_tbFlow_TriggerUpdate]...';


go
CREATE   TRIGGER Object.Object_tbFlow_TriggerUpdate 
   ON  Object.tbFlow
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY		
		UPDATE Object.tbFlow
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Object.tbFlow INNER JOIN inserted AS i ON tbFlow.ParentCode = i.ParentCode AND tbFlow.StepNumber = i.StepNumber;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Object].[Object_tbAttribute_TriggerUpdate]...';


go
CREATE   TRIGGER Object.Object_tbAttribute_TriggerUpdate 
   ON  Object.tbAttribute
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		UPDATE Object.tbAttribute
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Object.tbAttribute INNER JOIN inserted AS i ON tbAttribute.ObjectCode = i.ObjectCode AND tbAttribute.Attribute = i.Attribute;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Object].[Object_tbObject_TriggerUpdate]...';


go

/*  TRIGGERS ****/
CREATE   TRIGGER Object.Object_tbObject_TriggerUpdate
   ON  Object.tbObject
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		IF EXISTS (SELECT * FROM inserted i WHERE App.fnParsePrimaryKey(ObjectCode) = 0)
			BEGIN
			DECLARE @Msg NVARCHAR(MAX);
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 2004;
			RAISERROR (@Msg, 10, 1)
			ROLLBACK
			END
		ELSE
			BEGIN
			UPDATE Object.tbObject
			SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
			FROM Object.tbObject INNER JOIN inserted AS i ON tbObject.ObjectCode = i.ObjectCode;
			END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Object].[Object_tbMirror_Trigger_Insert]...';


go
CREATE   TRIGGER [Object].Object_tbMirror_Trigger_Insert
ON Object.tbMirror
FOR INSERT
AS
	SET NOCOUNT ON;

	BEGIN TRY

		UPDATE mirror
		SET TransmitStatusCode = Subject.TransmitStatusCode
		FROM Object.tbMirror mirror 
			JOIN inserted ON mirror.SubjectCode = inserted.SubjectCode AND mirror.ObjectCode = inserted.ObjectCode
			JOIN Subject.tbSubject Subject ON inserted.SubjectCode = Subject.SubjectCode;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Object].[Object_tbMirror_Trigger_Update]...';


go
CREATE   TRIGGER [Object].Object_tbMirror_Trigger_Update
ON Object.tbMirror
FOR UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY
		IF NOT UPDATE(TransmitStatusCode)
		BEGIN
			UPDATE mirror
			SET 
				TransmitStatusCode = CASE WHEN Subject.TransmitStatusCode = 1 THEN 2 ELSE 0 END,
				UpdatedBy = SUSER_NAME(),
				UpdatedOn = CURRENT_TIMESTAMP
			FROM Object.tbMirror mirror 
				JOIN inserted ON mirror.SubjectCode = inserted.SubjectCode AND mirror.ObjectCode = inserted.ObjectCode
				JOIN Subject.tbSubject Subject ON inserted.SubjectCode = Subject.SubjectCode
			WHERE inserted.TransmitStatusCode <> 1;
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Subject].[Subject_tbAddress_TriggerInsert]...';


go
CREATE   TRIGGER Subject.Subject_tbAddress_TriggerInsert
ON Subject.tbAddress 
FOR INSERT
AS
	SET NOCOUNT ON;
	BEGIN TRY
		If EXISTS(SELECT     Subject.tbSubject.AddressCode, Subject.tbSubject.SubjectCode
				  FROM         Subject.tbSubject INNER JOIN
										inserted AS i ON Subject.tbSubject.SubjectCode = i.SubjectCode
				  WHERE     ( Subject.tbSubject.AddressCode IS NULL))
			BEGIN
			UPDATE Subject.tbSubject
			SET AddressCode = i.AddressCode
			FROM         Subject.tbSubject INNER JOIN
										inserted AS i ON Subject.tbSubject.SubjectCode = i.SubjectCode
				  WHERE     ( Subject.tbSubject.AddressCode IS NULL)
			END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Subject].[Subject_tbAddress_TriggerUpdate]...';


go
CREATE   TRIGGER Subject.Subject_tbAddress_TriggerUpdate 
   ON  Subject.tbAddress
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE Subject.tbAddress
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Subject.tbAddress INNER JOIN inserted AS i ON tbAddress.AddressCode = i.AddressCode;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Subject].[Subject_tbDoc_TriggerUpdate]...';


go
CREATE   TRIGGER Subject.Subject_tbDoc_TriggerUpdate 
   ON  Subject.tbDoc
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		UPDATE Subject.tbDoc
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Subject.tbDoc INNER JOIN inserted AS i ON tbDoc.SubjectCode = i.SubjectCode AND tbDoc.DocumentName = i.DocumentName;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Subject].[Subject_tbContact_TriggerInsert]...';


go
CREATE   TRIGGER Subject.Subject_tbContact_TriggerInsert 
   ON  Subject.tbContact
   AFTER INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
	
		UPDATE Subject.tbContact
		SET 
			NickName = RTRIM(CASE 
				WHEN LEN(ISNULL(i.NickName, '')) > 0 THEN i.NickName
				WHEN CHARINDEX(' ', tbContact.ContactName, 0) = 0 THEN tbContact.ContactName 
				ELSE LEFT(tbContact.ContactName, CHARINDEX(' ', tbContact.ContactName, 0)) END),
			FileAs = Subject.fnContactFileAs(tbContact.ContactName)
		FROM Subject.tbContact INNER JOIN inserted AS i ON tbContact.SubjectCode = i.SubjectCode AND tbContact.ContactName = i.ContactName;

	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
		EXEC App.proc_ErrorLog;
		THROW;
	END CATCH
END
go
PRINT N'Creating Trigger [Subject].[Subject_tbContact_TriggerUpdate]...';


go
CREATE   TRIGGER Subject.Subject_tbContact_TriggerUpdate 
   ON  Subject.tbContact
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	

		IF UPDATE(ContactName)
		BEGIN
			UPDATE Subject.tbContact
			SET 
				FileAs = Subject.fnContactFileAs(tbContact.ContactName)
			FROM Subject.tbContact INNER JOIN inserted AS i ON tbContact.SubjectCode = i.SubjectCode AND tbContact.ContactName = i.ContactName;
		END

		UPDATE Subject.tbContact
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Subject.tbContact INNER JOIN inserted AS i ON tbContact.SubjectCode = i.SubjectCode AND tbContact.ContactName = i.ContactName;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Subject].[Subject_tbSubject_TriggerUpdate]...';


go
CREATE   TRIGGER Subject.Subject_tbSubject_TriggerUpdate 
   ON  Subject.tbSubject
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		IF EXISTS (SELECT * FROM inserted i WHERE App.fnParsePrimaryKey(SubjectCode) = 0)
			BEGIN
			DECLARE @Msg NVARCHAR(MAX);
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 2004;
			RAISERROR (@Msg, 10, 1);
			ROLLBACK TRANSACTION;
			END
		ELSE
			BEGIN
			UPDATE Subject.tbSubject
			SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
			FROM Subject.tbSubject INNER JOIN inserted AS i ON tbSubject.SubjectCode = i.SubjectCode;
			END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Subject].[Subject_tbAccount_TriggerUpdate]...';


go
CREATE TRIGGER Subject.Subject_tbAccount_TriggerUpdate 
   ON  Subject.tbAccount
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
	DECLARE @Msg NVARCHAR(MAX);

		IF EXISTS (SELECT * FROM inserted i WHERE App.fnParsePrimaryKey(AccountCode) = 0)
			BEGIN		
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 2004;
			RAISERROR (@Msg, 10, 1)
			ROLLBACK
			END
		ELSE IF EXISTS (SELECT * FROM inserted i JOIN Cash.tbCode c ON i.CashCode = c.CashCode WHERE AccountTypeCode = 1)
			BEGIN
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 3015;
			RAISERROR (@Msg, 10, 1)
			ROLLBACK
			END
		ELSE
			BEGIN
			IF UPDATE(OpeningBalance)
			BEGIN
			
				WITH i AS
				(
					SELECT * FROM inserted WHERE AccountTypeCode = 0
				)
				UPDATE Subject.tbAccount
				SET CurrentBalance = balance.CurrentBalance
				FROM Subject.tbAccount 
					INNER JOIN i ON tbAccount.AccountCode = i.AccountCode
					INNER JOIN Cash.vwAccountRebuild balance ON balance.AccountCode = i.AccountCode;

				WITH i AS
				(
					SELECT * FROM inserted WHERE AccountTypeCode = 0
				)		
				UPDATE Subject.tbAccount
				SET CurrentBalance = Subject.tbAccount.OpeningBalance
				FROM  Cash.vwAccountRebuild 
					RIGHT OUTER JOIN Subject.tbAccount ON Cash.vwAccountRebuild.AccountCode = Subject.tbAccount.AccountCode
					JOIN i ON i.AccountCode = Subject.tbAccount.AccountCode
				WHERE   (Cash.vwAccountRebuild.AccountCode IS NULL);
			END

			UPDATE Subject.tbAccount
			SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
			FROM Subject.tbAccount INNER JOIN inserted AS i ON tbAccount.AccountCode = i.AccountCode;
			END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Project].[Project_tbFlow_TriggerUpdate]...';


go
CREATE   TRIGGER Project.Project_tbFlow_TriggerUpdate 
   ON  Project.tbFlow
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		UPDATE Project.tbFlow
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Project.tbFlow INNER JOIN inserted AS i ON tbFlow.ParentProjectCode = i.ParentProjectCode AND tbFlow.StepNumber = i.StepNumber;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Project].[Project_tbDoc_TriggerUpdate]...';


go
CREATE   TRIGGER Project.Project_tbDoc_TriggerUpdate 
   ON  Project.tbDoc
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		UPDATE Project.tbDoc
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Project.tbDoc INNER JOIN inserted AS i ON tbDoc.ProjectCode = i.ProjectCode AND tbDoc.DocumentName = i.DocumentName;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Project].[Project_tbAttribute_TriggerUpdate]...';


go
CREATE   TRIGGER Project.Project_tbAttribute_TriggerUpdate 
   ON  Project.tbAttribute
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		UPDATE Project.tbAttribute
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Project.tbAttribute INNER JOIN inserted AS i ON tbAttribute.ProjectCode = i.ProjectCode AND tbAttribute.Attribute = i.Attribute;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Project].[Project_tbQuote_TriggerUpdate]...';


go
CREATE   TRIGGER Project.Project_tbQuote_TriggerUpdate 
   ON  Project.tbQuote
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE Project.tbQuote
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Project.tbQuote INNER JOIN inserted AS i ON tbQuote.ProjectCode = i.ProjectCode AND tbQuote.Quantity = i.Quantity;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Project].[Project_tbOp_TriggerUpdate]...';


go
CREATE   TRIGGER Project.Project_tbOp_TriggerUpdate 
   ON  Project.tbOp 
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		DECLARE @Msg NVARCHAR(MAX);

		UPDATE ops
		SET StartOn = CAST(ops.StartOn AS DATE), EndOn = CAST(ops.EndOn AS DATE)
		FROM Project.tbOp ops JOIN inserted i ON ops.ProjectCode = i.ProjectCode AND ops.OperationNumber = i.OperationNumber
		WHERE (DATEDIFF(SECOND, CAST(i.StartOn AS DATE), i.StartOn) <> 0 
				OR DATEDIFF(SECOND, CAST(i.EndOn AS DATE), i.EndOn) <> 0);
					
		IF EXISTS (	SELECT *
				FROM inserted
					JOIN Project.tbOp ops ON inserted.ProjectCode = ops.ProjectCode AND inserted.OperationNumber = ops.OperationNumber
				WHERE inserted.StartOn > inserted.EndOn)
			BEGIN
			UPDATE ops
			SET EndOn = ops.StartOn
			FROM Project.tbOp ops JOIN inserted i ON ops.ProjectCode = i.ProjectCode AND ops.OperationNumber = i.OperationNumber;
						
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 3016;
			EXEC App.proc_EventLog @EventMessage = @Msg, @EventTypeCode = 1		
			END;

		WITH Projects AS
		(
			SELECT ProjectCode FROM inserted GROUP BY ProjectCode
		), last_calloff AS
		(
			SELECT ops.ProjectCode, MAX(OperationNumber) AS OperationNumber
			FROM Project.tbOp ops JOIN Projects ON ops.ProjectCode = Projects.ProjectCode	
			WHERE SyncTypeCode = 2 
			GROUP BY ops.ProjectCode
		), calloff AS
		(
			SELECT inserted.ProjectCode, inserted.EndOn FROM inserted 
			JOIN last_calloff ON inserted.ProjectCode = last_calloff.ProjectCode AND inserted.OperationNumber = last_calloff.OperationNumber
			WHERE SyncTypeCode = 2
		)
		UPDATE Project
		SET ActionOn = calloff.EndOn
		FROM Project.tbProject Project
		JOIN calloff ON Project.ProjectCode = calloff.ProjectCode
		WHERE calloff.EndOn <> Project.ActionOn AND Project.ProjectStatusCode < 3;

		UPDATE Project.tbOp
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Project.tbOp INNER JOIN inserted AS i ON tbOp.ProjectCode = i.ProjectCode AND tbOp.OperationNumber = i.OperationNumber;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
PRINT N'Creating Trigger [Project].[Project_tbAllocation_Insert]...';


go
CREATE   TRIGGER [Project].Project_tbAllocation_Insert
ON Project.tbAllocation
FOR INSERT
AS
	SET NOCOUNT ON;

	BEGIN TRY
		INSERT INTO Project.tbAllocationEvent (ContractAddress, EventTypeCode, ProjectStatusCode, ActionOn, UnitCharge, TaxRate, QuantityOrdered, QuantityDelivered)
		SELECT ContractAddress, 2 EventTypeCode, ProjectStatusCode, ActionOn, UnitCharge, TaxRate, QuantityOrdered, QuantityDelivered
		FROM inserted
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Project].[Project_tbAllocation_Trigger_Update]...';


go
CREATE   TRIGGER [Project].Project_tbAllocation_Trigger_Update
ON Project.tbAllocation
FOR UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY

		IF UPDATE(ProjectStatusCode)
		BEGIN
			INSERT INTO Project.tbAllocationEvent (ContractAddress, EventTypeCode, ProjectStatusCode, ActionOn, UnitCharge, TaxRate, QuantityOrdered, QuantityDelivered)
			SELECT i.ContractAddress, 6 EventTypeCode, i.ProjectStatusCode, i.ActionOn, i.UnitCharge, i.TaxRate, i.QuantityOrdered, i.QuantityDelivered
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE d.ProjectStatusCode <> i.ProjectStatusCode
		END

		IF UPDATE(UnitCharge) OR UPDATE(TaxRate)
		BEGIN
			INSERT INTO Project.tbAllocationEvent (ContractAddress, EventTypeCode, ProjectStatusCode, ActionOn, UnitCharge, TaxRate, QuantityOrdered, QuantityDelivered)
			SELECT i.ContractAddress, 3 EventTypeCode, i.ProjectStatusCode, i.ActionOn, i.UnitCharge, i.TaxRate, i.QuantityOrdered, i.QuantityDelivered
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE d.UnitCharge <> i.UnitCharge OR d.TaxRate <> i.TaxRate
		END

		IF UPDATE(ActionOn) OR UPDATE(QuantityOrdered)
		BEGIN
			INSERT INTO Project.tbAllocationEvent (ContractAddress, EventTypeCode, ProjectStatusCode, ActionOn, UnitCharge, TaxRate, QuantityOrdered, QuantityDelivered)
			SELECT i.ContractAddress, 4 EventTypeCode, i.ProjectStatusCode, i.ActionOn, i.UnitCharge, i.TaxRate, i.QuantityOrdered, i.QuantityDelivered
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE d.ActionOn <> i.ActionOn OR d.QuantityOrdered <> i.QuantityOrdered
		END

		IF UPDATE(QuantityDelivered)
		BEGIN
			INSERT INTO Project.tbAllocationEvent (ContractAddress, EventTypeCode, ProjectStatusCode, ActionOn, UnitCharge, TaxRate, QuantityOrdered, QuantityDelivered)
			SELECT i.ContractAddress, 5 EventTypeCode, i.ProjectStatusCode, i.ActionOn, i.UnitCharge, i.TaxRate, i.QuantityOrdered, i.QuantityDelivered
			FROM inserted i
				JOIN deleted d ON i.ContractAddress = d.ContractAddress
			WHERE d.QuantityDelivered <> i.QuantityDelivered
		END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Project].[Project_tbProject_TriggerInsert]...';


go
CREATE TRIGGER Project.Project_tbProject_TriggerInsert
ON Project.tbProject
FOR INSERT
AS
	SET NOCOUNT ON;
	BEGIN TRY

	    UPDATE Project
	    SET Project.ActionOn = CAST(Project.ActionOn AS DATE)
	    FROM Project.tbProject Project JOIN inserted i ON Project.ProjectCode = i.ProjectCode
	    WHERE DATEDIFF(SECOND, CAST(i.ActionOn AS DATE), i.ActionOn) <> 0;

	    UPDATE Project
	    SET Project.TotalCharge = i.UnitCharge * i.Quantity
	    FROM Project.tbProject Project JOIN inserted i ON Project.ProjectCode = i.ProjectCode
	    WHERE i.TotalCharge = 0 

	    UPDATE Project
	    SET Project.UnitCharge = i.TotalCharge / i.Quantity
	    FROM Project.tbProject Project JOIN inserted i ON Project.ProjectCode = i.ProjectCode
	    WHERE i.UnitCharge = 0 AND i.Quantity > 0;

	    UPDATE Project
	    SET PaymentOn = App.fnAdjustToCalendar(
            CASE WHEN Subject.PayDaysFromMonthEnd <> 0 THEN 
                    DATEADD(d, -1, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, Subject.PaymentDays + Subject.ExpectedDays, Project.ActionOn), 'yyyyMM'), '01')))												
                ELSE 
                    DATEADD(d, Subject.PaymentDays + Subject.ExpectedDays, Project.ActionOn) END, 0) 
	    FROM Project.tbProject Project
		    JOIN Subject.tbSubject Subject ON Project.SubjectCode = Subject.SubjectCode
		    JOIN inserted i ON Project.ProjectCode = i.ProjectCode
	    WHERE NOT Project.CashCode IS NULL 

	    INSERT INTO Subject.tbContact (SubjectCode, ContactName)
	    SELECT DISTINCT SubjectCode, ContactName 
	    FROM inserted
	    WHERE EXISTS (SELECT ContactName FROM inserted AS i WHERE (NOT (ContactName IS NULL)) AND (ContactName <> N''))
                AND NOT EXISTS(SELECT Subject.tbContact.ContactName FROM inserted AS i INNER JOIN Subject.tbContact ON i.SubjectCode = Subject.tbContact.SubjectCode AND i.ContactName = Subject.tbContact.ContactName)

		INSERT INTO Project.tbChangeLog
								 (ProjectCode, TransmitStatusCode, SubjectCode, ObjectCode, ProjectStatusCode, ActionOn, Quantity, CashCode, TaxCode, UnitCharge)
		SELECT inserted.ProjectCode, Subject.tbSubject.TransmitStatusCode, inserted.SubjectCode, inserted.ObjectCode, inserted.ProjectStatusCode, 
								 inserted.ActionOn, inserted.Quantity, inserted.CashCode, inserted.TaxCode, inserted.UnitCharge
		FROM inserted 
			JOIN Subject.tbSubject ON inserted.SubjectCode = Subject.tbSubject.SubjectCode
			JOIN Cash.tbCode ON inserted.CashCode = Cash.tbCode.CashCode
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
		EXEC App.proc_ErrorLog;
		THROW;
	END CATCH
go
PRINT N'Creating Trigger [Project].[Project_tbProject_TriggerUpdate]...';


go
CREATE TRIGGER Project.Project_tbProject_TriggerUpdate
ON Project.tbProject
FOR UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY

		UPDATE Project
		SET Project.ActionOn = CAST(Project.ActionOn AS DATE)
		FROM Project.tbProject Project JOIN inserted i ON Project.ProjectCode = i.ProjectCode
		WHERE DATEDIFF(SECOND, CAST(i.ActionOn AS DATE), i.ActionOn) <> 0;

		IF UPDATE(ProjectStatusCode)
		BEGIN
			UPDATE ops
			SET OpStatusCode = 2
			FROM inserted JOIN Project.tbOp ops ON inserted.ProjectCode = ops.ProjectCode
			WHERE ProjectStatusCode > 1 AND OpStatusCode < 2;

			WITH first_ops AS
			(
				SELECT ops.ProjectCode, MIN(ops.OperationNumber) AS OperationNumber
				FROM inserted i JOIN Project.tbOp ops ON i.ProjectCode = ops.ProjectCode 
				WHERE i.ProjectStatusCode = 1		
				GROUP BY ops.ProjectCode		
			), next_ops AS
			(
				SELECT ops.ProjectCode, ops.OperationNumber, ops.SyncTypeCode,
					LEAD(ops.OperationNumber) OVER (PARTITION BY ops.ProjectCode ORDER BY ops.OperationNumber) AS NextOpNo
				FROM inserted i JOIN Project.tbOp ops ON i.ProjectCode = ops.ProjectCode 
			), async_ops AS
			(
				SELECT first_ops.ProjectCode, first_ops.OperationNumber, next_ops.NextOpNo
				FROM first_ops JOIN next_ops ON first_ops.ProjectCode = next_ops.ProjectCode AND first_ops.OperationNumber = next_ops.OperationNumber

				UNION ALL

				SELECT next_ops.ProjectCode, next_ops.OperationNumber, next_ops.NextOpNo
				FROM next_ops JOIN async_ops ON next_ops.ProjectCode = async_ops.ProjectCode AND next_ops.OperationNumber = async_ops.NextOpNo
				WHERE next_ops.SyncTypeCode = 1

			)
			UPDATE ops
			SET OpStatusCode = 1
			FROM async_ops JOIN Project.tbOp ops ON async_ops.ProjectCode = ops.ProjectCode
				AND async_ops.OperationNumber = ops.OperationNumber;
			
			WITH cascade_status AS
			(
				SELECT ProjectCode, ProjectStatusCode
				FROM Project.tbProject inserted
				WHERE NOT CashCode IS NULL
			), Project_flow AS
			(
				SELECT cascade_status.ProjectStatusCode ParentStatusCode, child.ParentProjectCode, child.ChildProjectCode, child_Project.ProjectStatusCode
				FROM Project.tbFlow child 
					JOIN cascade_status ON child.ParentProjectCode = cascade_status.ProjectCode
					JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode

				UNION ALL

				SELECT parent.ParentStatusCode, child.ParentProjectCode, child.ChildProjectCode, child_Project.ProjectStatusCode
				FROM Project.tbFlow child 
					JOIN Project_flow parent ON child.ParentProjectCode = parent.ChildProjectCode
					JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode
			)
			UPDATE Project
			SET ProjectStatusCode = CASE Project_flow.ParentStatusCode WHEN 3 THEN 2 ELSE Project_flow.ParentStatusCode END
			FROM Project.tbProject Project JOIN Project_flow ON Project_flow.ChildProjectCode = Project.ProjectCode
			WHERE Project.ProjectStatusCode < 2;

			--not triggering fix
			WITH cascade_status AS
			(
				SELECT ProjectCode, ProjectStatusCode
				FROM Project.tbProject inserted
				WHERE NOT CashCode IS NULL AND ProjectStatusCode > 1
			), Project_flow AS
			(
				SELECT cascade_status.ProjectStatusCode ParentStatusCode, child.ParentProjectCode, child.ChildProjectCode, child_Project.ProjectStatusCode
				FROM Project.tbFlow child 
					JOIN cascade_status ON child.ParentProjectCode = cascade_status.ProjectCode
					JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode
				WHERE CashCode IS NULL

				UNION ALL

				SELECT parent.ParentStatusCode, child.ParentProjectCode, child.ChildProjectCode, child_Project.ProjectStatusCode
				FROM Project.tbFlow child 
					JOIN Project_flow parent ON child.ParentProjectCode = parent.ChildProjectCode
					JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode
				WHERE CashCode IS NULL
			)
			UPDATE ops
			SET OpStatusCode = 2
			FROM Project.tbOp ops JOIN Project_flow ON Project_flow.ChildProjectCode = ops.ProjectCode
			WHERE ops.OpStatusCode < 2;

			DELETE cost_set 
			FROM inserted 
				JOIN Project.tbCostSet cost_set ON inserted.ProjectCode = cost_set.ProjectCode
			WHERE inserted.ProjectStatusCode > 0;
			
		END

		IF UPDATE(Quantity)
		BEGIN
			UPDATE flow
			SET UsedOnQuantity = inserted.Quantity / parent_Project.Quantity
			FROM Project.tbFlow AS flow 
				JOIN inserted ON flow.ChildProjectCode = inserted.ProjectCode 
				JOIN Project.tbProject AS parent_Project ON flow.ParentProjectCode = parent_Project.ProjectCode
				JOIN Cash.tbCode ON parent_Project.CashCode = Cash.tbCode.CashCode
			WHERE (flow.UsedOnQuantity <> 0) AND (inserted.Quantity <> 0) 
				AND (inserted.Quantity / parent_Project.Quantity <> flow.UsedOnQuantity)
		END

		IF UPDATE(Quantity) OR UPDATE(UnitCharge)
		BEGIN
			UPDATE Project
			SET Project.TotalCharge = i.Quantity * i.UnitCharge
			FROM Project.tbProject Project JOIN inserted i ON Project.ProjectCode = i.ProjectCode
		END

		IF UPDATE(TotalCharge)
		BEGIN
			UPDATE Project
			SET Project.UnitCharge = CASE i.TotalCharge + i.Quantity WHEN 0 THEN 0 ELSE i.TotalCharge / i.Quantity END
			FROM Project.tbProject Project JOIN inserted i ON Project.ProjectCode = i.ProjectCode			
		END

		IF UPDATE(ActionOn)
		BEGIN			
			WITH parent_Project AS
			(
				SELECT        ParentProjectCode
				FROM            Project.tbFlow flow
					JOIN Project.tbProject Project ON flow.ParentProjectCode = Project.ProjectCode
					JOIN Cash.tbCode cash ON Project.CashCode = cash.CashCode
					JOIN inserted ON flow.ChildProjectCode = inserted.ProjectCode
				--manual scheduling only
				WHERE (SELECT SUM(UsedOnQuantity) FROM inserted JOIN Project.tbFlow ON inserted.ProjectCode = Project.tbFlow.ChildProjectCode) = 0	
			), Project_flow AS
			(
				SELECT        flow.ParentProjectCode, flow.StepNumber, Project.ActionOn,
						LAG(Project.ActionOn, 1, Project.ActionOn) OVER (PARTITION BY flow.ParentProjectCode ORDER BY StepNumber) AS PrevActionOn
				FROM Project.tbFlow flow
					JOIN Project.tbProject Project ON flow.ChildProjectCode = Project.ProjectCode
					JOIN parent_Project ON flow.ParentProjectCode = parent_Project.ParentProjectCode
			), step_disordered AS
			(
				SELECT ParentProjectCode 
				FROM Project_flow
				WHERE ActionOn < PrevActionOn
				GROUP BY ParentProjectCode
			), step_ordered AS
			(
				SELECT flow.ParentProjectCode, flow.ChildProjectCode,
					ROW_NUMBER() OVER (PARTITION BY flow.ParentProjectCode ORDER BY Project.ActionOn, flow.StepNumber) * 10 AS StepNumber 
				FROM step_disordered
					JOIN Project.tbFlow flow ON step_disordered.ParentProjectCode = flow.ParentProjectCode
					JOIN Project.tbProject Project ON flow.ChildProjectCode = Project.ProjectCode
			)
			UPDATE flow
			SET
				StepNumber = step_ordered.StepNumber
			FROM Project.tbFlow flow
				JOIN step_ordered ON flow.ParentProjectCode = step_ordered.ParentProjectCode AND flow.ChildProjectCode = step_ordered.ChildProjectCode;
			
			IF EXISTS(SELECT * FROM App.tbOptions WHERE IsAutoOffsetDays <> 0)
			BEGIN
				UPDATE flow
				SET OffsetDays = App.fnOffsetDays(inserted.ActionOn, parent_Project.ActionOn)
									- ISNULL((SELECT SUM(OffsetDays) FROM Project.tbFlow sub_flow WHERE sub_flow.ParentProjectCode = flow.ParentProjectCode AND sub_flow.StepNumber > flow.StepNumber), 0)
				FROM Project.tbFlow AS flow 
					JOIN inserted ON flow.ChildProjectCode = inserted.ProjectCode 
					JOIN Project.tbProject AS parent_Project ON flow.ParentProjectCode = parent_Project.ProjectCode
					JOIN Cash.tbCode ON parent_Project.CashCode = Cash.tbCode.CashCode
				WHERE (SELECT SUM(UsedOnQuantity) FROM inserted JOIN Project.tbFlow ON inserted.ProjectCode = Project.tbFlow.ChildProjectCode) = 0
			END

			UPDATE Project
			SET PaymentOn = App.fnAdjustToCalendar(CASE WHEN Subject.PayDaysFromMonthEnd <> 0 
													THEN 
														DATEADD(d, -1, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, Subject.PaymentDays + Subject.ExpectedDays, i.ActionOn), 'yyyyMM'), '01')))												
													ELSE
														DATEADD(d, Subject.PaymentDays + Subject.ExpectedDays, i.ActionOn)	
													END, 0) 
			FROM Project.tbProject Project
				JOIN inserted i ON Project.ProjectCode = i.ProjectCode
				JOIN Subject.tbSubject Subject ON i.SubjectCode = Subject.SubjectCode				
			WHERE NOT Project.CashCode IS NULL 
		END

		IF UPDATE (ProjectTitle)
		BEGIN
			WITH cascade_title_change AS
			(
				SELECT inserted.ProjectCode, inserted.ProjectTitle AS NewTitle, deleted.ProjectTitle AS PreviousTitle 				
				FROM inserted
					JOIN deleted ON inserted.ProjectCode = deleted.ProjectCode
			), Project_flow AS
			(
				SELECT cascade_title_change.NewTitle, cascade_title_change.PreviousTitle, child.ParentProjectCode, child.ChildProjectCode, child_Project.ProjectTitle
				FROM Project.tbFlow child 
					JOIN cascade_title_change ON child.ParentProjectCode = cascade_title_change.ProjectCode
					JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode

				UNION ALL

				SELECT parent.NewTitle, parent.PreviousTitle, child.ParentProjectCode, child.ChildProjectCode, child_Project.ProjectTitle
				FROM Project.tbFlow child 
					JOIN Project_flow parent ON child.ParentProjectCode = parent.ChildProjectCode
					JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode
			)
			UPDATE Project
			SET ProjectTitle = NewTitle
			FROM Project.tbProject Project JOIN Project_flow ON Project.ProjectCode = Project_flow.ChildProjectCode
			WHERE Project_flow.PreviousTitle = Project_flow.ProjectTitle;
		END

		IF UPDATE (Spooled)
		BEGIN
			INSERT INTO App.tbDocSpool (DocTypeCode, DocumentNumber)
			SELECT CASE 
					WHEN CashPolarityCode = 0 THEN		--Expense
						CASE WHEN ProjectStatusCode = 0 THEN 2	ELSE 3 END	--Enquiry								
					WHEN CashPolarityCode = 1 THEN		--Income
						CASE WHEN ProjectStatusCode = 0 THEN 0	ELSE 1 END	--Quote
					END AS DocTypeCode, Project.ProjectCode
			FROM   inserted Project INNER JOIN
									 Cash.tbCode ON Project.CashCode = Cash.tbCode.CashCode INNER JOIN
									 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE (Project.Spooled <> 0)
				
			DELETE App.tbDocSpool
			FROM         inserted i INNER JOIN
								  App.tbDocSpool ON i.ProjectCode = App.tbDocSpool.DocumentNumber
			WHERE    (i.Spooled = 0) AND ( App.tbDocSpool.DocTypeCode <= 3)
		END

		IF UPDATE (ContactName)
		BEGIN
			INSERT INTO Subject.tbContact (SubjectCode, ContactName)
			SELECT DISTINCT SubjectCode, ContactName FROM inserted
			WHERE EXISTS (SELECT     *
						FROM         inserted AS i
						WHERE     (NOT (ContactName IS NULL)) AND
												(ContactName <> N''))
				AND NOT EXISTS(SELECT  *
								FROM inserted AS i 
								INNER JOIN Subject.tbContact ON i.SubjectCode = Subject.tbContact.SubjectCode AND i.ContactName = Subject.tbContact.ContactName)
		END
		
		UPDATE Project.tbProject
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Project.tbProject INNER JOIN inserted AS i ON tbProject.ProjectCode = i.ProjectCode;

		IF UPDATE(ProjectStatusCode) OR UPDATE(Quantity) OR UPDATE(ActionOn) OR UPDATE(UnitCharge) OR UPDATE(ObjectCode) OR UPDATE(CashCode) OR UPDATE (TaxCode)
		BEGIN
			WITH candidates AS
			(
				SELECT ProjectCode, SubjectCode, ObjectCode, ProjectStatusCode, ActionOn, Quantity, CashCode, TaxCode, UnitCharge
				FROM inserted
				WHERE EXISTS (SELECT * FROM Project.tbChangeLog WHERE ProjectCode = inserted.ProjectCode)
			)
			, logs AS
			(
				SELECT clog.LogId, clog.ProjectCode, clog.SubjectCode, clog.ObjectCode, clog.ProjectStatusCode, clog.TransmitStatusCode, clog.ActionOn, clog.Quantity, clog.CashCode, clog.TaxCode, clog.UnitCharge
				FROM Project.tbChangeLog clog
				JOIN candidates ON clog.ProjectCode = candidates.ProjectCode AND LogId = (SELECT MAX(LogId) FROM Project.tbChangeLog WHERE ProjectCode = candidates.ProjectCode)		
			)
			INSERT INTO Project.tbChangeLog
									(ProjectCode, TransmitStatusCode, SubjectCode, ObjectCode, ProjectStatusCode, ActionOn, Quantity, CashCode, TaxCode, UnitCharge)
			SELECT candidates.ProjectCode, CASE Subjects.TransmitStatusCode WHEN 1 THEN 2 ELSE 0 END TransmitStatusCode, candidates.SubjectCode,
				candidates.ObjectCode, candidates.ProjectStatusCode, candidates.ActionOn, candidates.Quantity, candidates.CashCode, candidates.TaxCode, candidates.UnitCharge
			FROM candidates 
				JOIN Subject.tbSubject Subjects ON candidates.SubjectCode = Subjects.SubjectCode 
				JOIN logs ON candidates.ProjectCode = logs.ProjectCode
			WHERE (logs.ProjectStatusCode <> candidates.ProjectStatusCode) 
				OR (logs.TransmitStatusCode < 2)
				OR (logs.ActionOn <> candidates.ActionOn) 
				OR (logs.Quantity <> candidates.Quantity)
				OR (logs.UnitCharge <> candidates.UnitCharge)
				OR (logs.TaxCode <> candidates.TaxCode);
		END;

		IF UPDATE(SubjectCode)
		BEGIN
			WITH candidates AS
			(
				SELECT inserted.* FROM inserted
				JOIN deleted ON inserted.ProjectCode = deleted.ProjectCode
				WHERE inserted.SubjectCode <> deleted.SubjectCode
			)
			INSERT INTO Project.tbChangeLog
									 (ProjectCode, TransmitStatusCode, SubjectCode, ObjectCode, ProjectStatusCode, ActionOn, Quantity, CashCode, TaxCode, UnitCharge)
			SELECT candidates.ProjectCode, Subject.tbSubject.TransmitStatusCode, candidates.SubjectCode, candidates.ObjectCode, candidates.ProjectStatusCode, 
									 candidates.ActionOn, candidates.Quantity, candidates.CashCode, candidates.TaxCode, candidates.UnitCharge
			FROM candidates 
				JOIN Subject.tbSubject ON candidates.SubjectCode = Subject.tbSubject.SubjectCode
				JOIN Cash.tbCode ON candidates.CashCode = Cash.tbCode.CashCode;
		END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Trigger [Project].[Project_tbProject_TriggerDelete]...';


go
CREATE   TRIGGER Project.Project_tbProject_TriggerDelete
ON Project.tbProject
FOR DELETE
AS
	SET NOCOUNT ON;

	BEGIN TRY
		INSERT INTO Project.tbChangeLog
								 (ProjectCode, TransmitStatusCode, SubjectCode, ObjectCode, ProjectStatusCode, ActionOn, Quantity, CashCode, TaxCode, UnitCharge)
		SELECT deleted.ProjectCode, CASE Subject.tbSubject.TransmitStatusCode WHEN 1 THEN 2 ELSE 0 END TransmitStatusCode, 
					deleted.SubjectCode, deleted.ObjectCode, 4 CancelledStatusCode, 
					deleted.ActionOn, deleted.Quantity, deleted.CashCode, deleted.TaxCode, deleted.UnitCharge
		FROM deleted INNER JOIN Subject.tbSubject ON deleted.SubjectCode = Subject.tbSubject.SubjectCode;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [dbo].[AspNetGetId]...';


go
CREATE   PROCEDURE dbo.AspNetGetId(@UserId nvarchar(10), @Id nvarchar(450) OUTPUT)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
	
		SELECT @Id = UserId 
		FROM AspNetUsers asp JOIN Usr.tbUser u ON asp.UserName = u.EmailAddress
		WHERE u.UserId = @UserId;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [dbo].[AspNetGetUserName]...';


go
CREATE   PROCEDURE dbo.AspNetGetUserName(@Id nvarchar(450), @UserName nvarchar(50) OUTPUT)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
	
		SELECT @UserName = u.UserName
		FROM AspNetUsers asp JOIN Usr.tbUser u ON asp.UserName = u.EmailAddress
		WHERE asp.Id = @Id;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [dbo].[AspNetGetUserId]...';


go
CREATE   PROCEDURE dbo.AspNetGetUserId(@Id nvarchar(450), @UserId nvarchar(10) OUTPUT)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
	
		WITH asp AS
		(
			SELECT Id, UserName
			FROM AspNetUsers 
			WHERE Id = @Id
		)
		SELECT @UserId = UserId 
		FROM asp JOIN Usr.tbUser u ON asp.UserName = u.EmailAddress;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Web].[proc_ImageTag]...';


go
CREATE   PROCEDURE Web.proc_ImageTag(@ImageTag nvarchar(50), @NewImageTag nvarchar(50))
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		UPDATE Web.tbImage
		SET ImageTag = @NewImageTag
		WHERE ImageTag = @ImageTag
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Usr].[proc_MenuItemDelete]...';


go

CREATE   PROCEDURE Usr.proc_MenuItemDelete( @EntryId int )
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		BEGIN TRANSACTION

		DECLARE @MenuId SMALLINT = (SELECT MenuId FROM Usr.tbMenuEntry menu WHERE menu.EntryId = @EntryId);

		DELETE FROM Usr.tbMenuEntry
		WHERE Command = 1 
			AND MenuId = @MenuId
			AND Argument = (SELECT FolderId FROM Usr.tbMenuEntry menu WHERE Command = 0 AND menu.EntryId = @EntryId);

		 WITH root_folder AS
		 (
			 SELECT FolderId, MenuId 
			 FROM Usr.tbMenuEntry menu
			 WHERE Command = 0 AND menu.EntryId = @EntryId
		), child_folders AS
		(
			SELECT CAST(Argument AS smallint) AS FolderId, root_folder.MenuId
			FROM Usr.tbMenuEntry sub_folder 
			JOIN root_folder ON sub_folder.FolderId = root_folder.FolderId
			WHERE Command = 1 AND sub_folder.MenuId = @MenuId

			UNION ALL

			SELECT CAST(Argument AS smallint) AS FolderId, p.MenuId
			FROM child_folders p 
				JOIN Usr.tbMenuEntry m ON p.FolderId = m.FolderId
			WHERE Command = 1 AND m.MenuId = p.MenuId
		), folders AS
		(
			select FolderId from root_folder
			UNION
			select FolderId from child_folders
		)
		DELETE Usr.tbMenuEntry 
		FROM Usr.tbMenuEntry JOIN folders ON Usr.tbMenuEntry.FolderId = folders.FolderId

		DELETE FROM Usr.tbMenuEntry WHERE EntryId = @EntryId;

		COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Usr].[proc_MenuInsert]...';


go

CREATE   PROCEDURE Usr.proc_MenuInsert
	(
		@MenuName nvarchar(50),
		@FromMenuId smallint = 0,
		@MenuId smallint = null OUTPUT
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		BEGIN TRANSACTION
	
		INSERT INTO Usr.tbMenu (MenuName) VALUES (@MenuName)
		SELECT @MenuId = @@IDENTITY
	
		IF @FromMenuId = 0
			BEGIN
			INSERT INTO Usr.tbMenuEntry (MenuId, FolderId, ItemId, ItemText, Command,  Argument)
					VALUES (@MenuId, 1, 0, @MenuName, 0, 'Root')
			END
		ELSE
			BEGIN
			INSERT INTO Usr.tbMenuEntry
								  (MenuId, FolderId, ItemId, OpenMode, Argument, ProjectName, Command, ItemText)
			SELECT     @MenuId AS ToMenuId, FolderId, ItemId, OpenMode, Argument, ProjectName, Command, ItemText
			FROM         Usr.tbMenuEntry
			WHERE     (MenuId = @FromMenuId)
			END

		COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Usr].[proc_MenuCleanReferences]...';


go

CREATE   PROCEDURE Usr.proc_MenuCleanReferences(@MenuId SMALLINT)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		WITH tbFolderRefs AS 
		(	SELECT        MenuId, EntryId, CAST(Argument AS int) AS FolderIdRef
			FROM            Usr.tbMenuEntry
			WHERE        (Command = 1))
		, tbBadRefs AS
		(
			SELECT        tbFolderRefs.EntryId
			FROM            tbFolderRefs LEFT OUTER JOIN
									Usr.tbMenuEntry AS tbMenuEntry ON tbFolderRefs.FolderIdRef = tbMenuEntry.FolderId AND tbFolderRefs.MenuId = tbMenuEntry.MenuId
			WHERE (tbMenuEntry.MenuId = @MenuId) AND (tbMenuEntry.MenuId IS NULL)
		)
		DELETE FROM Usr.tbMenuEntry
		FROM            Usr.tbMenuEntry INNER JOIN
								 tbBadRefs ON Usr.tbMenuEntry.EntryId = tbBadRefs.EntryId;

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Invoice].[proc_NetworkUpdated]...';


go
CREATE   PROCEDURE Invoice.proc_NetworkUpdated (@InvoiceNumber nvarchar(20))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		UPDATE Invoice.tbChangeLog
		SET TransmitStatusCode = 3
		WHERE InvoiceNumber = @InvoiceNumber AND TransmitStatusCode < 3;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Invoice].[proc_ChangeLogCleardown]...';


go
CREATE   PROCEDURE Invoice.proc_ChangeLogCleardown (@RetentionDays SMALLINT = 30)
AS
	SET NOCOUNT, XACT_ABORT OFF;

	BEGIN TRY					
		DECLARE 
			@EventMessage nvarchar(max) = (SELECT [Message] FROM App.tbText WHERE TextId = 1223)
			, @EventTypeCode smallint = 2
			, @LogCode nvarchar(20)

		DELETE FROM Invoice.tbChangeLog
		WHERE ChangedOn < DATEADD(DAY, @RetentionDays * -1, CAST(CURRENT_TIMESTAMP AS DATE)) 

		EXECUTE App.proc_EventLog @EventMessage, @EventTypeCode, @LogCode OUTPUT

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Invoice].[proc_CancelById]...';


go
CREATE   PROCEDURE Invoice.proc_CancelById(@UserId nvarchar(10))
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		BEGIN TRAN

		UPDATE       Project
		SET                ProjectStatusCode = 2
		FROM            Project.tbProject AS Project INNER JOIN
								 Invoice.tbProject AS InvoiceProject ON Project.ProjectCode = InvoiceProject.ProjectCode AND Project.ProjectCode = InvoiceProject.ProjectCode INNER JOIN
								 Invoice.tbInvoice ON InvoiceProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber 
		WHERE        (Invoice.tbInvoice.InvoiceTypeCode = 0 OR Invoice.tbInvoice.InvoiceTypeCode = 2) 
			AND (Invoice.tbInvoice.InvoiceStatusCode = 0) AND (Project.ProjectStatusCode = 3) AND (Invoice.tbInvoice.UserId = @UserId)
	                      
		DELETE Invoice.tbInvoice
		FROM         Invoice.tbInvoice INNER JOIN
							  Usr.vwCredentials ON Invoice.tbInvoice.UserId = Usr.vwCredentials.UserId
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode = 0) AND (Invoice.tbInvoice.UserId = @UserId)
		
		COMMIT TRAN

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Invoice].[proc_DefaultDocType]...';


go

CREATE   PROCEDURE Invoice.proc_DefaultDocType
	(
		@InvoiceNumber nvarchar(20),
		@DocTypeCode smallint OUTPUT
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @InvoiceTypeCode smallint

			SELECT  @InvoiceTypeCode = InvoiceTypeCode
			FROM         Invoice.tbInvoice
			WHERE     (InvoiceNumber = @InvoiceNumber)
	
			SET @DocTypeCode = CASE @InvoiceTypeCode
									WHEN 0 THEN 4
									WHEN 1 THEN 5							
									WHEN 3 THEN 6
									ELSE 4
									END
							
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Invoice].[proc_Cancel]...';


go
CREATE PROCEDURE Invoice.proc_Cancel
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		
		DECLARE @UserId nvarchar(10) = (SELECT TOP 1 UserId FROM Usr.vwCredentials)
		EXEC Invoice.proc_CancelById @UserId

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Invoice].[proc_Total]...';


go
CREATE PROCEDURE Invoice.proc_Total 
	(
	@InvoiceNumber nvarchar(20)
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		WITH totals AS
		(
			SELECT InvoiceNumber, SUM(InvoiceValue) AS InvoiceValue, 
				SUM(TaxValue) AS TaxValue
			FROM         Invoice.tbProject
			GROUP BY InvoiceNumber
			HAVING      (InvoiceNumber = @InvoiceNumber)
			UNION
			SELECT InvoiceNumber, SUM(InvoiceValue) AS InvoiceValue, 
				SUM(TaxValue) AS TaxValue
			FROM         Invoice.tbItem
			GROUP BY InvoiceNumber
			HAVING      (InvoiceNumber = @InvoiceNumber)
		), grand_total AS
		(
			SELECT InvoiceNumber, ISNULL(SUM(InvoiceValue), 0) AS InvoiceValue, 
				ISNULL(SUM(TaxValue), 0) AS TaxValue
			FROM totals
			GROUP BY InvoiceNumber
		) 
		UPDATE    Invoice.tbInvoice
		SET InvoiceValue = grand_total.InvoiceValue, TaxValue = grand_total.TaxValue
		FROM Invoice.tbInvoice INNER JOIN grand_total ON Invoice.tbInvoice.InvoiceNumber = grand_total.InvoiceNumber;
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Invoice].[proc_Credit]...';


go

CREATE   PROCEDURE Invoice.proc_Credit
	(
		@InvoiceNumber nvarchar(20) output
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	DECLARE 
		@InvoiceTypeCode smallint
		, @CreditNumber nvarchar(20)
		, @UserId nvarchar(10)
		, @NextNumber int
		, @InvoiceSuffix nvarchar(4)

		SELECT @UserId = UserId FROM Usr.vwCredentials
	
		SELECT @InvoiceTypeCode =	CASE InvoiceTypeCode 
										WHEN 0 THEN 1 
										WHEN 2 THEN 3 
										ELSE 3 
									END 
		FROM Invoice.tbInvoice WHERE InvoiceNumber = @InvoiceNumber
	
	
		SELECT @UserId = UserId FROM Usr.vwCredentials

		SET @InvoiceSuffix = '.' + @UserId
	
		SELECT @NextNumber = NextNumber
		FROM Invoice.tbType
		WHERE InvoiceTypeCode = @InvoiceTypeCode
	
		SELECT @CreditNumber = FORMAT(@NextNumber, '000000') + @InvoiceSuffix
	
		WHILE EXISTS (SELECT     InvoiceNumber
					  FROM         Invoice.tbInvoice
					  WHERE     (InvoiceNumber = @CreditNumber))
			BEGIN
			SET @NextNumber = @NextNumber + 1
			SET @CreditNumber = FORMAT(@NextNumber, '000000') + @InvoiceSuffix
			END

		BEGIN TRANSACTION

		EXEC Invoice.proc_Cancel
	
		UPDATE    Invoice.tbType
		SET              NextNumber = @NextNumber + 1
		WHERE     (InvoiceTypeCode = @InvoiceTypeCode)	
	
		INSERT INTO Invoice.tbInvoice	
							(InvoiceNumber, InvoiceStatusCode, SubjectCode, InvoiceValue, TaxValue, UserId, InvoiceTypeCode, InvoicedOn)
		SELECT     @CreditNumber AS InvoiceNumber, 0 AS InvoiceStatusCode, SubjectCode, InvoiceValue, TaxValue, @UserId AS UserId, 
							@InvoiceTypeCode AS InvoiceTypeCode, CURRENT_TIMESTAMP AS InvoicedOn
		FROM         Invoice.tbInvoice
		WHERE     (InvoiceNumber = @InvoiceNumber)
	
		INSERT INTO Invoice.tbItem
							  (InvoiceNumber, CashCode, TaxCode, InvoiceValue, TaxValue)
		SELECT     @CreditNumber AS InvoiceNumber, CashCode, TaxCode, InvoiceValue, TaxValue
		FROM         Invoice.tbItem
		WHERE     (InvoiceNumber = @InvoiceNumber)
	
		INSERT INTO Invoice.tbProject
							  (InvoiceNumber, ProjectCode, Quantity, InvoiceValue, TaxValue, CashCode, TaxCode)
		SELECT     @CreditNumber AS InvoiceNumber, ProjectCode, Quantity, InvoiceValue, TaxValue, CashCode, TaxCode
		FROM         Invoice.tbProject
		WHERE     (InvoiceNumber = @InvoiceNumber)

		SET @InvoiceNumber = @CreditNumber
	
		COMMIT TRANSACTION
	 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Invoice].[proc_DefaultPaymentOn]...';


go
CREATE   PROCEDURE Invoice.proc_DefaultPaymentOn
	(
		@SubjectCode nvarchar(10),
		@ActionOn datetime,
		@PaymentOn datetime output
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		SELECT @ActionOn = CASE WHEN Subject.PayDaysFromMonthEnd <> 0 
				THEN 
					DATEADD(d, -1, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, Subject.PaymentDays, @ActionOn), 'yyyyMM'), '01')))												
				ELSE
					DATEADD(d, Subject.PaymentDays, @ActionOn)	
				END
		FROM Subject.tbSubject Subject 
		WHERE Subject.SubjectCode = @SubjectCode

		SELECT @PaymentOn = App.fnAdjustToCalendar(@ActionOn, 0) 					
	 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Invoice].[proc_Mirror]...';


go
CREATE   PROCEDURE Invoice.proc_Mirror(@ContractAddress nvarchar(42), @InvoiceNumber nvarchar(20) OUTPUT)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	DECLARE 
		@UserId nvarchar(10)
		, @NextNumber int
		, @InvoiceSuffix nvarchar(4)
		, @SubjectCode nvarchar(10)
		, @InvoiceTypeCode smallint
	
		SELECT @UserId = UserId FROM Usr.vwCredentials
		SET @InvoiceSuffix = '.' + @UserId

		SELECT 
			@InvoiceTypeCode = CASE InvoiceTypeCode 
								WHEN 0 THEN 2
								WHEN 1 THEN 3
								WHEN 2 THEN 0
								WHEN 3 THEN 1
							END
		FROM Invoice.tbMirror
		WHERE ContractAddress = @ContractAddress
	
		SELECT @NextNumber = NextNumber
		FROM Invoice.tbType
		WHERE InvoiceTypeCode = @InvoiceTypeCode
	
		SELECT @InvoiceNumber = FORMAT(@NextNumber, '000000') + @InvoiceSuffix
	
		WHILE EXISTS (SELECT     InvoiceNumber
						FROM         Invoice.tbInvoice
						WHERE     (InvoiceNumber = @InvoiceNumber))
			BEGIN
			SET @NextNumber = @NextNumber + 1
			SET @InvoiceNumber = FORMAT(@NextNumber, '000000') + @InvoiceSuffix
			END

		BEGIN TRAN

		UPDATE    Invoice.tbType
		SET              NextNumber = @NextNumber + 1
		WHERE     (InvoiceTypeCode = @InvoiceTypeCode)
	
		INSERT INTO Invoice.tbInvoice
							(InvoiceNumber, UserId, SubjectCode, InvoiceTypeCode, InvoicedOn, DueOn, ExpectedOn, InvoiceStatusCode, PaymentTerms)
		SELECT     @InvoiceNumber AS InvoiceNumber, @UserId AS UserId, mirror.SubjectCode, 
				@InvoiceTypeCode AS InvoiceTypeCode, CAST(mirror.InvoicedOn AS DATE) AS InvoicedOn, mirror.DueOn, mirror.DueOn ExpectedOn, 0 AS InvoiceStatusCode, 
				CASE WHEN Subject.tbSubject.PaymentTerms IS NULL THEN mirror.PaymentTerms ELSE Subject.tbSubject.PaymentTerms END PaymentTerms
		FROM Invoice.tbMirror mirror
			JOIN Subject.tbSubject ON mirror.SubjectCode = Subject.tbSubject.SubjectCode
		WHERE ContractAddress = @ContractAddress;

		INSERT INTO Invoice.tbMirrorReference (ContractAddress, InvoiceNumber)
		VALUES (@ContractAddress, @InvoiceNumber);

		WITH allocations AS
		(
			SELECT 0 Allocationid, 
				allocation.ProjectCode,
				Object_mirror.ObjectCode, allocation.SubjectCode, 
					CASE allocation.CashPolarityCode 
						WHEN 0 THEN Project_mirror.Quantity * -1
						WHEN 1 THEN Project_mirror.Quantity
					END Quantity, allocation.CashPolarityCode
			FROM Invoice.tbMirror invoice_mirror
				JOIN Invoice.tbMirrorProject Project_mirror ON invoice_mirror.ContractAddress = Project_mirror.ContractAddress
				JOIN Project.tbAllocation allocation ON invoice_mirror.SubjectCode = allocation.SubjectCode AND Project_mirror.ProjectCode = allocation.ProjectCode			
				JOIN Object.tbMirror Object_mirror ON invoice_mirror.SubjectCode = Object_mirror.SubjectCode AND allocation.AllocationCode = Object_mirror.AllocationCode
			WHERE invoice_mirror.ContractAddress = @ContractAddress
		), Projects AS
		(
			SELECT ROW_NUMBER() OVER (PARTITION BY Projects.SubjectCode, Projects.ObjectCode ORDER BY ActionOn) Allocationid,
				Projects.ProjectCode, Projects.ObjectCode, Projects.SubjectCode, Projects.Quantity, category.CashPolarityCode
			FROM allocations
				JOIN Project.tbProject Projects ON Projects.ObjectCode = allocations.ObjectCode AND Projects.SubjectCode = allocations.SubjectCode
				JOIN Cash.tbCode cash_code ON Projects.CashCode = cash_code.CashCode
				JOIN Cash.tbCategory category ON cash_code.CategoryCode = category.CategoryCode
			WHERE Projects.ProjectStatusCode BETWEEN 1 AND 2
		), order_book AS
		(
			SELECT Projects.ProjectCode, SUM(COALESCE(invoice_quantity.Quantity, 0)) InvoiceQuantity
			FROM Projects
				OUTER APPLY 
					(
						SELECT CASE invoice.InvoiceTypeCode 
									WHEN 1 THEN delivery.Quantity * -1 
									WHEN 3 THEN delivery.Quantity * -1 
									ELSE delivery.Quantity 
								END Quantity
						FROM Invoice.tbProject delivery 
							JOIN Invoice.tbInvoice invoice ON delivery.InvoiceNumber = invoice.InvoiceNumber
						WHERE delivery.ProjectCode = Projects.ProjectCode
					) invoice_quantity
			GROUP BY Projects.ProjectCode
		), deliveries AS
		(
			SELECT Allocationid, Projects.ProjectCode, ObjectCode, SubjectCode,
						CASE CashPolarityCode 
							WHEN 0 THEN (Projects.Quantity - order_book.InvoiceQuantity) * -1
							WHEN 1 THEN Projects.Quantity - order_book.InvoiceQuantity
						END Quantity, CashPolarityCode		
			FROM Projects
				JOIN order_book ON Projects.ProjectCode = order_book.ProjectCode
		), svd_union AS
		(
			SELECT * FROM deliveries
			UNION 
			SELECT * FROM allocations
		), svd_projected AS
		(
			SELECT *,
				SUM(Quantity) OVER (PARTITION BY SubjectCode, ObjectCode  ORDER BY AllocationId ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
			FROM svd_union
		), svd_balance AS
		(
			SELECT *, LAG(Balance) OVER (PARTITION BY SubjectCode, ObjectCode  ORDER BY AllocationId) PreviousBalance
			FROM svd_projected
		), alloc_deliveries AS
		(
			SELECT *, 
					CASE CashPolarityCode 
						WHEN 0 THEN
							CASE WHEN Balance > 0 THEN ABS(Quantity) 
								WHEN PreviousBalance > 0 THEN PreviousBalance
								ELSE 0
							END 
						WHEN 1 THEN
							CASE WHEN Balance < 0 THEN Quantity
								WHEN PreviousBalance < 0 THEN ABS(PreviousBalance)
								ELSE 0
							END 
					END QuantityDelivered
			FROM svd_balance
		)
		INSERT INTO Invoice.tbProject (InvoiceNumber, ProjectCode, Quantity, InvoiceValue, CashCode, TaxCode)
		SELECT @InvoiceNumber InvoiceNumber, alloc_deliveries.ProjectCode, alloc_deliveries.QuantityDelivered, Project.UnitCharge * alloc_deliveries.QuantityDelivered, Project.CashCode, Project.TaxCode 
		FROM alloc_deliveries
			JOIN Project.tbProject Project ON alloc_deliveries.ProjectCode = Project.ProjectCode
		WHERE QuantityDelivered > 0;

		INSERT INTO Invoice.tbItem (InvoiceNumber, CashCode, TaxCode, InvoiceValue, ItemReference)
		SELECT @InvoiceNumber InvoiceNumber, cash_code_mirror.CashCode, 
			CASE WHEN (item_mirror.TaxValue / item_mirror.InvoiceValue) <> tax_code.TaxRate 
				THEN (SELECT TOP 1 TaxCode FROM App.tbTaxCode WHERE TaxTypeCode = 1 AND ROUND(TaxRate, 3) =  ROUND((item_mirror.TaxValue / item_mirror.InvoiceValue), 3))
				ELSE tax_code.TaxCode 
				END TaxCode,
				item_mirror.InvoiceValue, item_mirror.ChargeDescription ItemReference
		FROM Invoice.tbMirror invoice_mirror 
			JOIN Invoice.tbMirrorItem item_mirror ON invoice_mirror.ContractAddress = item_mirror.ContractAddress			
			JOIN Cash.tbMirror cash_code_mirror ON item_mirror.ChargeCode = cash_code_mirror.ChargeCode and invoice_mirror.SubjectCode = cash_code_mirror.SubjectCode
			JOIN Cash.tbCode cash_code ON cash_code_mirror.CashCode = cash_code.CashCode
			JOIN App.tbTaxCode tax_code ON cash_code.TaxCode = tax_code.TaxCode
		WHERE invoice_mirror.ContractAddress = @ContractAddress

		EXEC Invoice.proc_Total @InvoiceNumber	

		COMMIT TRAN

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Invoice].[proc_RaiseBlank]...';


go
CREATE PROCEDURE Invoice.proc_RaiseBlank
	(
	@SubjectCode nvarchar(10),
	@InvoiceTypeCode smallint,
	@InvoiceNumber nvarchar(20) = null output
	)
  AS
  SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		DECLARE 
			@UserId nvarchar(10)
			, @NextNumber int
			, @InvoiceSuffix nvarchar(4)
			, @InvoicedOn datetime

		SELECT @UserId = UserId FROM Usr.vwCredentials

		SET @InvoiceSuffix = '.' + @UserId
	
		SELECT @NextNumber = NextNumber
		FROM Invoice.tbType
		WHERE InvoiceTypeCode = @InvoiceTypeCode
	
		SELECT @InvoiceNumber = FORMAT(@NextNumber, '000000') + @InvoiceSuffix
	
		WHILE EXISTS (SELECT     InvoiceNumber
						FROM         Invoice.tbInvoice
						WHERE     (InvoiceNumber = @InvoiceNumber))
			BEGIN
			SET @NextNumber = @NextNumber + 1
			SET @InvoiceNumber = FORMAT(@NextNumber, '000000') + @InvoiceSuffix
			END
		
		SET @InvoicedOn = isnull(CAST(@InvoicedOn AS DATE), CAST(CURRENT_TIMESTAMP AS DATE))

		BEGIN TRANSACTION
	
		EXEC Invoice.proc_Cancel
	
		UPDATE    Invoice.tbType
		SET              NextNumber = @NextNumber + 1
		WHERE     (InvoiceTypeCode = @InvoiceTypeCode)
	
		INSERT INTO Invoice.tbInvoice
								(InvoiceNumber, UserId, SubjectCode, InvoiceTypeCode, InvoicedOn, InvoiceStatusCode, PaymentTerms)
		 SELECT @InvoiceNumber, @UserId, @SubjectCode, @InvoiceTypeCode, @InvoicedOn, 0, PaymentTerms
		 FROM Subject.tbSubject
		 WHERE SubjectCode = @SubjectCode
	
		COMMIT TRANSACTION
	
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_CodeDefaults]...';


go
CREATE PROCEDURE Cash.proc_CodeDefaults 
	(
	@CashCode nvarchar(50)
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		SELECT     Cash.tbCode.CashCode, Cash.tbCode.CashDescription, Cash.tbCode.CategoryCode, Cash.tbCode.TaxCode, 
				App.tbTaxCode.TaxTypeCode, ISNULL( Cash.tbCategory.CashPolarityCode, 0) AS CashPolarityCode, ISNULL(Cash.tbCategory.CashTypeCode, 0) AS CashTypeCode
		FROM         Cash.tbCode INNER JOIN
							  App.tbTaxCode ON Cash.tbCode.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
							  Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
		WHERE     ( Cash.tbCode.CashCode = @CashCode)
    END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_ChangeNote]...';


go
CREATE   PROCEDURE Cash.proc_ChangeNote (@PaymentAddress nvarchar(42), @Note nvarchar(256))
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		UPDATE Cash.tbChange 
		SET Note = @Note
		WHERE PaymentAddress = @PaymentAddress;			
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_ChangeDelete]...';


go
CREATE   PROCEDURE Cash.proc_ChangeDelete (@PaymentAddress nvarchar(42))
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		IF EXISTS (
			SELECT * FROM Cash.tbChange change
				OUTER APPLY
				(
					SELECT PaymentAddress, SUM(MoneyIn) Balance
					FROM Cash.tbTx tx
					WHERE tx.PaymentAddress = change.PaymentAddress AND tx.TxStatusCode = 1
					GROUP BY PaymentAddress			
				) change_balance
			WHERE change.PaymentAddress = @PaymentAddress AND ChangeStatusCode = 0 AND COALESCE(change_balance.Balance, 0) = 0)
		BEGIN
			DELETE FROM Cash.tbChangeReference WHERE PaymentAddress = @PaymentAddress;
			DELETE FROM Cash.tbChange WHERE PaymentAddress = @PaymentAddress;			
		END
		ELSE
			RETURN 1;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_TaxAdjustment]...';


go
CREATE   PROCEDURE Cash.proc_TaxAdjustment (@StartOn datetime, @TaxTypeCode smallint, @TaxAdjustment decimal(18, 5))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		DECLARE 		
			@PayTo datetime,
			@PayFrom datetime;

		SELECT 
			@PayFrom = PayFrom,
			@PayTo = PayTo 
		FROM Cash.fnTaxTypeDueDates(@TaxTypeCode) due_dates 
		WHERE @StartOn >= due_dates.PayFrom AND @StartOn < due_dates.PayTo

		UPDATE App.tbYearPeriod
		SET 
			TaxAdjustment = CASE @TaxTypeCode WHEN 0 THEN 0 ELSE TaxAdjustment END,
			VatAdjustment = CASE @TaxTypeCode WHEN 1 THEN 0 ELSE VatAdjustment END
		WHERE StartOn >= @PayFrom AND StartOn < @PayTo;

		SELECT @StartOn = MAX(StartOn)
		FROM App.tbYearPeriod
		WHERE StartOn < @PayTo;

		UPDATE App.tbYearPeriod
		SET 
			TaxAdjustment = CASE @TaxTypeCode WHEN 0 THEN @TaxAdjustment ELSE TaxAdjustment END,
			VatAdjustment = CASE @TaxTypeCode WHEN 1 THEN @TaxAdjustment ELSE VatAdjustment END
		WHERE StartOn = @StartOn;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_VatBalance]...';


go
CREATE   PROCEDURE Cash.proc_VatBalance(@Balance decimal(18, 5) output)
 AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SELECT TOP (1)  @Balance = Balance FROM Cash.vwTaxVatStatement ORDER BY StartOn DESC, VatDue DESC
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_FlowCategoryCodeFromName]...';


go

CREATE   PROCEDURE Cash.proc_FlowCategoryCodeFromName
	(
		@Category nvarchar(50),
		@CategoryCode nvarchar(10) output
	)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS (SELECT CategoryCode
					FROM         Cash.tbCategory
					WHERE     (Category = @Category))
			SELECT @CategoryCode = CategoryCode
			FROM         Cash.tbCategory
			WHERE     (Category = @Category)
		ELSE
			SET @CategoryCode = 0 
    END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_FlowCashCodeValues]...';


go
CREATE   PROCEDURE Cash.proc_FlowCashCodeValues(@CashCode nvarchar(50), @YearNumber smallint, @IncludeActivePeriods BIT = 0, @IncludeOrderBook BIT = 0, @IncludeTaxAccruals BIT = 0)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @StartOn DATE
			, @IsTaxCode BIT = 0;

		DECLARE @tbReturn AS TABLE (
			StartOn DATETIME NOT NULL, 
			CashStatusCode SMALLINT NOT NULL, 
			ForecastValue DECIMAL(18, 5) NOT NULL, 
			ForecastTax DECIMAL(18, 5) NOT NULL, 
			InvoiceValue DECIMAL(18, 5) NOT NULL, 
			InvoiceTax DECIMAL(18, 5) NOT NULL);

		INSERT INTO @tbReturn (StartOn, CashStatusCode, ForecastValue, ForecastTax, InvoiceValue, InvoiceTax)
		SELECT   Cash.tbPeriod.StartOn, App.tbYearPeriod.CashStatusCode,
			Cash.tbPeriod.ForecastValue, 
			Cash.tbPeriod.ForecastTax, 
			CASE App.tbYearPeriod.CashStatusCode WHEN 2 THEN Cash.tbPeriod.InvoiceValue ELSE 0 END AS InvoiceValue, 
			CASE App.tbYearPeriod.CashStatusCode WHEN 2 THEN Cash.tbPeriod.InvoiceTax ELSE 0 END AS InvoiceTax
		FROM            Cash.tbPeriod INNER JOIN
									App.tbYearPeriod ON Cash.tbPeriod.StartOn = App.tbYearPeriod.StartOn INNER JOIN
									App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
		WHERE        (App.tbYear.CashStatusCode < 3) AND (App.tbYearPeriod.YearNumber = @YearNumber) AND (Cash.tbPeriod.CashCode = @CashCode);

	
		SELECT @StartOn = (SELECT CAST(MIN(StartOn) AS DATE) FROM @tbReturn WHERE CashStatusCode < 2);
		IF EXISTS(SELECT * FROM Cash.tbTaxType tt WHERE tt.CashCode = @CashCode) SET @IsTaxCode = 1;

		IF (@IncludeActivePeriods <> 0)
			BEGIN		
			WITH active_candidates AS
			(
				SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod AS p WHERE (StartOn <= invoices.InvoicedOn) ORDER BY StartOn DESC) AS StartOn,
						CASE WHEN invoice_type.CashPolarityCode = 0 THEN Projects.InvoiceValue * - 1 ELSE Projects.InvoiceValue END AS InvoiceValue, 
						CASE WHEN invoice_type.CashPolarityCode = 0 THEN Projects.TaxValue * - 1 ELSE Projects.TaxValue END AS InvoiceTax
				FROM Invoice.tbInvoice invoices
					JOIN Invoice.tbType invoice_type ON invoices.InvoiceTypeCode = invoice_type.InvoiceTypeCode
					JOIN Invoice.tbProject Projects ON invoices.InvoiceNumber = Projects.InvoiceNumber
				WHERE invoices.InvoicedOn >= @StartOn
					AND Projects.CashCode = @CashCode
			), active_Projects AS
			(
				SELECT StartOn, SUM(InvoiceValue) InvoiceValue, SUM(InvoiceTax) InvoiceTax
				FROM active_candidates
				GROUP BY StartOn
			), active_items AS
			(
				SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod AS p WHERE (StartOn <= invoices.InvoicedOn) ORDER BY StartOn DESC) AS StartOn,
						CASE WHEN invoice_type.CashPolarityCode = 0 THEN items.InvoiceValue * - 1 ELSE items.InvoiceValue END AS InvoiceValue, 
						CASE WHEN invoice_type.CashPolarityCode = 0 THEN items.TaxValue * - 1 ELSE items.TaxValue END AS InvoiceTax
				FROM Invoice.tbInvoice invoices
					JOIN Invoice.tbType invoice_type ON invoices.InvoiceTypeCode = invoice_type.InvoiceTypeCode
					JOIN Invoice.tbItem items ON invoices.InvoiceNumber = items.InvoiceNumber
				WHERE invoices.InvoicedOn >= @StartOn AND items.CashCode = @CashCode
			), active_invoices AS
			(
				SELECT StartOn, InvoiceValue, InvoiceTax FROM active_Projects
				UNION
				SELECT StartOn, InvoiceValue, InvoiceTax FROM active_items
			), active_periods AS
			(
				SELECT StartOn, SUM(InvoiceValue) AS InvoiceValue, SUM(InvoiceTax) AS InvoiceTax
				FROM active_invoices
				GROUP BY StartOn
			)
			UPDATE cashcode_values
			SET InvoiceValue += ABS(active_periods.InvoiceValue), InvoiceTax += ABS(active_periods.InvoiceTax)
			FROM @tbReturn cashcode_values JOIN active_periods ON cashcode_values.StartOn = active_periods.StartOn;

			IF @IsTaxCode <> 0
				BEGIN
				IF EXISTS (SELECT CashCode FROM Cash.tbTaxType WHERE CashCode = @CashCode AND TaxTypeCode = 0)
					BEGIN	
					WITH ct_due AS
					(
						SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod AS p WHERE (StartOn <= ct_statement.StartOn) ORDER BY StartOn DESC) AS StartOn, TaxDue 
						FROM Cash.vwTaxCorpStatement ct_statement
						WHERE ct_statement.StartOn >= @StartOn
					)							
					UPDATE cashcode_values
					SET InvoiceValue += TaxDue
					FROM ct_due
						JOIN @tbReturn cashcode_values ON ct_due.StartOn = cashcode_values.StartOn;	
					END

				IF EXISTS (SELECT CashCode FROM Cash.tbTaxType WHERE CashCode = @CashCode AND TaxTypeCode = 1)
					BEGIN			
					WITH vat_due AS
					(
						SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod AS p WHERE (StartOn <= vat_statement.StartOn) ORDER BY StartOn DESC) AS StartOn, VatDue 
						FROM Cash.vwTaxVatStatement vat_statement
						WHERE vat_statement.StartOn >= @StartOn
					)
					UPDATE cashcode_values
					SET InvoiceValue += VatDue
					FROM vat_due
						JOIN @tbReturn cashcode_values ON vat_due.StartOn = cashcode_values.StartOn;		
					END
				END
			END

		IF (@IncludeOrderBook <> 0)
			BEGIN
			WITH Projects AS
			(
				SELECT Project.ProjectCode,
						(SELECT        TOP (1) StartOn
						FROM            App.tbYearPeriod
						WHERE        (StartOn <= Project.ActionOn)
						ORDER BY StartOn DESC) AS StartOn, Project.TotalCharge, ISNULL(tax.TaxRate, 0) AS TaxRate
				FROM            Project.tbProject AS Project INNER JOIN
											App.tbTaxCode AS tax ON Project.TaxCode = tax.TaxCode
				WHERE     (Project.CashCode = @CashCode) AND ((Project.ProjectStatusCode = 1) OR (Project.ProjectStatusCode = 2))
			), Projects_foryear AS
			(
				SELECT Projects.ProjectCode, Projects.StartOn, Projects.TotalCharge, Projects.TaxRate
				FROM Projects
					JOIN @tbReturn invoice_history ON Projects.StartOn = invoice_history.StartOn		
			)
			, order_invoice_value AS
			(
				SELECT   invoices.ProjectCode, Projects_foryear.StartOn, SUM(invoices.InvoiceValue) AS InvoiceValue, SUM(invoices.TaxValue) AS InvoiceTax
				FROM  Invoice.tbProject invoices
					JOIN Projects_foryear ON invoices.ProjectCode = Projects_foryear.ProjectCode 
				GROUP BY invoices.ProjectCode, StartOn
			), orders AS
			(
				SELECT Projects_foryear.StartOn, 
					Projects_foryear.TotalCharge - ISNULL(order_invoice_value.InvoiceValue, 0) AS InvoiceValue,
					(Projects_foryear.TotalCharge * Projects_foryear.TaxRate) - ISNULL(order_invoice_value.InvoiceTax, 0) AS InvoiceTax
				FROM Projects_foryear LEFT OUTER JOIN order_invoice_value ON Projects_foryear.ProjectCode = order_invoice_value.ProjectCode
			), order_summary AS
			(
				SELECT StartOn, SUM(InvoiceValue) As InvoiceValue, SUM(InvoiceTax) As InvoiceTax
				FROM orders
				GROUP BY StartOn
			)
			UPDATE cashcode_values
			SET InvoiceValue += order_summary.InvoiceValue, InvoiceTax += order_summary.InvoiceTax
			FROM @tbReturn cashcode_values JOIN order_summary ON cashcode_values.StartOn = order_summary.StartOn;

			END
	
		IF (@IncludeTaxAccruals <> 0) AND (@IsTaxCode <> 0)
			BEGIN
			IF EXISTS (SELECT CashCode FROM Cash.tbTaxType WHERE CashCode = @CashCode AND TaxTypeCode = 0)
				BEGIN
				WITH ct_dates AS
				(
					SELECT PayOn, PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(0)
				), ct_period AS
				(
					SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod WHERE (StartOn <= PayOn) ORDER BY StartOn DESC) AS StartOn, PayOn, PayFrom, PayTo
					FROM  ct_dates 
						JOIN  App.tbYearPeriod AS year_period ON ct_dates.PayTo = year_period.StartOn 
						JOIN App.tbYear AS y ON year_period.YearNumber = y.YearNumber 
					WHERE     year_period.StartOn >= (SELECT StartOn FROM App.vwActivePeriod)
				), ct_accrual_details AS
				(		
					SELECT StartOn, SUM(TaxDue) AS TaxDue 
					FROM Cash.vwTaxCorpAccruals
					WHERE TaxDue <> 0
					GROUP BY StartOn
				), ct_accruals AS
				(
					SELECT (SELECT ct_period.StartOn FROM ct_period WHERE ct_accrual_details.StartOn >= ct_period.PayFrom AND ct_accrual_details.StartOn < ct_period.PayTo) AS StartOn, TaxDue
					FROM ct_accrual_details
				), ct_due AS
				(
					SELECT StartOn, SUM(TaxDue) AS TaxDue
					FROM ct_accruals
					GROUP BY StartOn
				)
				UPDATE cashcode_values
				SET InvoiceValue += TaxDue
				FROM ct_due
					JOIN @tbReturn cashcode_values ON ct_due.StartOn = cashcode_values.StartOn;	

				END

			IF EXISTS (SELECT CashCode FROM Cash.tbTaxType WHERE CashCode = @CashCode AND TaxTypeCode = 1)
				BEGIN
				WITH vat_dates AS
				(
					SELECT PayOn, PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(1)
				), vat_period AS
				(
					SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod WHERE (StartOn <= PayOn) ORDER BY StartOn DESC) AS StartOn, PayOn, PayFrom, PayTo
					FROM  vat_dates 
						JOIN  App.tbYearPeriod AS year_period ON vat_dates.PayTo = year_period.StartOn 
						JOIN App.tbYear AS y ON year_period.YearNumber = y.YearNumber 
					WHERE     (y.CashStatusCode = 1) OR (y.CashStatusCode = 2)
				), vat_accrual_details AS
				(		
					SELECT StartOn, SUM(VatDue) AS VatDue 
					FROM Cash.vwTaxVatAccruals
					WHERE VatDue <> 0
					GROUP BY StartOn
				), vat_accruals AS
				(
					SELECT (SELECT vat_period.StartOn FROM vat_period WHERE vat_accrual_details.StartOn >= vat_period.PayFrom AND vat_accrual_details.StartOn < vat_period.PayTo) AS StartOn, VatDue
					FROM vat_accrual_details
				), vat_due AS
				(
					SELECT StartOn, SUM(VatDue) AS VatDue
					FROM vat_accruals
					GROUP BY StartOn
				)
				UPDATE cashcode_values
				SET InvoiceValue += VatDue
				FROM vat_due
					JOIN @tbReturn cashcode_values ON vat_due.StartOn = cashcode_values.StartOn;	
				END
			END

		SELECT StartOn, InvoiceValue, InvoiceTax, ForecastValue, ForecastTax FROM @tbReturn;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_NextPaymentCode]...';


go
CREATE   PROCEDURE Cash.proc_NextPaymentCode (@PaymentCode NVARCHAR(20) OUTPUT)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SELECT @PaymentCode = PaymentCode FROM Cash.vwPaymentCode;
		WHILE EXISTS (SELECT * FROM Cash.tbPayment WHERE PaymentCode = @PaymentCode)
			SELECT @PaymentCode = PaymentCode FROM Cash.vwPaymentCode;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_NetworkUpdated]...';


go

CREATE   PROCEDURE Cash.proc_NetworkUpdated(@SubjectCode nvarchar(10), @ChargeCode nvarchar(50))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		UPDATE Cash.tbMirror
		SET TransmitStatusCode = 3
		WHERE SubjectCode = @SubjectCode AND ChargeCode = @ChargeCode;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_Mirror]...';


go
CREATE   PROCEDURE Cash.proc_Mirror(@CashCode nvarchar(50), @SubjectCode nvarchar(10), @ChargeCode nvarchar(50))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF NOT EXISTS (SELECT * FROM Cash.tbMirror WHERE CashCode = @CashCode AND SubjectCode = @SubjectCode AND ChargeCode = @ChargeCode)
		BEGIN
			INSERT INTO Cash.tbMirror (CashCode, SubjectCode, ChargeCode)
			VALUES (@CashCode, @SubjectCode, @ChargeCode);
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_PaymentPostReconcile]...';


go
CREATE PROCEDURE Cash.proc_PaymentPostReconcile
	(
	@PaymentCode nvarchar(20),
	@PostValue decimal(18, 5),
	@CashCode nvarchar(50),
	@TaxCode nvarchar(5),
	@InvoiceTypeCode smallint
	)
 AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@InvoiceNumber nvarchar(20)
			, @NextNumber int;

		SELECT @NextNumber = NextNumber
		FROM Invoice.tbType
		WHERE InvoiceTypeCode = @InvoiceTypeCode;
		
		SET @InvoiceNumber = FORMAT(@NextNumber, '000000') + '.' + (SELECT UserId FROM Usr.vwCredentials)

		WHILE EXISTS (SELECT     InvoiceNumber
					  FROM         Invoice.tbInvoice
					  WHERE     (InvoiceNumber = @InvoiceNumber))
			BEGIN
			SET @NextNumber += @NextNumber 
			SET @InvoiceNumber = FORMAT(@NextNumber, '000000') + '.' + (SELECT UserId FROM Usr.vwCredentials)
			END

		UPDATE    Invoice.tbType
		SET              NextNumber = @NextNumber + 1
		WHERE     (InvoiceTypeCode = @InvoiceTypeCode)

		INSERT INTO Invoice.tbInvoice
								 (InvoiceNumber, UserId, SubjectCode, InvoiceTypeCode, InvoiceStatusCode, InvoicedOn, DueOn, ExpectedOn, Printed)
		SELECT        @InvoiceNumber AS InvoiceNumber, Cash.tbPayment.UserId, Cash.tbPayment.SubjectCode, @InvoiceTypeCode AS InvoiceTypeCode, 3 AS InvoiceStatusCode, 
								Cash.tbPayment.PaidOn, Cash.tbPayment.PaidOn AS DueOn, Cash.tbPayment.PaidOn AS ExpectedOn, 1 AS Printed
		FROM            Cash.tbPayment 
		WHERE        ( Cash.tbPayment.PaymentCode = @PaymentCode)

		INSERT INTO Invoice.tbItem (InvoiceNumber, CashCode, TotalValue, TaxCode)
		VALUES (@InvoiceNumber, @CashCode, @PostValue, @TaxCode)

		EXEC Invoice.proc_Total @InvoiceNumber

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_ChangeNew]...';


go
CREATE   PROCEDURE Cash.proc_ChangeNew 
(
	@AccountCode nvarchar(10), 
	@KeyName nvarchar(50), 
	@ChangeTypeCode smallint,
	@PaymentAddress nvarchar(42), 
	@AddressIndex int = 0, 
	@InvoiceNumber nvarchar(20) = NULL,
	@Note nvarchar(256) = NULL
)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		BEGIN TRAN

		INSERT INTO Cash.tbChange (PaymentAddress, AccountCode, HDPath, ChangeTypeCode, AddressIndex, Note)
		SELECT @PaymentAddress, @AccountCode, account_key.HDPath, @ChangeTypeCode, @AddressIndex, @Note
		FROM Subject.tbAccountKey account_key
		WHERE account_key.AccountCode = @AccountCode AND KeyName = @KeyName;

		IF EXISTS (SELECT * FROM Invoice.tbInvoice inv 
						JOIN Invoice.tbType typ ON inv.InvoiceTypeCode = typ.InvoiceTypeCode  
						WHERE typ.CashPolarityCode = 1 AND InvoiceNumber = @InvoiceNumber)
		BEGIN
			IF EXISTS (SELECT * FROM Cash.tbChangeReference WHERE InvoiceNumber = @InvoiceNumber)
				DELETE FROM Cash.tbChangeReference WHERE InvoiceNumber = @InvoiceNumber;
			INSERT INTO Cash.tbChangeReference (PaymentAddress, InvoiceNumber)
			VALUES (@PaymentAddress, @InvoiceNumber);
		END

		COMMIT TRAN

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_ChangeAddressIndex]...';


go
CREATE   PROCEDURE Cash.proc_ChangeAddressIndex 
(
	@AccountCode nvarchar(10), 
	@KeyName nvarchar(50), 
	@ChangeTypeCode smallint,
	@AddressIndex int = 0 output
)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY

		SELECT @AddressIndex = COALESCE(MAX(change.AddressIndex) + 1, 0) 
		FROM Cash.tbChange change
			JOIN Subject.tbAccountKey account_key ON change.AccountCode = account_key.AccountCode AND change.HDPath = account_key.HDPath
		WHERE account_key.AccountCode = @AccountCode AND KeyName = @KeyName AND change.ChangeTypeCode = @ChangeTypeCode

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_AccountRebuild]...';


go

CREATE   PROCEDURE Cash.proc_AccountRebuild
	(
	@AccountCode nvarchar(10)
	)
  AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		
		UPDATE Subject.tbAccount
		SET CurrentBalance = Cash.vwAccountRebuild.CurrentBalance
		FROM         Cash.vwAccountRebuild INNER JOIN
							Subject.tbAccount ON Cash.vwAccountRebuild.AccountCode = Subject.tbAccount.AccountCode
		WHERE Cash.vwAccountRebuild.AccountCode = @AccountCode 

		UPDATE Subject.tbAccount
		SET CurrentBalance = 0
		FROM         Cash.vwAccountRebuild RIGHT OUTER JOIN
							  Subject.tbAccount ON Cash.vwAccountRebuild.AccountCode = Subject.tbAccount.AccountCode
		WHERE     (Cash.vwAccountRebuild.AccountCode IS NULL) AND Subject.tbAccount.AccountCode = @AccountCode
    END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_PaymentMove]...';


go
CREATE   PROCEDURE Cash.proc_PaymentMove
	(
	@PaymentCode nvarchar(20),
	@AccountCode nvarchar(10)
	)
  AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @OldAccountCode nvarchar(10)

		SELECT @OldAccountCode = AccountCode
		FROM         Cash.tbPayment
		WHERE     (PaymentCode = @PaymentCode)
	
		BEGIN TRANSACTION
	
		UPDATE Cash.tbPayment 
		SET AccountCode = @AccountCode,
			UpdatedOn = CURRENT_TIMESTAMP,
			UpdatedBy = (suser_sname())
		WHERE PaymentCode = @PaymentCode	

		EXEC Cash.proc_AccountRebuild @AccountCode
		EXEC Cash.proc_AccountRebuild @OldAccountCode
	
		COMMIT TRANSACTION
	 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_GeneratePeriods]...';


go
CREATE PROCEDURE Cash.proc_GeneratePeriods
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	DECLARE 
		@YearNumber smallint
		, @StartOn datetime
		, @PeriodStartOn datetime
		, @CashStatusCode smallint
		, @Period smallint
	
		DECLARE curYr cursor for	
			SELECT     YearNumber, CAST(CONCAT(FORMAT(YearNumber, '0000'), FORMAT(StartMonth, '00'), FORMAT(1, '00')) AS DATE) AS StartOn, CashStatusCode
			FROM         App.tbYear
			WHERE CashStatusCode < 2

		OPEN curYr
	
		FETCH NEXT FROM curYr INTO @YearNumber, @StartOn, @CashStatusCode
		WHILE @@FETCH_STATUS = 0
			BEGIN
			SET @PeriodStartOn = @StartOn
			SET @Period = 1
			WHILE @Period < 13
				BEGIN
				IF not EXISTS (SELECT MonthNumber FROM App.tbYearPeriod WHERE YearNumber = @YearNumber and MonthNumber = DATEPART(m, @PeriodStartOn))
					BEGIN
					INSERT INTO App.tbYearPeriod (YearNumber, StartOn, MonthNumber, CashStatusCode)
					VALUES (@YearNumber, @PeriodStartOn, DATEPART(m, @PeriodStartOn), 0)				
					END
				SET @PeriodStartOn = DATEADD(m, 1, @PeriodStartOn)	
				SET @Period = @Period + 1
				END		
				
			FETCH NEXT FROM curYr INTO @YearNumber, @StartOn, @CashStatusCode
			END
	
		CLOSE curYr
		DEALLOCATE curYr
	
		INSERT INTO Cash.tbPeriod
							  (CashCode, StartOn)
		SELECT     Cash.vwPeriods.CashCode, Cash.vwPeriods.StartOn
		FROM         Cash.vwPeriods LEFT OUTER JOIN
							  Cash.tbPeriod ON Cash.vwPeriods.CashCode = Cash.tbPeriod.CashCode AND Cash.vwPeriods.StartOn = Cash.tbPeriod.StartOn
		WHERE     ( Cash.tbPeriod.CashCode IS NULL)
		 
		UPDATE Cash.tbPeriod
		SET InvoiceValue = 0, InvoiceTax = 0;
		
		WITH invoice_entries AS
		(
			SELECT invoices.CashCode, invoices.StartOn, categories.CashPolarityCode, SUM(invoices.InvoiceValue) InvoiceValue, SUM(invoices.TaxValue) TaxValue
			FROM  Invoice.vwRegisterDetail invoices
				JOIN Cash.tbCode cash_codes ON invoices.CashCode = cash_codes.CashCode 
				JOIN Cash.tbCategory categories ON cash_codes.CategoryCode = categories .CategoryCode
			WHERE   (StartOn < (SELECT StartOn FROM App.fnActivePeriod()))
			GROUP BY invoices.CashCode, invoices.StartOn, categories.CashPolarityCode
		), invoice_summary AS
		(
			SELECT CashCode, StartOn,
				CASE CashPolarityCode 
					WHEN 0 THEN
						InvoiceValue * -1
					ELSE 
						InvoiceValue
				END AS InvoiceValue,
				CASE CashPolarityCode 
					WHEN 0 THEN
						TaxValue * -1
					ELSE 
						TaxValue
				END AS TaxValue						
			FROM invoice_entries
		)
		UPDATE Cash.tbPeriod
		SET InvoiceValue = invoice_summary.InvoiceValue, 
			InvoiceTax = invoice_summary.TaxValue
		FROM    Cash.tbPeriod 
			JOIN invoice_summary 
				ON Cash.tbPeriod.CashCode = invoice_summary.CashCode AND Cash.tbPeriod.StartOn = invoice_summary.StartOn;

		WITH asset_entries AS
		(
			SELECT payment.CashCode, 
				(SELECT TOP 1 StartOn FROM App.tbYearPeriod WHERE (StartOn <= payment.PaidOn) ORDER BY StartOn DESC) AS StartOn,
				CASE cash_category.CashPolarityCode
					WHEN 0 THEN (PaidInValue + (PaidOutValue * -1)) * -1
					WHEN 1 THEN PaidInValue + (PaidOutValue * -1)
				END AssetValue
			FROM Cash.tbPayment payment
				JOIN Subject.tbAccount account ON payment.AccountCode = account.AccountCode
				JOIN Cash.tbCode cash_code ON account.CashCode = cash_code.CashCode
				JOIN Cash.tbCategory cash_category ON cash_code.CategoryCode = cash_category.CategoryCode
			WHERE account.AccountTypeCode = 2 AND payment.IsProfitAndLoss <> 0 AND PaidOn < (SELECT StartOn FROM App.fnActivePeriod())
		), asset_summary AS
		(
			SELECT CashCode, StartOn, SUM(AssetValue) AssetValue
			FROM asset_entries
			GROUP BY CashCode, StartOn
		)
		UPDATE Cash.tbPeriod
		SET InvoiceValue = AssetValue
		FROM  Cash.tbPeriod 
			JOIN asset_summary 
				ON Cash.tbPeriod.CashCode = asset_summary.CashCode AND Cash.tbPeriod.StartOn = asset_summary.StartOn;	

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_CurrentAccount]...';


go
CREATE PROCEDURE Cash.proc_CurrentAccount(@AccountCode NVARCHAR(10) OUTPUT)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		SELECT  @AccountCode = AccountCode
		FROM Cash.vwCurrentAccount;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_CoinType]...';


go
CREATE   PROCEDURE Cash.proc_CoinType(@CoinTypeCode smallint output)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	
		DECLARE @AccountCode nvarchar(10);

		EXEC Cash.proc_CurrentAccount @AccountCode output
		SELECT @CoinTypeCode = CoinTypeCode FROM Subject.tbAccount WHERE AccountCode = @AccountCode

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_ReserveAccount]...';


go
CREATE PROCEDURE Cash.proc_ReserveAccount(@AccountCode NVARCHAR(10) OUTPUT)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		SELECT  @AccountCode = AccountCode
		FROM Cash.vwReserveAccount;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_PaymentPostInvoiced]...';


go
CREATE PROCEDURE Cash.proc_PaymentPostInvoiced (@PaymentCode nvarchar(20))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@SubjectCode nvarchar(10)
			, @PostValue decimal(18, 5)
			, @CashCode nvarchar(50);

		SELECT   @PostValue = CASE WHEN PaidInValue = 0 THEN PaidOutValue ELSE PaidInValue * -1 END,
			@SubjectCode = Subject.tbSubject.SubjectCode
		FROM         Cash.tbPayment INNER JOIN
							  Subject.tbSubject ON Cash.tbPayment.SubjectCode = Subject.tbSubject.SubjectCode
		WHERE     ( Cash.tbPayment.PaymentCode = @PaymentCode);

		IF NOT EXISTS (SELECT InvoiceNumber FROM Invoice.tbInvoice WHERE (InvoiceStatusCode BETWEEN 1 AND 2) AND (SubjectCode = @SubjectCode))
			RETURN;

		IF EXISTS (SELECT * FROM  Invoice.tbInvoice 
						INNER JOIN Invoice.tbProject ON Invoice.tbInvoice.InvoiceNumber = Invoice.tbProject.InvoiceNumber
					WHERE        (Invoice.tbInvoice.SubjectCode = @SubjectCode) AND (Invoice.tbInvoice.InvoiceStatusCode < 3))
		BEGIN
			SELECT  @CashCode = Invoice.tbProject.CashCode
			FROM  Invoice.tbInvoice 
				INNER JOIN Invoice.tbProject ON Invoice.tbInvoice.InvoiceNumber = Invoice.tbProject.InvoiceNumber
			WHERE        (Invoice.tbInvoice.SubjectCode = @SubjectCode) AND (Invoice.tbInvoice.InvoiceStatusCode < 3)
			GROUP BY Invoice.tbProject.CashCode;
		END
		ELSE IF EXISTS (SELECT * FROM Invoice.tbInvoice 
							INNER JOIN Invoice.tbItem ON Invoice.tbInvoice.InvoiceNumber = Invoice.tbItem.InvoiceNumber
						WHERE        (Invoice.tbInvoice.SubjectCode = @SubjectCode) AND (Invoice.tbInvoice.InvoiceStatusCode < 3)
						GROUP BY Invoice.tbItem.CashCode)
		BEGIN
			SELECT @CashCode = Invoice.tbItem.CashCode
			FROM  Invoice.tbInvoice 
				INNER JOIN Invoice.tbItem ON Invoice.tbInvoice.InvoiceNumber = Invoice.tbItem.InvoiceNumber
			WHERE        (Invoice.tbInvoice.SubjectCode = @SubjectCode) AND (Invoice.tbInvoice.InvoiceStatusCode < 3)
			GROUP BY Invoice.tbItem.CashCode;
		END

		BEGIN TRANSACTION;

		UPDATE Cash.tbPayment
		SET PaymentStatusCode = 1, CashCode = @CashCode
		WHERE (PaymentCode = @PaymentCode);
		
		WITH invoice_status AS
		(
			SELECT InvoiceNumber, InvoiceStatusCode, PaidValue, PaidTaxValue
			FROM Invoice.vwStatusLive
			WHERE SubjectCode = @SubjectCode
		)
		UPDATE invoices
		SET 
			InvoiceStatusCode = invoice_status.InvoiceStatusCode,
			PaidValue = invoice_status.PaidValue,
			PaidTaxValue = invoice_status.PaidTaxValue
		FROM Invoice.tbInvoice invoices	
			JOIN invoice_status ON invoices.InvoiceNumber = invoice_status.InvoiceNumber
		WHERE 
			invoices.InvoiceStatusCode <> invoice_status.InvoiceStatusCode 
			OR invoices.PaidValue <> invoice_status.PaidValue 
			OR invoices.PaidTaxValue <> invoice_status.PaidTaxValue;

		UPDATE  Subject.tbAccount
		SET CurrentBalance = Subject.tbAccount.CurrentBalance + (@PostValue * -1)
		FROM         Subject.tbAccount INNER JOIN
							  Cash.tbPayment ON Subject.tbAccount.AccountCode = Cash.tbPayment.AccountCode
		WHERE Cash.tbPayment.PaymentCode = @PaymentCode
		
		COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_PaymentDelete]...';


go
CREATE   PROCEDURE Cash.proc_PaymentDelete
	(
	@PaymentCode nvarchar(20)
	)
 AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@SubjectCode nvarchar(10)
			, @AccountCode nvarchar(10)

		SELECT  @SubjectCode = SubjectCode, @AccountCode = AccountCode
		FROM         Cash.tbPayment
		WHERE     (PaymentCode = @PaymentCode)

		DELETE FROM Cash.tbPayment
		WHERE     (PaymentCode = @PaymentCode)
	
		EXEC Subject.proc_Rebuild @SubjectCode

		BEGIN TRANSACTION
		EXEC Cash.proc_AccountRebuild @AccountCode
		COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_PaymentAdd]...';


go
CREATE PROCEDURE Cash.proc_PaymentAdd(@SubjectCode nvarchar(10), @AccountCode AS nvarchar(10), @CashCode nvarchar(50), @PaidOn datetime, @ToPay decimal(18, 5), @PaymentReference nvarchar(50) = null, @PaymentCode nvarchar(20) output)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		
		EXECUTE Cash.proc_NextPaymentCode  @PaymentCode OUTPUT

		INSERT INTO Cash.tbPayment (PaymentCode, UserId, PaymentStatusCode, SubjectCode, AccountCode, CashCode, TaxCode, PaidOn, PaidInValue, PaidOutValue, PaymentReference)
		SELECT   @PaymentCode AS PaymentCode, 
			(SELECT UserId FROM Usr.vwCredentials) AS UserId,
			0 AS PaymentStatusCode,
			@SubjectCode AS SubjectCode,
			@AccountCode AS AccountCode,
			@CashCode AS CashCode,
			Cash.tbCode.TaxCode,
			@PaidOn As PaidOn,
			CASE WHEN @ToPay > 0 THEN @ToPay ELSE 0 END AS PaidInValue,
			CASE WHEN @ToPay < 0 THEN ABS(@ToPay) ELSE 0 END AS PaidOutValue,
			@PaymentReference PaymentReference
		FROM Cash.tbCode 
			INNER JOIN Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode 
		WHERE        (Cash.tbCode.CashCode = @CashCode)


	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_TxPayInChange]...';


go
CREATE   PROCEDURE Cash.proc_TxPayInChange
(
	@AccountCode nvarchar(10), 
	@PaymentAddress nvarchar(42),
	@TxId nvarchar(64),
	@SubjectCode nvarchar(10), 
	@CashCode nvarchar(50),
	@PaymentReference nvarchar(50) = null
)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		DECLARE 
			@PaymentCode nvarchar(20)
			, @TaxCode nvarchar(10) = (SELECT TaxCode FROM Cash.tbCode WHERE CashCode = @CashCode);
			
		BEGIN TRAN

		EXECUTE Cash.proc_NextPaymentCode  @PaymentCode OUTPUT

		INSERT INTO Cash.tbPayment (UserId, PaymentCode, AccountCode, PaidOn, SubjectCode, PaymentStatusCode, PaidInValue, CashCode, TaxCode, PaymentReference)
		SELECT 
			(SELECT UserId FROM Usr.vwCredentials) UserId,
			@PaymentCode PaymentCode, @AccountCode AccountCode, CURRENT_TIMESTAMP PaidOn, @SubjectCode SubjectCode, 1 PaymentStatusCode, MoneyIn, 
			@CashCode CashCode, @TaxCode TaxCode, @PaymentReference PaymentReference
		FROM Cash.tbTx
		WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress;

		UPDATE  Subject.tbAccount
		SET CurrentBalance = Subject.tbAccount.CurrentBalance + PaidInValue
		FROM         Subject.tbAccount INNER JOIN
							  Cash.tbPayment ON Subject.tbAccount.AccountCode = Cash.tbPayment.AccountCode
		WHERE Cash.tbPayment.PaymentCode = @PaymentCode

		UPDATE Cash.tbTx
		SET TxStatusCode = 1
		WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress;

		INSERT INTO Cash.tbTxReference (TxNumber, PaymentCode, TxStatusCode)
		SELECT TxNumber, @PaymentCode PaymentCode, TxStatusCode
		FROM Cash.tbTx
		WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress;

		COMMIT TRAN

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_PaymentPostMisc]...';


go
CREATE PROCEDURE Cash.proc_PaymentPostMisc
	(
	@PaymentCode nvarchar(20) 
	)
 AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@InvoiceNumber nvarchar(20), 
			@NextNumber int, 
			@InvoiceTypeCode smallint;

		IF NOT EXISTS (SELECT        Cash.tbPayment.PaymentCode
						FROM            Cash.tbPayment INNER JOIN
												 Cash.tbCode ON Cash.tbPayment.CashCode = Cash.tbCode.CashCode INNER JOIN
												 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
						WHERE        (Cash.tbPayment.PaymentStatusCode <> 1)  
							AND Cash.tbPayment.UserId = (SELECT UserId FROM Usr.vwCredentials))
			RETURN 

		SELECT @InvoiceTypeCode = CASE WHEN PaidInValue != 0 THEN 0 ELSE 2 END 
		FROM         Cash.tbPayment
		WHERE     (PaymentCode = @PaymentCode)
	
		SELECT @NextNumber = NextNumber
		FROM Invoice.tbType
		WHERE InvoiceTypeCode = @InvoiceTypeCode;
		
		SET @InvoiceNumber = FORMAT(@NextNumber, '000000') + '.' + (SELECT UserId FROM Usr.vwCredentials)

		WHILE EXISTS (SELECT     InvoiceNumber
					  FROM         Invoice.tbInvoice
					  WHERE     (InvoiceNumber = @InvoiceNumber))
			BEGIN
			SET @NextNumber += @NextNumber 
			SET @InvoiceNumber = FORMAT(@NextNumber, '000000') + '.' + (SELECT UserId FROM Usr.vwCredentials)
			END
		
		BEGIN TRANSACTION

		UPDATE    Invoice.tbType
		SET              NextNumber = @NextNumber + 1
		WHERE     (InvoiceTypeCode = @InvoiceTypeCode);

		WITH payment AS
		(
			SELECT UserId, SubjectCode, PaidOn, PaidInValue, PaidOutValue,
					CASE TaxRate WHEN 0 THEN 0
					ELSE
					(
						CASE App.tbTaxCode.RoundingCode 
							WHEN 0 THEN ROUND(Cash.tbPayment.PaidInValue - ( Cash.tbPayment.PaidInValue / (1 + App.tbTaxCode.TaxRate)), Decimals) 
							WHEN 1 THEN ROUND(Cash.tbPayment.PaidInValue - ( Cash.tbPayment.PaidInValue / (1 + App.tbTaxCode.TaxRate)), Decimals, 1) 
						END
					)
					END TaxInValue, 			 
					CASE TaxRate WHEN 0 THEN 0
					ELSE
					(
						CASE App.tbTaxCode.RoundingCode 
							WHEN 0 THEN ROUND(Cash.tbPayment.PaidOutValue - ( Cash.tbPayment.PaidOutValue / (1 + App.tbTaxCode.TaxRate)), Decimals) 
							WHEN 1 THEN ROUND(Cash.tbPayment.PaidOutValue - ( Cash.tbPayment.PaidOutValue / (1 + App.tbTaxCode.TaxRate)), Decimals, 1) 
						END
					)
					END TaxOutValue
			FROM Cash.tbPayment
				INNER JOIN App.tbTaxCode ON Cash.tbPayment.TaxCode = App.tbTaxCode.TaxCode
			WHERE     (PaymentCode = @PaymentCode)
		)
		INSERT INTO Invoice.tbInvoice
								 (InvoiceNumber, UserId, SubjectCode, InvoiceTypeCode, InvoiceStatusCode, InvoicedOn, DueOn, ExpectedOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue, Printed)
		SELECT        @InvoiceNumber AS InvoiceNumber, payment.UserId, payment.SubjectCode, @InvoiceTypeCode AS InvoiceTypeCode, 3 AS InvoiceStatusCode, 
								payment.PaidOn, payment.PaidOn AS DueOn, payment.PaidOn AS ExpectedOn,
								CASE WHEN PaidInValue > 0 THEN PaidInValue - TaxInValue
									WHEN PaidOutValue > 0 THEN PaidOutValue - TaxOutValue
								END AS InvoiceValue, 
								CASE WHEN payment.PaidInValue > 0 THEN payment.TaxInValue 
									WHEN payment.PaidOutValue > 0 THEN payment.TaxOutValue
								END AS TaxValue, 
								CASE WHEN PaidInValue > 0 THEN PaidInValue - TaxInValue
									WHEN PaidOutValue > 0 THEN PaidOutValue - TaxOutValue
								END AS PaidValue, 
								CASE WHEN payment.PaidInValue > 0 THEN payment.TaxInValue 
									WHEN payment.PaidOutValue > 0 THEN payment.TaxOutValue
								END AS PaidTaxValue, 
								1 AS Printed
		FROM payment;

		WITH payment AS
		(
			SELECT CashCode, TaxCode
			FROM Cash.tbPayment
			WHERE (Cash.tbPayment.PaymentCode = @PaymentCode)
		), invoice_header AS
		(
			SELECT InvoiceNumber, InvoiceValue, TaxValue
			FROM Invoice.tbInvoice
			WHERE InvoiceNumber = @InvoiceNumber
		)
		INSERT INTO Invoice.tbItem
							(InvoiceNumber, CashCode, InvoiceValue, TaxValue, TaxCode)
		SELECT TOP 1 InvoiceNumber, CashCode, InvoiceValue, TaxValue, TaxCode
		FROM payment
			CROSS JOIN invoice_header;

		UPDATE  Subject.tbAccount
		SET CurrentBalance = CASE WHEN PaidInValue > 0 THEN Subject.tbAccount.CurrentBalance + PaidInValue ELSE Subject.tbAccount.CurrentBalance - PaidOutValue END
		FROM         Subject.tbAccount INNER JOIN
							  Cash.tbPayment ON Subject.tbAccount.AccountCode = Cash.tbPayment.AccountCode
		WHERE Cash.tbPayment.PaymentCode = @PaymentCode

		UPDATE Cash.tbPayment
		SET PaymentStatusCode = 1
		WHERE (PaymentCode = @PaymentCode)

		COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_BasicSetup]...';


go
CREATE PROCEDURE App.proc_BasicSetup
(
	@TemplateName NVARCHAR(100),
	@FinancialMonth SMALLINT = 4,
	@CoinTypeCode SMALLINT,
	@GovAccountName NVARCHAR(255),
	@BankName NVARCHAR(255) = null,
	@BankAddress NVARCHAR(MAX) = null,
	@DummyAccount NVARCHAR(50) = null, 
	@CurrentAccount NVARCHAR(50) = null,
	@CA_SortCode NVARCHAR(10) = null,
	@CA_AccountNumber NVARCHAR(20) = null,
	@ReserveAccount NVARCHAR(50) = null, 
	@RA_SortCode NVARCHAR(10) = null,
	@RA_AccountNumber NVARCHAR(20) = null
)
AS
	DECLARE 
		@FinancialYear SMALLINT = DATEPART(YEAR, CURRENT_TIMESTAMP);

		IF EXISTS (SELECT * FROM App.tbOptions WHERE UnitOfCharge <> 'BTC') AND (@CoinTypeCode <> 2)
			SET @CoinTypeCode = 2;

		IF DATEPART(MONTH, CURRENT_TIMESTAMP) < @FinancialMonth
			 SET @FinancialYear -= 1;
		
	DECLARE 
		@Year SMALLINT = @FinancialYear - 1;

	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		BEGIN TRAN
		
		UPDATE App.tbOptions
		SET CoinTypeCode = @CoinTypeCode;

		DECLARE 
			@ProcName nvarchar(100) = (SELECT StoredProcedure FROM App.tbTemplate WHERE TemplateName = @TemplateName);		

		EXEC @ProcName
				@FinancialMonth = @FinancialMonth,
				@GovAccountName = @GovAccountName, 
				@BankName = @BankName, 
				@BankAddress = @BankAddress, 
				@DummyAccount = @DummyAccount, 
				@CurrentAccount = @CurrentAccount, 
				@CA_SortCode = @CA_SortCode, 
				@CA_AccountNumber = @CA_AccountNumber, 
				@ReserveAccount = @ReserveAccount, 
				@RA_SortCode = @RA_SortCode, 
				@RA_AccountNumber = @RA_AccountNumber;

		--TIME PERIODS
		WHILE (@Year < DATEPART(YEAR, CURRENT_TIMESTAMP) + 2)
		BEGIN
		
			INSERT INTO App.tbYear (YearNumber, StartMonth, CashStatusCode, Description)
			VALUES (@Year, @FinancialMonth, 0, 
						CASE WHEN @FinancialMonth > 1 THEN CONCAT(@Year, '-', @Year - ROUND(@Year, -2) + 1) ELSE CONCAT(@Year, '.') END
					);
			SET @Year += 1;
		END

		EXEC Cash.proc_GeneratePeriods;

		UPDATE App.tbYearPeriod
		SET CorporationTaxRate = 0.19;

		UPDATE App.tbYearPeriod
		SET CashStatusCode = 2
		WHERE StartOn < DATEADD(MONTH, -1, CURRENT_TIMESTAMP)

		IF EXISTS(SELECT * FROM App.tbYearPeriod WHERE CashStatusCode = 3)
			WITH current_month AS
			(
				SELECT MAX(StartOn) AS StartOn
				FROM App.tbYearPeriod
				WHERE CashStatusCode = 2
			)
			UPDATE App.tbYearPeriod
			SET CashStatusCode = 1
			FROM App.tbYearPeriod JOIN current_month ON App.tbYearPeriod.StartOn = current_month.StartOn;	
		ELSE
			WITH current_month AS
			(
				SELECT MIN(StartOn) AS StartOn
				FROM App.tbYearPeriod
				WHERE CashStatusCode = 0
			)
			UPDATE App.tbYearPeriod
			SET CashStatusCode = 1
			FROM App.tbYearPeriod JOIN current_month ON App.tbYearPeriod.StartOn = current_month.StartOn;
	
	
		WITH current_month AS
		(
			SELECT YearNumber
			FROM App.tbYearPeriod
			WHERE CashStatusCode = 1
		)
		UPDATE App.tbYear
		SET CashStatusCode = 1
		FROM App.tbYear JOIN current_month ON App.tbYear.YearNumber = current_month.YearNumber;

		UPDATE App.tbYear
		SET CashStatusCode = 2
		WHERE YearNumber < 	(SELECT YearNumber FROM App.tbYear	WHERE CashStatusCode = 1);

		COMMIT TRAN
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_DocDespool]...';


go

CREATE   PROCEDURE App.proc_DocDespool
	(
	@DocTypeCode SMALLINT
	)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF @DocTypeCode = 0
		--Quotations:
			UPDATE       Project.tbProject
			SET           Spooled = 0, Printed = 1
			FROM            Project.tbProject INNER JOIN
									 Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
									 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE        ( Project.tbProject.ProjectStatusCode = 0) AND ( Cash.tbCategory.CashPolarityCode = 1) AND ( Project.tbProject.Spooled <> 0)
		ELSE IF @DocTypeCode = 1
		--SalesOrder:
			UPDATE       Project.tbProject
			SET           Spooled = 0, Printed = 1
			FROM            Project.tbProject INNER JOIN
									 Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
									 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE        ( Project.tbProject.ProjectStatusCode > 0) AND ( Cash.tbCategory.CashPolarityCode = 1) AND ( Project.tbProject.Spooled <> 0)
		ELSE IF @DocTypeCode = 2
		--PurchaseEnquiry:
			UPDATE       Project.tbProject
			SET           Spooled = 0, Printed = 1
			FROM            Project.tbProject INNER JOIN
									 Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
									 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE        ( Project.tbProject.ProjectStatusCode = 0) AND ( Cash.tbCategory.CashPolarityCode = 0) AND ( Project.tbProject.Spooled <> 0)	
		ELSE IF @DocTypeCode = 3
		--PurchaseOrder:
			UPDATE       Project.tbProject
			SET           Spooled = 0, Printed = 1
			FROM            Project.tbProject INNER JOIN
									 Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
									 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE        ( Project.tbProject.ProjectStatusCode > 0) AND ( Cash.tbCategory.CashPolarityCode = 0) AND ( Project.tbProject.Spooled <> 0)
		ELSE IF @DocTypeCode = 4
		--SalesInvoice:
			UPDATE       Invoice.tbInvoice
			SET                Spooled = 0, Printed = 1
			WHERE        (InvoiceTypeCode = 0) AND (Spooled <> 0)
		ELSE IF @DocTypeCode = 5
		--CreditNote:
			UPDATE       Invoice.tbInvoice
			SET                Spooled = 0, Printed = 1
			WHERE        (InvoiceTypeCode = 1) AND (Spooled <> 0)
		ELSE IF @DocTypeCode = 6
		--DebitNote:
			UPDATE       Invoice.tbInvoice
			SET                Spooled = 0, Printed = 1
			WHERE        (InvoiceTypeCode = 3) AND (Spooled <> 0)
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_DocDespoolAll]...';


go
CREATE   PROCEDURE App.proc_DocDespoolAll
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		BEGIN TRAN
		UPDATE Project.tbProject
		SET Spooled = 0, Printed = 1;

		UPDATE  Invoice.tbInvoice
		SET  Spooled = 0, Printed = 1;
		COMMIT TRAN
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_DelCalDateRange]...';


go

CREATE   PROCEDURE App.proc_DelCalDateRange
	(
		@CalendarCode nvarchar(10),
		@FromDate datetime,
		@ToDate datetime
	)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DELETE FROM App.tbCalendarHoliday
			WHERE UnavailableOn >= @FromDate
				AND UnavailableOn <= @ToDate
				AND CalendarCode = @CalendarCode
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_AdjustToCalendar]...';


go

CREATE   PROCEDURE App.proc_AdjustToCalendar
	(
	@SourceDate datetime,
	@OffsetDays int,
	@OutputDate datetime output
	)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@CalendarCode nvarchar(10)
			, @WorkingDay bit
			, @UserId nvarchar(10)
	
		DECLARE
			 @CurrentDay smallint
			, @Monday smallint
			, @Tuesday smallint
			, @Wednesday smallint
			, @Thursday smallint
			, @Friday smallint
			, @Saturday smallint
			, @Sunday smallint
		
		SELECT @UserId = UserId
		FROM         Usr.vwCredentials	

		SET @OutputDate = @SourceDate

		SELECT     @CalendarCode = App.tbCalendar.CalendarCode, @Monday = Monday, @Tuesday = Tuesday, @Wednesday = Wednesday, @Thursday = Thursday, @Friday = Friday, @Saturday = Saturday, @Sunday = Sunday
		FROM         App.tbCalendar INNER JOIN
							  Usr.tbUser ON App.tbCalendar.CalendarCode = Usr.tbUser.CalendarCode
		WHERE UserId = @UserId
	
		WHILE @OffsetDays > -1
			BEGIN
			SET @CurrentDay = App.fnWeekDay(@OutputDate)
			IF @CurrentDay = 1				
				SET @WorkingDay = CASE WHEN @Monday != 0 THEN 1 ELSE 0 END
			ELSE IF @CurrentDay = 2
				SET @WorkingDay = CASE WHEN @Tuesday != 0 THEN 1 ELSE 0 END
			ELSE IF @CurrentDay = 3
				SET @WorkingDay = CASE WHEN @Wednesday != 0 THEN 1 ELSE 0 END
			ELSE IF @CurrentDay = 4
				SET @WorkingDay = CASE WHEN @Thursday != 0 THEN 1 ELSE 0 END
			ELSE IF @CurrentDay = 5
				SET @WorkingDay = CASE WHEN @Friday != 0 THEN 1 ELSE 0 END
			ELSE IF @CurrentDay = 6
				SET @WorkingDay = CASE WHEN @Saturday != 0 THEN 1 ELSE 0 END
			ELSE IF @CurrentDay = 7
				SET @WorkingDay = CASE WHEN @Sunday != 0 THEN 1 ELSE 0 END
		
			IF @WorkingDay = 1
				BEGIN
				IF NOT EXISTS(SELECT     UnavailableOn
							FROM         App.tbCalendarHoliday
							WHERE     (CalendarCode = @CalendarCode) AND (UnavailableOn = @OutputDate))
					SET @OffsetDays -= 1
				END
			
			IF @OffsetDays > -1
				SET @OutputDate = DATEADD(d, -1, @OutputDate)
			END
					
		

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_AddCalDateRange]...';


go

CREATE   PROCEDURE App.proc_AddCalDateRange
	(
		@CalendarCode nvarchar(10),
		@FromDate datetime,
		@ToDate datetime
	)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @UnavailableDate datetime

		SELECT @UnavailableDate = @FromDate
	
		BEGIN TRANSACTION

		WHILE @UnavailableDate <= @ToDate
		BEGIN
			INSERT INTO App.tbCalendarHoliday (CalendarCode, UnavailableOn)
			VALUES (@CalendarCode, @UnavailableDate)
			SELECT @UnavailableDate = DateAdd(d, 1, @UnavailableDate)
		END

		COMMIT TRANSACTION

		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_PeriodClose]...';


go
CREATE PROCEDURE App.proc_PeriodClose
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT * FROM App.fnActivePeriod())
			BEGIN
			DECLARE @StartOn datetime, @YearNumber smallint

			SELECT @StartOn = StartOn, @YearNumber = YearNumber
			FROM App.fnActivePeriod() fnSystemActivePeriod
		 	
			EXEC Cash.proc_GeneratePeriods

			BEGIN TRAN

			UPDATE       Cash.tbPeriod
			SET                InvoiceValue = 0, InvoiceTax = 0
			FROM            Cash.tbPeriod 
			WHERE        (Cash.tbPeriod.StartOn = @StartOn);

			WITH invoice_entries AS
			(
				SELECT invoices.CashCode, invoices.StartOn, categories.CashPolarityCode, SUM(invoices.InvoiceValue) InvoiceValue, SUM(invoices.TaxValue) TaxValue
				FROM  Invoice.vwRegisterDetail invoices
					JOIN Cash.tbCode cash_codes ON invoices.CashCode = cash_codes.CashCode 
					JOIN Cash.tbCategory categories ON cash_codes.CategoryCode = categories .CategoryCode
				WHERE StartOn = @StartOn
				GROUP BY invoices.CashCode, invoices.StartOn, categories.CashPolarityCode
			), invoice_summary AS
			(
				SELECT CashCode, StartOn,
					CASE CashPolarityCode 
						WHEN 0 THEN
							InvoiceValue * -1
						ELSE 
							InvoiceValue
					END AS InvoiceValue,
					CASE CashPolarityCode 
						WHEN 0 THEN
							TaxValue * -1
						ELSE 
							TaxValue
					END AS TaxValue						
				FROM invoice_entries
			)
			UPDATE Cash.tbPeriod
			SET InvoiceValue = invoice_summary.InvoiceValue, 
				InvoiceTax = invoice_summary.TaxValue
			FROM    Cash.tbPeriod 
				JOIN invoice_summary 
					ON Cash.tbPeriod.CashCode = invoice_summary.CashCode AND Cash.tbPeriod.StartOn = invoice_summary.StartOn;

			WITH asset_entries AS
			(
				SELECT payment.CashCode, 
					(SELECT TOP 1 StartOn FROM App.tbYearPeriod WHERE (StartOn <= payment.PaidOn) ORDER BY StartOn DESC) AS StartOn,
					(PaidInValue - PaidOutValue) AssetValue
				FROM Cash.tbPayment payment
					JOIN Subject.tbAccount account ON payment.AccountCode = account.AccountCode
				WHERE account.AccountTypeCode = 2 AND payment.IsProfitAndLoss <> 0 AND PaidOn >= @StartOn
			), asset_summary AS
			(
				SELECT CashCode, StartOn, SUM(AssetValue) AssetValue
				FROM asset_entries
				WHERE StartOn = @StartOn
				GROUP BY CashCode, StartOn				
			)
			UPDATE Cash.tbPeriod
			SET InvoiceValue = AssetValue
			FROM  Cash.tbPeriod 
				JOIN asset_summary 
					ON Cash.tbPeriod.CashCode = asset_summary.CashCode AND Cash.tbPeriod.StartOn = asset_summary.StartOn;		
	
			UPDATE App.tbYearPeriod
			SET CashStatusCode = 2
			WHERE StartOn = @StartOn			
		
			IF NOT EXISTS (SELECT     CashStatusCode
						FROM         App.tbYearPeriod
						WHERE     (YearNumber = @YearNumber) AND (CashStatusCode < 2)) 
				BEGIN
				UPDATE App.tbYear
				SET CashStatusCode = 2
				WHERE YearNumber = @YearNumber	
				END
			IF EXISTS(SELECT * FROM App.fnActivePeriod())
				BEGIN
				UPDATE App.tbYearPeriod
				SET CashStatusCode = 1
				FROM App.fnActivePeriod() fnSystemActivePeriod INNER JOIN
									App.tbYearPeriod ON fnSystemActivePeriod.YearNumber = App.tbYearPeriod.YearNumber AND fnSystemActivePeriod.MonthNumber = App.tbYearPeriod.MonthNumber
			
				END		
			IF EXISTS(SELECT * FROM App.fnActivePeriod())
				BEGIN
				UPDATE App.tbYear
				SET CashStatusCode = 1
				FROM App.fnActivePeriod() fnSystemActivePeriod INNER JOIN
									App.tbYear ON fnSystemActivePeriod.YearNumber = App.tbYear.YearNumber  
				END

			COMMIT TRAN

			END
					
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_SystemRebuild]...';


go
CREATE PROCEDURE App.proc_SystemRebuild
AS
  	SET NOCOUNT, XACT_ABORT ON;

	DECLARE @SubjectCode nvarchar(10), @PaymentCode nvarchar(20);

	BEGIN TRY
		BEGIN TRANSACTION;

		UPDATE Project.tbFlow
		SET UsedOnQuantity = Project.Quantity / parent_Project.Quantity
		FROM            Project.tbFlow AS flow 
			JOIN Project.tbProject AS Project ON flow.ChildProjectCode = Project.ProjectCode 
			JOIN Project.tbProject AS parent_Project ON flow.ParentProjectCode = parent_Project.ProjectCode
			JOIN Cash.tbCode ON parent_Project.CashCode = Cash.tbCode.CashCode
		WHERE        (flow.UsedOnQuantity <> 0) AND (Project.Quantity <> 0) 
			AND (Project.Quantity / parent_Project.Quantity <> flow.UsedOnQuantity);

		WITH parent_Project AS
		(
			SELECT        ParentProjectCode
			FROM            Project.tbFlow flow
				JOIN Project.tbProject Project ON flow.ParentProjectCode = Project.ProjectCode
				JOIN Cash.tbCode cash ON Project.CashCode = cash.CashCode
		), Project_flow AS
		(
			SELECT        flow.ParentProjectCode, flow.StepNumber, Project.ActionOn,
					LAG(Project.ActionOn, 1, Project.ActionOn) OVER (PARTITION BY flow.ParentProjectCode ORDER BY StepNumber) AS PrevActionOn
			FROM Project.tbFlow flow
				JOIN Project.tbProject Project ON flow.ChildProjectCode = Project.ProjectCode
				JOIN parent_Project ON flow.ParentProjectCode = parent_Project.ParentProjectCode
		), step_disordered AS
		(
			SELECT ParentProjectCode 
			FROM Project_flow
			WHERE ActionOn < PrevActionOn
			GROUP BY ParentProjectCode
		), step_ordered AS
		(
			SELECT flow.ParentProjectCode, flow.ChildProjectCode,
				ROW_NUMBER() OVER (PARTITION BY flow.ParentProjectCode ORDER BY Project.ActionOn, flow.StepNumber) * 10 AS StepNumber 
			FROM step_disordered
				JOIN Project.tbFlow flow ON step_disordered.ParentProjectCode = flow.ParentProjectCode
				JOIN Project.tbProject Project ON flow.ChildProjectCode = Project.ProjectCode
		)
		UPDATE flow
		SET
			StepNumber = step_ordered.StepNumber
		FROM Project.tbFlow flow
			JOIN step_ordered ON flow.ParentProjectCode = step_ordered.ParentProjectCode AND flow.ChildProjectCode = step_ordered.ChildProjectCode;

		--invoices	
		UPDATE Invoice.tbItem
		SET 
			InvoiceValue =  ROUND(Invoice.tbItem.TotalValue / (1 + App.tbTaxCode.TaxRate), Decimals)
		FROM         Invoice.tbItem INNER JOIN
								App.tbTaxCode ON Invoice.tbItem.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
								Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode <> 0) AND Invoice.tbItem.TotalValue <> 0;

		UPDATE Invoice.tbItem
		SET TaxValue = CASE App.tbTaxCode.RoundingCode 
				WHEN 0 THEN ROUND(Invoice.tbItem.InvoiceValue * App.tbTaxCode.TaxRate, Decimals)
				WHEN 1 THEN ROUND( Invoice.tbItem.InvoiceValue * App.tbTaxCode.TaxRate, Decimals, 1) END
		FROM         Invoice.tbItem INNER JOIN
								App.tbTaxCode ON Invoice.tbItem.TaxCode = App.tbTaxCode.TaxCode 
								INNER JOIN Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode <> 0);
                   
		UPDATE Invoice.tbProject
		SET InvoiceValue =  ROUND(Invoice.tbProject.TotalValue / (1 + App.tbTaxCode.TaxRate), Decimals)
		FROM         Invoice.tbProject INNER JOIN
								App.tbTaxCode ON Invoice.tbProject.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
								Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode <> 0) AND Invoice.tbProject.TotalValue <> 0;

		UPDATE Invoice.tbProject
		SET TaxValue = CASE App.tbTaxCode.RoundingCode 
				WHEN 0 THEN ROUND(Invoice.tbProject.InvoiceValue * App.tbTaxCode.TaxRate, Decimals)
				WHEN 1 THEN ROUND( Invoice.tbProject.InvoiceValue * App.tbTaxCode.TaxRate, Decimals, 1) END,
			InvoiceValue = CASE WHEN Invoice.tbProject.TotalValue = 0 
								THEN Invoice.tbProject.InvoiceValue 
								ELSE ROUND(Invoice.tbProject.TotalValue / (1 + App.tbTaxCode.TaxRate), Decimals) 
							END
		FROM         Invoice.tbProject INNER JOIN
								App.tbTaxCode ON Invoice.tbProject.TaxCode = App.tbTaxCode.TaxCode 
								INNER JOIN Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode <> 0);
						   	
	
		WITH items AS
		(
			SELECT     Invoice.tbInvoice.InvoiceNumber, SUM( Invoice.tbItem.InvoiceValue) AS TotalInvoiceValue, SUM( Invoice.tbItem.TaxValue) AS TotalTaxValue
			FROM         Invoice.tbItem INNER JOIN
								Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
			GROUP BY Invoice.tbInvoice.InvoiceNumber
		), Projects AS
		(
			SELECT     Invoice.tbInvoice.InvoiceNumber, SUM( Invoice.tbProject.InvoiceValue) AS TotalInvoiceValue, SUM( Invoice.tbProject.TaxValue) AS TotalTaxValue
			FROM         Invoice.tbProject INNER JOIN
								Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
			GROUP BY Invoice.tbInvoice.InvoiceNumber
		), invoice_totals AS
		(
			SELECT invoices.InvoiceNumber, 
				COALESCE(items.TotalInvoiceValue, 0) + COALESCE(Projects.TotalInvoiceValue, 0) TotalInvoiceValue,
				COALESCE(items.TotalTaxValue, 0) + COALESCE(Projects.TotalTaxValue, 0) TotalTaxValue
			FROM Invoice.tbInvoice invoices
				LEFT OUTER JOIN Projects ON invoices.InvoiceNumber = Projects.InvoiceNumber
				LEFT OUTER JOIN items ON invoices.InvoiceNumber = items.InvoiceNumber
			WHERE ( invoices.InvoiceStatusCode > 0)
		)
		UPDATE invoices
		SET InvoiceValue = TotalInvoiceValue, 
			TaxValue = TotalTaxValue
		FROM  Invoice.tbInvoice invoices 
			JOIN invoice_totals ON invoices.InvoiceNumber = invoice_totals.InvoiceNumber
		WHERE (InvoiceValue <> TotalInvoiceValue OR TaxValue <> TotalTaxValue);

		WITH invoice_status AS
		(
			SELECT InvoiceNumber, InvoiceStatusCode, PaidValue, PaidTaxValue
			FROM Invoice.vwStatusLive
		)
		UPDATE invoices
		SET 
			InvoiceStatusCode = invoice_status.InvoiceStatusCode,
			PaidValue = invoice_status.PaidValue,
			PaidTaxValue = invoice_status.PaidTaxValue
		FROM Invoice.tbInvoice invoices	
			JOIN invoice_status ON invoices.InvoiceNumber = invoice_status.InvoiceNumber
		WHERE 
			invoices.InvoiceStatusCode <> invoice_status.InvoiceStatusCode 
			OR invoices.PaidValue <> invoice_status.PaidValue 
			OR invoices.PaidTaxValue <> invoice_status.PaidTaxValue;		
		--cash accounts
		UPDATE Subject.tbAccount
		SET CurrentBalance = Cash.vwAccountRebuild.CurrentBalance
		FROM         Cash.vwAccountRebuild INNER JOIN
							Subject.tbAccount ON Cash.vwAccountRebuild.AccountCode = Subject.tbAccount.AccountCode;
	
		UPDATE Subject.tbAccount
		SET CurrentBalance = Subject.tbAccount.OpeningBalance
		FROM         Cash.vwAccountRebuild RIGHT OUTER JOIN
							  Subject.tbAccount ON Cash.vwAccountRebuild.AccountCode = Subject.tbAccount.AccountCode
		WHERE     (Cash.vwAccountRebuild.AccountCode IS NULL);

		EXEC Cash.proc_GeneratePeriods;
	            
		COMMIT TRANSACTION

		DECLARE @Msg NVARCHAR(MAX);
		SELECT @Msg = Message FROM App.tbText WHERE TextId = 3006;
		EXEC App.proc_EventLog @EventMessage = @Msg, @EventTypeCode = 2;

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_DemoServices]...';


go
CREATE PROCEDURE App.proc_DemoServices
(
	@CreateOrders BIT = 0,
	@InvoiceOrders BIT = 0,
	@PayInvoices BIT = 0
)
AS
	 SET NOCOUNT, XACT_ABORT ON;

	 BEGIN TRY
	
		IF NOT EXISTS (SELECT * FROM Usr.vwCredentials WHERE IsAdministrator <> 0)
		BEGIN
			DECLARE @Msg NVARCHAR(100) = CONCAT('Access Denied: User ', SUSER_SNAME(), ' is not an administrsator');
			RAISERROR ('%s', 13, 1, @Msg);
		END
				
		BEGIN TRAN

		-->>>>>>>>>>>>> RESET >>>>>>>>>>>>>>>>>>>>>>>>>>>
		DELETE FROM Cash.tbPayment;
		DELETE FROM Invoice.tbInvoice;
		DELETE FROM Project.tbFlow;
		DELETE FROM Project.tbProject;
		DELETE FROM Object.tbFlow;
		DELETE FROM Object.tbObject;

		WITH sys_accounts AS
		(
			SELECT SubjectCode FROM App.tbOptions
			UNION
			SELECT DISTINCT SubjectCode FROM Subject.tbAccount
			UNION
			SELECT DISTINCT SubjectCode FROM Cash.tbTaxType
			UNION
			SELECT MinerAccountCode FROM App.tbOptions opt JOIN Subject.tbSubject miner ON opt.MinerAccountCode = miner.SubjectCode
		), candidates AS
		(
			SELECT SubjectCode
			FROM Subject.tbSubject
			EXCEPT
			SELECT SubjectCode 
			FROM sys_accounts
		)
		DELETE Subject.tbSubject 
		FROM Subject.tbSubject JOIN candidates ON Subject.tbSubject.SubjectCode = candidates.SubjectCode;

		UPDATE App.tbOptions
		SET IsAutoOffsetDays = 0;

		EXEC App.proc_SystemRebuild;		
		--<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

		DECLARE @UserId NVARCHAR(10) = (SELECT UserId FROM Usr.vwCredentials);

		UPDATE App.tbYear SET CashStatusCode = 2 WHERE CashStatusCode = 3;
		UPDATE App.tbYearPeriod SET CashStatusCode = 2 WHERE CashStatusCode = 3;

		INSERT INTO App.tbRegister (RegisterName, NextNumber)
		SELECT 'Dividends', (SELECT MAX(NextNumber) + 10000 FROM App.tbRegister)
		WHERE NOT EXISTS (SELECT * FROM App.tbRegister WHERE RegisterName = 'Dividends');

		INSERT INTO Object.tbObject (ObjectCode, ProjectStatusCode, ObjectDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
		VALUES ('Car Parking / Tolls', 3, '', 'each', '213', 0.0000, 0, 'Expenses')
		, ('Communications monthly charge', 3, '', 'each', '202', 0.0000, 0, 'Expenses')
		, ('Company Administration', 3, '', 'each', '201', 0.0000, 0, 'Expenses')
		, ('Directors Dividend Accrual', 2, '', 'each', '401', 0.0000, 0, 'Dividends')
		, ('Employee Transport', 3, '', 'miles', '212', 0.4500, 0, 'Expenses')
		, ('Mobile phone charges', 3, '', 'each', '202', 0.0000, 0, 'Expenses')
		, ('Office Equipment', 3, '', 'each', '204', 0.0000, 0, 'Expenses')
		, ('Office Rent', 3, '', 'each', '205', 0.0000, 0, 'Expenses')
		, ('PO Book', 1, '', 'copies', '200', 0.0000, 1, 'Purchase Order')
		, ('PO Brochure or Catalogue', 1, '', 'copies', '200', 0.0000, 1, 'Purchase Order')
		, ('PO Card', 1, '', 'copies', '200', 0.0000, 1, 'Purchase Order')
		, ('PO Design', 1, '', 'each', '200', 0.0000, 1, 'Purchase Order')
		, ('PO Finishing', 1, '', 'copies', '200', 0.0000, 1, 'Purchase Order')
		, ('PO Leaflet', 1, '', 'copies', '200', 0.0000, 1, 'Purchase Order')
		, ('PO Packaging', 1, '', 'copies', '200', 0.0000, 1, 'Purchase Order')
		, ('PO POS', 1, '', 'copies', '200', 0.0000, 1, 'Purchase Order')
		, ('PO Poster', 1, '', 'copies', '200', 0.0000, 1, 'Purchase Order')
		, ('PO Promotional', 1, '', 'copies', '200', 0.0000, 1, 'Purchase Order')
		, ('PO Stationery', 1, '', 'copies', '200', 0.0000, 1, 'Purchase Order')
		, ('PO Transport', 1, '', 'each', '200', 0.0000, 1, 'Purchase Order')
		, ('Postage', 3, '', 'each', '207', 0.0000, 0, 'Expenses')
		, ('Project', 1, '', 'each', null, 0.0000, 0, 'Project')
		, ('SO Book', 1, '', 'copies', '103', 0.0000, 1, 'Sales Order')
		, ('SO Brochure or Catalogue', 1, '', 'copies', '103', 0.0000, 1, 'Sales Order')
		, ('SO Card', 1, '', 'copies', '103', 0.0000, 1, 'Sales Order')
		, ('SO Consultancy', 1, '', 'days', '104', 0.0000, 1, 'Sales Order')
		, ('SO Design', 1, '', 'each', '103', 0.0000, 1, 'Sales Order')
		, ('SO Leaflet', 1, '', 'copies', '103', 0.0000, 1, 'Sales Order')
		, ('SO Mailing and Fulfilment', 1, '', 'copies', '103', 0.0000, 1, 'Sales Order')
		, ('SO Packaging', 1, '', 'copies', '103', 0.0000, 1, 'Sales Order')
		, ('SO POS', 1, '', 'copies', '103', 0.0000, 1, 'Sales Order')
		, ('SO Promotional', 1, '', 'copies', '103', 0.0000, 1, 'Sales Order')
		, ('SO Stationery', 1, '', 'copies', '103', 0.0000, 1, 'Sales Order')
		, ('SO Transport', 1, '', 'each', '103', 0.0000, 1, 'Sales Order')
		, ('Stationery - General', 3, '', 'each', '209', 0.0000, 0, 'Expenses')
		, ('Stationery - Office Printer Paper', 3, '', 'each', '209', 0.0000, 0, 'Expenses')
		, ('Subsistence', 3, '', 'each', '218', 0.0000, 0, 'Expenses')
		, ('Sundry (Indirect)', 3, '', 'each', '208', 0.0000, 0, 'Expenses')
		, ('Train/Tube fares', 3, '', 'each', '213', 0.0000, 0, 'Expenses')
		, ('Travel (Flights etc)', 3, '', 'each', '213', 0.0000, 0, 'Expenses')
		, ('Wages monthly payment', 2, '', 'each', '402', 0.0000, 0, 'Expenses')
		;
		INSERT INTO Object.tbAttribute (ObjectCode, Attribute, PrintOrder, AttributeTypeCode, DefaultText)
		VALUES ('PO Book', 'Extent', 20, 0, '')
		, ('PO Book', 'Finishing', 70, 0, '')
		, ('PO Book', 'Origination', 30, 0, '')
		, ('PO Book', 'Packing', 80, 0, '')
		, ('PO Book', 'Paper', 60, 0, '')
		, ('PO Book', 'Printing', 50, 0, '')
		, ('PO Book', 'Proofs', 40, 0, '')
		, ('PO Book', 'Text Size', 10, 0, '')
		, ('PO Brochure or Catalogue', 'Delivery #1', 155, 0, '')
		, ('PO Brochure or Catalogue', 'File Copies', 160, 0, '')
		, ('PO Brochure or Catalogue', 'Finishing', 90, 0, '')
		, ('PO Brochure or Catalogue', 'Note', 150, 0, '')
		, ('PO Brochure or Catalogue', 'Origination', 40, 0, '')
		, ('PO Brochure or Catalogue', 'Packing', 100, 0, '')
		, ('PO Brochure or Catalogue', 'Pagination', 20, 0, '')
		, ('PO Brochure or Catalogue', 'Paper', 80, 0, '')
		, ('PO Brochure or Catalogue', 'Printing', 60, 0, '')
		, ('PO Brochure or Catalogue', 'Proofing', 50, 0, '')
		, ('PO Brochure or Catalogue', 'Trim Size', 30, 0, '')
		, ('PO Brochure or Catalogue', 'UV Varnish', 70, 0, '')
		, ('PO Card', 'File Copies', 200, 0, '')
		, ('PO Card', 'Finishing', 90, 0, '')
		, ('PO Card', 'Origination', 40, 0, '')
		, ('PO Card', 'Packing', 100, 0, '')
		, ('PO Card', 'Pagination', 20, 0, '')
		, ('PO Card', 'Paper', 80, 0, '')
		, ('PO Card', 'Printing', 60, 0, '')
		, ('PO Card', 'Proofing', 50, 0, '')
		, ('PO Card', 'Trim Size', 30, 0, '')
		, ('PO Card', 'Versions', 10, 0, '')
		, ('PO Design', 'Autojoy', 10, 0, '')
		, ('PO Design', 'RVS', 30, 0, '')
		, ('PO Design', 'WVS', 20, 0, '')
		, ('PO Finishing', 'Advance sample', 210, 1, '')
		, ('PO Finishing', 'Extent', 20, 0, '')
		, ('PO Finishing', 'File Copies', 200, 0, '')
		, ('PO Finishing', 'Finishing', 100, 0, '')
		, ('PO Finishing', 'Paper', 70, 0, '')
		, ('PO Finishing', 'Printing', 60, 0, '')
		, ('PO Finishing', 'Size', 30, 0, '')
		, ('PO Leaflet', 'File Copies', 120, 0, '')
		, ('PO Leaflet', 'Finishing', 90, 0, '')
		, ('PO Leaflet', 'Labelling', 110, 0, '')
		, ('PO Leaflet', 'Lamination', 70, 0, '')
		, ('PO Leaflet', 'Origination', 40, 0, '')
		, ('PO Leaflet', 'Packing', 100, 0, '')
		, ('PO Leaflet', 'Pagination', 20, 0, '')
		, ('PO Leaflet', 'Paper', 80, 0, '')
		, ('PO Leaflet', 'Printing', 60, 0, '')
		, ('PO Leaflet', 'Proofing', 50, 0, '')
		, ('PO Leaflet', 'Trim Size', 30, 0, '')
		, ('PO Packaging', '10 Litre labels', 20, 0, '')
		, ('PO Packaging', '5 Litre labels', 15, 0, '')
		, ('PO Packaging', 'File Copies', 100, 0, '')
		, ('PO Packaging', 'Finishing', 80, 0, '')
		, ('PO Packaging', 'Material', 60, 0, '')
		, ('PO Packaging', 'Origination', 30, 0, '')
		, ('PO Packaging', 'Packing', 90, 0, '')
		, ('PO Packaging', 'Printing', 50, 0, '')
		, ('PO Packaging', 'Proofing', 40, 0, '')
		, ('PO Packaging', 'Size', 25, 0, '')
		, ('PO POS', 'File Copies', 70, 0, '')
		, ('PO POS', 'Finishing', 60, 0, '')
		, ('PO POS', 'Origination', 20, 0, '')
		, ('PO POS', 'Paper', 50, 0, '')
		, ('PO POS', 'Printing', 40, 0, '')
		, ('PO POS', 'Proofing', 30, 0, '')
		, ('PO POS', 'Size', 10, 0, '')
		, ('PO Poster', 'Extent', 20, 0, '')
		, ('PO Poster', 'File Copies', 90, 0, '')
		, ('PO Poster', 'Finishing', 70, 0, '')
		, ('PO Poster', 'Flat sheets', 50, 0, '')
		, ('PO Poster', 'Packing', 80, 0, '')
		, ('PO Poster', 'Paper', 60, 0, '')
		, ('PO Poster', 'Size', 10, 0, '')
		, ('PO Promotional', 'Delivery Note', 90, 0, '')
		, ('PO Promotional', 'Description', 10, 0, '')
		, ('PO Promotional', 'File Copies', 100, 0, '')
		, ('PO Promotional', 'Finishing', 70, 0, '')
		, ('PO Promotional', 'Material', 60, 0, '')
		, ('PO Promotional', 'Origination', 30, 0, '')
		, ('PO Promotional', 'Packing', 80, 0, '')
		, ('PO Promotional', 'Printing', 50, 0, '')
		, ('PO Promotional', 'Proofing', 40, 0, '')
		, ('PO Promotional', 'Size', 20, 0, '')
		, ('PO Stationery', 'File Copies', 110, 0, '')
		, ('PO Stationery', 'Finishing', 90, 0, '')
		, ('PO Stationery', 'Lamination', 70, 0, '')
		, ('PO Stationery', 'Material', 80, 0, '')
		, ('PO Stationery', 'Origination', 40, 0, '')
		, ('PO Stationery', 'Packing', 100, 0, '')
		, ('PO Stationery', 'Prices', 20, 0, '')
		, ('PO Stationery', 'Printing', 60, 0, '')
		, ('PO Stationery', 'Proofing', 50, 0, '')
		, ('PO Stationery', 'Qty Splits', 10, 0, '')
		, ('PO Stationery', 'Trim Sizes', 30, 0, '')
		, ('PO Transport', 'Collection', 20, 0, '')
		, ('PO Transport', 'Description', 10, 0, '')
		, ('PO Transport', 'Note', 30, 1, '')
		, ('SO Book', 'Binder Size', 15, 0, '')
		, ('SO Book', 'Extent', 20, 0, '')
		, ('SO Book', 'Finishing', 70, 0, '')
		, ('SO Book', 'Origination', 30, 0, '')
		, ('SO Book', 'Packing', 80, 0, '')
		, ('SO Book', 'Paper', 60, 0, '')
		, ('SO Book', 'Printing', 50, 0, '')
		, ('SO Book', 'Proofs', 40, 0, '')
		, ('SO Book', 'Ring Binder', 75, 0, '')
		, ('SO Book', 'Text Size', 10, 0, '')
		, ('SO Brochure or Catalogue', 'Delivery #1', 160, 0, '')
		, ('SO Brochure or Catalogue', 'Finishing', 90, 0, '')
		, ('SO Brochure or Catalogue', 'Note', 150, 0, '')
		, ('SO Brochure or Catalogue', 'Origination', 40, 0, '')
		, ('SO Brochure or Catalogue', 'Packing', 100, 0, '')
		, ('SO Brochure or Catalogue', 'Pagination', 20, 0, '')
		, ('SO Brochure or Catalogue', 'Paper', 80, 0, '')
		, ('SO Brochure or Catalogue', 'Printing', 60, 0, '')
		, ('SO Brochure or Catalogue', 'Proofing', 50, 0, '')
		, ('SO Brochure or Catalogue', 'Trim Size', 30, 0, '')
		, ('SO Brochure or Catalogue', 'UV Varnish', 70, 0, '')
		, ('SO Card', 'Changes', 70, 0, '')
		, ('SO Card', 'Envelopes', 110, 1, '')
		, ('SO Card', 'Finishing', 90, 0, '')
		, ('SO Card', 'Origination', 40, 0, '')
		, ('SO Card', 'Pagination', 20, 0, '')
		, ('SO Card', 'Paper', 80, 0, '')
		, ('SO Card', 'Printing', 60, 0, '')
		, ('SO Card', 'Proofing', 50, 0, '')
		, ('SO Card', 'Trim Size', 30, 0, '')
		, ('SO Consultancy', 'Description', 10, 0, '')
		, ('SO Design', 'Autojoy', 10, 0, '')
		, ('SO Design', 'RVS', 30, 0, '')
		, ('SO Design', 'WVS', 20, 0, '')
		, ('SO Leaflet', 'Extent', 20, 0, '')
		, ('SO Leaflet', 'Finishing', 70, 0, '')
		, ('SO Leaflet', 'Origination', 30, 0, '')
		, ('SO Leaflet', 'Packing', 80, 0, '')
		, ('SO Leaflet', 'Paper', 60, 0, '')
		, ('SO Leaflet', 'Printing', 50, 0, '')
		, ('SO Leaflet', 'Proofing', 40, 0, '')
		, ('SO Leaflet', 'Size', 10, 0, '')
		, ('SO Mailing and Fulfilment', 'Call-off #1', 40, 0, '')
		, ('SO Mailing and Fulfilment', 'Call-off #2', 50, 0, '')
		, ('SO Mailing and Fulfilment', 'Call-off #3', 60, 0, '')
		, ('SO Mailing and Fulfilment', 'Call-off #4', 70, 0, '')
		, ('SO Mailing and Fulfilment', 'Call-off #5', 80, 0, '')
		, ('SO Mailing and Fulfilment', 'Call-off #6', 90, 0, '')
		, ('SO Mailing and Fulfilment', 'Call-off #7', 100, 0, '')
		, ('SO Mailing and Fulfilment', 'Call-off #8', 110, 0, '')
		, ('SO Mailing and Fulfilment', 'Scale prices', 5, 0, '')
		, ('SO Mailing and Fulfilment', 'Storage', 30, 0, '')
		, ('SO Packaging', 'Description', 10, 0, '')
		, ('SO Packaging', 'Finishing', 90, 0, '')
		, ('SO Packaging', 'Lamination', 70, 0, '')
		, ('SO Packaging', 'Material', 80, 0, '')
		, ('SO Packaging', 'Origination', 40, 0, '')
		, ('SO Packaging', 'Packing', 100, 0, '')
		, ('SO Packaging', 'Printing', 60, 0, '')
		, ('SO Packaging', 'Proofing', 50, 0, '')
		, ('SO Packaging', 'Tolerance', 110, 0, '')
		, ('SO Packaging', 'Trim Size', 30, 0, '')
		, ('SO POS', 'Finishing', 60, 0, '')
		, ('SO POS', 'Origination', 20, 0, '')
		, ('SO POS', 'Paper', 50, 0, '')
		, ('SO POS', 'Printing', 40, 0, '')
		, ('SO POS', 'Proofing', 30, 0, '')
		, ('SO POS', 'Size', 10, 0, '')
		, ('SO Promotional', 'Description', 60, 0, '')
		, ('SO Promotional', 'Embroidery', 140, 0, '')
		, ('SO Promotional', 'FOTL Mens Polo', 100, 0, '')
		, ('SO Promotional', 'Gildan Mens Polo', 80, 0, '')
		, ('SO Promotional', 'Henbury Mens Polo', 110, 0, '')
		, ('SO Promotional', 'Note', 150, 0, '')
		, ('SO Promotional', 'Purple Womans T', 70, 0, '')
		, ('SO Promotional', 'Result Fleece', 130, 0, '')
		, ('SO Promotional', 'Uneek Mens Polo', 90, 0, '')
		, ('SO Promotional', 'Womens Polo', 120, 0, '')
		, ('SO Stationery', 'Finishing', 90, 0, '')
		, ('SO Stationery', 'Origination', 40, 0, '')
		, ('SO Stationery', 'Packing', 100, 0, '')
		, ('SO Stationery', 'Pagination', 20, 0, '')
		, ('SO Stationery', 'Paper', 80, 0, '')
		, ('SO Stationery', 'Printing', 60, 0, '')
		, ('SO Stationery', 'Proofing', 50, 0, '')
		, ('SO Stationery', 'Trim Size', 30, 0, '')
		, ('SO Transport', 'Call-off #1', 40, 0, '')
		, ('SO Transport', 'Call-off #2', 50, 0, '')
		, ('SO Transport', 'Scale prices', 5, 0, '')
		, ('SO Transport', 'Storage', 30, 0, '')
		;
		INSERT INTO Object.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
		VALUES ('SO Book', 10, 'PO Book', 0, 0, 0)
		, ('SO Book', 20, 'PO Transport', 0, 0, 0)
		, ('SO Brochure or Catalogue', 10, 'PO Brochure or Catalogue', 0, 0, 0)
		, ('SO Brochure or Catalogue', 20, 'PO Transport', 0, 0, 0)
		, ('SO Card', 20, 'PO Card', 0, 0, 0)
		, ('SO Card', 10, 'PO Design', 0, 0, 0)
		, ('SO Design', 10, 'PO Design', 0, 0, 0)
		, ('SO Leaflet', 10, 'PO Leaflet', 0, 0, 0)
		, ('SO Leaflet', 20, 'PO Poster', 0, 0, 0)
		, ('SO Packaging', 20, 'PO Design', 0, 0, 0)
		, ('SO Packaging', 10, 'PO Packaging', 0, 0, 0)
		, ('SO POS', 10, 'PO POS', 0, 0, 0)
		, ('SO Promotional', 10, 'PO Card', 0, 0, 0)
		, ('SO Stationery', 10, 'PO Stationery', 0, 0, 0)
		, ('SO Transport', 10, 'PO Transport', 0, 0, 0)
		;
		INSERT INTO Object.tbOp (ObjectCode, OperationNumber, SyncTypeCode, Operation, Duration, OffsetDays)
		VALUES ('PO Book', 10, 0, 'Artwork', 0, 0)
		, ('PO Book', 20, 0, 'Proofs', 0, 0)
		, ('PO Book', 30, 0, 'Approval', 0, 0)
		, ('PO Book', 40, 2, 'Delivery', 0, 0)
		, ('PO Brochure or Catalogue', 10, 0, 'Artwork', 0, 0)
		, ('PO Brochure or Catalogue', 20, 0, 'Proofs', 0, 0)
		, ('PO Brochure or Catalogue', 30, 0, 'Approval', 0, 0)
		, ('PO Brochure or Catalogue', 50, 2, 'Delivery', 0, 0)
		, ('PO Card', 10, 0, 'Artwork', 0, 0)
		, ('PO Card', 20, 0, 'Proofs', 0, 0)
		, ('PO Card', 30, 0, 'Approval', 0, 0)
		, ('PO Card', 40, 2, 'Delivery', 0, 0)
		, ('PO Design', 10, 0, 'Completion', 0, 0)
		, ('PO Finishing', 10, 0, 'Advance sample', 0, 0)
		, ('PO Finishing', 20, 0, 'Flat sheets', 0, 0)
		, ('PO Finishing', 30, 2, 'Delivery', 0, 0)
		, ('PO Leaflet', 10, 0, 'Artwork', 0, 0)
		, ('PO Leaflet', 20, 0, 'Proofs', 0, 0)
		, ('PO Leaflet', 30, 0, 'Approval', 0, 0)
		, ('PO Leaflet', 40, 2, 'Delivery', 0, 0)
		, ('PO Packaging', 10, 0, 'Flat sheets', 0, 0)
		, ('PO Packaging', 20, 2, 'Delivery', 0, 0)
		, ('PO Poster', 30, 0, 'Flat sheets', 0, 0)
		, ('PO Poster', 40, 2, 'Delivery', 0, 0)
		, ('PO Promotional', 10, 2, 'Delivery', 0, 0)
		, ('PO Transport', 10, 0, 'Despatch', 0, 0)
		, ('PO Transport', 20, 2, 'Delivery', 0, 0)
		, ('SO Book', 20, 0, 'Artwork', 0, 0)
		, ('SO Book', 30, 0, 'Proofs', 0, 0)
		, ('SO Book', 40, 0, 'Approval', 0, 0)
		, ('SO Book', 70, 2, 'Delivery', 0, 0)
		, ('SO Brochure or Catalogue', 10, 0, 'Artwork', 0, 0)
		, ('SO Brochure or Catalogue', 20, 0, 'Proofs', 0, 2)
		, ('SO Brochure or Catalogue', 30, 0, 'Approval', 0, 3)
		, ('SO Brochure or Catalogue', 40, 2, 'Delivery', 0, 5)
		, ('SO Card', 10, 0, 'Artwork', 0, 0)
		, ('SO Card', 20, 0, 'Proofs', 0, 2)
		, ('SO Card', 30, 0, 'Approval', 0, 3)
		, ('SO Card', 40, 2, 'Delivery', 0, 5)
		, ('SO Design', 10, 0, 'Completion', 0, 0)
		, ('SO Leaflet', 40, 0, 'Artwork', 0, 0)
		, ('SO Leaflet', 60, 0, 'PDF Proofs', 0, 0)
		, ('SO Leaflet', 70, 0, 'Approval', 0, 0)
		, ('SO Leaflet', 80, 2, 'Delivery', 0, 0)
		, ('SO Mailing and Fulfilment', 10, 2, 'Completion', 0, 0)
		, ('SO Packaging', 10, 0, 'Artwork', 0, 0)
		, ('SO Packaging', 20, 0, 'Proofs', 0, 0)
		, ('SO Packaging', 30, 0, 'Approval', 0, 0)
		, ('SO Packaging', 40, 2, 'Delivery', 0, 5)
		, ('SO POS', 40, 2, 'Delivery', 0, 0)
		, ('SO Promotional', 10, 0, 'Copy', 0, 0)
		, ('SO Promotional', 20, 0, 'Proofs', 0, 0)
		, ('SO Promotional', 30, 0, 'Approval', 0, 0)
		, ('SO Promotional', 40, 2, 'Delivery', 0, 0)
		, ('SO Stationery', 10, 0, 'Proofs', 0, 0)
		, ('SO Stationery', 20, 0, 'Approval', 0, 0)
		, ('SO Stationery', 40, 2, 'Delivery', 0, 5)
		, ('SO Transport', 10, 0, 'Despatch', 0, 0)
		, ('SO Transport', 20, 2, 'Delivery', 0, 0)
		, ('Stationery - General', 10, 0, 'Artwork', 0, 0)
		, ('Stationery - General', 20, 0, 'Proofs', 0, 2)
		, ('Stationery - General', 30, 0, 'Approval', 0, 3)
		, ('Stationery - General', 40, 2, 'Delivery', 0, 5)
		;

		IF (@CreateOrders = 0)
			GOTO CommitTran;

		INSERT INTO Subject.tbSubject (SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode, TaxCode, AddressCode, AreaCode, PhoneNumber, EmailAddress, WebSite, SubjectSource, PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance)
		VALUES ('ABCUST', 'AB Customer', 1, 1, 'T1', 'ABCUST_001', null, '+1234 56789', 'email@abcus.com', null, null, '30 days from date of invoice', 0, 30, 0, 0)
		, ('CDCUST', 'CD Customer', 1, 1, 'T0', 'CDCUST_001', null, '+1234 123456', 'admin@cdcus.com', 'www.cdcus.com#http://www.cdcus.com#', null, '30 days end of month following date of invoice', 0, 30, 1, 0)
		, ('EFCUST', 'EF Customer', 1, 1, 'T0', 'EFCUST_001', null, '01234 654321', 'accounts@efcust.net', 'www.efcust.net#http://www.efcust.net#', null, '30 days from date of invoice', 15, 30, 0, 1)
		, ('SUPONE', 'Supplier One Ltd', 8, 1, 'T1', 'SUPONE_001', null, '0102 030405', 'contact@supplierone.co.uk', null, null, '30 days end of month following date of invoice', 0, 30, 1, 0)
		, ('EXWORK', 'Ex Works', 7, 1, 'T0', 'EXWORK_001', null, null, null, null, null, null, 0, 0, 0, 1)
		, ('TRACOM', 'Transport Company Ltd', 0, 1, 'T1', 'TRACOM_001', null, '01112 333444', 'bookings@transportco.biz', 'www.transportco.biz#http://www.transportco.biz#', null, '30 days end of month following date of invoice', 0, 30, 1, 0)
		, ('BUSOWN', 'Business Owner', 9, 1, 'T0', null, null, null, null, null, null, 'Expenses paid end of month', 0, 0, 1, 1)
		, ('TELPRO', 'Telecom Provider', 0, 1, 'T1', null, null, '09876 54312', null, null, null, 'Paid with order', 0, 0, 0, 0)
		, ('SUNSUP', 'Sundry Supplier', 1, 1, 'T0', null, null, null, null, null, null, 'Paid with order', 0, 0, 0, 1)
		, ('SUPTWO', 'Supplier Two', 8, 1, 'T0', 'SUPTWO_001', null, '0987 454545', 'info@suptwo.com', null, null, '30 days end of month following date of invoice', 0, 30, 1, 0)
		, ('SUPTHR', 'Supplier Three Cartons Ltd', 0, 1, 'T1', 'SUPTHR_001', null, '0505 505050', 'sales@supplierthree.ltd', null, null, '30 days end of month following date of invoice', 0, 30, 1, 0)
		, ('THEPAP', 'The Paper Supplier', 8, 1, 'T1', 'THEPAP_001', null, '01254 400000', 'adam@papersupplier.eu', 'www.papersupplier.eu#http://www.papersupplier.eu#', null, '30 days from date of invoice', 30, 0, 0, 1)
		, ('BRICRA', 'British Crafts', 1, 1, 'T0', 'BRICRA_001', null, '1234 987654', 'ed@britishcrafts.Subject.uk', null, null, '30 days end of month following date of invoice', 10, 30, 1, 1)
		;
		INSERT INTO Subject.tbAddress (AddressCode, SubjectCode, Address)
		VALUES ('ABCUST_001', 'ABCUST', '1 The Street
		Anytown
		AT1 100')
		, ('ABCUST_002', 'ABCUST', 'AB Customer, 1 The Street, Anytown AT1 100 Contact: Andy Brass  T:07177 897897')
		, ('BRICRA_001', 'BRICRA', 'The Farm
		Farmtown
		FM1 1AA')
		, ('BRICRA_002', 'BRICRA', 'British Crafts, The Farm, Farmtown FM1 1AA Contact: Ed Shire M:07854 00001')
		, ('CDCUST_001', 'CDCUST', '1 The Avenue
		Othertown
		OT1 100')
		, ('CDCUST_002', 'CDCUST', 'CD Customer, 1 The Avenue, Othertown, OT1 100 Attn. Ben Boyd Tel:+1234 123456')
		, ('EFCUST_001', 'EFCUST', '9 The Road
		Greentown
		GT1 2AR')
		, ('EFCUST_002', 'EFCUST', 'EF Customer, 9 The Road, Greentown GT1 2AR')
		, ('EXWORK_001', 'EXWORK', 'Ex Works - carriage cost extra if required')
		, ('SUPONE_001', 'SUPONE', 'Palm Close
		Forest Trading Estate
		Treetown
		TT1 1TT')
		, ('SUPONE_002', 'SUPONE', 'Supplier One Ltd, Palm Close, Forest Trading Estate, Treetown TT1 1TT Tel:0102 030405 (deliveries/pickups only accepted between 8am-4pm Monday-Friday)')
		, ('SUPTHR_001', 'SUPTHR', 'Acacia Avenue
		Brownton
		BR1 098')
		, ('SUPTHR_002', 'SUPTHR', 'Acacia Avenue, Brownton BR1 098 Attn. Goods-In Supervisor T:0505 505050')
		, ('SUPTWO_001', 'SUPTWO', 'The Trading Centre
		High Street
		Nothiston
		NO1 1NO')
		, ('SUPTWO_002', 'SUPTWO', 'Supplier Two, The Trading Centre, High Street, Nothiston NO1 1NO')
		, ('THEPAP_001', 'THEPAP', 'Paper House
		Paper Mill Lane
		Stoneleigh
		ST1 1PP')
		, ('TRACOM_001', 'TRACOM', 'The Transport Company
		Haulage Way
		ThisTown
		ThatCounty
		TT1 1CC')
		;
		INSERT INTO Subject.tbContact (SubjectCode, ContactName, FileAs, OnMailingList, NameTitle, NickName, JobTitle, PhoneNumber, MobileNumber, EmailAddress)
		VALUES ('ABCUST', 'Andy Brass', 'Brass, Andy', 1, null, 'Andy', null, null, '07177 897897', 'andy@abcus.com')
		, ('CDCUST', 'Ben Boyd', 'Boyd, Ben', 1, null, 'Ben', null, null, '07177 777566', 'ben@cdcus.com')
		, ('EFCUST', 'Christine Cook', 'Cook, Christine', 1, null, 'Chrissie', null, null, '07891 123456', 'chrissie@efcust.net')
		, ('SUPONE', 'Diane Durrel', 'Durrel, Diane', 1, null, 'Di', null, null, null, 'di@supplierone.co.uk')
		, ('SUPONE', 'Andy Brass', 'Brass, Andy', 1, null, 'Andy', null, null, null, null)
		, ('TRACOM', 'Dave Gomez', 'Gomez, Dave', 1, null, 'Dave', null, '01112 333452', '07755 5411000', 'daveg@transportco.biz')
		, ('THEPAP', 'Adam Jones', 'Jones, Adam', 1, null, 'Adam', null, null, null, 'adam@papersupplier.eu')
		, ('TRACOM', 'Andy Brass', 'Brass, Andy', 1, null, 'Andy', null, '01112 333444', null, 'bookings@transportco.biz')
		, ('SUPTHR', 'Andy Brass', 'Brass, Andy', 1, null, 'Andy', null, null, null, null)
		, ('BRICRA', 'Ed Shire', 'Shire, Ed', 1, null, 'Ed', null, null, '07854 00001', 'ed@britishcrafts.Subject.uk')
		, ('SUPTWO', 'Fred Flint', 'Flint, Fred', 1, null, 'Fred', null, null, null, 'fred@@suptwo.com')
		, ('SUPTHR', 'GeSubjectia Onmymind', 'Onmymind, GeSubjectia', 1, null, 'GeSubjectia', null, null, null, 'gonmy@supplierthree.ltd')
		, ('ABCUST', 'Ted Baker', 'Baker, Ted', 1, null, 'Ted', 'Accounts/Payments', null, null, 'ted@abcus.com')
		;

		INSERT INTO Project.tbProject (ProjectCode, UserId, SubjectCode, SecondReference, ProjectTitle, ContactName, ObjectCode, ProjectStatusCode, ActionById, ActionOn, ActionedOn, PaymentOn, ProjectNotes, Quantity, CashCode, TaxCode, UnitCharge, TotalCharge, AddressCodeFrom, AddressCodeTo, Spooled, Printed)
		VALUES (CONCAT(@UserId, '_10000'), @UserId, 'ABCUST', 'Order No. 12345', 'One-Off Book Order', 'Andy Brass', 'SO Book', 1, @UserId, '20190910', null, '20190910', null, 50, '103', 'T0', 9, 450.0000, 'ABCUST_001', 'ABCUST_002', 0, 0)
		, (CONCAT(@UserId, '_10007'), @UserId, 'CDCUST', 'Monthly Contract', 'CD Monthly Brochure', 'Ben Boyd', 'SO Brochure or Catalogue', 2, @UserId, '20190126', '20190126', '20190228', null, 5000, '103', 'T1', 0.4, 2000, 'CDCUST_001', 'CDCUST_002', 0, 0)
		, (CONCAT(@UserId, '_10008'), @UserId, 'CDCUST', 'Monthly Contract', 'CD Monthly Brochure', 'Ben Boyd', 'SO Brochure or Catalogue', 2, @UserId, '20190225', '20190225', '20190329', null, 5000, '103', 'T1', 0.4, 2000, 'CDCUST_001', 'CDCUST_002', 0, 0)
		, (CONCAT(@UserId, '_10009'), @UserId, 'CDCUST', 'Monthly Contract', 'CD Monthly Brochure', 'Ben Boyd', 'SO Brochure or Catalogue', 2, @UserId, '20190328', '20190328', '20190430', null, 5000, '103', 'T1', 0.4, 2000, 'CDCUST_001', 'CDCUST_002', 0, 0)
		, (CONCAT(@UserId, '_10010'), @UserId, 'CDCUST', 'Monthly Contract', 'CD Monthly Brochure', 'Ben Boyd', 'SO Brochure or Catalogue', 2, @UserId, '20190428', '20190428', '20190531', null, 5000, '103', 'T1', 0.4, 2000, 'CDCUST_001', 'CDCUST_002', 0, 0)
		, (CONCAT(@UserId, '_10011'), @UserId, 'CDCUST', 'Monthly Contract', 'CD Monthly Brochure', 'Ben Boyd', 'SO Brochure or Catalogue', 2, @UserId, '20190525', '20190525', '20190628', null, 5000, '103', 'T1', 0.4, 2000, 'CDCUST_001', 'CDCUST_002', 0, 0)
		, (CONCAT(@UserId, '_10012'), @UserId, 'CDCUST', 'Monthly Contract', 'CD Monthly Brochure', 'Ben Boyd', 'SO Brochure or Catalogue', 2, @UserId, '20190627', '20190822', '20190731', null, 5000, '103', 'T1', 0.4, 2000, 'CDCUST_001', 'CDCUST_002', 0, 0)
		, (CONCAT(@UserId, '_10013'), @UserId, 'CDCUST', 'Monthly Contract', 'CD Monthly Brochure', 'Ben Boyd', 'SO Brochure or Catalogue', 1, @UserId, '20190726', null, '20190830', null, 5000, '103', 'T1', 0.4, 2000, 'CDCUST_001', 'CDCUST_002', 0, 0)
		, (CONCAT(@UserId, '_10014'), @UserId, 'CDCUST', 'Monthly Contract', 'CD Monthly Brochure', 'Ben Boyd', 'SO Brochure or Catalogue', 1, @UserId, '20190828', null, '20190930', null, 5000, '103', 'T1', 0.4, 2000, 'CDCUST_001', 'CDCUST_002', 0, 0)
		, (CONCAT(@UserId, '_10015'), @UserId, 'EFCUST', 'PO12131', 'Outer Carton Ref X12-2', 'Christine Cook', 'SO Packaging', 1, @UserId, '20190917', null, '20190917', null, 1000, '103', 'T1', 0.62, 1240.0000, 'EFCUST_001', 'EFCUST_002', 0, 0)
		, (CONCAT(@UserId, '_10017'), @UserId, 'EFCUST', 'Ref B123234', 'McBurger Scratchcards', 'Christine Cook', 'SO Promotional', 2, @UserId, '20190331', '20190708', '20190515', null, 5000000, '103', 'T1', 0.0037, 18500.0000, 'EFCUST_001', 'EFCUST_002', 0, 0)
		, (CONCAT(@UserId, '_10018'), @UserId, 'ABCUST', 'PO 15550', 'Test Book Order', 'Andy Brass', 'SO Book', 1, @UserId, '20190903', null, '20190903', 'Call Andy 24 hours before delivery and send him 2 file copies

		The colour of the logo on the back cover must match previous orders', 50, '103', 'T1', 15.9, 795.0000, 'ABCUST_001', 'ABCUST_002', 0, 0)
		, (CONCAT(@UserId, '_10019'), @UserId, 'ABCUST', 'PO 15595', 'Main Book Order', 'Andy Brass', 'SO Book', 1, @UserId, '20191027', null, '20191126', 'Call Andy 24 hours before delivery and send him 2 file copies

		The colour of the logo on the back cover must match previous orders', 1000, '103', 'T1', 9.5, 9500.0000, 'ABCUST_001', 'ABCUST_002', 0, 0)
		, (CONCAT(@UserId, '_20000'), @UserId, 'SUPONE', 'Estimate 95456', 'One-Off Book Order', 'Andy Brass', 'PO Book', 1, @UserId, '20190725', null, '20190830', null, 50, '200', 'T0', 7.5, 375.0000, 'SUPONE_001', 'ABCUST_002', 0, 0)
		, (CONCAT(@UserId, '_20010'), @UserId, 'SUPTWO', 'Quote 12345A', 'CD Monthly Brochure', 'Fred Flint', 'PO Brochure or Catalogue', 2, @UserId, '20190125', '20190125', '20190228', null, 5000, '200', 'T0', 0.13, 650.0000, 'SUPTWO_001', 'EXWORK_001', 0, 0)
		, (CONCAT(@UserId, '_20011'), @UserId, 'TRACOM', 'Pallet scale rate', 'CD Monthly Brochure - Transport', 'Dave Gomez', 'PO Transport', 2, @UserId, '20190126', '20190126', '20190228', null, 2, '200', 'T1', 75, 150.0000, 'SUPTWO_002', 'CDCUST_002', 0, 0)
		, (CONCAT(@UserId, '_20013'), @UserId, 'SUPTWO', 'Quote 12345A', 'CD Monthly Brochure', 'Fred Flint', 'PO Brochure or Catalogue', 2, @UserId, '20190224', '20190224', '20190329', null, 5000, '200', 'T0', 0.13, 650.0000, 'SUPTWO_001', 'EXWORK_001', 0, 0)
		, (CONCAT(@UserId, '_20014'), @UserId, 'TRACOM', 'Pallet scale rate', 'CD Monthly Brochure - Transport', 'Dave Gomez', 'PO Transport', 2, @UserId, '20190225', '20190225', '20190329', null, 2, '200', 'T1', 75, 150.0000, 'SUPTWO_002', 'CDCUST_002', 0, 0)
		, (CONCAT(@UserId, '_20015'), @UserId, 'SUPTWO', 'Quote 12345A', 'CD Monthly Brochure', 'Fred Flint', 'PO Brochure or Catalogue', 2, @UserId, '20190327', '20190327', '20190430', null, 5000, '200', 'T0', 0.13, 650.0000, 'SUPTWO_001', 'EXWORK_001', 0, 0)
		, (CONCAT(@UserId, '_20016'), @UserId, 'TRACOM', 'Pallet scale rate', 'CD Monthly Brochure - Transport', 'Dave Gomez', 'PO Transport', 2, @UserId, '20190328', '20190328', '20190430', null, 2, '200', 'T1', 75, 150.0000, 'SUPTWO_002', 'CDCUST_002', 0, 0)
		, (CONCAT(@UserId, '_20017'), @UserId, 'SUPTWO', 'Quote 12345A', 'CD Monthly Brochure', 'Fred Flint', 'PO Brochure or Catalogue', 2, @UserId, '20190427', '20190427', '20190531', null, 5000, '200', 'T0', 0.13, 650.0000, 'SUPTWO_001', 'EXWORK_001', 0, 0)
		, (CONCAT(@UserId, '_20018'), @UserId, 'TRACOM', 'Pallet scale rate', 'CD Monthly Brochure - Transport', 'Dave Gomez', 'PO Transport', 2, @UserId, '20190428', '20190428', '20190531', null, 2, '200', 'T1', 75, 150.0000, 'SUPTWO_002', 'CDCUST_002', 0, 0)
		, (CONCAT(@UserId, '_20019'), @UserId, 'SUPTWO', 'Quote 12345A', 'CD Monthly Brochure', 'Fred Flint', 'PO Brochure or Catalogue', 2, @UserId, '20190524', '20190524', '20190628', null, 5000, '200', 'T0', 0.13, 650.0000, 'SUPTWO_001', 'EXWORK_001', 0, 0)
		, (CONCAT(@UserId, '_20020'), @UserId, 'TRACOM', 'Pallet scale rate', 'CD Monthly Brochure - Transport', 'Dave Gomez', 'PO Transport', 2, @UserId, '20190525', '20190525', '20190628', null, 2, '200', 'T1', 75, 150.0000, 'SUPTWO_002', 'CDCUST_002', 0, 0)
		, (CONCAT(@UserId, '_20021'), @UserId, 'SUPTWO', 'Quote 12345A', 'CD Monthly Brochure', 'Fred Flint', 'PO Brochure or Catalogue', 2, @UserId, '20190626', '20190822', '20190731', null, 5000, '200', 'T0', 0.13, 650.0000, 'SUPTWO_001', 'EXWORK_001', 0, 0)
		, (CONCAT(@UserId, '_20022'), @UserId, 'TRACOM', 'Pallet scale rate', 'CD Monthly Brochure - Transport', 'Dave Gomez', 'PO Transport', 2, @UserId, '20190626', '20190822', '20190731', null, 2, '200', 'T1', 75, 150.0000, 'SUPTWO_002', 'CDCUST_002', 0, 0)
		, (CONCAT(@UserId, '_20025'), @UserId, 'SUPTWO', 'Quote 12345A', 'CD Monthly Brochure', 'Fred Flint', 'PO Brochure or Catalogue', 1, @UserId, '20190725', null, '20190830', null, 5000, '200', 'T0', 0.13, 650.0000, 'SUPTWO_001', 'EXWORK_001', 0, 0)
		, (CONCAT(@UserId, '_20026'), @UserId, 'TRACOM', 'Pallet scale rate', 'CD Monthly Brochure - Transport', 'Dave Gomez', 'PO Transport', 1, @UserId, '20190726', null, '20190830', null, 2, '200', 'T1', 75, 150.0000, 'SUPTWO_002', 'CDCUST_002', 0, 0)
		, (CONCAT(@UserId, '_20027'), @UserId, 'SUPTWO', 'Quote 12345A', 'CD Monthly Brochure', 'Fred Flint', 'PO Brochure or Catalogue', 1, @UserId, '20190827', null, '20190930', null, 5000, '200', 'T0', 0.13, 650.0000, 'SUPTWO_001', 'EXWORK_001', 0, 0)
		, (CONCAT(@UserId, '_20028'), @UserId, 'TRACOM', 'Pallet scale rate', 'CD Monthly Brochure - Transport', 'Dave Gomez', 'PO Transport', 0, @UserId, '20190828', null, '20190930', null, 2, '200', 'T1', 75, 150.0000, 'SUPTWO_002', 'CDCUST_002', 0, 0)
		, (CONCAT(@UserId, '_20029'), @UserId, 'SUPTHR', 'Estimate B115536', 'Outer Carton Ref X12', 'GeSubjectia Onmymind', 'PO Packaging', 1, @UserId, '20190708', null, '20190830', null, 2000, '200', 'T1', 0.48, 960.0000, 'SUPTHR_001', 'EFCUST_002', 0, 0)
		, (CONCAT(@UserId, '_20031'), @UserId, 'SUPTWO', null, 'McBurger Scratchcards', 'Fred Flint', 'PO Promotional', 2, @UserId, '20190330', '20190708', '20190430', null, 5000000, '200', 'T1', 0.0012, 6000.0000, 'SUPTWO_001', 'SUPTWO_001', 0, 0)
		, (CONCAT(@UserId, '_20032'), @UserId, 'THEPAP', null, 'McBurger Scratchcards', 'Adam Jones', 'PO Packaging', 2, @UserId, '20190316', '20190708', '20190415', null, 13, '200', 'T1', 750, 9750.0000, 'THEPAP_001', 'SUPTWO_002', 0, 0)
		, (CONCAT(@UserId, '_20034'), @UserId, 'SUPONE', 'Scale rates', 'Test Book Order', 'Andy Brass', 'PO Book', 1, @UserId, '20190721', null, '20190830', null, 50, '200', 'T1', 11.9, 595.0000, 'ABCUST_001', 'ABCUST_001', 0, 0)
		, (CONCAT(@UserId, '_20035'), @UserId, 'TRACOM', null, 'Test Book Order', 'Andy Brass', 'PO Transport', 1, @UserId, '20190722', null, '20190830', null, 1, '200', 'T1', 75, 75.0000, 'SUPONE_002', 'ABCUST_002', 0, 0)
		, (CONCAT(@UserId, '_20037'), @UserId, 'SUPONE', 'Scale rates', 'Main Book Order', 'Andy Brass', 'PO Book', 1, @UserId, '20191026', null, '20191129', null, 1000, '200', 'T1', 7.95, 7950.0000, 'ABCUST_001', 'ABCUST_001', 0, 0)
		, (CONCAT(@UserId, '_20038'), @UserId, 'TRACOM', null, 'Main Book Order - Transport', 'Andy Brass', 'PO Transport', 1, @UserId, '20191027', null, '20191129', null, 8, '200', 'T1', 55, 440.0000, 'SUPONE_002', 'ABCUST_002', 0, 0)
		, (CONCAT(@UserId, '_30000'), @UserId, 'CDCUST', null, 'Monthly Brochures', null, 'Project', 0, @UserId, '20190101', null, '20190131', null, 1, null, null, 0, 0.0000, 'CDCUST_001', 'CDCUST_001', 0, 1)
		, (CONCAT(@UserId, '_30001'), @UserId, 'BUSOWN', null, 'Salaries', null, 'Project', 0, @UserId, '20191231', null, '20191231', null, 1, null, null, 0, 0.0000, 'CDCUST_001', 'CDCUST_001', 0, 1)
		, (CONCAT(@UserId, '_30002'), @UserId, 'TELPRO', null, 'Monthly Telecom Charges', null, 'Project', 0, @UserId, '20191231', null, '20191231', null, 1, null, null, 0, 0.0000, 'CDCUST_001', 'CDCUST_001', 0, 1)
		, (CONCAT(@UserId, '_40000'), @UserId, 'BUSOWN', null, '142 miles travel Client visit', null, 'Employee Transport', 2, @UserId, '20190110', '20190708', '20190131', null, 142, '212', 'T0', 0.45, 63.9000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40003'), @UserId, 'BUSOWN', null, 'Car parking Client visit 10/1', null, 'Car Parking / Tolls', 2, @UserId, '20190110', '20190708', '20190131', null, 1, '213', 'T1', 4, 4.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40004'), @UserId, 'BUSOWN', null, 'Rental for Home Office use £4/week x 4 weeks', null, 'Office Rent', 2, @UserId, '20190131', '20190708', '20190131', null, 4, '205', 'T0', 4, 16.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40005'), @UserId, 'BUSOWN', null, 'Wages', null, 'Wages monthly payment', 2, @UserId, '20190131', '20190708', '20190131', null, 1, '402', 'NI1', 1000, 1000.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40006'), @UserId, 'BUSOWN', null, 'Wages', null, 'Wages monthly payment', 2, @UserId, '20190228', '20190708', '20190228', null, 1, '402', 'NI1', 1000, 1000.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40007'), @UserId, 'BUSOWN', null, 'Wages', null, 'Wages monthly payment', 2, @UserId, '20190329', '20190708', '20190329', null, 1, '402', 'NI1', 1000, 1000.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40008'), @UserId, 'BUSOWN', null, 'Wages', null, 'Wages monthly payment', 2, @UserId, '20190430', '20190708', '20190430', null, 1, '402', 'NI1', 1000, 1000.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40009'), @UserId, 'BUSOWN', null, 'Wages', null, 'Wages monthly payment', 2, @UserId, '20190531', '20190708', '20190531', null, 1, '402', 'NI1', 1000, 1000.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40010'), @UserId, 'BUSOWN', null, 'Wages', null, 'Wages monthly payment', 2, @UserId, '20190628', '20190708', '20190628', null, 1, '402', 'NI1', 1000, 1000.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40011'), @UserId, 'BUSOWN', null, 'Wages', null, 'Wages monthly payment', 2, @UserId, '20190731', '20190822', '20190731', null, 1, '402', 'NI1', 1000, 1000.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40012'), @UserId, 'BUSOWN', null, 'Wages', null, 'Wages monthly payment', 1, @UserId, '20190830', null, '20190830', null, 1, '402', 'NI1', 1000, 1000.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40013'), @UserId, 'BUSOWN', null, 'Wages', null, 'Wages monthly payment', 1, @UserId, '20190930', null, '20190930', null, 1, '402', 'NI1', 1000, 1000.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40014'), @UserId, 'BUSOWN', null, 'Wages', null, 'Wages monthly payment', 0, @UserId, '20191031', null, '20191031', null, 1, '402', 'NI1', 1000, 1000.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40015'), @UserId, 'BUSOWN', null, 'Wages', null, 'Wages monthly payment', 0, @UserId, '20191129', null, '20191129', null, 1, '402', 'NI1', 1000, 1000.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40016'), @UserId, 'BUSOWN', null, 'Wages', null, 'Wages monthly payment', 0, @UserId, '20191231', null, '20191231', null, 1, '402', 'NI1', 1000, 1000.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40017'), @UserId, 'BUSOWN', null, '185 miles press pass book sections', null, 'Employee Transport', 2, @UserId, '20190215', '20190708', '20190228', null, 185, '212', 'T0', 0.45, 83.2500, null, null, 0, 1)
		, (CONCAT(@UserId, '_40018'), @UserId, 'BUSOWN', null, '24 First Class postage stamps', null, 'Postage', 2, @UserId, '20190208', '20190708', '20190228', null, 1, '207', 'T0', 19.2, 19.2000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40019'), @UserId, 'BUSOWN', null, 'Rental for Home Office use £4/week x 4 weeks', null, 'Office Rent', 2, @UserId, '20190228', '20190708', '20190228', null, 1, '205', 'T0', 16, 16.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40020'), @UserId, 'BUSOWN', null, '178 miles visiting AB Ltd', null, 'Employee Transport', 2, @UserId, '20190302', '20190708', '20190329', null, 178, '212', 'T0', 0.45, 80.1000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40021'), @UserId, 'BUSOWN', null, 'Dartford Crossing x 2', null, 'Car Parking / Tolls', 2, @UserId, '20190302', '20190708', '20190329', null, 1, '213', 'T0', 5, 5.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40022'), @UserId, 'BUSOWN', null, 'Rental for Home Office use £4/week x 5 weeks', null, 'Office Rent', 2, @UserId, '20190329', '20190708', '20190329', null, 1, '205', 'T0', 20, 20.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40023'), @UserId, 'BUSOWN', null, 'Business mileage April 19 total 340 miles', null, 'Employee Transport', 2, @UserId, '20190430', '20190708', '20190430', null, 340, '212', 'T0', 0.45, 153.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40024'), @UserId, 'BUSOWN', null, 'Rental for Home Office use £4/week x 4 weeks', null, 'Office Rent', 2, @UserId, '20190430', '20190708', '20190430', null, 1, '205', 'T0', 16, 16.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40025'), @UserId, 'BUSOWN', null, 'Business mileage May 19 total 395 miles', null, 'Employee Transport', 2, @UserId, '20190531', '20190708', '20190531', null, 395, '212', 'T0', 0.45, 177.7500, null, null, 0, 1)
		, (CONCAT(@UserId, '_40026'), @UserId, 'BUSOWN', null, '6 reams of office paper', null, 'Stationery - General', 2, @UserId, '20190531', '20190708', '20190531', null, 1, '209', 'T1', 18, 18.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40027'), @UserId, 'BUSOWN', null, 'Rental for Home Office use £4/week x 4 weeks', null, 'Office Rent', 2, @UserId, '20190531', '20190708', '20190531', null, 1, '205', 'T0', 16, 16.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40028'), @UserId, 'BUSOWN', null, 'Business mileage June 19 412miles', null, 'Employee Transport', 2, @UserId, '20190628', '20190708', '20190628', null, 412, '212', 'T0', 0.45, 185.4000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40029'), @UserId, 'BUSOWN', null, 'Car parking Client visit 10/6', null, 'Car Parking / Tolls', 2, @UserId, '20190610', '20190708', '20190628', null, 1, '213', 'T1', 5, 5.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40030'), @UserId, 'BUSOWN', null, 'Rental for Home Office use £4/week x 4 weeks', null, 'Office Rent', 2, @UserId, '20190628', '20190708', '20190628', null, 1, '205', 'T0', 12, 12.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40031'), @UserId, 'TELPRO', null, 'Telecom Charge', null, 'Communications monthly charge', 2, @UserId, '20190125', '20190125', '20190125', null, 1, '202', 'T1', 40, 40.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40032'), @UserId, 'TELPRO', null, 'Telecom Charge', null, 'Communications monthly charge', 2, @UserId, '20190226', '20190226', '20190226', null, 1, '202', 'T1', 39.6, 39.6000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40033'), @UserId, 'TELPRO', null, 'Telecom Charge', null, 'Communications monthly charge', 2, @UserId, '20190326', '20190326', '20190326', null, 1, '202', 'T1', 43.12, 43.1200, null, null, 0, 1)
		, (CONCAT(@UserId, '_40034'), @UserId, 'TELPRO', null, 'Telecom Charge', null, 'Communications monthly charge', 2, @UserId, '20190426', '20190326', '20190426', null, 1, '202', 'T1', 43.52, 43.5200, null, null, 0, 1)
		, (CONCAT(@UserId, '_40035'), @UserId, 'TELPRO', null, 'Telecom Charge', null, 'Communications monthly charge', 2, @UserId, '20190524', '20190524', '20190524', null, 1, '202', 'T1', 42.52, 42.5200, null, null, 0, 1)
		, (CONCAT(@UserId, '_40036'), @UserId, 'TELPRO', null, 'Telecom Charge', null, 'Communications monthly charge', 2, @UserId, '20190626', '20190626', '20190626', null, 1, '202', 'T1', 41.15, 41.1500, null, null, 0, 1)
		, (CONCAT(@UserId, '_40037'), @UserId, 'TELPRO', null, 'Telecom Charge', null, 'Communications monthly charge', 2, @UserId, '20190726', '20190822', '20190726', null, 1, '202', 'T1', 40, 40.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40038'), @UserId, 'TELPRO', null, 'Telecom Charge', null, 'Communications monthly charge', 1, @UserId, '20190826', null, '20190826', null, 1, '202', 'T1', 40, 40.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40039'), @UserId, 'TELPRO', null, 'Telecom Charge', null, 'Communications monthly charge', 1, @UserId, '20190926', null, '20190926', null, 1, '202', 'T1', 40, 40.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40040'), @UserId, 'TELPRO', null, 'Telecom Charge', null, 'Communications monthly charge', 0, @UserId, '20191025', null, '20191025', null, 1, '202', 'T1', 40, 40.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40041'), @UserId, 'TELPRO', null, 'Telecom Charge', null, 'Communications monthly charge', 0, @UserId, '20191126', null, '20191126', null, 1, '202', 'T1', 40, 40.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40042'), @UserId, 'TELPRO', null, 'Telecom Charge', null, 'Communications monthly charge', 0, @UserId, '20191224', null, '20191224', null, 1, '202', 'T1', 40, 40.0000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40044'), @UserId, 'BUSOWN', null, 'Subsistence for NEC Show', null, 'Subsistence', 2, @UserId, '20190801', '20190801', '20190830', null, 1, '218', 'T0', 8.5, 8.5000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40045'), @UserId, 'BUSOWN', null, '320 miles travel to NEC Show', null, 'Employee Transport', 1, @UserId, '20190801', null, '20190830', null, 212, '212', 'T0', 0.45, 95.4000, null, null, 0, 1)
		, (CONCAT(@UserId, '_40046'), @UserId, 'SUNSUP', null, 'Ring Binders x 12 from local shop', null, 'Stationery - General', 2, @UserId, '20190702', '20190722', '20190702', null, 12, '209', 'T1', 4.5, 54.0000, null, null, 0, 1)
		;
		INSERT INTO Project.tbFlow (ParentProjectCode, StepNumber, ChildProjectCode, SyncTypeCode, UsedOnQuantity, OffsetDays)
		VALUES (CONCAT(@UserId, '_10000'), 10, CONCAT(@UserId, '_20000'), 0, 0, 0)
		, (CONCAT(@UserId, '_10007'), 10, CONCAT(@UserId, '_20010'), 0, 0, 7)
		, (CONCAT(@UserId, '_10007'), 20, CONCAT(@UserId, '_20011'), 0, 0, -7)
		, (CONCAT(@UserId, '_10008'), 10, CONCAT(@UserId, '_20013'), 0, 0, -2)
		, (CONCAT(@UserId, '_10008'), 20, CONCAT(@UserId, '_20014'), 0, 0, 0)
		, (CONCAT(@UserId, '_10009'), 10, CONCAT(@UserId, '_20015'), 0, 0, 11)
		, (CONCAT(@UserId, '_10009'), 20, CONCAT(@UserId, '_20016'), 0, 0, 56)
		, (CONCAT(@UserId, '_10010'), 10, CONCAT(@UserId, '_20017'), 0, 0, 0)
		, (CONCAT(@UserId, '_10010'), 20, CONCAT(@UserId, '_20018'), 0, 0, 0)
		, (CONCAT(@UserId, '_10011'), 10, CONCAT(@UserId, '_20019'), 0, 0, -10)
		, (CONCAT(@UserId, '_10011'), 20, CONCAT(@UserId, '_20020'), 0, 0, 0)
		, (CONCAT(@UserId, '_10012'), 10, CONCAT(@UserId, '_20022'), 0, 0, 0)
		, (CONCAT(@UserId, '_10012'), 20, CONCAT(@UserId, '_20021'), 0, 0, 1)
		, (CONCAT(@UserId, '_10013'), 10, CONCAT(@UserId, '_20025'), 0, 0, 1)
		, (CONCAT(@UserId, '_10013'), 30, CONCAT(@UserId, '_20026'), 0, 0, 0)
		, (CONCAT(@UserId, '_10014'), 10, CONCAT(@UserId, '_20027'), 0, 0, 1)
		, (CONCAT(@UserId, '_10014'), 30, CONCAT(@UserId, '_20028'), 0, 0, 0)
		, (CONCAT(@UserId, '_10015'), 10, CONCAT(@UserId, '_20029'), 0, 0, 0)
		, (CONCAT(@UserId, '_10017'), 10, CONCAT(@UserId, '_20032'), 0, 0, 10)
		, (CONCAT(@UserId, '_10017'), 20, CONCAT(@UserId, '_20031'), 0, 0, 0)
		, (CONCAT(@UserId, '_10018'), 10, CONCAT(@UserId, '_20034'), 0, 0, 1)
		, (CONCAT(@UserId, '_10018'), 20, CONCAT(@UserId, '_20035'), 0, 0, 0)
		, (CONCAT(@UserId, '_10019'), 10, CONCAT(@UserId, '_20037'), 0, 0, 0)
		, (CONCAT(@UserId, '_10019'), 20, CONCAT(@UserId, '_20038'), 0, 0, 0)
		, (CONCAT(@UserId, '_30000'), 10, CONCAT(@UserId, '_10007'), 0, 0, 0)
		, (CONCAT(@UserId, '_30000'), 20, CONCAT(@UserId, '_10008'), 0, 0, 0)
		, (CONCAT(@UserId, '_30000'), 30, CONCAT(@UserId, '_10009'), 0, 0, 0)
		, (CONCAT(@UserId, '_30000'), 40, CONCAT(@UserId, '_10010'), 0, 0, 0)
		, (CONCAT(@UserId, '_30000'), 50, CONCAT(@UserId, '_10011'), 0, 0, 0)
		, (CONCAT(@UserId, '_30000'), 60, CONCAT(@UserId, '_10012'), 0, 0, 0)
		, (CONCAT(@UserId, '_30000'), 70, CONCAT(@UserId, '_10013'), 0, 0, 0)
		, (CONCAT(@UserId, '_30000'), 80, CONCAT(@UserId, '_10014'), 0, 0, 0)
		, (CONCAT(@UserId, '_30001'), 10, CONCAT(@UserId, '_40005'), 0, 0, 0)
		, (CONCAT(@UserId, '_30001'), 20, CONCAT(@UserId, '_40006'), 0, 0, 0)
		, (CONCAT(@UserId, '_30001'), 30, CONCAT(@UserId, '_40007'), 0, 0, 0)
		, (CONCAT(@UserId, '_30001'), 40, CONCAT(@UserId, '_40008'), 0, 0, 0)
		, (CONCAT(@UserId, '_30001'), 50, CONCAT(@UserId, '_40009'), 0, 0, 0)
		, (CONCAT(@UserId, '_30001'), 60, CONCAT(@UserId, '_40010'), 0, 0, 0)
		, (CONCAT(@UserId, '_30001'), 70, CONCAT(@UserId, '_40011'), 0, 0, 0)
		, (CONCAT(@UserId, '_30001'), 80, CONCAT(@UserId, '_40012'), 0, 0, 0)
		, (CONCAT(@UserId, '_30001'), 90, CONCAT(@UserId, '_40013'), 0, 0, 0)
		, (CONCAT(@UserId, '_30001'), 100, CONCAT(@UserId, '_40014'), 0, 0, 0)
		, (CONCAT(@UserId, '_30001'), 110, CONCAT(@UserId, '_40015'), 0, 0, 0)
		, (CONCAT(@UserId, '_30001'), 120, CONCAT(@UserId, '_40016'), 0, 0, 0)
		, (CONCAT(@UserId, '_30002'), 10, CONCAT(@UserId, '_40031'), 0, 0, 0)
		, (CONCAT(@UserId, '_30002'), 20, CONCAT(@UserId, '_40032'), 0, 0, 0)
		, (CONCAT(@UserId, '_30002'), 30, CONCAT(@UserId, '_40033'), 0, 0, 0)
		, (CONCAT(@UserId, '_30002'), 40, CONCAT(@UserId, '_40034'), 0, 0, 0)
		, (CONCAT(@UserId, '_30002'), 50, CONCAT(@UserId, '_40035'), 0, 0, 0)
		, (CONCAT(@UserId, '_30002'), 60, CONCAT(@UserId, '_40036'), 0, 0, 0)
		, (CONCAT(@UserId, '_30002'), 70, CONCAT(@UserId, '_40037'), 0, 0, 0)
		, (CONCAT(@UserId, '_30002'), 80, CONCAT(@UserId, '_40038'), 0, 0, 0)
		, (CONCAT(@UserId, '_30002'), 90, CONCAT(@UserId, '_40039'), 0, 0, 0)
		, (CONCAT(@UserId, '_30002'), 100, CONCAT(@UserId, '_40040'), 0, 0, 0)
		, (CONCAT(@UserId, '_30002'), 110, CONCAT(@UserId, '_40041'), 0, 0, 0)
		, (CONCAT(@UserId, '_30002'), 120, CONCAT(@UserId, '_40042'), 0, 0, 0)
		;
		INSERT INTO Project.tbOp (ProjectCode, OperationNumber, SyncTypeCode, OpStatusCode, UserId, Operation, Note, StartOn, EndOn, Duration, OffsetDays)
		VALUES (CONCAT(@UserId, '_10000'), 10, 0, 1, @UserId, 'Artwork', null, '20190718', '20190718', 0, 0)
		, (CONCAT(@UserId, '_10000'), 20, 0, 0, @UserId, 'Proofs', null, '20190718', '20190718', 0, 0)
		, (CONCAT(@UserId, '_10000'), 30, 0, 0, @UserId, 'Approval', null, '20190717', '20190717', 0, 0)
		, (CONCAT(@UserId, '_10000'), 40, 2, 0, @UserId, 'Delivery', null, '20190725', '20190910', 0, 0)
		, (CONCAT(@UserId, '_10007'), 10, 0, 2, @UserId, 'Artwork', 'by 5pm', '20190118', '20190120', 0, 0)
		, (CONCAT(@UserId, '_10007'), 20, 0, 2, @UserId, 'Proofs', 'by noon', '20190117', '20190121', 0, 2)
		, (CONCAT(@UserId, '_10007'), 30, 0, 2, @UserId, 'Approval', 'by 4pm', '20190116', '20190121', 0, 3)
		, (CONCAT(@UserId, '_10007'), 40, 2, 2, @UserId, 'Delivery', null, '20190118', '20190126', 0, 1)
		, (CONCAT(@UserId, '_10008'), 10, 0, 2, @UserId, 'Artwork', 'by 5pm', '20190219', '20190219', 0, 0)
		, (CONCAT(@UserId, '_10008'), 20, 0, 2, @UserId, 'Proofs', 'by noon', '20190218', '20190220', 0, 2)
		, (CONCAT(@UserId, '_10008'), 30, 0, 2, @UserId, 'Approval', 'by 4pm', '20190215', '20190220', 0, 3)
		, (CONCAT(@UserId, '_10008'), 40, 2, 2, @UserId, 'Delivery', null, '20190218', '20190225', 0, 1)
		, (CONCAT(@UserId, '_10009'), 10, 0, 2, @UserId, 'Artwork', 'by 5pm', '20190322', '20190323', 0, 0)
		, (CONCAT(@UserId, '_10009'), 20, 0, 2, @UserId, 'Proofs', 'by noon', '20190320', '20190324', 0, 2)
		, (CONCAT(@UserId, '_10009'), 30, 0, 2, @UserId, 'Approval', 'by 4pm', '20190319', '20190324', 0, 3)
		, (CONCAT(@UserId, '_10009'), 40, 2, 2, @UserId, 'Delivery', null, '20190321', '20190328', 0, 1)
		, (CONCAT(@UserId, '_10010'), 10, 0, 2, @UserId, 'Artwork', 'by 5pm', '20190423', '20190423', 0, 0)
		, (CONCAT(@UserId, '_10010'), 20, 0, 2, @UserId, 'Proofs', 'by noon', '20190422', '20190424', 0, 2)
		, (CONCAT(@UserId, '_10010'), 30, 0, 2, @UserId, 'Approval', 'by 4pm', '20190419', '20190424', 0, 3)
		, (CONCAT(@UserId, '_10010'), 40, 2, 2, @UserId, 'Delivery', null, '20190419', '20190428', 0, 1)
		, (CONCAT(@UserId, '_10011'), 10, 0, 2, @UserId, 'Artwork', 'by 5pm', '20190517', '20190519', 0, 0)
		, (CONCAT(@UserId, '_10011'), 20, 0, 2, @UserId, 'Proofs', 'by noon', '20190516', '20190520', 0, 2)
		, (CONCAT(@UserId, '_10011'), 30, 0, 2, @UserId, 'Approval', 'by 4pm', '20190515', '20190520', 0, 3)
		, (CONCAT(@UserId, '_10011'), 40, 2, 2, @UserId, 'Delivery', null, '20190517', '20190525', 0, 5)
		, (CONCAT(@UserId, '_10012'), 10, 0, 2, @UserId, 'Artwork', 'by 5pm', '20190620', '20190620', 0, 0)
		, (CONCAT(@UserId, '_10012'), 20, 0, 2, @UserId, 'Proofs', 'by noon', '20190619', '20190621', 0, 2)
		, (CONCAT(@UserId, '_10012'), 30, 0, 2, @UserId, 'Approval', 'by 4pm', '20190618', '20190621', 0, 3)
		, (CONCAT(@UserId, '_10012'), 40, 2, 2, @UserId, 'Delivery', null, '20190620', '20190627', 0, 5)
		, (CONCAT(@UserId, '_10013'), 10, 0, 1, @UserId, 'Artwork', 'by 5pm', '20190719', '20190719', 0, 0)
		, (CONCAT(@UserId, '_10013'), 20, 0, 0, @UserId, 'Proofs', 'by noon', '20190718', '20190722', 0, 2)
		, (CONCAT(@UserId, '_10013'), 30, 0, 0, @UserId, 'Approval', 'by 4pm', '20190717', '20190722', 0, 3)
		, (CONCAT(@UserId, '_10013'), 40, 2, 0, @UserId, 'Delivery', null, '20190719', '20190726', 0, 5)
		, (CONCAT(@UserId, '_10014'), 10, 0, 1, @UserId, 'Artwork', 'by 5pm', '20190819', '20190819', 0, 0)
		, (CONCAT(@UserId, '_10014'), 20, 0, 0, @UserId, 'Proofs', 'by noon', '20190816', '20190820', 0, 2)
		, (CONCAT(@UserId, '_10014'), 30, 0, 0, @UserId, 'Approval', 'by 4pm', '20190815', '20190820', 0, 3)
		, (CONCAT(@UserId, '_10014'), 40, 2, 0, @UserId, 'Delivery', null, '20190821', '20190828', 0, 5)
		, (CONCAT(@UserId, '_10015'), 40, 2, 0, @UserId, 'Delivery', null, '20190701', '20190917', 0, 5)
		, (CONCAT(@UserId, '_10017'), 10, 0, 2, @UserId, 'Artwork', null, '20190308', '20190308', 0, 0)
		, (CONCAT(@UserId, '_10017'), 20, 0, 2, @UserId, 'Proofs', null, '20190310', '20190310', 0, 0)
		, (CONCAT(@UserId, '_10017'), 30, 0, 2, @UserId, 'Approval', null, '20190311', '20190311', 0, 0)
		, (CONCAT(@UserId, '_10017'), 40, 0, 2, @UserId, 'Delivery', null, '20190331', '20190331', 0, 0)
		, (CONCAT(@UserId, '_10018'), 10, 0, 1, @UserId, 'Artwork', null, '20190708', '20190708', 0, 0)
		, (CONCAT(@UserId, '_10018'), 20, 0, 0, @UserId, 'Proofs', null, '20190708', '20190709', 0, 1)
		, (CONCAT(@UserId, '_10018'), 30, 0, 0, @UserId, 'Approval', null, '20190709', '20190711', 0, 2)
		, (CONCAT(@UserId, '_10018'), 40, 2, 0, @UserId, 'Delivery', null, '20190711', '20190903', 0, 1)
		, (CONCAT(@UserId, '_10019'), 10, 0, 1, @UserId, 'Artwork', null, '20191008', '20191008', 0, 0)
		, (CONCAT(@UserId, '_10019'), 20, 0, 0, @UserId, 'Proofs', null, '20191008', '20191009', 0, 1)
		, (CONCAT(@UserId, '_10019'), 30, 0, 0, @UserId, 'Approval', null, '20191008', '20191010', 0, 2)
		, (CONCAT(@UserId, '_10019'), 40, 2, 0, @UserId, 'Delivery', null, '20191004', '20191027', 0, 1)
		, (CONCAT(@UserId, '_20010'), 10, 0, 2, @UserId, 'Artwork', 'by 5pm', '20190118', '20190120', 0, 0)
		, (CONCAT(@UserId, '_20010'), 20, 0, 2, @UserId, 'Proofs', 'by noon', '20190121', '20190121', 0, 0)
		, (CONCAT(@UserId, '_20010'), 30, 0, 2, @UserId, 'Approval', 'by 4pm', '20190121', '20190121', 0, 0)
		, (CONCAT(@UserId, '_20010'), 50, 2, 2, @UserId, 'Collection', 'from 10am', '20190125', '20190125', 0, 0)
		, (CONCAT(@UserId, '_20011'), 10, 0, 2, @UserId, 'Collect', 'after 10am', '20190125', '20190125', 0, 0)
		, (CONCAT(@UserId, '_20011'), 20, 2, 2, @UserId, 'Delivery', null, '20190125', '20190126', 0, 0)
		, (CONCAT(@UserId, '_20013'), 10, 0, 2, @UserId, 'Artwork', 'by 5pm', '20190219', '20190219', 0, 0)
		, (CONCAT(@UserId, '_20013'), 20, 0, 2, @UserId, 'Proofs', 'by noon', '20190220', '20190220', 0, 0)
		, (CONCAT(@UserId, '_20013'), 30, 0, 2, @UserId, 'Approval', 'by 4pm', '20190220', '20190220', 0, 0)
		, (CONCAT(@UserId, '_20013'), 50, 2, 2, @UserId, 'Collection', 'from 10am', '20190222', '20190224', 0, 0)
		, (CONCAT(@UserId, '_20014'), 10, 0, 2, @UserId, 'Collect', 'after 10am', '20190222', '20190224', 0, 0)
		, (CONCAT(@UserId, '_20014'), 20, 2, 2, @UserId, 'Delivery', null, '20190225', '20190225', 0, 0)
		, (CONCAT(@UserId, '_20015'), 10, 0, 2, @UserId, 'Artwork', 'by 5pm', '20190322', '20190323', 0, 0)
		, (CONCAT(@UserId, '_20015'), 20, 0, 2, @UserId, 'Proofs', 'by noon', '20190322', '20190324', 0, 0)
		, (CONCAT(@UserId, '_20015'), 30, 0, 2, @UserId, 'Approval', 'by 4pm', '20190322', '20190324', 0, 0)
		, (CONCAT(@UserId, '_20015'), 50, 2, 2, @UserId, 'Collection', 'from 10am', '20190327', '20190327', 0, 0)
		, (CONCAT(@UserId, '_20016'), 10, 0, 2, @UserId, 'Collect', 'after 10am', '20190327', '20190327', 0, 0)
		, (CONCAT(@UserId, '_20016'), 20, 2, 2, @UserId, 'Delivery', null, '20190328', '20190328', 0, 0)
		, (CONCAT(@UserId, '_20017'), 10, 0, 2, @UserId, 'Artwork', 'by 5pm', '20190423', '20190423', 0, 0)
		, (CONCAT(@UserId, '_20017'), 20, 0, 2, @UserId, 'Proofs', 'by noon', '20190424', '20190424', 0, 0)
		, (CONCAT(@UserId, '_20017'), 30, 0, 2, @UserId, 'Approval', 'by 4pm', '20190424', '20190424', 0, 0)
		, (CONCAT(@UserId, '_20017'), 50, 2, 2, @UserId, 'Collection', 'from 10am', '20190426', '20190427', 0, 0)
		, (CONCAT(@UserId, '_20018'), 10, 0, 2, @UserId, 'Collect', 'after 10am', '20190426', '20190427', 0, 0)
		, (CONCAT(@UserId, '_20018'), 20, 2, 2, @UserId, 'Delivery', null, '20190426', '20190428', 0, 0)
		, (CONCAT(@UserId, '_20019'), 10, 0, 2, @UserId, 'Artwork', 'by 5pm', '20190517', '20190519', 0, 0)
		, (CONCAT(@UserId, '_20019'), 20, 0, 2, @UserId, 'Proofs', 'by noon', '20190520', '20190520', 0, 0)
		, (CONCAT(@UserId, '_20019'), 30, 0, 2, @UserId, 'Approval', 'by 4pm', '20190520', '20190520', 0, 0)
		, (CONCAT(@UserId, '_20019'), 50, 2, 2, @UserId, 'Collection', 'from 10am', '20190524', '20190524', 0, 0)
		, (CONCAT(@UserId, '_20020'), 10, 0, 2, @UserId, 'Collect', 'after 10am', '20190524', '20190524', 0, 0)
		, (CONCAT(@UserId, '_20020'), 20, 2, 2, @UserId, 'Delivery', null, '20190524', '20190525', 0, 0)
		, (CONCAT(@UserId, '_20021'), 10, 0, 2, @UserId, 'Artwork', 'by 5pm', '20190620', '20190620', 0, 0)
		, (CONCAT(@UserId, '_20021'), 20, 0, 2, @UserId, 'Proofs', 'by noon', '20190621', '20190621', 0, 0)
		, (CONCAT(@UserId, '_20021'), 30, 0, 2, @UserId, 'Approval', 'by 4pm', '20190621', '20190621', 0, 0)
		, (CONCAT(@UserId, '_20021'), 50, 2, 2, @UserId, 'Collection', 'from 10am', '20190626', '20190626', 0, 0)
		, (CONCAT(@UserId, '_20022'), 10, 0, 2, @UserId, 'Collect', 'after 10am', '20190625', '20190625', 0, 0)
		, (CONCAT(@UserId, '_20022'), 20, 2, 2, @UserId, 'Delivery', null, '20190626', '20190626', 0, 0)
		, (CONCAT(@UserId, '_20025'), 10, 0, 1, @UserId, 'Artwork', 'by 5pm', '20190719', '20190719', 0, 0)
		, (CONCAT(@UserId, '_20025'), 20, 0, 0, @UserId, 'Proofs', 'by noon', '20190722', '20190722', 0, 0)
		, (CONCAT(@UserId, '_20025'), 30, 0, 0, @UserId, 'Approval', 'by 4pm', '20190722', '20190722', 0, 0)
		, (CONCAT(@UserId, '_20025'), 50, 2, 0, @UserId, 'Collection', 'from 10am', '20190725', '20190725', 0, 0)
		, (CONCAT(@UserId, '_20026'), 10, 0, 1, @UserId, 'Collect', 'after 10am', '20190725', '20190725', 0, 0)
		, (CONCAT(@UserId, '_20026'), 20, 2, 0, @UserId, 'Delivery', null, '20190726', '20190726', 0, 0)
		, (CONCAT(@UserId, '_20027'), 10, 0, 1, @UserId, 'Artwork', 'by 5pm', '20190819', '20190819', 0, 0)
		, (CONCAT(@UserId, '_20027'), 20, 0, 0, @UserId, 'Proofs', 'by noon', '20190820', '20190820', 0, 0)
		, (CONCAT(@UserId, '_20027'), 30, 0, 0, @UserId, 'Approval', 'by 4pm', '20190820', '20190820', 0, 0)
		, (CONCAT(@UserId, '_20027'), 50, 2, 0, @UserId, 'Collection', 'from 10am', '20190827', '20190827', 0, 0)
		, (CONCAT(@UserId, '_20028'), 10, 0, 1, @UserId, 'Collect', 'after 10am', '20190827', '20190827', 0, 0)
		, (CONCAT(@UserId, '_20028'), 20, 2, 0, @UserId, 'Delivery', null, '20190828', '20190828', 0, 0)
		, (CONCAT(@UserId, '_20031'), 10, 0, 2, @UserId, 'Artwork', null, '20190308', '20190308', 0, 0)
		, (CONCAT(@UserId, '_20031'), 20, 0, 2, @UserId, 'Proofs', null, '20190308', '20190310', 0, 0)
		, (CONCAT(@UserId, '_20031'), 30, 0, 2, @UserId, 'Approval', null, '20190311', '20190311', 0, 0)
		, (CONCAT(@UserId, '_20031'), 35, 0, 2, @UserId, 'Paper In', null, '20190316', '20190316', 0, 0)
		, (CONCAT(@UserId, '_20031'), 40, 0, 2, @UserId, 'Delivery', null, '20190328', '20190328', 0, 0)
		, (CONCAT(@UserId, '_20032'), 10, 0, 2, @UserId, 'Delivery', null, '20190316', '20190316', 0, 0)
		, (CONCAT(@UserId, '_20034'), 10, 0, 0, @UserId, 'Artwork', null, '20190708', '20190708', 0, 0)
		, (CONCAT(@UserId, '_20034'), 20, 0, 0, @UserId, 'Proofs', null, '20190709', '20190709', 0, 0)
		, (CONCAT(@UserId, '_20034'), 30, 0, 0, @UserId, 'Approval', null, '20190711', '20190711', 0, 0)
		, (CONCAT(@UserId, '_20034'), 40, 0, 0, @UserId, 'Collection', 'between 10am - 4pm', '20190721', '20190721', 0, 0)
		, (CONCAT(@UserId, '_20035'), 10, 0, 1, @UserId, 'Collection', null, '20190719', '20190721', 0, 0)
		, (CONCAT(@UserId, '_20035'), 20, 2, 0, @UserId, 'Delivery', null, '20190722', '20190722', 0, 0)
		, (CONCAT(@UserId, '_20037'), 10, 0, 1, @UserId, 'Artwork', null, '20190808', '20190808', 0, 0)
		, (CONCAT(@UserId, '_20037'), 20, 0, 0, @UserId, 'Proofs', null, '20190808', '20190808', 0, 0)
		, (CONCAT(@UserId, '_20037'), 30, 0, 0, @UserId, 'Approval', null, '20190808', '20190808', 0, 0)
		, (CONCAT(@UserId, '_20037'), 40, 0, 0, @UserId, 'Collection', 'between 10am - 4pm', '20190808', '20190808', 0, 0)
		, (CONCAT(@UserId, '_20038'), 10, 0, 1, @UserId, 'Collection', null, '20191025', '20191026', 0, 0)
		, (CONCAT(@UserId, '_20038'), 20, 2, 0, @UserId, 'Delivery', null, '20191025', '20191027', 0, 0)
		, (CONCAT(@UserId, '_40026'), 10, 0, 2, @UserId, 'Artwork', null, '20190624', '20190624', 0, 0)
		, (CONCAT(@UserId, '_40026'), 20, 0, 2, @UserId, 'Proofs', null, '20190624', '20190626', 0, 2)
		, (CONCAT(@UserId, '_40026'), 30, 0, 2, @UserId, 'Approval', null, '20190626', '20190701', 0, 3)
		, (CONCAT(@UserId, '_40026'), 40, 2, 2, @UserId, 'Delivery', null, '20190701', '20190708', 0, 1)
		;
		INSERT INTO Project.tbQuote (ProjectCode, Quantity, TotalPrice, RunOnQuantity, RunOnPrice, RunBackQuantity, RunBackPrice)
		VALUES (CONCAT(@UserId, '_10014'), 5000, 1000.0000, 1000, 50.0000, 1000, 45)
		, (CONCAT(@UserId, '_10014'), 10000, 1400.0000, 1000, 48.0000, 1000, 43)
		, (CONCAT(@UserId, '_10014'), 20000, 2200.0000, 1000, 45.0000, 1000, 42)
		;
		INSERT INTO Project.tbAttribute (ProjectCode, Attribute, PrintOrder, AttributeTypeCode, AttributeDescription)
		VALUES (CONCAT(@UserId, '_10000'), 'Extent', 20, 0, '180')
		, (CONCAT(@UserId, '_10000'), 'Finishing', 70, 0, 'Perfect bind with cover drawn on, glued with 6mm hinge, trim flush')
		, (CONCAT(@UserId, '_10000'), 'Origination', 30, 0, 'Complete print ready single page PDFs to be supplied')
		, (CONCAT(@UserId, '_10000'), 'Packing', 80, 0, 'Carton in suitable quantities not to exceed 12kg per carton')
		, (CONCAT(@UserId, '_10000'), 'Paper', 60, 0, 'Cover: 350gsm FSC Silk Coated Board
		Text: 100gsm FSC Smooth Uncoated')
		, (CONCAT(@UserId, '_10000'), 'Printing', 50, 0, 'Cover: Full colour digital printed outer only
		Text: Black only throughout')
		, (CONCAT(@UserId, '_10000'), 'Proofs', 40, 0, 'PDF proofs to be emailed for approval prior to production')
		, (CONCAT(@UserId, '_10000'), 'Size', 10, 0, '210 x 148mm A5 Portrait')
		, (CONCAT(@UserId, '_10007'), 'Finishing', 90, 0, 'Fold, saddlestitch 2 wires and trim flush')
		, (CONCAT(@UserId, '_10007'), 'Note', 150, 0, 'Ensure masthead matches previous issue')
		, (CONCAT(@UserId, '_10007'), 'Origination', 40, 0, 'Complete print ready single page PDFs to be supplied')
		, (CONCAT(@UserId, '_10007'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_10007'), 'Pagination', 20, 0, '16 pages self cover')
		, (CONCAT(@UserId, '_10007'), 'Paper', 80, 0, '130gsm FSC Silk Coated')
		, (CONCAT(@UserId, '_10007'), 'Printing', 60, 0, '4 colour process throughout')
		, (CONCAT(@UserId, '_10007'), 'Proofing', 50, 0, 'PDF proofs to be emailed for approval')
		, (CONCAT(@UserId, '_10007'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_10008'), 'Finishing', 90, 0, 'Fold, saddlestitch 2 wires and trim flush')
		, (CONCAT(@UserId, '_10008'), 'Note', 150, 0, 'Ensure masthead matches previous issue')
		, (CONCAT(@UserId, '_10008'), 'Origination', 40, 0, 'Complete print ready single page PDFs to be supplied')
		, (CONCAT(@UserId, '_10008'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_10008'), 'Pagination', 20, 0, '16 pages self cover')
		, (CONCAT(@UserId, '_10008'), 'Paper', 80, 0, '130gsm FSC Silk Coated')
		, (CONCAT(@UserId, '_10008'), 'Printing', 60, 0, '4 colour process throughout')
		, (CONCAT(@UserId, '_10008'), 'Proofing', 50, 0, 'PDF proofs to be emailed for approval')
		, (CONCAT(@UserId, '_10008'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_10009'), 'Finishing', 90, 0, 'Fold, saddlestitch 2 wires and trim flush')
		, (CONCAT(@UserId, '_10009'), 'Note', 150, 0, 'Ensure masthead matches previous issue')
		, (CONCAT(@UserId, '_10009'), 'Origination', 40, 0, 'Complete print ready single page PDFs to be supplied')
		, (CONCAT(@UserId, '_10009'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_10009'), 'Pagination', 20, 0, '16 pages self cover')
		, (CONCAT(@UserId, '_10009'), 'Paper', 80, 0, '130gsm FSC Silk Coated')
		, (CONCAT(@UserId, '_10009'), 'Printing', 60, 0, '4 colour process throughout')
		, (CONCAT(@UserId, '_10009'), 'Proofing', 50, 0, 'PDF proofs to be emailed for approval')
		, (CONCAT(@UserId, '_10009'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_10010'), 'Finishing', 90, 0, 'Fold, saddlestitch 2 wires and trim flush')
		, (CONCAT(@UserId, '_10010'), 'Note', 150, 0, 'Ensure masthead matches previous issue')
		, (CONCAT(@UserId, '_10010'), 'Origination', 40, 0, 'Complete print ready single page PDFs to be supplied')
		, (CONCAT(@UserId, '_10010'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_10010'), 'Pagination', 20, 0, '16 pages self cover')
		, (CONCAT(@UserId, '_10010'), 'Paper', 80, 0, '130gsm FSC Silk Coated')
		, (CONCAT(@UserId, '_10010'), 'Printing', 60, 0, '4 colour process throughout')
		, (CONCAT(@UserId, '_10010'), 'Proofing', 50, 0, 'PDF proofs to be emailed for approval')
		, (CONCAT(@UserId, '_10010'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_10011'), 'Finishing', 90, 0, 'Fold, saddlestitch 2 wires and trim flush')
		, (CONCAT(@UserId, '_10011'), 'Note', 150, 0, 'Ensure masthead matches previous issue')
		, (CONCAT(@UserId, '_10011'), 'Origination', 40, 0, 'Complete print ready single page PDFs to be supplied')
		, (CONCAT(@UserId, '_10011'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_10011'), 'Pagination', 20, 0, '16 pages self cover')
		, (CONCAT(@UserId, '_10011'), 'Paper', 80, 0, '130gsm FSC Silk Coated')
		, (CONCAT(@UserId, '_10011'), 'Printing', 60, 0, '4 colour process throughout')
		, (CONCAT(@UserId, '_10011'), 'Proofing', 50, 0, 'PDF proofs to be emailed for approval')
		, (CONCAT(@UserId, '_10011'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_10012'), 'Finishing', 90, 0, 'Fold, saddlestitch 2 wires and trim flush')
		, (CONCAT(@UserId, '_10012'), 'Note', 150, 0, 'Ensure masthead matches previous issue')
		, (CONCAT(@UserId, '_10012'), 'Origination', 40, 0, 'Complete print ready single page PDFs to be supplied')
		, (CONCAT(@UserId, '_10012'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_10012'), 'Pagination', 20, 0, '16 pages self cover')
		, (CONCAT(@UserId, '_10012'), 'Paper', 80, 0, '130gsm FSC Silk Coated')
		, (CONCAT(@UserId, '_10012'), 'Printing', 60, 0, '4 colour process throughout')
		, (CONCAT(@UserId, '_10012'), 'Proofing', 50, 0, 'PDF proofs to be emailed for approval')
		, (CONCAT(@UserId, '_10012'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_10013'), 'Finishing', 90, 0, 'Fold, saddlestitch 2 wires and trim flush')
		, (CONCAT(@UserId, '_10013'), 'Note', 150, 0, 'Ensure masthead matches previous issue')
		, (CONCAT(@UserId, '_10013'), 'Origination', 40, 0, 'Complete print ready single page PDFs to be supplied')
		, (CONCAT(@UserId, '_10013'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_10013'), 'Pagination', 20, 0, '16 pages self cover')
		, (CONCAT(@UserId, '_10013'), 'Paper', 80, 0, '130gsm FSC Silk Coated')
		, (CONCAT(@UserId, '_10013'), 'Printing', 60, 0, '4 colour process throughout')
		, (CONCAT(@UserId, '_10013'), 'Proofing', 50, 0, 'PDF proofs to be emailed for approval')
		, (CONCAT(@UserId, '_10013'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_10014'), 'Finishing', 90, 0, 'Fold, saddlestitch 2 wires and trim flush')
		, (CONCAT(@UserId, '_10014'), 'Note', 150, 0, 'Ensure masthead matches previous issue')
		, (CONCAT(@UserId, '_10014'), 'Origination', 40, 0, 'Complete print ready single page PDFs to be supplied')
		, (CONCAT(@UserId, '_10014'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_10014'), 'Pagination', 20, 0, '16 pages self cover')
		, (CONCAT(@UserId, '_10014'), 'Paper', 80, 0, '130gsm FSC Silk Coated')
		, (CONCAT(@UserId, '_10014'), 'Printing', 60, 0, '4 colour process throughout')
		, (CONCAT(@UserId, '_10014'), 'Proofing', 50, 0, 'PDF proofs to be emailed for approval')
		, (CONCAT(@UserId, '_10014'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_10015'), 'Description', 10, 0, 'Outer carton 0201 Glued')
		, (CONCAT(@UserId, '_10015'), 'Finishing', 90, 0, 'Die cut, glue and form as flat carton')
		, (CONCAT(@UserId, '_10015'), 'Material', 80, 0, 'B150K150T corrugated single walled')
		, (CONCAT(@UserId, '_10015'), 'Packing', 100, 0, 'Bundle in 10s, palletise in 250s')
		, (CONCAT(@UserId, '_10015'), 'Printing', 60, 0, 'Plain unprinted cartons')
		, (CONCAT(@UserId, '_10015'), 'Tolerance', 110, 0, '+/-10% tolerance applies, quantity delivered will be invoiced at the agreed unit rate')
		, (CONCAT(@UserId, '_10015'), 'Trim Size', 30, 0, 'Internal dimensions: Height 140 x Width 170 x Length 200mm')
		, (CONCAT(@UserId, '_10017'), 'Finishing', 70, 0, 'Seed prize sheets into bulk master sheets, trim to size and pack into cartons in 1,000s.')
		, (CONCAT(@UserId, '_10017'), 'Labelling', 80, 0, 'Apply timestamped label to short end of each carton')
		, (CONCAT(@UserId, '_10017'), 'Latexing', 50, 0, 'Screen print silver latex in 9 positions, common to all variants')
		, (CONCAT(@UserId, '_10017'), 'Litho Printing', 40, 0, 'Print four colour process to face with slip plate for black text changes to create 40 variants (split as spreadsheet supplied). Reverse print black line only. Apply inline slip varnish to face only.')
		, (CONCAT(@UserId, '_10017'), 'Origination', 20, 0, 'PDFs supplied for 40 variants')
		, (CONCAT(@UserId, '_10017'), 'Paper', 60, 0, '280gsm 1-sided gloss coated card as sampled')
		, (CONCAT(@UserId, '_10017'), 'Proofing', 30, 0, 'Proof master sheet with single PDFs of other 39 variants')
		, (CONCAT(@UserId, '_10017'), 'Trim Size', 10, 0, '100 x 75mm')
		, (CONCAT(@UserId, '_10018'), 'Cover Finish', 70, 0, 'Matt UV varnish')
		, (CONCAT(@UserId, '_10018'), 'Finishing', 90, 0, 'Fold text, threadsew in 16pp sections, case-in with printed paper case')
		, (CONCAT(@UserId, '_10018'), 'Material', 80, 0, 'Cover: 150gsm FSC Silk over 2000 micron smooth greyboard
		Text: 150gsm FSC Silk')
		, (CONCAT(@UserId, '_10018'), 'Origination', 40, 0, 'PDFs supplied as single pages to our specification')
		, (CONCAT(@UserId, '_10018'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_10018'), 'Pagination', 20, 0, '72pp text + 4pp cover')
		, (CONCAT(@UserId, '_10018'), 'Printing', 60, 0, 'Cover: 4 colour process to outer only
		Text: 4 colour process throughout')
		, (CONCAT(@UserId, '_10018'), 'Proofing', 50, 0, 'Ripped PDFs to be emailed for approval prior to printing')
		, (CONCAT(@UserId, '_10018'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_10019'), 'Cover Finish', 70, 0, 'Matt UV varnish')
		, (CONCAT(@UserId, '_10019'), 'Finishing', 90, 0, 'Fold text, threadsew in 16pp sections, case-in with printed paper case')
		, (CONCAT(@UserId, '_10019'), 'Material', 80, 0, 'Cover: 150gsm FSC Silk over 2000 micron smooth greyboard
		Text: 150gsm FSC Silk')
		, (CONCAT(@UserId, '_10019'), 'Origination', 40, 0, 'Straight reprint from July 19 order')
		, (CONCAT(@UserId, '_10019'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_10019'), 'Pagination', 20, 0, '72pp text + 4pp cover')
		, (CONCAT(@UserId, '_10019'), 'Printing', 60, 0, 'Cover: 4 colour process to outer only
		Text: 4 colour process throughout')
		, (CONCAT(@UserId, '_10019'), 'Proofing', 50, 0, 'None required')
		, (CONCAT(@UserId, '_10019'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_20000'), 'Extent', 20, 0, '180')
		, (CONCAT(@UserId, '_20000'), 'File Copies', 90, 0, 'One file copy to be sent to us by First Class post on despatch of main order')
		, (CONCAT(@UserId, '_20000'), 'Finishing', 70, 0, 'Perfect bind with cover drawn on, glued with 6mm hinge, trim flush')
		, (CONCAT(@UserId, '_20000'), 'Origination', 30, 0, 'Complete print ready single page PDFs to be supplied')
		, (CONCAT(@UserId, '_20000'), 'Packing', 80, 0, 'Carton in suitable quantities not to exceed 12kg per carton')
		, (CONCAT(@UserId, '_20000'), 'Paper', 60, 0, 'Cover: 350gsm FSC Silk Coated Board
		Text: 100gsm FSC Smooth Uncoated')
		, (CONCAT(@UserId, '_20000'), 'Printing', 50, 0, 'Cover: Full colour digital printed outer only
		Text: Black only throughout')
		, (CONCAT(@UserId, '_20000'), 'Proofs', 40, 0, 'PDF proofs to be emailed for approval prior to production')
		, (CONCAT(@UserId, '_20000'), 'Size', 10, 0, '210 x 148mm A5 Portrait')
		, (CONCAT(@UserId, '_20010'), 'File Copies', 160, 0, 'Post 3 file copies by First Class post on completion of order')
		, (CONCAT(@UserId, '_20010'), 'Finishing', 90, 0, 'Fold, saddlestitch 2 wires and trim flush')
		, (CONCAT(@UserId, '_20010'), 'Note', 150, 0, 'Ensure masthead matches previous issue')
		, (CONCAT(@UserId, '_20010'), 'Origination', 40, 0, 'Complete print ready single page PDFs to be supplied')
		, (CONCAT(@UserId, '_20010'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_20010'), 'Pagination', 20, 0, '16 pages self cover')
		, (CONCAT(@UserId, '_20010'), 'Paper', 80, 0, '130gsm FSC Silk Coated')
		, (CONCAT(@UserId, '_20010'), 'Printing', 60, 0, '4 colour process throughout')
		, (CONCAT(@UserId, '_20010'), 'Proofing', 50, 0, 'PDF proofs to be emailed for approval')
		, (CONCAT(@UserId, '_20010'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_20011'), 'Description', 10, 0, '2 pallets x 350kg per pallet, overnight delivery')
		, (CONCAT(@UserId, '_20013'), 'File Copies', 160, 0, 'Post 3 file copies by First Class post on completion of order')
		, (CONCAT(@UserId, '_20013'), 'Finishing', 90, 0, 'Fold, saddlestitch 2 wires and trim flush')
		, (CONCAT(@UserId, '_20013'), 'Note', 150, 0, 'Ensure masthead matches previous issue')
		, (CONCAT(@UserId, '_20013'), 'Origination', 40, 0, 'Complete print ready single page PDFs to be supplied')
		, (CONCAT(@UserId, '_20013'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_20013'), 'Pagination', 20, 0, '16 pages self cover')
		, (CONCAT(@UserId, '_20013'), 'Paper', 80, 0, '130gsm FSC Silk Coated')
		, (CONCAT(@UserId, '_20013'), 'Printing', 60, 0, '4 colour process throughout')
		, (CONCAT(@UserId, '_20013'), 'Proofing', 50, 0, 'PDF proofs to be emailed for approval')
		, (CONCAT(@UserId, '_20013'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_20014'), 'Description', 10, 0, '2 pallets x 350kg per pallet, overnight delivery')
		, (CONCAT(@UserId, '_20015'), 'File Copies', 160, 0, 'Post 3 file copies by First Class post on completion of order')
		, (CONCAT(@UserId, '_20015'), 'Finishing', 90, 0, 'Fold, saddlestitch 2 wires and trim flush')
		, (CONCAT(@UserId, '_20015'), 'Note', 150, 0, 'Ensure masthead matches previous issue')
		, (CONCAT(@UserId, '_20015'), 'Origination', 40, 0, 'Complete print ready single page PDFs to be supplied')
		, (CONCAT(@UserId, '_20015'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_20015'), 'Pagination', 20, 0, '16 pages self cover')
		, (CONCAT(@UserId, '_20015'), 'Paper', 80, 0, '130gsm FSC Silk Coated')
		, (CONCAT(@UserId, '_20015'), 'Printing', 60, 0, '4 colour process throughout')
		, (CONCAT(@UserId, '_20015'), 'Proofing', 50, 0, 'PDF proofs to be emailed for approval')
		, (CONCAT(@UserId, '_20015'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_20016'), 'Description', 10, 0, '2 pallets x 350kg per pallet, overnight delivery')
		, (CONCAT(@UserId, '_20017'), 'File Copies', 160, 0, 'Post 3 file copies by First Class post on completion of order')
		, (CONCAT(@UserId, '_20017'), 'Finishing', 90, 0, 'Fold, saddlestitch 2 wires and trim flush')
		, (CONCAT(@UserId, '_20017'), 'Note', 150, 0, 'Ensure masthead matches previous issue')
		, (CONCAT(@UserId, '_20017'), 'Origination', 40, 0, 'Complete print ready single page PDFs to be supplied')
		, (CONCAT(@UserId, '_20017'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_20017'), 'Pagination', 20, 0, '16 pages self cover')
		, (CONCAT(@UserId, '_20017'), 'Paper', 80, 0, '130gsm FSC Silk Coated')
		, (CONCAT(@UserId, '_20017'), 'Printing', 60, 0, '4 colour process throughout')
		, (CONCAT(@UserId, '_20017'), 'Proofing', 50, 0, 'PDF proofs to be emailed for approval')
		, (CONCAT(@UserId, '_20017'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_20018'), 'Description', 10, 0, '2 pallets x 350kg per pallet, overnight delivery')
		, (CONCAT(@UserId, '_20019'), 'File Copies', 160, 0, 'Post 3 file copies by First Class post on completion of order')
		, (CONCAT(@UserId, '_20019'), 'Finishing', 90, 0, 'Fold, saddlestitch 2 wires and trim flush')
		, (CONCAT(@UserId, '_20019'), 'Note', 150, 0, 'Ensure masthead matches previous issue')
		, (CONCAT(@UserId, '_20019'), 'Origination', 40, 0, 'Complete print ready single page PDFs to be supplied')
		, (CONCAT(@UserId, '_20019'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_20019'), 'Pagination', 20, 0, '16 pages self cover')
		, (CONCAT(@UserId, '_20019'), 'Paper', 80, 0, '130gsm FSC Silk Coated')
		, (CONCAT(@UserId, '_20019'), 'Printing', 60, 0, '4 colour process throughout')
		, (CONCAT(@UserId, '_20019'), 'Proofing', 50, 0, 'PDF proofs to be emailed for approval')
		, (CONCAT(@UserId, '_20019'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_20020'), 'Description', 10, 0, '2 pallets x 350kg per pallet, overnight delivery')
		, (CONCAT(@UserId, '_20021'), 'File Copies', 160, 0, 'Post 3 file copies by First Class post on completion of order')
		, (CONCAT(@UserId, '_20021'), 'Finishing', 90, 0, 'Fold, saddlestitch 2 wires and trim flush')
		, (CONCAT(@UserId, '_20021'), 'Note', 150, 0, 'Ensure masthead matches previous issue')
		, (CONCAT(@UserId, '_20021'), 'Origination', 40, 0, 'Complete print ready single page PDFs to be supplied')
		, (CONCAT(@UserId, '_20021'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_20021'), 'Pagination', 20, 0, '16 pages self cover')
		, (CONCAT(@UserId, '_20021'), 'Paper', 80, 0, '130gsm FSC Silk Coated')
		, (CONCAT(@UserId, '_20021'), 'Printing', 60, 0, '4 colour process throughout')
		, (CONCAT(@UserId, '_20021'), 'Proofing', 50, 0, 'PDF proofs to be emailed for approval')
		, (CONCAT(@UserId, '_20021'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_20022'), 'Description', 10, 0, '2 pallets x 350kg per pallet, overnight delivery')
		, (CONCAT(@UserId, '_20025'), 'File Copies', 160, 0, 'Post 3 file copies by First Class post on completion of order')
		, (CONCAT(@UserId, '_20025'), 'Finishing', 90, 0, 'Fold, saddlestitch 2 wires and trim flush')
		, (CONCAT(@UserId, '_20025'), 'Note', 150, 0, 'Ensure masthead matches previous issue')
		, (CONCAT(@UserId, '_20025'), 'Origination', 40, 0, 'Complete print ready single page PDFs to be supplied')
		, (CONCAT(@UserId, '_20025'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_20025'), 'Pagination', 20, 0, '16 pages self cover')
		, (CONCAT(@UserId, '_20025'), 'Paper', 80, 0, '130gsm FSC Silk Coated')
		, (CONCAT(@UserId, '_20025'), 'Printing', 60, 0, '4 colour process throughout')
		, (CONCAT(@UserId, '_20025'), 'Proofing', 50, 0, 'PDF proofs to be emailed for approval')
		, (CONCAT(@UserId, '_20025'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_20026'), 'Description', 10, 0, '2 pallets x 350kg per pallet, overnight delivery')
		, (CONCAT(@UserId, '_20027'), 'File Copies', 160, 0, 'Post 3 file copies by First Class post on completion of order')
		, (CONCAT(@UserId, '_20027'), 'Finishing', 90, 0, 'Fold, saddlestitch 2 wires and trim flush')
		, (CONCAT(@UserId, '_20027'), 'Note', 150, 0, 'Ensure masthead matches previous issue')
		, (CONCAT(@UserId, '_20027'), 'Origination', 40, 0, 'Complete print ready single page PDFs to be supplied')
		, (CONCAT(@UserId, '_20027'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_20027'), 'Pagination', 20, 0, '16 pages self cover')
		, (CONCAT(@UserId, '_20027'), 'Paper', 80, 0, '130gsm FSC Silk Coated')
		, (CONCAT(@UserId, '_20027'), 'Printing', 60, 0, '4 colour process throughout')
		, (CONCAT(@UserId, '_20027'), 'Proofing', 50, 0, 'PDF proofs to be emailed for approval')
		, (CONCAT(@UserId, '_20027'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_20028'), 'Description', 10, 0, '2 pallets x 350kg per pallet, overnight delivery')
		, (CONCAT(@UserId, '_20029'), 'Description', 10, 0, 'Outer carton 0201 Glued')
		, (CONCAT(@UserId, '_20029'), 'Finishing', 90, 0, 'Die cut, glue and form as flat carton')
		, (CONCAT(@UserId, '_20029'), 'Material', 80, 0, 'B150K150T corrugated single walled')
		, (CONCAT(@UserId, '_20029'), 'Packing', 100, 0, 'Bundle in 10s, palletise in 250s')
		, (CONCAT(@UserId, '_20029'), 'Printing', 60, 0, 'Plain unprinted cartons')
		, (CONCAT(@UserId, '_20029'), 'Tolerance', 110, 0, '+/-10% tolerance applies, quantity delivered will be invoiced at the agreed unit rate')
		, (CONCAT(@UserId, '_20029'), 'Trim Size', 30, 0, 'Internal dimensions: Height 140 x Width 170 x Length 200mm')
		, (CONCAT(@UserId, '_20031'), 'Delivery Note', 90, 0, 'Please ensure that you use our delivery note supplied')
		, (CONCAT(@UserId, '_20031'), 'File Copies', 100, 0, '2 complete Voided sets of cards x 40 variants to be sent to us on completion of order')
		, (CONCAT(@UserId, '_20031'), 'Finishing', 70, 0, 'Seed prize sheets into bulk master sheets, trim to size and pack into cartons in 1,000s.')
		, (CONCAT(@UserId, '_20031'), 'Labelling', 80, 0, 'Apply timestamped label to short end of each carton')
		, (CONCAT(@UserId, '_20031'), 'Latexing', 50, 0, 'Screen print silver latex in 9 positions, common to all variants')
		, (CONCAT(@UserId, '_20031'), 'Litho Printing', 40, 0, 'Print four colour process to face with slip plate for black text changes to create 40 variants (split as spreadsheet supplied). Reverse print black line only. Apply inline slip varnish to face only.')
		, (CONCAT(@UserId, '_20031'), 'Origination', 20, 0, 'PDFs supplied for 40 variants')
		, (CONCAT(@UserId, '_20031'), 'Paper', 60, 0, '280gsm 1-sided gloss coated card supplied - 13 tonnes in sheet size 640 x 900mm')
		, (CONCAT(@UserId, '_20031'), 'Proofing', 30, 0, 'Proof master sheet with single PDFs of other 39 variants')
		, (CONCAT(@UserId, '_20031'), 'Trim Size', 10, 0, '100 x 75mm')
		, (CONCAT(@UserId, '_20032'), 'Paper', 60, 0, '280gsm 1-sided Special gloss coated card - 13 tonnes in sheet size 640 x 900mm (80,600 ssheets)')
		, (CONCAT(@UserId, '_20034'), 'Cover Finish', 70, 0, 'Matt UV varnish')
		, (CONCAT(@UserId, '_20034'), 'File Copies', 200, 0, '2 file copies to be posted to us on completion of order')
		, (CONCAT(@UserId, '_20034'), 'Finishing', 90, 0, 'Fold text, threadsew in 16pp sections, case-in with printed paper case')
		, (CONCAT(@UserId, '_20034'), 'Material', 80, 0, 'Cover: 150gsm FSC Silk over 2000 micron smooth greyboard
		Text: 150gsm FSC Silk')
		, (CONCAT(@UserId, '_20034'), 'Origination', 40, 0, 'PDFs supplied as single pages to our specification')
		, (CONCAT(@UserId, '_20034'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_20034'), 'Pagination', 20, 0, '72pp text + 4pp cover')
		, (CONCAT(@UserId, '_20034'), 'Printing', 60, 0, 'Cover: 4 colour process to outer only
		Text: 4 colour process throughout')
		, (CONCAT(@UserId, '_20034'), 'Proofing', 50, 0, 'Ripped PDFs to be emailed for approval prior to printing')
		, (CONCAT(@UserId, '_20034'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_20035'), 'Collection', 20, 0, 'Between 10am - 4pm')
		, (CONCAT(@UserId, '_20035'), 'Description', 10, 0, '1 overnight pallet')
		, (CONCAT(@UserId, '_20035'), 'Note', 30, 1, 'Please call warehouse on 0785 456756 on arrival to enable access')
		, (CONCAT(@UserId, '_20037'), 'Cover Finish', 70, 0, 'Matt UV varnish')
		, (CONCAT(@UserId, '_20037'), 'File Copies', 200, 0, '2 file copies to be posted to us on completion of order')
		, (CONCAT(@UserId, '_20037'), 'Finishing', 90, 0, 'Fold text, threadsew in 16pp sections, case-in with printed paper case')
		, (CONCAT(@UserId, '_20037'), 'Material', 80, 0, 'Cover: 150gsm FSC Silk over 2000 micron smooth greyboard
		Text: 150gsm FSC Silk')
		, (CONCAT(@UserId, '_20037'), 'Origination', 40, 0, 'Straight reprint from July 19 order')
		, (CONCAT(@UserId, '_20037'), 'Packing', 100, 0, 'Carton pack in suitable quantities not to exceed 12kg')
		, (CONCAT(@UserId, '_20037'), 'Pagination', 20, 0, '72pp text + 4pp cover')
		, (CONCAT(@UserId, '_20037'), 'Printing', 60, 0, 'Cover: 4 colour process to outer only
		Text: 4 colour process throughout')
		, (CONCAT(@UserId, '_20037'), 'Proofing', 50, 0, 'None required')
		, (CONCAT(@UserId, '_20037'), 'Trim Size', 30, 0, '297 x 210mm A4 Portrait')
		, (CONCAT(@UserId, '_20038'), 'Collection', 20, 0, 'Between 10am - 4pm')
		, (CONCAT(@UserId, '_20038'), 'Description', 10, 0, '8 pallets')
		, (CONCAT(@UserId, '_20038'), 'Note', 30, 1, 'Please call warehouse on 0785 456756 on arrival to enable access')
		, (CONCAT(@UserId, '_40005'), 'Description', 10, 0, 'Monthly wages')
		, (CONCAT(@UserId, '_40006'), 'Description', 10, 0, 'Monthly wages')
		, (CONCAT(@UserId, '_40007'), 'Description', 10, 0, 'Monthly wages')
		, (CONCAT(@UserId, '_40008'), 'Description', 10, 0, 'Monthly wages')
		, (CONCAT(@UserId, '_40009'), 'Description', 10, 0, 'Monthly wages')
		, (CONCAT(@UserId, '_40010'), 'Description', 10, 0, 'Monthly wages')
		, (CONCAT(@UserId, '_40011'), 'Description', 10, 0, 'Monthly wages')
		, (CONCAT(@UserId, '_40012'), 'Description', 10, 0, 'Monthly wages')
		, (CONCAT(@UserId, '_40013'), 'Description', 10, 0, 'Monthly wages')
		, (CONCAT(@UserId, '_40014'), 'Description', 10, 0, 'Monthly wages')
		, (CONCAT(@UserId, '_40015'), 'Description', 10, 0, 'Monthly wages')
		, (CONCAT(@UserId, '_40016'), 'Description', 10, 0, 'Monthly wages')
		;

		UPDATE App.tbRegister SET NextNumber = 40047 WHERE RegisterName = 'Expenses';
		UPDATE App.tbRegister SET NextNumber = 30003 WHERE RegisterName = 'Project';
		UPDATE App.tbRegister SET NextNumber = 20039 WHERE RegisterName = 'Purchase Order';
		UPDATE App.tbRegister SET NextNumber = 10020 WHERE RegisterName = 'Sales Order';

		DECLARE @OffsetMonth INT = (SELECT DATEDIFF(MONTH, '20190801', CURRENT_TIMESTAMP));

		UPDATE Project.tbProject SET ActionOn = App.fnAdjustToCalendar(DATEADD(MONTH, @OffsetMonth, ActionOn), 0);
		UPDATE Project.tbProject SET ActionedOn = ActionOn;

		DECLARE @ProjectCode NVARCHAR(10);
		DECLARE live_Projects CURSOR FOR
			SELECT  Project.tbProject.ProjectCode
			FROM Project.tbProject INNER JOIN
				Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
				Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE        (Cash.tbCategory.CashPolarityCode = 1) AND (Project.tbProject.ProjectStatusCode = 1);

		OPEN live_Projects;
		FETCH NEXT FROM live_Projects INTO @ProjectCode;
		WHILE @@FETCH_STATUS = 0
		BEGIN
			EXEC Project.proc_Schedule @ParentProjectCode=@ProjectCode;
			FETCH NEXT FROM live_Projects INTO @ProjectCode;
		END

		CLOSE live_Projects;
		DEALLOCATE live_Projects;

		IF (@InvoiceOrders = 0)
			GOTO CommitTran;

		INSERT INTO Invoice.tbInvoice (InvoiceNumber, UserId, SubjectCode, InvoiceTypeCode, InvoiceStatusCode, InvoicedOn, ExpectedOn, DueOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue, PaymentTerms, Notes, Printed, Spooled)
		VALUES (CONCAT('010000.', @UserId), @UserId, 'CDCUST', 0, 1, '20190126', '20190228', '20190228', 2000, 400, 2000, 400, '30 days end of month following date of invoice', null, 0, 0)
		, (CONCAT('010001.', @UserId), @UserId, 'CDCUST', 0, 1, '20190225', '20190329', '20190329', 2000, 400, 2000, 400, '30 days end of month following date of invoice', null, 0, 0)
		, (CONCAT('010002.', @UserId), @UserId, 'CDCUST', 0, 1, '20190328', '20190430', '20190430', 2000, 400, 2000, 400, '30 days end of month following date of invoice', null, 0, 0)
		, (CONCAT('010003.', @UserId), @UserId, 'CDCUST', 0, 1, '20190428', '20190531', '20190531', 2000, 400, 2000, 400, '30 days end of month following date of invoice', null, 0, 0)
		, (CONCAT('010004.', @UserId), @UserId, 'CDCUST', 0, 1, '20190525', '20190628', '20190628', 2000, 400, 2000, 400, '30 days end of month following date of invoice', null, 0, 0)
		, (CONCAT('010005.', @UserId), @UserId, 'BUSOWN', 0, 1, '20190101', '20190101', '20190101', 10000.0000, 0.0000, 10000.0000, 0.0000, null, null, 0, 0)
		, (CONCAT('010006.', @UserId), @UserId, 'EFCUST', 0, 1, '20190330', '20190514', '20190429', 18500.0000, 3700.0000, 18500.0000, 3700.0000, '30 days from date of invoice', null, 0, 0)
		, (CONCAT('010007.', @UserId), @UserId, 'BUSOWN', 0, 1, '20190101', '20190101', '20190101', 15000.0000, 0.0000, 15000.0000, 0.0000, null, null, 0, 0)
		, (CONCAT('010008.', @UserId), @UserId, 'HOME', 0, 1, '20190415', '20190415', '20190415', 5000.0000, 0.0000, 5000.0000, 0.0000, null, null, 0, 0)
		, (CONCAT('010009.', @UserId), @UserId, 'HOME', 0, 1, '20190601', '20190531', '20190531', 5000.0000, 0.0000, 5000.0000, 0.0000, null, null, 0, 0)
		, (CONCAT('010010.', @UserId), @UserId, 'HOME', 0, 1, '20190731', '20190731', '20190731', 1000.0000, 0.0000, 1000.0000, 0.0000, null, null, 0, 0)
		, (CONCAT('010011.', @UserId), @UserId, 'CDCUST', 0, 1, '20190822', '20190930', '20190930', 2000, 400, 2000, 400, '30 days end of month following date of invoice', null, 0, 0)
		, (CONCAT('030000.', @UserId), @UserId, 'SUPTWO', 2, 1, '20190125', '20190228', '20190228', 650.0000, 0.0000, 650.0000, 0.0000, '30 days end of month following date of invoice', 'Invoice 122112', 0, 0)
		, (CONCAT('030001.', @UserId), @UserId, 'TRACOM', 2, 1, '20190126', '20190228', '20190228', 150.0000, 30.0000, 150.0000, 30.0000, '30 days end of month following date of invoice', 'Invoice INV122222', 0, 0)
		, (CONCAT('030002.', @UserId), @UserId, 'SUPTWO', 2, 1, '20190224', '20190329', '20190329', 650.0000, 0.0000, 650.0000, 0.0000, '30 days end of month following date of invoice', 'Invoice 122250', 0, 0)
		, (CONCAT('030003.', @UserId), @UserId, 'TRACOM', 2, 1, '20190225', '20190329', '20190329', 150.0000, 30.0000, 150.0000, 30.0000, '30 days end of month following date of invoice', 'Invoice INV123456', 0, 0)
		, (CONCAT('030004.', @UserId), @UserId, 'SUPTWO', 2, 1, '20190327', '20190430', '20190430', 650.0000, 0.0000, 650.0000, 0.0000, '30 days end of month following date of invoice', 'Invoice 122501', 0, 0)
		, (CONCAT('030005.', @UserId), @UserId, 'TRACOM', 2, 1, '20190328', '20190430', '20190430', 150.0000, 30.0000, 150.0000, 30.0000, '30 days end of month following date of invoice', 'Invoice INV124555', 0, 0)
		, (CONCAT('030006.', @UserId), @UserId, 'SUPTWO', 2, 1, '20190427', '20190531', '20190531', 650.0000, 0.0000, 650.0000, 0.0000, '30 days end of month following date of invoice', 'Invoice 123011', 0, 0)
		, (CONCAT('030007.', @UserId), @UserId, 'TRACOM', 2, 1, '20190428', '20190531', '20190531', 150.0000, 30.0000, 150.0000, 30.0000, '30 days end of month following date of invoice', 'Invoice INV124212', 0, 0)
		, (CONCAT('030008.', @UserId), @UserId, 'SUPTWO', 2, 1, '20190524', '20190628', '20190628', 650.0000, 0.0000, 650.0000, 0.0000, '30 days end of month following date of invoice', 'Invoice 124100', 0, 0)
		, (CONCAT('030009.', @UserId), @UserId, 'TRACOM', 2, 1, '20190525', '20190628', '20190628', 150.0000, 30.0000, 150.0000, 30.0000, '30 days end of month following date of invoice', 'Invoice INV190112', 0, 0)
		, (CONCAT('030010.', @UserId), @UserId, 'BUSOWN', 2, 1, '20190131', '20190131', '20190131', 83.9000, 0.8000, 83.9000, 0.8000, 'Expenses paid end of month', null, 0, 0)
		, (CONCAT('030011.', @UserId), @UserId, 'BUSOWN', 2, 1, '20190708', '20190731', '20190731', 1000.0000, 0.0000, 1000.0000, 0.0000, 'Expenses paid end of month', null, 0, 0)
		, (CONCAT('030012.', @UserId), @UserId, 'BUSOWN', 2, 1, '20190708', '20190731', '20190731', 1000.0000, 0.0000, 1000.0000, 0.0000, 'Expenses paid end of month', null, 0, 0)
		, (CONCAT('030013.', @UserId), @UserId, 'BUSOWN', 2, 1, '20190708', '20190731', '20190731', 1000.0000, 0.0000, 1000.0000, 0.0000, 'Expenses paid end of month', null, 0, 0)
		, (CONCAT('030014.', @UserId), @UserId, 'BUSOWN', 2, 1, '20190708', '20190731', '20190731', 1000.0000, 0.0000, 1000.0000, 0.0000, 'Expenses paid end of month', null, 0, 0)
		, (CONCAT('030015.', @UserId), @UserId, 'BUSOWN', 2, 1, '20190708', '20190731', '20190731', 1000.0000, 0.0000, 1000.0000, 0.0000, 'Expenses paid end of month', null, 0, 0)
		, (CONCAT('030016.', @UserId), @UserId, 'BUSOWN', 2, 1, '20190708', '20190731', '20190731', 1000.0000, 0.0000, 1000.0000, 0.0000, 'Expenses paid end of month', null, 0, 0)
		, (CONCAT('030017.', @UserId), @UserId, 'SUPTWO', 2, 1, '20190329', '20190430', '20190430', 6000.0000, 1200.0000, 6000.0000, 1200.0000, '30 days end of month following date of invoice', null, 0, 0)
		, (CONCAT('030018.', @UserId), @UserId, 'THEPAP', 2, 1, '20190416', '20190516', '20190416', 9750.0000, 1950.0000, 9750.0000, 1950.0000, '30 days from date of invoice', null, 0, 0)
		, (CONCAT('030019.', @UserId), @UserId, 'BUSOWN', 2, 1, '20190228', '20190228', '20190228', 118.4500, 0.0000, 118.4500, 0.0000, 'Expenses paid end of month', null, 0, 0)
		, (CONCAT('030020.', @UserId), @UserId, 'BUSOWN', 2, 1, '20190329', '20190329', '20190329', 105.1000, 0.0000, 105.1000, 0.0000, 'Expenses paid end of month', null, 0, 0)
		, (CONCAT('030021.', @UserId), @UserId, 'HOME', 2, 1, '20190415', '20190415', '20190415', 5000.0000, 0.0000, 5000.0000, 0.0000, null, null, 0, 0)
		, (CONCAT('030022.', @UserId), @UserId, 'BUSOWN', 2, 1, '20190430', '20190430', '20190430', 169.0000, 0.0000, 169.0000, 0.0000, 'Expenses paid end of month', null, 0, 0)
		, (CONCAT('030023.', @UserId), @UserId, 'BUSOWN', 2, 1, '20190531', '20190531', '20190531', 211.7500, 3.6000, 211.7500, 3.6000, 'Expenses paid end of month', null, 0, 0)
		, (CONCAT('030024.', @UserId), @UserId, 'BUSOWN', 2, 1, '20190628', '20190628', '20190628', 202.4000, 1.0000, 202.4000, 1.0000, 'Expenses paid end of month', null, 0, 0)
		, (CONCAT('030025.', @UserId), @UserId, 'HOME', 2, 1, '20190601', '20190531', '20190531', 5000.0000, 0.0000, 5000.0000, 0.0000, null, null, 0, 0)
		, (CONCAT('030026.', @UserId), @UserId, 'TELPRO', 2, 1, '20190719', '20190719', '20190719', 40.0000, 8.0000, 40.0000, 8.0000, 'Paid with order', null, 0, 0)
		, (CONCAT('030027.', @UserId), @UserId, 'TELPRO', 2, 1, '20190719', '20190719', '20190719', 39.6000, 7.9200, 39.6000, 7.9200, 'Paid with order', null, 0, 0)
		, (CONCAT('030028.', @UserId), @UserId, 'TELPRO', 2, 1, '20190719', '20190719', '20190719', 43.1200, 8.6200, 43.1200, 8.6200, 'Paid with order', null, 0, 0)
		, (CONCAT('030029.', @UserId), @UserId, 'TELPRO', 2, 1, '20190719', '20190719', '20190719', 43.5200, 8.7000, 43.5200, 8.7000, 'Paid with order', null, 0, 0)
		, (CONCAT('030030.', @UserId), @UserId, 'TELPRO', 2, 1, '20190719', '20190719', '20190719', 42.5200, 8.5000, 42.5200, 8.5000, 'Paid with order', null, 0, 0)
		, (CONCAT('030031.', @UserId), @UserId, 'TELPRO', 2, 1, '20190719', '20190719', '20190719', 41.1500, 8.2300, 41.1500, 8.2300, 'Paid with order', null, 0, 0)
		, (CONCAT('030033.', @UserId), @UserId, 'TELPRO', 2, 1, '20190822', '20190822', '20190822', 40.0000, 8.0000, 40.0000, 8.0000, 'Paid with order', null, 0, 0)
		, (CONCAT('030034.', @UserId), @UserId, 'HOME', 2, 1, '20190731', '20190731', '20190731', 1000.0000, 0.0000, 1000.0000, 0.0000, null, null, 0, 0)
		, (CONCAT('030035.', @UserId), @UserId, 'BUSOWN', 2, 1, '20190731', '20190731', '20190731', 1000.0000, 0.0000, 1000.0000, 0.0000, 'Expenses paid end of month', null, 0, 0)
		, (CONCAT('030036.', @UserId), @UserId, 'SUPTWO', 2, 1, '20190822', '20190930', '20190930', 650.0000, 0.0000, 650.0000, 0.0000, '30 days end of month following date of invoice', null, 0, 0)
		, (CONCAT('030037.', @UserId), @UserId, 'TRACOM', 2, 1, '20190822', '20190930', '20190930', 150.0000, 30.0000, 150.0000, 30.0000, '30 days end of month following date of invoice', null, 0, 0)
		, (CONCAT('030038.', @UserId), @UserId, 'SUNSUP', 2, 1, '20190722', '20190722', '20190722', 54.0000, 10.8000, 54.0000, 10.8000, 'Paid with order', null, 0, 0)
		;

		INSERT INTO Invoice.tbProject (InvoiceNumber, ProjectCode, Quantity, TotalValue, InvoiceValue, TaxValue, CashCode, TaxCode)
		VALUES (CONCAT('010000.', @UserId), CONCAT(@UserId, '_10007'), 5000, 0.0000, 2000, 400, '103', 'T1')
		, (CONCAT('010001.', @UserId), CONCAT(@UserId, '_10008'), 5000, 0.0000, 2000, 400, '103', 'T1')
		, (CONCAT('010002.', @UserId), CONCAT(@UserId, '_10009'), 5000, 0.0000, 2000, 400, '103', 'T1')
		, (CONCAT('010003.', @UserId), CONCAT(@UserId, '_10010'), 5000, 0.0000, 2000, 400, '103', 'T1')
		, (CONCAT('010004.', @UserId), CONCAT(@UserId, '_10011'), 5000, 0.0000, 2000, 400, '103', 'T1')
		, (CONCAT('010006.', @UserId), CONCAT(@UserId, '_10017'), 5000000, 0.0000, 18500.0000, 3700.0000, '103', 'T1')
		, (CONCAT('010011.', @UserId), CONCAT(@UserId, '_10012'), 5000, 0.0000, 2000, 400, '103', 'T1')
		, (CONCAT('030000.', @UserId), CONCAT(@UserId, '_20010'), 5000, 0.0000, 650.0000, 0.0000, '200', 'T0')
		, (CONCAT('030001.', @UserId), CONCAT(@UserId, '_20011'), 2, 0.0000, 150.0000, 30.0000, '200', 'T1')
		, (CONCAT('030002.', @UserId), CONCAT(@UserId, '_20013'), 5000, 0.0000, 650.0000, 0.0000, '200', 'T0')
		, (CONCAT('030003.', @UserId), CONCAT(@UserId, '_20014'), 2, 0.0000, 150.0000, 30.0000, '200', 'T1')
		, (CONCAT('030004.', @UserId), CONCAT(@UserId, '_20015'), 5000, 0.0000, 650.0000, 0.0000, '200', 'T0')
		, (CONCAT('030005.', @UserId), CONCAT(@UserId, '_20016'), 2, 0.0000, 150.0000, 30.0000, '200', 'T1')
		, (CONCAT('030006.', @UserId), CONCAT(@UserId, '_20017'), 5000, 0.0000, 650.0000, 0.0000, '200', 'T0')
		, (CONCAT('030007.', @UserId), CONCAT(@UserId, '_20018'), 2, 0.0000, 150.0000, 30.0000, '200', 'T1')
		, (CONCAT('030008.', @UserId), CONCAT(@UserId, '_20019'), 5000, 0.0000, 650.0000, 0.0000, '200', 'T0')
		, (CONCAT('030009.', @UserId), CONCAT(@UserId, '_20020'), 2, 0.0000, 150.0000, 30.0000, '200', 'T1')
		, (CONCAT('030010.', @UserId), CONCAT(@UserId, '_40000'), 142, 0.0000, 63.9000, 0.0000, '212', 'T0')
		, (CONCAT('030010.', @UserId), CONCAT(@UserId, '_40003'), 1, 0.0000, 4.0000, 0.8000, '213', 'T1')
		, (CONCAT('030010.', @UserId), CONCAT(@UserId, '_40004'), 4, 0.0000, 16.0000, 0.0000, '205', 'T0')
		, (CONCAT('030011.', @UserId), CONCAT(@UserId, '_40005'), 1, 0.0000, 1000.0000, 0.0000, '402', 'NI1')
		, (CONCAT('030012.', @UserId), CONCAT(@UserId, '_40006'), 1, 0.0000, 1000.0000, 0.0000, '402', 'NI1')
		, (CONCAT('030013.', @UserId), CONCAT(@UserId, '_40007'), 1, 0.0000, 1000.0000, 0.0000, '402', 'NI1')
		, (CONCAT('030014.', @UserId), CONCAT(@UserId, '_40008'), 1, 0.0000, 1000.0000, 0.0000, '402', 'NI1')
		, (CONCAT('030015.', @UserId), CONCAT(@UserId, '_40009'), 1, 0.0000, 1000.0000, 0.0000, '402', 'NI1')
		, (CONCAT('030016.', @UserId), CONCAT(@UserId, '_40010'), 1, 0.0000, 1000.0000, 0.0000, '402', 'NI1')
		, (CONCAT('030017.', @UserId), CONCAT(@UserId, '_20031'), 5000000, 0.0000, 6000.0000, 1200.0000, '200', 'T1')
		, (CONCAT('030018.', @UserId), CONCAT(@UserId, '_20032'), 13, 0.0000, 9750.0000, 1950.0000, '200', 'T1')
		, (CONCAT('030019.', @UserId), CONCAT(@UserId, '_40017'), 185, 0.0000, 83.2500, 0.0000, '212', 'T0')
		, (CONCAT('030019.', @UserId), CONCAT(@UserId, '_40018'), 1, 0.0000, 19.2000, 0.0000, '207', 'T0')
		, (CONCAT('030019.', @UserId), CONCAT(@UserId, '_40019'), 1, 0.0000, 16.0000, 0.0000, '205', 'T0')
		, (CONCAT('030020.', @UserId), CONCAT(@UserId, '_40020'), 178, 0.0000, 80.1000, 0.0000, '212', 'T0')
		, (CONCAT('030020.', @UserId), CONCAT(@UserId, '_40021'), 1, 0.0000, 5.0000, 0.0000, '213', 'T0')
		, (CONCAT('030020.', @UserId), CONCAT(@UserId, '_40022'), 1, 0.0000, 20.0000, 0.0000, '205', 'T0')
		, (CONCAT('030022.', @UserId), CONCAT(@UserId, '_40023'), 340, 0.0000, 153.0000, 0.0000, '212', 'T0')
		, (CONCAT('030022.', @UserId), CONCAT(@UserId, '_40024'), 1, 0.0000, 16.0000, 0.0000, '205', 'T0')
		, (CONCAT('030023.', @UserId), CONCAT(@UserId, '_40025'), 395, 0.0000, 177.7500, 0.0000, '212', 'T0')
		, (CONCAT('030023.', @UserId), CONCAT(@UserId, '_40026'), 1, 0.0000, 18.0000, 3.6000, '209', 'T1')
		, (CONCAT('030023.', @UserId), CONCAT(@UserId, '_40027'), 1, 0.0000, 16.0000, 0.0000, '205', 'T0')
		, (CONCAT('030024.', @UserId), CONCAT(@UserId, '_40028'), 412, 0.0000, 185.4000, 0.0000, '212', 'T0')
		, (CONCAT('030024.', @UserId), CONCAT(@UserId, '_40029'), 1, 0.0000, 5.0000, 1.0000, '213', 'T1')
		, (CONCAT('030024.', @UserId), CONCAT(@UserId, '_40030'), 1, 0.0000, 12.0000, 0.0000, '205', 'T0')
		, (CONCAT('030026.', @UserId), CONCAT(@UserId, '_40031'), 1, 0.0000, 40.0000, 8.0000, '202', 'T1')
		, (CONCAT('030027.', @UserId), CONCAT(@UserId, '_40032'), 1, 0.0000, 39.6000, 7.9200, '202', 'T1')
		, (CONCAT('030028.', @UserId), CONCAT(@UserId, '_40033'), 1, 0.0000, 43.1200, 8.6200, '202', 'T1')
		, (CONCAT('030029.', @UserId), CONCAT(@UserId, '_40034'), 1, 0.0000, 43.5200, 8.7000, '202', 'T1')
		, (CONCAT('030030.', @UserId), CONCAT(@UserId, '_40035'), 1, 0.0000, 42.5200, 8.5000, '202', 'T1')
		, (CONCAT('030031.', @UserId), CONCAT(@UserId, '_40036'), 1, 0.0000, 41.1500, 8.2300, '202', 'T1')
		, (CONCAT('030033.', @UserId), CONCAT(@UserId, '_40037'), 1, 0.0000, 40.0000, 8.0000, '202', 'T1')
		, (CONCAT('030035.', @UserId), CONCAT(@UserId, '_40011'), 1, 0.0000, 1000.0000, 0.0000, '402', 'NI1')
		, (CONCAT('030036.', @UserId), CONCAT(@UserId, '_20021'), 5000, 0.0000, 650.0000, 0.0000, '200', 'T0')
		, (CONCAT('030037.', @UserId), CONCAT(@UserId, '_20022'), 2, 0.0000, 150.0000, 30.0000, '200', 'T1')
		, (CONCAT('030038.', @UserId), CONCAT(@UserId, '_40046'), 12, 0.0000, 54.0000, 10.8000, '209', 'T1')
		;
		INSERT INTO Invoice.tbItem (InvoiceNumber, CashCode, TaxCode, TotalValue, InvoiceValue, TaxValue, ItemReference)
		VALUES (CONCAT('010005.', @UserId), '215', 'N/A', 0.0000, 10000.0000, 0.0000, null)
		, (CONCAT('010007.', @UserId), '215', 'N/A', 0.0000, 15000.0000, 0.0000, null)
		, (CONCAT('010008.', @UserId), '305', 'N/A', 0.0000, 5000.0000, 0.0000, null)
		, (CONCAT('010009.', @UserId), '305', 'N/A', 0.0000, 5000.0000, 0.0000, null)
		, (CONCAT('010010.', @UserId), '305', 'N/A', 0.0000, 1000.0000, 0.0000, null)
		, (CONCAT('030021.', @UserId), '303', 'N/A', 0.0000, 5000.0000, 0.0000, null)
		, (CONCAT('030025.', @UserId), '303', 'N/A', 0.0000, 5000.0000, 0.0000, null)
		, (CONCAT('030034.', @UserId), '303', 'N/A', 0.0000, 1000.0000, 0.0000, null)
		;

		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_10007');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_10008');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_10009');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_10010');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_10011');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_10012');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_10017');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_20010');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_20011');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_20013');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_20014');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_20015');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_20016');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_20017');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_20018');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_20019');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_20020');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_20021');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_20022');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_20031');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_20032');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40000');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40003');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40004');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40005');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40006');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40007');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40008');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40009');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40010');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40011');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40017');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40018');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40019');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40020');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40021');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40022');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40023');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40024');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40025');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40026');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40027');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40028');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40029');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40030');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40031');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40032');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40033');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40034');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40035');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40036');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40037');
		UPDATE Project.tbProject SET ProjectStatusCode = 3 WHERE ProjectCode = CONCAT(@UserId, '_40046');

		UPDATE       Invoice.tbInvoice
		SET                InvoicedOn = Project.ActionedOn
		FROM            Invoice.tbProject AS Projectinvoice INNER JOIN
								 Project.tbProject AS Project ON Projectinvoice.ProjectCode = Project.ProjectCode INNER JOIN
								 Invoice.tbInvoice ON Projectinvoice.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber;

		WITH invoice_items AS
		(
			SELECT        Invoice.tbInvoice.InvoiceNumber
			FROM            Invoice.tbInvoice INNER JOIN
									 Invoice.tbItem ON Invoice.tbInvoice.InvoiceNumber = Invoice.tbItem.InvoiceNumber
			EXCEPT
			SELECT        Invoice.tbInvoice.InvoiceNumber
			FROM            Invoice.tbInvoice INNER JOIN
									 Invoice.tbProject ON Invoice.tbInvoice.InvoiceNumber = Invoice.tbProject.InvoiceNumber
		)
		UPDATE invoices
		SET InvoicedOn = DATEADD(MONTH, @OffsetMonth, App.fnAdjustToCalendar(InvoicedOn, 0))
		FROM Invoice.tbInvoice invoices
			JOIN invoice_items ON invoices.InvoiceNumber = invoice_items.InvoiceNumber;

		IF (@PayInvoices = 0)
			GOTO CommitTran;

		DECLARE @CurrentAccount nvarchar(10), @ReserveAccount nvarchar(10);
		EXEC Cash.proc_CurrentAccount @CurrentAccount OUTPUT;
		EXEC Cash.proc_ReserveAccount @ReserveAccount OUTPUT;

		INSERT INTO Cash.tbPayment (PaymentCode, UserId, PaymentStatusCode, SubjectCode, AccountCode, CashCode, TaxCode, PaidOn, PaidInValue, PaidOutValue, PaymentReference)
		VALUES (CONCAT(@UserId, '_20190008_120000'), @UserId, 1, 'CDCUST', @CurrentAccount, '103', 'T1', '20190228', 2400.0000, 0.0000, CONCAT('010000.', @UserId))
		, (CONCAT(@UserId, '_20190008_120001'), @UserId, 1, 'CDCUST', @CurrentAccount, '103', 'T1', '20190329', 2400.0000, 0.0000, CONCAT('010001.', @UserId))
		, (CONCAT(@UserId, '_20190008_120002'), @UserId, 1, 'CDCUST', @CurrentAccount, '103', 'T1', '20190430', 2400.0000, 0.0000, CONCAT('010002.', @UserId))
		, (CONCAT(@UserId, '_20190008_120003'), @UserId, 1, 'CDCUST', @CurrentAccount, '103', 'T1', '20190531', 2400.0000, 0.0000, CONCAT('010003.', @UserId))
		, (CONCAT(@UserId, '_20190008_120004'), @UserId, 1, 'CDCUST', @CurrentAccount, '103', 'T1', '20190628', 2400.0000, 0.0000, CONCAT('010004.', @UserId))
		, (CONCAT(@UserId, '_20190008_120005'), @UserId, 1, 'SUPTWO', @CurrentAccount, '200', 'T0', '20190228', 0.0000, 650.0000, CONCAT('030000.', @UserId))
		, (CONCAT(@UserId, '_20190008_120006'), @UserId, 1, 'TRACOM', @CurrentAccount, '200', 'T1', '20190228', 0.0000, 180.0000, CONCAT('030001.', @UserId))
		, (CONCAT(@UserId, '_20190008_120007'), @UserId, 1, 'SUPTWO', @CurrentAccount, '200', 'T0', '20190329', 0.0000, 650.0000, CONCAT('030002.', @UserId))
		, (CONCAT(@UserId, '_20190008_120008'), @UserId, 1, 'TRACOM', @CurrentAccount, '200', 'T1', '20190329', 0.0000, 180.0000, CONCAT('030003.', @UserId))
		, (CONCAT(@UserId, '_20190008_120009'), @UserId, 1, 'SUPTWO', @CurrentAccount, '200', 'T0', '20190430', 0.0000, 650.0000, CONCAT('030004.', @UserId))
		, (CONCAT(@UserId, '_20190008_120010'), @UserId, 1, 'TRACOM', @CurrentAccount, '200', 'T1', '20190430', 0.0000, 180.0000, CONCAT('030005.', @UserId))
		, (CONCAT(@UserId, '_20190008_120011'), @UserId, 1, 'SUPTWO', @CurrentAccount, '200', 'T0', '20190430', 0.0000, 650.0000, CONCAT('030006.', @UserId))
		, (CONCAT(@UserId, '_20190008_120012'), @UserId, 1, 'TRACOM', @CurrentAccount, '200', 'T1', '20190531', 0.0000, 180.0000, CONCAT('030007.', @UserId))
		, (CONCAT(@UserId, '_20190008_120013'), @UserId, 1, 'SUPTWO', @CurrentAccount, '200', 'T0', '20190628', 0.0000, 650.0000, CONCAT('030008.', @UserId))
		, (CONCAT(@UserId, '_20190008_120014'), @UserId, 1, 'TRACOM', @CurrentAccount, '200', 'T1', '20190628', 0.0000, 180.0000, CONCAT('030009.', @UserId))
		, (CONCAT(@UserId, '_20190008_120015'), @UserId, 1, 'BUSOWN', @CurrentAccount, '205', 'T0', '20190131', 0.0000, 84.7000, null)
		, (CONCAT(@UserId, '_20190008_120016'), @UserId, 1, 'BUSOWN', @CurrentAccount, '402', 'NI1', '20190131', 0.0000, 1000.0000, null)
		, (CONCAT(@UserId, '_20190008_120017'), @UserId, 1, 'BUSOWN', @CurrentAccount, '402', 'NI1', '20190228', 0.0000, 1000.0000, null)
		, (CONCAT(@UserId, '_20190008_120018'), @UserId, 1, 'BUSOWN', @CurrentAccount, '402', 'NI1', '20190329', 0.0000, 1000.0000, null)
		, (CONCAT(@UserId, '_20190008_120019'), @UserId, 1, 'BUSOWN', @CurrentAccount, '402', 'NI1', '20190430', 0.0000, 1000.0000, null)
		, (CONCAT(@UserId, '_20190008_120020'), @UserId, 1, 'BUSOWN', @CurrentAccount, '402', 'NI1', '20190531', 0.0000, 1000.0000, null)
		, (CONCAT(@UserId, '_20190008_120021'), @UserId, 1, 'BUSOWN', @CurrentAccount, '402', 'NI1', '20190628', 0.0000, 1000.0000, null)
		, (CONCAT(@UserId, '_20190008_120022'), @UserId, 1, 'BUSOWN', @CurrentAccount, '205', 'T0', '20190228', 0.0000, 118.4500, 'Monthly expenses')
		, (CONCAT(@UserId, '_20190008_120023'), @UserId, 1, 'BUSOWN', @CurrentAccount, '205', 'T0', '20190329', 0.0000, 105.1000, 'Monthly expenses')
		, (CONCAT(@UserId, '_20190008_120024'), @UserId, 1, 'SUPTWO', @CurrentAccount, '200', 'T1', '20190430', 0.0000, 7200.0000, CONCAT('030017.', @UserId))
		, (CONCAT(@UserId, '_20190008_120025'), @UserId, 1, 'BUSOWN', @CurrentAccount, '205', 'T0', '20190430', 0.0000, 169.0000, 'Monthly expenses')
		, (CONCAT(@UserId, '_20190008_120026'), @UserId, 1, 'EFCUST', @CurrentAccount, '103', 'T1', '20190518', 22200.0000, 0.0000, null)
		, (CONCAT(@UserId, '_20190008_120027'), @UserId, 1, 'THEPAP', @CurrentAccount, '200', 'T1', '20190518', 0.0000, 11700.0000, null)
		, (CONCAT(@UserId, '_20190008_120028'), @UserId, 1, 'BUSOWN', @CurrentAccount, '205', 'T0', '20190708', 0.0000, 215.3500, 'Monthly expenses')
		, (CONCAT(@UserId, '_20190008_120029'), @UserId, 1, 'BUSOWN', @CurrentAccount, '205', 'T0', '20190708', 0.0000, 203.4000, 'Monthly expenses')
		, (CONCAT(@UserId, '_20190019_120000'), @UserId, 1, 'TELPRO', @CurrentAccount, '202', 'T1', '20190125', 0.0000, 48.0000, CONCAT('030026.', @UserId))
		, (CONCAT(@UserId, '_20190019_120001'), @UserId, 1, 'TELPRO', @CurrentAccount, '202', 'T1', '20190226', 0.0000, 47.5200, CONCAT('030027.', @UserId))
		, (CONCAT(@UserId, '_20190019_120002'), @UserId, 1, 'TELPRO', @CurrentAccount, '202', 'T1', '20190326', 0.0000, 51.7400, CONCAT('030028.', @UserId))
		, (CONCAT(@UserId, '_20190019_120003'), @UserId, 1, 'TELPRO', @CurrentAccount, '202', 'T1', '20190426', 0.0000, 52.2200, CONCAT('030029.', @UserId))
		, (CONCAT(@UserId, '_20190019_120004'), @UserId, 1, 'TELPRO', @CurrentAccount, '202', 'T1', '20190526', 0.0000, 51.0200, CONCAT('030030.', @UserId))
		, (CONCAT(@UserId, '_20190019_120005'), @UserId, 1, 'TELPRO', @CurrentAccount, '202', 'T1', '20190626', 0.0000, 49.3800, CONCAT('030031.', @UserId))
		, (CONCAT(@UserId, '_20190022_120000'), @UserId, 1, 'TELPRO', @CurrentAccount, '202', 'T1', '20190726', 0.0000, 48.0000, CONCAT('030033.', @UserId))
		, (CONCAT(@UserId, '_20190022_120001'), @UserId, 1, 'BUSOWN', @CurrentAccount, '402', 'NI1', '20190731', 0.0000, 1000.0000, CONCAT('030035.', @UserId))
		, (CONCAT(@UserId, '_20190022_120002'), @UserId, 1, 'CDCUST', @CurrentAccount, '103', 'T1', '20190731', 2400.0000, 0.0000, CONCAT('010011.', @UserId))
		, (CONCAT(@UserId, '_20190022_120003'), @UserId, 1, 'SUPTWO', @CurrentAccount, '200', 'T0', '20190731', 0.0000, 650.0000, CONCAT('030036.', @UserId))
		, (CONCAT(@UserId, '_20190022_120004'), @UserId, 1, 'TRACOM', @CurrentAccount, '200', 'T1', '20190731', 0.0000, 180.0000, CONCAT('030037.', @UserId))
		, (CONCAT(@UserId, '_20190022_120005'), @UserId, 1, 'SUNSUP', @CurrentAccount, '209', 'T1', '20190702', 0.0000, 64.8000, null)
		, (CONCAT(@UserId, '_20190708_030747'), @UserId, 1, 'HOME', @CurrentAccount, '305', 'N/A', '20190415', 5000.0000, 0.0000, 'Transfer from Reserve Account')
		, (CONCAT(@UserId, '_20191822_121834'), @UserId, 2, 'HOME', @CurrentAccount, '303', 'N/A', '20190831', 0.0000, 5000.0000, 'Transfer to Reserve account')
		, (CONCAT(@UserId, '_20192408_042438'), @UserId, 1, 'HOME', @CurrentAccount, '303', 'N/A', '20190601', 0.0000, 5000.0000, 'Transfer to Reserve Account')
		, (CONCAT(@UserId, '_20194708_014729'), @UserId, 1, 'BUSOWN', @CurrentAccount, '215', 'N/A', '20190101', 10000.0000, 0.0000, 'owner transfer')
		, (CONCAT(@UserId, '_20195222_125225'), @UserId, 1, 'HOME', @CurrentAccount, '303', 'N/A', '20190731', 0.0000, 1000.0000, 'Transfer to Reserve account')
		;

		IF (LEN(COALESCE(@ReserveAccount, '')) > 0)
		BEGIN
			INSERT INTO Cash.tbPayment (PaymentCode, UserId, PaymentStatusCode, SubjectCode, AccountCode, CashCode, TaxCode, PaidOn, PaidInValue, PaidOutValue, PaymentReference)
			VALUES (CONCAT(@UserId, '_20191822_121848'), @UserId, 2, 'HOME', @ReserveAccount, '305', 'N/A', '20190831', 5000.0000, 0.0000, 'Transfer from current account')
			, (CONCAT(@UserId, '_20192508_042502'), @UserId, 1, 'HOME', @ReserveAccount, '305', 'N/A', '20190601', 5000.0000, 0.0000, 'Transfer from Current Account')
			, (CONCAT(@UserId, '_20190608_030639'), @UserId, 1, 'BUSOWN', @ReserveAccount, '215', 'N/A', '20190101', 15000.0000, 0.0000, 'owner transfer')
			, (CONCAT(@UserId, '_20190708_030716'), @UserId, 1, 'HOME', @ReserveAccount, '303', 'N/A', '20190415', 0.0000, 5000.0000, 'Transfer to current account')
			, (CONCAT(@UserId, '_20195322_125307'), @UserId, 1, 'HOME', @ReserveAccount, '305', 'N/A', '20190731', 1000.0000, 0.0000, 'Transfer from current account')
			;
		END

		UPDATE Invoice.tbInvoice
		SET InvoiceStatusCode = 3;

		UPDATE Cash.tbPayment
		SET PaidOn = DATEADD(MONTH, @OffsetMonth, App.fnAdjustToCalendar(PaidOn, 0));

CommitTran:
		EXEC App.proc_SystemRebuild;
		COMMIT TRAN;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Subject].[proc_AddAddress]...';


go

CREATE   PROCEDURE Subject.proc_AddAddress 
	(
	@SubjectCode nvarchar(10),
	@Address ntext
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @AddressCode nvarchar(15)
	
		EXECUTE Subject.proc_NextAddressCode @SubjectCode, @AddressCode OUTPUT
	
		INSERT INTO Subject.tbAddress
							  (AddressCode, SubjectCode, Address)
		VALUES     (@AddressCode, @SubjectCode, @Address)
	
		IF NOT EXISTS (SELECT * FROM Subject.tbSubject Subject JOIN Subject.tbAddress Subject_addr ON Subject.AddressCode = Subject_addr.AddressCode WHERE Subject.SubjectCode = @SubjectCode)
		BEGIN
			UPDATE Subject.tbSubject
			SET AddressCode = @AddressCode
			WHERE Subject.tbSubject.SubjectCode = @SubjectCode
		END

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Subject].[proc_BalanceToPay]...';


go
CREATE   PROCEDURE [Subject].[proc_BalanceToPay](@SubjectCode NVARCHAR(10), @Balance DECIMAL(18, 5) = 0 OUTPUT)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @PayBalance BIT

		SELECT @PayBalance = PayBalance FROM Subject.tbSubject WHERE SubjectCode = @SubjectCode

		IF @PayBalance <> 0
			EXEC Subject.proc_BalanceOutstanding @SubjectCode, @Balance OUTPUT
		ELSE
			BEGIN
			SELECT TOP (1)   @Balance = CASE Invoice.tbType.CashPolarityCode 
											WHEN 0 THEN ((InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue)) * - 1 
											WHEN 1 THEN (InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue) END 
			FROM            Invoice.tbInvoice INNER JOIN
									 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
			WHERE  Invoice.tbInvoice.SubjectCode = @SubjectCode AND (Invoice.tbInvoice.InvoiceStatusCode > 0) AND (Invoice.tbInvoice.InvoiceStatusCode < 3) 
			ORDER BY ExpectedOn
			END

		SET @Balance = ISNULL(@Balance, 0)

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Invoice].[proc_AddProject]...';


go

CREATE   PROCEDURE Invoice.proc_AddProject 
	(
	@InvoiceNumber nvarchar(20),
	@ProjectCode nvarchar(20)	
	)
 AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	DECLARE 
		@InvoiceTypeCode smallint
		, @InvoiceQuantity float
		, @QuantityInvoiced float

		IF EXISTS(SELECT     InvoiceNumber, ProjectCode
				  FROM         Invoice.tbProject
				  WHERE     (InvoiceNumber = @InvoiceNumber) AND (ProjectCode = @ProjectCode))
			RETURN
		
		SELECT   @InvoiceTypeCode = InvoiceTypeCode
		FROM         Invoice.tbInvoice
		WHERE     (InvoiceNumber = @InvoiceNumber) 

		IF EXISTS(SELECT     SUM( Invoice.tbProject.Quantity) AS QuantityInvoiced
				  FROM         Invoice.tbProject INNER JOIN
										Invoice.tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
				  WHERE     ( Invoice.tbInvoice.InvoiceTypeCode = 0 OR
										Invoice.tbInvoice.InvoiceTypeCode = 2) AND ( Invoice.tbProject.ProjectCode = @ProjectCode) AND ( Invoice.tbInvoice.InvoiceStatusCode > 0))
			BEGIN
			SELECT TOP 1 @QuantityInvoiced = isnull(SUM( Invoice.tbProject.Quantity), 0)
			FROM         Invoice.tbProject INNER JOIN
					tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
			WHERE     ( Invoice.tbInvoice.InvoiceTypeCode = 0 OR
					tbInvoice.InvoiceTypeCode = 2) AND ( Invoice.tbProject.ProjectCode = @ProjectCode) AND ( Invoice.tbInvoice.InvoiceStatusCode > 0)				
			END
		ELSE
			SET @QuantityInvoiced = 0
		
		IF @InvoiceTypeCode = 1 or @InvoiceTypeCode = 3
			BEGIN
			IF EXISTS(SELECT     SUM( Invoice.tbProject.Quantity) AS QuantityInvoiced
					  FROM         Invoice.tbProject INNER JOIN
											tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
					  WHERE     ( Invoice.tbInvoice.InvoiceTypeCode = 1 OR
											tbInvoice.InvoiceTypeCode = 3) AND ( Invoice.tbProject.ProjectCode = @ProjectCode) AND ( Invoice.tbInvoice.InvoiceStatusCode > 0))
				BEGIN
				SELECT TOP 1 @InvoiceQuantity = isnull(@QuantityInvoiced, 0) - isnull(SUM( Invoice.tbProject.Quantity), 0)
				FROM         Invoice.tbProject INNER JOIN
						tbInvoice ON Invoice.tbProject.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
				WHERE     ( Invoice.tbInvoice.InvoiceTypeCode = 1 OR
						tbInvoice.InvoiceTypeCode = 3) AND ( Invoice.tbProject.ProjectCode = @ProjectCode) AND ( Invoice.tbInvoice.InvoiceStatusCode > 0)										
				END
			ELSE
				SET @InvoiceQuantity = isnull(@QuantityInvoiced, 0)
			END
		ELSE
			BEGIN
			SELECT  @InvoiceQuantity = Quantity - isnull(@QuantityInvoiced, 0)
			FROM         Project.tbProject
			WHERE     (ProjectCode = @ProjectCode)
			END
			
		IF isnull(@InvoiceQuantity, 0) <= 0
			SET @InvoiceQuantity = 1
		
		INSERT INTO Invoice.tbProject
							  (InvoiceNumber, ProjectCode, Quantity, InvoiceValue, CashCode, TaxCode)
		SELECT     @InvoiceNumber AS InvoiceNumber, ProjectCode, @InvoiceQuantity AS Quantity, UnitCharge * @InvoiceQuantity AS InvoiceValue, CashCode, 
							  TaxCode
		FROM         Project.tbProject
		WHERE     (ProjectCode = @ProjectCode)

		UPDATE Project.tbProject
		SET ActionedOn = CURRENT_TIMESTAMP
		WHERE ProjectCode = @ProjectCode;
	
		EXEC Invoice.proc_Total @InvoiceNumber	

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Invoice].[proc_Accept]...';


go
CREATE PROCEDURE Invoice.proc_Accept 
	(
	@InvoiceNumber nvarchar(20)
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT     InvoiceNumber
	          FROM         Invoice.tbItem
	          WHERE     (InvoiceNumber = @InvoiceNumber)) 
		or EXISTS(SELECT     InvoiceNumber
	          FROM         Invoice.tbProject
	          WHERE     (InvoiceNumber = @InvoiceNumber))
		BEGIN
			BEGIN TRANSACTION
			
			EXEC Invoice.proc_Total @InvoiceNumber
			
			UPDATE    Invoice.tbInvoice
			SET              InvoiceStatusCode = 1
			WHERE     (InvoiceNumber = @InvoiceNumber) AND (InvoiceStatusCode = 0); 
	
			WITH Project_codes AS
			(
				SELECT ProjectCode
				FROM Invoice.tbProject 
				WHERE InvoiceNumber = @InvoiceNumber
				GROUP BY ProjectCode
			), deliveries AS
			(
				SELECT invoices.ProjectCode, SUM(Quantity) QuantityDelivered
				FROM Invoice.tbProject invoices JOIN Project_codes ON invoices.ProjectCode = Project_codes.ProjectCode
				GROUP BY invoices.ProjectCode
			)
			UPDATE Project
			SET ProjectStatusCode = 3
			FROM Project.tbProject Project JOIN deliveries ON Project.ProjectCode = deliveries.ProjectCode
			WHERE Quantity <= QuantityDelivered;
			
			COMMIT TRANSACTION
		END
			
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Invoice].[proc_PostEntriesById]...';


go
CREATE   PROCEDURE Invoice.proc_PostEntriesById(@UserId nvarchar(10))
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@SubjectCode nvarchar(10)
			, @InvoiceTypeCode smallint
			, @InvoiceNumber nvarchar(20);
			
		DECLARE c1 CURSOR LOCAL FOR
			SELECT SubjectCode, InvoiceTypeCode
			FROM Invoice.tbEntry
			WHERE UserId = @UserId
			GROUP BY SubjectCode, InvoiceTypeCode;

		OPEN c1;

		BEGIN TRAN;

		FETCH NEXT FROM c1 INTO @SubjectCode, @InvoiceTypeCode;
		
		WHILE (@@FETCH_STATUS = 0)
		BEGIN
			EXEC Invoice.proc_RaiseBlank @SubjectCode, @InvoiceTypeCode, @InvoiceNumber output;

			WITH invoice_entry AS
			(
				SELECT @InvoiceNumber InvoiceNumber, MIN(InvoicedOn) InvoicedOn
				FROM Invoice.tbEntry
				WHERE SubjectCode = @SubjectCode AND InvoiceTypeCode = @InvoiceTypeCode
			)
			UPDATE Invoice.tbInvoice
			SET 
				UserId = @UserId,
				InvoicedOn = invoice_entry.InvoicedOn,
				Printed = CASE WHEN  @InvoiceTypeCode < 2 THEN 0 ELSE 1 END
			FROM Invoice.tbInvoice invoice_header 
				JOIN invoice_entry ON invoice_header.InvoiceNumber = invoice_entry.InvoiceNumber;

			INSERT INTO Invoice.tbItem (InvoiceNumber, CashCode, TaxCode, ItemReference, TotalValue, InvoiceValue)
			SELECT @InvoiceNumber InvoiceNumber, CashCode, TaxCode, ItemReference, TotalValue, InvoiceValue
			FROM Invoice.tbEntry
			WHERE SubjectCode = @SubjectCode AND InvoiceTypeCode = @InvoiceTypeCode

			EXEC Invoice.proc_Accept @InvoiceNumber;

			FETCH NEXT FROM c1 INTO @SubjectCode, @InvoiceTypeCode;
		END

		DELETE FROM Invoice.tbEntry
		WHERE UserId = @UserId;

		COMMIT TRAN;

		CLOSE c1;
		DEALLOCATE c1;

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Invoice].[proc_PostAccountById]...';


go
CREATE   PROCEDURE Invoice.proc_PostAccountById(@UserId nvarchar(10), @SubjectCode nvarchar(10))
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 			
			@InvoiceTypeCode smallint
			, @InvoiceNumber nvarchar(20);
			
		DECLARE c1 CURSOR LOCAL FOR
			SELECT InvoiceTypeCode
			FROM Invoice.tbEntry
			WHERE UserId = @UserId AND SubjectCode = @SubjectCode
			GROUP BY InvoiceTypeCode;

		OPEN c1;

		BEGIN TRAN;

		FETCH NEXT FROM c1 INTO @InvoiceTypeCode;
		
		WHILE (@@FETCH_STATUS = 0)
		BEGIN
			EXEC Invoice.proc_RaiseBlank @SubjectCode, @InvoiceTypeCode, @InvoiceNumber output;

			WITH invoice_entry AS
			(
				SELECT @InvoiceNumber InvoiceNumber, MIN(InvoicedOn) InvoicedOn
				FROM Invoice.tbEntry
				WHERE SubjectCode = @SubjectCode AND InvoiceTypeCode = @InvoiceTypeCode
			)
			UPDATE Invoice.tbInvoice
			SET 
				UserId = @UserId,
				InvoicedOn = invoice_entry.InvoicedOn,
				Printed = CASE WHEN  @InvoiceTypeCode < 2 THEN 0 ELSE 1 END
			FROM Invoice.tbInvoice invoice_header 
				JOIN invoice_entry ON invoice_header.InvoiceNumber = invoice_entry.InvoiceNumber;

			INSERT INTO Invoice.tbItem (InvoiceNumber, CashCode, TaxCode, ItemReference, TotalValue, InvoiceValue)
			SELECT @InvoiceNumber InvoiceNumber, CashCode, TaxCode, ItemReference, TotalValue, InvoiceValue
			FROM Invoice.tbEntry
			WHERE UserId = @UserId AND SubjectCode = @SubjectCode AND InvoiceTypeCode = @InvoiceTypeCode

			EXEC Invoice.proc_Accept @InvoiceNumber;

			FETCH NEXT FROM c1 INTO @InvoiceTypeCode;
		END

		DELETE FROM Invoice.tbEntry
		WHERE UserId = @UserId AND SubjectCode = @SubjectCode;

		COMMIT TRAN;

		CLOSE c1;
		DEALLOCATE c1;

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Invoice].[proc_PostEntryById]...';


go
CREATE   PROCEDURE Invoice.proc_PostEntryById(@UserId nvarchar(10), @SubjectCode nvarchar(10), @CashCode nvarchar(50))
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@InvoiceTypeCode smallint
			, @InvoiceNumber nvarchar(20);
			

		BEGIN TRAN;

		SELECT @InvoiceTypeCode = InvoiceTypeCode 
		FROM Invoice.tbEntry 
		WHERE UserId = @UserId AND SubjectCode = @SubjectCode AND CashCode = @CashCode;
		
		EXEC Invoice.proc_RaiseBlank @SubjectCode, @InvoiceTypeCode, @InvoiceNumber output;

		WITH invoice_entry AS
		(
			SELECT @InvoiceNumber InvoiceNumber, MIN(InvoicedOn) InvoicedOn
			FROM Invoice.tbEntry
			WHERE SubjectCode = @SubjectCode AND InvoiceTypeCode = @InvoiceTypeCode
		)
		UPDATE Invoice.tbInvoice
		SET 
			UserId = @UserId,
			InvoicedOn = invoice_entry.InvoicedOn,
			Printed = CASE WHEN  @InvoiceTypeCode < 2 THEN 0 ELSE 1 END
		FROM Invoice.tbInvoice invoice_header 
			JOIN invoice_entry ON invoice_header.InvoiceNumber = invoice_entry.InvoiceNumber;

		INSERT INTO Invoice.tbItem (InvoiceNumber, CashCode, TaxCode, ItemReference, TotalValue, InvoiceValue)
		SELECT @InvoiceNumber InvoiceNumber, CashCode, TaxCode, ItemReference, TotalValue, InvoiceValue
		FROM Invoice.tbEntry
		WHERE SubjectCode = @SubjectCode AND CashCode = @CashCode

		EXEC Invoice.proc_Accept @InvoiceNumber;

		DELETE FROM Invoice.tbEntry
		WHERE UserId = @UserId AND SubjectCode = @SubjectCode AND CashCode = @CashCode;

		COMMIT TRAN;

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Invoice].[proc_Raise]...';


go

CREATE   PROCEDURE Invoice.proc_Raise
	(
	@ProjectCode nvarchar(20),
	@InvoiceTypeCode smallint,
	@InvoicedOn datetime,
	@InvoiceNumber nvarchar(20) = null output
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	DECLARE 
		@UserId nvarchar(10)
		, @NextNumber int
		, @InvoiceSuffix nvarchar(4)
		, @SubjectCode nvarchar(10)
	
		SELECT @UserId = UserId FROM Usr.vwCredentials

		SET @InvoiceSuffix = '.' + @UserId
	
		SELECT @NextNumber = NextNumber
		FROM Invoice.tbType
		WHERE InvoiceTypeCode = @InvoiceTypeCode
	
		SELECT @InvoiceNumber = FORMAT(@NextNumber, '000000') + @InvoiceSuffix
	
		WHILE EXISTS (SELECT     InvoiceNumber
					  FROM         Invoice.tbInvoice
					  WHERE     (InvoiceNumber = @InvoiceNumber))
			BEGIN
			SET @NextNumber = @NextNumber + 1
			SET @InvoiceNumber = FORMAT(@NextNumber, '000000') + @InvoiceSuffix
			END

		SET @InvoicedOn = isnull(CAST(@InvoicedOn AS DATE), CAST(CURRENT_TIMESTAMP AS DATE))
		SELECT @SubjectCode = SubjectCode FROM Project.tbProject WHERE ProjectCode = @ProjectCode


		BEGIN TRANSACTION
	
		EXEC Invoice.proc_Cancel
	
		UPDATE    Invoice.tbType
		SET              NextNumber = @NextNumber + 1
		WHERE     (InvoiceTypeCode = @InvoiceTypeCode)
	
		INSERT INTO Invoice.tbInvoice
							(InvoiceNumber, UserId, SubjectCode, InvoiceTypeCode, InvoicedOn, InvoiceStatusCode, PaymentTerms)
		SELECT     @InvoiceNumber AS InvoiceNumber, @UserId AS UserId, Project.tbProject.SubjectCode, @InvoiceTypeCode AS InvoiceTypeCode, @InvoicedOn AS InvoicedOn, 
							0 AS InvoiceStatusCode, Subject.tbSubject.PaymentTerms
		FROM         Project.tbProject INNER JOIN
							Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode
		WHERE     ( Project.tbProject.ProjectCode = @ProjectCode)

		EXEC Invoice.proc_AddProject @InvoiceNumber, @ProjectCode
	
		IF @@TRANCOUNT > 0		
			COMMIT TRANSACTION
	
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Invoice].[proc_Pay]...';


go
CREATE PROCEDURE Invoice.proc_Pay
	(
	@InvoiceNumber nvarchar(20),
	@PaidOn datetime,
	@Post bit = 1,
	@PaymentCode nvarchar(20) NULL OUTPUT
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	DECLARE 
		@PaidOut decimal(18, 5) = 0
		, @PaidIn decimal(18, 5) = 0
		, @BalanceOutstanding decimal(18, 5) = 0
		, @InvoiceOutstanding decimal(18, 5) = 0
		, @CashPolarityCode smallint
		, @SubjectCode nvarchar(10)
		, @AccountCode nvarchar(10)
		, @InvoiceStatusCode smallint
		, @UserId nvarchar(10)
		, @PaymentReference nvarchar(20)
		, @PayBalance BIT

		SELECT 
			@CashPolarityCode = Invoice.tbType.CashPolarityCode, 
			@SubjectCode = Invoice.tbInvoice.SubjectCode, 
			@PayBalance = Subject.tbSubject.PayBalance,
			@InvoiceStatusCode = Invoice.tbInvoice.InvoiceStatusCode,
			@InvoiceOutstanding = InvoiceValue + TaxValue - PaidValue - PaidTaxValue
		FROM Invoice.tbInvoice 
			INNER JOIN Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
			INNER JOIN Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode
		WHERE     ( Invoice.tbInvoice.InvoiceNumber = @InvoiceNumber)
	
		EXEC Subject.proc_BalanceOutstanding @SubjectCode, @BalanceOutstanding OUTPUT
		IF @BalanceOutstanding = 0 
		BEGIN
			DECLARE @Msg NVARCHAR(MAX);
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 3018;
			RAISERROR (@Msg, 10, 1)
		END
		ELSE IF @InvoiceStatusCode > 2
			RETURN 1

		SELECT @UserId = UserId FROM Usr.vwCredentials	
		SET @PaidOn = CAST(@PaidOn AS DATE)

		SET @PaymentCode = CONCAT(@UserId, '_', FORMAT(@PaidOn, 'yyyymmdd_hhmmss'))

		WHILE EXISTS (SELECT * FROM Cash.tbPayment WHERE PaymentCode = @PaymentCode)
			BEGIN
			SET @PaidOn = DATEADD(s, 1, @PaidOn)
			SET @PaymentCode = CONCAT(@UserId, '_', FORMAT(@PaidOn, 'yyyymmdd_hhmmss'))
			END
			
		IF @PayBalance = 0
			BEGIN	
			SET @PaymentReference = @InvoiceNumber
														
			IF @CashPolarityCode = 0
				BEGIN
				SET @PaidOut = @InvoiceOutstanding
				SET @PaidIn = 0
				END
			ELSE
				BEGIN
				SET @PaidIn = @InvoiceOutstanding
				SET @PaidOut = 0
				END
			END
		ELSE
			BEGIN
			SET @PaidIn = CASE WHEN @BalanceOutstanding > 0 THEN @BalanceOutstanding ELSE 0 END
			SET @PaidOut = CASE WHEN @BalanceOutstanding < 0 THEN ABS(@BalanceOutstanding) ELSE 0 END
			END
	
		EXEC Cash.proc_CurrentAccount @AccountCode OUTPUT

		BEGIN TRANSACTION

		IF @PaidIn + @PaidOut > 0
			BEGIN			

			INSERT INTO Cash.tbPayment
								  (PaymentCode, UserId, PaymentStatusCode, SubjectCode, AccountCode, PaidOn, PaidInValue, PaidOutValue, PaymentReference)
			VALUES     (@PaymentCode,@UserId, 0, @SubjectCode, @AccountCode, @PaidOn, @PaidIn, @PaidOut, @PaymentReference)		
		
			IF @Post <> 0
				EXEC Cash.proc_PaymentPostInvoiced @PaymentCode			
			END
		
		IF @@TRANCOUNT > 0
			COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_PayAccrual]...';


go
CREATE PROCEDURE Cash.proc_PayAccrual (@PaymentCode NVARCHAR(20))
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		
		IF EXISTS (	SELECT        *
					FROM            Cash.tbPayment 
					WHERE        (PaymentStatusCode = 2) 
						AND UserId = (SELECT UserId FROM Usr.vwCredentials))
			BEGIN

			BEGIN TRANSACTION
			EXEC Cash.proc_PaymentPostMisc @PaymentCode	
			COMMIT TRANSACTION
			
			END

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_TxPayOutInvoice]...';


go
CREATE   PROCEDURE Cash.proc_TxPayOutInvoice 
(
	@SubjectCode nvarchar(10),
	@CashCode nvarchar(50),
	@TaxCode nvarchar(10),
	@ItemReference nvarchar(50),
	@PaidOutValue decimal(18,5)
)
AS
  SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		DECLARE @InvoiceNumber nvarchar(20)
		
		BEGIN TRANSACTION
		
		EXEC Invoice.proc_RaiseBlank @SubjectCode, 2, @InvoiceNumber OUTPUT;
		
		INSERT INTO Invoice.tbItem (InvoiceNumber, CashCode, TaxCode, ItemReference, TotalValue)
		VALUES (@InvoiceNumber, @CashCode, @TaxCode, @ItemReference, @PaidOutValue);
		
		EXEC Invoice.proc_Accept @InvoiceNumber;

		COMMIT TRANSACTION
	
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_PaymentPostById]...';


go
CREATE   PROCEDURE Cash.proc_PaymentPostById(@UserId nvarchar(10)) 
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @PaymentCode nvarchar(20)

		DECLARE curMisc cursor local for
			SELECT        Cash.tbPayment.PaymentCode
			FROM            Cash.tbPayment 
				INNER JOIN Cash.tbCode ON Cash.tbPayment.CashCode = Cash.tbCode.CashCode 
				INNER JOIN Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
				INNER JOIN Subject.tbAccount ON Subject.tbAccount.AccountCode = Cash.tbPayment.AccountCode
			WHERE (Subject.tbAccount.AccountTypeCode < 2)
				AND (Cash.tbPayment.PaymentStatusCode = 0) 
				AND (Cash.tbPayment.UserId = @UserId)

			ORDER BY Cash.tbPayment.SubjectCode, Cash.tbPayment.PaidOn

		DECLARE curInv cursor local for
			SELECT     PaymentCode
			FROM         Cash.tbPayment
			WHERE     (PaymentStatusCode = 0) AND (CashCode IS NULL)
				AND (Cash.tbPayment.UserId = @UserId)
			ORDER BY SubjectCode, PaidOn
		
		BEGIN TRANSACTION

		OPEN curMisc
		FETCH NEXT FROM curMisc INTO @PaymentCode
		WHILE @@FETCH_STATUS = 0
			BEGIN
			EXEC Cash.proc_PaymentPostMisc @PaymentCode		
			FETCH NEXT FROM curMisc INTO @PaymentCode	
			END

		CLOSE curMisc
		DEALLOCATE curMisc
	
		OPEN curInv
		FETCH NEXT FROM curInv INTO @PaymentCode
		WHILE @@FETCH_STATUS = 0
			BEGIN
			EXEC Cash.proc_PaymentPostInvoiced @PaymentCode		
			FETCH NEXT FROM curInv INTO @PaymentCode	
			END

		CLOSE curInv
		DEALLOCATE curInv

		COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_NodeInitialisation]...';


go
CREATE PROCEDURE App.proc_NodeInitialisation
(
	@SubjectCode NVARCHAR(10),
	@BusinessName NVARCHAR(255),
	@FullName NVARCHAR(100),
	@BusinessAddress NVARCHAR(MAX),
	@BusinessEmailAddress NVARCHAR(255) = null,
	@UserEmailAddress NVARCHAR(255) = null,
	@PhoneNumber NVARCHAR(50) = null,
	@CompanyNumber NVARCHAR(20) = null,
	@VatNumber NVARCHAR(20) = null,
	@CalendarCode NVARCHAR(10),
	@UnitOfCharge NVARCHAR(5)
)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		BEGIN TRAN

		UPDATE Cash.tbTaxType
		SET SubjectCode = null, CashCode = null;

		DELETE FROM App.tbOptions;

		DELETE FROM Cash.tbPayment;
		DELETE FROM Invoice.tbInvoice;
		DELETE FROM Project.tbFlow;
		DELETE FROM Project.tbProject;
		DELETE FROM Object.tbFlow;
		DELETE FROM Object.tbObject;
		DELETE FROM Subject.tbAccount;
		DELETE FROM Subject.tbSubject;
		DELETE FROM Usr.tbMenuUser;
		DELETE FROM Usr.tbMenu;
		DELETE FROM Usr.tbUser;
		DELETE FROM App.tbCalendar;

		DELETE FROM App.tbYear;
		DELETE FROM App.tbBucket;
		DELETE FROM App.tbUom;
		DELETE FROM Cash.tbCategoryTotal;
		DELETE FROM Cash.tbCategoryExp;	
		DELETE FROM Cash.tbCode;
		DELETE FROM App.tbTaxCode;
		DELETE FROM Cash.tbTaxType;
		DELETE FROM Cash.tbCategory;
	
		/***************** CONTROL DATA *****************************************/
		IF NOT EXISTS(SELECT * FROM App.tbMonth)
			INSERT INTO App.tbMonth (MonthNumber, MonthName)
			VALUES (1, 'JAN')
			, (2, 'FEB')
			, (3, 'MAR')
			, (4, 'APR')
			, (5, 'MAY')
			, (6, 'JUN')
			, (7, 'JUL')
			, (8, 'AUG')
			, (9, 'SEP')
			, (10, 'OCT')
			, (11, 'NOV')
			, (12, 'DEC');

		IF NOT EXISTS(SELECT * FROM Object.tbAttributeType)
			INSERT INTO Object.tbAttributeType (AttributeTypeCode, AttributeType)
			VALUES (0, 'Order')
			, (1, 'Quote');

		IF NOT EXISTS(SELECT * FROM Object.tbSyncType)
			INSERT INTO Object.tbSyncType (SyncTypeCode, SyncType)
			VALUES (0, 'SYNC')
			, (1, 'ASYNC')
			, (2, 'CALL-OFF');

		IF NOT EXISTS(SELECT * FROM App.tbBucketInterval)
			INSERT INTO App.tbBucketInterval (BucketIntervalCode, BucketInterval)
			VALUES (0, 'Day')
			, (1, 'Week')
			, (2, 'Month');

		IF NOT EXISTS(SELECT * FROM App.tbBucketType)
			INSERT INTO App.tbBucketType (BucketTypeCode, BucketType)
			VALUES (0, 'Default')
			, (1, 'Sunday')
			, (2, 'Monday')
			, (3, 'Tuesday')
			, (4, 'Wednesday')
			, (5, 'Thursday')
			, (6, 'Friday')
			, (7, 'Saturday')
			, (8, 'Month');

		IF NOT EXISTS(SELECT * FROM App.tbCodeExclusion)
			INSERT INTO App.tbCodeExclusion (ExcludedTag)
			VALUES ('Limited')
			, ('Ltd')
			, ('PLC');

		IF NOT EXISTS(SELECT * FROM App.tbDocClass)
			INSERT INTO App.tbDocClass (DocClassCode, DocClass)
			VALUES (0, 'Product')
			, (1, 'Money');

		IF NOT EXISTS(SELECT * FROM App.tbDocType)
			INSERT INTO App.tbDocType (DocTypeCode, DocType, DocClassCode)
			VALUES (0, 'Quotation', 0)
			, (1, 'Sales Order', 0)
			, (2, 'Enquiry', 0)
			, (3, 'Purchase Order', 0)
			, (4, 'Sales Invoice', 1)
			, (5, 'Credit Note', 1)
			, (6, 'Debit Note', 1);

		IF NOT EXISTS(SELECT * FROM App.tbRecurrence)
			INSERT INTO App.tbRecurrence (RecurrenceCode, Recurrence)
			VALUES (0, 'On Demand')
			, (1, 'Monthly')
			, (2, 'Quarterly')
			, (3, 'Bi-annual')
			, (4, 'Yearly');

		IF NOT EXISTS(SELECT * FROM App.tbRounding)
			INSERT INTO App.tbRounding (RoundingCode, Rounding)
			VALUES (0, 'Round')
			, (1, 'Truncate');


		IF NOT EXISTS(SELECT * FROM Cash.tbCategoryType)
			INSERT INTO Cash.tbCategoryType (CategoryTypeCode, CategoryType)
			VALUES (0, 'Cash Code')
			, (1, 'Total')
			, (2, 'Expression');

		IF NOT EXISTS(SELECT * FROM Cash.tbEntryType)
			INSERT INTO Cash.tbEntryType (CashEntryTypeCode, CashEntryType)
			VALUES (0, 'Payment')
			, (1, 'Invoice')
			, (2, 'Order')
			, (3, 'Quote')
			, (4, 'Corporation Tax')
			, (5, 'Vat')
			, (6, 'Forecast');

		IF NOT EXISTS(SELECT * FROM Cash.tbPolarity)
			INSERT INTO Cash.tbPolarity (CashPolarityCode, CashPolarity)
			VALUES (0, 'Expense')
			, (1, 'Income')
			, (2, 'Neutral');

		IF NOT EXISTS(SELECT * FROM Cash.tbStatus)
			INSERT INTO Cash.tbStatus (CashStatusCode, CashStatus)
			VALUES (0, 'Forecast')
			, (1, 'Current')
			, (2, 'Closed')
			, (3, 'Archived');

		IF NOT EXISTS(SELECT * FROM Cash.tbTaxType)
			INSERT INTO Cash.tbTaxType (TaxTypeCode, TaxType, MonthNumber, RecurrenceCode, OffsetDays)
			VALUES (0, 'Corporation Tax', 12, 4, 275)
			, (1, 'Vat', 4, 2, 31)
			, (2, 'N.I.', 4, 1, 0)
			, (3, 'General', 4, 0, 0);

		IF NOT EXISTS(SELECT * FROM Cash.tbType)
			INSERT INTO Cash.tbType (CashTypeCode, CashType)
			VALUES (0, 'TRADE')
			, (1, 'EXTERNAL')
			, (2, 'MONEY');

		IF NOT EXISTS(SELECT * FROM Invoice.tbStatus)
			INSERT INTO Invoice.tbStatus (InvoiceStatusCode, InvoiceStatus)
			VALUES (1, 'Invoiced')
			, (2, 'Partially Paid')
			, (3, 'Paid')
			, (0, 'Pending');

		IF NOT EXISTS(SELECT * FROM Invoice.tbType)
			INSERT INTO Invoice.tbType (InvoiceTypeCode, InvoiceType, CashPolarityCode, NextNumber)
			VALUES (0, 'Sales Invoice', 1, 10000)
			, (1, 'Credit Note', 0, 20000)
			, (2, 'Purchase Invoice', 0, 30000)
			, (3, 'Debit Note', 1, 40000);

		IF NOT EXISTS (SELECT * FROM Cash.tbPaymentStatus)
			INSERT INTO Cash.tbPaymentStatus (PaymentStatusCode, PaymentStatus)
			VALUES (0, 'Unposted')
			, (1, 'Payment')
			, (2, 'Transfer');

		IF NOT EXISTS(SELECT * FROM Subject.tbStatus)
			INSERT INTO Subject.tbStatus (SubjectStatusCode, SubjectStatus)
			VALUES (0, 'Pending')
			, (1, 'Active')
			, (2, 'Hot')
			, (3, 'Dead');

		IF NOT EXISTS(SELECT * FROM Project.tbOpStatus)
			INSERT INTO Project.tbOpStatus (OpStatusCode, OpStatus)
			VALUES (0, 'Pending')
			, (1, 'In-progress')
			, (2, 'Complete');

		IF NOT EXISTS(SELECT * FROM Project.tbStatus)
			INSERT INTO Project.tbStatus (ProjectStatusCode, ProjectStatus)
			VALUES (0, 'Pending')
			, (1, 'Open')
			, (2, 'Closed')
			, (3, 'Charged')
			, (4, 'Cancelled')
			, (5, 'Archive');

		IF NOT EXISTS(SELECT * FROM Usr.tbMenuCommand)
			INSERT INTO Usr.tbMenuCommand (Command, CommandText)
			VALUES (0, 'Folder')
			, (1, 'Link')
			, (2, 'Form In Read Mode')
			, (3, 'Form In Add Mode')
			, (4, 'Form In Edit Mode')
			, (5, 'Report');

		IF NOT EXISTS(SELECT * FROM Usr.tbMenuOpenMode) 
			INSERT INTO Usr.tbMenuOpenMode (OpenMode, OpenModeDescription)
			VALUES (0, 'Normal')
			, (1, 'Datasheet')
			, (2, 'Default Printing')
			, (3, 'Direct Printing')
			, (4, 'Print Preview')
			, (5, 'Email RTF')
			, (6, 'Email HTML')
			, (7, 'Email Snapshot')
			, (8, 'Email PDF');

		IF NOT EXISTS(SELECT * FROM App.tbRegister)
			INSERT INTO App.tbRegister (RegisterName, NextNumber)
			VALUES ('Expenses', 40000)
			, ('Event Log', 1)
			, ('Project', 30000)
			, ('Purchase Order', 20000)
			, ('Sales Order', 10000);

		IF NOT EXISTS(SELECT * FROM App.tbDoc)
			INSERT INTO App.tbDoc (DocTypeCode, ReportName, OpenMode, Description)
			VALUES (0, 'Project_QuotationStandard', 2, 'Standard Quotation')
			, (0, 'Project_QuotationTextual', 2, 'Textual Quotation')
			, (1, 'Project_SalesOrder', 2, 'Standard Sales Order')
			, (2, 'Project_PurchaseEnquiryDeliveryStandard', 2, 'Standard Transport Enquiry')
			, (2, 'Project_PurchaseEnquiryDeliveryTextual', 2, 'Textual Transport Enquiry')
			, (2, 'Project_PurchaseEnquiryStandard', 2, 'Standard Purchase Enquiry')
			, (2, 'Project_PurchaseEnquiryTextual', 2, 'Textual Purchase Enquiry')
			, (3, 'Project_PurchaseOrder', 2, 'Standard Purchase Order')
			, (3, 'Project_PurchaseOrderDelivery', 2, 'Purchase Order for Delivery')
			, (4, 'Invoice_Sales', 2, 'Standard Sales Invoice')
			, (4, 'Invoice_SalesLetterhead', 2, 'Sales Invoice for Letterhead Paper')
			, (5, 'Invoice_CreditNote', 2, 'Standard Credit Note')
			, (5, 'Invoice_CreditNoteLetterhead', 2, 'Credit Note for Letterhead Paper')
			, (6, 'Invoice_DebitNote', 2, 'Standard Debit Note')
			, (6, 'Invoice_DebitNoteLetterhead', 2, 'Debit Note for Letterhead Paper');

		IF NOT EXISTS(SELECT * FROM Subject.tbType)
			INSERT INTO Subject.tbType (SubjectTypeCode, CashPolarityCode, SubjectType)
			VALUES (0, 0, 'Supplier')
			, (1, 1, 'Customer')
			, (2, 1, 'Prospect')
			, (4, 1, 'Company')
			, (5, 0, 'Bank')
			, (7, 0, 'Other')
			, (8, 0, 'TBC')
			, (9, 0, 'Employee');

		IF NOT EXISTS(SELECT * FROM Cash.tbCoinType)
			INSERT INTO Cash.tbCoinType (CoinTypeCode, CoinType)
			VALUES (0, 'Main')
			, (1, 'TestNet')
			, (2, 'Fiat');

		IF NOT EXISTS(SELECT * FROM Cash.tbChangeType)
			INSERT INTO Cash.tbChangeType (ChangeTypeCode, ChangeType) 
			VALUES (0, 'Receipt')
			, (1, 'Change');

		IF NOT EXISTS(SELECT * FROM Cash.tbChangeStatus)
			INSERT INTO Cash.tbChangeStatus (ChangeStatusCode, ChangeStatus) 
			VALUES (0, 'Unused')
			, (1, 'Paid')
			, (2, 'Spent');

		IF NOT EXISTS(SELECT * FROM Subject.tbTransmitStatus)
			INSERT INTO Subject.tbTransmitStatus (TransmitStatusCode, TransmitStatus)
			VALUES (0, 'Disconnected')
			, (1, 'Deploy')
			, (2, 'Update')
			, (3, 'Processed');

		IF NOT EXISTS(SELECT * FROM Subject.tbAccountType)
			INSERT INTO Subject.tbAccountType (AccountTypeCode, AccountType)
			VALUES (0, 'CASH'), (1, 'DUMMY'), (2, 'ASSET');

		IF NOT EXISTS(SELECT * FROM Cash.tbAssetType)
			INSERT INTO Cash.tbAssetType (AssetTypeCode, AssetType)
			VALUES (0, 'DEBTORS')
			, (1, 'CREDITORS')
			, (2, 'BANK')
			, (3, 'CASH')
			, (4, 'CASH ACCOUNTS')
			, (5, 'CAPITAL');

		IF NOT EXISTS(SELECT * FROM App.tbTemplate)
			INSERT INTO App.tbTemplate (TemplateName, StoredProcedure)
			VALUES ('Basic Company Setup', 'App.proc_TemplateCompanyGeneral') 
				, ('HMRC Company Accounts 2020-21', 'App.proc_TemplateCompanyHMRC2021')
				, ('MIS Tutorials', 'App.proc_TemplateTutorials');

		IF NOT EXISTS(SELECT * FROM Usr.tbMenuView)
			INSERT INTO Usr.tbMenuView (MenuViewCode, MenuView)
			VALUES (0, 'List'), (1, 'Tree');

		IF NOT EXISTS(SELECT * FROM Usr.tbInterface)
			INSERT INTO Usr.tbInterface (InterfaceCode, Interface)
			VALUES (0, 'Accounts')
			, (1, 'MIS');

		IF NOT EXISTS(SELECT * FROM App.tbText)
		BEGIN
			INSERT INTO App.tbText (TextId, Message, Arguments)
			VALUES (1003, 'Enter new menu name', 0)
			, (1004, 'Team Menu', 0)
			, (1005, 'Ok to delete <1>', 1)
			, (1006, 'Documents cannot be converted into folders. Either delete the document or create a new folder elsewhere on the menu. Press esc key to undo changes.', 0)
			, (1007, '<Menu Item Text>', 0)
			, (1008, 'Documents cannot have other menu items added to them. Please select a folder then try again.', 0)
			, (1009, 'The root cannot be deleted. Please modify the text or remove the menu itself.', 0)
			, (1189, 'Error <1>', 1)
			, (1190, '<1> Source: <2>  (err <3>) <4>', 4)
			, (1192, 'Server error listing:', 0)
			, (1193, 'days', 0)
			, (1194, 'Ok to delete the selected task and all tasks upon which it depends?', 0)
			, (1208, 'A/No: <3>, Ref.: <2>, Title: <4>, Status: <6>. Dear <1>, <5> <7>', 7)
			, (1209, 'Yours sincerely, <1> <2> T: <3> M: <4> W: <5>', 5)
			, (1210, 'Okay to cancel invoice <1>?', 1)
			, (1211, 'Invoice <1> cannot be cancelled because there are payments assigned to it.  Use the debit/credit facility if this account is not properly reconciled.', 1)
			, (1212, 'Invoices are outstanding against account <1>.	By specifying a cash code, invoices will not be matched. Cash codes should only be entered for miscellaneous charges.', 1)
			, (1213, 'Account <1> has no invoices outstanding for this payment and therefore cannot be posted. Please specify a cash code so that one can be automatically generated.', 1)
			, (1214, 'Invoiced', 0)
			, (1215, 'Ordered', 0)
			, (1217, 'Order charge differs from the invoice. Reconcile <1>?', 1)
			, (1218, 'Raise invoice and pay expenses now?', 0)
			, (1219, 'Reserve Balance', 0)
			, (2002, 'Only administrators have access to the system configuration features of this application.', 0)
			, (2003, 'You are not a registered user of this system. Please contact the Administrator if you believe you should have access.', 0)
			, (2004, 'The primary key you have entered contains invalid characters. Digits and letters should be used for these keys. Please amend accordingly or press Esc to cancel.', 0)
			, (2136, 'You have attempted to execute an Application.Run command with an invalid string. The run string is <1>. The error is <2>', 2)
			, (2188, '<1>', 1)
			, (2206, 'Reminder: You are due for a period end close down.  Please follow the relevant procedures to complete this task. Once all financial data has been consolidated, use the Administrator to move onto the next period.', 0)
			, (2312, 'The system is not setup correctly. Make sure you have completed the initialisation procedures then try again.', 0)
			, (3002, 'Periods not generated successfully. Contact support.', 0)
			, (3003, 'Okay to close down the active period? Before proceeding make sure that you have entered and checked your cash details. All invoices and cash transactions will be transferred into the Cash Flow analysis module.', 0)
			, (3004, 'Margin', 0)
			, (3005, 'Opening Balance', 0)
			, (3006, 'Rebuild executed successfully', 0)
			, (3007, 'Ok to rebuild cash accounts? Make sure no transactions are being processed, as this will re-set and update all your invoices.', 0)
			, (3009, 'Charged', 0)
			, (3010, 'Service', 0)
			, (3011, 'Ok to rebuild cash flow history for account <1>? This would normally be required when payments or invoices have been retrospectively revised, or opening balances altered.', 1)
			, (3012, 'Ok to raise an invoice for this task? Use the Invoicing program to create specific invoice types with multiple tasks and additional charges.', 0)
			, (3013, 'Current Balance', 0)
			, (3014, 'This entry cannot be rescheduled', 0)
			, (3015, 'Dummy accounts should not be assigned a cash code', 0)
			, (3016, 'Operations cannot end before they have been started', 0)
			, (3017, 'Cash codes must be of catagory type MONEY', 0)
			, (3018, 'The balance for this account is zero. Check for unposted payments.', 0)
			, (1220, 'Invoices deployed to the network cannot be deleted. Add a credit/debit note instead.', 0)
			, (1221, 'Service Log cleared down.', 0)
			, (1222, 'Task Change Log cleared down.', 0)
			, (1223, 'Invoice Change Log cleared down.', 0)
			, (1224, 'Raise corresponding invoices?', 0)
			, (1225, 'Initialising <1>', 1)
			;
		END

		/***************** BUSINESS DATA *****************************************/

		INSERT INTO Subject.tbSubject (SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode, PhoneNumber, EmailAddress, CompanyNumber, VatNumber)
		VALUES (@SubjectCode, @BusinessName, 4, 1, @PhoneNumber, @BusinessEmailAddress, @CompanyNumber, @VatNumber);

		EXEC Subject.proc_AddContact @SubjectCode = @SubjectCode, @ContactName = @FullName;
		EXEC Subject.proc_AddAddress @SubjectCode = @SubjectCode, @Address = @BusinessAddress;

		INSERT INTO App.tbCalendar (CalendarCode, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday)
		VALUES (@CalendarCode, 1, 1, 1, 1, 1, 0, 0);
		
		INSERT INTO Usr.tbUser (UserId, UserName, LogonName, IsAdministrator, IsEnabled, CalendarCode, EmailAddress, PhoneNumber)
		VALUES (CONCAT(LEFT(@FullName, 1), SUBSTRING(@FullName, CHARINDEX(' ', @FullName) + 1, 1)), @FullName, 
			SUSER_NAME() , 1, 1, @CalendarCode, @UserEmailAddress, @PhoneNumber);

		INSERT INTO App.tbOptions (Identifier, IsInitialised, SubjectCode, RegisterName, DefaultPrintMode, BucketIntervalCode, BucketTypeCode, TaxHorizon, IsAutoOffsetDays, UnitOfCharge)
		VALUES ('TC', 0, @SubjectCode, 'Event Log', 2, 1, 1, 730, 0, @UnitOfCharge);

		SET IDENTITY_INSERT [Usr].[tbMenu] ON;
		INSERT INTO [Usr].[tbMenu] ([MenuId], [MenuName], [InterfaceCode])
		VALUES (1, 'Accounts', 0)
		, (2, 'MIS', 1);
		SET IDENTITY_INSERT [Usr].[tbMenu] OFF;

		SET IDENTITY_INSERT [Usr].[tbMenuEntry] ON;
		INSERT INTO [Usr].[tbMenuEntry] ([MenuId], [EntryId], [FolderId], [ItemId], [ItemText], [Command], [ProjectName], [Argument], [OpenMode])
		VALUES (1, 1, 1, 0, 'Accounts', 0, '', 'Root', 0)
		, (1, 2, 2, 0, 'System Settings', 0, 'Trader', '', 0)
		, (1, 3, 2, 1, 'Administration', 4, 'Trader', 'App_Admin', 0)
		, (1, 4, 2, 2, 'SQL Connect', 4, 'Trader', 'Sys_SQLConnect', 0)
		, (1, 5, 2, 5, 'Definitions', 4, 'Trader', 'App_Definition', 0)
		, (1, 6, 4, 0, 'Cash Accounts', 0, 'Trader', '', 0)
		, (1, 7, 4, 2, 'Cash Account Statements', 4, 'Trader', 'Subject_PaymentAccount', 0)
		, (1, 8, 5, 0, 'Invoicing', 0, 'Trader', '', 0)
		, (1, 9, 5, 3, 'Raise Invoices and Credit Notes', 4, 'Trader', 'Invoice_Entry', 0)
		, (1, 10, 6, 0, 'Transaction Entry', 0, 'Trader', '', 0)
		, (1, 12, 6, 5, 'Asset Entry', 4, 'Trader', 'Cash_Assets', 0)
		, (1, 13, 1, 1, 'System Settings', 1, '', '2', 0)
		, (1, 14, 1, 3, 'Cash Accounts', 1, '', '4', 0)
		, (1, 15, 1, 4, 'Invoicing', 1, '', '5', 0)
		, (1, 16, 1, 5, 'Transaction Entry', 1, '', '6', 0)
		, (1, 17, 5, 5, 'Invoice Register', 4, 'Trader', 'Invoice_Register', 0)
		, (1, 18, 4, 5, 'Bank Transfers', 4, 'Trader', 'Cash_Transfer', 0)
		, (1, 20, 6, 6, 'Budget', 4, 'Trader', 'Cash_Budget', 0)
		, (1, 21, 7, 0, 'Subjects', 0, 'Trader', '', 1)
		, (1, 22, 1, 6, 'Subjects', 1, '', '7', 1)
		, (1, 23, 7, 1, 'Subject Maintenance', 4, 'Trader', 'Subject_Maintenance', 0)
		, (1, 24, 7, 2, 'Subject Enquiry', 4, 'Trader', 'Subject_Enquiry', 0)
		, (1, 25, 7, 3, 'Balance Sheet Audit', 5, 'Trader', 'Subject_BalanceSheetAudit', 2)
		, (2, 26, 1, 0, 'MIS', 0, '', 'Root', 0)
		, (2, 27, 2, 0, 'System Settings', 0, 'Trader', '', 0)
		, (2, 28, 2, 1, 'Administration', 4, 'Trader', 'App_Admin', 0)
		, (2, 29, 2, 2, 'SQL Connect', 4, 'Trader', 'Sys_SQLConnect', 0)
		, (2, 30, 2, 5, 'Definitions', 4, 'Trader', 'App_Definition', 0)
		, (2, 31, 4, 0, 'Maintenance', 0, 'Trader', '', 0)
		, (2, 32, 4, 1, 'Subjects', 4, 'Trader', 'Subject_Maintenance', 0)
		, (2, 33, 4, 2, 'Activities', 4, 'Trader', 'Object_Edit', 0)
		, (2, 34, 5, 0, 'Work Flow', 0, 'Trader', '', 0)
		, (2, 35, 5, 1, 'Project Explorer', 4, 'Trader', 'Project_Explorer', 0)
		, (2, 36, 5, 2, 'Document Manager', 4, 'Trader', 'App_DocManager', 0)
		, (2, 37, 5, 3, 'Raise Invoices', 4, 'Trader', 'Invoice_Raise', 0)
		, (2, 38, 6, 0, 'Information', 0, 'Trader', '', 0)
		, (2, 39, 6, 1, 'Subject Enquiry', 2, 'Trader', 'Subject_Enquiry', 0)
		, (2, 40, 6, 2, 'Invoice Register', 4, 'Trader', 'Invoice_Register', 0)
		, (2, 41, 6, 3, 'Cash Statements', 4, 'Trader', 'Subject_PaymentAccount', 0)
		, (2, 42, 6, 4, 'Data Warehouse', 4, 'Trader', 'App_Warehouse', 0)
		, (2, 43, 6, 5, 'Company Statement', 4, 'Trader', 'Cash_Statement', 0)
		, (2, 44, 4, 3, 'Subject Datasheet', 4, 'Trader', 'Subject_Maintenance', 1)
		, (2, 45, 6, 6, 'Job Profit Status by Month', 4, 'Trader', 'Project_ProfitStatus', 0)
		, (2, 46, 5, 6, 'Expenses', 3, 'Trader', 'Project_Expenses', 0)
		, (2, 47, 1, 1, 'System Settings', 1, '', '2', 0)
		, (2, 48, 1, 3, 'Maintenance', 1, '', '4', 0)
		, (2, 49, 1, 4, 'Workflow', 1, '', '5', 0)
		, (2, 50, 1, 5, 'Information', 1, '', '6', 0)
		, (2, 51, 6, 7, 'Status Graphs', 4, 'Trader', 'Cash_StatusGraphs', 0)
		, (2, 53, 4, 4, 'Budget', 4, 'Trader', 'Cash_Budget', 0)
		, (2, 54, 4, 5, 'Assets', 4, 'Trader', 'Cash_Assets', 0)
		, (2, 57, 5, 7, 'Network Allocations', 4, 'Trader', 'Project_Allocation', 0)
		, (2, 58, 5, 8, 'Network Invoices', 4, 'Trader', 'Invoice_Mirror', 0)
		, (2, 62, 7, 0, 'Audit Reports', 0, 'Trader', '', 1)
		, (2, 63, 6, 11, 'Audit Reports', 1, '', '7', 1)
		, (2, 64, 7, 1, 'Corporation Tax Accruals', 5, 'Trader', 'Cash_CorpTaxAuditAccruals', 4)
		, (2, 65, 7, 2, 'Vat Accruals', 5, 'Trader', 'Cash_VatAuditAccruals', 4)
		, (2, 66, 7, 3, 'Balance Sheet Audit', 5, 'Trader', 'Subject_BalanceSheetAudit', 4);
		SET IDENTITY_INSERT [Usr].[tbMenuEntry] OFF;

		IF @UnitOfCharge <> 'BTC'
		BEGIN
			INSERT INTO Usr.tbMenuEntry (MenuId, FolderId, ItemId, ItemText, Command, ProjectName, Argument, OpenMode)
			VALUES 
				(1, 6, 3, 'Payment Entry', 4, 'Trader', 'Cash_PaymentEntry', 0)
				, (2, 5, 5, 'Transfers', 4, 'Trader', 'Cash_Transfer', 0)
				, (2, 5, 4, 'Payment Entry', 4, 'Trader', 'Cash_PaymentEntry', 0)
				

		END


		INSERT INTO Usr.tbMenuUser (UserId, MenuId)
		SELECT (SELECT UserId FROM Usr.tbUser) AS UserId, MenuId 
		FROM Usr.tbMenu;

		COMMIT TRAN
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_TemplateCompanyHMRC2021]...';


go
CREATE PROCEDURE App.proc_TemplateCompanyHMRC2021
(
	@FinancialMonth SMALLINT = 4,
	@GovAccountName NVARCHAR(255),
	@BankName NVARCHAR(255) = null,
	@BankAddress NVARCHAR(MAX) = null,
	@DummyAccount NVARCHAR(50), 
	@CurrentAccount NVARCHAR(50) = null,
	@CA_SortCode NVARCHAR(10) = null,
	@CA_AccountNumber NVARCHAR(20) = null,
	@ReserveAccount NVARCHAR(50) = null, 
	@RA_SortCode NVARCHAR(10) = null,
	@RA_AccountNumber NVARCHAR(20) = null
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE
			@CoinTypeCode SMALLINT = (SELECT TOP (1) CoinTypeCode FROM App.tbOptions),
			@SubjectCode NVARCHAR(10),
			@AccountCode NVARCHAR(10);

		INSERT INTO [App].[tbBucket] ([Period], [BucketId], [BucketDescription], [AllowForecasts])
		VALUES (0, 'Overdue', 'Overdue Orders', 0)
		, (1, 'Current', 'Current Week', 0)
		, (2, 'Week 2', 'Week Two', 0)
		, (3, 'Week 3', 'Week Three', 0)
		, (4, 'Week 4', 'Week Four', 0)
		, (8, 'Next Month', 'Next Month', 0)
		, (16, '2 Months', '2 Months', 1)
		, (52, 'Forward', 'Forward Orders', 1)
		;
		INSERT INTO [App].[tbUom] ([UnitOfMeasure])
		VALUES ('each')
		, ('days')
		, ('hrs')
		, ('kilo')
		, ('miles')
		, ('mins')
		, ('units')
		;

		DECLARE @Decimals smallint = CASE @CoinTypeCode WHEN 2 THEN 2 ELSE 3 END

		INSERT INTO [App].[tbTaxCode] ([TaxCode], [TaxRate], [TaxDescription], [TaxTypeCode], [RoundingCode], [Decimals])
		VALUES ('INT', 0, 'Interest Tax', 3, 0, @Decimals)
		, ('N/A', 0, 'Untaxed', 3, 0, @Decimals)
		, ('NI1', 0, 'Directors National Insurance', 2, 0, @Decimals)
		, ('NI2', 0.121, 'Employees National Insurance', 2, 0, @Decimals)
		, ('T0', 0, 'Zero Rated VAT', 1, 0, @Decimals)
		, ('T1', 0.2, 'Standard VAT Rate', 1, 0, @Decimals)
		, ('T9', 0, 'TBC', 1, 0, @Decimals)

		INSERT INTO [Cash].[tbCategory] ([CategoryCode], [Category], [CategoryTypeCode], [CashPolarityCode], [CashTypeCode], [DisplayOrder], [IsEnabled])
		VALUES ('AC12', 'Turnover', 1, 2, 0, 0, 1)
		, ('AC24', 'Income from Coronavirus business support grants', 1, 2, 0, 10, 1)
		, ('AC34', 'Tax On Profit', 1, 2, 0, 70, 1)
		, ('AC405', 'Other Income', 1, 2, 0, 20, 1)
		, ('AC410', 'Cost of raw material and consumables', 1, 2, 0, 30, 1)
		, ('AC415', 'Staff Costs', 1, 2, 0, 40, 1)
		, ('AC420', 'Depreciation and other amounts written off', 1, 2, 0, 50, 1)
		, ('AC425', 'Other Charges', 1, 2, 0, 60, 1)
		, ('AC435', 'Profit and Loss', 1, 2, 0, 90, 1)
		, ('CP14-39', 'CP Gross Profit or Loss', 1, 2, 1, 120, 1)
		, ('CP23', 'Rent and Rates', 0, 0, 0, 4, 1)
		, ('CP37', 'Sundry Costs', 0, 0, 0, 0, 1)
		, ('CP40', 'CP Total Expenses', 1, 2, 1, 130, 1)
		, ('CP44', 'CP Profit or losses before tax', 1, 2, 1, 150, 1)
		, ('CP500', 'CP Profit or losses before adjustments', 1, 2, 1, 140, 1)
		, ('CP511', 'CP Income from Property', 1, 2, 1, 180, 1)
		, ('CP54', 'CP Total Additions', 1, 2, 1, 160, 1)
		, ('CP59', 'CP Total Deductions', 1, 2, 1, 170, 1)
		, ('CP7', 'CP Turnover/Sales', 1, 2, 1, 100, 1)
		, ('CP8', 'CP Cost of Sales', 1, 2, 1, 110, 1)
		, ('TC-ADMIN', 'General Administrative Expenses', 0, 0, 0, 6, -1)
		, ('TC-ASSETAJ', 'Adjustments - Assets', 0, 0, 2, 12, -1)
		, ('TC-ASSETGP', 'Assets - Gross Profit', 0, 1, 2, 8, 1)
		, ('TC-ASSETNP', 'Assets - Net Profit', 0, 1, 2, 7, 1)
		, ('TC-BANK', 'Bank Accounts', 0, 2, 2, 9, 1)
		, ('TC-COSTAJ', 'Adjustments - Expenditure', 0, 0, 0, 13, 1)
		, ('TC-DIRECT', 'Direct Costs', 0, 0, 0, 1, 1)
		, ('TC-GRANTS', 'Business Grants', 0, 1, 0, 10, -1)
		, ('TC-INCOME', 'Additional Income', 0, 1, 0, 11, 1)
		, ('TC-INTERP', 'Intercompany Payment', 0, 0, 2, 0, 1)
		, ('TC-INTERR', 'Intercompany Receipt', 0, 1, 2, 0, 1)
		, ('TC-INVEST', 'Investment', 0, 2, 0, 0, 1)
		, ('TC-LIAB', 'Liabilities', 0, 0, 2, 0, 1)
		, ('TC-NP', 'Profit Before Tax', 1, 2, 0, 80, 1)
		, ('TC-PROPC', 'Expenses - property costs', 0, 0, 0, 5, 1)
		, ('TC-PROPE', 'Property - Expenditure', 0, 0, 0, 15, 1)
		, ('TC-PROPI', 'Property - Income', 0, 1, 0, 14, 1)
		, ('TC-SALES', 'Sales', 0, 1, 0, 0, 1)
		, ('TC-SALESAJ', 'Adjustments - Income', 0, 1, 0, 13, 1)
		, ('TC-SUBCON', 'Subcontractor Costs', 0, 1, 0, 3, 1)
		, ('TC-TAXCO', 'Tax On Company', 0, 0, 1, 101, 1)
		, ('TC-TAXGD', 'Tax On Goods', 0, 0, 1, 100, 1)
		, ('TC-VAT', 'VAT Cash Codes', 1, 2, 1, 900, 1)
		, ('TC-WAGES', 'Directors and Employee Wages', 0, 0, 0, 1, 1)
		;
		INSERT INTO [Cash].[tbCategoryTotal] ([ParentCode], [ChildCode])
		VALUES ('AC12', 'TC-INVEST')
		, ('AC12', 'TC-SALES')
		, ('AC24', 'TC-GRANTS')
		, ('AC34', 'TC-TAXCO')
		, ('AC405', 'TC-ASSETGP')
		, ('AC405', 'TC-INCOME')
		, ('AC405', 'TC-PROPI')
		, ('AC405', 'TC-SALESAJ')
		, ('AC410', 'TC-DIRECT')
		, ('AC415', 'TC-WAGES')
		, ('AC420', 'TC-ASSETAJ')
		, ('AC420', 'TC-ASSETNP')
		, ('AC420', 'TC-LIAB')
		, ('AC425', 'CP23')
		, ('AC425', 'CP37')
		, ('AC425', 'TC-ADMIN')
		, ('AC425', 'TC-COSTAJ')
		, ('AC425', 'TC-PROPC')
		, ('AC425', 'TC-PROPE')
		, ('AC435', 'AC34')
		, ('AC435', 'TC-NP')
		, ('CP14-39', 'CP7')
		, ('CP14-39', 'CP8')
		, ('CP40', 'CP23')
		, ('CP40', 'CP37')
		, ('CP40', 'TC-ADMIN')
		, ('CP40', 'TC-ASSETNP')
		, ('CP40', 'TC-PROPC')
		, ('CP40', 'TC-SUBCON')
		, ('CP40', 'TC-WAGES')
		, ('CP44', 'CP500')
		, ('CP44', 'TC-INCOME')
		, ('CP500', 'CP14-39')
		, ('CP500', 'CP40')
		, ('CP511', 'TC-PROPE')
		, ('CP511', 'TC-PROPI')
		, ('CP54', 'TC-ASSETAJ')
		, ('CP54', 'TC-SALESAJ')
		, ('CP59', 'TC-COSTAJ')
		, ('CP7', 'TC-INVEST')
		, ('CP7', 'TC-SALES')
		, ('CP8', 'TC-ASSETGP')
		, ('CP8', 'TC-DIRECT')
		, ('CP8', 'TC-TAXCO')
		, ('CP8', 'TC-LIAB')
		, ('TC-NP', 'AC12')
		, ('TC-NP', 'AC24')
		, ('TC-NP', 'AC405')
		, ('TC-NP', 'AC410')
		, ('TC-NP', 'AC415')
		, ('TC-NP', 'AC420')
		, ('TC-NP', 'AC425')
		, ('TC-VAT', 'TC-ADMIN')
		, ('TC-VAT', 'TC-COSTAJ')
		, ('TC-VAT', 'TC-DIRECT')
		, ('TC-VAT', 'TC-INCOME')
		, ('TC-VAT', 'TC-PROPC')
		, ('TC-VAT', 'TC-PROPE')
		, ('TC-VAT', 'TC-PROPI')
		, ('TC-VAT', 'TC-SALES')
		, ('TC-VAT', 'TC-SALESAJ')
		, ('TC-VAT', 'TC-SUBCON')
		, ('TC-VAT', 'CP37')
		;
		INSERT INTO [Cash].[tbCode] ([CashCode], [CashDescription], [CategoryCode], [TaxCode], [IsEnabled])
		VALUES ('CP130', 'Cornovirus (Covid-19) support', 'TC-GRANTS', 'N/A', -1)
		, ('CP15', 'Directors Pension', 'TC-WAGES', 'N/A', 1)
		, ('CP16', 'Directors Remuneration', 'TC-WAGES', 'N/A', 1)
		, ('CP17', 'Salaries and Wages', 'TC-WAGES', 'N/A', 1)
		, ('CP18', 'Subcon payments (construction ind. only)', 'TC-SUBCON', 'N/A', 0)
		, ('CP19', 'Accountancy and audit', 'TC-SUBCON', 'T1', 1)
		, ('CP20', 'Consultancy', 'TC-SUBCON', 'T1', 1)
		, ('CP21', 'Legal and professional charges', 'TC-SUBCON', 'T1', 1)
		, ('CP22', 'Light, heat and power', 'TC-PROPC', 'T1', 1)
		, ('CP24', 'Repairs, renewals and maintenance', 'TC-PROPC', 'T1', 1)
		, ('CP25', 'Advertising and promotion', 'TC-ADMIN', 'T1', 1)
		, ('CP26', 'Bad debts', 'TC-ADMIN', 'T1', 1)
		, ('CP27', 'Bank, credit card and other financial charges', 'TC-ADMIN', 'T0', 1)
		, ('CP28', 'Depreciation', 'TC-ASSETNP', 'N/A', 1)
		, ('CP29', 'Donations', 'TC-ADMIN', 'T0', 1)
		, ('CP30', 'Entertainment', 'TC-ADMIN', 'T1', 1)
		, ('CP31', 'Insurance', 'TC-ADMIN', 'T1', 1)
		, ('CP32', 'Interest paid', 'TC-ADMIN', 'T0', 1)
		, ('CP33', 'Profit/loss on disposal of assets', 'TC-ASSETNP', 'N/A', 0)
		, ('CP34', 'Travel and subsistence', 'TC-ADMIN', 'T1', 1)
		, ('CP35', 'Vehicle expenses', 'TC-ADMIN', 'T1', 1)
		, ('CP36', 'Administration and office expenses', 'TC-ADMIN', 'T1', 1)
		, ('CP43', 'Interest Received', 'TC-INCOME', 'T0', 1)
		, ('CP46', 'Depreciation Adjustrment', 'TC-ASSETAJ', 'N/A', -1)
		, ('CP47', 'Disallowable Entertainment', 'TC-SALESAJ', 'T1', 1)
		, ('CP48', 'Donations Received', 'TC-SALESAJ', 'T0', 1)
		, ('CP49', 'Legal and professional fees', 'TC-SALESAJ', 'T1', 0)
		, ('CP501', 'Gross income from property', 'TC-INCOME', 'T0', 0)
		, ('CP502', 'Ancillary Income', 'TC-INCOME', 'T0', 0)
		, ('CP503', 'Claimed expenses directly related to income from property', 'TC-SALESAJ', 'T0', 0)
		, ('CP507', 'Income from property', 'TC-PROPI', 'T0', 1)
		, ('CP508', 'Expenses directly related to income from property', 'TC-PROPE', 'T0', 1)
		, ('CP51', 'Net loss on sale of fixed assets', 'TC-SALESAJ', 'T0', 1)
		, ('CP510', 'Unallowable property expenses', 'TC-PROPI', 'T0', 1)
		, ('CP52', 'Penalties and fines', 'TC-SALESAJ', 'T1', 0)
		, ('CP53', 'Unpaid employees remuneration', 'TC-SALESAJ', 'N/A', 0)
		, ('CP55', 'Employees remuneration previously disallowed', 'TC-COSTAJ', 'N/A', 1)
		, ('CP57', 'Net profit on sale of fixed assets', 'TC-COSTAJ', 'N/A', 1)
		, ('CP58', 'Non-trade interest received', 'TC-COSTAJ', 'N/A', 1)
		, ('TC100', 'Sales - Home', 'TC-SALES', 'T1', 1)
		, ('TC101', 'Sales - Export', 'TC-SALES', 'T1', 1)
		, ('TC102', 'Sales - Carriage', 'TC-SALES', 'T1', 1)
		, ('TC103', 'Sales - Consultancy', 'TC-SALES', 'T1', 1)
		, ('TC200', 'Commission', 'TC-DIRECT', 'T1', 1)
		, ('TC201', 'Direct Purchase', 'TC-DIRECT', 'T1', 1)
		, ('TC202', 'Direct Purchase - Carriage', 'TC-DIRECT', 'T1', 1)
		, ('TC203', 'Direct Purchase - Materials', 'TC-DIRECT', 'T1', 1)
		, ('TC204', 'Direct Purchase - Sundry', 'TC-DIRECT', 'T1', 1)
		, ('TC205', 'Tooling', 'TC-DIRECT', 'T1', 1)
		, ('TC206', 'Sundry', 'CP37', 'T1', 1)
		, ('TC207', 'Post and Stationary', 'CP37', 'T1', 1)
		, ('TC208', 'Software', 'CP37', 'T1', 1)
		, ('TC209', 'Hardware', 'CP37', 'T1', 1)
		, ('TC210', 'Communications', 'CP37', 'T1', 1)
		, ('TC211', 'Machinery', 'CP37', 'T1', 1)
		, ('TC300', 'Company Cash', 'TC-BANK', 'N/A', 1)
		, ('TC301', 'Account Payment', 'TC-INTERP', 'N/A', 1)
		, ('TC302', 'Transfer Receipt', 'TC-INTERR', 'N/A', 1)
		, ('TC400', 'Rent', 'CP23', 'T0', -1)
		, ('TC401', 'Business Rates', 'CP23', 'N/A', -1)
		, ('TC500', 'Company Loan', 'TC-INVEST', 'N/A', 1)
		, ('TC501', 'Directors Loan', 'TC-INVEST', 'N/A', 1)
		, ('TC600', 'VAT', 'TC-TAXGD', 'N/A', 1)
		, ('TC601', 'Employers NI', 'TC-TAXCO', 'N/A', 1)
		, ('TC602', 'Taxes (Corporation)', 'TC-TAXGD', 'N/A', 1)
		, ('TC603', 'Taxes (General)', 'TC-TAXCO', 'N/A', 1)
		, ('TC900', 'Stock Movement', 'TC-ASSETGP', 'N/A', 1)
		, ('TC901', 'Share Capital', 'TC-LIAB', 'N/A', 1)
		, ('TC902', 'Debt Repayment', 'TC-LIAB', 'N/A', 1)
		;

		IF @CoinTypeCode < 2
		BEGIN
			INSERT INTO [Cash].[tbCode] ([CashCode], [CashDescription], [CategoryCode], [TaxCode], [IsEnabled])
			VALUES ('TC212', 'Miner Fees', 'TC-DIRECT', 'N/A', 1);
		
			UPDATE App.tbOptions
			SET MinerFeeCode = 'TC212';
		END

		--ASSIGN NET PROFIT CALCULATION
		UPDATE App.tbOptions
		SET NetProfitCode = 'AC435', VatCategoryCode = 'TC-VAT';

		--SET HOME TAX CODE
		UPDATE Subject.tbSubject
		SET TaxCode = 'T1'
		WHERE SubjectCode = (SELECT SubjectCode FROM App.tbOptions)

		--CREATE GOV
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @GovAccountName, @SubjectCode = @SubjectCode OUTPUT
		INSERT INTO Subject.tbSubject (SubjectCode, SubjectName, SubjectStatusCode, SubjectTypeCode, TaxCode)
			VALUES (@SubjectCode, @GovAccountName, 1, 7, 'N/A');

		--ASSIGN CASH CODES AND GOV TO TAX TYPES
		UPDATE Cash.tbTaxType
		SET SubjectCode = @SubjectCode, CashCode = 'TC602', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 0;

		UPDATE Cash.tbTaxType
		SET SubjectCode = @SubjectCode, CashCode = 'TC600', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 1;

		UPDATE Cash.tbTaxType
		SET SubjectCode = @SubjectCode, CashCode = 'TC601', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 2;

		UPDATE Cash.tbTaxType
		SET SubjectCode = @SubjectCode, CashCode = 'TC603', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 3;
		
		IF @CoinTypeCode = 2
		BEGIN
			--fiat
			EXEC Subject.proc_DefaultSubjectCode @SubjectName = @BankName, @SubjectCode = @SubjectCode OUTPUT	
			INSERT INTO Subject.tbSubject (SubjectCode, SubjectName, SubjectStatusCode, SubjectTypeCode, TaxCode)
			VALUES (@SubjectCode, @BankName, 1, 5, 'T0');

			EXEC Subject.proc_AddAddress @SubjectCode = @SubjectCode, @Address = @BankAddress;
		END
		ELSE
		BEGIN
			--crypto
			EXEC Subject.proc_DefaultSubjectCode @SubjectName = 'BITCOIN MINER', @SubjectCode = @SubjectCode OUTPUT
			INSERT INTO Subject.tbSubject (SubjectCode, SubjectName, SubjectStatusCode, SubjectTypeCode, TaxCode)
			VALUES (@SubjectCode, 'BITCOIN MINER', 1, 7, 'N/A');

			UPDATE App.tbOptions
			SET MinerAccountCode = @SubjectCode;

			SELECT @SubjectCode = SubjectCode FROM App.tbOptions 
		END

		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CurrentAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, OpeningBalance, SortCode, AccountNumber, CashCode)
		VALUES        (@AccountCode, @SubjectCode, @CurrentAccount, 0, @CA_SortCode, @CA_AccountNumber, 'TC300')

		IF (LEN(COALESCE(@ReserveAccount, '')) > 0)
		BEGIN
			EXEC Subject.proc_DefaultSubjectCode @SubjectName = @ReserveAccount, @SubjectCode = @AccountCode OUTPUT
			INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, OpeningBalance, SortCode, AccountNumber)
			VALUES        (@AccountCode, @SubjectCode, @ReserveAccount, 0, @RA_SortCode, @RA_AccountNumber)
		END

		SELECT @SubjectCode = (SELECT SubjectCode FROM App.tbOptions)

		IF (LEN(COALESCE(@DummyAccount, '')) > 0)
		BEGIN
			EXEC Subject.proc_DefaultSubjectCode @SubjectName = @DummyAccount, @SubjectCode = @AccountCode OUTPUT
			INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, CashCode)
			VALUES        (@AccountCode, @SubjectCode, @DummyAccount, 1, NULL);
		END

		--CAPITAL 
		DECLARE @CapitalAccount NVARCHAR(50);

		SET @CapitalAccount = 'LONGTERM LIABILITIES';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 50, 'TC902', 0);

		SET @CapitalAccount = 'CALLED UP SHARE CAPITAL';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 60, 'TC901', 0);

		SET @CapitalAccount = 'PLANT AND MACHINERY';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 30, 'CP28', 1);

		SET @CapitalAccount = 'DEPRECIATION ADJUSTMENTS';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 40, 'CP46', 1);

		SET @CapitalAccount = 'STOCK';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 10, 'TC900', 1);

		SET @CapitalAccount = 'VEHICLES';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 20, 'CP28', 1);

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_TemplateCompanyGeneral]...';


go
CREATE PROCEDURE App.proc_TemplateCompanyGeneral
(
	@FinancialMonth SMALLINT = 4,
	@GovAccountName NVARCHAR(255),
	@BankName NVARCHAR(255) = null,
	@BankAddress NVARCHAR(MAX) = null,
	@DummyAccount NVARCHAR(50), 
	@CurrentAccount NVARCHAR(50) = null,
	@CA_SortCode NVARCHAR(10) = null,
	@CA_AccountNumber NVARCHAR(20) = null,
	@ReserveAccount NVARCHAR(50) = null, 
	@RA_SortCode NVARCHAR(10) = null,
	@RA_AccountNumber NVARCHAR(20) = null
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE
			@CoinTypeCode SMALLINT = (SELECT TOP (1) CoinTypeCode FROM App.tbOptions),
			@SubjectCode NVARCHAR(10),
			@AccountCode NVARCHAR(10);

		INSERT INTO [App].[tbBucket] ([Period], [BucketId], [BucketDescription], [AllowForecasts])
		VALUES (0, 'Overdue', 'Overdue Orders', 0)
		, (1, 'Current', 'Current Week', 0)
		, (2, 'Week 2', 'Week Two', 0)
		, (3, 'Week 3', 'Week Three', 0)
		, (4, 'Week 4', 'Week Four', 0)
		, (8, 'Next Month', 'Next Month', 0)
		, (16, '2 Months', '2 Months', 1)
		, (52, 'Forward', 'Forward Orders', 1)
		;
		INSERT INTO [App].[tbUom] ([UnitOfMeasure])
		VALUES ('each')
		, ('days')
		, ('hrs')
		, ('kilo')
		, ('miles')
		, ('mins')
		, ('units')
		;

		DECLARE @Decimals smallint = CASE @CoinTypeCode WHEN 2 THEN 2 ELSE 3 END

		INSERT INTO [App].[tbTaxCode] ([TaxCode], [TaxRate], [TaxDescription], [TaxTypeCode], [RoundingCode], [Decimals])
		VALUES ('INT', 0, 'Interest Tax', 3, 0, @Decimals)
		, ('N/A', 0, 'Untaxed', 3, 0, @Decimals)
		, ('NI1', 0, 'Directors National Insurance', 2, 0, @Decimals)
		, ('NI2', 0.121, 'Employees National Insurance', 2, 0, @Decimals)
		, ('T0', 0, 'Zero Rated VAT', 1, 0, @Decimals)
		, ('T1', 0.2, 'Standard VAT Rate', 1, 0, @Decimals)
		, ('T9', 0, 'TBC', 1, 0, @Decimals)

		INSERT INTO Cash.tbCategory (CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled)
		VALUES ('AL', 'Assets and Liabilities', 1, 2, 0, 20, 1)
		, ('AS', 'Assets', 0, 1, 2, 70, 1)
		, ('BA', 'Bank Accounts', 0, 2, 2, 80, 1)
		, ('BP', 'Bank Payments', 0, 0, 0, 90, 1)
		, ('BR', 'Bank Receipts', 0, 1, 0, 100, 1)
		, ('DB', 'Directors Bank Account', 0, 1, 0, 0, 1)
		, ('DBA', 'Director Account', 1, 2, 0, 11, 1)
		, ('DC', 'Direct Cost', 0, 0, 0, 20, 1)
		, ('DI', 'Dividends', 0, 0, 0, 110, -1)
		, ('DR', 'Drawings', 0, 2, 0, 150, 0)
		, ('EX', 'Expenses', 1, 2, 0, 10, 1)
		, ('FY', 'Profit for Financial Year', 1, 2, 0, 50, 1)
		, ('IC', 'Indirect Cost', 0, 0, 0, 30, 1)
		, ('IP', 'Intercompany Payment', 0, 0, 2, 120, 1)
		, ('IR', 'Intercompany Receipt', 0, 1, 2, 130, 1)
		, ('IV', 'Investment', 0, 2, 0, 160, 1)
		, ('LI', 'Liabilities', 0, 0, 2, 71, 1)
		, ('PL', 'Profit Before Taxation', 1, 2, 0, 30, 1)
		, ('SA', 'Sales', 0, 1, 0, 10, 1)
		, ('TA', 'Tax on Company', 0, 0, 1, 60, 1)
		, ('TO', 'Turnover', 1, 2, 0, 0, 1)
		, ('TP', 'Tax on Profit', 1, 2, 0, 40, 1)
		, ('TR', 'Trading Profit', 1, 2, 0, 12, 1)
		, ('TV', 'Tax on Goods', 0, 0, 1, 61, -1)
		, ('VAT', 'Vat Cash Codes', 1, 2, 0, 100, 1)
		, ('WA', 'Wages', 0, 0, 0, 50, 1)
		;

		INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
		VALUES ('ACCOUNTS', 'Professional Fees', 'IC', 'T1', 1)
		, ('ADMIN', 'Company Administration', 'IC', 'T1', 1)
		, ('BANKINTR', 'Bank Interest', 'BR', 'N/A', 1)
		, ('BC', 'Bank Charges', 'BP', 'N/A', 1)
		, ('CAPITAL', 'Share Capital', 'LI', 'N/A', 1)
		, ('CASH', 'Company Cash', 'BA', 'N/A', 1)
		, ('COMS', 'Communications', 'IC', 'T1', 1)
		, ('DEBTWRITEOFF', 'Capital Debt Write-off', 'DB', 'N/A', 1)
		, ('DEPR', 'Depreciation', 'AS', 'N/A', 1)
		, ('DIVIDEND', 'Dividends', 'DI', 'N/A', 1)
		, ('DLAP', 'Directors Personal Bank', 'DB', 'N/A', 1)
		, ('EQUIP', 'Equipment Expensed', 'IC', 'T1', 1)
		, ('EXPENSES', 'Directors Expenses reimbursement', 'IC', 'N/A', 1)
		, ('IT', 'IT and Software', 'IC', 'T1', 1)
		, ('LOANCOM', 'Company Loan', 'IV', 'N/A', -1)
		, ('LOANDIR', 'Directors Loan', 'IV', 'N/A', 1)
		, ('LOANREPAY', 'Dept Repayment', 'LI', 'N/A', 1)
		, ('MAT', 'Material Purchases', 'DC', 'T1', 1)
		, ('MILEAGE', 'Travel - Car Mileage', 'IC', 'T0', 1)
		, ('NI', 'Employers NI', 'TA', 'N/A', 1)
		, ('OFFICERENT', 'Office Rent', 'IC', 'T0', 1)
		, ('PAYIN', 'Transfer Receipt', 'IR', 'N/A', 1)
		, ('PAYOUT', 'Account Payment', 'IP', 'N/A', 1)
		, ('POST', 'Post and Stationary', 'IC', 'T1', 1)
		, ('PURCHASES', 'Direct Purchase', 'DC', 'T1', 1)
		, ('SALARY', 'Salaries', 'WA', 'NI1', 1)
		, ('SALES', 'Sales', 'SA', 'T1', 1)
		, ('SUNDRYCOST', 'Sundry Costs', 'IC', 'T1', 1)
		, ('TAXCOMPANY', 'Taxes (Corporation)', 'TV', 'N/A', 1)
		, ('TAXGENERAL', 'Taxes (General)', 'TA', 'N/A', 1)
		, ('TAXVAT', 'VAT', 'TV', 'N/A', 1)
		, ('TRAVEL', 'Travel - General', 'IC', 'T1', 1)
		;
		INSERT INTO Cash.tbCategoryTotal (ParentCode, ChildCode)
		VALUES ('AL', 'AS')
		, ('AL', 'LI')
		, ('DBA', 'DB')
		, ('EX', 'BP')
		, ('EX', 'DC')
		, ('EX', 'IC')
		, ('EX', 'WA')
		, ('FY', 'PL')
		, ('FY', 'TP')
		, ('PL', 'AL')
		, ('PL', 'TR')
		, ('TO', 'BR')
		, ('TO', 'IV')
		, ('TO', 'SA')
		, ('TP', 'TA')
		, ('TR', 'DB')
		, ('TR', 'EX')
		, ('TR', 'TO')
		, ('VAT', 'DC')
		, ('VAT', 'IC')
		, ('VAT', 'SA')
		;

		IF @CoinTypeCode < 2
		BEGIN
			INSERT INTO [Cash].[tbCode] ([CashCode], [CashDescription], [CategoryCode], [TaxCode], [IsEnabled])
			VALUES ('MINERFEE', 'Miner Fees', 'IC', 'N/A', 1);
		
			UPDATE App.tbOptions
			SET MinerFeeCode = 'MINERFEE';
		END

		--ASSIGN NET PROFIT CALCULATION
		UPDATE App.tbOptions
		SET NetProfitCode = 'FY', VatCategoryCode = 'VAT';

		--SET HOME TAX CODE
		UPDATE Subject.tbSubject
		SET TaxCode = 'T1'
		WHERE SubjectCode = (SELECT SubjectCode FROM App.tbOptions)

		--CREATE GOV
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @GovAccountName, @SubjectCode = @SubjectCode OUTPUT
		INSERT INTO Subject.tbSubject (SubjectCode, SubjectName, SubjectStatusCode, SubjectTypeCode, TaxCode)
			VALUES (@SubjectCode, @GovAccountName, 1, 7, 'N/A');

		--ASSIGN CASH CODES AND GOV TO TAX TYPES
		UPDATE Cash.tbTaxType
		SET SubjectCode = @SubjectCode, CashCode = 'TAXCOMPANY', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 0;

		UPDATE Cash.tbTaxType
		SET SubjectCode = @SubjectCode, CashCode = 'TAXVAT', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 1;

		UPDATE Cash.tbTaxType
		SET SubjectCode = @SubjectCode, CashCode = 'NI', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 2;

		UPDATE Cash.tbTaxType
		SET SubjectCode = @SubjectCode, CashCode = 'TAXGENERAL', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 3;
		
		IF @CoinTypeCode = 2
		BEGIN
			--fiat
			EXEC Subject.proc_DefaultSubjectCode @SubjectName = @BankName, @SubjectCode = @SubjectCode OUTPUT	
			INSERT INTO Subject.tbSubject (SubjectCode, SubjectName, SubjectStatusCode, SubjectTypeCode, TaxCode)
			VALUES (@SubjectCode, @BankName, 1, 5, 'T0');

			EXEC Subject.proc_AddAddress @SubjectCode = @SubjectCode, @Address = @BankAddress;
		END
		ELSE
		BEGIN
			--crypto
			EXEC Subject.proc_DefaultSubjectCode @SubjectName = 'BITCOIN MINER', @SubjectCode = @SubjectCode OUTPUT
			INSERT INTO Subject.tbSubject (SubjectCode, SubjectName, SubjectStatusCode, SubjectTypeCode, TaxCode)
			VALUES (@SubjectCode, 'BITCOIN MINER', 1, 7, 'N/A');

			UPDATE App.tbOptions
			SET MinerAccountCode = @SubjectCode;

			SELECT @SubjectCode = SubjectCode FROM App.tbOptions 
		END

		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CurrentAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, OpeningBalance, SortCode, AccountNumber, CashCode)
		VALUES        (@AccountCode, @SubjectCode, @CurrentAccount, 0, @CA_SortCode, @CA_AccountNumber, 'CASH')

		IF (LEN(COALESCE(@ReserveAccount, '')) > 0)
		BEGIN
			EXEC Subject.proc_DefaultSubjectCode @SubjectName = @ReserveAccount, @SubjectCode = @AccountCode OUTPUT
			INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, OpeningBalance, SortCode, AccountNumber)
			VALUES        (@AccountCode, @SubjectCode, @ReserveAccount, 0, @RA_SortCode, @RA_AccountNumber)
		END

		SELECT @SubjectCode = (SELECT SubjectCode FROM App.tbOptions)

		IF (LEN(COALESCE(@DummyAccount, '')) > 0)
		BEGIN
			EXEC Subject.proc_DefaultSubjectCode @SubjectName = @DummyAccount, @SubjectCode = @AccountCode OUTPUT
			INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode)
			VALUES        (@AccountCode, @SubjectCode, @DummyAccount, 1);
		END

		--CAPITAL 
		DECLARE @CapitalAccount NVARCHAR(50);

		SET @CapitalAccount = 'LONGTERM LIABILITIES';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 50, 'LOANREPAY', 0);

		SET @CapitalAccount = 'CALLED UP SHARE CAPITAL';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 60, 'CAPITAL', 0);

		SET @CapitalAccount = 'PLANT AND MACHINERY';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 30, 'DEPR', 1);

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_TemplateTutorials]...';


go
CREATE PROCEDURE App.proc_TemplateTutorials
(
	@FinancialMonth SMALLINT = 4,
	@GovAccountName NVARCHAR(255),
	@BankName NVARCHAR(255) = null,
	@BankAddress NVARCHAR(MAX) = null,
	@DummyAccount NVARCHAR(50), 
	@CurrentAccount NVARCHAR(50) = null,
	@CA_SortCode NVARCHAR(10) = null,
	@CA_AccountNumber NVARCHAR(20) = null,
	@ReserveAccount NVARCHAR(50) = null, 
	@RA_SortCode NVARCHAR(10) = null,
	@RA_AccountNumber NVARCHAR(20) = null
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE
			@CoinTypeCode SMALLINT = (SELECT TOP (1) CoinTypeCode FROM App.tbOptions),
			@SubjectCode NVARCHAR(10),
			@AccountCode NVARCHAR(10);

		INSERT INTO [App].[tbBucket] ([Period], [BucketId], [BucketDescription], [AllowForecasts])
		VALUES (0, 'Overdue', 'Overdue Orders', 0)
		, (1, 'Current', 'Current Week', 0)
		, (2, 'Week 2', 'Week Two', 0)
		, (3, 'Week 3', 'Week Three', 0)
		, (4, 'Week 4', 'Week Four', 0)
		, (8, 'Next Month', 'Next Month', 0)
		, (16, '2 Months', '2 Months', 1)
		, (52, 'Forward', 'Forward Orders', 1)
		;
		INSERT INTO [App].[tbUom] ([UnitOfMeasure])
		VALUES ('copies')
		, ('days')
		, ('each')
		, ('hrs')
		, ('kilo')
		, ('miles')
		, ('mins')
		, ('pallets')
		, ('units')
		;

		DECLARE @Decimals smallint = CASE @CoinTypeCode WHEN 2 THEN 2 ELSE 3 END

		INSERT INTO [App].[tbTaxCode] ([TaxCode], [TaxRate], [TaxDescription], [TaxTypeCode], [RoundingCode], [Decimals])
		VALUES ('INT', 0, 'Interest Tax', 3, 0, @Decimals)
		, ('N/A', 0, 'Untaxed', 3, 0, @Decimals)
		, ('NI1', 0, 'Directors National Insurance', 2, 0, @Decimals)
		, ('NI2', 0.121, 'Employees National Insurance', 2, 0, @Decimals)
		, ('T0', 0, 'Zero Rated VAT', 1, 0, @Decimals)
		, ('T1', 0.2, 'Standard VAT Rate', 1, 0, @Decimals)
		, ('T9', 0, 'TBC', 1, 0, @Decimals)
		;

		INSERT INTO [Cash].[tbCategory] ([CategoryCode], [Category], [CategoryTypeCode], [CashPolarityCode], [CashTypeCode], [DisplayOrder], [IsEnabled])
		VALUES ('AS', 'Assets', 0, 1, 2, 70, 1)
		, ('BA', 'Bank Accounts', 0, 2, 2, 80, 1)
		, ('BP', 'Bank Payments', 0, 0, 0, 90, 1)
		, ('BR', 'Bank Receipts', 0, 1, 0, 100, 1)
		, ('DC', 'Direct Cost', 0, 0, 0, 20, 1)
		, ('DI', 'Dividends', 0, 0, 0, 110, -1)
		, ('DR', 'Drawings', 0, 2, 0, 150, 0)
		, ('IC', 'Indirect Cost', 0, 0, 0, 30, 1)
		, ('IP', 'Intercompany Payment', 0, 0, 2, 120, 1)
		, ('IR', 'Intercompany Receipt', 0, 1, 2, 130, 1)
		, ('IV', 'Investment', 0, 2, 0, 160, 1)
		, ('LI', 'Liabilities', 0, 0, 2, 71, 1)
		, ('SA', 'Sales', 0, 1, 0, 10, 1)
		, ('TA1', 'Taxes on Company', 0, 0, 1, 60, 1)
		, ('TA2', 'Taxes on Trade', 0, 0, 1, 60, 1)
		, ('WA', 'Wages', 0, 0, 0, 50, 1)
		;

		INSERT INTO [Cash].[tbCategory] ([CategoryCode], [Category], [CategoryTypeCode], [CashPolarityCode], [CashTypeCode], [DisplayOrder], [IsEnabled])
		VALUES 
			('TO', 'Turnover', 1, 2, 0, 0, 1)			
			, ('EX', 'Expenses', 1, 2, 0, 1, 1)
			, ('AL', 'Assets and Liabilities', 1, 2, 0, 2, 1)
			, ('PL', 'Profit Before Taxation', 1, 2, 0, 3, 1)			
			, ('TP', 'Tax on Profit', 1, 2, 0, 4, 1)
			, ('FY', 'Profit for Financial Year', 1, 2, 0, 5, 1)
			, ('VAT', 'Vat Cash Codes', 1, 2, 0, 100, 1)
			, ('WR', 'Wages Ratio', 2, 2, 0, 0, 1)
			;

		INSERT INTO [Cash].[tbCategoryTotal] ([ParentCode], [ChildCode])
		VALUES ('EX', 'BP')
		, ('EX', 'DC')
		, ('EX', 'IC')
		, ('EX', 'WA')
		, ('FY', 'PL')
		, ('FY', 'TP')
		, ('PL', 'EX')
		, ('PL', 'TO')
		, ('PL', 'AL')
		, ('TO', 'BR')
		, ('TO', 'SA')
		, ('TO', 'IV')
		, ('TP', 'TA1')
		, ('VAT', 'DC')
		, ('VAT', 'IC')
		, ('VAT', 'SA')
		, ('AL', 'AS')
		, ('AL', 'LI')
		;

		INSERT INTO [Cash].[tbCategoryExp] ([CategoryCode], [Expression], [Format])
		VALUES ('WR', 'IF([Sales]=0,0,(ABS([Wages])/[Sales]))', '0%');

		INSERT INTO [Cash].[tbCode] ([CashCode], [CashDescription], [CategoryCode], [TaxCode], [IsEnabled])
		VALUES ('101', 'Sales - Carriage', 'SA', 'T1', 1)
		, ('102', 'Sales - Export', 'SA', 'T1', 1)
		, ('103', 'Sales - Home', 'SA', 'T1', 1)
		, ('104', 'Sales - Consultancy', 'SA', 'T1', 1)
		, ('200', 'Direct Purchase', 'DC', 'T1', 1)
		, ('201', 'Company Administration', 'IC', 'T1', 1)
		, ('202', 'Communications', 'IC', 'T1', 1)
		, ('203', 'Entertaining', 'IC', 'N/A', 1)
		, ('204', 'Office Equipment', 'IC', 'T1', 1)
		, ('205', 'Office Rent', 'IC', 'T0', 1)
		, ('206', 'Professional Fees', 'IC', 'T1', 1)
		, ('207', 'Postage', 'IC', 'T1', 1)
		, ('208', 'Sundry', 'IC', 'T1', 1)
		, ('209', 'Stationery', 'IC', 'T1', 1)
		, ('210', 'Subcontracting', 'IC', 'T1', 1)
		, ('211', 'Systems', 'IC', 'T9', 1)
		, ('212', 'Travel - Car Mileage', 'IC', 'N/A', 1)
		, ('213', 'Travel - General', 'IC', 'N/A', 1)
		, ('214', 'Company Loan', 'IV', 'N/A', 1)
		, ('215', 'Directors Loan', 'IV', 'N/A', 1)
		, ('216', 'Directors Expenses reimbursement', 'IC', 'N/A', 1)
		, ('217', 'Office Expenses (General)', 'IC', 'N/A', 1)
		, ('218', 'Subsistence', 'IC', 'N/A', 1)
		, ('250', 'Commission', 'DC', 'T1', 1)
		, ('301', 'Company Cash', 'BA', 'N/A', 1)
		, ('302', 'Bank Charges', 'BP', 'N/A', 1)
		, ('303', 'Account Payment', 'IP', 'N/A', 1)
		, ('304', 'Bank Interest', 'BR', 'N/A', 1)
		, ('305', 'Transfer Receipt', 'IR', 'N/A', 1)
		, ('401', 'Dividends', 'DI', 'N/A', -1)
		, ('402', 'Salaries', 'WA', 'NI1', 1)
		, ('403', 'Pensions', 'WA', 'N/A', 1)
		, ('501', 'Charitable Donation', 'IC', 'N/A', 1)
		, ('601', 'VAT', 'TA2', 'N/A', 1)
		, ('602', 'Taxes (General)', 'TA1', 'N/A', 1)
		, ('603', 'Taxes (Corporation)', 'TA2', 'N/A', 1)
		, ('604', 'Employers NI', 'TA1', 'N/A', 1)
		, ('700', 'Stock Movement', 'AS', 'N/A', 0)
		, ('701', 'Depreciation', 'AS', 'N/A', 1)
		, ('702', 'Dept Repayment', 'LI', 'N/A', 1)
		, ('703', 'Share Capital', 'LI', 'N/A', 1)
		;

		IF @CoinTypeCode < 2
		BEGIN
			INSERT INTO [Cash].[tbCode] ([CashCode], [CashDescription], [CategoryCode], [TaxCode], [IsEnabled])
			VALUES ('219', 'Miner Fees', 'IC', 'N/A', 1);
		
			UPDATE App.tbOptions
			SET MinerFeeCode = '219';
		END

		--ASSIGN NET PROFIT CALCULATION
		UPDATE App.tbOptions
		SET NetProfitCode = 'FY', VatCategoryCode = 'VAT';

		--SET HOME TAX CODE
		UPDATE Subject.tbSubject
		SET TaxCode = 'T1'
		WHERE SubjectCode = (SELECT SubjectCode FROM App.tbOptions)

		--CREATE GOV
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @GovAccountName, @SubjectCode = @SubjectCode OUTPUT
		INSERT INTO Subject.tbSubject (SubjectCode, SubjectName, SubjectStatusCode, SubjectTypeCode, TaxCode)
			VALUES (@SubjectCode, @GovAccountName, 1, 7, 'N/A');

		--ASSIGN CASH CODES AND GOV TO TAX TYPES
		UPDATE Cash.tbTaxType
		SET SubjectCode = @SubjectCode, CashCode = '603', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 0;

		UPDATE Cash.tbTaxType
		SET SubjectCode = @SubjectCode, CashCode = '601', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 1;

		UPDATE Cash.tbTaxType
		SET SubjectCode = @SubjectCode, CashCode = '604', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 2;

		UPDATE Cash.tbTaxType
		SET SubjectCode = @SubjectCode, CashCode = '602', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 3;

		--BANK ACCOUNTS / WALLETS

		IF @CoinTypeCode = 2
		BEGIN
			--fiat
			EXEC Subject.proc_DefaultSubjectCode @SubjectName = @BankName, @SubjectCode = @SubjectCode OUTPUT	
			INSERT INTO Subject.tbSubject (SubjectCode, SubjectName, SubjectStatusCode, SubjectTypeCode, TaxCode)
			VALUES (@SubjectCode, @BankName, 1, 5, 'T0');

			EXEC Subject.proc_AddAddress @SubjectCode = @SubjectCode, @Address = @BankAddress;
		END
		ELSE
		BEGIN
			--crypto
			EXEC Subject.proc_DefaultSubjectCode @SubjectName = 'BITCOIN MINER', @SubjectCode = @SubjectCode OUTPUT
			INSERT INTO Subject.tbSubject (SubjectCode, SubjectName, SubjectStatusCode, SubjectTypeCode, TaxCode)
			VALUES (@SubjectCode, 'BITCOIN MINER', 1, 7, 'N/A');

			UPDATE App.tbOptions
			SET MinerAccountCode = @SubjectCode;

			SELECT @SubjectCode = SubjectCode FROM App.tbOptions 
		END

		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CurrentAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, OpeningBalance, SortCode, AccountNumber, CashCode)
		VALUES        (@AccountCode, @SubjectCode, @CurrentAccount, 0, @CA_SortCode, @CA_AccountNumber, '301')

		IF (LEN(@ReserveAccount) > 0)
		BEGIN
			EXEC Subject.proc_DefaultSubjectCode @SubjectName = @ReserveAccount, @SubjectCode = @AccountCode OUTPUT
			INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, OpeningBalance, SortCode, AccountNumber)
			VALUES        (@AccountCode, @SubjectCode, @ReserveAccount, 0, @RA_SortCode, @RA_AccountNumber)
		END

		SELECT @SubjectCode = (SELECT SubjectCode FROM App.tbOptions)

		IF (LEN(@DummyAccount) > 0)
		BEGIN
			EXEC Subject.proc_DefaultSubjectCode @SubjectName = @DummyAccount, @SubjectCode = @AccountCode OUTPUT
			INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode)
			VALUES        (@AccountCode, @SubjectCode, @DummyAccount, 1);
		END

		--CAPITAL 
		DECLARE @CapitalAccount NVARCHAR(50);

		SET @CapitalAccount = 'PREMISES';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 50, '701', 1);

		SET @CapitalAccount = 'FIXTURES AND FITTINGS';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 40, '701', 1);

		SET @CapitalAccount = 'PLANT AND MACHINERY';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 30, '701', 1);

		SET @CapitalAccount = 'VEHICLES';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 20, '701', 1);

		SET @CapitalAccount = 'STOCK';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 10, '700', 1)

		SET @CapitalAccount = 'LONGTERM LIABILITIES';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 50, '702', 0);

		SET @CapitalAccount = 'CALLED UP SHARE CAPITAL';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 60, '703', 0);


	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog
	END CATCH
go
PRINT N'Creating Procedure [Project].[proc_Pay]...';


go
CREATE PROCEDURE Project.proc_Pay (@ProjectCode NVARCHAR(20), @Post BIT = 0,	@PaymentCode nvarchar(20) NULL OUTPUT)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		
		DECLARE 
			@InvoiceTypeCode smallint
			, @InvoiceNumber NVARCHAR(20)
			, @InvoicedOn DATETIME = CURRENT_TIMESTAMP

		SELECT @InvoiceTypeCode = CASE CashPolarityCode WHEN 0 THEN 2 ELSE 0 END, @InvoicedOn = Project.tbProject.PaymentOn
		FROM  Project.tbProject INNER JOIN
				Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
				Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode AND Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode AND 
				Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
		WHERE Project.tbProject.ProjectCode = @ProjectCode
		
		EXEC Invoice.proc_Raise @ProjectCode = @ProjectCode, @InvoiceTypeCode = @InvoiceTypeCode, @InvoicedOn = @InvoicedOn, @InvoiceNumber = @InvoiceNumber OUTPUT
		EXEC Invoice.proc_Accept @InvoiceNumber
		EXEC Invoice.proc_Pay @InvoiceNumber = @InvoiceNumber, @PaidOn = @InvoicedOn, @Post = @Post, @PaymentCode = @PaymentCode OUTPUT

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Invoice].[proc_PostEntries]...';


go
CREATE   PROCEDURE Invoice.proc_PostEntries
AS
	DECLARE @UserId nvarchar(10) = (SELECT UserId FROM Usr.vwCredentials);
	EXECUTE Invoice.proc_PostEntriesById @UserId;
go
PRINT N'Creating Procedure [Cash].[proc_PaymentPost]...';


go
CREATE PROCEDURE Cash.proc_PaymentPost
AS
	DECLARE @UserId nvarchar(10) = (SELECT UserId FROM Usr.vwCredentials);
	EXECUTE Cash.proc_PaymentPostById @UserId;
go
PRINT N'Creating Procedure [Cash].[proc_TxInvoice]...';


go
CREATE   PROCEDURE Cash.proc_TxInvoice (@PaymentAddress nvarchar(42), @TxId nvarchar(64))
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY

		DECLARE @PaymentCode nvarchar(20);

		IF EXISTS (
				SELECT * 
				FROM Cash.tbTx 
				WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress AND TxStatusCode = 0
			)
			AND EXISTS (
				SELECT * 
				FROM Cash.tbTx tx
					JOIN Cash.tbChangeReference ref ON tx.PaymentAddress = ref.PaymentAddress 
					JOIN Invoice.tbInvoice inv ON ref.InvoiceNumber = inv.InvoiceNumber 
				WHERE tx.TxId = @TxId AND inv.InvoiceStatusCode < 3	
			)
			AND NOT EXISTS (
				SELECT * 
				FROM Cash.tbTxReference ref 
					JOIN Cash.tbTx tx ON tx.TxNumber = ref.TxNumber WHERE tx.TxId = @TxId AND tx.PaymentAddress = @PaymentAddress
			)		
		BEGIN
			EXEC Cash.proc_NextPaymentCode @PaymentCode output;

			INSERT INTO Cash.tbPayment (PaymentCode, UserId, PaymentStatusCode, SubjectCode, AccountCode, PaidInValue, PaymentReference)
			SELECT @PaymentCode PaymentCode, (SELECT UserId FROM Usr.vwCredentials) UserId, 0 PaymentStatusCode, invoice.SubjectCode, change.AccountCode, tx.MoneyIn - tx.MoneyOut PaidInValue, invoice.InvoiceNumber
			FROM Cash.tbTx tx
				JOIN Cash.tbChange change ON tx.PaymentAddress = change.PaymentAddress
				JOIN Cash.tbChangeReference ref ON change.PaymentAddress = ref.PaymentAddress
				JOIN Invoice.tbInvoice invoice ON ref.InvoiceNumber = invoice.InvoiceNumber
			WHERE tx.TxId = @TxId;

			UPDATE Cash.tbTx
			SET TxStatusCode = 1
			WHERE TxId = @TxId;

			INSERT INTO Cash.tbTxReference (TxNumber, TxStatusCode, PaymentCode)
			SELECT TxNumber, TxStatusCode, @PaymentCode PaymentCode
			FROM Cash.tbTx
			WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress;

			Exec Cash.proc_PaymentPost;

		END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_TxPayAccount]...';


go
CREATE   PROCEDURE Cash.proc_TxPayAccount
(
	@PaymentAddress nvarchar(42)
	, @TxId nvarchar(64)
	, @Spent decimal(18, 5)
	, @MinerFee decimal(18, 5)
	, @SubjectCode nvarchar(10)
)
AS
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY

		DECLARE 						
			@PaymentCode nvarchar(20)
			, @TxNumber int
			, @PaidOutValue decimal(18, 5)
			, @AccountCode nvarchar(10) = (SELECT AccountCode FROM Cash.tbChange WHERE PaymentAddress = @PaymentAddress)
			, @UserId nvarchar(10) = (SELECT UserId FROM Usr.vwCredentials)
			;

		SELECT @TxNumber = TxNumber, @PaidOutValue = MoneyIn - @MinerFee
		FROM Cash.tbTx 
		WHERE PaymentAddress = @PaymentAddress AND TxId = @TxId;

		BEGIN TRAN

		UPDATE Cash.tbTx
		SET
			TxStatusCode = 2, 
			MoneyOut = @Spent
		WHERE PaymentAddress = @PaymentAddress AND TxNumber = @TxNumber;

		EXEC Cash.proc_NextPaymentCode @PaymentCode OUTPUT;
		
		INSERT INTO Cash.tbPayment (PaymentCode, UserId, PaidOn, SubjectCode, AccountCode, PaidOutValue)
		VALUES (@PaymentCode, @UserId, CURRENT_TIMESTAMP, @SubjectCode, @AccountCode, @PaidOutValue);

		EXEC Cash.proc_PaymentPost;

		INSERT INTO Cash.tbTxReference (TxNumber, TxStatusCode, PaymentCode)
		VALUES (@TxNumber, 2, @PaymentCode);

		IF @MinerFee > 0
		BEGIN
			EXEC Cash.proc_NextPaymentCode @PaymentCode OUTPUT;
		
			INSERT INTO Cash.tbPayment (PaymentCode, UserId, PaidOn, PaymentStatusCode, SubjectCode, AccountCode, CashCode, TaxCode, PaidOutValue)
			SELECT @PaymentCode PaymentCode, @UserId UserId, CURRENT_TIMESTAMP PaidOn, 0 PaymentStatusCode, options.MinerAccountCode SubjectCode, @AccountCode AccountCode,
				cash_code.CashCode CashCode, cash_code.TaxCode TaxCode, @MinerFee PaidOutValue
			FROM App.tbOptions options
				JOIN Cash.tbCode cash_code ON options.MinerFeeCode = cash_code.CashCode;				

			EXEC Cash.proc_PaymentPost;
		END
		
		COMMIT TRAN

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_TxPayOutMisc]...';


go
CREATE   PROCEDURE Cash.proc_TxPayOutMisc
(
	@PaymentAddress nvarchar(42)
	, @TxId nvarchar(64)
	, @Spent decimal(18, 5)
	, @MinerFee decimal(18, 5)
	, @SubjectCode nvarchar(10)
	, @CashCode nvarchar(50)
	, @TaxCode nvarchar(10)
	, @PaymentReference nvarchar(50)
)
AS
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY

		DECLARE 						
			@PaymentCode nvarchar(20)
			, @TxNumber int
			, @PaidOutValue decimal(18, 5)
			, @AccountCode nvarchar(10) = (SELECT AccountCode FROM Cash.tbChange WHERE PaymentAddress = @PaymentAddress)
			, @UserId nvarchar(10) = (SELECT UserId FROM Usr.vwCredentials)
			;

		SELECT @TxNumber = TxNumber, @PaidOutValue = MoneyIn - @MinerFee
		FROM Cash.tbTx 
		WHERE PaymentAddress = @PaymentAddress AND TxId = @TxId;

		BEGIN TRAN

		UPDATE Cash.tbTx
		SET
			TxStatusCode = 2, 
			MoneyOut = @Spent
		WHERE PaymentAddress = @PaymentAddress AND TxNumber = @TxNumber;

		EXEC Cash.proc_NextPaymentCode @PaymentCode OUTPUT;
		
		INSERT INTO Cash.tbPayment (PaymentCode, UserId, PaidOn, SubjectCode, AccountCode, CashCode, TaxCode, PaidOutValue, PaymentStatusCode)
		VALUES (@PaymentCode, @UserId, CURRENT_TIMESTAMP, @SubjectCode, @AccountCode, @CashCode, @TaxCode, @PaidOutValue, 1);

		UPDATE  Subject.tbAccount
		SET CurrentBalance = Subject.tbAccount.CurrentBalance - PaidOutValue
		FROM         Subject.tbAccount INNER JOIN
							  Cash.tbPayment ON Subject.tbAccount.AccountCode = Cash.tbPayment.AccountCode
		WHERE Cash.tbPayment.PaymentCode = @PaymentCode

		INSERT INTO Cash.tbTxReference (TxNumber, TxStatusCode, PaymentCode)
		VALUES (@TxNumber, 2, @PaymentCode);

		IF @MinerFee > 0
		BEGIN
			EXEC Cash.proc_NextPaymentCode @PaymentCode OUTPUT;
		
			INSERT INTO Cash.tbPayment (PaymentCode, UserId, PaidOn, PaymentStatusCode, SubjectCode, AccountCode, CashCode, TaxCode, PaidOutValue)
			SELECT @PaymentCode PaymentCode, @UserId UserId, CURRENT_TIMESTAMP PaidOn, 0 PaymentStatusCode, options.MinerAccountCode SubjectCode, @AccountCode AccountCode,
				cash_code.CashCode CashCode, cash_code.TaxCode TaxCode, @MinerFee PaidOutValue
			FROM App.tbOptions options
				JOIN Cash.tbCode cash_code ON options.MinerFeeCode = cash_code.CashCode;				

			EXEC Cash.proc_PaymentPost;
		END
		
		COMMIT TRAN

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_TxPayIn]...';


go
CREATE   PROCEDURE Cash.proc_TxPayIn
(
	@AccountCode nvarchar(10), 
	@PaymentAddress nvarchar(42),
	@TxId nvarchar(64),
	@SubjectCode nvarchar(10), 
	@CashCode nvarchar(50), 
	@PaidOn datetime, 
	@PaymentReference nvarchar(50) = null, 
	@PaymentCode nvarchar(20) output)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		DECLARE @ToPay decimal(18, 5), @Confirmations int;

		SELECT @ToPay = MoneyIn - MoneyOut, @Confirmations = Confirmations 
		FROM Cash.tbTx 
		WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress

		IF NOT EXISTS (SELECT * FROM Subject.tbSubject WHERE SubjectCode = @SubjectCode)
			SELECT @SubjectCode = SubjectCode FROM App.vwHomeAccount;
		ELSE IF @Confirmations = 0 
			RETURN 1;

		BEGIN TRAN

		EXEC Cash.proc_PaymentAdd @SubjectCode, @AccountCode, @CashCode, @PaidOn, @ToPay, @PaymentReference, @PaymentCode OUTPUT;

		UPDATE Cash.tbTx
		SET TxStatusCode = 1
		WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress;

		INSERT INTO Cash.tbTxReference (TxNumber, PaymentCode, TxStatusCode)
		SELECT TxNumber, @PaymentCode PaymentCode, TxStatusCode
		FROM Cash.tbTx
		WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress;

		IF EXISTS (SELECT * FROM Cash.tbPayment WHERE PaymentCode = @PaymentCode AND PaymentStatusCode = 2)
			EXEC Cash.proc_PayAccrual @PaymentCode;
		ELSE
			EXEC Cash.proc_PaymentPost;

		COMMIT TRAN

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_TxPayOutTransfer]...';


go
CREATE   PROCEDURE Cash.proc_TxPayOutTransfer
(
	@PaymentAddress nvarchar(42)
	, @TxId nvarchar(64)
	, @Spent decimal(18, 5)
	, @MinerFee decimal(18, 5)
	, @CashCode nvarchar(50)
	, @PaymentReference nvarchar(50)
)
AS
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY

		DECLARE 						
			@SubjectCode nvarchar(10)
			, @TaxCode nvarchar(10)
			, @PaymentCode nvarchar(20)
			, @TxNumber int
			, @PaidOutValue decimal(18, 5)
			, @AccountCode nvarchar(10) = (SELECT AccountCode FROM Cash.tbChange WHERE PaymentAddress = @PaymentAddress)
			, @UserId nvarchar(10) = (SELECT UserId FROM Usr.vwCredentials)
			;

		SELECT @TxNumber = TxNumber, @PaidOutValue = MoneyIn - @MinerFee
		FROM Cash.tbTx 
		WHERE PaymentAddress = @PaymentAddress AND TxId = @TxId;

		SELECT @SubjectCode = SubjectCode FROM App.vwHomeAccount;
		SELECT @TaxCode = TaxCode FROM Cash.tbCode WHERE CashCode = @CashCode;

		BEGIN TRAN

		UPDATE Cash.tbTx
		SET
			TxStatusCode = 2, 
			MoneyOut = @Spent
		WHERE PaymentAddress = @PaymentAddress AND TxNumber = @TxNumber;

		EXEC Cash.proc_NextPaymentCode @PaymentCode OUTPUT;
		
		INSERT INTO Cash.tbPayment (PaymentCode, UserId, PaidOn, SubjectCode, AccountCode, CashCode, TaxCode, PaidOutValue, PaymentReference)
		VALUES (@PaymentCode, @UserId, CURRENT_TIMESTAMP, @SubjectCode, @AccountCode, @CashCode, @TaxCode, @PaidOutValue, @PaymentReference);

		IF EXISTS (SELECT * FROM Cash.tbPayment WHERE PaymentCode = @PaymentCode AND PaymentStatusCode = 2)
			EXEC Cash.proc_PayAccrual @PaymentCode;
		ELSE
			EXEC Cash.proc_PaymentPost;

		INSERT INTO Cash.tbTxReference (TxNumber, TxStatusCode, PaymentCode)
		VALUES (@TxNumber, 2, @PaymentCode);

		IF @MinerFee > 0
		BEGIN
			EXEC Cash.proc_NextPaymentCode @PaymentCode OUTPUT;
		
			INSERT INTO Cash.tbPayment (PaymentCode, UserId, PaidOn, PaymentStatusCode, SubjectCode, AccountCode, CashCode, TaxCode, PaidOutValue, PaymentReference)
			SELECT @PaymentCode PaymentCode, @UserId UserId, CURRENT_TIMESTAMP PaidOn, 0 PaymentStatusCode, options.MinerAccountCode SubjectCode, @AccountCode AccountCode,
				cash_code.CashCode CashCode, cash_code.TaxCode TaxCode, @MinerFee PaidOutValue, @PaymentReference PaymentReference
			FROM App.tbOptions options
				JOIN Cash.tbCode cash_code ON options.MinerFeeCode = cash_code.CashCode;	

			EXEC Cash.proc_PaymentPost;
		END
		
		COMMIT TRAN

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog
	END CATCH
go
PRINT N'Creating Procedure [App].[proc_DemoBom]...';


go
CREATE PROCEDURE App.proc_DemoBom
(
	@CreateOrders BIT = 0,
	@InvoiceOrders BIT = 0,
	@PayInvoices BIT = 0
)
AS
	 SET NOCOUNT, XACT_ABORT ON;
	 
	 BEGIN TRY
	
		IF NOT EXISTS (SELECT * FROM Usr.vwCredentials WHERE IsAdministrator <> 0)
		BEGIN
			DECLARE @Msg NVARCHAR(100) = CONCAT('Access Denied: User ', SUSER_SNAME(), ' is not an administrsator');
			RAISERROR ('%s', 13, 1, @Msg);
		END
	
		DECLARE @ExchangeRate float = CASE (SELECT UnitOfCharge FROM App.tbOptions) 
										WHEN 'BTC' THEN 0.135 
										ELSE 1 
										END				

		BEGIN TRAN

		-->>>>>>>>>>>>> RESET >>>>>>>>>>>>>>>>>>>>>>>>>>>
		DELETE FROM Cash.tbPayment;
		DELETE FROM Invoice.tbInvoice;
		DELETE FROM Project.tbFlow;
		DELETE FROM Project.tbProject;
		DELETE FROM Object.tbFlow;
		DELETE FROM Object.tbObject;

		--WITH sys_accounts AS
		--(
		--	SELECT SubjectCode FROM App.tbOptions
		--	UNION
		--	SELECT DISTINCT SubjectCode FROM Subject.tbAccount
		--	UNION
		--	SELECT DISTINCT SubjectCode FROM Cash.tbTaxType
		--), candidates AS
		--(
		--	SELECT SubjectCode
		--	FROM Subject.tbSubject
		--	EXCEPT
		--	SELECT SubjectCode 
		--	FROM sys_accounts
		--)
		--DELETE Subject.tbSubject 
		--FROM Subject.tbSubject JOIN candidates ON Subject.tbSubject.SubjectCode = candidates.SubjectCode;
		
		UPDATE App.tbOptions
		SET IsAutoOffsetDays = 0;

		EXEC App.proc_SystemRebuild;
		--<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

		IF NOT EXISTS( SELECT * FROM App.tbRegister WHERE RegisterName = 'Works Order')
			INSERT INTO App.tbRegister (RegisterName, NextNumber)
			SELECT 'Works Order', (SELECT MAX(NextNumber) + 10000 FROM App.tbRegister) AS NextNumber;

		INSERT INTO Object.tbObject (ObjectCode, ProjectStatusCode, ObjectDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
		VALUES ('M/00/70/00', 1, 'PIGEON HOLE SHELF ASSEMBLY CLEAR', 'each', '103', 1.67 * @ExchangeRate, 1, 'Sales Order')
		, ('M/100/70/00', 1, 'PIGEON HOLE SUB SHELF CLEAR', 'each', NULL, 0.0000, 0, 'Works Order')
		, ('M/101/70/00', 1, 'PIGEON HOLE BACK DIVIDER', 'each', NULL, 0.0000, 0, 'Works Order')
		, ('M/97/70/00', 1, 'SHELF DIVIDER (WIDE FOOT)', 'each', NULL, 0.0000, 0, 'Works Order')
		, ('M/99/70/00', 1, 'SHELF DIVIDER (NARROW FOOT)', 'each', NULL, 0.0000, 0, 'Works Order')
		, ('PALLET/01', 1, 'EURO 3 1200 x 800 4 WAY', 'each', '200', 2.4 * @ExchangeRate, 1, 'Purchase Order')
		, ('BOX/41', 1, 'PIGEON ASSY 125KTB S WALL 404x220x90', 'each', '200', 0.05 * @ExchangeRate, 1, 'Purchase Order')
		, ('BOX/99', 1, 'INTERNAL USE ANY BLACK,BLUE,RED ANY', 'each', NULL, 0.0000, 0, 'Works Order')
		, ('PC/999', 1, 'CALIBRE 303EP CLEAR UL94-V2', 'kilo', '200', 0.22 * @ExchangeRate, 1, 'Purchase Order')
		, ('INSERT/09', 1, 'HEAT-LOK SHK B M3.5 HEADED BRASS 8620035-80', 'each', '200', 0.005 * @ExchangeRate, 1, 'Purchase Order')
		, ('PROJECT', 0, NULL, 'each', NULL, 0, 0, 'Works Order')
		, ('DELIVERY', 1, NULL, 'each', '200', 0, 1, 'Purchase Order')
		;
		INSERT INTO Object.tbAttribute (ObjectCode, Attribute, PrintOrder, AttributeTypeCode, DefaultText)
		VALUES ('M/00/70/00', 'Colour', 20, 0, 'CLEAR')
		, ('M/00/70/00', 'Colour Number', 10, 0, '-')
		, ('M/00/70/00', 'Count Type', 50, 0, 'Weigh Count')
		, ('M/00/70/00', 'Drawing Issue', 40, 0, '1')
		, ('M/00/70/00', 'Drawing Number', 30, 0, '321554')
		, ('M/00/70/00', 'Label Type', 70, 0, 'Assembly Card')
		, ('M/00/70/00', 'Mould Tool Specification', 110, 1, NULL)
		, ('M/00/70/00', 'Pack Type', 60, 0, 'Despatched')
		, ('M/00/70/00', 'Quantity/Box', 80, 0, '100')
		, ('M/100/70/00', 'Cavities', 170, 0, '1')
		, ('M/100/70/00', 'Colour', 20, 0, 'CLEAR')
		, ('M/100/70/00', 'Colour Number', 10, 0, '-')
		, ('M/100/70/00', 'Count Type', 50, 0, 'Weigh Count')
		, ('M/100/70/00', 'Drawing Issue', 40, 0, '1')
		, ('M/100/70/00', 'Drawing Number', 30, 0, '321554-01')
		, ('M/100/70/00', 'Impressions', 180, 0, '1')
		, ('M/100/70/00', 'Label Type', 70, 0, 'Route Card')
		, ('M/100/70/00', 'Location', 150, 0, 'STORES')
		, ('M/100/70/00', 'Pack Type', 60, 0, 'Assembled')
		, ('M/100/70/00', 'Part Weight', 160, 0, '175g')
		, ('M/100/70/00', 'Quantity/Box', 80, 0, '100')
		, ('M/100/70/00', 'Tool Number', 190, 0, '1437')
		, ('M/101/70/00', 'Cavities', 170, 0, '2')
		, ('M/101/70/00', 'Colour', 20, 0, 'CLEAR')
		, ('M/101/70/00', 'Colour Number', 10, 0, '-')
		, ('M/101/70/00', 'Count Type', 50, 0, 'Weigh Count')
		, ('M/101/70/00', 'Drawing Issue', 40, 0, '1')
		, ('M/101/70/00', 'Drawing Number', 30, 0, '321554-02')
		, ('M/101/70/00', 'Impressions', 180, 0, '2')
		, ('M/101/70/00', 'Label Type', 70, 0, 'Route Card')
		, ('M/101/70/00', 'Location', 150, 0, 'STORES')
		, ('M/101/70/00', 'Pack Type', 60, 0, 'Assembled')
		, ('M/101/70/00', 'Part Weight', 160, 0, '61g')
		, ('M/101/70/00', 'Quantity/Box', 80, 0, '100')
		, ('M/101/70/00', 'Tool Number', 190, 0, '1439')
		, ('M/97/70/00', 'Cavities', 170, 0, '4')
		, ('M/97/70/00', 'Colour', 20, 0, 'CLEAR')
		, ('M/97/70/00', 'Colour Number', 10, 0, '-')
		, ('M/97/70/00', 'Count Type', 50, 0, 'Weigh Count')
		, ('M/97/70/00', 'Drawing Issue', 40, 0, '1')
		, ('M/97/70/00', 'Drawing Number', 30, 0, '321554A')
		, ('M/97/70/00', 'Impressions', 180, 0, '4')
		, ('M/97/70/00', 'Label Type', 70, 0, 'Route Card')
		, ('M/97/70/00', 'Location', 150, 0, 'STORES')
		, ('M/97/70/00', 'Pack Type', 60, 0, 'Assembled')
		, ('M/97/70/00', 'Part Weight', 160, 0, '171g')
		, ('M/97/70/00', 'Quantity/Box', 80, 0, '100')
		, ('M/97/70/00', 'Tool Number', 190, 0, '1440')
		, ('M/99/70/00', 'Cavities', 170, 0, '1')
		, ('M/99/70/00', 'Colour', 20, 0, 'CLEAR')
		, ('M/99/70/00', 'Colour Number', 10, 0, '-')
		, ('M/99/70/00', 'Count Type', 50, 0, 'Weigh Count')
		, ('M/99/70/00', 'Drawing Issue', 40, 0, '1')
		, ('M/99/70/00', 'Drawing Number', 30, 0, '321554A')
		, ('M/99/70/00', 'Impressions', 180, 0, '1')
		, ('M/99/70/00', 'Label Type', 70, 0, 'Route Card')
		, ('M/99/70/00', 'Location', 150, 0, 'STORES')
		, ('M/99/70/00', 'Pack Type', 60, 0, 'Assembled')
		, ('M/99/70/00', 'Part Weight', 160, 0, '171g')
		, ('M/99/70/00', 'Quantity/Box', 80, 0, '100')
		, ('M/99/70/00', 'Tool Number', 190, 0, '1441')
		, ('PC/999', 'Colour', 50, 0, 'CLEAR')
		, ('PC/999', 'Grade', 20, 0, '303EP')
		, ('PC/999', 'Location', 60, 0, 'R2123-9')
		, ('PC/999', 'Material Type', 10, 0, 'PC')
		, ('PC/999', 'Name', 30, 0, 'Calibre')
		, ('PC/999', 'SG', 40, 0, '1.21')
		;
		INSERT INTO Object.tbOp (ObjectCode, OperationNumber, SyncTypeCode, Operation, Duration, OffsetDays)
		VALUES ('M/00/70/00', 10, 0, 'ASSEMBLE', 0.5, 3)
		, ('M/00/70/00', 20, 0, 'QUALITY CHECK', 0, 0)
		, ('M/00/70/00', 30, 0, 'PACK', 0, 1)
		, ('M/00/70/00', 40, 2, 'DELIVER', 0, 1)
		, ('M/100/70/00', 10, 0, 'MOULD', 10, 2)
		, ('M/100/70/00', 20, 1, 'INSERTS', 0, 0)
		, ('M/100/70/00', 30, 0, 'QUALITY CHECK', 0, 0)
		, ('M/101/70/00', 10, 0, 'MOULD', 10, 0)
		, ('M/101/70/00', 20, 0, 'QUALITY CHECK', 0, 0)
		, ('M/97/70/00', 10, 0, 'MOULD', 10, 2)
		, ('M/97/70/00', 20, 0, 'QUALITY CHECK', 0, 0)
		, ('M/99/70/00', 10, 0, 'MOULD', 0, 2)
		, ('M/99/70/00', 20, 0, 'QUALITY CHECK', 0, 0)
		;
		INSERT INTO Object.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
		VALUES ('M/00/70/00', 10, 'M/100/70/00', 1, 0, 8)
		, ('M/00/70/00', 20, 'M/101/70/00', 1, 0, 4)
		, ('M/00/70/00', 30, 'M/97/70/00', 1, 0, 3)
		, ('M/00/70/00', 40, 'M/99/70/00', 0, 0, 2)
		, ('M/00/70/00', 50, 'BOX/41', 1, 0, 1)
		, ('M/00/70/00', 60, 'PALLET/01', 1, 0, 0.01)
		, ('M/00/70/00', 70, 'DELIVERY', 2, 1, 0)
		, ('M/100/70/00', 10, 'BOX/99', 1, 0, 0.01)
		, ('M/100/70/00', 20, 'PC/999', 1, 0, 0.175)
		, ('M/101/70/00', 10, 'BOX/99', 1, 0, 0.01)
		, ('M/101/70/00', 20, 'PC/999', 1, 0, 0.061)
		, ('M/97/70/00', 10, 'BOX/99', 1, 0, 0.01)
		, ('M/97/70/00', 20, 'PC/999', 1, 0, 0.172)
		, ('M/99/70/00', 10, 'BOX/99', 1, 0, 0.01)
		, ('M/99/70/00', 20, 'PC/999', 1, 0, 0.171)
		, ('M/100/70/00', 30, 'INSERT/09', 1, 0, 2)
		;

		IF (NOT EXISTS(SELECT * FROM Subject.tbSubject WHERE SubjectCode = 'TFCSPE'))
		BEGIN
			INSERT INTO Subject.tbSubject (SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode, TaxCode, AddressCode, PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance, NumberOfEmployees, CompanyNumber, VatNumber, Turnover, OpeningBalance, EUJurisdiction)
			VALUES 
			  ('PACSER', 'PACKING SERVICES', 8, 1, 'T1', 'PACSER_001', 'EOM', 10, 30, 1, 1, 0, NULL, NULL, 0.0000, 0.0000, 0)
			, ('PALSUP', 'PALLET SUPPLIER', 8, 1, 'T1', 'PALSUP_001', 'COD', 0, -10, 0, 1, 0, NULL, NULL, 0.0000, 0.0000, 0)
			, ('PLAPRO', 'PLASTICS PROVIDER', 8, 1, 'T1', 'PLAPRO_001', '30 days from invoice', 15, 30, 0, 1, 0, NULL, NULL, 0.0000, 0.0000, 0)
			, ('TFCSPE', 'FASTENER SPECIALIST', 8, 1, 'T1', 'TFCSPE_001', 'EOM', 0, 30, 1, 1, 0, NULL, NULL, 0.0000, 0.0000, 0)
			, ('STOBOX', 'STORAGE BOXES', 1, 1, 'T1', 'STOBOX_001', '60 days from invoice', 5, 60, 0, 1, 0, NULL, NULL, 0.0000, 0.0000, 0)
			, ('HAULOG', 'HAULIER LOGISTICS', 8, 1, 'T1', 'HAULOG_001', 'EOM', 0, 30, 1, 1, 0, NULL, NULL, 0.0000, 0.0000, 0)
			;
			INSERT INTO Subject.tbAddress (AddressCode, SubjectCode, Address)
			VALUES ('STOBOX_001', 'STOBOX', 'SURREY GU24 9BJ')
			, ('PACSER_001', 'PACSER', 'FAREHAM, HAMPSHIRE	PO15 5RZ')
			, ('PLAPRO_001', 'PLAPRO', 'WARRINGTON, CHESHIRE WA1 4RA')
			, ('PALSUP_001', 'PALSUP', 'HAMPSHIRE PO13 9NY')
			, ('TFCSPE_001', 'TFCSPE', 'ESSEX CO4 9TZ')
			, ('HAULOG_001', 'HAULOG', 'BERKSHIRE SL3 0BH')
			;
		END

		-- ***************************************************************************
		IF @CreateOrders = 0
			GOTO CommitTran;
		-- ***************************************************************************

		DECLARE @UserId NVARCHAR(10) = (SELECT UserId FROM Usr.vwCredentials),
			@ProjectCode NVARCHAR(20),
			@ParentProjectCode NVARCHAR(20), 
			@ToProjectCode NVARCHAR(20),
			@Quantity DECIMAL(18, 4) = 100;

		EXEC Project.proc_NextCode 'PROJECT', @ParentProjectCode OUTPUT
		INSERT INTO Project.tbProject
								 (ProjectCode, UserId, SubjectCode, ProjectTitle, ObjectCode, ProjectStatusCode, ActionById)
		VALUES        (@ParentProjectCode,@UserId, 'STOBOX', N'PIGEON HOLE SHELF ASSEMBLY', N'PROJECT', 0,@UserId)
	
		EXEC Project.proc_NextCode 'M/00/70/00', @ProjectCode OUTPUT
		
		INSERT INTO Project.tbProject
				(ProjectCode, UserId, SubjectCode, ProjectTitle, ContactName, ObjectCode, ProjectStatusCode, ActionById, ProjectNotes, Quantity, CashCode, TaxCode, UnitCharge, AddressCodeFrom, AddressCodeTo, SecondReference, Printed)
		SELECT @ProjectCode,@UserId, 'STOBOX', ObjectDescription, 'Francis Brown', ObjectCode, 1,@UserId, ObjectDescription, @Quantity, '103', 'T1', UnitCharge, 'STOBOX_001', 'STOBOX_001', N'12354/2', 0		
		FROM Object.tbObject
		WHERE ObjectCode = 'M/00/70/00';

		EXEC Project.proc_Configure @ProjectCode;
		EXEC Project.proc_AssignToParent @ProjectCode, @ParentProjectCode;

	
		UPDATE Project.tbProject
		SET SubjectCode = 'PACSER', ContactName = 'John OGroats', AddressCodeFrom = 'PACSER_001', AddressCodeTo = 'PACSER_001'
		WHERE ObjectCode = 'BOX/41';

		UPDATE Project.tbProject
		SET SubjectCode = 'TFCSPE', ContactName = 'Gary Granger', AddressCodeFrom = 'TFCSPE_001', AddressCodeTo = 'TFCSPE_001'
		WHERE ObjectCode = 'INSERT/09';

		UPDATE Project.tbProject
		SET SubjectCode = 'PALSUP', ContactName = 'Allan Rain', AddressCodeFrom = 'PALSUP_001', AddressCodeTo = 'PALSUP_001', CashCode = NULL, UnitCharge = 0
		WHERE ObjectCode = 'PALLET/01';

		UPDATE Project.tbProject
		SET SubjectCode = 'PLAPRO', ContactName = 'Kim Burnell', AddressCodeFrom = 'PLAPRO_001', AddressCodeTo = 'PLAPRO_001'
		WHERE ObjectCode = 'PC/999';
		
		UPDATE Project.tbProject
		SET SubjectCode = 'HAULOG', ContactName = 'John Iron',  AddressCodeFrom = 'HOME_001', AddressCodeTo = 'STOBOX_001', Quantity = 1, UnitCharge = 25.0 * @ExchangeRate, TotalCharge = 25.0 * @ExchangeRate
		WHERE ObjectCode = 'DELIVERY';

		UPDATE Project.tbProject
		SET SubjectCode = (SELECT SubjectCode FROM App.tbOptions), ContactName = (SELECT UserName FROM Usr.vwCredentials)
		WHERE (CashCode IS NULL) AND (SubjectCode <> 'PALSUP');

		EXEC Project.proc_Schedule @ProjectCode;

		--forward orders
		DECLARE @Month SMALLINT = 1;

		WHILE (@Month < 5)
		BEGIN

			EXEC Project.proc_Copy @FromProjectCode = @ProjectCode, 
					@ToProjectCode = @ToProjectCode OUTPUT;

			UPDATE Project.tbProject
			SET ActionOn = DATEADD(MONTH, @Month, ActionOn)
			WHERE ProjectCode = @ToProjectCode;

			EXEC Project.proc_Schedule @ToProjectCode;

			SET @ProjectCode = @ToProjectCode;
			SET @Month += 1;
		END

		--order the pallets
		EXEC Project.proc_NextCode 'PALLET/01', @ProjectCode OUTPUT
		
		INSERT INTO Project.tbProject
				(ProjectCode, UserId, SubjectCode, ProjectTitle, ObjectCode, ProjectStatusCode, ActionById)
		VALUES        (@ProjectCode,@UserId, 'PALSUP', N'PALLETS', 'PALLET/01', 1, @UserId);

		WITH demand AS
		(
			SELECT ObjectCode, ROUND(SUM(Quantity), -1) AS Quantity, MIN(ActionOn) AS ActionOn
			FROM Project.tbProject project 
			WHERE ObjectCode = 'PALLET/01' AND ProjectCode <> @ProjectCode
			GROUP BY ObjectCode
		)
		UPDATE Project
		SET 
			ProjectNotes = Object.ObjectDescription, 
			Quantity = demand.Quantity,
			ActionOn = demand.ActionOn,
			CashCode = Object.CashCode, 
			TaxCode = Subject.TaxCode, 
			UnitCharge = Object.UnitCharge, 
			AddressCodeFrom = Subject.AddressCode, 
			AddressCodeTo = Subject.AddressCode, 
			Printed = Object.Printed
		FROM Project.tbProject Project
			JOIN Subject.tbSubject Subject ON Project.SubjectCode = Subject.SubjectCode
			JOIN Object.tbObject Object ON Project.ObjectCode = Object.ObjectCode
			JOIN demand ON Project.ObjectCode = demand.ObjectCode
		WHERE ProjectCode = @ProjectCode;

		EXEC Project.proc_Configure @ProjectCode;
		EXEC Project.proc_AssignToParent @ProjectCode, @ParentProjectCode;

		UPDATE Project.tbFlow
		SET StepNumber = 0
		WHERE (ChildProjectCode = @ProjectCode);

		--identify ordered boms
		WITH unique_id AS
		(
			SELECT ProjectCode, ObjectCode, ROW_NUMBER() OVER (PARTITION BY ObjectCode ORDER BY ActionOn) AS RowNo
			FROM Project.tbProject
		)
		UPDATE Project
		SET 
			ProjectTitle = CONCAT(ProjectTitle, ' ', unique_id.RowNo)
		FROM Project.tbProject Project
			JOIN unique_id ON Project.ProjectCode = unique_id.ProjectCode
		WHERE Project.ObjectCode = 'M/00/70/00';

		--borrow some money
		UPDATE Cash.tbCategory
		SET IsEnabled = 1
		WHERE CategoryCode = 'IV';

		UPDATE Cash.tbCode
		SET IsEnabled = 1
		WHERE CashCode = '214';

		DECLARE @PaymentCode NVARCHAR(20), @AccountCode NVARCHAR(10);
		EXEC Cash.proc_CurrentAccount @AccountCode OUTPUT;

		IF @ExchangeRate = 1
		BEGIN
			
			EXEC Cash.proc_NextPaymentCode @PaymentCode OUTPUT
			INSERT INTO Cash.tbPayment (AccountCode, PaymentCode, UserId, SubjectCode, CashCode, TaxCode, PaidInValue)
			SELECT TOP 1
				@AccountCode AccountCode,
				@PaymentCode AS PaymentCode, 
				@UserId AS UserId,
				SubjectCode,
				'214' AS CashCode,
				'T0' AS TaxCode,
				(SELECT ABS(ROUND(MIN(Balance), -3)) + 1000	FROM Cash.vwStatement) AS PaidInValue
			FROM Subject.tbAccount 
			WHERE AccountCode = @AccountCode

			EXEC Cash.proc_PaymentPost;
		END

		-- ***************************************************************************
		IF @InvoiceOrders = 0
			GOTO CommitTran;
		-- ***************************************************************************
		
		DECLARE 
			@InvoiceTypeCode SMALLINT,
			@InvoiceNumber NVARCHAR(10),
			@InvoicedOn DATETIME = CAST(CURRENT_TIMESTAMP AS DATE);

		DECLARE cur_Projects CURSOR LOCAL FOR
			WITH parent AS
			(
				SELECT DISTINCT FIRST_VALUE(ProjectCode) OVER (PARTITION BY ObjectCode ORDER BY ActionOn) AS ProjectCode
				FROM Project.tbProject Project
				WHERE Project.ObjectCode = 'M/00/70/00'
			), candidates AS
			(
				SELECT child.ParentProjectCode, child.ChildProjectCode
					, 1 AS Depth
				FROM Project.tbFlow child 
					JOIN parent ON child.ParentProjectCode = parent.ProjectCode
					JOIN Project.tbProject Project ON child.ChildProjectCode = Project.ProjectCode

				UNION ALL

				SELECT child.ParentProjectCode, child.ChildProjectCode
					, parent.Depth + 1 AS Depth
				FROM Project.tbFlow child 
					JOIN candidates parent ON child.ParentProjectCode = parent.ChildProjectCode
					JOIN Project.tbProject Project ON child.ChildProjectCode = Project.ProjectCode
			), selected AS
			(
				SELECT ProjectCode
				FROM parent

				UNION

				SELECT ChildProjectCode AS ProjectCode
				FROM candidates

				UNION

				SELECT ProjectCode
				FROM Project.tbProject 
				WHERE ObjectCode = 'PALLET/01'
			)
			SELECT Project.ProjectCode, CASE category.CashPolarityCode WHEN 0 THEN 2 ELSE 0 END AS InvoiceTypeCode
			FROM selected
				JOIN Project.tbProject Project ON selected.ProjectCode = Project.ProjectCode
				JOIN Cash.tbCode cash_code ON Project.CashCode = cash_code.CashCode
				JOIN Cash.tbCategory category ON cash_code.CategoryCode = category.CategoryCode;

		OPEN cur_Projects
		FETCH NEXT FROM cur_Projects INTO @ProjectCode, @InvoiceTypeCode;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			IF @PayInvoices = 0
			BEGIN
				EXEC Invoice.proc_Raise @ProjectCode = @ProjectCode, @InvoiceTypeCode = @InvoiceTypeCode, @InvoicedOn = @InvoicedOn, @InvoiceNumber = @InvoiceNumber OUTPUT
				EXEC Invoice.proc_Accept @InvoiceNumber;
			END
			ELSE
				EXEC Project.proc_Pay @ProjectCode = @ProjectCode, @Post = 1, @PaymentCode = @PaymentCode OUTPUT;

			FETCH NEXT FROM cur_Projects INTO @ProjectCode, @InvoiceTypeCode;
		END

		CLOSE cur_Projects;
		DEALLOCATE cur_Projects;

CommitTran:
			
		COMMIT TRAN;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_ChangeTxAdd]...';


go
CREATE   PROCEDURE Cash.proc_ChangeTxAdd(@PaymentAddress nvarchar(42), @TxId nvarchar(64), @TxStatusCode smallint, @MoneyIn decimal(18, 5), @Confirmations int, @TxMessage nvarchar(50) = null)
AS
	SET XACT_ABORT, NOCOUNT ON;
	BEGIN TRY

		BEGIN TRAN

		DECLARE @PaymentCode nvarchar(20);

		IF EXISTS (SELECT * FROM Cash.tbTx WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress)
		BEGIN
			UPDATE Cash.tbTx
			SET 
				MoneyIn = @MoneyIn, 
				TxStatusCode = CASE WHEN @TxStatusCode = 2 THEN @TxStatusCode ELSE TxStatusCode END,
				Confirmations = @Confirmations
			WHERE TxId = @TxId AND PaymentAddress = @PaymentAddress;
		END
		ELSE
		BEGIN
			SELECT @TxStatusCode = CASE change.ChangeTypeCode WHEN 1 THEN 1 ELSE @TxStatusCode END
			FROM Cash.tbChange change
				JOIN Cash.tbTx tx ON change.PaymentAddress = tx.PaymentAddress
			WHERE tx.PaymentAddress = @PaymentAddress AND tx.TxId = @TxId;

			INSERT INTO Cash.tbTx (TxId, PaymentAddress, TxStatusCode, MoneyIn, Confirmations, TxMessage)
			VALUES (@TxId, @PaymentAddress, @TxStatusCode, @MoneyIn, @Confirmations, @TxMessage);
		END

		EXEC Cash.proc_TxInvoice @PaymentAddress, @TxId;

		COMMIT TRAN

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
PRINT N'Creating Procedure [Cash].[proc_ChangeAssign]...';


go
CREATE   PROCEDURE Cash.proc_ChangeAssign
(
	@AccountCode nvarchar(10), 
	@KeyName nvarchar(50), 
	@PaymentAddress nvarchar(42), 
	@InvoiceNumber nvarchar(20),
	@Note nvarchar(256) = NULL
)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		BEGIN TRAN

		UPDATE Cash.tbChange
		SET Note = @Note
		WHERE PaymentAddress = @PaymentAddress;

		IF EXISTS (SELECT * FROM Invoice.tbInvoice inv 
						JOIN Invoice.tbType typ ON inv.InvoiceTypeCode = typ.InvoiceTypeCode  
						WHERE typ.CashPolarityCode = 1 AND InvoiceNumber = @InvoiceNumber AND inv.InvoiceStatusCode BETWEEN 1 AND 2)
		BEGIN
			IF EXISTS (SELECT * FROM Cash.tbChangeReference WHERE InvoiceNumber = @InvoiceNumber)
				DELETE FROM Cash.tbChangeReference WHERE InvoiceNumber = @InvoiceNumber;

			INSERT INTO Cash.tbChangeReference (PaymentAddress, InvoiceNumber)
			VALUES (@PaymentAddress, @InvoiceNumber);

			DECLARE @TxId nvarchar(64);
			DECLARE txIds CURSOR LOCAL FOR
				SELECT TxId FROM Cash.tbTx tx
				WHERE TxStatusCode = 0 AND tx.PaymentAddress = @PaymentAddress;

			OPEN txIds;
			FETCH NEXT FROM txIds INTO @TxId

			WHILE @@FETCH_STATUS = 0
			BEGIN
				EXEC Cash.proc_TxInvoice @PaymentAddress, @TxId;
				FETCH NEXT FROM txIds INTO @TxId
			END

			CLOSE txIds;
			DEALLOCATE txIds;

		END

		COMMIT TRAN

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go

PRINT N'>>>manually adding to tbNodeDb4 output...'
PRINT N'Creating Procedure [Usr].[proc_AddUser]...';

go
CREATE PROCEDURE [Usr].[proc_AddUser]
(
	@UserName NVARCHAR(25), 
	@FullName NVARCHAR(100),
	@HomeAddress NVARCHAR(MAX),
	@EmailAddress NVARCHAR(255),
	@MobileNumber NVARCHAR(50),
	@CalendarCode NVARCHAR(10),
	@IsAdministrator BIT = 0
)
AS

	DECLARE @SQL NVARCHAR(MAX);
	DECLARE @ObjectName NVARCHAR(100);

	SET @SQL = CONCAT('CREATE USER [', @UserName, '] FOR LOGIN [', @UserName, '] WITH DEFAULT_SCHEMA=[dbo];');
	EXECUTE sys.sp_executesql @stmt = @SQL;

	SET @SQL = CONCAT('ALTER ROLE [db_datareader] ADD MEMBER [', @UserName, '];');
	EXECUTE sys.sp_executesql @stmt = @SQL;
	SET @SQL = CONCAT('ALTER ROLE [db_datawriter] ADD MEMBER [', @UserName, '];');
	EXECUTE sys.sp_executesql @stmt = @SQL;

	--Register with client
	DECLARE @UserId NVARCHAR(10) = CONCAT(LEFT(@FullName, 1), SUBSTRING(@FullName, CHARINDEX(' ', @FullName) + 1, 1)); 

	INSERT INTO Usr.tbUser (UserId, UserName, LogonName, IsAdministrator, IsEnabled, CalendarCode, EmailAddress, MobileNumber, [Address])
	VALUES (@UserId, @FullName, @UserName, @IsAdministrator, 1, @CalendarCode, @EmailAddress, @MobileNumber, @HomeAddress)

	INSERT INTO Usr.tbMenuUser (UserId, MenuId)
	SELECT @UserId AS UserId, (SELECT MenuId FROM Usr.tbMenu) AS MenuId;

	--protect system tables
	DECLARE tbs CURSOR FOR
		WITH tbnames AS
		(
			SELECT SCHEMA_NAME(schema_id) AS SchemaName, CONCAT(SCHEMA_NAME(schema_id), '.', [name]) AS TableName
			FROM sys.tables
			WHERE type = 'U' AND SCHEMA_NAME(schema_id) <> 'dbo' 
		)
		SELECT TableName
		FROM tbnames
		WHERE (TableName like '%Status%' or TableName like '%Type%')
			OR (TableName = 'App.tbDocClass')
			OR (TableName = 'App.tbEventLog')
			OR (TableName = 'App.tbInstall')
			OR (TableName = 'App.tbRecurrence')
			OR (TableName = 'App.tbRounding')
			OR (TableName = 'App.tbText')
			OR (TableName = 'Cash.tbMode')
			OR (TableName = 'App.tbEth');

		OPEN tbs
		FETCH NEXT FROM tbs INTO @ObjectName
		WHILE @@FETCH_STATUS = 0
		BEGIN
			SET @SQL = CONCAT('DENY DELETE ON ', DB_NAME(), '.', @ObjectName, ' TO [', @UserName, ']')
			EXECUTE sys.sp_executesql @stmt = @SQL
			SET @SQL = CONCAT('DENY INSERT ON ', DB_NAME(), '.', @ObjectName, ' TO [', @UserName, ']')
			EXECUTE sys.sp_executesql @stmt = @SQL
			SET @SQL = CONCAT('DENY UPDATE ON ', DB_NAME(), '.', @ObjectName, ' TO [', @UserName, ']')
			EXECUTE sys.sp_executesql @stmt = @SQL
			SET @SQL = CONCAT('GRANT SELECT ON ', DB_NAME(), '.', @ObjectName, ' TO [', @UserName, ']')
			EXECUTE sys.sp_executesql @stmt = @SQL
			 
			FETCH NEXT FROM tbs INTO @ObjectName
		END
		CLOSE tbs
		DEALLOCATE tbs

	--Deny non-administrators insert, delete and update permission on Usr schema tables
	IF @IsAdministrator = 0
	BEGIN
		DECLARE tbs CURSOR FOR
			WITH tbnames AS
			(
				SELECT SCHEMA_NAME(schema_id) AS SchemaName, CONCAT(SCHEMA_NAME(schema_id), '.', [name]) AS TableName
				FROM sys.tables
				WHERE type = 'U' AND SCHEMA_NAME(schema_id) <> 'dbo' 
			)
			SELECT TableName
			FROM tbnames
			WHERE (SchemaName = 'Usr');

			OPEN tbs
			FETCH NEXT FROM tbs INTO @ObjectName
			WHILE @@FETCH_STATUS = 0
			BEGIN
				SET @SQL = CONCAT('DENY DELETE ON ', DB_NAME(), '.', @ObjectName, ' TO [', @UserName, ']')
				EXECUTE sys.sp_executesql @stmt = @SQL
				SET @SQL = CONCAT('DENY INSERT ON ', DB_NAME(), '.', @ObjectName, ' TO [', @UserName, ']')
				EXECUTE sys.sp_executesql @stmt = @SQL
				SET @SQL = CONCAT('DENY UPDATE ON ', DB_NAME(), '.', @ObjectName, ' TO [', @UserName, ']')
				EXECUTE sys.sp_executesql @stmt = @SQL
				SET @SQL = CONCAT('GRANT SELECT ON ', DB_NAME(), '.', @ObjectName, ' TO [', @UserName, ']')
				EXECUTE sys.sp_executesql @stmt = @SQL
			 
				FETCH NEXT FROM tbs INTO @ObjectName
			END
			CLOSE tbs
			DEALLOCATE tbs
	END

	--Assign full read/write/execute permissions
	DECLARE procs CURSOR FOR
		SELECT CONCAT(SCHEMA_NAME([schema_id]), '.', name) AS proc_name
		FROM sys.procedures;
	
		OPEN procs
		FETCH NEXT FROM procs INTO @ObjectName
		WHILE @@FETCH_STATUS = 0
		BEGIN
			SET @SQL = CONCAT('GRANT EXECUTE ON ', DB_NAME(), '.', @ObjectName, ' TO [', @UserName, '];');
			EXECUTE sys.sp_executesql @stmt = @SQL 
			FETCH NEXT FROM procs INTO @ObjectName
		END
		CLOSE procs
		DEALLOCATE procs

	DECLARE funcs CURSOR FOR
		SELECT CONCAT(SCHEMA_NAME([schema_id]), '.', name), type 
		FROM sys.objects where type IN ('TF', 'IF', 'FN');

	DECLARE @Type CHAR(2);

		OPEN funcs
		FETCH NEXT FROM funcs INTO @ObjectName, @Type
		WHILE @@FETCH_STATUS = 0
		BEGIN
			IF @Type = 'FN'
				SET @SQL = CONCAT('GRANT EXECUTE ON ', DB_NAME(), '.', @ObjectName, ' TO [', @UserName, '];');
			ELSE
				SET @SQL = CONCAT('GRANT SELECT ON ', DB_NAME(), '.', @ObjectName, ' TO [', @UserName, '];');

			EXECUTE sys.sp_executesql @stmt = @SQL 

			FETCH NEXT FROM funcs INTO @ObjectName, @Type
		END
		CLOSE funcs
		DEALLOCATE funcs
go
PRINT N'Insert Units of Charge';
go

INSERT App.tbUoc (UnitOfCharge, UocSymbol, UocName) VALUES 
	(N'AED', N'د.إ.‏', N'United Arab Emirates Dirhams')
	,(N'ALL', N'Lek', N'Albania Leke')
	,(N'AMD', N'դր.', N'Armenia Drams')
	,(N'ARS', N'$', N'Argentina Pesos')
	,(N'AUD', N'$', N'Australia Dollars')
	,(N'AZM', N'man.', N'Azerbaijan Manats')
	,(N'BGL', N'лв', N'Bulgaria')
	,(N'BHD', N'د.ب.‏', N'Bahrain Dinars')
	,(N'BND', N'$', N'Brunei Dollars')
	,(N'BOB', N'$b', N'Bolivia Bolivianos')
	,(N'BRL', N'R$ ', N'Brazil Reais')
	,(N'BTC', N'₿', N'Bitcoin')
	,(N'BYB', N'р.', N'Belarus')
	,(N'BZD', N'BZ$', N'Belize Dollars')
	,(N'CAD', N'$', N'Canada Dollars')
	,(N'CHF', N'SFr.', N'Switzerland Francs')
	,(N'CLP', N'$', N'Chile Pesos')
	,(N'CNY', N'￥', N'China Yuan Renminbi')
	,(N'COP', N'$', N'Colombia Pesos')
	,(N'CRC', N'₡', N'Costa Rica Colones')
	,(N'CZK', N'Kč', N'Czech Republic Koruny')
	,(N'DKK', N'kr', N'Denmark Kroner')
	,(N'DOP', N'RD$', N'Dominican Republic Pesos')
	,(N'DZD', N'د.ج.‏', N'Algeria Dinars')
	,(N'EEK', N'kr', N'Estonia Krooni')
	,(N'EGP', N'ج.م.‏', N'Egypt Pounds')
	,(N'EUR', N'€', N'Euro')
	,(N'GBP', N'£', N'UK Pounds')
	,(N'GEL', N'Lari', N'Georgia Lari')
	,(N'GTQ', N'Q', N'Guatemala Quetzales')
	,(N'HKD', N'HK$', N'Hong Kong Dollars')
	,(N'HNL', N'L.', N'Honduras Lempiras')
	,(N'HRK', N'kn', N'Croatia Kuna')
	,(N'HUF', N'Ft', N'Hungary Forint')
	,(N'IDR', N'Rp', N'Indonesia Rupiahs')
	,(N'ILS', N'₪', N'Israel New Shekels')
	,(N'INR', N'रु', N'India Rupees')
	,(N'IQD', N'د.ع.‏', N'Iraq Dinars')
	,(N'IRR', N'ريال', N'Iran Rials')
	,(N'ISK', N'kr.', N'Iceland Kronur')
	,(N'JMD', N'J$', N'Jamaica Dollars')
	,(N'JOD', N'د.ا.‏', N'Jordan Dinars')
	,(N'JPY', N'¥', N'Japan Yen')
	,(N'KES', N'S', N'Kenya Shillings')
	,(N'KGS', N'сом', N'Kyrgyzstan Soms')
	,(N'KRW', N'₩', N'South Korea Won')
	,(N'KWD', N'د.ك.‏', N'Kuwait Dinars')
	,(N'KZT', N'Т', N'Kazakhstan Tenge')
	,(N'LBP', N'ل.ل.‏', N'Lebanon Pounds')
	,(N'LTL', N'Lt', N'Lithuania Litai')
	,(N'LVL', N'Ls', N'Latvia Lati')
	,(N'LYD', N'د.ل.‏', N'Libya Dinars')
	,(N'MAD', N'د.م.‏', N'Morocco Dirhams')
	,(N'MKD', N'ден.', N'Macedonia Denars')
	,(N'MNT', N'₮', N'Mongolia Tugriks')
	,(N'MOP', N'P', N'Macau Patacas')
	,(N'MVR', N'ރ.', N'Maldives Rufiyaa')
	,(N'MXN', N'$', N'Mexico Pesos')
	,(N'MYR', N'R', N'Malaysia Ringgits')
	,(N'NIO', N'C$', N'Nicaragua Cordobas')
	,(N'NOK', N'kr', N'Norway Kroner')
	,(N'NZD', N'$', N'New Zealand Dollars')
	,(N'OMR', N'ر.ع.‏', N'Oman Rials')
	,(N'PAB', N'B/.', N'Panama Balboas')
	,(N'PEN', N'S/.', N'Peru Nuevos Soles')
	,(N'PHP', N'Php', N'Philippines Pesos')
	,(N'PKR', N'Rs', N'Pakistan Rupees')
	,(N'PLN', N'zł', N'Poland Zlotych')
	,(N'PYG', N'Gs', N'Paraguay Guarani')
	,(N'QAR', N'ر.ق.‏', N'Qatar Riyals')
	,(N'ROL', N'lei', N'Romania Lei')
	,(N'RUR', N'р.', N'Russia')
	,(N'SAR', N'ر.س.‏', N'Saudi Arabia Riyals')
	,(N'SEK', N'kr', N'Sweden Kronor')
	,(N'SGD', N'$', N'Singapore Dollars')
	,(N'SIT', N'SIT', N'Slovenia Tolars')
	,(N'SKK', N'Sk', N'Slovakia Koruny')
	,(N'SYP', N'ل.س.‏', N'Syria Pounds')
	,(N'THB', N'฿', N'Thailand Baht')
	,(N'TND', N'د.ت.‏', N'Tunisia Dinars')
	,(N'TRL', N'TL', N'Turkey Liras')
	,(N'TTD', N'TT$', N'Trinidad and Tobago Dollars')
	,(N'TWD', N'NT$', N'Taiwan New Dollars')
	,(N'UAH', N'грн.', N'Ukraine Hryvnia')
	,(N'USD', N'$', N'US Dollars')
	,(N'UYU', N'$U', N'Uruguay Pesos')
	,(N'UZS', N'su''m', N'Uzbekistan Sums')
	,(N'VEB', N'Bs', N'Venezuela Bolivares')
	,(N'VND', N'₫', N'Vietnam Dong')
	,(N'YER', N'ر.ي.‏', N'Yemen Rials')
	,(N'YUN', N'Din.', N'Serbia')
	,(N'ZAR', N'R', N'South Africa Rand')
	,(N'ZWD', N'Z$', N'Zimbabwe Dollar');
go
PRINT N'Insert Event Types';
go

INSERT INTO App.tbEventType (EventTypeCode, EventType)
VALUES (0, 'Error')
, (1, 'Warning')
, (2, 'Information')
, (3, 'Price Change')
, (4, 'Reschedule')
, (5, 'Delivered')
, (6, 'Status Change')
, (7, 'Payment')
, (8, 'Pay Address');
go

PRINT N'Register Install.';
go

INSERT INTO App.tbInstall (SQLDataVersion, SQLRelease) 
VALUES (4.1, 1)
go

PRINT N'Update complete.';
go
