namespace Reci.Services;

public class RecipeExportService(IRecipeRepository recipeRepository, ILogger<RecipeExportService> logger, JsonSerializerOptions jsonOptions) : IRecipeExportService
{
    private readonly IRecipeRepository _recipeRepository = recipeRepository.ThrowIfNull();
    private readonly ILogger<RecipeExportService> _logger = logger.ThrowIfNull();
    private readonly JsonSerializerOptions _jsonOptions = jsonOptions.ThrowIfNull();

    public async Task<byte[]?> ExportAllAsZipAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Starting full zip export");

            List<Recipe> recipes = await _recipeRepository.GetRecipesAsync(cancellationToken);

            byte[] zipBytes = BuildZip(recipes);

            _logger.LogInformation("Exported {RecipeCount} recipes to zip", recipes.Count);
            return zipBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting all recipes as zip");
            return null;
        }
    }

    public async Task<byte[]?> ExportGroupAsZipAsync(string groupName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);

        try
        {
            _logger.LogDebug("Starting group zip export for '{GroupName}'", groupName);

            List<Recipe> allRecipes = await _recipeRepository.GetRecipesAsync(cancellationToken);
            List<Recipe> groupRecipes = allRecipes
                .Where(r => string.Equals(r.Group, groupName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (groupRecipes.Count == 0)
            {
                _logger.LogWarning("No recipes found in group '{GroupName}'", groupName);
                return null;
            }

            byte[] zipBytes = BuildZip(groupRecipes);

            _logger.LogInformation("Exported {RecipeCount} recipes from group '{GroupName}' to zip", groupRecipes.Count, groupName);
            return zipBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting group '{GroupName}' as zip", groupName);
            return null;
        }
    }

    public async Task<RecipeExportResult?> ExportRecipeAsync(Guid recipeId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Starting single recipe export for ID {RecipeId}", recipeId);

            Recipe? recipe = await _recipeRepository.GetRecipeAsync(recipeId, cancellationToken);

            if (recipe is null)
            {
                _logger.LogWarning("Recipe with ID {RecipeId} not found for export", recipeId);
                return null;
            }

            string json = JsonSerializer.Serialize(recipe, _jsonOptions);
            byte[] content = Encoding.UTF8.GetBytes(json);
            string fileName = FileNameHelper.ToFileName(recipe.Name, recipe.Id);

            _logger.LogInformation("Exported recipe '{RecipeName}'", recipe.Name);
            return new RecipeExportResult(content, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting recipe with ID {RecipeId}", recipeId);
            return null;
        }
    }

    private byte[] BuildZip(List<Recipe> recipes)
    {
        using MemoryStream memoryStream = new();
        using (ZipArchive archive = new(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (Recipe recipe in recipes)
            {
                string fileName = FileNameHelper.ToFileName(recipe.Name, recipe.Id);
                string entryPath = string.IsNullOrEmpty(recipe.Group) ? fileName : $"{recipe.Group}/{fileName}";

                ZipArchiveEntry entry = archive.CreateEntry(entryPath);
                using StreamWriter writer = new(entry.Open(), Encoding.UTF8);
                string json = JsonSerializer.Serialize(recipe, _jsonOptions);
                writer.Write(json);
            }
        }

        return memoryStream.ToArray();
    }
}
