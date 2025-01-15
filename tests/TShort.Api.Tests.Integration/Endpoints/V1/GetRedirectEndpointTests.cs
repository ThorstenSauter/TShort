using System.Net;
using System.Security.Claims;
using Microsoft.Identity.Web;
using TShort.Api.Authentication;
using TShort.Contracts.V1.Requests;
using TShort.Contracts.V1.Responses;

namespace TShort.Api.Tests.Integration.Endpoints.V1;

[ClassDataSource<ApiFactory>(Shared = SharedType.PerAssembly)]
public sealed class GetRedirectEndpointTests(ApiFactory apiFactory) : EndpointTestBase(apiFactory)
{
    [Test]
    public async Task ShouldReturnOk_WhenRedirectExists()
    {
        // Arrange
        var request = new CreateRedirectRequest { RedirectTo = "https://www.google.com", ShortName = "google" };
        await Host.Scenario(s =>
        {
            s.Post.Json(request).ToUrl("/api/redirects");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        // Act
        var result = await Host.Scenario(s =>
        {
            s.Get.Url($"/api/redirects/{request.ShortName}");
            s.StatusCodeShouldBeOk();
        });

        var body = result.ReadAsJson<RedirectResponse>();

        // Assert
        await Verify(body);
    }

    [Test]
    public async Task ShouldReturnNotFound_WhenRedirectDoesNotExist()
    {
        // Arrange
        const string shortName = "non-existing";

        // Act
        await Host.Scenario(s =>
        {
            s.Get.Url($"/api/redirects/{shortName}");
            s.StatusCodeShouldBe(HttpStatusCode.NotFound);
        });
    }

    [Test]
    [Arguments(Role.Administrator)]
    [Arguments(Role.Superadministrator)]
    public async Task ShouldReturnOk_WhenRedirectBelongsToAnotherUserAndCallerIsPrivileged(string role)
    {
        // Arrange
        var request = new CreateRedirectRequest { RedirectTo = "https://www.google.com", ShortName = "google" };
        await Host.Scenario(s =>
        {
            s.RemoveClaim(ClaimConstants.Oid);
            s.WithClaim(new(ClaimConstants.Oid, Guid.NewGuid().ToString()));

            s.Post.Json(request).ToUrl("/api/redirects");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        // Act
        var result = await Host.Scenario(s =>
        {
            s.WithClaim(new(ClaimTypes.Role, role));

            s.Get.Url($"/api/redirects/{request.ShortName}");
            s.StatusCodeShouldBeOk();
        });

        var body = result.ReadAsJson<RedirectResponse>();

        // Assert
        await Verify(body);
    }

    [Test]
    public async Task ShouldReturnForbidden_WhenRedirectBelongsToAnotherUserAndCallerIsUnprivileged()
    {
        // Arrange
        var request = new CreateRedirectRequest { RedirectTo = "https://www.google.com", ShortName = "google" };
        await Host.Scenario(s =>
        {
            s.RemoveClaim(ClaimConstants.Oid);
            s.WithClaim(new(ClaimConstants.Oid, Guid.NewGuid().ToString()));

            s.Post.Json(request).ToUrl("/api/redirects");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        // Act
        await Host.Scenario(s =>
        {
            s.Get.Url($"/api/redirects/{request.ShortName}");
            s.StatusCodeShouldBe(HttpStatusCode.Forbidden);
        });
    }
}
