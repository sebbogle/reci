namespace Tests.UserActions;

public class ExportTests(AppFixture app) : ReciPage(app)
{
    [Fact]
    [ReciPageState]
    public async Task Export_FromRecipePage_TriggersDownload()
    {
        await GotoRecipeAsync("Classic Pancakes", "Breakfast");

        // Click the export button in the header.
        ILocator exportButton = Page.GetByRole(AriaRole.Button, new() { Name = "Export" });
        await exportButton.ClickAsync();

        // The shim captures downloads in window.__lastDownload.
        object? download = await Page.EvaluateAsync<object?>("() => window.__lastDownload");
        Assert.NotNull(download);
    }

    [Fact]
    [ReciPageState]
    public async Task Export_FromContentsPage_TriggersZipDownload()
    {
        await GotoContentsAsync();

        ILocator exportButton = Page.GetByRole(AriaRole.Button, new() { Name = "Export" });
        await exportButton.ClickAsync();

        object? download = await Page.EvaluateAsync<object?>("() => window.__lastDownload");
        Assert.NotNull(download);
    }
}
