using Drivers.BLL.DTOs.Requests;
using FluentValidation;

namespace Drivers.BLL.DTOs.Validation
{
    public class MiniDriverReqDTO_Validator : AbstractValidator<MiniDriverReqDTO> 
    {
        public MiniDriverReqDTO_Validator()
        {
            RuleFor(model => model.FirstName)
                .NotNull().WithMessage("Hey man! FirstName cannot be empty.");

            RuleFor(model => model.LastName)
                .NotNull().WithMessage("LastName cannot be empty.");

            RuleFor(model => model.MiddleName)
                .NotNull().WithMessage("MiddleName cannot be empty.");

            RuleFor(model => model.Gender)
                .NotNull().WithMessage("Gender cannot be empty.");

            RuleFor(model => model.Birthdate)
                .NotEmpty().WithMessage("Birthdate cannot be empty.")
                .Must(date => !date.Equals(default(DateTime))).WithMessage("Birthdate must be a valid date.");

            RuleFor(model => model.DriverLicenseNumber)
                .NotNull().WithMessage("DriverLicenseNumber cannot be empty.");

            RuleFor(model => model.DriverLicenseExpirationDate)
                .Must(date => !date.Equals(default(DateTime))).When(model => model.DriverLicenseExpirationDate.HasValue)
                .WithMessage("DriverLicenseExpirationDate must be a valid date.");
        }
    }
}
