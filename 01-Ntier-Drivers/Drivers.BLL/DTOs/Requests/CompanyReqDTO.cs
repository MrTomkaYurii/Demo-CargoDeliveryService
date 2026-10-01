namespace Drivers.BLL.DTOs.Requests;

/// <summary>
/// DTO запиту для створення або оновлення компанії.
/// 
/// ⚖️ АРХІТЕКТУРНЕ ПОРІВНЯННЯ: Data Annotations vs FluentValidation:
/// 
/// 1. Підхід через Data Annotations (Вбудований у .NET):
///    - Як це виглядало б тут:
///        [Required(ErrorMessage = "Назва обов'язкова")]
///        [StringLength(100, ErrorMessage = "Не більше 100 символів")]
///        public string CompanyName { get; set; }
/// 
///        [EmailAddress(ErrorMessage = "Некоректна пошта")]
///        public string? Email { get; set; }
///    - Переваги: Простота, декларативність, підтримка "з коробки" у ASP.NET Core (через ModelState.IsValid).
///    - Недоліки: Змішування моделі даних з логікою перевірок, неможливість асинхронної валідації (наприклад,
///      перевірка унікальності в БД), неможливість складних крос-перевірок декількох полів, важко юніт-тестувати.
/// 
/// 2. Підхід через FluentValidation (Використовується в цьому проекті):
///    - DTO залишається чистим POCO (Plain Old C# Object) без залежностей від атрибутів.
///    - Вся бізнес-валідація винесена в окремий клас-валідатор: CompanyReqDTO_Validator.
///    - Результати автоматично інтегруються в конвеєр ASP.NET Core та мапляться у ModelState.
/// </summary>
public class CompanyReqDTO
{
    public string CompanyName { get; set; } = default!;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
}
