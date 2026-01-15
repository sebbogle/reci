namespace Reci.ViewModels;

public class RecipeSummaryVM
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public List<string> Tags { get; init; } = [];
}
