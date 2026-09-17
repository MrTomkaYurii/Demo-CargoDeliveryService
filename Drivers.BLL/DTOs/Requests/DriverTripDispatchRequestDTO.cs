namespace Drivers.BLL.DTOs.Requests;

/// <summary>
/// Модель запиту диспетчера на призначення водія на запланований вантажний рейс.
/// Містить вхідні параметри маршруту та вимоги до виконання бізнес-перевірки.
/// </summary>
public class DriverTripDispatchRequestDTO
{
    /// <summary>
    /// Ідентифікатор призначуваного водія
    /// </summary>
    public int DriverId { get; set; }

    /// <summary>
    /// Ідентифікатор обраної вантажівки
    /// </summary>
    public int TruckId { get; set; }

    /// <summary>
    /// Тип рейсу або вантажу: "Стандартний регіональний", "Міжнародний", "Небезпечний вантаж (ADR)", "Швидкопсувний (рефрижератор)"
    /// </summary>
    public string CargoType { get; set; } = "Стандартний регіональний";

    /// <summary>
    /// Загальна планова дистанція рейсу в кілометрах (туди й назад)
    /// </summary>
    public decimal DistanceKm { get; set; }

    /// <summary>
    /// Орієнтовна тривалість рейсу в днях
    /// </summary>
    public int EstimatedDurationDays { get; set; }

    /// <summary>
    /// Базовий тариф компанії за 1 км для даного типу транспорту
    /// </summary>
    public decimal BaseRatePerKm { get; set; } = 15.0m;
}
