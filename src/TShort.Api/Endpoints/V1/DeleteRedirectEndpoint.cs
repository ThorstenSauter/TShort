using FastEndpoints;
using FastEndpoints.AspVersioning;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using TShort.Api.Authentication;
using TShort.Api.Errors;
using TShort.Api.Extensions;
using TShort.Api.Services;
using TShort.Api.Validation;
using TShort.Api.Versioning;
using TShort.Contracts.V1.Requests;

namespace TShort.Api.Endpoints.V1;

public class DeleteRedirectEndpoint(IRedirectService redirectService)
    : Endpoint<DeleteRedirectRequest, Results<NoContent, NotFound, ForbidHttpResult, InternalServerError<string>>>
{
    private readonly IRedirectService _redirectService = redirectService;

    public override void Configure()
    {
        Delete("/api/redirects/{ShortName}");
        Policies(AuthorizationPolicies.User);
        Options(x => x
            .WithVersionSet(Versions.ManagementApi)
            .MapToApiVersion(Versions.V1));
    }

    public override async Task<Results<NoContent, NotFound, ForbidHttpResult, InternalServerError<string>>>
        ExecuteAsync(DeleteRedirectRequest req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        var deleteRedirectResult = await _redirectService.GetRedirectAsync(
            req.ShortName, User.GetUserId(), User.IsPrivileged(), ct);

        if (deleteRedirectResult.IsSuccess)
        {
            return TypedResults.NoContent();
        }

        if (deleteRedirectResult.HasError<NotFoundError>())
        {
            return TypedResults.NotFound();
        }

        if (deleteRedirectResult.HasError<InvalidObjectAccessError>())
        {
            return TypedResults.Forbid();
        }

        return TypedResults.InternalServerError(deleteRedirectResult.GetAllErrorMessages());
    }
}

public sealed class DeleteRedirectRequestValidator : Validator<DeleteRedirectRequest>
{
    public DeleteRedirectRequestValidator()
    {
        RuleFor(x => x.ShortName)
            .NotEmpty()
            .Matches(ValidationPatterns.ShortName)
            .WithMessage(
                "Short name must be alphanumeric and not contain slashes at the beginning or end; Slashes may also not directly follow a previous slash");
    }
}
