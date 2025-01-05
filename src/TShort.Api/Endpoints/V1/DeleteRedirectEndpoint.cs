using FastEndpoints;
using FastEndpoints.AspVersioning;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TShort.Api.Authentication;
using TShort.Api.Data;
using TShort.Api.Validation;
using TShort.Api.Versioning;
using TShort.Contracts.V1.Requests;

namespace TShort.Api.Endpoints.V1;

public class DeleteRedirectEndpoint(AppDbContext dbContext, ILogger<DeleteRedirectEndpoint> logger)
    : Endpoint<DeleteRedirectRequest, Results<NoContent, NotFound, ForbidHttpResult, InternalServerError>>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogger<DeleteRedirectEndpoint> _logger = logger;

    public override void Configure()
    {
        Delete("/api/redirects/{ShortName}");
        Policies(AuthorizationPolicies.User);
        Options(x => x
            .WithVersionSet(Versions.ManagementApi)
            .MapToApiVersion(Versions.V1));
    }

    public override async Task<Results<NoContent, NotFound, ForbidHttpResult, InternalServerError>> ExecuteAsync(
        DeleteRedirectRequest req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        var redirect = await _dbContext.Redirects.FindAsync([req.ShortName], ct);
        if (redirect is null)
        {
            return TypedResults.NotFound();
        }

        if (redirect.CreatedBy != HttpContext.User.GetUserId() && !HttpContext.User.IsInRole(Role.Administrator)
                                                               && !HttpContext.User.IsInRole(Role.Superadministrator))
        {
            return TypedResults.Forbid();
        }

        try
        {
            _dbContext.Redirects.Remove(redirect);
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to remove redirect {ShortName}", req.ShortName);
            return TypedResults.InternalServerError();
        }

        return TypedResults.NoContent();
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
