CREATE TABLE [dbo].[Expenses] (
    [Id]              INT             IDENTITY (1, 1) NOT NULL,
    [DriverPayment]   DECIMAL (10, 2) NOT NULL,
    [FuelCost]        DECIMAL (10, 2) NOT NULL,
    [MaintenanceCost] DECIMAL (10, 2) NOT NULL,
    [Category]        VARCHAR (255)   NOT NULL,
    [Date]            DATE            NOT NULL,
    [Note]            TEXT            NULL,
    [CreatedAt]       DATETIME        DEFAULT (getdate()) NULL,
    [UpdatedAt]       DATETIME        NULL,
    [DeletedAt]       DATETIME        NULL,
    [DriverID]        INT             NOT NULL,
    [TruckID]         INT             NOT NULL,
    CONSTRAINT [PK_Expenses] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Expenses_Drivers_DriverID] FOREIGN KEY ([DriverID]) REFERENCES [dbo].[Drivers] ([Id]),
    CONSTRAINT [FK_Expenses_Trucks_TruckID] FOREIGN KEY ([TruckID]) REFERENCES [dbo].[Trucks] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Expenses_DriverID]
    ON [dbo].[Expenses]([DriverID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Expenses_TruckID]
    ON [dbo].[Expenses]([TruckID] ASC);

