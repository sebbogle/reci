using System.Text.RegularExpressions;
using Group = Reci.Data.Models.Group;

namespace Reci.Services;

public partial class DataTransferService(IRecipeRepository recipeRepository, IGroupingRepository groupingRepository, ISettingsRepository settingsRepository, IRecipeStateNotifier recipeStateNotifier, ILogger<DataTransferService> logger) : IDataTransferService
{
    private readonly IRecipeRepository _recipeRepository = recipeRepository.ThrowIfNull();
    private readonly IGroupingRepository _groupingRepository = groupingRepository.ThrowIfNull();
    private readonly ISettingsRepository _settingsRepository = settingsRepository.ThrowIfNull();
    private readonly IRecipeStateNotifier _recipeStateNotifier = recipeStateNotifier.ThrowIfNull();
    private readonly ILogger<DataTransferService> _logger = logger.ThrowIfNull();

    [GeneratedRegex(@"""Id""\s*:\s*""([^""]+)""")]
    private static partial Regex IdPropertyRegex();

    public async Task<ReciFile?> ExportReciDefinitionAsync(CancellationToken cancellationToken = default)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting Reci definition");
            return null;
        }
    }

    public async Task<Result> ImportReciDefinitionAsync(ReciFile reciFile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reciFile);

        try
        {
            _logger.LogDebug("Starting import of Reci definition (version {Version})", reciFile.Version);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing Reci definition (version {Version})", reciFile.Version);
            return Result.Failure($"Failed to import: {ex.Message}");
        }
    }


    public string MendGuidsFromImportedData(string data)
    {
        foreach (Match match in IdPropertyRegex().Matches(data))
        {
            string idValue = match.Groups[1].Value;

            if (!Guid.TryParse(idValue, out Guid _))
            {
                data = data.Replace(idValue, Guid.NewGuid().ToString());
            }
        }

        return data;
    }
}
