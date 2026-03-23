using PatientReferral.Application.Entities;
using PatientReferral.Application.Interfaces;

namespace PatientReferral.Application.Services;

public sealed class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;

    public PatientService(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public Task<bool> PatientExistsAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return _patientRepository.ExistsAsync(patientId, cancellationToken);
    }

    public Task<Patient> CreatePatientAsync(string firstName, string lastName, DateOnly dateOfBirth, CancellationToken cancellationToken = default)
    {
        var patient = new Patient
        {
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            CreatedDate = DateTime.UtcNow
        };

        return _patientRepository.CreateAsync(patient, cancellationToken);
    }
}
