using FluentAssertions;
using PatientReferral.Application.Services;
using PatientReferral.Tests.TestDoubles;
using Xunit;

namespace PatientReferral.Tests.Unit.Services;

public sealed class PatientServiceTests
{
    [Fact]
    public async Task CreatePatientAsync_ValidRequest_ReturnsPatientWithId()
    {
        var repo = new FakePatientRepository();
        var service = new PatientService(repo);

        var result = await service.CreatePatientAsync("John", "Smith", new DateOnly(1943, 2, 1));

        result.PatientId.Should().Be(1);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Smith");
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task PatientExistsAsync_ReturnsRepositoryResult()
    {
        var repo = new FakePatientRepository { ExistsResult = true };
        var service = new PatientService(repo);

        var exists = await service.PatientExistsAsync(42);

        exists.Should().BeTrue();
    }
}
