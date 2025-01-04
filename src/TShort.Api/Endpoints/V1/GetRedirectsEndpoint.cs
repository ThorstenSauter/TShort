using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TShort.Api.Authentication;
using TShort.Api.Data;
using TShort.Api.Mappers;
using TShort.Api.Versioning;
using TShort.Contracts.V1.Responses;

namespace TShort.Api.Endpoints.V1;

public sealed class GetRedirectsEndpoint(AppDbContext dbContext)
    : EndpointWithoutRequest<Ok<RedirectsResponse>>
{
    private readonly AppDbContext _dbContext = dbContext;

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
        var redirects = IsPrivilegedUser(HttpContext.User)
            ? _dbContext.Redirects
            : _dbContext.Redirects.Where(x => x.CreatedBy == HttpContext.User.GetUserId());

        return TypedResults.Ok<RedirectsResponse>(new()
        {
            Redirects = await redirects.Select(x => x.ToResponse()).ToListAsync(ct)
        });
    }

    private static bool IsPrivilegedUser(ClaimsPrincipal user)
    {
        return user.IsInRole(Role.Administrator)
               || user.IsInRole(Role.Superadministrator);
    }
}
