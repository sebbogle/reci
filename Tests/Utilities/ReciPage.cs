
namespace Tests.Utilities;

[Collection(AppCollection.Name)]
public abstract class ReciPage : PageTest
{
    private static readonly string _mockScriptTemplate = File.ReadAllText(
        Path.Combine(ProjectPaths.FindRoot(), "Tests", "Resources", "fileSystemMock.js"));

    private readonly AppFixture _app;
    private bool _stateInitialized;

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

    public async Task GotoRecipeAsync(Guid recipeId)
    {
        await EnsureStateInitializedAsync();
        await Page.GotoAsync($"{_app.BaseUrl}/recipe/{recipeId}");
        await WaitForAppReadyAsync();
    }

    public async Task GotoGroupAsync(string groupName)
    {
        await EnsureStateInitializedAsync();
        await Page.GotoAsync($"{_app.BaseUrl}/group/{groupName}");
        await WaitForAppReadyAsync();
    }

    private async Task EnsureStateInitializedAsync()
    {
        if (_stateInitialized)
        {
            return;
        }

        _stateInitialized = true;

        // Block the real fileSystemHelper.js so it can't overwrite our mock
        await Page.RouteAsync("**/js/fileSystemHelper.js", route =>
            route.FulfillAsync(new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "application/javascript",
                Body = "// blocked by test mock",
            }));

        string filesJson = BuildTestFilesJson();
        string mockScript = _mockScriptTemplate.Replace("__TEST_FILES__", filesJson);

        await Page.Context.AddInitScriptAsync(mockScript);
    }

    private string BuildTestFilesJson()
    {
        string? stateName = ResolveStateName();

        if (stateName is null)
        {
            return "{}";
        }

        string recipesDir = Path.Combine(
            ProjectPaths.FindRoot(), "Tests", "Resources", "Recipes", stateName);

        if (!Directory.Exists(recipesDir))
        {
            return "{}";
        }

        Dictionary<string, string> fileMap = [];

        // Scan root-level .reci files (ungrouped recipes)
        foreach (string file in Directory.GetFiles(recipesDir, "*.reci"))
        {
            string relativePath = Path.GetFileName(file);
            fileMap[relativePath] = File.ReadAllText(file);
        }

        // Scan subdirectories (grouped recipes)
        foreach (string subDir in Directory.GetDirectories(recipesDir))
        {
            string groupName = Path.GetFileName(subDir);
            foreach (string file in Directory.GetFiles(subDir, "*.reci"))
            {
                string relativePath = $"{groupName}/{Path.GetFileName(file)}";
                fileMap[relativePath] = File.ReadAllText(file);
            }
        }

        return JsonSerializer.Serialize(fileMap);
    }

    private string? ResolveStateName()
    {
        string testMethodName = TestContext.Current.TestMethod!.MethodName;

        MethodInfo? method = GetType().GetMethod(
            testMethodName, BindingFlags.Public | BindingFlags.Instance);

        ReciPageStateAttribute? attr = method?.GetCustomAttribute<ReciPageStateAttribute>();

        if (attr is null)
        {
            return null;
        }

        return attr.StateName ?? "BaseState";
    }

    private async Task WaitForAppReadyAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.WaitForFunctionAsync(
            "() => !document.querySelector('fluent-progress-ring')",
            arg: null,
            new PageWaitForFunctionOptions { Timeout = 30_000 });
    }

    #region Editor Interaction

    /// <summary>Opens the "Add New Recipe" editor and returns the dialog locator.</summary>
    protected async Task<ILocator> OpenNewRecipeEditorAsync()
    {
        await Page.Locator(".fluent-nav-item:has-text('Add New Recipe')").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
        return Page.Locator("fluent-dialog");
    }

    /// <summary>Opens the editor for the current recipe and returns the dialog locator.</summary>
    protected async Task<ILocator> OpenRecipeEditorAsync()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit recipe" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
        return Page.Locator("fluent-dialog");
    }

    #endregion

    #region Fluent UI Form Helpers

    protected static async Task FillTextField(ILocator container, string label, string value)
    {
        await container.Locator($"label:has-text('{label}') ~ fluent-text-field").Locator("input").First.FillAsync(value);
    }

    protected static async Task FillTextArea(ILocator container, string label, string value)
    {
        await container.Locator($"label:has-text('{label}') ~ fluent-text-area").Locator("textarea").First.FillAsync(value);
    }

    protected static async Task FillNumberField(ILocator container, string label, string value)
    {
        await container.Locator($"label:has-text('{label}') ~ fluent-number-field").Locator("input").First.FillAsync(value);
    }

    protected static async Task FillCombobox(ILocator container, string label, string value)
    {
        ILocator comboInput = container.Locator($"label:has-text('{label}') ~ fluent-combobox").Locator("input").First;
        await comboInput.FillAsync(value);
        await comboInput.PressAsync("Tab");
    }

    protected static async Task FillPlaceholderField(ILocator container, string elementTag, string placeholder, string value)
    {
        await container.Locator($"{elementTag}[placeholder='{placeholder}']").First.Locator("input").FillAsync(value);
    }

    protected static async Task FillPlaceholderTextArea(ILocator container, string placeholder, string value)
    {
        await container.Locator($"fluent-text-area[placeholder='{placeholder}']").First.Locator("textarea").FillAsync(value);
    }

    #endregion

    #region Mock Filesystem Helpers

    protected async Task<JsonElement> GetMutationsAsync()
    {
        return await Page.EvaluateAsync<JsonElement>("() => window.__fsMutations");
    }

    protected async Task<string?> GetLastWrittenFileContentAsync()
    {
        JsonElement mutations = await GetMutationsAsync();
        for (int i = mutations.GetArrayLength() - 1; i >= 0; i--)
        {
            JsonElement m = mutations[i];
            if (m.GetProperty("op").GetString() == "writeFile")
            {
                return m.GetProperty("detail").GetProperty("content").GetString();
            }
        }
        return null;
    }

    protected static bool HasMutation(JsonElement mutations, string operation)
    {
        for (int i = 0; i < mutations.GetArrayLength(); i++)
        {
            if (mutations[i].GetProperty("op").GetString() == operation)
            {
                return true;
            }
        }
        return false;
    }

    #endregion

    #region Navigation Helpers

    /// <summary>Navigates to contents via brand logo click (client-side routing, preserves mock FS state).</summary>
    protected async Task NavigateToContentsViaClientAsync()
    {
        await Page.Locator(".header-brand").ClickAsync();
        await Expect(Page).ToHaveTitleAsync("Contents");
    }

    #endregion
}
