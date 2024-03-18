using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;

namespace OrganizationsDomain;

public static class Events
{
    public sealed class Created : IDomainEvent
    {
        public static Created Create(Identifier id, Ownership ownership, Identifier createdBy, DisplayName name)
        {
            return new Created
            {
                Name = name,
                Ownership = ownership,
                CreatedById = createdBy,
                RootId = id,
                OccurredUtc = DateTime.UtcNow
            };
        }

        public required string CreatedById { get; set; }

        public required string Name { get; set; }

        public required Ownership Ownership { get; set; }

        public required string RootId { get; set; }

        public required DateTime OccurredUtc { get; set; }
    }

    public sealed class SettingCreated : IDomainEvent
    {
        public static SettingCreated Create(Identifier id, string name, string value, SettingValueType valueType,
            bool isEncrypted)
        {
            return new SettingCreated
            {
                RootId = id,
                Name = name,
                StringValue = value,
                ValueType = valueType,
                IsEncrypted = isEncrypted,
                OccurredUtc = DateTime.UtcNow
            };
        }

        public required bool IsEncrypted { get; set; }

        public required string Name { get; set; }

        public required string StringValue { get; set; }

        public required SettingValueType ValueType { get; set; }

        public required string RootId { get; set; }

        public required DateTime OccurredUtc { get; set; }
    }

    public sealed class SettingUpdated : IDomainEvent
    {
        public static SettingUpdated Create(Identifier id, string name, string from, SettingValueType fromType,
            string to, SettingValueType toType, bool isEncrypted)
        {
            return new SettingUpdated
            {
                RootId = id,
                Name = name,
                From = from,
                FromType = fromType,
                To = to,
                ToType = toType,
                IsEncrypted = isEncrypted,
                OccurredUtc = DateTime.UtcNow
            };
        }

        public required string From { get; set; }

        public required SettingValueType FromType { get; set; }

        public required bool IsEncrypted { get; set; }

        public required string Name { get; set; }

        public required string To { get; set; }

        public required SettingValueType ToType { get; set; }

        public required string RootId { get; set; }

        public required DateTime OccurredUtc { get; set; }
    }
}