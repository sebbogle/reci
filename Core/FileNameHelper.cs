namespace Reci.Core;

public static partial class FileNameHelper
{
    private const int MaxFileNameLength = 200;
    private const int GuidPrefixLength = 8;
    private const string FileExtension = ".reci";

    [GeneratedRegex(@"[<>:""/\\|?*\x00-\x1F]")]
    private static partial Regex InvalidFileNameCharsRegex();

    public static string ToFileName(string recipeName, Guid recipeId)
    {
        string sanitized = SanitizeForFileSystem(recipeName);
        string guidPrefix = recipeId.ToString("N")[..GuidPrefixLength];

        string baseName = $"{sanitized}_{guidPrefix}";

        int maxBaseLength = MaxFileNameLength - FileExtension.Length;
        if (baseName.Length > maxBaseLength)
        {
            int maxSanitizedLength = maxBaseLength - GuidPrefixLength - 1;
            sanitized = sanitized[..maxSanitizedLength].TrimEnd();
            baseName = $"{sanitized}_{guidPrefix}";
        }

        return $"{baseName}{FileExtension}";
    }

    public static string SanitizeForFileSystem(string name)
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

    public static ParsedFileName? ParseFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        if (!fileName.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string withoutExtension = fileName[..^FileExtension.Length];
        int lastUnderscore = withoutExtension.LastIndexOf('_');

        if (lastUnderscore < 0 || lastUnderscore == withoutExtension.Length - 1)
        {
            return new ParsedFileName(withoutExtension, null);
        }

        string namePart = withoutExtension[..lastUnderscore];
        string guidPrefix = withoutExtension[(lastUnderscore + 1)..];

        if (guidPrefix.Length == GuidPrefixLength)
        {
            return new ParsedFileName(namePart, guidPrefix);
        }

        return new ParsedFileName(withoutExtension, null);
    }

    public static string? FindFileByGuidPrefix(IEnumerable<string> fileNames, Guid recipeId)
    {
        string guidPrefix = recipeId.ToString("N")[..GuidPrefixLength];

        return fileNames.FirstOrDefault(f =>
        {
            ParsedFileName? parsed = ParseFileName(f);
            return parsed?.GuidPrefix != null
                && parsed.GuidPrefix.Equals(guidPrefix, StringComparison.OrdinalIgnoreCase);
        });
    }
}

public record ParsedFileName(string Name, string? GuidPrefix);
