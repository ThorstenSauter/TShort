namespace TShort.Contracts.V1.Responses;

/// <summary>
///     Represents a response for a stored redirect.
/// </summary>
public sealed class RedirectResponse
{
    /// <summary>
    ///     The short name of the redirect.
    /// </summary>
    public required string ShortName { get; init; }

    /// <summary>
    ///     The URL to redirect to.
    /// </summary>
    public required string RedirectTo { get; init; }

    /// <summary>
    ///     The id of the user that created the redirect.
    /// </summary>
    public required string CreatedBy { get; set; }
}
