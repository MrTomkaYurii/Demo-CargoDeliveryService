namespace WPFDrivers.Models;

/// <summary>
/// Режим архітектурного підключення клієнта WPF до даних
/// </summary>
public enum ArchitectureMode
{
    /// <summary>
    /// Прямий виклик бізнес-логіки та репозиторіїв через локальний DI-контейнер (In-Process / Rich Client)
    /// </summary>
    DirectBll = 0,

    /// <summary>
    /// Виклик віддаленого REST API через HTTP (Client-Server / Distributed Architecture)
    /// </summary>
    WebApi = 1
}
