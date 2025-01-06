namespace TShort.Api.Extensions;

public static class ResultExtensions
{
    public static string GetAllErrorMessages<T>(this Result<T> result) =>
        string.Join(". ", result.Errors.Select(x => x.Message));
}
