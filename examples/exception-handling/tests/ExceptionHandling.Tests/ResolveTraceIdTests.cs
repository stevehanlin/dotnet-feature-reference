using System.Diagnostics;
using ExceptionHandling.Api;

namespace ExceptionHandling.Tests;

public class ResolveTraceIdTests
{
    private const string Fallback = "0HN-fallback";

    [Fact]
    public void GivenNoActivity_WhenResolvingTraceId_ThenUsesFallback() =>
        Assert.Equal(Fallback, ErrorHandlingExtensions.ResolveTraceId(null, Fallback));

    [Fact]
    public void GivenActivityThatNeverStarted_WhenResolvingTraceId_ThenUsesFallbackBecauseIdIsNull()
    {
        var notStarted = new Activity("never-started");

        Assert.Equal(Fallback, ErrorHandlingExtensions.ResolveTraceId(notStarted, Fallback));
    }

    [Fact]
    public void GivenStartedActivity_WhenResolvingTraceId_ThenUsesActivityId()
    {
        using var source = new ActivitySource("tests");
        using var listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == "tests",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(listener);
        using var activity = source.StartActivity("work");

        Assert.Equal(activity!.Id, ErrorHandlingExtensions.ResolveTraceId(activity, Fallback));
    }
}
