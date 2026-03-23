using PatientReferral.Application.Exceptions;
using PatientReferral.Application.Entities;
using PatientReferral.Application.Interfaces;

namespace PatientReferral.Application.Services;

public sealed class ReferralService : IReferralService
{
    private readonly IReferralRepository _referralRepository;
    private readonly IPatientRepository _patientRepository;

    public ReferralService(IReferralRepository referralRepository, IPatientRepository patientRepository)
    {
        _referralRepository = referralRepository;
        _patientRepository = patientRepository;
    }

    public async Task<Referral> CreateReferralAsync(int patientId, string referralSource, string referralType, string? referralNote, CancellationToken cancellationToken = default)
    {
        var patientExists = await _patientRepository.ExistsAsync(patientId, cancellationToken);
        if (!patientExists)
        {
            throw new NotFoundException($"Patient with id {patientId} was not found.");
        }

        var referral = new Referral
        {
            PatientId = patientId,
            ReferralSource = referralSource,
            ReferralType = referralType,
            ReferralNote = referralNote,
            CreatedDate = DateTime.UtcNow
        };

        return await _referralRepository.CreateAsync(referral, cancellationToken);
    }

    public Task<Referral?> GetReferralByIdAsync(int referralId, CancellationToken cancellationToken = default)
    {
        return _referralRepository.GetByIdAsync(referralId, cancellationToken);
    }

    public async Task<(IReadOnlyList<Referral> Items, int TotalCount)> GetReferralsByPatientAsync(int patientId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var patientExists = await _patientRepository.ExistsAsync(patientId, cancellationToken);
        if (!patientExists)
        {
            throw new NotFoundException($"Patient with id {patientId} was not found.");
        }

        var items = await _referralRepository.GetByPatientIdAsync(patientId, page, pageSize, cancellationToken);
        var total = await _referralRepository.CountByPatientIdAsync(patientId, cancellationToken);
        return (items, total);
    }
}
