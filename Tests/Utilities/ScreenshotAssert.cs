namespace Tests.Utilities;

public static class ScreenshotAssert
{
    private static readonly string _goldenDirectory = Path.Combine(
        ProjectPaths.FindRoot(), "Tests", "Resources", "Golden Screenshots");

    private static readonly string _actualDirectory = Path.Combine(
        AppContext.BaseDirectory, "Temp Test Screenshots");

    public static async Task MatchesAsync(IPage page, string name, bool fullPage = true)
    {
        byte[] actual = await page.ScreenshotAsync(new PageScreenshotOptions
        {
            FullPage = fullPage,
        });

        string goldenPath = Path.Combine(_goldenDirectory, $"{name}.png");
        string actualPath = Path.Combine(_actualDirectory, $"{name}.actual.png");

        bool updateSnapshots = string.Equals(
            Environment.GetEnvironmentVariable("UPDATE_SNAPSHOTS"), "true", StringComparison.OrdinalIgnoreCase);

        if (!File.Exists(goldenPath) || updateSnapshots)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(goldenPath)!);
            await File.WriteAllBytesAsync(goldenPath, actual);

            if (File.Exists(actualPath))
            {
                File.Delete(actualPath);
            }

            if (updateSnapshots)
            {
                return;
            }

            Assert.Fail(
                $"Golden screenshot '{name}.png' did not exist and has been created at:\n" +
                $"  {goldenPath}\n" +
                "Review it and re-run the test to verify.");
            return;
        }

        byte[] expected = await File.ReadAllBytesAsync(goldenPath);

        if (actual.AsSpan().SequenceEqual(expected))
        {
            if (File.Exists(actualPath))
            {
                File.Delete(actualPath);
            }

            return;
        }

        Directory.CreateDirectory(_actualDirectory);
        await File.WriteAllBytesAsync(actualPath, actual);
        Assert.Fail(
            $"Screenshot '{name}' does not match golden file.\n" +
            $"  Golden:  {goldenPath}\n" +
            $"  Actual:  {actualPath}\n" +
            "To update the golden file, set UPDATE_SNAPSHOTS=true and re-run.");
    }
}
