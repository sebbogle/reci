namespace Tests.UserActions;

public class RecipeCrudTests(AppFixture app) : ReciPage(app)
{
    [Fact]
    [ReciPageState]
    public async Task CreateRecipe_MinimalFields_SavesAndNavigates()
    {
        await GotoContentsAsync();

        // Open the "Add New Recipe" editor.
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add New Recipe" }).ClickAsync();
        ILocator panel = Page.Locator("fluent-dialog, [role='dialog'], .editor-panel").First;
        await Expect(panel).ToBeVisibleAsync(new() { Timeout = 5_000 });

        // Fill in just the name.
        ILocator nameInput = panel.GetByLabel("Name").Or(panel.Locator("input").First);
        await nameInput.FillAsync("Test Recipe");

        // Save.
        await panel.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await WaitForNavigationIdleAsync();

        // Verify the recipe appears somewhere in the app.
        ILocator body = Page.Locator("body");
        await Expect(body).ToContainTextAsync("Test Recipe");
    }

    [Fact]
    [ReciPageState]
    public async Task CreateRecipe_EmptyName_ShowsValidation()
    {
        await GotoContentsAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Add New Recipe" }).ClickAsync();
        ILocator panel = Page.Locator("fluent-dialog, [role='dialog'], .editor-panel").First;
        await Expect(panel).ToBeVisibleAsync(new() { Timeout = 5_000 });

        // Try to save without filling name.
        ILocator saveButton = panel.GetByRole(AriaRole.Button, new() { Name = "Save" });
        await saveButton.ClickAsync();

        // Expect a validation error to appear.
        await Expect(panel).ToContainTextAsync("required", new() { IgnoreCase = true, Timeout = 3_000 });
    }

    [Fact]
    [ReciPageState]
    public async Task EditRecipe_ModifyDescription_SavesChanges()
    {
        await GotoRecipeAsync("Classic Pancakes", "Breakfast");

        // Open editor.
        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        ILocator panel = Page.Locator("fluent-dialog, [role='dialog'], .editor-panel").First;
        await Expect(panel).ToBeVisibleAsync(new() { Timeout = 5_000 });

        // Modify description.
        ILocator descriptionInput = panel.GetByLabel("Description").Or(panel.Locator("textarea").First);
        await descriptionInput.FillAsync("Updated description for testing");

        // Save.
        await panel.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await WaitForNavigationIdleAsync();

        // Verify updated text appears on page.
        ILocator body = Page.Locator("body");
        await Expect(body).ToContainTextAsync("Updated description for testing");
    }

    [Fact]
    [ReciPageState]
    public async Task DeleteRecipe_ConfirmDialog_RemovesRecipe()
    {
        await GotoRecipeAsync("Classic Pancakes", "Breakfast");

        // Open editor to access delete.
        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        ILocator panel = Page.Locator("fluent-dialog, [role='dialog'], .editor-panel").First;
        await Expect(panel).ToBeVisibleAsync(new() { Timeout = 5_000 });

        // Click delete.
        await panel.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();

        // Confirm the dialog.
        ILocator confirmDialog = Page.Locator("fluent-dialog, [role='alertdialog'], [role='dialog']").Last;
        await confirmDialog.GetByRole(AriaRole.Button, new() { Name = "Delete" }).Or(
            confirmDialog.GetByRole(AriaRole.Button, new() { Name = "Confirm" })).Or(
            confirmDialog.GetByRole(AriaRole.Button, new() { Name = "Yes" })).First.ClickAsync();

        await WaitForNavigationIdleAsync();

        // Should be redirected away from the deleted recipe.
        Assert.DoesNotContain("Classic%20Pancakes", Page.Url);
    }

    [Fact]
    [ReciPageState]
    public async Task DeleteRecipe_CancelDialog_KeepsRecipe()
    {
        await GotoRecipeAsync("Classic Pancakes", "Breakfast");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        ILocator panel = Page.Locator("fluent-dialog, [role='dialog'], .editor-panel").First;
        await Expect(panel).ToBeVisibleAsync(new() { Timeout = 5_000 });

        await panel.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();

        // Cancel the confirmation dialog.
        ILocator confirmDialog = Page.Locator("fluent-dialog, [role='alertdialog'], [role='dialog']").Last;
        await confirmDialog.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).Or(
            confirmDialog.GetByRole(AriaRole.Button, new() { Name = "No" })).First.ClickAsync();

        // Recipe should still be there.
        await Expect(Page).ToHaveTitleAsync("Classic Pancakes");
    }
}
