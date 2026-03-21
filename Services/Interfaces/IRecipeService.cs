namespace Reci.Services.Interfaces;

public interface IRecipeService
{
    Task<List<RecipeSummary>> GetRecipeSummariesAsync(CancellationToken cancellationToken = default);

    Task<Recipe?> GetRecipeAsync(RecipeKey key, CancellationToken cancellationToken = default);

    Task<Result> SaveRecipeAsync(Recipe recipe, RecipeKey? originalKey = null, CancellationToken cancellationToken = default);

    Task<Result> DeleteRecipeAsync(RecipeKey key, CancellationToken cancellationToken = default);

    Task<bool> IsRecipeModifiedAsync(Recipe recipe, RecipeKey? originalKey = null, CancellationToken cancellationToken = default);

    bool IsRecipeEmpty(Recipe recipe);
}
