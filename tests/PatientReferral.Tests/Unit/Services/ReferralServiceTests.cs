using FluentAssertions;
using PatientReferral.Application.Exceptions;
using PatientReferral.Application.Entities;
using PatientReferral.Application.Services;
using PatientReferral.Tests.TestDoubles;
using Xunit;

namespace PatientReferral.Tests.Unit.Services;

public sealed class ReferralServiceTests
{
    [Fact]
    public async Task CreateReferralAsync_PatientMissing_ThrowsNotFound()
    {
        var service = new ReferralService(new FakeReferralRepository(), new FakePatientRepository { ExistsResult = false });

        var act = async () => await service.CreateReferralAsync(1, "Hospital", "Short Stay", "Note");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateReferralAsync_PatientExists_CreatesReferral()
    {
        var repo = new FakeReferralRepository();
        var service = new ReferralService(repo, new FakePatientRepository { ExistsResult = true });

        var result = await service.CreateReferralAsync(1, "Hospital", "Short Stay", "Note");

        result.ReferralId.Should().Be(1);
        result.PatientId.Should().Be(1);
        result.ReferralSource.Should().Be("Hospital");
        result.ReferralType.Should().Be("Short Stay");
        result.ReferralNote.Should().Be("Note");
        repo.CreatedReferral.Should().NotBeNull();
    }

    [Fact]
    public async Task GetReferralsByPatientAsync_PatientExists_ReturnsItemsAndTotal()
    {
        var repo = new FakeReferralRepository
        {
            Referrals = new List<Referral>
            {
                new() { ReferralId = 1, PatientId = 1, ReferralSource = "Hospital", ReferralType = "Short Stay" }
            },
            TotalCount = 1
        };
        var service = new ReferralService(repo, new FakePatientRepository { ExistsResult = true });

        var (items, total) = await service.GetReferralsByPatientAsync(1, 1, 10);

        items.Should().HaveCount(1);
        total.Should().Be(1);
    }

    [Fact]
    public async Task GetReferralsByPatientAsync_PatientMissing_ThrowsNotFound()
    {
        var repo = new FakeReferralRepository();
        var service = new ReferralService(repo, new FakePatientRepository { ExistsResult = false });

        var act = async () => await service.GetReferralsByPatientAsync(999, 1, 10);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
