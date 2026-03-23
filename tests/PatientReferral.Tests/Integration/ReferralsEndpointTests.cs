using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using PatientReferral.Api.Dtos.Requests;
using PatientReferral.Api.Dtos.Responses;
using Xunit;

namespace PatientReferral.Tests.Integration;

public sealed class ReferralsEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ReferralsEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<PatientResponse> CreatePatientAsync(
        string firstName = "Jane",
        string lastName = "Doe",
        DateOnly? dateOfBirth = null)
    {
        var patientRequest = new CreatePatientRequest
        {
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth ?? new DateOnly(1980, 5, 5)
        };

        var patientResponse = await _client.PostAsJsonAsync("/api/patients", patientRequest);
        patientResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdPatient = await patientResponse.Content.ReadFromJsonAsync<PatientResponse>();
        createdPatient.Should().NotBeNull();
        return createdPatient!;
    }

    [Fact]
    public async Task CreateReferral_ForMissingPatient_ReturnsNotFound()
    {
        var request = new CreateReferralRequest
        {
            PatientId = 999,
            ReferralSource = "Hospital",
            ReferralType = "Short Stay",
            ReferralNote = "Test note"
        };

        var response = await _client.PostAsJsonAsync("/api/referrals", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateReferral_ForExistingPatient_ReturnsCreated()
    {
        var createdPatient = await CreatePatientAsync();

        var referralRequest = new CreateReferralRequest
        {
            PatientId = createdPatient.PatientId,
            ReferralSource = "Hospital",
            ReferralType = "Short Stay",
            ReferralNote = "Test note"
        };

        var referralResponse = await _client.PostAsJsonAsync("/api/referrals", referralRequest);
        referralResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdReferral = await referralResponse.Content.ReadFromJsonAsync<ReferralResponse>();
        createdReferral.Should().NotBeNull();
        createdReferral!.ReferralId.Should().BeGreaterThan(0);
        referralResponse.Headers.Location.Should().NotBeNull();
        referralResponse.Headers.Location!.ToString().Should().Contain($"/api/referrals/{createdReferral.ReferralId}");
    }

    [Fact]
    public async Task CreateReferral_MissingReferralSource_ReturnsBadRequest()
    {
        var createdPatient = await CreatePatientAsync();

        var referralRequest = new CreateReferralRequest
        {
            PatientId = createdPatient.PatientId,
            ReferralSource = "",
            ReferralType = "Short Stay",
            ReferralNote = "Test note"
        };

        var referralResponse = await _client.PostAsJsonAsync("/api/referrals", referralRequest);

        referralResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateReferral_MissingReferralType_ReturnsBadRequestWithValidationBody()
    {
        var createdPatient = await CreatePatientAsync();

        var referralRequest = new CreateReferralRequest
        {
            PatientId = createdPatient.PatientId,
            ReferralSource = "Hospital",
            ReferralType = "",
            ReferralNote = "Test note"
        };

        var referralResponse = await _client.PostAsJsonAsync("/api/referrals", referralRequest);

        referralResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await referralResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Keys.Should().Contain(key => key.Contains("ReferralType", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CreateReferral_PatientIdZero_ReturnsBadRequestWithValidationBody()
    {
        var referralRequest = new CreateReferralRequest
        {
            PatientId = 0,
            ReferralSource = "Hospital",
            ReferralType = "Short Stay",
            ReferralNote = "Test note"
        };

        var referralResponse = await _client.PostAsJsonAsync("/api/referrals", referralRequest);

        referralResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await referralResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Keys.Should().Contain(key => key.Contains("PatientId", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CreateReferral_ReferralNoteTooLong_ReturnsBadRequestWithValidationBody()
    {
        var createdPatient = await CreatePatientAsync("Length", "Tester");

        var referralRequest = new CreateReferralRequest
        {
            PatientId = createdPatient.PatientId,
            ReferralSource = "Hospital",
            ReferralType = "Short Stay",
            ReferralNote = new string('x', 10001)
        };

        var referralResponse = await _client.PostAsJsonAsync("/api/referrals", referralRequest);

        referralResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await referralResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Keys.Should().Contain(key => key.Contains("ReferralNote", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CreateReferral_ReferralSourceAtMaxLength_ReturnsCreated()
    {
        var createdPatient = await CreatePatientAsync("SourceMax", "Tester");

        var referralRequest = new CreateReferralRequest
        {
            PatientId = createdPatient.PatientId,
            ReferralSource = new string('S', 200),
            ReferralType = "Short Stay",
            ReferralNote = "Boundary"
        };

        var response = await _client.PostAsJsonAsync("/api/referrals", referralRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateReferral_ReferralSourceOverMaxLength_ReturnsBadRequest()
    {
        var createdPatient = await CreatePatientAsync("SourceOver", "Tester");

        var referralRequest = new CreateReferralRequest
        {
            PatientId = createdPatient.PatientId,
            ReferralSource = new string('S', 201),
            ReferralType = "Short Stay",
            ReferralNote = "Boundary"
        };

        var response = await _client.PostAsJsonAsync("/api/referrals", referralRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateReferral_ReferralTypeAtMaxLength_ReturnsCreated()
    {
        var createdPatient = await CreatePatientAsync("TypeMax", "Tester");

        var referralRequest = new CreateReferralRequest
        {
            PatientId = createdPatient.PatientId,
            ReferralSource = "Hospital",
            ReferralType = new string('T', 100),
            ReferralNote = "Boundary"
        };

        var response = await _client.PostAsJsonAsync("/api/referrals", referralRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateReferral_ReferralTypeOverMaxLength_ReturnsBadRequest()
    {
        var createdPatient = await CreatePatientAsync("TypeOver", "Tester");

        var referralRequest = new CreateReferralRequest
        {
            PatientId = createdPatient.PatientId,
            ReferralSource = "Hospital",
            ReferralType = new string('T', 101),
            ReferralNote = "Boundary"
        };

        var response = await _client.PostAsJsonAsync("/api/referrals", referralRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateReferral_ReferralNoteAtMaxLength_ReturnsCreated()
    {
        var createdPatient = await CreatePatientAsync("NoteMax", "Tester");

        var referralRequest = new CreateReferralRequest
        {
            PatientId = createdPatient.PatientId,
            ReferralSource = "Hospital",
            ReferralType = "Short Stay",
            ReferralNote = new string('N', 10000)
        };

        var response = await _client.PostAsJsonAsync("/api/referrals", referralRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetReferralById_Missing_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/referrals/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetReferralById_Existing_ReturnsReferral()
    {
        var createdPatient = await CreatePatientAsync("Lookup", "Tester", new DateOnly(1985, 5, 5));

        var referralRequest = new CreateReferralRequest
        {
            PatientId = createdPatient.PatientId,
            ReferralSource = "Hospital",
            ReferralType = "Short Stay",
            ReferralNote = "Lookup note"
        };

        var referralResponse = await _client.PostAsJsonAsync("/api/referrals", referralRequest);
        referralResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdReferral = await referralResponse.Content.ReadFromJsonAsync<ReferralResponse>();
        createdReferral.Should().NotBeNull();

        var response = await _client.GetAsync($"/api/referrals/{createdReferral!.ReferralId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ReferralResponse>();
        body.Should().NotBeNull();
        body!.ReferralId.Should().Be(createdReferral.ReferralId);
        body.PatientId.Should().Be(createdPatient.PatientId);
    }

    [Fact]
    public async Task GetReferralsForPatient_ReturnsPagedItems()
    {
        var createdPatient = await CreatePatientAsync("Paging", "Tester", new DateOnly(1975, 7, 7));

        var referralRequest = new CreateReferralRequest
        {
            PatientId = createdPatient.PatientId,
            ReferralSource = "Hospital",
            ReferralType = "Short Stay",
            ReferralNote = "Test note"
        };

        var referralResponse = await _client.PostAsJsonAsync("/api/referrals", referralRequest);
        referralResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await _client.GetAsync($"/api/patients/{createdPatient.PatientId}/referrals?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<ReferralResponse>>();
        body.Should().NotBeNull();
        body!.Items.Should().NotBeEmpty();
        body.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetReferralsForPatient_MissingPatient_ReturnsNotFoundProblemDetails()
    {
        var response = await _client.GetAsync("/api/patients/999999/referrals?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetReferralsForPatient_WithNoReferrals_ReturnsEmptyList()
    {
        var createdPatient = await CreatePatientAsync("No", "Referrals", new DateOnly(1990, 1, 1));

        var response = await _client.GetAsync($"/api/patients/{createdPatient.PatientId}/referrals?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<ReferralResponse>>();
        body.Should().NotBeNull();
        body!.Items.Should().BeEmpty();
        body.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetReferralsForPatient_PageSizeAboveMax_IsClampedTo100()
    {
        var createdPatient = await CreatePatientAsync("Clamp", "Tester", new DateOnly(1988, 8, 8));

        var response = await _client.GetAsync($"/api/patients/{createdPatient.PatientId}/referrals?page=1&pageSize=1000");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<ReferralResponse>>();
        body.Should().NotBeNull();
        body!.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task GetReferralsForPatient_PageBelowOne_IsClampedToOne()
    {
        var createdPatient = await CreatePatientAsync("Page", "Clamp", new DateOnly(1982, 2, 2));

        var response = await _client.GetAsync($"/api/patients/{createdPatient.PatientId}/referrals?page=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<ReferralResponse>>();
        body.Should().NotBeNull();
        body!.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetReferralsForPatient_PaginationMetadata_IsCalculatedCorrectly()
    {
        var createdPatient = await CreatePatientAsync("Meta", "Pagination", new DateOnly(1979, 9, 9));

        var referral1 = new CreateReferralRequest
        {
            PatientId = createdPatient.PatientId,
            ReferralSource = "Hospital",
            ReferralType = "Short Stay",
            ReferralNote = "First"
        };
        var referral2 = new CreateReferralRequest
        {
            PatientId = createdPatient.PatientId,
            ReferralSource = "Clinic",
            ReferralType = "Follow-up",
            ReferralNote = "Second"
        };

        (await _client.PostAsJsonAsync("/api/referrals", referral1)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await _client.PostAsJsonAsync("/api/referrals", referral2)).StatusCode.Should().Be(HttpStatusCode.Created);

        var page1Response = await _client.GetAsync($"/api/patients/{createdPatient.PatientId}/referrals?page=1&pageSize=1");
        page1Response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page1 = await page1Response.Content.ReadFromJsonAsync<PagedResponse<ReferralResponse>>();
        page1.Should().NotBeNull();
        page1!.TotalCount.Should().Be(2);
        page1.TotalPages.Should().Be(2);
        page1.HasNextPage.Should().BeTrue();
        page1.HasPreviousPage.Should().BeFalse();

        var page2Response = await _client.GetAsync($"/api/patients/{createdPatient.PatientId}/referrals?page=2&pageSize=1");
        page2Response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page2 = await page2Response.Content.ReadFromJsonAsync<PagedResponse<ReferralResponse>>();
        page2.Should().NotBeNull();
        page2!.TotalCount.Should().Be(2);
        page2.TotalPages.Should().Be(2);
        page2.HasNextPage.Should().BeFalse();
        page2.HasPreviousPage.Should().BeTrue();
    }
}
