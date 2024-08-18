#if TESTINGONLY
using Infrastructure.Persistence.Interfaces;
using Infrastructure.Persistence.Shared.ApplicationServices;
using JetBrains.Annotations;
using Xunit;

namespace Infrastructure.Persistence.Shared.IntegrationTests;

[CollectionDefinition("InProcessInMemStore", DisableParallelization = true)]
public class AllInProcessInMemStoreSpecs : ICollectionFixture<InProcessInMemStoreSpecSetup>;

[UsedImplicitly]
public class InProcessInMemStoreSpecSetup : StoreSpecSetupBase
{
    private readonly InProcessInMemStore _store = InProcessInMemStore.Create();

    public IBlobStore BlobStore => _store;

    public IDataStore DataStore => _store;

    public IEventStore EventStore => _store;

    public IMessageBusStore MessageBusStore => _store;

    public IQueueStore QueueStore => _store;
}

[Trait("Category", "Integration.Persistence")]
[Collection("InProcessInMemStore")]
[UsedImplicitly]
public class InProcessInMemDataStoreSpec : AnyDataStoreBaseSpec
{
    public InProcessInMemDataStoreSpec(InProcessInMemStoreSpecSetup setup) : base(setup.DataStore)
    {
    }
}

[Trait("Category", "Integration.Persistence")]
[Collection("InProcessInMemStore")]
[UsedImplicitly]
public class InProcessInMemBlobStoreSpec : AnyBlobStoreBaseSpec
{
    public InProcessInMemBlobStoreSpec(InProcessInMemStoreSpecSetup setup) : base(setup.BlobStore)
    {
    }
}

[Trait("Category", "Integration.Persistence")]
[Collection("InProcessInMemStore")]
[UsedImplicitly]
public class InProcessInMemQueueStoreSpec : AnyQueueStoreBaseSpec
{
    public InProcessInMemQueueStoreSpec(InProcessInMemStoreSpecSetup setup) : base(setup.QueueStore)
    {
    }
}

[Trait("Category", "Integration.Persistence")]
[Collection("InProcessInMemStore")]
[UsedImplicitly]
public class InProcessInMemEventStoreSpec : AnyEventStoreBaseSpec
{
    public InProcessInMemEventStoreSpec(InProcessInMemStoreSpecSetup setup) : base(setup.EventStore)
    {
    }
}

[Trait("Category", "Integration.Persistence")]
[Collection("InProcessInMemStore")]
[UsedImplicitly]
public class InProcessInMemMessageBusStoreSpec : AnyMessageBusStoreBaseSpec
{
    public InProcessInMemMessageBusStoreSpec(InProcessInMemStoreSpecSetup setup) : base(setup.MessageBusStore)
    {
    }
}
#endif