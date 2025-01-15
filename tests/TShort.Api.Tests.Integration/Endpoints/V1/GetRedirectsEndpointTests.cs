using System.Net;
using System.Security.Claims;
using Microsoft.Identity.Web;
using TShort.Api.Authentication;
using TShort.Contracts.V1.Requests;
using TShort.Contracts.V1.Responses;

namespace TShort.Api.Tests.Integration.Endpoints.V1;

[ClassDataSource<ApiFactory>(Shared = SharedType.PerAssembly)]
public sealed class GetRedirectsEndpointTests(ApiFactory apiFactory) : EndpointTestBase(apiFactory)
{
    [Test]
    public async Task ShouldReturnFilteredRedirects_WhenUserIsUnprivileged()
    {
        // Arrange
        var otherUsersRequest =
            new CreateRedirectRequest { RedirectTo = "https://www.google.com", ShortName = "google" };
        await Host.Scenario(s =>
        {
            s.RemoveClaim(ClaimConstants.Oid);
            s.WithClaim(new(ClaimConstants.Oid, Guid.NewGuid().ToString()));

            s.Post.Json(otherUsersRequest).ToUrl("/api/redirects");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var thisUsersRequest = new CreateRedirectRequest { RedirectTo = "https://www.tshort.me", ShortName = "tshort" };
        await Host.Scenario(s =>
        {
            s.Post.Json(thisUsersRequest).ToUrl("/api/redirects");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        // Act
        var result = await Host.Scenario(s =>
        {
            s.Get.Url("/api/redirects");
            s.StatusCodeShouldBeOk();
        });

        var response = result.ReadAsJson<RedirectsResponse>();

        // Assert
        await Assert.That(response.Redirects.Count).IsEqualTo(1);
        await Verify(response);
    }

    [Test]
    [Arguments(Role.Administrator)]
    [Arguments(Role.Superadministrator)]
    public async Task ShouldReturnAllRedirects_WhenUserIsPrivileged(string role)
    {
        // Arrange
        var otherUsersRequest =
            new CreateRedirectRequest { RedirectTo = "https://www.google.com", ShortName = "google" };
        await Host.Scenario(s =>
        {
            s.RemoveClaim(ClaimConstants.Oid);
            s.WithClaim(new(ClaimConstants.Oid, Guid.NewGuid().ToString()));

            s.Post.Json(otherUsersRequest).ToUrl("/api/redirects");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var thisUsersRequest = new CreateRedirectRequest { RedirectTo = "https://www.tshort.me", ShortName = "tshort" };
        await Host.Scenario(s =>
        {
            s.Post.Json(thisUsersRequest).ToUrl("/api/redirects");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        // Act
        var result = await Host.Scenario(s =>
        {
            s.WithClaim(new(ClaimTypes.Role, role));
            s.Get.Url("/api/redirects");
            s.StatusCodeShouldBeOk();
        });

        var response = result.ReadAsJson<RedirectsResponse>();

        // Assert
        await Assert.That(response.Redirects.Count).IsEqualTo(2);
        await Verify(response);
    }
}
