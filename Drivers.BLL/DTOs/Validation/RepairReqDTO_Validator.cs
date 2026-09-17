using Drivers.BLL.DTOs.Requests;
using FluentValidation;

namespace Drivers.BLL.DTOs.Validation;

/// <summary>
/// Валідатор запиту для ремонту
/// </summary>
public class RepairReqDTO_Validator : AbstractValidator<RepairReqDTO>
{
    public RepairReqDTO_Validator()
    {
        RuleFor(r => r.RepairDate)
            .NotEmpty().WithMessage("Дата ремонту є обов'язковою.")
            .Must(d => !d.Equals(default)).WithMessage("Дата ремонту має бути коректною датою.");

        RuleFor(r => r.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("Вартість ремонту не може бути від'ємною.");

        RuleFor(r => r.TruckId)
            .GreaterThan(0).When(r => r.TruckId.HasValue)
            .WithMessage("Ідентифікатор вантажівки має бути більшим за 0.");
    }
}
