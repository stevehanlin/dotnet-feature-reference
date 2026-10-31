using ErrorOr;
using Microsoft.AspNetCore.Http;
using ResultProblemDetails.Api;

namespace ResultProblemDetails.Tests;

public class ErrorOrExtensionsTests
{
    private static int StatusOf(IResult result) => ((IStatusCodeHttpResult)result).StatusCode!.Value;

    private static int StatusOfProblemFor(params Error[] errors) => StatusOf(errors.ToProblem());

    [Fact]
    public void GivenNoErrors_WhenMappingToProblem_ThenReturnsGeneric500() =>
        Assert.Equal(500, StatusOfProblemFor());

    [Theory]
    [InlineData(ErrorType.NotFound, 404)]
    [InlineData(ErrorType.Conflict, 409)]
    [InlineData(ErrorType.Unauthorized, 401)]
    [InlineData(ErrorType.Forbidden, 403)]
    [InlineData(ErrorType.Failure, 500)]
    [InlineData(ErrorType.Unexpected, 500)]
    public void GivenSingleError_WhenMappingToProblem_ThenUsesDocumentedStatus(ErrorType type, int expectedStatus) =>
        Assert.Equal(expectedStatus, StatusOfProblemFor(Error.Custom((int)type, "Some.Code", "Something happened.")));

    [Fact]
    public void GivenOnlyValidationErrors_WhenMappingToProblem_ThenReturns400() =>
        Assert.Equal(400, StatusOfProblemFor(Error.Validation("Name", "Required."), Error.Validation("Price", "Too low.")));

    [Fact]
    public void GivenValidationMixedWithOtherTypes_WhenMappingToProblem_ThenFirstErrorDecidesStatus() =>
        Assert.Equal(400, StatusOfProblemFor(Error.Validation("Name", "Required."), Error.Conflict("Dup", "Duplicate.")));

    [Fact]
    public void GivenSuccessValue_WhenConvertingToResult_ThenReturnsOk()
    {
        ErrorOr<int> success = 5;

        Assert.Equal(200, StatusOf(success.ToResult(value => Results.Ok(value))));
    }
}
