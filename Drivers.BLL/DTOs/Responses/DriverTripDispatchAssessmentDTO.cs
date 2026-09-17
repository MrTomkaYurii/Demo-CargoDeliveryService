namespace Drivers.BLL.DTOs.Responses;

/// <summary>
/// Результат комплексної бізнес-оцінки допуску водія до рейсу.
/// Відображає прийняте системою бізнес-рішення, розрахунки бюджету та перелік попереджень чи заборон.
/// </summary>
public class DriverTripDispatchAssessmentDTO
{
    public int DriverId { get; set; }
    public string DriverFullName { get; set; } = string.Empty;
    public int TruckId { get; set; }
    public string TruckInfo { get; set; } = string.Empty;

    /// <summary>
    /// Статус рішення: "Approved" (Схвалено), "ConditionalApproval" (Потрібне узгодження), "Rejected" (Відхилено)
    /// </summary>
    public string DecisionStatus { get; set; } = "Rejected";

    /// <summary>
    /// Чи дозволено фактичний виїзд у рейс
    /// </summary>
    public bool IsDispatchAllowed { get; set; }

    /// <summary>
    /// Підсумковий текстовий вердикт системи логістики
    /// </summary>
    public string DecisionSummary { get; set; } = string.Empty;

    // --- Фінансові розрахунки бізнес-логіки ---
    public decimal BaseDriverPayment { get; set; }
    public decimal ExperienceBonus { get; set; }
    public decimal CargoRiskBonus { get; set; }
    public decimal PerDiemAllowance { get; set; }
    public decimal TotalDriverCompensation { get; set; }
    public decimal EstimatedFuelExpense { get; set; }
    public decimal EmergencyReserveFund { get; set; }
    public decimal TotalTripBudget { get; set; }

    /// <summary>
    /// Ідентифікатор створеного планового запису витрат у разі успішного затвердження
    /// </summary>
    public int? CreatedExpensePlanId { get; set; }

    /// <summary>
    /// Список критичних порушень бізнес-правил (блокують рейс)
    /// </summary>
    public List<string> CriticalViolations { get; set; } = new();

    /// <summary>
    /// Список некритичних зауважень та рекомендацій для диспетчера
    /// </summary>
    public List<string> WarningsAndRecommendations { get; set; } = new();
}
