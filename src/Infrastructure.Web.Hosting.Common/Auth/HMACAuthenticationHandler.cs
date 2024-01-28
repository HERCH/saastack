using System.Security.Claims;
using System.Text.Encodings.Web;
using Application.Common;
using Application.Interfaces.Services;
using Common;
using Common.Extensions;
using Infrastructure.Common.Extensions;
using Infrastructure.Interfaces;
using Infrastructure.Web.Api.Common;
using Infrastructure.Web.Api.Common.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Web.Hosting.Common.Auth;

/// <summary>
///     Provides a <see cref="IAuthenticationHandler" /> for HMAC authentication
/// </summary>
public class HMACAuthenticationHandler : AuthenticationHandler<HMACOptions>
{
    public const string AuthenticationScheme = "HMAC";

    public HMACAuthenticationHandler(IOptionsMonitor<HMACOptions> options, ILoggerFactory logger, UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.IsHttps)
        {
            return AuthenticateResult.Fail(Resources.AuthenticationHandler_NotHttps);
        }

        var signature = Request.GetHMACAuth();
        if (!signature.HasValue)
        {
            return AuthenticateResult.Fail(
                Resources.HMACAuthenticationHandler_MissingHeader.Format(HttpHeaders.HMACSignature));
        }

        var hmacSecret = Context.RequestServices.GetRequiredService<IHostSettings>()
            .GetAncillaryApiHostHmacAuthSecret();
        if (hmacSecret.HasNoValue())
        {
            return AuthenticateResult.Success(IssueTicket());
        }

        var isAuthenticated = await Request.VerifyHMACSignatureAsync(signature, hmacSecret, CancellationToken.None);
        if (!isAuthenticated)
        {
            var recorder = Context.RequestServices.GetRequiredService<IRecorder>();
            var caller = Context.RequestServices.GetRequiredService<ICallerContextFactory>().Create();
            recorder.Audit(caller.ToCall(), AuditingConstants.HMACAuthenticationFailed,
                Resources.HMACAuthenticationHandler_FailedAuthentication);
            return AuthenticateResult.Fail(Resources.AuthenticationHandler_Failed);
        }

        return AuthenticateResult.Success(IssueTicket());

        AuthenticationTicket IssueTicket()
        {
            var principal =
                new ClaimsPrincipal(new ClaimsIdentity(ClaimExtensions.ToClaimsForServiceAccount(), Scheme.Name));
            return new AuthenticationTicket(principal, Scheme.Name)
            {
                Properties =
                {
                    AllowRefresh = false,
                    IsPersistent = false
                }
            };
        }
    }
}

/// <summary>
///     Provides options for configuring HMAC authentication
/// </summary>
public class HMACOptions : AuthenticationSchemeOptions
{
}