namespace TShort.Contracts.V1.Requests;

/// <summary>
///     Request for creating a new redirect.
/// </summary>
public sealed class CreateRedirectRequest
{
    /// <summary>
    ///     The short path for the redirect.
    /// </summary>
    public string ShortName { get; set; } = null!;

    /// <summary>
    ///     The URL to redirect to.
    /// </summary>
    public string RedirectTo { get; set; } = null!;
}
