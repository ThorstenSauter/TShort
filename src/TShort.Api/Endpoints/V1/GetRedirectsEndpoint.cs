using FastEndpoints;
using FastEndpoints.AspVersioning;
using Microsoft.AspNetCore.Http.HttpResults;
using TShort.Api.Authentication;
using TShort.Api.Mappers;
using TShort.Api.Services;
using TShort.Api.Versioning;
using TShort.Contracts.V1.Responses;

namespace TShort.Api.Endpoints.V1;

public sealed class GetRedirectsEndpoint(IRedirectService redirectService)
    : EndpointWithoutRequest<Ok<RedirectsResponse>>
{
    private readonly IRedirectService _redirectService = redirectService;

    public override void Configure()
    {
        Get("/api/redirects");
        Policies(AuthorizationPolicies.User);
        Options(x => x
            .WithVersionSet(Versions.ManagementApi)
            .MapToApiVersion(Versions.V1));
    }

    public override async Task<Ok<RedirectsResponse>> ExecuteAsync(CancellationToken ct)
    {
        var redirects = await _redirectService.GetRedirectsAsync(User.GetUserId(), User.IsPrivileged(), ct);
        return TypedResults.Ok(new RedirectsResponse { Redirects = redirects.Select(x => x.ToResponse()).ToList() });
    }
}
