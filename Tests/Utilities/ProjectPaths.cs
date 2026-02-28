namespace Tests.Utilities;

public static class ProjectPaths
{
    public static string FindRoot()
    {
        string? directory = AppContext.BaseDirectory;

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory, "Reci.slnx")))
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new DirectoryNotFoundException(
            "Could not find project root. Looked for Reci.slnx starting from: " + AppContext.BaseDirectory);
    }
}
