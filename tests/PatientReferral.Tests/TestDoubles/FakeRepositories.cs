using PatientReferral.Application.Entities;
using PatientReferral.Application.Interfaces;

namespace PatientReferral.Tests.TestDoubles;

internal sealed class FakePatientRepository : IPatientRepository
{
    private int _nextId = 1;

    public Patient? CreatedPatient { get; private set; }
    public bool ExistsResult { get; set; }

    public Task<Patient> CreateAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        patient.PatientId = _nextId++;
        CreatedPatient = patient;
        return Task.FromResult(patient);
    }

    public Task<Patient?> GetByIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Patient?>(null);
    }

    public Task<bool> ExistsAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ExistsResult);
    }
}

internal sealed class FakeReferralRepository : IReferralRepository
{
    public Referral? CreatedReferral { get; private set; }
    public IReadOnlyList<Referral> Referrals { get; set; } = Array.Empty<Referral>();
    public int TotalCount { get; set; }

    public Task<Referral> CreateAsync(Referral referral, CancellationToken cancellationToken = default)
    {
        referral.ReferralId = 1;
        CreatedReferral = referral;
        return Task.FromResult(referral);
    }

    public Task<Referral?> GetByIdAsync(int referralId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Referral?>(null);
    }

    public Task<IReadOnlyList<Referral>> GetByPatientIdAsync(int patientId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Referrals);
    }

    public Task<int> CountByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TotalCount);
    }
}
