
namespace Tests.Utilities;

[Collection(AppCollection.Name)]
public abstract class ReciPage : PageTest
{
    private readonly AppFixture _app;
    private bool _stateInitialized;

    private static readonly string _shimPath = Path.Combine(
        AppContext.BaseDirectory, "Resources", "fileSystemShim.js");

    protected ReciPage(AppFixture app)
    {
        ArgumentNullException.ThrowIfNull(app);
        _app = app;
    }

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
        };
    }

    public async Task GotoContentsAsync()
    {
        await EnsureStateInitializedAsync();
        await Page.GotoAsync(_app.BaseUrl);
        await WaitForAppReadyAsync();
    }

    public async Task GotoRecipeAsync(string recipeName, string? group = null)
    {
        await EnsureStateInitializedAsync();
        string slug = group is not null
            ? $"{Uri.EscapeDataString(group)}/{Uri.EscapeDataString(recipeName)}"
            : Uri.EscapeDataString(recipeName);
        await Page.GotoAsync($"{_app.BaseUrl}/recipe/{slug}");
        await WaitForAppReadyAsync();
    }

    public async Task GotoGroupAsync(string groupName)
    {
        await EnsureStateInitializedAsync();
        await Page.GotoAsync($"{_app.BaseUrl}/group/{Uri.EscapeDataString(groupName)}");
        await WaitForAppReadyAsync();
    }

    private async Task EnsureStateInitializedAsync()
    {
        if (_stateInitialized)
        {
            return;
        }

        _stateInitialized = true;

        string testMethodName = TestContext.Current.TestMethod!.MethodName;

        MethodInfo? method = GetType().GetMethod(
            testMethodName, BindingFlags.Public | BindingFlags.Instance);

        ReciPageStateAttribute? attr = method?.GetCustomAttribute<ReciPageStateAttribute>();

        if (attr is null)
        {
            // No attribute: no shim injected, app shows natural unconnected state.
            return;
        }

        if (!attr.Connected)
        {
            // Explicitly disconnected: no shim, app falls through to ConnectFolder.
            return;
        }

        // Load the shim script.
        string shimScript = await File.ReadAllTextAsync(_shimPath);

        // Load recipe data from the state file.
        string statePath = attr.StateName is not null
            ? Path.Combine(AppContext.BaseDirectory, "Resources", $"{attr.StateName}.json")
            : GetDefaultStatePath();

        string recipesJson = await File.ReadAllTextAsync(statePath);

        // Inject: set test recipes, then run the shim which reads them.
        string initScript =
            $$"""
            window.__testRecipes = {{recipesJson}};
            {{shimScript}}
            """;

        await Page.Context.AddInitScriptAsync(initScript);
    }

    protected async Task WaitForNavigationIdleAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.WaitForFunctionAsync(
            "() => !document.querySelector('fluent-progress-ring')",
            arg: null,
            new PageWaitForFunctionOptions { Timeout = 30_000 });
    }

    private async Task WaitForAppReadyAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.WaitForFunctionAsync(
            "() => !document.querySelector('fluent-progress-ring')",
            arg: null,
            new PageWaitForFunctionOptions { Timeout = 30_000 });
    }

    private static string GetDefaultStatePath()
    {
        return Path.Combine(AppContext.BaseDirectory, "Resources", "BaseState.json");
    }
}
