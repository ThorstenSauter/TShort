using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using TShort.Api.Data;

namespace TShort.Api.Endpoints;

public sealed class RedirectEndpoint(AppDbContext dbContext)
    : EndpointWithoutRequest<Results<RedirectHttpResult, NotFound>>
{
    private readonly AppDbContext _dbContext = dbContext;

    public override void Configure()
    {
        Get("/{**path}");
        RoutePrefixOverride(string.Empty);
        AllowAnonymous();
        Description(x => x.WithName("Redirect"));
    }

    public override async Task<Results<RedirectHttpResult, NotFound>> ExecuteAsync(CancellationToken ct)
    {
        var shortName = HttpContext.Request.Path.Value!.Trim('/');
        var redirect = await _dbContext.Redirects.FindAsync([shortName], ct);

        return redirect is not null
            ? TypedResults.Redirect(redirect.RedirectTo)
            : TypedResults.NotFound();
    }
}
