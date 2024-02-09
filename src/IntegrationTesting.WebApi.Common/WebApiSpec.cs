using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Application.Resources.Shared;
using Application.Services.Shared;
using Common.Extensions;
using FluentAssertions;
using Infrastructure.Web.Api.Operations.Shared.Identities;
using Infrastructure.Web.Api.Operations.Shared.TestingOnly;
using Infrastructure.Web.Common.Clients;
using Infrastructure.Web.Interfaces.Clients;
using IntegrationTesting.WebApi.Common.Stubs;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnitTesting.Common;
using Xunit;

namespace IntegrationTesting.WebApi.Common;

/// <summary>
///     Provides an xUnit class fixture for integration testing APIs
/// </summary>
[UsedImplicitly]
public class WebApiSetup<THost> : WebApplicationFactory<THost>
    where THost : class
{
    private Action<IServiceCollection>? _overridenTestingDependencies;
    private IServiceScope? _scope;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _scope?.Dispose();
        }

        base.Dispose(disposing);
    }

    private IConfiguration? Configuration { get; set; }

    public TInterface GetRequiredService<TInterface>()
        where TInterface : notnull
    {
        if (_scope.NotExists())
        {
            _scope = Services.GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
        }

        return _scope.ServiceProvider.GetRequiredService<TInterface>();
    }

    public void OverrideTestingDependencies(Action<IServiceCollection> overrideDependencies)
    {
        _overridenTestingDependencies = overrideDependencies;
    }

    public TInterface? TryGetService<TInterface>(Type serviceType)
    {
        if (_scope.NotExists())
        {
            _scope = Services.GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
        }

        return (TInterface?)_scope.ServiceProvider.GetService(serviceType);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .ConfigureAppConfiguration(config =>
            {
                Configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.Testing.json", true)
                    .Build();
                config.AddConfiguration(Configuration);
            })
            .ConfigureTestServices(services =>
            {
                services.AddSingleton<INotificationsService, StubNotificationsService>();
                if (_overridenTestingDependencies.Exists())
                {
                    _overridenTestingDependencies.Invoke(services);
                }
            });
    }
}

/// <summary>
///     Provides an xUnit class fixture for integration testing APIs
/// </summary>
public abstract class WebApiSpec<THost> : IClassFixture<WebApiSetup<THost>>, IDisposable
    where THost : class
{
    private const string DotNetCommandLineWithLaunchProfileArgumentsFormat =
        "run --no-build --configuration {0} --launch-profile {1} --project {2}";
    private const string WebServerBaseUrlFormat = "https://localhost:{0}/";
    protected readonly IHttpJsonClient Api;
    protected readonly HttpClient HttpApi;
    protected readonly StubNotificationsService NotificationsService;

    private readonly List<int> _additionalServerProcesses = new();
    private readonly WebApplicationFactory<THost> _setup;

    protected WebApiSpec(WebApiSetup<THost> setup, Action<IServiceCollection>? overrideDependencies = null)
    {
        if (overrideDependencies.Exists())
        {
            setup.OverrideTestingDependencies(overrideDependencies);
        }

        _setup = setup.WithWebHostBuilder(_ => { });
        var clients = CreateClients(setup);
        HttpApi = clients.HttpApi;
        Api = clients.Api;
        NotificationsService = setup.GetRequiredService<INotificationsService>().As<StubNotificationsService>();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            HttpApi.Dispose();
            _setup.Dispose();
            _additionalServerProcesses.ForEach(ShutdownProcess);
        }
    }

    private static string DotNetExe
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                return @"%ProgramFiles%\dotnet\dotnet.exe";
            }

            if (OperatingSystem.IsLinux())
            {
                return @"/usr/share/dotnet/dotnet";
            }

            if (OperatingSystem.IsMacOS())
            {
                return @"/usr/local/share/dotnet/dotnet";
            }

            throw new InvalidOperationException("Unsupported Platform");
        }
    }

    protected void EmptyAllRepositories()
    {
#if TESTINGONLY
        Api.PostAsync(new DestroyAllRepositoriesRequest()).GetAwaiter().GetResult();
#endif
    }

    protected async Task<LoginDetails> LoginUserAsync(LoginUser who = LoginUser.PersonA)
    {
        var emailAddress = who switch
        {
            LoginUser.PersonA => "person.a@company.com",
            LoginUser.PersonB => "person.b@company.com",
            LoginUser.Operator => "operator@company.com",
            _ => throw new ArgumentOutOfRangeException(nameof(who), who, null)
        };
        var firstName = who switch
        {
            LoginUser.PersonA => "aperson",
            LoginUser.PersonB => "bperson",
            LoginUser.Operator => "operator",
            _ => throw new ArgumentOutOfRangeException(nameof(who), who, null)
        };

        const string password = "1Password!";
        var person = await Api.PostAsync(new RegisterPersonPasswordRequest
        {
            EmailAddress = emailAddress,
            FirstName = firstName,
            LastName = "alastname",
            Password = password,
            TermsAndConditionsAccepted = true
        });

        var token = NotificationsService.LastRegistrationConfirmationToken;
        await Api.PostAsync(new ConfirmRegistrationPersonPasswordRequest
        {
            Token = token!
        });

        var login = await Api.PostAsync(new AuthenticatePasswordRequest
        {
            Username = emailAddress,
            Password = password
        });

        var accessToken = login.Content.Value.AccessToken!;
        var refreshToken = login.Content.Value.RefreshToken!;
        var user = person.Content.Value.Credential!.User;

        return new LoginDetails(accessToken, refreshToken, user);
    }

    protected void StartupServer<TAnotherHost>()
        where TAnotherHost : class
    {
        var assembly = typeof(TAnotherHost).Assembly;
        var projectName = assembly.GetName().Name!;
        var projectPath = Path.Combine(Solution.NavigateUpToSolutionDirectoryPath(), projectName);

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var launchProfileName = $"{projectName}-{env}";
        const string configuration = "Debug";
        var arguments =
            DotNetCommandLineWithLaunchProfileArgumentsFormat.Format(configuration, launchProfileName, projectPath);
        var executable = Environment.ExpandEnvironmentVariables(DotNetExe);
        var process = Process.Start(new ProcessStartInfo
        {
            Arguments = arguments,
            FileName = executable,
            RedirectStandardError = false,
            RedirectStandardOutput = false,
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = true
        });
        if (process.NotExists())
        {
            throw new InvalidOperationException($"Failed to launch Server {projectName}");
        }

        if (process.HasExited)
        {
            throw new InvalidOperationException($"Failed to launch Server {projectName}, failed to startup");
        }

        _additionalServerProcesses.Add(process.Id);
    }

    private static void ShutdownProcess(int processId)
    {
        if (processId != 0)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                process.Kill();
            }
            catch (ArgumentException)
            {
                //Ignore, the process does not exist
            }
        }
    }

    private static (IHttpJsonClient Api, HttpClient HttpApi) CreateClients<TAnotherHost>(WebApiSetup<TAnotherHost> host)
        where TAnotherHost : class
    {
        var httpApi = host.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            BaseAddress = new Uri(WebServerBaseUrlFormat.Format(GetNextAvailablePort()))
        });

        var jsonOptions = host.GetRequiredService<JsonSerializerOptions>();
        var api = new JsonClient(httpApi, jsonOptions);

        return (api, httpApi);
    }

    private static int GetNextAvailablePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        return port;
    }

    protected class LoginDetails
    {
        public LoginDetails(string accessToken, string refreshToken, RegisteredEndUser user)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            User = user;
        }

        public string AccessToken { get; }

        public string RefreshToken { get; set; }

        public RegisteredEndUser User { get; }
    }

    protected enum LoginUser
    {
        PersonA = 0,
        PersonB = 1,
        Operator = 2
    }
}