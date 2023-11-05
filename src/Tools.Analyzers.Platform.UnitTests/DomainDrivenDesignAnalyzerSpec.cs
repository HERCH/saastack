extern alias Analyzers;
using JetBrains.Annotations;
using Xunit;
using DomainDrivenDesignAnalyzer = Analyzers::Tools.Analyzers.Platform.DomainDrivenDesignAnalyzer;

namespace Tools.Analyzers.Platform.UnitTests;

extern alias Analyzers;

[UsedImplicitly]
public class DomainDrivenDesignAnalyzerSpec
{
    [Trait("Category", "Unit")]
    public class GivenAnyRule
    {
        [Fact]
        public async Task WhenInExcludedNamespace_ThenNoAlert()
        {
            const string input = @"
using Infrastructure.Web.Api.Interfaces;
namespace Common;
public class AClass : IWebApiService
{
}";

            await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
        }

        [Fact]
        public async Task WhenNotRootAggregate_ThenNoAlert()
        {
            const string input = @"
namespace ANamespace;
public class AClass
{
}";

            await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
        }

        [Fact]
        public async Task WhenNotEntity_ThenNoAlert()
        {
            const string input = @"
namespace ANamespace;
public class AClass
{
}";

            await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
        }

        [Fact]
        public async Task WhenNotValueObject_ThenNoAlert()
        {
            const string input = @"
namespace ANamespace;
public class AClass
{
}";

            await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
        }
    }

    [UsedImplicitly]
    public class GivenARootAggregate
    {
        [Trait("Category", "Unit")]
        public class GivenRuleSas030
        {
            [Fact]
            public async Task WhenHasNoCreateMethod_ThenAlerts()
            {
                const string input = @"
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas030, input, 9,
                    14,
                    "AClass");
            }

            [Fact]
            public async Task WhenHasCreateInstanceMethod_ThenAlerts()
            {
                const string input = @"
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }

    public void Create()
    {
    }
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas030, input, 9,
                    14,
                    "AClass");
            }

            [Fact]
            public async Task WhenHasAtLeastOneCreateStaticMethod_ThenNoAlert()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(new CreateEvent());
        return root;
    }    
}
public class CreateEvent : IDomainEvent
{
    public string RootId { get; set; } = ""anid"";

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas031
        {
            [Fact]
            public async Task WhenCreateReturnsVoid_ThenAlerts()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }

    public static void Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(new CreateEvent());
    }    
}
public class CreateEvent : IDomainEvent
{
    public string RootId { get; set; } = ""anid"";

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas031, input, 31,
                    24,
                    "Create", "ANamespace.AClass, Common.Result<ANamespace.AClass, Common.Error>");
            }

            [Fact]
            public async Task WhenCreateReturnsOther_ThenAlerts()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }

    public static string Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(new CreateEvent());
        return string.Empty;
    }    
}
public class CreateEvent : IDomainEvent
{
    public string RootId { get; set; } = ""anid"";

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas031, input, 31,
                    26,
                    "Create", "ANamespace.AClass, Common.Result<ANamespace.AClass, Common.Error>");
            }

            [Fact]
            public async Task WhenCreateReturnsNakedClass_ThenNoAlert()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(new CreateEvent());
        return root;
    }    
}
public class CreateEvent : IDomainEvent
{
    public string RootId { get; set; } = ""anid"";

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }

            [Fact]
            public async Task WhenCreateReturnsResultOfClassOrError_ThenNoAlert()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }

    public static Result<AClass, Error> Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(new CreateEvent());
        return root;
    }    
}
public class CreateEvent : IDomainEvent
{
    public string RootId { get; set; } = ""anid"";

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas032
        {
            [Fact]
            public async Task WhenCreateMethodIsEmpty_ThenAlerts()
            {
                const string input = @"
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }

    public static AClass Create()
    {
        return null!;
    }    
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas032, input, 30,
                    26,
                    "Create", DomainDrivenDesignAnalyzer.ConstructorMethodCall);
            }

            [Fact]
            public async Task WhenCreateCallsRaiseEvent_ThenNoAlert()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(new CreateEvent());
        return root;
    }    
}
public class CreateEvent : IDomainEvent
{
    public string RootId { get; set; } = ""anid"";

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas033
        {
            [Fact]
            public async Task WhenNonPrivateConstructor_ThenAlerts()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : AggregateRootBase
{
    public AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(new CreateEvent());
        return root;
    }    
}
public class CreateEvent : IDomainEvent
{
    public string RootId { get; set; } = ""anid"";

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas033, input, 12,
                    12, "AClass");
            }

            [Fact]
            public async Task WhenPrivateConstructor_ThenNoAlert()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(new CreateEvent());
        return root;
    }    
}
public class CreateEvent : IDomainEvent
{
    public string RootId { get; set; } = ""anid"";

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas034
        {
            [Fact]
            public async Task WhenMissingRehydrateMethod_ThenAlerts()
            {
                const string input = @"
using System;
using Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(new CreateEvent());
        return root;
    }    
}
public class CreateEvent : IDomainEvent
{
    public string RootId { get; set; } = ""anid"";

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas034, input, 8,
                    14,
                    "AClass");
            }

            [Fact]
            public async Task WhenHasRehydrateMethod_ThenNoAlert()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using QueryAny;
namespace ANamespace;
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(new CreateEvent());
        return root;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }
}
public class CreateEvent : IDomainEvent
{
    public string RootId { get; set; } = ""anid"";

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas035
        {
            [Fact]
            public async Task WhenDehydratableAndMissingDehydrateMethod_ThenAlerts()
            {
                const string input = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using QueryAny;
namespace ANamespace;
[EntityName(""AClass"")]
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(new CreateEvent());
        return root;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }
}
public class CreateEvent : IDomainEvent
{
    public string RootId { get; set; } = ""anid"";

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas035, input, 13,
                    14,
                    "AClass");
            }

            [Fact]
            public async Task WhenDehydratableAndHasDehydrateMethod_ThenNoAlert()
            {
                const string input = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using QueryAny;
namespace ANamespace;
[EntityName(""AClass"")]
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(new CreateEvent());
        return root;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }

    public override Dictionary<string, object?> Dehydrate()
    {
        return base.Dehydrate();
    }
}
public class CreateEvent : IDomainEvent
{
    public string RootId { get; set; } = ""anid"";

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas036
        {
            [Fact]
            public async Task WhenDehydratableAndMissingEntityNameAttribute_ThenAlerts()
            {
                const string input = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using QueryAny;
namespace ANamespace;
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(new CreateEvent());
        return root;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }

    public override Dictionary<string, object?> Dehydrate()
    {
        return base.Dehydrate();
    }
}
public class CreateEvent : IDomainEvent
{
    public string RootId { get; set; } = ""anid"";

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas036, input, 12,
                    14,
                    "AClass");
            }

            [Fact]
            public async Task WhenDehydratableAndHasEntityNameAttribute_ThenNoAlert()
            {
                const string input = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using QueryAny;
namespace ANamespace;
[EntityName(""AClass"")]
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(new CreateEvent());
        return root;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }

    public override Dictionary<string, object?> Dehydrate()
    {
        return base.Dehydrate();
    }
}
public class CreateEvent : IDomainEvent
{
    public string RootId { get; set; } = ""anid"";

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas037
        {
            [Fact]
            public async Task WhenPropertyHasPublicSetter_ThenAlerts()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(new CreateEvent());
        return root;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }

    public string AProperty { get; set; }
}
public class CreateEvent : IDomainEvent
{
    public string RootId { get; set; } = ""anid"";

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas037, input, 38,
                    19, "AProperty");
            }

            [Fact]
            public async Task WhenPropertyHasPrivateSetter_ThenNoAlert()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(new CreateEvent());
        return root;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }

    public string AProperty { get; private set; }
}
public class CreateEvent : IDomainEvent
{
    public string RootId { get; set; } = ""anid"";

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }

            [Fact]
            public async Task WhenPropertyHasArrowFunction_ThenNoAlert()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(new CreateEvent());
        return root;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }

    public string AProperty => string.Empty;
}
public class CreateEvent : IDomainEvent
{
    public string RootId { get; set; } = ""anid"";

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }

            [Fact]
            public async Task WhenPropertyHasNoSetter_ThenNoAlert()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : AggregateRootBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory) : base(recorder, idFactory)
    {
    }

    private AClass(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier) : base(recorder, idFactory, identifier)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event, bool isReconstituting)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        var root = new AClass(null!, null!);
        root.RaiseCreateEvent(new CreateEvent());
        return root;
    }

    public static AggregateRootFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container.Resolve<IRecorder>(),
            container.Resolve<IIdentifierFactory>(), identifier);
    }

    public string AProperty { get; }
}
public class CreateEvent : IDomainEvent
{
    public string RootId { get; set; } = ""anid"";

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
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
            public async Task WhenHasNoCreateMethod_ThenAlerts()
            {
                const string input = @"
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event)
    {
        return Result.Ok;
    }
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas040, input, 9,
                    14, "AClass");
            }

            [Fact]
            public async Task WhenHasCreateInstanceMethod_ThenAlerts()
            {
                const string input = @"
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event)
    {
        return Result.Ok;
    }

    public void Create()
    {
    }
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas040, input, 9,
                    14,
                    "AClass");
            }

            [Fact]
            public async Task WhenHasAtLeastOneCreateStaticMethod_ThenNoAlert()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        return new AClass(null!, null!, null!);
    }    
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas041
        {
            [Fact]
            public async Task WhenCreateReturnsVoid_ThenAlerts()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event)
    {
        return Result.Ok;
    }

    public static void Create()
    {
    }    
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas041, input, 21,
                    24, "Create", "ANamespace.AClass, Common.Result<ANamespace.AClass, Common.Error>");
            }

            [Fact]
            public async Task WhenCreateReturnsOther_ThenAlerts()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event)
    {
        return Result.Ok;
    }

    public static string Create()
    {
        return string.Empty;
    }    
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas041, input, 21,
                    26, "Create", "ANamespace.AClass, Common.Result<ANamespace.AClass, Common.Error>");
            }

            [Fact]
            public async Task WhenCreateReturnsNakedClass_ThenNoAlert()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        return new AClass(null!, null!, null!);
    }    
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }

            [Fact]
            public async Task WhenCreateReturnsResultOfClassOrError_ThenNoAlert()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event)
    {
        return Result.Ok;
    }

    public static Result<AClass, Error> Create()
    {
        return new AClass(null!, null!, null!);
    }    
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas042
        {
            [Fact]
            public async Task WhenNonPrivateConstructor_ThenAlerts()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : EntityBase
{
    public AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        return new AClass(null!, null!, null!);
    }    
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas042, input, 12,
                    12, "AClass");
            }

            [Fact]
            public async Task WhenPrivateConstructor_ThenNoAlert()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    protected override Result<Error> OnStateChanged(IDomainEvent @event)
    {
        return Result.Ok;
    }

    public static AClass Create()
    {
        return new AClass(null!, null!, null!);
    }    
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas043
        {
            [Fact]
            public async Task WhenDehydratableAndMissingRehydrateMethod_ThenAlerts()
            {
                const string input = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using QueryAny;
namespace ANamespace;
[EntityName(""AClass"")]
public class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    private AClass(IDependencyContainer container, IReadOnlyDictionary<string, object?> rehydratingProperties, Identifier identifier) : base(identifier, container, rehydratingProperties)
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

    public override Dictionary<string, object?> Dehydrate()
    {
        return base.Dehydrate();
    }
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas043, input, 14,
                    14, "AClass");
            }

            [Fact]
            public async Task WhenDehydratableAndHasRehydrateMethod_ThenNoAlert()
            {
                const string input = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using QueryAny;
namespace ANamespace;
[EntityName(""AClass"")]
public class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    private AClass(IDependencyContainer container, IReadOnlyDictionary<string, object?> rehydratingProperties, Identifier identifier) : base(identifier, container, rehydratingProperties)
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

    public override Dictionary<string, object?> Dehydrate()
    {
        return base.Dehydrate();
    }

    public static EntityFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container, properties, identifier);
    }
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas044
        {
            [Fact]
            public async Task WhenDehydratableAndMissingDehydrateMethod_ThenAlerts()
            {
                const string input = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using QueryAny;
namespace ANamespace;
[EntityName(""AClass"")]
public class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    private AClass(IDependencyContainer container, IReadOnlyDictionary<string, object?> rehydratingProperties, Identifier identifier) : base(identifier, container, rehydratingProperties)
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

    public static EntityFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container, properties, identifier);
    }
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas044, input, 14,
                    14, "AClass");
            }

            [Fact]
            public async Task WhenDehydratableAndHasDehydrateMethod_ThenNoAlert()
            {
                const string input = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using QueryAny;
namespace ANamespace;
[EntityName(""AClass"")]
public class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    private AClass(IDependencyContainer container, IReadOnlyDictionary<string, object?> rehydratingProperties, Identifier identifier) : base(identifier, container, rehydratingProperties)
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

    public override Dictionary<string, object?> Dehydrate()
    {
        return base.Dehydrate();
    }

    public static EntityFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container, properties, identifier);
    }
}";
                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas045
        {
            [Fact]
            public async Task WhenDehydratableAndMissingEntityNameAttribute_ThenAlerts()
            {
                const string input = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using QueryAny;
namespace ANamespace;
public class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    private AClass(IDependencyContainer container, IReadOnlyDictionary<string, object?> rehydratingProperties, Identifier identifier) : base(identifier, container, rehydratingProperties)
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

    public override Dictionary<string, object?> Dehydrate()
    {
        return base.Dehydrate();
    }

    public static EntityFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container, properties, identifier);
    }
}";
                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas045, input, 13,
                    14,
                    "AClass");
            }

            [Fact]
            public async Task WhenDehydratableAndHasEntityNameAttribute_ThenNoAlert()
            {
                const string input = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using QueryAny;
namespace ANamespace;
[EntityName(""AClass"")]
public class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
    {
    }

    private AClass(IDependencyContainer container, IReadOnlyDictionary<string, object?> rehydratingProperties, Identifier identifier) : base(identifier, container, rehydratingProperties)
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

    public override Dictionary<string, object?> Dehydrate()
    {
        return base.Dehydrate();
    }

    public static EntityFactory<AClass> Rehydrate()
    {
        return (identifier, container, properties) => new AClass(container, properties, identifier);
    }
}";
                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas046
        {
            [Fact]
            public async Task WhenPropertyHasPublicSetter_ThenAlerts()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
namespace ANamespace;
public class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
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

    public string AProperty { get;set; }
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas046, input, 27,
                    19, "AProperty");
            }

            [Fact]
            public async Task WhenPropertyHasPrivateSetter_ThenNoAlert()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
namespace ANamespace;
public class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
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

    public string AProperty { get; private set; }
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }

            [Fact]
            public async Task WhenPropertyHasArrowFunction_ThenNoAlert()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
namespace ANamespace;
public class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
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

    public string AProperty => string.Empty;
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }

            [Fact]
            public async Task WhenPropertyHasNoSetter_ThenNoAlert()
            {
                const string input = @"
using System;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
namespace ANamespace;
public class AClass : EntityBase
{
    private AClass(IRecorder recorder, IIdentifierFactory idFactory, RootEventHandler rootEventHandler) : base(recorder, idFactory, rootEventHandler)
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

    public string AProperty { get; }
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
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
            public async Task WhenHasNoCreateMethod_ThenAlerts()
            {
                const string input = @"
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : ValueObjectBase<AClass>
{
    private AClass(string avalue)
    {
        AProperty = avalue;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty { get;}
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas050, input, 10,
                    14, "AClass");
            }

            [Fact]
            public async Task WhenHasCreateInstanceMethod_ThenAlerts()
            {
                const string input = @"
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : ValueObjectBase<AClass>
{
    private AClass(string avalue)
    {
        AProperty = avalue;
    }

    public AClass Create()
    {
        return null!;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty { get;}
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas050, input, 10,
                    14, "AClass");
            }

            [Fact]
            public async Task WhenHasAtLeastOneCreateStaticMethod_ThenNoAlert()
            {
                const string input = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : ValueObjectBase<AClass>
{
    private AClass(string avalue)
    {
        AProperty = avalue;
    }

    public static AClass Create()
    {
        return new AClass(null!);
    }    

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }
    
    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty { get;}
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas051
        {
            [Fact]
            public async Task WhenCreateReturnsVoid_ThenAlerts()
            {
                const string input = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : ValueObjectBase<AClass>
{
    private AClass(string avalue)
    {
        AProperty = avalue;
    }

    public static void Create()
    {
    }    

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty { get;}
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas051, input, 18,
                    24, "Create", "ANamespace.AClass, Common.Result<ANamespace.AClass, Common.Error>");
            }

            [Fact]
            public async Task WhenCreateReturnsOther_ThenAlerts()
            {
                const string input = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : ValueObjectBase<AClass>
{
    private AClass(string avalue)
    {
        AProperty = avalue;
    }

    public static string Create()
    {
        return string.Empty;
    }    

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty { get;}
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas051, input, 18,
                    26, "Create", "ANamespace.AClass, Common.Result<ANamespace.AClass, Common.Error>");
            }

            [Fact]
            public async Task WhenCreateReturnsNakedClass_ThenNoAlert()
            {
                const string input = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : ValueObjectBase<AClass>
{
    private AClass(string avalue)
    {
        AProperty = avalue;
    }

    public static AClass Create()
    {
        return new AClass(null!);
    }    

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty { get;}
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }

            [Fact]
            public async Task WhenCreateReturnsResultOfClassOrError_ThenNoAlert()
            {
                const string input = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : ValueObjectBase<AClass>
{
    private AClass(string avalue)
    {
        AProperty = avalue;
    }

    public static Result<AClass, Error> Create()
    {
        return new AClass(null!);
    }    

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty { get;}
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas052
        {
            [Fact]
            public async Task WhenNonPrivateConstructor_ThenAlerts()
            {
                const string input = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : ValueObjectBase<AClass>
{
    public AClass(string avalue)
    {
        AProperty = avalue;
    }

    public static AClass Create()
    {
        return new AClass(null!);
    }    

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty { get;}
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas052, input, 13,
                    12, "AClass");
            }

            [Fact]
            public async Task WhenPrivateConstructor_ThenNoAlert()
            {
                const string input = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
namespace ANamespace;
public class AClass : ValueObjectBase<AClass>
{
    private AClass(string avalue)
    {
        AProperty = avalue;
    }

    public static AClass Create()
    {
        return new AClass(null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty { get;}
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas053
        {
            [Fact]
            public async Task WhenMissingRehydrateMethod_ThenAlerts()
            {
                const string input = @"
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
    private AClass(string avalue)
    {
        AProperty = avalue;
    }

    public static AClass Create()
    {
        return new AClass(null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }
    
    public string AProperty { get;}
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas053, input, 12,
                    14, "AClass");
            }

            [Fact]
            public async Task WhenHasRehydrateMethod_ThenNoAlert()
            {
                const string input = @"
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
    private AClass(string avalue)
    {
        AProperty = avalue;
    }

    public static AClass Create()
    {
        return new AClass(null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty { get;}
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas054
        {
            [Fact]
            public async Task WhenPropertyHasPublicSetter_ThenAlerts()
            {
                const string input = @"
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
    private AClass(string avalue)
    {
        AProperty = avalue;
    }

    public static AClass Create()
    {
        return new AClass(null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty { get;set; }
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas054, input, 34,
                    19, "AProperty");
            }

            [Fact]
            public async Task WhenPropertyHasPrivateSetter_ThenNoAlert()
            {
                const string input = @"
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
    private AClass(string avalue)
    {
        AProperty = avalue;
    }

    public static AClass Create()
    {
        return new AClass(null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty { get; private set; }
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }

            [Fact]
            public async Task WhenPropertyHasArrowFunction_ThenNoAlert()
            {
                const string input = @"
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
    private AClass(string avalue)
    {
    }

    public static AClass Create()
    {
        return new AClass(null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty => string.Empty;
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }

            [Fact]
            public async Task WhenPropertyHasNoSetter_ThenNoAlert()
            {
                const string input = @"
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
    private AClass(string avalue)
    {
        AProperty = avalue;
    }

    public static AClass Create()
    {
        return new AClass(null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty { get; }
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }
        }

        [Trait("Category", "Unit")]
        public class GivenRuleSas055
        {
            [Fact]
            public async Task WhenMethodReturnsVoid_ThenAlerts()
            {
                const string input = @"
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
    private AClass(string avalue)
    {
        AProperty = avalue;
    }

    public static AClass Create()
    {
        return new AClass(null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty { get; }

    public void AMethod()
    {
    }
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas055, input, 36,
                    17, "AMethod",
                    "ANamespace.AClass, Common.Result<ANamespace.AClass, Common.Error>");
            }

            [Fact]
            public async Task WhenMethodReturnsOther_ThenAlerts()
            {
                const string input = @"
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
    private AClass(string avalue)
    {
        AProperty = avalue;
    }

    public static AClass Create()
    {
        return new AClass(null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty { get; }

    public string AMethod()
    {
        return string.Empty;
    }
}";

                await Verify.DiagnosticExists<DomainDrivenDesignAnalyzer>(DomainDrivenDesignAnalyzer.Sas055, input, 36,
                    19, "AMethod",
                    "ANamespace.AClass, Common.Result<ANamespace.AClass, Common.Error>");
            }

            [Fact]
            public async Task WhenMethodReturnsVoidAndSkipped_ThenNoAlert()
            {
                const string input = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
namespace ANamespace;
public class AClass : ValueObjectBase<AClass>
{
    private AClass(string avalue)
    {
        AProperty = avalue;
    }

    public static AClass Create()
    {
        return new AClass(null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty { get; }

    [SkipImmutabilityCheck]
    public void AMethod()
    {
    }
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }

            [Fact]
            public async Task WhenMethodReturnsOtherAndSkipped_ThenNoAlert()
            {
                const string input = @"
using System;
using System.Collections.Generic;
using Common;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Identity;
using Domain.Common.ValueObjects;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Services;
using Domain.Interfaces.ValueObjects;
namespace ANamespace;
public class AClass : ValueObjectBase<AClass>
{
    private AClass(string avalue)
    {
        AProperty = avalue;
    }

    public static AClass Create()
    {
        return new AClass(null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty { get; }

    [SkipImmutabilityCheck]
    public string AMethod()
    {
        return string.Empty;
    }
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }

            [Fact]
            public async Task WhenMethodReturnsInstance_ThenNoAlert()
            {
                const string input = @"
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
    private AClass(string avalue)
    {
        AProperty = avalue;
    }

    public static AClass Create()
    {
        return new AClass(null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty { get; }

    public AClass AMethod()
    {
        return null!;
    }
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }

            [Fact]
            public async Task WhenMethodReturnsResultOfInstance_ThenNoAlert()
            {
                const string input = @"
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
    private AClass(string avalue)
    {
        AProperty = avalue;
    }

    public static AClass Create()
    {
        return new AClass(null!);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        return new object[] { AProperty };
    }

    public static ValueObjectFactory<AClass> Rehydrate()
    {
        return (property, container) => new AClass(null!);
    }

    public string AProperty { get; }

    public Result<AClass, Error> AMethod()
    {
        return null!;
    }
}";

                await Verify.NoDiagnosticExists<DomainDrivenDesignAnalyzer>(input);
            }
        }
    }
}