namespace Tests.Views;

public class ConnectionStateTests(AppFixture app) : ReciPage(app)
{
    [Fact]
    public async Task InitialLoad_NoShim_ShowsConnectOrUnsupported()
    {
        // No [ReciPageState] attribute: no shim injected.
        // Without File System Access API support in headless Chromium,
        // the app will show either ConnectFolder or BrowserNotSupported.
        await Page.GotoAsync("http://localhost:5265");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        ILocator body = Page.Locator("body");

        // Should show some connection prompt or browser warning -- not the recipe listing.
        bool hasConnect = await body.GetByText("Connect", new() { Exact = false }).CountAsync() > 0;
        bool hasBrowserWarning = await body.GetByText("not supported", new() { Exact = false }).CountAsync() > 0;

        Assert.True(hasConnect || hasBrowserWarning,
            "Expected ConnectFolder or BrowserNotSupported screen when no shim is injected");
    }

    [Fact]
    [ReciPageState("EmptyState")]
    public async Task ConnectedEmpty_ShowsEmptyState()
    {
        await GotoContentsAsync();
        await Expect(Page).ToHaveTitleAsync("Contents");

        // Connected with empty recipes -- should show some "no recipes" indicator
        // rather than the ConnectFolder screen.
        ILocator body = Page.Locator("body");
        bool hasConnect = await body.GetByText("Connect Folder", new() { Exact = false }).CountAsync() > 0;
        Assert.False(hasConnect, "Should not show ConnectFolder when connected via shim");
    }
}
