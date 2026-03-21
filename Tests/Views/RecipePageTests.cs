namespace Tests.Views;

public class RecipePageTests(AppFixture app) : ReciPage(app)
{
    [Fact]
    [ReciPageState]
    public async Task SimpleRecipe_ShowsAllFields()
    {
        await GotoRecipeAsync("Classic Pancakes", "Breakfast");
        await Expect(Page).ToHaveTitleAsync("Classic Pancakes");

        await ScreenshotAssert.MatchesAsync(Page, "RecipePage-SimpleRecipe", fullPage: true);
    }

    [Fact]
    [ReciPageState]
    public async Task RecipeWithGroupedIngredients_ShowsGroupedLayout()
    {
        await GotoRecipeAsync("Spaghetti Bolognese", "Dinner");
        await Expect(Page).ToHaveTitleAsync("Spaghetti Bolognese");

        await ScreenshotAssert.MatchesAsync(Page, "RecipePage-GroupedIngredients", fullPage: true);
    }

    [Fact]
    [ReciPageState]
    public async Task RecipeNotFound_ShowsNotFoundMessage()
    {
        await GotoRecipeAsync("NonExistentRecipe");
        await Expect(Page).ToHaveTitleAsync("Recipe");

        await ScreenshotAssert.MatchesAsync(Page, "RecipePage-NotFound");
    }
}
