namespace Reci.Core;

public readonly record struct RecipeKey(string Name, string? Group) : IEquatable<RecipeKey>
{
    public bool Equals(RecipeKey other)
    {
        return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Group, other.Group, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(Name),
            Group is not null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Group) : 0);
    }

    public string ToSlug()
    {
        string encodedName = Uri.EscapeDataString(Name);

        if (string.IsNullOrEmpty(Group))
        {
            return encodedName;
        }

        string encodedGroup = Uri.EscapeDataString(Group);
        return $"{encodedGroup}/{encodedName}";
    }

    public static RecipeKey FromSlug(string slug)
    {
        if (string.IsNullOrEmpty(slug))
        {
            return default;
        }

        int separatorIndex = slug.IndexOf('/');

        if (separatorIndex < 0)
        {
            return new RecipeKey(Uri.UnescapeDataString(slug), null);
        }

        string group = Uri.UnescapeDataString(slug[..separatorIndex]);
        string name = Uri.UnescapeDataString(slug[(separatorIndex + 1)..]);
        return new RecipeKey(name, group);
    }
}
