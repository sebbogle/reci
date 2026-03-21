namespace Reci.Services.Interfaces;

public interface ISearchService
{
    Task<List<SearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
