namespace Reci.Services;

public partial class FilePathService : IFilePathService
{
    private const int MaxFileNameLength = 200;
    private const int MaxGroupNameLength = 200;
    private const int MaxCombinedPathLength = 240;
    private const string FileExtension = ".reci";

    [GeneratedRegex(@"[<>:""/\\|?*\x00-\x1F]")]
    private static partial Regex InvalidFileNameCharsRegex();

    public string SanitizeForFileSystem(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Untitled";
        }

        string sanitized = InvalidFileNameCharsRegex().Replace(name, "");
        sanitized = sanitized.Trim().TrimEnd('.');

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return "Untitled";
        }

        return sanitized;
    }

    public string ToFileName(string recipeName)
    {
        string sanitized = SanitizeForFileSystem(recipeName);

        int maxBaseLength = MaxFileNameLength - FileExtension.Length;
        if (sanitized.Length > maxBaseLength)
        {
            sanitized = sanitized[..maxBaseLength].TrimEnd();
        }

        return $"{sanitized}{FileExtension}";
    }

    public string? ValidateFileName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Recipe name is required.";
        }

        if (name != name.Trim())
        {
            return "Recipe name must not have leading or trailing spaces.";
        }

        if (name.EndsWith('.'))
        {
            return "Recipe name must not end with a period.";
        }

        if (InvalidFileNameCharsRegex().IsMatch(name))
        {
            return "Recipe name contains invalid characters: < > : \" / \\ | ? *";
        }

        int maxBaseLength = MaxFileNameLength - FileExtension.Length;
        if (name.Length > maxBaseLength)
        {
            return $"Recipe name is too long (maximum {maxBaseLength} characters).";
        }

        return null;
    }

    public string? ValidateGroupName(string? group)
    {
        if (string.IsNullOrEmpty(group))
        {
            return null;
        }

        if (group != group.Trim())
        {
            return "Group name must not have leading or trailing spaces.";
        }

        if (group.EndsWith('.'))
        {
            return "Group name must not end with a period.";
        }

        if (group is "." or "..")
        {
            return "Group name cannot be '.' or '..'.";
        }

        if (InvalidFileNameCharsRegex().IsMatch(group))
        {
            return "Group name contains invalid characters: < > : \" / \\ | ? *";
        }

        if (group.Length > MaxGroupNameLength)
        {
            return $"Group name is too long (maximum {MaxGroupNameLength} characters).";
        }

        return null;
    }

    public string? ValidateRecipePath(string? name, string? group)
    {
        string? nameError = ValidateFileName(name);
        if (nameError is not null)
        {
            return nameError;
        }

        string? groupError = ValidateGroupName(group);
        if (groupError is not null)
        {
            return groupError;
        }

        int combinedLength = name!.Length + FileExtension.Length;
        if (!string.IsNullOrEmpty(group))
        {
            combinedLength += group.Length + 1;
        }

        if (combinedLength > MaxCombinedPathLength)
        {
            return "The combined recipe name and group name is too long.";
        }

        return null;
    }
}
