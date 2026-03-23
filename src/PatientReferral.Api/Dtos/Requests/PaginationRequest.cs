namespace PatientReferral.Api.Dtos.Requests;

public sealed class PaginationRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public int SafePage => Math.Max(1, Page);
    public int SafePageSize => Math.Clamp(PageSize, 1, 100);
}
