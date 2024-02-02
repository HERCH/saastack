using Infrastructure.Hosting.Common;

namespace Infrastructure.Web.Hosting.Common;

/// <summary>
///     Defines options for different web hosts
/// </summary>
public class WebHostOptions : HostOptions
{
    public new static readonly WebHostOptions BackEndAncillaryApiHost = new(HostOptions.BackEndAncillaryApiHost)
    {
        CORS = CORSOption.AnyOrigin,
        Authorization = new AuthorizationOptions
        {
            UsesCookies = false,
            UsesTokens = true,
            UsesApiKeys = true,
            UsesHMAC = true
        }
    };
    public new static readonly WebHostOptions BackEndApiHost = new(HostOptions.BackEndApiHost)
    {
        CORS = CORSOption.AnyOrigin,
        Authorization = new AuthorizationOptions
        {
            UsesCookies = false,
            UsesTokens = true,
            UsesApiKeys = true,
            UsesHMAC = true
        }
    };

    public new static readonly WebHostOptions BackEndForFrontEndWebHost = new(HostOptions.BackEndForFrontEndWebHost)
    {
        CORS = CORSOption.SameOrigin,
        Authorization = new AuthorizationOptions
        {
            UsesCookies = true,
            UsesTokens = false,
            UsesApiKeys = false,
            UsesHMAC = false
        }
    };

    public new static readonly WebHostOptions TestingStubsHost = new(HostOptions.TestingStubsHost)
    {
        CORS = CORSOption.AnyOrigin,
        Authorization = new AuthorizationOptions
        {
            UsesCookies = false,
            UsesTokens = false,
            UsesApiKeys = false,
            UsesHMAC = false
        }
    };

    private WebHostOptions(HostOptions options) : base(options)
    {
        CORS = CORSOption.None;
        Authorization = new AuthorizationOptions();
    }

    public AuthorizationOptions Authorization { get; private init; }

    public CORSOption CORS { get; private init; }
}

/// <summary>
///     Defines a CORS option
/// </summary>
public enum CORSOption
{
    None = 0,
    SameOrigin = 1,
    AnyOrigin = 2
}

/// <summary>
///     Defines options for handling authorization in a host
/// </summary>
public class AuthorizationOptions
{
    public bool HasNone => !UsesApiKeys && !UsesCookies && !UsesTokens && !UsesHMAC;

    public bool UsesApiKeys { get; set; }

    public bool UsesCookies { get; set; }

    public bool UsesHMAC { get; set; }

    public bool UsesTokens { get; set; }
}