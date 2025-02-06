using Common.Recording;
using Infrastructure.Persistence.OnPremises.ApplicationServices;
using Infrastructure.Persistence.Interfaces;
using IntegrationTesting.Persistence.Common;
using JetBrains.Annotations;
using Xunit;

namespace Infrastructure.Persistence.Shared.IntegrationTests.RabbitMQ;

[CollectionDefinition("RabbitStorageAccount", DisableParallelization = true)]
public class RabbitStorageAccountSetup : ICollectionFixture<RabbitStorageAccountSpecSetup>;

[UsedImplicitly]
public class RabbitStorageAccountSpecSetup : StoreSpecSetupBase, IAsyncLifetime
{
    private readonly RabbitMqEmulator _rabbitMq = new();

    public IBlobStore BlobStore { get; private set; } = null!;

    public IQueueStore QueueStore { get; private set; } = null!;

    public async Task DisposeAsync()
    {
        await _rabbitMq.StopAsync();
    }

    public async Task InitializeAsync()
    {
        await _rabbitMq.StartAsync();
        var connectionString = _rabbitMq.GetConnectionString();
#if TESTINGONLY || HOSTEDONPREMISES
        QueueStore = RabbitMqQueueStore.Create(NoOpRecorder.Instance,
            RabbitMqStoreOptions.ParseConnectionString(connectionString));
        BlobStore = FileSystemBlobStore.Create(NoOpRecorder.Instance,
            FileSystemBlobStoreOptions.CustomConnectionString(connectionString));
#endif
    }
}