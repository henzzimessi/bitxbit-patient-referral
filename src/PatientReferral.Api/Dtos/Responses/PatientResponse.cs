namespace PatientReferral.Api.Dtos.Responses;

public sealed class PatientResponse
{
    public int PatientId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateOnly DateOfBirth { get; init; }
    public DateTime CreatedDate { get; init; }
}
