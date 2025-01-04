namespace TShort.Contracts.V1.Requests;

/// <summary>
///     Request for updating a redirect.
/// </summary>
public sealed class UpdateRedirectRequest
{
    /// <summary>
    ///     The short path to update the redirect for.
    /// </summary>
    public string ShortName { get; set; } = null!;

    /// <summary>
    ///     The URL to redirect to.
    /// </summary>
    public string RedirectTo { get; set; } = null!;
}
