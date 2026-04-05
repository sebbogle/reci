namespace Tests.Unit.Services;

public class SearchServiceTests
{
    private readonly SearchService _service = new();

    private static RecipeSummary MakeSummary(
        string name,
        string? group = null,
        string? description = null,
        params string[] tags)
    {
        return new RecipeSummary
        {
            Name = name,
            Group = group,
            Description = description,
            Tags = [.. tags]
        };
    }

    #region Basic

    [Fact]
    public void Search_EmptyQuery_ReturnsEmpty()
    {
        List<RecipeSummary> summaries = [MakeSummary("Pancakes")];
        Assert.Empty(_service.Search("", summaries));
    }

    [Fact]
    public void Search_WhitespaceQuery_ReturnsEmpty()
    {
        List<RecipeSummary> summaries = [MakeSummary("Pancakes")];
        Assert.Empty(_service.Search("   ", summaries));
    }

    [Fact]
    public void Search_NoMatches_ReturnsEmpty()
    {
        List<RecipeSummary> summaries = [MakeSummary("Pancakes")];
        Assert.Empty(_service.Search("zzz", summaries));
    }

    [Fact]
    public void Search_EmptySummaries_ReturnsEmpty()
    {
        Assert.Empty(_service.Search("test", []));
    }

    #endregion

    #region Name Match (Pass 1)

    [Fact]
    public void Search_ExactNameMatch_ReturnsRecipe()
    {
        List<RecipeSummary> summaries = [MakeSummary("Classic Pancakes", "Breakfast")];
        List<SearchResult> results = _service.Search("Classic Pancakes", summaries);
        Assert.Single(results);
        Assert.Equal("Classic Pancakes", results[0].Name);
    }

    [Fact]
    public void Search_PartialNameMatch_CaseInsensitive()
    {
        List<RecipeSummary> summaries = [MakeSummary("Classic Pancakes")];
        List<SearchResult> results = _service.Search("pancakes", summaries);
        Assert.Single(results);
    }

    [Fact]
    public void Search_MultipleNameMatches_ReturnsAll()
    {
        List<RecipeSummary> summaries =
        [
            MakeSummary("Chicken Soup"),
            MakeSummary("Chicken Curry"),
            MakeSummary("Beef Stew")
        ];
        List<SearchResult> results = _service.Search("Chicken", summaries);
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void Search_NameMatch_HasRecipeKind()
    {
        List<RecipeSummary> summaries = [MakeSummary("Pancakes")];
        List<SearchResult> results = _service.Search("Pancakes", summaries);
        Assert.Equal(SearchResultKind.Recipe, results[0].Kind);
        Assert.Equal(MatchMethod.Name, results[0].MatchMethod);
    }

    #endregion

    #region Group Match (Pass 2)

    [Fact]
    public void Search_GroupNameMatch_ReturnsGroupResult()
    {
        List<RecipeSummary> summaries = [MakeSummary("Pancakes", "Breakfast")];
        List<SearchResult> results = _service.Search("Break", summaries);
        Assert.Contains(results, r => r.Kind == SearchResultKind.Group && r.Name == "Breakfast");
    }

    [Fact]
    public void Search_DuplicateGroups_ReturnsDistinct()
    {
        List<RecipeSummary> summaries =
        [
            MakeSummary("Pancakes", "Breakfast"),
            MakeSummary("Waffles", "Breakfast")
        ];
        List<SearchResult> results = _service.Search("Break", summaries);
        int groupResults = results.Count(r => r.Kind == SearchResultKind.Group);
        Assert.Equal(1, groupResults);
    }

    [Fact]
    public void Search_GroupMatch_CorrectNavigationUrl()
    {
        List<RecipeSummary> summaries = [MakeSummary("Pancakes", "Breakfast")];
        List<SearchResult> results = _service.Search("Break", summaries);
        SearchResult groupResult = results.First(r => r.Kind == SearchResultKind.Group);
        Assert.Equal("/group/Breakfast", groupResult.NavigationUrl);
    }

    #endregion

    #region Tag Match (Pass 3)

    [Fact]
    public void Search_TagMatch_ReturnsRecipe()
    {
        List<RecipeSummary> summaries = [MakeSummary("Spaghetti", "Dinner", null, "pasta", "italian")];
        List<SearchResult> results = _service.Search("pasta", summaries);
        Assert.Contains(results, r => r.Name == "Spaghetti");
    }

    [Fact]
    public void Search_TagMatch_HasTagMatchMethod()
    {
        List<RecipeSummary> summaries = [MakeSummary("Spaghetti", null, null, "pasta")];
        List<SearchResult> results = _service.Search("pasta", summaries);
        SearchResult tagResult = results.First(r => r.Kind == SearchResultKind.Recipe);
        Assert.Equal(MatchMethod.Tag, tagResult.MatchMethod);
    }

    [Fact]
    public void Search_TagMatch_SkipsAlreadyMatchedByName()
    {
        // "pasta" matches both name and tag -- should appear only once via name match
        List<RecipeSummary> summaries = [MakeSummary("Pasta Salad", null, null, "pasta")];
        List<SearchResult> results = _service.Search("pasta", summaries);
        Assert.Single(results, r => r.Kind == SearchResultKind.Recipe);
        Assert.Equal(MatchMethod.Name, results.First(r => r.Kind == SearchResultKind.Recipe).MatchMethod);
    }

    #endregion

    #region Description Match (Pass 4)

    [Fact]
    public void Search_DescriptionMatch_ReturnsRecipe()
    {
        List<RecipeSummary> summaries = [MakeSummary("Bolognese", "Dinner", "A hearty Italian meat sauce")];
        List<SearchResult> results = _service.Search("hearty", summaries);
        Assert.Contains(results, r => r.Name == "Bolognese");
    }

    [Fact]
    public void Search_DescriptionMatch_HasDescriptionMatchMethod()
    {
        List<RecipeSummary> summaries = [MakeSummary("Bolognese", null, "A hearty Italian meat sauce")];
        List<SearchResult> results = _service.Search("hearty", summaries);
        Assert.Equal(MatchMethod.Description, results[0].MatchMethod);
    }

    [Fact]
    public void Search_DescriptionMatch_SkipsAlreadyMatchedByName()
    {
        List<RecipeSummary> summaries = [MakeSummary("Hearty Soup", null, "A hearty recipe")];
        List<SearchResult> results = _service.Search("hearty", summaries);
        Assert.Single(results, r => r.Kind == SearchResultKind.Recipe);
        Assert.Equal(MatchMethod.Name, results[0].MatchMethod);
    }

    #endregion

    #region Priority and Limits

    [Fact]
    public void Search_PriorityOrder_NameBeforeTag()
    {
        List<RecipeSummary> summaries =
        [
            MakeSummary("Salad", null, null, "chicken"),
            MakeSummary("Chicken Soup")
        ];
        List<SearchResult> results = _service.Search("chicken", summaries);
        // Name match (Chicken Soup) should come before tag match (Salad)
        List<SearchResult> recipeResults = results.Where(r => r.Kind == SearchResultKind.Recipe).ToList();
        Assert.Equal("Chicken Soup", recipeResults[0].Name);
        Assert.Equal("Salad", recipeResults[1].Name);
    }

    [Fact]
    public void Search_PriorityOrder_TagBeforeDescription()
    {
        List<RecipeSummary> summaries =
        [
            MakeSummary("Recipe A", null, "Uses Italian herbs"),
            MakeSummary("Recipe B", null, null, "italian")
        ];
        List<SearchResult> results = _service.Search("italian", summaries);
        List<SearchResult> recipeResults = results.Where(r => r.Kind == SearchResultKind.Recipe).ToList();
        Assert.Equal("Recipe B", recipeResults[0].Name);
        Assert.Equal("Recipe A", recipeResults[1].Name);
    }

    [Fact]
    public void Search_MaxResults_StopsAt10()
    {
        List<RecipeSummary> summaries = Enumerable.Range(1, 15)
            .Select(i => MakeSummary($"Chicken Recipe {i}"))
            .ToList();
        List<SearchResult> results = _service.Search("Chicken", summaries);
        Assert.Equal(10, results.Count);
    }

    [Fact]
    public void Search_MaxResults_NameMatchesFillFirst()
    {
        // 10 name matches + 1 tag match -- tag match should be excluded
        List<RecipeSummary> summaries = Enumerable.Range(1, 10)
            .Select(i => MakeSummary($"Chicken Recipe {i}"))
            .ToList();
        summaries.Add(MakeSummary("Tofu Stir Fry", null, null, "chicken"));

        List<SearchResult> results = _service.Search("Chicken", summaries);
        Assert.Equal(10, results.Count);
        Assert.DoesNotContain(results, r => r.Name == "Tofu Stir Fry");
    }

    #endregion

    #region Deduplication

    [Fact]
    public void Search_RecipeMatchedByNameAndTag_AppearsOnce()
    {
        List<RecipeSummary> summaries = [MakeSummary("Pasta Primavera", null, null, "pasta")];
        List<SearchResult> results = _service.Search("pasta", summaries);
        Assert.Single(results, r => r.Kind == SearchResultKind.Recipe);
    }

    [Fact]
    public void Search_RecipeMatchedByNameAndDescription_AppearsOnce()
    {
        List<RecipeSummary> summaries = [MakeSummary("Hearty Stew", null, "A hearty recipe")];
        List<SearchResult> results = _service.Search("hearty", summaries);
        Assert.Single(results, r => r.Kind == SearchResultKind.Recipe);
    }

    #endregion
}
