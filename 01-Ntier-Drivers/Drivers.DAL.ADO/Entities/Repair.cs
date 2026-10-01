namespace Drivers.DAL.ADO.Entities;

/// <summary>
/// Сутність ремонту (ADO.NET / Dapper).
/// Відповідає структурі таблиці [dbo].[Repairs].
/// </summary>
public class Repair
{
    public int Id { get; set; }
    public DateTime RepairDate { get; set; }
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public int? TruckID { get; set; }
    public Truck? Truck { get; set; }
}
