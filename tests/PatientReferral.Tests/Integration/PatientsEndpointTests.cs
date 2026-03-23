using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using PatientReferral.Api.Dtos.Requests;
using PatientReferral.Api.Dtos.Responses;
using Xunit;

namespace PatientReferral.Tests.Integration;

public sealed class PatientsEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PatientsEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreatePatient_ReturnsCreated()
    {
        var request = new CreatePatientRequest
        {
            FirstName = "John",
            LastName = "Smith",
            DateOfBirth = new DateOnly(1943, 2, 1)
        };

        var response = await _client.PostAsJsonAsync("/api/patients", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<PatientResponse>();
        body.Should().NotBeNull();
        body!.PatientId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreatePatient_MissingFirstName_ReturnsBadRequest()
    {
        var request = new CreatePatientRequest
        {
            FirstName = "",
            LastName = "Smith",
            DateOfBirth = new DateOnly(1943, 2, 1)
        };

        var response = await _client.PostAsJsonAsync("/api/patients", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePatient_FutureDateOfBirth_ReturnsBadRequestWithValidationBody()
    {
        var request = new CreatePatientRequest
        {
            FirstName = "John",
            LastName = "Smith",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1))
        };

        var response = await _client.PostAsJsonAsync("/api/patients", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Keys.Should().Contain(key => key.Contains("DateOfBirth", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CreatePatient_MissingLastName_ReturnsBadRequest()
    {
        var request = new CreatePatientRequest
        {
            FirstName = "John",
            LastName = "",
            DateOfBirth = new DateOnly(1943, 2, 1)
        };

        var response = await _client.PostAsJsonAsync("/api/patients", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePatient_DefaultDateOfBirth_ReturnsBadRequestWithValidationBody()
    {
        var request = new CreatePatientRequest
        {
            FirstName = "John",
            LastName = "Smith",
            DateOfBirth = default
        };

        var response = await _client.PostAsJsonAsync("/api/patients", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Keys.Should().Contain(key => key.Contains("DateOfBirth", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CreatePatient_DateOfBirthBefore1900_ReturnsBadRequestWithValidationBody()
    {
        var request = new CreatePatientRequest
        {
            FirstName = "John",
            LastName = "Smith",
            DateOfBirth = new DateOnly(1899, 12, 31)
        };

        var response = await _client.PostAsJsonAsync("/api/patients", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Keys.Should().Contain(key => key.Contains("DateOfBirth", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CreatePatient_DateOfBirthToday_ReturnsBadRequest()
    {
        var request = new CreatePatientRequest
        {
            FirstName = "John",
            LastName = "Smith",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.Date)
        };

        var response = await _client.PostAsJsonAsync("/api/patients", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePatient_FirstNameAtMaxLength_ReturnsCreated()
    {
        var request = new CreatePatientRequest
        {
            FirstName = new string('A', 100),
            LastName = "Smith",
            DateOfBirth = new DateOnly(1943, 2, 1)
        };

        var response = await _client.PostAsJsonAsync("/api/patients", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreatePatient_FirstNameOverMaxLength_ReturnsBadRequest()
    {
        var request = new CreatePatientRequest
        {
            FirstName = new string('A', 101),
            LastName = "Smith",
            DateOfBirth = new DateOnly(1943, 2, 1)
        };

        var response = await _client.PostAsJsonAsync("/api/patients", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePatient_LastNameOverMaxLength_ReturnsBadRequest()
    {
        var request = new CreatePatientRequest
        {
            FirstName = "John",
            LastName = new string('B', 101),
            DateOfBirth = new DateOnly(1943, 2, 1)
        };

        var response = await _client.PostAsJsonAsync("/api/patients", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
