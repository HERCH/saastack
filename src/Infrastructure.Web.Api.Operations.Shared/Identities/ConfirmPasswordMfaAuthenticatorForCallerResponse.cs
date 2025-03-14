using Application.Resources.Shared;
using Infrastructure.Web.Api.Interfaces;

namespace Infrastructure.Web.Api.Operations.Shared.Identities;

public class ConfirmPasswordMfaAuthenticatorForCallerResponse : IWebResponse
{
    public List<PasswordCredentialMfaAuthenticator>? Authenticators { get; set; }

    public AuthenticateTokens? Tokens { get; set; }
}