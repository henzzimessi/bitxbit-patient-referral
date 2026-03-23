namespace PatientReferral.Application.Entities;

public sealed class Patient
{
    public int PatientId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public DateTime CreatedDate { get; set; }

    public ICollection<Referral> Referrals { get; set; } = new List<Referral>();
}
