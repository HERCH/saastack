using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Application.Interfaces.Services;
using Common;
using Common.Extensions;
using Infrastructure.Interfaces;
using Infrastructure.Web.Api.Common;
using Infrastructure.Web.Hosting.Common.Pipeline;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Infrastructure.Web.Hosting.Common.UnitTests.Pipeline;

[Trait("Category", "Unit")]
public class CSRFMiddlewareSpec
{
    private readonly Mock<CSRFMiddleware.ICSRFService> _csrfService;
    private readonly Mock<IHostSettings> _hostSettings;
    private readonly CSRFMiddleware _middleware;
    private readonly Mock<RequestDelegate> _next;
    private readonly ServiceProvider _serviceProvider;

    public CSRFMiddlewareSpec()
    {
        var recorder = new Mock<IRecorder>();
        _hostSettings = new Mock<IHostSettings>();
        _hostSettings.Setup(s => s.GetWebsiteHostCSRFEncryptionSecret())
            .Returns("anexcryptionsecret");
        _hostSettings.Setup(s => s.GetWebsiteHostCSRFSigningSecret())
            .Returns("asigningsecret");
        _hostSettings.Setup(s => s.GetWebsiteHostBaseUrl())
            .Returns("https://localhost");
        _next = new Mock<RequestDelegate>();
        _csrfService = new Mock<CSRFMiddleware.ICSRFService>();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ILoggerFactory>(new LoggerFactory());
        _serviceProvider = serviceCollection.BuildServiceProvider();

        _middleware = new CSRFMiddleware(_next.Object, recorder.Object, _hostSettings.Object, _csrfService.Object);
    }

    [Fact]
    public async Task WhenInvokeAsyncAndIsIgnoredMethod_ThenContinuesPipeline()
    {
        var context = new DefaultHttpContext
        {
            Request = { Method = HttpMethods.Get }
        };

        await _middleware.InvokeAsync(context);

        _next.Verify(n => n.Invoke(context));
        _csrfService.Verify(
            cs => cs.VerifyTokens(It.IsAny<Optional<string>>(), It.IsAny<Optional<string>>(),
                It.IsAny<Optional<string>>()), Times.Never);
    }

    [Fact]
    public async Task WhenInvokeAsyncAndMissingHostName_ThenReturnsError()
    {
        var context = SetupContext();
        _hostSettings.Setup(s => s.GetWebsiteHostBaseUrl()).Returns("notauri");

        await _middleware.InvokeAsync(context);

        context.Response.Should().BeAProblem(HttpStatusCode.InternalServerError,
            Resources.CSRFMiddleware_InvalidHostName.Format("notauri"));
        _next.Verify(n => n.Invoke(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task WhenInvokeAsyncAndMissingCookie_ThenReturnsError()
    {
        var context = SetupContext();

        await _middleware.InvokeAsync(context);

        context.Response.Should().BeAProblem(HttpStatusCode.Forbidden,
            Resources.CSRFMiddleware_MissingCSRFCookieValue);
        _next.Verify(n => n.Invoke(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task WhenInvokeAsyncAndMissingHeader_ThenReturnsError()
    {
        var context = SetupContext();
        context.Request.Cookies = SetupCookies(new Dictionary<string, string>
            { { CSRFConstants.Cookies.AntiCSRF, "ananticsrfcookie" } });

        await _middleware.InvokeAsync(context);

        context.Response.Should().BeAProblem(HttpStatusCode.Forbidden,
            Resources.CSRFMiddleware_MissingCSRFHeaderValue);
        _next.Verify(n => n.Invoke(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task WhenInvokeAsyncAndAuthTokenIsInvalid_ThenReturnsError()
    {
        var context = SetupContext();
        context.Request.Cookies = SetupCookies(new Dictionary<string, string>
        {
            { CSRFConstants.Cookies.AntiCSRF, "ananticsrfcookie" },
            { AuthenticationConstants.Cookies.Token, "notavalidtoken" }
        });
        context.Request.Headers.Add(CSRFConstants.Headers.AntiCSRF, new StringValues("ananticsrfheader"));

        await _middleware.InvokeAsync(context);

        context.Response.Should().BeAProblem(HttpStatusCode.Forbidden,
            Resources.CSRFMiddleware_InvalidToken);
        _next.Verify(n => n.Invoke(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task WhenInvokeAsyncAndTokenNotContainUserIdClaim_ThenReturnsError()
    {
        var tokenWithoutUserClaim = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            claims: new Claim[] { }
        ));
        var context = SetupContext();
        context.Request.Cookies = SetupCookies(new Dictionary<string, string>
        {
            { CSRFConstants.Cookies.AntiCSRF, "ananticsrfcookie" },
            { AuthenticationConstants.Cookies.Token, tokenWithoutUserClaim }
        });
        context.Request.Headers.Add(CSRFConstants.Headers.AntiCSRF, new StringValues("ananticsrfheader"));

        await _middleware.InvokeAsync(context);

        context.Response.Should().BeAProblem(HttpStatusCode.Forbidden,
            Resources.CSRFMiddleware_InvalidToken);
        _next.Verify(n => n.Invoke(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task WhenInvokeAsyncAndTokensNotVerifiedForNoUser_ThenReturnsError()
    {
        var context = SetupContext();
        context.Request.Cookies = SetupCookies(new Dictionary<string, string>
        {
            { CSRFConstants.Cookies.AntiCSRF, "ananticsrfcookie" },
            { AuthenticationConstants.Cookies.Token, string.Empty }
        });
        context.Request.Headers.Add(CSRFConstants.Headers.AntiCSRF, new StringValues("ananticsrfheader"));
        _csrfService.Setup(crs => crs.VerifyTokens(It.IsAny<Optional<string>>(), It.IsAny<Optional<string>>(),
                It.IsAny<Optional<string>>()))
            .Returns(false);

        await _middleware.InvokeAsync(context);

        context.Response.Should().BeAProblem(HttpStatusCode.Forbidden,
            Resources.CSRFMiddleware_InvalidSignature.Format(nameof(Optional.None)));
        _csrfService.Setup(crs => crs.VerifyTokens("ananticsrfheader", "ananticsrfcookie", Optional<string>.None))
            .Returns(false);
        _next.Verify(n => n.Invoke(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task WhenInvokeAsyncAndTokensNotVerifiedForUser_ThenReturnsError()
    {
        var tokenForUser = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            claims: new Claim[]
            {
                new(AuthenticationConstants.Claims.ForId, "auserid")
            }
        ));
        var context = SetupContext();
        context.Request.Cookies = SetupCookies(new Dictionary<string, string>
        {
            { CSRFConstants.Cookies.AntiCSRF, "ananticsrfcookie" },
            { AuthenticationConstants.Cookies.Token, tokenForUser }
        });
        context.Request.Headers.Add(CSRFConstants.Headers.AntiCSRF, new StringValues("ananticsrfheader"));
        _csrfService.Setup(crs => crs.VerifyTokens(It.IsAny<Optional<string>>(), It.IsAny<Optional<string>>(),
                It.IsAny<Optional<string>>()))
            .Returns(false);

        await _middleware.InvokeAsync(context);

        context.Response.Should().BeAProblem(HttpStatusCode.Forbidden,
            Resources.CSRFMiddleware_InvalidSignature.Format("auserid"));
        _csrfService.Setup(crs => crs.VerifyTokens("ananticsrfheader", "ananticsrfcookie", "auserid"))
            .Returns(false);
        _next.Verify(n => n.Invoke(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task WhenInvokeAsyncAndTokensIsVerifiedButNoOriginAndNoReferer_ThenReturnsError()
    {
        var tokenForUser = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            claims: new Claim[]
            {
                new(AuthenticationConstants.Claims.ForId, "auserid")
            }
        ));
        var context = SetupContext();
        context.Request.Cookies = SetupCookies(new Dictionary<string, string>
        {
            { CSRFConstants.Cookies.AntiCSRF, "ananticsrfcookie" },
            { AuthenticationConstants.Cookies.Token, tokenForUser }
        });
        context.Request.Headers.Add(CSRFConstants.Headers.AntiCSRF, new StringValues("ananticsrfheader"));
        context.Request.Headers.Add(HttpHeaders.Origin, new StringValues(string.Empty));
        context.Request.Headers.Add(HttpHeaders.Referer, new StringValues(string.Empty));
        _csrfService.Setup(crs => crs.VerifyTokens(It.IsAny<Optional<string>>(), It.IsAny<Optional<string>>(),
                It.IsAny<Optional<string>>()))
            .Returns(true);

        await _middleware.InvokeAsync(context);

        context.Response.Should().BeAProblem(HttpStatusCode.Forbidden,
            Resources.CSRFMiddleware_MissingOriginAndReferer);
        _csrfService.Setup(crs => crs.VerifyTokens("ananticsrfheader", "ananticsrfcookie", "auserid"))
            .Returns(false);
        _next.Verify(n => n.Invoke(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task WhenInvokeAsyncAndTokensIsVerifiedButOriginNotHost_ThenReturnsError()
    {
        var tokenForUser = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            claims: new Claim[]
            {
                new(AuthenticationConstants.Claims.ForId, "auserid")
            }
        ));
        var context = SetupContext();
        context.Request.Cookies = SetupCookies(new Dictionary<string, string>
        {
            { CSRFConstants.Cookies.AntiCSRF, "ananticsrfcookie" },
            { AuthenticationConstants.Cookies.Token, tokenForUser }
        });
        context.Request.Headers.Add(CSRFConstants.Headers.AntiCSRF, new StringValues("ananticsrfheader"));
        context.Request.Headers.Add(HttpHeaders.Origin, new StringValues("anotherhostname"));
        context.Request.Headers.Add(HttpHeaders.Referer, new StringValues(string.Empty));
        _csrfService.Setup(crs => crs.VerifyTokens(It.IsAny<Optional<string>>(), It.IsAny<Optional<string>>(),
                It.IsAny<Optional<string>>()))
            .Returns(true);

        await _middleware.InvokeAsync(context);

        context.Response.Should().BeAProblem(HttpStatusCode.Forbidden,
            Resources.CSRFMiddleware_OriginMismatched);
        _csrfService.Setup(crs => crs.VerifyTokens("ananticsrfheader", "ananticsrfcookie", "auserid"))
            .Returns(false);
        _next.Verify(n => n.Invoke(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task WhenInvokeAsyncAndTokensIsVerifiedButRefererNotHost_ThenReturnsError()
    {
        var tokenForUser = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            claims: new Claim[]
            {
                new(AuthenticationConstants.Claims.ForId, "auserid")
            }
        ));
        var context = SetupContext();
        context.Request.Cookies = SetupCookies(new Dictionary<string, string>
        {
            { CSRFConstants.Cookies.AntiCSRF, "ananticsrfcookie" },
            { AuthenticationConstants.Cookies.Token, tokenForUser }
        });
        context.Request.Headers.Add(CSRFConstants.Headers.AntiCSRF, new StringValues("ananticsrfheader"));
        context.Request.Headers.Add(HttpHeaders.Origin, new StringValues(string.Empty));
        context.Request.Headers.Add(HttpHeaders.Referer, new StringValues("anotherhostname"));
        _csrfService.Setup(crs => crs.VerifyTokens(It.IsAny<Optional<string>>(), It.IsAny<Optional<string>>(),
                It.IsAny<Optional<string>>()))
            .Returns(true);

        await _middleware.InvokeAsync(context);

        context.Response.Should().BeAProblem(HttpStatusCode.Forbidden,
            Resources.CSRFMiddleware_RefererMismatched);
        _csrfService.Setup(crs => crs.VerifyTokens("ananticsrfheader", "ananticsrfcookie", "auserid"))
            .Returns(false);
        _next.Verify(n => n.Invoke(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task WhenInvokeAsyncAndTokensIsVerifiedAndOriginIsHostForUser_ThenContinuesPipeline()
    {
        var tokenForUser = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            claims: new Claim[]
            {
                new(AuthenticationConstants.Claims.ForId, "auserid")
            }
        ));
        var context = SetupContext();
        context.Request.Cookies = SetupCookies(new Dictionary<string, string>
        {
            { CSRFConstants.Cookies.AntiCSRF, "ananticsrfcookie" },
            { AuthenticationConstants.Cookies.Token, tokenForUser }
        });
        context.Request.Headers.Add(CSRFConstants.Headers.AntiCSRF, new StringValues("ananticsrfheader"));
        context.Request.Headers.Add(HttpHeaders.Origin, new StringValues("https://localhost"));
        context.Request.Headers.Add(HttpHeaders.Referer, new StringValues(string.Empty));
        _csrfService.Setup(crs => crs.VerifyTokens(It.IsAny<Optional<string>>(), It.IsAny<Optional<string>>(),
                It.IsAny<Optional<string>>()))
            .Returns(true);

        await _middleware.InvokeAsync(context);

        context.Response.Should().NotBeAProblem();
        _csrfService.Setup(crs => crs.VerifyTokens("ananticsrfheader", "ananticsrfcookie", "auserid"))
            .Returns(false);
        _next.Verify(n => n.Invoke(context));
    }

    [Fact]
    public async Task WhenInvokeAsyncAndTokensIsVerifiedAndRefererIsHostForUser_ThenContinuesPipeline()
    {
        var tokenForUser = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            claims: new Claim[]
            {
                new(AuthenticationConstants.Claims.ForId, "auserid")
            }
        ));
        var context = SetupContext();
        context.Request.Cookies = SetupCookies(new Dictionary<string, string>
        {
            { CSRFConstants.Cookies.AntiCSRF, "ananticsrfcookie" },
            { AuthenticationConstants.Cookies.Token, tokenForUser }
        });
        context.Request.Headers.Add(CSRFConstants.Headers.AntiCSRF, new StringValues("ananticsrfheader"));
        context.Request.Headers.Add(HttpHeaders.Origin, new StringValues(string.Empty));
        context.Request.Headers.Add(HttpHeaders.Referer, new StringValues("https://localhost"));
        _csrfService.Setup(crs => crs.VerifyTokens(It.IsAny<Optional<string>>(), It.IsAny<Optional<string>>(),
                It.IsAny<Optional<string>>()))
            .Returns(true);

        await _middleware.InvokeAsync(context);

        context.Response.Should().NotBeAProblem();
        _csrfService.Setup(crs => crs.VerifyTokens("ananticsrfheader", "ananticsrfcookie", "auserid"))
            .Returns(false);
        _next.Verify(n => n.Invoke(context));
    }

    [Fact]
    public async Task WhenInvokeAsyncAndTokensIsVerifiedAndOriginIsHostForNoUser_ThenContinuesPipeline()
    {
        var context = SetupContext();
        context.Request.Cookies = SetupCookies(new Dictionary<string, string>
        {
            { CSRFConstants.Cookies.AntiCSRF, "ananticsrfcookie" }
        });
        context.Request.Headers.Add(CSRFConstants.Headers.AntiCSRF, new StringValues("ananticsrfheader"));
        context.Request.Headers.Add(HttpHeaders.Origin, new StringValues("https://localhost"));
        context.Request.Headers.Add(HttpHeaders.Referer, new StringValues(string.Empty));
        _csrfService.Setup(crs => crs.VerifyTokens(It.IsAny<Optional<string>>(), It.IsAny<Optional<string>>(),
                It.IsAny<Optional<string>>()))
            .Returns(true);

        await _middleware.InvokeAsync(context);

        context.Response.Should().NotBeAProblem();
        _csrfService.Setup(crs => crs.VerifyTokens("ananticsrfheader", "ananticsrfcookie", Optional<string>.None))
            .Returns(false);
        _next.Verify(n => n.Invoke(context));
    }

    [Fact]
    public async Task WhenInvokeAsyncAndTokensIsVerifiedAndRefererIsHostForNoUser_ThenContinuesPipeline()
    {
        var context = SetupContext();
        context.Request.Cookies = SetupCookies(new Dictionary<string, string>
        {
            { CSRFConstants.Cookies.AntiCSRF, "ananticsrfcookie" }
        });
        context.Request.Headers.Add(CSRFConstants.Headers.AntiCSRF, new StringValues("ananticsrfheader"));
        context.Request.Headers.Add(HttpHeaders.Origin, new StringValues(string.Empty));
        context.Request.Headers.Add(HttpHeaders.Referer, new StringValues("https://localhost"));
        _csrfService.Setup(crs => crs.VerifyTokens(It.IsAny<Optional<string>>(), It.IsAny<Optional<string>>(),
                It.IsAny<Optional<string>>()))
            .Returns(true);

        await _middleware.InvokeAsync(context);

        context.Response.Should().NotBeAProblem();
        _csrfService.Setup(crs => crs.VerifyTokens("ananticsrfheader", "ananticsrfcookie", Optional<string>.None))
            .Returns(false);
        _next.Verify(n => n.Invoke(context));
    }

    private DefaultHttpContext SetupContext()
    {
        var context = new DefaultHttpContext
        {
            Request = { Method = HttpMethods.Post },
            RequestServices = _serviceProvider,
            Response =
            {
                StatusCode = 200,
                Body = new MemoryStream()
            }
        };
        return context;
    }

    private static IRequestCookieCollection SetupCookies(Dictionary<string, string> values)
    {
        var cookies = new Mock<IRequestCookieCollection>();
        foreach (var value in values)
        {
            cookies.Setup(c => c.TryGetValue(value.Key, out It.Ref<string?>.IsAny))
                .Returns((string _, ref string? val) =>
                {
                    val = value.Value;
                    return value.Value.HasValue();
                });
        }

        return cookies.Object;
    }
}