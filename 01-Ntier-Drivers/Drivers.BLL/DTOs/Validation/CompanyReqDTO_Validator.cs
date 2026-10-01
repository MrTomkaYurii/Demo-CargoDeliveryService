using Drivers.BLL.DTOs.Requests;
using FluentValidation;

namespace Drivers.BLL.DTOs.Validation;

/// <summary>
/// ПАТЕРН ВАЛІДАЦІЇ (FluentValidation Pattern):
/// Окремий клас-валідатор, що реалізує правила валідації для CompanyReqDTO.
/// 
/// 💡 Як це взаємодіє з ASP.NET Core:
/// 1. При отриманні HTTP-запиту ASP.NET Core автоматично запускає зареєстрований валідатор.
/// 2. Якщо будь-яке правило порушено (наприклад, порожня назва), FluentValidation додає 
///    помилку безпосередньо у вбудований ModelState контролера: ModelState.AddModelError("CompanyName", "...").
/// 3. Завдяки атрибуту [ApiController], якщо ModelState.IsValid == false, фреймворк автоматично 
///    повертає HTTP 400 Bad Request з переліком помилок валідації.
/// </summary>
public class CompanyReqDTO_Validator : AbstractValidator<CompanyReqDTO>
{
    public CompanyReqDTO_Validator()
    {
        // Правило 1: Назва компанії обов'язкова і не довша за 100 символів
        RuleFor(c => c.CompanyName)
            .NotEmpty().WithMessage("Назва компанії є обов'язковою.")
            .MaximumLength(100).WithMessage("Назва компанії не може перевищувати 100 символів.");

        // Правило 2: Умовна валідація email (перевіряється формат тільки якщо значення не пусте)
        RuleFor(c => c.Email)
            .EmailAddress().When(c => !string.IsNullOrEmpty(c.Email))
            .WithMessage("Некоректний формат електронної пошти компанії.");

        // Правило 3: Умовна валідація контактного email
        RuleFor(c => c.ContactEmail)
            .EmailAddress().When(c => !string.IsNullOrEmpty(c.ContactEmail))
            .WithMessage("Некоректний формат контактної електронної пошти.");

        // Правило 4: Обмеження довжини телефону
        RuleFor(c => c.Phone)
            .MaximumLength(20).WithMessage("Телефон не може бути довшим за 20 символів.");
    }
}
