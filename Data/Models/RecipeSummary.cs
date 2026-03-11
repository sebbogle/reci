namespace Reci.Data.Models;

public record RecipeSummary
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public string? Group { get; init; }

    public List<string> Tags { get; init; } = [];

    public static RecipeSummary FromRecipe(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        return new RecipeSummary
        {
            Id = recipe.Id,
            Name = recipe.Name,
            Description = recipe.Description,
            Group = recipe.Group,
            Tags = recipe.Tags
        };
    }
}
