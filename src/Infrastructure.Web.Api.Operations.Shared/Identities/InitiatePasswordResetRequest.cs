using System.ComponentModel.DataAnnotations;
using Infrastructure.Web.Api.Interfaces;

namespace Infrastructure.Web.Api.Operations.Shared.Identities;

/// <summary>
///     Begins a password reset attempt
/// </summary>
[Route("/passwords/reset", OperationMethod.Post)]
public class InitiatePasswordResetRequest : UnTenantedEmptyRequest
{
    [Required] public string? EmailAddress { get; set; }
}