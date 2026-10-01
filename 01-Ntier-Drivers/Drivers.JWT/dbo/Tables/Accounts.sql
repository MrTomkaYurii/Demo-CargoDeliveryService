CREATE TABLE [dbo].[Accounts] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [Title]             NVARCHAR (MAX) NULL,
    [FirstName]         NVARCHAR (MAX) NULL,
    [LastName]          NVARCHAR (MAX) NULL,
    [Email]             NVARCHAR (MAX) NULL,
    [PasswordHash]      NVARCHAR (MAX) NULL,
    [AcceptTerms]       BIT            NOT NULL,
    [Role]              INT            NOT NULL,
    [VerificationToken] NVARCHAR (MAX) NULL,
    [Verified]          DATETIME2 (7)  NULL,
    [ResetToken]        NVARCHAR (MAX) NULL,
    [ResetTokenExpires] DATETIME2 (7)  NULL,
    [PasswordReset]     DATETIME2 (7)  NULL,
    [Created]           DATETIME2 (7)  NOT NULL,
    [Updated]           DATETIME2 (7)  NULL,
    CONSTRAINT [PK_Accounts] PRIMARY KEY CLUSTERED ([Id] ASC)
);

