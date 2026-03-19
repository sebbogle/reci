namespace Tests.Views;

public class RecipePageTests(AppFixture app) : ReciPage(app)
{
    [Fact]
    [ReciPageState]
    public async Task SimpleRecipe_ShowsAllFields()
    {
        Guid pancakesId = new("20000000-0000-0000-0000-000000000001");
        await GotoRecipeAsync(pancakesId);
        await Expect(Page).ToHaveTitleAsync("Classic Pancakes");

        await ScreenshotAssert.MatchesAsync(Page, "RecipePage-SimpleRecipe", fullPage: true);
    }

    [Fact]
    [ReciPageState]
    public async Task RecipeWithGroupedIngredients_ShowsGroupedLayout()
    {
        Guid spaghettiId = new("20000000-0000-0000-0000-000000000002");
        await GotoRecipeAsync(spaghettiId);
        await Expect(Page).ToHaveTitleAsync("Spaghetti Bolognese");

        await ScreenshotAssert.MatchesAsync(Page, "RecipePage-GroupedIngredients", fullPage: true);
    }

    [Fact]
    [ReciPageState]
    public async Task RecipeNotFound_ShowsNotFoundMessage()
    {
        Guid nonExistentId = new("99999999-9999-9999-9999-999999999999");
        await GotoRecipeAsync(nonExistentId);
        await Expect(Page).ToHaveTitleAsync("Recipe");

        await ScreenshotAssert.MatchesAsync(Page, "RecipePage-NotFound");
    }

    [Fact]
    [ReciPageState("FullRecipeState")]
    public async Task RecipeWithAllFields_ShowsCompletely()
    {
        Guid shakshuka = new("30000000-0000-0000-0000-000000000001");
        await GotoRecipeAsync(shakshuka);
        await Expect(Page).ToHaveTitleAsync("Shakshuka");

        await ScreenshotAssert.MatchesAsync(Page, "RecipePage-AllFields", fullPage: true);
    }
}
