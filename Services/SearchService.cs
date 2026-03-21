namespace Reci.Services;

public class SearchService(IRecipeService recipeService) : ISearchService
{
    private const int MaxResults = 10;

    public async Task<List<SearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        List<RecipeSummary> summaries = await recipeService.GetRecipeSummariesAsync(cancellationToken);
        List<SearchResult> results = new(MaxResults);
        HashSet<string> matchedSlugs = [];

        foreach (RecipeSummary summary in summaries)
        {
            if (results.Count >= MaxResults)
            {
                break;
            }

            if (summary.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                matchedSlugs.Add(summary.Slug);
                results.Add(CreateRecipeResult(summary, MatchMethod.Name));
            }
        }

        if (results.Count < MaxResults)
        {
            IEnumerable<string> distinctGroups = summaries
                .Select(s => s.Group)
                .Where(g => g is not null && g.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)!;

            foreach (string group in distinctGroups)
            {
                if (results.Count >= MaxResults)
                {
                    break;
                }

                results.Add(new SearchResult
                {
                    Name = group,
                    Kind = SearchResultKind.Group,
                    MatchMethod = MatchMethod.Name,
                    NavigationUrl = $"/group/{Uri.EscapeDataString(group)}"
                });
            }
        }

        if (results.Count < MaxResults)
        {
            foreach (RecipeSummary summary in summaries)
            {
                if (results.Count >= MaxResults)
                {
                    break;
                }

                if (matchedSlugs.Contains(summary.Slug))
                {
                    continue;
                }

                if (summary.Tags.Any(tag => tag.Contains(query, StringComparison.OrdinalIgnoreCase)))
                {
                    matchedSlugs.Add(summary.Slug);
                    results.Add(CreateRecipeResult(summary, MatchMethod.Tag));
                }
            }
        }

        if (results.Count < MaxResults)
        {
            foreach (RecipeSummary summary in summaries)
            {
                if (results.Count >= MaxResults)
                {
                    break;
                }

                if (matchedSlugs.Contains(summary.Slug))
                {
                    continue;
                }

                if (summary.Description is not null
                    && summary.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    matchedSlugs.Add(summary.Slug);
                    results.Add(CreateRecipeResult(summary, MatchMethod.Description));
                }
            }
        }

        return results;
    }

    private static SearchResult CreateRecipeResult(RecipeSummary summary, MatchMethod matchMethod)
    {
        return new SearchResult
        {
            Name = summary.Name,
            Kind = SearchResultKind.Recipe,
            MatchMethod = matchMethod,
            NavigationUrl = $"/Recipe/{summary.Slug}"
        };
    }
}
