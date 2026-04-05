namespace Tests.UserActions;

public class SearchTests(AppFixture app) : ReciPage(app)
{
    [Fact]
    [ReciPageState]
    public async Task Search_ByName_ShowsMatchingRecipes()
    {
        await GotoContentsAsync();

        ILocator searchInput = Page.GetByRole(AriaRole.Searchbox).Or(
            Page.Locator("input[type='search'], input[placeholder*='Search']")).First;
        await searchInput.FillAsync("Pancakes");

        ILocator dropdown = Page.Locator(".search-results, [role='listbox'], fluent-listbox").First;
        await Expect(dropdown).ToBeVisibleAsync(new() { Timeout = 5_000 });
        await Expect(dropdown).ToContainTextAsync("Classic Pancakes");
    }

    [Fact]
    [ReciPageState]
    public async Task Search_ByTag_ShowsMatchingRecipes()
    {
        await GotoContentsAsync();

        ILocator searchInput = Page.GetByRole(AriaRole.Searchbox).Or(
            Page.Locator("input[type='search'], input[placeholder*='Search']")).First;
        await searchInput.FillAsync("pasta");

        ILocator dropdown = Page.Locator(".search-results, [role='listbox'], fluent-listbox").First;
        await Expect(dropdown).ToBeVisibleAsync(new() { Timeout = 5_000 });
        await Expect(dropdown).ToContainTextAsync("Spaghetti Bolognese");
    }

    [Fact]
    [ReciPageState]
    public async Task Search_ByGroupName_ShowsGroupResult()
    {
        await GotoContentsAsync();

        ILocator searchInput = Page.GetByRole(AriaRole.Searchbox).Or(
            Page.Locator("input[type='search'], input[placeholder*='Search']")).First;
        await searchInput.FillAsync("Break");

        ILocator dropdown = Page.Locator(".search-results, [role='listbox'], fluent-listbox").First;
        await Expect(dropdown).ToBeVisibleAsync(new() { Timeout = 5_000 });
        await Expect(dropdown).ToContainTextAsync("Breakfast");
    }

    [Fact]
    [ReciPageState]
    public async Task Search_SelectResult_NavigatesToRecipe()
    {
        await GotoContentsAsync();

        ILocator searchInput = Page.GetByRole(AriaRole.Searchbox).Or(
            Page.Locator("input[type='search'], input[placeholder*='Search']")).First;
        await searchInput.FillAsync("Pancakes");

        ILocator dropdown = Page.Locator(".search-results, [role='listbox'], fluent-listbox").First;
        await Expect(dropdown).ToBeVisibleAsync(new() { Timeout = 5_000 });

        await dropdown.GetByText("Classic Pancakes").First.ClickAsync();
        await WaitForNavigationIdleAsync();

        Assert.Contains("/recipe/", Page.Url);
    }

    [Fact]
    [ReciPageState]
    public async Task Search_NoResults_NoDropdown()
    {
        await GotoContentsAsync();

        ILocator searchInput = Page.GetByRole(AriaRole.Searchbox).Or(
            Page.Locator("input[type='search'], input[placeholder*='Search']")).First;
        await searchInput.FillAsync("zzzznotfound");

        // Give a short delay for dropdown to potentially appear.
        await Page.WaitForTimeoutAsync(500);

        ILocator dropdown = Page.Locator(".search-results, [role='listbox'], fluent-listbox").First;
        await Expect(dropdown).Not.ToBeVisibleAsync(new() { Timeout = 2_000 });
    }
}
