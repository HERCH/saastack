using System.Reflection;
using Application.Services.Shared;
using CarsApplication;
using CarsApplication.Persistence;
using CarsDomain;
using CarsInfrastructure.Api.Cars;
using CarsInfrastructure.ApplicationServices;
using CarsInfrastructure.Persistence;
using CarsInfrastructure.Persistence.ReadModels;
using Common;
using Domain.Interfaces;
using Infrastructure.Hosting.Common.Extensions;
using Infrastructure.Persistence.Interfaces;
using Infrastructure.Web.Hosting.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarsInfrastructure;

public class CarsModule : ISubDomainModule
{
    public Assembly ApiAssembly => typeof(CarsApi).Assembly;

    public Assembly DomainAssembly => typeof(CarRoot).Assembly;

    public Dictionary<Type, string> AggregatePrefixes => new()
    {
        { typeof(CarRoot), "car" },
        { typeof(UnavailabilityEntity), "unavail" }
    };

    public Action<WebApplication, List<MiddlewareRegistration>> ConfigureMiddleware
    {
        get { return (app, _) => app.RegisterRoutes(); }
    }

    public Action<ConfigurationManager, IServiceCollection> RegisterServices
    {
        get
        {
            return (_, services) =>
            {
                services.AddPerHttpRequest<ICarsApplication, CarsApplication.CarsApplication>();
                services.AddPerHttpRequest<ICarRepository, CarRepository>();
                services.RegisterTenantedEventing<CarRoot, CarProjection>(
                    c => new CarProjection(c.GetRequiredService<IRecorder>(), c.GetRequiredService<IDomainFactory>(),
                        c.GetRequiredService<IDataStore>())
                );

                services.AddPerHttpRequest<ICarsService, CarsInProcessServiceClient>();
            };
        }
    }
}