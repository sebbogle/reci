namespace Tests.Views;

public class ContentsPageTests(AppFixture app) : ReciPage(app)
{
    [Fact]
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
        const string GroupName = "Dinner";

        await GotoGroupAsync(GroupName);

        await Expect(Page).ToHaveTitleAsync(GroupName);
    }

    #endregion

    [Fact]
    [ReciPageState]
    public async Task NonExistentGroup_ShowsNotFound()
    {
        await GotoGroupAsync("NonExistentGroup");

        await ScreenshotAssert.MatchesAsync(Page, "ContentsPage-GroupNotFound");
    }
}
