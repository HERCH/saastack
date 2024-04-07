﻿using Application.Persistence.Interfaces;
using Common;
using Common.Extensions;
using Domain.Common.Extensions;
using Domain.Interfaces.Entities;
using Infrastructure.Eventing.Interfaces.Notifications;

namespace Infrastructure.Eventing.Common.Notifications;

/// <summary>
///     Provides a round-robin notifier of domain events to registered consumers
/// </summary>
public sealed class EventNotificationNotifier : IEventNotificationNotifier, IDisposable
{
    private readonly IEventNotificationMessageBroker _messageBroker;
    private readonly IEventSourcedChangeEventMigrator _migrator;
    private readonly IRecorder _recorder;

    public EventNotificationNotifier(IRecorder recorder, IEventSourcedChangeEventMigrator migrator,
        List<IEventNotificationRegistration> registrations, IEventNotificationMessageBroker messageBroker)
    {
        _recorder = recorder;
        Registrations = registrations;
        _migrator = migrator;
        _messageBroker = messageBroker;
    }

    ~EventNotificationNotifier()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        if (Registrations.Any())
        {
            foreach (var pair in Registrations)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                (pair.IntegrationEventTranslator as IDisposable)?.Dispose();
            }
        }
    }

    public IReadOnlyList<IEventNotificationRegistration> Registrations { get; }

    public async Task<Result<Error>> WriteEventStreamAsync(string streamName, List<EventStreamChangeEvent> eventStream,
        CancellationToken cancellationToken)
    {
        streamName.ThrowIfNotValuedParameter(nameof(streamName));

        if (!eventStream.Any())
        {
            return Result.Ok;
        }

        if (!Registrations.Any())
        {
            return Result.Ok;
        }

        var rootAggregateType = Enumerable.First(eventStream).RootAggregateType;
        var registrations = GetRegistrationsForStream(Registrations, rootAggregateType);
        if (registrations.HasNone())
        {
            return Result.Ok;
        }

        var results = await Task.WhenAll(registrations.Select(
            registration => RelayEventStreamToAllConsumersInOrderAsync(registration, eventStream, cancellationToken)));
        if (results.Any(r => !r.IsSuccessful))
        {
            return results.First(r => !r.IsSuccessful).Error;
        }

        return Result.Ok;
    }

    private async Task<Result<Error>> RelayEventStreamToAllConsumersInOrderAsync(
        IEventNotificationRegistration registration, List<EventStreamChangeEvent> eventStream,
        CancellationToken cancellationToken)
    {
        foreach (var changeEvent in eventStream)
        {
            var deserialized = DeserializeChangeEvent(changeEvent, _migrator);
            if (!deserialized.IsSuccessful)
            {
                return deserialized.Error;
            }

            var @event = deserialized.Value;
            var domainEventsRelayed =
                await RelayDomainEventToAllConsumersAsync(registration, @event, cancellationToken);
            if (!domainEventsRelayed.IsSuccessful)
            {
                return domainEventsRelayed.Error;
            }

            var integrationEventsRelayed =
                await RelayIntegrationEventToBrokerAsync(registration, changeEvent, @event, cancellationToken);
            if (!integrationEventsRelayed.IsSuccessful)
            {
                return integrationEventsRelayed.Error;
            }
        }

        return Result.Ok;
    }

    private static async Task<Result<Error>> RelayDomainEventToAllConsumersAsync(
        IEventNotificationRegistration registration,
        IDomainEvent @event, CancellationToken cancellationToken)
    {
        if (registration.DomainEventConsumers.HasNone())
        {
            return Result.Ok;
        }

        var results = await Task.WhenAll(registration.DomainEventConsumers
            .Select(consumer => consumer.NotifyAsync(@event, cancellationToken)));
        if (results.Any(r => !r.IsSuccessful))
        {
            return results.First(r => !r.IsSuccessful).Error;
        }

        return Result.Ok;
    }

    private async Task<Result<Error>> RelayIntegrationEventToBrokerAsync(
        IEventNotificationRegistration registration, EventStreamChangeEvent changeEvent, IDomainEvent @event,
        CancellationToken cancellationToken)
    {
        var published = await registration.IntegrationEventTranslator.TranslateAsync(@event, cancellationToken);
        if (!published.IsSuccessful)
        {
            return published.Error.Wrap(Resources.EventNotificationNotifier_ProducerError.Format(
                registration.IntegrationEventTranslator.GetType().Name,
                @event, changeEvent.Metadata.Fqn));
        }

        var publishedEvent = published.Value;
        if (!publishedEvent.HasValue)
        {
            _recorder.TraceInformation(null,
                "The producer '{Producer}' chose not publish the integration event '{Event}' with event type '{Type}'",
                registration.IntegrationEventTranslator.GetType().Name, changeEvent.Id, changeEvent.Metadata.Fqn);
            return Result.Ok;
        }

        var integrationEvent = publishedEvent.Value;
        var brokered = await _messageBroker.PublishAsync(integrationEvent, cancellationToken);
        if (!brokered.IsSuccessful)
        {
            return brokered.Error;
        }

        return Result.Ok;
    }

    private static List<IEventNotificationRegistration> GetRegistrationsForStream(
        IEnumerable<IEventNotificationRegistration> registrations, string rootAggregateType)
    {
        return registrations
            .Where(prj => prj.IntegrationEventTranslator.RootAggregateType.Name == rootAggregateType)
            .ToList();
    }

    private static Result<IDomainEvent, Error> DeserializeChangeEvent(EventStreamChangeEvent changeEvent,
        IEventSourcedChangeEventMigrator migrator)
    {
        return changeEvent.Metadata.CreateEventFromJson(changeEvent.Id, changeEvent.Data, migrator);
    }
}