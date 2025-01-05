using System.Collections.Frozen;
using System.Text.RegularExpressions;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TShort.Api.Authentication;
using TShort.Api.Data;
using TShort.Api.Data.Models;
using TShort.Api.Validation;
using TShort.Api.Versioning;
using TShort.Contracts.V1.Requests;

namespace TShort.Api.Endpoints.V1;

public sealed class CreateRedirectEndpoint(AppDbContext dbContext, ILogger<CreateRedirectEndpoint> logger)
    : Endpoint<CreateRedirectRequest,
        Results<Created<Uri>, ProblemDetails, InternalServerError<string>>>
{
    public static readonly FrozenSet<string> RestrictedShortNames = new HashSet<string> { "admin", "api" }
        .ToFrozenSet();

    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogger<CreateRedirectEndpoint> _logger = logger;

    public override void Configure()
    {
        Post("/api/redirects");
        Policies(AuthorizationPolicies.User);
        Options(x => x
            .WithVersionSet(Versions.ManagementApi)
            .MapToApiVersion(Versions.V1));
    }

    public override async Task<Results<Created<Uri>, ProblemDetails, InternalServerError<string>>>
        ExecuteAsync(CreateRedirectRequest req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        if (RestrictedShortNames.Contains(req.ShortName))
        {
            AddError(r => r.ShortName, "The short name is reserved");
            return new ProblemDetails(ValidationFailures);
        }

        var existingRedirect = await _dbContext.Redirects.FindAsync([req.ShortName], ct);
        if (existingRedirect is not null)
        {
            AddError(r => r.ShortName, "The short name is already in use");
            return new ProblemDetails(ValidationFailures);
        }

        var redirect = new Redirect
        {
            ShortName = req.ShortName, RedirectTo = req.RedirectTo, CreatedBy = HttpContext.User.GetUserId()
        };

        try
        {
            _dbContext.Redirects.Add(redirect);
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to save redirect");
            return TypedResults.InternalServerError<string>("Failed to save redirect");
        }

        return TypedResults.Created<Uri>(new Uri($"/{req.ShortName}", UriKind.Relative), null);
    }
}

public sealed class CreateRedirectRequestValidator : Validator<CreateRedirectRequest>
{
    public CreateRedirectRequestValidator()
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
