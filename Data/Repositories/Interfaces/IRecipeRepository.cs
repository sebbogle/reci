namespace Reci.Data.Repositories.Interfaces;

public interface IRecipeRepository
{
    Task<Recipe?> GetRecipeAsync(RecipeKey key, CancellationToken cancellationToken = default);

    Task<List<Recipe>> GetRecipesAsync(CancellationToken cancellationToken = default);

    Task<List<RecipeSummary>> GetRecipeSummariesAsync(CancellationToken cancellationToken = default);

    Task<Result> CreateRecipeAsync(Recipe recipe, CancellationToken cancellationToken = default);

    Task<Result> UpdateRecipeAsync(Recipe recipe, RecipeKey originalKey, CancellationToken cancellationToken = default);

    Task<Result> DeleteRecipeAsync(RecipeKey key, CancellationToken cancellationToken = default);
}
