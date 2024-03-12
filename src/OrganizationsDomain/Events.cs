using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;

namespace OrganizationsDomain;

public static class Events
{
    public class Created : IDomainEvent
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

        public Ownership Ownership { get; set; }

        public required string RootId { get; set; }

        public DateTime OccurredUtc { get; set; }
    }

    public class SettingCreated : IDomainEvent
    {
        public static SettingCreated Create(Identifier id, string name, string value, bool isEncrypted)
        {
            return new SettingCreated
            {
                RootId = id,
                Name = name,
                Value = value,
                IsEncrypted = isEncrypted,
                OccurredUtc = DateTime.UtcNow
            };
        }

        public bool IsEncrypted { get; set; }

        public required string Name { get; set; }

        public required string Value { get; set; }

        public required string RootId { get; set; }

        public DateTime OccurredUtc { get; set; }
    }

    public class SettingUpdated : IDomainEvent
    {
        public static SettingUpdated Create(Identifier id, string name, string from, string to, bool isEncrypted)
        {
            return new SettingUpdated
            {
                RootId = id,
                Name = name,
                From = from,
                To = to,
                IsEncrypted = isEncrypted,
                OccurredUtc = DateTime.UtcNow
            };
        }

        public required string From { get; set; }

        public bool IsEncrypted { get; set; }

        public required string Name { get; set; }

        public required string To { get; set; }

        public required string RootId { get; set; }

        public DateTime OccurredUtc { get; set; }
    }
}