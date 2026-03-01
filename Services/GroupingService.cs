namespace Reci.Services;

public class GroupingService(IGroupingRepository groupingRepository, ILogger<GroupingService> logger) : IGroupingService
{
    private readonly IGroupingRepository _groupingRepository = groupingRepository.ThrowIfNull();
    private readonly ILogger<GroupingService> _logger = logger.ThrowIfNull();

    public async Task<List<Group>> GetGroupingsAsync(GroupType? groupType = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving groupings with type filter: {GroupType}", groupType?.ToString() ?? "All");
        List<Group> groups = await _groupingRepository.GetGroupsAsync(cancellationToken);

        if (groupType.HasValue)
        {
            groups = groups.FindAll(g => g.GroupType == groupType.Value);
        }

        _logger.LogInformation("Retrieved {GroupCount} groupings", groups.Count);
        return groups.OrderBy(g => g.SortOrder).ToList();
    }

    public async Task<Group?> GetGroupingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving grouping with ID {GroupId}", id);
        List<Group> groups = await _groupingRepository.GetGroupsAsync(cancellationToken);

        Group? group = groups.FirstOrDefault(g => g.Id == id);

        if (group is null)
        {
            _logger.LogWarning("Grouping with ID {GroupId} not found", id);
        }

        return group;
    }

    public async Task<Result> SaveGroupingAsync(Group group, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);

        if (group.Id == Guid.Empty)
        {
            group.Id = Guid.NewGuid();
            _logger.LogDebug("Creating new grouping '{GroupName}' with ID {GroupId}", group.Name, group.Id);
            Result result = await _groupingRepository.AddGroupAsync(group, cancellationToken);
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created grouping '{GroupName}' with ID {GroupId}", group.Name, group.Id);
            }
            return result;
        }
        else
        {
            _logger.LogDebug("Updating grouping with ID {GroupId}", group.Id);
            Result result = await _groupingRepository.UpdateGroup(group, cancellationToken);
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated grouping '{GroupName}' with ID {GroupId}", group.Name, group.Id);
            }
            return result;
        }
    }

    public async Task<Result> DeleteGroupingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting grouping with ID {GroupId}", id);
        Result result = await _groupingRepository.DeleteGroup(id, cancellationToken);
        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully deleted grouping with ID {GroupId}", id);
        }
        else
        {
            _logger.LogWarning("Failed to delete grouping with ID {GroupId}: {Error}", id, result.ErrorMessage);
        }
        return result;
    }
}
