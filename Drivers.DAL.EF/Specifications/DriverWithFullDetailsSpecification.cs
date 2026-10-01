using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Specifications;

namespace Drivers.DAL.EF.Specifications;

/// <summary>
/// Специфікація для отримання повного агрегату водія за Id:
/// завантажує фото, компанію та історію витрат з деталями вантажівки.
/// </summary>
public class DriverWithFullDetailsSpecification : BaseSpecification<EFDriver>
{
    public DriverWithFullDetailsSpecification(int driverId)
        : base(driver => driver.Id == driverId)
    {
        AddInclude(d => d.Company);
        AddInclude(d => d.Photo);
        AddInclude(d => d.Expenses);
        AddInclude("Expenses.Truck");
    }
}
