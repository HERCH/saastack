using Common;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Shared;

namespace EndUsersDomain;

public sealed class EndUserProfile : ValueObjectBase<EndUserProfile>
{
    public static Result<EndUserProfile, Error> Create(string firstName, string? lastName = null,
        string? timezone = null,
        string? countryCode = null)
    {
        var name = PersonName.Create(firstName, lastName);
        if (!name.IsSuccessful)
        {
            return name.Error;
        }

        var tz = Timezone.Create(Timezones.FindOrDefault(timezone));
        if (!tz.IsSuccessful)
        {
            return tz.Error;
        }

        var address = Address.Create(CountryCodes.FindOrDefault(countryCode));
        if (!address.IsSuccessful)
        {
            return address.Error;
        }

        return new EndUserProfile(name.Value, tz.Value, address.Value);
    }

    private EndUserProfile(PersonName name, Timezone timezone, Address address)
    {
        Name = name;
        Timezone = timezone;
        Address = address;
    }

    public Address Address { get; }

    public PersonName Name { get; }

    public Timezone Timezone { get; }

    public static ValueObjectFactory<EndUserProfile> Rehydrate()
    {
        return (property, container) =>
        {
            var parts = RehydrateToList(property, false);
            return new EndUserProfile(PersonName.Rehydrate()(parts[0]!, container),
                Timezone.Rehydrate()(parts[1]!, container),
                Address.Rehydrate()(parts[2]!, container));
        };
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { Name, Timezone, Address };
    }
}