using Drivers.BLL.DTOs.Requests;
using FluentValidation;

namespace Drivers.BLL.DTOs.Validation;

/// <summary>
/// Валідатор запиту для витрат
/// </summary>
public class ExpenseReqDTO_Validator : AbstractValidator<ExpenseReqDTO>
{
    public ExpenseReqDTO_Validator()
    {
        RuleFor(e => e.Category)
            .NotEmpty().WithMessage("Категорія витрат є обов'язковою.")
            .MaximumLength(50).WithMessage("Категорія витрат не може перевищувати 50 символів.");

        RuleFor(e => e.DriverPayment)
            .GreaterThanOrEqualTo(0).WithMessage("Виплата водієві не може бути від'ємною.");

        RuleFor(e => e.FuelCost)
            .GreaterThanOrEqualTo(0).WithMessage("Витрати на пальне не можуть бути від'ємними.");

        RuleFor(e => e.MaintenanceCost)
            .GreaterThanOrEqualTo(0).WithMessage("Витрати на обслуговування не можуть бути від'ємними.");

        RuleFor(e => e.Date)
            .NotEmpty().WithMessage("Дата витрати є обов'язковою.")
            .Must(d => !d.Equals(default)).WithMessage("Дата витрати має бути коректною датою.");

        RuleFor(e => e.DriverId)
            .GreaterThan(0).WithMessage("Ідентифікатор водія має бути більшим за 0.");

        RuleFor(e => e.TruckId)
            .GreaterThan(0).WithMessage("Ідентифікатор вантажівки має бути більшим за 0.");
    }
}
