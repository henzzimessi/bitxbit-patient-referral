using Microsoft.AspNetCore.Mvc;
using PatientReferral.Api.Dtos.Requests;
using PatientReferral.Api.Dtos.Responses;
using PatientReferral.Application.Interfaces;

namespace PatientReferral.Api.Controllers;

[ApiController]
[Route("api/patients")]
public sealed class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly IReferralService _referralService;

    public PatientsController(IPatientService patientService, IReferralService referralService)
    {
        _patientService = patientService;
        _referralService = referralService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePatient([FromBody] CreatePatientRequest request, CancellationToken cancellationToken)
    {
        var patient = await _patientService.CreatePatientAsync(request.FirstName, request.LastName, request.DateOfBirth, cancellationToken);
        var response = patient.ToResponse();

        return Created($"/api/patients/{patient.PatientId}", response);
    }

    [HttpGet("{id:int}/referrals")]
    [ProducesResponseType(typeof(PagedResponse<ReferralResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReferralsForPatient([FromRoute] int id, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _referralService.GetReferralsByPatientAsync(id, pagination.SafePage, pagination.SafePageSize, cancellationToken);

        var responses = items.Select(r => r.ToResponse()).ToList();

        var paged = new PagedResponse<ReferralResponse>
        {
            Items = responses,
            Page = pagination.SafePage,
            PageSize = pagination.SafePageSize,
            TotalCount = totalCount
        };

        return Ok(paged);
    }
}
