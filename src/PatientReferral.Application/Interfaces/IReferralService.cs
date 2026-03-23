using PatientReferral.Application.Entities;

namespace PatientReferral.Application.Interfaces;

public interface IReferralService
{
    Task<Referral> CreateReferralAsync(int patientId, string referralSource, string referralType, string? referralNote, CancellationToken cancellationToken = default);
    Task<Referral?> GetReferralByIdAsync(int referralId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Referral> Items, int TotalCount)> GetReferralsByPatientAsync(int patientId, int page, int pageSize, CancellationToken cancellationToken = default);
}
