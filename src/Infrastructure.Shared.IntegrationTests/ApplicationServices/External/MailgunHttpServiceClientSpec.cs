using Common.Configuration;
using Common.Recording;
using FluentAssertions;
using Infrastructure.Shared.ApplicationServices.External;
using IntegrationTesting.WebApi.Common;
using Microsoft.Extensions.DependencyInjection;
using UnitTesting.Common;
using Xunit;

namespace Infrastructure.Shared.IntegrationTests.ApplicationServices.External;

[Trait("Category", "Integration.External")]
[Collection("External")]
public class MailgunHttpServiceClientSpec : ExternalApiSpec
{
    private readonly string _recipientEmail;
    private readonly string _senderEmail;
    private readonly MailgunHttpServiceClient _serviceClient;

    public MailgunHttpServiceClientSpec(ExternalApiSetup setup) : base(setup, OverrideDependencies)
    {
        var settings = setup.GetRequiredService<IConfigurationSettings>();
        _serviceClient = new MailgunHttpServiceClient(NoOpRecorder.Instance, settings, new TestHttpClientFactory());
        _recipientEmail = settings.GetString("ApplicationServices:Mailgun:TestingOnly:RecipientEmailAddress");
        _senderEmail = settings.GetString("ApplicationServices:Mailgun:TestingOnly:SenderEmailAddress");
    }

    [Fact]
    public async Task WhenSendAsync_ThenSends()
    {
        var result = await _serviceClient.SendAsync(new TestCaller(), "asubject", "<body>abody</body>",
            _recipientEmail, "arecipient", _senderEmail, "asender", CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.ReceiptId.Should().NotBeEmpty();
    }

    private static void OverrideDependencies(IServiceCollection services)
    {
        //Do nothing
    }
}