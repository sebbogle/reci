namespace Reci.Data.Models;

public record RecipeSummary
{
    public required string Name { get; init; }

    public string? Description { get; init; }

    public string? Group { get; init; }

    public List<string> Tags { get; init; } = [];

    public string Slug => new RecipeKey(Name, Group).ToSlug();

    public static RecipeSummary FromRecipe(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        return new RecipeSummary
        {
            Name = recipe.Name,
            Description = recipe.Description,
            Group = recipe.Group,
            Tags = recipe.Tags
        };
    }
}
