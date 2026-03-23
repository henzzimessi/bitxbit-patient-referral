namespace PatientReferral.Api.Dtos.Responses;

public sealed class ReferralResponse
{
    public int ReferralId { get; init; }
    public int PatientId { get; init; }
    public string ReferralSource { get; init; } = string.Empty;
    public string ReferralType { get; init; } = string.Empty;
    public string? ReferralNote { get; init; }
    public DateTime CreatedDate { get; init; }
}
