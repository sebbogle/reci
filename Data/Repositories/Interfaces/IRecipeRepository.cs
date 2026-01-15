using Reci.Data.Models;

namespace Reci.Data.Repositories.Interfaces;

public interface IRecipeRepository
{
    Task<Recipe?> GetRecipeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Recipe>> GetRecipesAsync(CancellationToken cancellationToken = default);

    Task<Result> CreateRecipeAsync(Recipe recipe, CancellationToken cancellationToken = default);

    Task<Result> UpdateRecipeAsync(Recipe recipe, CancellationToken cancellationToken = default);

    Task<Result> DeleteRecipeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result> SetRecipesAsync(List<Recipe> recipes, CancellationToken cancellationToken = default);
}
