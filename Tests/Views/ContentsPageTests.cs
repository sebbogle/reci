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
        Guid breakfastGroupId = new("10000000-0000-0000-0000-000000000001");
        await GotoGroupAsync(breakfastGroupId);
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
        const string GroupId = "10000000-0000-0000-0000-000000000002";
        const string GroupPageTitle = "Dinner";

        await GotoGroupAsync(new Guid(GroupId));

        await Expect(Page).ToHaveTitleAsync(GroupPageTitle);
    }

    #endregion
}
