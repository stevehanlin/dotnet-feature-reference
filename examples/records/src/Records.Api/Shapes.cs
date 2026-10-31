namespace Records.Api;

/// <summary>Record hierarchies model "one of these" data; switch expressions stay exhaustive-looking.</summary>
public abstract record Shape
{
    public sealed record Circle(double Radius) : Shape;
    public sealed record Rectangle(double Width, double Height) : Shape;
    public sealed record Triangle(double Base, double Height) : Shape;

    public double Area() => this switch
    {
        Circle c => Math.PI * c.Radius * c.Radius,
        Rectangle r => r.Width * r.Height,
        Triangle t => 0.5 * t.Base * t.Height,
        _ => throw new NotSupportedException(),
    };
}
