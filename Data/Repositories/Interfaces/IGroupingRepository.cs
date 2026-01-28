namespace Reci.Data.Repositories.Interfaces;

public interface IGroupingRepository
{
    Task<List<Group>> GetGroupsAsync(CancellationToken cancellationToken = default);

    Task<Result> AddGroupAsync(Group group, CancellationToken cancellationToken = default);

    Task<Result> UpdateGroup(Group group, CancellationToken cancellationToken = default);

    Task<Result> DeleteGroup(Guid id, CancellationToken cancellationToken = default);

    Task<Result> SetGroups(List<Group> groups, CancellationToken cancellationToken = default);
}
