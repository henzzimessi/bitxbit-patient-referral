namespace PatientReferral.Application.Entities;

public sealed class Referral
{
    public int ReferralId { get; set; }
    public int PatientId { get; set; }
    public string ReferralSource { get; set; } = string.Empty;
    public string ReferralType { get; set; } = string.Empty;
    public string? ReferralNote { get; set; }
    public DateTime CreatedDate { get; set; }

    public Patient? Patient { get; set; }
}
