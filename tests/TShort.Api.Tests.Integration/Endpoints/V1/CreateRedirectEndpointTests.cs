using System.Net;
using FastEndpoints;
using Microsoft.Net.Http.Headers;
using TShort.Contracts.V1.Requests;

namespace TShort.Api.Tests.Integration.Endpoints.V1;

[ClassDataSource<ApiFactory>(Shared = SharedType.PerAssembly)]
public class CreateRedirectEndpointTests(ApiFactory apiFactory) : EndpointTestBase(apiFactory)
{
    [Test]
    public async Task ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreateRedirectRequest { RedirectTo = "https://www.google.com", ShortName = "google" };

        // Act
        await Host.Scenario(s =>
        {
            s.Post.Json(request).ToUrl("/api/redirects");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
            s.Header(HeaderNames.Location).SingleValueShouldEqual($"/{request.ShortName}");
        });
    }

    [Test]
    public async Task ShouldReturnBadRequest_WhenRedirectAlreadyExists()
    {
        // Arrange
        var request = new CreateRedirectRequest { RedirectTo = "https://www.google.com", ShortName = "google" };
        await Host.Scenario(s =>
        {
            s.Post.Json(request).ToUrl("/api/redirects");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
            s.Header(HeaderNames.Location).SingleValueShouldEqual($"/{request.ShortName}");
        });

        // Act
        var result = await Host.Scenario(s =>
        {
            s.Post.Json(request).ToUrl("/api/redirects");
            s.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });

        var problemDetails = result.ReadAsJson<ProblemDetails>();

        await Verify(problemDetails, VerifySettings);
    }

    [Test]
    public async Task ShouldReturnBadRequest_WhenRedirectUriMalformed()
    {
        // Arrange
        var request = new CreateRedirectRequest { RedirectTo = "invalidUri", ShortName = "invalid" };

        // Act
        var result = await Host.Scenario(s =>
        {
            s.Post.Json(request).ToUrl("/api/redirects");
            s.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });

        var problemDetails = result.ReadAsJson<ProblemDetails>();

        await Verify(problemDetails, VerifySettings);
    }

    [Test]
    public async Task ShouldReturnBadRequest_WhenShortNameIsReserved()
    {
        // Arrange
        var request = new CreateRedirectRequest { RedirectTo = "https://api.com", ShortName = "api" };

        // Act
        var result = await Host.Scenario(s =>
        {
            s.Post.Json(request).ToUrl("/api/redirects");
            s.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });

        var problemDetails = result.ReadAsJson<ProblemDetails>();

        // Assert
        await Verify(problemDetails, VerifySettings);
    }
}
