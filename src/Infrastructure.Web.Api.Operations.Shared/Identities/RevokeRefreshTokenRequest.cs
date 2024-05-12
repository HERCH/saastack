using System.ComponentModel.DataAnnotations;
using Infrastructure.Web.Api.Interfaces;

namespace Infrastructure.Web.Api.Operations.Shared.Identities;

[Route("/tokens/{RefreshToken}", OperationMethod.Delete)]
public class RevokeRefreshTokenRequest : UnTenantedDeleteRequest
{
    [Required] public string? RefreshToken { get; set; }
}