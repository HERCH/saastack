using Application.Persistence.Common.Extensions;
using Application.Persistence.Interfaces;
using CarsApplication.Persistence.ReadModels;
using CarsDomain;
using Common;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Infrastructure.Persistence.Common;
using Infrastructure.Persistence.Interfaces;

namespace CarsInfrastructure.Persistence.ReadModels;

public class CarProjection : IReadModelProjection
{
    private readonly IReadModelStore<Car> _cars;
    private readonly IReadModelStore<Unavailability> _unavailabilities;

    public CarProjection(IRecorder recorder, IDomainFactory domainFactory, IDataStore store)
    {
        _cars = new ReadModelStore<Car>(recorder, domainFactory, store);
        _unavailabilities = new ReadModelStore<Unavailability>(recorder, domainFactory, store);
    }

    public Type RootAggregateType => typeof(CarRoot);

    public async Task<Result<bool, Error>> ProjectEventAsync(IDomainEvent changeEvent,
        CancellationToken cancellationToken)
    {
        switch (changeEvent)
        {
            case CarsDomain.Events.Car.Created e:
                return await _cars.HandleCreateAsync(e.RootId.ToId(), dto =>
                {
                    dto.OrganizationId = e.OrganizationId;
                    dto.Status = e.Status;
                }, cancellationToken);
            case CarsDomain.Events.Car.ManufacturerChanged e:
                return await _cars.HandleUpdateAsync(e.RootId, dto =>
                {
                    dto.ManufactureYear = e.Year;
                    dto.ManufactureMake = e.Make;
                    dto.ManufactureModel = e.Model;
                }, cancellationToken);
            case CarsDomain.Events.Car.OwnershipChanged e:
                return await _cars.HandleUpdateAsync(e.RootId, dto =>
                {
                    dto.VehicleOwnerId = e.Owner;
                    dto.ManagerIds = VehicleManagers.Create(e.Owner).Value;
                }, cancellationToken);
            case CarsDomain.Events.Car.RegistrationChanged e:
                return await _cars.HandleUpdateAsync(e.RootId, dto =>
                {
                    dto.LicenseJurisdiction = e.Jurisdiction;
                    dto.LicenseNumber = e.Number;
                    dto.Status = e.Status;
                }, cancellationToken);
            case CarsDomain.Events.Car.UnavailabilitySlotAdded e:
                return await _unavailabilities.HandleCreateAsync(e.UnavailabilityId!, dto =>
                {
                    dto.OrganizationId = e.OrganizationId;
                    dto.CarId = e.RootId;
                    dto.From = e.From;
                    dto.To = e.To;
                    dto.CausedBy = e.CausedByReason;
                    dto.CausedByReference = e.CausedByReference!;
                }, cancellationToken);
            case CarsDomain.Events.Car.UnavailabilitySlotRemoved e:
                return await _unavailabilities.HandleDeleteAsync(e.UnavailabilityId, cancellationToken);

            default:
                return false;
        }
    }
}