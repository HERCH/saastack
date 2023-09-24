using System.Net;
using Common.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Infrastructure.WebApi.Common.UnitTests;

[Trait("Category", "Unit")]
public class ContentNegotiationFilterSpec
{
    private readonly ContentNegotiationFilter _filter;

    public ContentNegotiationFilterSpec()
    {
        _filter = new ContentNegotiationFilter();
    }

    [Fact]
    public void WhenInvokeAsyncWithNullResponse_ThenReturnsNull()
    {
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext());
        var next = new EndpointFilterDelegate(_ => new ValueTask<object?>());

        var result = _filter.InvokeAsync(context, next);

        result.Result.Should().BeNull();
    }

    [Fact]
    public void WhenInvokeAsyncWithNakedObjectResponse_ThenReturnsJsonContentAsOk()
    {
        var response = new TestResponse();
        var httpContext = SetupHttpContext();
        var context = new DefaultEndpointFilterInvocationContext(httpContext.Object);
        var next = new EndpointFilterDelegate(_ => new ValueTask<object?>(response));

        var result = _filter.InvokeAsync(context, next);

        result.Result.Should().BeOfType<JsonHttpResult<object>>();
        result.Result.As<JsonHttpResult<object>>().ContentType.Should().BeNull();
        result.Result.As<JsonHttpResult<object>>().StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Result.As<JsonHttpResult<object>>().Value.Should().Be(response);
    }

    [Fact]
    public void WhenInvokeAsyncWithNakedEmptyStringResponse_ThenReturnsJsonContentAsOk()
    {
        var response = string.Empty;
        var httpContext = SetupHttpContext();
        var context = new DefaultEndpointFilterInvocationContext(httpContext.Object);
        var next = new EndpointFilterDelegate(_ => new ValueTask<object?>(response));

        var result = _filter.InvokeAsync(context, next);

        result.Result.Should().BeOfType<NoContent>();
        result.Result.As<NoContent>().StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [Fact]
    public void WhenInvokeAsyncWithIValueHttpResultResponse_ThenReturnsJsonContentAsOk()
    {
        var payload = new TestResponse();
        var response = Results.Ok(payload);
        var httpContext = SetupHttpContext();
        var context = new DefaultEndpointFilterInvocationContext(httpContext.Object);
        var next = new EndpointFilterDelegate(_ => new ValueTask<object?>(response));

        var result = _filter.InvokeAsync(context, next);

        result.Result.Should().BeOfType<JsonHttpResult<object>>();
        result.Result.As<JsonHttpResult<object>>().ContentType.Should().BeNull();
        result.Result.As<JsonHttpResult<object>>().StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Result.As<JsonHttpResult<object>>().Value.Should().Be(payload);
    }

    [Fact]
    public void WhenInvokeAsyncWithStreamResultResponse_ThenReturnsJsonContentAsOk()
    {
        using var stream = new MemoryStream();
        var response = Results.Stream(stream);
        var httpContext = SetupHttpContext();
        var context = new DefaultEndpointFilterInvocationContext(httpContext.Object);
        var next = new EndpointFilterDelegate(_ => new ValueTask<object?>(response));

        var result = _filter.InvokeAsync(context, next);

        result.Result.Should().BeOfType<FileStreamHttpResult>();
        result.Result.As<FileStreamHttpResult>().ContentType.Should().Be(HttpContentTypes.OctetStream);
        result.Result.As<FileStreamHttpResult>().FileStream.Should().BeSameAs(stream);
    }

    [Fact]
    public void WhenInvokeAsyncWithNoContentResponse_ThenReturnsJsonContentAsOk()
    {
        var response = Results.NoContent();
        var httpContext = SetupHttpContext();
        var context = new DefaultEndpointFilterInvocationContext(httpContext.Object);
        var next = new EndpointFilterDelegate(_ => new ValueTask<object?>(response));

        var result = _filter.InvokeAsync(context, next);

        result.Result.Should().BeOfType<NoContent>();
        result.Result.As<NoContent>().StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [Fact]
    public void WhenInvokeAsyncWithOtherEmptyIResultResponse_ThenReturnsJsonContentAsOk()
    {
        var response = Results.Ok();
        var httpContext = SetupHttpContext();
        var context = new DefaultEndpointFilterInvocationContext(httpContext.Object);
        var next = new EndpointFilterDelegate(_ => new ValueTask<object?>(response));

        var result = _filter.InvokeAsync(context, next);

        result.Result.Should().BeOfType<Ok>();
        result.Result.As<Ok>().StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public void WhenInvokeAsyncWithNullValueIResultResponse_ThenReturnsJsonContentAsOk()
    {
        var response = TypedResults.Ok((string)null!);
        var httpContext = SetupHttpContext();
        var context = new DefaultEndpointFilterInvocationContext(httpContext.Object);
        var next = new EndpointFilterDelegate(_ => new ValueTask<object?>(response));

        var result = _filter.InvokeAsync(context, next);

        result.Result.Should().BeOfType<Ok<string>>();
        result.Result.As<Ok<string>>().StatusCode.Should().Be(200);
    }

    [Fact]
    public void WhenInvokeAsyncWithNakedObjectResponseAndAcceptXml_ThenReturnsXml()
    {
        var response = new TestResponse();
        var httpContext = SetupHttpContext(HttpContentTypes.Xml);
        var context = new DefaultEndpointFilterInvocationContext(httpContext.Object);
        var next = new EndpointFilterDelegate(_ => new ValueTask<object?>(response));

        var result = _filter.InvokeAsync(context, next);

        result.Result.Should().BeOfType<XmlHttpResult<object>>();
        result.Result.As<XmlHttpResult<object>>().ContentType.Should().Be(HttpContentTypes.XmlWithCharset);
        result.Result.As<XmlHttpResult<object>>().StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Result.As<XmlHttpResult<object>>().Value.Should().Be(response);
    }

    [Fact]
    public void WhenInvokeAsyncWithNakedObjectResponseAndFormatXml_ThenReturnsXml()
    {
        var response = new TestResponse();
        var httpContext = SetupHttpContext(format: HttpContentTypeFormatters.Xml);
        var context = new DefaultEndpointFilterInvocationContext(httpContext.Object);
        var next = new EndpointFilterDelegate(_ => new ValueTask<object?>(response));

        var result = _filter.InvokeAsync(context, next);

        result.Result.Should().BeOfType<XmlHttpResult<object>>();
        result.Result.As<XmlHttpResult<object>>().ContentType.Should().Be(HttpContentTypes.XmlWithCharset);
        result.Result.As<XmlHttpResult<object>>().StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Result.As<XmlHttpResult<object>>().Value.Should().Be(response);
    }

    [Fact]
    public void WhenInvokeAsyncWithIResultResponseAndFormatXml_ThenReturnsXml()
    {
        var payload = new TestResponse();
        var response = Results.Ok(payload);
        var httpContext = SetupHttpContext(format: HttpContentTypeFormatters.Xml);
        var context = new DefaultEndpointFilterInvocationContext(httpContext.Object);
        var next = new EndpointFilterDelegate(_ => new ValueTask<object?>(response));

        var result = _filter.InvokeAsync(context, next);

        result.Result.Should().BeOfType<XmlHttpResult<object>>();
        result.Result.As<XmlHttpResult<object>>().ContentType.Should().Be(HttpContentTypes.XmlWithCharset);
        result.Result.As<XmlHttpResult<object>>().StatusCode.Should().Be((int)HttpStatusCode.OK);
        result.Result.As<XmlHttpResult<object>>().Value.Should().Be(payload);
    }

    private Mock<HttpContext> SetupHttpContext(string? accept = null, string? format = null)
    {
        var jsonOptions = new Mock<IOptions<JsonOptions>>();
        jsonOptions.Setup(jo => jo.Value)
            .Returns(new JsonOptions());

        var httpRequest = new Mock<HttpRequest>();
        httpRequest.Setup(hr => hr.Headers)
            .Returns(accept.HasNoValue()
                ? new HeaderDictionary()
                : new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { HttpHeaders.Accept, new StringValues(accept) }
                }));
        httpRequest.Setup(hr => hr.Query)
            .Returns(format.HasNoValue()
                ? new QueryCollection()
                : new QueryCollection(new Dictionary<string, StringValues>
                {
                    { HttpQueryParams.Format, new StringValues(format) }
                }));

        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(hc => hc.RequestServices.GetService(It.IsAny<Type>()))
            .Returns(jsonOptions.Object);
        httpContext.Setup(hc => hc.Request).Returns(httpRequest.Object);

        return httpContext;
    }
}