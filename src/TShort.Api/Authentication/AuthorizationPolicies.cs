namespace TShort.Api.Authentication;

/// <summary>
///     Contains the names of
///     <a href="https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies">Policies</a> used for
///     authorization.
/// </summary>
internal static class AuthorizationPolicies
{
    public const string Administrator = nameof(Administrator);

    public const string Superadministrator = nameof(Superadministrator);

    public const string User = nameof(User);
}
