using FastEndpoints;
using FastEndpoints.AspVersioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Identity.Web;
using TShort.Api.Authentication;
using TShort.Api.Data;
using TShort.Api.Mappers;
using TShort.Api.Versioning;
using TShort.Contracts.V1.Requests;
using TShort.Contracts.V1.Responses;

namespace TShort.Api.Endpoints.V1;

public sealed class GetRedirectEndpoint(AppDbContext dbContext)
    : Endpoint<GetRedirectRequest, Results<Ok<RedirectResponse>, NotFound, ForbidHttpResult>>
{
    private readonly AppDbContext _dbContext = dbContext;

    public override void Configure()
    {
        Get("/api/redirects/{ShortName}");
        Policies(AuthorizationPolicies.User);
        Options(x => x
            .WithVersionSet(Versions.ManagementApi)
            .MapToApiVersion(Versions.V1));
    }

    public override async Task<Results<Ok<RedirectResponse>, NotFound, ForbidHttpResult>> ExecuteAsync(
        GetRedirectRequest req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        var redirect = await _dbContext.Redirects.FindAsync([req.ShortName], ct);
        if (redirect is null)
        {
            return TypedResults.NotFound();
        }

        var userId = HttpContext.User.GetUserId();

        if (redirect.CreatedBy != userId
            && !HttpContext.User.IsInRole(Role.Administrator)
            && !HttpContext.User.IsInRole(Role.Superadministrator))
        {
            return TypedResults.Forbid();
        }

        return TypedResults.Ok(redirect.ToResponse());
    }
}
