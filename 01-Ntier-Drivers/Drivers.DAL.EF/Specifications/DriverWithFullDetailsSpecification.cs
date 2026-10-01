using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Specifications;

namespace Drivers.DAL.EF.Specifications;

/// <summary>
/// Специфікація для завантаження повного графа агрегату сутності водія за ідентифікатором (Id).
/// <para>
/// <b>Бізнес-призначення:</b> Використовується для формування детальної картки водія в інтерфейсі (WPF/API),
/// де необхідно одночасно відобразити:
/// <list type="bullet">
///   <item>Дані компанії-роботодавця (<see cref="EFDriver.Company"/>)</item>
///   <item>Бінарне фото водія (<see cref="EFDriver.Photo"/>)</item>
///   <item>Історію дорожніх витрат (<see cref="EFDriver.Expenses"/>)</item>
///   <item>Вкладені дані вантажівки для кожного запису витрат (<c>Expenses.Truck</c>)</item>
/// </list>
/// </para>
/// <para>
/// <b>Технічна деталь:</b> Зв'язок першого рівня завантажується типізовано через лямбда-вираз,
/// а вкладений зв'язок другого рівня (<c>Expenses.Truck</c>) передається через рядковий шлях
/// <see cref="BaseSpecification{T}.AddInclude(string)"/>, транслюючись у <c>ThenInclude</c> еквівалент.
/// </para>
/// </summary>
public class DriverWithFullDetailsSpecification : BaseSpecification<EFDriver>
{
    /// <summary>
    /// Створює специфікацію завантаження повного профілю водія.
    /// </summary>
    /// <param name="driverId">Унікальний первинний ключ водія в базі даних</param>
    public DriverWithFullDetailsSpecification(int driverId)
        : base(driver => driver.Id == driverId)
    {
        // Зв'язки першого рівня (Direct Navigation Properties)
        AddInclude(d => d.Company);
        AddInclude(d => d.Photo);
        AddInclude(d => d.Expenses);

        // Вкладений зв'язок другого рівня (Nested Navigation: Driver -> Expenses -> Truck)
        AddInclude("Expenses.Truck");
    }
}
