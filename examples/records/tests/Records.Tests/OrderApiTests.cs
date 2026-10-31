using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Records.Api;
using static Records.Tests.TestData;

namespace Records.Tests;

public class OrderApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GivenOrderWithTwoLines_WhenPosting_ThenRecordsRoundTripAsJsonWithSummedTotal()
    {
        var request = Order("Ada", Line("A", quantity: 2, unitPrice: 10m), Line("B", quantity: 1, unitPrice: 5m));

        var response = await _client.PostAsJsonAsync("/orders", request);

        var order = await response.Content.ReadFromJsonAsync<OrderResponse>(Web);
        Assert.Equal(Usd(25m), order!.Total);
        Assert.Equal("Ada", order.Customer);
    }

    public void Dispose() => _client.Dispose();
}
