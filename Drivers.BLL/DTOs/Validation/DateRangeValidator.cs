using FluentValidation;

namespace Drivers.BLL.DTOs.Validation;

/// <summary>
/// Допоміжна модель для перевірки коректності діапазону дат
/// </summary>
public record DateRange(DateTime? FromDate, DateTime? ToDate);

/// <summary>
/// Валідатор діапазону дат для фінансових звітів
/// </summary>
public class DateRangeValidator : AbstractValidator<DateRange>
{
    public DateRangeValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.FromDate.Value <= x.ToDate.Value)
            .WithMessage("Початкова дата (FromDate) не може бути пізнішою за кінцеву дату (ToDate).");
    }
}
