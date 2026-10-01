CREATE TABLE [dbo].[Drivers] (
    [Id]                            INT            IDENTITY (1, 1) NOT NULL,
    [FirstName]                     NVARCHAR (50)  NOT NULL,
    [LastName]                      NVARCHAR (50)  NOT NULL,
    [MiddleName]                    NVARCHAR (50)  NULL,
    [Gender]                        NVARCHAR (10)  NOT NULL,
    [Birthdate]                     DATE           NOT NULL,
    [PlaceOfBirth]                  NVARCHAR (100) NULL,
    [Nationality]                   NVARCHAR (50)  NULL,
    [MaritalStatus]                 NVARCHAR (20)  NULL,
    [IdentificationType]            NVARCHAR (50)  NULL,
    [IdentificationNumber]          NVARCHAR (50)  NULL,
    [IdentificationExpirationDate]  DATE           NULL,
    [Address]                       NVARCHAR (100) NULL,
    [Phone]                         NVARCHAR (20)  NULL,
    [Email]                         NVARCHAR (50)  NULL,
    [DriverLicenseNumber]           NVARCHAR (20)  NOT NULL,
    [DriverLicenseCategory]         NVARCHAR (50)  NULL,
    [DriverLicenseIssuingDate]      DATE           NOT NULL,
    [DriverLicenseExpirationDate]   DATE           NOT NULL,
    [DriverLicenseIssuingAuthority] NVARCHAR (100) NULL,
    [EmploymentStatus]              NVARCHAR (50)  NULL,
    [EmploymentStartDate]           DATE           NULL,
    [EmploymentEndDate]             DATE           NULL,
    [CreatedAt]                     DATETIME       DEFAULT (getdate()) NULL,
    [UpdatedAt]                     DATETIME       NULL,
    [DeletedAt]                     DATETIME       NULL,
    [CompanyID]                     INT            NULL,
    [PhotoID]                       INT            NULL,
    CONSTRAINT [PK_Drivers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Drivers_Companies_CompanyID] FOREIGN KEY ([CompanyID]) REFERENCES [dbo].[Companies] ([Id]),
    CONSTRAINT [FK_Drivers_Photos_PhotoID] FOREIGN KEY ([PhotoID]) REFERENCES [dbo].[Photos] ([Id])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Drivers_CompanyID]
    ON [dbo].[Drivers]([CompanyID] ASC) WHERE ([CompanyID] IS NOT NULL);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Drivers_PhotoID]
    ON [dbo].[Drivers]([PhotoID] ASC) WHERE ([PhotoID] IS NOT NULL);

