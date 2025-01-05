namespace TShort.Api.Data.Models;

public sealed class Redirect
{
    public required string ShortName { get; set; }

    public required string RedirectTo { get; set; }

    public required string CreatedBy { get; set; }
}
