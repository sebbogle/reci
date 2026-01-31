namespace Reci.Services;

public class RecipeService(IRecipeRepository recipeRepository, IGroupingRepository groupingRepository, IRecipeStateNotifier recipeStateNotifier) : IRecipeService
{
    private readonly IRecipeRepository _recipeRepository = recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));
    private readonly IGroupingRepository _groupingRepository = groupingRepository ?? throw new ArgumentNullException(nameof(groupingRepository));
    private readonly IRecipeStateNotifier _recipeStateNotifier = recipeStateNotifier ?? throw new ArgumentNullException(nameof(recipeStateNotifier));

    public async Task<List<GroupVM<RecipeSummaryVM>>> GetRecipeSummariesAsync(CancellationToken cancellationToken = default)
    {
        List<Recipe> recipes = await _recipeRepository.GetRecipesAsync(cancellationToken);
        List<Group> groups = await _groupingRepository.GetGroupsAsync(cancellationToken);

        List<GroupVM<RecipeSummaryVM>> recipeSummariesVM = recipes.ToViewModelGroups(groups);

        return recipeSummariesVM;
    }

    public async Task<RecipeVM?> GetRecipeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Recipe? recipe = await _recipeRepository.GetRecipeAsync(id, cancellationToken);
        List<Group> groups = await _groupingRepository.GetGroupsAsync(cancellationToken);

        RecipeVM? recipeVM = recipe?.ToViewModel(groups);

        return recipeVM;
    }

    public async Task<Result> SaveRecipeAsync(RecipeVM recipeVM, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recipeVM);
        
        Recipe recipe = recipeVM.ToModel();
        
        Result result;
        if (recipe.Id == Guid.Empty)
        {
            result = await _recipeRepository.CreateRecipeAsync(recipe, cancellationToken);
        }
        else
        {
            result = await _recipeRepository.UpdateRecipeAsync(recipe, cancellationToken);
        }

        if (result.IsSuccess)
        {
            await _recipeStateNotifier.NotifyRecipesChangedAsync();
        }

        return result;
    }

    public async Task<Result> DeleteRecipeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Result result = await _recipeRepository.DeleteRecipeAsync(id, cancellationToken);

        if (result.IsSuccess)
        {
            await _recipeStateNotifier.NotifyRecipesChangedAsync();
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
