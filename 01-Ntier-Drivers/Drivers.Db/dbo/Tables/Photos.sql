CREATE TABLE [dbo].[Photos] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [PhotoData]   VARBINARY (MAX) NOT NULL,
    [ContentType] NVARCHAR (50)   NOT NULL,
    [FileName]    NVARCHAR (255)  NOT NULL,
    [FileSize]    INT             NOT NULL,
    [CreatedAt]   DATETIME        DEFAULT (getdate()) NULL,
    [UpdatedAt]   DATETIME        NULL,
    [DeletedAt]   DATETIME        NULL,
    CONSTRAINT [PK_Photos] PRIMARY KEY CLUSTERED ([Id] ASC)
);

