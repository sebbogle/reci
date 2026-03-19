namespace Tests.UserActions;

public class CreateRecipe(AppFixture app) : ReciPage(app)
{
    [Fact]
    public async Task WithBasicFields_SavesAndAppearsInContents()
    {
        const string recipeName = "Test Recipe";
        const string recipeDescription = "A test recipe created by E2E tests";

        await GotoContentsAsync();
        await Expect(Page.GetByText("No recipes found")).ToBeVisibleAsync();

        ILocator dialog = await OpenNewRecipeEditorAsync();

        await FillTextField(dialog, "Name", recipeName);
        await FillTextArea(dialog, "Description", recipeDescription);

        await dialog.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(Page.GetByText("saved successfully")).ToBeVisibleAsync();

        await Expect(Page.Locator($".recipe-card:has-text('{recipeName}')")).ToBeVisibleAsync();
        await Expect(Page.Locator($".fluent-nav-text:has-text('{recipeName}')")).ToBeVisibleAsync();

        JsonElement mutations = await GetMutationsAsync();
        Assert.True(mutations.GetArrayLength() > 0, "Expected at least one filesystem mutation");
        Assert.Equal("writeFile", mutations[0].GetProperty("op").GetString());
    }

    [Fact]
    public async Task WithGroup_SavesInGroupFolder()
    {
        const string recipeName = "Grouped Recipe";
        const string groupName = "Lunch";

        await GotoContentsAsync();
        ILocator dialog = await OpenNewRecipeEditorAsync();

        await FillTextField(dialog, "Name", recipeName);
        await FillCombobox(dialog, "Group", groupName);

        await dialog.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(Page.GetByText("saved successfully")).ToBeVisibleAsync();

        JsonElement mutations = await GetMutationsAsync();
        bool hasGroupWrite = false;
        for (int i = 0; i < mutations.GetArrayLength(); i++)
        {
            JsonElement m = mutations[i];
            string op = m.GetProperty("op").GetString()!;
            if (op == "writeFile")
            {
                string path = m.GetProperty("detail").GetProperty("path").GetString()!;
                if (path.StartsWith($"{groupName}/", StringComparison.OrdinalIgnoreCase))
                {
                    hasGroupWrite = true;
                    break;
                }
            }
        }
        Assert.True(hasGroupWrite, $"Expected writeFile with path starting with '{groupName}/'");
    }

    [Fact]
    public async Task WithAllFields_SavesCompleteRecipe()
    {
        await GotoContentsAsync();
        ILocator dialog = await OpenNewRecipeEditorAsync();

        await FillTextField(dialog, "Name", "Full Recipe");
        await FillCombobox(dialog, "Group", "Dinner");
        await FillTextArea(dialog, "Description", "A complete test recipe");

        // Fill the default empty ingredient row
        await FillPlaceholderField(dialog, "fluent-text-field", "Ingredient name...", "Flour");
        await FillPlaceholderField(dialog, "fluent-number-field", "Qty", "2");
        await FillPlaceholderField(dialog, "fluent-text-field", "Unit", "cups");

        // Fill the default empty instruction row
        await FillPlaceholderTextArea(dialog, "Describe this step...", "Mix all ingredients together");

        // Add a tag
        await FillPlaceholderField(dialog, "fluent-text-field", "Add a tag...", "test-tag");
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Add", Exact = true }).ClickAsync();

        // Fill nutrition info
        await FillNumberField(dialog, "Calories (kcal)", "300");
        await FillNumberField(dialog, "Protein (g)", "15");
        await FillNumberField(dialog, "Carbohydrates (g)", "40");
        await FillNumberField(dialog, "Fat (g)", "10");

        // Fill source
        await FillTextField(dialog, "Source", "Test Cookbook");
        await FillTextField(dialog, "Link", "https://example.com/recipe");

        // Fill further notes
        await FillTextArea(dialog, "Further Notes", "These are test notes");

        await dialog.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(Page.GetByText("saved successfully")).ToBeVisibleAsync();

        // Verify the saved JSON contains all fields
        string? writtenContent = await GetLastWrittenFileContentAsync();
        Assert.NotNull(writtenContent);

        JsonElement saved = JsonSerializer.Deserialize<JsonElement>(writtenContent);
        Assert.Equal("Full Recipe", saved.GetProperty("name").GetString());
        Assert.Equal("Dinner", saved.GetProperty("group").GetString());
        Assert.Equal("A complete test recipe", saved.GetProperty("description").GetString());
        Assert.True(saved.GetProperty("ingredients").GetArrayLength() > 0);
        Assert.True(saved.GetProperty("instructions").GetArrayLength() > 0);
        Assert.True(saved.GetProperty("tags").GetArrayLength() > 0);
        Assert.Equal("test-tag", saved.GetProperty("tags")[0].GetString());
        Assert.Equal(300, saved.GetProperty("nutritionInfo").GetProperty("calories").GetInt32());
        Assert.Equal("Test Cookbook", saved.GetProperty("source").GetProperty("text").GetString());
        Assert.Equal("https://example.com/recipe", saved.GetProperty("source").GetProperty("url").GetString());
        Assert.Equal("These are test notes", saved.GetProperty("furtherNotes").GetString());
    }

    [Fact]
    public async Task Cancel_EmptyForm_ClosesWithoutConfirmation()
    {
        await GotoContentsAsync();
        ILocator dialog = await OpenNewRecipeEditorAsync();

        await dialog.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();

        // Dialog should close immediately without confirmation
        await Expect(Page.GetByRole(AriaRole.Dialog)).Not.ToBeVisibleAsync();

        // No filesystem mutations should have occurred
        JsonElement mutations = await GetMutationsAsync();
        Assert.Equal(0, mutations.GetArrayLength());
    }

    [Fact]
    public async Task Cancel_WithContent_ConfirmDiscard()
    {
        await GotoContentsAsync();
        ILocator dialog = await OpenNewRecipeEditorAsync();

        await FillTextField(dialog, "Name", "Unsaved Recipe");

        await dialog.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();

        // Confirmation dialog should appear
        await Expect(Page.GetByText("Are you sure you want to discard your changes?")).ToBeVisibleAsync();

        // Confirm discard
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();

        // Editor should close
        await Expect(Page.GetByRole(AriaRole.Dialog)).Not.ToBeVisibleAsync();

        // No filesystem mutations
        JsonElement mutations = await GetMutationsAsync();
        Assert.Equal(0, mutations.GetArrayLength());
    }

    [Fact]
    public async Task Cancel_WithContent_DenyDiscard_StaysOpen()
    {
        await GotoContentsAsync();
        ILocator dialog = await OpenNewRecipeEditorAsync();

        await FillTextField(dialog, "Name", "Unsaved Recipe");

        await dialog.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();

        // Confirmation dialog should appear
        await Expect(Page.GetByText("Are you sure you want to discard your changes?")).ToBeVisibleAsync();

        // Deny discard
        await Page.GetByRole(AriaRole.Button, new() { Name = "No" }).ClickAsync();

        // Editor should still be open with content preserved
        await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
    }
}
