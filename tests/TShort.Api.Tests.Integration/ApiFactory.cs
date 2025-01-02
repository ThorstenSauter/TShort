using Microsoft.AspNetCore.Mvc.Testing;

namespace TShort.Api.Tests.Integration;

public sealed class ApiFactory : WebApplicationFactory<IApiAssemblyMarker>;
