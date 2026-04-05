namespace Tests.Unit.Data;

public class RecipeExtensionsTests
{
    private static Recipe MakeEmptyRecipe() => new() { Name = "" };

    private static Recipe MakeFullRecipe() => new()
    {
        Name = "Pancakes",
        Description = "Fluffy",
        Group = "Breakfast",
        Ingredients = [new Ingredient { Name = "Flour", QuantityAmount = 2, QuantityUnit = "cups" }],
        Instructions = [new Instruction { Text = "Mix ingredients" }],
        NutritionInfo = new NutritionInfo { Calories = 350 },
        Source = new RecipeSource { Text = "Family Recipe" },
        Tags = ["breakfast"],
        FurtherNotes = "Let batter rest"
    };

    #region IsEmpty

    [Fact]
    public void IsEmpty_Null_ReturnsTrue()
    {
        Assert.True(((Recipe?)null).IsEmpty());
    }

    [Fact]
    public void IsEmpty_AllFieldsEmpty_ReturnsTrue()
    {
        Recipe recipe = MakeEmptyRecipe();
        Assert.True(recipe.IsEmpty());
    }

    [Fact]
    public void IsEmpty_HasName_ReturnsFalse()
    {
        Recipe recipe = MakeEmptyRecipe();
        recipe.Name = "Pancakes";
        Assert.False(recipe.IsEmpty());
    }

    [Fact]
    public void IsEmpty_OnlyEmptyIngredients_ReturnsTrue()
    {
        Recipe recipe = MakeEmptyRecipe();
        recipe.Ingredients.Add(new Ingredient { Name = "", QuantityAmount = 0, QuantityUnit = "" });
        Assert.True(recipe.IsEmpty());
    }

    [Fact]
    public void IsEmpty_HasNonEmptyIngredient_ReturnsFalse()
    {
        Recipe recipe = MakeEmptyRecipe();
        recipe.Ingredients.Add(new Ingredient { Name = "Flour", QuantityAmount = 2, QuantityUnit = "cups" });
        Assert.False(recipe.IsEmpty());
    }

    [Fact]
    public void IsEmpty_HasTags_ReturnsFalse()
    {
        Recipe recipe = MakeEmptyRecipe();
        recipe.Tags.Add("breakfast");
        Assert.False(recipe.IsEmpty());
    }

    [Fact]
    public void IsEmpty_HasDescription_ReturnsFalse()
    {
        Recipe recipe = MakeEmptyRecipe();
        recipe.Description = "A description";
        Assert.False(recipe.IsEmpty());
    }

    #endregion

    #region Sanitise

    [Fact]
    public void Sanitise_RemovesEmptyIngredients()
    {
        Recipe recipe = MakeFullRecipe();
        recipe.Ingredients.Add(new Ingredient { Name = "", QuantityAmount = 0, QuantityUnit = "" });
        recipe.Sanitise();
        Assert.Single(recipe.Ingredients);
        Assert.Equal("Flour", recipe.Ingredients[0].Name);
    }

    [Fact]
    public void Sanitise_RemovesEmptyInstructions()
    {
        Recipe recipe = MakeFullRecipe();
        recipe.Instructions.Add(new Instruction { Text = "" });
        recipe.Sanitise();
        Assert.Single(recipe.Instructions);
    }

    [Fact]
    public void Sanitise_NullsEmptyNutritionInfo()
    {
        Recipe recipe = MakeFullRecipe();
        recipe.NutritionInfo = new NutritionInfo();
        recipe.Sanitise();
        Assert.Null(recipe.NutritionInfo);
    }

    [Fact]
    public void Sanitise_NullsEmptySource()
    {
        Recipe recipe = MakeFullRecipe();
        recipe.Source = new RecipeSource { Text = "" };
        recipe.Sanitise();
        Assert.Null(recipe.Source);
    }

    [Fact]
    public void Sanitise_NullRecipe_NoException()
    {
        ((Recipe?)null).Sanitise();
    }

    #endregion

    #region DeepClone

    [Fact]
    public void DeepClone_AllFieldsCopied()
    {
        Recipe original = MakeFullRecipe();
        Recipe clone = original.DeepClone();

        Assert.Equal(original.Name, clone.Name);
        Assert.Equal(original.Description, clone.Description);
        Assert.Equal(original.Group, clone.Group);
        Assert.Equal(original.Ingredients.Count, clone.Ingredients.Count);
        Assert.Equal(original.Instructions.Count, clone.Instructions.Count);
        Assert.Equal(original.NutritionInfo?.Calories, clone.NutritionInfo?.Calories);
        Assert.Equal(original.Source?.Text, clone.Source?.Text);
        Assert.Equal(original.Tags, clone.Tags);
        Assert.Equal(original.FurtherNotes, clone.FurtherNotes);
    }

    [Fact]
    public void DeepClone_ModifyingClone_DoesNotAffectOriginal()
    {
        Recipe original = MakeFullRecipe();
        Recipe clone = original.DeepClone();

        clone.Name = "Waffles";
        clone.Ingredients[0] = clone.Ingredients[0] with { Name = "Sugar" };
        clone.Tags.Add("new-tag");

        Assert.Equal("Pancakes", original.Name);
        Assert.Equal("Flour", original.Ingredients[0].Name);
        Assert.Single(original.Tags);
    }

    [Fact]
    public void DeepClone_ListsAreIndependent()
    {
        Recipe original = MakeFullRecipe();
        Recipe clone = original.DeepClone();

        clone.Ingredients.Clear();
        Assert.Single(original.Ingredients);
    }

    #endregion

    #region IsEqualTo

    [Fact]
    public void IsEqualTo_SameRecipe_ReturnsTrue()
    {
        Recipe a = MakeFullRecipe();
        Recipe b = MakeFullRecipe();
        Assert.True(a.IsEqualTo(b));
    }

    [Fact]
    public void IsEqualTo_DifferentName_ReturnsFalse()
    {
        Recipe a = MakeFullRecipe();
        Recipe b = MakeFullRecipe();
        b.Name = "Waffles";
        Assert.False(a.IsEqualTo(b));
    }

    [Fact]
    public void IsEqualTo_DifferentIngredients_ReturnsFalse()
    {
        Recipe a = MakeFullRecipe();
        Recipe b = MakeFullRecipe();
        b.Ingredients.Add(new Ingredient { Name = "Sugar", QuantityAmount = 1, QuantityUnit = "tsp" });
        Assert.False(a.IsEqualTo(b));
    }

    [Fact]
    public void IsEqualTo_BothNull_ReturnsTrue()
    {
        Assert.True(((Recipe?)null).IsEqualTo(null));
    }

    [Fact]
    public void IsEqualTo_OneNull_ReturnsFalse()
    {
        Recipe a = MakeFullRecipe();
        Assert.False(a.IsEqualTo(null));
        Assert.False(((Recipe?)null).IsEqualTo(a));
    }

    #endregion
}
