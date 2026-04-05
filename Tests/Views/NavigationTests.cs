namespace Tests.Views;

public class NavigationTests(AppFixture app) : ReciPage(app)
{
    [Fact]
    [ReciPageState]
    public async Task NavMenu_ShowsGroupsAndRecipes()
    {
        await GotoContentsAsync();

        ILocator nav = Page.Locator("fluent-nav-menu, nav").First;
        await Expect(nav).ToContainTextAsync("Contents");
        await Expect(nav).ToContainTextAsync("Breakfast");
        await Expect(nav).ToContainTextAsync("Dinner");
        await Expect(nav).ToContainTextAsync("Classic Pancakes");
        await Expect(nav).ToContainTextAsync("Spaghetti Bolognese");
        await Expect(nav).ToContainTextAsync("Chocolate Chip Cookies");
    }

    [Fact]
    [ReciPageState]
    public async Task NavMenu_ClickRecipe_NavigatesToRecipePage()
    {
        await GotoContentsAsync();

        ILocator nav = Page.Locator("fluent-nav-menu, nav").First;
        await nav.GetByText("Classic Pancakes").ClickAsync();
        await WaitForNavigationIdleAsync();

        Assert.Contains("/recipe/", Page.Url);
        await Expect(Page).ToHaveTitleAsync("Classic Pancakes");
    }

    [Fact]
    [ReciPageState]
    public async Task NavMenu_ClickContents_NavigatesToHome()
    {
        await GotoRecipeAsync("Classic Pancakes", "Breakfast");

        ILocator nav = Page.Locator("fluent-nav-menu, nav").First;
        await nav.GetByText("Contents").ClickAsync();
        await WaitForNavigationIdleAsync();

        await Expect(Page).ToHaveTitleAsync("Contents");
    }

    [Fact]
    [ReciPageState]
    public async Task NavMenu_AddNewRecipe_OpensEditorPanel()
    {
        await GotoContentsAsync();

        ILocator addButton = Page.GetByRole(AriaRole.Button, new() { Name = "Add New Recipe" });
        await addButton.ClickAsync();

        ILocator panel = Page.Locator("fluent-dialog, [role='dialog'], .editor-panel").First;
        await Expect(panel).ToBeVisibleAsync(new() { Timeout = 5_000 });
    }

    [Fact]
    [ReciPageState]
    public async Task Header_ClickLogo_NavigatesToHome()
    {
        await GotoRecipeAsync("Classic Pancakes", "Breakfast");

        // Click the app brand/logo in the header.
        ILocator header = Page.Locator("fluent-header, header").First;
        ILocator logo = header.GetByText("Reci").First;
        await logo.ClickAsync();
        await WaitForNavigationIdleAsync();

        await Expect(Page).ToHaveTitleAsync("Contents");
    }
}
