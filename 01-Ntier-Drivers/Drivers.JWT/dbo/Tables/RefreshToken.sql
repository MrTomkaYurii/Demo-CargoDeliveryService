CREATE TABLE [dbo].[RefreshToken] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [AccountId]       INT            NOT NULL,
    [Token]           NVARCHAR (MAX) NOT NULL,
    [Expires]         DATETIME2 (7)  NOT NULL,
    [Created]         DATETIME2 (7)  NOT NULL,
    [CreatedByIp]     NVARCHAR (MAX) NOT NULL,
    [Revoked]         DATETIME2 (7)  NULL,
    [RevokedByIp]     NVARCHAR (MAX) NOT NULL,
    [ReplacedByToken] NVARCHAR (MAX) NOT NULL,
    [ReasonRevoked]   NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_RefreshToken] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RefreshToken_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[Accounts] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_RefreshToken_AccountId]
    ON [dbo].[RefreshToken]([AccountId] ASC);

