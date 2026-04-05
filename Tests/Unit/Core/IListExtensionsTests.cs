namespace Tests.Unit.Core;

public class IListExtensionsTests
{
    [Fact]
    public void Replace_MatchFound_ReplacesItem_ReturnsTrue()
    {
        List<string> list = ["a", "b", "c"];
        bool result = list.Replace("B", item => item == "b");
        Assert.True(result);
        Assert.Equal(["a", "B", "c"], list);
    }

    [Fact]
    public void Replace_NoMatch_ReturnsFalse_ListUnchanged()
    {
        List<string> list = ["a", "b", "c"];
        bool result = list.Replace("X", item => item == "z");
        Assert.False(result);
        Assert.Equal(["a", "b", "c"], list);
    }

    [Fact]
    public void Replace_MultipleMatches_ReplacesFirst()
    {
        List<int> list = [1, 2, 2, 3];
        bool result = list.Replace(99, item => item == 2);
        Assert.True(result);
        Assert.Equal([1, 99, 2, 3], list);
    }

    [Fact]
    public void Replace_NullSource_ThrowsArgumentNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            IListExtensions.Replace<string>(null!, "x", _ => true));
    }

    [Fact]
    public void Replace_NullPredicate_ThrowsArgumentNull()
    {
        List<string> list = ["a"];
        Assert.Throws<ArgumentNullException>(() =>
            list.Replace("x", null!));
    }

    [Fact]
    public void Replace_EmptyList_ReturnsFalse()
    {
        List<string> list = [];
        Assert.False(list.Replace("x", _ => true));
    }
}
