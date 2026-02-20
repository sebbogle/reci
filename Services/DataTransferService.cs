namespace Reci.Services;

public class DataTransferService(IRecipeRepository recipeRepository, IGroupingRepository groupingRepository, ISettingsRepository settingsRepository, IRecipeStateNotifier recipeStateNotifier, ILogger<DataTransferService> logger) : IDataTransferService
{
    private readonly IRecipeRepository _recipeRepository = recipeRepository.ThrowIfNull();
    private readonly IGroupingRepository _groupingRepository = groupingRepository.ThrowIfNull();
    private readonly ISettingsRepository _settingsRepository = settingsRepository.ThrowIfNull();
    private readonly IRecipeStateNotifier _recipeStateNotifier = recipeStateNotifier.ThrowIfNull();
    private readonly ILogger<DataTransferService> _logger = logger.ThrowIfNull();

    public async Task<ReciFile?> ExportReciDefinitionAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Starting export of Reci definition");
        string version = Core.Version.GetVersionString();

        List<Recipe> recipes = await _recipeRepository.GetRecipesAsync(cancellationToken); 
        List<Group> groups = await _groupingRepository.GetGroupsAsync(cancellationToken);
        Settings settings = await _settingsRepository.GetSettingsAsync(cancellationToken);

        _logger.LogInformation("Exported Reci definition with {RecipeCount} recipes and {GroupCount} groups (version {Version})", recipes.Count, groups.Count, version);

        return new ReciFile
        {
            Version = version,
            Settings = settings,
            Recipes = recipes.Any() ? recipes : null,
            Groups = groups.Any() ? groups : null
        };
    }

    public async Task<Result> ImportReciDefinitionAsync(ReciFile reciFile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reciFile);

        _logger.LogDebug("Starting import of Reci definition (version {Version})", reciFile.Version);

        // Process groups first to get valid group IDs
        HashSet<Guid> validGroupIds = [];
        if (reciFile.Groups is not null)
        {
            _logger.LogDebug("Importing {GroupCount} groups", reciFile.Groups.Count);
            List<Group> processedGroups = NormalizeGroupIds(reciFile.Groups);
            validGroupIds = processedGroups.Select(g => g.Id).ToHashSet();
            await _groupingRepository.SetGroups(processedGroups, cancellationToken);
        }

        if (reciFile.Recipes is not null)
        {
            _logger.LogDebug("Importing {RecipeCount} recipes", reciFile.Recipes.Count);
            List<Recipe> processedRecipes = NormalizeRecipeIds(reciFile.Recipes, validGroupIds);
            await _recipeRepository.SetRecipesAsync(processedRecipes, cancellationToken);
        }

        if (reciFile.Settings is not null)
        {
            _logger.LogDebug("Importing settings");
            await _settingsRepository.SaveSettingsAsync(reciFile.Settings, cancellationToken);
        }

        await _recipeStateNotifier.NotifyRecipesChangedAsync();

        _logger.LogInformation("Successfully imported Reci definition (version {Version})", reciFile.Version);
        return Result.Success();
    }

    private List<Group> NormalizeGroupIds(List<Group> groups)
    {
        List<Group> processedGroups = [];
        Dictionary<Guid, Guid> guidMapping = [];

        foreach (Group group in groups)
        {
            if (group.Id == Guid.Empty)
            {
                Guid newId = Guid.NewGuid();
                guidMapping[Guid.Empty] = newId;
                _logger.LogDebug("Generated new GUID {NewId} for group '{GroupName}' with empty GUID", newId, group.Name);

                processedGroups.Add(new Group
                {
                    Id = newId,
                    Name = group.Name,
                    SortOrder = group.SortOrder,
                    GroupType = group.GroupType
                });
            }
            else
            {
                processedGroups.Add(group);
            }
        }

        return processedGroups;
    }

    private List<Recipe> NormalizeRecipeIds(List<Recipe> recipes, HashSet<Guid> validGroupIds)
    {
        List<Recipe> processedRecipes = [];

        foreach (Recipe recipe in recipes)
        {
            Recipe processedRecipe = new Recipe
            {
                Id = recipe.Id == Guid.Empty ? Guid.NewGuid() : recipe.Id,
                Name = recipe.Name,
                Description = recipe.Description,
                GroupId = NormalizeGroupReference(recipe.GroupId, validGroupIds),
                Ingredients = NormalizeIngredientGroupIds(recipe.Ingredients, validGroupIds),
                Instructions = NormalizeInstructionGroupIds(recipe.Instructions, validGroupIds),
                NutritionInfo = recipe.NutritionInfo,
                Source = recipe.Source,
                Tags = recipe.Tags,
                FurtherNotes = recipe.FurtherNotes
            };

            if (recipe.Id == Guid.Empty)
            {
                _logger.LogDebug("Generated new GUID {NewId} for recipe '{RecipeName}' with empty GUID", processedRecipe.Id, recipe.Name);
            }

            if (recipe.GroupId != processedRecipe.GroupId)
            {
                _logger.LogDebug("Normalized GroupId for recipe '{RecipeName}' from {OldGroupId} to null (invalid reference)", recipe.Name, recipe.GroupId);
            }

            processedRecipes.Add(processedRecipe);
        }

        return processedRecipes;
    }

    private static Guid? NormalizeGroupReference(Guid? groupId, HashSet<Guid> validGroupIds)
    {
        if (groupId is null || groupId == Guid.Empty)
        {
            return null;
        }

        return validGroupIds.Contains(groupId.Value) ? groupId : null;
    }

    private static List<Ingredient> NormalizeIngredientGroupIds(List<Ingredient> ingredients, HashSet<Guid> validGroupIds)
    {
        List<Ingredient> processedIngredients = [];

        foreach (Ingredient ingredient in ingredients)
        {
            processedIngredients.Add(new Ingredient
            {
                Name = ingredient.Name,
                QuantityAmount = ingredient.QuantityAmount,
                QuantityUnit = ingredient.QuantityUnit,
                GroupId = NormalizeGroupReference(ingredient.GroupId, validGroupIds)
            });
        }

        return processedIngredients;
    }

    private static List<Instruction> NormalizeInstructionGroupIds(List<Instruction> instructions, HashSet<Guid> validGroupIds)
    {
        List<Instruction> processedInstructions = [];

        foreach (Instruction instruction in instructions)
        {
            processedInstructions.Add(new Instruction
            {
                Text = instruction.Text,
                GroupId = NormalizeGroupReference(instruction.GroupId, validGroupIds)
            });
        }

        return processedInstructions;
    }
}
