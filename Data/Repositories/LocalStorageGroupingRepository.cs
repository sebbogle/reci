namespace Reci.Data.Repositories;

public class LocalStorageGroupingRepository(ILocalStorageService localStorage, ILogger<LocalStorageGroupingRepository> logger) : IGroupingRepository, IDisposable
{
    private readonly ILocalStorageService _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
    private readonly ILogger<LocalStorageGroupingRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private const string _localStorageKey = "groups";

    private List<Group>? _cachedGroups = null;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

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
            if (id == Guid.Empty)
                return Result.Failure("Group ID is required");

            List<Group>? groups = await LoadGroupsAsync(cancellationToken);
            if (groups is null || !groups.Any())
                return Result.Failure("No groups found");

            Group? groupToRemove = groups.FirstOrDefault(r => r.Id == id);

            if (groupToRemove == null)
                return Result.Failure($"Group with ID {id} not found");

            groups.Remove(groupToRemove);
            await SaveGroupsAsync(groups, cancellationToken);

            _logger.LogInformation("Successfully deleted group with ID {GroupId}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group with ID {GroupId}", id);
            return Result.Failure($"Failed to delete group: {ex.Message}");
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

    public void Dispose()
    {
        _cacheLock.Dispose();
    }

    private async Task<List<Group>?> LoadGroupsAsync(CancellationToken cancellationToken)
    {
        // Fast path: cache already loaded
        if (_cachedGroups is not null)
            return _cachedGroups;

        // Slow path: load from storage with lock to prevent multiple loads
        await _cacheLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check: another caller may have loaded while we waited
            if (_cachedGroups is not null)
                return _cachedGroups;

            List<Group>? groups = await _localStorage.GetItemAsync<List<Group>?>(_localStorageKey, cancellationToken);
            _cachedGroups = groups;
            return groups;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private async Task SaveGroupsAsync(List<Group> groups, CancellationToken cancellationToken)
    {
        await _localStorage.SetItemAsync<List<Group>>(_localStorageKey, groups, cancellationToken);

        await _cacheLock.WaitAsync(cancellationToken);
        try
        {
            _cachedGroups = groups;
        }
        finally
        {
            _cacheLock.Release();
        }
    }
}
