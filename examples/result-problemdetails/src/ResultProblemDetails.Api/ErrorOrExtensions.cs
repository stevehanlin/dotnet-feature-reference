using ErrorOr;

namespace ResultProblemDetails.Api;

public static class ErrorOrExtensions
{
    /// <summary>Maps a list of errors to an RFC 9457 problem details response.</summary>
    public static IResult ToProblem(this IReadOnlyList<Error> errors)
    {
        if (errors.Count == 0)
            return Results.Problem();

        // All validation errors: use the validation shape, which carries an "errors" dictionary.
        if (errors.All(e => e.Type == ErrorType.Validation))
        {
            var byField = errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

            return Results.ValidationProblem(byField);
        }

        var first = errors[0];
        var status = first.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError,
        };

        return Results.Problem(statusCode: status, title: first.Code, detail: first.Description);
    }

    /// <summary>Success goes through <paramref name="onValue"/>, failure becomes a problem response.</summary>
    public static IResult ToResult<T>(this ErrorOr<T> result, Func<T, IResult> onValue) =>
        result.Match(onValue, errors => errors.ToProblem());
}
