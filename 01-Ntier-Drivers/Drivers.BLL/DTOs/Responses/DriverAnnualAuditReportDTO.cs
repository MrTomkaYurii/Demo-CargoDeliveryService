namespace Drivers.BLL.DTOs.Responses;

/// <summary>
/// Підсумковий звіт щорічного або періодичного аудиту ефективності та надійності водія.
/// Результат роботи інтегрального оціночного алгоритму шару бізнес-логіки.
/// </summary>
public class DriverAnnualAuditReportDTO
{
    public int DriverId { get; set; }
    public string DriverFullName { get; set; } = string.Empty;
    public int AuditYear { get; set; }
    public DateTime AuditGeneratedAt { get; set; } = DateTime.UtcNow;

    // --- Агреговані показники діяльності водія за рік ---
    public int TotalTripsCompleted { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal TotalFuelCostIncurred { get; set; }
    public decimal TotalVehicleMaintenanceIncurred { get; set; }
    public decimal GrandTotalOperationalExpenses { get; set; }

    // --- Розраховані індекси KPI (від 0 до 100 балів) ---
    /// <summary>
    /// Оцінка паливної дисципліни та відсутності перевитрат
    /// </summary>
    public double FuelEfficiencyScore { get; set; }

    /// <summary>
    /// Оцінка дбайливого ставлення до техніки (співвідношення планового ТО до аварійних ремонтів)
    /// </summary>
    public double VehicleCareScore { get; set; }

    /// <summary>
    /// Оцінка дисципліни, проходження техоглядів та дотримання термінів дії документів
    /// </summary>
    public double SafetyComplianceScore { get; set; }

    /// <summary>
    /// Інтегральний комбінований бал ефективності (зважена сума окремих KPI)
    /// </summary>
    public double OverallPerformanceRating { get; set; }

    // --- Кваліфікаційний висновок та бонуси ---
    /// <summary>
    /// Присвоєний грейд: "Grade A (Platinum)", "Grade B (Gold)", "Grade C (Silver)", "UnderReview (Потребує переатестації)"
    /// </summary>
    public string AssignedDriverGrade { get; set; } = string.Empty;

    /// <summary>
    /// Розрахована річна премія за результатами роботи
    /// </summary>
    public decimal CalculatedAnnualBonus { get; set; }

    /// <summary>
    /// Чи створено автоматичну заявку на виплату річної премії
    /// </summary>
    public bool IsBonusRecordedInExpenses { get; set; }

    /// <summary>
    /// Офіційні рекомендації відділу безпеки та управління персоналом
    /// </summary>
    public List<string> ExecutiveRecommendations { get; set; } = new();
}
