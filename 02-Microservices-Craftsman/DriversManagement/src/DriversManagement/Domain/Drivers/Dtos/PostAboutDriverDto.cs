namespace DriversManagement.Domain.Drivers.Dtos;

/// <summary>
/// Власна (локальна) модель поста про водія у Bounded Context DriversManagement.
/// <para>
/// Дані надходять із DriversBlogManagement через gRPC (повідомлення PostAboutDriverResponse
/// з контракту Driver.proto) і мапляться в цей DTO у <c>GetDriver.Handler</c>.
/// </para>
/// <para>
/// Чому не перевикористати DTO з DriversBlogManagement? Тому що мікросервіси не повинні
/// ділити між собою внутрішні класи: спільним є лише контракт (.proto). Так кожен сервіс
/// може змінювати власні моделі незалежно, не ламаючи сусідів.
/// </para>
/// </summary>
public sealed record PostAboutDriverDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
