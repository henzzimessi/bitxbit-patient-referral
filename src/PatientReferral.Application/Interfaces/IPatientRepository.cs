using PatientReferral.Application.Entities;

namespace PatientReferral.Application.Interfaces;

public interface IPatientRepository
{
    Task<Patient> CreateAsync(Patient patient, CancellationToken cancellationToken = default);
    Task<Patient?> GetByIdAsync(int patientId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int patientId, CancellationToken cancellationToken = default);
}
