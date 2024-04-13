using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Common;
using Common.Extensions;
using Infrastructure.Web.Api.Common;
using Infrastructure.Web.Api.Common.Extensions;
using Infrastructure.Web.Api.Interfaces;
using Infrastructure.Web.Common.Extensions;
using Infrastructure.Web.Interfaces.Clients;
using Microsoft.AspNetCore.Mvc;
using HttpHeaders = Infrastructure.Web.Api.Common.HttpHeaders;
using JsonException = System.Text.Json.JsonException;
using Task = System.Threading.Tasks.Task;

namespace Infrastructure.Web.Common.Clients;

/// <summary>
///     Provides a convenient typed <see cref="HttpClient" /> that accepts and returns JSON
/// </summary>
[ExcludeFromCodeCoverage]
public class JsonClient : IHttpJsonClient, IDisposable
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonClient(IHttpClientFactory clientFactory, JsonSerializerOptions jsonOptions) : this(
        clientFactory.CreateClient(), jsonOptions)
    {
    }

    public JsonClient(HttpClient client, JsonSerializerOptions jsonOptions)
    {
        _client = client;
        _jsonOptions = jsonOptions;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _client.Dispose();
        }
    }

    public async Task<JsonResponse> DeleteAsync<TResponse>(IWebRequest<TResponse> request,
        Action<HttpRequestMessage>? requestFilter = null, CancellationToken? cancellationToken = default)
        where TResponse : IWebResponse, new()
    {
        var response = await SendRequestAsync(HttpMethod.Delete, request, null, requestFilter, cancellationToken);
        var content = await GetStringResponseAsync(response, _jsonOptions, cancellationToken);

        return CreateResponse(response, content);
    }

    public async Task<JsonResponse> DeleteAsync(IWebRequest request,
        Action<HttpRequestMessage>? requestFilter = null, CancellationToken? cancellationToken = default)
    {
        var response = await SendRequestAsync(HttpMethod.Delete, request, null, requestFilter, cancellationToken);
        var content = await GetStringResponseAsync(response, _jsonOptions, cancellationToken);

        return CreateResponse(response, content);
    }

    public async Task<JsonResponse<TResponse>> GetAsync<TResponse>(IWebRequest<TResponse> request,
        Action<HttpRequestMessage>? requestFilter = null,
        CancellationToken? cancellationToken = default)
        where TResponse : IWebResponse, new()
    {
        var response = await SendRequestAsync(HttpMethod.Get, request, null, requestFilter, cancellationToken);
        var content = await GetTypedResponseAsync<TResponse>(response, _jsonOptions, cancellationToken);

        return CreateResponse(response, content);
    }

    public async Task<JsonResponse> GetAsync(IWebRequest request,
        Action<HttpRequestMessage>? requestFilter = null,
        CancellationToken? cancellationToken = default)
    {
        var response = await SendRequestAsync(HttpMethod.Get, request, null, requestFilter, cancellationToken);
        var content = await GetStringResponseAsync(response, _jsonOptions, cancellationToken);

        return CreateResponse(response, content);
    }

    public async Task<JsonResponse<TResponse>> PatchAsync<TResponse>(IWebRequest<TResponse> request,
        Action<HttpRequestMessage>? requestFilter = null,
        CancellationToken? cancellationToken = default)
        where TResponse : IWebResponse, new()
    {
        var response = await SendRequestAsync(HttpMethod.Patch, request, null, requestFilter, cancellationToken);
        var content = await GetTypedResponseAsync<TResponse>(response, _jsonOptions, cancellationToken);

        return CreateResponse(response, content);
    }

    public async Task<JsonResponse> PatchAsync(IWebRequest request,
        Action<HttpRequestMessage>? requestFilter = null,
        CancellationToken? cancellationToken = default)
    {
        var response = await SendRequestAsync(HttpMethod.Patch, request, null, requestFilter, cancellationToken);
        var content = await GetStringResponseAsync(response, _jsonOptions, cancellationToken);

        return CreateResponse(response, content);
    }

    public async Task<JsonResponse<TResponse>> PostAsync<TResponse>(IWebRequest<TResponse> request,
        Action<HttpRequestMessage>? requestFilter = null,
        CancellationToken? cancellationToken = default)
        where TResponse : IWebResponse, new()
    {
        var response = await SendRequestAsync(HttpMethod.Post, request, null, requestFilter, cancellationToken);
        var content = await GetTypedResponseAsync<TResponse>(response, _jsonOptions, cancellationToken);

        return CreateResponse(response, content);
    }

    public async Task<JsonResponse> PostAsync(IWebRequest request,
        Action<HttpRequestMessage>? requestFilter = null,
        CancellationToken? cancellationToken = default)
    {
        var response = await SendRequestAsync(HttpMethod.Post, request, null, requestFilter, cancellationToken);
        var content = await GetStringResponseAsync(response, _jsonOptions, cancellationToken);

        return CreateResponse(response, content);
    }

    public async Task<JsonResponse<TResponse>> PostAsync<TResponse>(IWebRequest<TResponse> request, PostFile file,
        Action<HttpRequestMessage>? requestFilter = null,
        CancellationToken? cancellationToken = default)
        where TResponse : IWebResponse, new()
    {
        var response = await SendRequestAsync(HttpMethod.Post, request, file, requestFilter, cancellationToken);
        var content = await GetTypedResponseAsync<TResponse>(response, _jsonOptions, cancellationToken);

        return CreateResponse(response, content);
    }

    public async Task<JsonResponse> PostAsync(IWebRequest request, PostFile file,
        Action<HttpRequestMessage>? requestFilter = null,
        CancellationToken? cancellationToken = default)
    {
        var response = await SendRequestAsync(HttpMethod.Post, request, file, requestFilter, cancellationToken);
        var content = await GetStringResponseAsync(response, _jsonOptions, cancellationToken);

        return CreateResponse(response, content);
    }

    public async Task<JsonResponse<TResponse>> PutAsync<TResponse>(IWebRequest<TResponse> request,
        Action<HttpRequestMessage>? requestFilter = null,
        CancellationToken? cancellationToken = default)
        where TResponse : IWebResponse, new()
    {
        var response = await SendRequestAsync(HttpMethod.Put, request, null, requestFilter, cancellationToken);
        var content = await GetTypedResponseAsync<TResponse>(response, _jsonOptions, cancellationToken);

        return CreateResponse(response, content);
    }

    public async Task<JsonResponse> PutAsync(IWebRequest request,
        Action<HttpRequestMessage>? requestFilter = null,
        CancellationToken? cancellationToken = default)
    {
        var response = await SendRequestAsync(HttpMethod.Put, request, null, requestFilter, cancellationToken);
        var content = await GetStringResponseAsync(response, _jsonOptions, cancellationToken);

        return CreateResponse(response, content);
    }

    internal static async Task<Result<string?, ResponseProblem>> GetStringResponseAsync(HttpResponseMessage response,
        JsonSerializerOptions? jsonOptions, CancellationToken? cancellationToken)
    {
        var contentType = response.Content.Headers.ContentType;
        if (contentType.NotExists())
        {
            if (response.IsSuccessStatusCode)
            {
                return default;
            }

            return response.StatusCode.ToResponseProblem(response.ReasonPhrase);
        }

        if (contentType.MediaType == HttpContentTypes.JsonProblem)
        {
            if (TryReadRfc7807Error(response, jsonOptions, cancellationToken, out var problem))
            {
                return problem;
            }
        }

        if (contentType.MediaType == HttpContentTypes.Json)
        {
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync(cancellationToken ?? CancellationToken.None);
            }

            if (response.Content.Headers.ContentType.Exists())
            {
                if (TryReadRfc6749Error(response, jsonOptions, cancellationToken, out var problem))
                {
                    return problem;
                }
            }

            return response.StatusCode.ToResponseProblem(response.ReasonPhrase);
        }

        if (contentType.MediaType is HttpContentTypes.Text or HttpContentTypes.Html)
        {
            return await response.Content.ReadAsStringAsync(cancellationToken ?? CancellationToken.None);
        }

        if (contentType.MediaType == HttpContentTypes.OctetStream)
        {
            return default;
        }

        //Unrecognized content type (could be a file or image?)
        return default;
    }

    internal static async Task<Result<TResponse, ResponseProblem>> GetTypedResponseAsync<TResponse>(
        HttpResponseMessage response, JsonSerializerOptions? jsonOptions, CancellationToken? cancellationToken)
        where TResponse : IWebResponse, new()
    {
        var contentType = response.Content.Headers.ContentType;
        if (contentType.NotExists())
        {
            if (response.IsSuccessStatusCode)
            {
                return new TResponse();
            }

            return response.StatusCode.ToResponseProblem(response.ReasonPhrase);
        }

        if (contentType.MediaType == HttpContentTypes.JsonProblem)
        {
            if (TryReadRfc7807Error(response, jsonOptions, cancellationToken, out var problem))
            {
                return problem;
            }
        }

        if (contentType.MediaType == HttpContentTypes.Json)
        {
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TResponse>(jsonOptions,
                    cancellationToken ?? CancellationToken.None) ?? new TResponse();
            }

            if (response.Content.Headers.ContentType.Exists())
            {
                if (TryReadRfc6749Error(response, jsonOptions, cancellationToken, out var problem))
                {
                    return problem;
                }
            }

            return response.StatusCode.ToResponseProblem(response.ReasonPhrase);
        }

        return new TResponse();
    }

    public async Task SendOneWayAsync(IWebRequest request, Action<HttpRequestMessage>? requestFilter = null,
        CancellationToken? cancellationToken = default)
    {
        await SendRequestAsync(HttpMethod.Put, request, null, requestFilter, cancellationToken);
    }

    public void SetBaseUrl(string baseUrl)
    {
        _client.BaseAddress = new Uri(baseUrl.WithTrailingSlash(), UriKind.Absolute);
    }

    private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, IWebRequest request,
        PostFile? file, Action<HttpRequestMessage>? requestFilter, CancellationToken? cancellationToken = default)
    {
        var requestUri = request.GetRequestInfo().Route;

        HttpContent? content = null;
        try
        {
            if (file.Exists())
            {
                var multipart = new MultipartFormDataContent();
                var requestFields =
                    request.SerializeToJson()
                        .FromJson<Dictionary<string, string>>()
                    !; //HACK: really need these values to be serialized as QueryString parameters
                if (requestFields.HasAny())
                {
                    foreach (var field in requestFields)
                    {
                        multipart.Add(new StringContent(field.Value), field.Key);
                    }
                }

                var streamContent = new StreamContent(file.Stream);
                if (file.ContentType.HasValue())
                {
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                }

                multipart.Add(streamContent, "file", file.Filename);
                content = multipart;
            }
            else
            {
                content = method.CanHaveBody()
                    ? new StringContent(request.SerializeToJson(), new MediaTypeHeaderValue(HttpContentTypes.Json))
                    : null;
            }

            return await SendRequestAsync(method, requestUri, content, requestFilter, cancellationToken);
        }
        finally
        {
            (content as IDisposable)?.Dispose();
        }
    }

    private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string requestUri,
        HttpContent? requestContent, Action<HttpRequestMessage>? requestFilter,
        CancellationToken? cancellationToken = default)
    {
        var request = new HttpRequestMessage
        {
            Method = method,
            RequestUri = new Uri(_client.BaseAddress!, requestUri.WithoutLeadingSlash()),
            Content = requestContent,
            Headers = { { HttpHeaders.Accept, HttpContentTypes.Json } }
        };
        if (requestFilter is not null)
        {
            requestFilter(request);
        }

        return await _client.SendAsync(request, cancellationToken ?? CancellationToken.None);
    }

    private static bool TryReadRfc7807Error(HttpResponseMessage response, JsonSerializerOptions? jsonOptions,
        CancellationToken? cancellationToken, out ResponseProblem problem)
    {
        if (cancellationToken.HasValue)
        {
            cancellationToken.Value.ThrowIfCancellationRequested();
        }

        problem = new ResponseProblem();

        try
        {
            var details = response.Content.ReadFromJsonAsync<ProblemDetails>(jsonOptions, CancellationToken.None)
                .GetAwaiter().GetResult()!;
            if (details.Type.HasNoValue())
            {
                return false;
            }

            problem = details.ToResponseProblem();
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryReadRfc6749Error(HttpResponseMessage response, JsonSerializerOptions? jsonOptions,
        CancellationToken? cancellationToken, out ResponseProblem problem)
    {
        if (cancellationToken.HasValue)
        {
            cancellationToken.Value.ThrowIfCancellationRequested();
        }

        problem = new ResponseProblem();
        try
        {
            var details = response.Content
                .ReadFromJsonAsync<OAuth2Rfc6749ProblemDetails>(jsonOptions, CancellationToken.None)
                .GetAwaiter().GetResult()!;
            if (details.Error.HasNoValue())
            {
                return false;
            }

            problem = details.ToResponseProblem((int)response.StatusCode);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static JsonResponse CreateResponse(HttpResponseMessage response, Result<string?, ResponseProblem> content)
    {
        return new JsonResponse
        {
            StatusCode = response.StatusCode,
            Content = content,
            Headers = response.Headers,
            RequestId = response.GetOrCreateRequestId(),
            RawContent = content.IsSuccessful && !content.HasValue
                ? response.Content.ReadAsStream()
                : null
        };
    }

    private static JsonResponse<TResponse> CreateResponse<TResponse>(HttpResponseMessage response,
        Result<TResponse, ResponseProblem> content)
        where TResponse : IWebResponse, new()
    {
        return new JsonResponse<TResponse>
        {
            StatusCode = response.StatusCode,
            Content = content,
            Headers = response.Headers,
            RequestId = response.GetOrCreateRequestId(),
            RawContent = content.IsSuccessful && !content.HasValue
                ? response.Content.ReadAsStream()
                : null
        };
    }
}