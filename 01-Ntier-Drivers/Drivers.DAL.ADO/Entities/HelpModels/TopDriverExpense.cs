namespace Drivers.DAL.ADO.Entities.HelpModels;

/// <summary>
/// Інформація про топ-водія за витратами (ADO.NET / Dapper)
/// </summary>
public class TopDriverExpense
{
    public int DriverId { get; set; }
    public string FullName { get; set; } = default!;
    public string? CompanyName { get; set; }
    public decimal TotalExpenses { get; set; }
    public int ExpensesCount { get; set; }
    public DateTime? LastExpenseDate { get; set; }
}
