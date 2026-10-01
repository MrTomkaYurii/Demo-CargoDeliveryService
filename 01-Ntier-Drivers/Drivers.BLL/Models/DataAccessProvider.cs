namespace Drivers.BLL.Models;

/// <summary>
/// Провайдер доступу до даних.
/// Дозволяє перемикатися між повнофункціональним ORM (Entity Framework Core)
/// та високопродуктивним Micro-ORM (Dapper з параметризованим ADO.NET).
/// </summary>
public enum DataAccessProvider
{
    /// <summary>
    /// Entity Framework Core (LINQ, Change Tracking, Eager Loading via Include)
    /// </summary>
    EntityFramework = 0,

    /// <summary>
    /// ADO.NET + Dapper (Raw Parameterized SQL, Multiple Result Sets, Manual Mapping)
    /// </summary>
    Dapper = 1
}
