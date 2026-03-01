namespace Reci.Services.Interfaces;

public interface ISettingsService
{
    Task<SettingsVM> GetSettingsAsync(CancellationToken cancellationToken = default);

    Task<Result> SaveSettingsAsync(SettingsVM settings, CancellationToken cancellationToken = default);

    void ClearCache();
}
