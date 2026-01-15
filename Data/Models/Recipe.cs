namespace Reci.Data.Models;

public class Recipe
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public Guid? GroupId { get; set; }

    public List<Ingredient> Ingredients { get; set; } = [];

    public List<Instruction> Instructions { get; set; } = [];

    public NutritionInfo? NutritionInfo { get; set; }

    public List<string> Tags { get; set; } = [];

    public string? FurtherNotes { get; set; }
}

public static class RecipeExtensions
{
    public static bool IsEmpty(this Recipe? recipe)
    {
        if (recipe is null)
        {
            return true;
        }
        return recipe.Id == Guid.Empty
            && string.IsNullOrEmpty(recipe.Name)
            && string.IsNullOrEmpty(recipe.Description)
            && recipe.GroupId is null
            && recipe.Ingredients.Count == 0
            && recipe.Instructions.Count == 0
            && recipe.NutritionInfo.IsEmpty()
            && recipe.Tags.Count == 0
            && string.IsNullOrEmpty(recipe.FurtherNotes);
    }

    public static bool IsEqualTo(this Recipe? first, Recipe? second)
    {
        if (first is null && second is null)
        {
            return true;
        }
        if (first is null || second is null)
        {
            return false;
        }
        return first.Id == second.Id
            && first.Name == second.Name
            && first.Description == second.Description
            && first.GroupId == second.GroupId
            && first.Ingredients.IsEqualTo(second.Ingredients)
            && first.Instructions.IsEqualTo(second.Instructions)
            && first.NutritionInfo.IsEqualTo(second.NutritionInfo)
            && first.Tags.SequenceEqual(second.Tags)
            && first.FurtherNotes == second.FurtherNotes;
    }

    private static bool IsEqualTo(this List<Ingredient> first, List<Ingredient> second)
    {
        if (first.Count != second.Count)
        {
            return false;
        }
        for (int i = 0; i < first.Count; i++)
        {
            if (!first[i].IsEqualTo(second[i]))
            {
                return false;
            }
        }
        return true;
    }

    private static bool IsEqualTo(this List<Instruction> first, List<Instruction> second)
    {
        if (first.Count != second.Count)
        {
            return false;
        }
        for (int i = 0; i < first.Count; i++)
        {
            if (!first[i].IsEqualTo(second[i]))
            {
                return false;
            }
        }
        return true;
    }
}
