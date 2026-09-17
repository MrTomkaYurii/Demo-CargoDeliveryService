using Drivers.BLL.DTOs.Requests;
using FluentValidation;

namespace Drivers.BLL.DTOs.Validation;

/// <summary>
/// Валідатор запиту для компанії
/// </summary>
public class CompanyReqDTO_Validator : AbstractValidator<CompanyReqDTO>
{
    public CompanyReqDTO_Validator()
    {
        RuleFor(c => c.CompanyName)
            .NotEmpty().WithMessage("Назва компанії є обов'язковою.")
            .MaximumLength(100).WithMessage("Назва компанії не може перевищувати 100 символів.");

        RuleFor(c => c.Email)
            .EmailAddress().When(c => !string.IsNullOrEmpty(c.Email))
            .WithMessage("Некоректний формат електронної пошти компанії.");

        RuleFor(c => c.ContactEmail)
            .EmailAddress().When(c => !string.IsNullOrEmpty(c.ContactEmail))
            .WithMessage("Некоректний формат контактної електронної пошти.");

        RuleFor(c => c.Phone)
            .MaximumLength(20).WithMessage("Телефон не може бути довшим за 20 символів.");
    }
}
