using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using Xunit.Sdk;

namespace PatientReferral.E2E.Tests;

public sealed class ApiExternalE2ETests
{
    private static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("E2E_BASE_URL")?.TrimEnd('/')
        ?? "http://localhost:8080";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static HttpClient CreateClient()
    {
        return new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(20)
        };
    }

    private static async Task EnsureApiAvailableAsync(HttpClient client)
    {
        try
        {
            using var response = await client.GetAsync("/health");
            if (!response.IsSuccessStatusCode)
            {
                throw new XunitException($"API is reachable but unhealthy. Status: {(int)response.StatusCode}. Expected 200. Ensure Docker stack is running: dev.cmd up");
            }
        }
        catch (Exception ex)
        {
            throw new XunitException($"Cannot reach API at '{BaseUrl}'. Start backend first (dev.cmd up) and retry. Details: {ex.Message}");
        }
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturn200()
    {
        using var client = CreateClient();
        await EnsureApiAvailableAsync(client);

        using var response = await client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreatePatientAndReferral_ShouldPersist_AndBeQueryable()
    {
        using var client = CreateClient();
        await EnsureApiAvailableAsync(client);

        var suffix = Guid.NewGuid().ToString("N")[..8];

        var createPatientPayload = new
        {
            firstName = $"E2E_{suffix}",
            lastName = "Tester",
            dateOfBirth = "1985-06-15"
        };

        using var createPatientResponse = await client.PostAsJsonAsync("/api/patients", createPatientPayload, JsonOptions);
        createPatientResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var patientDoc = await JsonDocument.ParseAsync(await createPatientResponse.Content.ReadAsStreamAsync());
        var patientId = patientDoc.RootElement.GetProperty("patientId").GetInt32();
        patientId.Should().BeGreaterThan(0);

        var referralNote = $"E2E note {suffix}";
        var createReferralPayload = new
        {
            patientId,
            referralSource = "External E2E",
            referralType = "Validation",
            referralNote
        };

        using var createReferralResponse = await client.PostAsJsonAsync("/api/referrals", createReferralPayload, JsonOptions);
        createReferralResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var referralDoc = await JsonDocument.ParseAsync(await createReferralResponse.Content.ReadAsStreamAsync());
        var referralId = referralDoc.RootElement.GetProperty("referralId").GetInt32();
        referralId.Should().BeGreaterThan(0);

        using var getReferralResponse = await client.GetAsync($"/api/referrals/{referralId}");
        getReferralResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var getReferralDoc = await JsonDocument.ParseAsync(await getReferralResponse.Content.ReadAsStreamAsync());
        getReferralDoc.RootElement.GetProperty("patientId").GetInt32().Should().Be(patientId);
        getReferralDoc.RootElement.GetProperty("referralNote").GetString().Should().Be(referralNote);

        using var patientReferralsResponse = await client.GetAsync($"/api/patients/{patientId}/referrals?page=1&pageSize=50");
        patientReferralsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var patientReferralsDoc = await JsonDocument.ParseAsync(await patientReferralsResponse.Content.ReadAsStreamAsync());
        var items = patientReferralsDoc.RootElement.GetProperty("items").EnumerateArray().ToList();
        items.Should().NotBeEmpty();
        items.Any(x => x.GetProperty("referralId").GetInt32() == referralId).Should().BeTrue();
    }
}
