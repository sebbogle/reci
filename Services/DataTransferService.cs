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

        PopulateEmptyGuids(reciFile);

        if (reciFile.Recipes is not null)
        {
            _logger.LogDebug("Importing {RecipeCount} recipes", reciFile.Recipes.Count);
            await _recipeRepository.SetRecipesAsync(reciFile.Recipes, cancellationToken);
        }

        if (reciFile.Groups is not null)
        {
            _logger.LogDebug("Importing {GroupCount} groups", reciFile.Groups.Count);
            await _groupingRepository.SetGroups(reciFile.Groups, cancellationToken);
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

    private static void PopulateEmptyGuids(ReciFile reciFile)
    {
        reciFile.Recipes?.ForEach(r => r.Id.PopulateIfEmpty());
        reciFile.Groups?.ForEach(g => g.Id.PopulateIfEmpty());
    }
}
