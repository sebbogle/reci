namespace Tests.Views;

public class ContentsPageTests(AppFixture app) : ReciPage(app)
{
    [Fact]
    [ReciPageState("EmptyState")]
    public async Task EmptyState_ShowsNoRecipesMessage()
    {
        await GotoContentsAsync();
        await ScreenshotAssert.MatchesAsync(Page, "ContentsPage-EmptyState");
    }

    [Fact]
    [ReciPageState]
    public async Task WithRecipes_ShowsRecipeCards()
    {
        await GotoContentsAsync();
        await Expect(Page).ToHaveTitleAsync("Contents");

        await ScreenshotAssert.MatchesAsync(Page, "ContentsPage-WithRecipes");
    }

    [Fact]
    [ReciPageState]
    public async Task GroupFilter_ShowsOnlyGroupRecipes()
    {
        await GotoGroupAsync("Breakfast");
        await Expect(Page).ToHaveTitleAsync("Breakfast");

        await ScreenshotAssert.MatchesAsync(Page, "ContentsPage-GroupFilter");
    }

    [Fact]
    [ReciPageState]
    public async Task RecipeCards_ShowDescriptionAndTags()
    {
        await GotoContentsAsync();

        ILocator body = Page.Locator("body");
        await Expect(body).ToContainTextAsync("Classic Pancakes");
        await Expect(body).ToContainTextAsync("Fluffy golden pancakes perfect for weekend mornings");
        await Expect(body).ToContainTextAsync("Spaghetti Bolognese");
        await Expect(body).ToContainTextAsync("Chocolate Chip Cookies");
    }

    [Fact]
    [ReciPageState]
    public async Task RecipeCard_ClickNavigatesToRecipe()
    {
        await GotoContentsAsync();

        await Page.GetByText("Classic Pancakes").First.ClickAsync();
        await WaitForNavigationIdleAsync();

        Assert.Contains("/recipe/", Page.Url);
    }

    [Fact]
    [ReciPageState]
    public async Task GroupFilter_NonexistentGroup_ShowsNotFound()
    {
        await GotoGroupAsync("Nonexistent");

        ILocator body = Page.Locator("body");
        await Expect(body).ToContainTextAsync("not found", new() { IgnoreCase = true });
    }

    #region Page Title Tests

    [Fact]
    [ReciPageState]
    public async Task PageTitle_Default()
    {
        const string DefaultPageTitle = "Contents";

        await GotoContentsAsync();

        await Expect(Page).ToHaveTitleAsync(DefaultPageTitle);
    }

    [Fact]
    [ReciPageState]
    public async Task PageTitle_ViewingGroup()
    {
        const string GroupPageTitle = "Dinner";

        await GotoGroupAsync("Dinner");

        await Expect(Page).ToHaveTitleAsync(GroupPageTitle);
    }

    #endregion
}
