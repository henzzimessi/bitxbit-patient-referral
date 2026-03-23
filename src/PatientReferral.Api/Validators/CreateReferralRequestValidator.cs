using FluentValidation;
using PatientReferral.Api.Dtos.Requests;

namespace PatientReferral.Api.Validators;

public sealed class CreateReferralRequestValidator : AbstractValidator<CreateReferralRequest>
{
    public CreateReferralRequestValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0);

        RuleFor(x => x.ReferralSource)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ReferralType)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ReferralNote)
            .MaximumLength(10000)
            .When(x => !string.IsNullOrWhiteSpace(x.ReferralNote));
    }
}
