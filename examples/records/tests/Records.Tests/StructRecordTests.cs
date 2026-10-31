using Records.Api;

namespace Records.Tests;

public class StructRecordTests
{
    [Fact]
    public void GivenSameGuid_WhenComparingOrderIds_ThenRecordStructsAreEqual()
    {
        var guid = Guid.NewGuid();

        Assert.Equal(new OrderId(guid), new OrderId(guid));
    }

    [Fact]
    public void GivenMutableRecordStruct_WhenCopiedThenIncremented_ThenOriginalIsUnchanged()
    {
        var original = new Counter(0);
        var copy = original;

        copy.Increment();

        Assert.Equal(0, original.Count);
        Assert.Equal(1, copy.Count);
    }

    [Fact]
    public void Gotcha_GivenEqualCollectionsInRecords_WhenComparing_ThenReferenceEqualityMakesThemDiffer() =>
        Assert.NotEqual(TestData.Order(lines: TestData.Line()), TestData.Order(lines: TestData.Line()));
}
