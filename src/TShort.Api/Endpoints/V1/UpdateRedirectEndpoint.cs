using System.Text.RegularExpressions;
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

public sealed class UpdateRedirectEndpoint(AppDbContext dbContext, ILogger<UpdateRedirectEndpoint> logger)
    : Endpoint<UpdateRedirectRequest, Results<NoContent, NotFound, ForbidHttpResult, InternalServerError>>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogger<UpdateRedirectEndpoint> _logger = logger;

    public override void Configure()
    {
        Put("/api/redirects/{ShortName}");
        Policies(AuthorizationPolicies.User);
        Options(x => x
            .WithVersionSet(Versions.ManagementApi)
            .MapToApiVersion(Versions.V1));
    }

    public override async Task<Results<NoContent, NotFound, ForbidHttpResult, InternalServerError>> ExecuteAsync(
        UpdateRedirectRequest req, CancellationToken ct)
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
            redirect.RedirectTo = req.RedirectTo;
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to update redirect {ShortName}", req.ShortName);
            return TypedResults.InternalServerError();
        }

        return TypedResults.NoContent();
    }
}

public sealed class UpdateRedirectRequestValidator : Validator<UpdateRedirectRequest>
{
    public UpdateRedirectRequestValidator()
    {
        RuleFor(x => x.ShortName)
            .NotEmpty()
            .Matches(ValidationPatterns.ShortName)
            .WithMessage(
                "Short name must be alphanumeric and not contain slashes at the beginning or end; Slashes may also not directly follow a previous slash");

        RuleFor(x => x.RedirectTo)
            .NotEmpty()
            .Must(input => Uri.TryCreate(input, UriKind.Absolute, out var uri)
                           && uri.Scheme == Uri.UriSchemeHttps)
            .WithMessage("Invalid redirect URI - this service only accepts HTTPS URIs");
    }
}
