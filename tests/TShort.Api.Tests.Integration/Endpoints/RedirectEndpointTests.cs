using System.Net;
using TShort.Contracts.V1.Requests;

namespace TShort.Api.Tests.Integration.Endpoints;

[ClassDataSource<AlbaBootstrap>(Shared = SharedType.PerAssembly)]
public sealed class RedirectEndpointTests(AlbaBootstrap albaBootstrap) : AlbaTestBase(albaBootstrap)
{
    [Test]
    public async Task ShouldRedirectToUri_WhenRedirectExists()
    {
        // Arrange
        var request = new CreateRedirectRequest { RedirectTo = "https://www.google.com", ShortName = "google" };
        var redirectResult = await Host.Scenario(s =>
        {
            s.Post.Json(request).ToUrl("/api/redirects");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var location = redirectResult.Context.Response.Headers.Location.ToString();

        // Act
        var result = await Host.Scenario(s =>
        {
            s.Get.Url(location);
            s.RedirectShouldBe(request.RedirectTo);
        });
    }

    [Test]
    public async Task ShouldReturn404_WhenRedirectDoesNotExist()
    {
        // Arrange
        const string shortName = "/non-existing";

        // Act
        await Host.Scenario(s =>
        {
            s.Get.Url(shortName);
            s.StatusCodeShouldBe(HttpStatusCode.NotFound);
        });
    }
}
