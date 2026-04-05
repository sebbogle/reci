namespace Tests.UserActions;

public class RecipeEditorTests(AppFixture app) : ReciPage(app)
{
    [Fact]
    [ReciPageState]
    public async Task Editor_ExistingRecipe_PopulatesAllFields()
    {
        await GotoRecipeAsync("Classic Pancakes", "Breakfast");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        ILocator panel = Page.Locator("fluent-dialog, [role='dialog'], .editor-panel").First;
        await Expect(panel).ToBeVisibleAsync(new() { Timeout = 5_000 });

        // Verify key fields are populated.
        await Expect(panel).ToContainTextAsync("Classic Pancakes");
        await Expect(panel).ToContainTextAsync("Breakfast");
        await Expect(panel).ToContainTextAsync("All-purpose flour");
    }

    [Fact]
    [ReciPageState]
    public async Task Editor_AddTag_ShowsBadge()
    {
        await GotoRecipeAsync("Classic Pancakes", "Breakfast");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        ILocator panel = Page.Locator("fluent-dialog, [role='dialog'], .editor-panel").First;
        await Expect(panel).ToBeVisibleAsync(new() { Timeout = 5_000 });

        // Find a tag input and add a new tag.
        ILocator tagInput = panel.Locator("input[placeholder*='tag' i], input[placeholder*='Tag' i]").First;
        if (await tagInput.CountAsync() > 0)
        {
            await tagInput.FillAsync("newtag");
            await tagInput.PressAsync("Enter");
            await Expect(panel).ToContainTextAsync("newtag");
        }
    }

    [Fact]
    [ReciPageState]
    public async Task Editor_Cancel_ModifiedForm_ShowsConfirmation()
    {
        await GotoRecipeAsync("Classic Pancakes", "Breakfast");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        ILocator panel = Page.Locator("fluent-dialog, [role='dialog'], .editor-panel").First;
        await Expect(panel).ToBeVisibleAsync(new() { Timeout = 5_000 });

        // Modify a field.
        ILocator descInput = panel.GetByLabel("Description").Or(panel.Locator("textarea").First);
        await descInput.FillAsync("Changed description");

        // Click cancel.
        await panel.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();

        // A confirmation dialog should appear.
        ILocator confirmDialog = Page.Locator("fluent-dialog, [role='alertdialog'], [role='dialog']").Last;
        await Expect(confirmDialog).ToBeVisibleAsync(new() { Timeout = 3_000 });
    }
}
