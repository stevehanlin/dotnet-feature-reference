using Records.Api;

namespace Records.Tests;

public class EmailTests
{
    [Fact]
    public void GivenMixedCaseAddress_WhenInitializing_ThenNormalizesToLowerCase() =>
        Assert.Equal("ada@example.com", new Email { Value = "Ada@Example.com" }.Value);

    [Theory]
    [InlineData("nope")]
    [InlineData("@example.com")]
    public void GivenInvalidAddress_WhenInitializing_ThenThrows(string value) =>
        Assert.Throws<ArgumentException>(() => new Email { Value = value });

    [Fact]
    public void GivenValidEmail_WhenUsingWithExpressionWithInvalidValue_ThenAccessorValidationStillApplies()
    {
        var email = new Email { Value = "ada@example.com" };

        Assert.Throws<ArgumentException>(() => email with { Value = "nope" });
    }
}
