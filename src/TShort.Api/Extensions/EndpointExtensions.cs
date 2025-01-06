using FastEndpoints;
using TShort.Api.Errors;

namespace TShort.Api.Extensions;

public static class EndpointExtensions
{
    public static void AddValidationErrors<TRequest, TResponse>(
        this Endpoint<TRequest, TResponse> endpoint, IEnumerable<PropertyValidationError?> errors)
        where TRequest : notnull
    {
        ArgumentNullException.ThrowIfNull(errors);

        foreach (var error in errors.Where(e => e is not null))
        {
            endpoint.AddError(error!.ToValidationFailure());
        }
    }
}
