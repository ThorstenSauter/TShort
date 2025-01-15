using System.Net;
using FastEndpoints;
using Microsoft.Identity.Web;
using TShort.Api.Authentication;
using TShort.Contracts.V1.Requests;
using TShort.Contracts.V1.Responses;

namespace TShort.Api.Tests.Integration.Endpoints.V1;

[ClassDataSource<ApiFactory>(Shared = SharedType.PerAssembly)]
public sealed class UpdateRedirectEndpointTests(ApiFactory apiFactory) : EndpointTestBase(apiFactory)
{
    [Test]
    public async Task ShouldReturnNoContent_WhenRedirectIsUpdated()
    {
        // Arrange
        var creationRequest = new CreateRedirectRequest { RedirectTo = "https://www.google.com", ShortName = "google" };
        await Host.Scenario(s =>
        {
            s.Post.Json(creationRequest).ToUrl("/api/redirects");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var updateRequest = new UpdateRedirectRequest { RedirectTo = "https://www.google.de" };

        // Act
        await Host.Scenario(s =>
        {
            s.Put.Json(updateRequest).ToUrl($"/api/redirects/{creationRequest.ShortName}");
            s.StatusCodeShouldBe(HttpStatusCode.NoContent);
        });

        // Assert
        var result = await Host.Scenario(s =>
        {
            s.Get.Url($"/api/redirects/{creationRequest.ShortName}");
            s.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var response = result.ReadAsJson<RedirectResponse>();

        await Verify(response);
    }

    [Test]
    public async Task ShouldReturnNotFound_WhenRedirectDoesNotExist()
    {
        // Arrange
        var updateRequest = new UpdateRedirectRequest { RedirectTo = "https://www.google.com", ShortName = "google" };

        // Act
        await Host.Scenario(s =>
        {
            s.Put.Json(updateRequest).ToUrl($"/api/redirects/{updateRequest.ShortName}");
            s.StatusCodeShouldBe(HttpStatusCode.NotFound);
        });
    }

    [Test]
    public async Task ShouldReturnBadRequest_WhenRedirectUriIsMalformed()
    {
        // Arrange
        var updateRequest = new UpdateRedirectRequest { RedirectTo = "malformed-uri", ShortName = "google" };

        // Act
        var result = await Host.Scenario(s =>
        {
            s.Put.Json(updateRequest).ToUrl($"/api/redirects/{updateRequest.ShortName}");
            s.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });

        var response = result.ReadAsJson<ProblemDetails>();

        // Assert
        await Verify(response, VerifySettings);
    }

    [Test]
    public async Task ShouldReturnForbidden_WhenRedirectWhenRedirectBelongsToAnotherUserAndCallerIsUnprivileged()
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

        var updateRequest = new UpdateRedirectRequest { RedirectTo = "https://www.google.de" };

        // Act
        await Host.Scenario(s =>
        {
            s.Put.Json(updateRequest).ToUrl($"/api/redirects/{creationRequest.ShortName}");
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
            s.Post.Json(creationRequest).ToUrl("/api/redirects");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var updateRequest = new UpdateRedirectRequest { RedirectTo = "https://www.google.de" };

        // Act
        await Host.Scenario(s =>
        {
            s.Put.Json(updateRequest).ToUrl($"/api/redirects/{creationRequest.ShortName}");
            s.StatusCodeShouldBe(HttpStatusCode.NoContent);
        });

        // Assert
        var result = await Host.Scenario(s =>
        {
            s.Get.Url($"/api/redirects/{creationRequest.ShortName}");
            s.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var response = result.ReadAsJson<RedirectResponse>();

        await Verify(response);
    }
}
