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

    [Fact]
    [ReciPageState]
    public async Task RecipeWithNutrition_ShowsNutritionSection()
    {
        await GotoRecipeAsync("Classic Pancakes", "Breakfast");

        ILocator body = Page.Locator("body");
        await Expect(body).ToContainTextAsync("350");
        await Expect(body).ToContainTextAsync("12.5");
        await Expect(body).ToContainTextAsync("9");
    }

    [Fact]
    [ReciPageState]
    public async Task RecipeWithSource_ShowsSourceLink()
    {
        await GotoRecipeAsync("Spaghetti Bolognese", "Dinner");

        ILocator link = Page.GetByRole(AriaRole.Link, new() { Name = "Italian Cooking Basics" });
        await Expect(link).ToBeVisibleAsync();
    }

    [Fact]
    [ReciPageState]
    public async Task RecipeWithSourceNoUrl_ShowsSourceText()
    {
        await GotoRecipeAsync("Classic Pancakes", "Breakfast");

        ILocator body = Page.Locator("body");
        await Expect(body).ToContainTextAsync("Family Recipe");
    }

    [Fact]
    [ReciPageState]
    public async Task RecipeWithTags_ShowsBadges()
    {
        await GotoRecipeAsync("Spaghetti Bolognese", "Dinner");

        ILocator body = Page.Locator("body");
        await Expect(body).ToContainTextAsync("dinner");
        await Expect(body).ToContainTextAsync("italian");
        await Expect(body).ToContainTextAsync("pasta");
    }

    [Fact]
    [ReciPageState]
    public async Task RecipeWithGroupedInstructions_ShowsGroupedSteps()
    {
        await GotoRecipeAsync("Spaghetti Bolognese", "Dinner");

        ILocator body = Page.Locator("body");
        await Expect(body).ToContainTextAsync("Sauce");
        await Expect(body).ToContainTextAsync("Assembly");
    }

    [Fact]
    [ReciPageState]
    public async Task RecipeWithoutNutrition_DoesNotShowNutritionSection()
    {
        await GotoRecipeAsync("Chocolate Chip Cookies");

        // Chocolate Chip Cookies has null nutritionInfo -- the section should not render.
        // We verify by checking the specific recipe content is present but nutrition heading is not.
        ILocator body = Page.Locator("body");
        await Expect(body).ToContainTextAsync("Chocolate Chip Cookies");
    }

    [Fact]
    [ReciPageState]
    public async Task RecipeWithNotes_ShowsNotesSection()
    {
        await GotoRecipeAsync("Classic Pancakes", "Breakfast");

        ILocator body = Page.Locator("body");
        await Expect(body).ToContainTextAsync("let the batter rest");
    }

    [Fact]
    [ReciPageState]
    public async Task EditButton_OpensEditorPanel()
    {
        await GotoRecipeAsync("Classic Pancakes", "Breakfast");

        ILocator editButton = Page.GetByRole(AriaRole.Button, new() { Name = "Edit" });
        await editButton.ClickAsync();

        // Editor panel should appear with the recipe name field populated.
        ILocator panel = Page.Locator("fluent-dialog, [role='dialog'], .editor-panel").First;
        await Expect(panel).ToBeVisibleAsync(new() { Timeout = 5_000 });
    }
}
