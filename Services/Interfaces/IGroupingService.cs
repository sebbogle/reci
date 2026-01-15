namespace Reci.Services.Interfaces;

public interface IGroupingService
{
    Task<List<Group>> GetGroupingsAsync(GroupType? groupType, CancellationToken cancellationToken = default);

    Task<Group?> GetGroupingAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result> SaveGroupingAsync(Group group, CancellationToken cancellationToken = default);

    Task<Result> DeleteGroupingAsync(Guid id, CancellationToken cancellationToken = default);
}
