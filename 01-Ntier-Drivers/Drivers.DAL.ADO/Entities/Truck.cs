namespace Drivers.DAL.ADO.Entities;

/// <summary>
/// Сутність вантажівки (ADO.NET / Dapper).
/// Відповідає структурі таблиці [dbo].[Trucks].
/// </summary>
public class Truck
{
    public int Id { get; set; }
    public string TruckNumber { get; set; } = default!;
    public string Model { get; set; } = default!;
    public int Year { get; set; }
    public int Capacity { get; set; }
    public string? FuelType { get; set; }
    public decimal? FuelConsumption { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? VIN { get; set; }
    public string? EngineNumber { get; set; }
    public string? ChassisNumber { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public DateTime? InsuranceExpirationDate { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public List<Expenses> Expenses { get; set; } = new();
    public List<Inspection> Inspections { get; set; } = new();
    public List<Repair> Repairs { get; set; } = new();
}
