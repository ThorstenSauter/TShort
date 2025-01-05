using System.Text.Json;
using Asp.Versioning;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Identity.Web;
using TShort.Api.Authentication;
using TShort.Api.Versioning;

namespace TShort.Api;

internal static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services) =>
        services;

    public static IServiceCollection AddMicrosoftIdentityPlatform(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMicrosoftIdentityWebApiAuthentication(configuration, "EntraId");
        return services
            .AddAuthorization(
                static options =>
                {
                    options.AddPolicy(
                        AuthorizationPolicies.Administrator,
                        static builder => builder.RequireRole(Role.Administrator));
                    options.AddPolicy(
                        AuthorizationPolicies.Superadministrator,
                        static builder => builder.RequireRole(Role.Superadministrator));
                    options.AddPolicy(
                        AuthorizationPolicies.User, static builder => builder.RequireAuthenticatedUser());
                });
    }

    public static IServiceCollection ConfigureEndpoints(this IServiceCollection services)
    {
        Versions.Register();

        return services
            .Configure<JsonOptions>(
                static options =>
                {
                    options.SerializerOptions.PropertyNameCaseInsensitive = true;
                    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                })
            .AddFastEndpoints(static options => options.SourceGeneratorDiscoveredTypes.AddRange(DiscoveredTypes.All))
            .AddVersioning(o =>
            {
                o.DefaultApiVersion = new(1.0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ApiVersionReader = new QueryStringApiVersionReader("api-version");
            });
    }
}
