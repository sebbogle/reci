
using System.Reflection;

namespace Tests.Utilities;

[Collection(AppCollection.Name)]
public abstract class ReciPage : PageTest
{
    private readonly AppFixture _app;
    private bool _stateInitialized;

    protected ReciPage(AppFixture app)
    {
        ArgumentNullException.ThrowIfNull(app);
        _app = app;
    }

    protected virtual string? StateFilePath => null;

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

    public async Task GotoRecipeAsync(Guid recipeId)
    {
        await EnsureStateInitializedAsync();
        await Page.GotoAsync($"{_app.BaseUrl}/recipe/{recipeId}");
        await WaitForAppReadyAsync();
    }

    public async Task GotoGroupAsync(Guid groupId)
    {
        await EnsureStateInitializedAsync();
        await Page.GotoAsync($"{_app.BaseUrl}/group/{groupId}");
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
            return;
        }

        string path = attr.StateName is not null
            ? Path.Combine(AppContext.BaseDirectory, "Resources", $"{attr.StateName}.json")
            : StateFilePath ?? GetDefaultStatePath();

        string json = await File.ReadAllTextAsync(path);

        await Page.Context.AddInitScriptAsync(
            $$"""
            (() => {
                const state = {{json}};
                for (const [key, value] of Object.entries(state)) {
                    localStorage.setItem(key, JSON.stringify(value));
                }
            })();
            """);
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
