CREATE TABLE [dbo].[Repairs] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [RepairDate]  DATE            NOT NULL,
    [Description] NVARCHAR (2000) NULL,
    [Cost]        DECIMAL (10, 2) NOT NULL,
    [CreatedAt]   DATETIME        DEFAULT (getdate()) NULL,
    [UpdatedAt]   DATETIME        NULL,
    [DeletedAt]   DATETIME        NULL,
    [TruckID]     INT             NULL,
    CONSTRAINT [PK_Repairs] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Repairs_Trucks_TruckID] FOREIGN KEY ([TruckID]) REFERENCES [dbo].[Trucks] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Repairs_TruckID]
    ON [dbo].[Repairs]([TruckID] ASC);

