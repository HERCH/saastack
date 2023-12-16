using Infrastructure.Common.Recording;

namespace Infrastructure.Web.Hosting.Common;

/// <summary>
///     Defines options for different web hosts
/// </summary>
public class WebHostOptions
{
    public static readonly WebHostOptions BackEndApiHost = new("BackendAPI")
    {
        DefaultApiPath = string.Empty,
        AllowCors = true,
        IsMultiTenanted = false, //TODO: change for multi-tenanted
        UsesQueues = true,
        UsesEventing = true,
        Recording = RecorderOptions.BackEndApiHost
    };

    public static readonly WebHostOptions BackEndForFrontEndWebHost = new("FrontendSite")
    {
        DefaultApiPath = "api",
        AllowCors = true,
        IsMultiTenanted = false, //TODO: change for multi-tenanted
        UsesQueues = true,
        UsesEventing = false,
        Recording = RecorderOptions.BackEndForFrontEndWebHost
    };

    public static readonly WebHostOptions TestingStubsHost = new("TestingStubs")
    {
        DefaultApiPath = string.Empty,
        AllowCors = true,
        IsMultiTenanted = false, //TODO: change for multi-tenanted
        UsesQueues = false,
        UsesEventing = false,
        Recording = RecorderOptions.TestingStubsHost
    };

    private WebHostOptions(string hostName)
    {
        HostName = hostName;
        DefaultApiPath = string.Empty;
        AllowCors = true;
        IsMultiTenanted = false;
        UsesQueues = false;
        UsesEventing = false;
        Recording = new RecorderOptions();
    }

    public bool AllowCors { get; private init; }

    public string DefaultApiPath { get; private init; }

    public string HostName { get; private init; }

    public bool IsMultiTenanted { get; private init; }

    public RecorderOptions Recording { get; private init; }

    public bool UsesEventing { get; private init; }

    public bool UsesQueues { get; private init; }
}