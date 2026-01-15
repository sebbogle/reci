using Reci.Data.Models;
using Reci.Data.Repositories.Interfaces;
using Reci.Services.Interfaces;
using Reci.ViewModels;
using Reci.Mappers;

namespace Reci.Services;

public class SettingsService(ISettingsRepository settingsRepository) : ISettingsService
{
    private readonly ISettingsRepository _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));

    private SettingsVM? _cachedsettingsVM = null;

    public async Task<SettingsVM> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedsettingsVM is not null)
        {
            return _cachedsettingsVM;
        }

        Settings settings = await _settingsRepository.GetSettingsAsync(cancellationToken);
        SettingsVM settingsVM = settings.ToViewModel();
        
        _cachedsettingsVM = settingsVM;

        return settingsVM;
    }

    public async Task<Result> SaveSettingsAsync(SettingsVM settingsVM, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settingsVM);
        ClearCache(); // maybe should only be on the success code path

        Settings settings = settingsVM.ToModel();
        return await _settingsRepository.SaveSettingsAsync(settings, cancellationToken);
    }
    
    public void ClearCache()
    {
        _cachedsettingsVM = null;
    }
}
