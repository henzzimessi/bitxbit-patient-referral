using PatientReferral.Application.Entities;

namespace PatientReferral.Api.Dtos.Responses;

internal static class ResponseMappings
{
    public static PatientResponse ToResponse(this Patient patient)
    {
        return new PatientResponse
        {
            PatientId = patient.PatientId,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            DateOfBirth = patient.DateOfBirth,
            CreatedDate = patient.CreatedDate
        };
    }

    public static ReferralResponse ToResponse(this Referral referral)
    {
        return new ReferralResponse
        {
            ReferralId = referral.ReferralId,
            PatientId = referral.PatientId,
            ReferralSource = referral.ReferralSource,
            ReferralType = referral.ReferralType,
            ReferralNote = referral.ReferralNote,
            CreatedDate = referral.CreatedDate
        };
    }
}