using System.Net;
using Common;
using Common.Extensions;
using Infrastructure.Web.Api.Common;
using Infrastructure.Web.Api.Interfaces;
using Infrastructure.Web.Common.Clients;
using Infrastructure.Web.Interfaces.Clients;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Web.Common.Extensions;

public static class ResponseProblemExtensions
{
    /// <summary>
    ///     Converts the specified <see cref="problem" /> to a <see cref="Error" />
    /// </summary>
    public static Error ToError(this ResponseProblem problem)
    {
        return problem.Status switch
        {
            (int)HttpStatusCode.BadRequest => problem.Errors.HasAny()
                ? Error.Validation(problem.Title)
                : Error.RuleViolation(problem.Title),
            (int)HttpStatusCode.Unauthorized => Error.NotAuthenticated(problem.Title),
            (int)HttpStatusCode.Forbidden => Error.ForbiddenAccess(problem.Title),
            (int)HttpStatusCode.NotFound => Error.EntityNotFound(problem.Title),
            (int)HttpStatusCode.MethodNotAllowed => Error.PreconditionViolation(problem.Title),
            (int)HttpStatusCode.Conflict => Error.EntityExists(problem.Title),
            (int)HttpStatusCode.InternalServerError => Error.Unexpected(problem.Title),
            _ => Error.Unexpected(problem.Title)
        };
    }

    /// <summary>
    ///     Returns an exception containing the specified <see cref="problem" />
    /// </summary>
    public static Exception ToException(this ResponseProblem problem)
    {
        var message = $"{problem.Status}: {problem.Title}";
        if (problem.Detail.HasValue())
        {
            message = $"{message}, {problem.Detail}";
        }

        if (problem.Exception.HasValue())
        {
            message = $"{message}, {problem.Exception}";
        }

        return new InvalidOperationException(message);
    }

    /// <summary>
    ///     Converts the given <see cref="status" /> to a <see cref="Infrastructure.Web.Interfaces.Clients.ResponseProblem" />
    /// </summary>
    public static ResponseProblem ToResponseProblem(this HttpStatusCode status, string? reason)
    {
        return new ResponseProblem
        {
            Title = reason ?? status.ToString(),
            Status = (int)status
        };
    }

    /// <summary>
    ///     Converts the given <see cref="details" /> to a <see cref="Infrastructure.Web.Interfaces.Clients.ResponseProblem" />
    /// </summary>
    public static ResponseProblem ToResponseProblem(this ProblemDetails? details)
    {
        if (details.NotExists())
        {
            return new ResponseProblem
            {
                Title = nameof(HttpStatusCode.InternalServerError),
                Status = (int)HttpStatusCode.InternalServerError
            };
        }

        var response = new ResponseProblem
        {
            Type = details.Type,
            Title = details.Title,
            Status = details.Status,
            Detail = details.Detail,
            Instance = details.Instance
        };

        if (details.Extensions.TryGetValue(HttpResponses.ProblemDetails.Extensions.ExceptionPropertyName,
                out var exception))
        {
            if (exception.Exists())
            {
                response.Exception = exception.ToString();
            }
        }

        if (details.Extensions.TryGetValue(HttpResponses.ProblemDetails.Extensions.ValidationErrorPropertyName,
                out var validations))
        {
            if (validations.Exists())
            {
                var errors = validations.ToString()!.FromJson<ValidatorProblem[]>()
                             ?? new List<ValidatorProblem>().ToArray();
                response.Errors = errors;
            }
        }

        return response;
    }

    public static ResponseProblem ToResponseProblem(this OAuth2Rfc6749ProblemDetails? details, int statusCode)
    {
        if (details.NotExists())
        {
            return new ResponseProblem
            {
                Title = nameof(HttpStatusCode.InternalServerError),
                Status = (int)HttpStatusCode.InternalServerError
            };
        }

        var response = new ResponseProblem
        {
            Type = OAuth2Rfc6749ProblemDetails.Reference,
            Title = details.Error,
            Status = statusCode,
            Detail = details.ErrorDescription,
            Instance = details.ErrorUri
        };


        return response;
    }
}