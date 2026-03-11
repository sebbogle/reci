namespace Reci.Services;

public class RecipeService(IRecipeRepository recipeRepository, IRecipeStateNotifier recipeStateNotifier, ILogger<RecipeService> logger) : IRecipeService
{
    private readonly IRecipeRepository _recipeRepository = recipeRepository.ThrowIfNull();
    private readonly IRecipeStateNotifier _recipeStateNotifier = recipeStateNotifier.ThrowIfNull();
    private readonly ILogger<RecipeService> _logger = logger.ThrowIfNull();

    public async Task<List<RecipeSummary>> GetRecipeSummariesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving recipe summaries");
        List<RecipeSummary> summaries = await _recipeRepository.GetRecipeSummariesAsync(cancellationToken);

        _logger.LogInformation("Retrieved {RecipeCount} recipe summaries", summaries.Count);
        return summaries;
    }

    public async Task<Recipe?> GetRecipeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving recipe with ID {RecipeId}", id);
        Recipe? recipe = await _recipeRepository.GetRecipeAsync(id, cancellationToken);

        if (recipe is null)
        {
            _logger.LogWarning("Recipe with ID {RecipeId} not found", id);
        }

        return recipe;
    }

    public async Task<Result> SaveRecipeAsync(Recipe recipe, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        Result result;
        if (recipe.Id == Guid.Empty)
        {
            _logger.LogDebug("Creating new recipe '{RecipeName}'", recipe.Name);
            result = await _recipeRepository.CreateRecipeAsync(recipe, cancellationToken);
        }
        else
        {
            _logger.LogDebug("Updating recipe with ID {RecipeId}", recipe.Id);
            result = await _recipeRepository.UpdateRecipeAsync(recipe, cancellationToken);
        }

        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully saved recipe '{RecipeName}' with ID {RecipeId}", recipe.Name, recipe.Id);
            await _recipeStateNotifier.NotifyRecipesChangedAsync();
        }
        else
        {
            _logger.LogWarning("Failed to save recipe '{RecipeName}': {Error}", recipe.Name, result.ErrorMessage);
        }

        return result;
    }

    public async Task<Result> DeleteRecipeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting recipe with ID {RecipeId}", id);
        Result result = await _recipeRepository.DeleteRecipeAsync(id, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully deleted recipe with ID {RecipeId}", id);
            await _recipeStateNotifier.NotifyRecipesChangedAsync();
        }
        else
        {
            _logger.LogWarning("Failed to delete recipe with ID {RecipeId}: {Error}", id, result.ErrorMessage);
        }

        return result;
    }

    public async Task<bool> IsRecipeModifiedAsync(Recipe recipe, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        if (recipe.Id == Guid.Empty)
        {
            return true;
        }

        Recipe? originalRecipe = await _recipeRepository.GetRecipeAsync(recipe.Id, cancellationToken);

        if (originalRecipe == null)
        {
            return true;
        }

        return !recipe.IsEqualTo(originalRecipe);
    }

    public bool IsRecipeEmpty(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        return recipe.IsEmpty();
    }
}
