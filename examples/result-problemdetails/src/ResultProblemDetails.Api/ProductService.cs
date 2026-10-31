using System.Collections.Concurrent;
using ErrorOr;

namespace ResultProblemDetails.Api;

/// <summary>
/// Expected failures (missing, invalid, duplicate) are returned as values,
/// not thrown. Exceptions stay reserved for the truly unexpected.
/// </summary>
public sealed class ProductService
{
    private readonly ConcurrentDictionary<Guid, Product> _products = new();

    public ErrorOr<Product> Get(Guid id) =>
        _products.TryGetValue(id, out var product)
            ? product
            : Error.NotFound("Product.NotFound", $"Product '{id}' was not found.");

    public ErrorOr<Product> Create(CreateProductRequest request)
    {
        List<Error> errors = [];

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add(Error.Validation("Name", "Name is required."));

        if (request.Price <= 0)
            errors.Add(Error.Validation("Price", "Price must be greater than zero."));

        if (errors.Count > 0)
            return errors;

        if (_products.Values.Any(p => p.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
            return Error.Conflict("Product.Duplicate", $"A product named '{request.Name}' already exists.");

        var product = new Product(Guid.NewGuid(), request.Name, request.Price);
        _products[product.Id] = product;
        return product;
    }
}
