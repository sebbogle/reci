using System.ComponentModel.DataAnnotations;

namespace Reci.Data.Models;

public class Recipe
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Recipe name is required.")]
    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? Group { get; set; }

    public List<Ingredient> Ingredients { get; set; } = [];

    public List<Instruction> Instructions { get; set; } = [];

    public NutritionInfo? NutritionInfo { get; set; }

    public RecipeSource? Source { get; set; }

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
            && recipe.Group is null
            && !recipe.Ingredients.Any(ingredient => !ingredient.IsEmpty())
            && !recipe.Instructions.Any(instruction => !instruction.IsEmpty())
            && recipe.NutritionInfo.IsEmpty()
            && recipe.Source.IsEmpty()
            && recipe.Tags.Count == 0
            && string.IsNullOrEmpty(recipe.FurtherNotes);
    }

    public static Recipe DeepClone(this Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        return new Recipe
        {
            Id = recipe.Id,
            Name = recipe.Name,
            Description = recipe.Description,
            Group = recipe.Group,
            Ingredients = recipe.Ingredients.Select(i => i with { }).ToList(),
            Instructions = recipe.Instructions.Select(i => i with { }).ToList(),
            NutritionInfo = recipe.NutritionInfo is not null ? recipe.NutritionInfo with { } : null,
            Source = recipe.Source is not null ? recipe.Source with { } : null,
            Tags = [.. recipe.Tags],
            FurtherNotes = recipe.FurtherNotes
        };
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
            && first.Group == second.Group
            && first.Ingredients.SequenceEqual(second.Ingredients)
            && first.Instructions.SequenceEqual(second.Instructions)
            && first.NutritionInfo == second.NutritionInfo
            && first.Source == second.Source
            && first.Tags.SequenceEqual(second.Tags)
            && first.FurtherNotes == second.FurtherNotes;
    }

}
