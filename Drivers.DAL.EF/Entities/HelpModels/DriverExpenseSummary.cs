namespace Drivers.DAL.EF.Entities.HelpModels;

/// <summary>
/// Агреговані фінансові показники витрат водія (Entity Framework Core)
/// </summary>
public class DriverExpenseSummary
{
    public int DriverId { get; set; }
    public int ExpenseCount { get; set; }
    public decimal TotalDriverPayment { get; set; }
    public decimal TotalFuelCost { get; set; }
    public decimal TotalMaintenanceCost { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal AverageExpense { get; set; }
    public DateTime? FirstExpenseDate { get; set; }
    public DateTime? LastExpenseDate { get; set; }
}
