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

    public async Task<Recipe?> GetRecipeAsync(RecipeKey key, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving recipe '{RecipeName}' in group '{Group}'", key.Name, key.Group);
        Recipe? recipe = await _recipeRepository.GetRecipeAsync(key, cancellationToken);

        if (recipe is null)
        {
            _logger.LogWarning("Recipe '{RecipeName}' not found in group '{Group}'", key.Name, key.Group);
        }

        return recipe;
    }

    public async Task<Result> SaveRecipeAsync(Recipe recipe, RecipeKey? originalKey = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        Result result;
        if (recipe.IsNew)
        {
            _logger.LogDebug("Creating new recipe '{RecipeName}'", recipe.Name);
            result = await _recipeRepository.CreateRecipeAsync(recipe, cancellationToken);
        }
        else
        {
            RecipeKey key = originalKey ?? new RecipeKey(recipe.Name, recipe.Group);
            _logger.LogDebug("Updating recipe '{RecipeName}'", recipe.Name);
            result = await _recipeRepository.UpdateRecipeAsync(recipe, key, cancellationToken);
        }

        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully saved recipe '{RecipeName}'", recipe.Name);
            await _recipeStateNotifier.NotifyRecipesChangedAsync();
        }
        else
        {
            _logger.LogWarning("Failed to save recipe '{RecipeName}': {Error}", recipe.Name, result.ErrorMessage);
        }

        return result;
    }

    public async Task<Result> DeleteRecipeAsync(RecipeKey key, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting recipe '{RecipeName}' in group '{Group}'", key.Name, key.Group);
        Result result = await _recipeRepository.DeleteRecipeAsync(key, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully deleted recipe '{RecipeName}'", key.Name);
            await _recipeStateNotifier.NotifyRecipesChangedAsync();
        }
        else
        {
            _logger.LogWarning("Failed to delete recipe '{RecipeName}': {Error}", key.Name, result.ErrorMessage);
        }

        return result;
    }

    public async Task<bool> IsRecipeModifiedAsync(Recipe recipe, RecipeKey? originalKey = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        if (recipe.IsNew)
        {
            return true;
        }

        RecipeKey key = originalKey ?? new RecipeKey(recipe.Name, recipe.Group);
        Recipe? originalRecipe = await _recipeRepository.GetRecipeAsync(key, cancellationToken);

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
