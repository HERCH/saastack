#if TESTINGONLY
using System.Reflection;
using ApiHost1.Api.TestingOnly;
using Infrastructure.Web.Hosting.Common;

namespace ApiHost1;

public class TestingOnlyApiModule : ISubDomainModule
{
    public Assembly ApiAssembly => typeof(TestingWebApi).Assembly;

    public Assembly DomainAssembly => null!;

    public Dictionary<Type, string> AggregatePrefixes => new();

    public Action<WebApplication, List<MiddlewareRegistration>> ConfigureMiddleware
    {
        get { return (app, _) => app.RegisterRoutes(); }
    }

    public Action<ConfigurationManager, IServiceCollection> RegisterServices
    {
        get { return (_, _) => { }; }
    }
}
#endif