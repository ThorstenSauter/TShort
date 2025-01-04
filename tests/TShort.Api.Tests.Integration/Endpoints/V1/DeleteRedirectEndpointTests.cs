using System.Net;
using FastEndpoints;
using Microsoft.Identity.Web;
using TShort.Api.Authentication;
using TShort.Contracts.V1.Requests;

namespace TShort.Api.Tests.Integration.Endpoints.V1;

[ClassDataSource<AlbaBootstrap>(Shared = SharedType.PerAssembly)]
public sealed class DeleteRedirectEndpointTests(AlbaBootstrap albaBootstrap) : AlbaTestBase(albaBootstrap)
{
    [Test]
    public async Task ShouldReturnNoContent_WhenRedirectIsDeleted()
    {
        // Arrange
        var creationRequest = new CreateRedirectRequest { RedirectTo = "https://www.google.com", ShortName = "google" };
        await Host.Scenario(s =>
        {
            s.Post.Json(creationRequest).ToUrl("/api/redirects");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        // Act
        await Host.Scenario(s =>
        {
            s.Delete.Url($"/api/redirects/{creationRequest.ShortName}");
            s.StatusCodeShouldBe(HttpStatusCode.NoContent);
        });
    }

    [Test]
    public async Task ShouldReturnNotFound_WhenRedirectDoesNotExist()
    {
        // Arrange
        const string shortName = "nonexisting";

        // Act
        await Host.Scenario(s =>
        {
            s.Delete.Url($"/api/redirects/{shortName}");
            s.StatusCodeShouldBe(HttpStatusCode.NotFound);
        });
    }

    [Test]
    public async Task ShouldReturnBadRequest_WhenRedirectShortNameIsInvalid()
    {
        // Arrange
        const string shortName = "non-existing";

        // Act
        var result = await Host.Scenario(s =>
        {
            s.Delete.Url($"/api/redirects/{shortName}");
            s.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });

        var problemDetails = result.ReadAsJson<ProblemDetails>();

        // Assert
        await Verify(problemDetails, VerifySettings);
    }

    [Test]
    public async Task ShouldReturnForbidden_WhenRedirectBelongsToAnotherUserAndCallerIsUnprivileged()
    {
        // Arrange
        var creationRequest = new CreateRedirectRequest { RedirectTo = "https://www.google.com", ShortName = "google" };
        await Host.Scenario(s =>
        {
            s.RemoveClaim(ClaimConstants.Oid);
            s.WithClaim(new(ClaimConstants.Oid, Guid.NewGuid().ToString()));

            s.Post.Json(creationRequest).ToUrl("/api/redirects");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        // Act
        await Host.Scenario(s =>
        {
            s.Delete.Url($"/api/redirects/{creationRequest.ShortName}");
            s.StatusCodeShouldBe(HttpStatusCode.Forbidden);
        });
    }

    [Test]
    [Arguments(Role.Administrator)]
    [Arguments(Role.Superadministrator)]
    public async Task ShouldReturnNoContent_WhenRedirectBelongsToAnotherUserAndCallerIsPrivileged(string role)
    {
        // Arrange
        var creationRequest = new CreateRedirectRequest { RedirectTo = "https://www.google.com", ShortName = "google" };
        await Host.Scenario(s =>
        {
            s.RemoveClaim(ClaimConstants.Oid);
            s.WithClaim(new(ClaimConstants.Oid, Guid.NewGuid().ToString()));

            s.Post.Json(creationRequest).ToUrl("/api/redirects");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        // Act
        await Host.Scenario(s =>
        {
            s.WithClaim(new(ClaimConstants.Role, role));

            s.Delete.Url($"/api/redirects/{creationRequest.ShortName}");
            s.StatusCodeShouldBe(HttpStatusCode.NoContent);
        });
    }
}
