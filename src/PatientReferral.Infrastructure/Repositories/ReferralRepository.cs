using Microsoft.EntityFrameworkCore;
using PatientReferral.Application.Entities;
using PatientReferral.Application.Interfaces;
using PatientReferral.Infrastructure.Data;

namespace PatientReferral.Infrastructure.Repositories;

public sealed class ReferralRepository : IReferralRepository
{
    private readonly AppDbContext _dbContext;

    public ReferralRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Referral> CreateAsync(Referral referral, CancellationToken cancellationToken = default)
    {
        _dbContext.Referrals.Add(referral);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return referral;
    }

    public Task<Referral?> GetByIdAsync(int referralId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Referrals
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ReferralId == referralId, cancellationToken);
    }

    public async Task<IReadOnlyList<Referral>> GetByPatientIdAsync(int patientId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var offset = (Math.Max(1, page) - 1) * Math.Clamp(pageSize, 1, 100);
        var take = Math.Clamp(pageSize, 1, 100);

        return await _dbContext.Referrals
            .AsNoTracking()
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.CreatedDate)
            .Skip(offset)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Referrals.CountAsync(r => r.PatientId == patientId, cancellationToken);
    }
}
