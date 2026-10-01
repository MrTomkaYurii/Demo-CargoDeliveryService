CREATE TABLE [dbo].[Inspections] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [InspectionDate] DATE           NOT NULL,
    [Description]    NVARCHAR (500) NULL,
    [Result]         BIT            NOT NULL,
    [CreatedAt]      DATETIME       DEFAULT (getdate()) NULL,
    [UpdatedAt]      DATETIME       NULL,
    [DeletedAt]      DATETIME       NULL,
    [TruckID]        INT            NULL,
    CONSTRAINT [PK_Inspections] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Inspections_Trucks_TruckID] FOREIGN KEY ([TruckID]) REFERENCES [dbo].[Trucks] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Inspections_TruckID]
    ON [dbo].[Inspections]([TruckID] ASC);

