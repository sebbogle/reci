namespace Reci.Data.Repositories;

public class FileSystemRecipeRepository(IFileSystemAccessService fileSystemAccess, IFilePathService filePathService, ILogger<FileSystemRecipeRepository> logger, JsonSerializerOptions jsonOptions) : IRecipeRepository, IDisposable
{
    private readonly IFileSystemAccessService _fileSystemAccess = fileSystemAccess.ThrowIfNull();
    private readonly IFilePathService _filePathService = filePathService.ThrowIfNull();
    private readonly ILogger<FileSystemRecipeRepository> _logger = logger.ThrowIfNull();
    private readonly JsonSerializerOptions _jsonOptions = jsonOptions.ThrowIfNull();

    private Dictionary<RecipeKey, CachedRecipe>? _cache;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    public async Task<Recipe?> GetRecipeAsync(RecipeKey key, CancellationToken cancellationToken = default)
    {
        try
        {
            Dictionary<RecipeKey, CachedRecipe> cache = await GetOrLoadCacheAsync(cancellationToken);
            return cache.TryGetValue(key, out CachedRecipe? cached) ? cached.Recipe : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recipe '{RecipeName}' in group '{Group}'", key.Name, key.Group);
            return null;
        }
    }

    public async Task<List<Recipe>> GetRecipesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Dictionary<RecipeKey, CachedRecipe> cache = await GetOrLoadCacheAsync(cancellationToken);
            return cache.Values.Select(c => c.Recipe).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all recipes");
            return [];
        }
    }

    public async Task<List<RecipeSummary>> GetRecipeSummariesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Dictionary<RecipeKey, CachedRecipe> cache = await GetOrLoadCacheAsync(cancellationToken);
            return cache.Values.Select(c => RecipeSummary.FromRecipe(c.Recipe)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recipe summaries");
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

            Dictionary<RecipeKey, CachedRecipe> cache = await GetOrLoadCacheAsync(cancellationToken);

            RecipeKey key = new(recipe.Name, recipe.Group);

            if (cache.ContainsKey(key))
                return Result.Failure($"A recipe named '{recipe.Name}' already exists in group '{recipe.Group ?? "ungrouped"}'");

            string folderName = string.IsNullOrEmpty(recipe.Group)
                ? string.Empty
                : _filePathService.SanitizeForFileSystem(recipe.Group);
            if (!string.IsNullOrEmpty(folderName))
                await EnsureFolderExistsAsync(folderName);

            string fileName = _filePathService.ToFileName(recipe.Name);
            string filePath = string.IsNullOrEmpty(folderName) ? fileName : $"{folderName}/{fileName}";

            string json = JsonSerializer.Serialize(recipe, _jsonOptions);
            await _fileSystemAccess.WriteFileAsync(filePath, json);

            recipe.IsNew = false;
            cache[key] = new CachedRecipe(recipe, filePath, folderName);

            _logger.LogInformation("Created recipe '{RecipeName}' at {FilePath}", recipe.Name, filePath);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating recipe '{RecipeName}'", recipe?.Name);
            return Result.Failure($"Failed to create recipe: {ex.Message}");
        }
    }

    public async Task<Result> UpdateRecipeAsync(Recipe recipe, RecipeKey originalKey, CancellationToken cancellationToken = default)
    {
        try
        {
            if (recipe == null)
                return Result.Failure("Recipe cannot be null");

            if (string.IsNullOrWhiteSpace(recipe.Name))
                return Result.Failure("Recipe name is required");

            Dictionary<RecipeKey, CachedRecipe> cache = await GetOrLoadCacheAsync(cancellationToken);

            if (!cache.TryGetValue(originalKey, out CachedRecipe? existing))
                return Result.Failure($"Recipe '{originalKey.Name}' not found in group '{originalKey.Group ?? "ungrouped"}'");

            RecipeKey newKey = new(recipe.Name, recipe.Group);

            if (!originalKey.Equals(newKey) && cache.ContainsKey(newKey))
                return Result.Failure($"A recipe named '{recipe.Name}' already exists in group '{recipe.Group ?? "ungrouped"}'");

            string newFolderName = string.IsNullOrEmpty(recipe.Group)
                ? string.Empty
                : _filePathService.SanitizeForFileSystem(recipe.Group);
            if (!string.IsNullOrEmpty(newFolderName))
                await EnsureFolderExistsAsync(newFolderName);

            string newFileName = _filePathService.ToFileName(recipe.Name);
            string newFilePath = string.IsNullOrEmpty(newFolderName) ? newFileName : $"{newFolderName}/{newFileName}";

            string json = JsonSerializer.Serialize(recipe, _jsonOptions);

            if (!string.Equals(existing.FilePath, newFilePath, StringComparison.Ordinal))
            {
                await _fileSystemAccess.WriteFileAsync(newFilePath, json);
                try
                {
                    await _fileSystemAccess.DeleteFileAsync(existing.FilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete old file at {OldPath} after move to {NewPath}", existing.FilePath, newFilePath);
                }
            }
            else
            {
                await _fileSystemAccess.WriteFileAsync(newFilePath, json);
            }

            cache.Remove(originalKey);
            cache[newKey] = new CachedRecipe(recipe, newFilePath, newFolderName);

            _logger.LogInformation("Updated recipe '{RecipeName}' at {FilePath}", recipe.Name, newFilePath);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating recipe '{RecipeName}'", recipe?.Name);
            return Result.Failure($"Failed to update recipe: {ex.Message}");
        }
    }

    public async Task<Result> DeleteRecipeAsync(RecipeKey key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key.Name))
                return Result.Failure("Recipe name is required");

            Dictionary<RecipeKey, CachedRecipe> cache = await GetOrLoadCacheAsync(cancellationToken);

            if (!cache.TryGetValue(key, out CachedRecipe? existing))
                return Result.Failure($"Recipe '{key.Name}' not found in group '{key.Group ?? "ungrouped"}'");

            await _fileSystemAccess.DeleteFileAsync(existing.FilePath);
            cache.Remove(key);

            _logger.LogInformation("Deleted recipe '{RecipeName}' from {FilePath}", key.Name, existing.FilePath);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting recipe '{RecipeName}'", key.Name);
            return Result.Failure($"Failed to delete recipe: {ex.Message}");
        }
    }

    private async Task<Dictionary<RecipeKey, CachedRecipe>> GetOrLoadCacheAsync(CancellationToken cancellationToken = default)
    {
        if (_cache is not null)
            return _cache;

        await _cacheLock.WaitAsync(cancellationToken);
        try
        {
            if (_cache is not null)
                return _cache;

            _logger.LogDebug("Loading recipes from file system into cache");
            _cache = await ScanAllRecipesAsync();
            return _cache;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private async Task<Dictionary<RecipeKey, CachedRecipe>> ScanAllRecipesAsync()
    {
        Dictionary<RecipeKey, CachedRecipe> cache = new();

        await ScanDirectoryAsync(cache, null, null);

        List<FileSystemEntry> rootEntries = await _fileSystemAccess.ListEntriesAsync(null);
        foreach (FileSystemEntry entry in rootEntries.Where(e => e.IsDirectory))
        {
            if (entry.Name.StartsWith('.'))
                continue;

            await ScanDirectoryAsync(cache, entry.Name, entry.Name);
        }

        _logger.LogInformation("Scanned {RecipeCount} recipes from file system", cache.Count);
        return cache;
    }

    private async Task ScanDirectoryAsync(Dictionary<RecipeKey, CachedRecipe> cache, string? subfolder, string? folderName)
    {
        List<FileSystemEntry> entries = await _fileSystemAccess.ListEntriesAsync(subfolder);

        foreach (FileSystemEntry entry in entries.Where(e => e.IsFile && e.Name.EndsWith(".reci", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                string filePath = subfolder is not null ? $"{subfolder}/{entry.Name}" : entry.Name;
                string json = await _fileSystemAccess.ReadFileAsync(filePath);
                Recipe? recipe = JsonSerializer.Deserialize<Recipe>(json, _jsonOptions);

                if (recipe is null)
                {
                    _logger.LogWarning("Skipping invalid recipe file: {FilePath}", filePath);
                    continue;
                }

                recipe.Group = folderName;
                recipe.IsNew = false;

                RecipeKey key = new(recipe.Name, folderName);
                cache[key] = new CachedRecipe(recipe, filePath, folderName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading recipe file: {FileName}", entry.Name);
            }
        }
    }

    private async Task EnsureFolderExistsAsync(string folderName)
    {
        List<FileSystemEntry> entries = await _fileSystemAccess.ListEntriesAsync(null);
        bool exists = entries.Any(e => e.IsDirectory && string.Equals(e.Name, folderName, StringComparison.Ordinal));
        if (!exists)
        {
            await _fileSystemAccess.CreateDirectoryAsync(folderName);
        }
    }

    public void Dispose()
    {
        _cacheLock.Dispose();
    }

    private sealed record CachedRecipe(Recipe Recipe, string FilePath, string? FolderName);
}
