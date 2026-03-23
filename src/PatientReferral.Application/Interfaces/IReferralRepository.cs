using PatientReferral.Application.Entities;

namespace PatientReferral.Application.Interfaces;

public interface IReferralRepository
{
    Task<Referral> CreateAsync(Referral referral, CancellationToken cancellationToken = default);
    Task<Referral?> GetByIdAsync(int referralId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Referral>> GetByPatientIdAsync(int patientId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
}
