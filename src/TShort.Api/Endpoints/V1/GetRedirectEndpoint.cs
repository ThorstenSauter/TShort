using FastEndpoints;
using FastEndpoints.AspVersioning;
using Microsoft.AspNetCore.Http.HttpResults;
using TShort.Api.Authentication;
using TShort.Api.Errors;
using TShort.Api.Extensions;
using TShort.Api.Mappers;
using TShort.Api.Services;
using TShort.Api.Versioning;
using TShort.Contracts.V1.Requests;
using TShort.Contracts.V1.Responses;

namespace TShort.Api.Endpoints.V1;

public sealed class GetRedirectEndpoint(IRedirectService redirectService)
    : Endpoint<GetRedirectRequest,
        Results<Ok<RedirectResponse>, NotFound, ForbidHttpResult, InternalServerError<string>>>
{
    private readonly IRedirectService _redirectService = redirectService;

    public override void Configure()
    {
        Get("/api/redirects/{ShortName}");
        Policies(AuthorizationPolicies.User);
        Options(x => x
            .WithVersionSet(Versions.ManagementApi)
            .MapToApiVersion(Versions.V1));
    }

    public override async Task<Results<Ok<RedirectResponse>, NotFound, ForbidHttpResult, InternalServerError<string>>>
        ExecuteAsync(GetRedirectRequest req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        var getRedirectResult = await _redirectService.GetRedirectAsync(
            req.ShortName, User.GetUserId(), User.IsPrivileged(), ct);

        if (getRedirectResult.IsSuccess)
        {
            return TypedResults.Ok(getRedirectResult.Value.ToResponse());
        }

        if (getRedirectResult.HasError<NotFoundError>())
        {
            return TypedResults.NotFound();
        }

        if (getRedirectResult.HasError<InvalidObjectAccessError>())
        {
            return TypedResults.Forbid();
        }

        return TypedResults.InternalServerError(getRedirectResult.GetAllErrorMessages());
    }
}
