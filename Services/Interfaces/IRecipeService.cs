namespace Reci.Services.Interfaces;

public interface IRecipeService
{
    Task<List<RecipeSummary>> GetRecipeSummariesAsync(CancellationToken cancellationToken = default);

    Task<Recipe?> GetRecipeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result> SaveRecipeAsync(Recipe recipe, CancellationToken cancellationToken = default);

    Task<Result> DeleteRecipeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> IsRecipeModifiedAsync(Recipe recipe, CancellationToken cancellationToken = default);

    bool IsRecipeEmpty(Recipe recipe);
}
