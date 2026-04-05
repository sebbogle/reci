namespace Tests.Unit.Data;

public class IngredientIsEmptyTests
{
    [Fact]
    public void IsEmpty_Null_ReturnsTrue()
    {
        Assert.True(((Ingredient?)null).IsEmpty());
    }

    [Fact]
    public void IsEmpty_AllDefaults_ReturnsTrue()
    {
        Ingredient ingredient = new() { Name = "", QuantityAmount = 0, QuantityUnit = "" };
        Assert.True(ingredient.IsEmpty());
    }

    [Fact]
    public void IsEmpty_HasName_ReturnsFalse()
    {
        Ingredient ingredient = new() { Name = "Flour", QuantityAmount = 0, QuantityUnit = "" };
        Assert.False(ingredient.IsEmpty());
    }

    [Fact]
    public void IsEmpty_HasQuantityOnly_ReturnsFalse()
    {
        Ingredient ingredient = new() { Name = "", QuantityAmount = 2.5m, QuantityUnit = "" };
        Assert.False(ingredient.IsEmpty());
    }

    [Fact]
    public void IsEmpty_HasUnitOnly_ReturnsFalse()
    {
        Ingredient ingredient = new() { Name = "", QuantityAmount = 0, QuantityUnit = "cups" };
        Assert.False(ingredient.IsEmpty());
    }
}

public class InstructionIsEmptyTests
{
    [Fact]
    public void IsEmpty_Null_ReturnsTrue()
    {
        Assert.True(((Instruction?)null).IsEmpty());
    }

    [Fact]
    public void IsEmpty_EmptyText_ReturnsTrue()
    {
        Instruction instruction = new() { Text = "" };
        Assert.True(instruction.IsEmpty());
    }

    [Fact]
    public void IsEmpty_HasText_ReturnsFalse()
    {
        Instruction instruction = new() { Text = "Mix ingredients" };
        Assert.False(instruction.IsEmpty());
    }

    [Fact]
    public void IsEmpty_NullText_ReturnsTrue()
    {
        Instruction instruction = new() { Text = null! };
        Assert.True(instruction.IsEmpty());
    }
}

public class NutritionInfoIsEmptyTests
{
    [Fact]
    public void IsEmpty_Null_ReturnsTrue()
    {
        Assert.True(((NutritionInfo?)null).IsEmpty());
    }

    [Fact]
    public void IsEmpty_AllNull_ReturnsTrue()
    {
        NutritionInfo info = new();
        Assert.True(info.IsEmpty());
    }

    [Fact]
    public void IsEmpty_HasCalories_ReturnsFalse()
    {
        NutritionInfo info = new() { Calories = 350 };
        Assert.False(info.IsEmpty());
    }

    [Fact]
    public void IsEmpty_HasOnlyProtein_ReturnsFalse()
    {
        NutritionInfo info = new() { Protein = 9.0m };
        Assert.False(info.IsEmpty());
    }
}

public class RecipeSourceIsEmptyTests
{
    [Fact]
    public void IsEmpty_Null_ReturnsTrue()
    {
        Assert.True(((RecipeSource?)null).IsEmpty());
    }

    [Fact]
    public void IsEmpty_WhitespaceTextAndNullUrl_ReturnsTrue()
    {
        RecipeSource source = new() { Text = "   " };
        Assert.True(source.IsEmpty());
    }

    [Fact]
    public void IsEmpty_HasText_ReturnsFalse()
    {
        RecipeSource source = new() { Text = "Family Recipe" };
        Assert.False(source.IsEmpty());
    }

    [Fact]
    public void IsEmpty_HasUrlOnly_ReturnsFalse()
    {
        RecipeSource source = new() { Text = "", Url = "https://example.com" };
        Assert.False(source.IsEmpty());
    }
}
