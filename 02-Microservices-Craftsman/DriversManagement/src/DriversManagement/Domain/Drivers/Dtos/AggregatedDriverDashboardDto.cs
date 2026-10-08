namespace DriversManagement.Domain.Drivers.Dtos;

/// <summary>
/// Зведена DTO-модель для аналітичної панелі водія (Патерн Gateway Aggregator / BFF).
/// Об'єднує дані з трьох незалежних мікросервісів:
/// 1. DriversManagement (локальна база: профіль водія, компанія, закріплене авто)
/// 2. CargoOrderingService (зовнішній HTTP REST: активні рейси та доставки)
/// 3. DriversBlogManagement (зовнішній HTTP REST / gRPC: останні публікації та активність)
/// </summary>
public sealed class AggregatedDriverDashboardDto
{
    /// <summary>
    /// Ідентифікатор водія в системі.
    /// </summary>
    public Guid DriverId { get; set; }

    /// <summary>
    /// Повне ім'я водія (FirstName + LastName).
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Номер посвідчення водія.
    /// </summary>
    public string LicenseNumber { get; set; } = string.Empty;

    /// <summary>
    /// Назва транспортної компанії-роботодавця.
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Закріплений вантажний автомобіль (модель та держномер).
    /// </summary>
    public string? AssignedTruckInfo { get; set; }

    /// <summary>
    /// Список активних доставок із CargoOrderingService (отримано через HTTP Client).
    /// </summary>
    public List<AggregatedDeliveryDto> ActiveDeliveries { get; set; } = [];

    /// <summary>
    /// Останні публікації водія у блозі з DriversBlogManagement (отримано через HTTP Client).
    /// </summary>
    public List<AggregatedBlogPostDto> RecentBlogPosts { get; set; } = [];

    /// <summary>
    /// Прапорець часткової деградації: true, якщо один із допоміжних сервісів був недоступний
    /// і відповідь сформована за принципом Graceful Degradation.
    /// </summary>
    public bool IsPartialData { get; set; }
}

/// <summary>
/// Скорочена модель доставки для зведеного дашборду.
/// </summary>
public sealed class AggregatedDeliveryDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RouteDescription { get; set; }
}

/// <summary>
/// Скорочена модель блог-поста для зведеного дашборду.
/// </summary>
public sealed class AggregatedBlogPostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
