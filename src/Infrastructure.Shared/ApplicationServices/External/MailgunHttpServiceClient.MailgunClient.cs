using System.Text;
using System.Text.Json;
using Application.Common;
using Application.Persistence.Shared;
using Common;
using Common.Configuration;
using Common.Extensions;
using Infrastructure.Web.Api.Interfaces;
using Infrastructure.Web.Api.Operations.Shared._3rdParties.Mailgun;
using Infrastructure.Web.Common.Clients;
using Infrastructure.Web.Common.Extensions;
using Infrastructure.Web.Interfaces.Clients;
using Polly;

namespace Infrastructure.Shared.ApplicationServices.External;

public interface IMailgunClient
{
    Task<Result<EmailDeliveryReceipt, Error>> SendHtmlAsync(ICallContext call, string subject, string from,
        string? fromDisplayName, MailGunRecipient to, string htmlMessage, CancellationToken cancellationToken);

    Task<Result<EmailDeliveryReceipt, Error>> SendTemplatedAsync(ICallContext call, string templateId, string? subject,
        string from,
        string? fromDisplayName, MailGunRecipient to,
        Dictionary<string, string> substitutions, CancellationToken cancellationToken);
}

public class MailGunRecipient
{
    public required string? DisplayName { get; init; }

    public required string EmailAddress { get; init; }

    public KeyValuePair<string, Dictionary<string, object>>? ToVariable(int index = 1)
    {
        if (DisplayName.HasNoValue())
        {
            return null;
        }

        return new KeyValuePair<string, Dictionary<string, object>>(EmailAddress, new Dictionary<string, object>
        {
            { "Name", DisplayName },
            { "Id", index }
        });
    }
}

/// <summary>
///     Provides a client for sending emails via the Mailgun API.
///     <see href="https://documentation.mailgun.com/docs/mailgun/api-reference/openapi-final/tag/Messages/" />
/// </summary>
public class MailgunClient : IMailgunClient
{
    private readonly string _apiKey;
    private readonly string _domainName;
    private readonly IRecorder _recorder;
    private readonly IAsyncPolicy _retryPolicy;
    private readonly IServiceClient _serviceClient;

    public MailgunClient(IRecorder recorder, IConfigurationSettings settings, IHttpClientFactory httpClientFactory)
        : this(recorder, settings.GetString(Constants.BaseUrlSettingName),
            settings.GetString(Constants.APIKeySettingName),
            settings.GetString(Constants.DomainNameSettingName),
            ApiClientRetryPolicies.CreateRetryWithExponentialBackoffAndJitter(), httpClientFactory)
    {
    }

    internal MailgunClient(IRecorder recorder, IServiceClient serviceClient, IAsyncPolicy retryPolicy, string apiKey,
        string domainName)
    {
        _recorder = recorder;
        _serviceClient = serviceClient;
        _retryPolicy = retryPolicy;
        _apiKey = apiKey;
        _domainName = domainName;
    }

    private MailgunClient(IRecorder recorder, string baseUrl, string apiKey, string domainName,
        IAsyncPolicy retryPolicy,
        IHttpClientFactory httpClientFactory) : this(recorder,
        new ApiServiceClient(httpClientFactory, JsonSerializerOptions.Default, baseUrl), retryPolicy, apiKey,
        domainName)
    {
    }

    public async Task<Result<EmailDeliveryReceipt, Error>> SendHtmlAsync(ICallContext call, string subject, string from,
        string? fromDisplayName, MailGunRecipient to, string htmlMessage, CancellationToken cancellationToken)
    {
        var recipient = to.ToVariable();
        var recipientVariables = recipient.Exists()
            ? new Dictionary<string, Dictionary<string, object>>
                {
                    { recipient.Value.Key, recipient.Value.Value }
                }
                .ToJson(casing: StringExtensions.JsonCasing.Camel)
            : null;
        var sender = fromDisplayName.HasValue()
            ? $"{fromDisplayName} <{from}>"
            : from;

        var caller = Caller.CreateAsCallerFromCall(call);
        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () => await _serviceClient.PostAsync(caller,
                new MailgunSendMessageRequest
                {
                    DomainName = _domainName,
                    From = sender,
                    To = to.EmailAddress,
                    Subject = subject,
                    Html = htmlMessage,
                    RecipientVariables = recipientVariables,
#if TESTINGONLY
                    TestingOnly = "yes",
#else
                    TestingOnly = "no",
#endif
                    Tracking = "no"
                }, req => PrepareRequest(req, _apiKey), cancellationToken));
            if (response.IsFailure)
            {
                return response.Error.ToError();
            }

            return new EmailDeliveryReceipt
            {
                ReceiptId = response.Value.Id ?? string.Empty.TrimStart('<').TrimEnd('>')
            };
        }
        catch (HttpRequestException ex)
        {
            _recorder.TraceError(call, ex, "Error sending Mailgun HTML email to {To}", to);
            return ex.ToError(ErrorCode.Unexpected);
        }
    }

    public async Task<Result<EmailDeliveryReceipt, Error>> SendTemplatedAsync(ICallContext call, string templateId,
        string? subject,
        string from, string? fromDisplayName, MailGunRecipient to, Dictionary<string, string> substitutions,
        CancellationToken cancellationToken)
    {
        var recipient = to.ToVariable();
        var recipients = recipient.Exists()
            ? new Dictionary<string, Dictionary<string, object>> { { recipient.Value.Key, recipient.Value.Value } }
                .ToJson(casing: StringExtensions.JsonCasing.Camel)
            : null;
        var sender = fromDisplayName.HasValue()
            ? $"{fromDisplayName} <{from}>"
            : from;
        var variables = substitutions.HasAny()
            ? substitutions.ToDictionary(pair => pair.Key, pair => pair.Value)
                .ToJson(casing: StringExtensions.JsonCasing.Camel)
            : null;

        var caller = Caller.CreateAsCallerFromCall(call);
        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () => await _serviceClient.PostAsync(caller,
                new MailgunSendMessageRequest
                {
                    DomainName = _domainName,
                    From = sender,
                    To = to.EmailAddress,
                    Subject = subject,
                    Template = templateId,
                    TemplateVariables = variables,
                    RecipientVariables = recipients,
#if TESTINGONLY
                    TestingOnly = "yes",
#else
                    TestingOnly = "no",
#endif
                    Tracking = "no"
                }, req => PrepareRequest(req, _apiKey), cancellationToken));
            if (response.IsFailure)
            {
                return response.Error.ToError();
            }

            return new EmailDeliveryReceipt
            {
                ReceiptId = response.Value.Id ?? string.Empty.TrimStart('<').TrimEnd('>')
            };
        }
        catch (HttpRequestException ex)
        {
            _recorder.TraceError(call, ex, "Error sending Mailgun templated email to {To} with template {Template}", to,
                templateId);
            return ex.ToError(ErrorCode.Unexpected);
        }
    }

    private static void PrepareRequest(HttpRequestMessage message, string apiKey)
    {
        message.Headers.Add(HttpConstants.Headers.Authorization,
            $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{apiKey}"))}");
    }

    public static class Constants
    {
        public const string APIKeySettingName = "ApplicationServices:Mailgun:ApiKey";
        public const string BaseUrlSettingName = "ApplicationServices:Mailgun:BaseUrl";
        public const string DomainNameSettingName = "ApplicationServices:Mailgun:DomainName";
        public const string WebhookSigningKeySettingName = "ApplicationServices:Mailgun:WebhookSigningKey";
    }
}