using PatientReferral.Application.Entities;

namespace PatientReferral.Application.Interfaces;

public interface IPatientService
{
    Task<Patient> CreatePatientAsync(string firstName, string lastName, DateOnly dateOfBirth, CancellationToken cancellationToken = default);
    Task<bool> PatientExistsAsync(int patientId, CancellationToken cancellationToken = default);
}
