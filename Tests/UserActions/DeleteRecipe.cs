namespace Tests.UserActions;

public class DeleteRecipe(AppFixture app) : ReciPage(app)
{
    private static readonly Guid PancakesId = new("20000000-0000-0000-0000-000000000001");

    [Fact]
    [ReciPageState]
    public async Task ConfirmDelete_RemovesRecipeFromUI()
    {
        await GotoRecipeAsync(PancakesId);
        await Expect(Page).ToHaveTitleAsync("Classic Pancakes");

        ILocator dialog = await OpenRecipeEditorAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Delete Recipe" }).ClickAsync();

        // Confirm deletion
        await Expect(Page.GetByText("Are you sure you want to delete")).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();

        // Dialog should close
        await Expect(Page.GetByRole(AriaRole.Dialog)).Not.ToBeVisibleAsync();

        // Navigate to contents via client-side routing (preserves mock FS state)
        await NavigateToContentsViaClientAsync();

        await Expect(Page.Locator(".recipe-card:has-text('Classic Pancakes')")).Not.ToBeVisibleAsync();

        // NavMenu should no longer show the recipe
        await Expect(Page.Locator(".fluent-nav-text:has-text('Classic Pancakes')")).Not.ToBeVisibleAsync();
    }

    [Fact]
    [ReciPageState]
    public async Task ConfirmDelete_RemovesFromFilesystem()
    {
        await GotoRecipeAsync(PancakesId);

        ILocator dialog = await OpenRecipeEditorAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Delete Recipe" }).ClickAsync();
        await Expect(Page.GetByText("Are you sure you want to delete")).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Dialog)).Not.ToBeVisibleAsync();

        // Verify filesystem has a deleteFile mutation
        JsonElement mutations = await GetMutationsAsync();
        Assert.True(HasMutation(mutations, "deleteFile"), "Expected a deleteFile mutation in the filesystem");
    }

    [Fact]
    [ReciPageState]
    public async Task CancelDelete_RecipeRemains()
    {
        await GotoRecipeAsync(PancakesId);

        ILocator dialog = await OpenRecipeEditorAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Delete Recipe" }).ClickAsync();

        // Confirmation should appear
        await Expect(Page.GetByText("Are you sure you want to delete")).ToBeVisibleAsync();

        // Cancel the deletion
        await Page.GetByRole(AriaRole.Button, new() { Name = "No" }).ClickAsync();

        // Editor should still be open
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();

        // No deleteFile mutation should have occurred
        JsonElement mutations = await GetMutationsAsync();
        Assert.False(HasMutation(mutations, "deleteFile"), "Expected no deleteFile mutation after cancelling");
    }
}
