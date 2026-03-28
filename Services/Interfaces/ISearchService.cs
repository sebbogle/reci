namespace Reci.Services.Interfaces;

public interface ISearchService
{
    List<SearchResult> Search(string query, IReadOnlyList<RecipeSummary> summaries);
}
