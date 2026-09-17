using Drivers.BLL.DTOs.Requests;
using FluentValidation;

namespace Drivers.BLL.DTOs.Validation;

/// <summary>
/// Валідатор запиту для техогляду
/// </summary>
public class InspectionReqDTO_Validator : AbstractValidator<InspectionReqDTO>
{
    public InspectionReqDTO_Validator()
    {
        RuleFor(i => i.InspectionDate)
            .NotEmpty().WithMessage("Дата техогляду є обов'язковою.")
            .Must(d => !d.Equals(default)).WithMessage("Дата техогляду має бути коректною датою.");

        RuleFor(i => i.TruckId)
            .GreaterThan(0).When(i => i.TruckId.HasValue)
            .WithMessage("Ідентифікатор вантажівки має бути більшим за 0.");
    }
}
