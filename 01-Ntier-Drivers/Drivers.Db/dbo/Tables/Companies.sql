CREATE TABLE [dbo].[Companies] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [CompanyName]   NVARCHAR (100) NOT NULL,
    [Address]       NVARCHAR (100) NULL,
    [Phone]         NVARCHAR (20)  NULL,
    [Email]         NVARCHAR (50)  NULL,
    [ContactPerson] NVARCHAR (100) NULL,
    [ContactPhone]  NVARCHAR (20)  NULL,
    [ContactEmail]  NVARCHAR (50)  NULL,
    [CreatedAt]     DATETIME       DEFAULT (getdate()) NULL,
    [UpdatedAt]     DATETIME       NULL,
    [DeletedAt]     DATETIME       NULL,
    CONSTRAINT [PK_Companies] PRIMARY KEY CLUSTERED ([Id] ASC)
);

