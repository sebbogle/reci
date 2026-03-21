namespace Reci.Data.Models;

public record SearchResult
{
    public required string Name { get; init; }

    public required SearchResultKind Kind { get; init; }

    public required MatchMethod MatchMethod { get; init; }

    public required string NavigationUrl { get; init; }
}

public enum SearchResultKind
{
    Recipe,
    Group
}

public enum MatchMethod
{
    Name,
    Tag,
    Description
}
