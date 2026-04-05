namespace Tests.Unit.Core;

public class RecipeKeyTests
{
    #region ToSlug

    [Fact]
    public void ToSlug_NameOnly_ReturnsEncodedName()
    {
        RecipeKey key = new("Classic Pancakes", null);
        Assert.Equal("Classic%20Pancakes", key.ToSlug());
    }

    [Fact]
    public void ToSlug_WithGroup_ReturnsGroupSlashName()
    {
        RecipeKey key = new("Classic Pancakes", "Breakfast");
        Assert.Equal("Breakfast/Classic%20Pancakes", key.ToSlug());
    }

    [Fact]
    public void ToSlug_SpecialCharacters_UrlEncoded()
    {
        RecipeKey key = new("Mac & Cheese", "Dinner & Sides");
        string slug = key.ToSlug();
        Assert.Equal("Dinner%20%26%20Sides/Mac%20%26%20Cheese", slug);
    }

    [Fact]
    public void ToSlug_EmptyGroup_ReturnsNameOnly()
    {
        RecipeKey key = new("Cookies", "");
        Assert.Equal("Cookies", key.ToSlug());
    }

    [Fact]
    public void ToSlug_NullGroup_ReturnsNameOnly()
    {
        RecipeKey key = new("Cookies", null);
        Assert.Equal("Cookies", key.ToSlug());
    }

    #endregion

    #region FromSlug

    [Fact]
    public void FromSlug_NameOnly_ReturnsKeyWithNullGroup()
    {
        RecipeKey key = RecipeKey.FromSlug("Classic%20Pancakes");
        Assert.Equal("Classic Pancakes", key.Name);
        Assert.Null(key.Group);
    }

    [Fact]
    public void FromSlug_GroupSlashName_ReturnsBoth()
    {
        RecipeKey key = RecipeKey.FromSlug("Breakfast/Classic%20Pancakes");
        Assert.Equal("Classic Pancakes", key.Name);
        Assert.Equal("Breakfast", key.Group);
    }

    [Fact]
    public void FromSlug_UrlEncodedCharacters_DecodesCorrectly()
    {
        RecipeKey key = RecipeKey.FromSlug("Dinner%20%26%20Sides/Mac%20%26%20Cheese");
        Assert.Equal("Mac & Cheese", key.Name);
        Assert.Equal("Dinner & Sides", key.Group);
    }

    [Fact]
    public void FromSlug_EmptyString_ReturnsDefault()
    {
        RecipeKey key = RecipeKey.FromSlug("");
        Assert.Equal(default, key);
    }

    [Fact]
    public void FromSlug_NullString_ReturnsDefault()
    {
        RecipeKey key = RecipeKey.FromSlug(null!);
        Assert.Equal(default, key);
    }

    #endregion

    #region Roundtrip

    [Fact]
    public void Roundtrip_SimpleNames()
    {
        RecipeKey original = new("Pancakes", "Breakfast");
        RecipeKey roundtripped = RecipeKey.FromSlug(original.ToSlug());
        Assert.Equal(original, roundtripped);
    }

    [Fact]
    public void Roundtrip_SpecialCharacters()
    {
        RecipeKey original = new("Mac & Cheese (Homemade)", "Dinner & Sides");
        RecipeKey roundtripped = RecipeKey.FromSlug(original.ToSlug());
        Assert.Equal(original, roundtripped);
    }

    [Fact]
    public void Roundtrip_UnicodeCharacters()
    {
        RecipeKey original = new("Crème Brûlée", "Français");
        RecipeKey roundtripped = RecipeKey.FromSlug(original.ToSlug());
        Assert.Equal(original, roundtripped);
    }

    [Fact]
    public void Roundtrip_NoGroup()
    {
        RecipeKey original = new("Cookies", null);
        RecipeKey roundtripped = RecipeKey.FromSlug(original.ToSlug());
        Assert.Equal(original, roundtripped);
    }

    #endregion

    #region Equality

    [Fact]
    public void Equals_SameNameAndGroup_CaseInsensitive()
    {
        RecipeKey a = new("Pancakes", "Breakfast");
        RecipeKey b = new("PANCAKES", "BREAKFAST");
        Assert.Equal(a, b);
    }

    [Fact]
    public void Equals_DifferentName_NotEqual()
    {
        RecipeKey a = new("Pancakes", "Breakfast");
        RecipeKey b = new("Waffles", "Breakfast");
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Equals_DifferentGroup_NotEqual()
    {
        RecipeKey a = new("Pancakes", "Breakfast");
        RecipeKey b = new("Pancakes", "Dinner");
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void GetHashCode_CaseInsensitive_SameForEquivalent()
    {
        RecipeKey a = new("Pancakes", "Breakfast");
        RecipeKey b = new("pancakes", "breakfast");
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equals_NullGroupVsEmptyGroup_NotEqual()
    {
        RecipeKey a = new("Cookies", null);
        RecipeKey b = new("Cookies", "");
        // null and "" differ because StringComparer.OrdinalIgnoreCase.Equals(null, "") is false
        Assert.NotEqual(a, b);
    }

    #endregion
}
