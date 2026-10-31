namespace ExceptionHandling.Api;

/// <summary>An exception that knows how it should be presented to the client.</summary>
public class DomainException(string title, string message, int statusCode) : Exception(message)
{
    public string Title { get; } = title;
    public int StatusCode { get; } = statusCode;
}

public sealed class OrderNotFoundException(int id)
    : DomainException("Order not found", $"Order {id} does not exist.", StatusCodes.Status404NotFound);

public sealed class InvalidQuantityException(int quantity)
    : DomainException("Invalid quantity", $"Quantity {quantity} is not allowed.", StatusCodes.Status400BadRequest);
