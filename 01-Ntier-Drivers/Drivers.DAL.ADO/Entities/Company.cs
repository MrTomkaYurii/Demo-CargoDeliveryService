namespace Drivers.DAL.ADO.Entities;

/// <summary>
/// Сутність компанії (ADO.NET / Dapper).
/// Відповідає структурі таблиці [dbo].[Companies].
/// </summary>
public class Company
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public List<Driver> Drivers { get; set; } = new();
}
