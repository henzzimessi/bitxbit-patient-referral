namespace PatientReferral.Api.Dtos.Requests;

public sealed class CreatePatientRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateOnly DateOfBirth { get; init; }
}
