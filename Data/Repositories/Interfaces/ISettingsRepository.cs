using Reci.Data.Models;

namespace Reci.Data.Repositories.Interfaces;

public interface ISettingsRepository
{
    Task<Settings> GetSettingsAsync(CancellationToken cancellationToken = default);

    Task<Result> SaveSettingsAsync(Settings settings, CancellationToken cancellationToken = default);
}
