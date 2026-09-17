namespace Drivers.BLL.DTOs.Responses;

/// <summary>
/// DTO відповіді про топ-водія за витратами
/// </summary>
public class TopDriverExpenseResponseDTO
{
    public int DriverId { get; set; }
    public string FullName { get; set; } = default!;
    public string? CompanyName { get; set; }
    public decimal TotalExpenses { get; set; }
    public int ExpensesCount { get; set; }
    public DateTime? LastExpenseDate { get; set; }
}
