using System.ComponentModel.DataAnnotations;
using Infrastructure.Web.Api.Interfaces;

namespace Infrastructure.Web.Api.Operations.Shared.Cars;

[Route("/cars/{Id}/offline", OperationMethod.PutPatch, AccessType.Token)]
[Authorize(Roles.Tenant_Member, Features.Tenant_PaidTrial)]
public class TakeOfflineCarRequest : TenantedRequest<GetCarResponse>
{
    public DateTime? FromUtc { get; set; }

    [Required] public string? Id { get; set; }

    public DateTime? ToUtc { get; set; }
}