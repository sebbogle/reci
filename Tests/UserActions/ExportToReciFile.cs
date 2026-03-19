namespace Tests.UserActions;

public class ExportToReciFile(AppFixture app) : ReciPage(app)
{
    [Fact]
    [ReciPageState]
    public async Task Export_SingleRecipe_TriggersDownload()
    {
        Guid _pancakesId = new("20000000-0000-0000-0000-000000000001");

        await GotoRecipeAsync(_pancakesId);
        await Expect(Page).ToHaveTitleAsync("Classic Pancakes");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Export recipe" }).ClickAsync();

        await Expect(Page.GetByText("Recipe exported")).ToBeVisibleAsync();

        JsonElement mutations = await GetMutationsAsync();
        Assert.True(HasMutation(mutations, "downloadBlob"), "Expected a downloadBlob mutation for single recipe export");
    }

    [Fact]
    [ReciPageState]
    public async Task Export_Group_TriggersZipDownload()
    {
        await GotoGroupAsync("Breakfast");

        await Page.Locator("fluent-button[title*='Export']").GetByRole(AriaRole.Button).ClickAsync();

        await Expect(Page.GetByText("Group exported")).ToBeVisibleAsync();

        JsonElement mutations = await GetMutationsAsync();
        Assert.True(HasMutation(mutations, "downloadBlob"), "Expected a downloadBlob mutation for group export");
    }

    [Fact]
    [ReciPageState]
    public async Task Export_AllRecipes_TriggersZipDownload()
    {
        await GotoContentsAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Export all recipes" }).ClickAsync();

        await Expect(Page.GetByText("All recipes exported")).ToBeVisibleAsync();

        JsonElement mutations = await GetMutationsAsync();
        Assert.True(HasMutation(mutations, "downloadBlob"), "Expected a downloadBlob mutation for all recipes export");
    }

    [Fact]
    public async Task Export_NoRecipes_StillTriggersDownload()
    {
        await GotoContentsAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Export all recipes" }).ClickAsync();

        // Brief wait for async export
        await Page.WaitForTimeoutAsync(1000);

        // Even with no recipes, the export produces a (potentially empty) download
        JsonElement mutations = await GetMutationsAsync();
        Assert.True(HasMutation(mutations, "downloadBlob"), "Expected downloadBlob mutation even with no recipes");
    }
}
