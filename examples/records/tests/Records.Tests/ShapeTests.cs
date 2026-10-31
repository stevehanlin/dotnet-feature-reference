using Records.Api;

namespace Records.Tests;

public class ShapeTests
{
    private sealed record UnknownShape : Shape;

    [Theory]
    [MemberData(nameof(KnownShapes))]
    public void GivenKnownShape_WhenCalculatingArea_ThenSwitchExpressionReturnsExpectedArea(Shape shape, double expected) =>
        Assert.Equal(expected, shape.Area(), precision: 5);

    public static TheoryData<Shape, double> KnownShapes => new()
    {
        { new Shape.Rectangle(3, 4), 12 },
        { new Shape.Circle(1), Math.PI },
        { new Shape.Triangle(3, 4), 6 },
    };

    [Fact]
    public void Gotcha_GivenShapeDerivedElsewhere_WhenCalculatingArea_ThenFallbackArmThrows() =>
        // C# has no closed hierarchies: the compiler cannot prove the switch is exhaustive.
        Assert.Throws<NotSupportedException>(() => new UnknownShape().Area());
}
