namespace Reci.Services;

public class GroupingService(IGroupingRepository groupingRepository) : IGroupingService
{
    private readonly IGroupingRepository _groupingRepository = groupingRepository ?? throw new ArgumentNullException(nameof(groupingRepository));

    public async Task<List<Group>> GetGroupingsAsync(GroupType? groupType = null, CancellationToken cancellationToken = default)
    {
        List<Group> groups = await _groupingRepository.GetGroupsAsync(cancellationToken);

        if (groupType.HasValue)
        {
            groups = groups.FindAll(g => g.GroupType == groupType.Value);
        }

        return groups.OrderBy(g => g.SortOrder).ToList();
    }

    public async Task<Group?> GetGroupingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        List<Group> groups = await _groupingRepository.GetGroupsAsync(cancellationToken);
        
        Group? group = groups.FirstOrDefault(g => g.Id == id);

        return group;
    }

    public async Task<Result> SaveGroupingAsync(Group group, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);
        
        if (group.Id == Guid.Empty)
        {
            group.Id = Guid.NewGuid();
            return await _groupingRepository.AddGroupAsync(group, cancellationToken);
        }
        else
        {
            return await _groupingRepository.UpdateGroup(group, cancellationToken);
        }
    }

    public async Task<Result> DeleteGroupingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _groupingRepository.DeleteGroup(id, cancellationToken);
    }
}
