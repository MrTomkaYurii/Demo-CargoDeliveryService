using FluentValidation;

namespace Drivers.BLL.DTOs.Validation;

/// <summary>
/// Валідатор параметрів запиту пагінації та фільтрації водіїв
/// </summary>
public class DriverParametersValidator : AbstractValidator<Drivers.DAL.EF.Entities.HelpModels.DriverParameters>
{
    public DriverParametersValidator()
    {
        RuleFor(p => p.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Номер сторінки повинен бути не менше 1.");

        RuleFor(p => p.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("Розмір сторінки має бути в межах від 1 до 50.");

        RuleFor(p => p)
            .Must(p => p.MaxYearOfBirth >= p.MinYearOfBirth)
            .WithMessage("Максимальний рік народження не може бути меншим за мінімальний рік народження.")
            .When(p => p.MinYearOfBirth > 0 && p.MaxYearOfBirth > 0);
    }
}
