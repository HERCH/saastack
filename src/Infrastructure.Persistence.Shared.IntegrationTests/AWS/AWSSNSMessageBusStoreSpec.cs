using Common;
using FluentAssertions;
using Infrastructure.Persistence.AWS;
using UnitTesting.Common.Validation;
using Xunit;

namespace Infrastructure.Persistence.Shared.IntegrationTests.AWS;

[Trait("Category", "Integration.Persistence")]
[Collection("AWSAccount")]
public class AWSSNSMessageBusStoreSpec : AnyMessageBusStoreBaseSpec
{
    private readonly AWSAccountSpecSetup _setup;

    public AWSSNSMessageBusStoreSpec(AWSAccountSpecSetup setup) : base(setup.MessageBusStore,
        setup.MessageBusStoreTestQueues[0])
    {
        _setup = setup;
    }

    [Fact]
    public async Task WhenSendWithInvalidTopicName_ThenThrows()
    {
        await _setup.MessageBusStore
            .Invoking(x => x.SendAsync("^aninvalidtopicname^", "amessage", CancellationToken.None))
            .Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessageLike(Resources.ValidationExtensions_InvalidMessageBusTopicName);
    }

    [Fact]
    public async Task WhenReceiveSingleWithInvalidTopicName_ThenThrows()
    {
#if TESTINGONLY
        await _setup.MessageBusStore
            .Invoking(x =>
                x.ReceiveSingleAsync("^aninvalidtopicname^", "asubscriptionname", (_, _) => Task.FromResult(Result.Ok),
                    CancellationToken.None))
            .Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessageLike(Resources.ValidationExtensions_InvalidMessageBusTopicName);
#endif
    }

    [Fact]
    public async Task WhenReceiveSingleWithInvalidSubscriptionName_ThenThrows()
    {
#if TESTINGONLY
        await _setup.MessageBusStore
            .Invoking(x =>
                x.ReceiveSingleAsync("atopicname", "^asubscriptionname^", (_, _) => Task.FromResult(Result.Ok),
                    CancellationToken.None))
            .Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessageLike(Resources.ValidationExtensions_InvalidMessageBusSubscriptionName);
#endif
    }

    [Fact]
    public async Task WhenCountWithInvalidTopicName_ThenThrows()
    {
#if TESTINGONLY
        await _setup.MessageBusStore
            .Invoking(x => x.CountAsync("^aninvalidtopicname^", "asubscriptionname", CancellationToken.None))
            .Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessageLike(Resources.ValidationExtensions_InvalidMessageBusTopicName);
#endif
    }

    [Fact]
    public async Task WhenDestroyAllWithInvalidTopicName_ThenThrows()
    {
#if TESTINGONLY
        await _setup.MessageBusStore
            .Invoking(x => x.DestroyAllAsync("^aninvalidtopicname^", CancellationToken.None))
            .Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessageLike(Resources.ValidationExtensions_InvalidMessageBusTopicName);
#endif
    }

    [Fact]
    public override Task WhenReceiveSingleOnTwoSubscriptions_ThenReturnsSameMessage()
    {
        // We cannot run this because we need multiple queues
        return Task.CompletedTask;
    }
}