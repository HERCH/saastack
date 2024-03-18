extern alias NonPlatformAnalyzers;
using NonPlatformAnalyzers::Domain.Interfaces.ValueObjects;
using Xunit;
using DomainDrivenDesignAnalyzer = NonPlatformAnalyzers::Tools.Analyzers.NonPlatform.DomainDrivenDesignAnalyzer;
using DomainDrivenDesignCodeFix = NonPlatformAnalyzers::Tools.Analyzers.NonPlatform.DomainDrivenDesignCodeFix;
using UsedImplicitly = NonPlatformAnalyzers::JetBrains.Annotations.UsedImplicitlyAttribute;
using Resources = NonPlatformAnalyzers::Tools.Analyzers.NonPlatform.Resources;

namespace Tools.Analyzers.NonPlatform.UnitTests;

[UsedImplicitly]
public class DomainDrivenDesignCodeFixSpec
{
    [UsedImplicitly]
    public class GivenARootAggregate
    {
        [Trait("Category", "Unit")]
        public class GivenRuleSas030
        {
            [Fact]
            public async Task WhenFixingMissingCreateMethod_ThenAddsMethod()
            {
                const string problem = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
namespace ANamespace;
public sealed class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(ISingleValueObject<string> identifier, IDependencyContainer container, HydrationProperties rehydratingProperties) : base(identifier, container, rehydratingProperties)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(identifier, container, properties);
    }
}
public sealed class Created : IDomainEvent
{
    public static Created Create(Identifier id, Identifier organizationId)
    {
        return new Created
        {
            RootId = id,
            OrganizationId = organizationId,
            OccurredUtc = DateTime.UtcNow
        };
    }

    public required string OrganizationId { get; set; }

    public required string RootId { get; set; }

    public required DateTime OccurredUtc { get; set; }
}";
                const string fix = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
namespace ANamespace;
public sealed class AClass : AggregateRootBase
{
    public static Result<AClass, Error> Create(IRecorder recorder, IIdentifierFactory idFactory, Identifier organizationId)
    {
        var root = new AClass(recorder, idFactory);
        root.RaiseCreateEvent(Created.Create(root.Id, organizationId));
        return root;
    }
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(ISingleValueObject<string> identifier, IDependencyContainer container, HydrationProperties rehydratingProperties) : base(identifier, container, rehydratingProperties)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(identifier, container, properties);
    }
}
public sealed class Created : IDomainEvent
{
    public static Created Create(Identifier id, Identifier organizationId)
    {
        return new Created
        {
            RootId = id,
            OrganizationId = organizationId,
            OccurredUtc = DateTime.UtcNow
        };
    }

    public required string OrganizationId { get; set; }

    public required string RootId { get; set; }

    public required DateTime OccurredUtc { get; set; }
}";

                await Verify.CodeFixed<DomainDrivenDesignAnalyzer, DomainDrivenDesignCodeFix>(
                    DomainDrivenDesignAnalyzer.Sas030,
                    problem, fix, 14, 21, "AClass");
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas034
        {
            [Fact]
            public async Task WhenFixingMissingRehydrateMethodAndDehydratable_ThenAddsMethod()
            {
                const string problem = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
using QueryAny;
namespace ANamespace;
[EntityName(""AClass"")]
public sealed class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(ISingleValueObject<string> identifier, IDependencyContainer container, HydrationProperties rehydratingProperties) : base(identifier, container, rehydratingProperties)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(EventOccurred.Create());
        return root;
    }

    public override HydrationProperties Dehydrate()
    {
        return base.Dehydrate();
    }
}
public sealed class EventOccurred : IDomainEvent
{
    public static EventOccurred Create()
    {
        return new EventOccurred
        {
            RootId = ""anid"",
            OccurredUtc = DateTime.UtcNow
        };
    }

    public required string RootId { get; set; }

    public required DateTime OccurredUtc { get; set; }
}";
                const string fix = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
using QueryAny;
namespace ANamespace;
[EntityName(""AClass"")]
public sealed class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(ISingleValueObject<string> identifier, IDependencyContainer container, HydrationProperties rehydratingProperties) : base(identifier, container, rehydratingProperties)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(EventOccurred.Create());
        return root;
    }

    public override HydrationProperties Dehydrate()
    {
        return base.Dehydrate();
    }

    public static Domain.Interfaces.AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(identifier, container, properties);
    }
}
public sealed class EventOccurred : IDomainEvent
{
    public static EventOccurred Create()
    {
        return new EventOccurred
        {
            RootId = ""anid"",
            OccurredUtc = DateTime.UtcNow
        };
    }

    public required string RootId { get; set; }

    public required DateTime OccurredUtc { get; set; }
}";

                await Verify.CodeFixed<DomainDrivenDesignAnalyzer, DomainDrivenDesignCodeFix>(
                    DomainDrivenDesignAnalyzer.Sas034,
                    problem, fix, 16, 21, "AClass");
            }

            [Fact]
            public async Task WhenFixingMissingRehydrateMethodAndNotDehydratable_ThenAddsMethod()
            {
                const string problem = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.ValueObjects;
namespace ANamespace;
public sealed class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, ISingleValueObject<string> identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(EventOccurred.Create());
        return root;
    }
}
public sealed class EventOccurred : IDomainEvent
{
    public static EventOccurred Create()
    {
        return new EventOccurred
        {
            RootId = ""anid"",
            OccurredUtc = DateTime.UtcNow
        };
    }

    public required string RootId { get; set; }

    public required DateTime OccurredUtc { get; set; }
}";
                const string fix = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.ValueObjects;
namespace ANamespace;
public sealed class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, ISingleValueObject<string> identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(EventOccurred.Create());
        return root;
    }

    public static Domain.Interfaces.AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.GetRequiredService<IRecorder>(), container.GetRequiredService<IIdentifierFactory>(), identifier);
    }
}
public sealed class EventOccurred : IDomainEvent
{
    public static EventOccurred Create()
    {
        return new EventOccurred
        {
            RootId = ""anid"",
            OccurredUtc = DateTime.UtcNow
        };
    }

    public required string RootId { get; set; }

    public required DateTime OccurredUtc { get; set; }
}";

                await Verify.CodeFixed<DomainDrivenDesignAnalyzer, DomainDrivenDesignCodeFix>(
                    DomainDrivenDesignAnalyzer.Sas034,
                    problem, fix, 11, 21, "AClass");
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas035
        {
            [Fact]
            public async Task WhenFixingMissingDehydrateMethodAndDehydratable_ThenAddsMethod()
            {
                const string problem = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
using QueryAny;
namespace ANamespace;
[EntityName(""AClass"")]
public sealed class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(ISingleValueObject<string> identifier, IDependencyContainer container, HydrationProperties rehydratingProperties) : base(identifier, container, rehydratingProperties)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(EventOccurred.Create());
        return root;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(identifier, container, properties);
    }
}
public sealed class EventOccurred : IDomainEvent
{
    public static EventOccurred Create()
    {
        return new EventOccurred
        {
            RootId = ""anid"",
            OccurredUtc = DateTime.UtcNow
        };
    }

    public required string RootId { get; set; }

    public required DateTime OccurredUtc { get; set; }
}";
                const string fix = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
using QueryAny;
namespace ANamespace;
[EntityName(""AClass"")]
public sealed class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(ISingleValueObject<string> identifier, IDependencyContainer container, HydrationProperties rehydratingProperties) : base(identifier, container, rehydratingProperties)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(EventOccurred.Create());
        return root;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(identifier, container, properties);
    }

    public override HydrationProperties Dehydrate()
    {
        var properties = base.Dehydrate();
        return properties;
    }
}
public sealed class EventOccurred : IDomainEvent
{
    public static EventOccurred Create()
    {
        return new EventOccurred
        {
            RootId = ""anid"",
            OccurredUtc = DateTime.UtcNow
        };
    }

    public required string RootId { get; set; }

    public required DateTime OccurredUtc { get; set; }
}";

                await Verify.CodeFixed<DomainDrivenDesignAnalyzer, DomainDrivenDesignCodeFix>(
                    DomainDrivenDesignAnalyzer.Sas035,
                    problem, fix, 16, 21, "AClass");
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas036
        {
            [Fact]
            public async Task WhenFixingMissingEntityNameAttributeAndDehydratable_ThenAddsAttribute()
            {
                const string problem = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
using QueryAny;
namespace ANamespace;
public sealed class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(ISingleValueObject<string> identifier, IDependencyContainer container, HydrationProperties rehydratingProperties) : base(identifier, container, rehydratingProperties)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(EventOccurred.Create());
        return root;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(identifier, container, properties);
    }

    public override HydrationProperties Dehydrate()
    {
        var properties = base.Dehydrate();
        return properties;
    }
}
public sealed class EventOccurred : IDomainEvent
{
    public static EventOccurred Create()
    {
        return new EventOccurred
        {
            RootId = ""anid"",
            OccurredUtc = DateTime.UtcNow
        };
    }

    public required string RootId { get; set; }

    public required DateTime OccurredUtc { get; set; }
}";
                const string fix = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
using QueryAny;
namespace ANamespace;

[EntityNameAttribute(""AClass"")]
public sealed class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(ISingleValueObject<string> identifier, IDependencyContainer container, HydrationProperties rehydratingProperties) : base(identifier, container, rehydratingProperties)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(EventOccurred.Create());
        return root;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(identifier, container, properties);
    }

    public override HydrationProperties Dehydrate()
    {
        var properties = base.Dehydrate();
        return properties;
    }
}
public sealed class EventOccurred : IDomainEvent
{
    public static EventOccurred Create()
    {
        return new EventOccurred
        {
            RootId = ""anid"",
            OccurredUtc = DateTime.UtcNow
        };
    }

    public required string RootId { get; set; }

    public required DateTime OccurredUtc { get; set; }
}";

                await Verify.CodeFixed<DomainDrivenDesignAnalyzer, DomainDrivenDesignCodeFix>(
                    DomainDrivenDesignAnalyzer.Sas036,
                    problem, fix, 15, 21, "AClass");
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas038
        {
            [Fact]
            public async Task WhenFixingNotSealed_ThenAddsSealed()
            {
                const string problem = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
using QueryAny;
namespace ANamespace;

[EntityNameAttribute(""AClass"")]
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(ISingleValueObject<string> identifier, IDependencyContainer container, HydrationProperties rehydratingProperties) : base(identifier, container, rehydratingProperties)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(EventOccurred.Create());
        return root;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(identifier, container, properties);
    }

    public override HydrationProperties Dehydrate()
    {
        var properties = base.Dehydrate();
        return properties;
    }
}
public sealed class EventOccurred : IDomainEvent
{
    public static EventOccurred Create()
    {
        return new EventOccurred
        {
            RootId = ""anid"",
            OccurredUtc = DateTime.UtcNow
        };
    }

    public required string RootId { get; set; }

    public required DateTime OccurredUtc { get; set; }
}";
                const string fix = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
using QueryAny;
namespace ANamespace;

[EntityNameAttribute(""AClass"")]
public sealed class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(ISingleValueObject<string> identifier, IDependencyContainer container, HydrationProperties rehydratingProperties) : base(identifier, container, rehydratingProperties)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(EventOccurred.Create());
        return root;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(identifier, container, properties);
    }

    public override HydrationProperties Dehydrate()
    {
        var properties = base.Dehydrate();
        return properties;
    }
}
public sealed class EventOccurred : IDomainEvent
{
    public static EventOccurred Create()
    {
        return new EventOccurred
        {
            RootId = ""anid"",
            OccurredUtc = DateTime.UtcNow
        };
    }

    public required string RootId { get; set; }

    public required DateTime OccurredUtc { get; set; }
}";

                await Verify.CodeFixed<DomainDrivenDesignAnalyzer, DomainDrivenDesignCodeFix>(
                    DomainDrivenDesignAnalyzer.Sas038,
                    problem, fix, 17, 14, "AClass");
            }
        }
    }

    [UsedImplicitly]
    public class GivenAnEntity
    {
        [Trait("Category", "Unit")]
        public class GivenRuleSas040
        {
            [Fact]
            public async Task WhenFixingMissingCreateMethod_ThenAddsMethod()
            {
                const string problem = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
namespace ANamespace;
public sealed class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    private AClass(ISingleValueObject<string> identifier, IDependencyContainer container, HydrationProperties rehydratingProperties) : base(identifier, container, rehydratingProperties)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event)
    {
        return Result.Ok;
    }

    public override HydrationProperties Dehydrate()
    {
        return base.Dehydrate();
    }
}";
                const string fix = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
namespace ANamespace;
public sealed class AClass : EntityBase
{
    public static Result<AClass, Error> Create(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler)
    {
        return new AClass(recorder, idFactory, rootEventHandler);
    }
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    private AClass(ISingleValueObject<string> identifier, IDependencyContainer container, HydrationProperties rehydratingProperties) : base(identifier, container, rehydratingProperties)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event)
    {
        return Result.Ok;
    }

    public override HydrationProperties Dehydrate()
    {
        return base.Dehydrate();
    }
}";

                await Verify.CodeFixed<DomainDrivenDesignAnalyzer, DomainDrivenDesignCodeFix>(
                    DomainDrivenDesignAnalyzer.Sas040,
                    problem, fix, 14, 21, "AClass");
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas043
        {
            [Fact]
            public async Task WhenFixingMissingRehydrateMethodAndDehydratable_ThenAddsMethod()
            {
                const string problem = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
using QueryAny;
namespace ANamespace;
[EntityName(""AClass"")]
public sealed class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    private AClass(ISingleValueObject<string> identifier, IDependencyContainer container, HydrationProperties rehydratingProperties) : base(identifier, container, rehydratingProperties)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event)
    {
        return Result.Ok;
    }

    public static AClass Create(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler)
    {
        return new AClass(recorder, idFactory, rootEventHandler);
    }

    public override HydrationProperties Dehydrate()
    {
        return base.Dehydrate();
    }
}";
                const string fix = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
using QueryAny;
namespace ANamespace;
[EntityName(""AClass"")]
public sealed class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    private AClass(ISingleValueObject<string> identifier, IDependencyContainer container, HydrationProperties rehydratingProperties) : base(identifier, container, rehydratingProperties)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event)
    {
        return Result.Ok;
    }

    public static AClass Create(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler)
    {
        return new AClass(recorder, idFactory, rootEventHandler);
    }

    public override HydrationProperties Dehydrate()
    {
        return base.Dehydrate();
    }

    public static Domain.Interfaces.EntityFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(identifier, container, properties);
    }
}";

                await Verify.CodeFixed<DomainDrivenDesignAnalyzer, DomainDrivenDesignCodeFix>(
                    DomainDrivenDesignAnalyzer.Sas043,
                    problem, fix, 16, 21, "AClass");
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas045
        {
            [Fact]
            public async Task WhenFixingMissingEntityNameAttributeAndDehydratable_ThenAddsAttribute()
            {
                const string problem = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
using QueryAny;
namespace ANamespace;
public sealed class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    private AClass(ISingleValueObject<string> identifier, IDependencyContainer container, HydrationProperties rehydratingProperties) : base(identifier, container, rehydratingProperties)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event)
    {
        return Result.Ok;
    }

    public static AClass Create(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler)
    {
        return new AClass(recorder, idFactory, rootEventHandler);
    }

    public override HydrationProperties Dehydrate()
    {
        return base.Dehydrate();
    }

    public static EntityFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(identifier, container, properties);
    }
}";
                const string fix = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
using QueryAny;
namespace ANamespace;

[EntityNameAttribute(""AClass"")]
public sealed class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    private AClass(ISingleValueObject<string> identifier, IDependencyContainer container, HydrationProperties rehydratingProperties) : base(identifier, container, rehydratingProperties)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event)
    {
        return Result.Ok;
    }

    public static AClass Create(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler)
    {
        return new AClass(recorder, idFactory, rootEventHandler);
    }

    public override HydrationProperties Dehydrate()
    {
        return base.Dehydrate();
    }

    public static EntityFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(identifier, container, properties);
    }
}";

                await Verify.CodeFixed<DomainDrivenDesignAnalyzer, DomainDrivenDesignCodeFix>(
                    DomainDrivenDesignAnalyzer.Sas045,
                    problem, fix, 15, 21, "AClass");
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas047
        {
            [Fact]
            public async Task WhenFixingNotSealed_ThenAddsSealed()
            {
                const string problem = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
using QueryAny;
namespace ANamespace;

[EntityNameAttribute(""AClass"")]
public class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    private AClass(ISingleValueObject<string> identifier, IDependencyContainer container, HydrationProperties rehydratingProperties) : base(identifier, container, rehydratingProperties)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event)
    {
        return Result.Ok;
    }

    public static AClass Create(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler)
    {
        return new AClass(recorder, idFactory, rootEventHandler);
    }

    public override HydrationProperties Dehydrate()
    {
        return base.Dehydrate();
    }

    public static EntityFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(identifier, container, properties);
    }
}";
                const string fix = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
using QueryAny;
namespace ANamespace;

[EntityNameAttribute(""AClass"")]
public sealed class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    private AClass(ISingleValueObject<string> identifier, IDependencyContainer container, HydrationProperties rehydratingProperties) : base(identifier, container, rehydratingProperties)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event)
    {
        return Result.Ok;
    }

    public static AClass Create(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler)
    {
        return new AClass(recorder, idFactory, rootEventHandler);
    }

    public override HydrationProperties Dehydrate()
    {
        return base.Dehydrate();
    }

    public static EntityFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(identifier, container, properties);
    }
}";

                await Verify.CodeFixed<DomainDrivenDesignAnalyzer, DomainDrivenDesignCodeFix>(
                    DomainDrivenDesignAnalyzer.Sas047,
                    problem, fix, 17, 14, "AClass");
            }
        }
    }

    [UsedImplicitly]
    public class GivenAValueObject
    {
        [Trait("Category", "Unit")]
        public class GivenRuleSas050
        {
            [Fact]
            public async Task WhenFixingMissingCreateMethodAndSingleValueObject_ThenAddsMethod()
            {
                const string problem = @"
using System;
using System.Collections.Generic;
using Common;
using Common.Extensions;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
namespace ANamespace;
public sealed class AClass : SingleValueObjectBase<AClass, string>
{
    private AClass(string avalue1): base(avalue1)
    {
        AProperty = avalue1;
    }

    public string AProperty { get;}

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) =>
        {
            var parts = RehydrateToList(property, true);
            return new AClass(parts[0]);
        };
    }
}";
                const string fix = @"
using System;
using System.Collections.Generic;
using Common;
using Common.Extensions;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
namespace ANamespace;
public sealed class AClass : SingleValueObjectBase<AClass, string>
{
    public static Result<AClass, Error> Create(string value)
    {
        if (value.IsNotValuedParameter(nameof(value), out var error))
        {
            return error;
        }

        return new AClass(value);
    }
    private AClass(string avalue1): base(avalue1)
    {
        AProperty = avalue1;
    }

    public string AProperty { get;}

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) =>
        {
            var parts = RehydrateToList(property, true);
            return new AClass(parts[0]);
        };
    }
}";

                await Verify.CodeFixed<DomainDrivenDesignAnalyzer, DomainDrivenDesignCodeFix>(
                    DomainDrivenDesignAnalyzer.Sas050,
                    problem, fix, 14, 21, "AClass");
            }

            [Fact]
            public async Task WhenFixingMissingCreateMethodAndMultiValueObject_ThenAddsMethod()
            {
                const string problem = @"
using System;
using System.Collections.Generic;
using Common;
using Common.Extensions;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
namespace ANamespace;
public sealed class AClass : ValueObjectBase<AClass>
{
    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public string AProperty { get;}

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) =>
        {
            var parts = RehydrateToList(property, false);
            return null!;
        };
    }
}";
                const string fix = @"
using System;
using System.Collections.Generic;
using Common;
using Common.Extensions;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
namespace ANamespace;
public sealed class AClass : ValueObjectBase<AClass>
{
    public static Result<AClass, Error> Create(string value1, string value2, string value3)
    {
        if (value1.IsNotValuedParameter(nameof(value1), out var error1))
        {
            return error1;
        }

        return new AClass(value1, value2, value3);
    }

    private AClass(string value1, string value2, string value3)
    {
    }
    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public string AProperty { get;}

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) =>
        {
            var parts = RehydrateToList(property, false);
            return null!;
        };
    }
}";

                await Verify.CodeFixed<DomainDrivenDesignAnalyzer, DomainDrivenDesignCodeFix>(
                    DomainDrivenDesignAnalyzer.Sas050,
                    problem, fix, 14, 21, "AClass");
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas053
        {
            [Fact]
            public async Task WhenFixingMissingRehydrateMethodAndSingleValueObject_ThenAddsMethod()
            {
                const string problem = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
namespace ANamespace;
public sealed class AClass : SingleValueObjectBase<AClass, string>
{
    private AClass(string avalue1): base(avalue1)
    {
        AProperty = avalue1;
    }

    public static AClass Create()
    {
        return new AClass(null!);
    }

    public string AProperty { get;}
}";
                const string fix = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
namespace ANamespace;
public sealed class AClass : SingleValueObjectBase<AClass, string>
{
    private AClass(string avalue1): base(avalue1)
    {
        AProperty = avalue1;
    }

    public static AClass Create()
    {
        return new AClass(null!);
    }

    public string AProperty { get;}

    public static Domain.Interfaces.ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) =>
        {
            var parts = RehydrateToList(property, true);
            return new AClass(parts[0]);
        };
    }
}";

                await Verify.CodeFixed<DomainDrivenDesignAnalyzer, DomainDrivenDesignCodeFix>(
                    DomainDrivenDesignAnalyzer.Sas053,
                    problem, fix, 12, 21, "AClass");
            }

            [Fact]
            public async Task WhenFixingMissingRehydrateMethodAndMultiValueObject_ThenAddsMethod()
            {
                const string problem = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
namespace ANamespace;
public sealed class AClass : ValueObjectBase<AClass>
{
    private AClass(string avalue1, string avalue2, string avalue3)
    {
        AProperty = avalue1;
    }

    public static AClass Create()
    {
        return new AClass(null!, null!, null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public string AProperty { get;}
}";
                const string fix = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
namespace ANamespace;
public sealed class AClass : ValueObjectBase<AClass>
{
    private AClass(string avalue1, string avalue2, string avalue3)
    {
        AProperty = avalue1;
    }

    public static AClass Create()
    {
        return new AClass(null!, null!, null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public string AProperty { get;}

    public static Domain.Interfaces.ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) =>
        {
            var parts = RehydrateToList(property, false);
            return new AClass(parts[0], parts[1], parts[2]);
        };
    }
}";

                await Verify.CodeFixed<DomainDrivenDesignAnalyzer, DomainDrivenDesignCodeFix>(
                    DomainDrivenDesignAnalyzer.Sas053,
                    problem, fix, 12, 21, "AClass");
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas055
        {
            [Fact(Skip = "see: https://github.com/dotnet/roslyn/issues/72535")]
            public async Task WhenFixingWrongReturnTypeWithCorrectReturnType_ThenChangesReturnType()
            {
                const string problem = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
namespace ANamespace;
public sealed class AClass : ValueObjectBase<AClass>
{
    private AClass(string avalue1, string avalue2, string avalue3)
    {
        AProperty = avalue1;
    }

    public static AClass Create()
    {
        return new AClass(null!, null!, null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public string AProperty { get;}

    public static Domain.Interfaces.ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) =>
        {
            var parts = RehydrateToList(property, false);
            return new AClass(parts[0], parts[1], parts[2]);
        };
    }

    public void AMethod()
    {
    }
}";
                const string fix = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
namespace ANamespace;
public sealed class AClass : ValueObjectBase<AClass>
{
    private AClass(string avalue1, string avalue2, string avalue3)
    {
        AProperty = avalue1;
    }

    public static AClass Create()
    {
        return new AClass(null!, null!, null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public string AProperty { get;}

    public static Domain.Interfaces.ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) =>
        {
            var parts = RehydrateToList(property, false);
            return new AClass(parts[0], parts[1], parts[2]);
        };
    }

    public Result<AClass, Error> AMethod()
    {
        return Create();
    }
}";

                await Verify.CodeFixed<DomainDrivenDesignAnalyzer, DomainDrivenDesignCodeFix>(
                    DomainDrivenDesignAnalyzer.Sas055,
                    Resources.CodeFix_Title_AddSkipImmutabilityCheckAttributeToValueObjectMethod, problem, fix, 40,
                    17,
                    "AMethod", "ANamespace.AClass or Common.Result<ANamespace.AClass, Common.Error>",
                    nameof(SkipImmutabilityCheckAttribute));
            }

            [Fact(Skip = "see: https://github.com/dotnet/roslyn/issues/72535")]
            public async Task WhenFixingWrongReturnTypeWithAttribute_ThenAddsAttribute()
            {
                const string problem = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
namespace ANamespace;
public sealed class AClass : ValueObjectBase<AClass>
{
    private AClass(string avalue1, string avalue2, string avalue3)
    {
        AProperty = avalue1;
    }

    public static AClass Create()
    {
        return new AClass(null!, null!, null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public string AProperty { get;}

    public static Domain.Interfaces.ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) =>
        {
            var parts = RehydrateToList(property, false);
            return new AClass(parts[0], parts[1], parts[2]);
        };
    }

    public void AMethod()
    {
    }
}";
                const string fix = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
namespace ANamespace;
public sealed class AClass : ValueObjectBase<AClass>
{
    private AClass(string avalue1, string avalue2, string avalue3)
    {
        AProperty = avalue1;
    }

    public static AClass Create()
    {
        return new AClass(null!, null!, null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public string AProperty { get;}

    public static Domain.Interfaces.ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) =>
        {
            var parts = RehydrateToList(property, false);
            return new AClass(parts[0], parts[1], parts[2]);
        };
    }

    [Domain.Interfaces.ValueObjects.SkipImmutabilityCheckAttribute]
    public void AMethod()
    {
    }
}";

                await Verify.CodeFixed<DomainDrivenDesignAnalyzer, DomainDrivenDesignCodeFix>(
                    DomainDrivenDesignAnalyzer.Sas055,
                    Resources.CodeFix_Title_ChangeValueObjectMethodReturnType, problem, fix, 40,
                    17,
                    "AMethod", "ANamespace.AClass or Common.Result<ANamespace.AClass, Common.Error>",
                    nameof(SkipImmutabilityCheckAttribute));
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas056
        {
            [Fact]
            public async Task WhenFixingNotSealed_ThenAddsSealed()
            {
                const string problem = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
namespace ANamespace;
public class AClass : ValueObjectBase<AClass>
{
    private AClass(string avalue1, string avalue2, string avalue3)
    {
        AProperty = avalue1;
    }

    public static AClass Create()
    {
        return new AClass(null!, null!, null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public string AProperty { get;}

    public static Domain.Interfaces.ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) =>
        {
            var parts = RehydrateToList(property, false);
            return new AClass(parts[0], parts[1], parts[2]);
        };
    }
}";
                const string fix = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
namespace ANamespace;
public sealed class AClass : ValueObjectBase<AClass>
{
    private AClass(string avalue1, string avalue2, string avalue3)
    {
        AProperty = avalue1;
    }

    public static AClass Create()
    {
        return new AClass(null!, null!, null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public string AProperty { get;}

    public static Domain.Interfaces.ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) =>
        {
            var parts = RehydrateToList(property, false);
            return new AClass(parts[0], parts[1], parts[2]);
        };
    }
}";

                await Verify.CodeFixed<DomainDrivenDesignAnalyzer, DomainDrivenDesignCodeFix>(
                    DomainDrivenDesignAnalyzer.Sas056, problem, fix, 12, 14, "AClass");
            }
        }
    }
}