using System.Text.RegularExpressions;

namespace TShort.Api.Validation;

public static partial class ValidationPatterns
{
    [GeneratedRegex("^(?!.*//)(?!/)(?!.*//$)[a-zA-Z0-9/]+(?<!/)$")]
    public static partial Regex ShortName { get; }
}
