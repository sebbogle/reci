namespace Reci.Core;

public static partial class FileNameHelper
{
    private const int MaxFileNameLength = 200;
    private const string FileExtension = ".reci";

    [GeneratedRegex(@"[<>:""/\\|?*\x00-\x1F]")]
    private static partial Regex InvalidFileNameCharsRegex();

    public static string ToFileName(string recipeName)
    {
        string sanitized = SanitizeForFileSystem(recipeName);

        int maxBaseLength = MaxFileNameLength - FileExtension.Length;
        if (sanitized.Length > maxBaseLength)
        {
            sanitized = sanitized[..maxBaseLength].TrimEnd();
        }

        return $"{sanitized}{FileExtension}";
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
}
