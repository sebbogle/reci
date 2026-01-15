namespace Reci.Core;

public static class Version
{
    public const int Major = 0;
    public const int Minor = 1;
    public const int Patch = 0;

    public static string GetVersionString() => $"{Major}.{Minor}.{Patch}";
}
