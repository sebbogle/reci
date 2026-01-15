namespace Reci.Core;

public class ReciFile
{
    public required string Version { get; init; }

    public Settings? Settings { get; init; }
    
    public List<Recipe>? Recipes { get; init; }

    public List<Group>? Groups { get; init; }
}
