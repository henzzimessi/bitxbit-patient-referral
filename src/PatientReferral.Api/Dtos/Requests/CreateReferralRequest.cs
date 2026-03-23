namespace PatientReferral.Api.Dtos.Requests;

public sealed class CreateReferralRequest
{
    public int PatientId { get; init; }
    public string ReferralSource { get; init; } = string.Empty;
    public string ReferralType { get; init; } = string.Empty;
    public string? ReferralNote { get; init; }
}
