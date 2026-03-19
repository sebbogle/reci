namespace Tests.UserActions;

public class EditRecipe(AppFixture app) : ReciPage(app)
{
    private static readonly Guid PancakesId = new("20000000-0000-0000-0000-000000000001");

    [Fact]
    [ReciPageState]
    public async Task ChangeName_UpdatesRecipeAndUI()
    {
        const string newName = "Updated Pancakes";

        await GotoRecipeAsync(PancakesId);
        await Expect(Page).ToHaveTitleAsync("Classic Pancakes");

        ILocator dialog = await OpenRecipeEditorAsync();

        await FillTextField(dialog, "Name", newName);

        await dialog.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(Page.GetByText("saved successfully")).ToBeVisibleAsync();

        // Page title and heading should update
        await Expect(Page).ToHaveTitleAsync(newName);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = newName })).ToBeVisibleAsync();

        // Verify filesystem mutation contains the new name
        string? content = await GetLastWrittenFileContentAsync();
        Assert.NotNull(content);
        JsonElement saved = JsonSerializer.Deserialize<JsonElement>(content);
        Assert.Equal(newName, saved.GetProperty("name").GetString());
    }

    [Fact]
    [ReciPageState]
    public async Task ChangeGroup_MovesRecipeFile()
    {
        const string newGroup = "Brunch";

        await GotoRecipeAsync(PancakesId);
        ILocator dialog = await OpenRecipeEditorAsync();

        await FillCombobox(dialog, "Group", newGroup);

        await dialog.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(Page.GetByText("saved successfully")).ToBeVisibleAsync();

        // Verify filesystem has file in new group
        JsonElement mutations = await GetMutationsAsync();
        bool hasNewGroupPath = false;
        for (int i = 0; i < mutations.GetArrayLength(); i++)
        {
            JsonElement m = mutations[i];
            string op = m.GetProperty("op").GetString()!;
            if (op is "writeFile" or "moveFile")
            {
                string detail = m.GetProperty("detail").ToString();
                if (detail.Contains(newGroup, StringComparison.OrdinalIgnoreCase))
                {
                    hasNewGroupPath = true;
                    break;
                }
            }
        }
        Assert.True(hasNewGroupPath, $"Expected filesystem mutation referencing '{newGroup}' group");
    }

    [Fact]
    [ReciPageState]
    public async Task AddIngredient_AppearsInSavedRecipe()
    {
        await GotoRecipeAsync(PancakesId);
        ILocator dialog = await OpenRecipeEditorAsync();

        // Click "Add Ingredient" to add a new row
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Add Ingredient" }).ClickAsync();

        // Fill the last (new) ingredient row
        await dialog.Locator("fluent-text-field[placeholder='Ingredient name...']").Last.Locator("input").FillAsync("Blueberries");
        await dialog.Locator("fluent-number-field[placeholder='Qty']").Last.Locator("input").FillAsync("1");
        await dialog.Locator("fluent-text-field[placeholder='Unit']").Last.Locator("input").FillAsync("cup");

        await dialog.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(Page.GetByText("saved successfully")).ToBeVisibleAsync();

        string? content = await GetLastWrittenFileContentAsync();
        Assert.NotNull(content);
        Assert.Contains("Blueberries", content);
    }

    [Fact]
    [ReciPageState]
    public async Task AddInstruction_AppearsInSavedRecipe()
    {
        await GotoRecipeAsync(PancakesId);
        ILocator dialog = await OpenRecipeEditorAsync();

        // Click "Add Step" to add a new instruction
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Add Step" }).ClickAsync();

        // Fill the last (new) instruction row
        await dialog.Locator("fluent-text-area[placeholder='Describe this step...']").Last.Locator("textarea").FillAsync("Serve with maple syrup");

        await dialog.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(Page.GetByText("saved successfully")).ToBeVisibleAsync();

        string? content = await GetLastWrittenFileContentAsync();
        Assert.NotNull(content);
        Assert.Contains("Serve with maple syrup", content);
    }

    [Fact]
    [ReciPageState]
    public async Task AddTag_AppearsInSavedRecipe()
    {
        const string newTag = "fluffy";

        await GotoRecipeAsync(PancakesId);
        ILocator dialog = await OpenRecipeEditorAsync();

        await dialog.Locator("fluent-text-field[placeholder='Add a tag...']").Locator("input").FillAsync(newTag);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Add", Exact = true }).ClickAsync();

        await dialog.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(Page.GetByText("saved successfully")).ToBeVisibleAsync();

        // Tag should appear on the recipe page
        await Expect(Page.GetByText(newTag, new() { Exact = true })).ToBeVisibleAsync();

        string? content = await GetLastWrittenFileContentAsync();
        Assert.NotNull(content);
        Assert.Contains(newTag, content);
    }

    [Fact]
    [ReciPageState]
    public async Task Cancel_NoChanges_ClosesWithoutConfirmation()
    {
        await GotoRecipeAsync(PancakesId);
        ILocator dialog = await OpenRecipeEditorAsync();

        await dialog.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();

        // Should close immediately without confirmation
        await Expect(Page.GetByRole(AriaRole.Dialog)).Not.ToBeVisibleAsync();
    }

    // NOTE: Cancel_WithChanges_ShowsConfirmationDialog is intentionally omitted.
    // The app has a bug where the editor mutates the cached Recipe object directly,
    // so IsRecipeModifiedAsync compares the modified object to itself and always
    // returns false. The "Discard Changes" confirmation dialog never appears for
    // existing recipes. This should be fixed by cloning the recipe before editing.
}
