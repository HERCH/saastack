using System.Reflection;
using AncillaryApplication;
using AncillaryApplication.Persistence;
using AncillaryDomain;
using AncillaryInfrastructure.Api.Usages;
using AncillaryInfrastructure.ApplicationServices;
using AncillaryInfrastructure.Persistence;
using AncillaryInfrastructure.Persistence.ReadModels;
using Application.Persistence.Interfaces;
using Application.Persistence.Shared;
using Common;
using Domain.Interfaces;
using Infrastructure.Hosting.Common.Extensions;
using Infrastructure.Persistence.Interfaces;
using Infrastructure.Persistence.Shared.ApplicationServices;
using Infrastructure.Web.Hosting.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AncillaryInfrastructure;

public class AncillaryModule : ISubDomainModule
{
    public Assembly ApiAssembly => typeof(UsagesApi).Assembly;

    public Assembly DomainAssembly => typeof(AuditRoot).Assembly;

    public Dictionary<Type, string> AggregatePrefixes => new()
    {
        { typeof(AuditRoot), "audit" },
        { typeof(EmailDeliveryRoot), "emaildelivery" }
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
                services.RegisterUnshared<IRecordingApplication, RecordingApplication>();
                services.RegisterUnshared<IFeatureFlagsApplication, FeatureFlagsApplication>();
                services.RegisterUnshared<IAncillaryApplication, AncillaryApplication.AncillaryApplication>();
                services.RegisterUnshared<IUsageMessageQueue>(c =>
                    new UsageMessageQueue(c.Resolve<IRecorder>(), c.Resolve<IMessageQueueIdFactory>(),
                        c.ResolveForPlatform<IQueueStore>()));
                services.RegisterUnshared<IAuditMessageQueueRepository>(c =>
                    new AuditMessageQueueRepository(c.Resolve<IRecorder>(), c.Resolve<IMessageQueueIdFactory>(),
                        c.ResolveForPlatform<IQueueStore>()));
                services.RegisterUnshared<IEmailMessageQueue>(c =>
                    new EmailMessageQueue(c.Resolve<IRecorder>(), c.Resolve<IMessageQueueIdFactory>(),
                        c.ResolveForPlatform<IQueueStore>()));
                services.RegisterUnshared<IAuditRepository>(c => new AuditRepository(c.ResolveForUnshared<IRecorder>(),
                    c.ResolveForUnshared<IDomainFactory>(),
                    c.ResolveForUnshared<IEventSourcingDddCommandStore<AuditRoot>>(),
                    c.ResolveForPlatform<IDataStore>()));
                services.RegisterUnTenantedEventing<AuditRoot, AuditProjection>(
                    c => new AuditProjection(c.ResolveForUnshared<IRecorder>(), c.ResolveForUnshared<IDomainFactory>(),
                        c.ResolveForPlatform<IDataStore>()));
                services.RegisterUnshared<IEmailDeliveryRepository>(c => new EmailDeliveryRepository(
                    c.ResolveForUnshared<IRecorder>(),
                    c.ResolveForUnshared<IDomainFactory>(),
                    c.ResolveForUnshared<IEventSourcingDddCommandStore<EmailDeliveryRoot>>(),
                    c.ResolveForPlatform<IDataStore>()));
                services.RegisterUnTenantedEventing<EmailDeliveryRoot, EmailDeliveryProjection>(
                    c => new EmailDeliveryProjection(c.ResolveForUnshared<IRecorder>(),
                        c.ResolveForUnshared<IDomainFactory>(),
                        c.ResolveForPlatform<IDataStore>()));
                services.RegisterUnshared<IProvisioningMessageQueue>(c =>
                    new ProvisioningMessageQueue(c.Resolve<IRecorder>(), c.Resolve<IMessageQueueIdFactory>(),
                        c.ResolveForPlatform<IQueueStore>()));

                services.RegisterUnshared<IUsageDeliveryService, NullUsageDeliveryService>();
                services.RegisterUnshared<IEmailDeliveryService, NullEmailDeliveryService>();
                services.RegisterUnshared<IProvisioningDeliveryService, OrganizationProvisioningDeliveryService>();
            };
        }
    }
}