namespace Reci.Services;

public class RecipeService(IRecipeRepository recipeRepository, IGroupingRepository groupingRepository, IRecipeStateNotifier recipeStateNotifier, ILogger<RecipeService> logger) : IRecipeService
{
    private readonly IRecipeRepository _recipeRepository = recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));
    private readonly IGroupingRepository _groupingRepository = groupingRepository ?? throw new ArgumentNullException(nameof(groupingRepository));
    private readonly IRecipeStateNotifier _recipeStateNotifier = recipeStateNotifier ?? throw new ArgumentNullException(nameof(recipeStateNotifier));
    private readonly ILogger<RecipeService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<GroupVM<RecipeSummaryVM>>> GetRecipeSummariesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving recipe summaries");
        List<Recipe> recipes = await _recipeRepository.GetRecipesAsync(cancellationToken);
        List<Group> groups = await _groupingRepository.GetGroupsAsync(cancellationToken);

        List<GroupVM<RecipeSummaryVM>> recipeSummariesVM = recipes.ToViewModelGroups(groups);

        _logger.LogInformation("Retrieved {RecipeCount} recipe summaries across {GroupCount} groups", recipeSummariesVM.Sum(g => g.Count), recipeSummariesVM.Count);
        return recipeSummariesVM;
    }

    public async Task<RecipeVM?> GetRecipeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving recipe with ID {RecipeId}", id);
        Recipe? recipe = await _recipeRepository.GetRecipeAsync(id, cancellationToken);
        List<Group> groups = await _groupingRepository.GetGroupsAsync(cancellationToken);

        RecipeVM? recipeVM = recipe?.ToViewModel(groups);

        if (recipeVM is null)
        {
            _logger.LogWarning("Recipe with ID {RecipeId} not found", id);
        }

        return recipeVM;
    }

    public async Task<Result> SaveRecipeAsync(RecipeVM recipeVM, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recipeVM);
        
        Recipe recipe = recipeVM.ToModel();
        
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

    public async Task<bool> IsRecipeModifiedAsync(RecipeVM recipeVM, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recipeVM);

        if (recipeVM.Id == null || recipeVM.Id == Guid.Empty)
        {
            return true;
        }

        Recipe? originalRecipe = await _recipeRepository.GetRecipeAsync(recipeVM.Id.Value, cancellationToken);

        if (originalRecipe == null)
        {
            return true;
        }

        Recipe currentRecipe = recipeVM.ToModel();

        return !currentRecipe.IsEqualTo(originalRecipe);
    }

    public bool IsRecipeEmpty(RecipeVM recipeVM)
    {
        ArgumentNullException.ThrowIfNull(recipeVM);

        Recipe recipe = recipeVM.ToModel();

        return recipe.IsEmpty();
    }
}
