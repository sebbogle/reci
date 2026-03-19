namespace Tests.UserActions;

public class Navigation(AppFixture app) : ReciPage(app)
{
    [Fact]
    [ReciPageState]
    public async Task ClickRecipeCard_NavigatesToRecipePage()
    {
        await GotoContentsAsync();

        await Page.Locator(".recipe-card:has-text('Classic Pancakes')").ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex(@"/recipe/20000000-0000-0000-0000-000000000001"));
        await Expect(Page).ToHaveTitleAsync("Classic Pancakes");
    }

    [Fact]
    [ReciPageState]
    public async Task ClickRecipeCard_FromGroupView_NavigatesToRecipePage()
    {
        await GotoGroupAsync("Breakfast");

        await Page.Locator(".recipe-card:has-text('Classic Pancakes')").ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex(@"/recipe/20000000-0000-0000-0000-000000000001"));
        await Expect(Page).ToHaveTitleAsync("Classic Pancakes");
    }

    [Fact]
    [ReciPageState]
    public async Task ClickBackToContents_FromNotFound()
    {
        await GotoRecipeAsync(new Guid("99999999-9999-9999-9999-999999999999"));

        await Page.GetByRole(AriaRole.Button, new() { Name = "Back to Contents" }).ClickAsync();

        await Expect(Page).ToHaveTitleAsync("Contents");
    }

    [Fact]
    [ReciPageState]
    public async Task ClickBrandLogo_NavigatesToHome()
    {
        await GotoRecipeAsync(new Guid("20000000-0000-0000-0000-000000000001"));
        await Expect(Page).ToHaveTitleAsync("Classic Pancakes");

        await Page.Locator(".header-brand").ClickAsync();

        await Expect(Page).ToHaveTitleAsync("Contents");
    }

    [Fact]
    [ReciPageState]
    public async Task NavMenu_ClickRecipeLink_NavigatesToRecipe()
    {
        await GotoContentsAsync();

        // Use ungrouped recipe (Chocolate Chip Cookies) which is always visible in the nav
        await Page.Locator("a.fluent-nav-link[href*='recipe/20000000-0000-0000-0000-000000000003']").ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex(@"/recipe/20000000-0000-0000-0000-000000000003"));
        await Expect(Page).ToHaveTitleAsync("Chocolate Chip Cookies");
    }

    [Fact]
    [ReciPageState]
    public async Task NavMenu_ClickGroupLink_NavigatesToGroup()
    {
        await GotoContentsAsync();

        await Page.Locator(".fluent-nav-group a[title='Breakfast']").ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex(@"/group/Breakfast"));
        await Expect(Page).ToHaveTitleAsync("Breakfast");
    }

    [Fact]
    [ReciPageState]
    public async Task NavMenu_AddNewRecipe_OpensEditor()
    {
        await GotoContentsAsync();

        await OpenNewRecipeEditorAsync();

        await Expect(Page.Locator(".fluent-dialog-header")).ToContainTextAsync("New Recipe");
    }
}
