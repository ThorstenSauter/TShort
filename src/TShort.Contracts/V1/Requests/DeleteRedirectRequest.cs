namespace TShort.Contracts.V1.Requests;

/// <summary>
///     Request for deleting a redirect.
/// </summary>
public sealed class DeleteRedirectRequest
{
    /// <summary>
    ///     The short path for the redirect to delete.
    /// </summary>
    public string ShortName { get; set; } = null!;
}
