namespace Reci.Data.Repositories;

using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using Reci.Data.Models;
using Reci.Data.Repositories.Interfaces;

public class LocalStorageRecipeRepository(ILocalStorageService localStorage, ILogger<LocalStorageRecipeRepository> logger) : IRecipeRepository
{
    private const string RecipesStorageKey = "recipes";
    private readonly ILocalStorageService _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
    private readonly ILogger<LocalStorageRecipeRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    
    private List<Recipe>? _cachedRecipes;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    public async Task<Recipe?> GetRecipeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var recipes = await GetOrLoadCachedRecipesAsync(cancellationToken);
            return recipes.FirstOrDefault(r => r.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recipe with ID {RecipeId}", id);
            return null;
        }
    }

    public async Task<List<Recipe>> GetRecipesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetOrLoadCachedRecipesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all recipes");
            return [];
        }
    }

    public async Task<Result> CreateRecipeAsync(Recipe recipe, CancellationToken cancellationToken = default)
    {
        try
        {
            if (recipe == null)
                return Result.Failure("Recipe cannot be null");

            if (string.IsNullOrWhiteSpace(recipe.Name))
                return Result.Failure("Recipe name is required");

            var recipes = await GetOrLoadCachedRecipesAsync(cancellationToken);

            // Ensure the recipe has a unique ID
            if (recipe.Id == Guid.Empty)
                recipe.Id = Guid.NewGuid();

            // Check if recipe with this ID already exists
            if (recipes.Any(r => r.Id == recipe.Id))
                return Result.Failure($"Recipe with ID {recipe.Id} already exists");

            recipes.Add(recipe);
            await SaveAndUpdateCacheAsync(recipes, cancellationToken);

            _logger.LogInformation("Successfully created recipe with ID {RecipeId} and name '{RecipeName}'", recipe.Id, recipe.Name);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating recipe with name '{RecipeName}'", recipe?.Name);
            return Result.Failure($"Failed to create recipe: {ex.Message}");
        }
    }

    public async Task<Result> UpdateRecipeAsync(Recipe recipe, CancellationToken cancellationToken = default)
    {
        try
        {
            if (recipe == null)
                return Result.Failure("Recipe cannot be null");

            if (recipe.Id == Guid.Empty)
                return Result.Failure("Recipe ID is required");

            if (string.IsNullOrWhiteSpace(recipe.Name))
                return Result.Failure("Recipe name is required");

            var recipes = await GetOrLoadCachedRecipesAsync(cancellationToken);
            var existingRecipeIndex = recipes.FindIndex(r => r.Id == recipe.Id);

            if (existingRecipeIndex == -1)
                return Result.Failure($"Recipe with ID {recipe.Id} not found");

            recipes[existingRecipeIndex] = recipe;
            await SaveAndUpdateCacheAsync(recipes, cancellationToken);

            _logger.LogInformation("Successfully updated recipe with ID {RecipeId} and name '{RecipeName}'", recipe.Id, recipe.Name);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating recipe with ID {RecipeId}", recipe?.Id);
            return Result.Failure($"Failed to update recipe: {ex.Message}");
        }
    }

    public async Task<Result> DeleteRecipeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id == Guid.Empty)
                return Result.Failure("Recipe ID is required");

            var recipes = await GetOrLoadCachedRecipesAsync(cancellationToken);
            var recipeToRemove = recipes.FirstOrDefault(r => r.Id == id);

            if (recipeToRemove == null)
                return Result.Failure($"Recipe with ID {id} not found");

            recipes.Remove(recipeToRemove);
            await SaveAndUpdateCacheAsync(recipes, cancellationToken);

            _logger.LogInformation("Successfully deleted recipe with ID {RecipeId}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting recipe with ID {RecipeId}", id);
            return Result.Failure($"Failed to delete recipe: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets recipes from cache or loads from LocalStorage if cache is empty.
    /// Thread-safe using SemaphoreSlim to prevent multiple simultaneous loads.
    /// </summary>
    private async Task<List<Recipe>> GetOrLoadCachedRecipesAsync(CancellationToken cancellationToken = default)
    {
        // Fast path: cache already loaded
        if (_cachedRecipes != null)
            return _cachedRecipes;

        // Slow path: load from storage with lock to prevent multiple loads
        await _cacheLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check pattern: another thread may have loaded while we waited
            if (_cachedRecipes != null)
                return _cachedRecipes;

            _logger.LogDebug("Loading recipes from LocalStorage into cache");
            var recipes = await _localStorage.GetItemAsync<List<Recipe>>(RecipesStorageKey);
            _cachedRecipes = recipes ?? new List<Recipe>();
            
            return _cachedRecipes;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private async Task SaveAndUpdateCacheAsync(List<Recipe> recipes, CancellationToken cancellationToken = default)
    {
        await _localStorage.SetItemAsync(RecipesStorageKey, recipes);
        
        await _cacheLock.WaitAsync(cancellationToken);
        try
        {
            _cachedRecipes = recipes;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task<Result> SetRecipesAsync(List<Recipe> recipes, CancellationToken cancellationToken = default)
    {
        try
        {
            await _localStorage.SetItemAsync(RecipesStorageKey, recipes);
            await _cacheLock.WaitAsync(cancellationToken);
            try
            {
                _cachedRecipes = recipes;
            }
            finally
            {
                _cacheLock.Release();
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting recipes");
            return Result.Failure($"Failed to set recipes: {ex.Message}");
        }
    }
}
