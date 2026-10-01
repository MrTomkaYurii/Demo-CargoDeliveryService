CREATE TABLE [dbo].[Trucks] (
    [Id]                      INT            IDENTITY (1, 1) NOT NULL,
    [TruckNumber]             NVARCHAR (20)  NOT NULL,
    [Model]                   NVARCHAR (50)  NOT NULL,
    [Year]                    INT            NOT NULL,
    [Capacity]                INT            NOT NULL,
    [FuelType]                NVARCHAR (50)  NULL,
    [FuelConsumption]         DECIMAL (4, 2) NULL,
    [RegistrationNumber]      NVARCHAR (20)  NULL,
    [VIN]                     NVARCHAR (50)  NULL,
    [EngineNumber]            NVARCHAR (50)  NULL,
    [ChassisNumber]           NVARCHAR (50)  NULL,
    [InsurancePolicyNumber]   NVARCHAR (50)  NULL,
    [InsuranceExpirationDate] DATE           NULL,
    [CreatedAt]               DATETIME       DEFAULT (getdate()) NULL,
    [UpdatedAt]               DATETIME       NULL,
    [DeletedAt]               DATETIME       NULL,
    CONSTRAINT [PK_Trucks] PRIMARY KEY CLUSTERED ([Id] ASC)
);

