using Microsoft.AspNetCore.Mvc;
using PatientReferral.Api.Dtos.Requests;
using PatientReferral.Api.Dtos.Responses;
using PatientReferral.Application.Interfaces;

namespace PatientReferral.Api.Controllers;

[ApiController]
[Route("api/referrals")]
public sealed class ReferralsController : ControllerBase
{
    private readonly IReferralService _referralService;

    public ReferralsController(IReferralService referralService)
    {
        _referralService = referralService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReferralResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateReferral([FromBody] CreateReferralRequest request, CancellationToken cancellationToken)
    {
        var referral = await _referralService.CreateReferralAsync(
            request.PatientId,
            request.ReferralSource,
            request.ReferralType,
            request.ReferralNote,
            cancellationToken);
        var response = referral.ToResponse();

        return CreatedAtAction(nameof(GetReferralById), new { id = referral.ReferralId }, response);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReferralResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReferralById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var referral = await _referralService.GetReferralByIdAsync(id, cancellationToken);
        if (referral is null)
        {
            return NotFound();
        }
        var response = referral.ToResponse();

        return Ok(response);
    }
}
