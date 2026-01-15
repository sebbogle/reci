namespace Reci.Services;

public class DataTransferService(IRecipeRepository recipeRepository, IGroupingRepository groupingRepository, ISettingsRepository settingsRepository) : IDataTransferService
{
    private readonly IRecipeRepository _recipeRepository = recipeRepository.ThrowIfNull();
    private readonly IGroupingRepository _groupingRepository = groupingRepository.ThrowIfNull();
    private readonly ISettingsRepository _settingsRepository = settingsRepository.ThrowIfNull();

    public async Task<ReciFile?> ExportReciDefinitionAsync(CancellationToken cancellationToken = default)
    {
        string version = Core.Version.GetVersionString();

        List<Recipe> recipes = await _recipeRepository.GetRecipesAsync(cancellationToken); 
        List<Group> groups = await _groupingRepository.GetGroupsAsync(cancellationToken);
        Settings settings = await _settingsRepository.GetSettingsAsync(cancellationToken);

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

        if (reciFile.Recipes is not null)
        {
            await _recipeRepository.SetRecipesAsync(reciFile.Recipes, cancellationToken);
        }

        if (reciFile.Groups is not null)
        {
            await _groupingRepository.SetGroups(reciFile.Groups, cancellationToken);
        }

        if (reciFile.Settings is not null)
        {
            await _settingsRepository.SaveSettingsAsync(reciFile.Settings, cancellationToken);
        }

        return Result.Success();
    }
}
