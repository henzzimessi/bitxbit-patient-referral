using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PatientReferral.Api.Dtos.Requests;
using PatientReferral.Application.Entities;
using PatientReferral.Application.Interfaces;
using Xunit;

namespace PatientReferral.Tests.Integration;

public sealed class ProblemDetailsContractTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProblemDetailsContractTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetReferralsForPatient_MissingPatient_ReturnsStrictProblemDetailsContract()
    {
        var response = await _client.GetAsync("/api/patients/999999/referrals?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType.Should().NotBeNull();
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be((int)HttpStatusCode.NotFound);
        problem.Type.Should().Be("https://www.rfc-editor.org/rfc/rfc9110#section-15.5.5");
        problem.Title.Should().Contain("Patient with id 999999 was not found.");
        problem.Extensions.Should().ContainKey("traceId");
        problem.Extensions["traceId"]?.ToString().Should().NotBeNullOrWhiteSpace();
    }
}

public sealed class InternalServerErrorProblemDetailsContractTests : IClassFixture<ThrowingReferralWebApplicationFactory>
{
    private readonly HttpClient _client;

    public InternalServerErrorProblemDetailsContractTests(ThrowingReferralWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateReferral_WhenUnhandledExceptionOccurs_ReturnsStrict500ProblemDetails()
    {
        var request = new CreateReferralRequest
        {
            PatientId = 1,
            ReferralSource = "Hospital",
            ReferralType = "Short Stay",
            ReferralNote = "Trigger exception"
        };

        var response = await _client.PostAsJsonAsync("/api/referrals", request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType.Should().NotBeNull();
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be((int)HttpStatusCode.InternalServerError);
        problem.Type.Should().Be("https://www.rfc-editor.org/rfc/rfc9110#section-15.6.1");
        problem.Title.Should().Be("An unexpected error occurred.");
        problem.Extensions.Should().ContainKey("traceId");
        problem.Extensions["traceId"]?.ToString().Should().NotBeNullOrWhiteSpace();
    }
}

public sealed class ThrowingReferralWebApplicationFactory : CustomWebApplicationFactory
{
    protected override string GetDatabaseNamePrefix() => "PatientReferral_TestDb_Throw";

    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
        services.RemoveAll<IReferralService>();
        services.AddScoped<IReferralService, ThrowingReferralService>();
    }

    private sealed class ThrowingReferralService : IReferralService
    {
        public Task<Referral> CreateReferralAsync(int patientId, string referralSource, string referralType, string? referralNote, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Forced for integration test");
        }

        public Task<Referral?> GetReferralByIdAsync(int referralId, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Forced for integration test");
        }

        public Task<(IReadOnlyList<Referral> Items, int TotalCount)> GetReferralsByPatientAsync(int patientId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Forced for integration test");
        }
    }
}
