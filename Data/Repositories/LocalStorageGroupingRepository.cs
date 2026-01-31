namespace Reci.Data.Repositories;

public class LocalStorageGroupingRepository(ILocalStorageService localStorage, ILogger<LocalStorageSettingsRepository> logger) : IGroupingRepository
{
    private readonly ILocalStorageService _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
    private readonly ILogger<LocalStorageSettingsRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private const string _localStorageKey = "groups";

    private List<Group>? _cachedGroups = null;

    public async Task<List<Group>> GetGroupsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await LoadGroupsAsync(cancellationToken) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving groups");
            return [];
        }
    }

    public async Task<Result> AddGroupAsync(Group group, CancellationToken cancellationToken = default)
    {
        try
        {
            List<Group> groups = await LoadGroupsAsync(cancellationToken) ?? [];
            groups.Add(group);
            await SaveGroupsAsync(groups, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding group");
            return Result.Failure("Error adding group");
        }
    }

    public async Task<Result> UpdateGroup(Group group, CancellationToken cancellationToken = default)
    {
        List<Group> groups = await LoadGroupsAsync(cancellationToken) ?? [];

        bool updated = groups.Replace(group, g => g.Id == group.Id);

        if (!updated)
        {
            return Result.Failure("Group not updated");
        }
        
        await SaveGroupsAsync(groups, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteGroup(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id == Guid.Empty) return Result.Failure("Group ID is required");

            var recipes = await LoadGroupsAsync(cancellationToken);
            if (recipes is null || !recipes.Any()) return Result.Failure("No recipes found");
            
            var recipeToRemove = recipes.FirstOrDefault(r => r.Id == id);

            if (recipeToRemove == null)
                return Result.Failure($"Recipe with ID {id} not found");

            recipes.Remove(recipeToRemove);
            await SaveGroupsAsync(recipes, cancellationToken);

            _logger.LogInformation("Successfully deleted recipe with ID {RecipeId}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting recipe with ID {RecipeId}", id);
            return Result.Failure($"Failed to delete recipe: {ex.Message}");
        }
    }

    public async Task<Result> SetGroups(List<Group> groups, CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveGroupsAsync(groups, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to save groups: {ex.Message}");
        }
    }

    private async Task<List<Group>?> LoadGroupsAsync(CancellationToken cancellationToken)
    {
        if (_cachedGroups is not null) return _cachedGroups;
        
        List<Group>? groups = await _localStorage.GetItemAsync<List<Group>?>(_localStorageKey, cancellationToken);
        _cachedGroups = groups;
        
        return groups;
    }

    private async Task SaveGroupsAsync(List<Group> groups, CancellationToken cancellationToken)
    {
        await _localStorage.SetItemAsync<List<Group>>(_localStorageKey, groups, cancellationToken);
        _cachedGroups = groups;
    }
}
