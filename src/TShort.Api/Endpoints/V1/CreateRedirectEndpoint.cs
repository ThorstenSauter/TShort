using FastEndpoints;
using FastEndpoints.AspVersioning;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using TShort.Api.Authentication;
using TShort.Api.Errors;
using TShort.Api.Extensions;
using TShort.Api.Mappers;
using TShort.Api.Services;
using TShort.Api.Validation;
using TShort.Api.Versioning;
using TShort.Contracts.V1.Requests;

namespace TShort.Api.Endpoints.V1;

public sealed class CreateRedirectEndpoint(IRedirectService redirectService)
    : Endpoint<CreateRedirectRequest, Results<Created<Uri>, ProblemDetails, InternalServerError<string>>>
{
    private readonly IRedirectService _redirectService = redirectService;

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

        var redirect = req.ToRedirect(User.GetUserId());

        var creationResult = await _redirectService.CreateRedirectAsync(redirect, ct);

        if (creationResult.HasError<PropertyValidationError>(out var validationErrors))
        {
            this.AddValidationErrors(validationErrors);
            return new ProblemDetails(ValidationFailures);
        }

        if (creationResult.IsFailed)
        {
            return TypedResults.InternalServerError(creationResult.GetAllErrorMessages());
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
