namespace Reci.Services.Interfaces;

public interface IRecipeService
{
    Task<List<GroupVM<RecipeSummaryVM>>> GetRecipeSummariesAsync(CancellationToken cancellationToken = default);

    Task<RecipeVM?> GetRecipeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result> SaveRecipeAsync(RecipeVM recipeVM, CancellationToken cancellationToken = default);

    Task<Result> DeleteRecipeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> IsRecipeModifiedAsync(RecipeVM recipeVM, CancellationToken cancellationToken = default);

    bool IsRecipeEmpty(RecipeVM recipeVM);
}
