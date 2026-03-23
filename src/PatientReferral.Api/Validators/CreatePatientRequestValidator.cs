using FluentValidation;
using PatientReferral.Api.Dtos.Requests;

namespace PatientReferral.Api.Validators;

public sealed class CreatePatientRequestValidator : AbstractValidator<CreatePatientRequest>
{
    public CreatePatientRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.DateOfBirth)
            .Must(d => d != default)
            .WithMessage("DateOfBirth is required.")
            .Must(d => d < DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("DateOfBirth must be in the past.")
            .Must(d => d >= new DateOnly(1900, 1, 1))
            .WithMessage("DateOfBirth must be on or after 1900-01-01.");
    }
}
