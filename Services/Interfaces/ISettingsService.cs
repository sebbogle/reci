using Reci.ViewModels;

namespace Reci.Services.Interfaces;

public interface ISettingsService
{
    public Task<SettingsVM> GetSettingsAsync(CancellationToken cancellationToken = default);

    public Task<Result> SaveSettingsAsync(SettingsVM settings, CancellationToken cancellationToken = default);

    public void ClearCache();
}
