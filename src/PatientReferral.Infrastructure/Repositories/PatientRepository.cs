using Microsoft.EntityFrameworkCore;
using PatientReferral.Application.Entities;
using PatientReferral.Application.Interfaces;
using PatientReferral.Infrastructure.Data;

namespace PatientReferral.Infrastructure.Repositories;

public sealed class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _dbContext;

    public PatientRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Patient> CreateAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return patient;
    }

    public Task<Patient?> GetByIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PatientId == patientId, cancellationToken);
    }

    public Task<bool> ExistsAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Patients.AnyAsync(p => p.PatientId == patientId, cancellationToken);
    }
}
