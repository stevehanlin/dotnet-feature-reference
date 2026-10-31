using Records.Api;
using static Records.Tests.TestData;

namespace Records.Tests;

public class MoneyTests
{
    [Fact]
    public void GivenSameValuesIgnoringCurrencyCase_WhenComparing_ThenRecordsAreEqual()
    {
        Assert.Equal(new Money(5m, "usd"), Usd(5m));
        Assert.NotEqual(Usd(5m), Usd(6m));
    }

    [Fact]
    public void GivenMoney_WhenUsingWithExpression_ThenCopiesAndLeavesOriginalUntouched()
    {
        var original = Usd(5m);

        var changed = original with { Amount = 10m };

        Assert.Equal(5m, original.Amount);
        Assert.Equal(10m, changed.Amount);
        Assert.NotSame(original, changed);
    }

    [Fact]
    public void GivenPositionalRecord_WhenDeconstructing_ThenYieldsAmountAndNormalizedCurrency()
    {
        var (amount, currency) = new Money(3m, "eur");

        Assert.Equal(3m, amount);
        Assert.Equal("EUR", currency);
    }

    [Fact]
    public void GivenMoney_WhenCallingToString_ThenListsMembers() =>
        Assert.Equal("Money { Amount = 5, Currency = USD }", Usd(5m).ToString());

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("US")]
    [InlineData("DOLLARS")]
    public void GivenInvalidCurrencyCode_WhenConstructing_ThenThrows(string currency) =>
        Assert.Throws<ArgumentException>(() => new Money(1m, currency));

    [Fact]
    public void Gotcha_GivenInvalidCurrency_WhenUsingWithExpression_ThenInitializerValidationIsBypassed()
    {
        var money = Usd(1m);

        Assert.Throws<ArgumentException>(() => new Money(1m, "DOLLARS"));
        Assert.Equal("DOLLARS", (money with { Currency = "DOLLARS" }).Currency);
    }

    [Fact]
    public void GivenSameCurrency_WhenAdding_ThenSumsAmounts() =>
        Assert.Equal(Usd(7m), Usd(3m) + new Money(4m, "usd"));

    [Fact]
    public void GivenDifferentCurrencies_WhenAdding_ThenThrows() =>
        Assert.Throws<InvalidOperationException>(() => Usd(1m) + new Money(1m, "EUR"));
}
